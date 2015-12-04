using System;
using UnityEngine;

public class MountReward : Reward
{
    public GameObject m_mount;

    protected override void HideReward()
    {
        base.HideReward();
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new MountRewardData(), false);
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            MountRewardData data = base.Data as MountRewardData;
            if (data != null)
            {
                string headline = string.Empty;
                switch (data.Mount)
                {
                    case MountRewardData.MountType.WOW_HEARTHSTEED:
                        headline = GameStrings.Get("GLOBAL_REWARD_HEARTHSTEED_HEADLINE");
                        break;

                    case MountRewardData.MountType.HEROES_MAGIC_CARPET_CARD:
                        headline = GameStrings.Get("GLOBAL_REWARD_HEROES_CARD_MOUNT_HEADLINE");
                        break;
                }
                base.SetRewardText(headline, string.Empty, string.Empty);
            }
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        if (!(base.Data is MountRewardData))
        {
            Debug.LogWarning(string.Format("MountReward.ShowReward() - Data {0} is not MountRewardData", base.Data));
        }
        else
        {
            base.m_root.SetActive(true);
            Vector3 localScale = this.m_mount.transform.localScale;
            this.m_mount.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            object[] args = new object[] { "scale", localScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
            iTween.ScaleTo(this.m_mount.gameObject, iTween.Hash(args));
        }
    }
}

