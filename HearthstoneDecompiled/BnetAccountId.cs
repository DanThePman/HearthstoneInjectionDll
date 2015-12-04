using PegasusShared;

public class BnetAccountId : BnetEntityId
{
    public BnetAccountId Clone()
    {
        return (BnetAccountId) base.Clone();
    }

    public static BnetAccountId CreateFromBnetEntityId(BnetEntityId src)
    {
        BnetAccountId id = new BnetAccountId();
        id.CopyFrom(src);
        return id;
    }

    public static BnetAccountId CreateFromDll(BattleNet.DllEntityId src)
    {
        BnetAccountId id = new BnetAccountId();
        id.CopyFrom(src);
        return id;
    }

    public static BnetAccountId CreateFromNet(BnetId src)
    {
        BnetAccountId id = new BnetAccountId();
        id.CopyFrom(src);
        return id;
    }
}

