using System;
using System.ComponentModel;

public enum ApplicationMode
{
    [Description("Internal")]
    INTERNAL = 1,
    INVALID = 0,
    [Description("Public")]
    PUBLIC = 2
}

