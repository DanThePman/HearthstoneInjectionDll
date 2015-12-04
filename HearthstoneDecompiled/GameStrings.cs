using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameStrings
{
    private static readonly char[] LANGUAGE_RULE_ARG_DELIMITERS = new char[] { ',' };
    private const string NUMBER_PATTERN = "(?:[0-9]+,)*[0-9]+";
    private static Map<TAG_CARD_SET, string> s_cardSetNames;
    private static Map<TAG_CARD_SET, string> s_cardSetNamesShortened;
    private static Map<TAG_CARDTYPE, string> s_cardTypeNames;
    private static Map<TAG_CLASS, string> s_classDescriptions;
    private static Map<TAG_CLASS, string> s_classNames;
    private static Map<GAME_TAG, string[]> s_keywordText;
    private static Map<TAG_RACE, string> s_raceNames;
    private static Map<TAG_RARITY, string> s_rarityNames;
    private static Map<GAME_TAG, string[]> s_refKeywordText;
    private static Map<GameStringCategory, GameStringTable> s_tables = new Map<GameStringCategory, GameStringTable>();
    public const string s_UnknownName = "UNKNOWN";

    static GameStrings()
    {
        Map<TAG_CLASS, string> map = new Map<TAG_CLASS, string>();
        map.Add(TAG_CLASS.DEATHKNIGHT, "GLOBAL_CLASS_DEATHKNIGHT");
        map.Add(TAG_CLASS.DRUID, "GLOBAL_CLASS_DRUID");
        map.Add(TAG_CLASS.HUNTER, "GLOBAL_CLASS_HUNTER");
        map.Add(TAG_CLASS.MAGE, "GLOBAL_CLASS_MAGE");
        map.Add(TAG_CLASS.PALADIN, "GLOBAL_CLASS_PALADIN");
        map.Add(TAG_CLASS.PRIEST, "GLOBAL_CLASS_PRIEST");
        map.Add(TAG_CLASS.ROGUE, "GLOBAL_CLASS_ROGUE");
        map.Add(TAG_CLASS.SHAMAN, "GLOBAL_CLASS_SHAMAN");
        map.Add(TAG_CLASS.WARLOCK, "GLOBAL_CLASS_WARLOCK");
        map.Add(TAG_CLASS.WARRIOR, "GLOBAL_CLASS_WARRIOR");
        map.Add(TAG_CLASS.INVALID, "GLOBAL_CLASS_NEUTRAL");
        s_classNames = map;
        map = new Map<TAG_CLASS, string>();
        map.Add(TAG_CLASS.DEATHKNIGHT, "GLOBAL_CLASS_DESCRIPTION_DEATHKNIGHT");
        map.Add(TAG_CLASS.DRUID, "GLOBAL_CLASS_DESCRIPTION_DRUID");
        map.Add(TAG_CLASS.HUNTER, "GLOBAL_CLASS_DESCRIPTION_HUNTER");
        map.Add(TAG_CLASS.MAGE, "GLOBAL_CLASS_DESCRIPTION_MAGE");
        map.Add(TAG_CLASS.PALADIN, "GLOBAL_CLASS_DESCRIPTION_PALADIN");
        map.Add(TAG_CLASS.PRIEST, "GLOBAL_CLASS_DESCRIPTION_PRIEST");
        map.Add(TAG_CLASS.ROGUE, "GLOBAL_CLASS_DESCRIPTION_ROGUE");
        map.Add(TAG_CLASS.SHAMAN, "GLOBAL_CLASS_DESCRIPTION_SHAMAN");
        map.Add(TAG_CLASS.WARLOCK, "GLOBAL_CLASS_DESCRIPTION_WARLOCK");
        map.Add(TAG_CLASS.WARRIOR, "GLOBAL_CLASS_DESCRIPTION_WARRIOR");
        s_classDescriptions = map;
        Map<TAG_RACE, string> map2 = new Map<TAG_RACE, string>();
        map2.Add(TAG_RACE.BLOODELF, "GLOBAL_RACE_BLOODELF");
        map2.Add(TAG_RACE.DRAENEI, "GLOBAL_RACE_DRAENEI");
        map2.Add(TAG_RACE.DWARF, "GLOBAL_RACE_DWARF");
        map2.Add(TAG_RACE.GNOME, "GLOBAL_RACE_GNOME");
        map2.Add(TAG_RACE.GOBLIN, "GLOBAL_RACE_GOBLIN");
        map2.Add(TAG_RACE.HUMAN, "GLOBAL_RACE_HUMAN");
        map2.Add(TAG_RACE.NIGHTELF, "GLOBAL_RACE_NIGHTELF");
        map2.Add(TAG_RACE.ORC, "GLOBAL_RACE_ORC");
        map2.Add(TAG_RACE.TAUREN, "GLOBAL_RACE_TAUREN");
        map2.Add(TAG_RACE.TROLL, "GLOBAL_RACE_TROLL");
        map2.Add(TAG_RACE.UNDEAD, "GLOBAL_RACE_UNDEAD");
        map2.Add(TAG_RACE.WORGEN, "GLOBAL_RACE_WORGEN");
        map2.Add(TAG_RACE.MURLOC, "GLOBAL_RACE_MURLOC");
        map2.Add(TAG_RACE.DEMON, "GLOBAL_RACE_DEMON");
        map2.Add(TAG_RACE.SCOURGE, "GLOBAL_RACE_SCOURGE");
        map2.Add(TAG_RACE.MECHANICAL, "GLOBAL_RACE_MECHANICAL");
        map2.Add(TAG_RACE.ELEMENTAL, "GLOBAL_RACE_ELEMENTAL");
        map2.Add(TAG_RACE.OGRE, "GLOBAL_RACE_OGRE");
        map2.Add(TAG_RACE.PET, "GLOBAL_RACE_PET");
        map2.Add(TAG_RACE.TOTEM, "GLOBAL_RACE_TOTEM");
        map2.Add(TAG_RACE.NERUBIAN, "GLOBAL_RACE_NERUBIAN");
        map2.Add(TAG_RACE.PIRATE, "GLOBAL_RACE_PIRATE");
        map2.Add(TAG_RACE.DRAGON, "GLOBAL_RACE_DRAGON");
        s_raceNames = map2;
        Map<TAG_RARITY, string> map3 = new Map<TAG_RARITY, string>();
        map3.Add(TAG_RARITY.COMMON, "GLOBAL_RARITY_COMMON");
        map3.Add(TAG_RARITY.EPIC, "GLOBAL_RARITY_EPIC");
        map3.Add(TAG_RARITY.LEGENDARY, "GLOBAL_RARITY_LEGENDARY");
        map3.Add(TAG_RARITY.RARE, "GLOBAL_RARITY_RARE");
        map3.Add(TAG_RARITY.FREE, "GLOBAL_RARITY_FREE");
        s_rarityNames = map3;
        Map<TAG_CARD_SET, string> map4 = new Map<TAG_CARD_SET, string>();
        map4.Add(TAG_CARD_SET.CORE, "GLOBAL_CARD_SET_CORE");
        map4.Add(TAG_CARD_SET.EXPERT1, "GLOBAL_CARD_SET_EXPERT1");
        map4.Add(TAG_CARD_SET.REWARD, "GLOBAL_CARD_SET_REWARD");
        map4.Add(TAG_CARD_SET.PROMO, "GLOBAL_CARD_SET_PROMO");
        map4.Add(TAG_CARD_SET.FP1, "GLOBAL_CARD_SET_NAXX");
        map4.Add(TAG_CARD_SET.PE1, "GLOBAL_CARD_SET_GVG");
        map4.Add(TAG_CARD_SET.BRM, "GLOBAL_CARD_SET_BRM");
        map4.Add(TAG_CARD_SET.TGT, "GLOBAL_CARD_SET_TGT");
        map4.Add(TAG_CARD_SET.LOE, "GLOBAL_CARD_SET_LOE");
        map4.Add(TAG_CARD_SET.SLUSH, "GLOBAL_CARD_SET_DEBUG");
        s_cardSetNames = map4;
        map4 = new Map<TAG_CARD_SET, string>();
        map4.Add(TAG_CARD_SET.CORE, "GLOBAL_CARD_SET_CORE");
        map4.Add(TAG_CARD_SET.EXPERT1, "GLOBAL_CARD_SET_EXPERT1");
        map4.Add(TAG_CARD_SET.REWARD, "GLOBAL_CARD_SET_REWARD");
        map4.Add(TAG_CARD_SET.PROMO, "GLOBAL_CARD_SET_PROMO");
        map4.Add(TAG_CARD_SET.FP1, "GLOBAL_CARD_SET_NAXX");
        map4.Add(TAG_CARD_SET.PE1, "GLOBAL_CARD_SET_GVG");
        map4.Add(TAG_CARD_SET.BRM, "GLOBAL_CARD_SET_BRM");
        map4.Add(TAG_CARD_SET.TGT, "GLOBAL_CARD_SET_TGT_SHORT");
        map4.Add(TAG_CARD_SET.LOE, "GLOBAL_CARD_SET_LOE_SHORT");
        map4.Add(TAG_CARD_SET.SLUSH, "GLOBAL_CARD_SET_DEBUG");
        s_cardSetNamesShortened = map4;
        Map<TAG_CARDTYPE, string> map5 = new Map<TAG_CARDTYPE, string>();
        map5.Add(TAG_CARDTYPE.HERO, "GLOBAL_CARDTYPE_HERO");
        map5.Add(TAG_CARDTYPE.MINION, "GLOBAL_CARDTYPE_MINION");
        map5.Add(TAG_CARDTYPE.SPELL, "GLOBAL_CARDTYPE_SPELL");
        map5.Add(TAG_CARDTYPE.ENCHANTMENT, "GLOBAL_CARDTYPE_ENCHANTMENT");
        map5.Add(TAG_CARDTYPE.WEAPON, "GLOBAL_CARDTYPE_WEAPON");
        map5.Add(TAG_CARDTYPE.ITEM, "GLOBAL_CARDTYPE_ITEM");
        map5.Add(TAG_CARDTYPE.TOKEN, "GLOBAL_CARDTYPE_TOKEN");
        map5.Add(TAG_CARDTYPE.HERO_POWER, "GLOBAL_CARDTYPE_HEROPOWER");
        s_cardTypeNames = map5;
        Map<GAME_TAG, string[]> map6 = new Map<GAME_TAG, string[]>();
        string[] textArray1 = new string[] { "GLOBAL_KEYWORD_TAUNT", "GLOBAL_KEYWORD_TAUNT_TEXT" };
        map6.Add(GAME_TAG.TAUNT, textArray1);
        string[] textArray2 = new string[] { "GLOBAL_KEYWORD_SPELLPOWER", "GLOBAL_KEYWORD_SPELLPOWER_TEXT" };
        map6.Add(GAME_TAG.SPELLPOWER, textArray2);
        string[] textArray3 = new string[] { "GLOBAL_KEYWORD_DIVINE_SHIELD", "GLOBAL_KEYWORD_DIVINE_SHIELD_TEXT" };
        map6.Add(GAME_TAG.DIVINE_SHIELD, textArray3);
        string[] textArray4 = new string[] { "GLOBAL_KEYWORD_CHARGE", "GLOBAL_KEYWORD_CHARGE_TEXT" };
        map6.Add(GAME_TAG.CHARGE, textArray4);
        string[] textArray5 = new string[] { "GLOBAL_KEYWORD_SECRET", "GLOBAL_KEYWORD_SECRET_TEXT" };
        map6.Add(GAME_TAG.SECRET, textArray5);
        string[] textArray6 = new string[] { "GLOBAL_KEYWORD_STEALTH", "GLOBAL_KEYWORD_STEALTH_TEXT" };
        map6.Add(GAME_TAG.STEALTH, textArray6);
        string[] textArray7 = new string[] { "GLOBAL_KEYWORD_ENRAGED", "GLOBAL_KEYWORD_ENRAGED_TEXT" };
        map6.Add(GAME_TAG.ENRAGED, textArray7);
        string[] textArray8 = new string[] { "GLOBAL_KEYWORD_BATTLECRY", "GLOBAL_KEYWORD_BATTLECRY_TEXT" };
        map6.Add(GAME_TAG.BATTLECRY, textArray8);
        string[] textArray9 = new string[] { "GLOBAL_KEYWORD_FROZEN", "GLOBAL_KEYWORD_FROZEN_TEXT" };
        map6.Add(GAME_TAG.FROZEN, textArray9);
        string[] textArray10 = new string[] { "GLOBAL_KEYWORD_FREEZE", "GLOBAL_KEYWORD_FREEZE_TEXT" };
        map6.Add(GAME_TAG.FREEZE, textArray10);
        string[] textArray11 = new string[] { "GLOBAL_KEYWORD_WINDFURY", "GLOBAL_KEYWORD_WINDFURY_TEXT" };
        map6.Add(GAME_TAG.WINDFURY, textArray11);
        string[] textArray12 = new string[] { "GLOBAL_KEYWORD_DEATHRATTLE", "GLOBAL_KEYWORD_DEATHRATTLE_TEXT" };
        map6.Add(GAME_TAG.DEATHRATTLE, textArray12);
        string[] textArray13 = new string[] { "GLOBAL_KEYWORD_COMBO", "GLOBAL_KEYWORD_COMBO_TEXT" };
        map6.Add(GAME_TAG.COMBO, textArray13);
        string[] textArray14 = new string[] { "GLOBAL_KEYWORD_OVERLOAD", "GLOBAL_KEYWORD_OVERLOAD_TEXT" };
        map6.Add(GAME_TAG.OVERLOAD, textArray14);
        string[] textArray15 = new string[] { "GLOBAL_KEYWORD_SILENCE", "GLOBAL_KEYWORD_SILENCE_TEXT" };
        map6.Add(GAME_TAG.SILENCE, textArray15);
        string[] textArray16 = new string[] { "GLOBAL_KEYWORD_COUNTER", "GLOBAL_KEYWORD_COUNTER_TEXT" };
        map6.Add(GAME_TAG.COUNTER, textArray16);
        string[] textArray17 = new string[] { "GLOBAL_KEYWORD_IMMUNE", "GLOBAL_KEYWORD_IMMUNE_TEXT" };
        map6.Add(GAME_TAG.CANT_BE_DAMAGED, textArray17);
        string[] textArray18 = new string[] { "GLOBAL_KEYWORD_AUTOCAST", "GLOBAL_KEYWORD_AUTOCAST_TEXT" };
        map6.Add(GAME_TAG.AI_MUST_PLAY, textArray18);
        string[] textArray19 = new string[] { "GLOBAL_KEYWORD_SPAREPART", "GLOBAL_KEYWORD_SPAREPART_TEXT" };
        map6.Add(GAME_TAG.SPARE_PART, textArray19);
        string[] textArray20 = new string[] { "GLOBAL_KEYWORD_INSPIRE", "GLOBAL_KEYWORD_INSPIRE_TEXT" };
        map6.Add(GAME_TAG.INSPIRE, textArray20);
        string[] textArray21 = new string[] { "GLOBAL_KEYWORD_TREASURE", "GLOBAL_KEYWORD_TREASURE_TEXT" };
        map6.Add(GAME_TAG.TREASURE, textArray21);
        s_keywordText = map6;
        map6 = new Map<GAME_TAG, string[]>();
        string[] textArray22 = new string[] { "GLOBAL_KEYWORD_TAUNT", "GLOBAL_KEYWORD_TAUNT_REF_TEXT" };
        map6.Add(GAME_TAG.TAUNT, textArray22);
        string[] textArray23 = new string[] { "GLOBAL_KEYWORD_SPELLPOWER", "GLOBAL_KEYWORD_SPELLPOWER_REF_TEXT" };
        map6.Add(GAME_TAG.SPELLPOWER, textArray23);
        string[] textArray24 = new string[] { "GLOBAL_KEYWORD_DIVINE_SHIELD", "GLOBAL_KEYWORD_DIVINE_SHIELD_REF_TEXT" };
        map6.Add(GAME_TAG.DIVINE_SHIELD, textArray24);
        string[] textArray25 = new string[] { "GLOBAL_KEYWORD_CHARGE", "GLOBAL_KEYWORD_CHARGE_TEXT" };
        map6.Add(GAME_TAG.CHARGE, textArray25);
        string[] textArray26 = new string[] { "GLOBAL_KEYWORD_SECRET", "GLOBAL_KEYWORD_SECRET_TEXT" };
        map6.Add(GAME_TAG.SECRET, textArray26);
        string[] textArray27 = new string[] { "GLOBAL_KEYWORD_STEALTH", "GLOBAL_KEYWORD_STEALTH_REF_TEXT" };
        map6.Add(GAME_TAG.STEALTH, textArray27);
        string[] textArray28 = new string[] { "GLOBAL_KEYWORD_ENRAGED", "GLOBAL_KEYWORD_ENRAGED_TEXT" };
        map6.Add(GAME_TAG.ENRAGED, textArray28);
        string[] textArray29 = new string[] { "GLOBAL_KEYWORD_BATTLECRY", "GLOBAL_KEYWORD_BATTLECRY_TEXT" };
        map6.Add(GAME_TAG.BATTLECRY, textArray29);
        string[] textArray30 = new string[] { "GLOBAL_KEYWORD_FROZEN", "GLOBAL_KEYWORD_FROZEN_TEXT" };
        map6.Add(GAME_TAG.FROZEN, textArray30);
        string[] textArray31 = new string[] { "GLOBAL_KEYWORD_FREEZE", "GLOBAL_KEYWORD_FREEZE_TEXT" };
        map6.Add(GAME_TAG.FREEZE, textArray31);
        string[] textArray32 = new string[] { "GLOBAL_KEYWORD_WINDFURY", "GLOBAL_KEYWORD_WINDFURY_TEXT" };
        map6.Add(GAME_TAG.WINDFURY, textArray32);
        string[] textArray33 = new string[] { "GLOBAL_KEYWORD_DEATHRATTLE", "GLOBAL_KEYWORD_DEATHRATTLE_TEXT" };
        map6.Add(GAME_TAG.DEATHRATTLE, textArray33);
        string[] textArray34 = new string[] { "GLOBAL_KEYWORD_COMBO", "GLOBAL_KEYWORD_COMBO_TEXT" };
        map6.Add(GAME_TAG.COMBO, textArray34);
        string[] textArray35 = new string[] { "GLOBAL_KEYWORD_OVERLOAD", "GLOBAL_KEYWORD_OVERLOAD_TEXT" };
        map6.Add(GAME_TAG.OVERLOAD, textArray35);
        string[] textArray36 = new string[] { "GLOBAL_KEYWORD_SILENCE", "GLOBAL_KEYWORD_SILENCE_TEXT" };
        map6.Add(GAME_TAG.SILENCE, textArray36);
        string[] textArray37 = new string[] { "GLOBAL_KEYWORD_COUNTER", "GLOBAL_KEYWORD_COUNTER_TEXT" };
        map6.Add(GAME_TAG.COUNTER, textArray37);
        string[] textArray38 = new string[] { "GLOBAL_KEYWORD_IMMUNE", "GLOBAL_KEYWORD_IMMUNE_REF_TEXT" };
        map6.Add(GAME_TAG.CANT_BE_DAMAGED, textArray38);
        string[] textArray39 = new string[] { "GLOBAL_KEYWORD_AUTOCAST", "GLOBAL_KEYWORD_AUTOCAST_TEXT" };
        map6.Add(GAME_TAG.AI_MUST_PLAY, textArray39);
        string[] textArray40 = new string[] { "GLOBAL_KEYWORD_SPAREPART", "GLOBAL_KEYWORD_SPAREPART_TEXT" };
        map6.Add(GAME_TAG.SPARE_PART, textArray40);
        string[] textArray41 = new string[] { "GLOBAL_KEYWORD_INSPIRE", "GLOBAL_KEYWORD_INSPIRE_TEXT" };
        map6.Add(GAME_TAG.INSPIRE, textArray41);
        string[] textArray42 = new string[] { "GLOBAL_KEYWORD_TREASURE", "GLOBAL_KEYWORD_TREASURE_TEXT" };
        map6.Add(GAME_TAG.TREASURE, textArray42);
        s_refKeywordText = map6;
    }

    private static void CheckConflicts(GameStringTable table)
    {
        Map<string, string>.KeyCollection keys = table.GetAll().Keys;
        GameStringCategory category = table.GetCategory();
        foreach (GameStringTable table2 in s_tables.Values)
        {
            foreach (string str in keys)
            {
                if (table2.Get(str) != null)
                {
                    string message = string.Format("GameStrings.CheckConflicts() - Tag {0} is used in {1} and {2}. All tags must be unique.", str, category, table2.GetCategory());
                    Error.AddDevWarning("GameStrings Error", message, new object[0]);
                }
            }
        }
    }

    private static string Find(string key)
    {
        foreach (GameStringTable table in s_tables.Values)
        {
            string str = table.Get(key);
            if (str != null)
            {
                return str;
            }
        }
        return null;
    }

    public static string Format(string key, params object[] args)
    {
        string format = Find(key);
        if (format == null)
        {
            return key;
        }
        return ParseLanguageRules(string.Format(Localization.GetCultureInfo(), format, args));
    }

    public static string FormatPlurals(string key, PluralNumber[] pluralNumbers, params object[] args)
    {
        string format = Find(key);
        if (format == null)
        {
            return key;
        }
        return ParseLanguageRules(string.Format(Localization.GetCultureInfo(), format, args), pluralNumbers);
    }

    public static string Get(string key)
    {
        string str = Find(key);
        if (str == null)
        {
            return key;
        }
        return ParseLanguageRules(str);
    }

    public static string GetAssetPath(Locale locale, string fileName)
    {
        return FileUtils.GetAssetPath(string.Format("Strings/{0}/{1}", locale, fileName));
    }

    public static string GetCardSetName(TAG_CARD_SET tag)
    {
        string str = null;
        return (!s_cardSetNames.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetCardSetNameKey(TAG_CARD_SET tag)
    {
        string str = null;
        return (!s_cardSetNames.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetCardSetNameKeyShortened(TAG_CARD_SET tag)
    {
        string str = null;
        return (!s_cardSetNamesShortened.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetCardSetNameShortened(TAG_CARD_SET tag)
    {
        string str = null;
        return (!s_cardSetNamesShortened.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetCardTypeName(TAG_CARDTYPE tag)
    {
        string str = null;
        return (!s_cardTypeNames.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetCardTypeNameKey(TAG_CARDTYPE tag)
    {
        string str = null;
        return (!s_cardTypeNames.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetClassDescription(TAG_CLASS tag)
    {
        string str = null;
        return (!s_classDescriptions.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetClassDescriptionKey(TAG_CLASS tag)
    {
        string str = null;
        return (!s_classDescriptions.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetClassName(TAG_CLASS tag)
    {
        string str = null;
        return (!s_classNames.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetClassNameKey(TAG_CLASS tag)
    {
        string str = null;
        return (!s_classNames.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetKeywordName(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_keywordText.TryGetValue(tag, out strArray) ? "UNKNOWN" : Get(strArray[0]));
    }

    public static string GetKeywordNameKey(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_keywordText.TryGetValue(tag, out strArray) ? null : strArray[0]);
    }

    public static string GetKeywordText(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_keywordText.TryGetValue(tag, out strArray) ? "UNKNOWN" : Get(strArray[1]));
    }

    public static string GetKeywordTextKey(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_keywordText.TryGetValue(tag, out strArray) ? null : strArray[1]);
    }

    private static int GetPluralIndex(int number)
    {
        switch (Localization.GetLocale())
        {
            case Locale.frFR:
            case Locale.koKR:
            case Locale.zhTW:
            case Locale.zhCN:
                if (number > 1)
                {
                    return 1;
                }
                return 0;

            case Locale.ruRU:
                switch ((number % 100))
                {
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                        return 2;
                }
                switch ((number % 10))
                {
                    case 1:
                        return 0;

                    case 2:
                    case 3:
                    case 4:
                        return 1;
                }
                return 2;

            case Locale.plPL:
                if (number != 1)
                {
                    if (number != 0)
                    {
                        switch ((number % 100))
                        {
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                                return 2;
                        }
                        switch ((number % 10))
                        {
                            case 2:
                            case 3:
                            case 4:
                                return 1;
                        }
                    }
                    return 2;
                }
                return 0;
        }
        if (number == 1)
        {
            return 0;
        }
        return 1;
    }

    public static string GetRaceName(TAG_RACE tag)
    {
        string str = null;
        return (!s_raceNames.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetRaceNameKey(TAG_RACE tag)
    {
        string str = null;
        return (!s_raceNames.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetRandomTip(TipCategory tipCategory)
    {
        int num = 0;
        List<string> list = new List<string>();
        while (true)
        {
            string key = string.Format("GLUE_TIP_{0}_{1}", tipCategory, num);
            string item = Get(key);
            if (item.Equals(key))
            {
                break;
            }
            if (UniversalInputManager.Get().IsTouchMode())
            {
                string str3 = key + "_TOUCH";
                string str4 = Get(str3);
                if (!str4.Equals(str3))
                {
                    item = str4;
                }
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    string str5 = key + "_PHONE";
                    string str6 = Get(str5);
                    if (!str6.Equals(str5))
                    {
                        item = str6;
                    }
                }
            }
            list.Add(item);
            num++;
        }
        if (list.Count == 0)
        {
            Debug.LogError(string.Format("GameStrings.GetRandomTip() - no tips in category {0}", tipCategory));
            return "UNKNOWN";
        }
        int num2 = UnityEngine.Random.Range(0, list.Count);
        return list[num2];
    }

    public static string GetRarityText(TAG_RARITY tag)
    {
        string str = null;
        return (!s_rarityNames.TryGetValue(tag, out str) ? "UNKNOWN" : Get(str));
    }

    public static string GetRarityTextKey(TAG_RARITY tag)
    {
        string str = null;
        return (!s_rarityNames.TryGetValue(tag, out str) ? null : str);
    }

    public static string GetRefKeywordText(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_refKeywordText.TryGetValue(tag, out strArray) ? "UNKNOWN" : Get(strArray[1]));
    }

    public static string GetRefKeywordTextKey(GAME_TAG tag)
    {
        string[] strArray = null;
        return (!s_refKeywordText.TryGetValue(tag, out strArray) ? null : strArray[1]);
    }

    public static string GetTip(TipCategory tipCategory, int progress, TipCategory randomTipCategory = 4)
    {
        int num = 0;
        List<string> list = new List<string>();
        while (true)
        {
            string key = string.Format("GLUE_TIP_{0}_{1}", tipCategory, num);
            string item = Get(key);
            if (item.Equals(key))
            {
                break;
            }
            if (UniversalInputManager.Get().IsTouchMode())
            {
                string str3 = key + "_TOUCH";
                string str4 = Get(str3);
                if (!str4.Equals(str3))
                {
                    item = str4;
                }
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    string str5 = key + "_PHONE";
                    string str6 = Get(str5);
                    if (!str6.Equals(str5))
                    {
                        item = str6;
                    }
                }
            }
            list.Add(item);
            num++;
        }
        if (progress < list.Count)
        {
            return list[progress];
        }
        return GetRandomTip(randomTipCategory);
    }

    public static bool HasCardSetName(TAG_CARD_SET tag)
    {
        return s_cardSetNames.ContainsKey(tag);
    }

    public static bool HasCardTypeName(TAG_CARDTYPE tag)
    {
        return s_cardTypeNames.ContainsKey(tag);
    }

    public static bool HasClassDescription(TAG_CLASS tag)
    {
        return s_classDescriptions.ContainsKey(tag);
    }

    public static bool HasClassName(TAG_CLASS tag)
    {
        return s_classNames.ContainsKey(tag);
    }

    public static bool HasKey(string key)
    {
        return (Find(key) != null);
    }

    public static bool HasKeywordName(GAME_TAG tag)
    {
        return s_keywordText.ContainsKey(tag);
    }

    public static bool HasKeywordText(GAME_TAG tag)
    {
        return s_keywordText.ContainsKey(tag);
    }

    public static bool HasRaceName(TAG_RACE tag)
    {
        return s_raceNames.ContainsKey(tag);
    }

    public static bool HasRarityText(TAG_RARITY tag)
    {
        return s_rarityNames.ContainsKey(tag);
    }

    public static bool HasRefKeywordText(GAME_TAG tag)
    {
        return s_refKeywordText.ContainsKey(tag);
    }

    public static bool LoadCategory(GameStringCategory cat)
    {
        if (s_tables.ContainsKey(cat))
        {
            Debug.LogWarning(string.Format("GameStrings.LoadCategory() - {0} is already loaded", cat));
            return false;
        }
        GameStringTable table = new GameStringTable();
        if (!table.Load(cat))
        {
            Debug.LogError(string.Format("GameStrings.LoadCategory() - {0} failed to load", cat));
            return false;
        }
        if (ApplicationMgr.IsInternal())
        {
            CheckConflicts(table);
        }
        s_tables.Add(cat, table);
        return true;
    }

    private static string ParseLanguageRule4(string str, PluralNumber[] pluralNumbers = null)
    {
        StringBuilder builder = null;
        int? nullable = null;
        int startIndex = 0;
        int num2 = 0;
        for (int i = str.IndexOf("|4"); i >= 0; i = str.IndexOf("|4", (int) (i + 2)))
        {
            int num4;
            int num5;
            num2++;
            string[] args = ParseLanguageRuleArgs(str, i, out num4, out num5);
            if (args != null)
            {
                int num6 = startIndex;
                int length = i - startIndex;
                string betweenRulesStr = str.Substring(num6, length);
                PluralNumber number = null;
                if (pluralNumbers != null)
                {
                    <ParseLanguageRule4>c__AnonStorey379 storey = new <ParseLanguageRule4>c__AnonStorey379 {
                        pluralArgIndex = num2 - 1
                    };
                    number = Array.Find<PluralNumber>(pluralNumbers, new Predicate<PluralNumber>(storey.<>m__262));
                }
                if (number != null)
                {
                    nullable = new int?(number.m_number);
                }
                else
                {
                    int num8;
                    if (ParseLanguageRule4Number(args, betweenRulesStr, out num8))
                    {
                        nullable = new int?(num8);
                    }
                    else if (!nullable.HasValue)
                    {
                        object[] objArray1 = new object[] { betweenRulesStr, num6, length, num2, str };
                        Debug.LogWarning(string.Format("GameStrings.ParseLanguageRule4() - failed to parse a number in substring \"{0}\" (indexes {1}-{2}) for rule {3} in string \"{4}\"", objArray1));
                        continue;
                    }
                }
                int pluralIndex = GetPluralIndex(nullable.Value);
                if (pluralIndex >= args.Length)
                {
                    Debug.LogWarning(string.Format("GameStrings.ParseLanguageRule4() - not enough arguments for rule {0} in string \"{1}\"", num2, str));
                }
                else
                {
                    string str3 = args[pluralIndex];
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }
                    builder.Append(betweenRulesStr);
                    builder.Append(str3);
                    startIndex = num5 + 1;
                }
                if ((number != null) && number.m_useForOnlyThisIndex)
                {
                    nullable = null;
                }
            }
        }
        if (builder == null)
        {
            return str;
        }
        builder.Append(str, startIndex, str.Length - startIndex);
        return builder.ToString();
    }

    private static bool ParseLanguageRule4Number(string[] args, string betweenRulesStr, out int number)
    {
        if (ParseLanguageRule4Number_Foreward(args[0], out number))
        {
            return true;
        }
        if (ParseLanguageRule4Number_Backward(betweenRulesStr, out number))
        {
            return true;
        }
        number = 0;
        return false;
    }

    private static bool ParseLanguageRule4Number_Backward(string str, out int number)
    {
        number = 0;
        MatchCollection matchs = Regex.Matches(str, "(?:[0-9]+,)*[0-9]+");
        if (matchs.Count == 0)
        {
            return false;
        }
        Match match = matchs[matchs.Count - 1];
        if (!GeneralUtils.TryParseInt(match.Value, out number))
        {
            return false;
        }
        return true;
    }

    private static bool ParseLanguageRule4Number_Foreward(string str, out int number)
    {
        number = 0;
        Match match = Regex.Match(str, "(?:[0-9]+,)*[0-9]+");
        if (!match.Success)
        {
            return false;
        }
        if (!GeneralUtils.TryParseInt(match.Value, out number))
        {
            return false;
        }
        return true;
    }

    private static string[] ParseLanguageRuleArgs(string str, int ruleIndex, out int argStartIndex, out int argEndIndex)
    {
        argStartIndex = -1;
        argEndIndex = -1;
        argStartIndex = str.IndexOf('(', ruleIndex + 2);
        if (argStartIndex < 0)
        {
            Debug.LogWarning(string.Format("GameStrings.ParseLanguageRuleArgs() - failed to parse '(' for rule at index {0} in string {1}", ruleIndex, str));
            return null;
        }
        argEndIndex = str.IndexOf(')', argStartIndex + 1);
        if (argEndIndex < 0)
        {
            Debug.LogWarning(string.Format("GameStrings.ParseLanguageRuleArgs() - failed to parse ')' for rule at index {0} in string {1}", ruleIndex, str));
            return null;
        }
        StringBuilder builder = new StringBuilder();
        builder.Append(str, argStartIndex + 1, (argEndIndex - argStartIndex) - 1);
        string input = builder.ToString();
        MatchCollection matchs = Regex.Matches(input, "(?:[0-9]+,)*[0-9]+");
        if (matchs.Count > 0)
        {
            builder.Remove(0, builder.Length);
            int startIndex = 0;
            IEnumerator enumerator = matchs.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Match current = (Match) enumerator.Current;
                    builder.Append(input, startIndex, current.Index - startIndex);
                    builder.Append('0', current.Length);
                    startIndex = current.Index + current.Length;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
            builder.Append(input, startIndex, input.Length - startIndex);
            input = builder.ToString();
        }
        string[] strArray = input.Split(LANGUAGE_RULE_ARG_DELIMITERS);
        int num2 = 0;
        for (int i = 0; i < strArray.Length; i++)
        {
            string str3 = strArray[i];
            if (matchs.Count > 0)
            {
                builder.Remove(0, builder.Length);
                int num4 = 0;
                IEnumerator enumerator2 = matchs.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        Match match2 = (Match) enumerator2.Current;
                        if ((match2.Index >= num2) && (match2.Index < (num2 + str3.Length)))
                        {
                            int num5 = match2.Index - num2;
                            builder.Append(str3, num4, num5 - num4);
                            builder.Append(match2.Value);
                            num4 = num5 + match2.Length;
                        }
                    }
                }
                finally
                {
                    IDisposable disposable2 = enumerator2 as IDisposable;
                    if (disposable2 == null)
                    {
                    }
                    disposable2.Dispose();
                }
                builder.Append(str3, num4, str3.Length - num4);
                str3 = builder.ToString();
                num2 += str3.Length + 1;
            }
            strArray[i] = str3.Trim();
        }
        return strArray;
    }

    public static string ParseLanguageRules(string str)
    {
        str = ParseLanguageRule4(str, null);
        return str;
    }

    public static string ParseLanguageRules(string str, PluralNumber[] pluralNumbers)
    {
        str = ParseLanguageRule4(str, pluralNumbers);
        return str;
    }

    public static bool UnloadCategory(GameStringCategory cat)
    {
        if (!s_tables.Remove(cat))
        {
            Debug.LogWarning(string.Format("GameStrings.UnloadCategory() - {0} was never loaded", cat));
            return false;
        }
        return true;
    }

    public static void WillReset()
    {
        IEnumerator enumerator = Enum.GetValues(typeof(GameStringCategory)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                GameStringCategory current = (GameStringCategory) ((int) enumerator.Current);
                if (s_tables.ContainsKey(current))
                {
                    UnloadCategory(current);
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
        LoadCategory(GameStringCategory.GLOBAL);
    }

    [CompilerGenerated]
    private sealed class <ParseLanguageRule4>c__AnonStorey379
    {
        internal int pluralArgIndex;

        internal bool <>m__262(GameStrings.PluralNumber currPluralNumber)
        {
            return (currPluralNumber.m_index == this.pluralArgIndex);
        }
    }

    public class PluralNumber
    {
        public int m_index;
        public int m_number;
        public bool m_useForOnlyThisIndex;
    }
}

