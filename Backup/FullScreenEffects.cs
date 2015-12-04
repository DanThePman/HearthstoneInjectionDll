using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class FullScreenEffects : MonoBehaviour
{
    private const string BLEND_SHADER_NAME = "Custom/FullScreen/Blend";
    private const string BLEND_TO_COLOR_SHADER_NAME = "Custom/FullScreen/BlendToColor";
    private const int BLUR_BUFFER_SIZE = 0x200;
    private const string BLUR_DESATURATION_SHADER_NAME = "Custom/FullScreen/DesaturationBlur";
    private const string BLUR_DESATURATION_VIGNETTING_SHADER_NAME = "Custom/FullScreen/BlurDesaturationVignetting";
    private const float BLUR_PASS_1_OFFSET = 1f;
    private const float BLUR_PASS_2_OFFSET = 0.4f;
    private const float BLUR_PASS_3_OFFSET = -0.2f;
    private const float BLUR_SECOND_PASS_REDUCTION = 0.5f;
    private const string BLUR_SHADER_NAME = "Custom/FullScreen/Blur";
    private const string BLUR_VIGNETTING_SHADER_NAME = "Custom/FullScreen/BlurVignetting";
    private const string DESATURATION_SHADER_NAME = "Custom/FullScreen/Desaturation";
    private const string DESATURATION_VIGNETTING_SHADER_NAME = "Custom/FullScreen/DesaturationVignetting";
    private Material m_BlendMaterial;
    private Shader m_BlendShader;
    private Color m_BlendToColor = Color.white;
    private float m_BlendToColorAmount;
    private bool m_BlendToColorEnable;
    private Material m_BlendToColorMaterial;
    private Shader m_BlendToColorShader;
    private float m_BlurAmount = 2f;
    public float m_BlurBlend = 1f;
    private float m_BlurBrightness = 1f;
    private Material m_BlurDesatMaterial;
    private float m_BlurDesaturation;
    private Shader m_BlurDesaturationShader;
    private Material m_BlurDesaturationVignettingMaterial;
    private Shader m_BlurDesaturationVignettingShader;
    private bool m_BlurEnabled;
    private Material m_BlurMaterial;
    private Shader m_BlurShader;
    private Material m_BlurVignettingMaterial;
    private Shader m_BlurVignettingShader;
    private Camera m_Camera;
    private bool m_CaptureFrozenImage;
    private int m_DeactivateFrameCount;
    private float m_Desaturation;
    private bool m_DesaturationEnabled;
    private Material m_DesaturationMaterial;
    private Shader m_DesaturationShader;
    private Material m_DesaturationVignettingMaterial;
    private Shader m_DesaturationVignettingShader;
    private Texture2D m_FrozenScreenTexture;
    private bool m_FrozenState;
    private int m_LowQualityFreezeBufferSize = 0x200;
    private float m_PreviousBlurAmount = 1f;
    private float m_PreviousBlurBrightness = 1f;
    private float m_PreviousBlurDesaturation;
    private UniversalInputManager m_UniversalInputManager;
    private bool m_VignettingEnable;
    private float m_VignettingIntensity;
    public Texture2D m_VignettingMask;
    private Material m_VignettingMaterial;
    private Shader m_VignettingShader;
    private bool m_WireframeRender;
    private const int NO_WORK_FRAMES_BEFORE_DEACTIVATE = 2;
    private const string VIGNETTING_SHADER_NAME = "Custom/FullScreen/Vignetting";

    protected void Awake()
    {
        this.m_Camera = base.GetComponent<Camera>();
    }

    private void Blur(RenderTexture source, RenderTexture destination, Material blurMat)
    {
        float width = source.width;
        float height = source.height;
        this.CalcTextureSize(source.width, source.height, 0x200, out width, out height);
        RenderTexture dest = RenderTexture.GetTemporary((int) width, (int) height, 0);
        RenderTexture texture2 = RenderTexture.GetTemporary((int) (width * 0.5f), (int) (height * 0.5f), 0);
        dest.wrapMode = TextureWrapMode.Clamp;
        texture2.wrapMode = TextureWrapMode.Clamp;
        blurMat.SetFloat("_BlurOffset", 1f);
        blurMat.SetFloat("_FirstPass", 1f);
        Graphics.Blit(source, dest, blurMat);
        blurMat.SetFloat("_BlurOffset", 0.4f);
        blurMat.SetFloat("_FirstPass", 0f);
        Graphics.Blit(dest, texture2, blurMat);
        blurMat.SetFloat("_BlurOffset", -0.2f);
        blurMat.SetFloat("_FirstPass", 0f);
        Graphics.Blit(texture2, destination, blurMat);
        RenderTexture.ReleaseTemporary(dest);
        RenderTexture.ReleaseTemporary(texture2);
    }

    private void CalcTextureSize(int currentWidth, int currentHeight, int resolution, out float sizeX, out float sizeY)
    {
        float num = currentWidth;
        float num2 = currentHeight;
        float num3 = resolution;
        if (num > num2)
        {
            sizeX = num3;
            sizeY = num3 * (num2 / num);
        }
        else
        {
            sizeX = num3 * (num / num2);
            sizeY = num3;
        }
    }

    public void Disable()
    {
        base.enabled = false;
        this.SetDefaults();
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr != null)
        {
            mgr.WillReset();
        }
    }

    [ContextMenu("Freeze")]
    public void Freeze()
    {
        Log.Kyle.Print("FullScreenEffects: Freeze()", new object[0]);
        base.enabled = true;
        if (!this.m_FrozenState)
        {
            this.m_BlurEnabled = true;
            this.m_BlurBlend = 1f;
            this.m_BlurAmount = this.m_PreviousBlurAmount * 0.75f;
            this.m_BlurDesaturation = this.m_PreviousBlurDesaturation;
            this.m_BlurBrightness = this.m_PreviousBlurBrightness;
            this.m_CaptureFrozenImage = true;
            int lowQualityFreezeBufferSize = this.m_LowQualityFreezeBufferSize;
            int height = this.m_LowQualityFreezeBufferSize;
            if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Low)
            {
                lowQualityFreezeBufferSize = this.m_LowQualityFreezeBufferSize;
                height = this.m_LowQualityFreezeBufferSize;
            }
            else
            {
                lowQualityFreezeBufferSize = Screen.currentResolution.width;
                height = Screen.currentResolution.height;
            }
            this.m_FrozenScreenTexture = new Texture2D(lowQualityFreezeBufferSize, height, TextureFormat.RGB24, false, true);
            this.m_FrozenScreenTexture.filterMode = FilterMode.Point;
            this.m_FrozenScreenTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }

    public bool isActive()
    {
        if (!base.enabled)
        {
            return false;
        }
        return (this.m_FrozenState || ((this.m_BlurEnabled && (this.m_BlurBlend > 0f)) || (this.m_VignettingEnable || (this.m_BlendToColorEnable || (this.m_DesaturationEnabled || this.m_WireframeRender)))));
    }

    protected void OnDestroy()
    {
        CheatMgr mgr = CheatMgr.Get();
        if (mgr != null)
        {
            mgr.UnregisterCheatHandler("wireframe", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_RenderWireframe));
        }
    }

    protected void OnDisable()
    {
        this.SetDefaults();
        if (this.m_BlurMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlurMaterial);
        }
        if (this.m_BlurVignettingMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlurVignettingMaterial);
        }
        if (this.m_BlurDesatMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlurDesatMaterial);
        }
        if (this.m_BlendMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlendMaterial);
        }
        if (this.m_VignettingMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_VignettingMaterial);
        }
        if (this.m_BlendToColorMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlendToColorMaterial);
        }
        if (this.m_DesaturationMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_DesaturationMaterial);
        }
        if (this.m_DesaturationVignettingMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_DesaturationVignettingMaterial);
        }
        if (this.m_BlurDesaturationVignettingMaterial != null)
        {
            UnityEngine.Object.Destroy(this.m_BlurDesaturationVignettingMaterial);
        }
    }

    private void OnPostRender()
    {
        GL.wireframe = false;
    }

    private void OnPreRender()
    {
        if (this.m_WireframeRender)
        {
            GL.wireframe = true;
        }
    }

    private bool OnProcessCheat_RenderWireframe(string func, string[] args, string rawArgs)
    {
        if (this.m_WireframeRender)
        {
            this.m_WireframeRender = false;
            return true;
        }
        this.m_WireframeRender = true;
        base.enabled = true;
        return true;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (((source != null) && (source.width != 0)) && (source.height != 0))
        {
            bool flag = false;
            if (this.m_CaptureFrozenImage && !this.m_FrozenState)
            {
                Log.Kyle.Print("FullScreenEffects: Capture Frozen Image!", new object[0]);
                Material blurMaterial = this.blurMaterial;
                blurMaterial.SetFloat("_Brightness", this.m_BlurBrightness);
                if ((this.m_BlurDesaturation > 0f) && !this.m_VignettingEnable)
                {
                    blurMaterial = this.blurDesatMaterial;
                    blurMaterial.SetFloat("_Desaturation", this.m_BlurDesaturation);
                }
                else if (this.m_VignettingEnable && (this.m_BlurDesaturation == 0f))
                {
                    blurMaterial = this.blurVignettingMaterial;
                    blurMaterial.SetFloat("_Amount", this.m_VignettingIntensity);
                    blurMaterial.SetTexture("_MaskTex", this.m_VignettingMask);
                }
                else if (this.m_VignettingEnable && (this.m_BlurDesaturation > 0f))
                {
                    blurMaterial = this.BlurDesaturationVignettingMaterial;
                    blurMaterial.SetFloat("_Amount", this.m_VignettingIntensity);
                    blurMaterial.SetTexture("_MaskTex", this.m_VignettingMask);
                    blurMaterial.SetFloat("_Desaturation", this.m_BlurDesaturation);
                }
                int lowQualityFreezeBufferSize = this.m_LowQualityFreezeBufferSize;
                int height = this.m_LowQualityFreezeBufferSize;
                if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.Low)
                {
                    lowQualityFreezeBufferSize = this.m_LowQualityFreezeBufferSize;
                    height = this.m_LowQualityFreezeBufferSize;
                }
                else
                {
                    lowQualityFreezeBufferSize = Screen.currentResolution.width;
                    height = Screen.currentResolution.height;
                }
                RenderTexture temporary = RenderTexture.GetTemporary(lowQualityFreezeBufferSize, height);
                this.Blur(source, temporary, blurMaterial);
                RenderTexture.active = temporary;
                this.m_FrozenScreenTexture.ReadPixels(new Rect(0f, 0f, (float) lowQualityFreezeBufferSize, (float) height), 0, 0, false);
                this.m_FrozenScreenTexture.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(temporary);
                this.m_CaptureFrozenImage = false;
                this.m_FrozenState = true;
                flag = true;
                this.m_DeactivateFrameCount = 0;
            }
            if (this.m_FrozenState)
            {
                if (this.m_FrozenScreenTexture != null)
                {
                    Material blendMaterial = this.blendMaterial;
                    blendMaterial.SetFloat("_Amount", 1f);
                    blendMaterial.SetTexture("_BlendTex", this.m_FrozenScreenTexture);
                    if (QualitySettings.antiAliasing > 0)
                    {
                        blendMaterial.SetFloat("_Flip", 1f);
                    }
                    else
                    {
                        blendMaterial.SetFloat("_Flip", 0f);
                    }
                    if (destination != null)
                    {
                        destination.DiscardContents();
                    }
                    Graphics.Blit(source, destination, blendMaterial);
                    this.m_DeactivateFrameCount = 0;
                    return;
                }
                Debug.LogWarning("m_FrozenScreenTexture is null. FullScreenEffect Freeze disabled");
                this.m_FrozenState = false;
            }
            if (this.m_BlurEnabled && (this.m_BlurBlend > 0f))
            {
                Material blurMat = this.blurMaterial;
                blurMat.SetFloat("_Brightness", this.m_BlurBrightness);
                if ((this.m_BlurDesaturation > 0f) && !this.m_VignettingEnable)
                {
                    blurMat = this.blurDesatMaterial;
                    blurMat.SetFloat("_Desaturation", this.m_BlurDesaturation);
                }
                else if (this.m_VignettingEnable && (this.m_BlurDesaturation == 0f))
                {
                    blurMat = this.blurVignettingMaterial;
                    blurMat.SetFloat("_Amount", this.m_VignettingIntensity);
                    blurMat.SetTexture("_MaskTex", this.m_VignettingMask);
                }
                else if (this.m_VignettingEnable && (this.m_BlurDesaturation > 0f))
                {
                    blurMat = this.BlurDesaturationVignettingMaterial;
                    blurMat.SetFloat("_Amount", this.m_VignettingIntensity);
                    blurMat.SetTexture("_MaskTex", this.m_VignettingMask);
                    blurMat.SetFloat("_Desaturation", this.m_BlurDesaturation);
                }
                if (this.m_BlurBlend >= 1f)
                {
                    this.Blur(source, destination, blurMat);
                    flag = true;
                }
                else
                {
                    RenderTexture texture2 = RenderTexture.GetTemporary(0x200, 0x200, 0);
                    this.Blur(source, texture2, blurMat);
                    Material mat = this.blendMaterial;
                    mat.SetFloat("_Amount", this.m_BlurBlend);
                    mat.SetTexture("_BlendTex", texture2);
                    Graphics.Blit(source, destination, mat);
                    RenderTexture.ReleaseTemporary(texture2);
                    flag = true;
                }
            }
            if (this.m_DesaturationEnabled && !flag)
            {
                Material desaturationMaterial = this.DesaturationMaterial;
                if (this.m_VignettingEnable)
                {
                    desaturationMaterial = this.DesaturationVignettingMaterial;
                    desaturationMaterial.SetFloat("_Amount", this.m_VignettingIntensity);
                    desaturationMaterial.SetTexture("_MaskTex", this.m_VignettingMask);
                }
                desaturationMaterial.SetFloat("_Desaturation", this.m_Desaturation);
                Graphics.Blit(source, destination, desaturationMaterial);
                flag = true;
            }
            if (this.m_VignettingEnable && !flag)
            {
                Material vignettingMaterial = this.VignettingMaterial;
                vignettingMaterial.SetFloat("_Amount", this.m_VignettingIntensity);
                vignettingMaterial.SetTexture("_MaskTex", this.m_VignettingMask);
                Graphics.Blit(source, destination, vignettingMaterial);
                flag = true;
            }
            if (this.m_BlendToColorEnable && !flag)
            {
                Material blendToColorMaterial = this.BlendToColorMaterial;
                blendToColorMaterial.SetFloat("_Amount", this.m_BlendToColorAmount);
                blendToColorMaterial.SetColor("_Color", this.m_BlendToColor);
                Graphics.Blit(source, destination, blendToColorMaterial);
                flag = true;
            }
            if (!flag)
            {
                Material material8 = this.blendMaterial;
                material8.SetFloat("_Amount", 0f);
                material8.SetTexture("_BlendTex", null);
                Graphics.Blit(source, destination, material8);
                if (this.m_DeactivateFrameCount > 2)
                {
                    this.m_DeactivateFrameCount = 0;
                    this.Disable();
                }
                else
                {
                    this.m_DeactivateFrameCount++;
                }
            }
        }
    }

    private void Sample(RenderTexture source, RenderTexture dest, float off, Material mat)
    {
        if (dest != null)
        {
            dest.DiscardContents();
        }
        Vector2[] offsets = new Vector2[] { new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off) };
        Graphics.BlitMultiTap(source, dest, mat, offsets);
    }

    private void SetDefaults()
    {
        this.m_BlurEnabled = false;
        this.m_BlurBlend = 1f;
        this.m_BlurAmount = 2f;
        this.m_BlurDesaturation = 0f;
        this.m_BlurBrightness = 1f;
        this.m_VignettingEnable = false;
        this.m_VignettingIntensity = 0f;
        this.m_BlendToColorEnable = false;
        this.m_BlendToColor = Color.white;
        this.m_BlendToColorAmount = 0f;
        this.m_DesaturationEnabled = false;
        this.m_Desaturation = 0f;
    }

    protected void Start()
    {
        CheatMgr mgr = CheatMgr.Get();
        if (mgr != null)
        {
            mgr.RegisterCheatHandler("wireframe", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_RenderWireframe), null, null, null);
        }
        base.gameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
        if (!SystemInfo.supportsImageEffects)
        {
            Debug.LogError("Fullscreen Effects not supported");
            base.enabled = false;
        }
        else
        {
            if (this.m_BlurShader == null)
            {
                this.m_BlurShader = ShaderUtils.FindShader("Custom/FullScreen/Blur");
            }
            if (this.m_BlurShader == null)
            {
                Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/Blur");
                base.enabled = false;
            }
            if ((this.m_BlurShader == null) || !this.blurMaterial.shader.isSupported)
            {
                Debug.LogError("Fullscreen Effect Shader not supported: Custom/FullScreen/Blur");
                base.enabled = false;
            }
            else
            {
                if (this.m_BlurVignettingShader == null)
                {
                    this.m_BlurVignettingShader = ShaderUtils.FindShader("Custom/FullScreen/BlurVignetting");
                }
                if (this.m_BlurVignettingShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/BlurVignetting");
                    base.enabled = false;
                }
                if (this.m_BlurDesaturationShader == null)
                {
                    this.m_BlurDesaturationShader = ShaderUtils.FindShader("Custom/FullScreen/DesaturationBlur");
                }
                if (this.m_BlurDesaturationShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/DesaturationBlur");
                    base.enabled = false;
                }
                if (this.m_BlendShader == null)
                {
                    this.m_BlendShader = ShaderUtils.FindShader("Custom/FullScreen/Blend");
                }
                if (this.m_BlendShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/Blend");
                    base.enabled = false;
                }
                if (this.m_VignettingShader == null)
                {
                    this.m_VignettingShader = ShaderUtils.FindShader("Custom/FullScreen/Vignetting");
                }
                if (this.m_VignettingShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/Vignetting");
                    base.enabled = false;
                }
                if (this.m_BlendToColorShader == null)
                {
                    this.m_BlendToColorShader = ShaderUtils.FindShader("Custom/FullScreen/BlendToColor");
                }
                if (this.m_BlendToColorShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/BlendToColor");
                    base.enabled = false;
                }
                if (this.m_DesaturationShader == null)
                {
                    this.m_DesaturationShader = ShaderUtils.FindShader("Custom/FullScreen/Desaturation");
                }
                if (this.m_DesaturationShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/Desaturation");
                    base.enabled = false;
                }
                if (this.m_DesaturationVignettingShader == null)
                {
                    this.m_DesaturationVignettingShader = ShaderUtils.FindShader("Custom/FullScreen/DesaturationVignetting");
                }
                if (this.m_DesaturationVignettingShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/DesaturationVignetting");
                    base.enabled = false;
                }
                if (this.m_BlurDesaturationVignettingShader == null)
                {
                    this.m_BlurDesaturationVignettingShader = ShaderUtils.FindShader("Custom/FullScreen/BlurDesaturationVignetting");
                }
                if (this.m_BlurDesaturationVignettingShader == null)
                {
                    Debug.LogError("Fullscreen Effect Failed to load Shader: Custom/FullScreen/BlurDesaturationVignetting");
                    base.enabled = false;
                }
            }
        }
    }

    [ContextMenu("Unfreeze")]
    public void Unfreeze()
    {
        Log.Kyle.Print("FullScreenEffects: Unfreeze()", new object[0]);
        this.m_BlurEnabled = false;
        this.m_BlurBlend = 0f;
        this.m_FrozenState = false;
        UnityEngine.Object.Destroy(this.m_FrozenScreenTexture);
        this.m_FrozenScreenTexture = null;
    }

    private void Update()
    {
        this.UpdateUniversalInputManager();
    }

    private void UpdateUniversalInputManager()
    {
        if (this.m_UniversalInputManager == null)
        {
            this.m_UniversalInputManager = UniversalInputManager.Get();
        }
        if (this.m_UniversalInputManager != null)
        {
            this.m_UniversalInputManager.SetFullScreenEffectsCamera(this.m_Camera, this.isActive());
        }
    }

    protected Material blendMaterial
    {
        get
        {
            if (this.m_BlendMaterial == null)
            {
                this.m_BlendMaterial = new Material(this.m_BlendShader);
                SceneUtils.SetHideFlags(this.m_BlendMaterial, HideFlags.DontSave);
            }
            return this.m_BlendMaterial;
        }
    }

    public Color BlendToColor
    {
        get
        {
            return this.m_BlendToColor;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlendToColorEnable = true;
            this.m_BlendToColor = value;
        }
    }

    public float BlendToColorAmount
    {
        get
        {
            return this.m_BlendToColorAmount;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlendToColorEnable = true;
            this.m_BlendToColorAmount = value;
        }
    }

    public bool BlendToColorEnable
    {
        get
        {
            return this.m_BlendToColorEnable;
        }
        set
        {
            if (!base.enabled && value)
            {
                base.enabled = true;
            }
            this.m_BlendToColorEnable = value;
        }
    }

    protected Material BlendToColorMaterial
    {
        get
        {
            if (this.m_BlendToColorMaterial == null)
            {
                this.m_BlendToColorMaterial = new Material(this.m_BlendToColorShader);
                SceneUtils.SetHideFlags(this.m_BlendToColorMaterial, HideFlags.DontSave);
            }
            return this.m_BlendToColorMaterial;
        }
    }

    public float BlurAmount
    {
        get
        {
            return this.m_BlurAmount;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlurEnabled = true;
            this.m_BlurAmount = value;
            this.m_PreviousBlurAmount = value;
        }
    }

    public float BlurBlend
    {
        get
        {
            return this.m_BlurBlend;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlurEnabled = true;
            this.m_BlurBlend = value;
        }
    }

    public float BlurBrightness
    {
        get
        {
            return this.m_BlurBrightness;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlurEnabled = true;
            this.m_BlurBrightness = value;
            this.m_PreviousBlurBrightness = value;
        }
    }

    protected Material blurDesatMaterial
    {
        get
        {
            if (this.m_BlurDesatMaterial == null)
            {
                this.m_BlurDesatMaterial = new Material(this.m_BlurDesaturationShader);
                SceneUtils.SetHideFlags(this.m_BlurDesatMaterial, HideFlags.DontSave);
            }
            return this.m_BlurDesatMaterial;
        }
    }

    public float BlurDesaturation
    {
        get
        {
            return this.m_BlurDesaturation;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_BlurEnabled = true;
            this.m_BlurDesaturation = value;
            this.m_PreviousBlurDesaturation = value;
        }
    }

    protected Material BlurDesaturationVignettingMaterial
    {
        get
        {
            if (this.m_BlurDesaturationVignettingMaterial == null)
            {
                this.m_BlurDesaturationVignettingMaterial = new Material(this.m_BlurDesaturationVignettingShader);
                SceneUtils.SetHideFlags(this.m_BlurDesaturationVignettingMaterial, HideFlags.DontSave);
            }
            return this.m_BlurDesaturationVignettingMaterial;
        }
    }

    public bool BlurEnabled
    {
        get
        {
            return this.m_BlurEnabled;
        }
        set
        {
            if (!base.enabled && value)
            {
                base.enabled = true;
            }
            this.m_BlurEnabled = value;
        }
    }

    protected Material blurMaterial
    {
        get
        {
            if (this.m_BlurMaterial == null)
            {
                this.m_BlurMaterial = new Material(this.m_BlurShader);
                SceneUtils.SetHideFlags(this.m_BlurMaterial, HideFlags.DontSave);
            }
            return this.m_BlurMaterial;
        }
    }

    protected Material blurVignettingMaterial
    {
        get
        {
            if (this.m_BlurVignettingMaterial == null)
            {
                this.m_BlurVignettingMaterial = new Material(this.m_BlurVignettingShader);
                SceneUtils.SetHideFlags(this.m_BlurVignettingMaterial, HideFlags.DontSave);
            }
            return this.m_BlurVignettingMaterial;
        }
    }

    public float Desaturation
    {
        get
        {
            return this.m_Desaturation;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_DesaturationEnabled = true;
            this.m_Desaturation = value;
        }
    }

    public bool DesaturationEnabled
    {
        get
        {
            return this.m_DesaturationEnabled;
        }
        set
        {
            if (!base.enabled && value)
            {
                base.enabled = true;
            }
            this.m_DesaturationEnabled = value;
        }
    }

    protected Material DesaturationMaterial
    {
        get
        {
            if (this.m_DesaturationMaterial == null)
            {
                this.m_DesaturationMaterial = new Material(this.m_DesaturationShader);
                SceneUtils.SetHideFlags(this.m_DesaturationMaterial, HideFlags.DontSave);
            }
            return this.m_DesaturationMaterial;
        }
    }

    protected Material DesaturationVignettingMaterial
    {
        get
        {
            if (this.m_DesaturationVignettingMaterial == null)
            {
                this.m_DesaturationVignettingMaterial = new Material(this.m_DesaturationVignettingShader);
                SceneUtils.SetHideFlags(this.m_DesaturationVignettingMaterial, HideFlags.DontSave);
            }
            return this.m_DesaturationVignettingMaterial;
        }
    }

    public bool VignettingEnable
    {
        get
        {
            return this.m_VignettingEnable;
        }
        set
        {
            if (!base.enabled && value)
            {
                base.enabled = true;
            }
            this.m_VignettingEnable = value;
        }
    }

    public float VignettingIntensity
    {
        get
        {
            return this.m_VignettingIntensity;
        }
        set
        {
            if (!base.enabled)
            {
                base.enabled = true;
            }
            this.m_VignettingEnable = true;
            this.m_VignettingIntensity = value;
        }
    }

    protected Material VignettingMaterial
    {
        get
        {
            if (this.m_VignettingMaterial == null)
            {
                this.m_VignettingMaterial = new Material(this.m_VignettingShader);
                SceneUtils.SetHideFlags(this.m_VignettingMaterial, HideFlags.DontSave);
            }
            return this.m_VignettingMaterial;
        }
    }
}

