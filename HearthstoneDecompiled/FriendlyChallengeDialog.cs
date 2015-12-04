using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FriendlyChallengeDialog : DialogBase
{
    public StandardPegButtonNew m_acceptButton;
    public UberText m_challengerName;
    public UberText m_challengeText1;
    public UberText m_challengeText2;
    public StandardPegButtonNew m_denyButton;
    public UberText m_nearbyPlayerNote;
    private ResponseCallback m_responseCallback;

    private void CancelButtonPress(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (this.m_responseCallback != null)
        {
            this.m_responseCallback(false);
        }
        this.Hide();
    }

    private void ConfirmButtonPress(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (this.m_responseCallback != null)
        {
            this.m_responseCallback(true);
        }
        this.Hide();
    }

    public override bool HandleKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            this.CancelButtonPress(null);
            return true;
        }
        return false;
    }

    public override void Hide()
    {
        base.Hide();
        SoundManager.Get().LoadAndPlay("banner_shrink");
    }

    public void SetInfo(Info info)
    {
        string key = "GLOBAL_FRIEND_CHALLENGE_BODY1";
        if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
        {
            key = "GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_BODY1";
        }
        this.m_challengeText1.Text = GameStrings.Get(key);
        this.m_challengeText2.Text = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_BODY2");
        this.m_challengerName.Text = FriendUtils.GetUniqueName(info.m_challenger);
        this.m_responseCallback = info.m_callback;
        bool flag = BnetNearbyPlayerMgr.Get().IsNearbyStranger(info.m_challenger);
        this.m_nearbyPlayerNote.gameObject.SetActive(flag);
    }

    public override void Show()
    {
        base.Show();
        if ((UniversalInputManager.UsePhoneUI != null) && this.m_nearbyPlayerNote.gameObject.activeSelf)
        {
            base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y + 50f, base.transform.localPosition.z);
        }
        base.DoShowAnimation();
        UniversalInputManager.Get().SetSystemDialogActive(true);
        SoundManager.Get().LoadAndPlay("friendly_challenge");
    }

    private void Start()
    {
        this.m_acceptButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ConfirmButtonPress));
        this.m_denyButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CancelButtonPress));
    }

    public class Info
    {
        public FriendlyChallengeDialog.ResponseCallback m_callback;
        public BnetPlayer m_challenger;
    }

    public delegate void ResponseCallback(bool accept);
}

