using System;
using System.ComponentModel;

public enum DbfDataType
{
    [Description("AssetPath")]
    ASSET_PATH = 8,
    [Description("Bool")]
    BOOL = 5,
    [Description("Date")]
    DATE = 9,
    [Description("Float")]
    FLOAT = 4,
    [Description("Int")]
    INT = 1,
    INVALID = 0,
    [Description("LocString")]
    LOC_STRING = 7,
    [Description("Long")]
    LONG = 2,
    [Description("String")]
    STRING = 6,
    [Description("ULong")]
    ULONG = 3
}

