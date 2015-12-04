using System;
using UnityEngine;

public class PlayerIcon : PegUIElement
{
    private bool m_hidden;
    public GameObject m_OfflineIcon;
    public GameObject m_OnlineIcon;
    public PlayerPortrait m_OnlinePortrait;
    private BnetPlayer m_player;

    public BnetPlayer GetPlayer()
    {
        return this.m_player;
    }

    public void Hide()
    {
        this.m_hidden = true;
        base.gameObject.SetActive(false);
    }

    public bool SetPlayer(BnetPlayer player)
    {
        if (this.m_player == player)
        {
            return false;
        }
        this.m_player = player;
        this.UpdateIcon();
        return true;
    }

    public void Show()
    {
        this.m_hidden = false;
        base.gameObject.SetActive(true);
    }

    public void UpdateIcon()
    {
        if (this.m_player.IsOnline() && (this.m_player.GetBestProgramId() != BnetProgramId.PHOENIX))
        {
            if (!this.m_hidden)
            {
                base.gameObject.SetActive(true);
            }
            BnetProgramId bestProgramId = this.m_player.GetBestProgramId();
            this.m_OnlinePortrait.SetProgramId(bestProgramId);
        }
        else
        {
            base.gameObject.SetActive(false);
        }
    }
}

