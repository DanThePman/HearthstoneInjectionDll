using bnet;
using bnet.protocol;
using bnet.protocol.attribute;
using bnet.protocol.channel;
using bnet.protocol.channel_invitation;
using bnet.protocol.invitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class PartyAPI : BattleNetAPI
{
    private Map<EntityId, PartyData> m_activeParties;
    private List<BattleNet.PartyEvent> m_partyEvents;
    private List<BattleNet.PartyListenerEvent> m_partyListenerEvents;
    public static string PARTY_TYPE_DEFAULT = "default";

    public PartyAPI(BattleNetCSharp battlenet) : base(battlenet, "Party")
    {
        this.m_activeParties = new Map<EntityId, PartyData>();
        this.m_partyEvents = new List<BattleNet.PartyEvent>();
        this.m_partyListenerEvents = new List<BattleNet.PartyListenerEvent>();
    }

    public void AcceptFriendlyChallenge(EntityId partyId)
    {
        EntityId friendGameAccount = new EntityId();
        PartyData partyData = this.GetPartyData(partyId);
        if (partyData != null)
        {
            friendGameAccount = partyData.m_friendGameAccount;
        }
        this.PushPartyEvent(partyId, "dll", "ok", friendGameAccount);
        this.FriendlyChallenge_PushStateChange(partyId, "deck", false);
    }

    public void AcceptPartyInvite(ulong invitationId)
    {
        <AcceptPartyInvite>c__AnonStorey30E storeye = new <AcceptPartyInvite>c__AnonStorey30E {
            invitationId = invitationId,
            <>f__this = this
        };
        ChannelAPI.ReceivedInvite receivedInvite = base.m_battleNet.Channel.GetReceivedInvite(ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY, storeye.invitationId);
        if (receivedInvite != null)
        {
            storeye.szPartyType = this.GetReceivedInvitationPartyType(storeye.invitationId);
            base.m_battleNet.Channel.AcceptInvitation(storeye.invitationId, receivedInvite.ChannelId, ChannelAPI.ChannelType.PARTY_CHANNEL, new RPCContextDelegate(storeye.<>m__129));
        }
    }

    public void AddActiveChannel(ulong objectId, ChannelAPI.ChannelReferenceObject channelRefObject, PartyData partyData)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        base.m_battleNet.Channel.AddActiveChannel(objectId, channelRefObject);
        this.m_activeParties.Add(channelId, partyData);
    }

    public void AssignPartyRole(EntityId partyId, EntityId memberId, uint roleId)
    {
        <AssignPartyRole>c__AnonStorey30C storeyc;
        storeyc = new <AssignPartyRole>c__AnonStorey30C {
            memberId = memberId,
            roleId = roleId,
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storeyc.partyId)
        };
        if (this.GetPartyData(storeyc.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, "AssignPartyRole no PartyData - assuming I'm not a member.", BnetFeatureEvent.Party_AssignRole_Callback, storeyc.partyId, storeyc.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storeyc.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, "AssignPartyRole no channelRefObject", BnetFeatureEvent.Party_AssignRole_Callback, storeyc.partyId, storeyc.szPartyType);
            }
            else
            {
                UpdateMemberStateRequest message = new UpdateMemberStateRequest();
                Member val = new Member();
                Identity identity = new Identity();
                MemberState state = new MemberState();
                state.AddRole(storeyc.roleId);
                identity.SetGameAccountId(storeyc.memberId);
                val.SetIdentity(identity);
                val.SetState(state);
                message.AddStateChange(val);
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 5, message, new RPCContextDelegate(storeyc.<>m__127), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    public void ClearPartyAttribute(EntityId partyId, string attributeKey)
    {
        bnet.protocol.attribute.Attribute attr = new bnet.protocol.attribute.Attribute();
        bnet.protocol.attribute.Variant val = new bnet.protocol.attribute.Variant();
        attr.SetName(attributeKey);
        attr.SetValue(val);
        this.SetPartyAttribute_Internal("ClearPartyAttribute key=" + attributeKey, BnetFeatureEvent.Party_ClearAttribute_Callback, partyId, attr);
    }

    public void ClearPartyListenerEvents()
    {
        this.m_partyListenerEvents.Clear();
    }

    public void ClearPartyUpdates()
    {
        this.m_partyEvents.Clear();
    }

    public void CreateParty(string szPartyType, int privacyLevel, byte[] creatorBlob)
    {
        <CreateParty>c__AnonStorey308 storey = new <CreateParty>c__AnonStorey308 {
            szPartyType = szPartyType,
            partyData = new PartyData(base.m_battleNet)
        };
        CreateChannelRequest message = new CreateChannelRequest();
        message.SetObjectId(ChannelAPI.GetNextObjectId());
        storey.partyData.SetSubscriberObjectId(message.ObjectId);
        ChannelState val = new ChannelState();
        val.SetChannelType(storey.szPartyType);
        val.SetPrivacyLevel((ChannelState.Types.PrivacyLevel) privacyLevel);
        val.AddAttribute(ProtocolHelper.CreateAttribute("WTCG.Party.Type", storey.szPartyType));
        val.AddAttribute(ProtocolHelper.CreateAttribute("WTCG.Party.Creator", creatorBlob));
        message.SetChannelState(val);
        base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelOwnerService.Id, 2, message, new RPCContextDelegate(storey.<>m__123), 2);
    }

    public void DeclineFriendlyChallenge(EntityId partyId, string action)
    {
        PartyData partyData = this.GetPartyData(partyId);
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if ((partyData == null) || (channelReferenceObject == null))
        {
            this.PushPartyEvent(partyId, "parm", "party", new EntityId());
        }
        else
        {
            this.PushPartyEvent(partyId, "dll", action, partyData.m_friendGameAccount);
            base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 6, new DissolveRequest(), new RPCContextDelegate(partyData.DeclineInvite_DissolvePartyInviteCallback), (uint) channelReferenceObject.m_channelData.m_objectId);
        }
    }

    public void DeclinePartyInvite(ulong invitationId)
    {
        <DeclinePartyInvite>c__AnonStorey30F storeyf = new <DeclinePartyInvite>c__AnonStorey30F {
            invitationId = invitationId,
            <>f__this = this
        };
        ChannelAPI.ReceivedInvite receivedInvite = base.m_battleNet.Channel.GetReceivedInvite(ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY, storeyf.invitationId);
        if (receivedInvite != null)
        {
            storeyf.szPartyType = this.GetReceivedInvitationPartyType(storeyf.invitationId);
            base.m_battleNet.Channel.DeclineInvitation(storeyf.invitationId, receivedInvite.ChannelId, new RPCContextDelegate(storeyf.<>m__12A));
        }
    }

    public void DissolveParty(EntityId partyId)
    {
        <DissolveParty>c__AnonStorey30B storeyb;
        storeyb = new <DissolveParty>c__AnonStorey30B {
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storeyb.partyId)
        };
        if (this.GetPartyData(storeyb.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, "DissolveParty no PartyData - assuming I'm not a member.", BnetFeatureEvent.Party_Dissolve_Callback, storeyb.partyId, storeyb.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storeyb.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, "DissolveParty no channelRefObject", BnetFeatureEvent.Party_Dissolve_Callback, storeyb.partyId, storeyb.szPartyType);
            }
            else
            {
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 6, new DissolveRequest(), new RPCContextDelegate(storeyb.<>m__126), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    private void FriendlyChallenge_PushStateChange(EntityId partyId, string state, bool onlyIfMaker = false)
    {
        PartyData partyData = this.GetPartyData(partyId);
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if (((partyData != null) && (channelReferenceObject != null)) && (!onlyIfMaker || partyData.m_maker))
        {
            List<bnet.protocol.attribute.Attribute> attributeList = new List<bnet.protocol.attribute.Attribute> {
                ProtocolHelper.CreateAttribute(!partyData.m_maker ? "s2" : "s1", state)
            };
            base.m_battleNet.Channel.UpdateChannelAttributes((ChannelAPI.ChannelData) channelReferenceObject.m_channelData, attributeList, null);
        }
    }

    private void GenericPartyRequestCallback(RPCContext context, string message, BnetFeatureEvent featureEvent, EntityId partyId, string szPartyType)
    {
        BattleNetErrors error = ((context != null) && (context.Header != null)) ? ((BattleNetErrors) context.Header.Status) : BattleNetErrors.ERROR_RPC_MALFORMED_RESPONSE;
        this.GenericPartyRequestCallback_Internal(error, message, featureEvent, partyId, szPartyType);
    }

    private void GenericPartyRequestCallback_Internal(BattleNetErrors error, string message, BnetFeatureEvent featureEvent, EntityId partyId, string szPartyType)
    {
        base.m_battleNet.Party.PushPartyErrorEvent(BattleNet.PartyListenerEventType.OPERATION_CALLBACK, message, error, featureEvent, partyId, szPartyType);
        if (error != BattleNetErrors.ERROR_OK)
        {
            if (partyId != null)
            {
                object[] args = new object[] { (int) error, error.ToString(), message, partyId.Low, szPartyType };
                message = string.Format("PartyRequestError: {0} {1} {2} partyId={3} type={4}", args);
            }
            else
            {
                object[] objArray2 = new object[] { (int) error, error.ToString(), message, szPartyType };
                message = string.Format("PartyRequestError: {0} {1} {2} type={3}", objArray2);
            }
            base.m_battleNet.Party.ApiLog.LogError(message);
        }
        else
        {
            if (partyId != null)
            {
                object[] objArray3 = new object[] { message, error.ToString(), partyId.Low, szPartyType };
                message = string.Format("PartyRequest {0} status={1} partyId={2} type={3}", objArray3);
            }
            else
            {
                message = string.Format("PartyRequest {0} status={1} type={2}", message, error.ToString(), szPartyType);
            }
            base.m_battleNet.Party.ApiLog.LogDebug(message);
        }
    }

    public void GetAllPartyAttributes(EntityId partyId, out string[] allKeys)
    {
        ChannelState partyState;
        <GetAllPartyAttributes>c__AnonStorey315 storey = new <GetAllPartyAttributes>c__AnonStorey315 {
            partyId = partyId
        };
        ChannelAPI.ReceivedInvite invite = Enumerable.FirstOrDefault<ChannelAPI.ReceivedInvite>(base.m_battleNet.Channel.GetAllReceivedInvites(), new Func<ChannelAPI.ReceivedInvite, bool>(storey.<>m__130));
        if ((invite != null) && (invite.State != null))
        {
            partyState = invite.State;
        }
        else
        {
            partyState = this.GetPartyState(storey.partyId);
        }
        if (partyState == null)
        {
            allKeys = new string[0];
        }
        else
        {
            allKeys = new string[partyState.AttributeList.Count];
            for (int i = 0; i < partyState.AttributeList.Count; i++)
            {
                bnet.protocol.attribute.Attribute attribute = partyState.AttributeList[i];
                allKeys[i] = attribute.Name;
            }
        }
    }

    public int GetCountPartyMembers(EntityId partyId)
    {
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if (channelReferenceObject != null)
        {
            ChannelAPI.ChannelData channelData = channelReferenceObject.m_channelData as ChannelAPI.ChannelData;
            if ((channelData != null) && (channelData.m_members != null))
            {
                return channelData.m_members.Count;
            }
        }
        return 0;
    }

    public int GetMaxPartyMembers(EntityId partyId)
    {
        ChannelState partyState = this.GetPartyState(partyId);
        if ((partyState != null) && partyState.HasMaxMembers)
        {
            return (int) partyState.MaxMembers;
        }
        return 0;
    }

    public void GetPartyAttributeBlob(EntityId partyId, string attributeKey, out byte[] value)
    {
        value = null;
        bnet.protocol.attribute.Attribute receivedInvitationAttribute = this.GetReceivedInvitationAttribute(partyId, attributeKey);
        if ((receivedInvitationAttribute != null) && receivedInvitationAttribute.Value.HasBlobValue)
        {
            value = receivedInvitationAttribute.Value.BlobValue;
        }
        else
        {
            ChannelState partyState = this.GetPartyState(partyId);
            if (partyState != null)
            {
                for (int i = 0; i < partyState.AttributeList.Count; i++)
                {
                    receivedInvitationAttribute = partyState.AttributeList[i];
                    if ((receivedInvitationAttribute.Name == attributeKey) && receivedInvitationAttribute.Value.HasBlobValue)
                    {
                        value = receivedInvitationAttribute.Value.BlobValue;
                        break;
                    }
                }
            }
        }
    }

    public bool GetPartyAttributeLong(EntityId partyId, string attributeKey, out long value)
    {
        value = 0L;
        bnet.protocol.attribute.Attribute receivedInvitationAttribute = this.GetReceivedInvitationAttribute(partyId, attributeKey);
        if ((receivedInvitationAttribute != null) && receivedInvitationAttribute.Value.HasIntValue)
        {
            value = receivedInvitationAttribute.Value.IntValue;
            return true;
        }
        ChannelState partyState = this.GetPartyState(partyId);
        if (partyState != null)
        {
            for (int i = 0; i < partyState.AttributeList.Count; i++)
            {
                receivedInvitationAttribute = partyState.AttributeList[i];
                if ((receivedInvitationAttribute.Name == attributeKey) && receivedInvitationAttribute.Value.HasIntValue)
                {
                    value = receivedInvitationAttribute.Value.IntValue;
                    return true;
                }
            }
        }
        return false;
    }

    public void GetPartyAttributeString(EntityId partyId, string attributeKey, out string value)
    {
        value = null;
        bnet.protocol.attribute.Attribute receivedInvitationAttribute = this.GetReceivedInvitationAttribute(partyId, attributeKey);
        if ((receivedInvitationAttribute != null) && receivedInvitationAttribute.Value.HasStringValue)
        {
            value = receivedInvitationAttribute.Value.StringValue;
        }
        else
        {
            ChannelState partyState = this.GetPartyState(partyId);
            if (partyState != null)
            {
                for (int i = 0; i < partyState.AttributeList.Count; i++)
                {
                    receivedInvitationAttribute = partyState.AttributeList[i];
                    if ((receivedInvitationAttribute.Name == attributeKey) && receivedInvitationAttribute.Value.HasStringValue)
                    {
                        value = receivedInvitationAttribute.Value.StringValue;
                        break;
                    }
                }
            }
        }
    }

    private PartyData GetPartyData(EntityId partyId)
    {
        PartyData data;
        if (this.m_activeParties.TryGetValue(partyId, out data))
        {
            return data;
        }
        return null;
    }

    public void GetPartyInviteRequests(EntityId partyId, out InviteRequest[] requests)
    {
        Suggestion[] inviteRequests = base.m_battleNet.Channel.GetInviteRequests(partyId);
        requests = new InviteRequest[inviteRequests.Length];
        for (int i = 0; i < requests.Length; i++)
        {
            Suggestion suggestion = inviteRequests[i];
            requests[i] = new InviteRequest { TargetName = suggestion.SuggesteeName, TargetId = BnetGameAccountId.CreateFromProtocol(suggestion.SuggesteeId), RequesterName = suggestion.SuggesterName, RequesterId = BnetGameAccountId.CreateFromProtocol(suggestion.SuggesterId) };
        }
    }

    public void GetPartyListenerEvents(out BattleNet.PartyListenerEvent[] updates)
    {
        updates = new BattleNet.PartyListenerEvent[this.m_partyListenerEvents.Count];
        this.m_partyListenerEvents.CopyTo(updates);
    }

    public void GetPartyMembers(EntityId partyId, out BattleNet.DllPartyMember[] members)
    {
        members = null;
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if (channelReferenceObject != null)
        {
            ChannelAPI.ChannelData channelData = channelReferenceObject.m_channelData as ChannelAPI.ChannelData;
            if ((channelData != null) && (channelData.m_members != null))
            {
                members = new BattleNet.DllPartyMember[channelData.m_members.Count];
                int index = 0;
                foreach (KeyValuePair<EntityId, Member> pair in channelData.m_members)
                {
                    EntityId key = pair.Key;
                    Member member = pair.Value;
                    BattleNet.DllPartyMember member2 = new BattleNet.DllPartyMember {
                        memberGameAccountId = new BattleNet.DllEntityId(key)
                    };
                    if (member.State.RoleCount > 0)
                    {
                        member2.firstMemberRole = member.State.RoleList[0];
                    }
                    members[index] = member2;
                    index++;
                }
            }
        }
        if (members == null)
        {
            members = new BattleNet.DllPartyMember[0];
        }
    }

    public int GetPartyPrivacy(EntityId partyId)
    {
        int privacyLevel = 4;
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if (channelReferenceObject != null)
        {
            ChannelAPI.ChannelData channelData = channelReferenceObject.m_channelData as ChannelAPI.ChannelData;
            if (((channelData != null) && (channelData.m_channelState != null)) && channelData.m_channelState.HasPrivacyLevel)
            {
                privacyLevel = (int) channelData.m_channelState.PrivacyLevel;
            }
        }
        return privacyLevel;
    }

    public void GetPartySentInvites(EntityId partyId, out PartyInvite[] invites)
    {
        invites = null;
        ChannelState partyState = this.GetPartyState(partyId);
        if (partyState != null)
        {
            invites = new PartyInvite[partyState.InvitationCount];
            PartyType partyTypeFromString = BnetParty.GetPartyTypeFromString(this.GetPartyType(partyId));
            for (int i = 0; i < invites.Length; i++)
            {
                Invitation invitation = partyState.InvitationList[i];
                invites[i] = new PartyInvite { InviteId = invitation.Id, PartyId = PartyId.FromProtocol(partyId), PartyType = partyTypeFromString, InviterName = invitation.InviterName, InviterId = BnetGameAccountId.CreateFromProtocol(invitation.InviterIdentity.GameAccountId), InviteeId = BnetGameAccountId.CreateFromProtocol(invitation.InviteeIdentity.GameAccountId) };
            }
        }
        if (invites == null)
        {
            invites = new PartyInvite[0];
        }
    }

    private ChannelState GetPartyState(EntityId partyId)
    {
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if (channelReferenceObject != null)
        {
            ChannelAPI.ChannelData channelData = channelReferenceObject.m_channelData as ChannelAPI.ChannelData;
            return channelData.m_channelState;
        }
        return null;
    }

    public string GetPartyType(EntityId partyId)
    {
        string channelType = string.Empty;
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if ((channelReferenceObject != null) && (channelReferenceObject.m_channelData is ChannelAPI.ChannelData))
        {
            ChannelState channelState = ((ChannelAPI.ChannelData) channelReferenceObject.m_channelData).m_channelState;
            if (channelState != null)
            {
                channelType = channelState.ChannelType;
                if (channelType == PARTY_TYPE_DEFAULT)
                {
                    string str2;
                    this.GetPartyAttributeString(partyId, "WTCG.Party.Type", out str2);
                    if (str2 != null)
                    {
                        channelType = str2;
                    }
                }
            }
        }
        return channelType;
    }

    public int GetPartyUpdateCount()
    {
        return this.m_partyEvents.Count;
    }

    public void GetPartyUpdates([Out] BattleNet.PartyEvent[] updates)
    {
        this.m_partyEvents.CopyTo(updates);
    }

    public bnet.protocol.attribute.Attribute GetReceivedInvitationAttribute(EntityId partyId, string attributeKey)
    {
        <GetReceivedInvitationAttribute>c__AnonStorey306 storey = new <GetReceivedInvitationAttribute>c__AnonStorey306 {
            partyId = partyId
        };
        ChannelAPI.ReceivedInvite invite = Enumerable.FirstOrDefault<ChannelAPI.ReceivedInvite>(base.m_battleNet.Channel.GetAllReceivedInvites(), new Func<ChannelAPI.ReceivedInvite, bool>(storey.<>m__121));
        if ((invite != null) && (invite.State != null))
        {
            ChannelState state = invite.State;
            for (int i = 0; i < state.AttributeList.Count; i++)
            {
                bnet.protocol.attribute.Attribute attribute = state.AttributeList[i];
                if (attribute.Name == attributeKey)
                {
                    return attribute;
                }
            }
        }
        return null;
    }

    public string GetReceivedInvitationPartyType(ulong invitationId)
    {
        string str = string.Empty;
        ChannelAPI.ReceivedInvite receivedInvite = base.m_battleNet.Channel.GetReceivedInvite(ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY, invitationId);
        if ((receivedInvite != null) && (receivedInvite.State != null))
        {
            ChannelState state = receivedInvite.State;
            for (int i = 0; i < state.AttributeList.Count; i++)
            {
                bnet.protocol.attribute.Attribute attribute = state.AttributeList[i];
                if ((attribute.Name == "WTCG.Party.Type") && attribute.Value.HasStringValue)
                {
                    return attribute.Value.StringValue;
                }
            }
        }
        return str;
    }

    public void GetReceivedPartyInvites(out PartyInvite[] invites)
    {
        ChannelAPI.ReceivedInvite[] receivedInvites = base.m_battleNet.Channel.GetReceivedInvites(ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY);
        invites = new PartyInvite[receivedInvites.Length];
        for (int i = 0; i < invites.Length; i++)
        {
            ChannelAPI.ReceivedInvite invite = receivedInvites[i];
            Invitation invitation = invite.Invitation;
            PartyType partyTypeFromString = BnetParty.GetPartyTypeFromString(this.GetReceivedInvitationPartyType(invitation.Id));
            invites[i] = new PartyInvite { InviteId = invitation.Id, PartyId = PartyId.FromProtocol(invite.ChannelId), PartyType = partyTypeFromString, InviterName = invitation.InviterName, InviterId = BnetGameAccountId.CreateFromProtocol(invitation.InviterIdentity.GameAccountId), InviteeId = BnetGameAccountId.CreateFromProtocol(invitation.InviteeIdentity.GameAccountId) };
        }
    }

    public void IgnoreInviteRequest(EntityId partyId, EntityId requestedTargetId)
    {
        base.m_battleNet.Channel.RemoveInviteRequestsFor(partyId, requestedTargetId, 1);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
    }

    public void JoinParty(EntityId partyId, string szPartyType)
    {
        <JoinParty>c__AnonStorey309 storey;
        storey = new <JoinParty>c__AnonStorey309 {
            partyId = partyId,
            szPartyType = szPartyType,
            partyData = this.GetPartyData(storey.partyId)
        };
        if (storey.partyData != null)
        {
            this.PushPartyError(storey.partyId, BattleNetErrors.ERROR_PARTY_ALREADY_IN_PARTY, BnetFeatureEvent.Party_Join_Callback);
        }
        else
        {
            storey.partyData = new PartyData(base.m_battleNet, storey.partyId, null);
            JoinChannelRequest message = new JoinChannelRequest();
            message.SetChannelId(storey.partyId);
            message.SetObjectId(ChannelAPI.GetNextObjectId());
            storey.partyData.SetSubscriberObjectId(message.ObjectId);
            storey.partyData.m_partyId = storey.partyId;
            base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelOwnerService.Id, 3, message, new RPCContextDelegate(storey.<>m__124), 2);
        }
    }

    public void KickPartyMember(EntityId partyId, EntityId memberId)
    {
        <KickPartyMember>c__AnonStorey312 storey;
        storey = new <KickPartyMember>c__AnonStorey312 {
            memberId = memberId,
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storey.partyId)
        };
        if (this.GetPartyData(storey.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, "KickPartyMember no PartyData - assuming I'm not a member.", BnetFeatureEvent.Party_KickMember_Callback, storey.partyId, storey.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storey.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, "KickPartyMember no channelRefObject", BnetFeatureEvent.Party_KickMember_Callback, storey.partyId, storey.szPartyType);
            }
            else
            {
                RemoveMemberRequest message = new RemoveMemberRequest {
                    MemberId = storey.memberId
                };
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 2, message, new RPCContextDelegate(storey.<>m__12D), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    public void LeaveParty(EntityId partyId)
    {
        <LeaveParty>c__AnonStorey30A storeya;
        storeya = new <LeaveParty>c__AnonStorey30A {
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storeya.partyId)
        };
        if (this.GetPartyData(storeya.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, "LeaveParty no PartyData - assuming I'm not a member.", BnetFeatureEvent.Party_Leave_Callback, storeya.partyId, storeya.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storeya.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, "LeaveParty no channelRefObject", BnetFeatureEvent.Party_Leave_Callback, storeya.partyId, storeya.szPartyType);
            }
            else
            {
                RemoveMemberRequest message = new RemoveMemberRequest();
                message.MemberId = base.m_battleNet.GetMyGameAccountId().ToProtocol();
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 2, message, new RPCContextDelegate(storeya.<>m__125), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    public void MemberRolesChanged(ChannelAPI.ChannelReferenceObject channelRefObject, IEnumerable<EntityId> membersWithRoleChanges)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        string partyType = this.GetPartyType(channelId);
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.MEMBER_ROLE_CHANGED,
            PartyId = PartyId.FromProtocol(channelId),
            StringData = partyType
        };
        IEnumerator<EntityId> enumerator = membersWithRoleChanges.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                EntityId current = enumerator.Current;
                item.SubjectMemberId = BnetGameAccountId.CreateFromProtocol(current);
                this.m_partyListenerEvents.Add(item);
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
        this.m_activeParties.Clear();
        this.m_partyEvents.Clear();
        this.m_partyListenerEvents.Clear();
    }

    public void PartyAttributeChanged(EntityId channelId, string attributeKey, bnet.protocol.attribute.Variant attributeValue)
    {
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.PARTY_ATTRIBUTE_CHANGED,
            PartyId = PartyId.FromProtocol(channelId),
            StringData = attributeKey
        };
        if (attributeValue.IsNone())
        {
            item.UintData = 0;
        }
        else if (attributeValue.HasIntValue)
        {
            item.UintData = 1;
            item.UlongData = (ulong) attributeValue.IntValue;
        }
        else if (attributeValue.HasStringValue)
        {
            item.UintData = 2;
            item.StringData2 = attributeValue.StringValue;
        }
        else if (attributeValue.HasBlobValue)
        {
            item.UintData = 3;
            item.BlobData = attributeValue.BlobValue;
        }
        else
        {
            item.UintData = 0;
        }
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyInvitationDelta(EntityId partyId, Invitation invite, uint? removeReason)
    {
        string partyType = this.GetPartyType(partyId);
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = !removeReason.HasValue ? BattleNet.PartyListenerEventType.PARTY_INVITE_SENT : BattleNet.PartyListenerEventType.PARTY_INVITE_REMOVED,
            PartyId = PartyId.FromProtocol(partyId),
            UlongData = invite.Id,
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(invite.InviterIdentity.GameAccountId),
            TargetMemberId = BnetGameAccountId.CreateFromProtocol(invite.InviteeIdentity.GameAccountId),
            StringData = partyType,
            StringData2 = invite.InviterName
        };
        if (removeReason.HasValue)
        {
            item.UintData = removeReason.Value;
        }
        else
        {
            item.UintData = 0;
            if (invite.HasChannelInvitation)
            {
                ChannelInvitation channelInvitation = invite.ChannelInvitation;
                if (channelInvitation.HasReserved && channelInvitation.Reserved)
                {
                    item.UintData |= 1;
                }
                if (channelInvitation.HasRejoin && channelInvitation.Rejoin)
                {
                    item.UintData |= 1;
                }
            }
        }
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyJoined(ChannelAPI.ChannelReferenceObject channelRefObject, AddNotification notification)
    {
        ChannelAPI.ChannelData channelData = (ChannelAPI.ChannelData) channelRefObject.m_channelData;
        EntityId channelId = channelData.m_channelId;
        string partyType = this.GetPartyType(channelId);
        PartyData partyData = this.GetPartyData(channelId);
        bool flag = false;
        if (partyData == null)
        {
            if (BnetParty.GetPartyTypeFromString(partyType) == PartyType.FRIENDLY_CHALLENGE)
            {
                flag = true;
                foreach (Member member in notification.MemberList)
                {
                    EntityId gameAccountId = member.Identity.GameAccountId;
                    if (!base.m_battleNet.GameAccountId.Equals(gameAccountId))
                    {
                        PartyData data3 = new PartyData(base.m_battleNet, channelId, gameAccountId);
                        this.m_activeParties.Add(channelId, data3);
                        data3.FriendlyChallenge_HandleChannelAttributeUpdate(notification.ChannelState.AttributeList);
                        break;
                    }
                }
            }
            else
            {
                PartyData data4 = new PartyData(base.m_battleNet, channelId, null);
                this.m_activeParties.Add(channelId, data4);
            }
        }
        else
        {
            flag = (partyData != null) && (partyData.m_friendGameAccount != null);
        }
        if (flag)
        {
            this.FriendlyChallenge_PushStateChange(channelId, "wait", false);
        }
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.JOINED_PARTY,
            PartyId = PartyId.FromProtocol(channelId),
            StringData = partyType
        };
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyLeft(ChannelAPI.ChannelReferenceObject channelRefObject, RemoveNotification notification)
    {
        ChannelAPI.ChannelData channelData = (ChannelAPI.ChannelData) channelRefObject.m_channelData;
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        string partyType = this.GetPartyType(channelId);
        PartyData partyData = this.GetPartyData(channelData.m_channelId);
        if (partyData != null)
        {
            if (partyData.m_friendGameAccount != null)
            {
                string data = "NO_SUPPLIED_REASON";
                if (notification.HasReason)
                {
                    data = notification.Reason.ToString();
                }
                this.PushPartyEvent(partyData.m_partyId, "left", data, partyData.m_friendGameAccount);
            }
            this.m_activeParties.Remove(partyData.m_partyId);
        }
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.LEFT_PARTY,
            PartyId = PartyId.FromProtocol(channelId),
            StringData = partyType,
            UintData = !notification.HasReason ? 0 : notification.Reason
        };
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyMemberJoined(ChannelAPI.ChannelReferenceObject channelRefObject, JoinNotification notification)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.MEMBER_JOINED,
            PartyId = PartyId.FromProtocol(channelId),
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(notification.Member.Identity.GameAccountId)
        };
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyMemberLeft(ChannelAPI.ChannelReferenceObject channelRefObject, LeaveNotification notification)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        PartyData partyData = this.GetPartyData(channelId);
        if ((partyData != null) && (partyData.m_friendGameAccount != null))
        {
            base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 6, new DissolveRequest(), new RPCContextDelegate(partyData.PartyMemberLeft_DissolvePartyInviteCallback), (uint) channelRefObject.m_channelData.m_objectId);
        }
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.MEMBER_LEFT,
            PartyId = PartyId.FromProtocol(channelId),
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(notification.MemberId),
            UintData = !notification.HasReason ? 0 : notification.Reason
        };
        this.m_partyListenerEvents.Add(item);
    }

    public void PartyMessageReceived(ChannelAPI.ChannelReferenceObject channelRefObject, SendMessageNotification notification)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        string stringValue = null;
        for (int i = 0; i < notification.Message.AttributeCount; i++)
        {
            bnet.protocol.attribute.Attribute attribute = notification.Message.AttributeList[i];
            if (attribute.TEXT_ATTRIBUTE.Equals(attribute.Name) && attribute.Value.HasStringValue)
            {
                stringValue = attribute.Value.StringValue;
                break;
            }
        }
        if (!string.IsNullOrEmpty(stringValue))
        {
            BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
                Type = BattleNet.PartyListenerEventType.CHAT_MESSAGE_RECEIVED,
                PartyId = PartyId.FromProtocol(channelId),
                SubjectMemberId = BnetGameAccountId.CreateFromProtocol(notification.AgentId),
                StringData = stringValue
            };
            this.m_partyListenerEvents.Add(item);
        }
    }

    public void PartyPrivacyChanged(EntityId channelId, ChannelState.Types.PrivacyLevel newPrivacyLevel)
    {
        string partyType = this.GetPartyType(channelId);
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.PRIVACY_CHANGED,
            PartyId = PartyId.FromProtocol(channelId),
            UintData = (uint) newPrivacyLevel,
            StringData = partyType
        };
        this.m_partyListenerEvents.Add(item);
    }

    public void PreprocessPartyChannelUpdated(ChannelAPI.ChannelReferenceObject channelRefObject, UpdateChannelStateNotification notification)
    {
        EntityId channelId = channelRefObject.m_channelData.m_channelId;
        PartyData partyData = this.GetPartyData(channelId);
        if ((partyData != null) && (partyData.m_friendGameAccount != null))
        {
            partyData.FriendlyChallenge_HandleChannelAttributeUpdate(notification.StateChange.AttributeList);
        }
    }

    private void PushPartyError(EntityId partyId, BattleNetErrors errorCode, BnetFeatureEvent featureEvent)
    {
        BattleNet.PartyEvent item = new BattleNet.PartyEvent();
        if (partyId != null)
        {
            item.partyId.hi = partyId.High;
            item.partyId.lo = partyId.Low;
        }
        item.errorInfo = new BnetErrorInfo(BnetFeature.Party, featureEvent, errorCode);
        this.m_partyEvents.Add(item);
    }

    private void PushPartyErrorEvent(BattleNet.PartyListenerEventType evtType, string szDebugContext, bnet.Error error, BnetFeatureEvent featureEvent, EntityId partyId = null, string szStringData = null)
    {
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = evtType,
            PartyId = (partyId != null) ? PartyId.FromProtocol(partyId) : new PartyId(),
            UintData = error.Code,
            UlongData = 0x400000000L | ((ulong) featureEvent),
            StringData = (szDebugContext != null) ? szDebugContext : string.Empty,
            StringData2 = (szStringData != null) ? szStringData : string.Empty
        };
        this.m_partyListenerEvents.Add(item);
    }

    private void PushPartyEvent(EntityId partyId, string type, string data, EntityId friendGameAccount)
    {
        BattleNet.PartyEvent item = new BattleNet.PartyEvent();
        item.partyId.hi = partyId.High;
        item.partyId.lo = partyId.Low;
        item.eventName = type;
        item.eventData = data;
        item.otherMemberId.hi = friendGameAccount.High;
        item.otherMemberId.lo = friendGameAccount.Low;
        this.m_partyEvents.Add(item);
    }

    public void ReceivedInvitationAdded(InvitationAddedNotification notification, ChannelInvitation channelInvitation)
    {
        string receivedInvitationPartyType = this.GetReceivedInvitationPartyType(notification.Invitation.Id);
        if (BnetParty.GetPartyTypeFromString(receivedInvitationPartyType) == PartyType.FRIENDLY_CHALLENGE)
        {
            base.m_battleNet.Channel.AcceptInvitation(notification.Invitation.Id, channelInvitation.ChannelDescription.ChannelId, ChannelAPI.ChannelType.PARTY_CHANNEL, null);
        }
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.RECEIVED_INVITE_ADDED,
            PartyId = PartyId.FromProtocol(channelInvitation.ChannelDescription.ChannelId),
            UlongData = notification.Invitation.Id,
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(notification.Invitation.InviterIdentity.GameAccountId),
            TargetMemberId = BnetGameAccountId.CreateFromProtocol(notification.Invitation.InviteeIdentity.GameAccountId),
            StringData = receivedInvitationPartyType,
            StringData2 = notification.Invitation.InviterName,
            UintData = 0
        };
        if (channelInvitation.HasReserved && channelInvitation.Reserved)
        {
            item.UintData |= 1;
        }
        if (channelInvitation.HasRejoin && channelInvitation.Rejoin)
        {
            item.UintData |= 1;
        }
        this.m_partyListenerEvents.Add(item);
    }

    public void ReceivedInvitationRemoved(string szPartyType, InvitationRemovedNotification notification, ChannelInvitation channelInvitation)
    {
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = BattleNet.PartyListenerEventType.RECEIVED_INVITE_REMOVED,
            PartyId = PartyId.FromProtocol(channelInvitation.ChannelDescription.ChannelId),
            UlongData = notification.Invitation.Id,
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(notification.Invitation.InviterIdentity.GameAccountId),
            TargetMemberId = BnetGameAccountId.CreateFromProtocol(notification.Invitation.InviteeIdentity.GameAccountId),
            StringData = szPartyType,
            StringData2 = notification.Invitation.InviterName
        };
        if (notification.HasReason)
        {
            item.UintData = notification.Reason;
        }
        this.m_partyListenerEvents.Add(item);
    }

    public void ReceivedInviteRequestDelta(EntityId partyId, Suggestion suggestion, uint? removeReason)
    {
        BattleNet.PartyListenerEvent item = new BattleNet.PartyListenerEvent {
            Type = !removeReason.HasValue ? BattleNet.PartyListenerEventType.INVITE_REQUEST_ADDED : BattleNet.PartyListenerEventType.INVITE_REQUEST_REMOVED,
            PartyId = PartyId.FromProtocol(partyId),
            SubjectMemberId = BnetGameAccountId.CreateFromProtocol(suggestion.SuggesterId),
            TargetMemberId = BnetGameAccountId.CreateFromProtocol(suggestion.SuggesteeId),
            StringData = suggestion.SuggesterName,
            StringData2 = suggestion.SuggesteeName
        };
        if (removeReason.HasValue)
        {
            item.UintData = removeReason.Value;
        }
        this.m_partyListenerEvents.Add(item);
    }

    public void RequestPartyInvite(EntityId partyId, EntityId whomToAskForApproval, EntityId whomToInvite, string szPartyType)
    {
        <RequestPartyInvite>c__AnonStorey311 storey = new <RequestPartyInvite>c__AnonStorey311 {
            whomToInvite = whomToInvite,
            whomToAskForApproval = whomToAskForApproval,
            partyId = partyId,
            szPartyType = szPartyType,
            <>f__this = this
        };
        base.m_battleNet.Channel.SuggestInvitation(storey.partyId, storey.whomToAskForApproval, storey.whomToInvite, new RPCContextDelegate(storey.<>m__12C));
    }

    public void RevokePartyInvite(EntityId partyId, ulong invitationId)
    {
        <RevokePartyInvite>c__AnonStorey310 storey;
        storey = new <RevokePartyInvite>c__AnonStorey310 {
            invitationId = invitationId,
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storey.partyId)
        };
        base.m_battleNet.Channel.RevokeInvitation(storey.invitationId, storey.partyId, new RPCContextDelegate(storey.<>m__12B));
    }

    public void SendFriendlyChallengeInvite(EntityId friendEntityId, int scenarioId)
    {
        <SendFriendlyChallengeInvite>c__AnonStorey307 storey = new <SendFriendlyChallengeInvite>c__AnonStorey307 {
            partyData = new PartyData(base.m_battleNet, friendEntityId)
        };
        storey.partyData.m_scenarioId = scenarioId;
        CreateChannelRequest message = new CreateChannelRequest();
        message.SetObjectId(ChannelAPI.GetNextObjectId());
        storey.partyData.SetSubscriberObjectId(message.ObjectId);
        ChannelState val = new ChannelState();
        val.SetName("FriendlyGame");
        val.SetPrivacyLevel(ChannelState.Types.PrivacyLevel.PRIVACY_LEVEL_OPEN);
        val.AddAttribute(ProtocolHelper.CreateAttribute("WTCG.Party.Type", "FriendlyGame"));
        val.AddAttribute(ProtocolHelper.CreateAttribute("WTCG.Game.ScenarioId", (long) scenarioId));
        message.SetChannelState(val);
        base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelOwnerService.Id, 2, message, new RPCContextDelegate(storey.<>m__122), 2);
    }

    public void SendPartyChatMessage(EntityId partyId, string message)
    {
        <SendPartyChatMessage>c__AnonStorey313 storey;
        storey = new <SendPartyChatMessage>c__AnonStorey313 {
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storey.partyId)
        };
        if (this.GetPartyData(storey.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, "SendPartyChatMessage no PartyData - assuming I'm not a member.", BnetFeatureEvent.Party_SendChatMessage_Callback, storey.partyId, storey.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storey.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, "SendPartyChatMessage no channelRefObject", BnetFeatureEvent.Party_SendChatMessage_Callback, storey.partyId, storey.szPartyType);
            }
            else
            {
                SendMessageRequest request = new SendMessageRequest();
                Message val = new Message();
                val.AddAttribute(ProtocolHelper.CreateAttribute(attribute.TEXT_ATTRIBUTE, message));
                request.SetMessage(val);
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 3, request, new RPCContextDelegate(storey.<>m__12E), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    public void SendPartyInvite(EntityId partyId, EntityId inviteeId, bool isReservation)
    {
        <SendPartyInvite>c__AnonStorey30D storeyd = new <SendPartyInvite>c__AnonStorey30D {
            partyId = partyId,
            inviteeId = inviteeId,
            <>f__this = this
        };
        if (this.GetPartyData(storeyd.partyId) != null)
        {
            storeyd.szPartyType = this.GetPartyType(storeyd.partyId);
            base.m_battleNet.Channel.SendInvitation(storeyd.partyId, storeyd.inviteeId, ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY, new RPCContextDelegate(storeyd.<>m__128));
        }
    }

    public void SendPartyInviteCallback(RPCContext context, EntityId partyId, EntityId inviteeId, string szPartyType)
    {
        string szDebugContext = "SendPartyInvite inviteeId=" + inviteeId.Low;
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        base.m_battleNet.Party.PushPartyErrorEvent(BattleNet.PartyListenerEventType.OPERATION_CALLBACK, szDebugContext, status, BnetFeatureEvent.Party_SendInvite_Callback, partyId, szPartyType);
        if (status != BattleNetErrors.ERROR_OK)
        {
            if (partyId != null)
            {
                object[] args = new object[] { (int) status, status.ToString(), szDebugContext, partyId.Low, szPartyType };
                szDebugContext = string.Format("PartyRequestError: {0} {1} {2} partyId={3} type={4}", args);
            }
            else
            {
                object[] objArray2 = new object[] { (int) status, status.ToString(), szDebugContext, szPartyType };
                szDebugContext = string.Format("PartyRequestError: {0} {1} {2} type={3}", objArray2);
            }
            base.m_battleNet.Party.ApiLog.LogError(szDebugContext);
        }
        else
        {
            if (partyId != null)
            {
                object[] objArray3 = new object[] { szDebugContext, status.ToString(), partyId.Low, szPartyType };
                szDebugContext = string.Format("PartyRequest {0} status={1} partyId={2} type={3}", objArray3);
            }
            else
            {
                szDebugContext = string.Format("PartyRequest {0} status={1} type={2}", szDebugContext, status.ToString(), szPartyType);
            }
            base.m_battleNet.Party.ApiLog.LogDebug(szDebugContext);
            base.m_battleNet.Channel.RemoveInviteRequestsFor(partyId, inviteeId, 0);
        }
    }

    private void SetPartyAttribute_Internal(string debugMessage, BnetFeatureEvent featureEvent, EntityId partyId, bnet.protocol.attribute.Attribute attr)
    {
        ChannelState state = new ChannelState();
        state.AddAttribute(attr);
        this.UpdatePartyState_Internal(debugMessage, featureEvent, partyId, state);
    }

    public void SetPartyAttributeBlob(EntityId partyId, string attributeKey, byte[] value)
    {
        this.SetPartyAttribute_Internal("SetPartyAttributeBlob key=" + attributeKey + " val=" + ((value != null) ? (value.Length + " bytes") : "null"), BnetFeatureEvent.Party_SetAttribute_Callback, partyId, ProtocolHelper.CreateAttribute(attributeKey, value));
    }

    public void SetPartyAttributeLong(EntityId partyId, string attributeKey, long value)
    {
        this.SetPartyAttribute_Internal(string.Concat(new object[] { "SetPartyAttributeLong key=", attributeKey, " val=", value }), BnetFeatureEvent.Party_SetAttribute_Callback, partyId, ProtocolHelper.CreateAttribute(attributeKey, value));
    }

    public void SetPartyAttributeString(EntityId partyId, string attributeKey, string value)
    {
        this.SetPartyAttribute_Internal("SetPartyAttributeString key=" + attributeKey + " val=" + value, BnetFeatureEvent.Party_SetAttribute_Callback, partyId, ProtocolHelper.CreateAttribute(attributeKey, value));
    }

    public void SetPartyDeck(EntityId partyId, long deckId)
    {
        PartyData partyData = this.GetPartyData(partyId);
        ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(partyId);
        if ((partyData == null) || (channelReferenceObject == null))
        {
            this.PushPartyEvent(partyId, "parm", "party", new EntityId());
        }
        else
        {
            this.PushPartyEvent(partyId, "dll", "deck", partyData.m_friendGameAccount);
            if (deckId != 0)
            {
                this.FriendlyChallenge_PushStateChange(partyId, "game", false);
            }
            else
            {
                this.FriendlyChallenge_PushStateChange(partyId, "deck", false);
            }
            List<bnet.protocol.attribute.Attribute> attributeList = new List<bnet.protocol.attribute.Attribute> {
                ProtocolHelper.CreateAttribute(!partyData.m_maker ? "d2" : "d1", deckId)
            };
            base.m_battleNet.Channel.UpdateChannelAttributes((ChannelAPI.ChannelData) channelReferenceObject.m_channelData, attributeList, null);
            if (partyData.m_maker)
            {
                partyData.m_makerDeck = deckId;
                if (deckId != 0)
                {
                    partyData.StartFriendlyChallengeGameIfReady();
                }
            }
        }
    }

    public void SetPartyPrivacy(EntityId partyId, int privacyLevel)
    {
        ChannelState state = new ChannelState {
            PrivacyLevel = (ChannelState.Types.PrivacyLevel) privacyLevel
        };
        this.UpdatePartyState_Internal("SetPartyPrivacy privacy=" + privacyLevel, BnetFeatureEvent.Party_SetPrivacy_Callback, partyId, state);
    }

    private void UpdatePartyState_Internal(string debugMessage, BnetFeatureEvent featureEvent, EntityId partyId, ChannelState state)
    {
        <UpdatePartyState_Internal>c__AnonStorey314 storey;
        storey = new <UpdatePartyState_Internal>c__AnonStorey314 {
            debugMessage = debugMessage,
            featureEvent = featureEvent,
            partyId = partyId,
            <>f__this = this,
            szPartyType = this.GetPartyType(storey.partyId)
        };
        if (this.GetPartyData(storey.partyId) == null)
        {
            this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_CHANNEL_NOT_MEMBER, string.Format("{0} no PartyData - assuming I'm not a member.", storey.debugMessage), storey.featureEvent, storey.partyId, storey.szPartyType);
        }
        else
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = base.m_battleNet.Channel.GetChannelReferenceObject(storey.partyId);
            if (channelReferenceObject == null)
            {
                this.GenericPartyRequestCallback_Internal(BattleNetErrors.ERROR_INVALID_ARGS, string.Format("{0} no channelRefObject", storey.debugMessage), storey.featureEvent, storey.partyId, storey.szPartyType);
            }
            else
            {
                UpdateChannelStateRequest message = new UpdateChannelStateRequest {
                    StateChange = state
                };
                base.m_rpcConnection.QueueRequest(base.m_battleNet.Channel.ChannelService.Id, 4, message, new RPCContextDelegate(storey.<>m__12F), (uint) channelReferenceObject.m_channelData.m_objectId);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AcceptPartyInvite>c__AnonStorey30E
    {
        internal PartyAPI <>f__this;
        internal ulong invitationId;
        internal string szPartyType;

        internal void <>m__129(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "AcceptPartyInvite inviteId=" + this.invitationId, BnetFeatureEvent.Party_AcceptInvite_Callback, null, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <AssignPartyRole>c__AnonStorey30C
    {
        internal PartyAPI <>f__this;
        internal EntityId memberId;
        internal EntityId partyId;
        internal uint roleId;
        internal string szPartyType;

        internal void <>m__127(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, string.Concat(new object[] { "AssignPartyRole memberId=", this.memberId.Low, " roleId=", this.roleId }), BnetFeatureEvent.Party_AssignRole_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <CreateParty>c__AnonStorey308
    {
        internal PartyAPI.PartyData partyData;
        internal string szPartyType;

        internal void <>m__123(RPCContext ctx)
        {
            this.partyData.CreateChannelCallback(ctx, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <DeclinePartyInvite>c__AnonStorey30F
    {
        internal PartyAPI <>f__this;
        internal ulong invitationId;
        internal string szPartyType;

        internal void <>m__12A(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "DeclinePartyInvite inviteId=" + this.invitationId, BnetFeatureEvent.Party_DeclineInvite_Callback, null, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <DissolveParty>c__AnonStorey30B
    {
        internal PartyAPI <>f__this;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__126(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "DissolveParty", BnetFeatureEvent.Party_Dissolve_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <GetAllPartyAttributes>c__AnonStorey315
    {
        internal EntityId partyId;

        internal bool <>m__130(ChannelAPI.ReceivedInvite i)
        {
            return (((i.ChannelId != null) && (i.ChannelId.High == this.partyId.High)) && (i.ChannelId.Low == this.partyId.Low));
        }
    }

    [CompilerGenerated]
    private sealed class <GetReceivedInvitationAttribute>c__AnonStorey306
    {
        internal EntityId partyId;

        internal bool <>m__121(ChannelAPI.ReceivedInvite i)
        {
            return (((i.ChannelId != null) && (i.ChannelId.High == this.partyId.High)) && (i.ChannelId.Low == this.partyId.Low));
        }
    }

    [CompilerGenerated]
    private sealed class <JoinParty>c__AnonStorey309
    {
        internal PartyAPI.PartyData partyData;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__124(RPCContext ctx)
        {
            this.partyData.JoinChannelCallback(ctx, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <KickPartyMember>c__AnonStorey312
    {
        internal PartyAPI <>f__this;
        internal EntityId memberId;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__12D(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "KickPartyMember memberId=" + this.memberId.Low, BnetFeatureEvent.Party_KickMember_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <LeaveParty>c__AnonStorey30A
    {
        internal PartyAPI <>f__this;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__125(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "LeaveParty", BnetFeatureEvent.Party_Leave_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <RequestPartyInvite>c__AnonStorey311
    {
        internal PartyAPI <>f__this;
        internal EntityId partyId;
        internal string szPartyType;
        internal EntityId whomToAskForApproval;
        internal EntityId whomToInvite;

        internal void <>m__12C(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, string.Concat(new object[] { "RequestPartyInvite whomToInvite=", this.whomToInvite, " whomToAskForApproval=", this.whomToAskForApproval }), BnetFeatureEvent.Party_RequestPartyInvite_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <RevokePartyInvite>c__AnonStorey310
    {
        internal PartyAPI <>f__this;
        internal ulong invitationId;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__12B(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "RevokePartyInvite inviteId=" + this.invitationId, BnetFeatureEvent.Party_RevokeInvite_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <SendFriendlyChallengeInvite>c__AnonStorey307
    {
        internal PartyAPI.PartyData partyData;

        internal void <>m__122(RPCContext ctx)
        {
            this.partyData.CreateChannelCallback(ctx, "FriendlyGame");
        }
    }

    [CompilerGenerated]
    private sealed class <SendPartyChatMessage>c__AnonStorey313
    {
        internal PartyAPI <>f__this;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__12E(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, "SendPartyChatMessage", BnetFeatureEvent.Party_SendChatMessage_Callback, this.partyId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <SendPartyInvite>c__AnonStorey30D
    {
        internal PartyAPI <>f__this;
        internal EntityId inviteeId;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__128(RPCContext ctx)
        {
            this.<>f__this.SendPartyInviteCallback(ctx, this.partyId, this.inviteeId, this.szPartyType);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdatePartyState_Internal>c__AnonStorey314
    {
        internal PartyAPI <>f__this;
        internal string debugMessage;
        internal BnetFeatureEvent featureEvent;
        internal EntityId partyId;
        internal string szPartyType;

        internal void <>m__12F(RPCContext ctx)
        {
            this.<>f__this.GenericPartyRequestCallback(ctx, this.debugMessage, this.featureEvent, this.partyId, this.szPartyType);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PartyCreateOptions
    {
        public string m_name;
        public ChannelState.Types.PrivacyLevel m_privacyLevel;
    }

    public class PartyData
    {
        private BattleNetCSharp m_battleNet;
        public EntityId m_friendGameAccount;
        public long m_inviteeDeck;
        public bool m_maker;
        public long m_makerDeck;
        public EntityId m_partyId;
        public int m_scenarioId;
        public ulong m_subscriberObjectId;

        public PartyData(BattleNetCSharp battlenet)
        {
            this.m_battleNet = battlenet;
        }

        public PartyData(BattleNetCSharp battlenet, EntityId friendGameAccount)
        {
            this.m_friendGameAccount = friendGameAccount;
            this.m_maker = true;
            this.m_battleNet = battlenet;
        }

        public PartyData(BattleNetCSharp battlenet, EntityId partyId, EntityId friendGameAccount)
        {
            this.m_partyId = partyId;
            this.m_friendGameAccount = friendGameAccount;
            this.m_battleNet = battlenet;
        }

        public void CreateChannelCallback(RPCContext context, string szPartyType)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            this.m_battleNet.Party.PushPartyErrorEvent(BattleNet.PartyListenerEventType.OPERATION_CALLBACK, "CreateParty", status, BnetFeatureEvent.Party_Create_Callback, null, szPartyType);
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_battleNet.Party.ApiLog.LogError("CreateChannelCallback: code=" + new bnet.Error(status));
                this.m_battleNet.Party.PushPartyError(new EntityId(), status, BnetFeatureEvent.Party_Create_Callback);
            }
            else
            {
                CreateChannelResponse response = CreateChannelResponse.ParseFrom(context.Payload);
                this.m_partyId = response.ChannelId;
                ChannelAPI.ChannelData channelData = new ChannelAPI.ChannelData(this.m_battleNet.Channel, response.ChannelId, response.ObjectId, ChannelAPI.ChannelType.PARTY_CHANNEL);
                channelData.SetSubscriberObjectId(this.m_subscriberObjectId);
                this.m_battleNet.Party.AddActiveChannel(this.m_subscriberObjectId, new ChannelAPI.ChannelReferenceObject(channelData), this);
                if (this.m_friendGameAccount != null)
                {
                    this.m_battleNet.Channel.SendInvitation(response.ChannelId, this.m_friendGameAccount, ChannelAPI.InvitationServiceType.INVITATION_SERVICE_TYPE_PARTY, new RPCContextDelegate(this.CreateParty_SendInvitationCallback));
                }
                this.m_battleNet.Party.ApiLog.LogDebug("CreateChannelCallback code=" + new bnet.Error(status));
            }
        }

        private void CreateParty_SendInvitationCallback(RPCContext context)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_battleNet.Party.ApiLog.LogError("SendInvitationCallback: " + status.ToString());
                this.m_battleNet.Party.PushPartyError(this.m_partyId, status, BnetFeatureEvent.Party_SendInvite_Callback);
            }
            else
            {
                SendInvitationResponse response = SendInvitationResponse.ParseFrom(context.Payload);
                if (response.Invitation.HasChannelInvitation && (response.Invitation.ChannelInvitation.ServiceType == 1))
                {
                    this.m_battleNet.Party.PushPartyEvent(this.m_partyId, "cb", "inv", this.m_friendGameAccount);
                }
                this.m_battleNet.Party.ApiLog.LogDebug("SendInvitationCallback, status=" + status.ToString());
            }
        }

        public void DeclineInvite_DissolvePartyInviteCallback(RPCContext context)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_battleNet.Party.ApiLog.LogError("DisolvePartyInviteCallback: " + status.ToString());
                this.m_battleNet.Party.PushPartyError(this.m_partyId, status, BnetFeatureEvent.Party_Dissolve_Callback);
            }
            else
            {
                if (this.m_friendGameAccount != null)
                {
                    this.m_battleNet.Party.PushPartyEvent(this.m_partyId, "cb", "drop", this.m_friendGameAccount);
                }
                this.m_battleNet.Party.ApiLog.LogDebug("DisolvePartyInviteCallback, status=" + status.ToString());
            }
        }

        public void FriendlyChallenge_HandleChannelAttributeUpdate(IList<bnet.protocol.attribute.Attribute> attributeList)
        {
            IEnumerator<bnet.protocol.attribute.Attribute> enumerator = attributeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    bnet.protocol.attribute.Attribute current = enumerator.Current;
                    if (current.Value.HasIntValue)
                    {
                        if (current.Name == "d2")
                        {
                            this.m_inviteeDeck = current.Value.IntValue;
                            this.StartFriendlyChallengeGameIfReady();
                        }
                    }
                    else
                    {
                        if (current.Value.HasStringValue)
                        {
                            this.m_battleNet.Party.PushPartyEvent(this.m_partyId, current.Name, current.Value.StringValue, this.m_friendGameAccount);
                            continue;
                        }
                        this.m_battleNet.Party.ApiLog.LogError("Party : unknown value type, key: " + current.Name);
                    }
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }

        public void JoinChannelCallback(RPCContext context, EntityId partyId, string szPartyType)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            this.m_battleNet.Party.PushPartyErrorEvent(BattleNet.PartyListenerEventType.OPERATION_CALLBACK, "JoinParty", status, BnetFeatureEvent.Party_Join_Callback, partyId, szPartyType);
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_battleNet.Party.ApiLog.LogError("JoinChannelCallback: code=" + new bnet.Error(status));
                this.m_battleNet.Party.PushPartyError(new EntityId(), status, BnetFeatureEvent.Party_Join_Callback);
            }
            else
            {
                JoinChannelResponse response = JoinChannelResponse.ParseFrom(context.Payload);
                ChannelAPI.ChannelData channelData = new ChannelAPI.ChannelData(this.m_battleNet.Channel, this.m_partyId, response.ObjectId, ChannelAPI.ChannelType.PARTY_CHANNEL);
                channelData.SetSubscriberObjectId(this.m_subscriberObjectId);
                this.m_battleNet.Party.AddActiveChannel(this.m_subscriberObjectId, new ChannelAPI.ChannelReferenceObject(channelData), this);
                this.m_battleNet.Party.ApiLog.LogDebug("JoinChannelCallback cide=" + new bnet.Error(status));
            }
        }

        public void PartyMemberLeft_DissolvePartyInviteCallback(RPCContext context)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                this.m_battleNet.Party.ApiLog.LogError("PartyMemberLeft_DissolvePartyInviteCallback: " + status.ToString());
                this.m_battleNet.Party.PushPartyError(this.m_partyId, status, BnetFeatureEvent.Party_Dissolve_Callback);
            }
            else
            {
                this.m_battleNet.Party.ApiLog.LogDebug("PartyMemberLeft_DissolvePartyInviteCallback, status=" + status.ToString());
            }
        }

        public void SetSubscriberObjectId(ulong objectId)
        {
            this.m_subscriberObjectId = objectId;
        }

        public void StartFriendlyChallengeGameIfReady()
        {
            ChannelAPI.ChannelReferenceObject channelReferenceObject = this.m_battleNet.Channel.GetChannelReferenceObject(this.m_partyId);
            if ((((channelReferenceObject != null) && this.m_maker) && (this.m_makerDeck != 0)) && (this.m_inviteeDeck != 0))
            {
                List<bnet.protocol.attribute.Attribute> attributeList = new List<bnet.protocol.attribute.Attribute> {
                    ProtocolHelper.CreateAttribute("s1", "goto"),
                    ProtocolHelper.CreateAttribute("s2", "goto")
                };
                this.m_battleNet.Channel.UpdateChannelAttributes((ChannelAPI.ChannelData) channelReferenceObject.m_channelData, attributeList, null);
                this.m_battleNet.Games.CreateFriendlyChallengeGame(this.m_makerDeck, this.m_inviteeDeck, this.m_friendGameAccount, this.m_scenarioId);
                this.m_makerDeck = 0L;
                this.m_inviteeDeck = 0L;
            }
        }
    }
}

