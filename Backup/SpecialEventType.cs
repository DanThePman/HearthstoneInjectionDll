using System;
using System.ComponentModel;

public enum SpecialEventType
{
    [Description("brm_1")]
    BRM_1_OPENS = 12,
    [Description("brm_2")]
    BRM_2_OPENS = 13,
    [Description("brm_3")]
    BRM_3_OPENS = 14,
    [Description("brm_4")]
    BRM_4_OPENS = 15,
    [Description("brm_5")]
    BRM_5_OPENS = 0x10,
    [Description("brm_normal_sale")]
    BRM_NORMAL_SALE = 0x12,
    [Description("brm_pre_sale")]
    BRM_PRE_SALE = 0x11,
    [Description("feast_of_winter_veil")]
    FEAST_OF_WINTER_VEIL = 0x55,
    [Description("gvg_arena")]
    GVG_ARENA_PLAY = 9,
    [Description("gvg_begin")]
    GVG_LAUNCH_PERIOD = 8,
    [Description("gvg_promote")]
    GVG_PROMOTION = 7,
    [Description("none")]
    IGNORE = 0,
    [Description("launch")]
    LAUNCH_DAY = 1,
    [Description("lunar_new_year")]
    LUNAR_NEW_YEAR = 11,
    [Description("naxx_1")]
    NAXX_1_OPENS = 2,
    [Description("naxx_2")]
    NAXX_2_OPENS = 3,
    [Description("naxx_3")]
    NAXX_3_OPENS = 4,
    [Description("naxx_4")]
    NAXX_4_OPENS = 5,
    [Description("naxx_5")]
    NAXX_5_OPENS = 6,
    [Description("loe_1")]
    SPECIAL_EVENT_LOE_WING_1_OPEN = 0x38,
    [Description("loe_2")]
    SPECIAL_EVENT_LOE_WING_2_OPEN = 0x39,
    [Description("loe_3")]
    SPECIAL_EVENT_LOE_WING_3_OPEN = 0x3a,
    [Description("loe_4")]
    SPECIAL_EVENT_LOE_WING_4_OPEN = 0x3b,
    [Description("tb_pre_event")]
    SPECIAL_EVENT_PRE_TAVERN_BRAWL = 0x13,
    [Description("samsung_galaxy_gifts")]
    SPECIAL_EVENT_SAMSUNG_GALAXY_GIFTS = 0x1f,
    [Description("tgt_arena_draftable")]
    SPECIAL_EVENT_TGT_ARENA_DRAFTABLE = 0x20,
    [Description("tgt_normal_sale")]
    SPECIAL_EVENT_TGT_NORMAL_SALE = 30,
    [Description("tgt_pre_sale")]
    SPECIAL_EVENT_TGT_PRE_SALE = 0x1d
}

