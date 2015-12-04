using System;
using System.Collections;
using UnityEngine;

public class GoldReward : Reward
{
    public GameObject m_coin;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new GoldRewardData(), false);
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            GoldRewardData data = base.Data as GoldRewardData;
            if (data == null)
            {
                Debug.LogWarning(string.Format("goldRewardData.SetData() - data {0} is not GoldRewardData", base.Data));
            }
            else
            {
                string headline = GameStrings.Get("GLOBAL_REWARD_GOLD_HEADLINE");
                string details = data.Amount.ToString();
                string source = string.Empty;
                UberText text = this.m_coin.GetComponentsInChildren<UberText>(true)[0];
                if (text != null)
                {
                    base.m_rewardBanner.m_detailsText = text;
                    base.m_rewardBanner.AlignHeadlineToCenterBone();
                }
                NetCache.ProfileNotice.NoticeOrigin origin = base.Data.Origin;
                switch (origin)
                {
                    case NetCache.ProfileNotice.NoticeOrigin.BETA_REIMBURSE:
                        headline = GameStrings.Get("GLOBAL_BETA_REIMBURSEMENT_HEADLINE");
                        source = GameStrings.Get("GLOBAL_BETA_REIMBURSEMENT_DETAILS");
                        break;

                    case NetCache.ProfileNotice.NoticeOrigin.TOURNEY:
                    {
                        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
                        object[] args = new object[] { netObject.WinsPerGold };
                        source = GameStrings.Format("GLOBAL_REWARD_GOLD_SOURCE_TOURNEY", args);
                        break;
                    }
                    default:
                        if (origin == NetCache.ProfileNotice.NoticeOrigin.IGR)
                        {
                            if (data.Date.HasValue)
                            {
                                object[] objArray2 = new object[] { data.Date };
                                string str4 = GameStrings.Format("GLOBAL_CURRENT_DATE", objArray2);
                                object[] objArray3 = new object[] { str4 };
                                source = GameStrings.Format("GLOBAL_REWARD_GOLD_SOURCE_IGR_DATED", objArray3);
                            }
                            else
                            {
                                source = GameStrings.Get("GLOBAL_REWARD_GOLD_SOURCE_IGR");
                            }
                        }
                        break;
                }
                base.SetRewardText(headline, details, source);
            }
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        GoldRewardData data = base.Data as GoldRewardData;
        if (!data.IsDummyReward)
        {
            bool flag = false;
            if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.BETA_REIMBURSE)
            {
                flag = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>().GetTotal() == 0L;
            }
            else
            {
                flag = updateCacheValues;
            }
            if (flag)
            {
                NetCache.Get().RefreshNetObject<NetCache.NetCacheGoldBalance>();
            }
        }
        base.m_root.SetActive(true);
        Vector3 localScale = this.m_coin.transform.localScale;
        this.m_coin.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        object[] args = new object[] { "scale", localScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
        iTween.ScaleTo(this.m_coin.gameObject, iTween.Hash(args));
        this.m_coin.transform.localEulerAngles = new Vector3(0f, 180f, 180f);
        object[] objArray2 = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(objArray2);
        iTween.RotateAdd(this.m_coin.gameObject, hashtable);
    }
}

