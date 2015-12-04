using System;
using UnityEngine;

public class BnetBarFriendButton : FriendListUIElement
{
    public Color m_AllOfflineColor;
    public Color m_AnyOnlineColor;
    public UberText m_OnlineCountText;
    public GameObject m_PendingInvitesIcon;
    private static BnetBarFriendButton s_instance;

    protected override void Awake()
    {
        s_instance = this;
        base.Awake();
        this.UpdateOnlineCount();
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        this.ShowPendingInvitesIcon(false);
    }

    public static BnetBarFriendButton Get()
    {
        return s_instance;
    }

    public void HideTooltip()
    {
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.HideTooltip();
        }
    }

    private void OnDestroy()
    {
        BnetFriendMgr.Get().RemoveChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        s_instance = null;
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        this.UpdateOnlineCount();
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.OnOut(oldState);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("Small_Mouseover");
        base.UpdateHighlight();
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        this.UpdateOnlineCount();
    }

    public void ShowPendingInvitesIcon(bool show)
    {
        if (this.m_PendingInvitesIcon != null)
        {
            this.m_PendingInvitesIcon.SetActive(show);
            this.m_OnlineCountText.gameObject.SetActive(!show);
        }
    }

    private void UpdateOnlineCount()
    {
        int activeOnlineFriendCount = BnetFriendMgr.Get().GetActiveOnlineFriendCount();
        if (activeOnlineFriendCount == 0)
        {
            this.m_OnlineCountText.TextColor = this.m_AllOfflineColor;
        }
        else
        {
            this.m_OnlineCountText.TextColor = this.m_AnyOnlineColor;
        }
        this.m_OnlineCountText.Text = activeOnlineFriendCount.ToString();
    }
}

