using bnet.protocol;
using PegasusShared;
using System;
using System.Runtime.CompilerServices;

public class PartyId
{
    public static readonly PartyId Empty = new PartyId(0L, 0L);

    public PartyId()
    {
        ulong num = 0L;
        this.Lo = num;
        this.Hi = num;
    }

    public PartyId(BattleNet.DllEntityId dllPartyId)
    {
        this.Set(dllPartyId.hi, dllPartyId.lo);
    }

    public PartyId(ulong highBits, ulong lowBits)
    {
        this.Set(highBits, lowBits);
    }

    public override bool Equals(object obj)
    {
        if (obj is PartyId)
        {
            return (this == ((PartyId) obj));
        }
        return base.Equals(obj);
    }

    public static PartyId FromBnetEntityId(BnetEntityId entityId)
    {
        return new PartyId(entityId.GetHi(), entityId.GetLo());
    }

    public static PartyId FromEntityId(BattleNet.DllEntityId entityId)
    {
        return new PartyId(entityId);
    }

    public static PartyId FromProtocol(EntityId protoEntityId)
    {
        return new PartyId(protoEntityId.High, protoEntityId.Low);
    }

    public static PartyId FromProtocol(BnetId protoEntityId)
    {
        return new PartyId(protoEntityId.Hi, protoEntityId.Lo);
    }

    public override int GetHashCode()
    {
        return (this.Hi.GetHashCode() ^ this.Lo.GetHashCode());
    }

    public static bool operator ==(PartyId a, PartyId b)
    {
        if (a == null)
        {
            return (b == null);
        }
        if (b == null)
        {
            return false;
        }
        return ((a.Hi == b.Hi) && (a.Lo == b.Lo));
    }

    public static implicit operator PartyId(BnetEntityId entityId)
    {
        if (entityId == null)
        {
            return null;
        }
        return new PartyId(entityId.GetHi(), entityId.GetLo());
    }

    public static bool operator !=(PartyId a, PartyId b)
    {
        return !(a == b);
    }

    public void Set(ulong highBits, ulong lowBits)
    {
        this.Hi = highBits;
        this.Lo = lowBits;
    }

    public BattleNet.DllEntityId ToDllEntityId()
    {
        return new BattleNet.DllEntityId { hi = this.Hi, lo = this.Lo };
    }

    public BnetId ToPegasusShared()
    {
        return new BnetId { Hi = this.Hi, Lo = this.Lo };
    }

    public override string ToString()
    {
        return string.Format("{0}-{1}", this.Hi, this.Lo);
    }

    public ulong Hi { get; private set; }

    public bool IsEmpty
    {
        get
        {
            return ((this.Hi == 0) && (this.Lo == 0L));
        }
    }

    public ulong Lo { get; private set; }
}

