using System;
using UnityEngine;

public class ArcaneDustReward : Reward
{
    public UberText m_dustCount;
    public GameObject m_dustJar;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new ArcaneDustRewardData(), false);
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            string headline = GameStrings.Get("GLOBAL_REWARD_ARCANE_DUST_HEADLINE");
            base.SetRewardText(headline, string.Empty, string.Empty);
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        ArcaneDustRewardData data = base.Data as ArcaneDustRewardData;
        if (data == null)
        {
            Debug.LogWarning(string.Format("ArcaneDustReward.ShowReward() - Data {0} is not ArcaneDustRewardData", base.Data));
        }
        else
        {
            if (!data.IsDummyReward && updateCacheValues)
            {
                NetCache.Get().OnArcaneDustBalanceChanged((long) data.Amount);
                if (CraftingManager.Get() != null)
                {
                    CraftingManager.Get().AdjustLocalArcaneDustBalance(data.Amount);
                    CraftingManager.Get().UpdateBankText();
                }
            }
            base.m_root.SetActive(true);
            this.m_dustCount.Text = data.Amount.ToString();
            Vector3 localScale = this.m_dustJar.transform.localScale;
            this.m_dustJar.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            object[] args = new object[] { "scale", localScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
            iTween.ScaleTo(this.m_dustJar.gameObject, iTween.Hash(args));
        }
    }
}

