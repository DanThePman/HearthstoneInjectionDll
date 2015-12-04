using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    private const int ANDROID_MIN_DPI_HIGH_RES_TEXTURES = 180;
    private List<GameObject> m_DisableLowQualityObjects;
    private GraphicsQuality m_GraphicsQuality;
    private int m_lastHeight;
    private int m_lastWidth;
    private bool m_RealtimeShadows;
    private bool m_VeryLowQuality;
    private int m_winPosX;
    private int m_winPosY;
    private const int REDUCE_MAX_WINDOW_SIZE_X = 20;
    private const int REDUCE_MAX_WINDOW_SIZE_Y = 60;
    private static GraphicsManager s_instance;

    private static void _GetLimits(ref GPULimits limits)
    {
        limits.highPrecisionBits = 0x10;
        limits.mediumPrecisionBits = 0x10;
        limits.lowPrecisionBits = 0x17;
        limits.maxFragmentTextureUnits = 0x10;
        limits.maxVertexTextureUnits = 0x10;
        limits.maxCombinedTextureUnits = 0x20;
        limits.maxTextureSize = 0x2000;
        limits.maxCubeMapSize = 0x2000;
        limits.maxRenderBufferSize = 0x2000;
        limits.maxFragmentUniforms = 0x100;
        limits.maxVertexUniforms = 0x100;
        limits.maxVaryings = 0x20;
        limits.maxVertexAttribs = 0x20;
    }

    private void Awake()
    {
        s_instance = this;
        this.m_DisableLowQualityObjects = new List<GameObject>();
        if (!Options.Get().HasOption(Option.GFX_QUALITY))
        {
            string intelDeviceName = W8Touch.Get().GetIntelDeviceName();
            object[] args = new object[] { intelDeviceName };
            Log.Yim.Print("Intel Device Name = {0}", args);
            if (((intelDeviceName != null) && intelDeviceName.Contains("Haswell")) && intelDeviceName.Contains("U28W"))
            {
                if (Screen.currentResolution.height > 0x438)
                {
                    Options.Get().SetInt(Option.GFX_QUALITY, 0);
                }
            }
            else if ((intelDeviceName != null) && intelDeviceName.Contains("Crystal-Well"))
            {
                Options.Get().SetInt(Option.GFX_QUALITY, 2);
            }
            else if ((intelDeviceName != null) && intelDeviceName.Contains("BayTrail"))
            {
                Options.Get().SetInt(Option.GFX_QUALITY, 0);
            }
        }
        this.m_GraphicsQuality = (GraphicsQuality) Options.Get().GetInt(Option.GFX_QUALITY);
        this.InitializeScreen();
        this.UpdateQualitySettings();
        this.m_lastWidth = Screen.width;
        this.m_lastHeight = Screen.height;
    }

    public void DeregisterLowQualityDisableObject(GameObject lowQualityObject)
    {
        if (this.m_DisableLowQualityObjects.Contains(lowQualityObject))
        {
            this.m_DisableLowQualityObjects.Remove(lowQualityObject);
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string className, string windowName);
    public static GraphicsManager Get()
    {
        return s_instance;
    }

    private static IntPtr GetActiveWindow()
    {
        return GetForegroundWindow();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    public GPULimits GetGPULimits()
    {
        GPULimits limits = new GPULimits();
        _GetLimits(ref limits);
        return limits;
    }

    private static int[] GetWindowPosition()
    {
        int[] numArray = new int[2];
        RECT lpRect = new RECT();
        GetWindowRect(GetActiveWindow(), out lpRect);
        numArray[0] = lpRect.Left;
        numArray[1] = lpRect.Top;
        return numArray;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    private void InitializeScreen()
    {
        int @int;
        int num2;
        bool @bool = Options.Get().GetBool(Option.GFX_FULLSCREEN, Screen.fullScreen);
        if (@bool)
        {
            @int = Options.Get().GetInt(Option.GFX_WIDTH, Screen.currentResolution.width);
            num2 = Options.Get().GetInt(Option.GFX_HEIGHT, Screen.currentResolution.height);
            if (!Options.Get().HasOption(Option.GFX_WIDTH) || !Options.Get().HasOption(Option.GFX_HEIGHT))
            {
                string intelDeviceName = W8Touch.Get().GetIntelDeviceName();
                if (((intelDeviceName != null) && ((intelDeviceName.Contains("Haswell") && intelDeviceName.Contains("Y6W")) || (intelDeviceName.Contains("Haswell") && intelDeviceName.Contains("U15W")))) && (Screen.currentResolution.height >= 0x438))
                {
                    @int = 0x780;
                    num2 = 0x438;
                }
            }
            if (((@int == Screen.currentResolution.width) && (num2 == Screen.currentResolution.height)) && (@bool == Screen.fullScreen))
            {
                return;
            }
        }
        else
        {
            if (!Options.Get().HasOption(Option.GFX_WIDTH) || !Options.Get().HasOption(Option.GFX_HEIGHT))
            {
                return;
            }
            @int = Options.Get().GetInt(Option.GFX_WIDTH);
            num2 = Options.Get().GetInt(Option.GFX_HEIGHT);
        }
        this.SetScreenResolution(@int, num2, @bool);
        if (!@bool && (Options.Get().HasOption(Option.GFX_WIN_POSX) && Options.Get().HasOption(Option.GFX_WIN_POSY)))
        {
            int x = Options.Get().GetInt(Option.GFX_WIN_POSX);
            int y = Options.Get().GetInt(Option.GFX_WIN_POSY);
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            base.StartCoroutine(this.SetPos(x, y, 0.6f));
        }
    }

    public bool isVeryLowQualityDevice()
    {
        return this.m_VeryLowQuality;
    }

    private void LogGPULimits()
    {
        GPULimits gPULimits = this.GetGPULimits();
        UnityEngine.Debug.Log("GPU Limits:");
        UnityEngine.Debug.Log(string.Format("GPU - Fragment High Precision: {0}", gPULimits.highPrecisionBits));
        UnityEngine.Debug.Log(string.Format("GPU - Fragment Medium Precision: {0}", gPULimits.mediumPrecisionBits));
        UnityEngine.Debug.Log(string.Format("GPU - Fragment Low Precision: {0}", gPULimits.lowPrecisionBits));
        UnityEngine.Debug.Log(string.Format("GPU - Fragment Max Texture Units: {0}", gPULimits.maxFragmentTextureUnits));
        UnityEngine.Debug.Log(string.Format("GPU - Vertex Max Texture Units: {0}", gPULimits.maxVertexTextureUnits));
        UnityEngine.Debug.Log(string.Format("GPU - Combined Max Texture Units: {0}", gPULimits.maxCombinedTextureUnits));
        UnityEngine.Debug.Log(string.Format("GPU - Max Texture Size: {0}", gPULimits.maxTextureSize));
        UnityEngine.Debug.Log(string.Format("GPU - Max Cube-Map Texture Size: {0}", gPULimits.maxCubeMapSize));
        UnityEngine.Debug.Log(string.Format("GPU - Max Renderbuffer Size: {0}", gPULimits.maxRenderBufferSize));
        UnityEngine.Debug.Log(string.Format("GPU - Fragment Max Uniform Vectors: {0}", gPULimits.maxFragmentUniforms));
        UnityEngine.Debug.Log(string.Format("GPU - Vertex Max Uniform Vectors: {0}", gPULimits.maxVertexUniforms));
        UnityEngine.Debug.Log(string.Format("GPU - Max Varying Vectors: {0}", gPULimits.maxVaryings));
        UnityEngine.Debug.Log(string.Format("GPU - Vertex Max Attribs: {0}", gPULimits.maxVertexAttribs));
    }

    private void LogSystemInfo()
    {
        UnityEngine.Debug.Log("System Info:");
        UnityEngine.Debug.Log(string.Format("SystemInfo - Device Name: {0}", SystemInfo.deviceName));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Device Model: {0}", SystemInfo.deviceModel));
        UnityEngine.Debug.Log(string.Format("SystemInfo - OS: {0}", SystemInfo.operatingSystem));
        UnityEngine.Debug.Log(string.Format("SystemInfo - CPU Type: {0}", SystemInfo.processorType));
        UnityEngine.Debug.Log(string.Format("SystemInfo - CPU Cores: {0}", SystemInfo.processorCount));
        UnityEngine.Debug.Log(string.Format("SystemInfo - System Memory: {0}", SystemInfo.systemMemorySize));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Screen Resolution: {0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Screen DPI: {0}", Screen.dpi));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU ID: {0}", SystemInfo.graphicsDeviceID));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU Name: {0}", SystemInfo.graphicsDeviceName));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU Vendor: {0}", SystemInfo.graphicsDeviceVendor));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU Memory: {0}", SystemInfo.graphicsMemorySize));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU Shader Level: {0}", SystemInfo.graphicsShaderLevel));
        UnityEngine.Debug.Log(string.Format("SystemInfo - GPU NPOT Support: {0}", SystemInfo.npotSupport));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics API (version): {0}", SystemInfo.graphicsDeviceVersion));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics API (type): {0}", SystemInfo.graphicsDeviceType));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supported Render Target Count: {0}", SystemInfo.supportedRenderTargetCount));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports 3D Textures: {0}", SystemInfo.supports3DTextures));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Compute Shaders: {0}", SystemInfo.supportsComputeShaders));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Image Effects: {0}", SystemInfo.supportsImageEffects));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Render Textures: {0}", SystemInfo.supportsRenderTextures));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Render To Cubemap: {0}", SystemInfo.supportsRenderToCubemap));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Shadows: {0}", SystemInfo.supportsShadows));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Sparse Textures: {0}", SystemInfo.supportsSparseTextures));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Supports Stencil: {0}", SystemInfo.supportsStencil));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics RenderTextureFormat.ARGBHalf: {0}", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)));
        UnityEngine.Debug.Log(string.Format("SystemInfo - Graphics Metal Support: {0}", SystemInfo.graphicsDeviceVersion.StartsWith("Metal")));
    }

    private void OnDestroy()
    {
        if (!Screen.fullScreen)
        {
            Options.Get().SetInt(Option.GFX_WIDTH, Screen.width);
            Options.Get().SetInt(Option.GFX_HEIGHT, Screen.height);
            int[] windowPosition = GetWindowPosition();
            Options.Get().SetInt(Option.GFX_WIN_POSX, windowPosition[0]);
            Options.Get().SetInt(Option.GFX_WIN_POSY, windowPosition[1]);
        }
        s_instance = null;
    }

    public void RegisterLowQualityDisableObject(GameObject lowQualityObject)
    {
        if (!this.m_DisableLowQualityObjects.Contains(lowQualityObject))
        {
            this.m_DisableLowQualityObjects.Add(lowQualityObject);
        }
    }

    [DebuggerHidden]
    private IEnumerator SetPos(int x, int y, float delay = 0f)
    {
        return new <SetPos>c__Iterator281 { delay = delay, x = x, y = y, <$>delay = delay, <$>x = x, <$>y = y, <>f__this = this };
    }

    private void SetQualityByName(string qualityName)
    {
        string[] names = QualitySettings.names;
        int index = -1;
        int num2 = 0;
        while (num2 < names.Length)
        {
            if (names[num2] == qualityName)
            {
                index = num2;
            }
            num2++;
        }
        if (num2 < 0)
        {
            UnityEngine.Debug.LogError(string.Format("GraphicsManager: Quality Level not found: {0}", qualityName));
        }
        else
        {
            QualitySettings.SetQualityLevel(index, true);
        }
    }

    [DebuggerHidden]
    private IEnumerator SetRes(int width, int height, bool fullscreen)
    {
        return new <SetRes>c__Iterator280 { width = width, height = height, fullscreen = fullscreen, <$>width = width, <$>height = height, <$>fullscreen = fullscreen, <>f__this = this };
    }

    private void SetScreenEffects()
    {
        if (ScreenEffectsMgr.Get() != null)
        {
            if (this.m_GraphicsQuality == GraphicsQuality.Low)
            {
                ScreenEffectsMgr.Get().gameObject.SetActive(false);
            }
            else
            {
                ScreenEffectsMgr.Get().gameObject.SetActive(true);
            }
        }
    }

    public void SetScreenResolution(int width, int height, bool fullscreen)
    {
        if ((height > (Screen.currentResolution.height - 60)) && !fullscreen)
        {
            height = Screen.currentResolution.height - 60;
        }
        if ((width > (Screen.currentResolution.width - 20)) && !fullscreen)
        {
            width = Screen.currentResolution.width - 20;
        }
        base.StartCoroutine(this.SetRes(width, height, fullscreen));
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    private static bool SetWindowPosition(int x, int y, int resX = 0, int resY = 0)
    {
        IntPtr activeWindow = GetActiveWindow();
        IntPtr ptr2 = FindWindow(null, "Hearthstone");
        if (activeWindow == ptr2)
        {
            SetWindowPos(activeWindow, 0, x, y, resX, resY, ((resX * resY) != 0) ? 0 : 1);
            return true;
        }
        return false;
    }

    private void Start()
    {
        if (Options.Get().HasOption(Option.GFX_TARGET_FRAME_RATE))
        {
            Application.targetFrameRate = Options.Get().GetInt(Option.GFX_TARGET_FRAME_RATE);
        }
        else
        {
            Application.targetFrameRate = 30;
        }
        this.LogSystemInfo();
    }

    private void Update()
    {
        if ((!Screen.fullScreen && ((this.m_lastWidth != Screen.width) || (this.m_lastHeight != Screen.height))) && (((((float) Screen.width) / ((float) Screen.height)) >= 1.777777f) || ((((float) Screen.width) / ((float) Screen.height)) <= 1.333333f)))
        {
            int width = Screen.width;
            int height = Screen.height;
            if ((((float) Screen.width) / ((float) Screen.height)) > 1.777777f)
            {
                width = (int) (Screen.height * 1.777777f);
            }
            else
            {
                width = (int) (Screen.height * 1.333333f);
            }
            int[] windowPosition = GetWindowPosition();
            int x = windowPosition[0];
            int y = windowPosition[1];
            Screen.SetResolution(width, height, Screen.fullScreen);
            this.m_lastWidth = width;
            this.m_lastHeight = height;
            base.StartCoroutine(this.SetPos(x, y, 0f));
        }
    }

    private void UpdateAntiAliasing()
    {
        bool @bool = false;
        int @int = 0;
        if (this.m_GraphicsQuality == GraphicsQuality.Low)
        {
            @int = 0;
            @bool = false;
        }
        if (this.m_GraphicsQuality == GraphicsQuality.Medium)
        {
            @int = 2;
            @bool = false;
            string intelDeviceName = W8Touch.Get().GetIntelDeviceName();
            if ((intelDeviceName != null) && ((intelDeviceName.Equals("BayTrail") || intelDeviceName.Equals("Poulsbo")) || (intelDeviceName.Equals("CloverTrail") || (intelDeviceName.Contains("Haswell") && intelDeviceName.Contains("Y6W")))))
            {
                @int = 0;
            }
        }
        if (this.m_GraphicsQuality == GraphicsQuality.High)
        {
            switch (Localization.GetLocale())
            {
                case Locale.koKR:
                case Locale.ruRU:
                case Locale.zhTW:
                case Locale.zhCN:
                case Locale.plPL:
                case Locale.jaJP:
                    @int = 2;
                    @bool = false;
                    goto Label_00E5;
            }
            @int = 0;
            @bool = true;
        }
    Label_00E5:
        if (Options.Get().HasOption(Option.GFX_MSAA))
        {
            @int = Options.Get().GetInt(Option.GFX_MSAA);
        }
        if (Options.Get().HasOption(Option.GFX_FXAA))
        {
            @bool = Options.Get().GetBool(Option.GFX_FXAA);
        }
        if (@bool)
        {
            @int = 0;
        }
        if (@int > 0)
        {
            @bool = false;
        }
        QualitySettings.antiAliasing = @int;
        FullScreenAntialiasing[] antialiasingArray = UnityEngine.Object.FindObjectsOfType(typeof(FullScreenAntialiasing)) as FullScreenAntialiasing[];
        foreach (FullScreenAntialiasing antialiasing in antialiasingArray)
        {
            antialiasing.enabled = @bool;
        }
    }

    private void UpdateQualitySettings()
    {
        Log.Kyle.Print("GraphicsManager Update, Graphics Quality: " + this.m_GraphicsQuality.ToString(), new object[0]);
        this.UpdateRenderQualitySettings();
        this.UpdateAntiAliasing();
    }

    private void UpdateRenderQualitySettings()
    {
        int num = 30;
        int num2 = 0;
        int num3 = 0x65;
        if (this.m_GraphicsQuality == GraphicsQuality.Low)
        {
            num = 30;
            num2 = 0;
            this.m_RealtimeShadows = false;
            this.SetQualityByName("Low");
            num3 = 0x65;
        }
        if (this.m_GraphicsQuality == GraphicsQuality.Medium)
        {
            num = 30;
            num2 = 0;
            this.m_RealtimeShadows = false;
            this.SetQualityByName("Medium");
            num3 = 0xc9;
        }
        if (this.m_GraphicsQuality == GraphicsQuality.High)
        {
            num = 60;
            num2 = 1;
            this.m_RealtimeShadows = true;
            this.SetQualityByName("High");
            num3 = 0x12d;
        }
        Shader.DisableKeyword("LOW_QUALITY");
        if (Options.Get().HasOption(Option.GFX_TARGET_FRAME_RATE))
        {
            Application.targetFrameRate = Options.Get().GetInt(Option.GFX_TARGET_FRAME_RATE);
        }
        else
        {
            if (((W8Touch.Get() != null) && (W8Touch.Get().GetBatteryMode() == W8Touch.PowerSource.BatteryPower)) && (num > 30))
            {
                object[] args = new object[] { num };
                Log.Yim.Print("Battery Mode Detected - Clamping Target Frame Rate from {0} to 30", args);
                num = 30;
                num2 = 0;
            }
            Application.targetFrameRate = num;
        }
        if (Options.Get().HasOption(Option.GFX_VSYNC))
        {
            QualitySettings.vSyncCount = Options.Get().GetInt(Option.GFX_VSYNC);
        }
        else
        {
            QualitySettings.vSyncCount = num2;
        }
        Log.Kyle.Print(string.Format("Target frame rate: {0}", Application.targetFrameRate), new object[0]);
        ProjectedShadow[] shadowArray = UnityEngine.Object.FindObjectsOfType(typeof(ProjectedShadow)) as ProjectedShadow[];
        foreach (ProjectedShadow shadow in shadowArray)
        {
            shadow.enabled = !this.m_RealtimeShadows;
        }
        RenderToTexture[] textureArray = UnityEngine.Object.FindObjectsOfType(typeof(RenderToTexture)) as RenderToTexture[];
        foreach (RenderToTexture texture in textureArray)
        {
            texture.ForceTextureRebuild();
        }
        Shader[] shaderArray = UnityEngine.Object.FindObjectsOfType(typeof(Shader)) as Shader[];
        foreach (Shader shader in shaderArray)
        {
            shader.maximumLOD = num3;
        }
        foreach (GameObject obj2 in this.m_DisableLowQualityObjects)
        {
            if (obj2 != null)
            {
                if (this.m_GraphicsQuality == GraphicsQuality.Low)
                {
                    Log.Kyle.Print(string.Format("Low Quality Disable: {0}", obj2.name), new object[0]);
                    obj2.SetActive(false);
                }
                else
                {
                    Log.Kyle.Print(string.Format("Low Quality Enable: {0}", obj2.name), new object[0]);
                    obj2.SetActive(true);
                }
            }
        }
        Shader.globalMaximumLOD = num3;
        this.SetScreenEffects();
    }

    public bool RealtimeShadows
    {
        get
        {
            return this.m_RealtimeShadows;
        }
    }

    public GraphicsQuality RenderQualityLevel
    {
        get
        {
            return this.m_GraphicsQuality;
        }
        set
        {
            this.m_GraphicsQuality = value;
            Options.Get().SetInt(Option.GFX_QUALITY, (int) this.m_GraphicsQuality);
            this.UpdateQualitySettings();
        }
    }

    [CompilerGenerated]
    private sealed class <SetPos>c__Iterator281 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal int <$>x;
        internal int <$>y;
        internal GraphicsManager <>f__this;
        internal int[] <currentPos>__0;
        internal int[] <newPos>__1;
        internal float <startTime>__2;
        internal float delay;
        internal int x;
        internal int y;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    goto Label_0142;

                case 1:
                    this.<>f__this.m_winPosX = this.x;
                    this.<>f__this.m_winPosY = this.y;
                    this.<currentPos>__0 = GraphicsManager.GetWindowPosition();
                    this.<newPos>__1 = new int[] { this.<>f__this.m_winPosX, this.<>f__this.m_winPosY };
                    this.<startTime>__2 = UnityEngine.Time.time;
                    break;

                case 2:
                    break;

                default:
                    goto Label_0140;
            }
            if ((this.<currentPos>__0 != this.<newPos>__1) && (UnityEngine.Time.time < (this.<startTime>__2 + 1f)))
            {
                this.<newPos>__1[0] = this.<>f__this.m_winPosX;
                this.<newPos>__1[1] = this.<>f__this.m_winPosY;
                if (GraphicsManager.SetWindowPosition(this.<>f__this.m_winPosX, this.<>f__this.m_winPosY, 0, 0))
                {
                    this.<currentPos>__0 = GraphicsManager.GetWindowPosition();
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_0142;
                }
            }
            this.$PC = -1;
        Label_0140:
            return false;
        Label_0142:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SetRes>c__Iterator280 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>fullscreen;
        internal int <$>height;
        internal int <$>width;
        internal GraphicsManager <>f__this;
        internal Camera <camera>__4;
        internal CameraFade <cameraFade>__3;
        internal int[] <oldPos>__0;
        internal int <posX>__1;
        internal int <posY>__2;
        internal Color <prevColor>__6;
        internal float <prevDepth>__5;
        internal float <prevFade>__7;
        internal bool <prevROA>__8;
        internal bool fullscreen;
        internal int height;
        internal int width;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<oldPos>__0 = GraphicsManager.GetWindowPosition();
                    this.<posX>__1 = this.<oldPos>__0[0];
                    this.<posY>__2 = this.<oldPos>__0[1];
                    this.<cameraFade>__3 = LoadingScreen.Get().GetCameraFade();
                    this.<camera>__4 = LoadingScreen.Get().GetFxCamera();
                    this.<prevDepth>__5 = this.<camera>__4.depth;
                    this.<prevColor>__6 = this.<cameraFade>__3.m_Color;
                    this.<prevFade>__7 = this.<cameraFade>__3.m_Fade;
                    this.<prevROA>__8 = this.<cameraFade>__3.m_RenderOverAll;
                    this.<cameraFade>__3.m_Color = Color.black;
                    this.<cameraFade>__3.m_Fade = 1f;
                    this.<cameraFade>__3.m_RenderOverAll = true;
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_02E2;

                case 1:
                    Screen.SetResolution(this.width, this.height, this.fullscreen);
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_02E2;

                case 2:
                    Screen.SetResolution(this.width, this.height, this.fullscreen);
                    this.<>f__this.m_lastWidth = Screen.width;
                    this.<>f__this.m_lastHeight = Screen.height;
                    this.$current = null;
                    this.$PC = 3;
                    goto Label_02E2;

                case 3:
                    this.<camera>__4.depth = this.<prevDepth>__5;
                    this.<cameraFade>__3.m_Color = this.<prevColor>__6;
                    this.<cameraFade>__3.m_Fade = this.<prevFade>__7;
                    this.<cameraFade>__3.m_RenderOverAll = this.<prevROA>__8;
                    if (((this.<posX>__1 + this.width) + 20) > Screen.currentResolution.width)
                    {
                        this.<posX>__1 = Screen.currentResolution.width - (this.width + 20);
                    }
                    if (((this.<posY>__2 + this.height) + 60) > Screen.currentResolution.height)
                    {
                        this.<posY>__2 = Screen.currentResolution.height - (this.height + 60);
                    }
                    if ((this.<posX>__1 < 0) || (this.<posX>__1 > Screen.currentResolution.width))
                    {
                        this.<posX>__1 = 0;
                    }
                    if (((this.<posY>__2 + this.height) + 120) > Screen.currentResolution.height)
                    {
                        this.<posY>__2 = 0;
                    }
                    if ((this.<posY>__2 < 0) || (this.<posY>__2 > Screen.currentResolution.height))
                    {
                        this.<posY>__2 = 0;
                    }
                    this.<>f__this.StartCoroutine(this.<>f__this.SetPos(this.<posX>__1, this.<posY>__2, 0f));
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_02E2:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GPULimits
    {
        public int highPrecisionBits;
        public int mediumPrecisionBits;
        public int lowPrecisionBits;
        public int maxFragmentTextureUnits;
        public int maxVertexTextureUnits;
        public int maxCombinedTextureUnits;
        public int maxTextureSize;
        public int maxCubeMapSize;
        public int maxRenderBufferSize;
        public int maxFragmentUniforms;
        public int maxVertexUniforms;
        public int maxVaryings;
        public int maxVertexAttribs;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

