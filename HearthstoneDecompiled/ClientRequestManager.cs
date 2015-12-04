using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class ClientRequestManager
{
    private readonly ClientRequestConfig m_defaultConfig;
    public uint m_nextContexId;
    public uint m_nextRequestId;
    private InternalState m_state;
    private readonly Subscribe m_subscribePacket;
    private static Map<int, string> s_typeToStringMap = new Map<int, string>();

    public ClientRequestManager()
    {
        ClientRequestConfig config = new ClientRequestConfig {
            ShouldRetryOnError = true,
            RequestedSystem = 0
        };
        this.m_defaultConfig = config;
        this.m_state = new InternalState();
        this.m_subscribePacket = new Subscribe();
    }

    private int CountPendingResponsesForSystemId(SystemChannel system)
    {
        int num = 0;
        foreach (KeyValuePair<uint, ClientRequestType> pair in this.m_state.m_contextIdToRequestMap)
        {
            if (pair.Value.System.SystemId == system.SystemId)
            {
                num++;
            }
        }
        return num;
    }

    public void DropNextClientRequest()
    {
        this.DropNextClientRequestImpl();
    }

    private void DropNextClientRequestImpl()
    {
        if (this.m_state.m_pendingDelivery.Count != 0)
        {
            PegasusPacket packet = this.m_state.m_pendingDelivery.Dequeue();
        }
    }

    private uint GenerateContextId()
    {
        return ++this.m_nextContexId;
    }

    private ClientRequestType GetClientRequest(uint contextId, string reason, bool removeIfFound = true)
    {
        ClientRequestType type;
        if (contextId == 0)
        {
            return null;
        }
        if (!this.m_state.m_contextIdToRequestMap.TryGetValue(contextId, out type))
        {
            if ((this.GetDroppedRequest(contextId, "get_client_request", false) == null) && (this.GetPendingSendRequest(contextId, "get_client_request", false) == null))
            {
            }
            return null;
        }
        if (removeIfFound)
        {
            this.m_state.m_contextIdToRequestMap.Remove(contextId);
        }
        return type;
    }

    private ClientRequestType GetDroppedRequest(uint contextId, string reason, bool removeIfFound = true)
    {
        ClientRequestType type = null;
        if (this.m_state.m_contextIdToDroppedPacketMap.TryGetValue(contextId, out type) && removeIfFound)
        {
            this.m_state.m_contextIdToDroppedPacketMap.Remove(contextId);
        }
        return type;
    }

    public PegasusPacket GetNextClientRequest()
    {
        return this.GetNextClientRequestImpl();
    }

    private PegasusPacket GetNextClientRequestImpl()
    {
        if (this.m_state.m_pendingDelivery.Count == 0)
        {
            return null;
        }
        return this.m_state.m_pendingDelivery.Peek();
    }

    private uint GetNextRequestId()
    {
        return ++this.m_nextRequestId;
    }

    private SystemChannel GetOrCreateSystem(int systemId)
    {
        SystemChannel channel = null;
        if (!this.m_state.m_systems.Systems.TryGetValue(systemId, out channel))
        {
            channel = new SystemChannel {
                SystemId = systemId
            };
            this.m_state.m_systems.Systems[systemId] = channel;
        }
        return channel;
    }

    private ClientRequestType GetPendingSendRequest(uint contextId, string reason, bool removeIfFound = true)
    {
        ClientRequestType type = null;
        foreach (KeyValuePair<int, SystemChannel> pair in this.m_state.m_systems.Systems)
        {
            SystemChannel channel = pair.Value;
            type = this.GetPendingSendRequestForPhase(contextId, removeIfFound, channel.Phases.Running);
            if (type != null)
            {
                return type;
            }
            type = this.GetPendingSendRequestForPhase(contextId, removeIfFound, channel.Phases.StartUp);
        }
        return type;
    }

    private ClientRequestType GetPendingSendRequestForPhase(uint contextId, bool removeIfFound, PendingMapType pendingMap)
    {
        ClientRequestType type = null;
        Queue<ClientRequestType> queue = new Queue<ClientRequestType>();
        foreach (ClientRequestType type2 in pendingMap.PendingSend)
        {
            if ((type == null) && (type2.Context == contextId))
            {
                type = type2;
                if (!removeIfFound)
                {
                    queue.Enqueue(type2);
                }
            }
            else
            {
                queue.Enqueue(type2);
            }
        }
        pendingMap.PendingSend = queue;
        return type;
    }

    private string GetTypeName(int type)
    {
        string str2;
        string str = type.ToString();
        if ((s_typeToStringMap.Count > 0) && s_typeToStringMap.TryGetValue(type, out str2))
        {
            return str2;
        }
        return str;
    }

    public bool HasPendingDeliveryPackets()
    {
        return this.HasPendingDeliveryPacketsImpl();
    }

    private bool HasPendingDeliveryPacketsImpl()
    {
        return (this.m_state.m_pendingDelivery.Count > 0);
    }

    [Conditional("CLIENTREQUESTMANAGER_LOGGING")]
    private void LOG_DEBUG(string format, params object[] args)
    {
        Log.ClientRequestManager.Print("D " + GeneralUtils.SafeFormat(format, args), new object[0]);
    }

    [Conditional("CLIENTREQUESTMANAGER_LOGGING")]
    private void LOG_ERROR(string format, params object[] args)
    {
        Log.ClientRequestManager.Print("E " + GeneralUtils.SafeFormat(format, args), new object[0]);
    }

    [Conditional("CLIENTREQUESTMANAGER_LOGGING")]
    private void LOG_WARN(string format, params object[] args)
    {
        Log.ClientRequestManager.Print("W " + GeneralUtils.SafeFormat(format, args), new object[0]);
    }

    private void MoveRequestsFromPendingResponseToSend(SystemChannel system, RequestPhase phase, PendingMapType pendingMap)
    {
        List<uint> list = new List<uint>();
        List<uint> list2 = new List<uint>();
        foreach (KeyValuePair<uint, ClientRequestType> pair in this.m_state.m_contextIdToRequestMap)
        {
            uint key = pair.Key;
            ClientRequestType type = pair.Value;
            if ((type.System.SystemId == system.SystemId) && (phase == type.Phase))
            {
                if (type.ShouldRetryOnError)
                {
                    list.Add(key);
                }
                else
                {
                    list2.Add(key);
                }
            }
        }
        foreach (uint num2 in list)
        {
            string reason = (phase != RequestPhase.STARTUP) ? "moving_to_pending_running_phase" : "moving_to_pending_startup_phase";
            ClientRequestType item = this.GetClientRequest(num2, reason, true);
            pendingMap.PendingSend.Enqueue(item);
            this.m_state.m_contextIdToDroppedPacketMap.Add(num2, item);
        }
        foreach (uint num3 in list2)
        {
            ClientRequestType type3 = this.GetClientRequest(num3, "moving_to_dropped", true);
            this.m_state.m_contextIdToDroppedPacketMap.Add(num3, type3);
        }
    }

    public void NotifyLoginSequenceCompleted()
    {
        this.NotifyLoginSequenceCompletedImpl();
    }

    private void NotifyLoginSequenceCompletedImpl()
    {
        this.m_state.m_loginCompleteNotificationReceived = true;
    }

    public void NotifyResponseReceived(PegasusPacket packet)
    {
        this.NotifyResponseReceivedImpl(packet);
    }

    private void NotifyResponseReceivedImpl(PegasusPacket packet)
    {
        uint context = (uint) packet.Context;
        ClientRequestType request = this.GetClientRequest(context, "received_response", true);
        if (request == null)
        {
            if ((packet.Context == 0) || (this.GetDroppedRequest(context, "received_response", true) == null))
            {
                this.m_state.m_pendingDelivery.Enqueue(packet);
            }
        }
        else
        {
            switch (packet.Type)
            {
                case 0x13b:
                    this.ProcessSubscribeResponse(packet, request);
                    break;

                case 0x148:
                    this.ProcessClientRequestResponse(packet, request);
                    break;

                default:
                    this.ProcessResponse(packet, request);
                    break;
            }
        }
    }

    public void NotifyStartupSequenceComplete()
    {
        this.NotifyStartupSequenceCompleteImpl();
    }

    private void NotifyStartupSequenceCompleteImpl()
    {
        this.m_state.m_runningPhaseEnabled = true;
    }

    public int PeekNetClientRequestType()
    {
        return this.PeekNetClientRequestTypeImpl();
    }

    private int PeekNetClientRequestTypeImpl()
    {
        if (this.m_state.m_pendingDelivery.Count == 0)
        {
            return 0;
        }
        return this.m_state.m_pendingDelivery.Peek().Type;
    }

    [Conditional("CLIENTREQUESTMANAGER_LOGGING")]
    private void PopulateStringMap()
    {
        s_typeToStringMap.Add(0xc9, "GetAccountInfo");
        s_typeToStringMap.Add(0xca, "DeckList");
        s_typeToStringMap.Add(0xcb, "UtilHandshake");
        s_typeToStringMap.Add(0xcc, "UtilAuth");
        s_typeToStringMap.Add(0xcd, "UpdateLogin");
        s_typeToStringMap.Add(0xce, "DebugAuth");
        s_typeToStringMap.Add(0xcf, "Collection");
        s_typeToStringMap.Add(0xd0, "GamesInfo");
        s_typeToStringMap.Add(0xd1, "CreateDeck");
        s_typeToStringMap.Add(210, "DeleteDeck");
        s_typeToStringMap.Add(0xd3, "RenameDeck");
        s_typeToStringMap.Add(0xd4, "ProfileNotices");
        s_typeToStringMap.Add(0xd5, "AckNotice");
        s_typeToStringMap.Add(0xd6, "GetDeck");
        s_typeToStringMap.Add(0xd7, "DeckContents");
        s_typeToStringMap.Add(0xd8, "DBAction");
        s_typeToStringMap.Add(0xd9, "DeckCreated");
        s_typeToStringMap.Add(0xda, "DeckDeleted");
        s_typeToStringMap.Add(0xdb, "DeckRenamed");
        s_typeToStringMap.Add(220, "DeckGainedCard");
        s_typeToStringMap.Add(0xdd, "DeckLostCard");
        s_typeToStringMap.Add(0xde, "DeckSetData");
        s_typeToStringMap.Add(0xdf, "AckCardSeen");
        s_typeToStringMap.Add(0xe0, "BoosterList");
        s_typeToStringMap.Add(0xe1, "OpenBooster");
        s_typeToStringMap.Add(0xe2, "BoosterContent");
        s_typeToStringMap.Add(0xe3, "ProfileLastLogin");
        s_typeToStringMap.Add(0xe4, "ClientTracking");
        s_typeToStringMap.Add(0xe5, "unused");
        s_typeToStringMap.Add(230, "SetProgress");
        s_typeToStringMap.Add(0xe7, "ProfileDeckLimit");
        s_typeToStringMap.Add(0xe8, "MedalInfo");
        s_typeToStringMap.Add(0xe9, "ProfileProgress");
        s_typeToStringMap.Add(0xea, "MedalHistory");
        s_typeToStringMap.Add(0xeb, "DraftBegin");
        s_typeToStringMap.Add(0xec, "CardBacks");
        s_typeToStringMap.Add(0xed, "GetBattlePayConfig");
        s_typeToStringMap.Add(0xee, "BattlePayConfigResponse");
        s_typeToStringMap.Add(0xef, "SetOptions");
        s_typeToStringMap.Add(240, "GetOptions");
        s_typeToStringMap.Add(0xf1, "ClientOptions");
        s_typeToStringMap.Add(0xf2, "DraftRetire");
        s_typeToStringMap.Add(0xf3, "AckAchieveProgress");
        s_typeToStringMap.Add(0xf4, "DraftGetPicksAndContents");
        s_typeToStringMap.Add(0xf5, "DraftMakePick");
        s_typeToStringMap.Add(0xf6, "DraftBeginning");
        s_typeToStringMap.Add(0xf7, "DraftRetired");
        s_typeToStringMap.Add(0xf8, "DraftChoicesAndContents");
        s_typeToStringMap.Add(0xf9, "DraftChosen");
        s_typeToStringMap.Add(250, "GetPurchaseMethod");
        s_typeToStringMap.Add(0xfb, "DraftError");
        s_typeToStringMap.Add(0xfc, "Achieves");
        s_typeToStringMap.Add(0xfd, "GetAchieves");
        s_typeToStringMap.Add(0xfe, "NOP");
        s_typeToStringMap.Add(0xff, "GetBattlePayStatus");
        s_typeToStringMap.Add(0x100, "PurchaseResponse");
        s_typeToStringMap.Add(0x101, "BuySellCard");
        s_typeToStringMap.Add(0x102, "BoughtSoldCard");
        s_typeToStringMap.Add(0x103, "DevBnetIdentify");
        s_typeToStringMap.Add(260, "CardValues");
        s_typeToStringMap.Add(0x105, "GuardianTrack");
        s_typeToStringMap.Add(0x106, "ArcaneDustBalance");
        s_typeToStringMap.Add(0x107, "CloseCardMarket");
        s_typeToStringMap.Add(0x108, "GuardianVars");
        s_typeToStringMap.Add(0x109, "BattlePayStatusResponse");
        s_typeToStringMap.Add(0x10a, "Error37 (deprecated)");
        s_typeToStringMap.Add(0x10b, "CheckAccountLicenses");
        s_typeToStringMap.Add(0x10c, "MassDisenchant");
        s_typeToStringMap.Add(0x10d, "MassDisenchantResponse");
        s_typeToStringMap.Add(270, "PlayerRecords");
        s_typeToStringMap.Add(0x10f, "RewardProgress");
        s_typeToStringMap.Add(0x110, "PurchaseMethod");
        s_typeToStringMap.Add(0x111, "DoPurchase");
        s_typeToStringMap.Add(0x112, "CancelPurchase");
        s_typeToStringMap.Add(0x113, "CancelPurchaseResponse");
        s_typeToStringMap.Add(0x114, "CheckGameLicenses");
        s_typeToStringMap.Add(0x115, "CheckLicensesResponse");
        s_typeToStringMap.Add(0x116, "GoldBalance");
        s_typeToStringMap.Add(0x117, "PurchaseWithGold");
        s_typeToStringMap.Add(280, "PurchaseWithGoldResponse");
        s_typeToStringMap.Add(0x119, "CancelQuest");
        s_typeToStringMap.Add(0x11a, "CancelQuestResponse");
        s_typeToStringMap.Add(0x11b, "HeroXP");
        s_typeToStringMap.Add(0x11c, "ValidateAchieve");
        s_typeToStringMap.Add(0x11d, "ValidateAchieveResponse");
        s_typeToStringMap.Add(0x11e, "PlayQueue");
        s_typeToStringMap.Add(0x11f, "DraftAckRewards");
        s_typeToStringMap.Add(0x120, "DraftRewardsAcked");
        s_typeToStringMap.Add(0x121, "Disconnected");
        s_typeToStringMap.Add(290, "Deadend");
        s_typeToStringMap.Add(0x123, "SetCardBack");
        s_typeToStringMap.Add(0x124, "SetCardBackResponse");
        s_typeToStringMap.Add(0x125, "SubmitThirdPartyReceipt");
        s_typeToStringMap.Add(0x126, "GetThirdPartyPurchaseStatus");
        s_typeToStringMap.Add(0x127, "ThirdPartyPurchaseStatusResponse");
        s_typeToStringMap.Add(0x128, "SetProgressResponse");
        s_typeToStringMap.Add(0x129, "CheckAccountLicenseAchieve");
        s_typeToStringMap.Add(0x12a, "TriggerLaunchDayEvent");
        s_typeToStringMap.Add(0x12b, "EventResponse");
        s_typeToStringMap.Add(300, "MassiveLoginReply");
        s_typeToStringMap.Add(0x12d, "(used in Console.proto)");
        s_typeToStringMap.Add(0x12e, "(used in Console.proto)");
        s_typeToStringMap.Add(0x12f, "GetAssetsVersion");
        s_typeToStringMap.Add(0x130, "AssetsVersionResponse");
        s_typeToStringMap.Add(0x131, "GetAdventureProgress");
        s_typeToStringMap.Add(0x132, "AdventureProgressResponse");
        s_typeToStringMap.Add(0x133, "UpdateLoginComplete");
        s_typeToStringMap.Add(0x134, "AckWingProgress");
        s_typeToStringMap.Add(0x135, "SetPlayerAdventureProgress");
        s_typeToStringMap.Add(310, "SetAdventureOptions");
        s_typeToStringMap.Add(0x137, "AccountLicenseAchieveResponse");
        s_typeToStringMap.Add(0x138, "StartThirdPartyPurchase");
        s_typeToStringMap.Add(0x139, "BoosterTally");
        s_typeToStringMap.Add(0x13a, "Subscribe");
        s_typeToStringMap.Add(0x13b, "SubscribeResponse");
        s_typeToStringMap.Add(0x13c, "TavernBrawlInfo");
        s_typeToStringMap.Add(0x13d, "TavernBrawlPlayerRecordResponse");
        s_typeToStringMap.Add(0x13e, "FavoriteHeroesResponse");
        s_typeToStringMap.Add(0x13f, "SetFavoriteHero");
        s_typeToStringMap.Add(320, "SetFavoriteHeroResponse");
        s_typeToStringMap.Add(0x141, "GetAssetRequest");
        s_typeToStringMap.Add(0x142, "GetAssetResponse");
        s_typeToStringMap.Add(0x143, "DebugCommandRequest");
        s_typeToStringMap.Add(0x144, "DebugCommandResponse");
        s_typeToStringMap.Add(0x145, "AccountLicensesInfoResponse");
        s_typeToStringMap.Add(0x146, "GenericResponse");
        s_typeToStringMap.Add(0x147, "GenericRequestList");
        s_typeToStringMap.Add(0x148, "ClientRequestResponse");
    }

    private void ProcessClientRequestResponse(PegasusPacket packet, ClientRequestType clientRequest)
    {
        if (packet.Body is ClientRequestResponse)
        {
            ClientRequestResponse body = (ClientRequestResponse) packet.Body;
            if (body.ResponseFlags == ClientRequestResponse.ClientRequestResponseFlags.CRRF_SERVICE_UNAVAILABLE)
            {
                this.ProcessServiceUnavailable(body, clientRequest);
            }
        }
    }

    private void ProcessClientRequests(SystemChannel system)
    {
        PendingMapType pendingMap = (system.CurrentPhase != RequestPhase.STARTUP) ? system.Phases.Running : system.Phases.StartUp;
        foreach (KeyValuePair<uint, ClientRequestType> pair in this.m_state.m_contextIdToRequestMap)
        {
            ClientRequestType type2 = pair.Value;
            if ((!type2.IsSubsribeRequest && (type2.System != null)) && ((type2.System.SystemId == system.SystemId) && (system.PendingResponseTimeout != 0)))
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - type2.SendTime);
                if (span.TotalSeconds >= system.PendingResponseTimeout)
                {
                    this.ScheduleResubscribeWithNewRoute(system);
                    return;
                }
            }
        }
        if (pendingMap.PendingSend.Count > 0)
        {
            ClientRequestType request = pendingMap.PendingSend.Dequeue();
            this.SendToUtil(system, system.Route, request, pendingMap);
        }
        else if ((system.CurrentPhase == RequestPhase.STARTUP) && this.m_state.m_runningPhaseEnabled)
        {
            system.CurrentPhase = RequestPhase.RUNNING;
        }
    }

    private void ProcessResponse(PegasusPacket packet, ClientRequestType clientRequest)
    {
        if (packet.Type != 0xfe)
        {
            this.m_state.m_pendingDelivery.Enqueue(packet);
        }
    }

    private void ProcessServiceUnavailable(ClientRequestResponse response, ClientRequestType clientRequest)
    {
        this.ScheduleResubscribeWithNewRoute(clientRequest.System);
    }

    private void ProcessSubscribeResponse(PegasusPacket packet, ClientRequestType request)
    {
        if (packet.Body is SubscribeResponse)
        {
            SystemChannel system = request.System;
            int systemId = system.SystemId;
            SubscribeResponse body = (SubscribeResponse) packet.Body;
            if (body.Result == SubscribeResponse.ResponseResult.FAILED_UNAVAILABLE)
            {
                this.ScheduleResubscribeWithNewRoute(system);
            }
            else
            {
                system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.SUBSCRIBED;
                system.SubscriptionStatus.SubscribedTime = DateTime.Now;
                system.Route = body.Route;
                system.CurrentPhase = RequestPhase.STARTUP;
                system.SubscribeAttempt = 0;
                system.KeepAliveSecs = body.KeepAliveSecs;
                system.MaxResubscribeAttempts = body.MaxResubscribeAttempts;
                system.PendingResponseTimeout = body.PendingResponseTimeout;
                system.PendingSubscribeTimeout = body.PendingSubscribeTimeout;
                this.m_state.m_pendingDelivery.Enqueue(packet);
                system.m_subscribePacketsReceived++;
            }
        }
    }

    private bool ProcessSubscribeStatePendingResponse(SystemChannel system)
    {
        TimeSpan span = (TimeSpan) (DateTime.Now - system.SubscriptionStatus.LastSend);
        if (span.TotalSeconds > system.PendingSubscribeTimeout)
        {
            this.ScheduleResubscribeKeepRoute(system);
        }
        return (system.Route != 0L);
    }

    private bool ProcessSubscribeStatePendingSend(SystemChannel system)
    {
        TimeSpan span = (TimeSpan) (DateTime.Now - system.SubscriptionStatus.LastSend);
        if (span.TotalSeconds > system.PendingSubscribeTimeout)
        {
            this.SendSubscriptionRequest(system);
        }
        return (system.Route != 0L);
    }

    private bool ProcessSubscribeStateSubscribed(SystemChannel system)
    {
        TimeSpan span = (TimeSpan) (DateTime.Now - system.SubscriptionStatus.SubscribedTime);
        if (((ulong) span.TotalSeconds) >= system.KeepAliveSecs)
        {
            if (this.CountPendingResponsesForSystemId(system) > 0)
            {
                return true;
            }
            if (system.KeepAliveSecs > 0L)
            {
                system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_SEND;
            }
        }
        return true;
    }

    public void ScheduleResubscribe()
    {
        foreach (KeyValuePair<int, SystemChannel> pair in this.m_state.m_systems.Systems)
        {
            this.ScheduleResubscribeWithNewRoute(pair.Value);
        }
    }

    private void ScheduleResubscribeKeepRoute(SystemChannel system)
    {
        system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_SEND;
    }

    private void ScheduleResubscribeWithNewRoute(SystemChannel system)
    {
        system.Route = 0L;
        system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_SEND;
    }

    public bool SendClientRequest(int type, IProtoBuf body, ClientRequestConfig clientRequestConfig, RequestPhase requestPhase = 1, int subID = 0)
    {
        return this.SendClientRequestImpl(type, body, clientRequestConfig, requestPhase, subID);
    }

    private bool SendClientRequestImpl(int type, IProtoBuf body, ClientRequestConfig clientRequestConfig, RequestPhase requestPhase, int subID)
    {
        if (type == 0)
        {
            return false;
        }
        if ((requestPhase < RequestPhase.STARTUP) || (requestPhase > RequestPhase.RUNNING))
        {
            return false;
        }
        ClientRequestConfig config = (clientRequestConfig != null) ? clientRequestConfig : this.m_defaultConfig;
        int requestedSystem = config.RequestedSystem;
        SystemChannel orCreateSystem = this.GetOrCreateSystem(requestedSystem);
        if (requestPhase < orCreateSystem.CurrentPhase)
        {
            return false;
        }
        if (orCreateSystem.WasEverInRunningPhase && (requestPhase < RequestPhase.RUNNING))
        {
            return false;
        }
        if (body == null)
        {
            return false;
        }
        ClientRequestType item = new ClientRequestType {
            Type = type,
            ShouldRetryOnError = config.ShouldRetryOnError,
            SubID = subID,
            Body = ProtobufUtil.ToByteArray(body),
            Phase = requestPhase,
            RetryCount = 0,
            RequestId = this.GetNextRequestId()
        };
        if (item.Phase == RequestPhase.STARTUP)
        {
            orCreateSystem.Phases.StartUp.PendingSend.Enqueue(item);
        }
        else
        {
            orCreateSystem.Phases.Running.PendingSend.Enqueue(item);
        }
        return true;
    }

    private void SendSubscriptionRequest(SystemChannel system)
    {
        int systemId = system.SystemId;
        if (system.Route == 0)
        {
            this.MoveRequestsFromPendingResponseToSend(system, RequestPhase.STARTUP, system.Phases.StartUp);
            this.MoveRequestsFromPendingResponseToSend(system, RequestPhase.RUNNING, system.Phases.Running);
        }
        ClientRequestType request = new ClientRequestType {
            Type = 0x13a,
            SubID = 0,
            Body = ProtobufUtil.ToByteArray(this.m_subscribePacket),
            RequestId = this.GetNextRequestId(),
            IsSubsribeRequest = true
        };
        system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_RESPONSE;
        system.SubscriptionStatus.LastSend = DateTime.Now;
        PendingMapType pendingMap = (system.CurrentPhase != RequestPhase.STARTUP) ? system.Phases.Running : system.Phases.StartUp;
        system.SubscriptionStatus.ContexId = this.SendToUtil(system, system.Route, request, pendingMap);
        system.SubscribeAttempt++;
        this.m_state.m_subscribePacketsSent++;
    }

    private uint SendToUtil(SystemChannel system, ulong route, ClientRequestType request, PendingMapType pendingMap)
    {
        uint key = this.GenerateContextId();
        BattleNet.SendUtilPacket(request.Type, system.SystemId, request.Body, request.Body.Length, request.SubID, (int) key, route);
        request.Context = key;
        request.System = system;
        request.SendTime = DateTime.Now;
        this.m_state.m_contextIdToRequestMap.Add(key, request);
        string str = !request.IsSubsribeRequest ? request.Phase.ToString() : "SUBSCRIBE";
        return key;
    }

    private bool ShouldIgnore_ERROR_GAME_UTILITY_SERVER_NO_SERVER(BnetErrorInfo errorInfo, ClientRequestType clientRequest)
    {
        this.ScheduleResubscribeWithNewRoute(clientRequest.System);
        return true;
    }

    public bool ShouldIgnoreError(BnetErrorInfo errorInfo)
    {
        return this.ShouldIgnoreErrorImpl(errorInfo);
    }

    private bool ShouldIgnoreErrorImpl(BnetErrorInfo errorInfo)
    {
        uint context = (uint) errorInfo.GetContext();
        if (context == 0)
        {
            return false;
        }
        ClientRequestType clientRequest = this.GetClientRequest(context, "should_ignore_error", true);
        if (clientRequest == null)
        {
            return ((this.GetDroppedRequest(context, "should_ignore", true) != null) || (this.GetPendingSendRequest(context, "should_ignore", true) != null));
        }
        BattleNetErrors error = errorInfo.GetError();
        if (clientRequest.IsSubsribeRequest)
        {
            if (clientRequest.System.SubscribeAttempt >= clientRequest.System.MaxResubscribeAttempts)
            {
                return !clientRequest.ShouldRetryOnError;
            }
            return true;
        }
        if (!clientRequest.ShouldRetryOnError)
        {
            return true;
        }
        if (clientRequest.System.PendingResponseTimeout == 0)
        {
            return false;
        }
        switch (error)
        {
            case (((BattleNetErrors) (BattleNetErrors.ERROR_INTERNAL && BattleNetErrors.ERROR_RPC_REQUEST_TIMED_OUT)) && BattleNetErrors.ERROR_GAME_UTILITY_SERVER_NO_SERVER):
                return false;
        }
        return this.ShouldIgnore_ERROR_GAME_UTILITY_SERVER_NO_SERVER(errorInfo, clientRequest);
    }

    public void Terminate()
    {
        this.TerminateImpl();
    }

    private void TerminateImpl()
    {
        Unsubscribe protobuf = new Unsubscribe();
        byte[] bytes = ProtobufUtil.ToByteArray(protobuf);
        foreach (KeyValuePair<int, SystemChannel> pair in this.m_state.m_systems.Systems)
        {
            SystemChannel channel = pair.Value;
            if ((channel.SubscriptionStatus.CurrentState == SubscriptionStatusType.State.SUBSCRIBED) && (channel.Route != 0))
            {
                BattleNet.SendUtilPacket(0x149, channel.SystemId, bytes, bytes.Length, 0, 0, channel.Route);
            }
        }
    }

    public void Update()
    {
        this.UpdateImpl();
    }

    private void UpdateImpl()
    {
        if (this.m_state.m_loginCompleteNotificationReceived)
        {
            foreach (KeyValuePair<int, SystemChannel> pair in this.m_state.m_systems.Systems)
            {
                if (this.UpdateStateSubscribeImpl(pair.Value))
                {
                    this.ProcessClientRequests(pair.Value);
                }
            }
        }
    }

    private bool UpdateStateSubscribeImpl(SystemChannel system)
    {
        switch (system.SubscriptionStatus.CurrentState)
        {
            case SubscriptionStatusType.State.PENDING_SEND:
                return this.ProcessSubscribeStatePendingSend(system);

            case SubscriptionStatusType.State.PENDING_RESPONSE:
                return this.ProcessSubscribeStatePendingResponse(system);

            case SubscriptionStatusType.State.SUBSCRIBED:
                return this.ProcessSubscribeStateSubscribed(system);
        }
        return (system.SubscriptionStatus.CurrentState == SubscriptionStatusType.State.SUBSCRIBED);
    }

    public void WillReset()
    {
        this.m_state = new InternalState();
    }

    public class ClientRequestConfig
    {
        public int RequestedSystem { get; set; }

        public bool ShouldRetryOnError { get; set; }
    }

    private class ClientRequestType
    {
        public byte[] Body;
        public uint Context;
        public bool IsSubsribeRequest;
        public ClientRequestManager.RequestPhase Phase;
        public uint RequestId;
        public uint RetryCount;
        public DateTime SendTime;
        public bool ShouldRetryOnError;
        public int SubID;
        public ClientRequestManager.SystemChannel System;
        public int Type;
    }

    private class InternalState
    {
        public Map<uint, ClientRequestManager.ClientRequestType> m_contextIdToDroppedPacketMap = new Map<uint, ClientRequestManager.ClientRequestType>();
        public Map<uint, ClientRequestManager.ClientRequestType> m_contextIdToRequestMap = new Map<uint, ClientRequestManager.ClientRequestType>();
        public bool m_loginCompleteNotificationReceived;
        public Queue<PegasusPacket> m_pendingDelivery = new Queue<PegasusPacket>();
        public bool m_runningPhaseEnabled;
        public uint m_subscribePacketsSent;
        public ClientRequestManager.SystemMap m_systems = new ClientRequestManager.SystemMap();
    }

    private class PendingMapType
    {
        public Queue<ClientRequestManager.ClientRequestType> PendingSend = new Queue<ClientRequestManager.ClientRequestType>();
    }

    private class PhaseMapType
    {
        public ClientRequestManager.PendingMapType Running = new ClientRequestManager.PendingMapType();
        public ClientRequestManager.PendingMapType StartUp = new ClientRequestManager.PendingMapType();
    }

    public enum RequestPhase
    {
        STARTUP,
        RUNNING
    }

    private class SubscriptionStatusType
    {
        public uint ContexId;
        public State CurrentState;
        public DateTime LastSend = DateTime.MinValue;
        public DateTime SubscribedTime;

        public enum State
        {
            PENDING_SEND,
            PENDING_RESPONSE,
            SUBSCRIBED
        }
    }

    private class SystemChannel
    {
        public ClientRequestManager.RequestPhase CurrentPhase;
        public ulong KeepAliveSecs;
        public uint m_subscribePacketsReceived;
        public ulong MaxResubscribeAttempts;
        public ulong PendingResponseTimeout;
        public ulong PendingSubscribeTimeout = 15L;
        public ClientRequestManager.PhaseMapType Phases = new ClientRequestManager.PhaseMapType();
        public ulong Route;
        public uint SubscribeAttempt;
        public ClientRequestManager.SubscriptionStatusType SubscriptionStatus = new ClientRequestManager.SubscriptionStatusType();
        public int SystemId;
        public bool WasEverInRunningPhase;
    }

    private class SystemMap
    {
        public Map<int, ClientRequestManager.SystemChannel> Systems = new Map<int, ClientRequestManager.SystemChannel>();
    }
}

