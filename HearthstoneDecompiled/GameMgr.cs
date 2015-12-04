using PegasusGame;
using PegasusShared;
using SpectatorProto;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameMgr
{
    private const string LOADING_POPUP_NAME = "LoadingPopup";
    private List<FindGameListener> m_findGameListeners;
    private FindGameState m_findGameState;
    private Network.GameSetup m_gameSetup;
    private GameType m_gameType;
    private Vector3 m_initialTransitionPopupPos;
    private uint m_lastEnterGameError;
    private int m_missionId;
    private GameType m_nextGameType;
    private int m_nextMissionId;
    private ReconnectType m_nextReconnectType;
    private bool m_nextSpectator;
    private bool m_pendingAutoConcede;
    private GameType m_prevGameType;
    private int m_prevMissionId;
    private ReconnectType m_prevReconnectType;
    private bool m_prevSpectator;
    private ReconnectType m_reconnectType;
    private bool m_spectator;
    private TransitionPopup m_transitionPopup;
    private PlatformDependentValue<string> MATCHING_POPUP_NAME;
    private const string MATCHING_POPUP_PC_NAME = "MatchingPopup3D";
    private const string MATCHING_POPUP_PHONE_NAME = "MatchingPopup3D_phone";
    private static Map<BattleNet.QueueEvent.Type, FindGameState?> s_bnetToFindGameResultMap;
    private static GameMgr s_instance = new GameMgr();
    private readonly Map<string, System.Type> s_transitionPopupNameToType;

    static GameMgr()
    {
        Map<BattleNet.QueueEvent.Type, FindGameState?> map = new Map<BattleNet.QueueEvent.Type, FindGameState?>();
        map.Add(BattleNet.QueueEvent.Type.UNKNOWN, null);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_ENTER, 4);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_LEAVE, null);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_DELAY, 5);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_UPDATE, 6);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_DELAY_ERROR, 8);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_AMM_ERROR, 8);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_WAIT_END, null);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_CANCEL, 7);
        map.Add(BattleNet.QueueEvent.Type.QUEUE_GAME_STARTED, 9);
        map.Add(BattleNet.QueueEvent.Type.ABORT_CLIENT_DROPPED, 8);
        s_bnetToFindGameResultMap = map;
    }

    public GameMgr()
    {
        Map<string, System.Type> map = new Map<string, System.Type>();
        map.Add("MatchingPopup3D", typeof(MatchingPopupDisplay));
        map.Add("MatchingPopup3D_phone", typeof(MatchingPopupDisplay));
        map.Add("LoadingPopup", typeof(LoadingPopupDisplay));
        this.s_transitionPopupNameToType = map;
        this.m_findGameListeners = new List<FindGameListener>();
    }

    public bool CancelFindGame()
    {
        if (!GameUtils.IsMatchmadeGameType(this.m_nextGameType))
        {
            return false;
        }
        if (!Network.Get().IsFindingGame())
        {
            return false;
        }
        Network.Get().CancelFindGame();
        this.ChangeFindGameState(FindGameState.CLIENT_CANCELED);
        return true;
    }

    private void ChangeBoardIfNecessary()
    {
        int board = this.m_gameSetup.Board;
        string str = Cheats.Get().GetBoard();
        bool flag = false;
        if (!string.IsNullOrEmpty(str))
        {
            board = GameUtils.GetBoardIdFromAssetName(str);
            flag = true;
        }
        if (!flag && DemoMgr.Get().IsExpoDemo())
        {
            str = Vars.Key("Demo.ForceBoard").GetStr(null);
            if (str != null)
            {
                board = GameUtils.GetBoardIdFromAssetName(str);
            }
        }
        this.m_gameSetup.Board = board;
    }

    private bool ChangeFindGameState(FindGameState state)
    {
        return this.ChangeFindGameState(state, null, null, null);
    }

    private bool ChangeFindGameState(FindGameState state, BattleNet.GameServerInfo serverInfo)
    {
        return this.ChangeFindGameState(state, null, serverInfo, null);
    }

    private bool ChangeFindGameState(FindGameState state, BattleNet.QueueEvent queueEvent)
    {
        return this.ChangeFindGameState(state, queueEvent, null, null);
    }

    private bool ChangeFindGameState(FindGameState state, Network.GameCancelInfo cancelInfo)
    {
        return this.ChangeFindGameState(state, null, null, cancelInfo);
    }

    private bool ChangeFindGameState(FindGameState state, BattleNet.QueueEvent queueEvent, BattleNet.GameServerInfo serverInfo, Network.GameCancelInfo cancelInfo)
    {
        this.m_findGameState = state;
        FindGameEventData eventData = new FindGameEventData {
            m_state = state,
            m_gameServer = serverInfo,
            m_cancelInfo = cancelInfo
        };
        if (queueEvent != null)
        {
            eventData.m_queueMinSeconds = queueEvent.MinSeconds;
            eventData.m_queueMaxSeconds = queueEvent.MaxSeconds;
        }
        switch (state)
        {
            case FindGameState.CLIENT_CANCELED:
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_QUEUE_CANCELED:
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_STARTED:
            case FindGameState.SERVER_GAME_CANCELED:
                Network.Get().RemoveGameServerDisconnectEventListener(new Network.GameServerDisconnectEvent(this.OnGameServerDisconnect));
                break;
        }
        bool flag = this.FireFindGameEvent(eventData);
        if (!flag)
        {
            this.DoDefaultFindGameEventBehavior(eventData);
        }
        this.FinalizeState(eventData);
        return flag;
    }

    public GameEntity CreateGameEntity()
    {
        ScenarioDbId missionId = (ScenarioDbId) this.m_missionId;
        switch (missionId)
        {
            case ScenarioDbId.TUTORIAL_ILLIDAN:
                return new Tutorial_05();

            case ScenarioDbId.TUTORIAL_CHO:
                return new Tutorial_06();

            case ScenarioDbId.NAXX_ANUBREKHAN:
            case ScenarioDbId.NAXX_HEROIC_ANUBREKHAN:
                return new NAX01_AnubRekhan();

            case ScenarioDbId.NAXX_FAERLINA:
            case ScenarioDbId.NAXX_CHALLENGE_DRUID_V_FAERLINA:
            case ScenarioDbId.NAXX_HEROIC_FAERLINA:
                return new NAX02_Faerlina();

            case ScenarioDbId.NAXX_NOTH:
            case ScenarioDbId.NAXX_HEROIC_NOTH:
                return new NAX04_Noth();

            case ScenarioDbId.NAXX_HEIGAN:
            case ScenarioDbId.NAXX_CHALLENGE_MAGE_V_HEIGAN:
            case ScenarioDbId.NAXX_HEROIC_HEIGAN:
                return new NAX05_Heigan();

            case ScenarioDbId.NAXX_LOATHEB:
            case ScenarioDbId.NAXX_CHALLENGE_HUNTER_V_LOATHEB:
            case ScenarioDbId.NAXX_HEROIC_LOATHEB:
                return new NAX06_Loatheb();

            case ScenarioDbId.NAXX_MAEXXNA:
            case ScenarioDbId.NAXX_CHALLENGE_ROGUE_V_MAEXXNA:
            case ScenarioDbId.NAXX_HEROIC_MAEXXNA:
                return new NAX03_Maexxna();

            case ScenarioDbId.NAXX_RAZUVIOUS:
            case ScenarioDbId.NAXX_HEROIC_RAZUVIOUS:
                return new NAX07_Razuvious();

            case ScenarioDbId.NAXX_GOTHIK:
            case ScenarioDbId.NAXX_CHALLENGE_SHAMAN_V_GOTHIK:
            case ScenarioDbId.NAXX_HEROIC_GOTHIK:
                return new NAX08_Gothik();

            case ScenarioDbId.NAXX_HORSEMEN:
            case ScenarioDbId.NAXX_CHALLENGE_WARLOCK_V_HORSEMEN:
            case ScenarioDbId.NAXX_HEROIC_HORSEMEN:
                return new NAX09_Horsemen();

            case ScenarioDbId.NAXX_PATCHWERK:
            case ScenarioDbId.NAXX_HEROIC_PATCHWERK:
                return new NAX10_Patchwerk();

            case ScenarioDbId.NAXX_GROBBULUS:
            case ScenarioDbId.NAXX_CHALLENGE_WARRIOR_V_GROBBULUS:
            case ScenarioDbId.NAXX_HEROIC_GROBBULUS:
                return new NAX11_Grobbulus();

            case ScenarioDbId.NAXX_GLUTH:
            case ScenarioDbId.NAXX_HEROIC_GLUTH:
                return new NAX12_Gluth();

            case ScenarioDbId.NAXX_THADDIUS:
            case ScenarioDbId.NAXX_CHALLENGE_PRIEST_V_THADDIUS:
            case ScenarioDbId.NAXX_HEROIC_THADDIUS:
                return new NAX13_Thaddius();

            case ScenarioDbId.NAXX_KELTHUZAD:
            case ScenarioDbId.NAXX_CHALLENGE_PALADIN_V_KELTHUZAD:
            case ScenarioDbId.NAXX_HEROIC_KELTHUZAD:
                return new NAX15_KelThuzad();

            case ScenarioDbId.NAXX_SAPPHIRON:
            case ScenarioDbId.NAXX_HEROIC_SAPPHIRON:
                return new NAX14_Sapphiron();

            case ScenarioDbId.BRM_GRIM_GUZZLER:
            case ScenarioDbId.BRM_HEROIC_GRIM_GUZZLER:
            case ScenarioDbId.BRM_CHALLENGE_HUNTER_V_GUZZLER:
                return new BRM01_GrimGuzzler();

            case ScenarioDbId.BRM_DARK_IRON_ARENA:
            case ScenarioDbId.BRM_HEROIC_DARK_IRON_ARENA:
            case ScenarioDbId.BRM_CHALLENGE_MAGE_V_DARK_IRON_ARENA:
                return new BRM02_DarkIronArena();

            case ScenarioDbId.BRM_THAURISSAN:
            case ScenarioDbId.BRM_HEROIC_THAURISSAN:
                return new BRM03_Thaurissan();

            case ScenarioDbId.BRM_GARR:
            case ScenarioDbId.BRM_HEROIC_GARR:
            case ScenarioDbId.BRM_CHALLENGE_WARRIOR_V_GARR:
                return new BRM04_Garr();

            case ScenarioDbId.BRM_BARON_GEDDON:
            case ScenarioDbId.BRM_HEROIC_BARON_GEDDON:
            case ScenarioDbId.BRM_CHALLENGE_SHAMAN_V_GEDDON:
                return new BRM05_BaronGeddon();

            case ScenarioDbId.BRM_MAJORDOMO:
            case ScenarioDbId.BRM_HEROIC_MAJORDOMO:
                return new BRM06_Majordomo();

            case ScenarioDbId.BRM_OMOKK:
            case ScenarioDbId.BRM_HEROIC_OMOKK:
                return new BRM07_Omokk();

            case ScenarioDbId.BRM_DRAKKISATH:
            case ScenarioDbId.BRM_CHALLENGE_PRIEST_V_DRAKKISATH:
            case ScenarioDbId.BRM_HEROIC_DRAKKISATH:
                return new BRM08_Drakkisath();

            case ScenarioDbId.BRM_REND_BLACKHAND:
            case ScenarioDbId.BRM_CHALLENGE_DRUID_V_BLACKHAND:
            case ScenarioDbId.BRM_HEROIC_REND_BLACKHAND:
                return new BRM09_RendBlackhand();

            case ScenarioDbId.BRM_RAZORGORE:
            case ScenarioDbId.BRM_HEROIC_RAZORGORE:
            case ScenarioDbId.BRM_CHALLENGE_WARLOCK_V_RAZORGORE:
                return new BRM10_Razorgore();

            case ScenarioDbId.BRM_VAELASTRASZ:
            case ScenarioDbId.BRM_HEROIC_VAELASTRASZ:
            case ScenarioDbId.BRM_CHALLENGE_ROGUE_V_VAELASTRASZ:
                return new BRM11_Vaelastrasz();

            case ScenarioDbId.BRM_CHROMAGGUS:
            case ScenarioDbId.BRM_HEROIC_CHROMAGGUS:
                return new BRM12_Chromaggus();

            case ScenarioDbId.BRM_NEFARIAN:
            case ScenarioDbId.BRM_HEROIC_NEFARIAN:
                return new BRM13_Nefarian();

            case ScenarioDbId.BRM_OMNOTRON:
            case ScenarioDbId.BRM_HEROIC_OMNOTRON:
            case ScenarioDbId.BRM_CHALLENGE_PALADIN_V_OMNOTRON:
                return new BRM14_Omnotron();

            case ScenarioDbId.BRM_MALORIAK:
            case ScenarioDbId.BRM_HEROIC_MALORIAK:
                return new BRM15_Maloriak();

            case ScenarioDbId.BRM_ATRAMEDES:
            case ScenarioDbId.BRM_HEROIC_ATRAMEDES:
                return new BRM16_Atramedes();

            case ScenarioDbId.BRM_ZOMBIE_NEF:
            case ScenarioDbId.BRM_HEROIC_ZOMBIE_NEF:
                return new BRM17_ZombieNef();

            case ScenarioDbId.TB_RAG_V_NEF:
                return new TB01_RagVsNef();

            case ScenarioDbId.TB_DECKBUILDING:
            case ScenarioDbId.TB_DECKBUILDING_1P_TEST:
                return new TB04_DeckBuilding();

            case ScenarioDbId.LOE_CHALLENGE_DRUID_V_SCARVASH:
            case ScenarioDbId.LOE_HEROIC_SCARVASH:
                goto Label_0675;

            case ScenarioDbId.LOE_CHALLENGE_MAGE_V_SENTINEL:
            case ScenarioDbId.LOE_HEROIC_STEEL_SENTINEL:
                goto Label_068B;

            case ScenarioDbId.LOE_CHALLENGE_ROGUE_V_SKELESAURUS:
            case ScenarioDbId.LOE_HEROIC_SKELESAURUS:
            case ScenarioDbId.LOE_SKELESAURUS:
                return new LOE13_Skelesaurus();

            case ScenarioDbId.TB_CO_OP_1P_TEST:
            case ScenarioDbId.TB_CO_OP_V2:
            case ScenarioDbId.TB_CO_OP_PRECON:
                break;

            case ScenarioDbId.LOE_CHALLENGE_SHAMAN_V_GIANTFIN:
            case ScenarioDbId.LOE_HEROIC_GIANTFIN:
                goto Label_0649;

            case ScenarioDbId.LOE_CHALLENGE_WARRIOR_V_ZINAAR:
            case ScenarioDbId.LOE_HEROIC_ZINAAR:
                goto Label_065F;

            case ScenarioDbId.LOE_CHALLENGE_HUNTER_V_SLITHERSPEAR:
            case ScenarioDbId.LOE_HEROIC_SLITHERSPEAR:
                goto Label_0654;

            case ScenarioDbId.LOE_CHALLENGE_PRIEST_V_NAZJAR:
            case ScenarioDbId.LOE_HEROIC_LADY_NAZJAR:
                goto Label_06AC;

            case ScenarioDbId.LOE_HEROIC_SUN_RAIDER_PHAERIX:
                goto Label_066A;

            case ScenarioDbId.LOE_HEROIC_TEMPLE_ESCAPE:
                goto Label_0696;

            case ScenarioDbId.TB_GIFTEXCHANGE_1P_TEST:
            case ScenarioDbId.TB_GIFTEXCHANGE:
                return new TB05_GiftExchange();

            case ScenarioDbId.LOE_HEROIC_ARCHAEDAS:
                goto Label_0680;

            case ScenarioDbId.TB_CHOOSEFATEBUILD_1P_TEST:
            case ScenarioDbId.TB_CHOOSEFATEBUILD:
                return new TB_ChooseYourFateBuildaround();

            case ScenarioDbId.LOE_HEROIC_MINE_CART:
                goto Label_06A1;

            case ScenarioDbId.LOE_HEROIC_RAFAAM_1:
            case ScenarioDbId.LOE_RAFAAM_1:
                return new LOE15_Boss1();

            case ScenarioDbId.LOE_HEROIC_RAFAAM_2:
                goto Label_06C2;

            case ScenarioDbId.TB_CHOOSEFATERANDOM_1P_TEST:
            case ScenarioDbId.TB_CHOOSEFATERANDOM:
                return new TB_ChooseYourFateRandom();

            case ScenarioDbId.TB_SINGLEPLAYERTRIAL:
                return new TB06_SinglePlayerTrial();

            default:
                switch (missionId)
                {
                    case ScenarioDbId.LOE_SCARVASH:
                        goto Label_0675;

                    case ScenarioDbId.LOE_TEMPLE_ESCAPE:
                        goto Label_0696;

                    case ScenarioDbId.LOE_MINE_CART:
                        goto Label_06A1;

                    case ScenarioDbId.TB_INSP_V_JOUST:
                        return new TB03_InspireVSJoust();

                    default:
                        switch (missionId)
                        {
                            case ScenarioDbId.LOE_ARCHAEDAS:
                                goto Label_0680;

                            case ScenarioDbId.LOE_LADY_NAZJAR:
                                goto Label_06AC;
                        }
                        break;
                }
                if (missionId != ScenarioDbId.TUTORIAL_HOGGER)
                {
                    if (missionId == ScenarioDbId.TUTORIAL_MILLHOUSE)
                    {
                        return new Tutorial_02();
                    }
                    if (missionId != ScenarioDbId.LOE_SUN_RAIDER_PHAERIX)
                    {
                        if (missionId != ScenarioDbId.LOE_ZINAAR)
                        {
                            if (missionId == ScenarioDbId.TUTORIAL_MUKLA)
                            {
                                return new Tutorial_03();
                            }
                            if (missionId == ScenarioDbId.TUTORIAL_NESINGWARY)
                            {
                                return new Tutorial_04();
                            }
                            if (((missionId != ScenarioDbId.TB_CO_OP_TEST) && (missionId != ScenarioDbId.TB_CO_OP)) && (missionId != ScenarioDbId.TB_CO_OP_TEST2))
                            {
                                if (missionId != ScenarioDbId.LOE_GIANTFIN)
                                {
                                    if (missionId != ScenarioDbId.LOE_RAFAAM_2)
                                    {
                                        if (missionId != ScenarioDbId.LOE_STEEL_SENTINEL)
                                        {
                                            if (missionId != ScenarioDbId.LOE_SLITHERSPEAR)
                                            {
                                                if (missionId != ScenarioDbId.LOE_CHALLENGE_PALADIN_V_ARCHAEDUS)
                                                {
                                                    if (missionId != ScenarioDbId.LOE_CHALLENGE_WARLOCK_V_SUN_RAIDER)
                                                    {
                                                        return new StandardGameEntity();
                                                    }
                                                    goto Label_066A;
                                                }
                                                goto Label_0680;
                                            }
                                            goto Label_0654;
                                        }
                                        goto Label_068B;
                                    }
                                    goto Label_06C2;
                                }
                                goto Label_0649;
                            }
                            break;
                        }
                        goto Label_065F;
                    }
                    goto Label_066A;
                }
                return new Tutorial_01();
        }
        return new TB02_CoOp();
    Label_0649:
        return new LOE10_Giantfin();
    Label_0654:
        return new LOE09_LordSlitherspear();
    Label_065F:
        return new LOE01_Zinaar();
    Label_066A:
        return new LOE02_Sun_Raider_Phaerix();
    Label_0675:
        return new LOE04_Scarvash();
    Label_0680:
        return new LOE08_Archaedas();
    Label_068B:
        return new LOE14_Steel_Sentinel();
    Label_0696:
        return new LOE03_AncientTemple();
    Label_06A1:
        return new LOE07_MineCart();
    Label_06AC:
        return new LOE12_Naga();
    Label_06C2:
        return new LOE16_Boss2();
    }

    private void DestroyTransitionPopup()
    {
        if (this.m_transitionPopup != null)
        {
            UnityEngine.Object.Destroy(this.m_transitionPopup.gameObject);
        }
    }

    private string DetermineTransitionPopupForFindGame(GameType gameType, int missionId)
    {
        if (gameType != GameType.GT_TAVERNBRAWL)
        {
            switch (gameType)
            {
                case GameType.GT_TUTORIAL:
                    return null;

                case GameType.GT_ARENA:
                case GameType.GT_RANKED:
                case GameType.GT_UNRANKED:
                    return (string) this.MATCHING_POPUP_NAME;
            }
            return "LoadingPopup";
        }
        switch (Network.TranslateGameTypeToBnet(gameType, missionId))
        {
            case BnetGameType.BGT_TAVERNBRAWL_PVP:
            case BnetGameType.BGT_TAVERNBRAWL_2P_COOP:
                return (string) this.MATCHING_POPUP_NAME;
        }
        return "LoadingPopup";
    }

    private void DoDefaultFindGameEventBehavior(FindGameEventData eventData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_ERROR:
                Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ERROR_GAME_DENIED", new object[0]);
                this.HideTransitionPopup();
                return;

            case FindGameState.BNET_QUEUE_ENTERED:
            case FindGameState.BNET_QUEUE_DELAYED:
            case FindGameState.BNET_QUEUE_UPDATED:
                return;

            case FindGameState.BNET_QUEUE_CANCELED:
                this.HideTransitionPopup();
                return;

            case FindGameState.SERVER_GAME_CONNECTING:
                Network.Get().GotoGameServer(eventData.m_gameServer, this.IsNextReconnect());
                return;

            case FindGameState.SERVER_GAME_STARTED:
                if (Box.Get() != null)
                {
                    LoadingScreen.Get().SetFreezeFrameCamera(Box.Get().GetCamera());
                    LoadingScreen.Get().SetTransitionAudioListener(Box.Get().GetAudioListener());
                }
                if (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
                {
                    SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAMEPLAY);
                    return;
                }
                if (!SpectatorManager.Get().IsSpectatingOpposingSide())
                {
                    SceneMgr.Get().ReloadMode();
                }
                return;

            case FindGameState.SERVER_GAME_CANCELED:
                if (eventData.m_cancelInfo != null)
                {
                    if (eventData.m_cancelInfo.CancelReason == Network.GameCancelInfo.Reason.OPPONENT_TIMEOUT)
                    {
                        Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ERROR_GAME_OPPONENT_TIMEOUT", new object[0]);
                        break;
                    }
                    object[] messageArgs = new object[] { eventData.m_cancelInfo.CancelReason };
                    Error.AddDevWarning("GAME ERROR", "The Game Server canceled the game. Error: {0}", messageArgs);
                }
                break;

            default:
                return;
        }
        this.HideTransitionPopup();
    }

    private void FinalizeState(FindGameEventData eventData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_QUEUE_CANCELED:
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_STARTED:
            case FindGameState.SERVER_GAME_CANCELED:
                this.ChangeFindGameState(FindGameState.INVALID);
                break;
        }
    }

    public void FindGame(GameType type, int missionId, long deckId = 0, long aiDeckId = 0)
    {
        this.m_lastEnterGameError = 0;
        this.ChangeFindGameState(FindGameState.CLIENT_STARTED);
        this.m_nextGameType = type;
        this.m_nextMissionId = missionId;
        string popupName = this.DetermineTransitionPopupForFindGame(type, missionId);
        if (popupName != null)
        {
            this.ShowTransitionPopup(popupName);
        }
        Network.Get().FindGame(type, missionId, deckId, aiDeckId);
    }

    private bool FireFindGameEvent(FindGameEventData eventData)
    {
        bool flag = false;
        foreach (FindGameListener listener in this.m_findGameListeners.ToArray())
        {
            flag = listener.Fire(eventData) || flag;
        }
        return flag;
    }

    public static GameMgr Get()
    {
        return s_instance;
    }

    public FindGameState GetFindGameState()
    {
        return this.m_findGameState;
    }

    public Network.GameSetup GetGameSetup()
    {
        return this.m_gameSetup;
    }

    public GameType GetGameType()
    {
        return this.m_gameType;
    }

    public uint GetLastEnterGameError()
    {
        return this.m_lastEnterGameError;
    }

    public int GetMissionId()
    {
        return this.m_missionId;
    }

    public GameType GetNextGameType()
    {
        return this.m_nextGameType;
    }

    public int GetNextMissionId()
    {
        return this.m_nextMissionId;
    }

    public ReconnectType GetNextReconnectType()
    {
        return this.m_nextReconnectType;
    }

    public SceneMgr.Mode GetPostDisconnectSceneMode()
    {
        if (this.IsSpectator())
        {
            if (GameUtils.AreAllTutorialsComplete())
            {
                return SceneMgr.Mode.HUB;
            }
            return SceneMgr.Mode.INVALID;
        }
        if (this.IsTutorial())
        {
            return SceneMgr.Mode.INVALID;
        }
        return this.GetPostGameSceneMode();
    }

    public SceneMgr.Mode GetPostGameSceneMode()
    {
        if (this.IsSpectator())
        {
            if (GameUtils.AreAllTutorialsComplete())
            {
                return SceneMgr.Mode.HUB;
            }
            return SceneMgr.Mode.INVALID;
        }
        SceneMgr.Mode hUB = SceneMgr.Mode.HUB;
        GameType gameType = this.m_gameType;
        switch (gameType)
        {
            case GameType.GT_VS_AI:
                return SceneMgr.Mode.ADVENTURE;

            case GameType.GT_VS_FRIEND:
                if (!FriendChallengeMgr.Get().HasChallenge())
                {
                    return hUB;
                }
                if (!FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                {
                    return SceneMgr.Mode.FRIENDLY;
                }
                return SceneMgr.Mode.TAVERN_BRAWL;

            case GameType.GT_ARENA:
                return SceneMgr.Mode.DRAFT;

            case GameType.GT_RANKED:
            case GameType.GT_UNRANKED:
                return SceneMgr.Mode.TOURNAMENT;
        }
        if (gameType != GameType.GT_TAVERNBRAWL)
        {
            return hUB;
        }
        return SceneMgr.Mode.TAVERN_BRAWL;
    }

    public GameType GetPreviousGameType()
    {
        return this.m_prevGameType;
    }

    public int GetPreviousMissionId()
    {
        return this.m_prevMissionId;
    }

    public ReconnectType GetPreviousReconnectType()
    {
        return this.m_prevReconnectType;
    }

    public ReconnectType GetReconnectType()
    {
        return this.m_reconnectType;
    }

    public TransitionPopup GetTransitionPopup()
    {
        return this.m_transitionPopup;
    }

    private void HandleGameCanceled()
    {
        this.m_nextGameType = GameType.GT_UNKNOWN;
        this.m_nextMissionId = 0;
        this.m_nextReconnectType = ReconnectType.INVALID;
        this.m_nextSpectator = false;
        Network.Get().ClearLastGameServerJoined();
    }

    private void HideTransitionPopup()
    {
        if (this.m_transitionPopup != null)
        {
            this.m_transitionPopup.Hide();
        }
    }

    public void Initialize()
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "MatchingPopup3D",
            Phone = "MatchingPopup3D_phone"
        };
        this.MATCHING_POPUP_NAME = value2;
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    public bool IsAboutToStopFindingGame()
    {
        switch (this.m_findGameState)
        {
            case FindGameState.CLIENT_CANCELED:
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_QUEUE_CANCELED:
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_STARTED:
            case FindGameState.SERVER_GAME_CANCELED:
                return true;
        }
        return false;
    }

    public bool IsAI()
    {
        return GameUtils.IsAIMission(this.m_missionId);
    }

    public bool IsArena()
    {
        return (this.m_gameType == GameType.GT_ARENA);
    }

    public bool IsClassChallengeMission()
    {
        return GameUtils.IsClassChallengeMission(this.m_missionId);
    }

    public bool IsClassicMission()
    {
        return GameUtils.IsClassicMission(this.m_missionId);
    }

    public bool IsExpansionMission()
    {
        return GameUtils.IsExpansionMission(this.m_missionId);
    }

    public bool IsFindingGame()
    {
        return (this.m_findGameState != FindGameState.INVALID);
    }

    public bool IsFriendly()
    {
        return (this.m_gameType == GameType.GT_VS_FRIEND);
    }

    public bool IsNextAI()
    {
        return GameUtils.IsAIMission(this.m_nextMissionId);
    }

    public bool IsNextArena()
    {
        return (this.m_nextGameType == GameType.GT_ARENA);
    }

    public bool IsNextClassicMission()
    {
        return GameUtils.IsClassicMission(this.m_nextMissionId);
    }

    public bool IsNextExpansionMission()
    {
        return GameUtils.IsExpansionMission(this.m_nextMissionId);
    }

    public bool IsNextFriendly()
    {
        return (this.m_nextGameType == GameType.GT_VS_FRIEND);
    }

    public bool IsNextPlay()
    {
        return (this.IsNextRankedPlay() || this.IsNextUnrankedPlay());
    }

    public bool IsNextPractice()
    {
        return GameUtils.IsPracticeMission(this.m_nextMissionId);
    }

    public bool IsNextRankedPlay()
    {
        return (this.m_nextGameType == GameType.GT_RANKED);
    }

    public bool IsNextReconnect()
    {
        return (this.m_nextReconnectType != ReconnectType.INVALID);
    }

    public bool IsNextSpectator()
    {
        return this.m_nextSpectator;
    }

    public bool IsNextTavernBrawl()
    {
        return (this.m_nextGameType == GameType.GT_TAVERNBRAWL);
    }

    public bool IsNextTutorial()
    {
        return GameUtils.IsTutorialMission(this.m_nextMissionId);
    }

    public bool IsNextUnrankedPlay()
    {
        return (this.m_nextGameType == GameType.GT_UNRANKED);
    }

    public bool IsPendingAutoConcede()
    {
        return this.m_pendingAutoConcede;
    }

    public bool IsPlay()
    {
        return (this.IsRankedPlay() || this.IsUnrankedPlay());
    }

    public bool IsPractice()
    {
        return GameUtils.IsPracticeMission(this.m_missionId);
    }

    public bool IsPreviousReconnect()
    {
        return (this.m_prevReconnectType != ReconnectType.INVALID);
    }

    public bool IsRankedPlay()
    {
        return (this.m_gameType == GameType.GT_RANKED);
    }

    public bool IsReconnect()
    {
        return (this.m_reconnectType != ReconnectType.INVALID);
    }

    public bool IsSpectator()
    {
        return this.m_spectator;
    }

    public bool IsTavernBrawl()
    {
        return (this.m_gameType == GameType.GT_TAVERNBRAWL);
    }

    public bool IsTransitionPopupShown()
    {
        if (this.m_transitionPopup == null)
        {
            return false;
        }
        return this.m_transitionPopup.IsShown();
    }

    public bool IsTutorial()
    {
        return GameUtils.IsTutorialMission(this.m_missionId);
    }

    public bool IsUnrankedPlay()
    {
        return (this.m_gameType == GameType.GT_UNRANKED);
    }

    private void LoadTransitionPopup(string popupName)
    {
        GameObject obj2 = AssetLoader.Get().LoadActor(popupName, false, false);
        if (obj2 == null)
        {
            object[] messageArgs = new object[] { popupName };
            Error.AddDevFatal("GameMgr.LoadTransitionPopup() - Failed to load {0}", messageArgs);
        }
        else
        {
            this.m_transitionPopup = obj2.GetComponent<TransitionPopup>();
            this.m_initialTransitionPopupPos = this.m_transitionPopup.transform.position;
            this.m_transitionPopup.RegisterMatchCanceledEvent(new TransitionPopup.MatchCanceledEvent(this.OnTransitionPopupCanceled));
            SceneUtils.SetLayer(this.m_transitionPopup, GameLayer.IgnoreFullScreenEffects);
        }
    }

    public bool OnBnetError(BnetErrorInfo info, object userData)
    {
        if (info.GetFeature() == BnetFeature.Games)
        {
            BattleNetErrors error = info.GetError();
            this.m_lastEnterGameError = (uint) error;
            string str = null;
            bool flag = false;
            FindGameState state = FindGameState.BNET_ERROR;
            switch (error)
            {
                case BattleNetErrors.ERROR_GAME_MASTER_INVALID_FACTORY:
                case BattleNetErrors.ERROR_GAME_MASTER_NO_GAME_SERVER:
                case BattleNetErrors.ERROR_GAME_MASTER_NO_FACTORY:
                    str = error.ToString();
                    flag = true;
                    break;
            }
            if (!flag)
            {
                PegasusShared.ErrorCode lastEnterGameError = (PegasusShared.ErrorCode) this.m_lastEnterGameError;
                string headerKey = string.Empty;
                string messageKey = null;
                switch (lastEnterGameError)
                {
                    case PegasusShared.ErrorCode.ERROR_SCENARIO_NOT_MULTIPLAYER:
                        headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
                        messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_NOT_MULTIPLAYER";
                        break;

                    case PegasusShared.ErrorCode.ERROR_SCENARIO_NO_DECK_SPECIFIED:
                        headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
                        messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_NO_DECK_SPECIFIED";
                        break;

                    case PegasusShared.ErrorCode.ERROR_TAVERN_BRAWL_SEASON_INCREMENTED:
                        headerKey = "GLOBAL_TAVERN_BRAWL";
                        messageKey = "GLOBAL_TAVERN_BRAWL_ERROR_SEASON_INCREMENTED";
                        TavernBrawlManager.Get().RefreshServerData();
                        break;
                }
                if (messageKey != null)
                {
                    Error.AddWarningLoc(headerKey, messageKey, new object[0]);
                    str = lastEnterGameError.ToString();
                    state = FindGameState.BNET_QUEUE_CANCELED;
                    flag = true;
                }
            }
            if (flag)
            {
                string format = string.Format("GameMgr.OnBnetError() - received error {0} {1}", this.m_lastEnterGameError, str);
                Log.BattleNet.Print(LogLevel.Error, format, new object[0]);
                this.HandleGameCanceled();
                this.ChangeFindGameState(state);
                return true;
            }
        }
        return false;
    }

    private void OnGameCanceled()
    {
        this.HandleGameCanceled();
        Network.GameCancelInfo gameCancelInfo = Network.GetGameCancelInfo();
        this.ChangeFindGameState(FindGameState.SERVER_GAME_CANCELED, gameCancelInfo);
    }

    private void OnGameEnded()
    {
        this.m_prevGameType = this.m_gameType;
        this.m_gameType = GameType.GT_UNKNOWN;
        this.m_prevMissionId = this.m_missionId;
        this.m_missionId = 0;
        this.m_prevReconnectType = this.m_reconnectType;
        this.m_reconnectType = ReconnectType.INVALID;
        this.m_prevSpectator = this.m_spectator;
        this.m_spectator = false;
        this.m_lastEnterGameError = 0;
        this.m_pendingAutoConcede = false;
        this.m_gameSetup = null;
    }

    private void OnGameQueueEvent(BattleNet.QueueEvent queueEvent)
    {
        FindGameState? nullable = null;
        s_bnetToFindGameResultMap.TryGetValue(queueEvent.EventType, out nullable);
        if (queueEvent.BnetError != 0)
        {
            this.m_lastEnterGameError = (uint) queueEvent.BnetError;
        }
        if (nullable.HasValue)
        {
            if (queueEvent.EventType == BattleNet.QueueEvent.Type.QUEUE_DELAY_ERROR)
            {
                string headerKey = string.Empty;
                string messageKey = null;
                switch (queueEvent.BnetError)
                {
                    case 0xf4628:
                        headerKey = "GLOBAL_TAVERN_BRAWL";
                        messageKey = "GLOBAL_TAVERN_BRAWL_ERROR_SEASON_INCREMENTED";
                        TavernBrawlManager.Get().RefreshServerData();
                        break;

                    case 0xf4629:
                        headerKey = "GLOBAL_TAVERN_BRAWL";
                        messageKey = "GLOBAL_TAVERN_BRAWL_ERROR_NOT_ACTIVE";
                        TavernBrawlManager.Get().RefreshServerData();
                        break;

                    case 0x61b9:
                        return;

                    case 0xf4434:
                        headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
                        messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_NOT_MULTIPLAYER";
                        break;
                }
                if (messageKey != null)
                {
                    Error.AddWarningLoc(headerKey, messageKey, new object[0]);
                    nullable = 7;
                    this.HandleGameCanceled();
                }
            }
            if (queueEvent.EventType == BattleNet.QueueEvent.Type.QUEUE_GAME_STARTED)
            {
                queueEvent.GameServer.Mission = this.m_nextMissionId;
                this.ChangeFindGameState(nullable.Value, queueEvent, queueEvent.GameServer, null);
            }
            else
            {
                this.ChangeFindGameState(nullable.Value, queueEvent);
            }
        }
    }

    private void OnGameServerDisconnect(BattleNetErrors error)
    {
        this.OnGameCanceled();
    }

    private void OnGameSetup()
    {
        if (!SpectatorManager.Get().IsSpectatingOpposingSide() || (this.m_gameSetup == null))
        {
            this.m_gameSetup = Network.GetGameSetupInfo();
            this.ChangeBoardIfNecessary();
            if ((this.m_findGameState == FindGameState.INVALID) && (this.m_gameType == GameType.GT_UNKNOWN))
            {
                Debug.LogError(string.Format("GameMgr.OnGameStarting() - Received {0} packet even though we're not looking for a game.", PegasusGame.GameSetup.PacketID.ID));
            }
            else
            {
                this.m_prevGameType = this.m_gameType;
                this.m_gameType = this.m_nextGameType;
                this.m_nextGameType = GameType.GT_UNKNOWN;
                this.m_prevMissionId = this.m_missionId;
                this.m_missionId = this.m_nextMissionId;
                this.m_nextMissionId = 0;
                this.m_prevReconnectType = this.m_reconnectType;
                this.m_reconnectType = this.m_nextReconnectType;
                this.m_nextReconnectType = ReconnectType.INVALID;
                this.m_prevSpectator = this.m_spectator;
                this.m_spectator = this.m_nextSpectator;
                this.m_nextSpectator = false;
                if (!this.IsSpectator())
                {
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_GAME_START);
                }
                this.ChangeFindGameState(FindGameState.SERVER_GAME_STARTED);
            }
        }
    }

    public void OnLoggedIn()
    {
        Network network = Network.Get();
        network.RegisterGameQueueHandler(new Network.GameQueueHandler(this.OnGameQueueEvent));
        network.RegisterNetHandler(PegasusGame.GameSetup.PacketID.ID, new Network.NetHandler(this.OnGameSetup), null);
        network.RegisterNetHandler(GameCanceled.PacketID.ID, new Network.NetHandler(this.OnGameCanceled), null);
        network.RegisterNetHandler(ServerResult.PacketID.ID, new Network.NetHandler(this.OnServerResult), null);
        network.AddBnetErrorListener(BnetFeature.Games, new Network.BnetErrorCallback(this.OnBnetError));
        SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
        SceneMgr.Get().RegisterScenePreLoadEvent(new SceneMgr.ScenePreLoadCallback(this.OnScenePreLoad));
        ReconnectMgr.Get().AddTimeoutListener(new ReconnectMgr.TimeoutCallback(this.OnReconnectTimeout));
    }

    private bool OnReconnectTimeout(object userData)
    {
        this.HandleGameCanceled();
        this.ChangeFindGameState(FindGameState.CLIENT_CANCELED);
        this.ChangeFindGameState(FindGameState.INVALID);
        return false;
    }

    private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
    {
        this.PreloadTransitionPopup();
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
        {
            this.DestroyTransitionPopup();
        }
        if ((mode == SceneMgr.Mode.GAMEPLAY) && (prevMode != SceneMgr.Mode.GAMEPLAY))
        {
            Screen.sleepTimeout = -1;
        }
        else if (((prevMode == SceneMgr.Mode.GAMEPLAY) && (mode != SceneMgr.Mode.GAMEPLAY)) && !SpectatorManager.Get().IsInSpectatorMode())
        {
            Screen.sleepTimeout = -2;
        }
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        if ((prevMode == SceneMgr.Mode.GAMEPLAY) && (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY))
        {
            this.OnGameEnded();
        }
    }

    private void OnServerResult()
    {
        if (this.IsFindingGame())
        {
            ServerResult serverResult = ConnectAPI.GetServerResult();
            if (serverResult.ResultCode == 1)
            {
                float a = !serverResult.HasRetryDelaySeconds ? 2f : serverResult.RetryDelaySeconds;
                a = Mathf.Max(a, 0.5f);
                ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.OnServerResult_Retry), null);
                ApplicationMgr.Get().ScheduleCallback(a, true, new ApplicationMgr.ScheduledCallback(this.OnServerResult_Retry), null);
            }
            else if (serverResult.ResultCode == 2)
            {
                this.OnGameCanceled();
            }
        }
    }

    private void OnServerResult_Retry(object userData)
    {
        Network.Get().RetryGotoGameServer();
    }

    private void OnTransitionPopupCanceled()
    {
        bool flag = Network.Get().IsFindingGame();
        if (flag)
        {
            Network.Get().CancelFindGame();
        }
        this.ChangeFindGameState(FindGameState.CLIENT_CANCELED);
        if (!flag)
        {
            this.ChangeFindGameState(FindGameState.INVALID);
        }
    }

    private void PreloadTransitionPopup()
    {
        switch (SceneMgr.Get().GetMode())
        {
            case SceneMgr.Mode.TOURNAMENT:
            case SceneMgr.Mode.DRAFT:
            case SceneMgr.Mode.TAVERN_BRAWL:
                this.LoadTransitionPopup((string) this.MATCHING_POPUP_NAME);
                break;

            case SceneMgr.Mode.FRIENDLY:
            case SceneMgr.Mode.ADVENTURE:
                this.LoadTransitionPopup("LoadingPopup");
                break;
        }
    }

    public void PreparePostGameSceneMode(SceneMgr.Mode mode)
    {
        if ((mode == SceneMgr.Mode.ADVENTURE) && (AdventureConfig.Get().GetCurrentSubScene() == AdventureSubScenes.Chooser))
        {
            int missionId = Get().GetMissionId();
            DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
            if (record != null)
            {
                int @int = record.GetInt("ADVENTURE_ID");
                if (@int != 0)
                {
                    int num3 = record.GetInt("MODE_ID");
                    if (num3 != 0)
                    {
                        AdventureConfig.Get().SetSelectedAdventureMode((AdventureDbId) @int, (AdventureModeDbId) num3);
                        AdventureConfig.Get().ChangeSubSceneToSelectedAdventure();
                        AdventureConfig.Get().SetMission((ScenarioDbId) missionId, false);
                    }
                }
            }
        }
    }

    public void ReconnectGame(GameType type, ReconnectType reconnectType, BattleNet.GameServerInfo serverInfo)
    {
        this.m_nextGameType = type;
        this.m_nextMissionId = serverInfo.Mission;
        this.m_nextReconnectType = reconnectType;
        this.m_nextSpectator = serverInfo.SpectatorMode;
        this.m_lastEnterGameError = 0;
        this.ChangeFindGameState(FindGameState.CLIENT_STARTED);
        this.ChangeFindGameState(FindGameState.SERVER_GAME_CONNECTING, serverInfo);
    }

    public void RegisterFindGameEvent(FindGameCallback callback)
    {
        this.RegisterFindGameEvent(callback, null);
    }

    public void RegisterFindGameEvent(FindGameCallback callback, object userData)
    {
        FindGameListener item = new FindGameListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_findGameListeners.Contains(item))
        {
            this.m_findGameListeners.Add(item);
        }
    }

    public void SetPendingAutoConcede(bool pendingAutoConcede)
    {
        if (Network.IsConnectedToGameServer())
        {
            this.m_pendingAutoConcede = pendingAutoConcede;
        }
    }

    private void ShowTransitionPopup(string popupName)
    {
        System.Type type = this.s_transitionPopupNameToType[popupName];
        if ((this.m_transitionPopup == null) || (this.m_transitionPopup.GetType() != type))
        {
            this.DestroyTransitionPopup();
            this.LoadTransitionPopup(popupName);
        }
        if (!this.m_transitionPopup.IsShown())
        {
            if ((Box.Get() != null) && (Box.Get().GetState() != Box.State.OPEN))
            {
                Vector3 vector2 = Box.Get().m_Camera.GetCameraPosition(BoxCamera.State.OPENED) - this.m_initialTransitionPopupPos;
                Vector3 vector4 = Box.Get().GetBoxCamera().transform.position - vector2;
                this.m_transitionPopup.transform.position = vector4;
            }
            AdventureDbId adventureId = GameUtils.GetAdventureId(this.m_nextMissionId);
            this.m_transitionPopup.SetAdventureId(adventureId);
            this.m_transitionPopup.Show();
        }
    }

    public void SpectateGame(JoinInfo joinInfo)
    {
        BattleNet.GameServerInfo serverInfo = new BattleNet.GameServerInfo {
            Address = joinInfo.ServerIpAddress,
            Port = (int) joinInfo.ServerPort,
            GameHandle = joinInfo.GameHandle,
            SpectatorPassword = joinInfo.SecretKey,
            SpectatorMode = true
        };
        this.m_nextGameType = joinInfo.GameType;
        this.m_nextMissionId = joinInfo.MissionId;
        this.m_nextSpectator = true;
        this.m_lastEnterGameError = 0;
        this.ChangeFindGameState(FindGameState.CLIENT_STARTED);
        this.ShowTransitionPopup("LoadingPopup");
        this.ChangeFindGameState(FindGameState.SERVER_GAME_CONNECTING, serverInfo);
        if (Gameplay.Get() == null)
        {
            Network.Get().SetGameServerDisconnectEventListener(new Network.GameServerDisconnectEvent(this.OnGameServerDisconnect));
        }
    }

    public bool UnregisterFindGameEvent(FindGameCallback callback)
    {
        return this.UnregisterFindGameEvent(callback, null);
    }

    public bool UnregisterFindGameEvent(FindGameCallback callback, object userData)
    {
        FindGameListener item = new FindGameListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_findGameListeners.Remove(item);
    }

    public void UpdatePresence()
    {
        if (Network.ShouldBeConnectedToAurora())
        {
            if (this.IsSpectator())
            {
                PresenceMgr mgr = PresenceMgr.Get();
                if (this.IsTutorial())
                {
                    Enum[] args = new Enum[] { PresenceStatus.SPECTATING_GAME_TUTORIAL };
                    mgr.SetStatus(args);
                }
                else if (this.IsPractice())
                {
                    Enum[] enumArray2 = new Enum[] { PresenceStatus.SPECTATING_GAME_PRACTICE };
                    mgr.SetStatus(enumArray2);
                }
                else if (this.IsPlay())
                {
                    Enum[] enumArray3 = new Enum[] { PresenceStatus.SPECTATING_GAME_PLAY };
                    mgr.SetStatus(enumArray3);
                }
                else if (this.IsArena())
                {
                    Enum[] enumArray4 = new Enum[] { PresenceStatus.SPECTATING_GAME_ARENA };
                    mgr.SetStatus(enumArray4);
                }
                else if (this.IsFriendly())
                {
                    Enum[] enumArray5 = new Enum[] { PresenceStatus.SPECTATING_GAME_FRIENDLY };
                    mgr.SetStatus(enumArray5);
                }
                else if (this.IsTavernBrawl())
                {
                    Enum[] enumArray6 = new Enum[] { PresenceStatus.SPECTATING_GAME_TAVERN_BRAWL };
                    mgr.SetStatus(enumArray6);
                }
                else if (this.IsExpansionMission())
                {
                    ScenarioDbId missionId = (ScenarioDbId) this.m_missionId;
                    mgr.SetStatus_SpectatingMission(missionId);
                }
                SpectatorManager.Get().UpdateMySpectatorInfo();
            }
            else
            {
                if (!this.IsTutorial())
                {
                    if (this.IsPractice())
                    {
                        Enum[] enumArray13 = new Enum[] { PresenceStatus.PRACTICE_GAME };
                        PresenceMgr.Get().SetStatus(enumArray13);
                    }
                    else if (this.IsPlay())
                    {
                        Enum[] enumArray14 = new Enum[] { PresenceStatus.PLAY_GAME };
                        PresenceMgr.Get().SetStatus(enumArray14);
                    }
                    else if (this.IsArena())
                    {
                        int wins = DraftManager.Get().GetWins();
                        if (wins >= 8)
                        {
                            int losses = DraftManager.Get().GetLosses();
                            object[] objArray1 = new object[] { wins, ",", losses, ",0" };
                            BnetPresenceMgr.Get().SetGameField(3, string.Concat(objArray1));
                        }
                        Enum[] enumArray15 = new Enum[] { PresenceStatus.ARENA_GAME };
                        PresenceMgr.Get().SetStatus(enumArray15);
                    }
                    else if (this.IsFriendly())
                    {
                        PresenceStatus status = PresenceStatus.FRIENDLY_GAME;
                        if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
                        {
                            status = PresenceStatus.TAVERN_BRAWL_FRIENDLY_GAME;
                        }
                        Enum[] enumArray16 = new Enum[] { status };
                        PresenceMgr.Get().SetStatus(enumArray16);
                    }
                    else if (this.IsTavernBrawl())
                    {
                        Enum[] enumArray17 = new Enum[] { PresenceStatus.TAVERN_BRAWL_GAME };
                        PresenceMgr.Get().SetStatus(enumArray17);
                    }
                    else if (this.IsExpansionMission())
                    {
                        ScenarioDbId id2 = (ScenarioDbId) this.m_missionId;
                        PresenceMgr.Get().SetStatus_PlayingMission(id2);
                    }
                }
                else
                {
                    switch (((ScenarioDbId) this.m_missionId))
                    {
                        case ScenarioDbId.TUTORIAL_HOGGER:
                        {
                            Enum[] enumArray7 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.HOGGER };
                            PresenceMgr.Get().SetStatus(enumArray7);
                            break;
                        }
                        case ScenarioDbId.TUTORIAL_MILLHOUSE:
                        {
                            Enum[] enumArray8 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.MILLHOUSE };
                            PresenceMgr.Get().SetStatus(enumArray8);
                            break;
                        }
                        case ScenarioDbId.TUTORIAL_ILLIDAN:
                        {
                            Enum[] enumArray11 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.ILLIDAN };
                            PresenceMgr.Get().SetStatus(enumArray11);
                            break;
                        }
                        case ScenarioDbId.TUTORIAL_CHO:
                        {
                            Enum[] enumArray12 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.CHO };
                            PresenceMgr.Get().SetStatus(enumArray12);
                            break;
                        }
                        case ScenarioDbId.TUTORIAL_MUKLA:
                        {
                            Enum[] enumArray9 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.MUKLA };
                            PresenceMgr.Get().SetStatus(enumArray9);
                            break;
                        }
                        case ScenarioDbId.TUTORIAL_NESINGWARY:
                        {
                            Enum[] enumArray10 = new Enum[] { PresenceStatus.TUTORIAL_GAME, PresenceTutorial.HEMET };
                            PresenceMgr.Get().SetStatus(enumArray10);
                            break;
                        }
                    }
                }
                SpectatorManager.Get().UpdateMySpectatorInfo();
            }
        }
    }

    public void WaitForFriendChallengeToStart(int missionId)
    {
        this.m_nextGameType = GameType.GT_VS_FRIEND;
        this.m_nextMissionId = missionId;
        this.m_lastEnterGameError = 0;
        this.ChangeFindGameState(FindGameState.CLIENT_STARTED);
        this.ShowTransitionPopup("LoadingPopup");
    }

    public bool WasAI()
    {
        return GameUtils.IsAIMission(this.m_prevMissionId);
    }

    public bool WasArena()
    {
        return (this.m_prevGameType == GameType.GT_ARENA);
    }

    public bool WasClassicMission()
    {
        return GameUtils.IsClassicMission(this.m_prevMissionId);
    }

    public bool WasExpansionMission()
    {
        return GameUtils.IsExpansionMission(this.m_prevMissionId);
    }

    public bool WasFriendly()
    {
        return (this.m_prevGameType == GameType.GT_VS_FRIEND);
    }

    public bool WasPlay()
    {
        return (this.WasRankedPlay() || this.WasUnrankedPlay());
    }

    public bool WasPractice()
    {
        return GameUtils.IsPracticeMission(this.m_prevMissionId);
    }

    public bool WasRankedPlay()
    {
        return (this.m_prevGameType == GameType.GT_RANKED);
    }

    public bool WasSpectator()
    {
        return this.m_prevSpectator;
    }

    public bool WasTutorial()
    {
        return GameUtils.IsTutorialMission(this.m_prevMissionId);
    }

    public bool WasUnrankedPlay()
    {
        return (this.m_prevGameType == GameType.GT_UNRANKED);
    }

    private void WillReset()
    {
        this.m_gameType = GameType.GT_UNKNOWN;
        this.m_prevGameType = GameType.GT_UNKNOWN;
        this.m_nextGameType = GameType.GT_UNKNOWN;
        this.m_missionId = 0;
        this.m_prevMissionId = 0;
        this.m_nextMissionId = 0;
        this.m_reconnectType = ReconnectType.INVALID;
        this.m_prevReconnectType = ReconnectType.INVALID;
        this.m_nextReconnectType = ReconnectType.INVALID;
        this.m_nextSpectator = false;
        this.m_lastEnterGameError = 0;
        this.m_findGameState = FindGameState.INVALID;
        this.m_gameSetup = null;
    }

    public delegate bool FindGameCallback(FindGameEventData eventData, object userData);

    private class FindGameListener : EventListener<GameMgr.FindGameCallback>
    {
        public bool Fire(FindGameEventData eventData)
        {
            return base.m_callback(eventData, base.m_userData);
        }
    }
}

