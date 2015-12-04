using System;
using System.Runtime.CompilerServices;

public class BoosterPackRewardData : RewardData
{
    public BoosterPackRewardData() : this(0, 0)
    {
    }

    public BoosterPackRewardData(int id, int count) : base(Reward.Type.BOOSTER_PACK)
    {
        this.Id = id;
        this.Count = count;
    }

    protected override string GetGameObjectName()
    {
        return "BoosterPackReward";
    }

    public override string ToString()
    {
        object[] args = new object[] { this.Id, this.Count, base.Origin, base.OriginData };
        return string.Format("[BoosterPackRewardData: BoosterType={0} Count={1} Origin={2} OriginData={3}]", args);
    }

    public int Count { get; set; }

    public int Id { get; set; }
}

