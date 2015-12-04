using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FatalErrorScreen : MonoBehaviour
{
    private bool m_allowClick;
    private Camera m_camera;
    public UberText m_closedSignText;
    public UberText m_closedSignTitle;
    public float m_delayBeforeNextReset;
    public UberText m_errorCodeText;
    private PegUIElement m_inputBlocker;
    public UberText m_reconnectTip;
    private bool m_redirectToStore;

    private void Awake()
    {
        SplashScreen screen = SplashScreen.Get();
        if (screen != null)
        {
            screen.HideLogo();
        }
        List<FatalErrorMessage> messages = FatalErrorMgr.Get().GetMessages();
        Log.JMac.Print(LogLevel.Warning, string.Format("Showing Fatal Error Screen with {0} messages", messages.Count), new object[0]);
        this.m_closedSignText.Text = messages[0].m_text;
        this.m_closedSignTitle.Text = GameStrings.Get("GLOBAL_SPLASH_CLOSED_SIGN_TITLE");
        this.m_allowClick = messages[0].m_allowClick;
        this.m_redirectToStore = messages[0].m_redirectToStore;
        this.m_delayBeforeNextReset = messages[0].m_delayBeforeNextReset;
    }

    private void OnClick(UIEvent e)
    {
        if (ApplicationMgr.AllowResetFromFatalError != null)
        {
            if (this.m_redirectToStore)
            {
                PlatformDependentValue<string> value4 = new PlatformDependentValue<string>(PlatformCategory.OS) {
                    iOS = "https://itunes.apple.com/app/hearthstone-heroes-warcraft/id625257520?ls=1&mt=8",
                    Android = "https://play.google.com/store/apps/details?id=com.blizzard.wtcg.hearthstone"
                };
                PlatformDependentValue<string> value2 = value4;
                value4 = new PlatformDependentValue<string>(PlatformCategory.OS) {
                    iOS = "https://itunes.apple.com/cn/app/lu-shi-chuan-shuo-mo-shou/id841140063?ls=1&mt=8",
                    Android = "https://www.battlenet.com.cn/account/download/hearthstone/android?style=hearthstone"
                };
                PlatformDependentValue<string> value3 = value4;
                if (ApplicationMgr.GetAndroidStore() == AndroidStore.AMAZON)
                {
                    value2.Android = "http://www.amazon.com/gp/mas/dl/android?p=com.blizzard.wtcg.hearthstone";
                }
                if (MobileDeviceLocale.GetCurrentRegionId() == Network.BnetRegion.REGION_CN)
                {
                    value2 = value3;
                }
                Application.OpenURL((string) value2);
            }
            else
            {
                float waitDuration = (ApplicationMgr.Get().LastResetTime() + this.m_delayBeforeNextReset) - UnityEngine.Time.realtimeSinceStartup;
                Log.JMac.Print("Remaining time to wait before allowing a reconnect attempt: " + waitDuration, new object[0]);
                if (waitDuration > 0f)
                {
                    this.m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClick));
                    this.m_closedSignText.Text = GameStrings.Get("GLOBAL_SPLASH_CLOSED_RECONNECTING");
                    this.m_allowClick = false;
                    this.m_reconnectTip.gameObject.SetActive(false);
                    base.StartCoroutine(this.WaitBeforeReconnecting(waitDuration));
                }
                else
                {
                    UnityEngine.Debug.Log("resetting!");
                    this.m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClick));
                    ApplicationMgr.Get().Reset();
                }
            }
        }
        else
        {
            this.m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClick));
            ApplicationMgr.Get().Exit();
        }
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
        object[] args = new object[] { "amount", 1f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(base.gameObject, hashtable);
    }

    private void Start()
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.Show();
        base.StartCoroutine(this.WaitForUIThenFinishSetup());
    }

    private void Update()
    {
        if ((ApplicationMgr.AllowResetFromFatalError != null) && this.m_allowClick)
        {
            if (this.m_redirectToStore)
            {
                this.m_reconnectTip.SetGameStringText("GLOBAL_MOBILE_TAP_TO_UPDATE");
            }
            else
            {
                this.m_reconnectTip.SetGameStringText("GLOBAL_MOBILE_TAP_TO_RECONNECT");
            }
            this.m_reconnectTip.gameObject.SetActive(true);
            float num = 1f;
            this.m_reconnectTip.TextAlpha = (Mathf.Sin((UnityEngine.Time.time * 3.141593f) / num) + 1f) / 2f;
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitBeforeReconnecting(float waitDuration)
    {
        return new <WaitBeforeReconnecting>c__Iterator83 { waitDuration = waitDuration, <$>waitDuration = waitDuration };
    }

    [DebuggerHidden]
    private IEnumerator WaitForUIThenFinishSetup()
    {
        return new <WaitForUIThenFinishSetup>c__Iterator82 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitBeforeReconnecting>c__Iterator83 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>waitDuration;
        internal float waitDuration;

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
                    this.$current = new WaitForSeconds(this.waitDuration);
                    this.$PC = 1;
                    return true;

                case 1:
                    ApplicationMgr.Get().Reset();
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

    [CompilerGenerated]
    private sealed class <WaitForUIThenFinishSetup>c__Iterator82 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal FatalErrorScreen <>f__this;
        internal GameObject <inputBlockerObject>__0;

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
                case 1:
                    if (PegUI.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_camera = CameraUtils.FindFirstByLayer(this.<>f__this.gameObject.layer);
                    PegUI.Get().SetInputCamera(this.<>f__this.m_camera);
                    this.<inputBlockerObject>__0 = CameraUtils.CreateInputBlocker(this.<>f__this.m_camera, "ClosedSignInputBlocker", this.<>f__this);
                    SceneUtils.SetLayer(this.<inputBlockerObject>__0, this.<>f__this.gameObject.layer);
                    this.<>f__this.m_inputBlocker = this.<inputBlockerObject>__0.AddComponent<PegUIElement>();
                    if (this.<>f__this.m_allowClick)
                    {
                        this.<>f__this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.OnClick));
                    }
                    if (FatalErrorMgr.Get().GetFormattedErrorCode() != null)
                    {
                        this.<>f__this.m_errorCodeText.gameObject.SetActive(true);
                        this.<>f__this.m_errorCodeText.Text = FatalErrorMgr.Get().GetFormattedErrorCode();
                        OverlayUI.Get().AddGameObject(this.<>f__this.m_errorCodeText.gameObject, CanvasAnchor.TOP_RIGHT, false, CanvasScaleMode.HEIGHT);
                    }
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

