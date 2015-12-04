using System;
using System.Runtime.CompilerServices;

public class CardBackRewardData : RewardData
{
    public CardBackRewardData() : this(0)
    {
    }

    public CardBackRewardData(int cardBackID) : base(Reward.Type.CARD_BACK)
    {
        this.CardBackID = cardBackID;
    }

    protected override string GetGameObjectName()
    {
        return "CardBackReward";
    }

    public override string ToString()
    {
        return string.Format("[CardBackRewardData: CardBackID={0} Origin={1} OriginData={2}]", this.CardBackID, base.Origin, base.OriginData);
    }

    public int CardBackID { get; set; }
}

