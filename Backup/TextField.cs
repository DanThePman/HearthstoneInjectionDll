using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextField : PegUIElement
{
    public bool autocorrect;
    public bool hideInput = true;
    public Transform inputBottomRight;
    public bool inputKeepFocusOnComplete;
    private UniversalInputManager.TextInputParams inputParams;
    public Transform inputTopLeft;
    private static TextField instance;
    public KeyboardType keyboardType;
    private Vector3 lastBottomRight;
    private Rect lastBounds = new Rect();
    private Vector3 lastTopLeft;
    private Font m_InputFont;
    public int maxCharacters;
    private static uint nextId;
    public KeyboardReturnKeyType returnKeyType;
    public Color textColor;
    public bool useNativeKeyboard;

    public event System.Action Canceled;

    public event Action<string> Changed;

    public event Action<Event> Preprocess;

    public event Action<string> Submitted;

    public void Activate()
    {
        Log.Cameron.Print("TextField::Activate", new object[0]);
        if ((instance != null) && (instance != this))
        {
            instance.Deactivate();
        }
        instance = this;
        this.lastBounds = this.ComputeBounds();
        KeyboardArea = (Rect) ActivateTextField(base.gameObject.name, this.lastBounds, !this.autocorrect ? 0 : 1, (uint) this.keyboardType, (uint) this.returnKeyType);
        SetTextFieldColor(this.textColor.r, this.textColor.g, this.textColor.b, this.textColor.a);
        SetTextFieldMaxCharacters(0x200);
    }

    private static PluginRect ActivateTextField(string name, PluginRect bounds, int autocorrect, uint keyboardType, uint returnKeyType)
    {
        Log.Cameron.Print(string.Concat(new object[] { "activate text field ", name, " ", bounds }), new object[0]);
        if (UseNativeKeyboard())
        {
            return Plugin_ActivateTextField(name, bounds, autocorrect, keyboardType, returnKeyType);
        }
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.TextInputParams @params = new UniversalInputManager.TextInputParams {
                m_owner = instance.gameObject,
                m_preprocessCallback = new UniversalInputManager.TextInputPreprocessCallback(instance.OnPreprocess),
                m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(instance.OnSubmitted),
                m_updatedCallback = new UniversalInputManager.TextInputUpdatedCallback(instance.OnChanged),
                m_canceledCallback = new UniversalInputManager.TextInputCanceledCallback(instance.InputCanceled),
                m_font = instance.m_InputFont,
                m_maxCharacters = instance.maxCharacters,
                m_inputKeepFocusOnComplete = instance.inputKeepFocusOnComplete,
                m_touchScreenKeyboardHideInput = instance.hideInput,
                m_useNativeKeyboard = UseNativeKeyboard()
            };
            instance.inputParams = @params;
            UniversalInputManager.Get().UseTextInput(instance.inputParams, false);
            SetTextFieldBounds(bounds);
            if (instance.Active)
            {
                return new Rect(0f, (float) Screen.height, (float) Screen.width, Screen.height * 0.5f);
            }
        }
        return new PluginRect();
    }

    protected override void Awake()
    {
        base.Awake();
        base.gameObject.name = string.Format("TextField_{0:000}", nextId++);
        if (base.gameObject.GetComponent<BoxCollider>() == null)
        {
            base.gameObject.AddComponent<BoxCollider>();
        }
        this.UpdateCollider();
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private Rect ComputeBounds()
    {
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        Vector2 lhs = camera.WorldToScreenPoint(this.inputTopLeft.transform.position);
        Vector2 rhs = camera.WorldToScreenPoint(this.inputBottomRight.transform.position);
        lhs.y = Screen.height - lhs.y;
        rhs.y = Screen.height - rhs.y;
        Vector2 vector3 = Vector2.Min(lhs, rhs);
        Vector2 vector4 = Vector2.Max(lhs, rhs);
        float left = Mathf.Round(vector3.x);
        float right = Mathf.Round(vector4.x);
        return Rect.MinMaxRect(left, Mathf.Round(vector3.y), right, Mathf.Round(vector4.y));
    }

    public void Deactivate()
    {
        Log.Cameron.Print("TextField::Deactivate", new object[0]);
        if (this == instance)
        {
            KeyboardArea = new Rect();
            DeactivateTextField();
            instance = null;
        }
    }

    private static void DeactivateTextField()
    {
        Log.Cameron.Print("deactivating text field " + instance.name, new object[0]);
        if (UseNativeKeyboard())
        {
            Plugin_DeactivateTextField();
        }
        else if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().CancelTextInput(instance.gameObject, false);
        }
    }

    private static string GetTextFieldText()
    {
        if (UseNativeKeyboard())
        {
            return Plugin_GetTextFieldText();
        }
        return UniversalInputManager.Get().GetInputText();
    }

    private void InputCanceled(bool userRequested, GameObject requester)
    {
        this.OnCanceled();
    }

    private void OnCanceled()
    {
        if (this.Canceled != null)
        {
            this.Canceled();
        }
        this.Deactivate();
    }

    private void OnChanged(string text)
    {
        if (this.Changed != null)
        {
            this.Changed(text);
        }
    }

    private void OnDestroy()
    {
        if (this.Active)
        {
            this.Deactivate();
        }
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.Deactivate();
    }

    private void OnKeyboardAreaChanged(Rect area)
    {
        KeyboardArea = area;
    }

    private bool OnPreprocess(Event e)
    {
        if (this.Preprocess != null)
        {
            this.Preprocess(e);
        }
        return false;
    }

    protected override void OnRelease()
    {
        if (!this.Active)
        {
            this.Activate();
        }
    }

    private void OnSubmitted(string text)
    {
        if (this.Submitted != null)
        {
            this.Submitted(text);
        }
    }

    private static PluginRect Plugin_ActivateTextField(string name, [MarshalAs(UnmanagedType.Struct)] PluginRect bounds, int autocorrect, uint keyboardType, uint returnKeyType)
    {
        return new PluginRect();
    }

    private static void Plugin_DeactivateTextField()
    {
    }

    private static string Plugin_GetTextFieldText()
    {
        return string.Empty;
    }

    private static void Plugin_SetTextFieldBounds([MarshalAs(UnmanagedType.Struct)] PluginRect bounds)
    {
    }

    private static void Plugin_SetTextFieldColor(float r, float g, float b, float a)
    {
    }

    private static void Plugin_SetTextFieldMaxCharacters(int maxCharacters)
    {
    }

    private static void Plugin_SetTextFieldText(string text)
    {
    }

    public void SetInputFont(Font font)
    {
        this.m_InputFont = font;
    }

    private static void SetTextFieldBounds(PluginRect bounds)
    {
        Log.Cameron.Print("TextField::SetTextFieldBounds " + bounds, new object[0]);
        if (UseNativeKeyboard())
        {
            Plugin_SetTextFieldBounds(bounds);
        }
        else
        {
            bounds.x /= (float) Screen.width;
            bounds.y /= (float) Screen.height;
            bounds.width /= (float) Screen.width;
            bounds.height /= (float) Screen.height;
            if (instance.inputParams.m_rect != bounds)
            {
                instance.inputParams.m_rect = (Rect) bounds;
                instance.UpdateTextInput();
            }
        }
    }

    private static void SetTextFieldColor(float r, float g, float b, float a)
    {
        if (UseNativeKeyboard())
        {
            Plugin_SetTextFieldColor(r, g, b, a);
        }
    }

    private static void SetTextFieldMaxCharacters(int maxCharacters)
    {
        if (UseNativeKeyboard())
        {
            Plugin_SetTextFieldMaxCharacters(maxCharacters);
        }
        else if (maxCharacters != instance.maxCharacters)
        {
            instance.maxCharacters = maxCharacters;
            instance.UpdateTextInput();
        }
    }

    private static void SetTextFieldText(string text)
    {
        if (UseNativeKeyboard())
        {
            Plugin_SetTextFieldText(text);
        }
        else
        {
            UniversalInputManager.Get().SetInputText(text);
        }
    }

    private void Unity_KeyboardAreaChanged(string rectString)
    {
        if (this.Active)
        {
            Match match = Regex.Match(rectString, string.Format(@"x\: (?<x>{0})\, y\: (?<y>{0})\, width\: (?<width>{0})\, height\: (?<height>{0})", @"[-+]?[0-9]*\.?[0-9]+"));
            Rect area = new Rect(float.Parse(match.Groups["x"].Value), float.Parse(match.Groups["y"].Value), float.Parse(match.Groups["width"].Value), float.Parse(match.Groups["height"].Value));
            this.OnKeyboardAreaChanged(area);
        }
    }

    private void Unity_TextInputCanceled(string unused)
    {
        if (this.Active)
        {
            this.OnCanceled();
        }
    }

    private void Unity_TextInputChanged(string text)
    {
        if (this.Active)
        {
            this.OnChanged(text);
        }
    }

    private void Unity_TextInputSubmitted(string text)
    {
        if (this.Active)
        {
            this.OnSubmitted(text);
        }
    }

    private void Update()
    {
        if ((this.lastTopLeft != this.inputTopLeft.position) || (this.lastBottomRight != this.inputBottomRight.position))
        {
            this.UpdateCollider();
            this.lastTopLeft = this.inputTopLeft.position;
            this.lastBottomRight = this.inputBottomRight.position;
        }
        if (this.Active)
        {
            Rect bounds = this.ComputeBounds();
            if (bounds != this.lastBounds)
            {
                this.lastBounds = bounds;
                SetTextFieldBounds(bounds);
            }
        }
    }

    private void UpdateCollider()
    {
        BoxCollider component = base.GetComponent<BoxCollider>();
        Vector3 vector = base.transform.InverseTransformPoint(this.inputTopLeft.transform.position);
        Vector3 vector2 = base.transform.InverseTransformPoint(this.inputBottomRight.transform.position);
        component.center = (Vector3) ((vector + vector2) / 2f);
        component.size = VectorUtils.Abs(vector2 - vector);
    }

    private void UpdateTextInput()
    {
        UniversalInputManager.Get().UseTextInput(instance.inputParams, true);
        UniversalInputManager.Get().FocusTextInput(instance.gameObject);
    }

    private static bool UseNativeKeyboard()
    {
        return false;
    }

    public bool Active
    {
        get
        {
            return (this == instance);
        }
    }

    public static Rect KeyboardArea
    {
        [CompilerGenerated]
        get
        {
            return <KeyboardArea>k__BackingField;
        }
        [CompilerGenerated]
        private set
        {
            <KeyboardArea>k__BackingField = value;
        }
    }

    public string Text
    {
        get
        {
            return GetTextFieldText();
        }
        set
        {
            SetTextFieldText(value);
        }
    }

    public enum KeyboardReturnKeyType
    {
        Default,
        Go,
        Google,
        Join,
        Next,
        Route,
        Search,
        Send,
        Yahoo,
        Done,
        EmergencyCall
    }

    public enum KeyboardType
    {
        Default,
        ASCIICapable,
        NumbersAndPunctuation,
        URL,
        NumberPad,
        PhonePad,
        NamePhonePad,
        EmailAddress,
        DecimalPad,
        Twitter
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PluginRect
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public PluginRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.x, this.y, this.width, this.height };
            return string.Format("[x: {0}, y: {1}, width: {2}, height: {3}]", args);
        }

        public static implicit operator TextField.PluginRect(Rect rect)
        {
            return new TextField.PluginRect(rect.x, rect.y, rect.width, rect.height);
        }

        public static implicit operator Rect(TextField.PluginRect rect)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }
    }
}

