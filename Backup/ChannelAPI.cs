using bnet.protocol;
using bnet.protocol.attribute;
using bnet.protocol.channel;
using bnet.protocol.channel_invitation;
using bnet.protocol.invitation;
using bnet.protocol.presence;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class ChannelAPI : BattleNetAPI
{
    [CompilerGenerated]
    private static Func<List<ReceivedInvite>, IEnumerable<ReceivedInvite>> <>f__am$cacheA;
    private Map<ulong, ChannelReferenceObject> m_activeChannels;
    private Map<EntityId, ulong> m_channelEntityObjectMap;
    private static ServiceDescriptor m_channelInvitationNotifyService = new RPCServices.ChannelInvitationNotifyService();
    private static ServiceDescriptor m_channelInvitationService = new RPCServices.ChannelInvitationService();
    private ServiceDescriptor m_channelOwnerService;
    private ServiceDescriptor m_channelService;
    private ServiceDescriptor m_channelSubscriberService;
    private Map<InvitationServiceType, List<ReceivedInvite>> m_receivedInvitations;
    private Map<EntityId, List<Suggestion>> m_receivedInviteRequests;
    private static ulong s_nextObjectId = 0L;

    public ChannelAPI(BattleNetCSharp battlenet) : base(battlenet, "Channel")
    {
        this.m_activeChannels = new Map<ulong, ChannelReferenceObject>();
        this.m_channelEntityObjectMap = new Map<EntityId, ulong>();
        this.m_receivedInvitations = new Map<InvitationServiceType, List<ReceivedInvite>>();
        this.m_channelService = new RPCServices.ChannelService();
        this.m_channelSubscriberService = new RPCServices.ChannelSubscriberService();
        this.m_channelOwnerService = new RPCServices.ChannelOwnerService();
    }

    public void AcceptInvitation(ulong invitationId, EntityId channelId, ChannelType channelType, RPCContextDelegate callback = null)
    {
        <AcceptInvitation>c__AnonStorey304 storey = new <AcceptInvitation>c__AnonStorey304 {
            callback = callback
        };
        AcceptInvitationRequest message = new AcceptInvitationRequest();
        message.SetInvitationId(invitationId);
        message.SetObjectId(GetNextObjectId());
        storey.channelData = new ChannelData(this, channelId, 0L, channelType);
        storey.channelData.SetSubscriberObjectId(message.ObjectId);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 4, message, new RPCContextDelegate(storey.<>m__11E), 0);
    }

    public void AddActiveChannel(ulong objectId, ChannelReferenceObject channelRefObject)
    {
        this.m_activeChannels.Add(objectId, channelRefObject);
        this.m_channelEntityObjectMap[channelRefObject.m_channelData.m_channelId] = objectId;
    }

    public void DeclineInvitation(ulong invitationId, EntityId channelId, RPCContextDelegate callback)
    {
        GenericRequest message = new GenericRequest();
        message.SetInvitationId(invitationId);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 5, message, callback, 0);
    }

    public ReceivedInvite[] GetAllReceivedInvites()
    {
        if (<>f__am$cacheA == null)
        {
            <>f__am$cacheA = l => l;
        }
        return Enumerable.SelectMany<List<ReceivedInvite>, ReceivedInvite>(this.m_receivedInvitations.Values, <>f__am$cacheA).ToArray<ReceivedInvite>();
    }

    public ChannelReferenceObject GetChannelReferenceObject(EntityId entityId)
    {
        ulong num;
        ChannelReferenceObject obj2;
        if (this.m_channelEntityObjectMap.TryGetValue(entityId, out num) && this.m_activeChannels.TryGetValue(num, out obj2))
        {
            return obj2;
        }
        return null;
    }

    public ChannelReferenceObject GetChannelReferenceObject(ulong objectId)
    {
        ChannelReferenceObject obj2;
        if (this.m_activeChannels.TryGetValue(objectId, out obj2))
        {
            return obj2;
        }
        return null;
    }

    public Suggestion[] GetInviteRequests(EntityId channelId)
    {
        Suggestion[] suggestionArray = null;
        List<Suggestion> list;
        if ((this.m_receivedInviteRequests != null) && this.m_receivedInviteRequests.TryGetValue(channelId, out list))
        {
            suggestionArray = list.ToArray();
        }
        if (suggestionArray == null)
        {
            suggestionArray = new Suggestion[0];
        }
        return suggestionArray;
    }

    public static ulong GetNextObjectId()
    {
        return (s_nextObjectId += ((ulong) 1L));
    }

    public ReceivedInvite GetReceivedInvite(InvitationServiceType serviceType, ulong invitationId)
    {
        foreach (ReceivedInvite invite in this.GetReceivedInvites(serviceType))
        {
            if (invite.Invitation.Id == invitationId)
            {
                return invite;
            }
        }
        return null;
    }

    public ReceivedInvite[] GetReceivedInvites(InvitationServiceType serviceType)
    {
        List<ReceivedInvite> list;
        this.m_receivedInvitations.TryGetValue(serviceType, out list);
        return ((list != null) ? list.ToArray() : new ReceivedInvite[0]);
    }

    private void HandleChannelInvitation_NotifyHasRoomForInvitation(RPCContext context)
    {
        base.ApiLog.LogDebug("HandleChannelInvitation_NotifyHasRoomForInvitation");
    }

    private void HandleChannelInvitation_NotifyReceivedInvitationAdded(RPCContext context)
    {
        base.ApiLog.LogDebug("HandleChannelInvitation_NotifyReceivedInvitationAdded");
        InvitationAddedNotification notification = InvitationAddedNotification.ParseFrom(context.Payload);
        if (notification.Invitation.HasChannelInvitation)
        {
            List<ReceivedInvite> list;
            ChannelInvitation channelInvitation = notification.Invitation.ChannelInvitation;
            InvitationServiceType serviceType = (InvitationServiceType) channelInvitation.ServiceType;
            if (!this.m_receivedInvitations.TryGetValue(serviceType, out list))
            {
                list = new List<ReceivedInvite>();
                this.m_receivedInvitations[serviceType] = list;
            }
            list.Add(new ReceivedInvite(channelInvitation, notification.Invitation));
            switch (serviceType)
            {
                case InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY:
                {
                    base.m_battleNet.Party.ReceivedInvitationAdded(notification, channelInvitation);
                }
            }
        }
    }

    private void HandleChannelInvitation_NotifyReceivedInvitationRemoved(RPCContext context)
    {
        base.ApiLog.LogDebug("HandleChannelInvitation_NotifyReceivedInvitationRemoved");
        InvitationRemovedNotification notification = InvitationRemovedNotification.ParseFrom(context.Payload);
        if (notification.Invitation.HasChannelInvitation)
        {
            List<ReceivedInvite> list;
            ChannelInvitation channelInvitation = notification.Invitation.ChannelInvitation;
            InvitationServiceType serviceType = (InvitationServiceType) channelInvitation.ServiceType;
            ulong id = notification.Invitation.Id;
            string szPartyType = string.Empty;
            if (serviceType == InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY)
            {
                szPartyType = base.m_battleNet.Party.GetReceivedInvitationPartyType(id);
            }
            if (this.m_receivedInvitations.TryGetValue(serviceType, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Invitation.Id == id)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
                if (list.Count == 0)
                {
                    this.m_receivedInvitations.Remove(serviceType);
                }
            }
            switch (serviceType)
            {
                case InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY:
                {
                    base.m_battleNet.Party.ReceivedInvitationRemoved(szPartyType, notification, channelInvitation);
                }
            }
        }
    }

    private void HandleChannelInvitation_NotifyReceivedSuggestionAdded(RPCContext context)
    {
        SuggestionAddedNotification notification = SuggestionAddedNotification.ParseFrom(context.Payload);
        EntityId entityId = !notification.Suggestion.HasChannelId ? null : notification.Suggestion.ChannelId;
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(entityId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelInvitation_NotifyReceivedSuggestionAdded had unexpected traffic for channelId: " + entityId);
        }
        else
        {
            List<Suggestion> list;
            base.ApiLog.LogDebug("HandleChannelInvitation_NotifyReceivedSuggestionAdded: " + notification);
            if (this.m_receivedInviteRequests == null)
            {
                this.m_receivedInviteRequests = new Map<EntityId, List<Suggestion>>();
            }
            if (!this.m_receivedInviteRequests.TryGetValue(entityId, out list))
            {
                list = new List<Suggestion>();
                this.m_receivedInviteRequests[entityId] = list;
            }
            if (list.IndexOf(notification.Suggestion) < 0)
            {
                list.Add(notification.Suggestion);
            }
            if (channelReferenceObject.m_channelData.m_channelType == ChannelType.PARTY_CHANNEL)
            {
                base.m_battleNet.Party.ReceivedInviteRequestDelta(entityId, notification.Suggestion, null);
            }
        }
    }

    private void HandleChannelSubscriber_NotifyAdd(RPCContext context)
    {
        AddNotification notification = AddNotification.ParseFrom(context.Payload);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyAdd had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else
        {
            base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyAdd: " + notification);
            ChannelType channelType = channelReferenceObject.m_channelData.m_channelType;
            switch (channelType)
            {
                case ChannelType.PRESENCE_CHANNEL:
                    if (notification.ChannelState.HasPresence)
                    {
                        bnet.protocol.presence.ChannelState presence = notification.ChannelState.Presence;
                        base.m_battleNet.Presence.HandlePresenceUpdates(presence, channelReferenceObject);
                    }
                    break;

                default:
                {
                    ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
                    if (channelData != null)
                    {
                        channelData.m_channelState = notification.ChannelState;
                        foreach (Member member in notification.MemberList)
                        {
                            EntityId gameAccountId = member.Identity.GameAccountId;
                            channelData.m_members.Add(gameAccountId, member);
                            if (!base.m_battleNet.GameAccountId.Equals(gameAccountId))
                            {
                                base.m_battleNet.Presence.PresenceSubscribe(member.Identity.GameAccountId);
                            }
                        }
                    }
                    break;
                }
            }
            if (channelType == ChannelType.PARTY_CHANNEL)
            {
                base.m_battleNet.Party.PartyJoined(channelReferenceObject, notification);
            }
        }
    }

    private void HandleChannelSubscriber_NotifyJoin(RPCContext context)
    {
        JoinNotification notification = JoinNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyJoin: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyJoin had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else
        {
            ChannelType channelType = channelReferenceObject.m_channelData.m_channelType;
            switch (channelType)
            {
                case ChannelType.PRESENCE_CHANNEL:
                    break;

                default:
                {
                    ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
                    if (channelData != null)
                    {
                        EntityId gameAccountId = notification.Member.Identity.GameAccountId;
                        channelData.m_members.Add(gameAccountId, notification.Member);
                        if (!base.m_battleNet.GameAccountId.Equals(gameAccountId))
                        {
                            base.m_battleNet.Presence.PresenceSubscribe(notification.Member.Identity.GameAccountId);
                        }
                    }
                    break;
                }
            }
            if (channelType == ChannelType.PARTY_CHANNEL)
            {
                base.m_battleNet.Party.PartyMemberJoined(channelReferenceObject, notification);
            }
        }
    }

    private void HandleChannelSubscriber_NotifyLeave(RPCContext context)
    {
        LeaveNotification notification = LeaveNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyLeave: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyLeave had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else
        {
            switch (channelReferenceObject.m_channelData.m_channelType)
            {
                case ChannelType.CHAT_CHANNEL:
                case ChannelType.GAME_CHANNEL:
                    break;

                case ChannelType.PARTY_CHANNEL:
                    base.m_battleNet.Party.PartyMemberLeft(channelReferenceObject, notification);
                    break;

                default:
                    return;
            }
            ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
            if (channelData != null)
            {
                channelData.m_members.Remove(notification.MemberId);
                if (!base.m_battleNet.GameAccountId.Equals(notification.MemberId))
                {
                    base.m_battleNet.Presence.PresenceUnsubscribe(notification.MemberId);
                }
            }
        }
    }

    private void HandleChannelSubscriber_NotifyRemove(RPCContext context)
    {
        RemoveNotification notification = RemoveNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyRemove: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyRemove had unexpected traffic for objectId : " + context.Header.ObjectId);
            return;
        }
        switch (channelReferenceObject.m_channelData.m_channelType)
        {
            case ChannelType.CHAT_CHANNEL:
                break;

            case ChannelType.PARTY_CHANNEL:
                base.m_battleNet.Party.PartyLeft(channelReferenceObject, notification);
                break;

            case ChannelType.GAME_CHANNEL:
                base.m_battleNet.Games.GameLeft(channelReferenceObject, notification);
                break;

            default:
                goto Label_0144;
        }
        ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
        if (channelData != null)
        {
            foreach (Member member in channelData.m_members.Values)
            {
                if (!base.m_battleNet.GameAccountId.Equals(member.Identity.GameAccountId))
                {
                    base.m_battleNet.Presence.PresenceUnsubscribe(member.Identity.GameAccountId);
                }
            }
        }
    Label_0144:
        this.RemoveActiveChannel(context.Header.ObjectId);
    }

    private void HandleChannelSubscriber_NotifySendMessage(RPCContext context)
    {
        SendMessageNotification notification = SendMessageNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifySendMessage: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifySendMessage had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else if (channelReferenceObject.m_channelData.m_channelType == ChannelType.PARTY_CHANNEL)
        {
            base.m_battleNet.Party.PartyMessageReceived(channelReferenceObject, notification);
        }
    }

    private void HandleChannelSubscriber_NotifyUpdateChannelState(RPCContext context)
    {
        UpdateChannelStateNotification notification = UpdateChannelStateNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyUpdateChannelState: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyUpdateChannelState had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else
        {
            ChannelType channelType = channelReferenceObject.m_channelData.m_channelType;
            switch (channelType)
            {
                case ChannelType.PRESENCE_CHANNEL:
                    if (notification.StateChange.HasPresence)
                    {
                        bnet.protocol.presence.ChannelState presence = notification.StateChange.Presence;
                        base.m_battleNet.Presence.HandlePresenceUpdates(presence, channelReferenceObject);
                    }
                    return;

                case ChannelType.CHAT_CHANNEL:
                case ChannelType.GAME_CHANNEL:
                    break;

                case ChannelType.PARTY_CHANNEL:
                    base.m_battleNet.Party.PreprocessPartyChannelUpdated(channelReferenceObject, notification);
                    break;

                default:
                    return;
            }
            ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
            if (channelData != null)
            {
                bool flag = channelType == ChannelType.PARTY_CHANNEL;
                bool flag2 = false;
                Map<string, bnet.protocol.attribute.Variant> map = null;
                bnet.protocol.channel.ChannelState channelState = channelData.m_channelState;
                bnet.protocol.channel.ChannelState stateChange = notification.StateChange;
                if (stateChange.HasMaxMembers)
                {
                    channelState.MaxMembers = stateChange.MaxMembers;
                }
                if (stateChange.HasMinMembers)
                {
                    channelState.MinMembers = stateChange.MinMembers;
                }
                if (stateChange.HasMaxInvitations)
                {
                    channelState.MaxInvitations = stateChange.MaxInvitations;
                }
                if (stateChange.HasPrivacyLevel && (channelState.PrivacyLevel != stateChange.PrivacyLevel))
                {
                    channelState.PrivacyLevel = stateChange.PrivacyLevel;
                    flag2 = true;
                }
                if (stateChange.HasName)
                {
                    channelState.Name = stateChange.Name;
                }
                if (stateChange.HasDelegateName)
                {
                    channelState.DelegateName = stateChange.DelegateName;
                }
                if (stateChange.HasChannelType)
                {
                    if (!flag)
                    {
                        channelState.ChannelType = stateChange.ChannelType;
                    }
                    if (flag && (stateChange.ChannelType != PartyAPI.PARTY_TYPE_DEFAULT))
                    {
                        channelState.ChannelType = stateChange.ChannelType;
                        int num = -1;
                        for (int j = 0; j < channelState.AttributeList.Count; j++)
                        {
                            if (channelState.AttributeList[j].Name == "WTCG.Party.Type")
                            {
                                num = j;
                                break;
                            }
                        }
                        bnet.protocol.attribute.Attribute item = ProtocolHelper.CreateAttribute("WTCG.Party.Type", channelState.ChannelType);
                        if (num >= 0)
                        {
                            channelState.AttributeList[num] = item;
                        }
                        else
                        {
                            channelState.AttributeList.Add(item);
                        }
                    }
                }
                if (stateChange.HasProgram)
                {
                    channelState.Program = stateChange.Program;
                }
                if (stateChange.HasAllowOfflineMembers)
                {
                    channelState.AllowOfflineMembers = stateChange.AllowOfflineMembers;
                }
                if (stateChange.HasSubscribeToPresence)
                {
                    channelState.SubscribeToPresence = stateChange.SubscribeToPresence;
                }
                if ((stateChange.AttributeCount > 0) && (map == null))
                {
                    map = new Map<string, bnet.protocol.attribute.Variant>();
                }
                for (int i = 0; i < stateChange.AttributeCount; i++)
                {
                    bnet.protocol.attribute.Attribute val = stateChange.AttributeList[i];
                    int index = -1;
                    for (int k = 0; k < channelState.AttributeList.Count; k++)
                    {
                        bnet.protocol.attribute.Attribute attribute3 = channelState.AttributeList[k];
                        if (attribute3.Name == val.Name)
                        {
                            index = k;
                            break;
                        }
                    }
                    if (val.Value.IsNone())
                    {
                        if (index >= 0)
                        {
                            channelState.AttributeList.RemoveAt(index);
                        }
                    }
                    else if (index >= 0)
                    {
                        channelState.Attribute[index] = val;
                    }
                    else
                    {
                        channelState.AddAttribute(val);
                    }
                    map.Add(val.Name, val.Value);
                }
                if (stateChange.HasReason)
                {
                    IList<Invitation> invitationList = stateChange.InvitationList;
                    IList<Invitation> list2 = channelState.InvitationList;
                    for (int m = 0; m < invitationList.Count; m++)
                    {
                        Invitation invitation = invitationList[m];
                        for (int n = 0; n < list2.Count; n++)
                        {
                            Invitation invitation2 = list2[n];
                            if (invitation2.Id == invitation.Id)
                            {
                                channelState.InvitationList.RemoveAt(n);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    channelState.Invitation.AddRange(stateChange.InvitationList);
                }
                channelData.m_channelState = channelState;
                if (flag)
                {
                    if (flag2)
                    {
                        base.m_battleNet.Party.PartyPrivacyChanged(channelData.m_channelId, channelState.PrivacyLevel);
                    }
                    if (stateChange.InvitationList.Count > 0)
                    {
                        uint? removeReason = null;
                        if (stateChange.HasReason)
                        {
                            removeReason = new uint?(stateChange.Reason);
                        }
                        foreach (Invitation invitation3 in stateChange.InvitationList)
                        {
                            base.m_battleNet.Party.PartyInvitationDelta(channelData.m_channelId, invitation3, removeReason);
                        }
                    }
                    if (map != null)
                    {
                        foreach (KeyValuePair<string, bnet.protocol.attribute.Variant> pair in map)
                        {
                            base.m_battleNet.Party.PartyAttributeChanged(channelData.m_channelId, pair.Key, pair.Value);
                        }
                    }
                }
            }
        }
    }

    private void HandleChannelSubscriber_NotifyUpdateMemberState(RPCContext context)
    {
        UpdateMemberStateNotification notification = UpdateMemberStateNotification.ParseFrom(context.Payload);
        base.ApiLog.LogDebug("HandleChannelSubscriber_NotifyUpdateMemberState: " + notification);
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(context.Header.ObjectId);
        if (channelReferenceObject == null)
        {
            base.ApiLog.LogError("HandleChannelSubscriber_NotifyUpdateMemberState had unexpected traffic for objectId : " + context.Header.ObjectId);
        }
        else
        {
            ChannelType channelType = channelReferenceObject.m_channelData.m_channelType;
            ChannelData channelData = (ChannelData) channelReferenceObject.m_channelData;
            EntityId channelId = channelData.m_channelId;
            List<EntityId> membersWithRoleChanges = null;
            for (int i = 0; i < notification.StateChangeList.Count; i++)
            {
                <HandleChannelSubscriber_NotifyUpdateMemberState>c__AnonStorey305 storey = new <HandleChannelSubscriber_NotifyUpdateMemberState>c__AnonStorey305 {
                    deltaMember = notification.StateChangeList[i]
                };
                if (!storey.deltaMember.Identity.HasGameAccountId)
                {
                    object[] args = new object[] { i, channelId.High, channelId.Low };
                    base.ApiLog.LogError("HandleChannelSubscriber_NotifyUpdateMemberState no identity/gameAccount in Member list at index={0} channelId={1}-{2}", args);
                }
                else
                {
                    EntityId gameAccountId = storey.deltaMember.Identity.GameAccountId;
                    Map<string, bnet.protocol.attribute.Variant> map = null;
                    Member deltaMember = null;
                    if (!channelData.m_members.TryGetValue(gameAccountId, out storey.cachedMember))
                    {
                        deltaMember = storey.deltaMember;
                    }
                    else
                    {
                        Member cachedMember = storey.cachedMember;
                        MemberState val = storey.cachedMember.State;
                        if (storey.deltaMember.State.AttributeCount > 0)
                        {
                            if (map == null)
                            {
                                map = new Map<string, bnet.protocol.attribute.Variant>();
                            }
                            for (int j = 0; j < storey.deltaMember.State.AttributeCount; j++)
                            {
                                bnet.protocol.attribute.Attribute attribute = storey.deltaMember.State.AttributeList[j];
                                int index = -1;
                                for (int k = 0; k < val.AttributeList.Count; k++)
                                {
                                    bnet.protocol.attribute.Attribute attribute2 = val.AttributeList[k];
                                    if (attribute2.Name == attribute.Name)
                                    {
                                        index = k;
                                        break;
                                    }
                                }
                                if (attribute.Value.IsNone())
                                {
                                    if (index >= 0)
                                    {
                                        val.AttributeList.RemoveAt(index);
                                    }
                                }
                                else if (index >= 0)
                                {
                                    val.Attribute[index] = attribute;
                                }
                                else
                                {
                                    val.AddAttribute(attribute);
                                }
                                map.Add(attribute.Name, attribute.Value);
                            }
                        }
                        else
                        {
                            if (storey.deltaMember.State.HasPrivileges)
                            {
                                val.Privileges = storey.deltaMember.State.Privileges;
                            }
                            if (((storey.cachedMember.State.RoleCount != storey.deltaMember.State.RoleCount) || !Enumerable.All<uint>(storey.cachedMember.State.RoleList, new Func<uint, bool>(storey.<>m__11F))) || !Enumerable.All<uint>(storey.deltaMember.State.RoleList, new Func<uint, bool>(storey.<>m__120)))
                            {
                                if (membersWithRoleChanges == null)
                                {
                                    membersWithRoleChanges = new List<EntityId>();
                                }
                                membersWithRoleChanges.Add(gameAccountId);
                                val.ClearRole();
                                val.Role.AddRange(storey.deltaMember.State.RoleList);
                            }
                            if (storey.deltaMember.State.HasInfo)
                            {
                                if (val.HasInfo)
                                {
                                    if (storey.deltaMember.State.Info.HasBattleTag)
                                    {
                                        val.Info.SetBattleTag(storey.deltaMember.State.Info.BattleTag);
                                    }
                                }
                                else
                                {
                                    val.SetInfo(storey.deltaMember.State.Info);
                                }
                            }
                        }
                        cachedMember.SetState(val);
                        deltaMember = cachedMember;
                    }
                    if (deltaMember != null)
                    {
                        channelData.m_members[gameAccountId] = deltaMember;
                    }
                    if (map == null)
                    {
                    }
                }
            }
            if ((membersWithRoleChanges != null) && (channelType == ChannelType.PARTY_CHANNEL))
            {
                base.m_battleNet.Party.MemberRolesChanged(channelReferenceObject, membersWithRoleChanges);
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        this.SubscribeToInvitationService();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 1, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyAdd));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 2, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyJoin));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 3, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyRemove));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 4, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyLeave));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 5, new RPCContextDelegate(this.HandleChannelSubscriber_NotifySendMessage));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 6, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyUpdateChannelState));
        base.m_rpcConnection.RegisterServiceMethodListener(this.m_channelSubscriberService.Id, 7, new RPCContextDelegate(this.HandleChannelSubscriber_NotifyUpdateMemberState));
        base.m_rpcConnection.RegisterServiceMethodListener(m_channelInvitationNotifyService.Id, 1, new RPCContextDelegate(this.HandleChannelInvitation_NotifyReceivedInvitationAdded));
        base.m_rpcConnection.RegisterServiceMethodListener(m_channelInvitationNotifyService.Id, 2, new RPCContextDelegate(this.HandleChannelInvitation_NotifyReceivedInvitationRemoved));
        base.m_rpcConnection.RegisterServiceMethodListener(m_channelInvitationNotifyService.Id, 3, new RPCContextDelegate(this.HandleChannelInvitation_NotifyReceivedSuggestionAdded));
        base.m_rpcConnection.RegisterServiceMethodListener(m_channelInvitationNotifyService.Id, 4, new RPCContextDelegate(this.HandleChannelInvitation_NotifyHasRoomForInvitation));
    }

    public void JoinChannel(EntityId channelId, ChannelType channelType)
    {
        JoinChannelRequest message = new JoinChannelRequest();
        message.SetChannelId(channelId);
        message.SetObjectId(GetNextObjectId());
        ChannelData data = new ChannelData(this, channelId, 0L, channelType);
        data.SetSubscriberObjectId(message.ObjectId);
        base.m_rpcConnection.QueueRequest(this.m_channelOwnerService.Id, 3, message, new RPCContextDelegate(data.JoinChannelCallback), (uint) channelType);
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
        this.m_activeChannels.Clear();
        this.m_channelEntityObjectMap.Clear();
    }

    public void RemoveActiveChannel(ulong objectId)
    {
        ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(objectId);
        if (channelReferenceObject != null)
        {
            this.m_channelEntityObjectMap.Remove(channelReferenceObject.m_channelData.m_channelId);
            this.m_activeChannels.Remove(objectId);
        }
    }

    public void RemoveInviteRequestsFor(EntityId channelId, EntityId suggesteeId, uint removeReason)
    {
        if ((this.m_receivedInviteRequests != null) && (suggesteeId != null))
        {
            List<Suggestion> list;
            ChannelReferenceObject channelReferenceObject = this.GetChannelReferenceObject(channelId);
            bool flag = (channelReferenceObject != null) && (channelReferenceObject.m_channelData.m_channelType == ChannelType.PARTY_CHANNEL);
            if (this.m_receivedInviteRequests.TryGetValue(channelId, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Suggestion suggestion = list[i];
                    if (suggesteeId.Equals(suggestion.SuggesterId))
                    {
                        list.RemoveAt(i);
                        i--;
                        if (flag)
                        {
                            base.m_battleNet.Party.ReceivedInviteRequestDelta(channelId, suggestion, new uint?(removeReason));
                        }
                    }
                }
                if (list.Count == 0)
                {
                    this.m_receivedInviteRequests.Remove(channelId);
                    if (this.m_receivedInviteRequests.Count == 0)
                    {
                        this.m_receivedInviteRequests = null;
                    }
                }
            }
        }
    }

    public void RevokeInvitation(ulong invitationId, EntityId channelId, RPCContextDelegate callback)
    {
        RevokeInvitationRequest message = new RevokeInvitationRequest();
        message.SetInvitationId(invitationId);
        message.SetChannelId(channelId);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 6, message, callback, 0);
    }

    public void SendInvitation(EntityId channelId, EntityId entityId, InvitationServiceType serviceType, RPCContextDelegate callback)
    {
        SendInvitationRequest message = new SendInvitationRequest();
        message.SetTargetId(entityId);
        InvitationParams val = new InvitationParams();
        ChannelInvitationParams params2 = new ChannelInvitationParams();
        params2.SetChannelId(channelId);
        params2.SetServiceType((uint) serviceType);
        val.SetChannelParams(params2);
        message.SetParams(val);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 3, message, callback, 0);
    }

    private void SubscribeToInvitationService()
    {
        bnet.protocol.channel_invitation.SubscribeRequest message = new bnet.protocol.channel_invitation.SubscribeRequest();
        message.SetObjectId(0L);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 1, message, new RPCContextDelegate(this.SubscribeToInvitationServiceCallback), 0);
    }

    private void SubscribeToInvitationServiceCallback(RPCContext context)
    {
        base.CheckRPCCallback("SubscribeToInvitationServiceCallback", context);
    }

    public void SuggestInvitation(EntityId partyId, EntityId whomToAskForApproval, EntityId whomToInvite, RPCContextDelegate callback)
    {
        SuggestInvitationRequest message = new SuggestInvitationRequest();
        message.SetChannelId(partyId);
        message.SetApprovalId(whomToAskForApproval);
        message.SetTargetId(whomToInvite);
        base.m_rpcConnection.QueueRequest(m_channelInvitationService.Id, 7, message, callback, 0);
    }

    public void UpdateChannelAttributes(ChannelData channelData, List<bnet.protocol.attribute.Attribute> attributeList, RPCContextDelegate callback)
    {
        UpdateChannelStateRequest message = new UpdateChannelStateRequest();
        bnet.protocol.channel.ChannelState val = new bnet.protocol.channel.ChannelState();
        foreach (bnet.protocol.attribute.Attribute attribute in attributeList)
        {
            val.AddAttribute(attribute);
        }
        message.SetStateChange(val);
        base.m_rpcConnection.QueueRequest(this.m_channelService.Id, 4, message, callback, (uint) channelData.m_objectId);
    }

    public ServiceDescriptor ChannelInvitationNotifyService
    {
        get
        {
            return m_channelInvitationNotifyService;
        }
    }

    public ServiceDescriptor ChannelInvitationService
    {
        get
        {
            return m_channelInvitationService;
        }
    }

    public ServiceDescriptor ChannelOwnerService
    {
        get
        {
            return this.m_channelOwnerService;
        }
    }

    public ServiceDescriptor ChannelService
    {
        get
        {
            return this.m_channelService;
        }
    }

    public ServiceDescriptor ChannelSubscriberService
    {
        get
        {
            return this.m_channelSubscriberService;
        }
    }

    [CompilerGenerated]
    private sealed class <AcceptInvitation>c__AnonStorey304
    {
        internal RPCContextDelegate callback;
        internal ChannelAPI.ChannelData channelData;

        internal void <>m__11E(RPCContext ctx)
        {
            this.channelData.AcceptInvitationCallback(ctx, this.callback);
        }
    }

    [CompilerGenerated]
    private sealed class <HandleChannelSubscriber_NotifyUpdateMemberState>c__AnonStorey305
    {
        internal Member cachedMember;
        internal Member deltaMember;

        internal bool <>m__11F(uint roleId)
        {
            return this.deltaMember.State.RoleList.Contains(roleId);
        }

        internal bool <>m__120(uint roleId)
        {
            return this.cachedMember.State.RoleList.Contains(roleId);
        }
    }

    public class BaseChannelData
    {
        public EntityId m_channelId;
        public ChannelAPI.ChannelType m_channelType;
        public ulong m_objectId;
        public ulong m_subscriberObjectId;

        public BaseChannelData(EntityId entityId, ulong objectId, ChannelAPI.ChannelType channelType)
        {
            this.m_channelId = entityId;
            this.m_channelType = channelType;
            this.m_objectId = objectId;
        }

        public void SetChannelId(EntityId channelId)
        {
            this.m_channelId = channelId;
        }

        public void SetObjectId(ulong objectId)
        {
            this.m_objectId = objectId;
        }

        public void SetSubscriberObjectId(ulong objectId)
        {
            this.m_subscriberObjectId = objectId;
        }
    }

    public class ChannelData : ChannelAPI.BaseChannelData
    {
        private ChannelAPI m_channelAPI;
        public bnet.protocol.channel.ChannelState m_channelState;
        public Map<EntityId, Member> m_members;

        public ChannelData(ChannelAPI channelAPI, EntityId entityId, ulong objectId, ChannelAPI.ChannelType channelType) : base(entityId, objectId, channelType)
        {
            this.m_channelState = new bnet.protocol.channel.ChannelState();
            this.m_members = new Map<EntityId, Member>();
            this.m_channelAPI = channelAPI;
        }

        public void AcceptInvitationCallback(RPCContext context, RPCContextDelegate callback)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_channelAPI.ApiLog.LogError("AcceptInvitationCallback: " + status.ToString());
            }
            else
            {
                AcceptInvitationResponse response = AcceptInvitationResponse.ParseFrom(context.Payload);
                base.SetObjectId(response.ObjectId);
                this.m_channelAPI.AddActiveChannel(base.m_subscriberObjectId, new ChannelAPI.ChannelReferenceObject(this));
                this.m_channelAPI.ApiLog.LogDebug("AcceptInvitationCallback, status=" + status.ToString());
                if (callback != null)
                {
                    callback(context);
                }
            }
        }

        public void JoinChannelCallback(RPCContext context)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_channelAPI.ApiLog.LogError("JoinChannelCallback: " + status.ToString());
            }
            else
            {
                JoinChannelResponse response = JoinChannelResponse.ParseFrom(context.Payload);
                base.SetObjectId(response.ObjectId);
                this.m_channelAPI.AddActiveChannel(base.m_subscriberObjectId, new ChannelAPI.ChannelReferenceObject(this));
                this.m_channelAPI.ApiLog.LogDebug("JoinChannelCallback, status=" + status.ToString());
            }
        }
    }

    public class ChannelReferenceObject
    {
        public ChannelAPI.BaseChannelData m_channelData;

        public ChannelReferenceObject(ChannelAPI.BaseChannelData channelData)
        {
            this.m_channelData = channelData;
        }

        public ChannelReferenceObject(EntityId entityId, ChannelAPI.ChannelType channelType)
        {
            this.m_channelData = new ChannelAPI.BaseChannelData(entityId, 0L, channelType);
        }
    }

    public enum ChannelType
    {
        PRESENCE_CHANNEL,
        CHAT_CHANNEL,
        PARTY_CHANNEL,
        GAME_CHANNEL
    }

    public enum InvitationServiceType
    {
        INVITATION_SERVICE_TYPE_NONE,
        INVITATION_SERVICE_TYPE_PARTY,
        INVITATION_SERVICE_TYPE_CHAT,
        INVITATION_SERVICE_TYPE_GAMES
    }

    public class ReceivedInvite
    {
        public bnet.protocol.channel_invitation.ChannelInvitation ChannelInvitation;
        public bnet.protocol.invitation.Invitation Invitation;

        public ReceivedInvite(bnet.protocol.channel_invitation.ChannelInvitation c, bnet.protocol.invitation.Invitation i)
        {
            this.ChannelInvitation = c;
            this.Invitation = i;
        }

        public IList<bnet.protocol.attribute.Attribute> Attributes
        {
            get
            {
                return this.State.AttributeList;
            }
        }

        public EntityId ChannelId
        {
            get
            {
                return this.ChannelInvitation.ChannelDescription.ChannelId;
            }
        }

        public string ChannelType
        {
            get
            {
                return this.State.ChannelType;
            }
        }

        public bnet.protocol.channel.ChannelState State
        {
            get
            {
                return this.ChannelInvitation.ChannelDescription.State;
            }
        }
    }
}

