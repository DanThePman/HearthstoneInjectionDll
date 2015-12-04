using PegasusGame;
using PegasusShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameUtils
{
    [CompilerGenerated]
    private static AssetLoader.GameObjectCallback <>f__am$cache0;

    public static void ApplyHideEntity(Entity entity, Network.HistHideEntity hideEntity)
    {
        entity.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
    }

    public static void ApplyPower(Entity entity, Network.PowerHistory power)
    {
        switch (power.Type)
        {
            case Network.PowerType.SHOW_ENTITY:
                ApplyShowEntity(entity, (Network.HistShowEntity) power);
                break;

            case Network.PowerType.HIDE_ENTITY:
                ApplyHideEntity(entity, (Network.HistHideEntity) power);
                break;

            case Network.PowerType.TAG_CHANGE:
                ApplyTagChange(entity, (Network.HistTagChange) power);
                break;
        }
    }

    public static void ApplyShowEntity(Entity entity, Network.HistShowEntity showEntity)
    {
        foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
        {
            entity.SetTag(tag.Name, tag.Value);
        }
    }

    public static void ApplyTagChange(Entity entity, Network.HistTagChange tagChange)
    {
        entity.SetTag(tagChange.Tag, tagChange.Value);
    }

    public static bool AreAllTutorialsComplete()
    {
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        if (netObject == null)
        {
            return false;
        }
        if (!AreAllTutorialsComplete(netObject.CampaignProgress))
        {
            return false;
        }
        return true;
    }

    public static bool AreAllTutorialsComplete(long campaignProgress)
    {
        return AreAllTutorialsComplete((TutorialProgress) campaignProgress);
    }

    public static bool AreAllTutorialsComplete(TutorialProgress progress)
    {
        if (DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE)
        {
            return false;
        }
        return (progress == TutorialProgress.ILLIDAN_COMPLETE);
    }

    public static int CardFlairSortComparisonAsc(CardFlair flair1, CardFlair flair2)
    {
        return (int) (flair1.Premium - flair2.Premium);
    }

    public static int CardFlairSortComparisonDesc(CardFlair flair1, CardFlair flair2)
    {
        return (int) (flair2.Premium - flair1.Premium);
    }

    public static int CardUID(int cardDbId, TAG_PREMIUM premium)
    {
        return (cardDbId | ((premium != TAG_PREMIUM.GOLDEN) ? 0 : 0x8000));
    }

    public static int CardUID(string cardStringId, TAG_PREMIUM premium)
    {
        return CardUID(TranslateCardIdToDbId(cardStringId), premium);
    }

    public static int ClientCardUID(int cardDbId, TAG_PREMIUM premium, bool owned)
    {
        return (CardUID(cardDbId, premium) | (!owned ? 0 : 0x10000));
    }

    public static int ClientCardUID(string cardStringId, TAG_PREMIUM premium, bool owned)
    {
        return ClientCardUID(TranslateCardIdToDbId(cardStringId), premium, owned);
    }

    public static int CountAllCollectibleCards()
    {
        return GameDbf.GetIndex().GetCollectibleCardCount();
    }

    public static int CountNonHeroCollectibleCards()
    {
        int num = 0;
        foreach (string str in GetAllCollectibleCardIds())
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(str);
            if ((entityDef != null) && (entityDef.GetCardType() != TAG_CARDTYPE.HERO))
            {
                num++;
            }
        }
        return num;
    }

    public static void DoDamageTasks(PowerTaskList powerTaskList, Card sourceCard, Card targetCard)
    {
        List<PowerTask> taskList = powerTaskList.GetTaskList();
        if ((taskList != null) && (taskList.Count != 0))
        {
            int entityId = sourceCard.GetEntity().GetEntityId();
            int num2 = targetCard.GetEntity().GetEntityId();
            foreach (PowerTask task in taskList)
            {
                Network.PowerHistory power = task.GetPower();
                if (power.Type == Network.PowerType.META_DATA)
                {
                    Network.HistMetaData data = (Network.HistMetaData) power;
                    if ((data.MetaType == HistoryMeta.Type.DAMAGE) || (data.MetaType == HistoryMeta.Type.HEALING))
                    {
                        foreach (int num3 in data.Info)
                        {
                            if ((num3 == entityId) || (num3 == num2))
                            {
                                task.DoTask();
                            }
                        }
                    }
                }
                else if (power.Type == Network.PowerType.TAG_CHANGE)
                {
                    Network.HistTagChange change = (Network.HistTagChange) power;
                    if ((change.Entity == entityId) || (change.Entity == num2))
                    {
                        switch (((GAME_TAG) change.Tag))
                        {
                            case GAME_TAG.DAMAGE:
                            case GAME_TAG.EXHAUSTED:
                                task.DoTask();
                                break;
                        }
                    }
                }
            }
        }
    }

    public static bool FixedRewardExistsForCraftingCard(string cardID, TAG_PREMIUM premium)
    {
        int num = TranslateCardIdToDbId(cardID);
        string str = EnumUtils.GetString<FixedRewardType>(FixedRewardType.CRAFTABLE_CARD);
        foreach (DbfRecord record in GameDbf.FixedReward.GetRecords())
        {
            if (record.GetString("TYPE").Equals(str))
            {
                int @int = record.GetInt("CARD_ID");
                if (num == @int)
                {
                    int num3 = record.GetInt("CARD_PREMIUM");
                    if (premium == num3)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static DbfRecord GetAdventureDataRecord(int adventureId, int modeId)
    {
        foreach (DbfRecord record in GameDbf.AdventureData.GetRecords())
        {
            if ((record.GetInt("ADVENTURE_ID") == adventureId) && (record.GetInt("MODE_ID") == modeId))
            {
                return record;
            }
        }
        return null;
    }

    public static List<DbfRecord> GetAdventureDataRecordsWithSubDefPrefab()
    {
        return GetRecordsWithAssetPathName(GameDbf.AdventureData, "ADVENTURE_SUB_DEF_PREFAB");
    }

    public static AdventureDbId GetAdventureId(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return AdventureDbId.INVALID;
        }
        return (AdventureDbId) record.GetInt("ADVENTURE_ID");
    }

    public static AdventureModeDbId GetAdventureModeId(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return AdventureModeDbId.INVALID;
        }
        return (AdventureModeDbId) record.GetInt("MODE_ID");
    }

    public static DbfRecord GetAdventureRecord(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return null;
        }
        int @int = record.GetInt("ADVENTURE_ID");
        return GameDbf.Adventure.GetRecord(@int);
    }

    public static List<DbfRecord> GetAdventureRecordsWithDefPrefab()
    {
        return GetRecordsWithAssetPathName(GameDbf.Adventure, "ADVENTURE_DEF_PREFAB");
    }

    public static List<DbfRecord> GetAdventureRecordsWithStorePrefab()
    {
        return GetRecordsWithAssetPathName(GameDbf.Adventure, "STORE_PREFAB");
    }

    public static List<string> GetAllCardIds()
    {
        return GameDbf.GetIndex().GetAllCardIds();
    }

    public static List<int> GetAllCollectibleCardDbIds()
    {
        return GameDbf.GetIndex().GetCollectibleCardDbIds();
    }

    public static List<string> GetAllCollectibleCardIds()
    {
        return GameDbf.GetIndex().GetCollectibleCardIds();
    }

    public static string GetBasicHeroCardIdFromClass(TAG_CLASS classTag)
    {
        switch (classTag)
        {
            case TAG_CLASS.DRUID:
                return "HERO_06";

            case TAG_CLASS.HUNTER:
                return "HERO_05";

            case TAG_CLASS.MAGE:
                return "HERO_08";

            case TAG_CLASS.PALADIN:
                return "HERO_04";

            case TAG_CLASS.PRIEST:
                return "HERO_09";

            case TAG_CLASS.ROGUE:
                return "HERO_03";

            case TAG_CLASS.SHAMAN:
                return "HERO_02";

            case TAG_CLASS.WARLOCK:
                return "HERO_07";

            case TAG_CLASS.WARRIOR:
                return "HERO_01";
        }
        UnityEngine.Debug.LogError(string.Format("GameUtils.GetBasicHeroCardIdFromClass() - unsupported class tag {0}", classTag));
        return string.Empty;
    }

    public static int GetBoardIdFromAssetName(string name)
    {
        foreach (DbfRecord record in GameDbf.Board.GetRecords())
        {
            string assetName = record.GetAssetName("PREFAB");
            if (name == assetName)
            {
                return record.GetId();
            }
        }
        return 0;
    }

    public static int GetBoosterCount()
    {
        NetCache.NetCacheBoosters netObject = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
        if (netObject == null)
        {
            return 0;
        }
        return netObject.GetTotalNumBoosters();
    }

    public static GameObject GetBrokenCardPrefab()
    {
        return null;
    }

    public static TextAsset GetBrokenCardXml()
    {
        return null;
    }

    public static DbfRecord GetCardRecord(int dbId)
    {
        return GameDbf.GetIndex().GetCardRecord(dbId);
    }

    public static DbfRecord GetCardRecord(string cardId)
    {
        if (cardId == null)
        {
            return null;
        }
        return GameDbf.GetIndex().GetCardRecord(cardId);
    }

    public static TAG_CARD_SET GetCardSetFromCardID(string cardID)
    {
        EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
        if (entityDef == null)
        {
            UnityEngine.Debug.LogError(string.Format("Null EntityDef in GetCardSetFromCardID() for {0}", cardID));
            return TAG_CARD_SET.INVALID;
        }
        return entityDef.GetCardSet();
    }

    public static TAG_CLASS GetClassChallengeHeroClass(DbfRecord rec)
    {
        if (rec.GetInt("MODE_ID") != 4)
        {
            return TAG_CLASS.INVALID;
        }
        int @int = rec.GetInt("PLAYER1_HERO_CARD_ID");
        EntityDef entityDef = DefLoader.Get().GetEntityDef(@int);
        if (entityDef == null)
        {
            return TAG_CLASS.INVALID;
        }
        return entityDef.GetClass();
    }

    public static List<TAG_CLASS> GetClassChallengeHeroClasses(int adventureId, int wingId)
    {
        List<DbfRecord> classChallengeRecords = GetClassChallengeRecords(adventureId, wingId);
        List<TAG_CLASS> list2 = new List<TAG_CLASS>();
        foreach (DbfRecord record in classChallengeRecords)
        {
            list2.Add(GetClassChallengeHeroClass(record));
        }
        return list2;
    }

    public static List<DbfRecord> GetClassChallengeRecords(int adventureId, int wingId)
    {
        List<DbfRecord> list = new List<DbfRecord>();
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            if (((record.GetInt("MODE_ID") == 4) && (record.GetInt("ADVENTURE_ID") == adventureId)) && (record.GetInt("WING_ID") == wingId))
            {
                list.Add(record);
            }
        }
        return list;
    }

    public static string GetCurrentTutorialCardRewardDetails()
    {
        return GetTutorialCardRewardDetails(GameMgr.Get().GetMissionId());
    }

    public static int GetFakePackCount()
    {
        if (!ApplicationMgr.IsInternal())
        {
            return 0;
        }
        return Options.Get().GetInt(Option.FAKE_PACK_COUNT);
    }

    public static TAG_ZONE GetFinalZoneForEntity(Entity entity)
    {
        PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
        List<PowerTaskList> list = new List<PowerTaskList>();
        if (powerProcessor.GetCurrentTaskList() != null)
        {
            list.Add(powerProcessor.GetCurrentTaskList());
        }
        list.AddRange(powerProcessor.GetPowerQueue().GetList());
        for (int i = list.Count - 1; i >= 0; i--)
        {
            List<PowerTask> taskList = list[i].GetTaskList();
            for (int j = taskList.Count - 1; j >= 0; j--)
            {
                PowerTask task = taskList[j];
                Network.HistTagChange power = task.GetPower() as Network.HistTagChange;
                if ((power != null) && ((power.Entity == entity.GetEntityId()) && (power.Tag == 0x31)))
                {
                    return (TAG_ZONE) power.Value;
                }
            }
        }
        return entity.GetZone();
    }

    public static List<DbfRecord> GetFixedActionRecords(FixedActionType actionType)
    {
        return GameDbf.GetIndex().GetFixedActionRecordsForType(actionType);
    }

    public static List<DbfRecord> GetFixedRewardMapRecordsForAction(int actionID)
    {
        return GameDbf.GetIndex().GetFixedRewardMapRecordsForAction(actionID);
    }

    public static NetCache.HeroLevel GetHeroLevel(TAG_CLASS heroClass)
    {
        <GetHeroLevel>c__AnonStorey38D storeyd = new <GetHeroLevel>c__AnonStorey38D {
            heroClass = heroClass
        };
        NetCache.NetCacheHeroLevels netObject = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
        if (netObject == null)
        {
            UnityEngine.Debug.LogError("GameUtils.GetHeroLevel() - NetCache.NetCacheHeroLevels is null");
        }
        return netObject.Levels.Find(new Predicate<NetCache.HeroLevel>(storeyd.<>m__289));
    }

    public static string GetHeroPowerCardIdFromHero(int heroDbId)
    {
        DbfRecord cardRecord = GetCardRecord(heroDbId);
        if (cardRecord == null)
        {
            UnityEngine.Debug.LogError(string.Format("GameUtils.GetHeroPowerCardIdFromHero() - failed to find record for heroDbId {0}", heroDbId));
            return string.Empty;
        }
        return TranslateDbIdToCardId(cardRecord.GetInt("HERO_POWER_ID"));
    }

    public static string GetHeroPowerCardIdFromHero(string heroCardId)
    {
        DbfRecord cardRecord = GetCardRecord(heroCardId);
        if (cardRecord == null)
        {
            UnityEngine.Debug.LogError(string.Format("GameUtils.GetHeroPowerCardIdFromHero() - failed to find record for heroCardId {0}", heroCardId));
            return string.Empty;
        }
        return TranslateDbIdToCardId(cardRecord.GetInt("HERO_POWER_ID"));
    }

    public static Card GetJoustWinner(Network.HistMetaData metaData)
    {
        if (metaData == null)
        {
            return null;
        }
        if (metaData.MetaType != HistoryMeta.Type.JOUST)
        {
            return null;
        }
        Entity entity = GameState.Get().GetEntity(metaData.Data);
        if (entity == null)
        {
            return null;
        }
        return entity.GetCard();
    }

    public static string GetMissionHeroCardId(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return null;
        }
        int @int = record.GetInt("CLIENT_PLAYER2_HERO_CARD_ID");
        if (@int == 0)
        {
            @int = record.GetInt("PLAYER2_HERO_CARD_ID");
        }
        return TranslateDbIdToCardId(@int);
    }

    public static string GetMissionHeroName(int missionId)
    {
        string missionHeroCardId = GetMissionHeroCardId(missionId);
        if (missionHeroCardId == null)
        {
            return null;
        }
        EntityDef entityDef = DefLoader.Get().GetEntityDef(missionHeroCardId);
        if (entityDef == null)
        {
            UnityEngine.Debug.LogError(string.Format("GameUtils.GetMissionHeroName() - hero {0} for mission {1} has no EntityDef", missionHeroCardId, missionId));
            return null;
        }
        return entityDef.GetName();
    }

    public static string GetMissionHeroPowerCardId(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return null;
        }
        int @int = record.GetInt("CLIENT_PLAYER2_HERO_CARD_ID");
        if (@int == 0)
        {
            @int = record.GetInt("PLAYER2_HERO_CARD_ID");
        }
        DbfRecord record2 = GameDbf.Card.GetRecord(@int);
        if (record2 == null)
        {
            return null;
        }
        return TranslateDbIdToCardId(record2.GetInt("HERO_POWER_ID"));
    }

    public static int GetNextTutorial()
    {
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        if (netObject == null)
        {
            return 0;
        }
        return GetNextTutorial(netObject.CampaignProgress);
    }

    public static int GetNextTutorial(TutorialProgress progress)
    {
        if (progress == TutorialProgress.NOTHING_COMPLETE)
        {
            return 3;
        }
        if (progress == TutorialProgress.HOGGER_COMPLETE)
        {
            return 4;
        }
        if (progress == TutorialProgress.MILLHOUSE_COMPLETE)
        {
            return 0xf9;
        }
        if (progress == TutorialProgress.CHO_COMPLETE)
        {
            return 0xb5;
        }
        if (progress == TutorialProgress.MUKLA_COMPLETE)
        {
            return 0xc9;
        }
        if (progress == TutorialProgress.NESINGWARY_COMPLETE)
        {
            return 0xf8;
        }
        return 0;
    }

    public static List<string> GetNonHeroCollectibleCardIds()
    {
        List<string> list = new List<string>();
        foreach (string str in GetAllCollectibleCardIds())
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(str);
            if ((entityDef != null) && (entityDef.GetCardType() != TAG_CARDTYPE.HERO))
            {
                list.Add(str);
            }
        }
        return list;
    }

    public static PackOpeningRarity GetPackOpeningRarity(TAG_RARITY tag)
    {
        switch (tag)
        {
            case TAG_RARITY.COMMON:
                return PackOpeningRarity.COMMON;

            case TAG_RARITY.FREE:
                return PackOpeningRarity.COMMON;

            case TAG_RARITY.RARE:
                return PackOpeningRarity.RARE;

            case TAG_RARITY.EPIC:
                return PackOpeningRarity.EPIC;

            case TAG_RARITY.LEGENDARY:
                return PackOpeningRarity.LEGENDARY;
        }
        return PackOpeningRarity.NONE;
    }

    public static List<DbfRecord> GetPackRecordsWithStorePrefab()
    {
        return GetRecordsWithAssetPathName(GameDbf.Booster, "STORE_PREFAB");
    }

    private static List<DbfRecord> GetRecordsWithAssetPathName(Dbf dbf, string assetPathColumn)
    {
        List<DbfRecord> list = new List<DbfRecord>();
        foreach (DbfRecord record in dbf.GetRecords())
        {
            if (!string.IsNullOrEmpty(record.GetAssetPath(assetPathColumn)))
            {
                list.Add(record);
            }
        }
        return list;
    }

    public static List<int> GetSortedPackIds()
    {
        List<int> list = new List<int>();
        foreach (DbfRecord record in GameDbf.Booster.GetRecords())
        {
            int id = record.GetId();
            list.Add(id);
        }
        list.Sort(new Comparison<int>(GameUtils.PackSortComparison));
        return list;
    }

    public static string GetTutorialCardRewardDetails(int missionId)
    {
        ScenarioDbId id = (ScenarioDbId) missionId;
        if (id != ScenarioDbId.TUTORIAL_HOGGER)
        {
            if (id == ScenarioDbId.TUTORIAL_MILLHOUSE)
            {
                return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL02");
            }
            if (id == ScenarioDbId.TUTORIAL_ILLIDAN)
            {
                return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL05");
            }
            if (id == ScenarioDbId.TUTORIAL_CHO)
            {
                return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL06");
            }
            if (id == ScenarioDbId.TUTORIAL_MUKLA)
            {
                return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL03");
            }
            if (id == ScenarioDbId.TUTORIAL_NESINGWARY)
            {
                return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL04");
            }
        }
        else
        {
            return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL01");
        }
        UnityEngine.Debug.LogWarning(string.Format("GameUtils.GetTutorialCardRewardDetails(): no card reward details for mission {0}", missionId));
        return string.Empty;
    }

    public static DbfRecord GetWingRecord(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return null;
        }
        int @int = record.GetInt("WING_ID");
        return GameDbf.Wing.GetRecord(@int);
    }

    public static bool HaveBoosters()
    {
        return (GetBoosterCount() > 0);
    }

    public static UnityEngine.Object Instantiate(UnityEngine.Object original)
    {
        if (original == null)
        {
            return null;
        }
        return UnityEngine.Object.Instantiate(original);
    }

    public static UnityEngine.Object Instantiate(Component original, GameObject parent, bool withRotation = false)
    {
        if (original == null)
        {
            return null;
        }
        Component child = UnityEngine.Object.Instantiate<Component>(original);
        SetParent(child, parent, withRotation);
        return child;
    }

    public static UnityEngine.Object Instantiate(GameObject original, GameObject parent, bool withRotation = false)
    {
        if (original == null)
        {
            return null;
        }
        GameObject child = UnityEngine.Object.Instantiate<GameObject>(original);
        SetParent(child, parent, withRotation);
        return child;
    }

    public static UnityEngine.Object InstantiateGameObject(string name, GameObject parent = null, bool withRotation = false)
    {
        if (name == null)
        {
            return null;
        }
        GameObject child = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(name), true, false);
        if (parent != null)
        {
            SetParent(child, parent, withRotation);
        }
        return child;
    }

    public static bool IsAIMission(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        return (record.GetInt("PLAYERS") == 1);
    }

    public static bool IsAnyTransitionActive()
    {
        SceneMgr mgr = SceneMgr.Get();
        if (mgr != null)
        {
            if (mgr.WillTransition())
            {
                return true;
            }
            if (mgr.IsTransitioning())
            {
                return true;
            }
        }
        Box box = Box.Get();
        if ((box != null) && box.IsTransitioningToSceneMode())
        {
            return true;
        }
        LoadingScreen screen = LoadingScreen.Get();
        return ((screen != null) && screen.IsTransitioning());
    }

    public static bool IsBeginPhase(TAG_STEP step)
    {
        switch (step)
        {
            case TAG_STEP.INVALID:
            case TAG_STEP.BEGIN_FIRST:
            case TAG_STEP.BEGIN_SHUFFLE:
            case TAG_STEP.BEGIN_DRAW:
            case TAG_STEP.BEGIN_MULLIGAN:
                return true;
        }
        return false;
    }

    public static bool IsCardCollectible(string cardId)
    {
        DbfRecord cardRecord = GetCardRecord(cardId);
        if (cardRecord == null)
        {
            return false;
        }
        return cardRecord.GetBool("IS_COLLECTIBLE");
    }

    public static bool IsCharacterDeathTagChange(Network.HistTagChange tagChange)
    {
        if (tagChange.Tag != 0x31)
        {
            return false;
        }
        if (tagChange.Value != 4)
        {
            return false;
        }
        Entity entity = GameState.Get().GetEntity(tagChange.Entity);
        if (entity == null)
        {
            return false;
        }
        if (!entity.IsCharacter())
        {
            return false;
        }
        return true;
    }

    public static bool IsClassChallengeMission(int missionId)
    {
        return (GetAdventureModeId(missionId) == AdventureModeDbId.CLASS_CHALLENGE);
    }

    public static bool IsClassicMission(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        int @int = record.GetInt("ADVENTURE_ID");
        if (@int == 0)
        {
            return false;
        }
        AdventureDbId id = (AdventureDbId) @int;
        if ((id != AdventureDbId.TUTORIAL) && (id != AdventureDbId.PRACTICE))
        {
            return false;
        }
        return true;
    }

    public static bool IsCoopMission(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        return record.GetBool("IS_COOP");
    }

    public static bool IsEntityHiddenAfterCurrentTasklist(Entity entity)
    {
        if (!entity.IsHidden())
        {
            return false;
        }
        PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
        if (powerProcessor.GetCurrentTaskList() != null)
        {
            foreach (PowerTask task in powerProcessor.GetCurrentTaskList().GetTaskList())
            {
                Network.HistShowEntity power = task.GetPower() as Network.HistShowEntity;
                if ((power != null) && ((power.Entity.ID == entity.GetEntityId()) && !string.IsNullOrEmpty(power.Entity.CardID)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static bool IsExpansionAdventure(AdventureDbId adventureId)
    {
        switch (adventureId)
        {
            case AdventureDbId.NAXXRAMAS:
            case AdventureDbId.BRM:
            case AdventureDbId.LOE:
                return true;
        }
        return false;
    }

    public static bool IsExpansionMission(int missionId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        int @int = record.GetInt("ADVENTURE_ID");
        if (@int == 0)
        {
            return false;
        }
        AdventureDbId id = (AdventureDbId) @int;
        return ((id != AdventureDbId.TUTORIAL) && (id != AdventureDbId.PRACTICE));
    }

    public static bool IsFakePackOpeningEnabled()
    {
        if (!ApplicationMgr.IsInternal())
        {
            return false;
        }
        return Options.Get().GetBool(Option.FAKE_PACK_OPENING);
    }

    public static bool IsFriendlyConcede(Network.HistTagChange tagChange)
    {
        if (tagChange.Tag != 0x11)
        {
            return false;
        }
        Player entity = GameState.Get().GetEntity(tagChange.Entity) as Player;
        if (entity == null)
        {
            return false;
        }
        if (!entity.IsFriendlySide())
        {
            return false;
        }
        return (tagChange.Value == 8);
    }

    public static bool IsGameOverTag(Player player, int tag, int val)
    {
        if (player != null)
        {
            if (tag != 0x11)
            {
                return false;
            }
            if (!player.IsFriendlySide())
            {
                return false;
            }
            switch (((TAG_PLAYSTATE) val))
            {
                case TAG_PLAYSTATE.WON:
                case TAG_PLAYSTATE.LOST:
                case TAG_PLAYSTATE.TIED:
                    return true;
            }
        }
        return false;
    }

    public static bool IsGameOverTag(int entityId, int tag, int val)
    {
        Player entity = GameState.Get().GetEntity(entityId) as Player;
        return IsGameOverTag(entity, tag, val);
    }

    public static bool IsHeroicAdventureMission(int missionId)
    {
        return (GetAdventureModeId(missionId) == AdventureModeDbId.HEROIC);
    }

    public static bool IsHistoryDeathTagChange(Network.HistTagChange tagChange)
    {
        if ((tagChange.Tag != 360) || (tagChange.Value != 1))
        {
            return false;
        }
        Entity entity = GameState.Get().GetEntity(tagChange.Entity);
        if (entity == null)
        {
            return false;
        }
        if (entity.IsEnchantment())
        {
            return false;
        }
        if (entity.GetCardType() == TAG_CARDTYPE.INVALID)
        {
            return false;
        }
        return true;
    }

    public static bool IsMainPhase(TAG_STEP step)
    {
        switch (step)
        {
            case TAG_STEP.MAIN_BEGIN:
            case TAG_STEP.MAIN_READY:
            case TAG_STEP.MAIN_RESOURCE:
            case TAG_STEP.MAIN_DRAW:
            case TAG_STEP.MAIN_START:
            case TAG_STEP.MAIN_ACTION:
            case TAG_STEP.MAIN_COMBAT:
            case TAG_STEP.MAIN_END:
            case TAG_STEP.MAIN_NEXT:
            case TAG_STEP.MAIN_CLEANUP:
            case TAG_STEP.MAIN_START_TRIGGERS:
                return true;
        }
        return false;
    }

    public static bool IsMatchmadeGameType(GameType gameType)
    {
        GameType type = gameType;
        switch (type)
        {
            case GameType.GT_ARENA:
            case GameType.GT_RANKED:
            case GameType.GT_UNRANKED:
                return true;
        }
        if (type != GameType.GT_TAVERNBRAWL)
        {
            return false;
        }
        return ((TavernBrawlManager.Get().CurrentMission() == null) || true);
    }

    public static bool IsMissionForAdventure(int missionId, int adventureId)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record == null)
        {
            return false;
        }
        return (record.GetInt("ADVENTURE_ID") == adventureId);
    }

    public static bool IsPastBeginPhase(TAG_STEP step)
    {
        return !IsBeginPhase(step);
    }

    public static bool IsPracticeMission(int missionId)
    {
        return IsMissionForAdventure(missionId, 2);
    }

    public static bool IsTutorialMission(int missionId)
    {
        return IsMissionForAdventure(missionId, 1);
    }

    public static bool IsWingComplete(int advId, int modeId, int wingId)
    {
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            int @int = record.GetInt("ADVENTURE_ID");
            int num2 = record.GetInt("MODE_ID");
            int num3 = record.GetInt("WING_ID");
            if (((@int == advId) && (num2 == modeId)) && ((num3 == wingId) && !AdventureProgressMgr.Get().HasDefeatedScenario(record.GetId())))
            {
                return false;
            }
        }
        return true;
    }

    public static bool LoadCardDefEmoteSound(CardDef cardDef, EmoteType type, EmoteSoundLoaded callback)
    {
        <LoadCardDefEmoteSound>c__AnonStorey38F storeyf = new <LoadCardDefEmoteSound>c__AnonStorey38F {
            type = type,
            callback = callback
        };
        if (storeyf.callback == null)
        {
            UnityEngine.Debug.LogError("No callback provided for LoadEmote!");
            return false;
        }
        if ((cardDef == null) || (cardDef.m_EmoteDefs == null))
        {
            return false;
        }
        EmoteEntryDef def = cardDef.m_EmoteDefs.Find(new Predicate<EmoteEntryDef>(storeyf.<>m__28B));
        if (def == null)
        {
            return false;
        }
        AssetLoader.Get().LoadSpell(def.m_emoteSoundSpellPath, new AssetLoader.GameObjectCallback(storeyf.<>m__28C), null, false);
        return true;
    }

    public static T LoadDbfGameObjectWithComponent<T>(DbfRecord record, string dbfRecordName) where T: Component
    {
        string assetPath = record.GetAssetPath(dbfRecordName);
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }
        return LoadGameObjectWithComponent<T>(assetPath);
    }

    public static T LoadGameObjectWithComponent<T>(string assetPath) where T: Component
    {
        GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetPath), true, false);
        if (obj2 == null)
        {
            return null;
        }
        T component = obj2.GetComponent<T>();
        if (component == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0} object does not contain {1} component.", assetPath, typeof(T)));
            UnityEngine.Object.Destroy(obj2);
            return null;
        }
        return component;
    }

    public static void LogoutConfirmation()
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get(!Network.ShouldBeConnectedToAurora() ? "GLOBAL_LOGIN_CONFIRM_TITLE" : "GLOBAL_LOGOUT_CONFIRM_TITLE"),
            m_text = GameStrings.Get(!Network.ShouldBeConnectedToAurora() ? "GLOBAL_LOGIN_CONFIRM_MESSAGE" : "GLOBAL_LOGOUT_CONFIRM_MESSAGE"),
            m_alertTextAlignment = UberText.AlignmentOptions.Center,
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
            m_responseCallback = new AlertPopup.ResponseCallback(GameUtils.OnLogoutConfirmationResponse)
        };
        DialogManager.Get().ShowPopup(info);
    }

    public static int MissionSortComparison(DbfRecord rec1, DbfRecord rec2)
    {
        int @int = rec1.GetInt("SORT_ORDER");
        int num2 = rec2.GetInt("SORT_ORDER");
        return (@int - num2);
    }

    private static void OnLogoutConfirmationResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            GameMgr.Get().SetPendingAutoConcede(true);
            if (Network.ShouldBeConnectedToAurora())
            {
                WebAuth.ClearLoginData();
            }
            ApplicationMgr.Get().ResetAndForceLogin();
        }
    }

    public static int PackSortComparison(int id1, int id2)
    {
        DbfRecord record = GameDbf.Booster.GetRecord(id1);
        DbfRecord record2 = GameDbf.Booster.GetRecord(id2);
        int @int = record.GetInt("SORT_ORDER");
        int num2 = record2.GetInt("SORT_ORDER");
        return (@int - num2);
    }

    public static void PlayCardEffectDefSounds(CardEffectDef cardEffectDef)
    {
        if (cardEffectDef != null)
        {
            foreach (string str in cardEffectDef.m_SoundSpellPaths)
            {
                if (<>f__am$cache0 == null)
                {
                    <>f__am$cache0 = delegate (string name, GameObject go, object data) {
                        <PlayCardEffectDefSounds>c__AnonStorey38E storeye = new <PlayCardEffectDefSounds>c__AnonStorey38E();
                        if (go == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Unable to load spell object: {0}", name));
                        }
                        else
                        {
                            storeye.destroyObj = go;
                            CardSoundSpell component = go.GetComponent<CardSoundSpell>();
                            if (component == null)
                            {
                                UnityEngine.Debug.LogError(string.Format("Card sound spell component not found: {0}", name));
                                UnityEngine.Object.Destroy(storeye.destroyObj);
                            }
                            else
                            {
                                component.AddStateFinishedCallback(new Spell.StateFinishedCallback(storeye.<>m__28D));
                                component.ForceDefaultAudioSource();
                                component.Activate();
                            }
                        }
                    };
                }
                AssetLoader.Get().LoadSpell(str, <>f__am$cache0, null, false);
            }
        }
    }

    public static void ResetTransform(Component comp)
    {
        ResetTransform(comp.gameObject);
    }

    public static void ResetTransform(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
    }

    public static void SetParent(Component child, Component parent, bool withRotation = false)
    {
        SetParent(child.transform, parent.transform, withRotation);
    }

    public static void SetParent(Component child, GameObject parent, bool withRotation = false)
    {
        SetParent(child.transform, parent.transform, withRotation);
    }

    public static void SetParent(GameObject child, Component parent, bool withRotation = false)
    {
        SetParent(child.transform, parent.transform, withRotation);
    }

    public static void SetParent(GameObject child, GameObject parent, bool withRotation = false)
    {
        SetParent(child.transform, parent.transform, withRotation);
    }

    private static void SetParent(Transform child, Transform parent, bool withRotation)
    {
        Vector3 localScale = child.localScale;
        Quaternion localRotation = child.localRotation;
        child.parent = parent;
        child.localPosition = Vector3.zero;
        child.localScale = localScale;
        if (withRotation)
        {
            child.localRotation = localRotation;
        }
    }

    public static bool ShouldShowRankedMedals()
    {
        return ShouldShowRankedMedals(GameMgr.Get().GetGameType());
    }

    public static bool ShouldShowRankedMedals(GameType gameType)
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            return false;
        }
        return (gameType == GameType.GT_RANKED);
    }

    private static void ShowDetailedCardError(string format, params object[] formatArgs)
    {
        string str = string.Format(format, formatArgs);
        StackTrace trace = new StackTrace();
        MethodBase method = trace.GetFrame(2).GetMethod();
        object[] messageArgs = new object[] { str, method.ReflectedType, method.Name };
        Error.AddDevWarning("Error", "{0}\nCalled by {1}.{2}", messageArgs);
    }

    public static int TranslateCardIdToDbId(string cardId)
    {
        DbfRecord cardRecord = GetCardRecord(cardId);
        if (cardRecord == null)
        {
            object[] formatArgs = new object[] { cardId };
            ShowDetailedCardError("GameUtils.TranslateCardIdToDbId() - There is no card with NOTE_MINI_GUID {0} in the Card DBF.", formatArgs);
            return 0;
        }
        return cardRecord.GetId();
    }

    public static string TranslateDbIdToCardId(int dbId)
    {
        DbfRecord cardRecord = GetCardRecord(dbId);
        if (cardRecord == null)
        {
            object[] formatArgs = new object[] { dbId };
            ShowDetailedCardError("GameUtils.TranslateDbIdToCardId() - Failed to find card with database id {0} in the Card DBF.", formatArgs);
            return null;
        }
        string str = cardRecord.GetString("NOTE_MINI_GUID");
        if (str == null)
        {
            object[] objArray2 = new object[] { dbId };
            ShowDetailedCardError("GameUtils.TranslateDbIdToCardId() - Card with database id {0} has no NOTE_MINI_GUID field in the Card DBF.", objArray2);
            return null;
        }
        return str;
    }

    [CompilerGenerated]
    private sealed class <GetHeroLevel>c__AnonStorey38D
    {
        internal TAG_CLASS heroClass;

        internal bool <>m__289(NetCache.HeroLevel obj)
        {
            return (obj.Class == this.heroClass);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadCardDefEmoteSound>c__AnonStorey38F
    {
        internal GameUtils.EmoteSoundLoaded callback;
        internal EmoteType type;

        internal bool <>m__28B(EmoteEntryDef e)
        {
            return (e.m_emoteType == this.type);
        }

        internal void <>m__28C(string name, GameObject go, object data)
        {
            if (go == null)
            {
                this.callback(null);
            }
            else
            {
                this.callback(go.GetComponent<CardSoundSpell>());
            }
        }
    }

    [CompilerGenerated]
    private sealed class <PlayCardEffectDefSounds>c__AnonStorey38E
    {
        internal GameObject destroyObj;

        internal void <>m__28D(Spell spell, SpellStateType prevStateType, object userData)
        {
            if (spell.GetActiveState() == SpellStateType.NONE)
            {
                UnityEngine.Object.Destroy(this.destroyObj);
            }
        }
    }

    public delegate void EmoteSoundLoaded(CardSoundSpell emoteObj);
}

