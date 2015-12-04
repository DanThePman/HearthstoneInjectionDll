using bnet.protocol;
using bnet.protocol.attribute;
using bnet.protocol.notification;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WhisperAPI : BattleNetAPI
{
    private List<BnetWhisper> m_whispers;

    public WhisperAPI(BattleNetCSharp battlenet) : base(battlenet, "Whisper")
    {
        this.m_whispers = new List<BnetWhisper>();
    }

    public void ClearWhispers()
    {
        this.m_whispers.Clear();
    }

    public void GetWhisperInfo(ref BattleNet.DllWhisperInfo info)
    {
        info.whisperSize = this.m_whispers.Count;
    }

    public void GetWhispers([Out] BnetWhisper[] whispers)
    {
        this.m_whispers.CopyTo(whispers, 0);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void InitRPCListeners(RPCConnection rpcConnection)
    {
        base.InitRPCListeners(rpcConnection);
    }

    public override void OnDisconnected()
    {
        base.OnDisconnected();
    }

    public void OnWhisper(bnet.protocol.notification.Notification notification)
    {
        if (notification.HasSenderId && (notification.AttributeCount > 0))
        {
            BnetWhisper item = new BnetWhisper();
            item.SetSpeakerId(BnetGameAccountId.CreateFromProtocol(notification.SenderId));
            item.SetReceiverId(BnetGameAccountId.CreateFromProtocol(notification.TargetId));
            for (int i = 0; i < notification.AttributeCount; i++)
            {
                bnet.protocol.attribute.Attribute attribute = notification.Attribute[i];
                if (attribute.Name == "whisper")
                {
                    item.SetMessage(attribute.Value.StringValue);
                }
            }
            if (!string.IsNullOrEmpty(item.GetMessage()))
            {
                TimeSpan elapsedTimeSinceEpoch = TimeUtils.GetElapsedTimeSinceEpoch(null);
                item.SetTimestampMilliseconds(elapsedTimeSinceEpoch.TotalMilliseconds);
                this.m_whispers.Add(item);
            }
        }
    }

    public void SendWhisper(BnetGameAccountId gameAccount, string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            bnet.protocol.notification.Notification notification = new bnet.protocol.notification.Notification();
            notification.SetType("WHISPER");
            EntityId val = new EntityId();
            val.SetLow(gameAccount.GetLo());
            val.SetHigh(gameAccount.GetHi());
            notification.SetTargetId(val);
            bnet.protocol.attribute.Attribute attribute = new bnet.protocol.attribute.Attribute();
            attribute.SetName("whisper");
            bnet.protocol.attribute.Variant variant = new bnet.protocol.attribute.Variant();
            variant.SetStringValue(message);
            attribute.SetValue(variant);
            notification.AddAttribute(attribute);
            base.m_rpcConnection.QueueRequest(base.m_battleNet.NotificationService.Id, 1, notification, new RPCContextDelegate(this.WhisperSentCallback), 0);
            BnetGameAccountId id = BnetGameAccountId.CreateFromDll(BattleNet.GetMyGameAccountId());
            BnetWhisper item = new BnetWhisper();
            item.SetSpeakerId(id);
            item.SetReceiverId(gameAccount);
            item.SetMessage(message);
            TimeSpan elapsedTimeSinceEpoch = TimeUtils.GetElapsedTimeSinceEpoch(null);
            item.SetTimestampMilliseconds(elapsedTimeSinceEpoch.TotalMilliseconds);
            this.m_whispers.Add(item);
        }
    }

    private void WhisperSentCallback(RPCContext context)
    {
        BattleNetErrors status = (BattleNetErrors) context.Header.Status;
        if (status != BattleNetErrors.ERROR_OK)
        {
            base.ApiLog.LogWarning("Battle.net Whisper API C#: Failed to SendWhisper. " + status);
            base.m_battleNet.EnqueueErrorInfo(BnetFeature.Whisper, BnetFeatureEvent.Whisper_OnSend, status, context.Context);
        }
    }
}

