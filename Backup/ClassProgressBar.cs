using System;
using UnityEngine;

public class ClassProgressBar : PegUIElement
{
    public TAG_CLASS m_class;
    public GameObject m_classIcon;
    public GameObject m_classLockedGO;
    public UberText m_classNameText;
    public UberText m_levelText;
    public UberText m_lockedText;
    private NetCache.HeroLevel.NextLevelReward m_nextLevelReward;
    public ProgressBar m_progressBar;
    private string m_rewardText;

    protected override void Awake()
    {
        base.Awake();
        this.m_lockedText.Text = GameStrings.Get("GLUE_QUEST_LOG_CLASS_LOCKED");
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnProgressBarOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnProgressBarOut));
    }

    public void Init()
    {
        this.m_classNameText.Text = GameStrings.GetClassName(this.m_class);
    }

    private void OnProgressBarOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    private void OnProgressBarOver(UIEvent e)
    {
        if (this.m_rewardText != null)
        {
            this.ShowTooltip();
        }
        else if (this.m_nextLevelReward != null)
        {
            RewardData reward = this.m_nextLevelReward.Reward;
            if (reward != null)
            {
                this.m_rewardText = RewardUtils.GetRewardText(reward);
                this.ShowTooltip();
            }
        }
    }

    public void SetNextReward(NetCache.HeroLevel.NextLevelReward nextLevelReward)
    {
        this.m_nextLevelReward = nextLevelReward;
    }

    private void ShowTooltip()
    {
        object[] args = new object[] { this.m_nextLevelReward.Level };
        KeywordHelpPanel panel = base.gameObject.GetComponent<TooltipZone>().ShowLayerTooltip(GameStrings.Format("GLOBAL_HERO_LEVEL_NEXT_REWARD_TITLE", args), this.m_rewardText);
        panel.m_name.WordWrap = false;
        panel.m_name.UpdateNow();
    }
}

