using PegasusShared;
using System;

public class TavernBrawlMission
{
    public bool canCreateDeck;
    public bool canEditDeck;
    public bool canSelectHeroForDeck;
    public DateTime? endDateLocal;
    public int missionId = -1;
    public long RewardData1;
    public long RewardData2;
    public RewardTrigger rewardTrigger = RewardTrigger.REWARD_TRIGGER_NONE;
    public RewardType rewardType = RewardType.REWARD_NONE;
    public int seasonId;
}

