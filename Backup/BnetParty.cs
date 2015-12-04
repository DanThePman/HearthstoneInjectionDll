using bnet;
using bnet.protocol.attribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public static class BnetParty
{
    [CompilerGenerated]
    private static Func<KeyValuePair<PartyId, PartyType>, PartyInfo> <>f__am$cacheD;
    [CompilerGenerated]
    private static Func<PartyAttributeChangedHandler, int, <>__AnonType1<PartyAttributeChangedHandler, int>> <>f__am$cacheE;
    public const string ATTRIBUTE_FRIENDLY_DECLINE_REASON = "WTCG.Friendly.DeclineReason";
    public const string ATTRIBUTE_PARTY_CREATOR = "WTCG.Party.Creator";
    public const string ATTRIBUTE_PARTY_SERVER_INFO = "WTCG.Party.ServerInfo";
    public const string ATTRIBUTE_PARTY_TYPE = "WTCG.Party.Type";
    public const string ATTRIBUTE_SCENARIO_ID = "WTCG.Game.ScenarioId";
    private static Map<string, List<PartyAttributeChangedHandler>> s_attributeChangedSubscribers;
    private static Map<BnetFeatureEvent, HashSet<BattleNetErrors>> s_ignorableErrorCodes;
    private static Map<PartyId, PartyType> s_joinedParties;
    private static Map<PartyType, CreateSuccessCallback> s_pendingPartyCreates;

    public static  event ChatMessageHandler OnChatMessage;

    public static  event PartyErrorHandler OnError;

    public static  event JoinedHandler OnJoined;

    public static  event MemberEventHandler OnMemberEvent;

    public static  event PartyAttributeChangedHandler OnPartyAttributeChanged;

    public static  event PrivacyLevelChangedHandler OnPrivacyLevelChanged;

    public static  event ReceivedInviteHandler OnReceivedInvite;

    public static  event ReceivedInviteRequestHandler OnReceivedInviteRequest;

    public static  event SentInviteHandler OnSentInvite;

    static BnetParty()
    {
        Map<BnetFeatureEvent, HashSet<BattleNetErrors>> map = new Map<BnetFeatureEvent, HashSet<BattleNetErrors>>();
        map.Add(BnetFeatureEvent.Party_KickMember_Callback, new HashSet<BattleNetErrors> { 0x2714 });
        map.Add(BnetFeatureEvent.Party_Leave_Callback, new HashSet<BattleNetErrors> { 0x2712, 0x2711 });
        s_ignorableErrorCodes = map;
        s_joinedParties = new Map<PartyId, PartyType>();
        s_pendingPartyCreates = null;
        s_attributeChangedSubscribers = null;
    }

    public static void AcceptInviteRequest(PartyId partyId, BnetGameAccountId requestedTargetId)
    {
        SendInvite(partyId, requestedTargetId);
    }

    public static void AcceptReceivedInvite(ulong inviteId)
    {
        BattleNet.AcceptPartyInvite(inviteId);
    }

    public static void ClearPartyAttribute(PartyId partyId, string attributeKey)
    {
        BattleNet.ClearPartyAttribute(partyId.ToDllEntityId(), attributeKey);
    }

    public static int CountMembers(PartyId partyId)
    {
        if (partyId == null)
        {
            return 0;
        }
        return BattleNet.GetCountPartyMembers(partyId.ToDllEntityId());
    }

    public static void CreateParty(PartyType partyType, PrivacyLevel privacyLevel, CreateSuccessCallback successCallback)
    {
        string szPartyType = EnumUtils.GetString<PartyType>(partyType);
        if ((s_pendingPartyCreates != null) && s_pendingPartyCreates.ContainsKey(partyType))
        {
            object[] args = new object[] { partyType };
            RaisePartyError(true, szPartyType, BnetFeatureEvent.Party_Create_Callback, 6, "CreateParty: Already creating party of type {0}", args);
        }
        else
        {
            if (s_pendingPartyCreates == null)
            {
                s_pendingPartyCreates = new Map<PartyType, CreateSuccessCallback>();
            }
            s_pendingPartyCreates[partyType] = successCallback;
            byte[] creatorBlob = ProtobufUtil.ToByteArray(BnetEntityId.CreateForNet(BnetPresenceMgr.Get().GetMyGameAccountId()));
            BattleNet.CreateParty(szPartyType, (int) privacyLevel, creatorBlob);
        }
    }

    public static void DeclineReceivedInvite(ulong inviteId)
    {
        BattleNet.DeclinePartyInvite(inviteId);
    }

    public static void DissolveParty(PartyId partyId)
    {
        if (IsInParty(partyId))
        {
            BattleNet.DissolveParty(partyId.ToDllEntityId());
        }
    }

    public static KeyValuePair<string, object>[] GetAllPartyAttributes(PartyId partyId)
    {
        string[] strArray;
        if (partyId == null)
        {
            return new KeyValuePair<string, object>[0];
        }
        BattleNet.GetAllPartyAttributes(partyId.ToDllEntityId(), out strArray);
        KeyValuePair<string, object>[] pairArray = new KeyValuePair<string, object>[strArray.Length];
        for (int i = 0; i < pairArray.Length; i++)
        {
            string attributeKey = strArray[i];
            object obj2 = null;
            long? partyAttributeLong = GetPartyAttributeLong(partyId, attributeKey);
            if (partyAttributeLong.HasValue)
            {
                obj2 = partyAttributeLong;
            }
            string partyAttributeString = GetPartyAttributeString(partyId, attributeKey);
            if (partyAttributeString != null)
            {
                obj2 = partyAttributeString;
            }
            byte[] partyAttributeBlob = GetPartyAttributeBlob(partyId, attributeKey);
            if (partyAttributeBlob != null)
            {
                obj2 = partyAttributeBlob;
            }
            pairArray[i] = new KeyValuePair<string, object>(attributeKey, obj2);
        }
        return pairArray;
    }

    public static InviteRequest[] GetInviteRequests(PartyId partyId)
    {
        InviteRequest[] requestArray;
        if (partyId == null)
        {
            return new InviteRequest[0];
        }
        BattleNet.GetPartyInviteRequests(partyId.ToDllEntityId(), out requestArray);
        return requestArray;
    }

    public static PartyInfo[] GetJoinedParties()
    {
        if (<>f__am$cacheD == null)
        {
            <>f__am$cacheD = kv => new PartyInfo(kv.Key, kv.Value);
        }
        return Enumerable.Select<KeyValuePair<PartyId, PartyType>, PartyInfo>(s_joinedParties, <>f__am$cacheD).ToArray<PartyInfo>();
    }

    public static PartyInfo GetJoinedParty(PartyId partyId)
    {
        PartyType type;
        if ((partyId != null) && s_joinedParties.TryGetValue(partyId, out type))
        {
            return new PartyInfo(partyId, type);
        }
        return null;
    }

    public static PartyId[] GetJoinedPartyIds()
    {
        return s_joinedParties.Keys.ToArray<PartyId>();
    }

    public static PartyMember GetLeader(PartyId partyId)
    {
        if (partyId != null)
        {
            PartyMember[] members = GetMembers(partyId);
            PartyType partyType = GetPartyType(partyId);
            for (int i = 0; i < members.Length; i++)
            {
                PartyMember member = members[i];
                if (member.IsLeader(partyType))
                {
                    return member;
                }
            }
        }
        return null;
    }

    public static PartyMember GetMember(PartyId partyId, BnetGameAccountId memberId)
    {
        foreach (PartyMember member in GetMembers(partyId))
        {
            if (member.GameAccountId == memberId)
            {
                return member;
            }
        }
        return null;
    }

    public static PartyMember[] GetMembers(PartyId partyId)
    {
        BattleNet.DllPartyMember[] memberArray;
        if (partyId == null)
        {
            return new PartyMember[0];
        }
        BattleNet.GetPartyMembers(partyId.ToDllEntityId(), out memberArray);
        PartyMember[] memberArray2 = new PartyMember[memberArray.Length];
        for (int i = 0; i < memberArray2.Length; i++)
        {
            BattleNet.DllPartyMember member = memberArray[i];
            PartyMember member2 = new PartyMember {
                GameAccountId = BnetGameAccountId.CreateFromDll(member.memberGameAccountId)
            };
            member2.RoleIds = new uint[] { member.firstMemberRole };
            memberArray2[i] = member2;
        }
        return memberArray2;
    }

    public static PartyMember GetMyselfMember(PartyId partyId)
    {
        if (partyId == null)
        {
            return null;
        }
        BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
        if (myGameAccountId == null)
        {
            return null;
        }
        return GetMember(partyId, myGameAccountId);
    }

    public static byte[] GetPartyAttributeBlob(PartyId partyId, string attributeKey)
    {
        byte[] buffer;
        if (partyId == null)
        {
            return null;
        }
        BattleNet.GetPartyAttributeBlob(partyId.ToDllEntityId(), attributeKey, out buffer);
        return buffer;
    }

    public static long? GetPartyAttributeLong(PartyId partyId, string attributeKey)
    {
        long num;
        if ((partyId != null) && BattleNet.GetPartyAttributeLong(partyId.ToDllEntityId(), attributeKey, out num))
        {
            return new long?(num);
        }
        return null;
    }

    public static string GetPartyAttributeString(PartyId partyId, string attributeKey)
    {
        string str;
        if (partyId == null)
        {
            return null;
        }
        BattleNet.GetPartyAttributeString(partyId.ToDllEntityId(), attributeKey, out str);
        return str;
    }

    public static bnet.protocol.attribute.Variant GetPartyAttributeVariant(PartyId partyId, string attributeKey)
    {
        long num;
        string str;
        byte[] buffer;
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        BattleNet.DllEntityId id = partyId.ToDllEntityId();
        if (BattleNet.GetPartyAttributeLong(id, attributeKey, out num))
        {
            variant.IntValue = num;
            return variant;
        }
        BattleNet.GetPartyAttributeString(id, attributeKey, out str);
        if (str != null)
        {
            variant.StringValue = str;
            return variant;
        }
        BattleNet.GetPartyAttributeBlob(id, attributeKey, out buffer);
        if (buffer != null)
        {
            variant.BlobValue = buffer;
        }
        return variant;
    }

    public static PartyType GetPartyType(PartyId partyId)
    {
        PartyType dEFAULT = PartyType.DEFAULT;
        if (partyId != null)
        {
            s_joinedParties.TryGetValue(partyId, out dEFAULT);
        }
        return dEFAULT;
    }

    public static PartyType GetPartyTypeFromString(string partyType)
    {
        PartyType dEFAULT = PartyType.DEFAULT;
        if (partyType != null)
        {
            EnumUtils.TryGetEnum<PartyType>(partyType, out dEFAULT);
        }
        return dEFAULT;
    }

    public static PrivacyLevel GetPrivacyLevel(PartyId partyId)
    {
        if (partyId == null)
        {
            return PrivacyLevel.CLOSED;
        }
        return (PrivacyLevel) BattleNet.GetPartyPrivacy(partyId.ToDllEntityId());
    }

    public static PartyInvite GetReceivedInvite(ulong inviteId)
    {
        <GetReceivedInvite>c__AnonStorey392 storey = new <GetReceivedInvite>c__AnonStorey392 {
            inviteId = inviteId
        };
        return Enumerable.FirstOrDefault<PartyInvite>(GetReceivedInvites(), new Func<PartyInvite, bool>(storey.<>m__297));
    }

    public static PartyInvite GetReceivedInviteFrom(BnetGameAccountId inviterId, PartyType partyType)
    {
        <GetReceivedInviteFrom>c__AnonStorey393 storey = new <GetReceivedInviteFrom>c__AnonStorey393 {
            inviterId = inviterId,
            partyType = partyType
        };
        return Enumerable.FirstOrDefault<PartyInvite>(GetReceivedInvites(), new Func<PartyInvite, bool>(storey.<>m__298));
    }

    public static PartyInvite[] GetReceivedInvites()
    {
        PartyInvite[] inviteArray;
        BattleNet.GetReceivedPartyInvites(out inviteArray);
        return inviteArray;
    }

    public static PartyInvite GetSentInvite(PartyId partyId, ulong inviteId)
    {
        <GetSentInvite>c__AnonStorey394 storey = new <GetSentInvite>c__AnonStorey394 {
            inviteId = inviteId
        };
        if (partyId == null)
        {
            return null;
        }
        return Enumerable.FirstOrDefault<PartyInvite>(GetSentInvites(partyId), new Func<PartyInvite, bool>(storey.<>m__299));
    }

    public static PartyInvite[] GetSentInvites(PartyId partyId)
    {
        PartyInvite[] inviteArray;
        if (partyId == null)
        {
            return new PartyInvite[0];
        }
        BattleNet.GetPartySentInvites(partyId.ToDllEntityId(), out inviteArray);
        return inviteArray;
    }

    public static void IgnoreInviteRequest(PartyId partyId, BnetGameAccountId requestedTargetId)
    {
        BattleNet.DllEntityId id = partyId.ToDllEntityId();
        BattleNet.DllEntityId id2 = BnetEntityId.CreateForDll(requestedTargetId);
        BattleNet.IgnoreInviteRequest(id, id2);
    }

    private static bool IsIgnorableError(BnetFeatureEvent feature, BattleNetErrors code)
    {
        HashSet<BattleNetErrors> set;
        return (s_ignorableErrorCodes.TryGetValue(feature, out set) && set.Contains(code));
    }

    public static bool IsInParty(PartyId partyId)
    {
        if (partyId == null)
        {
            return false;
        }
        return s_joinedParties.ContainsKey(partyId);
    }

    public static bool IsLeader(PartyId partyId)
    {
        if (partyId != null)
        {
            PartyMember myselfMember = GetMyselfMember(partyId);
            if (myselfMember != null)
            {
                PartyType partyType = GetPartyType(partyId);
                if (myselfMember.IsLeader(partyType))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsMember(PartyId partyId, BnetGameAccountId memberId)
    {
        if (partyId == null)
        {
            return false;
        }
        return (GetMember(partyId, memberId) != null);
    }

    public static bool IsPartyFull(PartyId partyId, bool includeInvites = true)
    {
        if (partyId == null)
        {
            return false;
        }
        int num = CountMembers(partyId);
        int num2 = !includeInvites ? 0 : GetSentInvites(partyId).Length;
        int maxPartyMembers = BattleNet.GetMaxPartyMembers(partyId.ToDllEntityId());
        return ((num + num2) >= maxPartyMembers);
    }

    public static void JoinParty(PartyId partyId, PartyType partyType)
    {
        BattleNet.JoinParty(partyId.ToDllEntityId(), EnumUtils.GetString<PartyType>(partyType));
    }

    public static void KickMember(PartyId partyId, BnetGameAccountId memberId)
    {
        if (IsInParty(partyId))
        {
            BattleNet.DllEntityId id = partyId.ToDllEntityId();
            BattleNet.DllEntityId id2 = BnetEntityId.CreateForDll(memberId);
            BattleNet.KickPartyMember(id, id2);
        }
    }

    public static void Leave(PartyId partyId)
    {
        if (IsInParty(partyId))
        {
            BattleNet.LeaveParty(partyId.ToDllEntityId());
        }
    }

    public static void Process()
    {
        BattleNet.PartyListenerEvent[] eventArray;
        BattleNet.GetPartyListenerEvents(out eventArray);
        BattleNet.ClearPartyListenerEvents();
        for (int i = 0; i < eventArray.Length; i++)
        {
            PartyError error;
            string str;
            PartyType partyTypeFromString;
            CreateSuccessCallback callback;
            PartyInfo info4;
            List<PartyAttributeChangedHandler> list;
            BattleNet.PartyListenerEvent event2 = eventArray[i];
            PartyId partyId = event2.PartyId;
            switch (event2.Type)
            {
                case BattleNet.PartyListenerEventType.ERROR_RAISED:
                case BattleNet.PartyListenerEventType.OPERATION_CALLBACK:
                    error = event2.ToPartyError();
                    if (error.ErrorCode == 0)
                    {
                        goto Label_0136;
                    }
                    if (!IsIgnorableError(error.FeatureEvent, error.ErrorCode.EnumVal))
                    {
                        break;
                    }
                    error.ErrorCode = 0;
                    if (error.FeatureEvent != BnetFeatureEvent.Party_Leave_Callback)
                    {
                        break;
                    }
                    if (!s_joinedParties.ContainsKey(partyId))
                    {
                        s_joinedParties[partyId] = PartyType.SPECTATOR_PARTY;
                    }
                    goto Label_022D;

                case BattleNet.PartyListenerEventType.JOINED_PARTY:
                    str = event2.StringData;
                    partyTypeFromString = GetPartyTypeFromString(str);
                    s_joinedParties[partyId] = partyTypeFromString;
                    if (s_pendingPartyCreates == null)
                    {
                        goto Label_0201;
                    }
                    callback = null;
                    if (!s_pendingPartyCreates.ContainsKey(partyTypeFromString))
                    {
                        goto Label_01B1;
                    }
                    callback = s_pendingPartyCreates[partyTypeFromString];
                    s_pendingPartyCreates.Remove(partyTypeFromString);
                    goto Label_01F0;

                case BattleNet.PartyListenerEventType.LEFT_PARTY:
                    goto Label_022D;

                case BattleNet.PartyListenerEventType.PRIVACY_CHANGED:
                    goto Label_0284;

                case BattleNet.PartyListenerEventType.MEMBER_JOINED:
                case BattleNet.PartyListenerEventType.MEMBER_LEFT:
                    goto Label_02AA;

                case BattleNet.PartyListenerEventType.MEMBER_ROLE_CHANGED:
                {
                    if (OnMemberEvent != null)
                    {
                        LeaveReason? reason = null;
                        OnMemberEvent(OnlineEventType.UPDATED, GetJoinedParty(partyId), event2.SubjectMemberId, true, reason);
                    }
                    continue;
                }
                case BattleNet.PartyListenerEventType.RECEIVED_INVITE_ADDED:
                case BattleNet.PartyListenerEventType.RECEIVED_INVITE_REMOVED:
                    goto Label_0344;

                case BattleNet.PartyListenerEventType.PARTY_INVITE_SENT:
                case BattleNet.PartyListenerEventType.PARTY_INVITE_REMOVED:
                    goto Label_03CF;

                case BattleNet.PartyListenerEventType.INVITE_REQUEST_ADDED:
                case BattleNet.PartyListenerEventType.INVITE_REQUEST_REMOVED:
                    goto Label_0476;

                case BattleNet.PartyListenerEventType.CHAT_MESSAGE_RECEIVED:
                {
                    if (OnChatMessage != null)
                    {
                        OnChatMessage(GetJoinedParty(partyId), event2.SubjectMemberId, event2.StringData);
                    }
                    continue;
                }
                case BattleNet.PartyListenerEventType.PARTY_ATTRIBUTE_CHANGED:
                    goto Label_054A;

                case BattleNet.PartyListenerEventType.MEMBER_ATTRIBUTE_CHANGED:
                {
                    continue;
                }
                default:
                {
                    continue;
                }
            }
            if (error.IsOperationCallback && (error.FeatureEvent == BnetFeatureEvent.Party_Create_Callback))
            {
                PartyType partyType = error.PartyType;
                if (s_pendingPartyCreates.ContainsKey(partyType))
                {
                    s_pendingPartyCreates.Remove(partyType);
                }
            }
        Label_0136:
            if (error.ErrorCode != 0)
            {
                RaisePartyError(error);
            }
            continue;
        Label_01B1:
            if ((str == "default") && (s_pendingPartyCreates.Count == 0))
            {
                callback = s_pendingPartyCreates.First<KeyValuePair<PartyType, CreateSuccessCallback>>().Value;
                s_pendingPartyCreates.Clear();
            }
        Label_01F0:
            if (callback != null)
            {
                callback(partyTypeFromString, partyId);
            }
        Label_0201:
            if (OnJoined != null)
            {
                LeaveReason? nullable5 = null;
                OnJoined(OnlineEventType.ADDED, new PartyInfo(partyId, partyTypeFromString), nullable5);
            }
            continue;
        Label_022D:
            if (s_joinedParties.ContainsKey(partyId))
            {
                PartyType type = s_joinedParties[partyId];
                s_joinedParties.Remove(partyId);
                if (OnJoined != null)
                {
                    OnJoined(OnlineEventType.REMOVED, new PartyInfo(partyId, type), new LeaveReason?((LeaveReason) event2.UintData));
                }
            }
            continue;
        Label_0284:
            if (OnPrivacyLevelChanged != null)
            {
                OnPrivacyLevelChanged(GetJoinedParty(partyId), (PrivacyLevel) event2.UintData);
            }
            continue;
        Label_02AA:
            if (OnMemberEvent != null)
            {
                OnlineEventType evt = (event2.Type != BattleNet.PartyListenerEventType.MEMBER_JOINED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
                LeaveReason? nullable = null;
                if (event2.Type == BattleNet.PartyListenerEventType.MEMBER_LEFT)
                {
                    nullable = new LeaveReason?((LeaveReason) event2.UintData);
                }
                OnMemberEvent(evt, GetJoinedParty(partyId), event2.SubjectMemberId, false, nullable);
            }
            continue;
        Label_0344:
            if (OnReceivedInvite != null)
            {
                OnlineEventType type5 = (event2.Type != BattleNet.PartyListenerEventType.RECEIVED_INVITE_ADDED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
                PartyType dEFAULT = PartyType.DEFAULT;
                if (event2.StringData != null)
                {
                    EnumUtils.TryGetEnum<PartyType>(event2.StringData, out dEFAULT);
                }
                PartyInfo party = new PartyInfo(partyId, dEFAULT);
                InviteRemoveReason? nullable2 = null;
                if (event2.Type == BattleNet.PartyListenerEventType.RECEIVED_INVITE_REMOVED)
                {
                    nullable2 = new InviteRemoveReason?((InviteRemoveReason) event2.UintData);
                }
                OnReceivedInvite(type5, party, event2.UlongData, nullable2);
            }
            continue;
        Label_03CF:
            if (OnSentInvite != null)
            {
                bool senderIsMyself = event2.SubjectMemberId == BnetPresenceMgr.Get().GetMyGameAccountId();
                OnlineEventType type7 = (event2.Type != BattleNet.PartyListenerEventType.PARTY_INVITE_SENT) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
                PartyType outVal = PartyType.DEFAULT;
                if (event2.StringData != null)
                {
                    EnumUtils.TryGetEnum<PartyType>(event2.StringData, out outVal);
                }
                PartyInfo info2 = new PartyInfo(partyId, outVal);
                InviteRemoveReason? nullable3 = null;
                if (event2.Type == BattleNet.PartyListenerEventType.PARTY_INVITE_REMOVED)
                {
                    nullable3 = new InviteRemoveReason?((InviteRemoveReason) event2.UintData);
                }
                OnSentInvite(type7, info2, event2.UlongData, senderIsMyself, nullable3);
            }
            continue;
        Label_0476:
            if (OnSentInvite != null)
            {
                OnlineEventType type9 = (event2.Type != BattleNet.PartyListenerEventType.INVITE_REQUEST_ADDED) ? OnlineEventType.REMOVED : OnlineEventType.ADDED;
                PartyInfo joinedParty = GetJoinedParty(partyId);
                InviteRequestRemovedReason? nullable4 = null;
                if (event2.Type == BattleNet.PartyListenerEventType.INVITE_REQUEST_REMOVED)
                {
                    nullable4 = new InviteRequestRemovedReason?((InviteRequestRemovedReason) event2.UintData);
                }
                InviteRequest request = new InviteRequest {
                    TargetId = event2.TargetMemberId,
                    TargetName = event2.StringData2,
                    RequesterId = event2.SubjectMemberId,
                    RequesterName = event2.StringData
                };
                OnReceivedInviteRequest(type9, joinedParty, request, nullable4);
            }
            continue;
        Label_054A:
            info4 = GetJoinedParty(partyId);
            string stringData = event2.StringData;
            if (stringData == "WTCG.Party.Type")
            {
                PartyType type10 = GetPartyTypeFromString(GetPartyAttributeString(partyId, "WTCG.Party.Type"));
                if (type10 != PartyType.DEFAULT)
                {
                    s_joinedParties[partyId] = type10;
                }
            }
            bnet.protocol.attribute.Variant attributeValue = new bnet.protocol.attribute.Variant();
            switch (event2.UintData)
            {
                case 1:
                    attributeValue.IntValue = (long) event2.UlongData;
                    break;

                case 2:
                    attributeValue.StringValue = event2.StringData2;
                    break;

                case 3:
                    attributeValue.BlobValue = event2.BlobData;
                    break;
            }
            if (OnPartyAttributeChanged != null)
            {
                OnPartyAttributeChanged(info4, stringData, attributeValue);
            }
            if ((s_attributeChangedSubscribers != null) && s_attributeChangedSubscribers.TryGetValue(stringData, out list))
            {
                foreach (PartyAttributeChangedHandler handler in list.ToArray())
                {
                    handler(info4, stringData, attributeValue);
                }
            }
        }
    }

    private static void RaisePartyError(PartyError error)
    {
        object[] args = new object[] { error.FeatureEvent.ToString(), (int) error.FeatureEvent, error.ErrorCode, error.PartyId, error.szPartyType, error.StringData };
        string message = string.Format("BnetParty: event={0} feature={1} code={2} partyId={3} type={4} strData={5}", args);
        LogLevel level = (error.ErrorCode != 0) ? LogLevel.Error : LogLevel.Info;
        Log.Party.Print(level, false, message);
        if (!Log.Party.CanPrint(LogTarget.CONSOLE, level, false))
        {
            if (level == LogLevel.Error)
            {
                Debug.LogError(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
        if (OnError != null)
        {
            OnError(error);
        }
    }

    private static void RaisePartyError(bool isOperationCallback, string szPartyType, BnetFeatureEvent featureEvent, bnet.Error errorCode, string errorMessageFormat, params object[] args)
    {
        string str = string.Format(errorMessageFormat, args);
        PartyError error = new PartyError {
            IsOperationCallback = isOperationCallback,
            DebugContext = str,
            ErrorCode = errorCode,
            Feature = BnetFeature.Party,
            FeatureEvent = featureEvent,
            szPartyType = szPartyType
        };
        RaisePartyError(error);
    }

    private static void RaisePartyError(bool isOperationCallback, string debugContext, bnet.Error errorCode, BnetFeature feature, BnetFeatureEvent featureEvent, PartyId partyId, string szPartyType, string stringData, string errorMessageFormat, params object[] args)
    {
        PartyError error = new PartyError {
            IsOperationCallback = isOperationCallback,
            DebugContext = debugContext,
            ErrorCode = errorCode,
            Feature = feature,
            FeatureEvent = featureEvent,
            PartyId = partyId,
            szPartyType = szPartyType,
            StringData = stringData
        };
        RaisePartyError(error);
    }

    public static void RegisterAttributeChangedHandler(string attributeKey, PartyAttributeChangedHandler handler)
    {
        List<PartyAttributeChangedHandler> list;
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
        if (s_attributeChangedSubscribers == null)
        {
            s_attributeChangedSubscribers = new Map<string, List<PartyAttributeChangedHandler>>();
        }
        if (!s_attributeChangedSubscribers.TryGetValue(attributeKey, out list))
        {
            list = new List<PartyAttributeChangedHandler>();
            s_attributeChangedSubscribers[attributeKey] = list;
        }
        if (!list.Contains(handler))
        {
            list.Add(handler);
        }
    }

    public static void RemoveFromAllEventHandlers(object targetObject)
    {
        System.Type type = (targetObject != null) ? targetObject.GetType() : null;
        if (OnError != null)
        {
            IEnumerator enumerator = (OnError.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Delegate current = (Delegate) enumerator.Current;
                    if ((current.Target == targetObject) || ((current.Target == null) && (current.Method.DeclaringType == type)))
                    {
                        OnError = (PartyErrorHandler) Delegate.Remove(OnError, (PartyErrorHandler) current);
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
        }
        if (OnJoined != null)
        {
            IEnumerator enumerator2 = (OnJoined.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    Delegate delegate3 = (Delegate) enumerator2.Current;
                    if ((delegate3.Target == targetObject) || ((delegate3.Target == null) && (delegate3.Method.DeclaringType == type)))
                    {
                        OnJoined = (JoinedHandler) Delegate.Remove(OnJoined, (JoinedHandler) delegate3);
                    }
                }
            }
            finally
            {
                IDisposable disposable2 = enumerator2 as IDisposable;
                if (disposable2 == null)
                {
                }
                disposable2.Dispose();
            }
        }
        if (OnPrivacyLevelChanged != null)
        {
            IEnumerator enumerator3 = (OnPrivacyLevelChanged.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    Delegate delegate4 = (Delegate) enumerator3.Current;
                    if ((delegate4.Target == targetObject) || ((delegate4.Target == null) && (delegate4.Method.DeclaringType == type)))
                    {
                        OnPrivacyLevelChanged = (PrivacyLevelChangedHandler) Delegate.Remove(OnPrivacyLevelChanged, (PrivacyLevelChangedHandler) delegate4);
                    }
                }
            }
            finally
            {
                IDisposable disposable3 = enumerator3 as IDisposable;
                if (disposable3 == null)
                {
                }
                disposable3.Dispose();
            }
        }
        if (OnMemberEvent != null)
        {
            IEnumerator enumerator4 = (OnMemberEvent.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator4.MoveNext())
                {
                    Delegate delegate5 = (Delegate) enumerator4.Current;
                    if ((delegate5.Target == targetObject) || ((delegate5.Target == null) && (delegate5.Method.DeclaringType == type)))
                    {
                        OnMemberEvent = (MemberEventHandler) Delegate.Remove(OnMemberEvent, (MemberEventHandler) delegate5);
                    }
                }
            }
            finally
            {
                IDisposable disposable4 = enumerator4 as IDisposable;
                if (disposable4 == null)
                {
                }
                disposable4.Dispose();
            }
        }
        if (OnReceivedInvite != null)
        {
            IEnumerator enumerator5 = (OnReceivedInvite.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator5.MoveNext())
                {
                    Delegate delegate6 = (Delegate) enumerator5.Current;
                    if ((delegate6.Target == targetObject) || ((delegate6.Target == null) && (delegate6.Method.DeclaringType == type)))
                    {
                        OnReceivedInvite = (ReceivedInviteHandler) Delegate.Remove(OnReceivedInvite, (ReceivedInviteHandler) delegate6);
                    }
                }
            }
            finally
            {
                IDisposable disposable5 = enumerator5 as IDisposable;
                if (disposable5 == null)
                {
                }
                disposable5.Dispose();
            }
        }
        if (OnSentInvite != null)
        {
            IEnumerator enumerator6 = (OnSentInvite.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator6.MoveNext())
                {
                    Delegate delegate7 = (Delegate) enumerator6.Current;
                    if ((delegate7.Target == targetObject) || ((delegate7.Target == null) && (delegate7.Method.DeclaringType == type)))
                    {
                        OnSentInvite = (SentInviteHandler) Delegate.Remove(OnSentInvite, (SentInviteHandler) delegate7);
                    }
                }
            }
            finally
            {
                IDisposable disposable6 = enumerator6 as IDisposable;
                if (disposable6 == null)
                {
                }
                disposable6.Dispose();
            }
        }
        if (OnReceivedInviteRequest != null)
        {
            IEnumerator enumerator7 = (OnReceivedInviteRequest.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator7.MoveNext())
                {
                    Delegate delegate8 = (Delegate) enumerator7.Current;
                    if ((delegate8.Target == targetObject) || ((delegate8.Target == null) && (delegate8.Method.DeclaringType == type)))
                    {
                        OnReceivedInviteRequest = (ReceivedInviteRequestHandler) Delegate.Remove(OnReceivedInviteRequest, (ReceivedInviteRequestHandler) delegate8);
                    }
                }
            }
            finally
            {
                IDisposable disposable7 = enumerator7 as IDisposable;
                if (disposable7 == null)
                {
                }
                disposable7.Dispose();
            }
        }
        if (OnChatMessage != null)
        {
            IEnumerator enumerator8 = (OnChatMessage.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator8.MoveNext())
                {
                    Delegate delegate9 = (Delegate) enumerator8.Current;
                    if ((delegate9.Target == targetObject) || ((delegate9.Target == null) && (delegate9.Method.DeclaringType == type)))
                    {
                        OnChatMessage = (ChatMessageHandler) Delegate.Remove(OnChatMessage, (ChatMessageHandler) delegate9);
                    }
                }
            }
            finally
            {
                IDisposable disposable8 = enumerator8 as IDisposable;
                if (disposable8 == null)
                {
                }
                disposable8.Dispose();
            }
        }
        if (OnPartyAttributeChanged != null)
        {
            IEnumerator enumerator9 = (OnPartyAttributeChanged.GetInvocationList().Clone() as Array).GetEnumerator();
            try
            {
                while (enumerator9.MoveNext())
                {
                    Delegate delegate10 = (Delegate) enumerator9.Current;
                    if ((delegate10.Target == targetObject) || ((delegate10.Target == null) && (delegate10.Method.DeclaringType == type)))
                    {
                        OnPartyAttributeChanged = (PartyAttributeChangedHandler) Delegate.Remove(OnPartyAttributeChanged, (PartyAttributeChangedHandler) delegate10);
                    }
                }
            }
            finally
            {
                IDisposable disposable9 = enumerator9 as IDisposable;
                if (disposable9 == null)
                {
                }
                disposable9.Dispose();
            }
        }
        if (s_attributeChangedSubscribers != null)
        {
            foreach (KeyValuePair<string, List<PartyAttributeChangedHandler>> pair in s_attributeChangedSubscribers)
            {
                if (<>f__am$cacheE == null)
                {
                    <>f__am$cacheE = (h, idx) => new <>__AnonType1<PartyAttributeChangedHandler, int>(h, idx);
                }
                foreach (<>__AnonType1<PartyAttributeChangedHandler, int> type2 in Enumerable.Select<PartyAttributeChangedHandler, <>__AnonType1<PartyAttributeChangedHandler, int>>(pair.Value, <>f__am$cacheE).ToArray<<>__AnonType1<PartyAttributeChangedHandler, int>>())
                {
                    if ((type2.Handler.Target == targetObject) || (type2.Handler.Method.DeclaringType == type))
                    {
                        pair.Value.RemoveAt(type2.Index);
                    }
                }
            }
        }
    }

    public static void RequestInvite(PartyId partyId, BnetGameAccountId whomToAskForApproval, BnetGameAccountId whomToInvite, PartyType partyType)
    {
        if (IsLeader(partyId))
        {
            PartyError error = new PartyError {
                IsOperationCallback = true,
                DebugContext = "RequestInvite",
                ErrorCode = 20,
                Feature = BnetFeature.Party,
                FeatureEvent = BnetFeatureEvent.Party_RequestPartyInvite_Callback,
                PartyId = partyId,
                szPartyType = EnumUtils.GetString<PartyType>(partyType),
                StringData = "leaders cannot RequestInvite - use SendInvite instead."
            };
            RaisePartyError(error);
        }
        else
        {
            BattleNet.DllEntityId id = partyId.ToDllEntityId();
            BattleNet.DllEntityId id2 = BnetEntityId.CreateForDll(whomToAskForApproval);
            BattleNet.DllEntityId id3 = BnetEntityId.CreateForDll(whomToInvite);
            string szPartyType = EnumUtils.GetString<PartyType>(partyType);
            BattleNet.RequestPartyInvite(id, id2, id3, szPartyType);
        }
    }

    public static void RevokeSentInvite(PartyId partyId, ulong inviteId)
    {
        if (IsInParty(partyId))
        {
            BattleNet.RevokePartyInvite(partyId.ToDllEntityId(), inviteId);
        }
    }

    public static void SendChatMessage(PartyId partyId, string chatMessage)
    {
        if (IsInParty(partyId))
        {
            BattleNet.SendPartyChatMessage(partyId.ToDllEntityId(), chatMessage);
        }
    }

    public static void SendInvite(PartyId toWhichPartyId, BnetGameAccountId recipientId)
    {
        if (IsInParty(toWhichPartyId))
        {
            BattleNet.DllEntityId partyId = toWhichPartyId.ToDllEntityId();
            BattleNet.DllEntityId inviteeId = BnetEntityId.CreateForDll(recipientId);
            BattleNet.SendPartyInvite(partyId, inviteeId, false);
        }
    }

    public static void SetLeader(PartyId partyId, BnetGameAccountId memberId)
    {
        if (IsInParty(partyId))
        {
            BattleNet.DllEntityId id = partyId.ToDllEntityId();
            BattleNet.DllEntityId id2 = BnetEntityId.CreateForDll(memberId);
            uint leaderRoleId = PartyMember.GetLeaderRoleId(GetPartyType(partyId));
            BattleNet.AssignPartyRole(id, id2, leaderRoleId);
        }
    }

    public static void SetPartyAttributeBlob(PartyId partyId, string attributeKey, byte[] value)
    {
        BattleNet.SetPartyAttributeBlob(partyId.ToDllEntityId(), attributeKey, value);
    }

    public static void SetPartyAttributeLong(PartyId partyId, string attributeKey, long value)
    {
        BattleNet.SetPartyAttributeLong(partyId.ToDllEntityId(), attributeKey, value);
    }

    public static void SetPartyAttributeString(PartyId partyId, string attributeKey, string value)
    {
        BattleNet.SetPartyAttributeString(partyId.ToDllEntityId(), attributeKey, value);
    }

    public static void SetPrivacy(PartyId partyId, PrivacyLevel privacyLevel)
    {
        if (IsInParty(partyId))
        {
            BattleNet.SetPartyPrivacy(partyId.ToDllEntityId(), (int) privacyLevel);
        }
    }

    public static bool UnregisterAttributeChangedHandler(string attributeKey, PartyAttributeChangedHandler handler)
    {
        List<PartyAttributeChangedHandler> list;
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
        if (s_attributeChangedSubscribers == null)
        {
            return false;
        }
        return (s_attributeChangedSubscribers.TryGetValue(attributeKey, out list) && list.Remove(handler));
    }

    [CompilerGenerated]
    private sealed class <GetReceivedInvite>c__AnonStorey392
    {
        internal ulong inviteId;

        internal bool <>m__297(PartyInvite i)
        {
            return (i.InviteId == this.inviteId);
        }
    }

    [CompilerGenerated]
    private sealed class <GetReceivedInviteFrom>c__AnonStorey393
    {
        internal BnetGameAccountId inviterId;
        internal PartyType partyType;

        internal bool <>m__298(PartyInvite i)
        {
            return ((i.InviterId == this.inviterId) && (i.PartyType == this.partyType));
        }
    }

    [CompilerGenerated]
    private sealed class <GetSentInvite>c__AnonStorey394
    {
        internal ulong inviteId;

        internal bool <>m__299(PartyInvite i)
        {
            return (i.InviteId == this.inviteId);
        }
    }

    public delegate void ChatMessageHandler(PartyInfo party, BnetGameAccountId speakerId, string chatMessage);

    public delegate void CreateSuccessCallback(PartyType type, PartyId newlyCreatedPartyId);

    public enum FriendlyGameRoleSet
    {
        Invitee = 2,
        Inviter = 1
    }

    public delegate void JoinedHandler(OnlineEventType evt, PartyInfo party, LeaveReason? reason);

    public delegate void MemberEventHandler(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason);

    public delegate void PartyAttributeChangedHandler(PartyInfo party, string attributeKey, bnet.protocol.attribute.Variant attributeValue);

    public delegate void PartyErrorHandler(PartyError error);

    public delegate void PrivacyLevelChangedHandler(PartyInfo party, PrivacyLevel newPrivacyLevel);

    public delegate void ReceivedInviteHandler(OnlineEventType evt, PartyInfo party, ulong inviteId, InviteRemoveReason? reason);

    public delegate void ReceivedInviteRequestHandler(OnlineEventType evt, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason);

    public delegate void SentInviteHandler(OnlineEventType evt, PartyInfo party, ulong inviteId, bool senderIsMyself, InviteRemoveReason? reason);

    public enum SpectatorPartyRoleSet
    {
        Leader = 2,
        Member = 1
    }
}

