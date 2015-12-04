using System;
using System.ComponentModel;

public enum LogLevel
{
    [Description("Debug")]
    Debug = 1,
    [Description("Error")]
    Error = 4,
    [Description("Info")]
    Info = 2,
    [Description("None")]
    None = 0,
    [Description("Warning")]
    Warning = 3
}

