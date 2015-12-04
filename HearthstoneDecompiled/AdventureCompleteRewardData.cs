using System;
using System.Runtime.CompilerServices;

public class AdventureCompleteRewardData : RewardData
{
    private const string s_DefaultRewardObject = "AdventureCompleteReward_Naxxramas";

    public AdventureCompleteRewardData() : this("AdventureCompleteReward_Naxxramas", string.Empty, false)
    {
    }

    public AdventureCompleteRewardData(string rewardObjectName, string bannerText, bool isBadlyHurt) : base(Reward.Type.CLASS_CHALLENGE)
    {
        this.RewardObjectName = rewardObjectName;
        this.BannerText = bannerText;
        this.IsBadlyHurt = isBadlyHurt;
    }

    protected override string GetGameObjectName()
    {
        return this.RewardObjectName;
    }

    public override string ToString()
    {
        return string.Format("[AdventureCompleteRewardData: RewardObjectName={0} Origin={1} OriginData={2}]", this.RewardObjectName, base.Origin, base.OriginData);
    }

    public string BannerText { get; set; }

    public bool IsBadlyHurt { get; set; }

    public string RewardObjectName { get; set; }
}

