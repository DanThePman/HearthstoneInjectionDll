using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct BnetNotification
{
    public const string NotificationType_UtilNotificationMessage = "WTCG.UtilNotificationMessage";
    public const string NotificationAttribute_MessageType = "message_type";
    public const string NotificationAttribute_MessageSize = "message_size";
    public const string NotificationAttribute_MessageFragmentPrefix = "fragment_";
    public string NotificationType;
    public byte[] BlobMessage;
    public int MessageType;
    public int MessageSize;
    public BnetNotification(string notificationType)
    {
        this.NotificationType = notificationType;
        this.BlobMessage = new byte[0];
        this.MessageType = 0;
        this.MessageSize = 0;
    }

    public static BnetNotification CreateFromDll(BattleNet.DllNotification src)
    {
        BnetNotification notification = new BnetNotification {
            NotificationType = MemUtils.StringFromUtf8Ptr(src.notificationType),
            MessageType = src.messageId,
            MessageSize = src.blobSize,
            BlobMessage = MemUtils.PtrToBytes(src.blobMessage, src.blobSize)
        };
        if (notification.BlobMessage == null)
        {
            notification.BlobMessage = new byte[0];
        }
        return notification;
    }
}

