using System;
using System.Runtime.CompilerServices;

public class MountRewardData : RewardData
{
    public MountRewardData() : this(MountType.UNKNOWN)
    {
    }

    public MountRewardData(MountType mount) : base(Reward.Type.MOUNT)
    {
        this.Mount = mount;
    }

    protected override string GetGameObjectName()
    {
        MountType mount = this.Mount;
        if (mount != MountType.WOW_HEARTHSTEED)
        {
            if (mount == MountType.HEROES_MAGIC_CARPET_CARD)
            {
                return "CardMountReward";
            }
            return string.Empty;
        }
        return "HearthSteedReward";
    }

    public override string ToString()
    {
        return string.Format("[MountRewardData Mount={0} Origin={1} OriginData={2}]", this.Mount, base.Origin, base.OriginData);
    }

    public MountType Mount { get; set; }

    public enum MountType
    {
        UNKNOWN,
        WOW_HEARTHSTEED,
        HEROES_MAGIC_CARPET_CARD
    }
}

