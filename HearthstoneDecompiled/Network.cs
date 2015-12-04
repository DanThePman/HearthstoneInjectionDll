using BobNetProto;
using PegasusGame;
using PegasusShared;
using PegasusUtil;
using SpectatorProto;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using WTCG.BI;

public class Network
{
    [CompilerGenerated]
    private static SpectatorInviteReceivedHandler <>f__am$cache25;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map2C;
    private DateTime lastCall = DateTime.Now;
    private DateTime lastCallReport = DateTime.Now;
    public static readonly PlatformDependentValue<bool> LAUNCHES_WITH_BNET_APP;
    private static readonly TimeSpan LOGIN_TIMEOUT = new TimeSpan(0, 0, 5);
    private BnetEventHandler m_bnetEventHandler;
    private ChallengeHandler m_challengeHandler;
    private static SortedDictionary<int, int> m_deferredGetAccountInfoMessageResponseMap;
    private static SortedDictionary<int, int> m_deferredMessageResponseMap;
    private string m_delayedError;
    private Map<BnetFeature, List<BnetErrorListener>> m_featureBnetErrorListeners;
    private BnetGameType m_findingBnetGameType;
    private FriendsHandler m_friendsHandler;
    private GameQueueHandler m_gameQueueHandler;
    private GameServerDisconnectEvent m_gameServerDisconnectEventListener;
    private List<BnetErrorListener> m_globalBnetErrorListeners;
    private static List<RequestContext> m_inTransitRequests = new List<RequestContext>();
    private BattleNet.GameServerInfo m_lastGameServerInfo;
    private DateTime m_loginStarted;
    private bool m_loginWaiting = true;
    private BattleNetLogSource m_logSource = new BattleNetLogSource("Network");
    private Map<int, List<NetHandler>> m_netHandlers = new Map<int, List<NetHandler>>();
    private Map<int, TimeoutHandler> m_netTimeoutHandlers = new Map<int, TimeoutHandler>();
    private PartyHandler m_partyHandler;
    private PresenceHandler m_presenceHandler;
    private QueueInfoHandler m_queueInfoHandler;
    private ShutdownHandler m_shutdownHandler;
    private SpectatorInviteReceivedHandler m_spectatorInviteReceivedHandler;
    private float m_timeBeforeAllowReset;
    private WhisperHandler m_whisperHandler;
    private static TimeSpan MAX_DEFERRED_WAIT = new TimeSpan(0, 0, 120);
    private const int MIN_DEFERRED_WAIT = 30;
    public const int NoPosition = 0;
    public const int NoSubOption = -1;
    private static readonly TimeSpan PROCESS_WARNING = new TimeSpan(0, 0, 15);
    private static readonly TimeSpan PROCESS_WARNING_REPORT_GAP = new TimeSpan(0, 0, 1);
    private static Network s_instance = new Network();
    private static int s_numConnectionFailures = 0;
    private static bool s_running;
    private static bool s_shouldBeConnectedToAurora;
    public static readonly PlatformDependentValue<bool> TUTORIALS_WITHOUT_ACCOUNT;

    static Network()
    {
        SortedDictionary<int, int> dictionary = new SortedDictionary<int, int>();
        dictionary.Add(0x131, 0x132);
        dictionary.Add(0x12f, 0x130);
        dictionary.Add(0x10b, 330);
        dictionary.Add(0xcd, 0x133);
        dictionary.Add(0xfd, 0xfc);
        dictionary.Add(0x13a, 0x13b);
        m_deferredMessageResponseMap = dictionary;
        dictionary = new SortedDictionary<int, int>();
        dictionary.Add(11, 0xe9);
        dictionary.Add(6, 0xe0);
        dictionary.Add(14, 0xf1);
        dictionary.Add(0x12, 0x108);
        dictionary.Add(4, 0xe8);
        dictionary.Add(12, 0xd4);
        dictionary.Add(2, 0xca);
        dictionary.Add(3, 0xcf);
        dictionary.Add(10, 0xe7);
        dictionary.Add(15, 260);
        dictionary.Add(0x11, 0x106);
        dictionary.Add(0x17, 300);
        dictionary.Add(0x13, 0x10f);
        dictionary.Add(8, 270);
        dictionary.Add(20, 0x116);
        dictionary.Add(0x15, 0x11b);
        dictionary.Add(7, 0xec);
        dictionary.Add(0x1b, 0x13e);
        dictionary.Add(0x1c, 0x145);
        dictionary.Add(0x18, 0x139);
        m_deferredGetAccountInfoMessageResponseMap = dictionary;
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = true,
            Mac = true,
            iOS = false,
            Android = false
        };
        LAUNCHES_WITH_BNET_APP = value2;
        value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = false,
            Mac = false,
            iOS = true,
            Android = true
        };
        TUTORIALS_WITHOUT_ACCOUNT = value2;
        s_shouldBeConnectedToAurora = true;
    }

    public Network()
    {
        if (<>f__am$cache25 == null)
        {
            <>f__am$cache25 = new SpectatorInviteReceivedHandler(Network.<m_spectatorInviteReceivedHandler>m__13D);
        }
        this.m_spectatorInviteReceivedHandler = <>f__am$cache25;
        this.m_featureBnetErrorListeners = new Map<BnetFeature, List<BnetErrorListener>>();
        this.m_globalBnetErrorListeners = new List<BnetErrorListener>();
    }

    [CompilerGenerated]
    private static void <m_spectatorInviteReceivedHandler>m__13D(Invite)
    {
    }

    public static void AcceptFriendChallenge(BnetEntityId partyId)
    {
        BattleNet.AcceptFriendlyChallenge(ref BnetEntityId.CreateForDll(partyId));
    }

    public static void AcceptFriendInvite(BnetInvitationId inviteId)
    {
        BattleNet.ManageFriendInvite(1, inviteId.GetVal());
    }

    public static AchieveList Achieves()
    {
        return ConnectAPI.GetAchieves();
    }

    public static void AckAchieveProgress(int ID, int ackProgress)
    {
        ConnectAPI.AckAchieveProgress(ID, ackProgress);
    }

    public static void AckCardSeenBefore(int assetID, CardFlair cardFlair)
    {
        ConnectAPI.AckCardSeen(assetID, (int) cardFlair.Premium);
    }

    public static void AckDraftRewards(long deckID, int slot)
    {
        ConnectAPI.DraftAckRewards(deckID, slot);
    }

    public static void AckNotice(long ID)
    {
        if (NetCache.Get().RemoveNotice(ID))
        {
            ConnectAPI.AckNotice(ID);
        }
    }

    public static void AcknowledgeBanner(int banner)
    {
        ConnectAPI.AcknowledgeBanner(banner);
    }

    public static void AckWingProgress(int wingId, int ackId)
    {
        ConnectAPI.AckWingProgress(wingId, ackId);
    }

    public void AddBnetErrorListener(BnetErrorCallback callback)
    {
        this.AddBnetErrorListener(callback, null);
    }

    public void AddBnetErrorListener(BnetFeature feature, BnetErrorCallback callback)
    {
        this.AddBnetErrorListener(feature, callback, null);
    }

    public void AddBnetErrorListener(BnetErrorCallback callback, object userData)
    {
        BnetErrorListener item = new BnetErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_globalBnetErrorListeners.Contains(item))
        {
            this.m_globalBnetErrorListeners.Add(item);
        }
    }

    public void AddBnetErrorListener(BnetFeature feature, BnetErrorCallback callback, object userData)
    {
        List<BnetErrorListener> list;
        BnetErrorListener item = new BnetErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_featureBnetErrorListeners.TryGetValue(feature, out list))
        {
            list = new List<BnetErrorListener>();
            this.m_featureBnetErrorListeners.Add(feature, list);
        }
        else if (list.Contains(item))
        {
            return;
        }
        list.Add(item);
    }

    public static void AddPendingRequestTimeout(int requestId, int requestSubId)
    {
        if (ShouldBeConnectedToAurora())
        {
            int num = 0;
            if (((requestId == 0xc9) && m_deferredGetAccountInfoMessageResponseMap.TryGetValue(requestSubId, out num)) || m_deferredMessageResponseMap.TryGetValue(requestId, out num))
            {
                TimeoutHandler handler = null;
                if (s_instance.m_netTimeoutHandlers.TryGetValue(num, out handler))
                {
                    m_inTransitRequests.Add(new RequestContext(num, handler));
                }
                else
                {
                    m_inTransitRequests.Add(new RequestContext(num, new TimeoutHandler(Network.OnRequestTimeout)));
                }
            }
        }
    }

    public void AnswerChallenge(ulong challengeID, string answer)
    {
        BattleNet.AnswerChallenge(challengeID, answer);
    }

    public static void AppAbort()
    {
        if (s_running)
        {
            ConcedeIfReconnectDisabled();
            s_instance.CancelFindGame();
            BattleNet.AppQuit();
            PlayErrors.AppQuit();
            BnetNearbyPlayerMgr.Get().Shutdown();
            s_running = false;
        }
    }

    public static void ApplicationPaused()
    {
        BattleNet.ApplicationWasPaused();
    }

    public static void ApplicationUnpaused()
    {
        BattleNet.ApplicationWasUnpaused();
    }

    public static void AppQuit()
    {
        if (s_running)
        {
            TrackClient(TrackLevel.LEVEL_INFO, TrackWhat.TRACK_LOGOUT_STARTING);
            ConcedeIfReconnectDisabled();
            s_instance.CancelFindGame();
            BattleNet.AppQuit();
            PlayErrors.AppQuit();
            BnetNearbyPlayerMgr.Get().Shutdown();
            s_running = false;
        }
    }

    public static void AutoConcede()
    {
        if (IsConnectedToGameServer() && !WasGameConceded())
        {
            Concede();
            ClientConnection<PegasusPacket> gameServerConnection = ConnectAPI.GetGameServerConnection();
            gameServerConnection.SendQueuedPackets();
            while (gameServerConnection.HasOutPacketsInFlight())
            {
            }
            Thread.Sleep(100);
        }
    }

    public static BnetLoginState BattleNetStatus()
    {
        return (BnetLoginState) BattleNet.BattleNetStatus();
    }

    public static void BeginThirdPartyPurchase(BattlePayProvider provider, string productID, int quantity)
    {
        ConnectAPI.BeginThirdPartyPurchase(provider, productID, quantity);
    }

    public static void BeginThirdPartyPurchaseWithReceipt(BattlePayProvider provider, string productID, int quantity, string thirdPartyID, string base64receipt, string thirdPartyUserID = "")
    {
        ConnectAPI.BeginThirdPartyPurchaseWithReceipt(provider, productID, quantity, thirdPartyID, base64receipt, thirdPartyUserID);
    }

    public static void BuyCard(int assetID, CardFlair cardFlair, int count, int unitBuyPrice)
    {
        ConnectAPI.BuyCard(assetID, (int) cardFlair.Premium, count, unitBuyPrice);
    }

    public static void CancelBlizzardPurchase(bool isAutoCanceled)
    {
        ConnectAPI.AbortBlizzardPurchase(isAutoCanceled);
    }

    public void CancelChallenge(ulong challengeID)
    {
        BattleNet.CancelChallenge(challengeID);
    }

    public void CancelFindGame()
    {
        if (this.m_findingBnetGameType != BnetGameType.BGT_UNKNOWN)
        {
            BattleNet.CancelFindGame();
            this.m_findingBnetGameType = BnetGameType.BGT_UNKNOWN;
        }
    }

    public static void CancelThirdPartyPurchase(CancelPurchase.ThirdPartyCancelReason reason)
    {
        ConnectAPI.AbortThirdPartyPurchase(reason);
    }

    private bool CanIgnoreUnhandledPacket(int id)
    {
        int num = id;
        if (((num != 15) && (num != 0x74)) && (num != 0xfe))
        {
            return false;
        }
        return true;
    }

    public static void CheckAccountLicenseAchieve(int achieveID)
    {
        ConnectAPI.CheckAccountLicenseAchieve(achieveID);
    }

    public static void ChooseFriendChallengeDeck(BnetEntityId partyId, long deckID)
    {
        BattleNet.SetPartyDeck(ref BnetEntityId.CreateForDll(partyId), deckID);
    }

    public void ClearLastGameServerJoined()
    {
        this.m_lastGameServerInfo = null;
    }

    public static void CloseCardMarket()
    {
    }

    public static void Concede()
    {
        ConnectAPI.Concede();
    }

    private static void ConcedeIfReconnectDisabled()
    {
        if (!ReconnectMgr.Get().IsReconnectEnabled())
        {
            AutoConcede();
        }
    }

    public static void ConfirmPurchase()
    {
        ConnectAPI.ConfirmPurchase();
    }

    public void CreateDeck(DeckType deckType, string name, int heroDatabaseAssetID, TAG_PREMIUM heroPremium)
    {
        Log.Rachelle.Print(string.Format("Network.CreateDeck hero={0},premium={1}", heroDatabaseAssetID, heroPremium), new object[0]);
        ConnectAPI.CreateDeck(deckType, name, heroDatabaseAssetID, heroPremium);
    }

    public static void DeclineFriendChallenge(BnetEntityId partyId)
    {
        BattleNet.DeclineFriendlyChallenge(ref BnetEntityId.CreateForDll(partyId));
    }

    public static void DeclineFriendInvite(BnetInvitationId inviteId)
    {
        BattleNet.ManageFriendInvite(3, inviteId.GetVal());
    }

    private float DelayForConnectionFailures(int numFailures)
    {
        float num = ((float) (new System.Random().NextDouble() * 3.0)) + 3.5f;
        return (Math.Min(numFailures, 3) * num);
    }

    public void DeleteDeck(long deck)
    {
        Log.Rachelle.Print(string.Format("Network.DeleteDeck {0}", deck), new object[0]);
        ConnectAPI.DeleteDeck(deck);
    }

    public static void DisconnectFromGameServer()
    {
        if (IsConnectedToGameServer())
        {
            ConnectAPI.DisconnectFromGameServer();
        }
    }

    public bool FakeHandleType(int id)
    {
        if (!ShouldBeConnectedToAurora())
        {
            this.HandleType(id);
            return true;
        }
        return false;
    }

    public bool FakeHandleType(object enumId)
    {
        int id = (int) enumId;
        return this.FakeHandleType(id);
    }

    public void FindGame(GameType gameType, int missionId, long deckId, long aiDeckId)
    {
        if (gameType == GameType.GT_VS_FRIEND)
        {
            Error.AddDevFatal("Network.FindGame() - friend games must go through the Party API", new object[0]);
        }
        else
        {
            BnetGameType type = TranslateGameTypeToBnet(gameType, missionId);
            if (type != BnetGameType.BGT_UNKNOWN)
            {
                this.m_findingBnetGameType = type;
                BattleNet.FindGame(type, missionId, deckId, aiDeckId);
            }
        }
    }

    public static void FindOutCurrentDraftState()
    {
        ConnectAPI.DraftGetPicksAndContents();
    }

    private bool FireErrorListeners(BnetErrorInfo info)
    {
        List<BnetErrorListener> list;
        bool flag = false;
        if (this.m_featureBnetErrorListeners.TryGetValue(info.GetFeature(), out list) && (list.Count > 0))
        {
            foreach (BnetErrorListener listener in list.ToArray())
            {
                flag = listener.Fire(info) || flag;
            }
        }
        foreach (BnetErrorListener listener2 in this.m_globalBnetErrorListeners.ToArray())
        {
            flag = listener2.Fire(info) || flag;
        }
        return flag;
    }

    public static Network Get()
    {
        return s_instance;
    }

    public static AccountLicenseAchieveResponse GetAccountLicenseAchieveResponse()
    {
        return ConnectAPI.GetAccountLicenseAchieveResponse();
    }

    public static List<AdventureProgress> GetAdventureProgressResponse()
    {
        return ConnectAPI.GetAdventureProgressResponse();
    }

    public static void GetAllClientOptions()
    {
        ConnectAPI.GetAllClientOptions();
    }

    public static int GetAssetsVersion()
    {
        return ConnectAPI.GetAssetsVersion();
    }

    public static BattlePayConfig GetBattlePayConfigResponse()
    {
        return ConnectAPI.GetBattlePayConfigResponse();
    }

    public static BattlePayStatus GetBattlePayStatusResponse()
    {
        return ConnectAPI.GetBattlePayStatusResponse();
    }

    public BnetEventHandler GetBnetEventHandler()
    {
        return this.m_bnetEventHandler;
    }

    public static CanceledQuest GetCanceledQuest()
    {
        return ConnectAPI.GetCanceledQuest();
    }

    public static CardSaleResult GetCardSaleResult()
    {
        return ConnectAPI.GetCardSaleResult();
    }

    public ChallengeHandler GetChallengeHandler()
    {
        return this.m_challengeHandler;
    }

    public static DraftChosen GetChosenAndNext()
    {
        return ConnectAPI.DraftCardChosen();
    }

    public static NetCache.DeckHeader GetCreatedDeck()
    {
        return ConnectAPI.DeckCreated();
    }

    public static DeckContents GetDeckContents()
    {
        return ConnectAPI.GetDeckContents();
    }

    public void GetDeckContents(long deck)
    {
        Log.Bob.Print(string.Format("Network.GetDeckContents {0}", deck), new object[0]);
        ConnectAPI.GetDeckContents(deck);
    }

    public static DBAction GetDeckResponse()
    {
        return ConnectAPI.DBAction();
    }

    public static long GetDeletedDeckID()
    {
        return ConnectAPI.DeckDeleted();
    }

    public static DraftChoicesAndContents GetDraftChoicesAndContents()
    {
        return ConnectAPI.DraftGetChoicesAndContents();
    }

    public static DraftError GetDraftError()
    {
        return (DraftError) ConnectAPI.DraftGetError();
    }

    public static EntitiesChosen GetEntitiesChosen()
    {
        return ConnectAPI.GetEntitiesChosen();
    }

    public static EntityChoices GetEntityChoices()
    {
        return ConnectAPI.GetEntityChoices();
    }

    public BnetGameType GetFindingBnetGameType()
    {
        return this.m_findingBnetGameType;
    }

    public FriendsHandler GetFriendsHandler()
    {
        return this.m_friendsHandler;
    }

    public static GameCancelInfo GetGameCancelInfo()
    {
        return ConnectAPI.GetGameCancelInfo();
    }

    public static GameSetup GetGameSetupInfo()
    {
        return ConnectAPI.GetGameSetup();
    }

    public void GetGameState()
    {
        ConnectAPI.GetGameState();
    }

    public static UnavailableReason GetHearthstoneUnavailable(bool gamePacket)
    {
        UnavailableReason reason = new UnavailableReason();
        if (gamePacket)
        {
            Deadend deadendGame = ConnectAPI.GetDeadendGame();
            reason.mainReason = deadendGame.Reply1;
            reason.subReason = deadendGame.Reply2;
            reason.extraData = deadendGame.Reply3;
            return reason;
        }
        DeadendUtil deadendUtil = ConnectAPI.GetDeadendUtil();
        reason.mainReason = deadendUtil.Reply1;
        reason.subReason = deadendUtil.Reply2;
        reason.extraData = deadendUtil.Reply3;
        return reason;
    }

    public static List<int> GetIntArray(List<int> ints, int size)
    {
        return ints;
    }

    public static List<int> GetIntArray(IntPtr ints, int size)
    {
        List<int> list = new List<int>();
        int[] numArray = IntPtrToIntArray(ints, size);
        for (int i = 0; i < size; i++)
        {
            list.Add(numArray[i]);
        }
        return list;
    }

    public BattleNet.GameServerInfo GetLastGameServerJoined()
    {
        return this.m_lastGameServerInfo;
    }

    public static MassDisenchantResponse GetMassDisenchantResponse()
    {
        return ConnectAPI.GetMassDisenchantResponse();
    }

    public static TimeSpan GetMaxDeferredWait()
    {
        return MAX_DEFERRED_WAIT;
    }

    public static int GetNAckOption()
    {
        return ConnectAPI.GetNAckOption();
    }

    public static BeginDraft GetNewDraftDeckID()
    {
        return ConnectAPI.DraftGetBeginning();
    }

    public static Options GetOptions()
    {
        return ConnectAPI.GetOptions();
    }

    public PartyHandler GetPartyHandler()
    {
        return this.m_partyHandler;
    }

    public static List<PowerHistory> GetPowerHistory()
    {
        return ConnectAPI.GetPowerHistory();
    }

    public PresenceHandler GetPresenceHandler()
    {
        return this.m_presenceHandler;
    }

    public static PurchaseCanceledResponse GetPurchaseCanceledResponse()
    {
        return ConnectAPI.GetPurchaseCanceledResponse();
    }

    public static void GetPurchaseMethod(string productID, int quantity, Currency currency)
    {
        ConnectAPI.RequestPurchaseMethod(productID, quantity, (int) currency);
    }

    public static PurchaseMethod GetPurchaseMethodResponse()
    {
        return ConnectAPI.GetPurchaseMethodResponse();
    }

    public static PurchaseResponse GetPurchaseResponse()
    {
        return ConnectAPI.GetPurchaseResponse();
    }

    public static PurchaseViaGoldResponse GetPurchaseWithGoldResponse()
    {
        return ConnectAPI.GetPurchaseWithGoldResponse();
    }

    public static BattleNet.QueueEvent GetQueueEvent()
    {
        return BattleNet.GetQueueEvent();
    }

    public static DeckName GetRenamedDeck()
    {
        return ConnectAPI.DeckRenamed();
    }

    public static DraftRetired GetRetiredDraft()
    {
        return ConnectAPI.DraftHandleRetired();
    }

    public static long GetRewardsAckDraftID()
    {
        return ConnectAPI.DraftHandleRewardsAck();
    }

    public static SetFavoriteHeroResponse GetSetFavoriteHeroResponse()
    {
        return ConnectAPI.GetSetFavoriteHeroResponse();
    }

    public ShutdownHandler GetShutdownHandler()
    {
        return this.m_shutdownHandler;
    }

    public static SpectatorNotify GetSpectatorNotify()
    {
        return ConnectAPI.GetSpectatorNotify();
    }

    public static List<Entity.Tag> GetTags(List<Entity.Tag> tags)
    {
        return tags;
    }

    public static List<Entity.Tag> GetTags(IntPtr src, IntPtr flags)
    {
        List<Entity.Tag> list = new List<Entity.Tag>();
        int[] numArray = IntPtrToIntArray(src, 0x200);
        int[] numArray2 = IntPtrToIntArray(flags, 0x200);
        for (int i = 0; i < 0x200; i++)
        {
            if (numArray2[i] == 1)
            {
                Entity.Tag item = new Entity.Tag {
                    Name = i,
                    Value = numArray[i]
                };
                list.Add(item);
            }
        }
        return list;
    }

    public static void GetThirdPartyPurchaseStatus(string transactionID)
    {
        ConnectAPI.GetThirdPartyPurchaseStatus(transactionID);
    }

    public static ThirdPartyPurchaseStatusResponse GetThirdPartyPurchaseStatusResponse()
    {
        return ConnectAPI.GetThirdPartyPurchaseStatusResponse();
    }

    public static TriggeredEvent GetTriggerEventResponse()
    {
        return ConnectAPI.GetTriggerEventResponse();
    }

    public static TurnTimerInfo GetTurnTimerInfo()
    {
        return ConnectAPI.GetTurnTimerInfo();
    }

    public static UserUI GetUserUI()
    {
        return ConnectAPI.GetUserUI();
    }

    public static ValidatedAchieve GetValidatedAchieve()
    {
        return ConnectAPI.GetValidatedAchieve();
    }

    public WhisperHandler GetWhisperHandler()
    {
        return this.m_whisperHandler;
    }

    public void GotoGameServer(BattleNet.GameServerInfo info, bool reconnecting)
    {
        this.m_lastGameServerInfo = info;
        ConnectAPI.GotoGameServer(info, reconnecting);
    }

    private bool HandleType(int id)
    {
        List<NetHandler> list;
        RemovePendingRequestTimeout(id);
        if (!this.m_netHandlers.TryGetValue(id, out list) || (list.Count == 0))
        {
            if (!this.CanIgnoreUnhandledPacket(id))
            {
                Debug.LogError(string.Format("Network.HandleType() - Received packet {0}, but there are no handlers for it.", id));
            }
            return false;
        }
        NetHandler[] handlerArray = list.ToArray();
        for (int i = 0; i < handlerArray.Length; i++)
        {
            handlerArray[i]();
        }
        return true;
    }

    public bool HaveUnhandledPackets()
    {
        return (ConnectAPI.HaveUtilPackets() || (ConnectAPI.HaveGamePackets() || (ConnectAPI.HaveDebugPackets() || ConnectAPI.HaveNotificationPackets())));
    }

    public static void Heartbeat()
    {
        if (s_running)
        {
            ProcessRequestTimeouts();
            NetCache.Get().Heartbeat();
            ConnectAPI.Heartbeat();
            StoreManager.Get().Heartbeat();
            if (AchieveManager.Get() != null)
            {
                AchieveManager.Get().Heartbeat();
            }
            NetCache.Get().CheckSeasonForRoll();
            TimeSpan span = (TimeSpan) (DateTime.Now - s_instance.lastCall);
            if (span >= PROCESS_WARNING)
            {
                TimeSpan span2 = (TimeSpan) (DateTime.Now - s_instance.lastCallReport);
                if (span2 >= PROCESS_WARNING_REPORT_GAP)
                {
                    s_instance.lastCallReport = DateTime.Now;
                    string devElapsedTimeString = TimeUtils.GetDevElapsedTimeString(span);
                    Debug.LogWarning(string.Format("Network.ProcessNetwork not called for {0}", devElapsedTimeString));
                }
            }
        }
    }

    public static void IgnoreFriendInvite(BnetInvitationId inviteId)
    {
        BattleNet.ManageFriendInvite(4, inviteId.GetVal());
    }

    public static void Initialize()
    {
        s_running = true;
        NetCache.Get().InitNetCache();
        BattleNet.Init(ApplicationMgr.IsInternal());
        s_instance.RegisterNetHandler(SubscribeResponse.PacketID.ID, new NetHandler(Network.OnSubscribeResponse), null);
        s_instance.RegisterNetHandler(PegasusUtil.GenericResponse.PacketID.ID, new NetHandler(Network.OnGenericResponse), null);
    }

    private static void IntArrayToIntPtr(int[] src, IntPtr dst, int length)
    {
        Marshal.Copy(src, 0, dst, length);
    }

    private static int[] IntPtrToIntArray(IntPtr src, int length)
    {
        int[] destination = new int[length];
        Marshal.Copy(src, destination, 0, length);
        return destination;
    }

    private static long[] IntPtrToLongArray(IntPtr src, int length)
    {
        long[] destination = new long[length];
        Marshal.Copy(src, destination, 0, length);
        return destination;
    }

    public static bool IsConnectedToGameServer()
    {
        return ConnectAPI.IsConnectedToGameServer();
    }

    public bool IsFindingGame()
    {
        return (this.m_findingBnetGameType != BnetGameType.BGT_UNKNOWN);
    }

    public static bool IsLoggedIn()
    {
        if (!BattleNet.IsInitialized())
        {
            return false;
        }
        return (BattleNet.BattleNetStatus() == 4);
    }

    public static bool IsRunning()
    {
        return s_running;
    }

    public static void LoginOk()
    {
        ConnectAPI.OnLoginCompleted();
    }

    private static void LongArrayToIntPtr(long[] src, IntPtr dst, int length)
    {
        Marshal.Copy(src, 0, dst, length);
    }

    private static List<int> MakeChoicesList(int choice1, int choice2, int choice3)
    {
        List<int> list = new List<int>();
        if (choice1 == 0)
        {
            return null;
        }
        list.Add(choice1);
        if (choice2 != 0)
        {
            list.Add(choice2);
            if (choice3 == 0)
            {
                return list;
            }
            list.Add(choice3);
        }
        return list;
    }

    public static void MakeDraftChoice(long deckID, int slot, int index)
    {
        ConnectAPI.DraftMakePick(deckID, slot, index);
    }

    public static void MassDisenchant()
    {
        ConnectAPI.MassDisenchant();
    }

    public void OnBnetError(BnetErrorInfo info)
    {
        if (!this.OnIgnorableBnetError(info))
        {
            this.OnFatalBnetError(info);
        }
    }

    public void OnFatalBnetError(BnetErrorInfo info)
    {
        string str;
        string str2;
        BattleNetErrors error = info.GetError();
        object[] args = new object[] { info };
        this.m_logSource.LogError("Network.OnFatalBnetError() - error={0}", args);
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_ERROR, BIReport.TelemetryEvent.EVENT_FATAL_BNET_ERROR, (int) error, null);
        BattleNetErrors errors2 = error;
        switch (errors2)
        {
            case BattleNetErrors.ERROR_RPC_PEER_UNKNOWN:
                break;

            case BattleNetErrors.ERROR_RPC_PEER_UNAVAILABLE:
                this.ShowConnectionFailureError("GLOBAL_ERROR_UNKNOWN_ERROR");
                Log.JMac.Print(LogLevel.Warning, string.Format("ERROR_RPC_PEER_UNAVAILABLE - {0} connection failures.", s_numConnectionFailures), new object[0]);
                return;

            case BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED:
                str = "GLOBAL_ERROR_NETWORK_DISCONNECT";
                this.ShowConnectionFailureError(str);
                return;

            case BattleNetErrors.ERROR_RPC_REQUEST_TIMED_OUT:
                str = "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT";
                this.ShowConnectionFailureError(str);
                return;

            case BattleNetErrors.ERROR_RPC_CONNECTION_TIMED_OUT:
                this.ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_CONNECTION_TIMEOUT");
                return;

            case BattleNetErrors.ERROR_RPC_QUOTA_EXCEEDED:
                str = "GLOBAL_ERROR_NETWORK_SPAM";
                Error.AddFatalLoc(str, new object[0]);
                return;

            default:
                switch (errors2)
                {
                    case BattleNetErrors.ERROR_GAME_ACCOUNT_BANNED:
                        WebAuth.DeleteStoredToken();
                        WebAuth.DeleteCookies();
                        Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_ACCOUNT_BANNED", new object[0]);
                        return;

                    case BattleNetErrors.ERROR_GAME_ACCOUNT_SUSPENDED:
                        WebAuth.DeleteStoredToken();
                        WebAuth.DeleteCookies();
                        Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_ACCOUNT_SUSPENDED", new object[0]);
                        return;

                    case BattleNetErrors.ERROR_SESSION_DUPLICATE:
                        Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_DUPLICATE_LOGIN", new object[0]);
                        return;

                    case BattleNetErrors.ERROR_SESSION_DISCONNECTED:
                        str = "GLOBAL_ERROR_NETWORK_DISCONNECT";
                        Error.AddFatalLoc(str, new object[0]);
                        return;

                    default:
                        if (errors2 != BattleNetErrors.ERROR_DENIED)
                        {
                            if (errors2 == BattleNetErrors.ERROR_PARENTAL_CONTROL_RESTRICTION)
                            {
                                WebAuth.DeleteStoredToken();
                                WebAuth.DeleteCookies();
                                Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_PARENTAL_CONTROLS", new object[0]);
                                return;
                            }
                            if (errors2 == BattleNetErrors.ERROR_SERVER_IS_PRIVATE)
                            {
                                this.ShowConnectionFailureError("GLOBAL_ERROR_UNKNOWN_ERROR");
                                Log.JMac.Print(LogLevel.Warning, string.Format("ERROR_SERVER_IS_PRIVATE - {0} connection failures.", s_numConnectionFailures), new object[0]);
                                return;
                            }
                            if (errors2 == BattleNetErrors.ERROR_PHONE_LOCK)
                            {
                                Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_PHONE_LOCK", new object[0]);
                                return;
                            }
                            if (errors2 == BattleNetErrors.ERROR_LOGON_WEB_VERIFY_TIMEOUT)
                            {
                                this.ShowConnectionFailureError("GLOBAL_MOBILE_ERROR_LOGON_WEB_TIMEOUT");
                                return;
                            }
                            break;
                        }
                        this.ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
                        return;
                }
                break;
        }
        if (ApplicationMgr.IsInternal())
        {
            str2 = string.Format("Unhandled Bnet Error: {0}", info);
        }
        else
        {
            Debug.LogError(string.Format("Unhandled Bnet Error: {0}", info));
            str2 = GameStrings.Format("GLOBAL_ERROR_UNKNOWN_ERROR", new object[0]);
        }
        this.ShowConnectionFailureError(str2);
    }

    private static void OnGenericResponse()
    {
        GenericResponse genericResponse = ConnectAPI.GetGenericResponse();
        if (genericResponse == null)
        {
            Debug.LogError(string.Format("Login - GenericResponse parse error", new object[0]));
        }
        else
        {
            bool flag = (genericResponse.RequestId == 0xc9) && m_deferredGetAccountInfoMessageResponseMap.ContainsKey(genericResponse.RequestSubId);
            bool flag2 = m_deferredMessageResponseMap.ContainsKey(genericResponse.RequestId);
            if ((flag || flag2) && (genericResponse.ResultCode != GenericResponse.Result.REQUEST_IN_PROCESS))
            {
                Debug.LogError(string.Format("Unhandled resultCode {0} for requestId {1}:{2}", genericResponse.ResultCode, genericResponse.RequestId, genericResponse.RequestSubId));
                FatalErrorMgr.Get().SetErrorCode("HS", "NG" + genericResponse.ResultCode.ToString(), genericResponse.RequestId.ToString(), genericResponse.RequestSubId.ToString());
                Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN", 0f);
            }
        }
    }

    public bool OnIgnorableBnetError(BnetErrorInfo info)
    {
        BattleNetErrors error = info.GetError();
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_ERROR, BIReport.TelemetryEvent.EVENT_IGNORABLE_BNET_ERROR, (int) error, null);
        BattleNetErrors errors2 = error;
        if (errors2 != BattleNetErrors.ERROR_OK)
        {
            if (errors2 != BattleNetErrors.ERROR_WAITING_FOR_DEPENDENCY)
            {
                if (errors2 == BattleNetErrors.ERROR_INVALID_TARGET_ID)
                {
                    return ((info.GetFeature() == BnetFeature.Friends) && (info.GetFeatureEvent() == BnetFeatureEvent.Friends_OnSendInvitation));
                }
                if (errors2 == BattleNetErrors.ERROR_API_NOT_READY)
                {
                    return (info.GetFeature() == BnetFeature.Presence);
                }
                if (errors2 != BattleNetErrors.ERROR_INCOMPLETE_PROFANITY_FILTERS)
                {
                    if (errors2 != BattleNetErrors.ERROR_TARGET_OFFLINE)
                    {
                        if (errors2 == BattleNetErrors.ERROR_PRESENCE_TEMPORARY_OUTAGE)
                        {
                            return true;
                        }
                        if (errors2 != BattleNetErrors.ERROR_GAME_UTILITY_SERVER_NO_SERVER)
                        {
                            return false;
                        }
                        object[] args = new object[] { info };
                        this.m_logSource.LogError("Network.IgnoreBnetError() - error={0}", args);
                    }
                    return true;
                }
                Locale locale = Localization.GetLocale();
                if (locale == Locale.zhCN)
                {
                    object[] objArray2 = new object[] { info, locale };
                    this.m_logSource.LogError("Network.IgnoreBnetError() - error={0} locale={1}", objArray2);
                }
            }
            return true;
        }
        return true;
    }

    public void OnLoginStarted()
    {
        ConnectAPI.OnLoginStarted();
        this.m_loginStarted = DateTime.Now;
    }

    private static void OnRequestTimeout(int pendingResponseId)
    {
        if (m_deferredMessageResponseMap.ContainsValue(pendingResponseId) || m_deferredGetAccountInfoMessageResponseMap.ContainsValue(pendingResponseId))
        {
            Debug.LogError(string.Format("OnRequestTimeout pending ID {0}", pendingResponseId));
            FatalErrorMgr.Get().SetErrorCode("HS", "NT" + pendingResponseId.ToString(), null, null);
            Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN", 0f);
        }
        else
        {
            Debug.LogError(string.Format("Unhandled OnRequestTimeout pending ID {0}", pendingResponseId));
            FatalErrorMgr.Get().SetErrorCode("HS", "NU" + pendingResponseId.ToString(), null, null);
            Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN", 0f);
        }
    }

    private static void OnSubscribeResponse()
    {
        SubscribeResponse subscribeResponse = ConnectAPI.GetSubscribeResponse();
        if (((subscribeResponse != null) && subscribeResponse.HasRequestMaxWaitSecs) && (subscribeResponse.RequestMaxWaitSecs >= 30L))
        {
            MAX_DEFERRED_WAIT = new TimeSpan(0, 0, (int) subscribeResponse.RequestMaxWaitSecs);
        }
    }

    public static void OpenBooster(int id)
    {
        Log.Bob.Print("Network.OpenBooster", new object[0]);
        ConnectAPI.OpenBooster(id);
    }

    public static List<NetCache.BoosterCard> OpenedBooster()
    {
        return ConnectAPI.GetOpenedBooster();
    }

    public void ProcessAurora()
    {
        BattleNet.ProcessAurora();
        this.ProcessBnetEvents();
        if (IsLoggedIn())
        {
            this.ProcessPresence();
            this.ProcessFriends();
            this.ProcessWhispers();
            this.ProcessChallenges();
            this.ProcessParties();
            this.ProcessBroadcasts();
            this.ProcessNotifications();
            BnetNearbyPlayerMgr.Get().Update();
        }
        this.ProcessErrors();
    }

    private void ProcessBnetEvents()
    {
        int bnetEventsSize = BattleNet.GetBnetEventsSize();
        if ((bnetEventsSize > 0) && (this.m_bnetEventHandler != null))
        {
            BattleNet.BnetEvent[] events = new BattleNet.BnetEvent[bnetEventsSize];
            BattleNet.GetBnetEvents(events);
            this.m_bnetEventHandler(events);
            BattleNet.ClearBnetEvents();
        }
    }

    private void ProcessBroadcasts()
    {
        int shutdownMinutes = BattleNet.GetShutdownMinutes();
        if ((shutdownMinutes > 0) && (this.m_shutdownHandler != null))
        {
            this.m_shutdownHandler(shutdownMinutes);
        }
    }

    private void ProcessChallenges()
    {
        int num = BattleNet.NumChallenges();
        if ((num != 0) && (this.m_challengeHandler != null))
        {
            BattleNet.DllChallengeInfo[] challenges = new BattleNet.DllChallengeInfo[num];
            BattleNet.GetChallenges(challenges);
            this.m_challengeHandler(challenges);
            BattleNet.ClearChallenges();
        }
    }

    private bool ProcessConsole()
    {
        int id = ConnectAPI.NextDebugConsoleType();
        bool flag = this.HandleType(id);
        ConnectAPI.DropDebugPacket();
        return flag;
    }

    private bool ProcessDelayedError()
    {
        if (this.m_delayedError == null)
        {
            return false;
        }
        bool flag = false;
        if (BreakingNews.Get().GetStatus() == BreakingNews.Status.Fetching)
        {
            return flag;
        }
        ErrorParams parms = new ErrorParams {
            m_delayBeforeNextReset = this.m_timeBeforeAllowReset
        };
        string text = BreakingNews.Get().GetText();
        if (string.IsNullOrEmpty(text))
        {
            if ((BreakingNews.Get().GetError() != null) && (this.m_delayedError == "GLOBAL_ERROR_NETWORK_NO_GAME_SERVER"))
            {
                parms.m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_NO_CONNECTION", new object[0]);
            }
            else if (ApplicationMgr.IsInternal() && (this.m_delayedError == "GLOBAL_ERROR_UNKNOWN_ERROR"))
            {
                parms.m_message = "Dev Message: Could not connect to Battle.net, and there was no breaking news to display. Maybe Battle.net is down?";
            }
            else
            {
                parms.m_message = GameStrings.Format(this.m_delayedError, new object[0]);
            }
        }
        else
        {
            object[] args = new object[] { text };
            parms.m_message = GameStrings.Format("GLOBAL_MOBILE_ERROR_BREAKING_NEWS", args);
        }
        Error.AddFatal(parms);
        this.m_delayedError = null;
        this.m_timeBeforeAllowReset = 0f;
        return true;
    }

    private void ProcessErrors()
    {
        this.ProcessDelayedError();
        int errorsCount = BattleNet.GetErrorsCount();
        if (errorsCount != 0)
        {
            BnetErrorInfo[] infoArray = new BnetErrorInfo[errorsCount];
            BattleNet.GetErrors(infoArray);
            for (int i = 0; i < infoArray.Length; i++)
            {
                BattleNetErrors error = infoArray[i].GetError();
                string str = !ApplicationMgr.IsPublic() ? error.ToString() : string.Empty;
                if (ConnectAPI.ShouldIgnoreError(infoArray[i]))
                {
                    if (!ApplicationMgr.IsPublic())
                    {
                        object[] args = new object[] { (int) error, str };
                        Log.BattleNet.PrintDebug("BattleNet/ConnectDLL generated error={0} {1} (can ignore)", args);
                    }
                }
                else
                {
                    object[] objArray2 = new object[] { (int) error, str };
                    Debug.LogErrorFormat("BattleNet/ConnectDLL generated error={0} {1}", objArray2);
                    if (!this.FireErrorListeners(infoArray[i]))
                    {
                        this.OnBnetError(infoArray[i]);
                    }
                }
            }
            BattleNet.ClearErrors();
        }
    }

    private void ProcessFriends()
    {
        BattleNet.DllFriendsInfo info = new BattleNet.DllFriendsInfo();
        BattleNet.GetFriendsInfo(ref info);
        if ((info.updateSize != 0) && (this.m_friendsHandler != null))
        {
            BattleNet.FriendsUpdate[] updates = new BattleNet.FriendsUpdate[info.updateSize];
            BattleNet.GetFriendsUpdates(updates);
            this.m_friendsHandler(updates);
            BattleNet.ClearFriendsUpdates();
        }
    }

    private bool ProcessGameQueue()
    {
        BattleNet.QueueEvent queueEvent = BattleNet.GetQueueEvent();
        if (queueEvent == null)
        {
            return false;
        }
        switch (queueEvent.EventType)
        {
            case BattleNet.QueueEvent.Type.QUEUE_LEAVE:
            case BattleNet.QueueEvent.Type.QUEUE_DELAY_ERROR:
            case BattleNet.QueueEvent.Type.QUEUE_AMM_ERROR:
            case BattleNet.QueueEvent.Type.QUEUE_CANCEL:
            case BattleNet.QueueEvent.Type.QUEUE_GAME_STARTED:
            case BattleNet.QueueEvent.Type.ABORT_CLIENT_DROPPED:
                this.m_findingBnetGameType = BnetGameType.BGT_UNKNOWN;
                break;
        }
        if (this.m_gameQueueHandler == null)
        {
            Debug.LogWarning("m_gameQueueHandler is null in Network.ProcessGameQueue!");
        }
        else
        {
            this.m_gameQueueHandler(queueEvent);
        }
        return true;
    }

    private bool ProcessGameServer()
    {
        int id = ConnectAPI.NextGameType();
        bool flag = this.HandleType(id);
        ConnectAPI.DropGamePacket();
        return flag;
    }

    public void ProcessNetwork()
    {
        if (s_running)
        {
            this.lastCall = DateTime.Now;
            if (this.m_loginWaiting && ((this.lastCall - this.m_loginStarted) > LOGIN_TIMEOUT))
            {
                this.m_loginWaiting = false;
            }
            if (BattleNet.Init(ApplicationMgr.IsInternal()))
            {
                if (ShouldBeConnectedToAurora())
                {
                    this.ProcessAurora();
                }
                else
                {
                    this.ProcessDelayedError();
                }
                if (!this.ProcessGameQueue())
                {
                    if (ConnectAPI.HaveGamePackets())
                    {
                        this.ProcessGameServer();
                    }
                    else
                    {
                        if ((ConnectAPI.GameServerDisconnectEvents != null) && (ConnectAPI.GameServerDisconnectEvents.Count > 0))
                        {
                            if (this.m_gameServerDisconnectEventListener != null)
                            {
                                foreach (BattleNetErrors errors in ConnectAPI.GameServerDisconnectEvents.ToArray())
                                {
                                    this.m_gameServerDisconnectEventListener(errors);
                                }
                            }
                            ConnectAPI.GameServerDisconnectEvents.Clear();
                        }
                        if (ConnectAPI.HaveUtilPackets())
                        {
                            this.ProcessUtilServer();
                        }
                        else if (ConnectAPI.HaveDebugPackets())
                        {
                            this.ProcessConsole();
                        }
                        else
                        {
                            this.ProcessQueuePosition();
                        }
                    }
                }
            }
        }
    }

    private void ProcessNotifications()
    {
        int notificationCount = BattleNet.GetNotificationCount();
        if (notificationCount > 0)
        {
            BnetNotification[] notifications = new BnetNotification[notificationCount];
            BattleNet.GetNotifications(notifications);
            BattleNet.ClearNotifications();
            foreach (BnetNotification notification in notifications)
            {
                string notificationType = notification.NotificationType;
                if (notificationType != null)
                {
                    int num3;
                    if (<>f__switch$map2C == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                        dictionary.Add("WTCG.UtilNotificationMessage", 0);
                        <>f__switch$map2C = dictionary;
                    }
                    if (<>f__switch$map2C.TryGetValue(notificationType, out num3) && (num3 == 0))
                    {
                        PegasusPacket pegasusPacket = new PegasusPacket(notification.MessageType, 0, notification.MessageSize, notification.BlobMessage);
                        ConnectAPI.QueueUtilNotificationPegasusPacket(pegasusPacket);
                    }
                }
            }
        }
    }

    private void ProcessParties()
    {
        BnetParty.Process();
        BattleNet.DllPartyInfo info = new BattleNet.DllPartyInfo();
        BattleNet.GetPartyUpdatesInfo(ref info);
        if ((info.size > 0) && (this.m_partyHandler != null))
        {
            BattleNet.PartyEvent[] updates = new BattleNet.PartyEvent[info.size];
            BattleNet.GetPartyUpdates(updates);
            this.m_partyHandler(updates);
            BattleNet.ClearPartyUpdates();
        }
    }

    private void ProcessPresence()
    {
        int num = BattleNet.PresenceSize();
        if ((num != 0) && (this.m_presenceHandler != null))
        {
            BattleNet.PresenceUpdate[] updates = new BattleNet.PresenceUpdate[num];
            BattleNet.GetPresence(updates);
            this.m_presenceHandler(updates);
            BattleNet.ClearPresence();
        }
    }

    public void ProcessQueuePosition()
    {
        BattleNet.DllQueueInfo queueInfo = new BattleNet.DllQueueInfo();
        BattleNet.GetQueueInfo(ref queueInfo);
        if (queueInfo.changed && (this.m_queueInfoHandler != null))
        {
            QueueInfo info2 = new QueueInfo {
                position = queueInfo.position,
                end = queueInfo.end,
                stdev = queueInfo.stdev
            };
            this.m_queueInfoHandler(info2);
        }
    }

    private static void ProcessRequestTimeouts()
    {
        <ProcessRequestTimeouts>c__AnonStorey31E storeye = new <ProcessRequestTimeouts>c__AnonStorey31E {
            now = DateTime.Now
        };
        m_inTransitRequests.ForEach(new Action<RequestContext>(storeye.<>m__13E));
        m_inTransitRequests.RemoveAll(new Predicate<RequestContext>(storeye.<>m__13F));
    }

    private bool ProcessUtilServer()
    {
        int id = ConnectAPI.NextUtilType();
        bool flag = this.HandleType(id);
        ConnectAPI.DropUtilPacket();
        return flag;
    }

    private void ProcessWhispers()
    {
        BattleNet.DllWhisperInfo info = new BattleNet.DllWhisperInfo();
        BattleNet.GetWhisperInfo(ref info);
        if ((info.whisperSize > 0) && (this.m_whisperHandler != null))
        {
            BnetWhisper[] whispers = new BnetWhisper[info.whisperSize];
            BattleNet.GetWhispers(whispers);
            this.m_whisperHandler(whispers);
            BattleNet.ClearWhispers();
        }
    }

    public static void PurchaseViaGold(int quantity, ProductType product, int data)
    {
        ConnectAPI.PurchaseViaGold(quantity, product, data);
    }

    public void RegisterGameQueueHandler(GameQueueHandler handler)
    {
        if (this.m_gameQueueHandler != null)
        {
            object[] args = new object[] { handler, this.m_gameQueueHandler };
            Log.Net.Print("handler {0} would bash game queue handler {1}", args);
        }
        else
        {
            this.m_gameQueueHandler = handler;
        }
    }

    public bool RegisterNetHandler(object enumId, NetHandler handler, TimeoutHandler timeoutHandler = null)
    {
        List<NetHandler> list;
        int key = (int) enumId;
        if (timeoutHandler != null)
        {
            if (this.m_netTimeoutHandlers.ContainsKey(key))
            {
                return false;
            }
            this.m_netTimeoutHandlers.Add(key, timeoutHandler);
        }
        if (this.m_netHandlers.TryGetValue(key, out list))
        {
            if (list.Contains(handler))
            {
                return false;
            }
        }
        else
        {
            list = new List<NetHandler>();
            this.m_netHandlers.Add(key, list);
        }
        list.Add(handler);
        return true;
    }

    public void RegisterQueueInfoHandler(QueueInfoHandler handler)
    {
        if (this.m_queueInfoHandler != null)
        {
            object[] args = new object[] { handler, this.m_queueInfoHandler };
            Log.Net.Print("handler {0} would bash queue info handler {1}", args);
        }
        else
        {
            this.m_queueInfoHandler = handler;
        }
    }

    public bool RemoveBnetErrorListener(BnetErrorCallback callback)
    {
        return this.RemoveBnetErrorListener(callback, null);
    }

    public bool RemoveBnetErrorListener(BnetFeature feature, BnetErrorCallback callback)
    {
        return this.RemoveBnetErrorListener(feature, callback, null);
    }

    public bool RemoveBnetErrorListener(BnetErrorCallback callback, object userData)
    {
        BnetErrorListener item = new BnetErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_globalBnetErrorListeners.Remove(item);
    }

    public bool RemoveBnetErrorListener(BnetFeature feature, BnetErrorCallback callback, object userData)
    {
        List<BnetErrorListener> list;
        if (!this.m_featureBnetErrorListeners.TryGetValue(feature, out list))
        {
            return false;
        }
        BnetErrorListener item = new BnetErrorListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return list.Remove(item);
    }

    public static void RemoveFriend(BnetAccountId id)
    {
        BattleNet.RemoveFriend(id);
    }

    public void RemoveGameQueueHandler(GameQueueHandler handler)
    {
        if (this.m_gameQueueHandler != handler)
        {
            object[] args = new object[] { handler };
            Log.Net.Print("Removing game queue handler that is not active {0}", args);
        }
        else
        {
            this.m_gameQueueHandler = null;
        }
    }

    public void RemoveGameServerDisconnectEventListener(GameServerDisconnectEvent handler)
    {
        if (this.m_gameServerDisconnectEventListener == handler)
        {
            this.m_gameServerDisconnectEventListener = null;
        }
    }

    public bool RemoveNetHandler(object enumId, NetHandler handler)
    {
        List<NetHandler> list;
        int key = (int) enumId;
        return (this.m_netHandlers.TryGetValue(key, out list) && list.Remove(handler));
    }

    private static void RemovePendingRequestTimeout(int pendingId)
    {
        <RemovePendingRequestTimeout>c__AnonStorey31F storeyf = new <RemovePendingRequestTimeout>c__AnonStorey31F {
            pendingId = pendingId
        };
        m_inTransitRequests.RemoveAll(new Predicate<RequestContext>(storeyf.<>m__140));
    }

    public void RemoveQueueInfoHandler(QueueInfoHandler handler)
    {
        if (this.m_queueInfoHandler != handler)
        {
            object[] args = new object[] { handler };
            Log.Net.Print("Removing queue info handler that is not active {0}", args);
        }
        else
        {
            this.m_queueInfoHandler = null;
        }
    }

    public void RenameDeck(long deck, string name)
    {
        Log.Rachelle.Print(string.Format("Network.RenameDeck {0}", deck), new object[0]);
        ConnectAPI.RenameDeck(deck, name);
    }

    public static void RequestAccountLicenses()
    {
        ConnectAPI.RequestAccountLicenses();
    }

    public static void RequestAchieves(bool activeOrNewCompleteOnly)
    {
        ConnectAPI.RequestAchieves(activeOrNewCompleteOnly);
    }

    public static void RequestAdventureProgress()
    {
        ConnectAPI.RequestAdventureProgress();
    }

    public static void RequestAssetsVersion()
    {
        ConnectAPI.RequestAssetsVersion();
    }

    public static void RequestBattlePayConfig()
    {
        ConnectAPI.RequestBattlePayConfig();
    }

    public static void RequestBattlePayStatus()
    {
        ConnectAPI.RequestBattlePayStatus();
    }

    public static void RequestCancelQuest(int achieveID)
    {
        ConnectAPI.RequestCancelQuest(achieveID);
    }

    public static void RequestGameLicenses()
    {
        ConnectAPI.RequestGameLicenses();
    }

    public static void RescindFriendChallenge(BnetEntityId partyId)
    {
        BattleNet.RescindFriendlyChallenge(ref BnetEntityId.CreateForDll(partyId));
    }

    public static void Reset()
    {
        NetCache.Get().Clear();
        s_instance.m_delayedError = null;
        s_instance.m_timeBeforeAllowReset = 0f;
        Network network = s_instance;
        s_instance = new Network();
        s_instance.m_netHandlers = network.m_netHandlers;
        s_instance.m_gameQueueHandler = network.m_gameQueueHandler;
        s_instance.m_spectatorInviteReceivedHandler = network.m_spectatorInviteReceivedHandler;
        s_running = true;
        ConnectAPI.CloseAll();
        BattleNet.Reset(ApplicationMgr.IsInternal());
    }

    public static void ResetConnectionFailureCount()
    {
        s_numConnectionFailures = 0;
    }

    public static void RetireDraftDeck(long deckID, int slot)
    {
        ConnectAPI.DraftRetire(deckID, slot);
    }

    public bool RetryGotoGameServer()
    {
        return ConnectAPI.RetryGotoGameServer(this.m_lastGameServerInfo);
    }

    public static void RevokeFriendInvite(BnetInvitationId inviteId)
    {
        BattleNet.ManageFriendInvite(2, inviteId.GetVal());
    }

    public static void SellCard(int assetID, CardFlair cardFlair, int count, int unitSellPrice)
    {
        ConnectAPI.SellCard(assetID, (int) cardFlair.Premium, count, unitSellPrice);
    }

    public static void SendAckCardsSeen()
    {
        ConnectAPI.SendAckCardsSeen();
    }

    public void SendChoices(int ID, List<int> picks)
    {
        ConnectAPI.SendChoices(ID, picks);
    }

    public static bool SendConsoleCmdToServer(string command)
    {
        if (!IsConnectedToGameServer())
        {
            Log.Rachelle.Print(string.Format("Cannot send command '{0}' to server; no game server is active.", command), new object[0]);
            return false;
        }
        ConnectAPI.SendIndirectConsoleCommand(command);
        return true;
    }

    public void SendEmote(EmoteType emote)
    {
        ConnectAPI.SendEmote((int) emote);
    }

    public static void SendFriendChallenge(BnetGameAccountId gameAccountId, int scenarioId)
    {
        BattleNet.SendFriendlyChallengeInvite(ref BnetEntityId.CreateForDll(gameAccountId), scenarioId);
    }

    private static void SendFriendInvite(string sender, string target, bool byEmail)
    {
        BattleNet.SendFriendInvite(sender, target, byEmail);
    }

    public static void SendFriendInviteByBattleTag(string sender, string target)
    {
        SendFriendInvite(sender, target, false);
    }

    public static void SendFriendInviteByEmail(string sender, string target)
    {
        SendFriendInvite(sender, target, true);
    }

    public void SendOption(int ID, int index, int target, int sub, int pos)
    {
        ConnectAPI.SendOption(ID, index, target, sub, pos);
    }

    public void SendRemoveAllSpectators(bool regenerateSpectatorPassword)
    {
        ConnectAPI.SendRemoveAllSpectators(regenerateSpectatorPassword);
    }

    public void SendRemoveSpectators(bool regenerateSpectatorPassword, params BnetGameAccountId[] bnetGameAccountIds)
    {
        ConnectAPI.SendRemoveSpectators(regenerateSpectatorPassword, bnetGameAccountIds);
    }

    public void SendSpectatorInvite(BnetAccountId bnetAccountId, BnetGameAccountId bnetGameAccountId)
    {
        ConnectAPI.SendSpectatorInvite(bnetAccountId, bnetGameAccountId);
    }

    public void SendUserUI(int overCard, int heldCard, int arrowOrigin, int x, int y)
    {
        if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.ShowUserUI != 0)
        {
            ConnectAPI.SendUserUI(overCard, heldCard, arrowOrigin, x, y);
        }
    }

    public static void SendWhisper(BnetGameAccountId gameAccount, string message)
    {
        BattleNet.SendWhisper(gameAccount, message);
    }

    public static void SetAdventureOptions(int id, ulong options)
    {
        ConnectAPI.SetAdventureOptions(id, options);
    }

    public void SetBnetStateHandler(BnetEventHandler handler)
    {
        this.m_bnetEventHandler = handler;
    }

    public void SetChallengeHandler(ChallengeHandler handler)
    {
        this.m_challengeHandler = handler;
    }

    public void SetDeckContents(long deck, List<CardUserData> cards, int newHeroAssetID, TAG_PREMIUM newHeroCardPremium, int newCardBackID)
    {
        Log.Bob.Print(string.Format("Network.DeckSetUserData {0}", deck), new object[0]);
        ConnectAPI.SendDeckData(deck, cards, newHeroAssetID, newHeroCardPremium, newCardBackID);
    }

    public static void SetFavoriteHero(TAG_CLASS heroClass, NetCache.CardDefinition hero)
    {
        ConnectAPI.SetFavoriteHero(heroClass, hero);
    }

    public void SetFriendsHandler(FriendsHandler handler)
    {
        this.m_friendsHandler = handler;
    }

    public void SetGameServerDisconnectEventListener(GameServerDisconnectEvent handler)
    {
        this.m_gameServerDisconnectEventListener = handler;
    }

    public void SetPartyHandler(PartyHandler handler)
    {
        this.m_partyHandler = handler;
    }

    public void SetPresenceHandler(PresenceHandler handler)
    {
        this.m_presenceHandler = handler;
    }

    public static void SetShouldBeConnectedToAurora(bool shouldBeConnected)
    {
        s_shouldBeConnectedToAurora = shouldBeConnected;
    }

    public void SetShutdownHandler(ShutdownHandler handler)
    {
        this.m_shutdownHandler = handler;
    }

    public void SetSpectatorInviteReceivedHandler(SpectatorInviteReceivedHandler handler)
    {
        this.m_spectatorInviteReceivedHandler = handler;
    }

    private static void SetTags(List<Entity.Tag> dst, IntPtr src)
    {
        int[] numArray = IntPtrToIntArray(src, 0x200);
        for (int i = 0; i < 0x200; i++)
        {
            if (numArray[i] != 0)
            {
                Entity.Tag item = new Entity.Tag {
                    Name = i,
                    Value = numArray[i]
                };
                dst.Add(item);
            }
        }
    }

    public void SetWhisperHandler(WhisperHandler handler)
    {
        this.m_whisperHandler = handler;
    }

    public static bool ShouldBeConnectedToAurora()
    {
        return ((TUTORIALS_WITHOUT_ACCOUNT == null) || s_shouldBeConnectedToAurora);
    }

    public void ShowBreakingNewsOrError(string error, float timeBeforeAllowReset = 0)
    {
        this.m_delayedError = error;
        this.m_timeBeforeAllowReset = timeBeforeAllowReset;
        Debug.LogError(string.Format("Setting delayed error for Error Message: {0} and prevent reset for {1} seconds", error, timeBeforeAllowReset));
        this.ProcessDelayedError();
    }

    public void ShowConnectionFailureError(string error)
    {
        this.ShowBreakingNewsOrError(error, this.DelayForConnectionFailures(s_numConnectionFailures++));
    }

    public void SpectateSecondPlayer(BattleNet.GameServerInfo info)
    {
        ConnectAPI.SpectateSecondPlayer(info);
    }

    public static void StartANewDraft()
    {
        ConnectAPI.DraftBegin();
    }

    public static void SubmitThirdPartyReceipt(long bpayID, string thirdPartyID, string base64receipt, string thirdPartyUserID = "")
    {
        ConnectAPI.SubmitThirdPartyPurchaseReceipt(bpayID, thirdPartyID, base64receipt, thirdPartyUserID);
    }

    public static void TrackClient(TrackLevel level, TrackWhat what)
    {
        switch (what)
        {
            case TrackWhat.TRACK_COLLECTION_MANAGER:
            case TrackWhat.TRACK_BOX_SCREEN_VISIT:
                ConnectAPI.GuardianTrack(what);
                break;
        }
        ConnectAPI.TrackClient((int) level, (int) what, null);
    }

    public static BnetGameType TranslateGameTypeToBnet(GameType gameType, int missionId)
    {
        switch (gameType)
        {
            case GameType.GT_VS_AI:
                return BnetGameType.BGT_VS_AI;

            case GameType.GT_VS_FRIEND:
                return BnetGameType.BGT_FRIENDS;

            case GameType.GT_TUTORIAL:
                return BnetGameType.BGT_TUTORIAL;

            case GameType.GT_ARENA:
                return BnetGameType.BGT_ARENA;

            case GameType.GT_RANKED:
                return BnetGameType.BGT_RANKED;

            case GameType.GT_UNRANKED:
                if (NetCache.Get().GetNetObject<NetCache.NetCachePlayQueue>().GameType != BnetGameType.BGT_NEWBIE)
                {
                    return BnetGameType.BGT_NORMAL;
                }
                return BnetGameType.BGT_NEWBIE;

            case GameType.GT_TAVERNBRAWL:
                if (GameUtils.IsCoopMission(missionId))
                {
                    return BnetGameType.BGT_TAVERNBRAWL_2P_COOP;
                }
                return BnetGameType.BGT_TAVERNBRAWL_PVP;
        }
        object[] messageArgs = new object[] { gameType };
        Error.AddDevFatal("Network.TranslateGameTypeToBnet() - do not know how to translate {0}", messageArgs);
        return BnetGameType.BGT_UNKNOWN;
    }

    public static void TriggerLaunchEvent(BnetGameAccountId lastOpponentHSGameAccountID, ulong lastOpponentSessionStartTime, BnetGameAccountId otherPlayerHSGameAccountID, ulong otherPlayerSessionStartTime)
    {
        ConnectAPI.TriggerLaunchEvent(lastOpponentHSGameAccountID.GetHi(), lastOpponentHSGameAccountID.GetLo(), lastOpponentSessionStartTime, otherPlayerHSGameAccountID.GetHi(), otherPlayerHSGameAccountID.GetLo(), otherPlayerSessionStartTime);
    }

    public static void ValidateAchieve(int achieveID)
    {
        ConnectAPI.ValidateAchieve(achieveID);
    }

    public static bool WasDisconnectRequested()
    {
        return ConnectAPI.WasDisconnectRequested();
    }

    public static bool WasGameConceded()
    {
        return ConnectAPI.WasGameConceded();
    }

    [CompilerGenerated]
    private sealed class <ProcessRequestTimeouts>c__AnonStorey31E
    {
        internal DateTime now;

        internal void <>m__13E(Network.RequestContext rc)
        {
            if ((rc.m_timeoutHandler != null) && (rc.m_waitUntil < this.now))
            {
                Debug.LogWarning(string.Format("Encountered timeout waiting for {0}", rc.m_pendingId));
                rc.m_timeoutHandler(rc.m_pendingId);
            }
        }

        internal bool <>m__13F(Network.RequestContext rc)
        {
            return (rc.m_waitUntil < this.now);
        }
    }

    [CompilerGenerated]
    private sealed class <RemovePendingRequestTimeout>c__AnonStorey31F
    {
        internal int pendingId;

        internal bool <>m__140(Network.RequestContext pc)
        {
            return (pc.m_pendingId == this.pendingId);
        }
    }

    public class AccountLicenseAchieveResponse
    {
        public int Achieve { get; set; }

        public AchieveResult Result { get; set; }

        public enum AchieveResult
        {
            COMPLETE = 4,
            IN_PROGRESS = 3,
            INVALID_ACHIEVE = 1,
            NOT_ACTIVE = 2,
            STATUS_UNKNOWN = 5
        }
    }

    public class AchieveList
    {
        public AchieveList()
        {
            this.Achieves = new List<Achieve>();
        }

        public List<Achieve> Achieves { get; set; }

        public class Achieve
        {
            public int AckProgress { get; set; }

            public bool Active { get; set; }

            public bool CanAck { get; set; }

            public int CompletionCount { get; set; }

            public long DateCompleted { get; set; }

            public long DateGiven { get; set; }

            public int ID { get; set; }

            public int Progress { get; set; }
        }
    }

    public class AdventureProgress
    {
        public AdventureProgress()
        {
            this.Wing = 0;
            this.Progress = 0;
            this.Ack = 0;
            this.Flags = 0L;
        }

        public int Ack { get; set; }

        public ulong Flags { get; set; }

        public int Progress { get; set; }

        public int Wing { get; set; }
    }

    public enum AuthResult
    {
        UNKNOWN,
        ALLOWED,
        INVALID,
        SECOND,
        OFFLINE
    }

    public class BattlePayConfig
    {
        public BattlePayConfig()
        {
            this.Available = false;
            this.Currency = Currency.UNKNOWN;
            this.Bundles = new List<Network.Bundle>();
            this.GoldCostBoosters = new List<Network.GoldCostBooster>();
            this.GoldCostArena = 0L;
            this.SecondsBeforeAutoCancel = StoreManager.DEFAULT_SECONDS_BEFORE_AUTO_CANCEL;
        }

        public bool Available { get; set; }

        public List<Network.Bundle> Bundles { get; set; }

        public Currency Currency { get; set; }

        public long GoldCostArena { get; set; }

        public List<Network.GoldCostBooster> GoldCostBoosters { get; set; }

        public int SecondsBeforeAutoCancel { get; set; }
    }

    public class BattlePayStatus
    {
        public BattlePayStatus()
        {
            this.State = PurchaseState.UNKNOWN;
            this.TransactionID = 0L;
            this.ThirdPartyID = string.Empty;
            this.ProductID = string.Empty;
            this.PurchaseError = new Network.PurchaseErrorInfo();
            this.BattlePayAvailable = false;
            this.CurrencyType = Currency.UNKNOWN;
            this.Provider = MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER;
        }

        public bool BattlePayAvailable { get; set; }

        public Currency CurrencyType { get; set; }

        public string ProductID { get; set; }

        public BattlePayProvider? Provider { get; set; }

        public Network.PurchaseErrorInfo PurchaseError { get; set; }

        public PurchaseState State { get; set; }

        public string ThirdPartyID { get; set; }

        public long TransactionID { get; set; }

        public enum PurchaseState
        {
            CHECK_RESULTS = 1,
            ERROR = 2,
            READY = 0,
            UNKNOWN = -1
        }
    }

    public class BeginDraft
    {
        public BeginDraft()
        {
            this.Heroes = new List<NetCache.CardDefinition>();
        }

        public long DeckID { get; set; }

        public List<NetCache.CardDefinition> Heroes { get; set; }
    }

    public delegate bool BnetErrorCallback(BnetErrorInfo info, object userData);

    private class BnetErrorListener : EventListener<Network.BnetErrorCallback>
    {
        public bool Fire(BnetErrorInfo info)
        {
            return base.m_callback(info, base.m_userData);
        }
    }

    public delegate void BnetEventHandler(BattleNet.BnetEvent[] updates);

    public enum BnetLoginState
    {
        BATTLE_NET_UNKNOWN,
        BATTLE_NET_LOGGING_IN,
        BATTLE_NET_TIMEOUT,
        BATTLE_NET_LOGIN_FAILED,
        BATTLE_NET_LOGGED_IN
    }

    public enum BnetRegion
    {
        REGION_CN = 5,
        REGION_DEV = 60,
        REGION_EU = 2,
        REGION_KR = 3,
        REGION_LIVE_VERIFICATION = 40,
        REGION_MSCHWEITZER_BN11 = 0x34,
        REGION_MSCHWEITZER_BN12 = 0x35,
        REGION_PTR = 0x62,
        REGION_PTR_LOC = 0x29,
        REGION_TW = 4,
        REGION_UNINITIALIZED = -1,
        REGION_UNKNOWN = 0,
        REGION_US = 1
    }

    public enum BoosterSource
    {
        ARENA_REWARD = 3,
        BOUGHT = 4,
        BOUGHT_GOLD = 11,
        CS_GIFT = 8,
        LICENSED = 6,
        QUEST_REWARD = 10,
        UNKNOWN = 0
    }

    public class Bundle
    {
        [CompilerGenerated]
        private static Predicate<Network.BundleItem> <>f__am$cache8;

        public Bundle()
        {
            this.ProductID = string.Empty;
            this.Cost = 0.0;
            this.GoldCost = 0L;
            this.AppleID = string.Empty;
            this.GooglePlayID = string.Empty;
            this.AmazonID = string.Empty;
            this.Items = new List<Network.BundleItem>();
            this.ProductEvent = SpecialEventType.IGNORE;
        }

        public bool IsFree()
        {
            return ((this.Cost < 0.0099999997764825821) && (this.Cost >= 0.0));
        }

        public bool IsPreOrder()
        {
            HashSet<ProductType> productsInItemList = StoreManager.Get().GetProductsInItemList(this.Items);
            if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_BRM) && productsInItemList.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
            {
                return true;
            }
            if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_BOOSTER) && productsInItemList.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
            {
                if (<>f__am$cache8 == null)
                {
                    <>f__am$cache8 = obj => (obj.Product == ProductType.PRODUCT_TYPE_BOOSTER) && (obj.ProductData == 10);
                }
                if (this.Items.Find(<>f__am$cache8) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public string AmazonID { get; set; }

        public string AppleID { get; set; }

        public double Cost { get; set; }

        public long GoldCost { get; set; }

        public string GooglePlayID { get; set; }

        public List<Network.BundleItem> Items { get; set; }

        public SpecialEventType ProductEvent { get; set; }

        public string ProductID { get; set; }
    }

    public class BundleItem
    {
        public BundleItem()
        {
            this.Product = ProductType.PRODUCT_TYPE_UNKNOWN;
            this.ProductData = 0;
            this.Quantity = 0;
        }

        public override bool Equals(object obj)
        {
            Network.BundleItem item = obj as Network.BundleItem;
            if (item == null)
            {
                return false;
            }
            if (item.Product != this.Product)
            {
                return false;
            }
            if (item.ProductData != this.ProductData)
            {
                return false;
            }
            return (item.Quantity == this.Quantity);
        }

        public override int GetHashCode()
        {
            return ((this.Product.GetHashCode() * this.ProductData.GetHashCode()) * this.Quantity.GetHashCode());
        }

        public ProductType Product { get; set; }

        public int ProductData { get; set; }

        public int Quantity { get; set; }
    }

    public class CanceledQuest
    {
        public CanceledQuest()
        {
            this.AchieveID = 0;
            this.Canceled = false;
            this.NextQuestCancelDate = 0L;
        }

        public int AchieveID { get; set; }

        public bool Canceled { get; set; }

        public long NextQuestCancelDate { get; set; }
    }

    public class CardBackResponse
    {
        public CardBackResponse()
        {
            this.Success = false;
            this.CardBack = 0;
        }

        public int CardBack { get; set; }

        public bool Success { get; set; }
    }

    public class CardQuote
    {
        public int AssetID { get; set; }

        public int BuyPrice { get; set; }

        public int SaleValue { get; set; }

        public QuoteState Status { get; set; }

        public enum QuoteState
        {
            SUCCESS,
            UNKNOWN_ERROR
        }
    }

    public class CardSaleResult
    {
        public SaleResult Action { get; set; }

        public int Amount { get; set; }

        public int AssetID { get; set; }

        public string AssetName { get; set; }

        public int Count { get; set; }

        public bool Nerfed { get; set; }

        public TAG_PREMIUM Premium { get; set; }

        public int UnitBuyPrice { get; set; }

        public int UnitSellPrice { get; set; }

        public enum SaleResult
        {
            CARD_WAS_BOUGHT = 3,
            CARD_WAS_SOLD = 2,
            FAILED_EVENT_NOT_ACTIVE = 8,
            FAILED_NO_PERMISSION = 7,
            FAILED_WRONG_BUY_PRICE = 6,
            FAILED_WRONG_SELL_PRICE = 5,
            GENERIC_FAILURE = 1,
            SOULBOUND = 4
        }
    }

    public class CardUserData
    {
        public int Count { get; set; }

        public int DbId { get; set; }

        public TAG_PREMIUM Premium { get; set; }

        public int UID
        {
            get
            {
                return GameUtils.CardUID(this.DbId, this.Premium);
            }
        }
    }

    public delegate void ChallengeHandler(BattleNet.DllChallengeInfo[] challenges);

    public class DBAction
    {
        public ActionType Action { get; set; }

        public long MetaData { get; set; }

        public ResultType Result { get; set; }

        public enum ActionType
        {
            UNKNOWN,
            GET_DECK,
            CREATE_DECK,
            RENAME_DECK,
            DELETE_DECK,
            SET_DECK,
            OPEN_BOOSTER,
            GAMES_INFO
        }

        public enum ResultType
        {
            UNKNOWN,
            SUCCESS,
            NOT_OWNED,
            CONSTRAINT
        }
    }

    public class DebugConsoleResponse
    {
        public DebugConsoleResponse()
        {
            this.Response = string.Empty;
        }

        public string Response { get; set; }

        public int Type { get; set; }
    }

    public class DeckCard
    {
        public long Card { get; set; }

        public long Deck { get; set; }
    }

    public class DeckContents
    {
        public DeckContents()
        {
            this.Cards = new List<Network.CardUserData>();
        }

        public List<Network.CardUserData> Cards { get; set; }

        public long Deck { get; set; }
    }

    public class DeckName
    {
        public long Deck { get; set; }

        public string Name { get; set; }
    }

    public class DraftChoicesAndContents
    {
        public DraftChoicesAndContents()
        {
            this.Choices = new List<NetCache.CardDefinition>();
            this.Hero = new NetCache.CardDefinition();
            this.DeckInfo = new Network.DeckContents();
            this.Chest = null;
        }

        public Network.RewardChest Chest { get; set; }

        public List<NetCache.CardDefinition> Choices { get; set; }

        public Network.DeckContents DeckInfo { get; set; }

        public NetCache.CardDefinition Hero { get; set; }

        public int Losses { get; set; }

        public int MaxWins { get; set; }

        public int Slot { get; set; }

        public int Wins { get; set; }
    }

    public class DraftChosen
    {
        public DraftChosen()
        {
            this.ChosenCard = new NetCache.CardDefinition();
            this.NextChoices = new List<NetCache.CardDefinition>();
        }

        public NetCache.CardDefinition ChosenCard { get; set; }

        public List<NetCache.CardDefinition> NextChoices { get; set; }
    }

    public enum DraftError
    {
        DE_UNKNOWN,
        DE_NO_LICENSE,
        DE_RETIRE_FIRST,
        DE_NOT_IN_DRAFT,
        DE_BAD_DECK,
        DE_BAD_SLOT,
        DE_BAD_INDEX,
        DE_NOT_IN_DRAFT_BUT_COULD_BE,
        DE_FEATURE_DISABLED
    }

    public class DraftRetired
    {
        public DraftRetired()
        {
            this.Deck = 0L;
            this.Chest = new Network.RewardChest();
        }

        public Network.RewardChest Chest { get; set; }

        public long Deck { get; set; }
    }

    public class EntitiesChosen
    {
        public List<int> Entities { get; set; }

        public int ID { get; set; }

        public int PlayerId { get; set; }
    }

    public class Entity
    {
        public Entity()
        {
            this.Tags = new List<Tag>();
        }

        public static Network.Entity CreateFromProto(PegasusGame.Entity src)
        {
            return new Network.Entity { ID = src.Id, CardID = string.Empty, Tags = CreateTagsFromProto(src.Tags) };
        }

        public static Network.Entity CreateFromProto(PowerHistoryEntity src)
        {
            return new Network.Entity { ID = src.Entity, CardID = src.Name, Tags = CreateTagsFromProto(src.Tags) };
        }

        public static List<Tag> CreateTagsFromProto(IList<PegasusGame.Tag> tagList)
        {
            List<Tag> list = new List<Tag>();
            for (int i = 0; i < tagList.Count; i++)
            {
                PegasusGame.Tag tag = tagList[i];
                Tag item = new Tag {
                    Name = tag.Name,
                    Value = tag.Value
                };
                list.Add(item);
            }
            return list;
        }

        public override string ToString()
        {
            return string.Format("id={0} cardId={1} tags={2}", this.ID, this.CardID, this.Tags.Count);
        }

        public string CardID { get; set; }

        public int ID { get; set; }

        public List<Tag> Tags { get; set; }

        public class Tag
        {
            public int Name { get; set; }

            public int Value { get; set; }
        }
    }

    public class EntityChoices
    {
        public CHOICE_TYPE ChoiceType { get; set; }

        public int CountMax { get; set; }

        public int CountMin { get; set; }

        public List<int> Entities { get; set; }

        public int ID { get; set; }

        public int PlayerId { get; set; }

        public int Source { get; set; }
    }

    public delegate void FriendsHandler(BattleNet.FriendsUpdate[] updates);

    public class GameCancelInfo
    {
        public Reason CancelReason { get; set; }

        public enum Reason
        {
            OPPONENT_TIMEOUT = 1
        }
    }

    public class GameEnd
    {
        public GameEnd()
        {
            this.Notices = new List<NetCache.ProfileNotice>();
        }

        public List<NetCache.ProfileNotice> Notices { get; set; }
    }

    public delegate void GameQueueHandler(BattleNet.QueueEvent queueEvent);

    public delegate void GameServerDisconnectEvent(BattleNetErrors errorCode);

    public class GameSetup
    {
        public int Board { get; set; }

        public int MaxFriendlyMinionsPerPlayer { get; set; }

        public int MaxSecretsPerPlayer { get; set; }
    }

    public class GenericResponse
    {
        public object GenericData { get; set; }

        public int RequestId { get; set; }

        public int RequestSubId { get; set; }

        public Result ResultCode { get; set; }

        public enum Result
        {
            DATA_MIGRATION_ERROR = 0x69,
            DB_ERROR = 0x66,
            FIRST_ERROR = 100,
            INTERNAL_ERROR = 0x65,
            INVALID_REQUEST = 0x67,
            LOGIN_LOAD = 0x68,
            OK = 0,
            REQUEST_COMPLETE = 2,
            REQUEST_IN_PROCESS = 1
        }
    }

    public class GoldCostBooster
    {
        public GoldCostBooster()
        {
            this.Cost = 0L;
            this.Id = 0;
        }

        public long Cost { get; set; }

        public int Id { get; set; }
    }

    public class HistActionEnd : Network.PowerHistory
    {
        public HistActionEnd() : base(Network.PowerType.ACTION_END)
        {
        }
    }

    public class HistActionStart : Network.PowerHistory
    {
        public HistActionStart(HistoryBlock.Type type) : base(Network.PowerType.ACTION_START)
        {
            this.BlockType = type;
        }

        public override string ToString()
        {
            object[] args = new object[] { base.Type, this.BlockType, this.Entity, this.Target };
            return string.Format("type={0} blockType={1} entity={2} target={3}", args);
        }

        public HistoryBlock.Type BlockType { get; set; }

        public int Entity { get; set; }

        public int Index { get; set; }

        public int Target { get; set; }
    }

    public class HistCreateGame : Network.PowerHistory
    {
        public HistCreateGame() : base(Network.PowerType.CREATE_GAME)
        {
        }

        public static Network.HistCreateGame CreateFromProto(PowerHistoryCreateGame src)
        {
            Network.HistCreateGame game = new Network.HistCreateGame {
                Game = Network.Entity.CreateFromProto(src.GameEntity)
            };
            if (src.Players != null)
            {
                game.Players = new List<PlayerData>();
                for (int i = 0; i < src.Players.Count; i++)
                {
                    PegasusGame.Player player = src.Players[i];
                    PlayerData item = PlayerData.CreateFromProto(player);
                    game.Players.Add(item);
                }
            }
            return game;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("game={0}", this.Game);
            if (this.Players == null)
            {
                builder.Append(" players=(null)");
            }
            else if (this.Players.Count == 0)
            {
                builder.Append(" players=0");
            }
            else
            {
                for (int i = 0; i < this.Players.Count; i++)
                {
                    builder.AppendFormat(" players[{0}]=[{1}]", i, this.Players[i]);
                }
            }
            return builder.ToString();
        }

        public Network.Entity Game { get; set; }

        public List<PlayerData> Players { get; set; }

        public class PlayerData
        {
            public static Network.HistCreateGame.PlayerData CreateFromProto(PegasusGame.Player src)
            {
                return new Network.HistCreateGame.PlayerData { ID = src.Id, GameAccountId = BnetGameAccountId.CreateFromNet(src.GameAccountId), Player = Network.Entity.CreateFromProto(src.Entity), CardBackID = src.CardBack };
            }

            public override string ToString()
            {
                object[] args = new object[] { this.ID, this.GameAccountId, this.Player, this.CardBackID };
                return string.Format("ID={0} GameAccountId={1} Player={2} CardBackID={3}", args);
            }

            public int CardBackID { get; set; }

            public BnetGameAccountId GameAccountId { get; set; }

            public int ID { get; set; }

            public Network.Entity Player { get; set; }
        }
    }

    public class HistFullEntity : Network.PowerHistory
    {
        public HistFullEntity() : base(Network.PowerType.FULL_ENTITY)
        {
        }

        public override string ToString()
        {
            return string.Format("type={0} entity=[{1}]", base.Type, this.Entity);
        }

        public Network.Entity Entity { get; set; }
    }

    public class HistHideEntity : Network.PowerHistory
    {
        public HistHideEntity() : base(Network.PowerType.HIDE_ENTITY)
        {
        }

        public override string ToString()
        {
            return string.Format("type={0} entity={1} zone={2}", base.Type, this.Entity, this.Zone);
        }

        public int Entity { get; set; }

        public int Zone { get; set; }
    }

    public class HistMetaData : Network.PowerHistory
    {
        public HistMetaData() : base(Network.PowerType.META_DATA)
        {
            this.Info = new List<int>();
        }

        public override string ToString()
        {
            object[] args = new object[] { base.Type, this.MetaType, this.Info.Count, this.Data };
            return string.Format("type={0} metaType={1} infoCount={2} data={3}", args);
        }

        public int Data { get; set; }

        public List<int> Info { get; set; }

        public HistoryMeta.Type MetaType { get; set; }
    }

    public class HistShowEntity : Network.PowerHistory
    {
        public HistShowEntity() : base(Network.PowerType.SHOW_ENTITY)
        {
        }

        public override string ToString()
        {
            return string.Format("type={0} entity=[{1}]", base.Type, this.Entity);
        }

        public Network.Entity Entity { get; set; }
    }

    public class HistTagChange : Network.PowerHistory
    {
        public HistTagChange() : base(Network.PowerType.TAG_CHANGE)
        {
        }

        public override string ToString()
        {
            object[] args = new object[] { base.Type, this.Entity, this.Tag, this.Value };
            return string.Format("type={0} entity={1} tag={2} value={3}", args);
        }

        public int Entity { get; set; }

        public int Tag { get; set; }

        public int Value { get; set; }
    }

    public enum InviteAction
    {
        INVITE_ACCEPT = 1,
        INVITE_DECLINE = 3,
        INVITE_IGNORE = 4,
        INVITE_REVOKE = 2
    }

    public class MassDisenchantResponse
    {
        public MassDisenchantResponse()
        {
            this.Amount = 0;
        }

        public int Amount { get; set; }
    }

    public delegate void NetHandler();

    public class Notification
    {
        public Type NotificationType { get; set; }

        public enum Type
        {
            IN_HAND_CARD_CAP = 1,
            MANA_CAP = 2
        }
    }

    public class Options
    {
        public Options()
        {
            this.List = new List<Option>();
        }

        public void CopyFrom(Network.Options options)
        {
            this.ID = options.ID;
            if (options.List == null)
            {
                this.List = null;
            }
            else
            {
                if (this.List != null)
                {
                    this.List.Clear();
                }
                else
                {
                    this.List = new List<Option>();
                }
                for (int i = 0; i < options.List.Count; i++)
                {
                    Option item = new Option();
                    item.CopyFrom(options.List[i]);
                    this.List.Add(item);
                }
            }
        }

        public int ID { get; set; }

        public List<Option> List { get; set; }

        public class Option
        {
            public Option()
            {
                this.Main = new SubOption();
                this.Subs = new List<SubOption>();
            }

            public void CopyFrom(Network.Options.Option option)
            {
                this.Type = option.Type;
                if (this.Main == null)
                {
                    this.Main = new SubOption();
                }
                this.Main.CopyFrom(option.Main);
                if (option.Subs == null)
                {
                    this.Subs = null;
                }
                else
                {
                    if (this.Subs == null)
                    {
                        this.Subs = new List<SubOption>();
                    }
                    else
                    {
                        this.Subs.Clear();
                    }
                    for (int i = 0; i < option.Subs.Count; i++)
                    {
                        SubOption item = new SubOption();
                        item.CopyFrom(option.Subs[i]);
                        this.Subs.Add(item);
                    }
                }
            }

            public SubOption Main { get; set; }

            public List<SubOption> Subs { get; set; }

            public OptionType Type { get; set; }

            public enum OptionType
            {
                END_TURN = 2,
                PASS = 1,
                POWER = 3
            }

            public class SubOption
            {
                public void CopyFrom(Network.Options.Option.SubOption subOption)
                {
                    this.ID = subOption.ID;
                    if (subOption.Targets == null)
                    {
                        this.Targets = null;
                    }
                    else
                    {
                        if (this.Targets == null)
                        {
                            this.Targets = new List<int>();
                        }
                        else
                        {
                            this.Targets.Clear();
                        }
                        for (int i = 0; i < subOption.Targets.Count; i++)
                        {
                            this.Targets.Add(subOption.Targets[i]);
                        }
                    }
                }

                public int ID { get; set; }

                public List<int> Targets { get; set; }
            }
        }
    }

    public delegate void PartyHandler(BattleNet.PartyEvent[] updates);

    public class PowerHistory
    {
        public PowerHistory(Network.PowerType init)
        {
            this.Type = init;
        }

        public override string ToString()
        {
            return string.Format("type={0}", this.Type);
        }

        public Network.PowerType Type { get; set; }
    }

    public enum PowerType
    {
        ACTION_END = 6,
        ACTION_START = 5,
        CREATE_GAME = 7,
        FULL_ENTITY = 1,
        HIDE_ENTITY = 3,
        META_DATA = 8,
        SHOW_ENTITY = 2,
        TAG_CHANGE = 4
    }

    public delegate void PresenceHandler(BattleNet.PresenceUpdate[] updates);

    public class ProfileNotices
    {
        public ProfileNotices()
        {
            this.Notices = new List<NetCache.ProfileNotice>();
        }

        public List<NetCache.ProfileNotice> Notices { get; set; }
    }

    public class PurchaseCanceledResponse
    {
        public Currency CurrencyType { get; set; }

        public string ProductID { get; set; }

        public CancelResult Result { get; set; }

        public long TransactionID { get; set; }

        public enum CancelResult
        {
            SUCCESS,
            NOT_ALLOWED,
            NOTHING_TO_CANCEL
        }
    }

    public class PurchaseErrorInfo
    {
        public PurchaseErrorInfo()
        {
            this.Error = ErrorType.UNKNOWN;
            this.PurchaseInProgressProductID = string.Empty;
            this.ErrorCode = string.Empty;
        }

        public ErrorType Error { get; set; }

        public string ErrorCode { get; set; }

        public string PurchaseInProgressProductID { get; set; }

        public enum ErrorType
        {
            BP_GENERIC_FAIL = 100,
            BP_INVALID_CC_EXPIRY = 0x65,
            BP_NO_VALID_PAYMENT = 0x67,
            BP_PARENTAL_CONTROL = 0x6c,
            BP_PAYMENT_AUTH = 0x68,
            BP_PRODUCT_UNIQUENESS_VIOLATED = 0x70,
            BP_PROVIDER_DENIED = 0x69,
            BP_PURCHASE_BAN = 0x6a,
            BP_REGION_IS_DOWN = 0x71,
            BP_RISK_ERROR = 0x66,
            BP_SPENDING_LIMIT = 0x6b,
            BP_THIRD_PARTY_BAD_RECEIPT = 110,
            BP_THIRD_PARTY_RECEIPT_USED = 0x6f,
            BP_THROTTLED = 0x6d,
            CANCELED = 11,
            DATABASE = 5,
            DUPLICATE_LICENSE = 7,
            FAILED_RISK = 10,
            INVALID_BNET = 2,
            INVALID_QUANTITY = 6,
            NO_ACTIVE_BPAY = 9,
            PRODUCT_ALREADY_OWNED = 0x11,
            PRODUCT_EVENT_HAS_ENDED = 0x13,
            PRODUCT_NA = 15,
            PURCHASE_IN_PROGRESS = 4,
            REQUEST_NOT_SENT = 8,
            RISK_TIMEOUT = 0x10,
            SERVICE_NA = 3,
            STILL_IN_PROGRESS = 1,
            SUCCESS = 0,
            UNKNOWN = -1,
            WAIT_CONFIRM = 13,
            WAIT_MOP = 12,
            WAIT_RISK = 14,
            WAIT_THIRD_PARTY_RECEIPT = 0x12
        }
    }

    public class PurchaseMethod
    {
        public PurchaseMethod()
        {
            this.TransactionID = 0L;
            this.ProductID = string.Empty;
            this.Quantity = 0;
            this.Currency = Currency.UNKNOWN;
            this.WalletName = string.Empty;
            this.UseEBalance = false;
            this.IsZeroCostLicense = false;
            this.PurchaseError = null;
        }

        public Currency Currency { get; set; }

        public bool IsZeroCostLicense { get; set; }

        public string ProductID { get; set; }

        public Network.PurchaseErrorInfo PurchaseError { get; set; }

        public int Quantity { get; set; }

        public long TransactionID { get; set; }

        public bool UseEBalance { get; set; }

        public string WalletName { get; set; }
    }

    public class PurchaseResponse
    {
        public PurchaseResponse()
        {
            this.PurchaseError = new Network.PurchaseErrorInfo();
            this.TransactionID = 0L;
            this.ProductID = string.Empty;
            this.ThirdPartyID = string.Empty;
            this.CurrencyType = Currency.UNKNOWN;
            this.ChallengeID = string.Empty;
            this.ChallengeURL = string.Empty;
        }

        public string ChallengeID { get; set; }

        public string ChallengeURL { get; set; }

        public Currency CurrencyType { get; set; }

        public string ProductID { get; set; }

        public Network.PurchaseErrorInfo PurchaseError { get; set; }

        public string ThirdPartyID { get; set; }

        public long TransactionID { get; set; }
    }

    public class PurchaseViaGoldResponse
    {
        public PurchaseViaGoldResponse()
        {
            this.Error = ErrorType.UNKNOWN;
            this.GoldUsed = 0L;
        }

        public ErrorType Error { get; set; }

        public long GoldUsed { get; set; }

        public enum ErrorType
        {
            FEATURE_NA = 4,
            INSUFFICIENT_GOLD = 2,
            INVALID_QUANTITY = 5,
            PRODUCT_NA = 3,
            SUCCESS = 1,
            UNKNOWN = -1
        }
    }

    public class QueueInfo
    {
        public long end;
        public int position;
        public long stdev;
    }

    public delegate void QueueInfoHandler(Network.QueueInfo queueInfo);

    public class RecruitInfo
    {
        public RecruitInfo()
        {
            this.ID = 0L;
            this.RecruitID = new BnetAccountId();
            this.Nickname = string.Empty;
            this.Status = 0;
            this.Level = 0;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.ID, this.RecruitID, this.Nickname, this.Status, this.Level };
            return string.Format("[RecruitInfo: ID={0}, RecruitID={1}, Nickname={2}, Status={3}, Level={4}]", args);
        }

        public ulong CreationTimeMicrosec { get; set; }

        public ulong ID { get; set; }

        public int Level { get; set; }

        public string Nickname { get; set; }

        public BnetAccountId RecruitID { get; set; }

        public int Status { get; set; }
    }

    private class RequestContext
    {
        public int m_pendingId;
        public Network.TimeoutHandler m_timeoutHandler;
        public DateTime m_waitUntil = (DateTime.Now + Network.GetMaxDeferredWait());

        public RequestContext(int pendingId, Network.TimeoutHandler timeoutHandler)
        {
            this.m_pendingId = pendingId;
            this.m_timeoutHandler = timeoutHandler;
        }
    }

    public class RewardChest
    {
        public RewardChest()
        {
            this.Rewards = new List<RewardData>();
        }

        public List<RewardData> Rewards { get; set; }
    }

    public class SetFavoriteHeroResponse
    {
        public NetCache.CardDefinition Hero = null;
        public TAG_CLASS HeroClass = TAG_CLASS.INVALID;
        public bool Success = false;
    }

    public delegate void ShutdownHandler(int minutes);

    public delegate void SpectatorInviteReceivedHandler(Invite invite);

    public class ThirdPartyPurchaseStatusResponse
    {
        public ThirdPartyPurchaseStatusResponse()
        {
            this.ThirdPartyID = string.Empty;
            this.Status = PurchaseStatus.UNKNOWN;
        }

        public PurchaseStatus Status { get; set; }

        public string ThirdPartyID { get; set; }

        public enum PurchaseStatus
        {
            FAILED = 3,
            IN_PROGRESS = 4,
            NOT_FOUND = 1,
            SUCCEEDED = 2,
            UNKNOWN = -1
        }
    }

    public delegate void TimeoutHandler(int pendingId);

    public enum TrackLevel
    {
        LEVEL_ERROR = 3,
        LEVEL_INFO = 1,
        LEVEL_WARN = 2
    }

    public enum TrackWhat
    {
        TRACK_ACCEPT_FRIEND_GAME_WITH_CUSTOM_DECK = 10,
        TRACK_ACCEPT_FRIEND_GAME_WITH_PRECON_DECK = 11,
        TRACK_AUTO_COMPLETE_DECK_CLICKED = 0x24,
        TRACK_BOOSTER_OPENED = 0x1b,
        TRACK_BOX_SCREEN_VISIT = 0x23,
        TRACK_BUTTON_DRAFT = 13,
        TRACK_BUTTON_PRACTICE = 15,
        TRACK_BUTTON_TOURNAMENT = 12,
        TRACK_CANCEL_MATCHMAKER = 0x18,
        TRACK_CHALLENGE_FRIEND_WITH_CUSTOM_DECK = 8,
        TRACK_CHALLENGE_FRIEND_WITH_PRECON_DECK = 9,
        TRACK_CM_CARDS_SEARCHED = 30,
        TRACK_CM_HIDE_PREMIUMS_NOT_OWNED = 0x26,
        TRACK_CM_MANA_FILTER_CLICKED = 0x1c,
        TRACK_CM_MANA_FILTER_OFF = 0x1d,
        TRACK_CM_NEW_DECK_CREATED = 0x21,
        TRACK_CM_PAGE_TURNED = 0x22,
        TRACK_CM_SHOW_CARDS_I_DONT_OWN_TURNED_OFF = 0x20,
        TRACK_CM_SHOW_CARDS_I_DONT_OWN_TURNED_ON = 0x1f,
        TRACK_CM_SHOW_PREMIUMS_NOT_OWNED = 0x25,
        TRACK_COLLECTION_MANAGER = 1,
        TRACK_DISPLAYED_LOSS_SCREEN = 0x12,
        TRACK_DISPLAYED_TIE_SCREEN = 0x13,
        TRACK_DISPLAYED_WIN_SCREEN = 0x11,
        TRACK_GAME_START = 0x17,
        TRACK_LOGIN_FINISHED = 0x15,
        TRACK_LOGOUT_STARTING = 0x16,
        TRACK_PLAY_CASUAL_WITH_CUSTOM_DECK = 4,
        TRACK_PLAY_CASUAL_WITH_PRECON_DECK = 5,
        TRACK_PLAY_FORGE_QUEUE = 0x27,
        TRACK_PLAY_PRACTICE_WITH_CUSTOM_DECK = 2,
        TRACK_PLAY_PRACTICE_WITH_PRECON_DECK = 3,
        TRACK_PLAY_TOURNAMENT_WITH_CUSTOM_DECK = 6,
        TRACK_PLAY_TOURNAMENT_WITH_PRECON_DECK = 7,
        TRACK_RECEIVED_BOOSTER_PACK = 20,
        TRACK_START_TUTORIAL = 0x10,
        TRACK_TOGGLE_DECK_TYPE = 0x1a,
        TRACK_VISIT_PACK_OPEN_SCREEN = 0x19,
        ZZ_DEPRECATED_TRACK_BUTTON_CASUAL = 14
    }

    public class TriggeredEvent
    {
        public TriggeredEvent()
        {
            this.EventID = 0;
            this.Success = false;
        }

        public int EventID { get; set; }

        public bool Success { get; set; }
    }

    public class TurnTimerInfo
    {
        public float Seconds { get; set; }

        public bool Show { get; set; }

        public int Turn { get; set; }
    }

    public class UnavailableReason
    {
        public string extraData;
        public string mainReason;
        public string subReason;
    }

    public class UserUI
    {
        public EmoteInfo emoteInfo;
        public MouseInfo mouseInfo;
        public int? playerId;

        public class EmoteInfo
        {
            public int Emote { get; set; }
        }

        public class MouseInfo
        {
            public int ArrowOriginID { get; set; }

            public int HeldCardID { get; set; }

            public int OverCardID { get; set; }

            public int X { get; set; }

            public int Y { get; set; }
        }
    }

    public class ValidatedAchieve
    {
        public int AchieveID { get; set; }
    }

    public delegate void WhisperHandler(BnetWhisper[] dllWhispers);
}

