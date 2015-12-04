using System;
using System.Collections;
using UnityEngine;

public class ArenaTicketReward : Reward
{
    public UberText m_countLabel;
    public UberText m_playerNameLabel;
    public GameObject m_ticketVisual;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new ForgeTicketRewardData(), false);
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        string headline = string.Empty;
        string details = string.Empty;
        string source = string.Empty;
        if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
        {
            ForgeTicketRewardData data = base.Data as ForgeTicketRewardData;
            headline = GameStrings.Get("GLOBAL_REWARD_FORGE_HEADLINE");
            object[] objArray1 = new object[] { data.Quantity };
            source = GameStrings.Format("GLOBAL_REWARD_BOOSTER_DETAILS_OUT_OF_BAND", objArray1);
        }
        else if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT)
        {
            headline = GameStrings.Get("GLOBAL_REWARD_ARENA_TICKET_HEADLINE");
        }
        else
        {
            headline = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_HEADLINE");
            source = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_SOURCE");
        }
        base.SetRewardText(headline, details, source);
        if (this.m_countLabel != null)
        {
            ForgeTicketRewardData data2 = base.Data as ForgeTicketRewardData;
            this.m_countLabel.Text = data2.Quantity.ToString();
        }
        if (this.m_playerNameLabel != null)
        {
            BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
            if (myPlayer != null)
            {
                this.m_playerNameLabel.Text = myPlayer.GetBattleTag().GetName();
            }
        }
        base.m_root.SetActive(true);
        this.m_ticketVisual.transform.localEulerAngles = new Vector3(this.m_ticketVisual.transform.localEulerAngles.x, this.m_ticketVisual.transform.localEulerAngles.y, 180f);
        object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(args);
        iTween.RotateAdd(this.m_ticketVisual, hashtable);
    }
}

