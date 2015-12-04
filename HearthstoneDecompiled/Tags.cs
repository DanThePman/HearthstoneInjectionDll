using System;
using System.Linq;
using System.Runtime.CompilerServices;

public class Tags
{
    [CompilerGenerated]
    private static Func<GAME_TAG, int> <>f__am$cache0;
    public const int MAX = 0x200;

    public static void DebugDump(EntityBase entity, params GAME_TAG[] specificTagsToDump)
    {
        Log.Henry.Print(LogLevel.Debug, string.Format("Tags.DebugDump: entity={0}", entity), new object[0]);
        Map<int, int> map = entity.GetTags().GetMap();
        if ((specificTagsToDump != null) && (specificTagsToDump.Length > 0))
        {
        }
        int[] numArray = (<>f__am$cache0 != null) ? map.Keys.ToArray<int>() : Enumerable.Select<GAME_TAG, int>(specificTagsToDump, <>f__am$cache0).ToArray<int>();
        foreach (int num in numArray)
        {
            string str = string.Empty;
            if (map.ContainsKey(num))
            {
                int val = map[num];
                str = DebugTag(num, val);
            }
            else
            {
                str = string.Format("tag={0} value=(NULL)", ((GAME_TAG) num).ToString());
            }
            Log.Henry.Print(LogLevel.Debug, string.Format("Tags.DebugDump:           {0}", str), new object[0]);
        }
    }

    public static string DebugTag(int tag, int val)
    {
        string str = tag.ToString();
        try
        {
            str = ((GAME_TAG) tag).ToString();
        }
        catch (Exception)
        {
        }
        string str2 = val.ToString();
        GAME_TAG game_tag = (GAME_TAG) tag;
        switch (game_tag)
        {
            case GAME_TAG.NEXT_STEP:
            case GAME_TAG.STEP:
                try
                {
                    str2 = ((TAG_STEP) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.CLASS:
                try
                {
                    str2 = ((TAG_CLASS) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.CARDRACE:
                try
                {
                    str2 = ((TAG_RACE) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.FACTION:
                try
                {
                    str2 = ((TAG_FACTION) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.CARDTYPE:
                try
                {
                    str2 = ((TAG_CARDTYPE) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.RARITY:
                try
                {
                    str2 = ((TAG_RARITY) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.STATE:
                try
                {
                    str2 = ((TAG_STATE) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            case GAME_TAG.PLAYSTATE:
                try
                {
                    str2 = ((TAG_PLAYSTATE) val).ToString();
                }
                catch (Exception)
                {
                }
                break;

            default:
                switch (game_tag)
                {
                    case GAME_TAG.ENCHANTMENT_BIRTH_VISUAL:
                    case GAME_TAG.ENCHANTMENT_IDLE_VISUAL:
                        try
                        {
                            str2 = ((TAG_ENCHANTMENT_VISUAL) val).ToString();
                        }
                        catch (Exception)
                        {
                        }
                        break;

                    default:
                        switch (game_tag)
                        {
                            case GAME_TAG.ZONE:
                                try
                                {
                                    str2 = ((TAG_ZONE) val).ToString();
                                }
                                catch (Exception)
                                {
                                }
                                break;

                            case GAME_TAG.CARD_SET:
                                try
                                {
                                    str2 = ((TAG_CARD_SET) val).ToString();
                                }
                                catch (Exception)
                                {
                                }
                                break;

                            case GAME_TAG.MULLIGAN_STATE:
                                try
                                {
                                    str2 = ((TAG_MULLIGAN) val).ToString();
                                }
                                catch (Exception)
                                {
                                }
                                break;
                        }
                        break;
                }
                break;
        }
        return string.Format("tag={0} value={1}", str, str2);
    }
}

