using System;
using System.ComponentModel;

public enum Option
{
    [Description("aimode")]
    AI_MODE = 0x3d,
    [Description("backgroundsound")]
    BACKGROUND_SOUND = 0x15,
    [Description("bundleJustPurchaseInHub")]
    BUNDLE_JUST_PURCHASE_IN_HUB = 0x7f,
    [Description("cardback")]
    CARD_BACK = 0x1d,
    [Description("cardback2")]
    CARD_BACK2 = 30,
    [Description("changedcardsdata")]
    CHANGED_CARDS_DATA = 0x25,
    [Description("cheatHistory")]
    CHEAT_HISTORY = 0x34,
    [Description("clientOptionsVersion")]
    CLIENT_OPTIONS_VERSION = 1,
    [Description("collectionPremiumType")]
    COLLECTION_PREMIUM_TYPE = 0x37,
    [Description("connecttobnet")]
    CONNECT_TO_AURORA = 0x20,
    [Description("covermouseovers")]
    COVER_MOUSE_OVERS = 60,
    [Description("cursor")]
    CURSOR = 4,
    [Description("deckpickermode")]
    DECK_PICKER_MODE = 0x43,
    [Description("devTimescale")]
    DEV_TIMESCALE = 0x38,
    [Description("errorScreen")]
    ERROR_SCREEN = 0x2f,
    [Description("fakepackcount")]
    FAKE_PACK_COUNT = 15,
    [Description("fakepackopening")]
    FAKE_PACK_OPENING = 14,
    [Description("friendslistcurrentgamesectionhide")]
    FRIENDS_LIST_CURRENTGAME_SECTION_HIDE = 0x77,
    [Description("friendslistfriendsectionhide")]
    FRIENDS_LIST_FRIEND_SECTION_HIDE = 120,
    [Description("friendslistnearbysectionhide")]
    FRIENDS_LIST_NEARBYPLAYER_SECTION_HIDE = 0x79,
    [Description("friendslistrecruitsectionhide")]
    FRIENDS_LIST_RECRUIT_SECTION_HIDE = 0x7a,
    [Description("friendslistrequestsectionhide")]
    FRIENDS_LIST_REQUEST_SECTION_HIDE = 0x76,
    [Description("graphicsfullscreen")]
    GFX_FULLSCREEN = 11,
    [Description("fxaa")]
    GFX_FXAA = 0x1a,
    [Description("graphicsheight")]
    GFX_HEIGHT = 10,
    [Description("msaa")]
    GFX_MSAA = 0x19,
    [Description("graphicsquality")]
    GFX_QUALITY = 13,
    [Description("targetframerate")]
    GFX_TARGET_FRAME_RATE = 0x1b,
    [Description("vsync")]
    GFX_VSYNC = 0x1c,
    [Description("graphicswidth")]
    GFX_WIDTH = 9,
    [Description("wincameraclear")]
    GFX_WIN_CAMERA_CLEAR = 0x18,
    [Description("winposx")]
    GFX_WIN_POSX = 0x27,
    [Description("winposy")]
    GFX_WIN_POSY = 40,
    [Description("hasAckedArenaRewards")]
    HAS_ACKED_ARENA_REWARDS = 0x74,
    [Description("hasAddedCardsToDeck")]
    HAS_ADDED_CARDS_TO_DECK = 0x66,
    [Description("hasBeenNudgedToCM")]
    HAS_BEEN_NUDGED_TO_CM = 0x65,
    [Description("hasClickedManaTab")]
    HAS_CLICKED_MANA_TAB = 0x86,
    [Description("hasclickedtournament")]
    HAS_CLICKED_TOURNAMENT = 0x4c,
    [Description("hasCrafted")]
    HAS_CRAFTED = 0x6c,
    [Description("hasDisenchanted")]
    HAS_DISENCHANTED = 0x69,
    [Description("hasEnteredNaxx")]
    HAS_ENTERED_NAXX = 0x7d,
    [Description("firstdeckcomplete")]
    HAS_FINISHED_A_DECK = 0x54,
    [Description("hasheardtgtpackvo")]
    HAS_HEARD_TGT_PACK_VO = 0x89,
    [Description("hasLostInArena")]
    HAS_LOST_IN_ARENA = 0x72,
    [Description("hasopenedbooster")]
    HAS_OPENED_BOOSTER = 0x4d,
    [Description("hasPlayedExpertAI")]
    HAS_PLAYED_EXPERT_AI = 0x68,
    [Description("hasPlayedNaxx")]
    HAS_PLAYED_NAXX = 0x80,
    [Description("hasRunOutOfQuests")]
    HAS_RUN_OUT_OF_QUESTS = 0x73,
    [Description("hasseen100goldReminder")]
    HAS_SEEN_100g_REMINDER = 0x6f,
    [Description("hasSeenAllBasicClassCardsComplete")]
    HAS_SEEN_ALL_BASIC_CLASS_CARDS_COMPLETE = 100,
    [Description("hasSeenBRM")]
    HAS_SEEN_BRM = 0x84,
    [Description("hasseencinematic")]
    HAS_SEEN_CINEMATIC = 12,
    [Description("hasseencollectionmanager")]
    HAS_SEEN_COLLECTIONMANAGER = 0x4f,
    [Description("hasseencollectionmanagerafterpractice")]
    HAS_SEEN_COLLECTIONMANAGER_AFTER_PRACTICE = 0x83,
    [Description("hasSeenCraftingInstruction")]
    HAS_SEEN_CRAFTING_INSTRUCTION = 0x6b,
    [Description("hasSeenCustomDeckPicker")]
    HAS_SEEN_CUSTOM_DECK_PICKER = 0x63,
    [Description("hasseendeckhelper")]
    HAS_SEEN_DECK_HELPER = 0x60,
    [Description("hasSeenExpertAI")]
    HAS_SEEN_EXPERT_AI = 0x5e,
    [Description("hasSeenExpertAIUnlock")]
    HAS_SEEN_EXPERT_AI_UNLOCK = 0x5f,
    [Description("hasseenforge")]
    HAS_SEEN_FORGE = 0x55,
    [Description("hasseenforge1win")]
    HAS_SEEN_FORGE_1WIN = 90,
    [Description("hasseenforge2loss")]
    HAS_SEEN_FORGE_2LOSS = 0x5b,
    [Description("hasseenforgecardchoice")]
    HAS_SEEN_FORGE_CARD_CHOICE = 0x57,
    [Description("hasseenforgecardchoice2")]
    HAS_SEEN_FORGE_CARD_CHOICE2 = 0x58,
    [Description("hasseenforgeherochoice")]
    HAS_SEEN_FORGE_HERO_CHOICE = 0x56,
    [Description("hasseenforgemaxwin")]
    HAS_SEEN_FORGE_MAX_WIN = 0x87,
    [Description("hasseenforgeplaymode")]
    HAS_SEEN_FORGE_PLAY_MODE = 0x59,
    [Description("hasseenforgeretire")]
    HAS_SEEN_FORGE_RETIRE = 0x5c,
    [Description("hasSeenGoldQtyInstruction")]
    HAS_SEEN_GOLD_QTY_INSTRUCTION = 0x70,
    [Description("hasSeenHeroicWarning")]
    HAS_SEEN_HEROIC_WARNING = 0x7b,
    [Description("firstHubVisitPastTutorial")]
    HAS_SEEN_HUB = 0x53,
    [Description("hasSeenLevel3")]
    HAS_SEEN_LEVEL_3 = 0x71,
    [Description("hasSeenLOE")]
    HAS_SEEN_LOE = 0x85,
    [Description("hasseenloestaffdisappear")]
    HAS_SEEN_LOE_STAFF_DISAPPEAR = 0x8a,
    [Description("hasseenloestaffreappear")]
    HAS_SEEN_LOE_STAFF_REAPPEAR = 0x8b,
    [Description("hasseenmulligan")]
    HAS_SEEN_MULLIGAN = 0x5d,
    [Description("hasSeenNaxx")]
    HAS_SEEN_NAXX = 0x7c,
    [Description("hasSeenNaxxClassChallenge")]
    HAS_SEEN_NAXX_CLASS_CHALLENGE = 0x7e,
    [Description("hasSeenPackOpening")]
    HAS_SEEN_PACK_OPENING = 0x61,
    [Description("hasSeenPracticeMode")]
    HAS_SEEN_PRACTICE_MODE = 0x62,
    [Description("hasseenpracticetray")]
    HAS_SEEN_PRACTICE_TRAY = 0x52,
    [Description("hasSeenShowAllCardsReminder")]
    HAS_SEEN_SHOW_ALL_CARDS_REMINDER = 0x6a,
    [Description("hasSeenStealthTaunter")]
    HAS_SEEN_STEALTH_TAUNTER = 0x75,
    [Description("hasSeenTheCoin")]
    HAS_SEEN_THE_COIN = 110,
    [Description("hasseentournament")]
    HAS_SEEN_TOURNAMENT = 0x4e,
    [Description("hasstartedadeck")]
    HAS_STARTED_A_DECK = 130,
    [Description("healthygamingdebug")]
    HEALTHY_GAMING_DEBUG = 0x10,
    [Description("hud")]
    HUD = 5,
    [Description("idlekicktime")]
    IDLE_KICK_TIME = 20,
    [Description("idlekicker")]
    IDLE_KICKER = 0x13,
    [Description("innkeepersSpecialCacheAge")]
    IKS_CACHE_AGE = 0x33,
    [Description("innkeepersSpecialLastResponse")]
    IKS_LAST_DOWNLOAD_RESPONSE = 50,
    [Description("innkeepersSpecialLastDownloadTime")]
    IKS_LAST_DOWNLOAD_TIME = 0x31,
    [Description("innkeepersSpecialLastShownAd")]
    IKS_LAST_SHOWN_AD = 0x39,
    [Description("innkeepersSpecialViews")]
    IKS_VIEWS = 0x30,
    [Description("inRankedPlayMode")]
    IN_RANKED_PLAY_MODE = 0x6d,
    [Description("intro")]
    INTRO = 0x2d,
    INVALID = 0,
    [Description("justfinishedtutorial")]
    JUST_FINISHED_TUTORIAL = 80,
    [Description("kelthuzadtaunts")]
    KELTHUZADTAUNTS = 0x26,
    [Description("lastChosenCustomDeck")]
    LAST_CUSTOM_DECK_CHOSEN = 0x42,
    [Description("lastfaileddopversion")]
    LAST_FAILED_DOP_VERSION = 0x2a,
    [Description("lastChosenPreconHero")]
    LAST_PRECON_HERO_CHOSEN = 0x41,
    [Description("laststate")]
    LAST_SCENE_MODE = 0x11,
    [Description("lastselectedadventure")]
    LAST_SELECTED_STORE_ADVENTURE_ID = 0x47,
    [Description("lastselectedbooster")]
    LAST_SELECTED_STORE_BOOSTER_ID = 70,
    [Description("lastselectedhero")]
    LAST_SELECTED_STORE_HERO_ID = 0x48,
    [Description("seenTB")]
    LATEST_SEEN_TAVERNBRAWL_SEASON = 0x49,
    [Description("seenTBScreen")]
    LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD = 0x4a,
    [Description("localtutorialprogress")]
    LOCAL_TUTORIAL_PROGRESS = 0x1f,
    [Description("locale")]
    LOCALE = 0x12,
    [Description("music")]
    MUSIC = 3,
    [Description("musicvolume")]
    MUSIC_VOLUME = 8,
    [Description("nearbyplayers2")]
    NEARBY_PLAYERS = 0x17,
    [Description("pagemouseovers")]
    PAGE_MOUSE_OVERS = 0x3b,
    [Description("preferredcdnindex")]
    PREFERRED_CDN_INDEX = 0x29,
    [Description("preferredregion")]
    PREFERRED_REGION = 0x16,
    [Description("preloadCardAssets")]
    PRELOAD_CARD_ASSETS = 0x35,
    [Description("reconnect")]
    RECONNECT = 0x22,
    [Description("reconnectRetryTime")]
    RECONNECT_RETRY_TIME = 0x24,
    [Description("reconnectTimeout")]
    RECONNECT_TIMEOUT = 0x23,
    [Description("seasonEndThreshold")]
    SEASON_END_THRESHOLD = 0x21,
    [Description("seenPackProductList")]
    SEEN_PACK_PRODUCT_LIST = 0x88,
    [Description("selectedAdventure")]
    SELECTED_ADVENTURE = 0x44,
    [Description("selectedAdventureMode")]
    SELECTED_ADVENTURE_MODE = 0x45,
    [Description("serverOptionsVersion")]
    SERVER_OPTIONS_VERSION = 0x3a,
    [Description("showadvancedcollectionmanager")]
    SHOW_ADVANCED_COLLECTIONMANAGER = 0x51,
    [Description("gfxdevicewarning")]
    SHOWN_GFX_DEVICE_WARNING = 0x2c,
    [Description("sound")]
    SOUND = 2,
    [Description("soundvolume")]
    SOUND_VOLUME = 7,
    [Description("spectatoropenjoin")]
    SPECTATOR_OPEN_JOIN = 0x81,
    [Description("streaming")]
    STREAMING = 6,
    [Description("seenCrazyRulesQuote")]
    TIMES_SEEN_TAVERNBRAWL_CRAZY_RULES_QUOTE = 0x4b,
    [Description("tipCraftingUnlocked")]
    TIP_CRAFTING_UNLOCKED = 0x67,
    [Description("forgetipprogress")]
    TIP_FORGE_PROGRESS = 0x40,
    [Description("playtipprogress")]
    TIP_PLAY_PROGRESS = 0x3f,
    [Description("practicetipporgress")]
    TIP_PRACTICE_PROGRESS = 0x3e,
    [Description("touchmode")]
    TOUCH_MODE = 0x2b,
    [Description("tutoriallostprogress")]
    TUTORIAL_LOST_PROGRESS = 0x2e,
    [Description("useExperimentalCode")]
    USE_EXPERIMENTAL_CODE = 0x36
}

