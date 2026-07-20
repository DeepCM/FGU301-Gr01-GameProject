using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public static class GlitchState
{
    public static float GlitchIntensity = 0f;
    public static bool CRTEnabled = false;
}

public class GlitchRendererFeature : ScriptableRendererFeature
{
    private class GlitchRenderPass : ScriptableRenderPass
    {
        private Material m_Material;

        public float glitchIntensity;
        public bool crtEnabled;

        public GlitchRenderPass(Material material)
        {
            m_Material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float glitchIntensity;
            public bool crtEnabled;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            TextureHandle activeColor = resourceData.activeColorTexture;

            if (!activeColor.IsValid()) return;

            TextureDesc desc = activeColor.GetDescriptor(renderGraph);
            desc.depthBufferBits = 0;
            desc.name = "_TempGlitchTexture";
            TextureHandle tempTexture = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass("Glitch_Copy", out PassData copyData))
            {
                copyData.source = activeColor;
                copyData.material = m_Material;
                copyData.glitchIntensity = glitchIntensity;
                copyData.crtEnabled = crtEnabled;

                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc<PassData>((data, context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass("Glitch_Apply", out PassData applyData))
            {
                applyData.source = tempTexture;
                applyData.material = m_Material;
                applyData.glitchIntensity = glitchIntensity;
                applyData.crtEnabled = crtEnabled;

                builder.UseTexture(tempTexture, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc<PassData>((data, context) =>
                {
                    data.material.SetFloat("_GlitchIntensity", data.glitchIntensity);
                    data.material.SetFloat("_TimeSeconds", Time.time);
                    data.material.SetFloat("_CRTEnabled", data.crtEnabled ? 1f : 0f);
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null) return;
            CommandBuffer cmd = CommandBufferPool.Get("GlitchEffect_Fallback");
            var renderer = renderingData.cameraData.renderer;
            RTHandle colorTarget = renderer.cameraColorTargetHandle;

            if (colorTarget != null)
            {
                m_Material.SetFloat("_GlitchIntensity", glitchIntensity);
                m_Material.SetFloat("_TimeSeconds", Time.time);
                m_Material.SetFloat("_CRTEnabled", crtEnabled ? 1f : 0f);

                RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;

                int tempId = Shader.PropertyToID("_TempGlitchTexture_Fallback");
                cmd.GetTemporaryRT(tempId, desc);

                cmd.Blit(colorTarget, tempId);
                cmd.Blit(tempId, colorTarget, m_Material, 0);

                cmd.ReleaseTemporaryRT(tempId);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [Range(0f, 1f)]
    public float glitchIntensity = 0f;
    public bool crtEnabled = false;
    public Shader glitchShader;

    private Material m_Material;
    private GlitchRenderPass m_GlitchPass;

    public override void Create()
    {
        if (glitchShader != null)
        {
            m_Material = CoreUtils.CreateEngineMaterial(glitchShader);
        }
        m_GlitchPass = new GlitchRenderPass(m_Material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_Material == null) return;

        m_GlitchPass.glitchIntensity = Mathf.Max(glitchIntensity, GlitchState.GlitchIntensity);
        m_GlitchPass.crtEnabled = crtEnabled || GlitchState.CRTEnabled;

        renderer.EnqueuePass(m_GlitchPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
    }
}

