using System;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenEffectsRender : MonoBehaviour
{
    private const string BLOOM_SHADER_NAME = "Hidden/ScreenEffectsGlow";
    private const string DISTORTION_SHADER_NAME = "Hidden/ScreenEffectDistortion";
    private const string GLOW_MASK_SHADER = "Hidden/ScreenEffectsMask";
    private const int GLOW_RANDER_BUFFER_RESOLUTION = 0x200;
    public bool m_Debug;
    private Material m_DistortionMaterial;
    private Shader m_DistortionShader;
    [HideInInspector]
    public Camera m_EffectsObjectsCamera;
    [HideInInspector]
    public RenderTexture m_EffectsTexture;
    private Material m_GlowMaterial;
    private Shader m_GlowShader;
    private int m_height;
    private RenderTexture m_MaskRenderTexture;
    private Shader m_MaskShader;
    private int m_previousHeight;
    private int m_previousWidth;
    private int m_width;

    private void Awake()
    {
        if (ScreenEffectsMgr.Get() == null)
        {
            base.enabled = false;
        }
        if (this.m_MaskShader == null)
        {
            this.m_MaskShader = Shader.Find("Hidden/ScreenEffectsMask");
            if (this.m_MaskShader == null)
            {
                Debug.LogError("Failed to load ScreenEffectsRender Shader: Hidden/ScreenEffectsMask");
            }
        }
    }

    private Material DistortionMaterialRender(RenderTexture source)
    {
        return this.DistortionMaterial;
    }

    private void OnDestroy()
    {
    }

    private void OnDisable()
    {
        UnityEngine.Object.DestroyImmediate(this.GlowMaterial);
        UnityEngine.Object.DestroyImmediate(this.DistortionMaterial);
        if (this.m_MaskRenderTexture != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_MaskRenderTexture);
            this.m_MaskRenderTexture = null;
        }
        if (this.m_GlowMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_GlowMaterial);
        }
        if (this.m_DistortionMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(this.m_DistortionMaterial);
        }
    }

    private void OnEnable()
    {
        this.SetupEffect();
    }

    private void OnPreRender()
    {
        this.RenderEffectsObjects();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        this.RenderGlow(source, destination);
    }

    private void RenderDistortion(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, this.DistortionMaterialRender(source));
    }

    private void RenderEffectsObjects()
    {
        if (this.m_EffectsObjectsCamera == null)
        {
            base.enabled = false;
        }
        else
        {
            float num = ((float) Screen.width) / ((float) Screen.height);
            int width = (int) (512f * num);
            int height = 0x200;
            if ((width != this.m_previousWidth) || (height != this.m_previousHeight))
            {
                UnityEngine.Object.DestroyImmediate(this.m_MaskRenderTexture);
                this.m_MaskRenderTexture = null;
            }
            if (this.m_MaskRenderTexture == null)
            {
                this.m_MaskRenderTexture = new RenderTexture(width, height, 0x20);
                this.m_MaskRenderTexture.filterMode = FilterMode.Bilinear;
                this.m_previousWidth = width;
                this.m_previousHeight = height;
            }
            if ((this.m_EffectsObjectsCamera.targetTexture == null) && (this.m_MaskRenderTexture != null))
            {
                this.m_EffectsObjectsCamera.targetTexture = this.m_MaskRenderTexture;
            }
            this.m_MaskRenderTexture.DiscardContents();
            this.m_EffectsObjectsCamera.RenderWithShader(this.m_MaskShader, "RenderType");
        }
    }

    private void RenderGlow(RenderTexture source, RenderTexture destination)
    {
        int width = this.m_MaskRenderTexture.width;
        int height = this.m_MaskRenderTexture.height;
        RenderTexture dest = RenderTexture.GetTemporary(width, height, 0, source.format);
        dest.filterMode = FilterMode.Bilinear;
        this.GlowMaterial.SetFloat("_BlurOffset", 1f);
        Graphics.Blit(this.m_MaskRenderTexture, dest, this.GlowMaterial, 0);
        RenderTexture texture2 = RenderTexture.GetTemporary(width, height, 0, source.format);
        texture2.filterMode = FilterMode.Bilinear;
        this.GlowMaterial.SetFloat("_BlurOffset", 2f);
        Graphics.Blit(dest, texture2, this.GlowMaterial, 0);
        this.GlowMaterial.SetFloat("_BlurOffset", 3f);
        dest.DiscardContents();
        Graphics.Blit(texture2, dest, this.GlowMaterial, 0);
        this.GlowMaterial.SetFloat("_BlurOffset", 5f);
        texture2.DiscardContents();
        Graphics.Blit(dest, texture2, this.GlowMaterial, 0);
        this.GlowMaterial.SetTexture("_BlurTex", texture2);
        if (!this.m_Debug)
        {
            Graphics.Blit(source, destination, this.GlowMaterial, 1);
        }
        else
        {
            Graphics.Blit(source, destination, this.GlowMaterial, 2);
        }
        RenderTexture.ReleaseTemporary(dest);
        RenderTexture.ReleaseTemporary(texture2);
    }

    private void SetupDistortion()
    {
        if (this.m_EffectsTexture != null)
        {
            this.DistortionMaterial.SetTexture("_EffectTex", this.m_EffectsTexture);
        }
    }

    private void SetupEffect()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    protected Material DistortionMaterial
    {
        get
        {
            if (this.m_DistortionMaterial == null)
            {
                if (this.m_DistortionShader == null)
                {
                    this.m_DistortionShader = Shader.Find("Hidden/ScreenEffectDistortion");
                    if (this.m_DistortionShader == null)
                    {
                        Debug.LogError("Failed to load ScreenEffectsRender Shader: Hidden/ScreenEffectDistortion");
                    }
                }
                this.m_DistortionMaterial = new Material(this.m_DistortionShader);
                SceneUtils.SetHideFlags(this.m_DistortionMaterial, HideFlags.HideAndDontSave);
            }
            return this.m_DistortionMaterial;
        }
    }

    protected Material GlowMaterial
    {
        get
        {
            if (this.m_GlowMaterial == null)
            {
                if (this.m_GlowShader == null)
                {
                    this.m_GlowShader = Shader.Find("Hidden/ScreenEffectsGlow");
                    if (this.m_GlowShader == null)
                    {
                        Debug.LogError("Failed to load ScreenEffectsRender Shader: Hidden/ScreenEffectsGlow");
                    }
                }
                this.m_GlowMaterial = new Material(this.m_GlowShader);
                SceneUtils.SetHideFlags(this.m_GlowMaterial, HideFlags.HideAndDontSave);
            }
            return this.m_GlowMaterial;
        }
    }
}

