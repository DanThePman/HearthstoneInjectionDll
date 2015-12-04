using bnet.protocol;
using bnet.protocol.attribute;
using bnet.protocol.presence;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class PresenceAPI : BattleNetAPI
{
    private const float COST_PER_REQUEST = 1f;
    private const float CREDIT_LIMIT = -100f;
    private long m_lastPresenceSubscriptionSent;
    private ServiceDescriptor m_presenceService;
    private float m_presenceSubscriptionBalance;
    private Map<EntityId, PresenceRefCountObject> m_presenceSubscriptions;
    private List<BattleNet.PresenceUpdate> m_presenceUpdates;
    private HashSet<EntityId> m_queuedSubscriptions;
    private Stopwatch m_stopWatch;
    private const float PAYDOWN_RATE_PER_MS = 0.003333333f;

    public PresenceAPI(BattleNetCSharp battlenet) : base(battlenet, "Presence")
    {
        this.m_queuedSubscriptions = new HashSet<EntityId>();
        this.m_presenceSubscriptions = new Map<EntityId, PresenceRefCountObject>();
        this.m_presenceUpdates = new List<BattleNet.PresenceUpdate>();
        this.m_presenceService = new RPCServices.PresenceService();
        this.m_stopWatch = new Stopwatch();
    }

    public void ClearPresence()
    {
        this.m_presenceUpdates.Clear();
    }

    public void GetPresence([Out] BattleNet.PresenceUpdate[] updates)
    {
        this.m_presenceUpdates.CopyTo(updates);
    }

    public void HandlePresenceUpdates(ChannelState channelState, ChannelAPI.ChannelReferenceObject channelRef)
    {
        BattleNet.DllEntityId id;
        id.hi = channelRef.m_channelData.m_channelId.High;
        id.lo = channelRef.m_channelData.m_channelId.Low;
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.BNET.GetValue());
        key.SetGroup(1);
        key.SetField(3);
        FieldKey key2 = key;
        List<BattleNet.PresenceUpdate> collection = new List<BattleNet.PresenceUpdate>();
        foreach (FieldOperation operation in channelState.FieldOperationList)
        {
            BattleNet.PresenceUpdate item = new BattleNet.PresenceUpdate {
                entityId = id,
                programId = operation.Field.Key.Program,
                groupId = operation.Field.Key.Group,
                fieldId = operation.Field.Key.Field,
                index = operation.Field.Key.Index,
                boolVal = false,
                intVal = 0L,
                stringVal = string.Empty,
                valCleared = false,
                blobVal = new byte[0]
            };
            if (operation.Operation == FieldOperation.Types.OperationType.CLEAR)
            {
                item.valCleared = true;
                bool flag = key2.Program == operation.Field.Key.Program;
                bool flag2 = key2.Group == operation.Field.Key.Group;
                bool flag3 = key2.Field == operation.Field.Key.Field;
                if ((flag && flag2) && flag3)
                {
                    BnetEntityId entityId = BnetEntityId.CreateFromDll(item.entityId);
                    base.m_battleNet.Friends.RemoveFriendsActiveGameAccount(entityId, operation.Field.Key.Index);
                }
            }
            else if (operation.Field.Value.HasBoolValue)
            {
                item.boolVal = operation.Field.Value.BoolValue;
            }
            else if (operation.Field.Value.HasIntValue)
            {
                item.intVal = operation.Field.Value.IntValue;
            }
            else if (operation.Field.Value.HasStringValue)
            {
                item.stringVal = operation.Field.Value.StringValue;
            }
            else if (operation.Field.Value.HasFourccValue)
            {
                item.stringVal = new BnetProgramId(operation.Field.Value.FourccValue).ToString();
            }
            else if (operation.Field.Value.HasEntityidValue)
            {
                item.entityIdVal.hi = operation.Field.Value.EntityidValue.High;
                item.entityIdVal.lo = operation.Field.Value.EntityidValue.Low;
                bool flag4 = key2.Program == operation.Field.Key.Program;
                bool flag5 = key2.Group == operation.Field.Key.Group;
                bool flag6 = key2.Field == operation.Field.Key.Field;
                if ((flag4 && flag5) && flag6)
                {
                    BnetEntityId id3 = BnetEntityId.CreateFromDll(item.entityId);
                    base.m_battleNet.Friends.AddFriendsActiveGameAccount(id3, operation.Field.Value.EntityidValue, operation.Field.Key.Index);
                }
            }
            else
            {
                if (!operation.Field.Value.HasBlobValue)
                {
                    continue;
                }
                item.blobVal = operation.Field.Value.BlobValue;
            }
            collection.Add(item);
        }
        collection.Reverse();
        this.m_presenceUpdates.AddRange(collection);
    }

    private void HandleSubscriptionRequests()
    {
        if (this.m_queuedSubscriptions.Count > 0)
        {
            long elapsedMilliseconds = this.m_stopWatch.ElapsedMilliseconds;
            this.m_presenceSubscriptionBalance = Math.Min((float) 0f, (float) (this.m_presenceSubscriptionBalance + ((elapsedMilliseconds - this.m_lastPresenceSubscriptionSent) * 0.003333333f)));
            this.m_lastPresenceSubscriptionSent = elapsedMilliseconds;
            List<EntityId> list = new List<EntityId>();
            foreach (EntityId id in this.m_queuedSubscriptions)
            {
                if ((this.m_presenceSubscriptionBalance - 1f) < -100f)
                {
                    break;
                }
                PresenceRefCountObject obj2 = this.m_presenceSubscriptions[id];
                SubscribeRequest message = new SubscribeRequest();
                message.SetObjectId(ChannelAPI.GetNextObjectId());
                message.SetEntityId(id);
                obj2.objectId = message.ObjectId;
                base.m_battleNet.Channel.AddActiveChannel(message.ObjectId, new ChannelAPI.ChannelReferenceObject(id, ChannelAPI.ChannelType.PRESENCE_CHANNEL));
                base.m_rpcConnection.QueueRequest(this.m_presenceService.Id, 1, message, new RPCContextDelegate(this.PresenceSubscribeCallback), 0);
                this.m_presenceSubscriptionBalance--;
                list.Add(id);
            }
            foreach (EntityId id2 in list)
            {
                this.m_queuedSubscriptions.Remove(id2);
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        this.m_stopWatch.Start();
        this.m_lastPresenceSubscriptionSent = 0L;
        this.m_presenceSubscriptionBalance = 0f;
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
        this.m_presenceSubscriptions.Clear();
        this.m_presenceUpdates.Clear();
        this.m_queuedSubscriptions.Clear();
        this.m_stopWatch.Stop();
        this.m_lastPresenceSubscriptionSent = 0L;
        this.m_presenceSubscriptionBalance = 0f;
    }

    public int PresenceSize()
    {
        return this.m_presenceUpdates.Count;
    }

    public void PresenceSubscribe(EntityId entityId)
    {
        if (this.m_presenceSubscriptions.ContainsKey(entityId))
        {
            PresenceRefCountObject local1 = this.m_presenceSubscriptions[entityId];
            local1.refCount++;
        }
        else
        {
            PresenceRefCountObject obj2 = new PresenceRefCountObject {
                objectId = 0L,
                refCount = 1
            };
            this.m_presenceSubscriptions.Add(entityId, obj2);
            this.m_queuedSubscriptions.Add(entityId);
            this.HandleSubscriptionRequests();
        }
    }

    private void PresenceSubscribeCallback(RPCContext context)
    {
        base.CheckRPCCallback("PresenceSubscribeCallback", context);
    }

    public void PresenceUnsubscribe(EntityId entityId)
    {
        if (this.m_presenceSubscriptions.ContainsKey(entityId))
        {
            PresenceRefCountObject local1 = this.m_presenceSubscriptions[entityId];
            local1.refCount--;
            if (this.m_presenceSubscriptions[entityId].refCount <= 0)
            {
                if (this.m_queuedSubscriptions.Contains(entityId))
                {
                    this.m_queuedSubscriptions.Remove(entityId);
                }
                else
                {
                    PresenceUnsubscribeContext context = new PresenceUnsubscribeContext(base.m_battleNet, this.m_presenceSubscriptions[entityId].objectId);
                    UnsubscribeRequest message = new UnsubscribeRequest();
                    message.SetEntityId(entityId);
                    base.m_rpcConnection.QueueRequest(this.m_presenceService.Id, 2, message, new RPCContextDelegate(context.PresenceUnsubscribeCallback), 0);
                    this.m_presenceSubscriptions.Remove(entityId);
                }
            }
        }
    }

    private void PresenceUpdateCallback(RPCContext context)
    {
        base.CheckRPCCallback("PresenceUpdateCallback", context);
    }

    public override void Process()
    {
        base.Process();
        this.HandleSubscriptionRequests();
    }

    public void PublishField(UpdateRequest updateRequest)
    {
        base.m_rpcConnection.QueueRequest(this.m_presenceService.Id, 3, updateRequest, new RPCContextDelegate(this.PresenceUpdateCallback), 0);
    }

    public void PublishRichPresence([In] BattleNet.DllRichPresenceUpdate[] updates)
    {
        UpdateRequest updateRequest = new UpdateRequest {
            EntityId = base.m_battleNet.GameAccountId
        };
        FieldOperation val = new FieldOperation();
        Field field = new Field();
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.BNET.GetValue());
        key.SetGroup(2);
        key.SetField(8);
        foreach (BattleNet.DllRichPresenceUpdate update in updates)
        {
            key.SetIndex(update.presenceFieldIndex);
            bnet.protocol.presence.RichPresence protobuf = new bnet.protocol.presence.RichPresence();
            protobuf.SetIndex(update.index);
            protobuf.SetProgramId(update.programId);
            protobuf.SetStreamId(update.streamId);
            bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
            variant.SetMessageValue(ProtobufUtil.ToByteArray(protobuf));
            field.SetKey(key);
            field.SetValue(variant);
            val.SetField(field);
            updateRequest.SetEntityId(base.m_battleNet.GameAccountId);
            updateRequest.AddFieldOperation(val);
        }
        this.PublishField(updateRequest);
    }

    public void RequestPresenceFields(bool isGameAccountEntityId, [In] BattleNet.DllEntityId entityId, [In] BattleNet.DllPresenceFieldKey[] fieldList)
    {
        <RequestPresenceFields>c__AnonStorey316 storey = new <RequestPresenceFields>c__AnonStorey316 {
            entityId = entityId,
            <>f__this = this
        };
        QueryRequest message = new QueryRequest();
        EntityId val = new EntityId();
        val.SetHigh(storey.entityId.hi);
        val.SetLow(storey.entityId.lo);
        message.SetEntityId(val);
        foreach (BattleNet.DllPresenceFieldKey key in fieldList)
        {
            FieldKey key2 = new FieldKey();
            key2.SetProgram(key.programId);
            key2.SetGroup(key.groupId);
            key2.SetField(key.fieldId);
            key2.SetIndex(key.index);
            message.AddKey(key2);
        }
        base.m_rpcConnection.QueueRequest(this.m_presenceService.Id, 4, message, new RPCContextDelegate(storey.<>m__131), 0);
    }

    private void RequestPresenceFieldsCallback(BattleNet.DllEntityId entityId, RPCContext context)
    {
        if (base.CheckRPCCallback("RequestPresenceFieldsCallback", context))
        {
            foreach (Field field in QueryResponse.ParseFrom(context.Payload).FieldList)
            {
                BattleNet.PresenceUpdate item = new BattleNet.PresenceUpdate {
                    entityId = entityId,
                    programId = field.Key.Program,
                    groupId = field.Key.Group,
                    fieldId = field.Key.Field,
                    index = field.Key.Index,
                    boolVal = false,
                    intVal = 0L,
                    stringVal = string.Empty,
                    valCleared = false,
                    blobVal = new byte[0]
                };
                if (field.Value.HasBoolValue)
                {
                    item.boolVal = field.Value.BoolValue;
                }
                else if (field.Value.HasIntValue)
                {
                    item.intVal = field.Value.IntValue;
                }
                else if (!field.Value.HasFloatValue)
                {
                    if (field.Value.HasStringValue)
                    {
                        item.stringVal = field.Value.StringValue;
                    }
                    else if (field.Value.HasBlobValue)
                    {
                        item.blobVal = field.Value.BlobValue;
                    }
                    else if (field.Value.HasMessageValue)
                    {
                        item.blobVal = field.Value.MessageValue;
                    }
                    else if (field.Value.HasFourccValue)
                    {
                        item.stringVal = new BnetProgramId(field.Value.FourccValue).ToString();
                    }
                    else if (!field.Value.HasUintValue)
                    {
                        if (field.Value.HasEntityidValue)
                        {
                            item.entityIdVal.hi = field.Value.EntityidValue.High;
                            item.entityIdVal.lo = field.Value.EntityidValue.Low;
                        }
                        else
                        {
                            item.valCleared = true;
                        }
                    }
                }
                this.m_presenceUpdates.Add(item);
            }
        }
    }

    public void SetPresenceBlob(uint field, byte[] val)
    {
        UpdateRequest updateRequest = new UpdateRequest {
            EntityId = base.m_battleNet.GameAccountId
        };
        FieldOperation operation = new FieldOperation();
        Field field2 = new Field();
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.HEARTHSTONE.GetValue());
        key.SetGroup(2);
        key.SetField(field);
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        if (val == null)
        {
            val = new byte[0];
        }
        variant.SetBlobValue(val);
        field2.SetKey(key);
        field2.SetValue(variant);
        operation.SetField(field2);
        updateRequest.SetEntityId(base.m_battleNet.GameAccountId);
        updateRequest.AddFieldOperation(operation);
        this.PublishField(updateRequest);
    }

    public void SetPresenceBool(uint field, bool val)
    {
        UpdateRequest updateRequest = new UpdateRequest();
        FieldOperation operation = new FieldOperation();
        Field field2 = new Field();
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.HEARTHSTONE.GetValue());
        key.SetGroup(2);
        key.SetField(field);
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetBoolValue(val);
        field2.SetKey(key);
        field2.SetValue(variant);
        operation.SetField(field2);
        updateRequest.SetEntityId(base.m_battleNet.GameAccountId);
        updateRequest.AddFieldOperation(operation);
        this.PublishField(updateRequest);
    }

    public void SetPresenceInt(uint field, long val)
    {
        UpdateRequest updateRequest = new UpdateRequest {
            EntityId = base.m_battleNet.GameAccountId
        };
        FieldOperation operation = new FieldOperation();
        Field field2 = new Field();
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.HEARTHSTONE.GetValue());
        key.SetGroup(2);
        key.SetField(field);
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetIntValue(val);
        field2.SetKey(key);
        field2.SetValue(variant);
        operation.SetField(field2);
        updateRequest.SetEntityId(base.m_battleNet.GameAccountId);
        updateRequest.AddFieldOperation(operation);
        this.PublishField(updateRequest);
    }

    public void SetPresenceString(uint field, string val)
    {
        UpdateRequest updateRequest = new UpdateRequest {
            EntityId = base.m_battleNet.GameAccountId
        };
        FieldOperation operation = new FieldOperation();
        Field field2 = new Field();
        FieldKey key = new FieldKey();
        key.SetProgram(BnetProgramId.HEARTHSTONE.GetValue());
        key.SetGroup(2);
        key.SetField(field);
        bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
        variant.SetStringValue(val);
        field2.SetKey(key);
        field2.SetValue(variant);
        operation.SetField(field2);
        updateRequest.SetEntityId(base.m_battleNet.GameAccountId);
        updateRequest.AddFieldOperation(operation);
        this.PublishField(updateRequest);
    }

    public ServiceDescriptor PresenceService
    {
        get
        {
            return this.m_presenceService;
        }
    }

    [CompilerGenerated]
    private sealed class <RequestPresenceFields>c__AnonStorey316
    {
        internal PresenceAPI <>f__this;
        internal BattleNet.DllEntityId entityId;

        internal void <>m__131(RPCContext context)
        {
            this.<>f__this.RequestPresenceFieldsCallback(new BattleNet.DllEntityId(this.entityId), context);
        }
    }

    public class PresenceRefCountObject
    {
        public ulong objectId;
        public int refCount;
    }

    public class PresenceUnsubscribeContext
    {
        private BattleNetCSharp m_battleNet;
        private ulong m_objectId;

        public PresenceUnsubscribeContext(BattleNetCSharp battleNet, ulong objectId)
        {
            this.m_battleNet = battleNet;
            this.m_objectId = objectId;
        }

        public void PresenceUnsubscribeCallback(RPCContext context)
        {
            if (this.m_battleNet.Presence.CheckRPCCallback("PresenceUnsubscribeCallback", context))
            {
                this.m_battleNet.Channel.RemoveActiveChannel(this.m_objectId);
            }
        }
    }
}

