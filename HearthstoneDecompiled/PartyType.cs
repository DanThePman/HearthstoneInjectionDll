using System;
using System.ComponentModel;

public enum PartyType
{
    [Description("default")]
    DEFAULT = 0,
    [Description("FriendlyGame")]
    FRIENDLY_CHALLENGE = 1,
    [Description("SpectatorParty")]
    SPECTATOR_PARTY = 2
}

