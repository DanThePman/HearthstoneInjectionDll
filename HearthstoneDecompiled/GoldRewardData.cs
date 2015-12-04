using System;
using System.Runtime.CompilerServices;

public class GoldRewardData : RewardData
{
    public GoldRewardData() : this(0L)
    {
    }

    public GoldRewardData(long amount) : this(amount, null)
    {
    }

    public GoldRewardData(long amount, DateTime? date) : base(Reward.Type.GOLD)
    {
        this.Amount = amount;
        this.Date = date;
    }

    protected override string GetGameObjectName()
    {
        return "GoldReward";
    }

    public override string ToString()
    {
        return string.Format("[GoldRewardData: Amount={0} Origin={1} OriginData={2}]", this.Amount, base.Origin, base.OriginData);
    }

    public long Amount { get; set; }

    public DateTime? Date { get; set; }
}

