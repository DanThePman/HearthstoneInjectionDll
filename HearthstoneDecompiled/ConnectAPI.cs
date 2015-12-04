using BobNetProto;
using PegasusGame;
using PegasusShared;
using PegasusUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using WTCG.BI;

public class ConnectAPI : IConnectionListener<PegasusPacket>
{
    public const int ACTION_END = 6;
    public const int ACTION_START = 5;
    public const int CREATE_GAME = 7;
    private static int DEBUG_CLIENT_TCP_PORT = 0x4ca;
    private const float ERROR_HANDLING_DELAY = 0.4f;
    public const int FULL_ENTITY = 1;
    public const int HIDE_ENTITY = 3;
    private static PegasusUtil.AckCardSeen m_ackCardSeenPacket;
    private static List<ThrottledPacketListener> m_throttledPacketListeners = new List<ThrottledPacketListener>();
    private const int MAX_SERVERS = 3;
    public const int META_DATA = 8;
    private const int NO_CONTEXT = 0;
    private static ClientRequestManager s_clientRequestManager = new ClientRequestManager();
    private static ConnectAPI s_connectAPI;
    private static ClientConnection<PegasusPacket> s_debugConsole;
    private static ServerConnection<PegasusPacket> s_debugListener;
    private static Queue<PegasusPacket> s_debugPackets = new Queue<PegasusPacket>();
    private static bool s_disconnectRequested;
    private static List<ConnectErrorParams> s_errorList = new List<ConnectErrorParams>();
    private static bool s_gameConceded;
    private static Queue<PegasusPacket> s_gamePackets = new Queue<PegasusPacket>();
    private static ClientConnection<PegasusPacket> s_gameServer;
    private static int s_gameServerKeepAliveFrequency = 0;
    private static GameStartState s_gameStartState = GameStartState.INVALID;
    private static bool s_initialized = false;
    private static float s_lastPingReceived;
    private static float s_lastPingSent;
    private static SortedDictionary<int, PacketDecoder> s_packetDecoders = new SortedDictionary<int, PacketDecoder>();
    private static int s_pingsSentSinceLastPong = 0;
    private static PlatformDependentValue<bool> s_reconnectAfterFailedPings;
    public static int SEND_DECK_DATA_NO_CARD_BACK_CHANGE;
    public static int SEND_DECK_DATA_NO_HERO_ASSET_CHANGE;
    public const int SHOW_ENTITY = 2;
    private const int STARTING_CONTEXT = 1;
    public const int TAG_CHANGE = 4;
    public const int UNKNOWN_HISTORY = 0;
    private const ulong VALID_TOURNEY_DECK_FLAGS = 0x1fL;

    static ConnectAPI()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = true,
            Android = true,
            iOS = true
        };
        s_reconnectAfterFailedPings = value2;
        SEND_DECK_DATA_NO_HERO_ASSET_CHANGE = -1;
        SEND_DECK_DATA_NO_CARD_BACK_CHANGE = -1;
        m_ackCardSeenPacket = new PegasusUtil.AckCardSeen();
    }

    public static void AbortBlizzardPurchase(bool isAutoCanceled)
    {
        CancelPurchase body = new CancelPurchase {
            IsAutoCancel = isAutoCanceled,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        };
        UtilOutbound(0x112, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void AbortThirdPartyPurchase(CancelPurchase.ThirdPartyCancelReason reason)
    {
        CancelPurchase body = new CancelPurchase {
            IsAutoCancel = false,
            ThirdPartyReason = reason,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        };
        UtilOutbound(0x112, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void AckAchieveProgress(int ID, int ackProgress)
    {
        PegasusUtil.AckAchieveProgress body = new PegasusUtil.AckAchieveProgress {
            Id = ID,
            AckProgress = ackProgress
        };
        UtilOutbound(0xf3, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void AckCardSeen(int assetID, int premium)
    {
        PegasusShared.CardDef item = new PegasusShared.CardDef {
            Asset = assetID
        };
        if (premium != 0)
        {
            item.Premium = premium;
        }
        m_ackCardSeenPacket.CardDefs.Add(item);
        if (m_ackCardSeenPacket.CardDefs.Count > 10)
        {
            SendAckCardsSeen();
        }
    }

    public static void AckNotice(long ID)
    {
        PegasusUtil.AckNotice body = new PegasusUtil.AckNotice {
            Entry = ID
        };
        UtilOutbound(0xd5, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void AcknowledgeBanner(int banner)
    {
        PegasusUtil.AcknowledgeBanner body = new PegasusUtil.AcknowledgeBanner {
            Banner = banner
        };
        UtilOutbound(0x135, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void AckWingProgress(int wing, int ack)
    {
        PegasusUtil.AckWingProgress body = new PegasusUtil.AckWingProgress {
            Wing = wing,
            Ack = ack
        };
        UtilOutbound(0x134, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void ApplicationPaused()
    {
        if (m_ackCardSeenPacket.CardDefs.Count != 0)
        {
            SendAckCardsSeen();
        }
    }

    private static bool AreDeckFlagsTourneyValid(ulong deckValidityFlags)
    {
        return ((deckValidityFlags & ((ulong) 0x1fL)) == 0x1fL);
    }

    public static void BeginThirdPartyPurchase(BattlePayProvider provider, string productID, int quantity)
    {
        StartThirdPartyPurchase body = new StartThirdPartyPurchase {
            Provider = provider,
            ProductId = productID,
            Quantity = quantity,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        };
        UtilOutbound(0x138, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void BeginThirdPartyPurchaseWithReceipt(BattlePayProvider provider, string productID, int quantity, string thirdPartyID, string base64receipt, string thirdPartyUserID)
    {
        object[] objArray1 = new object[] { provider, "|", productID, "|", thirdPartyID, "|", thirdPartyUserID };
        string message = string.Concat(objArray1);
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED_DANGLING, 0, message);
        ThirdPartyReceiptData data = new ThirdPartyReceiptData {
            ThirdPartyId = thirdPartyID,
            Receipt = base64receipt
        };
        if (!string.IsNullOrEmpty(thirdPartyUserID))
        {
            data.ThirdPartyUserId = thirdPartyUserID;
        }
        StartThirdPartyPurchase body = new StartThirdPartyPurchase {
            Provider = provider,
            ProductId = productID,
            Quantity = quantity,
            DanglingReceiptData = data,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        };
        UtilOutbound(0x138, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void BuyCard(int id, int premium, int count, int unitBuyPrice)
    {
        BuySellCard body = new BuySellCard();
        PegasusShared.CardDef def = new PegasusShared.CardDef {
            Asset = id
        };
        if (premium != 0)
        {
            def.Premium = premium;
        }
        body.Def = def;
        body.Buying = true;
        if (count != 1)
        {
            body.Count = count;
        }
        body.UnitBuyPrice = unitBuyPrice;
        UtilOutbound(0x101, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void CheckAccountLicenseAchieve(int achieveID)
    {
        PegasusUtil.CheckAccountLicenseAchieve body = new PegasusUtil.CheckAccountLicenseAchieve {
            Achieve = achieveID
        };
        UtilOutbound(0x129, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    private static bool CheckType(PegasusPacket p, int packetId)
    {
        bool flag = (p == null) || (p.Type != packetId);
        if (flag)
        {
            Debug.LogError("ERROR: invalid type " + p);
        }
        return !flag;
    }

    public static void CloseAll()
    {
        s_clientRequestManager.Terminate();
        if (m_ackCardSeenPacket.CardDefs.Count != 0)
        {
            SendAckCardsSeen();
        }
        if (s_gameServer != null)
        {
            s_gameServer.Disconnect();
        }
        if (s_debugConsole != null)
        {
            s_debugConsole.Disconnect();
        }
        if (s_debugListener != null)
        {
            s_debugListener.Disconnect();
        }
    }

    public static void Concede()
    {
        s_gameConceded = true;
        PegasusGame.Concede body = new PegasusGame.Concede();
        QueueGamePacket(11, body);
    }

    public static void ConfirmPurchase()
    {
        DoPurchase body = new DoPurchase();
        UtilOutbound(0x111, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    private static bool ConnectDebugConsole()
    {
        if (s_debugListener == null)
        {
            return false;
        }
        if ((s_debugConsole == null) || !s_debugConsole.Active)
        {
            ClientConnection<PegasusPacket> nextAcceptedConnection = s_debugListener.GetNextAcceptedConnection();
            if (nextAcceptedConnection == null)
            {
                return false;
            }
            s_debugConsole = nextAcceptedConnection;
            nextAcceptedConnection.AddListener(s_connectAPI, s_debugPackets);
            nextAcceptedConnection.StartReceiving();
        }
        return true;
    }

    public static bool ConnectInit()
    {
        s_gameStartState = GameStartState.INVALID;
        s_errorList.Clear();
        if (!s_initialized)
        {
            s_connectAPI = new ConnectAPI();
            ApplicationMgr.Get().WillReset += new System.Action(ConnectAPI.WillReset);
            s_gameServer = new ClientConnection<PegasusPacket>();
            s_gameServer.AddListener(s_connectAPI, s_gamePackets);
            s_gameServer.AddConnectHandler(new ClientConnection<PegasusPacket>.ConnectHandler(s_connectAPI.OnGameServerConnectCallback));
            s_gameServer.AddDisconnectHandler(new ClientConnection<PegasusPacket>.DisconnectHandler(s_connectAPI.OnGameServerDisconnectCallback));
            s_debugListener = new ServerConnection<PegasusPacket>();
            s_debugListener.Open(DEBUG_CLIENT_TCP_PORT);
            s_packetDecoders.Add(0x74, new PongPacketDecoder());
            s_packetDecoders.Add(0xa9, new DefaultProtobufPacketDecoder<Deadend>());
            s_packetDecoders.Add(0xa7, new DefaultProtobufPacketDecoder<DeadendUtil>());
            s_packetDecoders.Add(0x7b, new DefaultProtobufPacketDecoder<DebugConsoleCommand>());
            s_packetDecoders.Add(0x7c, new DefaultProtobufPacketDecoder<BobNetProto.DebugConsoleResponse>());
            s_packetDecoders.Add(14, new DefaultProtobufPacketDecoder<AllOptions>());
            s_packetDecoders.Add(5, new DefaultProtobufPacketDecoder<DebugMessage>());
            s_packetDecoders.Add(0x11, new DefaultProtobufPacketDecoder<PegasusGame.EntityChoices>());
            s_packetDecoders.Add(13, new DefaultProtobufPacketDecoder<PegasusGame.EntitiesChosen>());
            s_packetDecoders.Add(0x10, new DefaultProtobufPacketDecoder<PegasusGame.GameSetup>());
            s_packetDecoders.Add(0x13, new DefaultProtobufPacketDecoder<PegasusGame.PowerHistory>());
            s_packetDecoders.Add(15, new DefaultProtobufPacketDecoder<PegasusGame.UserUI>());
            s_packetDecoders.Add(9, new DefaultProtobufPacketDecoder<PegasusGame.TurnTimer>());
            s_packetDecoders.Add(10, new DefaultProtobufPacketDecoder<NAckOption>());
            s_packetDecoders.Add(12, new DefaultProtobufPacketDecoder<GameCanceled>());
            s_packetDecoders.Add(0x17, new DefaultProtobufPacketDecoder<ServerResult>());
            s_packetDecoders.Add(0x18, new DefaultProtobufPacketDecoder<SpectatorNotify>());
            s_packetDecoders.Add(0x121, new DefaultProtobufPacketDecoder<Disconnected>());
            s_packetDecoders.Add(0xca, new DefaultProtobufPacketDecoder<DeckList>());
            s_packetDecoders.Add(0xcf, new DefaultProtobufPacketDecoder<Collection>());
            s_packetDecoders.Add(0xd7, new DefaultProtobufPacketDecoder<PegasusUtil.DeckContents>());
            s_packetDecoders.Add(0xd8, new DefaultProtobufPacketDecoder<PegasusUtil.DBAction>());
            s_packetDecoders.Add(0xd9, new DefaultProtobufPacketDecoder<PegasusUtil.DeckCreated>());
            s_packetDecoders.Add(0xda, new DefaultProtobufPacketDecoder<PegasusUtil.DeckDeleted>());
            s_packetDecoders.Add(0xdb, new DefaultProtobufPacketDecoder<PegasusUtil.DeckRenamed>());
            s_packetDecoders.Add(0xd4, new DefaultProtobufPacketDecoder<PegasusUtil.ProfileNotices>());
            s_packetDecoders.Add(0xe0, new DefaultProtobufPacketDecoder<BoosterList>());
            s_packetDecoders.Add(0x139, new DefaultProtobufPacketDecoder<BoosterTallyList>());
            s_packetDecoders.Add(0xe2, new DefaultProtobufPacketDecoder<BoosterContent>());
            s_packetDecoders.Add(0xd0, new DefaultProtobufPacketDecoder<GamesInfo>());
            s_packetDecoders.Add(0xe7, new DefaultProtobufPacketDecoder<ProfileDeckLimit>());
            s_packetDecoders.Add(0x106, new DefaultProtobufPacketDecoder<ArcaneDustBalance>());
            s_packetDecoders.Add(0x116, new DefaultProtobufPacketDecoder<GoldBalance>());
            s_packetDecoders.Add(0xe9, new DefaultProtobufPacketDecoder<ProfileProgress>());
            s_packetDecoders.Add(270, new DefaultProtobufPacketDecoder<PlayerRecords>());
            s_packetDecoders.Add(0x10f, new DefaultProtobufPacketDecoder<RewardProgress>());
            s_packetDecoders.Add(0xe8, new DefaultProtobufPacketDecoder<MedalInfo>());
            s_packetDecoders.Add(0xea, new DefaultProtobufPacketDecoder<PegasusUtil.MedalHistory>());
            s_packetDecoders.Add(0xf1, new DefaultProtobufPacketDecoder<ClientOptions>());
            s_packetDecoders.Add(0xf6, new DefaultProtobufPacketDecoder<DraftBeginning>());
            s_packetDecoders.Add(0xf7, new DefaultProtobufPacketDecoder<PegasusUtil.DraftRetired>());
            s_packetDecoders.Add(0xf8, new DefaultProtobufPacketDecoder<PegasusUtil.DraftChoicesAndContents>());
            s_packetDecoders.Add(0xf9, new DefaultProtobufPacketDecoder<PegasusUtil.DraftChosen>());
            s_packetDecoders.Add(0x120, new DefaultProtobufPacketDecoder<DraftRewardsAcked>());
            s_packetDecoders.Add(0xfb, new DefaultProtobufPacketDecoder<PegasusUtil.DraftError>());
            s_packetDecoders.Add(0xfc, new DefaultProtobufPacketDecoder<Achieves>());
            s_packetDecoders.Add(0x11d, new DefaultProtobufPacketDecoder<ValidateAchieveResponse>());
            s_packetDecoders.Add(0x11a, new DefaultProtobufPacketDecoder<CancelQuestResponse>());
            s_packetDecoders.Add(0x108, new DefaultProtobufPacketDecoder<GuardianVars>());
            s_packetDecoders.Add(260, new DefaultProtobufPacketDecoder<CardValues>());
            s_packetDecoders.Add(0x102, new DefaultProtobufPacketDecoder<BoughtSoldCard>());
            s_packetDecoders.Add(0x10d, new DefaultProtobufPacketDecoder<PegasusUtil.MassDisenchantResponse>());
            s_packetDecoders.Add(0x109, new DefaultProtobufPacketDecoder<BattlePayStatusResponse>());
            s_packetDecoders.Add(0x127, new DefaultProtobufPacketDecoder<PegasusUtil.ThirdPartyPurchaseStatusResponse>());
            s_packetDecoders.Add(0x110, new DefaultProtobufPacketDecoder<PegasusUtil.PurchaseMethod>());
            s_packetDecoders.Add(0x113, new DefaultProtobufPacketDecoder<CancelPurchaseResponse>());
            s_packetDecoders.Add(0x100, new DefaultProtobufPacketDecoder<PegasusUtil.PurchaseResponse>());
            s_packetDecoders.Add(0xee, new DefaultProtobufPacketDecoder<BattlePayConfigResponse>());
            s_packetDecoders.Add(280, new DefaultProtobufPacketDecoder<PurchaseWithGoldResponse>());
            s_packetDecoders.Add(0x11b, new DefaultProtobufPacketDecoder<HeroXP>());
            s_packetDecoders.Add(0xfe, new NoOpPacketDecoder());
            s_packetDecoders.Add(0x11e, new DefaultProtobufPacketDecoder<PlayQueue>());
            s_packetDecoders.Add(330, new DefaultProtobufPacketDecoder<CheckAccountLicensesResponse>());
            s_packetDecoders.Add(0x14b, new DefaultProtobufPacketDecoder<CheckGameLicensesResponse>());
            s_packetDecoders.Add(0xec, new DefaultProtobufPacketDecoder<CardBacks>());
            s_packetDecoders.Add(0x124, new DefaultProtobufPacketDecoder<SetCardBackResponse>());
            s_packetDecoders.Add(0x128, new DefaultProtobufPacketDecoder<SetProgressResponse>());
            s_packetDecoders.Add(0x12b, new DefaultProtobufPacketDecoder<TriggerEventResponse>());
            s_packetDecoders.Add(300, new DefaultProtobufPacketDecoder<NotSoMassiveLoginReply>());
            s_packetDecoders.Add(0x130, new DefaultProtobufPacketDecoder<AssetsVersionResponse>());
            s_packetDecoders.Add(0x132, new DefaultProtobufPacketDecoder<AdventureProgressResponse>());
            s_packetDecoders.Add(0x133, new DefaultProtobufPacketDecoder<UpdateLoginComplete>());
            s_packetDecoders.Add(0x137, new DefaultProtobufPacketDecoder<PegasusUtil.AccountLicenseAchieveResponse>());
            s_packetDecoders.Add(0x13b, new DefaultProtobufPacketDecoder<SubscribeResponse>());
            s_packetDecoders.Add(0x13c, new DefaultProtobufPacketDecoder<TavernBrawlInfo>());
            s_packetDecoders.Add(0x13d, new DefaultProtobufPacketDecoder<TavernBrawlPlayerRecordResponse>());
            s_packetDecoders.Add(0x13e, new DefaultProtobufPacketDecoder<FavoriteHeroesResponse>());
            s_packetDecoders.Add(320, new DefaultProtobufPacketDecoder<PegasusUtil.SetFavoriteHeroResponse>());
            s_packetDecoders.Add(0x144, new DefaultProtobufPacketDecoder<DebugCommandResponse>());
            s_packetDecoders.Add(0x145, new DefaultProtobufPacketDecoder<AccountLicensesInfoResponse>());
            s_packetDecoders.Add(0x146, new DefaultProtobufPacketDecoder<PegasusUtil.GenericResponse>());
            s_packetDecoders.Add(0x148, new DefaultProtobufPacketDecoder<ClientRequestResponse>());
            s_packetDecoders.Add(0x142, new DefaultProtobufPacketDecoder<PegasusUtil.GetAssetResponse>());
            s_initialized = true;
        }
        return true;
    }

    public static bool ConnectsWithAurora()
    {
        return true;
    }

    public static bool ConnectsWithBobnet()
    {
        return false;
    }

    private static Network.PurchaseErrorInfo ConvertPurchaseError(PurchaseError purchaseError)
    {
        Network.PurchaseErrorInfo info = new Network.PurchaseErrorInfo {
            Error = (Network.PurchaseErrorInfo.ErrorType) purchaseError.Error_
        };
        if (purchaseError.HasPurchaseInProgress)
        {
            info.PurchaseInProgressProductID = purchaseError.PurchaseInProgress;
        }
        if (purchaseError.HasErrorCode)
        {
            info.ErrorCode = purchaseError.ErrorCode;
        }
        return info;
    }

    private static RewardData ConvertRewardBag(RewardBag bag)
    {
        if (bag.HasRewardBooster)
        {
            return new BoosterPackRewardData(bag.RewardBooster.BoosterType, bag.RewardBooster.BoosterCount);
        }
        if (bag.HasRewardCard)
        {
            return new CardRewardData(GameUtils.TranslateDbIdToCardId(bag.RewardCard.Card.Asset), (TAG_PREMIUM) bag.RewardCard.Card.Premium, 1);
        }
        if (bag.HasRewardDust)
        {
            return new ArcaneDustRewardData(bag.RewardDust.Amount);
        }
        if (bag.HasRewardGold)
        {
            return new GoldRewardData((long) bag.RewardGold.Amount);
        }
        if (bag.HasRewardCardBack)
        {
            return new CardBackRewardData(bag.RewardCardBack.CardBack);
        }
        Debug.LogError("Unrecognized draft bag reward");
        return null;
    }

    private static Network.RewardChest ConvertRewardChest(PegasusShared.RewardChest chest)
    {
        Network.RewardChest chest2 = new Network.RewardChest();
        if (chest.HasBag1)
        {
            chest2.Rewards.Add(ConvertRewardBag(chest.Bag1));
        }
        if (chest.HasBag2)
        {
            chest2.Rewards.Add(ConvertRewardBag(chest.Bag2));
        }
        if (chest.HasBag3)
        {
            chest2.Rewards.Add(ConvertRewardBag(chest.Bag3));
        }
        if (chest.HasBag4)
        {
            chest2.Rewards.Add(ConvertRewardBag(chest.Bag4));
        }
        if (chest.HasBag5)
        {
            chest2.Rewards.Add(ConvertRewardBag(chest.Bag5));
        }
        return chest2;
    }

    private static List<int> CopyIntList(IList<int> intList)
    {
        int[] array = new int[intList.Count];
        intList.CopyTo(array, 0);
        return new List<int>(array);
    }

    public static void CreateDeck(DeckType deckType, string name, int hero, TAG_PREMIUM heroPremium)
    {
        PegasusUtil.CreateDeck body = new PegasusUtil.CreateDeck {
            Name = name,
            Hero = hero,
            HeroPremium = (int) heroPremium,
            DeckType = deckType
        };
        UtilOutbound(0xd1, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static long DateToUTC(Date date)
    {
        DateTime time = new DateTime(date.Year, date.Month, date.Day, date.Hours, date.Min, date.Sec);
        return time.ToFileTimeUtc();
    }

    public static Network.DBAction DBAction()
    {
        PegasusUtil.DBAction action = Unpack<PegasusUtil.DBAction>(NextUtil(), 0xd8);
        if (action == null)
        {
            return null;
        }
        return new Network.DBAction { Action = (Network.DBAction.ActionType) action.Action, Result = (Network.DBAction.ResultType) action.Result, MetaData = action.MetaData };
    }

    public static NetCache.DeckHeader DeckCreated()
    {
        PegasusUtil.DeckCreated created = Unpack<PegasusUtil.DeckCreated>(NextUtil(), 0xd9);
        if (created == null)
        {
            return null;
        }
        DeckInfo info = created.Info;
        return new NetCache.DeckHeader { ID = info.Id, Name = info.Name, Hero = GameUtils.TranslateDbIdToCardId(info.Hero), HeroPremium = (TAG_PREMIUM) info.HeroPremium, HeroPower = GameUtils.GetHeroPowerCardIdFromHero(info.Hero), IsTourneyValid = AreDeckFlagsTourneyValid(info.Validity), Type = info.DeckType, CardBack = info.CardBack, CardBackOverridden = info.CardBackOverride, HeroOverridden = info.HeroOverride, SeasonId = info.SeasonId };
    }

    public static long DeckDeleted()
    {
        PegasusUtil.DeckDeleted deleted = Unpack<PegasusUtil.DeckDeleted>(NextUtil(), 0xda);
        if (deleted == null)
        {
            return 0L;
        }
        return deleted.Deck;
    }

    public static Network.DeckName DeckRenamed()
    {
        PegasusUtil.DeckRenamed renamed = Unpack<PegasusUtil.DeckRenamed>(NextUtil(), 0xdb);
        if (renamed == null)
        {
            return null;
        }
        return new Network.DeckName { Deck = renamed.Deck, Name = renamed.Name };
    }

    private static void DecodeAndProcessPacket(PegasusPacket input)
    {
        PacketDecoder decoder;
        if (s_packetDecoders.TryGetValue(input.Type, out decoder))
        {
            PegasusPacket packet = decoder.HandlePacket(input);
            if (packet != null)
            {
                s_clientRequestManager.NotifyResponseReceived(packet);
            }
        }
    }

    public static void DeleteClientOption(int key)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void DeleteDeck(long deckID)
    {
        PegasusUtil.DeleteDeck body = new PegasusUtil.DeleteDeck {
            Deck = deckID
        };
        UtilOutbound(210, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void DisconnectFromGameServer()
    {
        s_disconnectRequested = true;
        s_gameServer.Disconnect();
    }

    public static void DoLoginUpdate()
    {
        UpdateLogin body = new UpdateLogin();
        string str = Vars.Key("Application.Referral").GetStr("none");
        if (str != "none")
        {
            body.Referral = str;
        }
        else if ((PlatformSettings.OS == OSCategory.PC) || (PlatformSettings.OS == OSCategory.Mac))
        {
            body.Referral = "Battle.net";
        }
        else if (PlatformSettings.OS == OSCategory.iOS)
        {
            body.Referral = "AppleAppStore";
        }
        else if (PlatformSettings.OS == OSCategory.Android)
        {
            switch (ApplicationMgr.GetAndroidStore())
            {
                case AndroidStore.GOOGLE:
                    body.Referral = "GooglePlay";
                    break;

                case AndroidStore.AMAZON:
                    body.Referral = "AmazonAppStore";
                    break;
            }
        }
        UtilOutbound(0xcd, 0, body, ClientRequestManager.RequestPhase.STARTUP, 0);
    }

    public static void DraftAckRewards(long deckID, int slot)
    {
        PegasusUtil.DraftAckRewards body = new PegasusUtil.DraftAckRewards {
            DeckId = deckID,
            Slot = slot
        };
        UtilOutbound(0x11f, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void DraftBegin()
    {
        PegasusUtil.DraftBegin body = new PegasusUtil.DraftBegin();
        UtilOutbound(0xeb, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static Network.DraftChosen DraftCardChosen()
    {
        PegasusUtil.DraftChosen chosen = Unpack<PegasusUtil.DraftChosen>(NextUtil(), 0xf9);
        if (chosen == null)
        {
            return null;
        }
        Network.DraftChosen chosen2 = new Network.DraftChosen {
            ChosenCard = { Name = GameUtils.TranslateDbIdToCardId(chosen.Chosen.Asset), Premium = chosen.Chosen.Premium }
        };
        foreach (PegasusShared.CardDef def in chosen.NextChoiceList)
        {
            NetCache.CardDefinition item = new NetCache.CardDefinition {
                Name = GameUtils.TranslateDbIdToCardId(def.Asset),
                Premium = (TAG_PREMIUM) def.Premium
            };
            chosen2.NextChoices.Add(item);
        }
        return chosen2;
    }

    public static Network.BeginDraft DraftGetBeginning()
    {
        DraftBeginning beginning = Unpack<DraftBeginning>(NextUtil(), 0xf6);
        if (beginning == null)
        {
            return null;
        }
        Network.BeginDraft draft = new Network.BeginDraft {
            DeckID = beginning.DeckId
        };
        foreach (PegasusShared.CardDef def in beginning.ChoiceList)
        {
            NetCache.CardDefinition item = new NetCache.CardDefinition {
                Name = GameUtils.TranslateDbIdToCardId(def.Asset),
                Premium = (TAG_PREMIUM) def.Premium
            };
            draft.Heroes.Add(item);
        }
        return draft;
    }

    public static Network.DraftChoicesAndContents DraftGetChoicesAndContents()
    {
        PegasusUtil.DraftChoicesAndContents contents = Unpack<PegasusUtil.DraftChoicesAndContents>(NextUtil(), 0xf8);
        if (contents == null)
        {
            return null;
        }
        Network.DraftChoicesAndContents contents2 = new Network.DraftChoicesAndContents {
            DeckInfo = { Deck = contents.DeckId },
            Slot = contents.Slot
        };
        contents2.Hero.Name = (contents.HeroDef.Asset != 0) ? GameUtils.TranslateDbIdToCardId(contents.HeroDef.Asset) : string.Empty;
        contents2.Hero.Premium = (TAG_PREMIUM) contents.HeroDef.Premium;
        contents2.Wins = contents.Wins;
        contents2.Losses = contents.Losses;
        contents2.MaxWins = !contents.HasMaxWins ? 0x7fffffff : contents.MaxWins;
        foreach (PegasusShared.CardDef def in contents.ChoiceList)
        {
            if (def.Asset != 0)
            {
                NetCache.CardDefinition item = new NetCache.CardDefinition {
                    Name = GameUtils.TranslateDbIdToCardId(def.Asset),
                    Premium = (TAG_PREMIUM) def.Premium
                };
                contents2.Choices.Add(item);
            }
        }
        foreach (DeckCardData data in contents.Cards)
        {
            Network.CardUserData data2 = new Network.CardUserData {
                DbId = data.Def.Asset,
                Count = 1,
                Premium = !data.Def.HasPremium ? TAG_PREMIUM.NORMAL : ((TAG_PREMIUM) data.Def.Premium)
            };
            contents2.DeckInfo.Cards.Add(data2);
        }
        contents2.Chest = !contents.HasChest ? null : ConvertRewardChest(contents.Chest);
        return contents2;
    }

    public static int DraftGetError()
    {
        PegasusUtil.DraftError error = Unpack<PegasusUtil.DraftError>(NextUtil(), 0xfb);
        if (error == null)
        {
            return 0;
        }
        return (int) error.ErrorCode_;
    }

    public static void DraftGetPicksAndContents()
    {
        PegasusUtil.DraftGetPicksAndContents body = new PegasusUtil.DraftGetPicksAndContents();
        UtilOutbound(0xf4, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static Network.DraftRetired DraftHandleRetired()
    {
        PegasusUtil.DraftRetired retired = Unpack<PegasusUtil.DraftRetired>(NextUtil(), 0xf7);
        if (retired == null)
        {
            return null;
        }
        return new Network.DraftRetired { Deck = retired.DeckId, Chest = ConvertRewardChest(retired.Chest) };
    }

    public static long DraftHandleRewardsAck()
    {
        DraftRewardsAcked acked = Unpack<DraftRewardsAcked>(NextUtil(), 0x120);
        if (acked == null)
        {
            return 0L;
        }
        return acked.DeckId;
    }

    public static void DraftMakePick(long deckID, int slot, int index)
    {
        PegasusUtil.DraftMakePick body = new PegasusUtil.DraftMakePick {
            DeckId = deckID,
            Slot = slot,
            Index = index
        };
        UtilOutbound(0xf5, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void DraftRetire(long deckID, int slot)
    {
        PegasusUtil.DraftRetire body = new PegasusUtil.DraftRetire {
            DeckId = deckID,
            Slot = slot
        };
        UtilOutbound(0xf2, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void DropAllGamePackets()
    {
        DropAllPackets(ServerType.GAME_SERVER);
    }

    private static void DropAllPackets(ServerType type)
    {
        Queue<PegasusPacket> queue = null;
        switch (type)
        {
            case ServerType.GAME_SERVER:
                queue = s_gamePackets;
                break;

            case ServerType.DEBUG_CONSOLE:
                queue = s_debugPackets;
                break;
        }
        if (queue != null)
        {
            object[] args = new object[] { type, queue.Count };
            Log.LoadingScreen.Print("ConnectAPI.DropAllPackets() - {0} dropped {1} packets", args);
            queue.Clear();
        }
    }

    public static void DropDebugPacket()
    {
        DropPacket(ServerType.DEBUG_CONSOLE);
    }

    public static void DropGamePacket()
    {
        DropPacket(ServerType.GAME_SERVER);
    }

    private static void DropPacket(ServerType type)
    {
        Queue<PegasusPacket> queue = null;
        switch (type)
        {
            case ServerType.GAME_SERVER:
                queue = s_gamePackets;
                break;

            case ServerType.UTIL_SERVER:
                s_clientRequestManager.DropNextClientRequest();
                break;

            case ServerType.DEBUG_CONSOLE:
                queue = s_debugPackets;
                break;
        }
        if ((queue != null) && (queue.Count > 0))
        {
            queue.Dequeue();
        }
    }

    public static void DropUtilPacket()
    {
        DropPacket(ServerType.UTIL_SERVER);
    }

    private static void FakeUtilOutbound(int type, int system, IProtoBuf body, int subID)
    {
        if (type == 240)
        {
            Network.Get().FakeHandleType(ClientOptions.PacketID.ID);
        }
        else if (type == 0xfd)
        {
            Network.Get().FakeHandleType(Achieves.PacketID.ID);
        }
        else if (type == 0xc9)
        {
            GetAccountInfo.Request request = (GetAccountInfo.Request) subID;
            FakeUtilOutboundGetAccountInfo(request);
        }
        else if (type == 0x147)
        {
            GenericRequestList list = (GenericRequestList) body;
            foreach (GenericRequest request2 in list.Requests)
            {
                switch (request2.RequestId)
                {
                    case 0xc9:
                        FakeUtilOutboundGetAccountInfo((GetAccountInfo.Request) request2.RequestSubId);
                        break;

                    case 240:
                        Network.Get().FakeHandleType(ClientOptions.PacketID.ID);
                        break;

                    case 0xfd:
                        Network.Get().FakeHandleType(Achieves.PacketID.ID);
                        break;
                }
            }
        }
        else if ((((type != 0x131) && (type != 0x10b)) && (type != 0x114)) && (type != 0xef))
        {
            Debug.LogError(string.Format("Unhandled fake response for request {0} of packet ", type));
        }
    }

    private static void FakeUtilOutboundGetAccountInfo(GetAccountInfo.Request request)
    {
        Enum enumId = 0;
        switch (request)
        {
            case GetAccountInfo.Request.DECK_LIST:
                enumId = DeckList.PacketID.ID;
                break;

            case GetAccountInfo.Request.COLLECTION:
                enumId = Collection.PacketID.ID;
                break;

            case GetAccountInfo.Request.MEDAL_INFO:
                enumId = MedalInfo.PacketID.ID;
                break;

            case GetAccountInfo.Request.BOOSTERS:
                enumId = BoosterList.PacketID.ID;
                break;

            case GetAccountInfo.Request.CARD_BACKS:
                enumId = CardBacks.PacketID.ID;
                break;

            case GetAccountInfo.Request.PLAYER_RECORD:
                enumId = PlayerRecords.PacketID.ID;
                break;

            case GetAccountInfo.Request.DECK_LIMIT:
                enumId = ProfileDeckLimit.PacketID.ID;
                break;

            case GetAccountInfo.Request.CAMPAIGN_INFO:
                enumId = ProfileProgress.PacketID.ID;
                break;

            case GetAccountInfo.Request.NOTICES:
                enumId = PegasusUtil.ProfileNotices.PacketID.ID;
                break;

            case GetAccountInfo.Request.CLIENT_OPTIONS:
                enumId = ClientOptions.PacketID.ID;
                break;

            case GetAccountInfo.Request.CARD_VALUES:
                enumId = CardValues.PacketID.ID;
                break;

            case GetAccountInfo.Request.DISCONNECTED:
                enumId = Disconnected.PacketID.ID;
                break;

            case GetAccountInfo.Request.ARCANE_DUST_BALANCE:
                enumId = ArcaneDustBalance.PacketID.ID;
                break;

            case GetAccountInfo.Request.FEATURES:
                enumId = GuardianVars.PacketID.ID;
                break;

            case GetAccountInfo.Request.REWARD_PROGRESS:
                enumId = RewardProgress.PacketID.ID;
                break;

            case GetAccountInfo.Request.GOLD_BALANCE:
                enumId = GoldBalance.PacketID.ID;
                break;

            case GetAccountInfo.Request.HERO_XP:
                enumId = HeroXP.PacketID.ID;
                break;

            case GetAccountInfo.Request.NOT_SO_MASSIVE_LOGIN:
                enumId = NotSoMassiveLoginReply.PacketID.ID;
                break;

            case GetAccountInfo.Request.BOOSTER_TALLY:
                enumId = BoosterTallyList.PacketID.ID;
                break;

            case GetAccountInfo.Request.TAVERN_BRAWL_INFO:
                enumId = TavernBrawlInfo.PacketID.ID;
                break;

            case GetAccountInfo.Request.TAVERN_BRAWL_RECORD:
                enumId = TavernBrawlPlayerRecordResponse.PacketID.ID;
                break;

            case GetAccountInfo.Request.FAVORITE_HEROES:
                enumId = FavoriteHeroesResponse.PacketID.ID;
                break;

            case GetAccountInfo.Request.ACCOUNT_LICENSES:
                enumId = AccountLicensesInfoResponse.PacketID.ID;
                break;
        }
        if (enumId == 0)
        {
            Debug.LogError(string.Format("Fake response for request {0} of packet PegasusUtil.GetAccountInfo not handled", request));
        }
        else
        {
            Network.Get().FakeHandleType(enumId);
        }
    }

    public static Network.AccountLicenseAchieveResponse GetAccountLicenseAchieveResponse()
    {
        PegasusUtil.AccountLicenseAchieveResponse response = Unpack<PegasusUtil.AccountLicenseAchieveResponse>(NextUtil(), 0x137);
        if (response == null)
        {
            return null;
        }
        return new Network.AccountLicenseAchieveResponse { Achieve = response.Achieve, Result = (Network.AccountLicenseAchieveResponse.AchieveResult) response.Result_ };
    }

    public static AccountLicensesInfoResponse GetAccountLicensesInfoResponse()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new AccountLicensesInfoResponse();
        }
        AccountLicensesInfoResponse response = Unpack<AccountLicensesInfoResponse>(NextUtil(), 0x145);
        if (response == null)
        {
            return null;
        }
        return response;
    }

    public static Network.AchieveList GetAchieves()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new Network.AchieveList();
        }
        Achieves achieves = Unpack<Achieves>(NextUtil(), 0xfc);
        if (achieves == null)
        {
            return null;
        }
        Network.AchieveList list2 = new Network.AchieveList();
        foreach (PegasusUtil.Achieve achieve in achieves.List)
        {
            Network.AchieveList.Achieve item = new Network.AchieveList.Achieve {
                ID = achieve.Id,
                Progress = achieve.Progress,
                AckProgress = achieve.AckProgress,
                CompletionCount = !achieve.HasCompletionCount ? 0 : achieve.CompletionCount,
                Active = achieve.HasActive && achieve.Active,
                DateGiven = !achieve.HasDateGiven ? 0L : DateToUTC(achieve.DateGiven),
                DateCompleted = !achieve.HasDateCompleted ? 0L : DateToUTC(achieve.DateCompleted),
                CanAck = !achieve.HasDoNotAck || !achieve.DoNotAck
            };
            list2.Achieves.Add(item);
        }
        return list2;
    }

    public static List<Network.AdventureProgress> GetAdventureProgressResponse()
    {
        AdventureProgressResponse response = Unpack<AdventureProgressResponse>(NextUtil(), 0x132);
        if (response == null)
        {
            return null;
        }
        List<Network.AdventureProgress> list = new List<Network.AdventureProgress>();
        foreach (PegasusShared.AdventureProgress progress in response.List)
        {
            Network.AdventureProgress item = new Network.AdventureProgress {
                Wing = progress.WingId,
                Progress = progress.Progress,
                Ack = progress.Ack,
                Flags = progress.Flags_
            };
            list.Add(item);
        }
        return list;
    }

    public static void GetAllClientOptions()
    {
        PegasusUtil.GetOptions body = new PegasusUtil.GetOptions();
        UtilOutbound(240, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static NetCache.NetCacheHeroLevels GetAllHeroXP()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheHeroLevels();
        }
        HeroXP oxp = Unpack<HeroXP>(NextUtil(), 0x11b);
        if (oxp == null)
        {
            return null;
        }
        NetCache.NetCacheHeroLevels levels2 = new NetCache.NetCacheHeroLevels();
        foreach (HeroXPInfo info in oxp.XpInfos)
        {
            NetCache.HeroLevel item = new NetCache.HeroLevel {
                Class = (TAG_CLASS) info.ClassId
            };
            item.CurrentLevel.Level = info.Level;
            item.CurrentLevel.XP = info.CurrXp;
            item.CurrentLevel.MaxXP = info.MaxXp;
            levels2.Levels.Add(item);
            if (info.HasNextReward)
            {
                item.NextReward = new NetCache.HeroLevel.NextLevelReward();
                item.NextReward.Level = info.NextReward.Level;
                if (info.NextReward.HasRewardBooster)
                {
                    item.NextReward.Reward = new BoosterPackRewardData(info.NextReward.RewardBooster.BoosterType, info.NextReward.RewardBooster.BoosterCount);
                }
                else if (info.NextReward.HasRewardCard)
                {
                    int num;
                    string cardId = GameUtils.TranslateDbIdToCardId(info.NextReward.RewardCard.Card.Asset);
                    TAG_PREMIUM premium = !info.NextReward.RewardCard.Card.HasPremium ? TAG_PREMIUM.NORMAL : ((TAG_PREMIUM) info.NextReward.RewardCard.Card.Premium);
                    EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
                    if (entityDef.IsHero())
                    {
                        num = 1;
                    }
                    else if (premium == TAG_PREMIUM.NORMAL)
                    {
                        num = !entityDef.IsElite() ? 2 : 1;
                    }
                    else
                    {
                        num = 1;
                    }
                    item.NextReward.Reward = new CardRewardData(cardId, premium, num);
                }
                else if (info.NextReward.HasRewardDust)
                {
                    item.NextReward.Reward = new ArcaneDustRewardData(info.NextReward.RewardDust.Amount);
                }
                else if (info.NextReward.HasRewardGold)
                {
                    item.NextReward.Reward = new GoldRewardData((long) info.NextReward.RewardGold.Amount);
                }
                else if (info.NextReward.HasRewardMount)
                {
                    item.NextReward.Reward = new MountRewardData((MountRewardData.MountType) info.NextReward.RewardMount.MountId);
                }
                else if (info.NextReward.HasRewardForge)
                {
                    item.NextReward.Reward = new ForgeTicketRewardData(info.NextReward.RewardForge.Quantity);
                }
                else
                {
                    Debug.LogWarning(string.Format("ConnectAPI.GetAllHeroXP(): next reward for hero {0} is at level {1} but has no recognized reward type in packet", item.Class, item.NextReward.Level));
                }
            }
        }
        return levels2;
    }

    public static long GetArcaneDustBalance()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return 0L;
        }
        ArcaneDustBalance balance = Unpack<ArcaneDustBalance>(NextUtil(), 0x106);
        if (balance == null)
        {
            return 0L;
        }
        return balance.Balance;
    }

    public static PegasusUtil.GetAssetResponse GetAssetResponse()
    {
        return Unpack<PegasusUtil.GetAssetResponse>(NextUtil(), 0x142);
    }

    public static int GetAssetsVersion()
    {
        AssetsVersionResponse response = Unpack<AssetsVersionResponse>(NextUtil(), 0x130);
        if (response == null)
        {
            return 0;
        }
        return response.Version;
    }

    private static void GetBattleNetPackets()
    {
        PegasusPacket packet;
        while ((packet = BattleNet.NextUtilPacket()) != null)
        {
            DecodeAndProcessPacket(packet);
        }
    }

    public static Network.BattlePayConfig GetBattlePayConfigResponse()
    {
        BattlePayConfigResponse response = Unpack<BattlePayConfigResponse>(NextUtil(), 0xee);
        if (response == null)
        {
            return null;
        }
        Network.BattlePayConfig config = new Network.BattlePayConfig {
            Available = !response.HasUnavailable || !response.Unavailable,
            Currency = !response.HasCurrency ? Currency.UNKNOWN : ((Currency) response.Currency),
            SecondsBeforeAutoCancel = !response.HasSecsBeforeAutoCancel ? StoreManager.DEFAULT_SECONDS_BEFORE_AUTO_CANCEL : response.SecsBeforeAutoCancel
        };
        foreach (PegasusUtil.Bundle bundle in response.Bundles)
        {
            Network.Bundle bundle2 = new Network.Bundle {
                ProductID = bundle.Id,
                Cost = bundle.Cost,
                GoldCost = !bundle.HasGoldCost ? 0L : bundle.GoldCost,
                AppleID = !bundle.HasAppleId ? string.Empty : bundle.AppleId,
                GooglePlayID = !bundle.HasGooglePlayId ? string.Empty : bundle.GooglePlayId,
                AmazonID = !bundle.HasAmazonId ? string.Empty : bundle.AmazonId
            };
            if (bundle.HasProductEventName)
            {
                SpecialEventType iGNORE = SpecialEventType.IGNORE;
                if (EnumUtils.TryGetEnum<SpecialEventType>(bundle.ProductEventName, out iGNORE))
                {
                    bundle2.ProductEvent = iGNORE;
                }
            }
            foreach (PegasusUtil.BundleItem item in bundle.Items)
            {
                Network.BundleItem item2 = new Network.BundleItem {
                    Product = item.ProductType,
                    ProductData = item.Data,
                    Quantity = item.Quantity
                };
                bundle2.Items.Add(item2);
            }
            config.Bundles.Add(bundle2);
        }
        foreach (PegasusUtil.GoldCostBooster booster in response.GoldCostBoosters)
        {
            Network.GoldCostBooster booster2 = new Network.GoldCostBooster {
                Cost = booster.Cost,
                Id = booster.PackType
            };
            config.GoldCostBoosters.Add(booster2);
        }
        config.GoldCostArena = !response.HasGoldCostArena ? 0L : response.GoldCostArena;
        return config;
    }

    public static Network.BattlePayStatus GetBattlePayStatusResponse()
    {
        BattlePayStatusResponse response = Unpack<BattlePayStatusResponse>(NextUtil(), 0x109);
        if (response == null)
        {
            return null;
        }
        Network.BattlePayStatus status = new Network.BattlePayStatus {
            State = (Network.BattlePayStatus.PurchaseState) response.Status,
            BattlePayAvailable = response.BattlePayAvailable,
            CurrencyType = !response.HasCurrency ? Currency.UNKNOWN : ((Currency) response.Currency)
        };
        if (response.HasTransactionId)
        {
            status.TransactionID = response.TransactionId;
        }
        if (response.HasProductId)
        {
            status.ProductID = response.ProductId;
        }
        if (response.HasPurchaseError)
        {
            status.PurchaseError = ConvertPurchaseError(response.PurchaseError);
        }
        if (response.HasThirdPartyId)
        {
            status.ThirdPartyID = response.ThirdPartyId;
        }
        if (response.HasProvider)
        {
            status.Provider = new BattlePayProvider?(response.Provider);
        }
        return status;
    }

    public static NetCache.NetCacheBoosters GetBoosters()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheBoosters();
        }
        BoosterList list = Unpack<BoosterList>(NextUtil(), 0xe0);
        if (list == null)
        {
            return null;
        }
        NetCache.NetCacheBoosters boosters2 = new NetCache.NetCacheBoosters();
        foreach (BoosterInfo info in list.List)
        {
            NetCache.BoosterStack item = new NetCache.BoosterStack {
                Id = info.Type,
                Count = info.Count
            };
            boosters2.BoosterStacks.Add(item);
        }
        return boosters2;
    }

    public static NetCache.NetCacheBoosterTallies GetBoosterTallies()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheBoosterTallies();
        }
        BoosterTallyList list = Unpack<BoosterTallyList>(NextUtil(), 0x139);
        if (list == null)
        {
            return null;
        }
        NetCache.NetCacheBoosterTallies tallies = new NetCache.NetCacheBoosterTallies();
        foreach (PegasusUtil.BoosterTally tally in list.List)
        {
            NetCache.BoosterTally item = new NetCache.BoosterTally {
                Id = tally.TypeId,
                Source = (Network.BoosterSource) tally.SourceId,
                IsOpen = tally.IsOpen,
                WasBought = tally.IsBought,
                Count = tally.Count
            };
            tallies.BoosterTallies.Add(item);
        }
        return tallies;
    }

    public static Network.CanceledQuest GetCanceledQuest()
    {
        CancelQuestResponse response = Unpack<CancelQuestResponse>(NextUtil(), 0x11a);
        if (response == null)
        {
            return null;
        }
        return new Network.CanceledQuest { AchieveID = response.QuestId, Canceled = response.Success, NextQuestCancelDate = !response.HasNextQuestCancel ? 0L : DateToUTC(response.NextQuestCancel) };
    }

    public static Network.CardBackResponse GetCardBackReponse()
    {
        SetCardBackResponse response = Unpack<SetCardBackResponse>(NextUtil(), 0x124);
        if (response == null)
        {
            return null;
        }
        return new Network.CardBackResponse { Success = response.Success, CardBack = response.CardBack };
    }

    public static NetCache.NetCacheCardBacks GetCardBacks()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheCardBacks();
        }
        CardBacks backs2 = Unpack<CardBacks>(NextUtil(), 0xec);
        if (backs2 == null)
        {
            return null;
        }
        NetCache.NetCacheCardBacks backs3 = new NetCache.NetCacheCardBacks {
            DefaultCardBack = backs2.DefaultCardBack
        };
        foreach (int num in backs2.CardBacks_)
        {
            backs3.CardBacks.Add(num);
        }
        return backs3;
    }

    public static Network.CardSaleResult GetCardSaleResult()
    {
        BoughtSoldCard card = Unpack<BoughtSoldCard>(NextUtil(), 0x102);
        if (card == null)
        {
            return null;
        }
        return new Network.CardSaleResult { AssetID = card.Def.Asset, AssetName = GameUtils.TranslateDbIdToCardId(card.Def.Asset), Premium = !card.Def.HasPremium ? TAG_PREMIUM.NORMAL : ((TAG_PREMIUM) card.Def.Premium), Action = (Network.CardSaleResult.SaleResult) card.Result_, Amount = card.Amount, Count = !card.HasCount ? 1 : card.Count, Nerfed = card.HasNerfed && card.Nerfed, UnitSellPrice = !card.HasUnitSellPrice ? 0 : card.UnitSellPrice, UnitBuyPrice = !card.HasUnitBuyPrice ? 0 : card.UnitBuyPrice };
    }

    public static CardValues GetCardValues()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return null;
        }
        return Unpack<CardValues>(NextUtil(), 260);
    }

    public static CheckAccountLicensesResponse GetCheckAccountLicensesResponse()
    {
        return Unpack<CheckAccountLicensesResponse>(NextUtil(), 330);
    }

    public static CheckGameLicensesResponse GetCheckGameLicensesResponse()
    {
        return Unpack<CheckGameLicensesResponse>(NextUtil(), 0x14b);
    }

    public static NetCache.NetCacheCollection GetCollectionCardStacks()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheCollection();
        }
        Collection collection = Unpack<Collection>(NextUtil(), 0xcf);
        if (collection == null)
        {
            return null;
        }
        NetCache.NetCacheCollection caches2 = new NetCache.NetCacheCollection();
        foreach (PegasusShared.CardStack stack in collection.Stacks)
        {
            NetCache.CardStack item = new NetCache.CardStack {
                Def = { Name = GameUtils.TranslateDbIdToCardId(stack.CardDef.Asset), Premium = stack.CardDef.Premium },
                Date = DateToUTC(stack.LatestInsertDate),
                Count = stack.Count,
                NumSeen = stack.NumSeen
            };
            caches2.Stacks.Add(item);
            caches2.TotalCardsOwned += item.Count;
            if ((item.Def.Premium == TAG_PREMIUM.NORMAL) && (item.Count > 0))
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(item.Def.Name);
                if (entityDef.IsBasicCardUnlock())
                {
                    Map<TAG_CLASS, int> map;
                    TAG_CLASS tag_class;
                    int num = !entityDef.IsElite() ? 2 : 1;
                    int num2 = map[tag_class];
                    (map = caches2.BasicCardsUnlockedPerClass)[tag_class = entityDef.GetClass()] = num2 + Math.Min(num, item.Count);
                }
            }
        }
        return caches2;
    }

    private static Network.HistCreateGame GetCreateGame(PowerHistoryData data)
    {
        PowerHistoryCreateGame createGame = data.CreateGame;
        if (createGame.Players.Count < 2)
        {
            Debug.LogError(string.Format("History Create Game, players={0}", createGame.Players.Count));
            return null;
        }
        return Network.HistCreateGame.CreateFromProto(createGame);
    }

    public static Deadend GetDeadendGame()
    {
        return Unpack<Deadend>(NextGame(), 0xa9);
    }

    public static DeadendUtil GetDeadendUtil()
    {
        return Unpack<DeadendUtil>(NextUtil(), 0xa7);
    }

    public static DebugCommandResponse GetDebugCommandResponse()
    {
        return Unpack<DebugCommandResponse>(NextUtil(), 0x144);
    }

    public static string GetDebugConsoleCommand()
    {
        DebugConsoleCommand command = Unpack<DebugConsoleCommand>(NextDebug(), 0x7b);
        if (command == null)
        {
            return string.Empty;
        }
        return command.Command;
    }

    public static Network.DebugConsoleResponse GetDebugConsoleResponse()
    {
        BobNetProto.DebugConsoleResponse response = Unpack<BobNetProto.DebugConsoleResponse>(NextGame(), 0x7c);
        if (response == null)
        {
            return null;
        }
        return new Network.DebugConsoleResponse { Type = (int) response.ResponseType_, Response = response.Response };
    }

    public static Network.DeckContents GetDeckContents()
    {
        PegasusUtil.DeckContents contents = Unpack<PegasusUtil.DeckContents>(NextUtil(), 0xd7);
        if (contents == null)
        {
            return null;
        }
        Network.DeckContents contents2 = new Network.DeckContents {
            Deck = contents.Deck
        };
        foreach (DeckCardData data in contents.Cards)
        {
            Network.CardUserData item = new Network.CardUserData {
                DbId = data.Def.Asset,
                Count = !data.HasQty ? 1 : data.Qty,
                Premium = !data.Def.HasPremium ? TAG_PREMIUM.NORMAL : ((TAG_PREMIUM) data.Def.Premium)
            };
            contents2.Cards.Add(item);
        }
        return contents2;
    }

    public static void GetDeckContents(long deckID)
    {
        GetDeck body = new GetDeck {
            Deck = deckID
        };
        UtilOutbound(0xd6, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static NetCache.NetCacheDecks GetDeckHeaders()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheDecks();
        }
        DeckList list = Unpack<DeckList>(NextUtil(), 0xca);
        if (list == null)
        {
            return null;
        }
        NetCache.NetCacheDecks decks2 = new NetCache.NetCacheDecks();
        foreach (DeckInfo info in list.Decks)
        {
            NetCache.DeckHeader item = new NetCache.DeckHeader {
                ID = info.Id,
                Name = info.Name,
                Hero = GameUtils.TranslateDbIdToCardId(info.Hero),
                HeroPremium = (TAG_PREMIUM) info.HeroPremium,
                HeroPower = GameUtils.GetHeroPowerCardIdFromHero(info.Hero),
                IsTourneyValid = AreDeckFlagsTourneyValid(info.Validity),
                Type = info.DeckType,
                CardBack = info.CardBack,
                CardBackOverridden = info.CardBackOverride,
                HeroOverridden = info.HeroOverride,
                SeasonId = info.SeasonId
            };
            decks2.Decks.Add(item);
        }
        return decks2;
    }

    public static int GetDeckLimit()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return 0;
        }
        ProfileDeckLimit limit = Unpack<ProfileDeckLimit>(NextUtil(), 0xe7);
        if (limit == null)
        {
            return 0;
        }
        return limit.DeckLimit;
    }

    public static NetCache.NetCacheDisconnectedGame GetDisconnectedGameInfo()
    {
        NetCache.NetCacheDisconnectedGame game = new NetCache.NetCacheDisconnectedGame();
        if (Network.ShouldBeConnectedToAurora())
        {
            Disconnected disconnected = Unpack<Disconnected>(NextUtil(), 0x121);
            if ((disconnected != null) && disconnected.HasAddress)
            {
                game.ServerInfo = new BattleNet.GameServerInfo();
                game.ServerInfo.Address = disconnected.Address;
                game.ServerInfo.GameHandle = disconnected.GameHandle;
                game.ServerInfo.ClientHandle = disconnected.ClientHandle;
                game.ServerInfo.Port = disconnected.Port;
                game.ServerInfo.AuroraPassword = disconnected.AuroraPassword;
                game.ServerInfo.Mission = disconnected.Scenario;
                game.ServerInfo.Version = BattleNet.GetVersion();
                game.ServerInfo.Resumable = true;
            }
        }
        return game;
    }

    public static Network.EntitiesChosen GetEntitiesChosen()
    {
        PegasusGame.EntitiesChosen chosen = Unpack<PegasusGame.EntitiesChosen>(NextGame(), 13);
        if (chosen == null)
        {
            return null;
        }
        return new Network.EntitiesChosen { ID = chosen.ChooseEntities.Id, Entities = CopyIntList(chosen.ChooseEntities.Entities), PlayerId = chosen.PlayerId };
    }

    public static Network.EntityChoices GetEntityChoices()
    {
        PegasusGame.EntityChoices choices = Unpack<PegasusGame.EntityChoices>(NextGame(), 0x11);
        if (choices == null)
        {
            return null;
        }
        return new Network.EntityChoices { ID = choices.Id, ChoiceType = (CHOICE_TYPE) choices.ChoiceType, CountMax = choices.CountMax, CountMin = choices.CountMin, Entities = CopyIntList(choices.Entities), Source = choices.Source, PlayerId = choices.PlayerId };
    }

    public static FavoriteHeroesResponse GetFavoriteHeroesResponse()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new FavoriteHeroesResponse();
        }
        FavoriteHeroesResponse response = Unpack<FavoriteHeroesResponse>(NextUtil(), 0x13e);
        if (response == null)
        {
            return null;
        }
        return response;
    }

    public static NetCache.NetCacheFeatures GetFeatures()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheFeatures();
        }
        GuardianVars vars = Unpack<GuardianVars>(NextUtil(), 0x108);
        if (vars == null)
        {
            return null;
        }
        return new NetCache.NetCacheFeatures { Games = { Tournament = !vars.HasTourney ? true : vars.Tourney, Practice = !vars.HasPractice ? true : vars.Practice, Casual = !vars.HasCasual ? true : vars.Casual, Forge = !vars.HasForge ? true : vars.Forge, Friendly = !vars.HasFriendly ? true : vars.Friendly, TavernBrawl = !vars.HasTavernBrawl ? true : vars.TavernBrawl, ShowUserUI = !vars.HasShowUserUI ? 0 : vars.ShowUserUI }, Collection = { Manager = !vars.HasManager ? true : vars.Manager, Crafting = !vars.HasCrafting ? true : vars.Crafting }, Store = { Store = !vars.HasStore ? true : vars.Store, BattlePay = !vars.HasBattlePay ? true : vars.BattlePay, BuyWithGold = !vars.HasBuyWithGold ? true : vars.BuyWithGold }, Heroes = { Hunter = !vars.HasHunter ? true : vars.Hunter, Mage = !vars.HasMage ? true : vars.Mage, Paladin = !vars.HasPaladin ? true : vars.Paladin, Priest = !vars.HasPriest ? true : vars.Priest, Rogue = !vars.HasRogue ? true : vars.Rogue, Shaman = !vars.HasShaman ? true : vars.Shaman, Warlock = !vars.HasWarlock ? true : vars.Warlock, Warrior = !vars.HasWarrior ? true : vars.Warrior } };
    }

    private static Network.HistFullEntity GetFullEntity(PowerHistoryData data)
    {
        return new Network.HistFullEntity { Entity = Network.Entity.CreateFromProto(data.FullEntity) };
    }

    public static Network.GameCancelInfo GetGameCancelInfo()
    {
        GameCanceled canceled = Unpack<GameCanceled>(NextGame(), 12);
        if (canceled == null)
        {
            return null;
        }
        return new Network.GameCancelInfo { CancelReason = (Network.GameCancelInfo.Reason) canceled.Reason_ };
    }

    public static Queue<PegasusPacket> GetGamePackets()
    {
        return s_gamePackets;
    }

    public static ClientConnection<PegasusPacket> GetGameServerConnection()
    {
        return s_gameServer;
    }

    public static Network.GameSetup GetGameSetup()
    {
        PegasusGame.GameSetup setup = Unpack<PegasusGame.GameSetup>(NextGame(), 0x10);
        if (setup == null)
        {
            return null;
        }
        Network.GameSetup setup2 = new Network.GameSetup {
            Board = setup.Board,
            MaxSecretsPerPlayer = setup.MaxSecretsPerPlayer,
            MaxFriendlyMinionsPerPlayer = setup.MaxFriendlyMinionsPerPlayer
        };
        if (setup.HasKeepAliveFrequency)
        {
            s_gameServerKeepAliveFrequency = setup.KeepAliveFrequency;
            return setup2;
        }
        s_gameServerKeepAliveFrequency = 0;
        return setup2;
    }

    public static NetCache.NetCacheGamesPlayed GetGamesInfo()
    {
        GamesInfo info = Unpack<GamesInfo>(NextUtil(), 0xd0);
        if (info == null)
        {
            return null;
        }
        return new NetCache.NetCacheGamesPlayed { GamesStarted = info.GamesStarted, GamesWon = info.GamesWon, GamesLost = info.GamesLost, FreeRewardProgress = info.FreeRewardProgress };
    }

    public static void GetGameState()
    {
        PegasusGame.GetGameState body = new PegasusGame.GetGameState();
        QueueGamePacket(1, body);
    }

    public static Network.GenericResponse GetGenericResponse()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new Network.GenericResponse { RequestId = 0, RequestSubId = 0, ResultCode = Network.GenericResponse.Result.OK };
        }
        PegasusUtil.GenericResponse response2 = Unpack<PegasusUtil.GenericResponse>(NextUtil(), 0x146);
        if (response2 == null)
        {
            return null;
        }
        return new Network.GenericResponse { ResultCode = (Network.GenericResponse.Result) response2.ResultCode, RequestId = response2.RequestId, RequestSubId = !response2.HasRequestSubId ? 0 : response2.RequestSubId, GenericData = response2.GenericData };
    }

    public static NetCache.NetCacheGoldBalance GetGoldBalance()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheGoldBalance();
        }
        GoldBalance balance2 = Unpack<GoldBalance>(NextUtil(), 0x116);
        if (balance2 == null)
        {
            return null;
        }
        return new NetCache.NetCacheGoldBalance { CappedBalance = balance2.CappedBalance, BonusBalance = balance2.BonusBalance, Cap = balance2.Cap, CapWarning = balance2.CapWarning };
    }

    private static Network.HistHideEntity GetHideEntity(PowerHistoryData data)
    {
        Network.HistHideEntity entity = new Network.HistHideEntity();
        PowerHistoryHide hideEntity = data.HideEntity;
        entity.Entity = hideEntity.Entity;
        entity.Zone = hideEntity.Zone;
        return entity;
    }

    public static int GetHistoryType(PowerHistoryData history)
    {
        if (history == null)
        {
            return -2;
        }
        if (history.HasTagChange)
        {
            return 4;
        }
        if (history.HasMetaData)
        {
            return 8;
        }
        if (history.HasPowerStart)
        {
            return 5;
        }
        if (history.HasPowerEnd)
        {
            return 6;
        }
        if (history.HasFullEntity)
        {
            return 1;
        }
        if (history.HasShowEntity)
        {
            return 2;
        }
        if (history.HasHideEntity)
        {
            return 3;
        }
        if (history.HasCreateGame)
        {
            return 7;
        }
        return -3;
    }

    public static Network.MassDisenchantResponse GetMassDisenchantResponse()
    {
        PegasusUtil.MassDisenchantResponse response = Unpack<PegasusUtil.MassDisenchantResponse>(NextUtil(), 0x10d);
        if (response == null)
        {
            return null;
        }
        return new Network.MassDisenchantResponse { Amount = response.Amount };
    }

    public static NetCache.NetCacheMedalHistory GetMedalHistory()
    {
        PegasusUtil.MedalHistory history = Unpack<PegasusUtil.MedalHistory>(NextUtil(), 0xea);
        if (history == null)
        {
            return null;
        }
        NetCache.NetCacheMedalHistory history2 = new NetCache.NetCacheMedalHistory();
        foreach (MedalHistoryInfo info in history.Medals)
        {
            NetCache.MedalHistory item = new NetCache.MedalHistory {
                Season = info.Season,
                Date = DateToUTC(info.When),
                Stars = info.Stars,
                StarLevel = info.StarLevel,
                LegendRank = !info.HasLegendRank ? 0 : info.LegendRank
            };
            history2.Medals.Add(item);
        }
        return history2;
    }

    public static NetCache.NetCacheMedalInfo GetMedalInfo()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheMedalInfo();
        }
        MedalInfo info2 = Unpack<MedalInfo>(NextUtil(), 0xe8);
        if (info2 == null)
        {
            return null;
        }
        NetCache.NetCacheMedalInfo info3 = new NetCache.NetCacheMedalInfo {
            SeasonWins = info2.SeasonWins,
            Stars = info2.Stars,
            Streak = info2.Streak,
            StarLevel = info2.StarLevel,
            BestStarLevel = info2.BestStarLevel,
            CanLoseStars = info2.CanLoseStars,
            CanLoseLevel = info2.CanLoseLevel,
            GainLevelStars = info2.LevelEnd,
            StartStars = info2.LevelStart,
            LegendIndex = !info2.HasLegendRank ? 0 : info2.LegendRank
        };
        Log.Bob.Print(string.Format("legend rank {0}", info3.LegendIndex), new object[0]);
        return info3;
    }

    private static Network.HistMetaData GetMetaData(PowerHistoryData data)
    {
        PowerHistoryMetaData metaData = data.MetaData;
        Network.HistMetaData data3 = new Network.HistMetaData {
            MetaType = !metaData.HasType ? HistoryMeta.Type.TARGET : metaData.Type,
            Data = !metaData.HasData ? 0 : metaData.Data
        };
        foreach (int num in metaData.Info)
        {
            data3.Info.Add(num);
        }
        return data3;
    }

    public static int GetNAckOption()
    {
        NAckOption option = Unpack<NAckOption>(NextGame(), 10);
        if (option == null)
        {
            return 0;
        }
        return option.Id;
    }

    public static NotSoMassiveLoginReply GetNotSoMassiveLoginReply()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NotSoMassiveLoginReply();
        }
        NotSoMassiveLoginReply reply = Unpack<NotSoMassiveLoginReply>(NextUtil(), 300);
        if (reply == null)
        {
            return null;
        }
        return reply;
    }

    public static List<NetCache.BoosterCard> GetOpenedBooster()
    {
        BoosterContent content = Unpack<BoosterContent>(NextUtil(), 0xe2);
        if (content == null)
        {
            return null;
        }
        List<NetCache.BoosterCard> list = new List<NetCache.BoosterCard>();
        foreach (PegasusUtil.BoosterCard card in content.List)
        {
            NetCache.BoosterCard item = new NetCache.BoosterCard {
                Def = { Name = GameUtils.TranslateDbIdToCardId(card.CardDef.Asset), Premium = card.CardDef.Premium },
                Date = DateToUTC(card.InsertDate)
            };
            list.Add(item);
        }
        return list;
    }

    public static Network.Options GetOptions()
    {
        AllOptions options = Unpack<AllOptions>(NextGame(), 14);
        Network.Options options2 = new Network.Options {
            ID = options.Id
        };
        foreach (PegasusGame.Option option in options.Options)
        {
            Network.Options.Option item = new Network.Options.Option {
                Type = (Network.Options.Option.OptionType) option.Type_
            };
            if (option.HasMainOption)
            {
                item.Main.ID = option.MainOption.Id;
                item.Main.Targets = CopyIntList(option.MainOption.Targets);
            }
            foreach (PegasusGame.SubOption option3 in option.SubOptions)
            {
                Network.Options.Option.SubOption option4 = new Network.Options.Option.SubOption {
                    ID = option3.Id,
                    Targets = CopyIntList(option3.Targets)
                };
                item.Subs.Add(option4);
            }
            options2.List.Add(item);
        }
        return options2;
    }

    private static PegasusShared.Platform GetPlatformBuilder()
    {
        PegasusShared.Platform platform = new PegasusShared.Platform {
            Os = (int) PlatformSettings.OS,
            Screen = (int) PlatformSettings.Screen,
            Name = PlatformSettings.DeviceName
        };
        AndroidStore androidStore = ApplicationMgr.GetAndroidStore();
        if (androidStore != AndroidStore.NONE)
        {
            platform.Store = (int) androidStore;
        }
        return platform;
    }

    public static NetCache.NetCachePlayerRecords GetPlayerRecords()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCachePlayerRecords();
        }
        PlayerRecords records2 = Unpack<PlayerRecords>(NextUtil(), 270);
        if (records2 == null)
        {
            return null;
        }
        NetCache.NetCachePlayerRecords records3 = new NetCache.NetCachePlayerRecords();
        foreach (PegasusUtil.PlayerRecord record in records2.Records)
        {
            NetCache.PlayerRecord item = new NetCache.PlayerRecord {
                RecordType = record.Type,
                Data = !record.HasData ? 0 : record.Data,
                Wins = record.Wins,
                Losses = record.Losses,
                Ties = record.Ties
            };
            records3.Records.Add(item);
        }
        return records3;
    }

    public static NetCache.NetCachePlayQueue GetPlayQueue()
    {
        PlayQueue queue = Unpack<PlayQueue>(NextUtil(), 0x11e);
        if (queue == null)
        {
            return null;
        }
        return new NetCache.NetCachePlayQueue { GameType = queue.Queue.GameType };
    }

    private static Network.HistActionEnd GetPowerEnd(PowerHistoryData data)
    {
        return new Network.HistActionEnd();
    }

    public static List<Network.PowerHistory> GetPowerHistory()
    {
        PegasusGame.PowerHistory history = Unpack<PegasusGame.PowerHistory>(NextGame(), 0x13);
        if (history == null)
        {
            return null;
        }
        List<Network.PowerHistory> list = new List<Network.PowerHistory>();
        foreach (PowerHistoryData data in history.List)
        {
            int historyType = GetHistoryType(data);
            Network.PowerHistory item = null;
            switch (historyType)
            {
                case 1:
                    item = GetFullEntity(data);
                    break;

                case 2:
                    item = GetShowEntity(data);
                    break;

                case 3:
                    item = GetHideEntity(data);
                    break;

                case 4:
                    item = GetTagChange(data);
                    break;

                case 5:
                    item = GetPowerStart(data);
                    break;

                case 6:
                    item = GetPowerEnd(data);
                    break;

                case 7:
                    item = GetCreateGame(data);
                    break;

                case 8:
                    item = GetMetaData(data);
                    break;

                default:
                    Debug.LogError("Network.GetPowerHistory unknown type");
                    break;
            }
            if (item != null)
            {
                list.Add(item);
            }
        }
        return list;
    }

    private static Network.HistActionStart GetPowerStart(PowerHistoryData data)
    {
        PowerHistoryStart powerStart = data.PowerStart;
        return new Network.HistActionStart(powerStart.Type) { Index = powerStart.Index, Entity = powerStart.Source, Target = powerStart.Target };
    }

    public static List<NetCache.ProfileNotice> GetProfileNotices()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new List<NetCache.ProfileNotice>();
        }
        List<NetCache.ProfileNotice> list2 = new List<NetCache.ProfileNotice>();
        foreach (PegasusUtil.ProfileNotice notice in Unpack<PegasusUtil.ProfileNotices>(NextUtil(), 0xd4).List)
        {
            NetCache.ProfileNotice item = null;
            if (notice.HasMedal)
            {
                NetCache.ProfileNoticeMedal medal = new NetCache.ProfileNoticeMedal {
                    StarLevel = notice.Medal.StarLevel,
                    LegendRank = !notice.Medal.HasLegendRank ? 0 : notice.Medal.LegendRank,
                    BestStarLevel = !notice.Medal.HasBestStarLevel ? 0 : notice.Medal.BestStarLevel
                };
                if (notice.Medal.HasChest)
                {
                    medal.Chest = ConvertRewardChest(notice.Medal.Chest);
                }
                item = medal;
            }
            else if (notice.HasRewardBooster)
            {
                NetCache.ProfileNoticeRewardBooster booster = new NetCache.ProfileNoticeRewardBooster {
                    Id = notice.RewardBooster.BoosterType,
                    Count = notice.RewardBooster.BoosterCount
                };
                item = booster;
            }
            else if (notice.HasRewardCard)
            {
                NetCache.ProfileNoticeRewardCard card = new NetCache.ProfileNoticeRewardCard {
                    CardID = GameUtils.TranslateDbIdToCardId(notice.RewardCard.Card.Asset),
                    Premium = !notice.RewardCard.Card.HasPremium ? TAG_PREMIUM.NORMAL : ((TAG_PREMIUM) notice.RewardCard.Card.Premium),
                    Quantity = !notice.RewardCard.HasQuantity ? 1 : notice.RewardCard.Quantity
                };
                item = card;
            }
            else if (notice.HasPreconDeck)
            {
                NetCache.ProfileNoticePreconDeck deck = new NetCache.ProfileNoticePreconDeck {
                    DeckID = notice.PreconDeck.Deck,
                    HeroAsset = notice.PreconDeck.Hero
                };
                item = deck;
            }
            else if (notice.HasRewardDust)
            {
                NetCache.ProfileNoticeRewardDust dust = new NetCache.ProfileNoticeRewardDust {
                    Amount = notice.RewardDust.Amount
                };
                item = dust;
            }
            else if (notice.HasRewardMount)
            {
                NetCache.ProfileNoticeRewardMount mount = new NetCache.ProfileNoticeRewardMount {
                    MountID = notice.RewardMount.MountId
                };
                item = mount;
            }
            else if (notice.HasRewardForge)
            {
                NetCache.ProfileNoticeRewardForge forge = new NetCache.ProfileNoticeRewardForge {
                    Quantity = notice.RewardForge.Quantity
                };
                item = forge;
            }
            else if (notice.HasRewardGold)
            {
                NetCache.ProfileNoticeRewardGold gold = new NetCache.ProfileNoticeRewardGold {
                    Amount = notice.RewardGold.Amount
                };
                item = gold;
            }
            else if (notice.HasPurchase)
            {
                NetCache.ProfileNoticePurchase purchase = new NetCache.ProfileNoticePurchase {
                    ProductID = notice.Purchase.ProductId,
                    Data = !notice.Purchase.HasData ? 0L : notice.Purchase.Data,
                    CurrencyType = !notice.Purchase.HasCurrency ? Currency.UNKNOWN : ((Currency) notice.Purchase.Currency)
                };
                item = purchase;
            }
            else if (notice.HasRewardCardBack)
            {
                NetCache.ProfileNoticeRewardCardBack back = new NetCache.ProfileNoticeRewardCardBack {
                    CardBackID = notice.RewardCardBack.CardBack
                };
                item = back;
            }
            else if (notice.HasBonusStars)
            {
                NetCache.ProfileNoticeBonusStars stars = new NetCache.ProfileNoticeBonusStars {
                    StarLevel = notice.BonusStars.StarLevel,
                    Stars = notice.BonusStars.Stars
                };
                item = stars;
            }
            else if (notice.HasDcGameResult)
            {
                NetCache.ProfileNoticeDisconnectedGame game = new NetCache.ProfileNoticeDisconnectedGame();
                if (!notice.DcGameResult.HasGameType)
                {
                    Debug.LogError("ConnectAPI.GetProfileNotices(): Missing GameType");
                    continue;
                }
                if (!notice.DcGameResult.HasMissionId)
                {
                    Debug.LogError("ConnectAPI.GetProfileNotices(): Missing GameType");
                    continue;
                }
                if (!notice.DcGameResult.HasGameResult_)
                {
                    Debug.LogError("ConnectAPI.GetProfileNotices(): Missing GameResult");
                    continue;
                }
                game.GameType = notice.DcGameResult.GameType;
                game.MissionId = notice.DcGameResult.MissionId;
                game.GameResult = notice.DcGameResult.GameResult_;
                if (game.GameResult == ProfileNoticeDisconnectedGameResult.GameResult.GR_WINNER)
                {
                    if (!notice.DcGameResult.HasYourResult || !notice.DcGameResult.HasOpponentResult)
                    {
                        Debug.LogError("ConnectAPI.GetProfileNotices(): Missing PlayerResult");
                        continue;
                    }
                    game.YourResult = notice.DcGameResult.YourResult;
                    game.OpponentResult = notice.DcGameResult.OpponentResult;
                }
                item = game;
            }
            else if (notice.HasAdventureProgress)
            {
                NetCache.ProfileNoticeAdventureProgress progress = new NetCache.ProfileNoticeAdventureProgress {
                    Wing = notice.AdventureProgress.WingId
                };
                switch (((NetCache.ProfileNotice.NoticeOrigin) notice.Origin))
                {
                    case NetCache.ProfileNotice.NoticeOrigin.ADVENTURE_PROGRESS:
                        progress.Progress = new int?(!notice.HasOriginData ? 0 : ((int) notice.OriginData));
                        break;

                    case NetCache.ProfileNotice.NoticeOrigin.ADVENTURE_FLAGS:
                        progress.Flags = new ulong?(!notice.HasOriginData ? ((ulong) 0L) : ((ulong) notice.OriginData));
                        break;
                }
                item = progress;
            }
            else if (notice.HasLevelUp)
            {
                NetCache.ProfileNoticeLevelUp up = new NetCache.ProfileNoticeLevelUp {
                    HeroClass = notice.LevelUp.HeroClass,
                    NewLevel = notice.LevelUp.NewLevel
                };
                item = up;
            }
            else if (notice.HasAccountLicense)
            {
                NetCache.ProfileNoticeAcccountLicense license = new NetCache.ProfileNoticeAcccountLicense {
                    License = notice.AccountLicense.License,
                    CasID = notice.AccountLicense.CasId
                };
                item = license;
            }
            else
            {
                Debug.LogError("ConnectAPI.GetProfileNotices(): Unrecognized profile notice");
            }
            if (item == null)
            {
                Debug.LogError("ConnectAPI.GetProfileNotices(): Unhandled notice type! This notice will be lost!");
            }
            else
            {
                item.NoticeID = notice.Entry;
                item.Origin = (NetCache.ProfileNotice.NoticeOrigin) notice.Origin;
                item.OriginData = !notice.HasOriginData ? 0L : notice.OriginData;
                item.Date = DateToUTC(notice.When);
                list2.Add(item);
            }
        }
        return list2;
    }

    public static NetCache.NetCacheProfileProgress GetProfileProgress()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheProfileProgress { CampaignProgress = Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS) };
        }
        ProfileProgress progress2 = Unpack<ProfileProgress>(NextUtil(), 0xe9);
        if (progress2 == null)
        {
            return null;
        }
        NetCache.NetCacheProfileProgress progress3 = new NetCache.NetCacheProfileProgress {
            CampaignProgress = (TutorialProgress) progress2.Progress,
            BestForgeWins = progress2.BestForge,
            LastForgeDate = !progress2.HasLastForge ? 0L : DateToUTC(progress2.LastForge),
            DisplayBanner = progress2.DisplayBanner
        };
        if (progress2.AdventureOptions.Count > 0)
        {
            progress3.AdventureOptions = new AdventureOption[progress2.AdventureOptions.Count];
            for (int i = 0; i < progress2.AdventureOptions.Count; i++)
            {
                AdventureOptions options = progress2.AdventureOptions[i];
                AdventureOption option = new AdventureOption {
                    AdventureID = options.AdventureId,
                    Options = options.Options
                };
                progress3.AdventureOptions[i] = option;
            }
        }
        return progress3;
    }

    public static Network.PurchaseCanceledResponse GetPurchaseCanceledResponse()
    {
        CancelPurchaseResponse response = Unpack<CancelPurchaseResponse>(NextUtil(), 0x113);
        if (response == null)
        {
            return null;
        }
        Network.PurchaseCanceledResponse response2 = new Network.PurchaseCanceledResponse {
            TransactionID = !response.HasTransactionId ? 0L : response.TransactionId,
            ProductID = !response.HasProductId ? string.Empty : response.ProductId,
            CurrencyType = !response.HasCurrency ? Currency.UNKNOWN : ((Currency) response.Currency)
        };
        switch (response.Result)
        {
            case CancelPurchaseResponse.CancelResult.CR_SUCCESS:
                response2.Result = Network.PurchaseCanceledResponse.CancelResult.SUCCESS;
                return response2;

            case CancelPurchaseResponse.CancelResult.CR_NOT_ALLOWED:
                response2.Result = Network.PurchaseCanceledResponse.CancelResult.NOT_ALLOWED;
                return response2;

            case CancelPurchaseResponse.CancelResult.CR_NOTHING_TO_CANCEL:
                response2.Result = Network.PurchaseCanceledResponse.CancelResult.NOTHING_TO_CANCEL;
                return response2;
        }
        return response2;
    }

    public static Network.PurchaseMethod GetPurchaseMethodResponse()
    {
        PegasusUtil.PurchaseMethod method = Unpack<PegasusUtil.PurchaseMethod>(NextUtil(), 0x110);
        if (method == null)
        {
            return null;
        }
        Network.PurchaseMethod method2 = new Network.PurchaseMethod();
        if (method.HasTransactionId)
        {
            method2.TransactionID = method.TransactionId;
        }
        if (method.HasProductId)
        {
            method2.ProductID = method.ProductId;
        }
        if (method.HasQuantity)
        {
            method2.Quantity = method.Quantity;
        }
        if (method.HasCurrency)
        {
            method2.Currency = (Currency) method.Currency;
        }
        if (method.HasWalletName)
        {
            method2.WalletName = method.WalletName;
        }
        if (method.HasUseEbalance)
        {
            method2.UseEBalance = method.UseEbalance;
        }
        if (method.HasError)
        {
            method2.PurchaseError = ConvertPurchaseError(method.Error);
        }
        method2.IsZeroCostLicense = method.HasIsZeroCostLicense && method.IsZeroCostLicense;
        return method2;
    }

    public static Network.PurchaseResponse GetPurchaseResponse()
    {
        PegasusUtil.PurchaseResponse response = Unpack<PegasusUtil.PurchaseResponse>(NextUtil(), 0x100);
        if (response == null)
        {
            return null;
        }
        Network.PurchaseResponse response2 = new Network.PurchaseResponse {
            PurchaseError = ConvertPurchaseError(response.Error),
            TransactionID = !response.HasTransactionId ? 0L : response.TransactionId,
            ProductID = !response.HasProductId ? string.Empty : response.ProductId,
            ThirdPartyID = !response.HasThirdPartyId ? string.Empty : response.ThirdPartyId,
            CurrencyType = !response.HasCurrency ? Currency.UNKNOWN : ((Currency) response.Currency)
        };
        if (response.HasChallengeId)
        {
            response2.ChallengeID = response.ChallengeId;
        }
        if (response.HasChallengeUrl)
        {
            response2.ChallengeURL = response.ChallengeUrl;
        }
        return response2;
    }

    public static Network.PurchaseViaGoldResponse GetPurchaseWithGoldResponse()
    {
        PurchaseWithGoldResponse response = Unpack<PurchaseWithGoldResponse>(NextUtil(), 280);
        if (response == null)
        {
            return null;
        }
        Network.PurchaseViaGoldResponse response2 = new Network.PurchaseViaGoldResponse {
            Error = (Network.PurchaseViaGoldResponse.ErrorType) response.Result
        };
        if (response.HasGoldUsed)
        {
            response2.GoldUsed = response.GoldUsed;
        }
        return response2;
    }

    public static NetCache.NetCacheRewardProgress GetRewardProgress()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new NetCache.NetCacheRewardProgress();
        }
        RewardProgress progress2 = Unpack<RewardProgress>(NextUtil(), 0x10f);
        if (progress2 == null)
        {
            return null;
        }
        return new NetCache.NetCacheRewardProgress { Season = progress2.SeasonNumber, SeasonEndDate = DateToUTC(progress2.SeasonEnd), WinsPerGold = progress2.WinsPerGold, GoldPerReward = progress2.GoldPerReward, MaxGoldPerDay = progress2.MaxGoldPerDay, PackRewardId = !progress2.HasPackId ? 1 : progress2.PackId, XPSoloLimit = progress2.XpSoloLimit, MaxHeroLevel = progress2.MaxHeroLevel, NextQuestCancelDate = DateToUTC(progress2.NextQuestCancel), SpecialEventTimingMod = progress2.EventTimingMod };
    }

    public static ServerResult GetServerResult()
    {
        return Unpack<ServerResult>(NextGame(), 0x17);
    }

    public static Network.SetFavoriteHeroResponse GetSetFavoriteHeroResponse()
    {
        PegasusUtil.SetFavoriteHeroResponse response = Unpack<PegasusUtil.SetFavoriteHeroResponse>(NextUtil(), 320);
        if (response == null)
        {
            return null;
        }
        Network.SetFavoriteHeroResponse response2 = new Network.SetFavoriteHeroResponse {
            Success = response.Success
        };
        if (response.HasFavoriteHero)
        {
            TAG_PREMIUM tag_premium;
            if (!EnumUtils.TryCast<TAG_CLASS>(response.FavoriteHero.ClassId, out response2.HeroClass))
            {
                Debug.LogWarning(string.Format("ConnectAPI.GetSetFavoriteHeroResponse() invalid class {0}", response.FavoriteHero.ClassId));
            }
            if (!EnumUtils.TryCast<TAG_PREMIUM>(response.FavoriteHero.Hero.Premium, out tag_premium))
            {
                Debug.LogWarning(string.Format("ConnectAPI.GetSetFavoriteHeroResponse() invalid heroPremium {0}", response.FavoriteHero.Hero.Premium));
            }
            NetCache.CardDefinition definition = new NetCache.CardDefinition {
                Name = GameUtils.TranslateDbIdToCardId(response.FavoriteHero.Hero.Asset),
                Premium = tag_premium
            };
            response2.Hero = definition;
        }
        return response2;
    }

    public static SetProgressResponse GetSetProgressResponse()
    {
        return Unpack<SetProgressResponse>(NextUtil(), 0x128);
    }

    private static Network.HistShowEntity GetShowEntity(PowerHistoryData data)
    {
        return new Network.HistShowEntity { Entity = Network.Entity.CreateFromProto(data.ShowEntity) };
    }

    public static SpectatorNotify GetSpectatorNotify()
    {
        return Unpack<SpectatorNotify>(NextGame(), 0x18);
    }

    public static SubscribeResponse GetSubscribeResponse()
    {
        SubscribeResponse response = Unpack<SubscribeResponse>(NextUtil(), 0x13b);
        if (response == null)
        {
            return null;
        }
        return response;
    }

    private static Network.HistTagChange GetTagChange(PowerHistoryData data)
    {
        Network.HistTagChange change = new Network.HistTagChange();
        PowerHistoryTagChange tagChange = data.TagChange;
        change.Entity = tagChange.Entity;
        change.Tag = tagChange.Tag;
        change.Value = tagChange.Value;
        return change;
    }

    public static TavernBrawlInfo GetTavernBrawlInfoPacket()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new TavernBrawlInfo();
        }
        TavernBrawlInfo info = Unpack<TavernBrawlInfo>(NextUtil(), 0x13c);
        if (info == null)
        {
            return null;
        }
        return info;
    }

    public static TavernBrawlPlayerRecord GetTavernBrawlRecordPacket()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new TavernBrawlPlayerRecord();
        }
        TavernBrawlPlayerRecordResponse response = Unpack<TavernBrawlPlayerRecordResponse>(NextUtil(), 0x13d);
        if (response == null)
        {
            return null;
        }
        return response.Record;
    }

    public static void GetThirdPartyPurchaseStatus(string transactionID)
    {
        PegasusUtil.GetThirdPartyPurchaseStatus body = new PegasusUtil.GetThirdPartyPurchaseStatus {
            ThirdPartyId = transactionID
        };
        UtilOutbound(0x126, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static Network.ThirdPartyPurchaseStatusResponse GetThirdPartyPurchaseStatusResponse()
    {
        PegasusUtil.ThirdPartyPurchaseStatusResponse response = Unpack<PegasusUtil.ThirdPartyPurchaseStatusResponse>(NextUtil(), 0x127);
        if (response == null)
        {
            return null;
        }
        return new Network.ThirdPartyPurchaseStatusResponse { ThirdPartyID = response.ThirdPartyId, Status = (Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus) response.Status_ };
    }

    public static Network.TriggeredEvent GetTriggerEventResponse()
    {
        TriggerEventResponse response = Unpack<TriggerEventResponse>(NextUtil(), 0x12b);
        if (response == null)
        {
            return null;
        }
        return new Network.TriggeredEvent { EventID = response.EventId, Success = response.Success };
    }

    public static Network.TurnTimerInfo GetTurnTimerInfo()
    {
        PegasusGame.TurnTimer timer = Unpack<PegasusGame.TurnTimer>(NextGame(), 9);
        if (timer == null)
        {
            return null;
        }
        return new Network.TurnTimerInfo { Seconds = timer.Seconds, Turn = timer.Turn, Show = timer.Show };
    }

    public static UpdateLoginComplete GetUpdateLoginComplete()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return new UpdateLoginComplete();
        }
        return Unpack<UpdateLoginComplete>(NextUtil(), 0x133);
    }

    public static Network.UserUI GetUserUI()
    {
        PegasusGame.UserUI rui = Unpack<PegasusGame.UserUI>(NextGame(), 15);
        if (rui == null)
        {
            return null;
        }
        Network.UserUI rui2 = new Network.UserUI();
        if (rui.HasPlayerId)
        {
            rui2.playerId = new int?(rui.PlayerId);
        }
        if (rui.HasMouseInfo)
        {
            PegasusGame.MouseInfo mouseInfo = rui.MouseInfo;
            rui2.mouseInfo = new Network.UserUI.MouseInfo();
            rui2.mouseInfo.ArrowOriginID = mouseInfo.ArrowOrigin;
            rui2.mouseInfo.HeldCardID = mouseInfo.HeldCard;
            rui2.mouseInfo.OverCardID = mouseInfo.OverCard;
            rui2.mouseInfo.X = mouseInfo.X;
            rui2.mouseInfo.Y = mouseInfo.Y;
            return rui2;
        }
        if (rui.HasEmote)
        {
            rui2.emoteInfo = new Network.UserUI.EmoteInfo();
            rui2.emoteInfo.Emote = rui.Emote;
        }
        return rui2;
    }

    public static Network.ValidatedAchieve GetValidatedAchieve()
    {
        ValidateAchieveResponse response = Unpack<ValidateAchieveResponse>(NextUtil(), 0x11d);
        if (response == null)
        {
            return null;
        }
        return new Network.ValidatedAchieve { AchieveID = response.Achieve };
    }

    public static void GotoGameServer(BattleNet.GameServerInfo info, bool reconnecting)
    {
        if (s_gameStartState != GameStartState.INVALID)
        {
            string message = "GotoGameServer() was called when we're already waiting for a game to start.";
            Error.AddDevFatal(message, new object[0]);
        }
        else
        {
            string address = info.Address;
            int @int = Vars.Key("Application.GameServerPortOverride").GetInt(info.Port);
            Log.Net.Print(string.Concat(new object[] { "ConnectAPI.GotoGameServer -- address= ", address, ":", @int, ", game=", info.GameHandle, ", client=", info.ClientHandle, ", spectateKey=", info.SpectatorPassword }), new object[0]);
            if (address != null)
            {
                s_gameConceded = false;
                s_disconnectRequested = false;
                s_gameServerKeepAliveFrequency = 0;
                s_lastPingSent = 0f;
                s_lastPingReceived = 0f;
                s_pingsSentSinceLastPong = 0;
                if (GameServerDisconnectEvents != null)
                {
                    GameServerDisconnectEvents.Clear();
                }
                s_gameServer.Connect(address, @int);
                if (s_gameServer.Active)
                {
                    SendGameServerHandshake(info);
                    s_gameStartState = !reconnecting ? GameStartState.INITIAL_START : GameStartState.RECONNECTING;
                }
            }
        }
    }

    public static void GuardianTrack(Network.TrackWhat what)
    {
    }

    public static bool HaveDebugPackets()
    {
        return (s_debugPackets.Count > 0);
    }

    public static bool HaveGamePackets()
    {
        return (s_gamePackets.Count > 0);
    }

    public static bool HaveNotificationPackets()
    {
        return (BattleNet.GetNotificationCount() > 0);
    }

    public static bool HaveUtilPackets()
    {
        return s_clientRequestManager.HasPendingDeliveryPackets();
    }

    public static void Heartbeat()
    {
        GetBattleNetPackets();
        int count = s_errorList.Count;
        for (int i = 0; i < count; i++)
        {
            ConnectErrorParams parms = s_errorList[i];
            if (parms == null)
            {
                Debug.LogError("null error! " + s_errorList.Count);
            }
            else if (UnityEngine.Time.realtimeSinceStartup >= (parms.m_creationTime + 0.4f))
            {
                s_errorList.RemoveAt(i);
                i--;
                count = s_errorList.Count;
                Error.AddFatal(parms);
            }
        }
        if (s_gameServer != null)
        {
            s_gameServer.Update();
            UpdatePingPong();
        }
        s_clientRequestManager.Update();
        if (ConnectDebugConsole())
        {
            s_debugConsole.Update();
        }
    }

    public static bool IsConnectedToGameServer()
    {
        return ((s_gameServer != null) && s_gameServer.Active);
    }

    public static void MassDisenchant()
    {
        MassDisenchantRequest body = new MassDisenchantRequest();
        UtilOutbound(0x10c, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static PegasusPacket Next(Queue<PegasusPacket> packets, bool pop)
    {
        if ((packets == null) || (packets.Count == 0))
        {
            return null;
        }
        if (pop)
        {
            return packets.Dequeue();
        }
        return packets.Peek();
    }

    public static PegasusPacket NextDebug()
    {
        return NextDebug(false);
    }

    public static PegasusPacket NextDebug(bool pop)
    {
        return Next(s_debugPackets, pop);
    }

    public static int NextDebugConsoleType()
    {
        return NextType(s_debugPackets);
    }

    public static PegasusPacket NextGame()
    {
        return NextGame(false);
    }

    public static PegasusPacket NextGame(bool pop)
    {
        return Next(s_gamePackets, pop);
    }

    public static int NextGameType()
    {
        return NextType(s_gamePackets);
    }

    private static int NextType(Queue<PegasusPacket> packets)
    {
        if ((packets != null) && (packets.Count != 0))
        {
            return packets.Peek().Type;
        }
        return 0;
    }

    public static PegasusPacket NextUtil()
    {
        return s_clientRequestManager.GetNextClientRequest();
    }

    public static int NextUtilType()
    {
        return s_clientRequestManager.PeekNetClientRequestType();
    }

    private void OnGameServerConnectCallback(BattleNetErrors error)
    {
        Log.GameMgr.Print("Connecting to game server with error code " + error, new object[0]);
        if (error != BattleNetErrors.ERROR_OK)
        {
            GameStartState state = s_gameStartState;
            s_gameStartState = GameStartState.INVALID;
            if (Network.ShouldBeConnectedToAurora())
            {
                if ((Network.Get().GetLastGameServerJoined() == null) || (state != GameStartState.RECONNECTING))
                {
                    Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_NO_GAME_SERVER", 0f);
                    Debug.LogError("Failed to connect to game server with error " + error);
                }
            }
            else
            {
                Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_NO_GAME_SERVER", 0f);
                Debug.LogError("Failed to connect to game server with error " + error);
            }
        }
    }

    private void OnGameServerDisconnectCallback(BattleNetErrors error)
    {
        object[] args = new object[] { (int) error, error.ToString() };
        Log.GameMgr.Print("Disconnected from game server with error {0} {1}", args);
        bool flag = false;
        if (error != BattleNetErrors.ERROR_OK)
        {
            if (s_gameStartState == GameStartState.RECONNECTING)
            {
                flag = true;
            }
            else if (s_gameStartState == GameStartState.INITIAL_START)
            {
                BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
                if (!((lastGameServerJoined != null) && lastGameServerJoined.SpectatorMode))
                {
                    ConnectErrorParams item = new ConnectErrorParams {
                        m_message = GameStrings.Format((error != BattleNetErrors.ERROR_RPC_CONNECTION_TIMED_OUT) ? "GLOBAL_ERROR_NETWORK_DISCONNECT_GAME_SERVER" : "GLOBAL_ERROR_NETWORK_CONNECTION_TIMEOUT", new object[0])
                    };
                    s_errorList.Add(item);
                    Debug.LogError("Disconnected from game server with error " + error);
                    flag = true;
                }
            }
            s_gameStartState = GameStartState.INVALID;
        }
        if (!flag)
        {
            if (GameServerDisconnectEvents == null)
            {
                GameServerDisconnectEvents = new List<BattleNetErrors>();
            }
            GameServerDisconnectEvents.Add(error);
        }
    }

    public static void OnLoginCompleted()
    {
        s_clientRequestManager.NotifyLoginSequenceCompleted();
    }

    public static void OnLoginStarted()
    {
    }

    public static void OnStartupPacketSequenceComplete()
    {
        s_clientRequestManager.NotifyStartupSequenceComplete();
    }

    public static void OpenBooster(int id)
    {
        PegasusUtil.OpenBooster body = new PegasusUtil.OpenBooster {
            BoosterType = id
        };
        UtilOutbound(0xe1, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public void PacketReceived(PegasusPacket p, object state)
    {
        QueuePacketReceived(p, (Queue<PegasusPacket>) state);
    }

    public static void PurchaseViaGold(int quantity, ProductType product, int data)
    {
        PurchaseWithGold body = new PurchaseWithGold {
            Product = product,
            Data = data,
            Quantity = quantity
        };
        UtilOutbound(0x117, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    private static void QueueDebugPacket(int packetId, IProtoBuf body)
    {
        s_debugConsole.QueuePacket(new PegasusPacket(packetId, 0, body));
    }

    public static void QueueGamePacket(int packetId, IProtoBuf body)
    {
        s_gameServer.QueuePacket(new PegasusPacket(packetId, 0, body));
    }

    private static void QueuePacketReceived(PegasusPacket packet, Queue<PegasusPacket> queue)
    {
        PacketDecoder decoder;
        if ((queue == s_gamePackets) && (packet.Type == 0x10))
        {
            s_gameStartState = GameStartState.INVALID;
        }
        if ((queue == s_gamePackets) && (packet.Type == 0x74))
        {
            s_lastPingReceived = UnityEngine.Time.realtimeSinceStartup;
            s_pingsSentSinceLastPong = 0;
        }
        if (s_packetDecoders.TryGetValue(packet.Type, out decoder))
        {
            PegasusPacket item = decoder.HandlePacket(packet);
            if (item != null)
            {
                queue.Enqueue(item);
            }
        }
        else
        {
            Debug.LogError("Could not find a packet decoder for a packet of type " + packet.Type);
        }
    }

    public static void QueueUtilNotificationPegasusPacket(PegasusPacket pegasusPacket)
    {
        DecodeAndProcessPacket(pegasusPacket);
    }

    public static void ReadClientOptions(Map<ServerOption, NetCache.ClientOptionBase> options)
    {
        if (Network.ShouldBeConnectedToAurora())
        {
            ClientOptions options2 = Unpack<ClientOptions>(NextUtil(), 0xf1);
            if (options2 != null)
            {
                if (options2.HasFailed && options2.Failed)
                {
                    Debug.LogError("ReadClientOptions: packet.Failed=true. Unable to retrieve client options from UtilServer.");
                    Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_GENERIC");
                }
                else
                {
                    foreach (PegasusUtil.ClientOption option in options2.Options)
                    {
                        ServerOption index = (ServerOption) option.Index;
                        if (option.HasAsBool)
                        {
                            options[index] = new NetCache.ClientOptionBool(option.AsBool);
                        }
                        else if (option.HasAsInt32)
                        {
                            options[index] = new NetCache.ClientOptionInt(option.AsInt32);
                        }
                        else if (option.HasAsInt64)
                        {
                            options[index] = new NetCache.ClientOptionLong(option.AsInt64);
                        }
                        else if (option.HasAsFloat)
                        {
                            options[index] = new NetCache.ClientOptionFloat(option.AsFloat);
                        }
                        else if (option.HasAsUint64)
                        {
                            options[index] = new NetCache.ClientOptionULong(option.AsUint64);
                        }
                    }
                }
            }
        }
    }

    public static void RegisterThrottledPacketListener(ThrottledPacketListener listener)
    {
        if (!m_throttledPacketListeners.Contains(listener))
        {
            m_throttledPacketListeners.Add(listener);
        }
    }

    public static void RemoveThrottledPacketListener(ThrottledPacketListener listener)
    {
        m_throttledPacketListeners.Remove(listener);
    }

    public static void RenameDeck(long deckID, string name)
    {
        PegasusUtil.RenameDeck body = new PegasusUtil.RenameDeck {
            Deck = deckID,
            Name = name
        };
        UtilOutbound(0xd3, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestAccountLicenses()
    {
        CheckAccountLicenses body = new CheckAccountLicenses();
        UtilOutbound(0x10b, 0, body, ClientRequestManager.RequestPhase.STARTUP, 0);
    }

    public static void RequestAchieves(bool activeOrNewCompleteOnly)
    {
        PegasusUtil.GetAchieves body = new PegasusUtil.GetAchieves();
        if (activeOrNewCompleteOnly)
        {
            body.OnlyActiveOrNewComplete = activeOrNewCompleteOnly;
        }
        UtilOutbound(0xfd, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestAdventureProgress()
    {
        GetAdventureProgress body = new GetAdventureProgress();
        UtilOutbound(0x131, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestAssetsVersion()
    {
        PegasusUtil.GetAssetsVersion body = new PegasusUtil.GetAssetsVersion();
        UtilOutbound(0x12f, 0, body, ClientRequestManager.RequestPhase.STARTUP, 0);
    }

    public static void RequestBattlePayConfig()
    {
        GetBattlePayConfig body = new GetBattlePayConfig();
        UtilOutbound(0xed, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestBattlePayStatus()
    {
        GetBattlePayStatus body = new GetBattlePayStatus();
        UtilOutbound(0xff, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestCancelQuest(int achieveID)
    {
        CancelQuest body = new CancelQuest {
            QuestId = achieveID
        };
        UtilOutbound(0x119, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestGameLicenses()
    {
        CheckGameLicenses body = new CheckGameLicenses();
        UtilOutbound(0x114, 1, body, ClientRequestManager.RequestPhase.STARTUP, 0);
    }

    public static void RequestNetCacheObject(GetAccountInfo.Request request)
    {
        GetAccountInfo body = new GetAccountInfo {
            Request_ = request
        };
        UtilOutbound(0xc9, 0, body, ClientRequestManager.RequestPhase.RUNNING, (int) request);
    }

    public static void RequestNetCacheObjectList(List<GetAccountInfo.Request> rl)
    {
        GenericRequestList body = new GenericRequestList();
        foreach (GetAccountInfo.Request request in rl)
        {
            GenericRequest item = new GenericRequest {
                RequestId = 0xc9,
                RequestSubId = (int) request
            };
            body.Requests.Add(item);
        }
        UtilOutbound(0x147, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void RequestPurchaseMethod(string productID, int quantity, int currency)
    {
        GetPurchaseMethod body = new GetPurchaseMethod {
            ProductId = productID,
            Quantity = quantity,
            Currency = currency,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        };
        UtilOutbound(250, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void ResetSubscription()
    {
        s_clientRequestManager.WillReset();
    }

    public static bool RetryGotoGameServer(BattleNet.GameServerInfo info)
    {
        if (s_gameStartState == GameStartState.INVALID)
        {
            return false;
        }
        SendGameServerHandshake(info);
        return true;
    }

    public static void SellCard(int id, int premium, int count, int unitSellPrice)
    {
        BuySellCard body = new BuySellCard();
        PegasusShared.CardDef def = new PegasusShared.CardDef {
            Asset = id
        };
        if (premium != 0)
        {
            def.Premium = premium;
        }
        body.Def = def;
        body.Buying = false;
        if (count != 1)
        {
            body.Count = count;
        }
        body.UnitSellPrice = unitSellPrice;
        UtilOutbound(0x101, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SendAckCardsSeen()
    {
        if (m_ackCardSeenPacket.CardDefs.Count > 0)
        {
            UtilOutbound(0xdf, 0, m_ackCardSeenPacket, ClientRequestManager.RequestPhase.RUNNING, 0);
            m_ackCardSeenPacket.CardDefs.Clear();
        }
    }

    public static void SendAssetRequest(AssetType assetType, int assetId)
    {
        GetAssetRequest body = new GetAssetRequest();
        PegasusUtil.AssetRequestKey item = new PegasusUtil.AssetRequestKey {
            Type = assetType,
            AssetId = assetId
        };
        body.Requests.Add(item);
        UtilOutbound(0x141, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SendChoices(int id, List<int> picks)
    {
        ChooseEntities body = new ChooseEntities {
            Id = id
        };
        for (int i = 0; i < picks.Count; i++)
        {
            body.Entities.Add(picks[i]);
        }
        QueueGamePacket(3, body);
    }

    public static void SendDebugCommandRequest(DebugCommandRequest packet)
    {
        UtilOutbound(0x142, 0, packet, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SendDebugConsoleResponse(int responseType, string message)
    {
        if ((message != null) && (s_debugConsole != null))
        {
            if (!s_debugConsole.Active)
            {
                Debug.LogWarning("Cannont send console response " + message + "; no debug console is active.");
            }
            else
            {
                BobNetProto.DebugConsoleResponse body = new BobNetProto.DebugConsoleResponse {
                    ResponseType_ = (BobNetProto.DebugConsoleResponse.ResponseType) responseType,
                    Response = message
                };
                QueueDebugPacket(0x7c, body);
            }
        }
    }

    public static void SendDeckData(long deck, List<Network.CardUserData> cards, int newHeroAssetID, TAG_PREMIUM newHeroCardPremium, int newCardBackID)
    {
        DeckSetData body = new DeckSetData {
            Deck = deck
        };
        foreach (Network.CardUserData data2 in cards)
        {
            DeckCardData item = new DeckCardData();
            PegasusShared.CardDef def = new PegasusShared.CardDef {
                Asset = data2.DbId
            };
            if (data2.Premium != TAG_PREMIUM.NORMAL)
            {
                def.Premium = (int) data2.Premium;
            }
            item.Def = def;
            item.Qty = data2.Count;
            body.Cards.Add(item);
        }
        if (SEND_DECK_DATA_NO_HERO_ASSET_CHANGE != newHeroAssetID)
        {
            PegasusShared.CardDef def2 = new PegasusShared.CardDef {
                Asset = newHeroAssetID,
                Premium = (int) newHeroCardPremium
            };
            body.Hero = def2;
        }
        if (SEND_DECK_DATA_NO_CARD_BACK_CHANGE != newCardBackID)
        {
            body.CardBack = newCardBackID;
        }
        UtilOutbound(0xde, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SendEmote(int emote)
    {
        PegasusGame.UserUI body = new PegasusGame.UserUI {
            Emote = emote
        };
        QueueGamePacket(15, body);
    }

    private static void SendGameServerHandshake(BattleNet.GameServerInfo info)
    {
        if (info.SpectatorMode)
        {
            SpectatorHandshake body = new SpectatorHandshake {
                GameHandle = info.GameHandle,
                Password = info.SpectatorPassword,
                Version = BattleNet.GetVersion(),
                Platform = GetPlatformBuilder()
            };
            BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
            BnetId id2 = new BnetId {
                Hi = myGameAccountId.GetHi(),
                Lo = myGameAccountId.GetLo()
            };
            body.GameAccountId = id2;
            QueueGamePacket(0x16, body);
        }
        else
        {
            Handshake handshake2 = new Handshake {
                Password = info.AuroraPassword,
                GameHandle = info.GameHandle,
                ClientHandle = (int) info.ClientHandle,
                Mission = info.Mission,
                Version = info.Version,
                Platform = GetPlatformBuilder()
            };
            QueueGamePacket(0xa8, handshake2);
        }
    }

    public static void SendIndirectConsoleCommand(string command)
    {
        if (command != null)
        {
            if (!s_gameServer.Active)
            {
                Debug.LogWarning("Cannot send an indirect console command '" + command + "' to server; no game server is active.");
            }
            else
            {
                DebugConsoleCommand body = new DebugConsoleCommand {
                    Command = command
                };
                QueueGamePacket(0x7b, body);
            }
        }
    }

    public static void SendOption(int ID, int index, int target, int sub, int pos)
    {
        ChooseOption body = new ChooseOption {
            Id = ID,
            Index = index,
            Target = target,
            SubOption = sub,
            Position = pos
        };
        QueueGamePacket(2, body);
    }

    public static void SendPing()
    {
        PegasusGame.Ping body = new PegasusGame.Ping();
        QueueGamePacket(0x73, body);
    }

    public static void SendRemoveAllSpectators(bool regenerateSpectatorPassword)
    {
        RemoveSpectators body = new RemoveSpectators {
            KickAllSpectators = true
        };
        if (regenerateSpectatorPassword)
        {
            body.RegenerateSpectatorPassword = regenerateSpectatorPassword;
        }
        QueueGamePacket(0x1a, body);
    }

    public static void SendRemoveSpectators(bool regenerateSpectatorPassword, params BnetGameAccountId[] bnetGameAccountIds)
    {
        RemoveSpectators body = new RemoveSpectators();
        foreach (BnetGameAccountId id in bnetGameAccountIds)
        {
            BnetId item = new BnetId {
                Hi = id.GetHi(),
                Lo = id.GetLo()
            };
            body.TargetGameaccountIds.Add(item);
        }
        if (regenerateSpectatorPassword)
        {
            body.RegenerateSpectatorPassword = regenerateSpectatorPassword;
        }
        QueueGamePacket(0x1a, body);
    }

    public static void SendSpectatorInvite(BnetAccountId bnetAccountId, BnetGameAccountId bnetGameAccountId)
    {
        InviteToSpectate body = new InviteToSpectate();
        BnetId id = new BnetId {
            Hi = bnetAccountId.GetHi(),
            Lo = bnetAccountId.GetLo()
        };
        BnetId id2 = new BnetId {
            Hi = bnetGameAccountId.GetHi(),
            Lo = bnetGameAccountId.GetLo()
        };
        body.TargetBnetAccountId = id;
        body.TargetGameAccountId = id2;
        QueueGamePacket(0x19, body);
    }

    public static void SendUserUI(int overCard, int heldCard, int arrowOrigin, int x, int y)
    {
        PegasusGame.UserUI body = new PegasusGame.UserUI();
        PegasusGame.MouseInfo info = new PegasusGame.MouseInfo {
            ArrowOrigin = arrowOrigin,
            OverCard = overCard,
            HeldCard = heldCard,
            X = x,
            Y = y
        };
        body.MouseInfo = info;
        QueueGamePacket(15, body);
    }

    public static void SetAdventureOptions(int id, ulong options)
    {
        PegasusUtil.SetAdventureOptions body = new PegasusUtil.SetAdventureOptions();
        AdventureOptions options3 = new AdventureOptions {
            AdventureId = id,
            Options = options
        };
        body.AdventureOptions = options3;
        UtilOutbound(310, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetClientOptionBool(int key, bool value)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key,
            AsBool = value
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetClientOptionFloat(int key, float value)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key,
            AsFloat = value
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetClientOptionInt(int key, int value)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key,
            AsInt32 = value
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetClientOptionLong(int key, long value)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key,
            AsInt64 = value
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetClientOptionULong(int key, ulong value)
    {
        SetOptions body = new SetOptions();
        PegasusUtil.ClientOption item = new PegasusUtil.ClientOption {
            Index = key,
            AsUint64 = value
        };
        body.Options.Add(item);
        UtilOutbound(0xef, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetDeckCardBack(int cardBack, long deck)
    {
        SetCardBack body = new SetCardBack {
            CardBack = cardBack,
            DeckId = deck
        };
        UtilOutbound(0x123, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetDefaultCardBack(int cardBack)
    {
        SetCardBack body = new SetCardBack {
            CardBack = cardBack
        };
        UtilOutbound(0x123, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetFavoriteHero(TAG_CLASS heroClass, NetCache.CardDefinition hero)
    {
        PegasusUtil.SetFavoriteHero body = new PegasusUtil.SetFavoriteHero {
            FavoriteHero = new FavoriteHero()
        };
        body.FavoriteHero.ClassId = (int) heroClass;
        body.FavoriteHero.Hero = new PegasusShared.CardDef();
        body.FavoriteHero.Hero.Asset = GameUtils.TranslateCardIdToDbId(hero.Name);
        body.FavoriteHero.Hero.Premium = (int) hero.Premium;
        UtilOutbound(0x13f, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static void SetProgress(long value)
    {
        PegasusUtil.SetProgress body = new PegasusUtil.SetProgress {
            Value = value
        };
        UtilOutbound(230, 0, body, ClientRequestManager.RequestPhase.STARTUP, 0);
    }

    public static bool ShouldIgnoreError(BnetErrorInfo errorInfo)
    {
        return s_clientRequestManager.ShouldIgnoreError(errorInfo);
    }

    public static void SimulateUncleanDisconnectFromGameServer()
    {
        if (s_gameServer != null)
        {
            s_gameServer.Disconnect();
        }
    }

    public static void SpectateSecondPlayer(BattleNet.GameServerInfo info)
    {
        info.SpectatorMode = true;
        if (!s_gameServer.Active)
        {
            GotoGameServer(info, false);
        }
        else
        {
            SendGameServerHandshake(info);
        }
    }

    public static void SubmitThirdPartyPurchaseReceipt(long bpayID, string thirdPartyID, string base64receipt, string thirdPartyUserID)
    {
        object[] objArray1 = new object[] { bpayID, "|", thirdPartyID, "|", thirdPartyUserID };
        string message = string.Concat(objArray1);
        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_THIRD_PARTY_PURCHASE_RECEIPT_SUBMITTED, 0, message);
        ThirdPartyReceiptData data = new ThirdPartyReceiptData {
            ThirdPartyId = thirdPartyID,
            Receipt = base64receipt
        };
        if (!string.IsNullOrEmpty(thirdPartyUserID))
        {
            data.ThirdPartyUserId = thirdPartyUserID;
        }
        SubmitThirdPartyReceipt body = new SubmitThirdPartyReceipt {
            TransactionId = bpayID,
            ReceiptData = data
        };
        UtilOutbound(0x125, 1, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static float TimeSinceLastPong()
    {
        if ((IsConnectedToGameServer() && (s_gameServerKeepAliveFrequency > 0)) && ((s_lastPingSent > 0f) && (s_lastPingSent > s_lastPingReceived)))
        {
            return (UnityEngine.Time.realtimeSinceStartup - s_lastPingReceived);
        }
        return 0f;
    }

    public static void TrackClient(int level, int what, string message)
    {
    }

    public static void TriggerLaunchEvent(ulong lastPlayedBnetHi, ulong lastPlayedBnetLo, ulong lastPlayedStartTime, ulong otherPlayerBnetHi, ulong otherPlayerBnetLo, ulong otherPlayerStartTime)
    {
        PegasusUtil.NearbyPlayer player = new PegasusUtil.NearbyPlayer {
            BnetIdHi = lastPlayedBnetHi,
            BnetIdLo = lastPlayedBnetLo,
            SessionStartTime = lastPlayedStartTime
        };
        PegasusUtil.NearbyPlayer player2 = new PegasusUtil.NearbyPlayer {
            BnetIdHi = otherPlayerBnetHi,
            BnetIdLo = otherPlayerBnetLo,
            SessionStartTime = otherPlayerStartTime
        };
        TriggerLaunchDayEvent body = new TriggerLaunchDayEvent {
            LastPlayed = player,
            OtherPlayer = player2
        };
        UtilOutbound(0x12a, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    private static T Unpack<T>(PegasusPacket p)
    {
        if ((p != null) && (p.Body is T))
        {
            return (T) p.Body;
        }
        return default(T);
    }

    private static T Unpack<T>(PegasusPacket p, int packetId)
    {
        if (((p != null) && (p.Type == packetId)) && (p.Body is T))
        {
            return (T) p.Body;
        }
        return default(T);
    }

    private static void UpdatePingPong()
    {
        if (s_gameServerKeepAliveFrequency > 0)
        {
            float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            if (s_gameServer.Active && ((realtimeSinceStartup - s_lastPingSent) > s_gameServerKeepAliveFrequency))
            {
                if (s_lastPingSent <= s_lastPingReceived)
                {
                    s_lastPingReceived = realtimeSinceStartup - 0.001f;
                }
                s_lastPingSent = realtimeSinceStartup;
                SendPing();
                if (((s_reconnectAfterFailedPings != null) && (s_pingsSentSinceLastPong >= 3)) && ((realtimeSinceStartup - s_lastPingReceived) > 10f))
                {
                    DisconnectFromGameServer();
                }
                s_pingsSentSinceLastPong++;
            }
        }
    }

    private static void UtilOutbound(int type, int system, IProtoBuf body, ClientRequestManager.RequestPhase requestPhase = 1, int subID = 0)
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            FakeUtilOutbound(type, system, body, subID);
        }
        else
        {
            ClientRequestManager.ClientRequestConfig clientRequestConfig = null;
            if (system == 1)
            {
                clientRequestConfig = new ClientRequestManager.ClientRequestConfig {
                    ShouldRetryOnError = false,
                    RequestedSystem = 1
                };
            }
            if (s_clientRequestManager.SendClientRequest(type, body, clientRequestConfig, requestPhase, subID))
            {
                if (type == 0xc9)
                {
                    GetAccountInfo info = (GetAccountInfo) body;
                    Network.AddPendingRequestTimeout(type, (int) info.Request_);
                }
                else
                {
                    Network.AddPendingRequestTimeout(type, 0);
                }
            }
        }
    }

    public static void ValidateAchieve(int achieveID)
    {
        PegasusUtil.ValidateAchieve body = new PegasusUtil.ValidateAchieve {
            Achieve = achieveID
        };
        UtilOutbound(0x11c, 0, body, ClientRequestManager.RequestPhase.RUNNING, 0);
    }

    public static bool WasDisconnectRequested()
    {
        return s_disconnectRequested;
    }

    public static bool WasGameConceded()
    {
        return s_gameConceded;
    }

    private static void WillReset()
    {
        s_clientRequestManager.WillReset();
    }

    public static List<BattleNetErrors> GameServerDisconnectEvents
    {
        [CompilerGenerated]
        get
        {
            return <GameServerDisconnectEvents>k__BackingField;
        }
        [CompilerGenerated]
        private set
        {
            <GameServerDisconnectEvents>k__BackingField = value;
        }
    }

    private class ConnectErrorParams : ErrorParams
    {
        public float m_creationTime = UnityEngine.Time.realtimeSinceStartup;
    }

    public class DefaultProtobufPacketDecoder<T> : ConnectAPI.PacketDecoder where T: IProtoBuf, new()
    {
        public override PegasusPacket HandlePacket(PegasusPacket p)
        {
            return ConnectAPI.PacketDecoder.HandleProtobuf<T>(p);
        }
    }

    public class DeprecatedPacketDecoder : ConnectAPI.PacketDecoder
    {
        public override PegasusPacket HandlePacket(PegasusPacket p)
        {
            Debug.LogWarning("Dropping deprecated packet of type: " + p.Type);
            return null;
        }
    }

    private enum GameStartState
    {
        INVALID,
        INITIAL_START,
        RECONNECTING
    }

    public class NoOpPacketDecoder : ConnectAPI.PacketDecoder
    {
        public override PegasusPacket HandlePacket(PegasusPacket p)
        {
            return new PegasusPacket { Type = 0xfe, Context = p.Context };
        }
    }

    private class OutboundUtilPacket
    {
        public IProtoBuf Body;
        public int Context;
        public long ResendTime;
        public int SubID;
        public int System;
        public int Type;

        public OutboundUtilPacket(int type, int system, int subID, int context, IProtoBuf body)
        {
            this.Type = type;
            this.System = system;
            this.SubID = subID;
            this.Context = context;
            this.Body = body;
            this.ResendTime = 0L;
        }
    }

    public abstract class PacketDecoder
    {
        public abstract PegasusPacket HandlePacket(PegasusPacket p);
        public static PegasusPacket HandleProtobuf<T>(PegasusPacket p) where T: IProtoBuf, new()
        {
            byte[] body = (byte[]) p.Body;
            T local2 = default(T);
            T local = (local2 == null) ? Activator.CreateInstance<T>() : default(T);
            local.Deserialize(new MemoryStream(body));
            p.Body = local;
            return p;
        }
    }

    public class PongPacketDecoder : ConnectAPI.PacketDecoder
    {
        public override PegasusPacket HandlePacket(PegasusPacket p)
        {
            return null;
        }
    }

    private class QueuedPacket
    {
        public PegasusPacket packet;
        public Queue<PegasusPacket> targetQueue;

        public QueuedPacket(PegasusPacket p, Queue<PegasusPacket> t)
        {
            this.packet = p;
            this.targetQueue = t;
        }
    }

    private enum ServerType
    {
        GAME_SERVER,
        UTIL_SERVER,
        DEBUG_CONSOLE
    }

    public delegate void ThrottledPacketListener(int packetID, long retryMillis);
}

