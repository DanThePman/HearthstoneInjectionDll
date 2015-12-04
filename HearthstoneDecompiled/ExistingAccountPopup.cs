using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ExistingAccountPopup : DialogBase
{
    public GameObject m_bubble;
    private Vector3 m_buttonOffset = new Vector3(0.2f, 0f, 0.6f);
    private bool m_haveAccount;
    public PegUIElement m_haveAccountButton;
    public PegUIElement m_noAccountButton;
    private ResponseCallback m_responseCallback;
    public ExistingAccoundSound m_sound;

    protected void DownScale()
    {
        object[] args = new object[] { "scale", new Vector3(0f, 0f, 0f), "delay", 0.1, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "OnHideAnimFinished", "time", 0.2f };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
    }

    protected void FadeBubble()
    {
        object[] args = new object[] { "delay", 6f, "time", 1f, "amount", 0f };
        iTween.FadeTo(this.m_bubble, iTween.Hash(args));
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr != null)
        {
            mgr.SetBlurBrightness(1f);
            mgr.SetBlurDesaturation(0f);
            mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
            mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
        }
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr != null)
        {
            mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
            mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
        }
        this.ScaleAway();
    }

    private void HaveAccountButtonPress(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay(this.m_sound.m_buttonClick);
        Transform transform = this.m_haveAccountButton.transform;
        transform.localPosition += this.m_buttonOffset;
    }

    private void HaveAccountButtonRelease(UIEvent e)
    {
        this.m_haveAccount = true;
        Transform transform = this.m_haveAccountButton.transform;
        transform.localPosition -= this.m_buttonOffset;
        this.ScaleAway();
    }

    private void NoAccountButtonPress(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay(this.m_sound.m_buttonClick);
        Transform transform = this.m_noAccountButton.transform;
        transform.localPosition += this.m_buttonOffset;
    }

    private void NoAccountButtonRelease(UIEvent e)
    {
        this.m_haveAccount = false;
        Transform transform = this.m_noAccountButton.transform;
        transform.localPosition -= this.m_buttonOffset;
        this.FadeEffectsOut();
    }

    protected override void OnHideAnimFinished()
    {
        base.OnHideAnimFinished();
        base.m_shown = false;
        SoundManager.Get().LoadAndPlay(this.m_sound.m_popupHide);
        BaseUI.Get().m_BnetBar.Enable();
        this.m_responseCallback(this.m_haveAccount);
    }

    private void ScaleAway()
    {
        object[] args = new object[] { "scale", Vector3.Scale(base.PUNCH_SCALE, base.gameObject.transform.localScale), "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "DownScale", "time", 0.1f };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
    }

    public void SetInfo(Info info)
    {
        this.m_responseCallback = info.m_callback;
    }

    public override void Show()
    {
        base.Show();
        BaseUI.Get().m_BnetBar.Disable();
        this.m_bubble.SetActive(true);
        object[] args = new object[] { "time", 0f, "amount", 1f, "oncomplete", "ShowBubble", "oncompletetarget", base.gameObject };
        iTween.FadeTo(this.m_bubble, iTween.Hash(args));
        base.m_showAnimState = DialogBase.ShowAnimState.IN_PROGRESS;
        UniversalInputManager.Get().SetSystemDialogActive(true);
        SoundManager.Get().LoadAndPlay(this.m_sound.m_popupShow);
        SoundManager.Get().LoadAndPlay(this.m_sound.m_innkeeperWelcome);
    }

    protected void ShowBubble()
    {
        object[] args = new object[] { "delay", 1f, "time", 0.5f, "amount", 0f, "oncomplete", "FadeBubble", "oncompletetarget", base.gameObject };
        iTween.FadeFrom(this.m_bubble, iTween.Hash(args));
    }

    private void Start()
    {
        base.transform.position = new Vector3(base.transform.position.x, -525f, 800f);
        this.m_haveAccountButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.HaveAccountButtonRelease));
        this.m_noAccountButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.NoAccountButtonRelease));
        this.m_haveAccountButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.HaveAccountButtonPress));
        this.m_noAccountButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.NoAccountButtonPress));
        this.FadeEffectsIn();
    }

    public class Info
    {
        public ExistingAccountPopup.ResponseCallback m_callback;
    }

    public delegate void ResponseCallback(bool hasAccount);
}

