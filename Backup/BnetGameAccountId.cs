using bnet.protocol;
using PegasusShared;

public class BnetGameAccountId : BnetEntityId
{
    public BnetGameAccountId Clone()
    {
        return (BnetGameAccountId) base.Clone();
    }

    public static BnetGameAccountId CreateFromDll(BattleNet.DllEntityId src)
    {
        BnetGameAccountId id = new BnetGameAccountId();
        id.CopyFrom(src);
        return id;
    }

    public static BnetGameAccountId CreateFromNet(BnetId src)
    {
        BnetGameAccountId id = new BnetGameAccountId();
        id.CopyFrom(src);
        return id;
    }

    public static BnetGameAccountId CreateFromProtocol(EntityId src)
    {
        BnetGameAccountId id = new BnetGameAccountId();
        id.SetLo(src.Low);
        id.SetHi(src.High);
        return id;
    }
}

