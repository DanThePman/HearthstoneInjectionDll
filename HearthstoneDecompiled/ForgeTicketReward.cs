using System;
using System.Collections;
using UnityEngine;

[Obsolete("use ArenaTicketReward")]
public class ForgeTicketReward : Reward
{
    public GameObject m_rotateParent;

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
        else
        {
            headline = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_HEADLINE");
            source = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_SOURCE");
        }
        base.SetRewardText(headline, details, source);
        base.m_root.SetActive(true);
        this.m_rotateParent.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
        object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(args);
        iTween.RotateAdd(this.m_rotateParent, hashtable);
    }
}

