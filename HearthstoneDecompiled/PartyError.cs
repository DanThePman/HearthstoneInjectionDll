using bnet;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PartyError
{
    public bool IsOperationCallback;
    public string DebugContext;
    public bnet.Error ErrorCode;
    public BnetFeature Feature;
    public BnetFeatureEvent FeatureEvent;
    public PartyId PartyId;
    public string szPartyType;
    public string StringData;
    public PartyType PartyType
    {
        get
        {
            return BnetParty.GetPartyTypeFromString(this.szPartyType);
        }
    }
}

