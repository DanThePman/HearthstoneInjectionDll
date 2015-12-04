using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    public BnetBar m_BnetBar;
    public Camera m_BnetCamera;
    public Camera m_BnetDialogCamera;
    public BaseUIBones m_Bones;
    public ExistingAccountPopup m_ExistingAccountPopup;
    public BaseUIPrefabs m_Prefabs;
    private static BaseUI s_instance;

    private void Awake()
    {
        s_instance = this;
        UnityEngine.Object.Instantiate<ChatMgr>(this.m_Prefabs.m_ChatMgrPrefab).transform.parent = base.transform;
        this.m_BnetCamera.GetComponent<ScreenResizeDetector>().AddSizeChangedListener(new ScreenResizeDetector.SizeChangedCallback(this.OnScreenSizeChanged));
    }

    public static BaseUI Get()
    {
        return s_instance;
    }

    public Transform GetAddFriendBone()
    {
        if (!UniversalInputManager.Get().IsTouchMode())
        {
            return this.m_Bones.m_AddFriend;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            return this.m_Bones.m_AddFriendPhoneKeyboard;
        }
        return this.m_Bones.m_AddFriendVirtualKeyboard;
    }

    public Camera GetBnetCamera()
    {
        return this.m_BnetCamera;
    }

    public Camera GetBnetDialogCamera()
    {
        return this.m_BnetDialogCamera;
    }

    public Transform GetChatBubbleBone()
    {
        return this.m_Bones.m_ChatBubble;
    }

    public Transform GetGameMenuBone(bool withRatings = false)
    {
        if (SceneMgr.Get().IsInGame())
        {
            return this.m_Bones.m_InGameMenu;
        }
        return (!withRatings ? this.m_Bones.m_BoxMenu : this.m_Bones.m_BoxMenuWithRatings);
    }

    public Transform GetOptionsMenuBone()
    {
        return this.m_Bones.m_OptionsMenu;
    }

    public Transform GetQuickChatBone()
    {
        if ((!UniversalInputManager.Get().IsTouchMode() || !W8Touch.s_isWindows8OrGreater) && !W8Touch.Get().IsVirtualKeyboardVisible())
        {
            return this.m_Bones.m_QuickChat;
        }
        return this.m_Bones.m_QuickChatVirtualKeyboard;
    }

    public Transform GetRecruitAFriendBone()
    {
        return this.m_Bones.m_RecruitAFriend;
    }

    public bool HandleKeyboardInput()
    {
        if ((this.m_BnetBar != null) && this.m_BnetBar.HandleKeyboardInput())
        {
            return true;
        }
        if (Input.GetKeyUp(BackButton.backKey))
        {
        }
        return false;
    }

    [DebuggerHidden]
    private IEnumerator NotifyOfScreenshotComplete()
    {
        return new <NotifyOfScreenshotComplete>c__Iterator292();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Print:
                case KeyCode.SysReq:
                case KeyCode.F13:
                    this.TakeScreenshot();
                    break;
            }
        }
    }

    public void OnLoggedIn()
    {
        this.m_BnetBar.OnLoggedIn();
    }

    private void OnScreenSizeChanged(object userData)
    {
        this.UpdateLayout();
    }

    private void Start()
    {
        this.UpdateLayout();
        InnKeepersSpecial.Init();
    }

    private void TakeScreenshot()
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        DateTime now = DateTime.Now;
        string path = string.Format("{0}/Hearthstone Screenshot {1:MM-dd-yy HH.mm.ss}.png", folderPath, now);
        int num = 1;
        while (System.IO.File.Exists(path))
        {
            path = string.Format("{0}/Hearthstone Screenshot {1:MM-dd-yy HH.mm.ss} {2}.png", folderPath, now, num++);
        }
        UIStatus.Get().HideIfScreenshotMessage();
        Application.CaptureScreenshot(path);
        base.StartCoroutine(this.NotifyOfScreenshotComplete());
        UnityEngine.Debug.Log(string.Format("screenshot saved to {0}", path));
    }

    private void UpdateLayout()
    {
        this.m_BnetBar.UpdateLayout();
        if (ChatMgr.Get() != null)
        {
            ChatMgr.Get().UpdateLayout();
        }
        if (SplashScreen.Get() != null)
        {
            SplashScreen.Get().UpdateLayout();
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyOfScreenshotComplete>c__Iterator292 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    UIStatus.Get().AddInfo(GameStrings.Get("GLOBAL_SCREENSHOT_COMPLETE"), true);
                    this.$PC = -1;
                    break;
            }
            return false;
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
}

