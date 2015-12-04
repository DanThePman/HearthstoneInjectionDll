using System;

public class PartyInfo
{
    public PartyId Id;
    public PartyType Type;

    public PartyInfo()
    {
    }

    public PartyInfo(PartyId partyId, PartyType type)
    {
        this.Id = partyId;
        this.Type = type;
    }

    public override string ToString()
    {
        return string.Format("{0}:{1}", this.Type.ToString(), this.Id.ToString());
    }
}

