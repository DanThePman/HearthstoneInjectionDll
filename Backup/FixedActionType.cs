using System;
using System.ComponentModel;

public enum FixedActionType
{
    [Description("account_license_flags")]
    ACCOUNT_LICENSE_FLAGS = 6,
    [Description("achieve")]
    ACHIEVE = 5,
    [Description("hero_level")]
    HERO_LEVEL = 3,
    [Description("meta_action")]
    META_ACTION = 4,
    [Description("tutorial_progress")]
    TUTORIAL_PROGRESS = 0,
    [Description("wing_flags")]
    WING_FLAGS = 2,
    [Description("wing_progress")]
    WING_PROGRESS = 1
}

