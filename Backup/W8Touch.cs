using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class W8Touch : MonoBehaviour
{
    private static DelW8GetBatteryMode DLL_W8GetBatteryMode;
    private static DelW8GetDesktopRect DLL_W8GetDesktopRect;
    private static DelW8GetDeviceId DLL_W8GetDeviceId;
    private static DelW8GetPercentBatteryLife DLL_W8GetPercentBatteryLife;
    private static DelW8GetTouchPoint DLL_W8GetTouchPoint;
    private static DelW8GetTouchPointCount DLL_W8GetTouchPointCount;
    private static DelW8HideKeyboard DLL_W8HideKeyboard;
    private static DelW8Initialize DLL_W8Initialize;
    private static DelW8IsLastEventFromTouch DLL_W8IsLastEventFromTouch;
    private static DelW8IsVirtualKeyboardVisible DLL_W8IsVirtualKeyboardVisible;
    private static DelW8IsWindows8OrGreater DLL_W8IsWindows8OrGreater;
    private static DelW8ShowKeyboard DLL_W8ShowKeyboard;
    private static DelW8ShowOSK DLL_W8ShowOSK;
    private static DelW8Shutdown DLL_W8Shutdown;
    private bool m_bIsWindowFeedbackDisabled;
    private bool m_bWindowFeedbackSettingValue;
    private RECT m_desktopRect = new RECT();
    private int m_intializationAttemptCount;
    private bool m_isVirtualKeyboardHideRequested;
    private bool m_isVirtualKeyboardShowRequested;
    private bool m_isVirtualKeyboardVisible;
    private PowerSource m_lastPowerSourceState = PowerSource.Unintialized;
    private Vector2 m_touchDelta = new Vector2(0f, 0f);
    private Vector3 m_touchPosition = new Vector3(-1f, -1f, 0f);
    private TouchState[] m_touchState;
    private const int MaxInitializationAttempts = 10;
    private const int MaxTouches = 5;
    private static IntPtr s_DLL = IntPtr.Zero;
    public static bool s_initialized = false;
    private static W8Touch s_instance;
    public static bool s_isWindows8OrGreater = false;

    public event System.Action VirtualKeyboardDidHide;

    public event System.Action VirtualKeyboardDidShow;

    public static void AppQuit()
    {
        Log.Yim.Print("W8Touch.AppQuit()", new object[0]);
        if (s_DLL != IntPtr.Zero)
        {
            if (s_instance != null)
            {
                s_instance.ResetWindowFeedbackSetting();
            }
            if ((DLL_W8Shutdown != null) && s_initialized)
            {
                DLL_W8Shutdown();
                s_initialized = false;
            }
            if (!DLLUtils.FreeLibrary(s_DLL))
            {
                Debug.Log("Error unloading W8TouchDLL.dll");
            }
            s_DLL = IntPtr.Zero;
        }
    }

    private void Awake()
    {
        s_instance = this;
        if (this.LoadW8TouchDLL())
        {
            s_isWindows8OrGreater = DLL_W8IsWindows8OrGreater();
        }
    }

    private void Destroy()
    {
        s_instance = null;
    }

    [DllImport("User32.dll")]
    public static extern IntPtr FindWindow(string className, string windowName);
    public static W8Touch Get()
    {
        return s_instance;
    }

    public PowerSource GetBatteryMode()
    {
        if (!IsInitialized())
        {
            return PowerSource.Unintialized;
        }
        return (PowerSource) DLL_W8GetBatteryMode();
    }

    private IntPtr GetFunction(string name)
    {
        IntPtr procAddress = DLLUtils.GetProcAddress(s_DLL, name);
        if (procAddress == IntPtr.Zero)
        {
            Debug.LogError("Could not load W8TouchDLL." + name + "()");
            AppQuit();
        }
        return procAddress;
    }

    public string GetIntelDeviceName()
    {
        if (!IsInitialized())
        {
            return null;
        }
        return IntelDevice.GetDeviceName(DLL_W8GetDeviceId());
    }

    public int GetPercentBatteryLife()
    {
        if (!IsInitialized())
        {
            return -1;
        }
        return DLL_W8GetPercentBatteryLife();
    }

    public bool GetTouch(int touchCount)
    {
        if ((!IsInitialized() || (this.m_touchState == null)) || (touchCount >= 5))
        {
            return false;
        }
        if ((this.m_touchState[touchCount] != TouchState.InitialDown) && (this.m_touchState[touchCount] != TouchState.Down))
        {
            return false;
        }
        return true;
    }

    public Vector2 GetTouchDelta()
    {
        if (IsInitialized() && (this.m_touchState != null))
        {
            return new Vector2(this.m_touchDelta.x, this.m_touchDelta.y);
        }
        return new Vector2(0f, 0f);
    }

    public bool GetTouchDown(int touchCount)
    {
        if ((!IsInitialized() || (this.m_touchState == null)) || (touchCount >= 5))
        {
            return false;
        }
        return (this.m_touchState[touchCount] == TouchState.InitialDown);
    }

    public Vector3 GetTouchPosition()
    {
        if (IsInitialized() && (this.m_touchState != null))
        {
            return new Vector3(this.m_touchPosition.x, this.m_touchPosition.y, this.m_touchPosition.z);
        }
        return new Vector3(0f, 0f, 0f);
    }

    public Vector3 GetTouchPositionForGUI()
    {
        if (!IsInitialized() || (this.m_touchState == null))
        {
            return new Vector3(0f, 0f, 0f);
        }
        Vector2 vector = this.TransformTouchPosition(this.m_touchPosition);
        return new Vector3(vector.x, vector.y, this.m_touchPosition.z);
    }

    public bool GetTouchUp(int touchCount)
    {
        if ((!IsInitialized() || (this.m_touchState == null)) || (touchCount >= 5))
        {
            return false;
        }
        return (this.m_touchState[touchCount] == TouchState.InitialUp);
    }

    public void HideKeyboard()
    {
        if (IsInitialized() || this.m_isVirtualKeyboardVisible)
        {
            if (this.m_isVirtualKeyboardShowRequested)
            {
                this.m_isVirtualKeyboardShowRequested = false;
            }
            if (DLL_W8HideKeyboard() == 0)
            {
                this.m_isVirtualKeyboardHideRequested = true;
            }
        }
    }

    private void InitializeDLL()
    {
        if (this.m_intializationAttemptCount < 10)
        {
            string windowName = GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE");
            if (DLL_W8Initialize(windowName) < 0)
            {
                this.m_intializationAttemptCount++;
            }
            else
            {
                Log.Yim.Print("W8Touch Start Success!", new object[0]);
                s_initialized = true;
                IntPtr module = DLLUtils.LoadLibrary("User32.DLL");
                if (module == IntPtr.Zero)
                {
                    Log.Yim.Print("Could not load User32.DLL", new object[0]);
                }
                else
                {
                    IntPtr procAddress = DLLUtils.GetProcAddress(module, "SetWindowFeedbackSetting");
                    if (procAddress == IntPtr.Zero)
                    {
                        Log.Yim.Print("Could not load User32.SetWindowFeedbackSetting()", new object[0]);
                    }
                    else
                    {
                        IntPtr hwnd = FindWindow(null, "Hearthstone");
                        if (hwnd == IntPtr.Zero)
                        {
                            hwnd = FindWindow(null, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
                        }
                        if (hwnd == IntPtr.Zero)
                        {
                            Log.Yim.Print("Unable to retrieve Hearthstone window handle!", new object[0]);
                        }
                        else
                        {
                            DelSetWindowFeedbackSetting delegateForFunctionPointer = (DelSetWindowFeedbackSetting) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DelSetWindowFeedbackSetting));
                            int cb = Marshal.SizeOf(typeof(int));
                            IntPtr ptr = Marshal.AllocHGlobal(cb);
                            Marshal.WriteInt32(ptr, 0, !this.m_bWindowFeedbackSettingValue ? 0 : 1);
                            bool flag = true;
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_CONTACTVISUALIZATION, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_TOUCH_CONTACTVISUALIZATION failed!", new object[0]);
                                flag = false;
                            }
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_TAP, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_TOUCH_TAP failed!", new object[0]);
                                flag = false;
                            }
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_PRESSANDHOLD, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_TOUCH_PRESSANDHOLD failed!", new object[0]);
                                flag = false;
                            }
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_DOUBLETAP, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_TOUCH_DOUBLETAP failed!", new object[0]);
                                flag = false;
                            }
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_RIGHTTAP, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_TOUCH_RIGHTTAP failed!", new object[0]);
                                flag = false;
                            }
                            if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_GESTURE_PRESSANDTAP, 0, Convert.ToUInt32(cb), ptr))
                            {
                                Log.Yim.Print("FEEDBACK_GESTURE_PRESSANDTAP failed!", new object[0]);
                                flag = false;
                            }
                            this.m_bIsWindowFeedbackDisabled = flag;
                            if (this.m_bIsWindowFeedbackDisabled)
                            {
                                Log.Yim.Print("Windows 8 Feedback Touch Gestures Disabled!", new object[0]);
                            }
                            Marshal.FreeHGlobal(ptr);
                        }
                    }
                    if (!DLLUtils.FreeLibrary(module))
                    {
                        Log.Yim.Print("Error unloading User32.dll", new object[0]);
                    }
                }
            }
        }
    }

    private static bool IsInitialized()
    {
        return (((s_DLL != IntPtr.Zero) && s_isWindows8OrGreater) && s_initialized);
    }

    public bool IsVirtualKeyboardVisible()
    {
        if (!IsInitialized())
        {
            return false;
        }
        return this.m_isVirtualKeyboardVisible;
    }

    private bool LoadW8TouchDLL()
    {
        if ((Environment.OSVersion.Version.Major < 6) || ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor < 2)))
        {
            Log.Yim.Print("Windows Version is Pre-Windows 8", new object[0]);
            return false;
        }
        if (s_DLL == IntPtr.Zero)
        {
            s_DLL = FileUtils.LoadPlugin("W8TouchDLL", false);
            if (s_DLL == IntPtr.Zero)
            {
                Log.Yim.Print("Could not load W8TouchDLL.dll", new object[0]);
                return false;
            }
        }
        DLL_W8ShowKeyboard = (DelW8ShowKeyboard) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_ShowKeyboard"), typeof(DelW8ShowKeyboard));
        DLL_W8HideKeyboard = (DelW8HideKeyboard) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_HideKeyboard"), typeof(DelW8HideKeyboard));
        DLL_W8ShowOSK = (DelW8ShowOSK) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_ShowOSK"), typeof(DelW8ShowOSK));
        DLL_W8Initialize = (DelW8Initialize) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_Initialize"), typeof(DelW8Initialize));
        DLL_W8Shutdown = (DelW8Shutdown) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_Shutdown"), typeof(DelW8Shutdown));
        DLL_W8GetDeviceId = (DelW8GetDeviceId) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_GetDeviceId"), typeof(DelW8GetDeviceId));
        DLL_W8IsWindows8OrGreater = (DelW8IsWindows8OrGreater) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_IsWindows8OrGreater"), typeof(DelW8IsWindows8OrGreater));
        DLL_W8IsLastEventFromTouch = (DelW8IsLastEventFromTouch) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_IsLastEventFromTouch"), typeof(DelW8IsLastEventFromTouch));
        DLL_W8GetBatteryMode = (DelW8GetBatteryMode) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_GetBatteryMode"), typeof(DelW8GetBatteryMode));
        DLL_W8GetPercentBatteryLife = (DelW8GetPercentBatteryLife) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_GetPercentBatteryLife"), typeof(DelW8GetPercentBatteryLife));
        DLL_W8GetDesktopRect = (DelW8GetDesktopRect) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_GetDesktopRect"), typeof(DelW8GetDesktopRect));
        DLL_W8IsVirtualKeyboardVisible = (DelW8IsVirtualKeyboardVisible) Marshal.GetDelegateForFunctionPointer(this.GetFunction("W8_IsVirtualKeyboardVisible"), typeof(DelW8IsVirtualKeyboardVisible));
        DLL_W8GetTouchPointCount = (DelW8GetTouchPointCount) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetTouchPointCount"), typeof(DelW8GetTouchPointCount));
        DLL_W8GetTouchPoint = (DelW8GetTouchPoint) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetTouchPoint"), typeof(DelW8GetTouchPoint));
        return true;
    }

    private void OnGUI()
    {
        if ((s_isWindows8OrGreater || (s_DLL != IntPtr.Zero)) && !s_initialized)
        {
            this.InitializeDLL();
        }
    }

    private void ResetWindowFeedbackSetting()
    {
        if (s_initialized && this.m_bIsWindowFeedbackDisabled)
        {
            IntPtr module = DLLUtils.LoadLibrary("User32.DLL");
            if (module == IntPtr.Zero)
            {
                Log.Yim.Print("Could not load User32.DLL", new object[0]);
            }
            else
            {
                IntPtr procAddress = DLLUtils.GetProcAddress(module, "SetWindowFeedbackSetting");
                if (procAddress == IntPtr.Zero)
                {
                    Log.Yim.Print("Could not load User32.SetWindowFeedbackSetting()", new object[0]);
                }
                else
                {
                    IntPtr hwnd = FindWindow(null, "Hearthstone");
                    if (hwnd == IntPtr.Zero)
                    {
                        hwnd = FindWindow(null, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
                    }
                    if (hwnd == IntPtr.Zero)
                    {
                        Log.Yim.Print("Unable to retrieve Hearthstone window handle!", new object[0]);
                    }
                    else
                    {
                        DelSetWindowFeedbackSetting delegateForFunctionPointer = (DelSetWindowFeedbackSetting) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DelSetWindowFeedbackSetting));
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                        Marshal.WriteInt32(ptr, 0, !this.m_bWindowFeedbackSettingValue ? 0 : 1);
                        bool flag = true;
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_CONTACTVISUALIZATION, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_TOUCH_CONTACTVISUALIZATION failed!", new object[0]);
                            flag = false;
                        }
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_TAP, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_TOUCH_TAP failed!", new object[0]);
                            flag = false;
                        }
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_PRESSANDHOLD, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_TOUCH_PRESSANDHOLD failed!", new object[0]);
                            flag = false;
                        }
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_DOUBLETAP, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_TOUCH_DOUBLETAP failed!", new object[0]);
                            flag = false;
                        }
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_TOUCH_RIGHTTAP, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_TOUCH_RIGHTTAP failed!", new object[0]);
                            flag = false;
                        }
                        if (!delegateForFunctionPointer(hwnd, FEEDBACK_TYPE.FEEDBACK_GESTURE_PRESSANDTAP, 0, 0, IntPtr.Zero))
                        {
                            Log.Yim.Print("FEEDBACK_GESTURE_PRESSANDTAP failed!", new object[0]);
                            flag = false;
                        }
                        this.m_bIsWindowFeedbackDisabled = !flag;
                        if (!this.m_bIsWindowFeedbackDisabled)
                        {
                            Log.Yim.Print("Windows 8 Feedback Touch Gestures Reset!", new object[0]);
                        }
                        Marshal.FreeHGlobal(ptr);
                    }
                }
                if (!DLLUtils.FreeLibrary(module))
                {
                    Log.Yim.Print("Error unloading User32.dll", new object[0]);
                }
            }
        }
    }

    public void ShowKeyboard()
    {
        if ((IsInitialized() && !this.m_isVirtualKeyboardShowRequested) && (!this.m_isVirtualKeyboardVisible || this.m_isVirtualKeyboardHideRequested))
        {
            if (this.m_isVirtualKeyboardHideRequested)
            {
                this.m_isVirtualKeyboardHideRequested = false;
            }
            KeyboardFlags flags = (KeyboardFlags) DLL_W8ShowKeyboard();
            if ((flags & KeyboardFlags.Shown) != KeyboardFlags.Shown)
            {
            }
            if (((flags & KeyboardFlags.Shown) == KeyboardFlags.Shown) && ((flags & KeyboardFlags.SuccessTabTip) == KeyboardFlags.SuccessTabTip))
            {
                this.m_isVirtualKeyboardShowRequested = true;
            }
        }
    }

    public void ShowOSK()
    {
        if (IsInitialized() && ((DLL_W8ShowOSK() & 1) != 1))
        {
        }
    }

    private void Start()
    {
        this.m_touchState = new TouchState[5];
        for (int i = 0; i < 5; i++)
        {
            this.m_touchState[i] = TouchState.None;
        }
    }

    private void ToggleTouchMode()
    {
        if (IsInitialized())
        {
            bool @bool = Options.Get().GetBool(Option.TOUCH_MODE);
            Options.Get().SetBool(Option.TOUCH_MODE, !@bool);
        }
    }

    private Vector2 TransformTouchPosition(Vector2 touchInput)
    {
        Vector2 vector = new Vector2();
        if (Screen.fullScreen)
        {
            float num = ((float) Screen.width) / ((float) Screen.height);
            float num2 = ((float) this.m_desktopRect.Right) / ((float) this.m_desktopRect.Bottom);
            if (Mathf.Abs((float) (num - num2)) < Mathf.Epsilon)
            {
                float num3 = ((float) Screen.width) / ((float) this.m_desktopRect.Right);
                float num4 = ((float) Screen.height) / ((float) this.m_desktopRect.Bottom);
                vector.x = touchInput.x * num3;
                vector.y = (this.m_desktopRect.Bottom - touchInput.y) * num4;
                return vector;
            }
            if (num < num2)
            {
                float bottom = this.m_desktopRect.Bottom;
                float num6 = bottom * num;
                float num7 = ((float) Screen.height) / bottom;
                float num8 = ((float) Screen.width) / num6;
                float num9 = (this.m_desktopRect.Right - num6) / 2f;
                vector.x = (touchInput.x - num9) * num8;
                vector.y = (this.m_desktopRect.Bottom - touchInput.y) * num7;
                return vector;
            }
            float right = this.m_desktopRect.Right;
            float num11 = right / num;
            float num12 = ((float) Screen.height) / num11;
            float num13 = ((float) Screen.width) / right;
            float num14 = (this.m_desktopRect.Bottom - num11) / 2f;
            vector.x = touchInput.x * num13;
            vector.y = ((this.m_desktopRect.Bottom - touchInput.y) - num14) * num12;
            return vector;
        }
        vector.x = touchInput.x;
        vector.y = Screen.height - touchInput.y;
        return vector;
    }

    private void Update()
    {
        if (IsInitialized())
        {
            DLL_W8GetDesktopRect(out this.m_desktopRect);
            bool flag = DLL_W8IsVirtualKeyboardVisible();
            if (flag != this.m_isVirtualKeyboardVisible)
            {
                this.m_isVirtualKeyboardVisible = flag;
                if (flag && (this.VirtualKeyboardDidShow != null))
                {
                    this.VirtualKeyboardDidShow();
                }
                else if (!flag && (this.VirtualKeyboardDidHide != null))
                {
                    this.VirtualKeyboardDidHide();
                }
            }
            if (this.m_isVirtualKeyboardVisible)
            {
                this.m_isVirtualKeyboardShowRequested = false;
            }
            else
            {
                this.m_isVirtualKeyboardHideRequested = false;
            }
            PowerSource batteryMode = this.GetBatteryMode();
            if (batteryMode != this.m_lastPowerSourceState)
            {
                object[] args = new object[] { batteryMode };
                Log.Yim.Print("PowerSource Change Detected: {0}", args);
                this.m_lastPowerSourceState = batteryMode;
                GraphicsManager.Get().RenderQualityLevel = (GraphicsQuality) Options.Get().GetInt(Option.GFX_QUALITY);
            }
            if ((!DLL_W8IsLastEventFromTouch() && UniversalInputManager.Get().IsTouchMode()) || (DLL_W8IsLastEventFromTouch() && !UniversalInputManager.Get().IsTouchMode()))
            {
                this.ToggleTouchMode();
            }
            if (this.m_touchState != null)
            {
                int num = DLL_W8GetTouchPointCount();
                for (int i = 0; i < 5; i++)
                {
                    tTouchData n = new tTouchData();
                    bool flag2 = false;
                    if (i < num)
                    {
                        flag2 = DLL_W8GetTouchPoint(i, n);
                    }
                    if (flag2 && (i == 0))
                    {
                        Vector2 vector = this.TransformTouchPosition(new Vector2((float) n.m_x, (float) n.m_y));
                        if (((this.m_touchPosition.x != -1f) && (this.m_touchPosition.y != -1f)) && (this.m_touchState[i] == TouchState.Down))
                        {
                            this.m_touchDelta.x = vector.x - this.m_touchPosition.x;
                            this.m_touchDelta.y = vector.y - this.m_touchPosition.y;
                        }
                        else
                        {
                            this.m_touchDelta.x = this.m_touchDelta.y = 0f;
                        }
                        this.m_touchPosition.x = vector.x;
                        this.m_touchPosition.y = vector.y;
                    }
                    if (flag2 && (n.m_ID != -1))
                    {
                        if ((this.m_touchState[i] == TouchState.Down) || (this.m_touchState[i] == TouchState.InitialDown))
                        {
                            this.m_touchState[i] = TouchState.Down;
                        }
                        else
                        {
                            this.m_touchState[i] = TouchState.InitialDown;
                        }
                    }
                    else if ((this.m_touchState[i] == TouchState.Down) || (this.m_touchState[i] == TouchState.InitialDown))
                    {
                        this.m_touchState[i] = TouchState.InitialUp;
                    }
                    else
                    {
                        this.m_touchState[i] = TouchState.None;
                    }
                }
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelSetWindowFeedbackSetting(IntPtr hwnd, W8Touch.FEEDBACK_TYPE feedback, uint dwFlags, uint size, IntPtr configuration);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8GetBatteryMode();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelW8GetDesktopRect(out W8Touch.RECT desktopRect);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8GetDeviceId();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8GetPercentBatteryLife();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelW8GetTouchPoint(int i, W8Touch.tTouchData n);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8GetTouchPointCount();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8HideKeyboard();

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet=CharSet.Auto)]
    private delegate int DelW8Initialize(string windowName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelW8IsLastEventFromTouch();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelW8IsVirtualKeyboardVisible();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelW8IsWindows8OrGreater();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8ShowKeyboard();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DelW8ShowOSK();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelW8Shutdown();

    public enum FEEDBACK_TYPE
    {
        FEEDBACK_GESTURE_PRESSANDTAP = 11,
        FEEDBACK_PEN_BARRELVISUALIZATION = 2,
        FEEDBACK_PEN_DOUBLETAP = 4,
        FEEDBACK_PEN_PRESSANDHOLD = 5,
        FEEDBACK_PEN_RIGHTTAP = 6,
        FEEDBACK_PEN_TAP = 3,
        FEEDBACK_TOUCH_CONTACTVISUALIZATION = 1,
        FEEDBACK_TOUCH_DOUBLETAP = 8,
        FEEDBACK_TOUCH_PRESSANDHOLD = 9,
        FEEDBACK_TOUCH_RIGHTTAP = 10,
        FEEDBACK_TOUCH_TAP = 7
    }

    public class IntelDevice
    {
        private static readonly Map<int, string> DeviceIdMap;

        static IntelDevice()
        {
            Map<int, string> map = new Map<int, string>();
            map.Add(0x7800, "Auburn");
            map.Add(0x7121, "Whitney");
            map.Add(0x7123, "Whitney");
            map.Add(0x7125, "Whitney");
            map.Add(0x1132, "Solono");
            map.Add(0x2562, "Brookdale");
            map.Add(0x3582, "Montara");
            map.Add(0x2572, "Springdale");
            map.Add(0x2582, "Grantsdale");
            map.Add(0x2782, "Grantsdale");
            map.Add(0x2592, "Alviso");
            map.Add(0x2792, "Alviso");
            map.Add(0x2772, "Lakeport-G");
            map.Add(0x2776, "Lakeport-G");
            map.Add(0x27a2, "Calistoga");
            map.Add(0x27a6, "Calistoga");
            map.Add(0x2982, "Broadwater-G");
            map.Add(0x2983, "Broadwater-G");
            map.Add(0x2972, "Broadwater-G");
            map.Add(0x2973, "Broadwater-G");
            map.Add(0x2992, "Broadwater-G");
            map.Add(0x2993, "Broadwater-G");
            map.Add(0x29a2, "Broadwater-G");
            map.Add(0x29a3, "Broadwater-G");
            map.Add(0x2a02, "Crestline");
            map.Add(0x2a03, "Crestline");
            map.Add(0x2a12, "Crestline");
            map.Add(0x2a13, "Crestline");
            map.Add(0x29b2, "Bearlake");
            map.Add(0x29b3, "Bearlake");
            map.Add(0x29c2, "Bearlake");
            map.Add(0x29c3, "Bearlake");
            map.Add(0x29d2, "Bearlake");
            map.Add(0x29d3, "Bearlake");
            map.Add(0x2a42, "Cantiga");
            map.Add(0x2a43, "Cantiga");
            map.Add(0x2e02, "Eaglelake");
            map.Add(0x2e03, "Eaglelake");
            map.Add(0x2e22, "Eaglelake");
            map.Add(0x2e23, "Eaglelake");
            map.Add(0x2e12, "Eaglelake");
            map.Add(0x2e13, "Eaglelake");
            map.Add(0x2e32, "Eaglelake");
            map.Add(0x2e33, "Eaglelake");
            map.Add(0x2e42, "Eaglelake");
            map.Add(0x2e43, "Eaglelake");
            map.Add(0x2e92, "Eaglelake");
            map.Add(0x2e93, "Eaglelake");
            map.Add(70, "Arrandale");
            map.Add(0x42, "Clarkdale");
            map.Add(0x106, "Mobile_SandyBridge_GT1");
            map.Add(0x116, "Mobile_SandyBridge_GT2");
            map.Add(0x126, "Mobile_SandyBridge_GT2+");
            map.Add(0x102, "DT_SandyBridge_GT2+");
            map.Add(0x112, "DT_SandyBridge_GT2+");
            map.Add(290, "DT_SandyBridge_GT2+");
            map.Add(0x10a, "SandyBridge_Server");
            map.Add(270, "SandyBridge_Reserved");
            map.Add(0x152, "Desktop_IvyBridge_GT1");
            map.Add(0x156, "Mobile_IvyBridge_GT1");
            map.Add(0x15a, "Server_IvyBridge_GT1");
            map.Add(350, "Reserved_IvyBridge_GT1");
            map.Add(0x162, "Desktop_IvyBridge_GT2");
            map.Add(0x166, "Mobile_IvyBridge_GT2");
            map.Add(0x16a, "Server_IvyBridge_GT2");
            map.Add(0x402, "Desktop_Haswell_GT1_Y6W");
            map.Add(0x406, "Mobile_Haswell_GT1_Y6W");
            map.Add(0x40a, "Server_Haswell_GT1");
            map.Add(0x412, "Desktop_Haswell_GT2_U15W");
            map.Add(0x416, "Mobile_Haswell_GT2_U15W");
            map.Add(0x41b, "Workstation_Haswell_GT2");
            map.Add(0x41a, "Server_Haswell_GT2");
            map.Add(0x41e, "Reserved_Haswell_DT_GT1.5_U15W");
            map.Add(0xa06, "Mobile_Haswell_ULT_GT1_Y6W");
            map.Add(0xa0e, "Mobile_Haswell_ULX_GT1_Y6W");
            map.Add(0xa16, "Mobile_Haswell_ULT_GT2_U15W");
            map.Add(0xa1e, "Mobile_Haswell_ULX_GT2_Y6W");
            map.Add(0xa26, "Mobile_Haswell_ULT_GT3_U28W");
            map.Add(0xa2e, "Mobile_Haswell_ULT_GT3@28_U28W");
            map.Add(0xd12, "Desktop_Haswell_GT2F");
            map.Add(0xd16, "Mobile_Haswell_GT2F");
            map.Add(0xd22, "Desktop_Crystal-Well_GT3");
            map.Add(0xd26, "Mobile_Crystal-Well_GT3");
            map.Add(0xd2a, "Server_Crystal-Well_GT3");
            map.Add(0xf31, "BayTrail");
            map.Add(0x8108, "Poulsbo");
            map.Add(0x8109, "Poulsbo");
            map.Add(0x8cf, "CloverTrail");
            map.Add(0xa001, "CloverTrail");
            map.Add(0xa002, "CloverTrail");
            map.Add(0xa011, "CloverTrail");
            map.Add(0xa012, "CloverTrail");
            DeviceIdMap = map;
        }

        public static string GetDeviceName(int deviceId)
        {
            string str;
            if (!DeviceIdMap.TryGetValue(deviceId, out str))
            {
                return string.Empty;
            }
            return str;
        }
    }

    [Flags]
    public enum KeyboardFlags
    {
        ErrorOSK = 0x20,
        ErrorTabTip = 0x10,
        NotFoundOSK = 0x80,
        NotFoundTabTip = 0x40,
        NotShown = 2,
        Shown = 1,
        SuccessOSK = 8,
        SuccessTabTip = 4
    }

    public enum PowerSource
    {
        ACPower = 1,
        BatteryPower = 0,
        UndefinedPower = 0xff,
        Unintialized = -1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public enum TouchState
    {
        None,
        InitialDown,
        Down,
        InitialUp
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public class tTouchData
    {
        public int m_x;
        public int m_y;
        public int m_ID;
        public int m_Time;
    }
}

