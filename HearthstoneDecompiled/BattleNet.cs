using bnet.protocol;
using PegasusShared;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class BattleNet
{
    public const string CosmeticVersion = "4.1";
    public const string COUNTRY_CHINA = "CHN";
    public const string COUNTRY_KOREA = "KOR";
    public const string DEFAULT_INTERNAL_ENVIRONMENT = "bn11-01.battle.net";
    public const int DEFAULT_PORT = 0x45f;
    public const string DEFAULT_PUBLIC_ENVIRONMENT = "us.actual.battle.net";
    public const ushort RouteToAnyUtil = 0;
    private static IBattleNet s_impl = new BattleNetDll();
    public static string TutorialServer = "01";

    public static void AcceptFriendlyChallenge(ref DllEntityId partyId)
    {
        s_impl.AcceptFriendlyChallenge(ref partyId);
    }

    public static void AcceptPartyInvite(ulong invitationId)
    {
        s_impl.AcceptPartyInvite(invitationId);
    }

    public static void AnswerChallenge(ulong challengeID, string answer)
    {
        s_impl.AnswerChallenge(challengeID, answer);
    }

    public static void ApplicationWasPaused()
    {
        s_impl.ApplicationWasPaused();
    }

    public static void ApplicationWasUnpaused()
    {
        s_impl.ApplicationWasUnpaused();
    }

    public static void AppQuit()
    {
        s_impl.AppQuit();
    }

    public static void AssignPartyRole(DllEntityId partyId, DllEntityId memberId, uint roleId)
    {
        s_impl.AssignPartyRole(partyId, memberId, roleId);
    }

    public static int BattleNetStatus()
    {
        return s_impl.BattleNetStatus();
    }

    public static void CancelChallenge(ulong challengeID)
    {
        s_impl.CancelChallenge(challengeID);
    }

    public static void CancelFindGame()
    {
        s_impl.CancelFindGame();
    }

    public static bool CheckWebAuth(out string url)
    {
        return s_impl.CheckWebAuth(out url);
    }

    public static void ClearBnetEvents()
    {
        s_impl.ClearBnetEvents();
    }

    public static void ClearChallenges()
    {
        s_impl.ClearChallenges();
    }

    public static void ClearErrors()
    {
        s_impl.ClearErrors();
    }

    public static void ClearFriendsUpdates()
    {
        s_impl.ClearFriendsUpdates();
    }

    public static void ClearNotifications()
    {
        s_impl.ClearNotifications();
    }

    public static void ClearPartyAttribute(DllEntityId partyId, string attributeKey)
    {
        s_impl.ClearPartyAttribute(partyId, attributeKey);
    }

    public static void ClearPartyListenerEvents()
    {
        s_impl.ClearPartyListenerEvents();
    }

    public static void ClearPartyUpdates()
    {
        s_impl.ClearPartyUpdates();
    }

    public static void ClearPresence()
    {
        s_impl.ClearPresence();
    }

    public static void ClearWhispers()
    {
        s_impl.ClearWhispers();
    }

    public static void CloseAurora()
    {
        s_impl.CloseAurora();
    }

    public static void CreateParty(string partyType, int privacyLevel, byte[] creatorBlob)
    {
        s_impl.CreateParty(partyType, privacyLevel, creatorBlob);
    }

    public static void DeclineFriendlyChallenge(ref DllEntityId partyId)
    {
        s_impl.DeclineFriendlyChallenge(ref partyId);
    }

    public static void DeclinePartyInvite(ulong invitationId)
    {
        s_impl.DeclinePartyInvite(invitationId);
    }

    public static void DissolveParty(DllEntityId partyId)
    {
        s_impl.DissolveParty(partyId);
    }

    public static string FilterProfanity(string unfiltered)
    {
        return s_impl.FilterProfanity(unfiltered);
    }

    public static void FindGame(BnetGameType gameType, int missionId, long deckId, long aiDeckId)
    {
        Guid guid = Guid.NewGuid();
        bool setScenarioIdAttr = RequiresScenarioIdAttribute(gameType);
        s_impl.FindGame(guid.ToByteArray(), gameType, missionId, deckId, aiDeckId, setScenarioIdAttr);
    }

    public static IBattleNet Get()
    {
        return s_impl;
    }

    public static string GetAccountCountry()
    {
        return s_impl.GetAccountCountry();
    }

    public static Network.BnetRegion GetAccountRegion()
    {
        return (Network.BnetRegion) s_impl.GetAccountRegion();
    }

    public static void GetAllPartyAttributes(DllEntityId partyId, out string[] allKeys)
    {
        s_impl.GetAllPartyAttributes(partyId, out allKeys);
    }

    public static void GetBnetEvents([Out] BnetEvent[] events)
    {
        s_impl.GetBnetEvents(events);
    }

    public static int GetBnetEventsSize()
    {
        return s_impl.GetBnetEventsSize();
    }

    public static void GetChallenges([Out] DllChallengeInfo[] challenges)
    {
        s_impl.GetChallenges(challenges);
    }

    public static int GetCountPartyMembers(DllEntityId partyId)
    {
        return s_impl.GetCountPartyMembers(partyId);
    }

    public static Network.BnetRegion GetCurrentRegion()
    {
        return (Network.BnetRegion) s_impl.GetCurrentRegion();
    }

    public static string GetEnvironment()
    {
        return s_impl.GetEnvironment();
    }

    public static void GetErrors([Out] BnetErrorInfo[] errors)
    {
        s_impl.GetErrors(errors);
    }

    public static int GetErrorsCount()
    {
        return s_impl.GetErrorsCount();
    }

    public static void GetFriendsInfo(ref DllFriendsInfo info)
    {
        s_impl.GetFriendsInfo(ref info);
    }

    public static void GetFriendsUpdates([Out] FriendsUpdate[] updates)
    {
        s_impl.GetFriendsUpdates(updates);
    }

    public static string GetLaunchOption(string key)
    {
        return s_impl.GetLaunchOption(key);
    }

    public static int GetMaxPartyMembers(DllEntityId partyId)
    {
        return s_impl.GetMaxPartyMembers(partyId);
    }

    public static DllEntityId GetMyAccoundId()
    {
        return s_impl.GetMyAccountId();
    }

    public static DllEntityId GetMyGameAccountId()
    {
        return s_impl.GetMyGameAccountId();
    }

    public static int GetNotificationCount()
    {
        return s_impl.GetNotificationCount();
    }

    public static void GetNotifications([Out] BnetNotification[] notifications)
    {
        s_impl.GetNotifications(notifications);
    }

    public static void GetPartyAttributeBlob(DllEntityId partyId, string attributeKey, out byte[] value)
    {
        s_impl.GetPartyAttributeBlob(partyId, attributeKey, out value);
    }

    public static bool GetPartyAttributeLong(DllEntityId partyId, string attributeKey, out long value)
    {
        return s_impl.GetPartyAttributeLong(partyId, attributeKey, out value);
    }

    public static void GetPartyAttributeString(DllEntityId partyId, string attributeKey, out string value)
    {
        s_impl.GetPartyAttributeString(partyId, attributeKey, out value);
    }

    public static void GetPartyInviteRequests(DllEntityId partyId, out InviteRequest[] requests)
    {
        s_impl.GetPartyInviteRequests(partyId, out requests);
    }

    public static void GetPartyListenerEvents(out PartyListenerEvent[] events)
    {
        s_impl.GetPartyListenerEvents(out events);
    }

    public static void GetPartyMembers(DllEntityId partyId, out DllPartyMember[] members)
    {
        s_impl.GetPartyMembers(partyId, out members);
    }

    public static int GetPartyPrivacy(DllEntityId partyId)
    {
        return s_impl.GetPartyPrivacy(partyId);
    }

    public static void GetPartySentInvites(DllEntityId partyId, out PartyInvite[] invites)
    {
        s_impl.GetPartySentInvites(partyId, out invites);
    }

    public static void GetPartyUpdates([Out] PartyEvent[] updates)
    {
        s_impl.GetPartyUpdates(updates);
    }

    public static void GetPartyUpdatesInfo(ref DllPartyInfo info)
    {
        s_impl.GetPartyUpdatesInfo(ref info);
    }

    public static void GetPlayRestrictions(ref DllLockouts restrictions, bool reload)
    {
        s_impl.GetPlayRestrictions(ref restrictions, reload);
    }

    public static void GetPresence([Out] PresenceUpdate[] updates)
    {
        s_impl.GetPresence(updates);
    }

    public static QueueEvent GetQueueEvent()
    {
        return s_impl.GetQueueEvent();
    }

    public static void GetQueueInfo(ref DllQueueInfo queueInfo)
    {
        s_impl.GetQueueInfo(ref queueInfo);
    }

    public static void GetReceivedPartyInvites(out PartyInvite[] invites)
    {
        s_impl.GetReceivedPartyInvites(out invites);
    }

    public static int GetShutdownMinutes()
    {
        return s_impl.GetShutdownMinutes();
    }

    public static string GetStoredBNetIP()
    {
        return s_impl.GetStoredBNetIPAddress();
    }

    public static string GetVersion()
    {
        return s_impl.GetVersion();
    }

    public static int GetVersionInt()
    {
        return s_impl.GetVersionInt();
    }

    public static string GetVersionSource()
    {
        return s_impl.GetVersionSource();
    }

    public static string GetVersionString()
    {
        return s_impl.GetVersionString();
    }

    public static void GetWhisperInfo(ref DllWhisperInfo info)
    {
        s_impl.GetWhisperInfo(ref info);
    }

    public static void GetWhispers([Out] BnetWhisper[] whispers)
    {
        s_impl.GetWhispers(whispers);
    }

    public static void IgnoreInviteRequest(DllEntityId partyId, DllEntityId requestedTargetId)
    {
        s_impl.IgnoreInviteRequest(partyId, requestedTargetId);
    }

    public static bool Init(bool internalMode)
    {
        return s_impl.Init(internalMode);
    }

    public static bool IsInitialized()
    {
        return s_impl.IsInitialized();
    }

    public static bool IsVersionInt()
    {
        return (GetVersionSource() == "int");
    }

    public static void JoinParty(DllEntityId partyId, string partyType)
    {
        s_impl.JoinParty(partyId, partyType);
    }

    public static void KickPartyMember(DllEntityId partyId, DllEntityId memberId)
    {
        s_impl.KickPartyMember(partyId, memberId);
    }

    public static void LeaveParty(DllEntityId partyId)
    {
        s_impl.LeaveParty(partyId);
    }

    public static void ManageFriendInvite(int action, ulong inviteId)
    {
        s_impl.ManageFriendInvite(action, inviteId);
    }

    public static PegasusPacket NextUtilPacket()
    {
        return s_impl.NextUtilPacket();
    }

    public static int NumChallenges()
    {
        return s_impl.NumChallenges();
    }

    public static int PresenceSize()
    {
        return s_impl.PresenceSize();
    }

    public static void ProcessAurora()
    {
        s_impl.ProcessAurora();
    }

    public static int ProductVersion()
    {
        return 0x4010000;
    }

    public static void ProvideWebAuthToken(string token)
    {
        s_impl.ProvideWebAuthToken(token);
    }

    public static void QueryAurora()
    {
        s_impl.QueryAurora();
    }

    public static void RemoveFriend(BnetAccountId account)
    {
        s_impl.RemoveFriend(account);
    }

    public static void RequestCloseAurora()
    {
        s_impl.RequestCloseAurora();
    }

    public static void RequestPartyInvite(DllEntityId partyId, DllEntityId whomToAskForApproval, DllEntityId whomToInvite, string szPartyType)
    {
        s_impl.RequestPartyInvite(partyId, whomToAskForApproval, whomToInvite, szPartyType);
    }

    public static void RequestPresenceFields(bool isGameAccountEntityId, [In] DllEntityId entityId, [In] DllPresenceFieldKey[] fieldList)
    {
        s_impl.RequestPresenceFields(isGameAccountEntityId, entityId, fieldList);
    }

    public static bool RequiresScenarioIdAttribute(BnetGameType bnetGameType)
    {
        switch (bnetGameType)
        {
            case BnetGameType.BGT_TAVERNBRAWL_PVP:
            case BnetGameType.BGT_TAVERNBRAWL_2P_COOP:
            case BnetGameType.BGT_FRIENDS:
                return true;
        }
        return false;
    }

    public static void RescindFriendlyChallenge(ref DllEntityId partyId)
    {
        s_impl.RescindFriendlyChallenge(ref partyId);
    }

    public static void Reset(bool internalMode)
    {
        s_impl.Reset();
        s_impl = new BattleNetDll();
        Init(internalMode);
    }

    public static void RevokePartyInvite(DllEntityId partyId, ulong invitationId)
    {
        s_impl.RevokePartyInvite(partyId, invitationId);
    }

    public static void SendFriendInvite(string inviter, string invitee, bool byEmail)
    {
        s_impl.SendFriendInvite(inviter, invitee, byEmail);
    }

    public static void SendFriendlyChallengeInvite(ref DllEntityId gameAccount, int scenarioId)
    {
        s_impl.SendFriendlyChallengeInvite(ref gameAccount, scenarioId);
    }

    public static void SendPartyChatMessage(DllEntityId partyId, string message)
    {
        s_impl.SendPartyChatMessage(partyId, message);
    }

    public static void SendPartyInvite(DllEntityId partyId, DllEntityId inviteeId, bool isReservation)
    {
        s_impl.SendPartyInvite(partyId, inviteeId, isReservation);
    }

    public static void SendUtilPacket(int packetId, int systemId, byte[] bytes, int size, int subID, int context, ulong route)
    {
        s_impl.SendUtilPacket(packetId, systemId, bytes, size, subID, context, route);
    }

    public static void SendWhisper(BnetGameAccountId gameAccount, string message)
    {
        s_impl.SendWhisper(gameAccount, message);
    }

    public static void SetPartyAttributeBlob(DllEntityId partyId, string attributeKey, [In] byte[] value)
    {
        s_impl.SetPartyAttributeBlob(partyId, attributeKey, value);
    }

    public static void SetPartyAttributeLong(DllEntityId partyId, string attributeKey, [In] long value)
    {
        s_impl.SetPartyAttributeLong(partyId, attributeKey, value);
    }

    public static void SetPartyAttributeString(DllEntityId partyId, string attributeKey, [In] string value)
    {
        s_impl.SetPartyAttributeString(partyId, attributeKey, value);
    }

    public static void SetPartyDeck(ref DllEntityId partyId, long deckID)
    {
        s_impl.SetMyFriendlyChallengeDeck(ref partyId, deckID);
    }

    public static void SetPartyPrivacy(DllEntityId partyId, int privacyLevel)
    {
        s_impl.SetPartyPrivacy(partyId, privacyLevel);
    }

    public static void SetPresenceBlob(uint field, byte[] val)
    {
        s_impl.SetPresenceBlob(field, val);
    }

    public static void SetPresenceBool(uint field, bool val)
    {
        s_impl.SetPresenceBool(field, val);
    }

    public static void SetPresenceInt(uint field, long val)
    {
        s_impl.SetPresenceInt(field, val);
    }

    public static void SetPresenceString(uint field, string val)
    {
        s_impl.SetPresenceString(field, val);
    }

    public static void SetRichPresence([In] DllRichPresenceUpdate[] updates)
    {
        s_impl.SetRichPresence(updates);
    }

    public static BattleNetLogSource Log
    {
        get
        {
            return s_impl.GetLogSource();
        }
    }

    public enum BnetEvent
    {
        Disconnected
    }

    public enum BNetState
    {
        BATTLE_NET_UNKNOWN,
        BATTLE_NET_LOGGING_IN,
        BATTLE_NET_TIMEOUT,
        BATTLE_NET_LOGIN_FAILED,
        BATTLE_NET_LOGGED_IN
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllBnetEvent
    {
        public int bnetEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllChallengeInfo
    {
        public ulong challengeId;
        [MarshalAs(UnmanagedType.I1)]
        public bool isRetry;
        public int type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllEntityId
    {
        public ulong hi;
        public ulong lo;
        public DllEntityId(BattleNet.DllEntityId copyFrom)
        {
            this.hi = copyFrom.hi;
            this.lo = copyFrom.lo;
        }

        public DllEntityId(EntityId protoEntityId)
        {
            this.hi = protoEntityId.High;
            this.lo = protoEntityId.Low;
        }

        public EntityId ToProtocol()
        {
            EntityId id = new EntityId();
            id.SetHigh(this.hi);
            id.SetLow(this.lo);
            return id;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllErrorInfo
    {
        public int feature;
        public int featureEvent;
        public uint code;
        public int context;
        public IntPtr name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllFriendsInfo
    {
        public int maxFriends;
        public int maxRecvInvites;
        public int maxSentInvites;
        public int friendsSize;
        public int updateSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllFriendsUpdate
    {
        public int action;
        public BattleNet.DllEntityId entity1;
        public BattleNet.DllEntityId entity2;
        public int int1;
        public IntPtr string1;
        public IntPtr string2;
        public IntPtr string3;
        public ulong long1;
        public ulong long2;
        public ulong long3;
        [MarshalAs(UnmanagedType.I1)]
        public bool bool1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllGameServerInfo
    {
        public IntPtr Address;
        public int Port;
        public int GameHandle;
        public long ClientHandle;
        public IntPtr AuroraPassword;
        public IntPtr Version;
        public IntPtr SpectatorPassword;
        [MarshalAs(UnmanagedType.I1)]
        public bool Resumable;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllInvitation
    {
        public BattleNet.DllInvitationId id;
        public BattleNet.DllEntityId inviterId;
        public IntPtr inviterName;
        public BattleNet.DllEntityId inviteeId;
        public IntPtr inviteeName;
        public IntPtr message;
        public ulong creationTimeMicrosec;
        public ulong expirationTimeMicrosec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllInvitationId
    {
        public ulong val;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllLockouts
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool loaded;
        [MarshalAs(UnmanagedType.I1)]
        public bool loading;
        [MarshalAs(UnmanagedType.I1)]
        public bool readingPCI;
        [MarshalAs(UnmanagedType.I1)]
        public bool readingGTRI;
        [MarshalAs(UnmanagedType.I1)]
        public bool readingCAISI;
        [MarshalAs(UnmanagedType.I1)]
        public bool readingGSI;
        [MarshalAs(UnmanagedType.I1)]
        public bool parentalControls;
        [MarshalAs(UnmanagedType.I1)]
        public bool parentalTimedAccount;
        public int parentalMinutesRemaining;
        public IntPtr day1;
        public IntPtr day2;
        public IntPtr day3;
        public IntPtr day4;
        public IntPtr day5;
        public IntPtr day6;
        public IntPtr day7;
        [MarshalAs(UnmanagedType.I1)]
        public bool timedAccount;
        public int minutesRemaining;
        public ulong sessionStartTime;
        [MarshalAs(UnmanagedType.I1)]
        public bool CAISactive;
        public int CAISplayed;
        public int CAISrested;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllNotification
    {
        public IntPtr notificationType;
        public IntPtr blobMessage;
        public int blobSize;
        public int messageId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyEvent
    {
        public IntPtr eventName;
        public IntPtr eventData;
        public BattleNet.DllEntityId partyId;
        public BattleNet.DllEntityId otherMemberId;
        public BattleNet.DllErrorInfo errorInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyInfo
    {
        public int size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyMember
    {
        public BattleNet.DllEntityId memberGameAccountId;
        public uint firstMemberRole;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPresenceFieldKey
    {
        public uint programId;
        public uint groupId;
        public uint fieldId;
        public ulong index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPresenceUpdate
    {
        public BattleNet.DllEntityId entityId;
        public uint programId;
        public uint groupId;
        public uint fieldId;
        public ulong index;
        [MarshalAs(UnmanagedType.I1)]
        public bool boolVal;
        public long intVal;
        public IntPtr stringVal;
        public BattleNet.DllEntityId entityIdVal;
        public IntPtr blobVal;
        public int blobValSize;
        [MarshalAs(UnmanagedType.I1)]
        public bool valCleared;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllQueueEvent
    {
        public int EventType;
        public int MinSeconds;
        public int MaxSeconds;
        public int BnetError;
        public BattleNet.DllGameServerInfo GameServer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllQueueInfo
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool changed;
        public int position;
        public long end;
        public long stdev;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllRichPresenceUpdate
    {
        public ulong presenceFieldIndex;
        public uint programId;
        public uint streamId;
        public uint index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllWhisper
    {
        public BattleNet.DllEntityId speakerId;
        public BattleNet.DllEntityId receiverId;
        public IntPtr message;
        public ulong timestampMicrosec;
        public BattleNet.DllErrorInfo errorInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllWhisperInfo
    {
        public int whisperSize;
        public int sendResultsSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FriendsUpdate
    {
        public int action;
        public BnetEntityId entity1;
        public BnetEntityId entity2;
        public int int1;
        public string string1;
        public string string2;
        public string string3;
        public ulong long1;
        public ulong long2;
        public ulong long3;
        [MarshalAs(UnmanagedType.I1)]
        public bool bool1;
        public enum Action
        {
            UNKNOWN,
            FRIEND_ADDED,
            FRIEND_REMOVED,
            FRIEND_INVITE,
            FRIEND_INVITE_REMOVED,
            FRIEND_SENT_INVITE,
            FRIEND_SENT_INVITE_REMOVED,
            FRIEND_ROLE_CHANGE,
            FRIEND_ATTR_CHANGE,
            FRIEND_GAME_ADDED,
            FRIEND_GAME_REMOVED
        }
    }

    public class GameServerInfo
    {
        public string Address { get; set; }

        public string AuroraPassword { get; set; }

        public long ClientHandle { get; set; }

        public int GameHandle { get; set; }

        public int Mission { get; set; }

        public int Port { get; set; }

        public bool Resumable { get; set; }

        public bool SpectatorMode { get; set; }

        public string SpectatorPassword { get; set; }

        public string Version { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PartyEvent
    {
        public string eventName;
        public string eventData;
        public BattleNet.DllEntityId partyId;
        public BattleNet.DllEntityId otherMemberId;
        public BnetErrorInfo errorInfo;
        public void CopyFrom(BattleNet.DllPartyEvent partyEvent)
        {
            this.eventName = MemUtils.StringFromUtf8Ptr(partyEvent.eventName);
            this.eventData = MemUtils.StringFromUtf8Ptr(partyEvent.eventData);
            this.partyId = partyEvent.partyId;
            this.otherMemberId = partyEvent.otherMemberId;
            this.errorInfo = BnetErrorInfo.CreateFromDll(partyEvent.errorInfo);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PartyListenerEvent
    {
        public BattleNet.PartyListenerEventType Type;
        public PartyId PartyId;
        public BnetGameAccountId SubjectMemberId;
        public BnetGameAccountId TargetMemberId;
        public uint UintData;
        public ulong UlongData;
        public string StringData;
        public string StringData2;
        public byte[] BlobData;
        public PartyError ToPartyError()
        {
            return new PartyError { IsOperationCallback = this.Type == BattleNet.PartyListenerEventType.OPERATION_CALLBACK, DebugContext = this.StringData, ErrorCode = this.UintData, Feature = (BnetFeature) ((int) (this.UlongData >> 0x20)), FeatureEvent = (BnetFeatureEvent) ((uint) (this.UlongData & 0xffffffffL)), PartyId = this.PartyId, szPartyType = this.StringData2, StringData = this.StringData };
        }
        public enum AttributeChangeEvent_AttrType
        {
            ATTR_TYPE_NULL,
            ATTR_TYPE_LONG,
            ATTR_TYPE_STRING,
            ATTR_TYPE_BLOB
        }
    }

    public enum PartyListenerEventType
    {
        ERROR_RAISED,
        JOINED_PARTY,
        LEFT_PARTY,
        PRIVACY_CHANGED,
        MEMBER_JOINED,
        MEMBER_LEFT,
        MEMBER_ROLE_CHANGED,
        RECEIVED_INVITE_ADDED,
        RECEIVED_INVITE_REMOVED,
        PARTY_INVITE_SENT,
        PARTY_INVITE_REMOVED,
        INVITE_REQUEST_ADDED,
        INVITE_REQUEST_REMOVED,
        CHAT_MESSAGE_RECEIVED,
        PARTY_ATTRIBUTE_CHANGED,
        MEMBER_ATTRIBUTE_CHANGED,
        OPERATION_CALLBACK
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PresenceUpdate
    {
        public BattleNet.DllEntityId entityId;
        public uint programId;
        public uint groupId;
        public uint fieldId;
        public ulong index;
        public bool boolVal;
        public long intVal;
        public string stringVal;
        public BattleNet.DllEntityId entityIdVal;
        public byte[] blobVal;
        public bool valCleared;
        public void CopyFrom(BattleNet.DllPresenceUpdate update)
        {
            this.entityId = update.entityId;
            this.programId = update.programId;
            this.groupId = update.groupId;
            this.fieldId = update.fieldId;
            this.index = update.index;
            this.boolVal = update.boolVal;
            this.intVal = update.intVal;
            this.stringVal = MemUtils.StringFromUtf8Ptr(update.stringVal);
            this.entityIdVal = update.entityIdVal;
            this.blobVal = MemUtils.PtrToBytes(update.blobVal, update.blobValSize);
            this.valCleared = update.valCleared;
        }
    }

    public class QueueEvent
    {
        public QueueEvent(BattleNet.DllQueueEvent queueEvent)
        {
            this.EventType = (Type) queueEvent.EventType;
            this.MinSeconds = queueEvent.MinSeconds;
            this.MaxSeconds = queueEvent.MaxSeconds;
            this.BnetError = queueEvent.BnetError;
            if (this.EventType == Type.QUEUE_GAME_STARTED)
            {
                this.GameServer = new BattleNet.GameServerInfo();
                if (queueEvent.GameServer.Address != IntPtr.Zero)
                {
                    this.GameServer.Address = Marshal.PtrToStringAnsi(queueEvent.GameServer.Address);
                }
                if (queueEvent.GameServer.AuroraPassword != IntPtr.Zero)
                {
                    this.GameServer.AuroraPassword = Marshal.PtrToStringAnsi(queueEvent.GameServer.AuroraPassword);
                }
                this.GameServer.ClientHandle = queueEvent.GameServer.ClientHandle;
                this.GameServer.GameHandle = queueEvent.GameServer.GameHandle;
                this.GameServer.Port = queueEvent.GameServer.Port;
                if (queueEvent.GameServer.Version != IntPtr.Zero)
                {
                    this.GameServer.Version = Marshal.PtrToStringAnsi(queueEvent.GameServer.Version);
                }
                this.GameServer.Resumable = queueEvent.GameServer.Resumable;
                if (queueEvent.GameServer.SpectatorPassword != IntPtr.Zero)
                {
                    this.GameServer.SpectatorPassword = Marshal.PtrToStringAnsi(queueEvent.GameServer.SpectatorPassword);
                }
            }
            else
            {
                this.GameServer = null;
            }
        }

        public QueueEvent(Type t, int minSeconds, int maxSeconds, int bnetError, BattleNet.GameServerInfo gsInfo)
        {
            this.EventType = t;
            this.MinSeconds = minSeconds;
            this.MaxSeconds = maxSeconds;
            this.BnetError = bnetError;
            this.GameServer = gsInfo;
        }

        public int BnetError { get; set; }

        public Type EventType { get; set; }

        public BattleNet.GameServerInfo GameServer { get; set; }

        public int MaxSeconds { get; set; }

        public int MinSeconds { get; set; }

        public enum Type
        {
            UNKNOWN,
            QUEUE_ENTER,
            QUEUE_LEAVE,
            QUEUE_DELAY,
            QUEUE_UPDATE,
            QUEUE_DELAY_ERROR,
            QUEUE_AMM_ERROR,
            QUEUE_WAIT_END,
            QUEUE_CANCEL,
            QUEUE_GAME_STARTED,
            ABORT_CLIENT_DROPPED
        }
    }

    public enum Version
    {
        Major = 4,
        Minor = 1,
        Patch = 0,
        Sku = 0
    }
}

