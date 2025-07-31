using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class CustomPostProcessPass : ScriptableRenderPass
{
    // Materials
    private Material m_bloomMaterial;
    private Material m_compositeMaterial;

    // Bloom pyramid
    private const int k_MaxPyramidSize = 16;
    private readonly int[] _BloomMipUp = new int[k_MaxPyramidSize];
    private readonly int[] _BloomMipDown = new int[k_MaxPyramidSize];
    private readonly RTHandle[] m_BloomMipUp = new RTHandle[k_MaxPyramidSize];
    private readonly RTHandle[] m_BloomMipDown = new RTHandle[k_MaxPyramidSize];

    // Camera/Render targets
    RenderTextureDescriptor m_Descriptor;

    RTHandle m_CameraColorTarget;

    RTHandle m_CameraDepthTarget;
    private GraphicsFormat hdrFormat;

    // Bloom settings
    private BenDayBloomEffectComponent m_BloomEffect;

    internal void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle)
    {
        m_CameraColorTarget = cameraColorTargetHandle;
        m_CameraDepthTarget = cameraDepthTargetHandle;
    }

    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        m_bloomMaterial = bloomMaterial;
        m_compositeMaterial = compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        _BloomMipUp = new int[k_MaxPyramidSize];
        _BloomMipDown = new int[k_MaxPyramidSize];
        m_BloomMipUp = new RTHandle[k_MaxPyramidSize];
        m_BloomMipDown = new RTHandle[k_MaxPyramidSize];

        for (int i = 0; i < k_MaxPyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            m_BloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            m_BloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + i);
        }

        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
        }
    }

    RenderTextureDescriptor GetCompatibleDescriptor()
    => GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, hdrFormat);

    RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        var desc = new RenderTextureDescriptor(width, height);
        desc.depthBufferBits = (int)depthBufferBits;
        desc.graphicsFormat = format;
        desc.msaaSamples = 1;
        desc.useMipMap = false;
        desc.autoGenerateMips = false;
        desc.dimension = TextureDimension.Tex2D;
        return desc;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
        m_CameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        VolumeStack stack = VolumeManager.instance.stack;
        m_BloomEffect = stack.GetComponent<BenDayBloomEffectComponent>();

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Custom Bloom Processing")))
        {
            SetupBloom(cmd, m_CameraColorTarget);
            CompositeBloom(cmd, m_CameraColorTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    private void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        // Start at half resolution
        int downres = 1;
        int tw = m_Descriptor.width >> downres;
        int th = m_Descriptor.height >> downres;

        // Calculate iteration count based on texture size
        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
        int mipCount = Mathf.Clamp(iterations, 1, m_BloomEffect.maxIterations.value);

        // Pre-filtering parameters
        float clamp = m_BloomEffect.clamp.value;
        float threshold = Mathf.GammaToLinearSpace(m_BloomEffect.threshold.value);
        float thresholdKnee = threshold * 0.5f; // Soft knee

        // Material setup
        float scatter = Mathf.Lerp(0.05f, 0.95f, m_BloomEffect.scatter.value);
        var bloomMaterial = m_bloomMaterial;
        m_bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

        // Initial descriptor for bloom pyramid
        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        desc.enableRandomWrite = true;

        // Downsample - Gaussian pyramid construction
        var lastDown = m_BloomMipDown[0];
        for (int i = 1; i < mipCount; i++)
        {
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);

            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BloomMipUp" + i);
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BloomMipDown" + i);
    
            // Horizontal blur pass
            Blitter.BlitCameraTexture(
                cmd,
                lastDown,
                m_BloomMipUp[i],
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                m_bloomMaterial,
                1);

            // Vertical blur pass
            Blitter.BlitCameraTexture(
                cmd,
                m_BloomMipUp[i],
                m_BloomMipDown[i],
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                m_bloomMaterial,
                2);

            lastDown = m_BloomMipDown[i];
        }

        RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[0], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BloomMipDown0");
        Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_bloomMaterial, 0);

        // Upsample and combine - Bloom pyramid reconstruction
        for (int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount - 2) ? m_BloomMipDown[i + 1] : m_BloomMipUp[i + 1];
            var dst = m_BloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            cmd.SetGlobalFloat("_BloomScatter", m_BloomEffect.scatter.value);

            Blitter.BlitCameraTexture(
                cmd,
                m_BloomMipDown[i],
                dst,
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                m_bloomMaterial,
                3);
        }

        cmd.SetGlobalTexture("_Bloom_Texture", m_BloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", m_BloomEffect.intensity.value);
    }

    private void CompositeBloom(CommandBuffer cmd, RTHandle target)
    {
        // Final composite pass
        Blitter.BlitCameraTexture(
            cmd,
            m_BloomMipUp[0],
            target,
            m_compositeMaterial,
            0);
    }

    public void Dispose()
    {
        for (int i = 0; i < k_MaxPyramidSize; i++)
        {
            m_BloomMipUp[i]?.Release();
            m_BloomMipDown[i]?.Release();
        }
    }
}