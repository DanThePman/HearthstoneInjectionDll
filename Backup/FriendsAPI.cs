using bnet.protocol;
using bnet.protocol.friends;
using bnet.protocol.invitation;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FriendsAPI : BattleNetAPI
{
    private Map<BnetEntityId, Map<ulong, EntityId>> m_friendEntityId;
    private uint m_friendsCount;
    private ServiceDescriptor m_friendsNotifyService;
    private ServiceDescriptor m_friendsService;
    private float m_initializeTimeOut;
    private uint m_maxFriends;
    private uint m_maxReceivedInvitations;
    private uint m_maxSentInvitations;
    private FriendsAPIState m_state;
    private float m_subscribeTimeout;
    private List<BattleNet.FriendsUpdate> m_updateList;

    public FriendsAPI(BattleNetCSharp battlenet) : base(battlenet, "Friends")
    {
        this.m_friendsService = new RPCServices.FriendsService();
        this.m_friendsNotifyService = new FriendsNotify();
        this.m_initializeTimeOut = 5f;
        this.m_updateList = new List<BattleNet.FriendsUpdate>();
        this.m_friendEntityId = new Map<BnetEntityId, Map<ulong, EntityId>>();
    }

    private void AcceptInvitation(ulong inviteId)
    {
        GenericRequest request = new GenericRequest();
        request.SetInvitationId(inviteId);
        GenericRequest message = request;
        if (!message.IsInitialized)
        {
            base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to AcceptInvitation.");
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnAcceptInvitation, BattleNetErrors.ERROR_API_NOT_READY, 0);
        }
        else
        {
            base.m_rpcConnection.QueueRequest(this.m_friendsService.Id, 3, message, new RPCContextDelegate(this.AcceptInvitationCallback), 0);
        }
    }

    private void AcceptInvitationCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to AcceptInvitation. " + status);
                base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnAcceptInvitation, status, context.Context);
            }
        }
    }

    private void AddFriendInternal(BnetEntityId entityId)
    {
        if (entityId != null)
        {
            BattleNet.FriendsUpdate item = new BattleNet.FriendsUpdate {
                action = 1,
                entity1 = entityId
            };
            this.m_updateList.Add(item);
            base.m_battleNet.Presence.PresenceSubscribe(BnetEntityId.CreateForProtocol(entityId));
            this.m_friendEntityId.Add(entityId, new Map<ulong, EntityId>());
            this.m_friendsCount = (uint) this.m_friendEntityId.Count;
        }
    }

    public bool AddFriendsActiveGameAccount(BnetEntityId entityId, EntityId gameAccount, ulong index)
    {
        if (!this.IsFriend(entityId))
        {
            return false;
        }
        if (!this.m_friendEntityId[entityId].ContainsKey(index))
        {
            this.m_friendEntityId[entityId].Add(index, gameAccount);
            base.m_battleNet.Presence.PresenceSubscribe(gameAccount);
        }
        return true;
    }

    private void AddInvitationInternal(BattleNet.FriendsUpdate.Action action, Invitation invitation, int reason)
    {
        if (invitation != null)
        {
            BattleNet.FriendsUpdate item = new BattleNet.FriendsUpdate {
                action = (int) action,
                long1 = invitation.Id,
                entity1 = this.GetBnetEntityIdFromIdentity(invitation.InviterIdentity)
            };
            if (invitation.HasInviterName)
            {
                item.string1 = invitation.InviterName;
            }
            item.entity2 = this.GetBnetEntityIdFromIdentity(invitation.InviteeIdentity);
            if (invitation.HasInviteeName)
            {
                item.string2 = invitation.InviteeName;
            }
            if (invitation.HasInvitationMessage)
            {
                item.string3 = invitation.InvitationMessage;
            }
            item.bool1 = false;
            if (invitation.HasCreationTime)
            {
                item.long2 = invitation.CreationTime;
            }
            if (invitation.HasExpirationTime)
            {
                item.long3 = invitation.ExpirationTime;
            }
            this.m_updateList.Add(item);
        }
    }

    public void ClearFriendsUpdates()
    {
        this.m_updateList.Clear();
    }

    private void DeclineInvitation(ulong inviteId)
    {
        GenericRequest request = new GenericRequest();
        request.SetInvitationId(inviteId);
        GenericRequest message = request;
        if (!message.IsInitialized)
        {
            base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to DeclineInvitation.");
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnDeclineInvitation, BattleNetErrors.ERROR_API_NOT_READY, 0);
        }
        else
        {
            base.m_rpcConnection.QueueRequest(this.m_friendsService.Id, 5, message, new RPCContextDelegate(this.DeclineInvitationCallback), 0);
        }
    }

    private void DeclineInvitationCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to DeclineInvitation. " + status);
                base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnDeclineInvitation, status, context.Context);
            }
        }
    }

    private BnetEntityId ExtractEntityIdFromFriendNotification(byte[] payload)
    {
        FriendNotification notification2 = FriendNotification.ParseFrom(payload);
        if (!notification2.IsInitialized)
        {
            return null;
        }
        return BnetEntityId.CreateFromProtocol(notification2.Target.Id);
    }

    private Invitation ExtractInvitationFromInvitationNotification(byte[] payload)
    {
        InvitationNotification notification2 = InvitationNotification.ParseFrom(payload);
        if (!notification2.IsInitialized)
        {
            return null;
        }
        return notification2.Invitation;
    }

    private BnetEntityId GetBnetEntityIdFromIdentity(Identity identity)
    {
        BnetEntityId id = new BnetEntityId();
        if (identity.HasAccountId)
        {
            id.SetLo(identity.AccountId.Low);
            id.SetHi(identity.AccountId.High);
            return id;
        }
        if (identity.HasGameAccountId)
        {
            id.SetLo(identity.GameAccountId.Low);
            id.SetHi(identity.GameAccountId.High);
            return id;
        }
        id.SetLo(0L);
        id.SetHi(0L);
        return id;
    }

    public bool GetFriendsActiveGameAccounts(BnetEntityId entityId, [Out] Map<ulong, EntityId> gameAccounts)
    {
        return this.m_friendEntityId.TryGetValue(entityId, out gameAccounts);
    }

    public void GetFriendsInfo(ref BattleNet.DllFriendsInfo info)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            info.maxFriends = (int) this.m_maxFriends;
            info.maxRecvInvites = (int) this.m_maxReceivedInvitations;
            info.maxSentInvites = (int) this.m_maxSentInvitations;
            info.friendsSize = (int) this.m_friendsCount;
            info.updateSize = this.m_updateList.Count;
        }
    }

    public void GetFriendsUpdates([Out] BattleNet.FriendsUpdate[] updates)
    {
        this.m_updateList.CopyTo(updates, 0);
    }

    public override void Initialize()
    {
        base.Initialize();
        if (this.m_state != FriendsAPIState.INITIALIZING)
        {
            this.StartInitialize();
            this.Subscribe();
        }
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
        rpcConnection.RegisterServiceMethodListener(this.m_friendsNotifyService.Id, 1, new RPCContextDelegate(this.NotifyFriendAddedListenerCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_friendsNotifyService.Id, 2, new RPCContextDelegate(this.NotifyFriendRemovedListenerCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_friendsNotifyService.Id, 3, new RPCContextDelegate(this.NotifyReceivedInvitationAddedCallback));
        rpcConnection.RegisterServiceMethodListener(this.m_friendsNotifyService.Id, 4, new RPCContextDelegate(this.NotifyReceivedInvitationRemovedCallback));
    }

    public bool IsFriend(BnetEntityId entityId)
    {
        return this.m_friendEntityId.ContainsKey(entityId);
    }

    public void ManageFriendInvite(int action, ulong inviteId)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            switch (((Network.InviteAction) action))
            {
                case Network.InviteAction.INVITE_ACCEPT:
                    this.AcceptInvitation(inviteId);
                    break;

                case Network.InviteAction.INVITE_DECLINE:
                    this.DeclineInvitation(inviteId);
                    break;
            }
        }
    }

    private void NotifyFriendAddedListenerCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BnetEntityId entityId = this.ExtractEntityIdFromFriendNotification(context.Payload);
            this.AddFriendInternal(entityId);
        }
    }

    private void NotifyFriendRemovedListenerCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BnetEntityId entityId = this.ExtractEntityIdFromFriendNotification(context.Payload);
            this.RemoveFriendInternal(entityId);
        }
    }

    private void NotifyReceivedInvitationAddedCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            Invitation invitation = this.ExtractInvitationFromInvitationNotification(context.Payload);
            this.AddInvitationInternal(BattleNet.FriendsUpdate.Action.FRIEND_INVITE, invitation, 0);
        }
    }

    private void NotifyReceivedInvitationRemovedCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            Invitation invitation = this.ExtractInvitationFromInvitationNotification(context.Payload);
            this.AddInvitationInternal(BattleNet.FriendsUpdate.Action.FRIEND_INVITE_REMOVED, invitation, 0);
        }
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    public override void Process()
    {
        base.Process();
        if (this.m_state == FriendsAPIState.INITIALIZING)
        {
            this.m_subscribeTimeout += UnityEngine.Time.deltaTime;
            if (this.m_subscribeTimeout >= this.InitializeTimeOut)
            {
                this.m_state = FriendsAPIState.FAILED_TO_INITIALIZE;
                base.ApiLog.LogWarning("Battle.net Friends API C#: Initialize timed out.");
            }
        }
    }

    private void ProcessSubscribeToFriendsResponse(SubscribeToFriendsResponse response)
    {
        if (response.HasMaxFriends)
        {
            this.m_maxFriends = response.MaxFriends;
        }
        if (response.HasMaxReceivedInvitations)
        {
            this.m_maxReceivedInvitations = response.MaxReceivedInvitations;
        }
        if (response.HasMaxSentInvitations)
        {
            this.m_maxSentInvitations = response.MaxSentInvitations;
        }
        for (int i = 0; i < response.FriendsCount; i++)
        {
            Friend friend = response.Friends[i];
            BnetEntityId entityId = new BnetEntityId();
            entityId.SetLo(friend.Id.Low);
            entityId.SetHi(friend.Id.High);
            this.AddFriendInternal(entityId);
        }
        for (int j = 0; j < response.ReceivedInvitationsCount; j++)
        {
            Invitation invitation = response.ReceivedInvitations[j];
            this.AddInvitationInternal(BattleNet.FriendsUpdate.Action.FRIEND_INVITE, invitation, 0);
        }
        for (int k = 0; k < response.SentInvitationsCount; k++)
        {
            Invitation invitation2 = response.SentInvitations[k];
            this.AddInvitationInternal(BattleNet.FriendsUpdate.Action.FRIEND_SENT_INVITE, invitation2, 0);
        }
    }

    public void RemoveFriend(BnetAccountId account)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            EntityId val = new EntityId();
            val.SetLow(account.GetLo());
            val.SetHigh(account.GetHi());
            GenericFriendRequest request = new GenericFriendRequest();
            request.SetTargetId(val);
            GenericFriendRequest message = request;
            if (!message.IsInitialized)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to RemoveFriend.");
                base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnRemoveFriend, BattleNetErrors.ERROR_API_NOT_READY, 0);
            }
            else
            {
                base.m_rpcConnection.QueueRequest(this.m_friendsService.Id, 8, message, new RPCContextDelegate(this.RemoveFriendCallback), 0);
            }
        }
    }

    private void RemoveFriendCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to RemoveFriend. " + status);
                base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnRemoveFriend, status, context.Context);
            }
        }
    }

    private void RemoveFriendInternal(BnetEntityId entityId)
    {
        if (entityId != null)
        {
            BattleNet.FriendsUpdate item = new BattleNet.FriendsUpdate {
                action = 2,
                entity1 = entityId
            };
            this.m_updateList.Add(item);
            base.m_battleNet.Presence.PresenceUnsubscribe(BnetEntityId.CreateForProtocol(entityId));
            if (this.m_friendEntityId.ContainsKey(entityId))
            {
                foreach (EntityId id in this.m_friendEntityId[entityId].Values)
                {
                    base.m_battleNet.Presence.PresenceUnsubscribe(id);
                }
                this.m_friendEntityId.Remove(entityId);
            }
            this.m_friendsCount = (uint) this.m_friendEntityId.Count;
        }
    }

    public void RemoveFriendsActiveGameAccount(BnetEntityId entityId, ulong index)
    {
        EntityId id;
        if (this.IsFriend(entityId) && this.m_friendEntityId[entityId].TryGetValue(index, out id))
        {
            base.m_battleNet.Presence.PresenceUnsubscribe(id);
            this.m_friendEntityId[entityId].Remove(index);
        }
    }

    public void SendFriendInvite(string sender, string target, bool byEmail)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            SendInvitationRequest request = new SendInvitationRequest();
            EntityId val = new EntityId();
            val.SetLow(0L);
            val.SetHigh(0L);
            request.SetTargetId(val);
            InvitationParams @params = new InvitationParams();
            FriendInvitationParams params2 = new FriendInvitationParams();
            if (byEmail)
            {
                params2.SetTargetEmail(target);
                params2.AddRole(2);
            }
            else
            {
                params2.SetTargetBattleTag(target);
                params2.AddRole(1);
            }
            @params.SetFriendParams(params2);
            request.SetParams(@params);
            SendInvitationRequest message = request;
            if (!message.IsInitialized)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to SendFriendInvite.");
            }
            else
            {
                base.m_rpcConnection.QueueRequest(this.m_friendsService.Id, 2, message, new RPCContextDelegate(this.SendInvitationCallback), 0);
            }
        }
    }

    private void SendInvitationCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZED)
        {
            BattleNetErrors status = (BattleNetErrors) context.Header.Status;
            if (status != BattleNetErrors.ERROR_OK)
            {
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to SendInvitation. " + status);
            }
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Friends, BnetFeatureEvent.Friends_OnSendInvitation, status, context.Context);
        }
    }

    private void StartInitialize()
    {
        this.m_subscribeTimeout = 0f;
        this.m_state = FriendsAPIState.INITIALIZING;
        this.m_maxFriends = 0;
        this.m_maxReceivedInvitations = 0;
        this.m_maxSentInvitations = 0;
        this.m_friendsCount = 0;
        this.m_updateList = new List<BattleNet.FriendsUpdate>();
        this.m_friendEntityId = new Map<BnetEntityId, Map<ulong, EntityId>>();
    }

    private void Subscribe()
    {
        SubscribeToFriendsRequest request = new SubscribeToFriendsRequest();
        request.SetObjectId(0L);
        SubscribeToFriendsRequest message = request;
        if (!message.IsInitialized)
        {
            base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to Subscribe.");
        }
        else
        {
            base.m_rpcConnection.QueueRequest(this.m_friendsService.Id, 1, message, new RPCContextDelegate(this.SubscribeToFriendsCallback), 0);
        }
    }

    private void SubscribeToFriendsCallback(RPCContext context)
    {
        if (this.m_state == FriendsAPIState.INITIALIZING)
        {
            if (context.Header.Status == 0)
            {
                this.m_state = FriendsAPIState.INITIALIZED;
                base.ApiLog.LogDebug("Battle.net Friends API C#: Initialized.");
                SubscribeToFriendsResponse response = SubscribeToFriendsResponse.ParseFrom(context.Payload);
                this.ProcessSubscribeToFriendsResponse(response);
            }
            else
            {
                this.m_state = FriendsAPIState.FAILED_TO_INITIALIZE;
                base.ApiLog.LogWarning("Battle.net Friends API C#: Failed to initialize.");
            }
        }
    }

    public ServiceDescriptor FriendsNotifyService
    {
        get
        {
            return this.m_friendsNotifyService;
        }
    }

    public ServiceDescriptor FriendsService
    {
        get
        {
            return this.m_friendsService;
        }
    }

    public float InitializeTimeOut
    {
        get
        {
            return this.m_initializeTimeOut;
        }
        set
        {
            this.m_initializeTimeOut = value;
        }
    }

    public bool IsInitialized
    {
        get
        {
            return ((this.m_state == FriendsAPIState.INITIALIZED) || (this.m_state == FriendsAPIState.FAILED_TO_INITIALIZE));
        }
    }

    private enum FriendsAPIState
    {
        NOT_SET,
        INITIALIZING,
        INITIALIZED,
        FAILED_TO_INITIALIZE
    }
}

