using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_bloomShader;
    [SerializeField]
    private Shader m_compositeShader;

    private Material m_bloomMaterial;
    private Material m_compositeMaterial;
    private CustomPostProcessPass m_customPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled && renderingData.cameraData.cameraType == CameraType.Game)
        {
            m_customPass.SetTarget(
                renderer.cameraColorTargetHandle,
                renderer.cameraDepthTargetHandle);
            renderer.EnqueuePass(m_customPass);
        }
    }

    public override void Create()
    {
        // Auto-find bloom shader if not assigned
        if (m_bloomShader == null)
            m_bloomShader = Shader.Find("Hidden/Universal Render Pipeline/Bloom");
        
        if (m_bloomShader == null || m_compositeShader == null)
        {
            Debug.LogError("Shaders are missing!");
            return;
        }

        m_bloomMaterial = CoreUtils.CreateEngineMaterial(m_bloomShader);
        m_compositeMaterial = CoreUtils.CreateEngineMaterial(m_compositeShader);
        
        m_customPass = new CustomPostProcessPass(m_bloomMaterial, m_compositeMaterial)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_bloomMaterial);
        CoreUtils.Destroy(m_compositeMaterial);
    }
}
