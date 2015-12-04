using System;
using UnityEngine;

[Serializable]
public class RightGamesWonSegment : GamesWonSegment
{
    public GameObject m_boosterRoot;
    public UberText m_dustAmountText;
    public GameObject m_dustRoot;
    public UberText m_goldAmountText;
    public GameObject m_goldRoot;
    private Reward.Type m_rewardType;

    public override void AnimateReward()
    {
        base.m_crown.Animate();
        PlayMakerFSM component = null;
        switch (this.m_rewardType)
        {
            case Reward.Type.ARCANE_DUST:
                component = this.m_dustRoot.GetComponent<PlayMakerFSM>();
                break;

            case Reward.Type.BOOSTER_PACK:
                component = this.m_boosterRoot.GetComponent<PlayMakerFSM>();
                break;

            case Reward.Type.GOLD:
                component = this.m_goldRoot.GetComponent<PlayMakerFSM>();
                break;
        }
        if (component == null)
        {
            Debug.LogError(string.Format("GamesWonIndicatorSegment(): missing playMaker component for reward type {0}", this.m_rewardType));
        }
        else
        {
            component.SendEvent("Birth");
        }
    }

    public override float GetWidth()
    {
        Reward.Type rewardType = this.m_rewardType;
        if (rewardType != Reward.Type.ARCANE_DUST)
        {
            if (rewardType == Reward.Type.BOOSTER_PACK)
            {
                return this.m_boosterRoot.GetComponent<Renderer>().bounds.size.x;
            }
            if (rewardType == Reward.Type.GOLD)
            {
                return this.m_goldRoot.GetComponent<Renderer>().bounds.size.x;
            }
            return 0f;
        }
        return this.m_dustRoot.GetComponent<Renderer>().bounds.size.x;
    }

    public override void Init(Reward.Type rewardType, int rewardAmount, bool hideCrown)
    {
        base.Init(rewardType, rewardAmount, hideCrown);
        this.m_rewardType = rewardType;
        Reward.Type type = this.m_rewardType;
        if (type == Reward.Type.ARCANE_DUST)
        {
            this.m_boosterRoot.SetActive(false);
            this.m_goldRoot.SetActive(false);
            this.m_dustRoot.SetActive(true);
            this.m_dustAmountText.Text = rewardAmount.ToString();
        }
        else if (type == Reward.Type.BOOSTER_PACK)
        {
            this.m_dustRoot.SetActive(false);
            this.m_goldRoot.SetActive(false);
            this.m_boosterRoot.SetActive(true);
        }
        else if (type == Reward.Type.GOLD)
        {
            this.m_boosterRoot.SetActive(false);
            this.m_dustRoot.SetActive(false);
            this.m_goldRoot.SetActive(true);
            this.m_goldAmountText.Text = rewardAmount.ToString();
        }
        else
        {
            Debug.LogError(string.Format("GamesWonIndicatorSegment(): don't know how to init right segment with reward type {0}", this.m_rewardType));
            this.m_boosterRoot.SetActive(false);
            this.m_dustRoot.SetActive(false);
            this.m_goldRoot.SetActive(false);
        }
    }
}

