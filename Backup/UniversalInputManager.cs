using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class UniversalInputManager : MonoBehaviour
{
    private static readonly GameLayer[] HIT_TEST_PRIORITY_ORDER;
    private static bool IsIMEEverUsed;
    private static readonly PlatformDependentValue<bool> IsTouchDevice;
    private List<Camera> m_CameraMaskCameras = new List<Camera>();
    private TextAnchor m_defaultInputAlignment;
    private Font m_defaultInputFont;
    private Camera m_FullscreenEffectsCamera;
    private bool m_FullscreenEffectsCameraActive;
    private bool m_gameDialogActive;
    public bool m_hideVirtualKeyboardOnComplete = true;
    private Map<GameLayer, int> m_hitTestPriorityMap;
    private List<Camera> m_ignoredCameras = new List<Camera>();
    private bool m_inputActive;
    private TextAnchor m_inputAlignment;
    private TextInputCanceledCallback m_inputCanceledCallback;
    private Color? m_inputColor;
    private TextInputCompletedCallback m_inputCompletedCallback;
    private bool m_inputFocused;
    private Font m_inputFont;
    private TextInputIgnoreState m_inputIgnoreState;
    private Vector2 m_inputInitialScreenSize;
    private bool m_inputKeepFocusOnComplete;
    private int m_inputMaxCharacters;
    private bool m_inputMultiLine;
    private bool m_inputNeedsFocus;
    private bool m_inputNeedsFocusFromTabKeyDown;
    private Rect m_inputNormalizedRect;
    private bool m_inputNumber;
    private GameObject m_inputOwner;
    private bool m_inputPassword;
    private TextInputPreprocessCallback m_inputPreprocessCallback;
    private string m_inputText;
    private TextInputUpdatedCallback m_inputUpdatedCallback;
    private List<MouseOnOrOffScreenCallback> m_mouseOnOrOffScreenListeners = new List<MouseOnOrOffScreenCallback>();
    private bool m_mouseOnScreen;
    private int m_offCameraHitTestMask;
    private GUISkin m_skin;
    private GUISkinContainer m_skinContainer;
    private bool m_systemDialogActive;
    private bool m_tabKeyDown;
    private static UniversalInputManager s_instance;
    private const int TEXT_INPUT_FONT_SIZE_INSET = 4;
    private const int TEXT_INPUT_IME_FONT_SIZE_INSET = 9;
    private const int TEXT_INPUT_MAX_FONT_SIZE = 0x20;
    private const int TEXT_INPUT_MIN_FONT_SIZE = 2;
    private const string TEXT_INPUT_NAME = "UniversalInputManagerTextInput";
    private const float TEXT_INPUT_RECT_HEIGHT_OFFSET = 3f;
    public static readonly PlatformDependentValue<bool> UsePhoneUI;

    static UniversalInputManager()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Input) {
            Mouse = false,
            Touch = true
        };
        IsTouchDevice = value2;
        HIT_TEST_PRIORITY_ORDER = new GameLayer[] { GameLayer.IgnoreFullScreenEffects, GameLayer.BackgroundUI, GameLayer.PerspectiveUI, GameLayer.CameraMask, GameLayer.UI, GameLayer.BattleNet, GameLayer.BattleNetFriendList, GameLayer.BattleNetChat, GameLayer.BattleNetDialog };
        IsIMEEverUsed = false;
        value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            Phone = true,
            Tablet = false,
            PC = false
        };
        UsePhoneUI = value2;
    }

    public bool AddCameraMaskCamera(Camera camera)
    {
        if (this.m_CameraMaskCameras.Contains(camera))
        {
            return false;
        }
        this.m_CameraMaskCameras.Add(camera);
        return true;
    }

    public bool AddIgnoredCamera(Camera camera)
    {
        if (this.m_ignoredCameras.Contains(camera))
        {
            return false;
        }
        this.m_ignoredCameras.Add(camera);
        return true;
    }

    private void Awake()
    {
        s_instance = this;
        this.CreateHitTestPriorityMap();
        this.m_FullscreenEffectsCamera = CameraUtils.FindFullScreenEffectsCamera(true);
        if (this.m_FullscreenEffectsCamera != null)
        {
            this.m_FullscreenEffectsCameraActive = true;
        }
    }

    private void CancelTextInput(bool userRequested, GameObject requester)
    {
        if (this.IsTextInputPassword())
        {
            Input.imeCompositionMode = IMECompositionMode.Auto;
        }
        if ((requester != null) && (requester == this.m_inputOwner))
        {
            this.ClearTextInputVars();
        }
        else
        {
            TextInputCanceledCallback inputCanceledCallback = this.m_inputCanceledCallback;
            this.ClearTextInputVars();
            if (inputCanceledCallback != null)
            {
                inputCanceledCallback(userRequested, requester);
            }
        }
        if (this.IsTouchMode())
        {
            W8Touch.Get().HideKeyboard();
        }
    }

    public void CancelTextInput(GameObject requester, bool force = false)
    {
        if (this.IsTextInputActive() && (force || (requester == this.m_inputOwner)))
        {
            this.ObjectCancelTextInput(requester);
        }
    }

    public bool CanHitTestOffCamera(GameLayer layer)
    {
        return this.CanHitTestOffCamera(layer.LayerBit());
    }

    public bool CanHitTestOffCamera(LayerMask layerMask)
    {
        return ((this.m_offCameraHitTestMask & layerMask) != 0);
    }

    private void CleanDeadCameras()
    {
        GeneralUtils.CleanDeadObjectsFromList<Camera>(this.m_CameraMaskCameras);
        GeneralUtils.CleanDeadObjectsFromList<Camera>(this.m_ignoredCameras);
    }

    private void ClearTextInputVars()
    {
        this.m_inputActive = false;
        this.m_inputFocused = false;
        this.m_inputOwner = null;
        this.m_inputMaxCharacters = 0;
        this.m_inputUpdatedCallback = null;
        this.m_inputCompletedCallback = null;
        this.m_inputCanceledCallback = null;
    }

    private void CompleteTextInput()
    {
        if (this.IsTextInputPassword())
        {
            Input.imeCompositionMode = IMECompositionMode.Auto;
        }
        TextInputCompletedCallback inputCompletedCallback = this.m_inputCompletedCallback;
        if (!this.m_inputKeepFocusOnComplete)
        {
            this.ClearTextInputVars();
        }
        if (inputCompletedCallback != null)
        {
            inputCompletedCallback(this.m_inputText);
        }
        this.m_inputText = string.Empty;
        if (this.IsTouchMode() && this.m_hideVirtualKeyboardOnComplete)
        {
            W8Touch.Get().HideKeyboard();
        }
    }

    private int ComputeTextInputFontSize(Vector2 screenSize, float rectHeight)
    {
        int num = Mathf.CeilToInt(rectHeight);
        if (Localization.IsIMELocale() || IsIMEEverUsed)
        {
            num -= 9;
        }
        else
        {
            num -= 4;
        }
        return Mathf.Clamp(num, 2, 0x20);
    }

    private Rect ComputeTextInputRect(Vector2 screenSize)
    {
        float num = screenSize.x / screenSize.y;
        float num2 = this.m_inputInitialScreenSize.x / this.m_inputInitialScreenSize.y;
        float num3 = num2 / num;
        float num4 = screenSize.y / this.m_inputInitialScreenSize.y;
        float num5 = (0.5f - this.m_inputNormalizedRect.x) * this.m_inputInitialScreenSize.x;
        float num6 = num5 * num4;
        return new Rect((screenSize.x * 0.5f) - num6, (this.m_inputNormalizedRect.y * screenSize.y) - 1.5f, (this.m_inputNormalizedRect.width * screenSize.x) * num3, (this.m_inputNormalizedRect.height * screenSize.y) + 1.5f);
    }

    private void CreateHitTestPriorityMap()
    {
        this.m_hitTestPriorityMap = new Map<GameLayer, int>();
        int num = 1;
        for (int i = 0; i < HIT_TEST_PRIORITY_ORDER.Length; i++)
        {
            GameLayer key = HIT_TEST_PRIORITY_ORDER[i];
            this.m_hitTestPriorityMap.Add(key, num++);
        }
        IEnumerator enumerator = Enum.GetValues(typeof(GameLayer)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                GameLayer current = (GameLayer) ((int) enumerator.Current);
                if (!this.m_hitTestPriorityMap.ContainsKey(current))
                {
                    this.m_hitTestPriorityMap.Add(current, 0);
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
    }

    public void EnableHitTestOffCamera(GameLayer layer, bool enable)
    {
        this.EnableHitTestOffCamera(layer.LayerBit(), enable);
    }

    public void EnableHitTestOffCamera(LayerMask mask, bool enable)
    {
        if (enable)
        {
            this.m_offCameraHitTestMask |= mask;
        }
        else
        {
            this.m_offCameraHitTestMask &= ~mask;
        }
    }

    private bool FilteredRaycast(Camera camera, Vector3 screenPoint, LayerMask mask, out RaycastHit hitInfo)
    {
        if (this.CanHitTestOffCamera(mask))
        {
            if (!Physics.Raycast(camera.ScreenPointToRay(screenPoint), out hitInfo, camera.farClipPlane, (int) mask))
            {
                return false;
            }
        }
        else if (!CameraUtils.Raycast(camera, screenPoint, mask, out hitInfo))
        {
            return false;
        }
        return true;
    }

    public void FocusTextInput(GameObject owner)
    {
        if (owner == this.m_inputOwner)
        {
            if (this.m_tabKeyDown)
            {
                this.m_inputNeedsFocusFromTabKeyDown = true;
            }
            else
            {
                this.m_inputNeedsFocus = true;
            }
        }
    }

    public bool ForcedInputIsOver(Camera camera, GameObject gameObj)
    {
        RaycastHit hit;
        return this.ForcedInputIsOver(camera, gameObj, out hit);
    }

    public bool ForcedInputIsOver(Camera camera, GameObject gameObj, out RaycastHit hitInfo)
    {
        LayerMask layerMask = ((GameLayer) gameObj.layer).LayerBit();
        if (!CameraUtils.Raycast(camera, this.GetMousePosition(), layerMask, out hitInfo))
        {
            return false;
        }
        return (hitInfo.collider.gameObject == gameObj);
    }

    public static UniversalInputManager Get()
    {
        return s_instance;
    }

    private LayerMask GetHigherPriorityLayerMask(GameLayer layer)
    {
        int num = this.m_hitTestPriorityMap[layer];
        LayerMask mask = 0;
        foreach (KeyValuePair<GameLayer, int> pair in this.m_hitTestPriorityMap)
        {
            GameLayer key = pair.Key;
            if (pair.Value > num)
            {
                mask |= key.LayerBit();
            }
        }
        return mask;
    }

    public bool GetInputHitInfo(out RaycastHit hitInfo)
    {
        return this.GetInputHitInfo(GameLayer.Default, out hitInfo);
    }

    public bool GetInputHitInfo(GameLayer layer, out RaycastHit hitInfo)
    {
        return this.GetInputHitInfo(layer.LayerBit(), out hitInfo);
    }

    public bool GetInputHitInfo(Camera requestedCamera, out RaycastHit hitInfo)
    {
        if (requestedCamera == null)
        {
            return this.GetInputHitInfo(out hitInfo);
        }
        return this.GetInputHitInfo(requestedCamera, requestedCamera.cullingMask, out hitInfo);
    }

    public bool GetInputHitInfo(LayerMask mask, out RaycastHit hitInfo)
    {
        Camera requestedCamera = this.GuessBestHitTestCamera(mask);
        return this.GetInputHitInfo(requestedCamera, mask, out hitInfo);
    }

    public bool GetInputHitInfo(Camera requestedCamera, GameLayer layer, out RaycastHit hitInfo)
    {
        Camera camera;
        return this.Raycast(requestedCamera, layer.LayerBit(), out camera, out hitInfo, false);
    }

    public bool GetInputHitInfo(Camera requestedCamera, LayerMask mask, out RaycastHit hitInfo)
    {
        Camera camera;
        return this.Raycast(requestedCamera, mask, out camera, out hitInfo, false);
    }

    public bool GetInputPointOnPlane(Vector3 origin, out Vector3 point)
    {
        return this.GetInputPointOnPlane(GameLayer.Default, origin, out point);
    }

    public bool GetInputPointOnPlane(GameLayer layer, Vector3 origin, out Vector3 point)
    {
        Camera camera;
        RaycastHit hit;
        float num;
        point = Vector3.zero;
        LayerMask mask = layer.LayerBit();
        if (!this.Raycast(null, mask, out camera, out hit, false))
        {
            return false;
        }
        Ray ray = camera.ScreenPointToRay(this.GetMousePosition());
        Vector3 inNormal = -camera.transform.forward;
        Plane plane = new Plane(inNormal, origin);
        if (!plane.Raycast(ray, out num))
        {
            return false;
        }
        point = ray.GetPoint(num);
        return true;
    }

    public string GetInputText()
    {
        return this.m_inputText;
    }

    public bool GetMouseButton(int button)
    {
        if (Get().IsTouchMode())
        {
            return W8Touch.Get().GetTouch(button);
        }
        return Input.GetMouseButton(button);
    }

    public bool GetMouseButtonDown(int button)
    {
        if (Get().IsTouchMode())
        {
            return W8Touch.Get().GetTouchDown(button);
        }
        return Input.GetMouseButtonDown(button);
    }

    public bool GetMouseButtonUp(int button)
    {
        if (Get().IsTouchMode())
        {
            return W8Touch.Get().GetTouchUp(button);
        }
        return Input.GetMouseButtonUp(button);
    }

    public Vector3 GetMousePosition()
    {
        if (this.IsTouchMode())
        {
            return W8Touch.Get().GetTouchPosition();
        }
        return Input.mousePosition;
    }

    private Camera GuessBestHitTestCamera(LayerMask mask)
    {
        foreach (Camera camera in Camera.allCameras)
        {
            if (!this.m_ignoredCameras.Contains(camera) && ((camera.cullingMask & mask) != 0))
            {
                return camera;
            }
        }
        return null;
    }

    private void HandleGUIInputActive()
    {
        if (this.m_inputActive && this.PreprocessGUITextInput())
        {
            Vector2 screenSize = new Vector2((float) Screen.width, (float) Screen.height);
            Rect inputScreenRect = this.ComputeTextInputRect(screenSize);
            this.SetupTextInput(screenSize, inputScreenRect);
            string str = this.ShowTextInput(inputScreenRect);
            if ((this.IsTouchMode() && !W8Touch.Get().IsVirtualKeyboardVisible()) && (this.GetMouseButtonDown(0) && inputScreenRect.Contains(W8Touch.Get().GetTouchPositionForGUI())))
            {
                W8Touch.Get().ShowKeyboard();
            }
            this.UpdateTextInputFocus();
            if (this.m_inputFocused && (this.m_inputText != str))
            {
                if (this.m_inputNumber)
                {
                    str = StringUtils.StripNonNumbers(str);
                }
                if (!this.m_inputMultiLine)
                {
                    str = StringUtils.StripNewlines(str);
                }
                this.m_inputText = str;
                if (this.m_inputUpdatedCallback != null)
                {
                    this.m_inputUpdatedCallback(str);
                }
            }
        }
    }

    private void HandleGUIInputInactive()
    {
        if (!this.m_inputActive)
        {
            if (this.m_inputIgnoreState != TextInputIgnoreState.INVALID)
            {
                if (this.m_inputIgnoreState == TextInputIgnoreState.NEXT_CALL)
                {
                    this.m_inputIgnoreState = TextInputIgnoreState.INVALID;
                }
            }
            else if (ChatMgr.Get() != null)
            {
                ChatMgr.Get().HandleGUIInput();
            }
        }
    }

    private bool HigherPriorityCollisionExists(GameLayer layer)
    {
        if (this.m_systemDialogActive && (this.m_hitTestPriorityMap[layer] < this.m_hitTestPriorityMap[GameLayer.UI]))
        {
            return true;
        }
        if (this.m_gameDialogActive && (this.m_hitTestPriorityMap[layer] < this.m_hitTestPriorityMap[GameLayer.IgnoreFullScreenEffects]))
        {
            return true;
        }
        if (this.m_FullscreenEffectsCameraActive && (this.m_hitTestPriorityMap[layer] < this.m_hitTestPriorityMap[GameLayer.IgnoreFullScreenEffects]))
        {
            return true;
        }
        LayerMask higherPriorityLayerMask = this.GetHigherPriorityLayerMask(layer);
        foreach (Camera camera in Camera.allCameras)
        {
            RaycastHit hit;
            if ((!this.m_ignoredCameras.Contains(camera) && ((camera.cullingMask & higherPriorityLayerMask) != 0)) && this.FilteredRaycast(camera, this.GetMousePosition(), higherPriorityLayerMask, out hit))
            {
                GameLayer gameLayer = (GameLayer) hit.collider.gameObject.layer;
                if ((camera.cullingMask & gameLayer.LayerBit()) != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IgnoreGUIInput()
    {
        if (this.m_inputIgnoreState == TextInputIgnoreState.INVALID)
        {
            return false;
        }
        if (Event.current.type != EventType.KeyUp)
        {
            return false;
        }
        KeyCode keyCode = Event.current.keyCode;
        if (keyCode != KeyCode.Return)
        {
            if (keyCode != KeyCode.Escape)
            {
                return false;
            }
        }
        else
        {
            if (this.m_inputIgnoreState == TextInputIgnoreState.COMPLETE_KEY_UP)
            {
                this.m_inputIgnoreState = TextInputIgnoreState.NEXT_CALL;
            }
            return true;
        }
        if (this.m_inputIgnoreState == TextInputIgnoreState.CANCEL_KEY_UP)
        {
            this.m_inputIgnoreState = TextInputIgnoreState.NEXT_CALL;
        }
        return true;
    }

    public bool InputHitAnyObject(GameLayer layer)
    {
        RaycastHit hit;
        return this.GetInputHitInfo(layer, out hit);
    }

    public bool InputHitAnyObject(Camera requestedCamera)
    {
        RaycastHit hit;
        if (requestedCamera == null)
        {
            return this.GetInputHitInfo(out hit);
        }
        return this.GetInputHitInfo(requestedCamera, requestedCamera.cullingMask, out hit);
    }

    public bool InputHitAnyObject(LayerMask layerMask)
    {
        RaycastHit hit;
        return this.GetInputHitInfo(layerMask, out hit);
    }

    public bool InputHitAnyObject(Camera requestedCamera, GameLayer layer)
    {
        RaycastHit hit;
        return this.GetInputHitInfo(requestedCamera, layer, out hit);
    }

    public bool InputHitAnyObject(Camera requestedCamera, LayerMask mask)
    {
        RaycastHit hit;
        return this.GetInputHitInfo(requestedCamera, mask, out hit);
    }

    public bool InputIsOver(GameObject gameObj)
    {
        RaycastHit hit;
        return this.InputIsOver(gameObj, out hit);
    }

    public bool InputIsOver(Camera camera, GameObject gameObj)
    {
        RaycastHit hit;
        return this.InputIsOver(camera, gameObj, out hit);
    }

    public bool InputIsOver(GameObject gameObj, out RaycastHit hitInfo)
    {
        Camera camera;
        LayerMask mask = ((GameLayer) gameObj.layer).LayerBit();
        if (!this.Raycast(null, mask, out camera, out hitInfo, false))
        {
            return false;
        }
        return (hitInfo.collider.gameObject == gameObj);
    }

    public bool InputIsOver(Camera camera, GameObject gameObj, out RaycastHit hitInfo)
    {
        Camera camera2;
        LayerMask mask = ((GameLayer) gameObj.layer).LayerBit();
        if (!this.Raycast(camera, mask, out camera2, out hitInfo, false))
        {
            return false;
        }
        return (hitInfo.collider.gameObject == gameObj);
    }

    public bool InputIsOver(GameObject gameObj, int layerMask, out RaycastHit hitInfo)
    {
        Camera camera;
        if (!this.Raycast(null, layerMask, out camera, out hitInfo, false))
        {
            return false;
        }
        return (hitInfo.collider.gameObject == gameObj);
    }

    public bool IsMouseOnScreen()
    {
        return this.m_mouseOnScreen;
    }

    public bool IsTextInputActive()
    {
        return this.m_inputActive;
    }

    public bool IsTextInputPassword()
    {
        return this.m_inputPassword;
    }

    public bool IsTouchMode()
    {
        return ((IsTouchDevice != null) || Options.Get().GetBool(Option.TOUCH_MODE));
    }

    private void ObjectCancelTextInput(GameObject requester)
    {
        this.CancelTextInput(false, requester);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnGUI()
    {
        this.IgnoreGUIInput();
        this.HandleGUIInputInactive();
        this.HandleGUIInputActive();
    }

    private bool PreprocessGUITextInput()
    {
        this.UpdateTabKeyDown();
        if (this.m_inputPreprocessCallback != null)
        {
            this.m_inputPreprocessCallback(Event.current);
            if (!this.m_inputActive)
            {
                return false;
            }
        }
        if (this.ProcessTextInputFinishKeys())
        {
            return false;
        }
        return true;
    }

    private bool ProcessTextInputFinishKeys()
    {
        if (!this.m_inputFocused)
        {
            return false;
        }
        if (Event.current.type != EventType.KeyDown)
        {
            return false;
        }
        KeyCode keyCode = Event.current.keyCode;
        if (keyCode != KeyCode.Return)
        {
            if (keyCode != KeyCode.Escape)
            {
                return false;
            }
        }
        else
        {
            this.m_inputIgnoreState = TextInputIgnoreState.COMPLETE_KEY_UP;
            this.CompleteTextInput();
            return true;
        }
        this.m_inputIgnoreState = TextInputIgnoreState.CANCEL_KEY_UP;
        this.UserCancelTextInput();
        return true;
    }

    private bool Raycast(Camera requestedCamera, LayerMask mask, out Camera camera, out RaycastHit hitInfo, bool ignorePriority = false)
    {
        hitInfo = new RaycastHit();
        if (!ignorePriority)
        {
            foreach (Camera camera2 in this.m_CameraMaskCameras)
            {
                camera = camera2;
                LayerMask mask2 = GameLayer.CameraMask.LayerBit();
                if (this.RaycastWithPriority(camera2, mask2, out hitInfo))
                {
                    return true;
                }
            }
            camera = this.m_FullscreenEffectsCamera;
            if (camera != null)
            {
                LayerMask mask3 = GameLayer.IgnoreFullScreenEffects.LayerBit();
                if (this.RaycastWithPriority(camera, mask3, out hitInfo))
                {
                    return true;
                }
            }
        }
        camera = requestedCamera;
        if (camera == null)
        {
            camera = Camera.main;
        }
        return this.RaycastWithPriority(camera, mask, out hitInfo);
    }

    private bool RaycastWithPriority(Camera camera, LayerMask mask, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        if (camera == null)
        {
            return false;
        }
        if (!this.FilteredRaycast(camera, this.GetMousePosition(), mask, out hitInfo))
        {
            return false;
        }
        GameLayer layer = (GameLayer) hitInfo.collider.gameObject.layer;
        if (this.HigherPriorityCollisionExists(layer))
        {
            return false;
        }
        return true;
    }

    public bool RegisterMouseOnOrOffScreenListener(MouseOnOrOffScreenCallback listener)
    {
        if (this.m_mouseOnOrOffScreenListeners.Contains(listener))
        {
            return false;
        }
        this.m_mouseOnOrOffScreenListeners.Add(listener);
        return true;
    }

    public bool RemoveCameraMaskCamera(Camera camera)
    {
        return this.m_CameraMaskCameras.Remove(camera);
    }

    public bool RemoveIgnoredCamera(Camera camera)
    {
        return this.m_ignoredCameras.Remove(camera);
    }

    public void SetFullScreenEffectsCamera(Camera camera, bool active)
    {
        this.m_FullscreenEffectsCamera = camera;
        this.m_FullscreenEffectsCameraActive = false;
    }

    public void SetGameDialogActive(bool active)
    {
        this.m_gameDialogActive = active;
    }

    public void SetGUISkin(GUISkinContainer skinContainer)
    {
        if (this.m_skinContainer != null)
        {
            UnityEngine.Object.Destroy(this.m_skinContainer.gameObject);
        }
        this.m_skinContainer = skinContainer;
        this.m_skinContainer.transform.parent = base.transform;
        this.m_skin = skinContainer.GetGUISkin();
        this.m_defaultInputAlignment = this.m_skin.textField.alignment;
        this.m_defaultInputFont = this.m_skin.textField.font;
    }

    public void SetInputText(string text)
    {
        if (text == null)
        {
        }
        this.m_inputText = string.Empty;
    }

    public void SetSystemDialogActive(bool active)
    {
        this.m_systemDialogActive = active;
    }

    private void SetupTextInput(Vector2 screenSize, Rect inputScreenRect)
    {
        GUI.skin = this.m_skin;
        GUI.skin.textField.font = this.m_inputFont;
        int num = this.ComputeTextInputFontSize(screenSize, inputScreenRect.height);
        GUI.skin.textField.fontSize = num;
        if (this.m_inputColor.HasValue)
        {
            GUI.color = this.m_inputColor.Value;
        }
        GUI.skin.textField.alignment = this.m_inputAlignment;
        GUI.SetNextControlName("UniversalInputManagerTextInput");
    }

    private string ShowTextInput(Rect inputScreenRect)
    {
        if (this.m_inputPassword)
        {
            if (this.m_inputMaxCharacters <= 0)
            {
                return GUI.PasswordField(inputScreenRect, this.m_inputText, '*');
            }
            return GUI.PasswordField(inputScreenRect, this.m_inputText, '*', this.m_inputMaxCharacters);
        }
        if (this.m_inputMaxCharacters <= 0)
        {
            return GUI.TextField(inputScreenRect, this.m_inputText);
        }
        return GUI.TextField(inputScreenRect, this.m_inputText, this.m_inputMaxCharacters);
    }

    private void Start()
    {
        this.m_mouseOnScreen = InputUtil.IsMouseOnScreen();
    }

    public bool UnregisterMouseOnOrOffScreenListener(MouseOnOrOffScreenCallback listener)
    {
        return this.m_mouseOnOrOffScreenListeners.Remove(listener);
    }

    private void Update()
    {
        this.UpdateMouseOnOrOffScreen();
        this.UpdateInput();
        this.CleanDeadCameras();
    }

    private void UpdateInput()
    {
        if (!this.UpdateTextInput())
        {
            InputManager manager = InputManager.Get();
            if ((manager == null) || !manager.HandleKeyboardInput())
            {
                CheatMgr mgr = CheatMgr.Get();
                if ((mgr == null) || !mgr.HandleKeyboardInput())
                {
                    Cheats cheats = Cheats.Get();
                    if ((cheats == null) || !cheats.HandleKeyboardInput())
                    {
                        DialogManager manager2 = DialogManager.Get();
                        if ((manager2 == null) || !manager2.HandleKeyboardInput())
                        {
                            CollectionInputMgr mgr2 = CollectionInputMgr.Get();
                            if ((mgr2 == null) || !mgr2.HandleKeyboardInput())
                            {
                                DraftInputManager manager3 = DraftInputManager.Get();
                                if ((manager3 == null) || !manager3.HandleKeyboardInput())
                                {
                                    PackOpening opening = PackOpening.Get();
                                    if ((opening == null) || !opening.HandleKeyboardInput())
                                    {
                                        if (SceneMgr.Get() != null)
                                        {
                                            Scene scene = SceneMgr.Get().GetScene();
                                            if ((scene != null) && scene.HandleKeyboardInput())
                                            {
                                                return;
                                            }
                                        }
                                        BaseUI eui = BaseUI.Get();
                                        if ((eui != null) && eui.HandleKeyboardInput())
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateMouseOnOrOffScreen()
    {
        bool onScreen = InputUtil.IsMouseOnScreen();
        if (onScreen != this.m_mouseOnScreen)
        {
            this.m_mouseOnScreen = onScreen;
            foreach (MouseOnOrOffScreenCallback callback in this.m_mouseOnOrOffScreenListeners.ToArray())
            {
                callback(onScreen);
            }
        }
    }

    private void UpdateTabKeyDown()
    {
        this.m_tabKeyDown = (Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Tab);
    }

    private bool UpdateTextInput()
    {
        if (Input.imeIsSelected || !string.IsNullOrEmpty(Input.compositionString))
        {
            IsIMEEverUsed = true;
        }
        if (this.m_inputNeedsFocusFromTabKeyDown)
        {
            this.m_inputNeedsFocusFromTabKeyDown = false;
            this.m_inputNeedsFocus = true;
        }
        if (!this.m_inputActive)
        {
            return false;
        }
        return this.m_inputFocused;
    }

    private void UpdateTextInputFocus()
    {
        if (this.m_inputNeedsFocus)
        {
            GUI.FocusControl("UniversalInputManagerTextInput");
            this.m_inputFocused = true;
            this.m_inputNeedsFocus = false;
        }
        else
        {
            this.m_inputFocused = GUI.GetNameOfFocusedControl() == "UniversalInputManagerTextInput";
        }
    }

    public void UpdateTextInputRect(GameObject owner, Rect rect)
    {
        if (owner == this.m_inputOwner)
        {
            this.m_inputNormalizedRect = rect;
            this.m_inputInitialScreenSize.x = Screen.width;
            this.m_inputInitialScreenSize.y = Screen.height;
        }
    }

    private void UserCancelTextInput()
    {
        this.CancelTextInput(true, null);
    }

    public void UseTextInput(TextInputParams parms, bool force = false)
    {
        if (force || (parms.m_owner != this.m_inputOwner))
        {
            if ((this.m_inputOwner != null) && (this.m_inputOwner != parms.m_owner))
            {
                this.ObjectCancelTextInput(parms.m_owner);
            }
            this.m_inputOwner = parms.m_owner;
            this.m_inputUpdatedCallback = parms.m_updatedCallback;
            this.m_inputPreprocessCallback = parms.m_preprocessCallback;
            this.m_inputCompletedCallback = parms.m_completedCallback;
            this.m_inputCanceledCallback = parms.m_canceledCallback;
            this.m_inputPassword = parms.m_password;
            this.m_inputNumber = parms.m_number;
            this.m_inputMultiLine = parms.m_multiLine;
            this.m_inputActive = true;
            this.m_inputFocused = false;
            if (parms.m_text == null)
            {
            }
            this.m_inputText = string.Empty;
            this.m_inputNormalizedRect = parms.m_rect;
            this.m_inputInitialScreenSize.x = Screen.width;
            this.m_inputInitialScreenSize.y = Screen.height;
            this.m_inputMaxCharacters = parms.m_maxCharacters;
            this.m_inputColor = parms.m_color;
            TextAnchor? alignment = parms.m_alignment;
            this.m_inputAlignment = !alignment.HasValue ? this.m_defaultInputAlignment : alignment.Value;
            if (parms.m_font == null)
            {
            }
            this.m_inputFont = this.m_defaultInputFont;
            this.m_inputNeedsFocus = true;
            this.m_inputIgnoreState = TextInputIgnoreState.INVALID;
            this.m_inputKeepFocusOnComplete = parms.m_inputKeepFocusOnComplete;
            if (this.IsTextInputPassword())
            {
                Input.imeCompositionMode = IMECompositionMode.Off;
            }
            this.m_hideVirtualKeyboardOnComplete = parms.m_hideVirtualKeyboardOnComplete;
            if (Get().IsTouchMode() && parms.m_showVirtualKeyboard)
            {
                W8Touch.Get().ShowKeyboard();
            }
        }
    }

    public bool WasTouchCanceled()
    {
        if (IsTouchDevice != null)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Canceled)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public delegate void MouseOnOrOffScreenCallback(bool onScreen);

    public delegate void TextInputCanceledCallback(bool userRequested, GameObject requester);

    public delegate void TextInputCompletedCallback(string input);

    private enum TextInputIgnoreState
    {
        INVALID,
        COMPLETE_KEY_UP,
        CANCEL_KEY_UP,
        NEXT_CALL
    }

    public class TextInputParams
    {
        public TextAnchor? m_alignment;
        public UniversalInputManager.TextInputCanceledCallback m_canceledCallback;
        public Color? m_color;
        public UniversalInputManager.TextInputCompletedCallback m_completedCallback;
        public Font m_font;
        public bool m_hideVirtualKeyboardOnComplete = true;
        public bool m_inputKeepFocusOnComplete;
        public int m_maxCharacters;
        public bool m_multiLine;
        public bool m_number;
        public GameObject m_owner;
        public bool m_password;
        public UniversalInputManager.TextInputPreprocessCallback m_preprocessCallback;
        public Rect m_rect;
        public bool m_showVirtualKeyboard = true;
        public string m_text;
        public bool m_touchScreenKeyboardHideInput;
        public int m_touchScreenKeyboardType;
        public UniversalInputManager.TextInputUpdatedCallback m_updatedCallback;
        public bool m_useNativeKeyboard;
    }

    public delegate bool TextInputPreprocessCallback(Event e);

    public delegate void TextInputUpdatedCallback(string input);
}

