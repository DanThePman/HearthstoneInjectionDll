using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Achievement
{
    private int m_ackProgress;
    private bool m_active;
    private bool m_canAck;
    private TAG_CARD_SET? m_cardSetReq;
    private TAG_CLASS? m_classReq;
    private ClickTriggerType? m_clickType;
    private int m_completionCount;
    private long m_dateCompleted;
    private long m_dateGiven;
    private string m_description = string.Empty;
    private bool m_enabled;
    private SpecialEventType m_eventTrigger;
    private Group m_group;
    private int m_id;
    private int m_maxProgress;
    private string m_name = string.Empty;
    private int m_progress;
    private TAG_RACE? m_raceReq;
    private List<long> m_rewardNoticeIDs = new List<long>();
    private List<RewardData> m_rewards = new List<RewardData>();
    private RewardVisualTiming m_rewardTiming = RewardVisualTiming.IMMEDIATE;
    private Trigger m_trigger;
    private UnlockableFeature? m_unlockedFeature;
    private static readonly int NEW_ACHIEVE_ACK_PROGRESS = -1;

    public Achievement(int id, bool enabled, Group achieveGroup, int maxProgress, Trigger trigger, TAG_RACE? raceReq, TAG_CLASS? classReq, TAG_CARD_SET? cardSetReq, ClickTriggerType? clickType, SpecialEventType eventTrigger, UnlockableFeature? unlockedFeature, List<RewardData> rewards, RewardVisualTiming rewardTiming)
    {
        this.m_id = id;
        this.m_enabled = enabled;
        this.m_group = achieveGroup;
        this.m_maxProgress = maxProgress;
        this.m_trigger = trigger;
        this.m_raceReq = raceReq;
        this.m_classReq = classReq;
        this.m_cardSetReq = cardSetReq;
        this.m_clickType = clickType;
        this.m_eventTrigger = eventTrigger;
        this.SetRewards(rewards);
        this.m_unlockedFeature = unlockedFeature;
        this.m_rewardTiming = rewardTiming;
        this.m_progress = 0;
        this.m_ackProgress = NEW_ACHIEVE_ACK_PROGRESS;
        this.m_completionCount = 0;
        this.m_active = false;
        this.m_dateGiven = 0L;
        this.m_dateCompleted = 0L;
    }

    public void AckCurrentProgressAndRewardNotices()
    {
        this.AckCurrentProgressAndRewardNotices(false);
    }

    public void AckCurrentProgressAndRewardNotices(bool ackIntermediateProgress)
    {
        long[] numArray = this.m_rewardNoticeIDs.ToArray();
        this.m_rewardNoticeIDs.Clear();
        foreach (long num in numArray)
        {
            Network.AckNotice(num);
        }
        if (this.NeedToAcknowledgeProgress(ackIntermediateProgress))
        {
            this.m_ackProgress = this.Progress;
            if (this.m_canAck)
            {
                Network.AckAchieveProgress(this.ID, this.AcknowledgedProgress);
            }
        }
    }

    public void AddChildRewards(List<RewardData> childRewards)
    {
        List<RewardData> rewardDataList = new List<RewardData>(childRewards);
        this.FixUpRewardOrigins(rewardDataList);
        foreach (RewardData data in rewardDataList)
        {
            RewardUtils.AddRewardDataToList(data, this.m_rewards);
        }
    }

    public void AddRewardNoticeID(long noticeID)
    {
        if (!this.m_rewardNoticeIDs.Contains(noticeID))
        {
            if (this.IsCompleted() && !this.NeedToAcknowledgeProgress(false))
            {
                Network.AckNotice(noticeID);
            }
            this.m_rewardNoticeIDs.Add(noticeID);
        }
    }

    private void AutoAckIfNeeded()
    {
        if (this.IsInternal() || (Group.QUEST_DAILY_REPEATABLE == this.AchieveGroup))
        {
            this.AckCurrentProgressAndRewardNotices();
        }
    }

    public void Complete()
    {
        Log.Achievements.Print("Complete: " + this, new object[0]);
        this.SetProgress(this.MaxProgress);
        this.m_completionCount++;
        this.m_active = false;
        this.m_dateCompleted = DateTime.UtcNow.ToFileTimeUtc();
        this.m_canAck = true;
        this.AutoAckIfNeeded();
    }

    private void FixUpRewardOrigins(List<RewardData> rewardDataList)
    {
        foreach (RewardData data in rewardDataList)
        {
            data.SetOrigin(NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT, (long) this.ID);
        }
    }

    public bool IsActiveLicenseAddedAchieve()
    {
        if (this.AchieveTrigger != Trigger.ACCOUNT_LICENSE_ADDED)
        {
            return false;
        }
        return this.Active;
    }

    public bool IsCompleted()
    {
        return (this.Progress >= this.MaxProgress);
    }

    public bool IsInternal()
    {
        return ((this.AchieveGroup == Group.QUEST_INTERNAL_ACTIVE) || (Group.QUEST_INTERNAL_INACTIVE == this.AchieveGroup));
    }

    public bool IsNewlyActive()
    {
        return (this.m_ackProgress == NEW_ACHIEVE_ACK_PROGRESS);
    }

    private bool NeedToAcknowledgeProgress(bool ackIntermediateProgress)
    {
        if (this.AcknowledgedProgress >= this.MaxProgress)
        {
            return false;
        }
        if (this.AcknowledgedProgress == this.Progress)
        {
            return false;
        }
        if ((!ackIntermediateProgress && (this.Progress > 0)) && (this.Progress != this.MaxProgress))
        {
            return false;
        }
        return true;
    }

    public void OnAchieveData(int progress, int acknowledgedProgress, int completionCount, bool isActive, long dateGiven, long dateCompleted, bool canAcknowledge)
    {
        this.SetProgress(progress);
        this.SetAcknowledgedProgress(acknowledgedProgress);
        this.m_completionCount = completionCount;
        this.m_active = isActive;
        this.m_dateGiven = dateGiven;
        this.m_dateCompleted = dateCompleted;
        this.m_canAck = canAcknowledge;
        this.AutoAckIfNeeded();
    }

    public void OnCancelSuccess()
    {
        this.m_active = false;
    }

    private void SetAcknowledgedProgress(int acknowledgedProgress)
    {
        this.m_ackProgress = Mathf.Clamp(acknowledgedProgress, NEW_ACHIEVE_ACK_PROGRESS, this.Progress);
    }

    public void SetDescription(string description)
    {
        this.m_description = description;
    }

    public void SetName(string name)
    {
        this.m_name = name;
    }

    private void SetProgress(int progress)
    {
        this.m_progress = Mathf.Clamp(progress, 0, this.MaxProgress);
    }

    private void SetRewards(List<RewardData> rewardDataList)
    {
        this.m_rewards = new List<RewardData>(rewardDataList);
        this.FixUpRewardOrigins(this.m_rewards);
    }

    public override string ToString()
    {
        object[] args = new object[] { this.ID, this.AchieveGroup, this.Name, this.MaxProgress, this.Progress, this.AcknowledgedProgress, this.Active, this.DateGiven, this.DateCompleted, this.Description, this.AchieveTrigger };
        return string.Format("[Achievement: ID={0} AchieveGroup={1} Name='{2}' MaxProgress={3} Progress={4} AckProgress={5} IsActive={6} DateGiven={7} DateCompleted={8} Description='{9}' Trigger={10}]", args);
    }

    public void UpdateActiveAchieve(int progress, int acknowledgedProgress, long dateGiven, bool canAcknowledge)
    {
        this.SetProgress(progress);
        this.SetAcknowledgedProgress(acknowledgedProgress);
        this.m_active = true;
        this.m_dateGiven = dateGiven;
        this.m_canAck = canAcknowledge;
        this.AutoAckIfNeeded();
    }

    public Group AchieveGroup
    {
        get
        {
            return this.m_group;
        }
    }

    public Trigger AchieveTrigger
    {
        get
        {
            return this.m_trigger;
        }
    }

    public int AcknowledgedProgress
    {
        get
        {
            return this.m_ackProgress;
        }
    }

    public bool Active
    {
        get
        {
            return this.m_active;
        }
    }

    public bool CanBeAcknowledged
    {
        get
        {
            return this.m_canAck;
        }
    }

    public TAG_CARD_SET? CardSetRequirement
    {
        get
        {
            return this.m_cardSetReq;
        }
    }

    public TAG_CLASS? ClassRequirement
    {
        get
        {
            return this.m_classReq;
        }
    }

    public ClickTriggerType? ClickType
    {
        get
        {
            return this.m_clickType;
        }
    }

    public int CompletionCount
    {
        get
        {
            return this.m_completionCount;
        }
    }

    public long DateCompleted
    {
        get
        {
            return this.m_dateCompleted;
        }
    }

    public long DateGiven
    {
        get
        {
            return this.m_dateGiven;
        }
    }

    public string Description
    {
        get
        {
            return this.m_description;
        }
    }

    public bool Enabled
    {
        get
        {
            return this.m_enabled;
        }
    }

    public SpecialEventType EventTrigger
    {
        get
        {
            return this.m_eventTrigger;
        }
    }

    public int ID
    {
        get
        {
            return this.m_id;
        }
    }

    public int MaxProgress
    {
        get
        {
            return this.m_maxProgress;
        }
    }

    public string Name
    {
        get
        {
            return this.m_name;
        }
    }

    public int Progress
    {
        get
        {
            return this.m_progress;
        }
    }

    public TAG_RACE? RaceRequirement
    {
        get
        {
            return this.m_raceReq;
        }
    }

    public List<RewardData> Rewards
    {
        get
        {
            return this.m_rewards;
        }
    }

    public RewardVisualTiming RewardTiming
    {
        get
        {
            return this.m_rewardTiming;
        }
    }

    public UnlockableFeature? UnlockedFeature
    {
        get
        {
            return this.m_unlockedFeature;
        }
    }

    public enum ClickTriggerType
    {
        BUTTON_ARENA = 2,
        BUTTON_PLAY = 1
    }

    public enum Group
    {
        [Description("daily")]
        QUEST_DAILY = 0,
        [Description("daily_repeatable")]
        QUEST_DAILY_REPEATABLE = 1,
        [Description("hidden")]
        QUEST_HIDDEN = 2,
        [Description("internal_active")]
        QUEST_INTERNAL_ACTIVE = 3,
        [Description("internal_inactive")]
        QUEST_INTERNAL_INACTIVE = 4,
        [Description("starter")]
        QUEST_STARTER = 5,
        [Description("goldhero")]
        UNLOCK_GOLDEN_HERO = 6,
        [Description("hero")]
        UNLOCK_HERO = 7
    }

    public enum Trigger
    {
        [Description("licenseadded")]
        ACCOUNT_LICENSE_ADDED = 1,
        [Description("adventure_progress")]
        ADVENTURE_PROGRESS = 2,
        [Description("click")]
        CLICK = 3,
        [Description("cardset")]
        COMPLETE_CARD_SET = 4,
        [Description("event")]
        EVENT = 5,
        [Description("event_timing_only")]
        EVENT_TIMING_ONLY = 6,
        [Description("race")]
        GAIN_CARD = 7,
        [Description("goldrace")]
        GAIN_GOLDEN_CARD = 8,
        [Description("none")]
        IGNORE = 0,
        [Description("purchase")]
        PURCHASE = 9,
        [Description("win")]
        WIN = 10
    }

    public enum UnlockableFeature
    {
        [Description("daily")]
        DAILY_QUESTS = 0,
        [Description("forge")]
        FORGE = 1,
        [Description("naxx1_owned")]
        NAXX_WING_1_OWNED = 2,
        [Description("naxx1_playable")]
        NAXX_WING_1_PLAYABLE = 7,
        [Description("naxx2_owned")]
        NAXX_WING_2_OWNED = 3,
        [Description("naxx2_playable")]
        NAXX_WING_2_PLAYABLE = 8,
        [Description("naxx3_owned")]
        NAXX_WING_3_OWNED = 4,
        [Description("naxx3_playable")]
        NAXX_WING_3_PLAYABLE = 9,
        [Description("naxx4_owned")]
        NAXX_WING_4_OWNED = 5,
        [Description("naxx4_playable")]
        NAXX_WING_4_PLAYABLE = 10,
        [Description("naxx5_owned")]
        NAXX_WING_5_OWNED = 6,
        [Description("naxx5_playable")]
        NAXX_WING_5_PLAYABLE = 11,
        [Description("vanilla heroes")]
        VANILLA_HEROES = 12
    }
}

