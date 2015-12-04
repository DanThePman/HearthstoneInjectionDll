using System;
using System.ComponentModel;

public enum DemoMode
{
    [Description("Apple Store")]
    APPLE_STORE = 6,
    [Description("BlizzCon 2013")]
    BLIZZCON_2013 = 3,
    [Description("BlizzCon 2014")]
    BLIZZCON_2014 = 4,
    [Description("BlizzCon 2015")]
    BLIZZCON_2015 = 5,
    [Description("gamescom 2013")]
    GAMESCOM_2013 = 2,
    [Description("None")]
    NONE = 0,
    [Description("PAX East 2013")]
    PAX_EAST_2013 = 1
}

