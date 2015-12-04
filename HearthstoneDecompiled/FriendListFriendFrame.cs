using System;
using System.Collections;
using UnityEngine;

public class FriendListFriendFrame : FriendListBaseFriendFrame
{
    public FriendListFriendFrameBones m_Bones;
    public FriendListChallengeButton m_ChallengeButton;
    public FriendListFriendFrameOffsets m_Offsets;
    private Component[] m_rightComponentOrder;
    private const float REFRESH_FRIENDS_SECONDS = 30f;

    private Vector3 AddWidth(Component component)
    {
        Vector3 vector = new Vector3();
        if (component.gameObject.activeInHierarchy)
        {
            Bounds bounds = TransformUtil.ComputeSetPointBounds(component, true);
            vector.x += bounds.max.x - bounds.min.x;
        }
        return vector;
    }

    protected override void Awake()
    {
        base.Awake();
        this.m_ChallengeButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnChallengeButtonReleased));
        this.m_rightComponentOrder = new Component[] { base.m_RecruitUI, base.m_ChatIcon, this.m_ChallengeButton };
        object[] args = new object[] { "time", 30f, "looptype", iTween.LoopType.loop, "oncomplete", "UpdateFriend", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Timer(base.gameObject, hashtable);
    }

    private float ComputeLeftComponentWidth(Transform bone, Vector3 offset, Component rightComponent)
    {
        Vector3 vector = bone.position + offset;
        Bounds bounds = TransformUtil.ComputeSetPointBounds(rightComponent);
        float num = (bounds.center.x - bounds.extents.x) + this.m_Offsets.m_RightComponent.x;
        return (num - vector.x);
    }

    private void LayoutLeftText(UberText text, Transform bone, Vector3 offset, Component rightComponent)
    {
        if (text.gameObject.activeInHierarchy)
        {
            text.Width = this.ComputeLeftComponentWidth(bone, offset, rightComponent);
            TransformUtil.SetPoint((Component) text, Anchor.LEFT, (Component) bone, Anchor.RIGHT, offset);
        }
    }

    private void OnChallengeButtonReleased(UIEvent e)
    {
        OnPlayerChallengeButtonPressed(this.m_ChallengeButton, base.m_player);
    }

    private void OnEnable()
    {
        this.UpdateFriend();
    }

    private static void OnKickSpectatorDialogResponse(AlertPopup.Response response, object userData)
    {
        BnetPlayer player = (BnetPlayer) userData;
        if (response == AlertPopup.Response.CONFIRM)
        {
            SpectatorManager.Get().KickSpectator(player, true);
        }
    }

    private static void OnLeaveSpectatingDialogResponse(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CONFIRM)
        {
            SpectatorManager.Get().LeaveSpectatorMode();
        }
    }

    public static void OnPlayerChallengeButtonPressed(FriendListChallengeButton challengeButton, BnetPlayer player)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        if (ChatMgr.Get().FriendListFrame.IsInEditMode)
        {
            ChatMgr.Get().FriendListFrame.ShowRemoveFriendPopup(player);
        }
        else
        {
            BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
            SpectatorManager manager = SpectatorManager.Get();
            if (manager.CanSpectate(player))
            {
                manager.SpectatePlayer(player);
            }
            else if (manager.IsSpectatingMe(hearthstoneGameAccountId))
            {
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_KICK_PROMPT_HEADER")
                };
                object[] args = new object[] { FriendUtils.GetUniqueName(player) };
                info.m_text = GameStrings.Format("GLOBAL_SPECTATOR_KICK_PROMPT_TEXT", args);
                info.m_showAlertIcon = true;
                info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
                info.m_responseCallback = new AlertPopup.ResponseCallback(FriendListFriendFrame.OnKickSpectatorDialogResponse);
                info.m_responseUserData = player;
                DialogManager.Get().ShowPopup(info);
            }
            else if (manager.CanInviteToSpectateMyGame(hearthstoneGameAccountId))
            {
                manager.InviteToSpectateMe(player);
            }
            else if (manager.IsSpectatingPlayer(hearthstoneGameAccountId))
            {
                if ((GameMgr.Get().IsFindingGame() || SceneMgr.Get().IsTransitioning()) || GameMgr.Get().IsTransitionPopupShown())
                {
                    return;
                }
                AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_LEAVE_PROMPT_HEADER"),
                    m_text = GameStrings.Get("GLOBAL_SPECTATOR_LEAVE_PROMPT_TEXT"),
                    m_showAlertIcon = true,
                    m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                    m_responseCallback = new AlertPopup.ResponseCallback(FriendListFriendFrame.OnLeaveSpectatingDialogResponse)
                };
                DialogManager.Get().ShowPopup(info2);
            }
            else if (!manager.IsInvitedToSpectateMyGame(hearthstoneGameAccountId) && challengeButton.CanChallenge())
            {
                FriendChallengeMgr.Get().SendChallenge(player);
            }
            else
            {
                return;
            }
            challengeButton.UpdateButton();
            ChatMgr.Get().CloseChatUI();
        }
    }

    public override bool SetFriend(BnetPlayer player)
    {
        this.m_ChallengeButton.SetPlayer(player);
        if (!base.SetFriend(player))
        {
            return false;
        }
        return true;
    }

    public override void UpdateFriend()
    {
        if (base.gameObject.activeSelf && (base.m_player != null))
        {
            base.UpdateFriend();
            base.m_PlayerIcon.UpdateIcon();
            if (base.m_player.IsOnline())
            {
                base.m_PlayerNameText.Text = FriendUtils.GetFriendListName(base.m_player, true);
                base.UpdateOnlineStatus();
            }
            else
            {
                base.m_PlayerNameText.Text = FriendUtils.GetFriendListName(base.m_player, true);
                base.UpdateOfflineStatus();
            }
            base.m_recruitInfo = RecruitListMgr.Get().GetRecruitInfoFromAccountId(base.m_player.GetAccountId());
            base.m_RecruitUI.SetInfo(base.m_recruitInfo);
            if (base.m_recruitInfo != null)
            {
                base.m_RecruitUI.m_recruitText.TextColor = base.m_PlayerNameText.TextColor;
            }
            this.m_ChallengeButton.UpdateButton();
            this.UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        Component rightComponent = this.m_Bones.m_RightComponent;
        for (int i = this.m_rightComponentOrder.Length - 1; i >= 0; i--)
        {
            Component src = this.m_rightComponentOrder[i];
            if (src.gameObject.activeSelf)
            {
                TransformUtil.SetPoint(src, Anchor.RIGHT, rightComponent, Anchor.LEFT, this.m_Offsets.m_RightComponent);
                rightComponent = src;
            }
        }
        Vector3 vector = new Vector3(0f, 0f, 0f);
        vector += this.AddWidth(base.m_PlayerIcon);
        vector += this.AddWidth(base.m_rankMedal);
        this.LayoutLeftText(base.m_PlayerNameText, this.m_Bones.m_PlayerNameText, this.m_Offsets.m_PlayerNameText + vector, rightComponent);
        this.LayoutLeftText(base.m_StatusText, this.m_Bones.m_StatusText, this.m_Offsets.m_StatusText + vector, rightComponent);
        base.m_rankMedal.transform.position = this.m_Bones.m_Medal.transform.position;
    }
}

