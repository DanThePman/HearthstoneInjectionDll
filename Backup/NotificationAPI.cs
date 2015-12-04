using bnet.protocol.attribute;
using bnet.protocol.notification;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NotificationAPI : BattleNetAPI
{
    private List<BnetNotification> m_notifications;

    public NotificationAPI(BattleNetCSharp battlenet) : base(battlenet, "Notification")
    {
        this.m_notifications = new List<BnetNotification>();
    }

    public void ClearNotifications()
    {
        this.m_notifications.Clear();
    }

    public int GetNotificationCount()
    {
        return this.m_notifications.Count;
    }

    public void GetNotifications([Out] BnetNotification[] Notifications)
    {
        this.m_notifications.CopyTo(Notifications, 0);
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

    public void OnNotification(string notificationType, bnet.protocol.notification.Notification notification)
    {
        if (notification.AttributeCount > 0)
        {
            BnetNotification item = new BnetNotification(notificationType);
            SortedDictionary<string, int> dictionary = new SortedDictionary<string, int>();
            int num = 0;
            item.MessageType = 0;
            item.MessageSize = 0;
            for (int i = 0; i < notification.AttributeCount; i++)
            {
                bnet.protocol.attribute.Attribute attribute = notification.Attribute[i];
                if (attribute.Name == "message_type")
                {
                    item.MessageType = (int) attribute.Value.IntValue;
                }
                else if (attribute.Name == "message_size")
                {
                    item.MessageSize = (int) attribute.Value.IntValue;
                }
                else if (attribute.Name.StartsWith("fragment_"))
                {
                    num += attribute.Value.BlobValue.Length;
                    dictionary.Add(attribute.Name, i);
                }
            }
            if (item.MessageType == 0)
            {
                Debug.LogError(string.Format("Missing notification type {0} of size {1}", item.MessageType, item.MessageSize));
            }
            else
            {
                if (0 < num)
                {
                    byte[] blobValue;
                    item.BlobMessage = new byte[num];
                    SortedDictionary<string, int>.Enumerator enumerator = dictionary.GetEnumerator();
                    for (int j = 0; enumerator.MoveNext(); j += blobValue.Length)
                    {
                        KeyValuePair<string, int> current = enumerator.Current;
                        blobValue = notification.Attribute[current.Value].Value.BlobValue;
                        Array.Copy(blobValue, 0, item.BlobMessage, j, blobValue.Length);
                    }
                }
                if (item.MessageSize != num)
                {
                    Debug.LogError(string.Format("Message size mismatch for notification type {0} - {1} != {2}", item.MessageType, item.MessageSize, num));
                }
                else
                {
                    this.m_notifications.Add(item);
                }
            }
        }
    }
}

