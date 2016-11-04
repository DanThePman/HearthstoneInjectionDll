#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
// - HearthstoneInjectionDll v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class UniversalInputManager
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    UnityEngine.Vector3 GetMousePosition()
    {
        if (this.UseWindowsTouch())
        {
            return W8Touch.Get().GetTouchPosition();
        }
        return HearthstoneInjectionDll.Actions.GetMousePosition();

    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    UniversalInputManager()
    {
    }

    static UniversalInputManager()
    {
    }

    void Update()
    {
    }

    void Awake()
    {
    }

    void Start()
    {
    }

    void OnDestroy()
    {
    }

    void OnGUI()
    {
    }

    static UniversalInputManager Get()
    {
        return default(UniversalInputManager);
    }

    void SetGUISkin(GUISkinContainer skinContainer)
    {
    }

    bool IsTouchMode()
    {
        return default(bool);
    }

    bool UseWindowsTouch()
    {
        return default(bool);
    }

    bool WasTouchCanceled()
    {
        return default(bool);
    }

    bool IsMouseOnScreen()
    {
        return default(bool);
    }


    void SetGameDialogActive(bool active)
    {
    }

    void SetSystemDialogActive(bool active)
    {
    }


    void CancelTextInput(UnityEngine.GameObject requester, bool force)
    {
    }

    void FocusTextInput(UnityEngine.GameObject owner)
    {
    }

    void UpdateTextInputRect(UnityEngine.GameObject owner, UnityEngine.Rect rect)
    {
    }

    bool IsTextInputPassword()
    {
        return default(bool);
    }

    bool IsTextInputActive()
    {
        return default(bool);
    }

    string GetInputText()
    {
        return default(string);
    }

    void SetInputText(string text, bool moveCursorToEnd)
    {
    }

    bool InputIsOver(UnityEngine.GameObject gameObj)
    {
        return default(bool);
    }

    bool InputIsOver(UnityEngine.GameObject gameObj, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool InputIsOver(UnityEngine.GameObject gameObj, int layerMask, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool InputIsOver(UnityEngine.Camera camera, UnityEngine.GameObject gameObj)
    {
        return default(bool);
    }

    bool InputIsOver(UnityEngine.Camera camera, UnityEngine.GameObject gameObj, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool ForcedInputIsOver(UnityEngine.Camera camera, UnityEngine.GameObject gameObj)
    {
        return default(bool);
    }

    bool ForcedInputIsOver(UnityEngine.Camera camera, UnityEngine.GameObject gameObj, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool InputHitAnyObject(GameLayer layer)
    {
        return default(bool);
    }

    bool InputHitAnyObject(UnityEngine.LayerMask layerMask)
    {
        return default(bool);
    }

    bool InputHitAnyObject(UnityEngine.Camera requestedCamera)
    {
        return default(bool);
    }

    bool InputHitAnyObject(UnityEngine.Camera requestedCamera, GameLayer layer)
    {
        return default(bool);
    }

    bool InputHitAnyObject(UnityEngine.Camera requestedCamera, UnityEngine.LayerMask mask)
    {
        return default(bool);
    }

    bool GetInputHitInfo(ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputHitInfo(GameLayer layer, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputHitInfo(UnityEngine.LayerMask mask, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputHitInfo(UnityEngine.Camera requestedCamera, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputHitInfo(UnityEngine.Camera requestedCamera, GameLayer layer, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputHitInfo(UnityEngine.Camera requestedCamera, UnityEngine.LayerMask mask, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool GetInputPointOnPlane(UnityEngine.Vector3 origin, ref UnityEngine.Vector3 point)
    {
        return default(bool);
    }

    bool GetInputPointOnPlane(GameLayer layer, UnityEngine.Vector3 origin, ref UnityEngine.Vector3 point)
    {
        return default(bool);
    }

    bool CanHitTestOffCamera(GameLayer layer)
    {
        return default(bool);
    }

    bool CanHitTestOffCamera(UnityEngine.LayerMask layerMask)
    {
        return default(bool);
    }

    void EnableHitTestOffCamera(GameLayer layer, bool enable)
    {
    }

    void EnableHitTestOffCamera(UnityEngine.LayerMask mask, bool enable)
    {
    }

    void SetFullScreenEffectsCamera(UnityEngine.Camera camera, bool active)
    {
    }

    bool GetMouseButton(int button)
    {
        return default(bool);
    }

    bool GetMouseButtonDown(int button)
    {
        return default(bool);
    }

    bool GetMouseButtonUp(int button)
    {
        return default(bool);
    }

    bool AddCameraMaskCamera(UnityEngine.Camera camera)
    {
        return default(bool);
    }

    bool RemoveCameraMaskCamera(UnityEngine.Camera camera)
    {
        return default(bool);
    }

    bool AddIgnoredCamera(UnityEngine.Camera camera)
    {
        return default(bool);
    }

    bool RemoveIgnoredCamera(UnityEngine.Camera camera)
    {
        return default(bool);
    }

    void CreateHitTestPriorityMap()
    {
    }

    void CleanDeadCameras()
    {
    }

    UnityEngine.Camera GuessBestHitTestCamera(UnityEngine.LayerMask mask)
    {
        return default(UnityEngine.Camera);
    }

    bool Raycast(UnityEngine.Camera requestedCamera, UnityEngine.LayerMask mask, ref UnityEngine.Camera camera, ref UnityEngine.RaycastHit hitInfo, bool ignorePriority)
    {
        return default(bool);
    }

    bool RaycastWithPriority(UnityEngine.Camera camera, UnityEngine.LayerMask mask, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool FilteredRaycast(UnityEngine.Camera camera, UnityEngine.Vector3 screenPoint, UnityEngine.LayerMask mask, ref UnityEngine.RaycastHit hitInfo)
    {
        return default(bool);
    }

    bool HigherPriorityCollisionExists(GameLayer layer)
    {
        return default(bool);
    }

    UnityEngine.LayerMask GetHigherPriorityLayerMask(GameLayer layer)
    {
        return default(UnityEngine.LayerMask);
    }

    void UpdateMouseOnOrOffScreen()
    {
    }

    void UpdateInput()
    {
    }

    bool UpdateTextInput()
    {
        return default(bool);
    }

    void UserCancelTextInput()
    {
    }

    void ObjectCancelTextInput(UnityEngine.GameObject requester)
    {
    }

    void CancelTextInput(bool userRequested, UnityEngine.GameObject requester)
    {
    }

    void CompleteTextInput()
    {
    }

    void ClearTextInputVars()
    {
    }

    bool IgnoreGUIInput()
    {
        return default(bool);
    }

    void HandleGUIInputInactive()
    {
    }

    void HandleGUIInputActive()
    {
    }

    bool PreprocessGUITextInput()
    {
        return default(bool);
    }

    void UpdateTabKeyDown()
    {
    }

    bool ProcessTextInputFinishKeys()
    {
        return default(bool);
    }

    void SetupTextInput(UnityEngine.Vector2 screenSize, UnityEngine.Rect inputScreenRect)
    {
    }

    string ShowTextInput(UnityEngine.Rect inputScreenRect)
    {
        return default(string);
    }

    void UpdateTextInputFocus()
    {
    }

    UnityEngine.Rect ComputeTextInputRect(UnityEngine.Vector2 screenSize)
    {
        return default(UnityEngine.Rect);
    }

    int ComputeTextInputFontSize(UnityEngine.Vector2 screenSize, float rectHeight)
    {
        return default(int);
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static float TEXT_INPUT_RECT_HEIGHT_OFFSET;
    static int TEXT_INPUT_MAX_FONT_SIZE;
    static int TEXT_INPUT_MIN_FONT_SIZE;
    static int TEXT_INPUT_FONT_SIZE_INSET;
    static int TEXT_INPUT_IME_FONT_SIZE_INSET;
    static string TEXT_INPUT_NAME;
    static PlatformDependentValue<bool> IsTouchDevice;
    static GameLayer[] HIT_TEST_PRIORITY_ORDER;
    //static UniversalInputManager s_instance;
    static bool IsIMEEverUsed;
    bool m_mouseOnScreen;
    //System.Collections.Generic.List<UniversalInputManager.MouseOnOrOffScreenCallback> m_mouseOnOrOffScreenListeners;
    Map<GameLayer, int> m_hitTestPriorityMap;
    bool m_gameDialogActive;
    bool m_systemDialogActive;
    int m_offCameraHitTestMask;
    UnityEngine.Camera m_FullscreenEffectsCamera;
    System.Collections.Generic.List<UnityEngine.Camera> m_CameraMaskCameras;
    bool m_FullscreenEffectsCameraActive;
    System.Collections.Generic.List<UnityEngine.Camera> m_ignoredCameras;
    UnityEngine.GameObject m_inputOwner;
    //UniversalInputManager.TextInputUpdatedCallback m_inputUpdatedCallback;
    //UniversalInputManager.TextInputPreprocessCallback m_inputPreprocessCallback;
    //UniversalInputManager.TextInputCompletedCallback m_inputCompletedCallback;
    //UniversalInputManager.TextInputCanceledCallback m_inputCanceledCallback;
    bool m_inputPassword;
    bool m_inputNumber;
    bool m_inputMultiLine;
    bool m_inputActive;
    bool m_inputFocused;
    bool m_inputKeepFocusOnComplete;
    string m_inputText;
    UnityEngine.Rect m_inputNormalizedRect;
    UnityEngine.Vector2 m_inputInitialScreenSize;
    int m_inputMaxCharacters;
    UnityEngine.Font m_inputFont;
    UnityEngine.TextAnchor m_inputAlignment;
    System.Nullable<UnityEngine.Color> m_inputColor;
    UnityEngine.Font m_defaultInputFont;
    UnityEngine.TextAnchor m_defaultInputAlignment;
    bool m_inputNeedsFocus;
    bool m_tabKeyDown;
    bool m_inputNeedsFocusFromTabKeyDown;
    //UniversalInputManager.TextInputIgnoreState m_inputIgnoreState;
    bool m_hideVirtualKeyboardOnComplete;
    GUISkinContainer m_skinContainer;
    UnityEngine.GUISkin m_skin;
    static PlatformDependentValue<bool> UsePhoneUI;
    #endregion

}
