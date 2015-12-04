using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GameDbf
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3B;
    public static Dbf Achieve;
    public static Dbf Adventure;
    public static Dbf AdventureData;
    public static Dbf AdventureMission;
    public static Dbf AdventureMode;
    public static Dbf Banner;
    public static Dbf Board;
    public static Dbf Booster;
    public static Dbf Card;
    public static Dbf CardBack;
    public static Dbf FixedReward;
    public static Dbf FixedRewardAction;
    public static Dbf FixedRewardMap;
    public static Dbf Hero;
    private static GameDbfIndex s_index;
    public static Dbf Scenario;
    public static Dbf Season;
    public static Dbf Wing;

    public static GameDbfIndex GetIndex()
    {
        if (s_index == null)
        {
            s_index = new GameDbfIndex();
        }
        return s_index;
    }

    public static void Load()
    {
        if (s_index == null)
        {
            s_index = new GameDbfIndex();
        }
        else
        {
            s_index.Initialize();
        }
        Achieve = Dbf.Load("ACHIEVE", (Dbf.RecordAddedListener) null);
        Adventure = Dbf.Load("ADVENTURE", (Dbf.RecordAddedListener) null);
        AdventureData = Dbf.Load("ADVENTURE_DATA", (Dbf.RecordAddedListener) null);
        AdventureMission = Dbf.Load("ADVENTURE_MISSION", (Dbf.RecordAddedListener) null);
        AdventureMode = Dbf.Load("ADVENTURE_MODE", (Dbf.RecordAddedListener) null);
        Banner = Dbf.Load("BANNER", (Dbf.RecordAddedListener) null);
        Booster = Dbf.Load("BOOSTER", (Dbf.RecordAddedListener) null);
        Board = Dbf.Load("BOARD", (Dbf.RecordAddedListener) null);
        Card = Dbf.Load("CARD", new Dbf.RecordAddedListener(s_index.OnCardAdded));
        CardBack = Dbf.Load("CARD_BACK", (Dbf.RecordAddedListener) null);
        FixedReward = Dbf.Load("FIXED_REWARD", (Dbf.RecordAddedListener) null);
        FixedRewardAction = Dbf.Load("FIXED_REWARD_ACTION", new Dbf.RecordAddedListener(s_index.OnFixedRewardActionAdded));
        FixedRewardMap = Dbf.Load("FIXED_REWARD_MAP", new Dbf.RecordAddedListener(s_index.OnFixedRewardMapAdded));
        Hero = Dbf.Load("HERO", (Dbf.RecordAddedListener) null);
        Scenario = Dbf.Load("SCENARIO", (Dbf.RecordAddedListener) null);
        Season = Dbf.Load("SEASON", (Dbf.RecordAddedListener) null);
        Wing = Dbf.Load("WING", (Dbf.RecordAddedListener) null);
    }

    public static void Reload(string name, string xml)
    {
        string key = name;
        if (key != null)
        {
            int num;
            if (<>f__switch$map3B == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                dictionary.Add("ACHIEVE", 0);
                dictionary.Add("CARD_BACK", 1);
                <>f__switch$map3B = dictionary;
            }
            if (<>f__switch$map3B.TryGetValue(key, out num))
            {
                if (num == 0)
                {
                    Achieve = Dbf.Load(name, xml, (Dbf.RecordAddedListener) null);
                    if (AchieveManager.Get() != null)
                    {
                        AchieveManager.Get().InitAchieveManager();
                    }
                    return;
                }
                if (num == 1)
                {
                    CardBack = Dbf.Load(name, xml, (Dbf.RecordAddedListener) null);
                    if (CardBackManager.Get() != null)
                    {
                        CardBackManager.Get().InitCardBackData();
                    }
                    return;
                }
            }
        }
        object[] messageArgs = new object[] { name };
        Error.AddDevFatal("Reloading {0} is unsupported", messageArgs);
    }
}

