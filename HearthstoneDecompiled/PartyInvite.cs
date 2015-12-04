using System;

public class PartyInvite
{
    public BnetGameAccountId InviteeId;
    private Flags InviteFlags;
    public ulong InviteId;
    public BnetGameAccountId InviterId;
    public string InviterName;
    public PartyId PartyId;
    public PartyType PartyType;

    public PartyInvite()
    {
    }

    public PartyInvite(ulong inviteId, PartyId partyId, PartyType type)
    {
        this.InviteId = inviteId;
        this.PartyId = partyId;
        this.PartyType = type;
    }

    public uint GetFlags()
    {
        return (uint) this.InviteFlags;
    }

    public void SetFlags(uint flagsValue)
    {
        this.InviteFlags = (Flags) flagsValue;
    }

    public bool IsRejoin
    {
        get
        {
            return ((this.InviteFlags & Flags.REJOIN) == Flags.REJOIN);
        }
    }

    public bool IsReservation
    {
        get
        {
            return ((this.InviteFlags & Flags.REJOIN) == Flags.REJOIN);
        }
    }

    [Flags]
    public enum Flags
    {
        REJOIN = 1,
        RESERVATION = 1
    }
}

