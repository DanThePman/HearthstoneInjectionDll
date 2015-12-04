using System;
using UnityEngine;

public class FriendListChallengeButton : FriendListUIElement
{
    public GameObject m_AvailableIcon;
    public GameObject m_BusyIcon;
    public GameObject m_DeleteIcon;
    private BnetPlayer m_player;
    public TextureOffsetStates m_SpectatorIcon;
    public GameObject m_SpectatorIconHighlight;
    public GameObject m_TavernBrawlBusyIcon;
    public GameObject m_TavernBrawlChallengeIcon;
    public TooltipZone m_TooltipZone;

    public bool CanChallenge()
    {
        return FriendChallengeMgr.Get().CanChallenge(this.m_player);
    }

    public BnetPlayer GetPlayer()
    {
        return this.m_player;
    }

    private void HideTooltip()
    {
        this.m_TooltipZone.HideTooltip();
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.OnOut(oldState);
        this.HideTooltip();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.OnOver(oldState);
        this.ShowTooltip();
    }

    protected override void OnPress()
    {
        base.OnPress();
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.ShowTooltip();
        }
    }

    public bool SetPlayer(BnetPlayer player)
    {
        if (this.m_player == player)
        {
            return false;
        }
        this.m_player = player;
        this.UpdateButton();
        return true;
    }

    protected override bool ShouldBeHighlighted()
    {
        if (!this.CanChallenge())
        {
            return false;
        }
        return base.ShouldBeHighlighted();
    }

    private void ShowTooltip()
    {
        string str;
        string str2;
        BnetGameAccountId hearthstoneGameAccountId = this.m_player.GetHearthstoneGameAccountId();
        SpectatorManager manager = SpectatorManager.Get();
        if (manager.HasInvitedMeToSpectate(hearthstoneGameAccountId))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_AVAILABLE_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_RECEIVED_INVITE_TEXT";
        }
        else if (manager.CanSpectate(this.m_player))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_AVAILABLE_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_AVAILABLE_TEXT";
        }
        else if (manager.IsSpectatingMe(hearthstoneGameAccountId))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_KICK_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_KICK_TEXT";
        }
        else if (manager.CanInviteToSpectateMyGame(hearthstoneGameAccountId))
        {
            if (manager.IsPlayerSpectatingMyGamesOpposingSide(hearthstoneGameAccountId))
            {
                str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_OTHER_SIDE_HEADER";
                str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_OTHER_SIDE_TEXT";
            }
            else
            {
                str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_HEADER";
                str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_TEXT";
            }
        }
        else if (manager.IsInvitedToSpectateMyGame(hearthstoneGameAccountId))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITED_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITED_TEXT";
        }
        else if (manager.IsSpectatingPlayer(hearthstoneGameAccountId))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_SPECTATING_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_SPECTATING_TEXT";
        }
        else if (manager.HasPreviouslyKickedMeFromGame(hearthstoneGameAccountId))
        {
            str = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_PREVIOUSLY_KICKED_HEADER";
            str2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_PREVIOUSLY_KICKED_TEXT";
        }
        else
        {
            bool flag = TavernBrawlManager.Get().ShouldNewFriendlyChallengeBeTavernBrawl();
            if (flag)
            {
                str = "GLOBAL_FRIENDLIST_TAVERN_BRAWL_CHALLENGE_BUTTON_HEADER";
            }
            else
            {
                str = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
            }
            if (!FriendChallengeMgr.Get().AmIAvailable())
            {
                str2 = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_IM_UNAVAILABLE";
            }
            else if (!FriendChallengeMgr.Get().CanChallenge(this.m_player))
            {
                str2 = null;
                TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
                if ((flag && mission.canCreateDeck) && !TavernBrawlManager.Get().HasValidDeck())
                {
                    str2 = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_TAVERN_BRAWL_MUST_CREATE_DECK";
                }
                if (str2 == null)
                {
                    str2 = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_THEYRE_UNAVAILABLE";
                }
            }
            else if (flag)
            {
                str2 = "GLOBAL_FRIENDLIST_TAVERN_BRAWL_CHALLENGE_BUTTON_AVAILABLE";
            }
            else
            {
                str2 = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
            }
        }
        if (UniversalInputManager.Get().IsTouchMode())
        {
            if (GameStrings.HasKey(str + "_TOUCH"))
            {
                str = str + "_TOUCH";
            }
            if (GameStrings.HasKey(str2 + "_TOUCH"))
            {
                str2 = str2 + "_TOUCH";
            }
        }
        string headline = GameStrings.Get(str);
        object[] args = new object[] { this.m_player.GetBestName() };
        string bodytext = GameStrings.Format(str2, args);
        this.m_TooltipZone.ShowSocialTooltip(this, headline, bodytext, 75f, GameLayer.BattleNetDialog);
    }

    public void UpdateButton()
    {
        if (!this.UpdateEditModeButtonState())
        {
            if (((this.m_player == null) || !this.m_player.IsOnline()) || (this.m_player.GetBestProgramId() != BnetProgramId.HEARTHSTONE))
            {
                base.gameObject.SetActive(false);
            }
            else
            {
                base.gameObject.SetActive(true);
                if (!this.UpdateSpectateButtonState())
                {
                    bool flag = false;
                    bool flag2 = false;
                    if (this.CanChallenge())
                    {
                        flag = true;
                    }
                    else
                    {
                        flag2 = true;
                    }
                    bool flag3 = TavernBrawlManager.Get().ShouldNewFriendlyChallengeBeTavernBrawl();
                    this.m_AvailableIcon.SetActive(flag && !flag3);
                    this.m_BusyIcon.SetActive(flag2 && !flag3);
                    this.m_TavernBrawlChallengeIcon.SetActive(flag && flag3);
                    this.m_TavernBrawlBusyIcon.SetActive(flag2 && flag3);
                    this.UpdateTooltip();
                }
            }
        }
    }

    private bool UpdateEditModeButtonState()
    {
        if (this.m_player == null)
        {
            base.gameObject.SetActive(false);
            return true;
        }
        if (((ChatMgr.Get() != null) && (ChatMgr.Get().FriendListFrame != null)) && ChatMgr.Get().FriendListFrame.IsInEditMode)
        {
            base.gameObject.SetActive(true);
            this.m_AvailableIcon.SetActive(false);
            this.m_BusyIcon.SetActive(false);
            this.m_SpectatorIcon.gameObject.SetActive(false);
            this.m_TavernBrawlChallengeIcon.SetActive(false);
            this.m_TavernBrawlBusyIcon.SetActive(false);
            if (this.m_DeleteIcon != null)
            {
                this.m_DeleteIcon.SetActive(true);
            }
            return true;
        }
        if (this.m_DeleteIcon != null)
        {
            this.m_DeleteIcon.SetActive(false);
        }
        return false;
    }

    private bool UpdateSpectateButtonState()
    {
        BnetGameAccountId hearthstoneGameAccountId = this.m_player.GetHearthstoneGameAccountId();
        SpectatorManager manager = SpectatorManager.Get();
        bool flag = false;
        string str = null;
        if (manager.HasInvitedMeToSpectate(hearthstoneGameAccountId))
        {
            str = "HasInvitedMeToSpectate";
            flag = true;
        }
        else if (manager.CanSpectate(this.m_player))
        {
            str = "CanSpectateThisFriend";
        }
        else if (manager.IsSpectatingMe(hearthstoneGameAccountId))
        {
            str = "CurrentlySpectatingMe";
        }
        else if (manager.CanInviteToSpectateMyGame(hearthstoneGameAccountId))
        {
            str = "CanInviteToSpectateMe";
        }
        else if (manager.IsSpectatingPlayer(hearthstoneGameAccountId))
        {
            str = "CurrentlySpectatingThisFriend";
        }
        else if (manager.IsInvitedToSpectateMyGame(hearthstoneGameAccountId))
        {
            str = "DisabledInviteToSpectateMe";
        }
        if (this.m_SpectatorIcon != null)
        {
            this.m_SpectatorIcon.gameObject.SetActive(str != null);
            if (str != null)
            {
                this.m_SpectatorIcon.CurrentState = str;
                this.m_AvailableIcon.SetActive(false);
                this.m_BusyIcon.SetActive(false);
                this.m_TavernBrawlChallengeIcon.SetActive(false);
                this.m_TavernBrawlBusyIcon.SetActive(false);
            }
            if (this.m_SpectatorIconHighlight != null)
            {
                this.m_SpectatorIconHighlight.gameObject.SetActive(flag);
            }
        }
        return (str != null);
    }

    private void UpdateTooltip()
    {
        if (this.m_TooltipZone.IsShowingTooltip())
        {
            this.HideTooltip();
            this.ShowTooltip();
        }
    }
}

