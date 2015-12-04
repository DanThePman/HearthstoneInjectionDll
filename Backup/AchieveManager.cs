using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class AchieveManager
{
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache12;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache13;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache14;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache15;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache17;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache18;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache19;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache1A;
    [CompilerGenerated]
    private static Predicate<Achievement> <>f__am$cache1B;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map30;
    private static readonly long CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS = 0xb2d05e00L;
    private List<AchieveCanceledListener> m_achieveCanceledListeners = new List<AchieveCanceledListener>();
    private Map<int, Achievement> m_achievements = new Map<int, Achievement>();
    private HashSet<int> m_achieveValidationsRequested = new HashSet<int>();
    private HashSet<int> m_achieveValidationsToRequest = new HashSet<int>();
    private List<ActiveAchievesUpdatedListener> m_activeAchievesUpdatedListeners = new List<ActiveAchievesUpdatedListener>();
    private bool m_allNetAchievesReceived;
    private bool m_disableCancelButtonUntilServerReturns;
    private Map<int, long> m_lastCheckLicenseAddedByAchieve = new Map<int, long>();
    private long m_lastEventTimingAndLicenseAchieveCheck;
    private Map<int, long> m_lastEventTimingValidationByAchieve = new Map<int, long>();
    private List<LicenseAddedAchievesUpdatedListener> m_licenseAddedAchievesUpdatedListeners = new List<LicenseAddedAchievesUpdatedListener>();
    private List<NewlyCompleteAchievesListener> m_newlyCompletedAchievesListeners = new List<NewlyCompleteAchievesListener>();
    private int m_numEventResponsesNeeded;
    private bool m_waitingForActiveAchieves;
    private static AchieveManager s_instance = null;
    private static readonly long TIMED_ACHIEVE_VALIDATION_DELAY_TICKS = 0x23c34600L;
    private static readonly long TIMED_AND_LICENSE_ACHIEVE_CHECK_DELAY_TICKS = Math.Min(TIMED_ACHIEVE_VALIDATION_DELAY_TICKS, CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS);

    private AchieveManager()
    {
        this.LoadAchievesFromDBF();
    }

    private void AddAchievesToValidate(List<Achievement> possibleRaceAchieves, List<Achievement> possibleCardSetAchieves)
    {
        foreach (Achievement achievement in possibleRaceAchieves)
        {
            CardFlair flair = (achievement.AchieveTrigger != Achievement.Trigger.GAIN_GOLDEN_CARD) ? null : new CardFlair(TAG_PREMIUM.GOLDEN);
            TAG_CLASS? cardClass = null;
            TAG_RARITY? cardRarity = null;
            if (CollectionManager.Get().AllCardsInSetOwned(2, cardClass, cardRarity, new TAG_RACE?(achievement.RaceRequirement.Value), flair, false))
            {
                TAG_CLASS? nullable4 = null;
                TAG_RARITY? nullable5 = null;
                if (CollectionManager.Get().AllCardsInSetOwned(3, nullable4, nullable5, new TAG_RACE?(achievement.RaceRequirement.Value), flair, false))
                {
                    this.m_achieveValidationsToRequest.Add(achievement.ID);
                }
            }
        }
        foreach (Achievement achievement2 in possibleCardSetAchieves)
        {
            TAG_CLASS? nullable8 = null;
            TAG_RARITY? nullable9 = null;
            TAG_RACE? cardRace = null;
            if (CollectionManager.Get().AllCardsInSetOwned(new TAG_CARD_SET?(achievement2.CardSetRequirement.Value), nullable8, nullable9, cardRace, null, false))
            {
                this.m_achieveValidationsToRequest.Add(achievement2.ID);
            }
        }
    }

    public bool CanCancelQuest(int achieveID)
    {
        if (this.m_disableCancelButtonUntilServerReturns)
        {
            return false;
        }
        if (!this.CanCancelQuestNow())
        {
            return false;
        }
        Achievement achievement = this.GetAchievement(achieveID);
        if (achievement == null)
        {
            return false;
        }
        if (achievement.AchieveGroup != Achievement.Group.QUEST_DAILY)
        {
            return false;
        }
        return achievement.Active;
    }

    private bool CanCancelQuestNow()
    {
        if (Vars.Key("Quests.CanCancelManyTimes").GetBool(false))
        {
            return true;
        }
        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
        if (netObject == null)
        {
            return false;
        }
        long num = DateTime.Now.ToFileTimeUtc();
        return (netObject.NextQuestCancelDate <= num);
    }

    public void CancelQuest(int achieveID)
    {
        if (!this.CanCancelQuest(achieveID))
        {
            this.FireAchieveCanceledEvent(achieveID, false);
        }
        else
        {
            this.m_disableCancelButtonUntilServerReturns = true;
            Network.RequestCancelQuest(achieveID);
        }
    }

    private void CheckAllCardGainAchieves()
    {
        if (<>f__am$cache19 == null)
        {
            <>f__am$cache19 = delegate (Achievement obj) {
                if (!obj.IsCompleted())
                {
                    switch (obj.AchieveTrigger)
                    {
                        case Achievement.Trigger.GAIN_CARD:
                        case Achievement.Trigger.GAIN_GOLDEN_CARD:
                            return obj.RaceRequirement.HasValue;
                    }
                }
                return false;
            };
        }
        List<Achievement> possibleRaceAchieves = this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache19);
        if (<>f__am$cache1A == null)
        {
            <>f__am$cache1A = delegate (Achievement obj) {
                if (obj.IsCompleted())
                {
                    return false;
                }
                if (obj.AchieveTrigger != Achievement.Trigger.COMPLETE_CARD_SET)
                {
                    return false;
                }
                return obj.CardSetRequirement.HasValue;
            };
        }
        List<Achievement> possibleCardSetAchieves = this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache1A);
        this.AddAchievesToValidate(possibleRaceAchieves, possibleCardSetAchieves);
        this.ValidateAchievesNow(null);
    }

    private void CheckTimedTimedEventsAndLicenses(DateTime utcNow)
    {
        DateTime time = utcNow.ToLocalTime();
        if ((time.Ticks - this.m_lastEventTimingAndLicenseAchieveCheck) >= TIMED_AND_LICENSE_ACHIEVE_CHECK_DELAY_TICKS)
        {
            this.m_lastEventTimingAndLicenseAchieveCheck = time.Ticks;
            int num = 0;
            foreach (Achievement achievement in this.m_achievements.Values)
            {
                if (((achievement.Enabled && !achievement.IsCompleted()) && (achievement.Active && (achievement.AchieveTrigger == Achievement.Trigger.EVENT_TIMING_ONLY))) && (SpecialEventManager.Get().IsEventActive(achievement.EventTrigger, false) && (!this.m_lastEventTimingValidationByAchieve.ContainsKey(achievement.ID) || ((time.Ticks - this.m_lastEventTimingValidationByAchieve[achievement.ID]) >= TIMED_ACHIEVE_VALIDATION_DELAY_TICKS))))
                {
                    object[] args = new object[] { achievement.ID, time };
                    Log.Rachelle.Print("AchieveManager.CheckTimedTimedEventsAndLicenses(): checking on timed event achieve {0} time {1}", args);
                    this.m_lastEventTimingValidationByAchieve[achievement.ID] = time.Ticks;
                    this.m_achieveValidationsToRequest.Add(achievement.ID);
                    num++;
                }
                if (achievement.IsActiveLicenseAddedAchieve() && (!this.m_lastCheckLicenseAddedByAchieve.ContainsKey(achievement.ID) || ((utcNow.Ticks - this.m_lastCheckLicenseAddedByAchieve[achievement.ID]) >= CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS)))
                {
                    object[] objArray2 = new object[] { achievement.ID, time };
                    Log.Rachelle.Print("AchieveManager.CheckTimedTimedEventsAndLicenses(): checking on license added achieve {0} time {1}", objArray2);
                    this.m_lastCheckLicenseAddedByAchieve[achievement.ID] = utcNow.Ticks;
                    Network.CheckAccountLicenseAchieve(achievement.ID);
                }
            }
            if (num != 0)
            {
                this.ValidateAchievesNow(null);
            }
        }
    }

    private void FireAchieveCanceledEvent(int achieveID, bool success)
    {
        foreach (AchieveCanceledListener listener in this.m_achieveCanceledListeners.ToArray())
        {
            listener.Fire(achieveID, success);
        }
    }

    private void FireLicenseAddedAchievesUpdatedEvent()
    {
        List<Achievement> activeLicenseAddedAchieves = this.GetActiveLicenseAddedAchieves();
        foreach (LicenseAddedAchievesUpdatedListener listener in this.m_licenseAddedAchievesUpdatedListeners.ToArray())
        {
            listener.Fire(activeLicenseAddedAchieves);
        }
    }

    public static AchieveManager Get()
    {
        return s_instance;
    }

    public Achievement GetAchievement(int achieveID)
    {
        if (!this.m_achievements.ContainsKey(achieveID))
        {
            return null;
        }
        return this.m_achievements[achieveID];
    }

    public List<Achievement> GetAchievesInGroup(Achievement.Group achieveGroup)
    {
        <GetAchievesInGroup>c__AnonStorey355 storey = new <GetAchievesInGroup>c__AnonStorey355 {
            achieveGroup = achieveGroup
        };
        List<Achievement> list = new List<Achievement>(this.m_achievements.Values);
        return list.FindAll(new Predicate<Achievement>(storey.<>m__1E2));
    }

    public List<Achievement> GetAchievesInGroup(Achievement.Group achieveGroup, bool isComplete)
    {
        <GetAchievesInGroup>c__AnonStorey356 storey = new <GetAchievesInGroup>c__AnonStorey356 {
            isComplete = isComplete
        };
        return this.GetAchievesInGroup(achieveGroup).FindAll(new Predicate<Achievement>(storey.<>m__1E3));
    }

    private List<Achievement> GetActiveLicenseAddedAchieves()
    {
        if (<>f__am$cache1B == null)
        {
            <>f__am$cache1B = obj => obj.IsActiveLicenseAddedAchieve();
        }
        return this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache1B);
    }

    public List<Achievement> GetActiveQuests(bool onlyNewlyActive = false)
    {
        <GetActiveQuests>c__AnonStorey352 storey = new <GetActiveQuests>c__AnonStorey352 {
            onlyNewlyActive = onlyNewlyActive
        };
        return Enumerable.Where<Achievement>(this.m_achievements.Values, new Func<Achievement, bool>(storey.<>m__1DD)).ToList<Achievement>();
    }

    public List<Achievement> GetCompletedAchieves()
    {
        List<Achievement> list = new List<Achievement>(this.m_achievements.Values);
        if (<>f__am$cache14 == null)
        {
            <>f__am$cache14 = obj => obj.IsCompleted();
        }
        return list.FindAll(<>f__am$cache14);
    }

    public List<Achievement> GetNewCompletedAchieves()
    {
        if (<>f__am$cache12 == null)
        {
            <>f__am$cache12 = delegate (Achievement obj) {
                if (!obj.IsCompleted())
                {
                    return false;
                }
                if (obj.IsInternal())
                {
                    return false;
                }
                Achievement.Group achieveGroup = obj.AchieveGroup;
                switch (achieveGroup)
                {
                    case Achievement.Group.UNLOCK_GOLDEN_HERO:
                        return false;

                    case Achievement.Group.UNLOCK_HERO:
                        return false;
                }
                return (achieveGroup != Achievement.Group.QUEST_DAILY_REPEATABLE) && (obj.AcknowledgedProgress < obj.Progress);
            };
        }
        return this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache12);
    }

    public List<Achievement> GetNewlyProgressedQuests()
    {
        if (<>f__am$cache13 == null)
        {
            <>f__am$cache13 = obj => obj.Progress > obj.AcknowledgedProgress;
        }
        return Get().GetActiveQuests(false).FindAll(<>f__am$cache13);
    }

    public int GetNumAchievesInGroup(Achievement.Group achieveGroup)
    {
        return this.GetAchievesInGroup(achieveGroup).Count;
    }

    public Achievement GetUnlockGoldenHeroAchievement(string heroCardID, TAG_PREMIUM premium)
    {
        <GetUnlockGoldenHeroAchievement>c__AnonStorey357 storey = new <GetUnlockGoldenHeroAchievement>c__AnonStorey357 {
            heroCardID = heroCardID,
            premium = premium
        };
        return this.GetAchievesInGroup(Achievement.Group.UNLOCK_GOLDEN_HERO).Find(new Predicate<Achievement>(storey.<>m__1E4));
    }

    public bool HasActiveAchievesForEvent(SpecialEventType eventTrigger)
    {
        <HasActiveAchievesForEvent>c__AnonStorey358 storey = new <HasActiveAchievesForEvent>c__AnonStorey358 {
            eventTrigger = eventTrigger
        };
        if (storey.eventTrigger == SpecialEventType.IGNORE)
        {
            return false;
        }
        return (this.m_achievements.Values.ToList<Achievement>().FindAll(new Predicate<Achievement>(storey.<>m__1E5)).Count > 0);
    }

    public bool HasActiveLicenseAddedAchieves()
    {
        return (this.GetActiveLicenseAddedAchieves().Count > 0);
    }

    public bool HasActiveQuests(bool onlyNewlyActive = false)
    {
        <HasActiveQuests>c__AnonStorey353 storey = new <HasActiveQuests>c__AnonStorey353 {
            onlyNewlyActive = onlyNewlyActive
        };
        return Enumerable.Any<KeyValuePair<int, Achievement>>(this.m_achievements, new Func<KeyValuePair<int, Achievement>, bool>(storey.<>m__1DE));
    }

    public bool HasIncompletePurchaseAchieves()
    {
        if (<>f__am$cache15 == null)
        {
            <>f__am$cache15 = delegate (Achievement obj) {
                if (obj.IsCompleted())
                {
                    return false;
                }
                if (!obj.Enabled)
                {
                    return false;
                }
                return obj.AchieveTrigger == Achievement.Trigger.PURCHASE;
            };
        }
        return (this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache15).Count > 0);
    }

    public bool HasUnlockedFeature(Achievement.UnlockableFeature feature)
    {
        <HasUnlockedFeature>c__AnonStorey354 storey = new <HasUnlockedFeature>c__AnonStorey354 {
            feature = feature
        };
        if (DemoMgr.Get().ArenaIs1WinMode() && (storey.feature == Achievement.UnlockableFeature.FORGE))
        {
            return true;
        }
        Achievement achievement = this.m_achievements.Values.ToList<Achievement>().Find(new Predicate<Achievement>(storey.<>m__1E0));
        if (achievement == null)
        {
            Debug.LogWarning(string.Format("AchieveManager.HasUnlockedFeature(): could not find achieve that unlocks feature {0}", storey.feature));
            return false;
        }
        return achievement.IsCompleted();
    }

    public void Heartbeat()
    {
        this.CheckTimedTimedEventsAndLicenses(DateTime.UtcNow);
    }

    public static void Init()
    {
        if (s_instance == null)
        {
            s_instance = new AchieveManager();
            Network network = Network.Get();
            network.RegisterNetHandler(Achieves.PacketID.ID, new Network.NetHandler(s_instance.OnAchieves), null);
            network.RegisterNetHandler(CancelQuestResponse.PacketID.ID, new Network.NetHandler(s_instance.OnQuestCanceled), null);
            network.RegisterNetHandler(ValidateAchieveResponse.PacketID.ID, new Network.NetHandler(s_instance.OnAchieveValidated), null);
            network.RegisterNetHandler(TriggerEventResponse.PacketID.ID, new Network.NetHandler(s_instance.OnEventTriggered), null);
            network.RegisterNetHandler(PegasusUtil.AccountLicenseAchieveResponse.PacketID.ID, new Network.NetHandler(s_instance.OnAccountLicenseAchieveResponse), null);
            NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(s_instance.OnNewNotices));
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        }
        Network.RequestAchieves(false);
    }

    public void InitAchieveManager()
    {
        s_instance.WillReset();
    }

    private void InitAchievement(Achievement achievement)
    {
        if (this.m_achievements.ContainsKey(achievement.ID))
        {
            Debug.LogWarning(string.Format("AchieveManager.InitAchievement() - already registered achievement with ID {0}", achievement.ID));
        }
        else
        {
            this.m_achievements.Add(achievement.ID, achievement);
        }
    }

    private static bool IsActiveQuest(Achievement obj, bool onlyNewlyActive)
    {
        if (!obj.Active)
        {
            return false;
        }
        if (obj.IsInternal())
        {
            return false;
        }
        switch (obj.AchieveGroup)
        {
            case Achievement.Group.QUEST_DAILY_REPEATABLE:
                return false;

            case Achievement.Group.QUEST_HIDDEN:
                return false;

            case Achievement.Group.UNLOCK_GOLDEN_HERO:
                return false;

            case Achievement.Group.UNLOCK_HERO:
                return false;
        }
        if (onlyNewlyActive)
        {
            return obj.IsNewlyActive();
        }
        return true;
    }

    public bool IsReady()
    {
        if (!this.m_allNetAchievesReceived)
        {
            return false;
        }
        if (this.m_waitingForActiveAchieves)
        {
            return false;
        }
        if (this.m_numEventResponsesNeeded > 0)
        {
            return false;
        }
        if (this.m_achieveValidationsToRequest.Count > 0)
        {
            return false;
        }
        if (this.m_achieveValidationsRequested.Count > 0)
        {
            return false;
        }
        if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() == null)
        {
            return false;
        }
        return true;
    }

    private void LoadAchievesFromDBF()
    {
        List<DbfRecord> records = GameDbf.Achieve.GetRecords();
        Map<int, int> map = new Map<int, int>();
        foreach (DbfRecord record in records)
        {
            Achievement.Group group;
            <LoadAchievesFromDBF>c__AnonStorey35B storeyb = new <LoadAchievesFromDBF>c__AnonStorey35B();
            int id = record.GetId();
            bool @bool = record.GetBool("ENABLED");
            if (EnumUtils.TryGetEnum<Achievement.Group>(record.GetString("ACH_TYPE"), out group))
            {
                RewardVisualTiming timing;
                int @int = record.GetInt("ACH_QUOTA");
                int num3 = record.GetInt("RACE");
                TAG_RACE? raceReq = null;
                if (num3 != 0)
                {
                    raceReq = new TAG_RACE?((TAG_RACE) num3);
                }
                int num4 = record.GetInt("CARD_SET");
                TAG_CARD_SET? cardSetReq = null;
                if (num4 != 0)
                {
                    cardSetReq = new TAG_CARD_SET?((TAG_CARD_SET) num4);
                }
                string str2 = record.GetString("REWARD");
                long @long = record.GetLong("REWARD_DATA1");
                long num6 = record.GetLong("REWARD_DATA2");
                List<RewardData> rewards = new List<RewardData>();
                TAG_CLASS? classReq = null;
                string key = str2;
                if (key != null)
                {
                    int num9;
                    if (<>f__switch$map30 == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(13);
                        dictionary.Add("basic", 0);
                        dictionary.Add("card", 1);
                        dictionary.Add("card2x", 2);
                        dictionary.Add("cardback", 3);
                        dictionary.Add("cardset", 4);
                        dictionary.Add("craftable_golden", 5);
                        dictionary.Add("dust", 6);
                        dictionary.Add("forge", 7);
                        dictionary.Add("gold", 8);
                        dictionary.Add("goldhero", 9);
                        dictionary.Add("hero", 10);
                        dictionary.Add("mount", 11);
                        dictionary.Add("pack", 12);
                        <>f__switch$map30 = dictionary;
                    }
                    if (<>f__switch$map30.TryGetValue(key, out num9))
                    {
                        switch (num9)
                        {
                            case 0:
                                Debug.LogWarning(string.Format("AchieveManager.LoadAchievesFromFile(): unable to define reward {0} for achieve {1}", str2, id));
                                break;

                            case 1:
                            {
                                string cardID = GameUtils.TranslateDbIdToCardId((int) @long);
                                TAG_PREMIUM premium = (TAG_PREMIUM) ((int) num6);
                                rewards.Add(new CardRewardData(cardID, premium, 1));
                                break;
                            }
                            case 2:
                            {
                                string str4 = GameUtils.TranslateDbIdToCardId((int) @long);
                                TAG_PREMIUM tag_premium2 = (TAG_PREMIUM) ((int) num6);
                                rewards.Add(new CardRewardData(str4, tag_premium2, 2));
                                break;
                            }
                            case 3:
                                rewards.Add(new CardBackRewardData((int) @long));
                                break;

                            case 6:
                                rewards.Add(new ArcaneDustRewardData((int) @long));
                                break;

                            case 7:
                                rewards.Add(new ForgeTicketRewardData((int) @long));
                                break;

                            case 8:
                                rewards.Add(new GoldRewardData((long) ((int) @long)));
                                break;

                            case 9:
                            {
                                string str5 = GameUtils.TranslateDbIdToCardId((int) @long);
                                TAG_PREMIUM tag_premium3 = (TAG_PREMIUM) ((int) num6);
                                rewards.Add(new CardRewardData(str5, tag_premium3, 1));
                                break;
                            }
                            case 10:
                            {
                                classReq = new TAG_CLASS?((TAG_CLASS) ((int) num6));
                                string basicHeroCardIdFromClass = GameUtils.GetBasicHeroCardIdFromClass(classReq.Value);
                                if (!string.IsNullOrEmpty(basicHeroCardIdFromClass))
                                {
                                    rewards.Add(new CardRewardData(basicHeroCardIdFromClass, TAG_PREMIUM.NORMAL, 1));
                                }
                                break;
                            }
                            case 11:
                                rewards.Add(new MountRewardData((MountRewardData.MountType) ((int) @long)));
                                break;

                            case 12:
                            {
                                int num7 = (num6 <= 0L) ? 1 : ((int) num6);
                                rewards.Add(new BoosterPackRewardData(num7, (int) @long));
                                break;
                            }
                        }
                    }
                }
                if (EnumUtils.TryGetEnum<RewardVisualTiming>(record.GetString("REWARD_TIMING"), out timing))
                {
                    Achievement.UnlockableFeature feature;
                    Achievement.Trigger trigger;
                    string str10;
                    string str11;
                    string str = record.GetString("UNLOCKS");
                    Achievement.UnlockableFeature? unlockedFeature = null;
                    if (EnumUtils.TryGetEnum<Achievement.UnlockableFeature>(str, out feature))
                    {
                        unlockedFeature = new Achievement.UnlockableFeature?(feature);
                    }
                    storeyb.dbfParent = record.GetString("PARENT_ACH");
                    DbfRecord record2 = records.Find(new Predicate<DbfRecord>(storeyb.<>m__1EA));
                    int num8 = (record2 != null) ? record2.GetId() : 0;
                    map[id] = num8;
                    if (!EnumUtils.TryGetEnum<Achievement.Trigger>(record.GetString("TRIGGERED"), out trigger))
                    {
                        trigger = Achievement.Trigger.IGNORE;
                    }
                    SpecialEventType iGNORE = SpecialEventType.IGNORE;
                    Achievement.ClickTriggerType? clickType = null;
                    switch (trigger)
                    {
                        case Achievement.Trigger.CLICK:
                            clickType = new Achievement.ClickTriggerType?((Achievement.ClickTriggerType) ((int) @long));
                            break;

                        case Achievement.Trigger.EVENT:
                        case Achievement.Trigger.EVENT_TIMING_ONLY:
                            iGNORE = EnumUtils.GetEnum<SpecialEventType>(record.GetString("EVENT"));
                            break;
                    }
                    Achievement achievement = new Achievement(id, @bool, group, @int, trigger, raceReq, classReq, cardSetReq, clickType, iGNORE, unlockedFeature, rewards, timing);
                    if (record.TryGetLocString("NAME", out str10))
                    {
                        achievement.SetName(str10);
                    }
                    else
                    {
                        achievement.SetName(string.Empty);
                    }
                    if (record.TryGetLocString("DESCRIPTION", out str11))
                    {
                        achievement.SetDescription(str11);
                    }
                    else
                    {
                        achievement.SetDescription(string.Empty);
                    }
                    this.InitAchievement(achievement);
                }
            }
        }
        if (<>f__am$cache17 == null)
        {
            <>f__am$cache17 = delegate (Achievement obj) {
                if (!obj.IsInternal())
                {
                    return false;
                }
                return obj.Rewards.Count > 0;
            };
        }
        foreach (Achievement achievement2 in this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache17))
        {
            Achievement achievement3 = this.GetAchievement(map[achievement2.ID]);
            while ((achievement3 != null) && achievement3.IsInternal())
            {
                achievement3 = this.GetAchievement(map[achievement3.ID]);
            }
            if (achievement3 == null)
            {
                Debug.LogWarning(string.Format("AchieveManager.LoadAchievesFromDBF(): found internal achievement with reward but could not find non-internal parent: {0}", achievement2));
            }
            else
            {
                Achievement achievement4 = this.GetAchievement(achievement3.ID);
                if (achievement4 == null)
                {
                    Debug.LogWarning(string.Format("AchieveManager.LoadAchievesFromDBF(): parentAchieve with id {0} for internalRewardAchieve {1} is null!", achievement3, achievement2));
                }
                else
                {
                    achievement4.AddChildRewards(achievement2.Rewards);
                }
            }
        }
    }

    public void NotifyOfCardGained(EntityDef entityDef, CardFlair flair, int totalCount)
    {
        <NotifyOfCardGained>c__AnonStorey35A storeya = new <NotifyOfCardGained>c__AnonStorey35A {
            flair = flair,
            entityDef = entityDef
        };
        Log.Achievements.Print(string.Concat(new object[] { "NotifyOfCardGained: ", storeya.entityDef, " ", storeya.flair, " ", totalCount }), new object[0]);
        List<Achievement> possibleRaceAchieves = this.m_achievements.Values.ToList<Achievement>().FindAll(new Predicate<Achievement>(storeya.<>m__1E8));
        List<Achievement> possibleCardSetAchieves = this.m_achievements.Values.ToList<Achievement>().FindAll(new Predicate<Achievement>(storeya.<>m__1E9));
        this.AddAchievesToValidate(possibleRaceAchieves, possibleCardSetAchieves);
    }

    public void NotifyOfCardsGained(List<EntityDef> entityDefs, List<CardFlair> flairs)
    {
        bool flag = false;
        HashSet<TAG_CARD_SET> set = new HashSet<TAG_CARD_SET>();
        HashSet<TAG_RACE> set2 = new HashSet<TAG_RACE>();
        HashSet<Achievement> source = new HashSet<Achievement>();
        HashSet<Achievement> set4 = new HashSet<Achievement>();
        foreach (CardFlair flair in flairs)
        {
            if (flair.Premium == TAG_PREMIUM.GOLDEN)
            {
                flag = true;
            }
        }
        foreach (EntityDef def in entityDefs)
        {
            set.Add(def.GetCardSet());
            set2.Add(def.GetRace());
        }
        foreach (Achievement achievement in this.m_achievements.Values)
        {
            if ((!source.Contains(achievement) && !set4.Contains(achievement)) && !achievement.IsCompleted())
            {
                if (((achievement.AchieveTrigger == Achievement.Trigger.GAIN_CARD) || (flag && (achievement.AchieveTrigger == Achievement.Trigger.GAIN_GOLDEN_CARD))) && (achievement.RaceRequirement.HasValue && set2.Contains(achievement.RaceRequirement.Value)))
                {
                    source.Add(achievement);
                }
                else if ((achievement.AchieveTrigger == Achievement.Trigger.COMPLETE_CARD_SET) && set.Contains(achievement.CardSetRequirement.Value))
                {
                    set4.Add(achievement);
                }
            }
        }
        this.AddAchievesToValidate(source.ToList<Achievement>(), set4.ToList<Achievement>());
    }

    public void NotifyOfClick(Achievement.ClickTriggerType clickType)
    {
        <NotifyOfClick>c__AnonStorey359 storey = new <NotifyOfClick>c__AnonStorey359 {
            clickType = clickType
        };
        foreach (Achievement achievement in this.m_achievements.Values.ToList<Achievement>().FindAll(new Predicate<Achievement>(storey.<>m__1E7)))
        {
            this.m_achieveValidationsToRequest.Add(achievement.ID);
        }
        this.ValidateAchievesNow(null);
    }

    private void OnAccountLicenseAchieveResponse()
    {
        Network.AccountLicenseAchieveResponse accountLicenseAchieveResponse = Network.GetAccountLicenseAchieveResponse();
        if (accountLicenseAchieveResponse.Result != Network.AccountLicenseAchieveResponse.AchieveResult.COMPLETE)
        {
            this.FireLicenseAddedAchievesUpdatedEvent();
        }
        else
        {
            object[] args = new object[] { accountLicenseAchieveResponse.Achieve };
            Log.Rachelle.Print("AchieveManager.OnAccountLicenseAchieveResponse(): achieve {0} is now complete, refreshing achieves", args);
            this.UpdateActiveAchieves(new ActiveAchievesUpdatedCallback(this.OnAccountLicenseAchievesUpdated), accountLicenseAchieveResponse.Achieve);
        }
    }

    private void OnAccountLicenseAchievesUpdated(object userData)
    {
        int num = (int) userData;
        object[] args = new object[] { num };
        Log.Rachelle.Print("AchieveManager.OnAccountLicenseAchievesUpdated(): refreshing achieves complete, triggered by achieve {0}", args);
        this.FireLicenseAddedAchievesUpdatedEvent();
    }

    private void OnAchieves()
    {
        Network.AchieveList allAchievesList = Network.Achieves();
        if (!this.m_allNetAchievesReceived)
        {
            this.OnAllAchieves(allAchievesList);
        }
        else
        {
            this.OnActiveAndNewCompleteAchieves(allAchievesList);
        }
    }

    private void OnAchieveValidated()
    {
        Network.ValidatedAchieve validatedAchieve = Network.GetValidatedAchieve();
        this.m_achieveValidationsRequested.Remove(validatedAchieve.AchieveID);
        if (this.m_achieveValidationsRequested.Count <= 0)
        {
            this.UpdateActiveAchieves(null);
            NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
        }
    }

    private void OnActiveAndNewCompleteAchieves(Network.AchieveList activeAchievesList)
    {
        if (<>f__am$cache18 == null)
        {
            <>f__am$cache18 = obj => obj.Active;
        }
        List<Achievement> list = this.m_achievements.Values.ToList<Achievement>().FindAll(<>f__am$cache18);
        List<Achievement> achievements = new List<Achievement>();
        int num = 0;
        foreach (Network.AchieveList.Achieve achieve in activeAchievesList.Achieves)
        {
            <OnActiveAndNewCompleteAchieves>c__AnonStorey35C storeyc = new <OnActiveAndNewCompleteAchieves>c__AnonStorey35C {
                achievement = this.GetAchievement(achieve.ID)
            };
            if (storeyc.achievement != null)
            {
                Log.Achievements.Print("Processing achievement: " + storeyc.achievement, new object[0]);
                if (!achieve.Active)
                {
                    storeyc.achievement.OnAchieveData(achieve.Progress, achieve.AckProgress, achieve.CompletionCount, achieve.Active, achieve.DateGiven, achieve.DateCompleted, achieve.CanAck);
                    num++;
                    achievements.Add(storeyc.achievement);
                }
                else
                {
                    storeyc.achievement.UpdateActiveAchieve(achieve.Progress, achieve.AckProgress, achieve.DateGiven, achieve.CanAck);
                    Achievement item = list.Find(new Predicate<Achievement>(storeyc.<>m__1ED));
                    if (item != null)
                    {
                        list.Remove(item);
                    }
                }
            }
        }
        foreach (Achievement achievement2 in list)
        {
            num++;
            achievements.Add(achievement2);
            achievement2.Complete();
        }
        foreach (NewlyCompleteAchievesListener listener in this.m_newlyCompletedAchievesListeners.ToArray())
        {
            listener.Fire(achievements);
        }
        ActiveAchievesUpdatedListener[] listenerArray3 = this.m_activeAchievesUpdatedListeners.ToArray();
        this.m_activeAchievesUpdatedListeners.Clear();
        foreach (ActiveAchievesUpdatedListener listener2 in listenerArray3)
        {
            listener2.Fire();
        }
        if (num > 0)
        {
            NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
        }
        this.m_waitingForActiveAchieves = false;
    }

    private void OnAllAchieves(Network.AchieveList allAchievesList)
    {
        foreach (Network.AchieveList.Achieve achieve in allAchievesList.Achieves)
        {
            Achievement achievement = this.GetAchievement(achieve.ID);
            if (achievement != null)
            {
                achievement.OnAchieveData(achieve.Progress, achieve.AckProgress, achieve.CompletionCount, achieve.Active, achieve.DateGiven, achieve.DateCompleted, achieve.CanAck);
            }
        }
        this.CheckAllCardGainAchieves();
        this.m_allNetAchievesReceived = true;
    }

    private void OnEventTriggered()
    {
        Network.TriggeredEvent triggerEventResponse = Network.GetTriggerEventResponse();
        if (triggerEventResponse.Success)
        {
            if (Enum.IsDefined(typeof(SpecialEventType), triggerEventResponse.EventID))
            {
                SpecialEventType eventID = (SpecialEventType) triggerEventResponse.EventID;
                if (this.HasActiveAchievesForEvent(eventID))
                {
                    this.UpdateActiveAchieves(null);
                    NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
                }
            }
            else
            {
                Debug.LogWarning(string.Format("AchieveManager.OnEventTriggered(): unknown (successfully triggered) event ID {0}", triggerEventResponse.EventID));
            }
        }
        this.m_numEventResponsesNeeded--;
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        foreach (NetCache.ProfileNotice notice in newNotices)
        {
            if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT)
            {
                Achievement achievement = this.GetAchievement((int) notice.OriginData);
                if (achievement != null)
                {
                    achievement.AddRewardNoticeID(notice.NoticeID);
                }
            }
        }
    }

    private void OnQuestCanceled()
    {
        Network.CanceledQuest canceledQuest = Network.GetCanceledQuest();
        this.m_disableCancelButtonUntilServerReturns = false;
        if (canceledQuest.Canceled)
        {
            this.GetAchievement(canceledQuest.AchieveID).OnCancelSuccess();
            NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
            if (netObject != null)
            {
                netObject.NextQuestCancelDate = canceledQuest.NextQuestCancelDate;
            }
        }
        this.FireAchieveCanceledEvent(canceledQuest.AchieveID, canceledQuest.Canceled);
    }

    private bool RegisterActiveAchievesUpdatedListener(ActiveAchievesUpdatedCallback callback)
    {
        return this.RegisterActiveAchievesUpdatedListener(callback, null);
    }

    private bool RegisterActiveAchievesUpdatedListener(ActiveAchievesUpdatedCallback callback, object userData)
    {
        if (callback == null)
        {
            return false;
        }
        ActiveAchievesUpdatedListener item = new ActiveAchievesUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_activeAchievesUpdatedListeners.Contains(item))
        {
            return false;
        }
        this.m_activeAchievesUpdatedListeners.Add(item);
        return true;
    }

    public bool RegisterLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback)
    {
        return this.RegisterLicenseAddedAchievesUpdatedListener(callback, null);
    }

    public bool RegisterLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback, object userData)
    {
        LicenseAddedAchievesUpdatedListener item = new LicenseAddedAchievesUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_licenseAddedAchievesUpdatedListeners.Contains(item))
        {
            return false;
        }
        this.m_licenseAddedAchievesUpdatedListeners.Add(item);
        return true;
    }

    public bool RegisterNewlyCompletedAchievesListener(NewlyAcompletedAchievesCallback callback)
    {
        return this.RegisterNewlyCompletedAchievesListener(callback, null);
    }

    public bool RegisterNewlyCompletedAchievesListener(NewlyAcompletedAchievesCallback callback, object userData)
    {
        NewlyCompleteAchievesListener item = new NewlyCompleteAchievesListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_newlyCompletedAchievesListeners.Contains(item))
        {
            return false;
        }
        this.m_newlyCompletedAchievesListeners.Add(item);
        return true;
    }

    public bool RegisterQuestCanceledListener(AchieveCanceledCallback callback)
    {
        return this.RegisterQuestCanceledListener(callback, null);
    }

    public bool RegisterQuestCanceledListener(AchieveCanceledCallback callback, object userData)
    {
        AchieveCanceledListener item = new AchieveCanceledListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_achieveCanceledListeners.Contains(item))
        {
            return false;
        }
        this.m_achieveCanceledListeners.Add(item);
        return true;
    }

    public bool RemoveActiveAchievesUpdatedListener(ActiveAchievesUpdatedCallback callback)
    {
        return this.RemoveActiveAchievesUpdatedListener(callback, null);
    }

    public bool RemoveActiveAchievesUpdatedListener(ActiveAchievesUpdatedCallback callback, object userData)
    {
        if (callback == null)
        {
            return false;
        }
        ActiveAchievesUpdatedListener item = new ActiveAchievesUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_activeAchievesUpdatedListeners.Contains(item))
        {
            return false;
        }
        this.m_activeAchievesUpdatedListeners.Remove(item);
        return true;
    }

    public bool RemoveLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback)
    {
        return this.RemoveLicenseAddedAchievesUpdatedListener(callback, null);
    }

    public bool RemoveLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback, object userData)
    {
        LicenseAddedAchievesUpdatedListener item = new LicenseAddedAchievesUpdatedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_licenseAddedAchievesUpdatedListeners.Remove(item);
    }

    public bool RemoveNewlyCompletedAchievesListener(NewlyAcompletedAchievesCallback callback)
    {
        return this.RemoveNewlyCompletedAchievesListener(callback, null);
    }

    public bool RemoveNewlyCompletedAchievesListener(NewlyAcompletedAchievesCallback callback, object userData)
    {
        NewlyCompleteAchievesListener item = new NewlyCompleteAchievesListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_newlyCompletedAchievesListeners.Remove(item);
    }

    public bool RemoveQuestCanceledListener(AchieveCanceledCallback callback)
    {
        return this.RemoveQuestCanceledListener(callback, null);
    }

    public bool RemoveQuestCanceledListener(AchieveCanceledCallback callback, object userData)
    {
        AchieveCanceledListener item = new AchieveCanceledListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_achieveCanceledListeners.Remove(item);
    }

    public void TriggerLaunchDayEvent()
    {
        if (this.HasActiveAchievesForEvent(SpecialEventType.LAUNCH_DAY))
        {
            Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
            if (opposingSidePlayer != null)
            {
                BnetPlayer bnetPlayer = BnetNearbyPlayerMgr.Get().FindNearbyPlayer(opposingSidePlayer.GetGameAccountId());
                if (bnetPlayer != null)
                {
                    BnetAccountId accountId = bnetPlayer.GetAccountId();
                    if (accountId != null)
                    {
                        ulong num;
                        ulong num2;
                        List<BnetPlayer> nearbyPlayers = BnetNearbyPlayerMgr.Get().GetNearbyPlayers();
                        BnetPlayer player3 = null;
                        foreach (BnetPlayer player4 in nearbyPlayers)
                        {
                            BnetAccountId id2 = player4.GetAccountId();
                            if ((id2 != null) && !id2.Equals((BnetEntityId) accountId))
                            {
                                player3 = player4;
                                break;
                            }
                        }
                        if ((player3 != null) && (BnetNearbyPlayerMgr.Get().GetNearbySessionStartTime(bnetPlayer, out num) && BnetNearbyPlayerMgr.Get().GetNearbySessionStartTime(player3, out num2)))
                        {
                            BnetGameAccountId hearthstoneGameAccountId = bnetPlayer.GetHearthstoneGameAccountId();
                            if (hearthstoneGameAccountId != null)
                            {
                                BnetGameAccountId otherPlayerHSGameAccountID = player3.GetHearthstoneGameAccountId();
                                if (otherPlayerHSGameAccountID != null)
                                {
                                    this.m_numEventResponsesNeeded++;
                                    Network.TriggerLaunchEvent(hearthstoneGameAccountId, num, otherPlayerHSGameAccountID, num2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void UpdateActiveAchieves(ActiveAchievesUpdatedCallback callback)
    {
        this.UpdateActiveAchieves(callback, null);
    }

    public void UpdateActiveAchieves(ActiveAchievesUpdatedCallback callback, object userData)
    {
        this.RegisterActiveAchievesUpdatedListener(callback, userData);
        if (!this.m_waitingForActiveAchieves)
        {
            this.m_waitingForActiveAchieves = true;
            Network.RequestAchieves(true);
        }
    }

    public void ValidateAchievesNow(ActiveAchievesUpdatedCallback callback)
    {
        this.ValidateAchievesNow(callback, null);
    }

    public void ValidateAchievesNow(ActiveAchievesUpdatedCallback callback, object userData)
    {
        if (this.m_achieveValidationsToRequest.Count == 0)
        {
            if (callback != null)
            {
                callback(userData);
            }
        }
        else
        {
            this.RegisterActiveAchievesUpdatedListener(callback, userData);
            this.m_achieveValidationsRequested.Union<int>(this.m_achieveValidationsToRequest);
            foreach (int num in this.m_achieveValidationsToRequest)
            {
                Network.ValidateAchieve(num);
            }
            this.m_achieveValidationsToRequest.Clear();
        }
    }

    private void WillReset()
    {
        this.m_allNetAchievesReceived = false;
        this.m_waitingForActiveAchieves = false;
        this.m_achieveValidationsToRequest.Clear();
        this.m_achieveValidationsRequested.Clear();
        this.m_activeAchievesUpdatedListeners.Clear();
        this.m_newlyCompletedAchievesListeners.Clear();
        this.m_lastEventTimingValidationByAchieve.Clear();
        this.m_lastCheckLicenseAddedByAchieve.Clear();
        this.m_licenseAddedAchievesUpdatedListeners.Clear();
        this.m_achievements.Clear();
        this.LoadAchievesFromDBF();
    }

    [CompilerGenerated]
    private sealed class <GetAchievesInGroup>c__AnonStorey355
    {
        internal Achievement.Group achieveGroup;

        internal bool <>m__1E2(Achievement obj)
        {
            return (obj.AchieveGroup == this.achieveGroup);
        }
    }

    [CompilerGenerated]
    private sealed class <GetAchievesInGroup>c__AnonStorey356
    {
        internal bool isComplete;

        internal bool <>m__1E3(Achievement obj)
        {
            return (obj.IsCompleted() == this.isComplete);
        }
    }

    [CompilerGenerated]
    private sealed class <GetActiveQuests>c__AnonStorey352
    {
        internal bool onlyNewlyActive;

        internal bool <>m__1DD(Achievement obj)
        {
            return AchieveManager.IsActiveQuest(obj, this.onlyNewlyActive);
        }
    }

    [CompilerGenerated]
    private sealed class <GetUnlockGoldenHeroAchievement>c__AnonStorey357
    {
        private static Predicate<RewardData> <>f__am$cache2;
        internal string heroCardID;
        internal TAG_PREMIUM premium;

        internal bool <>m__1E4(Achievement achieveObj)
        {
            if (<>f__am$cache2 == null)
            {
                <>f__am$cache2 = rewardObj => rewardObj.RewardType == Reward.Type.CARD;
            }
            RewardData data = achieveObj.Rewards.Find(<>f__am$cache2);
            if (data == null)
            {
                return false;
            }
            CardRewardData data2 = data as CardRewardData;
            if (data2 == null)
            {
                return false;
            }
            return (data2.CardID.Equals(this.heroCardID) && data2.Premium.Equals(this.premium));
        }

        private static bool <>m__1F1(RewardData rewardObj)
        {
            return (rewardObj.RewardType == Reward.Type.CARD);
        }
    }

    [CompilerGenerated]
    private sealed class <HasActiveAchievesForEvent>c__AnonStorey358
    {
        internal SpecialEventType eventTrigger;

        internal bool <>m__1E5(Achievement obj)
        {
            if (obj.EventTrigger != this.eventTrigger)
            {
                return false;
            }
            if (!obj.Enabled)
            {
                return false;
            }
            return obj.Active;
        }
    }

    [CompilerGenerated]
    private sealed class <HasActiveQuests>c__AnonStorey353
    {
        internal bool onlyNewlyActive;

        internal bool <>m__1DE(KeyValuePair<int, Achievement> kv)
        {
            return AchieveManager.IsActiveQuest(kv.Value, this.onlyNewlyActive);
        }
    }

    [CompilerGenerated]
    private sealed class <HasUnlockedFeature>c__AnonStorey354
    {
        internal Achievement.UnlockableFeature feature;

        internal bool <>m__1E0(Achievement obj)
        {
            if (!obj.UnlockedFeature.HasValue)
            {
                return false;
            }
            return (((Achievement.UnlockableFeature) obj.UnlockedFeature.Value) == this.feature);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadAchievesFromDBF>c__AnonStorey35B
    {
        internal string dbfParent;

        internal bool <>m__1EA(DbfRecord obj)
        {
            return obj.GetString("NOTE_DESC").Equals(this.dbfParent);
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyOfCardGained>c__AnonStorey35A
    {
        internal EntityDef entityDef;
        internal CardFlair flair;

        internal bool <>m__1E8(Achievement obj)
        {
            if (obj.IsCompleted())
            {
                return false;
            }
            Achievement.Trigger achieveTrigger = obj.AchieveTrigger;
            if (achieveTrigger != Achievement.Trigger.GAIN_CARD)
            {
                if (achieveTrigger != Achievement.Trigger.GAIN_GOLDEN_CARD)
                {
                    return false;
                }
                if (this.flair.Premium != TAG_PREMIUM.GOLDEN)
                {
                    return false;
                }
            }
            if (!obj.RaceRequirement.HasValue)
            {
                return false;
            }
            return (((TAG_RACE) obj.RaceRequirement.Value) == this.entityDef.GetRace());
        }

        internal bool <>m__1E9(Achievement obj)
        {
            if (obj.IsCompleted())
            {
                return false;
            }
            if (obj.AchieveTrigger != Achievement.Trigger.COMPLETE_CARD_SET)
            {
                return false;
            }
            if (!obj.CardSetRequirement.HasValue)
            {
                return false;
            }
            return (((TAG_CARD_SET) obj.CardSetRequirement.Value) == this.entityDef.GetCardSet());
        }
    }

    [CompilerGenerated]
    private sealed class <NotifyOfClick>c__AnonStorey359
    {
        internal Achievement.ClickTriggerType clickType;

        internal bool <>m__1E7(Achievement obj)
        {
            if (obj.IsCompleted())
            {
                return false;
            }
            if (!obj.Enabled)
            {
                return false;
            }
            if (obj.AchieveTrigger != Achievement.Trigger.CLICK)
            {
                return false;
            }
            if (!obj.ClickType.HasValue)
            {
                return false;
            }
            return (((Achievement.ClickTriggerType) obj.ClickType.Value) == this.clickType);
        }
    }

    [CompilerGenerated]
    private sealed class <OnActiveAndNewCompleteAchieves>c__AnonStorey35C
    {
        internal Achievement achievement;

        internal bool <>m__1ED(Achievement obj)
        {
            return (obj.ID == this.achievement.ID);
        }
    }

    public delegate void AchieveCanceledCallback(int achieveID, bool success, object userData);

    private class AchieveCanceledListener : EventListener<AchieveManager.AchieveCanceledCallback>
    {
        public void Fire(int achieveID, bool success)
        {
            base.m_callback(achieveID, success, base.m_userData);
        }
    }

    public delegate void ActiveAchievesUpdatedCallback(object userData);

    private class ActiveAchievesUpdatedListener : EventListener<AchieveManager.ActiveAchievesUpdatedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void LicenseAddedAchievesUpdatedCallback(List<Achievement> activeLicenseAddedAchieves, object userData);

    private class LicenseAddedAchievesUpdatedListener : EventListener<AchieveManager.LicenseAddedAchievesUpdatedCallback>
    {
        public void Fire(List<Achievement> activeLicenseAddedAchieves)
        {
            base.m_callback(activeLicenseAddedAchieves, base.m_userData);
        }
    }

    public delegate void NewlyAcompletedAchievesCallback(List<Achievement> achievements, object userData);

    private class NewlyCompleteAchievesListener : EventListener<AchieveManager.NewlyAcompletedAchievesCallback>
    {
        public void Fire(List<Achievement> achievements)
        {
            base.m_callback(achievements, base.m_userData);
        }
    }
}

