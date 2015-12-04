using System;
using System.Runtime.CompilerServices;

public class CardRewardData : RewardData
{
    public CardRewardData() : this(string.Empty, TAG_PREMIUM.NORMAL, 0)
    {
    }

    public CardRewardData(string cardID, TAG_PREMIUM premium, int count) : base(Reward.Type.CARD)
    {
        this.CardID = cardID;
        this.Count = count;
        this.Premium = premium;
        this.InnKeeperLine = InnKeeperTrigger.NONE;
    }

    protected override string GetGameObjectName()
    {
        return "CardReward";
    }

    public bool Merge(CardRewardData other)
    {
        if (!this.CardID.Equals(other.CardID))
        {
            return false;
        }
        if (!this.Premium.Equals(other.Premium))
        {
            return false;
        }
        this.Count += other.Count;
        foreach (long num in other.m_noticeIDs)
        {
            base.AddNoticeID(num);
        }
        return true;
    }

    public override string ToString()
    {
        EntityDef entityDef = DefLoader.Get().GetEntityDef(this.CardID);
        string str = (entityDef != null) ? entityDef.GetName() : "[UNKNOWN]";
        object[] args = new object[] { str, this.CardID, this.Premium, this.Count, base.Origin, base.OriginData };
        return string.Format("[CardRewardData: cardName={0} CardID={1}, Premium={2} Count={3} Origin={4} OriginData={5}]", args);
    }

    public string CardID { get; set; }

    public int Count { get; set; }

    public InnKeeperTrigger InnKeeperLine { get; set; }

    public TAG_PREMIUM Premium { get; set; }

    public enum InnKeeperTrigger
    {
        NONE,
        CORE_CLASS_SET_COMPLETE,
        SECOND_REWARD_EVER
    }
}

