using System;
using UnityEngine;

public class FriendListCurrentGameFrame : FriendListBaseFriendFrame
{
    public GameObject m_Background;
    public FriendListButton m_PlayButton;

    protected override void Awake()
    {
        base.Awake();
        this.m_PlayButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPlayButtonPressed));
    }

    private void OnEnable()
    {
        this.UpdateFriend();
    }

    private void OnPlayButtonPressed(UIEvent e)
    {
    }

    public override void UpdateFriend()
    {
        if (base.gameObject.activeSelf && (base.m_player != null))
        {
            base.UpdateFriend();
            base.m_PlayerIcon.m_OnlinePortrait.SetProgramId(BnetProgramId.HEARTHSTONE);
            base.m_PlayerNameText.Text = FriendUtils.GetFriendListName(base.m_player, true);
            base.UpdateOnlineStatus();
        }
    }
}

