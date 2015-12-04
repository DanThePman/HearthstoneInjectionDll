using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class PresenceMgr
{
    [CompilerGenerated]
    private static Func<Enum, bool> <>f__am$cacheA;
    private Map<System.Type, byte> m_enumToIdMap = new Map<System.Type, byte>();
    private Map<byte, System.Type> m_idToEnumMap = new Map<byte, System.Type>();
    private Enum[] m_prevStatus;
    private Enum[] m_richPresence;
    private Enum[] m_status;
    private static readonly Map<KeyValuePair<AdventureDbId, AdventureModeDbId>, PresenceTargets> s_adventurePresenceMap;
    private static readonly System.Type[] s_enumIdList;
    private static PresenceMgr s_instance;
    private static readonly Map<Enum, Enum> s_richPresenceMap;
    private static readonly Map<Enum, string> s_stringKeyMap;

    static PresenceMgr()
    {
        Map<Enum, Enum> map = new Map<Enum, Enum>();
        map.Add(PresenceStatus.LOGIN, 0);
        map.Add(PresenceStatus.WELCOMEQUESTS, PresenceStatus.HUB);
        map.Add(PresenceStatus.STORE, PresenceStatus.HUB);
        map.Add(PresenceStatus.QUESTLOG, PresenceStatus.HUB);
        map.Add(PresenceStatus.PACKOPENING, PresenceStatus.HUB);
        map.Add(PresenceStatus.COLLECTION, PresenceStatus.HUB);
        map.Add(PresenceStatus.DECKEDITOR, PresenceStatus.HUB);
        map.Add(PresenceStatus.CRAFTING, PresenceStatus.HUB);
        map.Add(PresenceStatus.PLAY_DECKPICKER, PresenceStatus.HUB);
        map.Add(PresenceStatus.PLAY_QUEUE, PresenceStatus.HUB);
        map.Add(PresenceStatus.PRACTICE_DECKPICKER, PresenceStatus.HUB);
        map.Add(PresenceStatus.ARENA_PURCHASE, PresenceStatus.HUB);
        map.Add(PresenceStatus.ARENA_FORGE, PresenceStatus.HUB);
        map.Add(PresenceStatus.ARENA_IDLE, PresenceStatus.HUB);
        map.Add(PresenceStatus.ARENA_QUEUE, PresenceStatus.HUB);
        map.Add(PresenceStatus.ARENA_REWARD, PresenceStatus.HUB);
        map.Add(PresenceStatus.FRIENDLY_DECKPICKER, PresenceStatus.HUB);
        map.Add(PresenceStatus.ADVENTURE_CHOOSING_MODE, PresenceStatus.HUB);
        map.Add(PresenceStatus.ADVENTURE_SCENARIO_SELECT, PresenceStatus.HUB);
        map.Add(PresenceStatus.TAVERN_BRAWL_SCREEN, PresenceStatus.HUB);
        map.Add(PresenceStatus.TAVERN_BRAWL_DECKEDITOR, PresenceStatus.HUB);
        map.Add(PresenceStatus.TAVERN_BRAWL_QUEUE, PresenceStatus.HUB);
        map.Add(PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING, PresenceStatus.HUB);
        s_richPresenceMap = map;
        Map<Enum, string> map2 = new Map<Enum, string>();
        map2.Add(PresenceStatus.LOGIN, "PRESENCE_STATUS_LOGIN");
        map2.Add(PresenceStatus.TUTORIAL_PREGAME, "PRESENCE_STATUS_TUTORIAL_PREGAME");
        map2.Add(PresenceStatus.TUTORIAL_GAME, "PRESENCE_STATUS_TUTORIAL_GAME");
        map2.Add(PresenceStatus.WELCOMEQUESTS, "PRESENCE_STATUS_WELCOMEQUESTS");
        map2.Add(PresenceStatus.HUB, "PRESENCE_STATUS_HUB");
        map2.Add(PresenceStatus.STORE, "PRESENCE_STATUS_STORE");
        map2.Add(PresenceStatus.QUESTLOG, "PRESENCE_STATUS_QUESTLOG");
        map2.Add(PresenceStatus.PACKOPENING, "PRESENCE_STATUS_PACKOPENING");
        map2.Add(PresenceStatus.COLLECTION, "PRESENCE_STATUS_COLLECTION");
        map2.Add(PresenceStatus.DECKEDITOR, "PRESENCE_STATUS_DECKEDITOR");
        map2.Add(PresenceStatus.CRAFTING, "PRESENCE_STATUS_CRAFTING");
        map2.Add(PresenceStatus.PLAY_DECKPICKER, "PRESENCE_STATUS_PLAY_DECKPICKER");
        map2.Add(PresenceStatus.PLAY_QUEUE, "PRESENCE_STATUS_PLAY_QUEUE");
        map2.Add(PresenceStatus.PLAY_GAME, "PRESENCE_STATUS_PLAY_GAME");
        map2.Add(PresenceStatus.PRACTICE_DECKPICKER, "PRESENCE_STATUS_PRACTICE_DECKPICKER");
        map2.Add(PresenceStatus.PRACTICE_GAME, "PRESENCE_STATUS_PRACTICE_GAME");
        map2.Add(PresenceStatus.ARENA_PURCHASE, "PRESENCE_STATUS_ARENA_PURCHASE");
        map2.Add(PresenceStatus.ARENA_FORGE, "PRESENCE_STATUS_ARENA_FORGE");
        map2.Add(PresenceStatus.ARENA_IDLE, "PRESENCE_STATUS_ARENA_IDLE");
        map2.Add(PresenceStatus.ARENA_QUEUE, "PRESENCE_STATUS_ARENA_QUEUE");
        map2.Add(PresenceStatus.ARENA_GAME, "PRESENCE_STATUS_ARENA_GAME");
        map2.Add(PresenceStatus.ARENA_REWARD, "PRESENCE_STATUS_ARENA_REWARD");
        map2.Add(PresenceStatus.FRIENDLY_DECKPICKER, "PRESENCE_STATUS_FRIENDLY_DECKPICKER");
        map2.Add(PresenceStatus.FRIENDLY_GAME, "PRESENCE_STATUS_FRIENDLY_GAME");
        map2.Add(PresenceStatus.ADVENTURE_CHOOSING_MODE, "PRESENCE_STATUS_ADVENTURE_CHOOSING_MODE");
        map2.Add(PresenceStatus.ADVENTURE_SCENARIO_SELECT, "PRESENCE_STATUS_ADVENTURE_SCENARIO_SELECT");
        map2.Add(PresenceStatus.ADVENTURE_SCENARIO_PLAYING_GAME, "PRESENCE_STATUS_ADVENTURE_SCENARIO_PLAYING_GAME");
        map2.Add(PresenceStatus.TAVERN_BRAWL_SCREEN, "PRESENCE_STATUS_TAVERN_BRAWL_SCREEN");
        map2.Add(PresenceStatus.TAVERN_BRAWL_DECKEDITOR, "PRESENCE_STATUS_TAVERN_BRAWL_DECKEDITOR");
        map2.Add(PresenceStatus.TAVERN_BRAWL_QUEUE, "PRESENCE_STATUS_TAVERN_BRAWL_QUEUE");
        map2.Add(PresenceStatus.TAVERN_BRAWL_GAME, "PRESENCE_STATUS_TAVERN_BRAWL_GAME");
        map2.Add(PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING, "PRESENCE_STATUS_TAVERN_BRAWL_FRIENDLY_WAITING");
        map2.Add(PresenceStatus.TAVERN_BRAWL_FRIENDLY_GAME, "PRESENCE_STATUS_TAVERN_BRAWL_FRIENDLY_GAME");
        map2.Add(PresenceStatus.SPECTATING_GAME_TUTORIAL, "PRESENCE_STATUS_SPECTATING_GAME_TUTORIAL");
        map2.Add(PresenceStatus.SPECTATING_GAME_PRACTICE, "PRESENCE_STATUS_SPECTATING_GAME_PRACTICE");
        map2.Add(PresenceStatus.SPECTATING_GAME_PLAY, "PRESENCE_STATUS_SPECTATING_GAME_PLAY");
        map2.Add(PresenceStatus.SPECTATING_GAME_ARENA, "PRESENCE_STATUS_SPECTATING_GAME_ARENA");
        map2.Add(PresenceStatus.SPECTATING_GAME_FRIENDLY, "PRESENCE_STATUS_SPECTATING_GAME_FRIENDLY");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_NORMAL, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_NAXX_NORMAL");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_HEROIC, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_NAXX_HEROIC");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_CLASS_CHALLENGE, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_NAXX_CLASS_CHALLENGE");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_NORMAL, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_BRM_NORMAL");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_HEROIC, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_BRM_HEROIC");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_CLASS_CHALLENGE, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_BRM_CLASS_CHALLENGE");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_NORMAL, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_LOE_NORMAL");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_HEROIC, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_LOE_HEROIC");
        map2.Add(PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_CLASS_CHALLENGE, "PRESENCE_STATUS_SPECTATING_GAME_ADVENTURE_LOE_CLASS_CHALLENGE");
        map2.Add(PresenceStatus.SPECTATING_GAME_TAVERN_BRAWL, "PRESENCE_STATUS_SPECTATING_GAME_TAVERN_BRAWL");
        map2.Add(PresenceTutorial.HOGGER, "PRESENCE_TUTORIAL_HOGGER");
        map2.Add(PresenceTutorial.MILLHOUSE, "PRESENCE_TUTORIAL_MILLHOUSE");
        map2.Add(PresenceTutorial.MUKLA, "PRESENCE_TUTORIAL_MUKLA");
        map2.Add(PresenceTutorial.HEMET, "PRESENCE_TUTORIAL_HEMET");
        map2.Add(PresenceTutorial.ILLIDAN, "PRESENCE_TUTORIAL_ILLIDAN");
        map2.Add(PresenceTutorial.CHO, "PRESENCE_TUTORIAL_CHO");
        map2.Add(PresenceAdventureMode.NAXX_NORMAL, "PRESENCE_ADVENTURE_MODE_NAXX_NORMAL");
        map2.Add(PresenceAdventureMode.NAXX_HEROIC, "PRESENCE_ADVENTURE_MODE_NAXX_HEROIC");
        map2.Add(PresenceAdventureMode.NAXX_CLASS_CHALLENGE, "PRESENCE_ADVENTURE_MODE_NAXX_CLASS_CHALLENGE");
        map2.Add(PresenceAdventureMode.BRM_NORMAL, "PRESENCE_ADVENTURE_MODE_BRM_NORMAL");
        map2.Add(PresenceAdventureMode.BRM_HEROIC, "PRESENCE_ADVENTURE_MODE_BRM_HEROIC");
        map2.Add(PresenceAdventureMode.BRM_CLASS_CHALLENGE, "PRESENCE_ADVENTURE_MODE_BRM_CLASS_CHALLENGE");
        map2.Add(PresenceAdventureMode.LOE_NORMAL, "PRESENCE_ADVENTURE_MODE_LOE_NORMAL");
        map2.Add(PresenceAdventureMode.LOE_HEROIC, "PRESENCE_ADVENTURE_MODE_LOE_HEROIC");
        map2.Add(PresenceAdventureMode.LOE_CLASS_CHALLENGE, "PRESENCE_ADVENTURE_MODE_LOE_CLASS_CHALLENGE");
        map2.Add(ScenarioDbId.NAXX_ANUBREKHAN, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_ANUBREKHAN");
        map2.Add(ScenarioDbId.NAXX_FAERLINA, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_FAERLINA");
        map2.Add(ScenarioDbId.NAXX_MAEXXNA, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_MAEXXNA");
        map2.Add(ScenarioDbId.NAXX_NOTH, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_NOTH");
        map2.Add(ScenarioDbId.NAXX_HEIGAN, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_HEIGAN");
        map2.Add(ScenarioDbId.NAXX_LOATHEB, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_LOATHEB");
        map2.Add(ScenarioDbId.NAXX_RAZUVIOUS, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_RAZUVIOUS");
        map2.Add(ScenarioDbId.NAXX_GOTHIK, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_GOTHIK");
        map2.Add(ScenarioDbId.NAXX_HORSEMEN, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_HORSEMEN");
        map2.Add(ScenarioDbId.NAXX_PATCHWERK, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_PATCHWERK");
        map2.Add(ScenarioDbId.NAXX_GROBBULUS, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_GROBBULUS");
        map2.Add(ScenarioDbId.NAXX_GLUTH, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_GLUTH");
        map2.Add(ScenarioDbId.NAXX_THADDIUS, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_THADDIUS");
        map2.Add(ScenarioDbId.NAXX_SAPPHIRON, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_SAPPHIRON");
        map2.Add(ScenarioDbId.NAXX_KELTHUZAD, "PRESENCE_SCENARIO_NAXX_NORMAL_SCENARIO_KELTHUZAD");
        map2.Add(ScenarioDbId.NAXX_HEROIC_ANUBREKHAN, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_ANUBREKHAN");
        map2.Add(ScenarioDbId.NAXX_HEROIC_FAERLINA, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_FAERLINA");
        map2.Add(ScenarioDbId.NAXX_HEROIC_MAEXXNA, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_MAEXXNA");
        map2.Add(ScenarioDbId.NAXX_HEROIC_NOTH, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_NOTH");
        map2.Add(ScenarioDbId.NAXX_HEROIC_HEIGAN, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_HEIGAN");
        map2.Add(ScenarioDbId.NAXX_HEROIC_LOATHEB, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_LOATHEB");
        map2.Add(ScenarioDbId.NAXX_HEROIC_RAZUVIOUS, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_RAZUVIOUS");
        map2.Add(ScenarioDbId.NAXX_HEROIC_GOTHIK, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_GOTHIK");
        map2.Add(ScenarioDbId.NAXX_HEROIC_HORSEMEN, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_HORSEMEN");
        map2.Add(ScenarioDbId.NAXX_HEROIC_PATCHWERK, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_PATCHWERK");
        map2.Add(ScenarioDbId.NAXX_HEROIC_GROBBULUS, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_GROBBULUS");
        map2.Add(ScenarioDbId.NAXX_HEROIC_GLUTH, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_GLUTH");
        map2.Add(ScenarioDbId.NAXX_HEROIC_THADDIUS, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_THADDIUS");
        map2.Add(ScenarioDbId.NAXX_HEROIC_SAPPHIRON, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_SAPPHIRON");
        map2.Add(ScenarioDbId.NAXX_HEROIC_KELTHUZAD, "PRESENCE_SCENARIO_NAXX_HEROIC_SCENARIO_KELTHUZAD");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_HUNTER_V_LOATHEB, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_HUNTER");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_WARRIOR_V_GROBBULUS, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_WARRIOR");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_ROGUE_V_MAEXXNA, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_ROGUE");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_DRUID_V_FAERLINA, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_DRUID");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_PRIEST_V_THADDIUS, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_PRIEST");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_SHAMAN_V_GOTHIK, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_SHAMAN");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_MAGE_V_HEIGAN, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_MAGE");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_PALADIN_V_KELTHUZAD, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_PALADIN");
        map2.Add(ScenarioDbId.NAXX_CHALLENGE_WARLOCK_V_HORSEMEN, "PRESENCE_SCENARIO_NAXX_CLASS_CHALLENGE_WARLOCK");
        map2.Add(ScenarioDbId.BRM_GRIM_GUZZLER, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_GRIM_GUZZLER");
        map2.Add(ScenarioDbId.BRM_DARK_IRON_ARENA, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_DARK_IRON_ARENA");
        map2.Add(ScenarioDbId.BRM_THAURISSAN, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_THAURISSAN");
        map2.Add(ScenarioDbId.BRM_GARR, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_GARR");
        map2.Add(ScenarioDbId.BRM_MAJORDOMO, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_MAJORDOMO");
        map2.Add(ScenarioDbId.BRM_BARON_GEDDON, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_BARON_GEDDON");
        map2.Add(ScenarioDbId.BRM_OMOKK, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_OMOKK");
        map2.Add(ScenarioDbId.BRM_DRAKKISATH, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_DRAKKISATH");
        map2.Add(ScenarioDbId.BRM_REND_BLACKHAND, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_REND_BLACKHAND");
        map2.Add(ScenarioDbId.BRM_RAZORGORE, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_RAZORGORE");
        map2.Add(ScenarioDbId.BRM_VAELASTRASZ, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_VAELASTRASZ");
        map2.Add(ScenarioDbId.BRM_CHROMAGGUS, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_CHROMAGGUS");
        map2.Add(ScenarioDbId.BRM_NEFARIAN, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_NEFARIAN");
        map2.Add(ScenarioDbId.BRM_OMNOTRON, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_OMNOTRON");
        map2.Add(ScenarioDbId.BRM_MALORIAK, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_MALORIAK");
        map2.Add(ScenarioDbId.BRM_ATRAMEDES, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_ATRAMEDES");
        map2.Add(ScenarioDbId.BRM_ZOMBIE_NEF, "PRESENCE_SCENARIO_BRM_NORMAL_SCENARIO_ZOMBIE_NEF");
        map2.Add(ScenarioDbId.BRM_HEROIC_GRIM_GUZZLER, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_GRIM_GUZZLER");
        map2.Add(ScenarioDbId.BRM_HEROIC_DARK_IRON_ARENA, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_DARK_IRON_ARENA");
        map2.Add(ScenarioDbId.BRM_HEROIC_THAURISSAN, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_THAURISSAN");
        map2.Add(ScenarioDbId.BRM_HEROIC_GARR, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_GARR");
        map2.Add(ScenarioDbId.BRM_HEROIC_MAJORDOMO, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_MAJORDOMO");
        map2.Add(ScenarioDbId.BRM_HEROIC_BARON_GEDDON, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_BARON_GEDDON");
        map2.Add(ScenarioDbId.BRM_HEROIC_OMOKK, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_OMOKK");
        map2.Add(ScenarioDbId.BRM_HEROIC_DRAKKISATH, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_DRAKKISATH");
        map2.Add(ScenarioDbId.BRM_HEROIC_REND_BLACKHAND, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_REND_BLACKHAND");
        map2.Add(ScenarioDbId.BRM_HEROIC_RAZORGORE, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_RAZORGORE");
        map2.Add(ScenarioDbId.BRM_HEROIC_VAELASTRASZ, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_VAELASTRASZ");
        map2.Add(ScenarioDbId.BRM_HEROIC_CHROMAGGUS, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_CHROMAGGUS");
        map2.Add(ScenarioDbId.BRM_HEROIC_NEFARIAN, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_NEFARIAN");
        map2.Add(ScenarioDbId.BRM_HEROIC_OMNOTRON, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_OMNOTRON");
        map2.Add(ScenarioDbId.BRM_HEROIC_MALORIAK, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_MALORIAK");
        map2.Add(ScenarioDbId.BRM_HEROIC_ATRAMEDES, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_ATRAMEDES");
        map2.Add(ScenarioDbId.BRM_HEROIC_ZOMBIE_NEF, "PRESENCE_SCENARIO_BRM_HEROIC_SCENARIO_ZOMBIE_NEF");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_HUNTER_V_GUZZLER, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_HUNTER");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_WARRIOR_V_GARR, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_WARRIOR");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_ROGUE_V_VAELASTRASZ, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_ROGUE");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_DRUID_V_BLACKHAND, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_DRUID");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_PRIEST_V_DRAKKISATH, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_PRIEST");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_SHAMAN_V_GEDDON, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_SHAMAN");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_MAGE_V_DARK_IRON_ARENA, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_MAGE");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_PALADIN_V_OMNOTRON, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_PALADIN");
        map2.Add(ScenarioDbId.BRM_CHALLENGE_WARLOCK_V_RAZORGORE, "PRESENCE_SCENARIO_BRM_CLASS_CHALLENGE_WARLOCK");
        map2.Add(ScenarioDbId.LOE_ZINAAR, "PRESENCE_SCENARIO_LOE_NORMAL_ZINAAR");
        map2.Add(ScenarioDbId.LOE_SUN_RAIDER_PHAERIX, "PRESENCE_SCENARIO_LOE_NORMAL_SUN_RAIDER_PHAERIX");
        map2.Add(ScenarioDbId.LOE_TEMPLE_ESCAPE, "PRESENCE_SCENARIO_LOE_NORMAL_TEMPLE_ESCAPE");
        map2.Add(ScenarioDbId.LOE_SCARVASH, "PRESENCE_SCENARIO_LOE_NORMAL_SCARVASH");
        map2.Add(ScenarioDbId.LOE_MINE_CART, "PRESENCE_SCENARIO_LOE_NORMAL_MINE_CART");
        map2.Add(ScenarioDbId.LOE_ARCHAEDAS, "PRESENCE_SCENARIO_LOE_NORMAL_ARCHAEDAS");
        map2.Add(ScenarioDbId.LOE_SLITHERSPEAR, "PRESENCE_SCENARIO_LOE_NORMAL_SLITHERSPEAR");
        map2.Add(ScenarioDbId.LOE_GIANTFIN, "PRESENCE_SCENARIO_LOE_NORMAL_GIANTFIN");
        map2.Add(ScenarioDbId.LOE_LADY_NAZJAR, "PRESENCE_SCENARIO_LOE_NORMAL_LADY_NAZJAR");
        map2.Add(ScenarioDbId.LOE_SKELESAURUS, "PRESENCE_SCENARIO_LOE_NORMAL_SKELESAURUS");
        map2.Add(ScenarioDbId.LOE_STEEL_SENTINEL, "PRESENCE_SCENARIO_LOE_NORMAL_STEEL_SENTINEL");
        map2.Add(ScenarioDbId.LOE_RAFAAM_1, "PRESENCE_SCENARIO_LOE_NORMAL_RAFAAM_1");
        map2.Add(ScenarioDbId.LOE_RAFAAM_2, "PRESENCE_SCENARIO_LOE_NORMAL_RAFAAM_2");
        map2.Add(ScenarioDbId.LOE_HEROIC_ZINAAR, "PRESENCE_SCENARIO_LOE_HEROIC_ZINAAR");
        map2.Add(ScenarioDbId.LOE_HEROIC_SUN_RAIDER_PHAERIX, "PRESENCE_SCENARIO_LOE_HEROIC_SUN_RAIDER_PHAERIX");
        map2.Add(ScenarioDbId.LOE_HEROIC_TEMPLE_ESCAPE, "PRESENCE_SCENARIO_LOE_HEROIC_TEMPLE_ESCAPE");
        map2.Add(ScenarioDbId.LOE_HEROIC_SCARVASH, "PRESENCE_SCENARIO_LOE_HEROIC_SCARVASH");
        map2.Add(ScenarioDbId.LOE_HEROIC_MINE_CART, "PRESENCE_SCENARIO_LOE_HEROIC_MINE_CART");
        map2.Add(ScenarioDbId.LOE_HEROIC_ARCHAEDAS, "PRESENCE_SCENARIO_LOE_HEROIC_ARCHAEDAS");
        map2.Add(ScenarioDbId.LOE_HEROIC_SLITHERSPEAR, "PRESENCE_SCENARIO_LOE_HEROIC_SLITHERSPEAR");
        map2.Add(ScenarioDbId.LOE_HEROIC_GIANTFIN, "PRESENCE_SCENARIO_LOE_HEROIC_GIANTFIN");
        map2.Add(ScenarioDbId.LOE_HEROIC_LADY_NAZJAR, "PRESENCE_SCENARIO_LOE_HEROIC_LADY_NAZJAR");
        map2.Add(ScenarioDbId.LOE_HEROIC_SKELESAURUS, "PRESENCE_SCENARIO_LOE_HEROIC_SKELESAURUS");
        map2.Add(ScenarioDbId.LOE_HEROIC_STEEL_SENTINEL, "PRESENCE_SCENARIO_LOE_HEROIC_STEEL_SENTINEL");
        map2.Add(ScenarioDbId.LOE_HEROIC_RAFAAM_1, "PRESENCE_SCENARIO_LOE_HEROIC_RAFAAM_1");
        map2.Add(ScenarioDbId.LOE_HEROIC_RAFAAM_2, "PRESENCE_SCENARIO_LOE_HEROIC_RAFAAM_2");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_WARRIOR_V_ZINAAR, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_WARRIOR");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_WARLOCK_V_SUN_RAIDER, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_WARLOCK");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_DRUID_V_SCARVASH, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_DRUID");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_PALADIN_V_ARCHAEDUS, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_PALADIN");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_HUNTER_V_SLITHERSPEAR, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_HUNTER");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_SHAMAN_V_GIANTFIN, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_SHAMAN");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_PRIEST_V_NAZJAR, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_PRIEST");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_ROGUE_V_SKELESAURUS, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_ROGUE");
        map2.Add(ScenarioDbId.LOE_CHALLENGE_MAGE_V_SENTINEL, "PRESENCE_SCENARIO_LOE_CLASS_CHALLENGE_MAGE");
        s_stringKeyMap = map2;
        Map<KeyValuePair<AdventureDbId, AdventureModeDbId>, PresenceTargets> map3 = new Map<KeyValuePair<AdventureDbId, AdventureModeDbId>, PresenceTargets>();
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.NAXXRAMAS, AdventureModeDbId.NORMAL), new PresenceTargets(PresenceAdventureMode.NAXX_NORMAL, PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_NORMAL));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.NAXXRAMAS, AdventureModeDbId.HEROIC), new PresenceTargets(PresenceAdventureMode.NAXX_HEROIC, PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_HEROIC));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.NAXXRAMAS, AdventureModeDbId.CLASS_CHALLENGE), new PresenceTargets(PresenceAdventureMode.NAXX_CLASS_CHALLENGE, PresenceStatus.SPECTATING_GAME_ADVENTURE_NAXX_CLASS_CHALLENGE));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.BRM, AdventureModeDbId.NORMAL), new PresenceTargets(PresenceAdventureMode.BRM_NORMAL, PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_NORMAL));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.BRM, AdventureModeDbId.HEROIC), new PresenceTargets(PresenceAdventureMode.BRM_HEROIC, PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_HEROIC));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.BRM, AdventureModeDbId.CLASS_CHALLENGE), new PresenceTargets(PresenceAdventureMode.BRM_CLASS_CHALLENGE, PresenceStatus.SPECTATING_GAME_ADVENTURE_BRM_CLASS_CHALLENGE));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.LOE, AdventureModeDbId.NORMAL), new PresenceTargets(PresenceAdventureMode.LOE_NORMAL, PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_NORMAL));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.LOE, AdventureModeDbId.HEROIC), new PresenceTargets(PresenceAdventureMode.LOE_HEROIC, PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_HEROIC));
        map3.Add(new KeyValuePair<AdventureDbId, AdventureModeDbId>(AdventureDbId.LOE, AdventureModeDbId.CLASS_CHALLENGE), new PresenceTargets(PresenceAdventureMode.LOE_CLASS_CHALLENGE, PresenceStatus.SPECTATING_GAME_ADVENTURE_LOE_CLASS_CHALLENGE));
        s_adventurePresenceMap = map3;
        s_enumIdList = new System.Type[] { typeof(PresenceStatus), typeof(PresenceTutorial), typeof(PresenceAdventureMode), typeof(ScenarioDbId) };
    }

    private bool DecodeStatusVal(BinaryReader reader, ref Enum enumVal, ref string key)
    {
        System.Type type;
        key = null;
        byte num = 0;
        int num2 = 0;
        int position = (int) reader.BaseStream.Position;
        int num4 = position + 1;
        try
        {
            num = reader.ReadByte();
            num4 = (int) reader.BaseStream.Position;
        }
        catch (Exception exception)
        {
            object[] args = new object[] { num, position, exception.GetType().FullName, exception.Message };
            Log.Henry.Print("PresenceMgr.DecodeStatusVal - unable to decode enum id {0} at index {1} : {2} {3}", args);
            return false;
        }
        if (!this.m_idToEnumMap.TryGetValue(num, out type))
        {
            object[] objArray2 = new object[] { num, position };
            Log.Henry.Print("PresenceMgr.DecodeStatusVal - id {0} at index {1}, has no enum type", objArray2);
            return false;
        }
        try
        {
            num2 = reader.ReadInt32();
        }
        catch (Exception exception2)
        {
            object[] objArray3 = new object[] { num, num4, exception2.GetType().FullName, exception2.Message };
            Log.Henry.Print("PresenceMgr.DecodeStatusVal - unable to decode enum value {0} at index {1} : {2} {3}", objArray3);
            return false;
        }
        if (type == typeof(PresenceStatus))
        {
            PresenceStatus status = (PresenceStatus) num2;
            enumVal = status;
            if (!s_stringKeyMap.TryGetValue(status, out key))
            {
                object[] objArray4 = new object[] { type, status, num4 };
                Log.Henry.Print("PresenceMgr.DecodeStatusVal - value {0}.{1} at index {2}, has no string", objArray4);
                return false;
            }
        }
        else if (type == typeof(PresenceTutorial))
        {
            PresenceTutorial tutorial = (PresenceTutorial) num2;
            enumVal = tutorial;
            if (!s_stringKeyMap.TryGetValue(tutorial, out key))
            {
                object[] objArray5 = new object[] { type, tutorial, num4 };
                Log.Henry.Print("PresenceMgr.DecodeStatusVal - value {0}.{1} at index {2}, has no string", objArray5);
                return false;
            }
        }
        else if (type == typeof(PresenceAdventureMode))
        {
            PresenceAdventureMode mode = (PresenceAdventureMode) num2;
            enumVal = mode;
            if (!s_stringKeyMap.TryGetValue(mode, out key))
            {
                object[] objArray6 = new object[] { type, mode, num4 };
                Log.Henry.Print("PresenceMgr.DecodeStatusVal - value {0}.{1} at index {2}, has no string", objArray6);
                return false;
            }
        }
        else if (type == typeof(ScenarioDbId))
        {
            ScenarioDbId id = (ScenarioDbId) num2;
            enumVal = id;
            if (!s_stringKeyMap.TryGetValue(id, out key))
            {
                object[] objArray7 = new object[] { type, id, num4 };
                Log.Henry.Print("PresenceMgr.DecodeStatusVal - value {0}.{1} at index {2}, has no string", objArray7);
                return false;
            }
        }
        return true;
    }

    private bool EncodeStatusVal(Enum[] status, int index, out byte id, out int intVal)
    {
        Enum enum2 = status[index];
        System.Type key = enum2.GetType();
        intVal = Convert.ToInt32(enum2);
        if (!this.m_enumToIdMap.TryGetValue(key, out id))
        {
            object[] messageArgs = new object[] { enum2, index, key };
            Error.AddDevFatal("PresenceMgr.EncodeStatusVal() - {0} at index {1} belongs to type {2}, which has no id", messageArgs);
            return false;
        }
        return true;
    }

    public static PresenceMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new PresenceMgr();
            s_instance.Initialize();
        }
        return s_instance;
    }

    public static Map<Enum, string> GetEnumStringMap()
    {
        return s_stringKeyMap;
    }

    public Enum[] GetPrevStatus()
    {
        return this.m_prevStatus;
    }

    public Enum[] GetStatus()
    {
        return this.m_status;
    }

    public PresenceStatus GetStatus(BnetPlayer player)
    {
        string statusKey = null;
        return this.GetStatus_Internal(player, ref statusKey, null, null);
    }

    private PresenceStatus GetStatus_Internal(BnetPlayer player, ref string statusKey, List<string> stringArgs = null, List<Enum> enumVals = null)
    {
        byte[] buffer;
        PresenceStatus uNKNOWN = PresenceStatus.UNKNOWN;
        if (player == null)
        {
            return uNKNOWN;
        }
        if (player.GetBestGameAccount() == null)
        {
            return uNKNOWN;
        }
        BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
        if (hearthstoneGameAccount == null)
        {
            return uNKNOWN;
        }
        if (!hearthstoneGameAccount.TryGetGameFieldBytes(0x11, out buffer))
        {
            return uNKNOWN;
        }
        Enum enumVal = 0;
        using (MemoryStream stream = new MemoryStream(buffer))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (this.DecodeStatusVal(reader, ref enumVal, ref statusKey))
                {
                    uNKNOWN = (PresenceStatus) ((int) enumVal);
                    if (enumVals != null)
                    {
                        enumVals.Add(uNKNOWN);
                    }
                    if ((stringArgs == null) && (enumVals == null))
                    {
                        return uNKNOWN;
                    }
                    while (stream.Position < buffer.Length)
                    {
                        string key = null;
                        if (!this.DecodeStatusVal(reader, ref enumVal, ref key))
                        {
                            return uNKNOWN;
                        }
                        if (enumVals != null)
                        {
                            enumVals.Add(enumVal);
                        }
                        if (stringArgs != null)
                        {
                            string item = GameStrings.Get(key);
                            stringArgs.Add(item);
                        }
                    }
                }
                return uNKNOWN;
            }
        }
    }

    public Enum[] GetStatusEnums(BnetPlayer player)
    {
        string statusKey = null;
        List<Enum> enumVals = new List<Enum>();
        this.GetStatus_Internal(player, ref statusKey, null, enumVals);
        return enumVals.ToArray();
    }

    public string GetStatusText(BnetPlayer player)
    {
        List<string> stringArgs = new List<string>();
        string statusKey = null;
        if (this.GetStatus_Internal(player, ref statusKey, stringArgs, null) == PresenceStatus.UNKNOWN)
        {
            BnetGameAccount bestGameAccount = player.GetBestGameAccount();
            return ((bestGameAccount != null) ? bestGameAccount.GetRichPresence() : null);
        }
        string[] args = stringArgs.ToArray();
        return GameStrings.Format(statusKey, args);
    }

    private void Initialize()
    {
        for (int i = 0; i < s_enumIdList.Length; i++)
        {
            System.Type enumType = s_enumIdList[i];
            if (Enum.GetUnderlyingType(enumType) != typeof(int))
            {
                throw new Exception(string.Format("Underlying type of enum {0} (underlying={1}) must {2} be to used by Presence system.", enumType.FullName, Enum.GetUnderlyingType(enumType).FullName, typeof(int).Name));
            }
            byte num2 = (byte) (i + 1);
            this.m_enumToIdMap.Add(enumType, num2);
            this.m_idToEnumMap.Add(num2, enumType);
        }
    }

    public bool IsRichPresence(params Enum[] status)
    {
        if (status == null)
        {
            return false;
        }
        if (status.Length == 0)
        {
            return false;
        }
        for (int i = 0; i < status.Length; i++)
        {
            Enum key = status[i];
            if (key == 0)
            {
                return false;
            }
            if (s_richPresenceMap.ContainsKey(key))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsStatusPlayingGame(PresenceStatus status)
    {
        PresenceStatus status2 = status;
        switch (status2)
        {
            case PresenceStatus.ARENA_GAME:
            case PresenceStatus.FRIENDLY_GAME:
            case PresenceStatus.PLAY_GAME:
            case PresenceStatus.PRACTICE_GAME:
            case PresenceStatus.TAVERN_BRAWL_GAME:
            case PresenceStatus.TAVERN_BRAWL_FRIENDLY_GAME:
                break;

            default:
                if ((status2 != PresenceStatus.TUTORIAL_GAME) && (status2 != PresenceStatus.ADVENTURE_SCENARIO_PLAYING_GAME))
                {
                    return false;
                }
                break;
        }
        return true;
    }

    private bool SetGamePresence(Enum[] status)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < status.Length; i++)
                {
                    byte num2;
                    int num3;
                    if (!this.EncodeStatusVal(status, i, out num2, out num3))
                    {
                        return false;
                    }
                    writer.Write(num2);
                    writer.Write(num3);
                }
                byte[] sourceArray = stream.GetBuffer();
                byte[] destinationArray = new byte[stream.Position];
                Array.Copy(sourceArray, destinationArray, destinationArray.Length);
                return BnetPresenceMgr.Get().SetGameField(0x11, destinationArray);
            }
        }
    }

    public bool SetPrevStatus()
    {
        return this.SetStatusImpl(this.m_prevStatus);
    }

    private bool SetRichPresence(Enum[] status)
    {
        Enum[] enumArray = new Enum[status.Length];
        for (int i = 0; i < status.Length; i++)
        {
            Enum enum3;
            Enum key = status[i];
            if (s_richPresenceMap.TryGetValue(key, out enum3))
            {
                if (enum3 == 0)
                {
                    return false;
                }
            }
            else
            {
                enum3 = key;
            }
            enumArray[i] = enum3;
        }
        if (<>f__am$cacheA == null)
        {
            <>f__am$cacheA = e => !RichPresence.s_streamIds.ContainsKey(e.GetType());
        }
        if (Enumerable.Any<Enum>(enumArray, <>f__am$cacheA))
        {
            enumArray = new Enum[] { enumArray[0] };
        }
        if (GeneralUtils.AreArraysEqual<Enum>(this.m_richPresence, enumArray))
        {
            return true;
        }
        this.m_richPresence = enumArray;
        return BnetPresenceMgr.Get().SetRichPresence(enumArray);
    }

    public bool SetStatus(params Enum[] args)
    {
        return this.SetStatusImpl(args);
    }

    public bool SetStatus_EnteringAdventure(AdventureDbId adventureId, AdventureModeDbId adventureModeId)
    {
        PresenceTargets targets;
        KeyValuePair<AdventureDbId, AdventureModeDbId> key = new KeyValuePair<AdventureDbId, AdventureModeDbId>(adventureId, adventureModeId);
        if (s_adventurePresenceMap.TryGetValue(key, out targets))
        {
            Enum[] args = new Enum[] { PresenceStatus.ADVENTURE_SCENARIO_SELECT, targets.EnteringAdventureValue };
            this.SetStatus(args);
            return true;
        }
        return false;
    }

    public bool SetStatus_PlayingMission(ScenarioDbId missionId)
    {
        if (s_stringKeyMap.ContainsKey(missionId))
        {
            Enum[] args = new Enum[] { PresenceStatus.ADVENTURE_SCENARIO_PLAYING_GAME, missionId };
            return this.SetStatus(args);
        }
        return false;
    }

    public bool SetStatus_SpectatingMission(ScenarioDbId missionId)
    {
        PresenceTargets targets;
        AdventureDbId adventureId = GameUtils.GetAdventureId((int) missionId);
        AdventureModeDbId adventureModeId = GameUtils.GetAdventureModeId((int) missionId);
        KeyValuePair<AdventureDbId, AdventureModeDbId> key = new KeyValuePair<AdventureDbId, AdventureModeDbId>(adventureId, adventureModeId);
        if (s_adventurePresenceMap.TryGetValue(key, out targets))
        {
            Enum[] args = new Enum[] { targets.SpectatingValue };
            return this.SetStatus(args);
        }
        return false;
    }

    private bool SetStatusImpl(Enum[] status)
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return false;
        }
        if (Network.IsLoggedIn())
        {
            if ((status == null) || (status.Length == 0))
            {
                Error.AddDevFatal("PresenceMgr.SetStatusImpl() - Received status of length 0. Setting empty status is not supported.", new object[0]);
                return false;
            }
            if (!GeneralUtils.AreArraysEqual<Enum>(this.m_status, status))
            {
                if (!this.SetRichPresence(status))
                {
                    return false;
                }
                if (!this.SetGamePresence(status))
                {
                    return false;
                }
                this.m_prevStatus = this.m_status;
                this.m_status = new Enum[status.Length];
                Array.Copy(status, this.m_status, status.Length);
                if (!IsStatusPlayingGame((PresenceStatus) status[0]) && Network.ShouldBeConnectedToAurora())
                {
                    BnetPresenceMgr.Get().SetGameFieldBlob(0x15, null);
                }
            }
        }
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PresenceTargets
    {
        public PresenceAdventureMode EnteringAdventureValue;
        public PresenceStatus SpectatingValue;
        public PresenceTargets(PresenceAdventureMode enteringAdventureValue, PresenceStatus spectatingValue)
        {
            this.EnteringAdventureValue = enteringAdventureValue;
            this.SpectatingValue = spectatingValue;
        }
    }
}

