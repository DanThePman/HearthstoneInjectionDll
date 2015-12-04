using bnet.protocol;
using bnet.protocol.attribute;
using bnet.protocol.channel;
using bnet.protocol.game_master;
using bnet.protocol.game_utilities;
using bnet.protocol.notification;
using PegasusShared;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class GamesAPI : BattleNetAPI
{
    private ServiceDescriptor m_gameFactorySubscriberService;
    private ServiceDescriptor m_gameMasterService;
    private ServiceDescriptor m_gameMasterSubscriberService;
    private ServiceDescriptor m_gameUtilitiesService;
    private Queue<BattleNet.QueueEvent> m_queueEvents;
    private Queue<PegasusPacket> m_utilPackets;
    private const int NO_AI_DECK = 0;
    private const bool RANK_NA = true;
    private const bool RANKED = false;
    private static readonly Map<Network.BnetRegion, string> RegionToTutorialName;
    private ulong s_gameRequest;
    private const bool UNRANKED = true;
    private static bool warnComplete;

    static GamesAPI()
    {
        Map<Network.BnetRegion, string> map = new Map<Network.BnetRegion, string>();
        map.Add(Network.BnetRegion.REGION_US, "us");
        map.Add(Network.BnetRegion.REGION_EU, "eu");
        map.Add(Network.BnetRegion.REGION_KR, "kr");
        map.Add(Network.BnetRegion.REGION_CN, "cn");
        RegionToTutorialName = map;
        warnComplete = false;
    }

    public GamesAPI(BattleNetCSharp battlenet) : base(battlenet, "Games")
    {
        this.m_utilPackets = new Queue<PegasusPacket>();
        this.m_queueEvents = new Queue<BattleNet.QueueEvent>();
        this.m_gameUtilitiesService = new GameUtilitiesService();
        this.m_gameMasterService = new RPCServices.GameMasterService();
        this.m_gameMasterSubscriberService = new RPCServices.GameMasterSubscriberService();
        this.m_gameFactorySubscriberService = new GameFactorySubscriberService();
    }

    private void AddQueueEvent(BattleNet.QueueEvent.Type queueType, int minSeconds = 0, int maxSeconds = 0, int bnetError = 0, BattleNet.GameServerInfo gsInfo = null)
    {
        BattleNet.QueueEvent item = new BattleNet.QueueEvent(queueType, minSeconds, maxSeconds, bnetError, gsInfo);
        Queue<BattleNet.QueueEvent> queueEvents = this.m_queueEvents;
        lock (queueEvents)
        {
            this.m_queueEvents.Enqueue(item);
        }
    }

    public void CancelFindGame()
    {
        this.CancelFindGame(this.s_gameRequest);
        this.s_gameRequest = 0L;
    }

    private void CancelFindGame(ulong gameRequestId)
    {
        BnetGameType findingBnetGameType = Network.Get().GetFindingBnetGameType();
        if (!this.IsNoAccountTutorialGame(findingBnetGameType))
        {
            CancelGameEntryRequest message = new CancelGameEntryRequest {
                RequestId = gameRequestId
            };
            bnet.protocol.game_master.Player val = new bnet.protocol.game_master.Player();
            Identity identity = new Identity();
            identity.SetGameAccountId(base.m_battleNet.GameAccountId);
            val.SetIdentity(identity);
            message.AddPlayer(val);
            CancelGameContext context = new CancelGameContext(gameRequestId);
            base.m_rpcConnection.QueueRequest(this.m_gameMasterService.Id, 4, message, new RPCContextDelegate(context.CancelGameCallback), 0);
        }
    }

    private void ClientRequestCallback(RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        if (status != BattleNetErrors.ERROR_OK)
        {
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnClientRequest, status, context.Context);
        }
        else
        {
            ClientResponse response = ClientResponse.ParseFrom(context.Payload);
            if (response.AttributeCount >= 2)
            {
                bnet.protocol.attribute.Attribute attribute = response.AttributeList[0];
                bnet.protocol.attribute.Attribute attribute2 = response.AttributeList[1];
                if (!attribute.Value.HasIntValue || !attribute2.Value.HasBlobValue)
                {
                    base.ApiLog.LogError("Malformed Attribute in Util Packet: incorrect values");
                }
                int intValue = (int) attribute.Value.IntValue;
                byte[] blobValue = attribute2.Value.BlobValue;
                PegasusPacket item = new PegasusPacket(intValue, blobValue.Length, blobValue) {
                    Context = context.Context
                };
                this.m_utilPackets.Enqueue(item);
            }
            else
            {
                base.ApiLog.LogError("Malformed Attribute in Util Packet: missing values");
            }
        }
    }

    private ClientRequest CreateClientRequest(int type, int sys, byte[] bs, ulong route)
    {
        ClientRequest request = new ClientRequest();
        byte[] dst = new byte[bs.Length + 2];
        dst[0] = (byte) (type & 0xff);
        dst[1] = (byte) ((type & 0xff00) >> 8);
        Buffer.BlockCopy(bs, 0, dst, 2, bs.Length);
        request.AddAttribute(ProtocolHelper.CreateAttribute("p", dst));
        if (BattleNet.IsVersionInt())
        {
            request.AddAttribute(ProtocolHelper.CreateAttribute("v", (long) ((10 * BattleNet.GetVersionInt()) + sys)));
        }
        else
        {
            request.AddAttribute(ProtocolHelper.CreateAttribute("v", BattleNet.GetVersionString() + ((sys != 0) ? "b" : "c")));
        }
        if (route != 0)
        {
            request.AddAttribute(ProtocolHelper.CreateAttribute("r", route));
        }
        return request;
    }

    public void CreateFriendlyChallengeGame(long myDeck, long hisDeck, EntityId hisGameAccount, int scenario)
    {
        FindGameRequest request = new FindGameRequest();
        bnet.protocol.game_master.Player val = new bnet.protocol.game_master.Player();
        Identity identity = new Identity();
        identity.SetGameAccountId(base.m_battleNet.GameAccountId);
        GameProperties properties = new GameProperties();
        AttributeFilter filter = new AttributeFilter();
        filter.SetOp(AttributeFilter.Types.Operation.MATCH_ALL);
        if (!BattleNet.IsVersionInt() && (BattleNet.GetVersionString() == "PAX"))
        {
            filter.AddAttribute(ProtocolHelper.CreateAttribute("version", BattleNet.GetVersionString() + BattleNet.GetVersionInt().ToString()));
        }
        else if (BattleNet.IsVersionInt())
        {
            filter.AddAttribute(ProtocolHelper.CreateAttribute("version", (long) BattleNet.GetVersionInt()));
        }
        else
        {
            filter.AddAttribute(ProtocolHelper.CreateAttribute("version", BattleNet.GetVersionString()));
        }
        filter.AddAttribute(ProtocolHelper.CreateAttribute("GameType", (long) 1L));
        filter.AddAttribute(ProtocolHelper.CreateAttribute("ScenarioId", (long) scenario));
        properties.SetFilter(filter);
        properties.AddCreationAttributes(ProtocolHelper.CreateAttribute("type", (long) 1L));
        properties.AddCreationAttributes(ProtocolHelper.CreateAttribute("scenario", (long) scenario));
        val.SetIdentity(identity);
        val.AddAttribute(ProtocolHelper.CreateAttribute("type", (long) 1L));
        val.AddAttribute(ProtocolHelper.CreateAttribute("scenario", (long) scenario));
        val.AddAttribute(ProtocolHelper.CreateAttribute("deck", (long) ((int) myDeck)));
        request.AddPlayer(val);
        identity = new Identity();
        val = new bnet.protocol.game_master.Player();
        identity.SetGameAccountId(hisGameAccount);
        val.SetIdentity(identity);
        val.AddAttribute(ProtocolHelper.CreateAttribute("type", (long) 1L));
        val.AddAttribute(ProtocolHelper.CreateAttribute("scenario", (long) scenario));
        val.AddAttribute(ProtocolHelper.CreateAttribute("deck", (long) ((int) hisDeck)));
        request.AddPlayer(val);
        request.SetProperties(properties);
        request.SetAdvancedNotification(true);
        FindGameRequest request2 = request;
        this.PrintFindGameRequest(request2);
        this.IsFindGamePending = true;
        base.m_rpcConnection.QueueRequest(this.m_gameMasterService.Id, 3, request2, new RPCContextDelegate(this.FindGameCallback), 0);
    }

    public void FindGame(byte[] requestGuid, BnetGameType gameType, int scenario, long deckId, long aiDeckId, bool setScenarioIdAttr)
    {
        if (this.s_gameRequest != 0)
        {
            Debug.LogWarning("WARNING: FindGame called with an active game");
            this.CancelFindGame(this.s_gameRequest);
            this.s_gameRequest = 0L;
        }
        if (this.IsNoAccountTutorialGame(gameType))
        {
            this.GoToNoAccountTutorialServer(scenario);
        }
        else
        {
            object[] args = new object[] { gameType, scenario, deckId, aiDeckId, !setScenarioIdAttr ? 0 : 1, (requestGuid != null) ? requestGuid.ToHexString() : "null" };
            base.ApiLog.LogInfo("FindGame type={0} scenario={1} deck={2} aideck={3} setScenId={4} request_guid={5}", args);
            bnet.protocol.game_master.Player val = new bnet.protocol.game_master.Player();
            Identity identity = new Identity();
            identity.SetGameAccountId(base.m_battleNet.GameAccountId);
            val.SetIdentity(identity);
            val.AddAttribute(ProtocolHelper.CreateAttribute("type", (long) gameType));
            val.AddAttribute(ProtocolHelper.CreateAttribute("scenario", (long) scenario));
            val.AddAttribute(ProtocolHelper.CreateAttribute("deck", (long) ((int) deckId)));
            val.AddAttribute(ProtocolHelper.CreateAttribute("aideck", (long) ((int) aiDeckId)));
            val.AddAttribute(ProtocolHelper.CreateAttribute("request_guid", requestGuid));
            GameProperties properties = new GameProperties();
            AttributeFilter filter = new AttributeFilter();
            filter.SetOp(AttributeFilter.Types.Operation.MATCH_ALL);
            if (!BattleNet.IsVersionInt() && (BattleNet.GetVersionString() == "PAX"))
            {
                filter.AddAttribute(ProtocolHelper.CreateAttribute("version", BattleNet.GetVersionString() + BattleNet.GetVersionInt().ToString()));
            }
            else if (BattleNet.IsVersionInt())
            {
                filter.AddAttribute(ProtocolHelper.CreateAttribute("version", (long) BattleNet.GetVersionInt()));
            }
            else
            {
                filter.AddAttribute(ProtocolHelper.CreateAttribute("version", BattleNet.GetVersionString()));
            }
            filter.AddAttribute(ProtocolHelper.CreateAttribute("GameType", (long) gameType));
            if (setScenarioIdAttr)
            {
                filter.AddAttribute(ProtocolHelper.CreateAttribute("ScenarioId", (long) scenario));
            }
            properties.SetFilter(filter);
            properties.AddCreationAttributes(ProtocolHelper.CreateAttribute("type", (long) gameType));
            properties.AddCreationAttributes(ProtocolHelper.CreateAttribute("scenario", (long) scenario));
            FindGameRequest request = new FindGameRequest();
            request.AddPlayer(val);
            request.SetProperties(properties);
            request.SetAdvancedNotification(true);
            FindGameRequest request2 = request;
            this.PrintFindGameRequest(request2);
            this.IsFindGamePending = true;
            base.m_rpcConnection.QueueRequest(this.m_gameMasterService.Id, 3, request2, new RPCContextDelegate(this.FindGameCallback), 0);
        }
    }

    private void FindGameCallback(RPCContext context)
    {
        this.IsFindGamePending = false;
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        base.ApiLog.LogDebug("Find Game Callback, status=" + status);
        if (status != BattleNetErrors.ERROR_OK)
        {
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnFindGame, status, context.Context);
        }
        else
        {
            FindGameResponse response = FindGameResponse.ParseFrom(context.Payload);
            if (response.HasRequestId)
            {
                this.s_gameRequest = response.RequestId;
            }
        }
    }

    public void GameEntryHandler(bnet.protocol.notification.Notification notification)
    {
        base.ApiLog.LogDebug("GAME_ENTRY");
        string host = null;
        int port = 0;
        string stringValue = null;
        int intValue = 0;
        int num3 = 0;
        string str3 = null;
        bool boolValue = false;
        string str4 = null;
        foreach (bnet.protocol.attribute.Attribute attribute in notification.AttributeList)
        {
            if (attribute.Name.Equals("connection_info") && attribute.Value.HasMessageValue)
            {
                ConnectInfo info = ConnectInfo.ParseFrom(attribute.Value.MessageValue);
                host = info.Host;
                port = info.Port;
                if (info.HasToken)
                {
                    str3 = Encoding.UTF8.GetString(info.Token);
                }
                foreach (bnet.protocol.attribute.Attribute attribute2 in info.AttributeList)
                {
                    if (attribute2.Name.Equals("version") && attribute2.Value.HasStringValue)
                    {
                        stringValue = attribute2.Value.StringValue;
                    }
                    else if (attribute2.Name.Equals("game") && attribute2.Value.HasIntValue)
                    {
                        intValue = (int) attribute2.Value.IntValue;
                    }
                    else if (attribute2.Name.Equals("id") && attribute2.Value.HasIntValue)
                    {
                        num3 = (int) attribute2.Value.IntValue;
                    }
                    else if (attribute2.Name.Equals("resumable") && attribute2.Value.HasBoolValue)
                    {
                        boolValue = attribute2.Value.BoolValue;
                    }
                    else if (attribute2.Name.Equals("spectator_password") && attribute2.Value.HasStringValue)
                    {
                        str4 = attribute2.Value.StringValue;
                    }
                }
            }
            else if (attribute.Name.Equals("game_handle") && attribute.Value.HasMessageValue)
            {
                GameHandle handle = GameHandle.ParseFrom(attribute.Value.MessageValue);
                base.m_battleNet.Channel.JoinChannel(handle.GameId, ChannelAPI.ChannelType.GAME_CHANNEL);
            }
            else if (attribute.Name.Equals("sender_id") && attribute.Value.HasMessageValue)
            {
                base.ApiLog.LogDebug("sender_id");
            }
        }
        BattleNet.GameServerInfo gsInfo = new BattleNet.GameServerInfo {
            Address = host,
            Port = port,
            AuroraPassword = str3,
            Version = stringValue,
            GameHandle = intValue,
            ClientHandle = num3,
            Resumable = boolValue,
            SpectatorPassword = str4
        };
        this.AddQueueEvent(BattleNet.QueueEvent.Type.QUEUE_GAME_STARTED, 0, 0, 0, gsInfo);
    }

    public void GameLeft(ChannelAPI.ChannelReferenceObject channelRefObject, RemoveNotification notification)
    {
        object[] objArray1 = new object[] { "GameLeft ChannelID: ", channelRefObject.m_channelData.m_channelId, " notification: ", notification };
        base.ApiLog.LogDebug(string.Concat(objArray1));
        if (this.s_gameRequest != 0)
        {
            this.s_gameRequest = 0L;
        }
    }

    public BattleNet.QueueEvent GetQueueEvent()
    {
        BattleNet.QueueEvent event2 = null;
        Queue<BattleNet.QueueEvent> queueEvents = this.m_queueEvents;
        lock (queueEvents)
        {
            if (this.m_queueEvents.Count > 0)
            {
                event2 = this.m_queueEvents.Dequeue();
            }
        }
        return event2;
    }

    private void GoToNoAccountTutorialServer(int scenario)
    {
        BattleNet.GameServerInfo gameServer = new BattleNet.GameServerInfo();
        if (!BattleNet.IsVersionInt() && (BattleNet.GetVersionString() == "PAX"))
        {
            gameServer.Version = BattleNet.GetVersionString() + BattleNet.GetVersionInt().ToString();
        }
        else
        {
            gameServer.Version = BattleNet.GetVersion();
        }
        if (Vars.Key("Loopback.Active").GetBool(false))
        {
            gameServer.Address = Vars.Key("Loopback.Address").GetStr(string.Empty);
            gameServer.Port = Vars.Key("Loopback.Port").GetInt(0);
            gameServer.AuroraPassword = BattleNet.GetVersionString();
        }
        else
        {
            Network.BnetRegion currentRegionId = MobileDeviceLocale.GetCurrentRegionId();
            if (ApplicationMgr.GetMobileEnvironment() == MobileEnv.PRODUCTION)
            {
                string str;
                try
                {
                    str = RegionToTutorialName[currentRegionId];
                }
                catch (KeyNotFoundException)
                {
                    Debug.LogWarning("No matching tutorial server name found for region " + currentRegionId);
                    str = "us";
                }
                gameServer.Address = string.Format("{0}-tutorial{1}.actual.battle.net", str, BattleNet.TutorialServer);
                gameServer.Port = 0x45f;
            }
            else
            {
                gameServer.Address = "10.130.126.28";
                MobileDeviceLocale.ConnectionData connectionDataFromRegionId = MobileDeviceLocale.GetConnectionDataFromRegionId(currentRegionId, true);
                gameServer.Port = connectionDataFromRegionId.tutorialPort;
                gameServer.Version = connectionDataFromRegionId.version;
            }
            object[] args = new object[] { currentRegionId, gameServer.Address, gameServer.Port, gameServer.Version };
            Log.JMac.Print(string.Format("Connecting to account-free tutorial server for region {0}.  Address: {1}  Port: {2}  Version: {3}", args), new object[0]);
            gameServer.AuroraPassword = string.Empty;
        }
        gameServer.GameHandle = 0;
        gameServer.ClientHandle = 0L;
        gameServer.Mission = scenario;
        this.ResolveAddressAndGotoGameServer(gameServer);
    }

    private void HandleGameUtilityServerRequest(RPCContext context)
    {
        base.ApiLog.LogDebug("RPC Called: GameUtilityServer");
    }

    private void HandleNotifyGameFoundRequest(RPCContext context)
    {
        base.ApiLog.LogDebug("RPC Called: NotifyGameFound");
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_gameUtilitiesService.Id, 6, new RPCContextDelegate(this.HandleGameUtilityServerRequest));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_gameFactorySubscriberService.Id, 1, new RPCContextDelegate(this.HandleNotifyGameFoundRequest));
    }

    private bool IsNoAccountTutorialGame(BnetGameType gameType)
    {
        if (Network.ShouldBeConnectedToAurora())
        {
            return false;
        }
        if (gameType != BnetGameType.BGT_TUTORIAL)
        {
            return false;
        }
        return true;
    }

    public void MatchMakerEndHandler(bnet.protocol.notification.Notification notification)
    {
        BattleNetErrors errors = (BattleNetErrors) ((uint) ProtocolHelper.GetUintAttribute(notification.AttributeList, "error", 0L, null));
        ulong num = ProtocolHelper.GetUintAttribute(notification.AttributeList, "game_request_id", 0L, null);
        object[] args = new object[] { num, (uint) errors };
        base.ApiLog.LogDebug("MM_END requestId={0} code={1}", args);
        BattleNet.QueueEvent.Type queueType = BattleNet.QueueEvent.Type.QUEUE_LEAVE;
        switch (errors)
        {
            case BattleNetErrors.ERROR_OK:
                queueType = BattleNet.QueueEvent.Type.QUEUE_LEAVE;
                break;

            case BattleNetErrors.ERROR_GAME_MASTER_GAME_ENTRY_CANCELLED:
                queueType = BattleNet.QueueEvent.Type.QUEUE_CANCEL;
                break;

            case BattleNetErrors.ERROR_GAME_MASTER_GAME_ENTRY_ABORTED_CLIENT_DROPPED:
                queueType = BattleNet.QueueEvent.Type.ABORT_CLIENT_DROPPED;
                break;

            default:
                queueType = BattleNet.QueueEvent.Type.QUEUE_AMM_ERROR;
                break;
        }
        object[] objArray2 = new object[] { queueType, (uint) errors };
        base.ApiLog.LogDebug("MatchMakerEndHandler event={0} code={1}", objArray2);
        this.AddQueueEvent(queueType, 0, 0, (int) errors, null);
    }

    public void MatchMakerStartHandler(bnet.protocol.notification.Notification notification)
    {
        base.ApiLog.LogDebug("MM_START");
        this.AddQueueEvent(BattleNet.QueueEvent.Type.QUEUE_ENTER, 0, 0, 0, null);
    }

    public PegasusPacket NextUtilPacket()
    {
        if (this.m_utilPackets.Count > 0)
        {
            return this.m_utilPackets.Dequeue();
        }
        return null;
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
        this.s_gameRequest = 0L;
        this.m_queueEvents.Clear();
        this.m_utilPackets.Clear();
    }

    private void PrintFindGameRequest(FindGameRequest request)
    {
        string str2;
        string message = "FindGameRequest: { ";
        int playerCount = request.PlayerCount;
        for (int i = 0; i < playerCount; i++)
        {
            bnet.protocol.game_master.Player player = request.Player[i];
            message = message + this.PrintPlayer(player);
        }
        if (request.HasFactoryId)
        {
            str2 = message;
            object[] objArray1 = new object[] { str2, "Factory Id: ", request.FactoryId, " " };
            message = string.Concat(objArray1);
        }
        if (request.HasProperties)
        {
            message = message + this.PrintGameProperties(request.Properties);
        }
        if (request.HasObjectId)
        {
            str2 = message;
            object[] objArray2 = new object[] { str2, "Obj Id: ", request.ObjectId, " " };
            message = string.Concat(objArray2);
        }
        if (request.HasRequestId)
        {
            str2 = message;
            object[] objArray3 = new object[] { str2, "Request Id: ", request.RequestId, " " };
            message = string.Concat(objArray3);
        }
        message = message + "}";
        base.ApiLog.LogDebug(message);
    }

    private string PrintGameMasterAttributeFilter(AttributeFilter filter)
    {
        string str2;
        string str = "Attribute Filter: [";
        switch (filter.Op)
        {
            case AttributeFilter.Types.Operation.MATCH_NONE:
                str2 = "MATCH_NONE";
                break;

            case AttributeFilter.Types.Operation.MATCH_ANY:
                str2 = "MATCH_ANY";
                break;

            case AttributeFilter.Types.Operation.MATCH_ALL:
                str2 = "MATCH_ALL";
                break;

            case AttributeFilter.Types.Operation.MATCH_ALL_MOST_SPECIFIC:
                str2 = "MATCH_ALL_MOST_SPECIFIC";
                break;

            default:
                str2 = "UNKNOWN";
                break;
        }
        str = (str + "Operation: " + str2 + " ") + "Attributes: [";
        int attributeCount = filter.AttributeCount;
        for (int i = 0; i < attributeCount; i++)
        {
            bnet.protocol.attribute.Attribute attribute = filter.Attribute[i];
            string str3 = str;
            object[] objArray1 = new object[] { str3, "Name: ", attribute.Name, " Value: ", attribute.Value, " " };
            str = string.Concat(objArray1);
        }
        return (str + "] ");
    }

    private string PrintGameMasterIdentity(Identity identity)
    {
        string str2;
        string str = string.Empty + "Identity: [";
        if (identity.HasAccountId)
        {
            str2 = str;
            object[] objArray1 = new object[] { str2, "Acct Id: ", identity.AccountId.High, ":", identity.AccountId.Low, " " };
            str = string.Concat(objArray1);
        }
        if (identity.HasGameAccountId)
        {
            str2 = str;
            object[] objArray2 = new object[] { str2, "Game Acct Id: ", identity.GameAccountId.High, ":", identity.GameAccountId.Low, " " };
            str = string.Concat(objArray2);
        }
        return (str + "] ");
    }

    private string PrintGameProperties(GameProperties properties)
    {
        string str2;
        string str = string.Empty;
        str = "Game Properties: [";
        int creationAttributesCount = properties.CreationAttributesCount;
        str = str + "Creation Attributes: ";
        for (int i = 0; i < creationAttributesCount; i++)
        {
            bnet.protocol.attribute.Attribute attribute = properties.CreationAttributes[i];
            str2 = str;
            object[] objArray1 = new object[] { str2, "[Name: ", attribute.Name, " Value: ", attribute.Value, "] " };
            str = string.Concat(objArray1);
        }
        if (properties.HasFilter)
        {
            this.PrintGameMasterAttributeFilter(properties.Filter);
        }
        if (properties.HasCreate)
        {
            str2 = str;
            object[] objArray2 = new object[] { str2, "Create New Game?: ", properties.Create, " " };
            str = string.Concat(objArray2);
        }
        if (properties.HasOpen)
        {
            str2 = str;
            object[] objArray3 = new object[] { str2, "Game Is Open?: ", properties.Open, " " };
            str = string.Concat(objArray3);
        }
        if (properties.HasProgramId)
        {
            str = str + "Program Id(4CC): " + properties.ProgramId;
        }
        return str;
    }

    private string PrintPlayer(bnet.protocol.game_master.Player player)
    {
        string str = string.Empty + "Player: [";
        if (player.HasIdentity)
        {
            this.PrintGameMasterIdentity(player.Identity);
        }
        int attributeCount = player.AttributeCount;
        str = str + "Attributes: ";
        for (int i = 0; i < attributeCount; i++)
        {
            bnet.protocol.attribute.Attribute attribute = player.Attribute[i];
            string str2 = str;
            object[] objArray1 = new object[] { str2, "[Name: ", attribute.Name, " Value: ", attribute.Value, "] " };
            str = string.Concat(objArray1);
        }
        return (str + "] ");
    }

    public void QueueEntryHandler(bnet.protocol.notification.Notification notification)
    {
        base.ApiLog.LogDebug("QueueEntryHandler: " + notification);
        this.QueueUpdate(BattleNet.QueueEvent.Type.QUEUE_DELAY, notification);
    }

    public void QueueExitHandler(bnet.protocol.notification.Notification notification)
    {
        BattleNetErrors errors = (BattleNetErrors) ((uint) ProtocolHelper.GetUintAttribute(notification.AttributeList, "error", 0L, null));
        ulong num = ProtocolHelper.GetUintAttribute(notification.AttributeList, "game_request_id", 0L, null);
        object[] args = new object[] { num, (uint) errors };
        base.ApiLog.LogDebug("QueueExitHandler: requestId={0} code={1}", args);
        if (errors != BattleNetErrors.ERROR_OK)
        {
            BattleNet.QueueEvent.Type queueType = BattleNet.QueueEvent.Type.QUEUE_DELAY_ERROR;
            object[] objArray2 = new object[] { queueType, (uint) errors };
            base.ApiLog.LogDebug("QueueExitHandler event={0} code={1}", objArray2);
            this.AddQueueEvent(queueType, 0, 0, (int) errors, null);
        }
    }

    private void QueueUpdate(BattleNet.QueueEvent.Type queueType, bnet.protocol.notification.Notification notification)
    {
        int minSeconds = 0;
        int maxSeconds = 0;
        foreach (bnet.protocol.attribute.Attribute attribute in notification.AttributeList)
        {
            if ((attribute.Name == "min_wait") && attribute.Value.HasUintValue)
            {
                minSeconds = (int) attribute.Value.UintValue;
            }
            else if ((attribute.Name == "max_wait") && attribute.Value.HasUintValue)
            {
                maxSeconds = (int) attribute.Value.UintValue;
            }
        }
        this.AddQueueEvent(queueType, minSeconds, maxSeconds, 0, null);
    }

    public void QueueUpdateHandler(bnet.protocol.notification.Notification notification)
    {
        base.ApiLog.LogDebug("QueueUpdateHandler: " + notification);
        this.QueueUpdate(BattleNet.QueueEvent.Type.QUEUE_UPDATE, notification);
    }

    private void ResolveAddressAndGotoGameServer(BattleNet.GameServerInfo gameServer)
    {
        IPAddress address;
        if (IPAddress.TryParse(gameServer.Address, out address))
        {
            gameServer.Address = address.ToString();
            Network.Get().GotoGameServer(gameServer, false);
        }
        else
        {
            try
            {
                foreach (IPAddress address2 in Dns.GetHostEntry(gameServer.Address).AddressList)
                {
                    gameServer.Address = address2.ToString();
                    Network.Get().GotoGameServer(gameServer, false);
                    return;
                }
            }
            catch (Exception exception)
            {
                base.m_logSource.LogError("Exception within ResolveAddressAndGotoGameServer: " + exception.Message);
            }
            this.ThrowDnsResolveError(gameServer.Address);
        }
    }

    public void SendUtilPacket(int packetId, int systemId, byte[] bytes, int size, int subID, int context, ulong route)
    {
        ClientRequest message = this.CreateClientRequest(packetId, systemId, bytes, route);
        if (base.m_rpcConnection == null)
        {
            base.ApiLog.LogError("SendUtilPacket could not send, connection not valid : " + message.ToString());
        }
        else
        {
            if (!warnComplete)
            {
                base.ApiLog.LogWarning("SendUtilPacket: need to map context to RPCContext");
                warnComplete = true;
            }
            RPCContext context2 = base.m_rpcConnection.QueueRequest(this.m_gameUtilitiesService.Id, 1, message, new RPCContextDelegate(this.ClientRequestCallback), 0);
            context2.SystemId = systemId;
            context2.Context = context;
            if (context == 0)
            {
                object[] args = new object[] { packetId, systemId };
                Debug.LogFormat("PacketId: {0} systemId={1}", args);
            }
        }
    }

    private void ThrowDnsResolveError(string environment)
    {
        if (ApplicationMgr.IsInternal())
        {
            Error.AddDevFatal("Environment " + environment + " could not be resolved! Please check your environment and Internet connection!", new object[0]);
        }
        else
        {
            Error.AddFatalLoc("GLOBAL_ERROR_NETWORK_NO_CONNECTION", new object[0]);
        }
    }

    public ulong CurrentGameRequest
    {
        get
        {
            return this.s_gameRequest;
        }
        set
        {
            this.s_gameRequest = value;
        }
    }

    public ServiceDescriptor GameFactorySubscribeService
    {
        get
        {
            return this.m_gameFactorySubscriberService;
        }
    }

    public ServiceDescriptor GameMasterService
    {
        get
        {
            return this.m_gameMasterService;
        }
    }

    public ServiceDescriptor GameMasterSubscriberService
    {
        get
        {
            return this.m_gameMasterSubscriberService;
        }
    }

    public ServiceDescriptor GameUtilityService
    {
        get
        {
            return this.m_gameUtilitiesService;
        }
    }

    public bool IsFindGamePending { get; private set; }

    private class CancelGameContext
    {
        private ulong m_gameRequestId;

        public CancelGameContext(ulong gameRequestId)
        {
            this.m_gameRequestId = gameRequestId;
        }

        public void CancelGameCallback(RPCContext context)
        {
            BattleNetCSharp sharp = BattleNet.Get() as BattleNetCSharp;
            if ((sharp != null) && (sharp.Games != null))
            {
                BattleNetErrors status = (BattleNetErrors) context.Header.Status;
                sharp.Games.ApiLog.LogDebug("CancelGameCallback, status=" + status);
                switch (status)
                {
                    case BattleNetErrors.ERROR_OK:
                    case BattleNetErrors.ERROR_GAME_MASTER_INVALID_GAME:
                        if (sharp.Games.IsFindGamePending || ((sharp.Games.CurrentGameRequest != 0) && (sharp.Games.CurrentGameRequest != this.m_gameRequestId)))
                        {
                            object[] args = new object[] { this.m_gameRequestId, sharp.Games.CurrentGameRequest };
                            sharp.Games.ApiLog.LogDebug("CancelGameCallback received for id={0} but is not the current gameRequest={1}, ignoring it.", args);
                        }
                        else
                        {
                            sharp.Games.CurrentGameRequest = 0L;
                            sharp.Games.AddQueueEvent(BattleNet.QueueEvent.Type.QUEUE_CANCEL, 0, 0, 0, null);
                        }
                        break;

                    default:
                        sharp.EnqueueErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnCancelGame, status, context.Context);
                        break;
                }
            }
        }
    }
}

