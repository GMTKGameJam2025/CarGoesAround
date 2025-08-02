using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{   
    [SerializeField] private Material normalsMaterial;
    [SerializeField] private Material outlineMaterial;

    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        public GraphicsFormat ColorFormat = GraphicsFormat.R8G8B8A8_UNorm;
        public int DepthBufferBits = 0;
        public Color backgroundColor = Color.black;
    }

    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private ViewSpaceNormalsTextureSettings normalsTextureSettings;
        internal RTHandle normals;
        private RenderTextureDescriptor normalsTextureDescriptor;

        private readonly List<ShaderTagId> shaderTagIdList;
        private readonly Material normalsMaterial;
        private FilteringSettings filteringSettings;

        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask outlinesLayerMask, ViewSpaceNormalsTextureSettings settings, Material normalsMaterial)
        {
            this.renderPassEvent = renderPassEvent;
            normalsTextureSettings = settings;
            normals = RTHandles.Alloc("_NormalsTexture", name: "_NormalsTexture");

            this.normalsMaterial = normalsMaterial;

            shaderTagIdList = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, outlinesLayerMask);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!normalsMaterial) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = normalsMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.graphicsFormat = normalsTextureSettings.ColorFormat;
            normalsTextureDescriptor.depthBufferBits = normalsTextureSettings.DepthBufferBits;
            cmd.GetTemporaryRT(Shader.PropertyToID(normals.name), normalsTextureDescriptor, FilterMode.Point);

            ConfigureTarget(normals);
            ConfigureClear(ClearFlag.All, normalsTextureSettings.backgroundColor);
        }
    }

    private class ScreenSpaceOutlinesPass : ScriptableRenderPass
    {
        private readonly Material screenSpaceOutlineMaterial;
        private RenderTargetIdentifier cameraColorTargetHandle;
        private RTHandle normalsHandle;
        private int temporaryBufferId = Shader.PropertyToID("_TemporaryBuffer");

        public ScreenSpaceOutlinesPass(RenderPassEvent renderPassEvent, RTHandle normals, Material outlineMaterial)
        {
            this.renderPassEvent = renderPassEvent;
            this.normalsHandle = normals;
            this.screenSpaceOutlineMaterial = outlineMaterial;

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {   
            screenSpaceOutlineMaterial.SetTexture("_NormalsTex", normalsHandle);

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                Blitter.BlitCameraTexture(cmd, RTHandles.Alloc(cameraColorTargetHandle), RTHandles.Alloc(cameraColorTargetHandle), screenSpaceOutlineMaterial, 0);

            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (temporaryBufferId != 0)
            {
                cmd.ReleaseTemporaryRT(temporaryBufferId);
                temporaryBufferId = 0;
            }
        }
    }

    [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings = new ViewSpaceNormalsTextureSettings();
    [SerializeField] private LayerMask outlinesLayerMask;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinesPass screenSpaceOutlinesPass;

    public override void Create()
    {
        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, outlinesLayerMask, viewSpaceNormalsTextureSettings, normalsMaterial);
        screenSpaceOutlinesPass = new ScreenSpaceOutlinesPass(renderPassEvent, viewSpaceNormalsTexturePass.normals, outlineMaterial);

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinesPass);
    }
}