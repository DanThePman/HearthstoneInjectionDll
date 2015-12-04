using System;
using System.Runtime.CompilerServices;

public class ClassChallengeUnlockData : RewardData
{
    public ClassChallengeUnlockData() : this(0)
    {
    }

    public ClassChallengeUnlockData(int wingID) : base(Reward.Type.CLASS_CHALLENGE)
    {
        this.WingID = wingID;
    }

    protected override string GetGameObjectName()
    {
        return "ClassChallengeUnlocked";
    }

    public override string ToString()
    {
        return string.Format("[ClassChallengeUnlockData: WingID={0} Origin={1} OriginData={2}]", this.WingID, base.Origin, base.OriginData);
    }

    public int WingID { get; set; }
}

