using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChatLog : MonoBehaviour
{
    [CompilerGenerated]
    private static Func<ITouchListItem, MobileChatLogMessageFrame> <>f__am$cache7;
    public GameObject cameraTarget;
    private const int maxMessageFrames = 500;
    public TouchList messageFrames;
    public MessageInfo messageInfo;
    private const GameLayer messageLayer = GameLayer.BattleNetChat;
    private Camera messagesCamera;
    public MobileChatNotification notifications;
    public Prefabs prefabs;
    private BnetPlayer receiver;

    private void AddMyMessage(string message)
    {
        string str = ChatUtils.GetMessage(message);
        MobileChatLogMessageFrame item = this.CreateMessage(this.prefabs.myMessage, str);
        this.messageFrames.Add(item);
        this.OnMessagesAdded();
    }

    private void AddOfflineMessage()
    {
        object[] args = new object[] { this.receiver.GetBestName() };
        string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_OFFLINE", args);
        this.AddSystemMessage(message, this.messageInfo.errorColor);
    }

    private void AddOnlineMessage()
    {
        object[] args = new object[] { this.receiver.GetBestName() };
        string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_ONLINE", args);
        this.AddSystemMessage(message, this.messageInfo.infoColor);
    }

    private void AddSystemMessage(string message, Color color)
    {
        MobileChatLogMessageFrame item = this.CreateMessage(this.prefabs.systemMessage, message, color);
        this.messageFrames.Add(item);
        this.OnMessagesAdded();
    }

    private void AddWhisperMessage(BnetWhisper whisper)
    {
        string message = ChatUtils.GetMessage(whisper);
        MobileChatLogMessageFrame prefab = !whisper.IsSpeaker(this.receiver) ? this.prefabs.myMessage : this.prefabs.theirMessage;
        MobileChatLogMessageFrame item = this.CreateMessage(prefab, message);
        this.messageFrames.Add(item);
    }

    [Conditional("CHATLOG_DEBUG")]
    private void AssignMessageFrameNames()
    {
        for (int i = 0; i < this.messageFrames.Count; i++)
        {
            MobileChatLogMessageFrame component = this.messageFrames[i].GetComponent<MobileChatLogMessageFrame>();
            component.name = string.Format("MessageFrame {0} ({1})", i, component.Message);
        }
    }

    private void Awake()
    {
        this.CreateMessagesCamera();
        if (this.notifications != null)
        {
            this.notifications.Notified += new MobileChatNotification.NotifiedEvent(this.OnNotified);
        }
        BnetWhisperMgr.Get().AddWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
    }

    private MobileChatLogMessageFrame CreateMessage(MobileChatLogMessageFrame prefab, string message)
    {
        MobileChatLogMessageFrame c = UnityEngine.Object.Instantiate<MobileChatLogMessageFrame>(prefab);
        c.Width = (this.messageFrames.ClipSize.x - this.messageFrames.padding.x) - 10f;
        c.Message = message;
        SceneUtils.SetLayer(c, GameLayer.BattleNetChat);
        return c;
    }

    private MobileChatLogMessageFrame CreateMessage(MobileChatLogMessageFrame prefab, string message, Color color)
    {
        MobileChatLogMessageFrame frame = this.CreateMessage(prefab, message);
        frame.Color = color;
        return frame;
    }

    private void CreateMessagesCamera()
    {
        this.messagesCamera = new GameObject("MessagesCamera") { transform = { parent = this.messageFrames.transform, localPosition = new Vector3(0f, 0f, -100f) } }.AddComponent<Camera>();
        this.messagesCamera.orthographic = true;
        this.messagesCamera.depth = BnetBar.CameraDepth + 1;
        this.messagesCamera.clearFlags = CameraClearFlags.Depth;
        this.messagesCamera.cullingMask = GameLayer.BattleNetChat.LayerBit();
        this.UpdateMessagesCamera();
    }

    private Bounds GetBoundsFromGameObject(GameObject go)
    {
        Renderer component = go.GetComponent<Renderer>();
        if (component != null)
        {
            return component.bounds;
        }
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }
        return new Bounds();
    }

    private void OnDestroy()
    {
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetWhisperMgr.Get().RemoveWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        if (this.notifications != null)
        {
            this.notifications.Notified -= new MobileChatNotification.NotifiedEvent(this.OnNotified);
        }
    }

    private void OnMessagesAdded()
    {
        if (this.messageFrames.Count > 500)
        {
            ITouchListItem item = this.messageFrames[0];
            this.messageFrames.RemoveAt(0);
            UnityEngine.Object.Destroy(item.gameObject);
        }
        this.messageFrames.ScrollValue = 1f;
    }

    private void OnNotified(string text)
    {
        this.AddSystemMessage(text, this.messageInfo.notificationColor);
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        BnetPlayerChange change = changelist.FindChange(this.receiver);
        if (change != null)
        {
            BnetPlayer oldPlayer = change.GetOldPlayer();
            BnetPlayer newPlayer = change.GetNewPlayer();
            if ((oldPlayer == null) || (oldPlayer.IsOnline() != newPlayer.IsOnline()))
            {
                if (newPlayer.IsOnline())
                {
                    this.AddOnlineMessage();
                }
                else
                {
                    this.AddOfflineMessage();
                }
            }
        }
    }

    public void OnResize()
    {
        this.UpdateMessagesCamera();
    }

    private void OnWhisper(BnetWhisper whisper, object userData)
    {
        if ((this.receiver != null) && whisper.IsSpeakerOrReceiver(this.receiver))
        {
            this.AddWhisperMessage(whisper);
            this.messageFrames.ScrollValue = 1f;
        }
    }

    public void OnWhisperFailed()
    {
        this.AddOfflineMessage();
    }

    private void UpdateMessages()
    {
        if (<>f__am$cache7 == null)
        {
            <>f__am$cache7 = i => i.GetComponent<MobileChatLogMessageFrame>();
        }
        List<MobileChatLogMessageFrame> list = Enumerable.Select<ITouchListItem, MobileChatLogMessageFrame>(this.messageFrames, <>f__am$cache7).ToList<MobileChatLogMessageFrame>();
        this.messageFrames.Clear();
        foreach (MobileChatLogMessageFrame frame in list)
        {
            UnityEngine.Object.Destroy(frame.gameObject);
        }
        List<BnetWhisper> whispersWithPlayer = BnetWhisperMgr.Get().GetWhispersWithPlayer(this.receiver);
        if ((whispersWithPlayer != null) && (whispersWithPlayer.Count > 0))
        {
            for (int j = Mathf.Max(whispersWithPlayer.Count - 500, 0); j < whispersWithPlayer.Count; j++)
            {
                BnetWhisper whisper = whispersWithPlayer[j];
                this.AddWhisperMessage(whisper);
            }
        }
        this.OnMessagesAdded();
    }

    private void UpdateMessagesCamera()
    {
        Camera bnetCamera = BaseUI.Get().GetBnetCamera();
        Bounds boundsFromGameObject = this.GetBoundsFromGameObject(this.cameraTarget);
        Vector3 vector = bnetCamera.WorldToScreenPoint(boundsFromGameObject.min);
        Vector3 vector2 = bnetCamera.WorldToScreenPoint(boundsFromGameObject.max);
        this.messagesCamera.pixelRect = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
        this.messagesCamera.orthographicSize = this.messagesCamera.rect.height * bnetCamera.orthographicSize;
    }

    public BnetPlayer Receiver
    {
        get
        {
            return this.receiver;
        }
        set
        {
            if (this.receiver != value)
            {
                this.receiver = value;
                if (this.receiver != null)
                {
                    this.UpdateMessages();
                    if (!this.receiver.IsOnline())
                    {
                        this.AddOfflineMessage();
                    }
                    this.messageFrames.ScrollValue = 1f;
                }
            }
        }
    }

    [Serializable]
    public class MessageInfo
    {
        public Color errorColor = Color.red;
        public Color infoColor = Color.yellow;
        public Color notificationColor = Color.cyan;
    }

    [Serializable]
    public class Prefabs
    {
        public MobileChatLogMessageFrame myMessage;
        public MobileChatLogMessageFrame systemMessage;
        public MobileChatLogMessageFrame theirMessage;
    }
}

