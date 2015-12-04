using System;
using System.ComponentModel;

public enum FixedRewardType
{
    [Description("card")]
    CARD = 0,
    [Description("cardback")]
    CARD_BACK = 1,
    [Description("craftable_card")]
    CRAFTABLE_CARD = 2,
    [Description("meta_action_flags")]
    META_ACTION_FLAGS = 3
}

