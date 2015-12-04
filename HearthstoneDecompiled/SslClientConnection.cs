using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SslClientConnection
{
    private static int BACKING_BUFFER_SIZE = 0x40000;
    private const float BLOCKING_SEND_TIME_OUT = 1f;
    private byte[] m_backingBuffer = new byte[BACKING_BUFFER_SIZE];
    private int m_backingBufferBytes;
    private SslCertBundleInfo m_bundleInfo;
    private List<ConnectHandler> m_connectHandlers = new List<ConnectHandler>();
    private List<ConnectionEvent> m_connectionEvents = new List<ConnectionEvent>();
    private ConnectionState m_connectionState = ConnectionState.Disconnected;
    private BattleNetPacket m_currentPacket;
    private List<DisconnectHandler> m_disconnectHandlers = new List<DisconnectHandler>();
    private string m_hostAddress;
    private int m_hostPort;
    private List<ISslConnectionListener> m_listeners = new List<ISslConnectionListener>();
    private List<object> m_listenerStates = new List<object>();
    private Queue<BattleNetPacket> m_outQueue = new Queue<BattleNetPacket>();
    private byte[] m_receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
    private SslSocket m_sslSocket;
    private static int RECEIVE_BUFFER_SIZE = 0x40000;

    public SslClientConnection(SslCertBundleInfo bundleInfo)
    {
        this.m_bundleInfo = bundleInfo;
    }

    public bool AddConnectHandler(ConnectHandler handler)
    {
        if (this.m_connectHandlers.Contains(handler))
        {
            return false;
        }
        this.m_connectHandlers.Add(handler);
        return true;
    }

    public bool AddDisconnectHandler(DisconnectHandler handler)
    {
        if (this.m_disconnectHandlers.Contains(handler))
        {
            return false;
        }
        this.m_disconnectHandlers.Add(handler);
        return true;
    }

    public void AddListener(ISslConnectionListener listener, object state)
    {
        this.m_listeners.Add(listener);
        this.m_listenerStates.Add(state);
    }

    private void BytesReceived(int nBytes)
    {
        if (this.m_backingBufferBytes > 0)
        {
            int num = this.m_backingBufferBytes + nBytes;
            if (num > this.m_backingBuffer.Length)
            {
                int num2 = ((num + BACKING_BUFFER_SIZE) - 1) / BACKING_BUFFER_SIZE;
                byte[] destinationArray = new byte[num2 * BACKING_BUFFER_SIZE];
                Array.Copy(this.m_backingBuffer, 0, destinationArray, 0, this.m_backingBuffer.Length);
                this.m_backingBuffer = destinationArray;
            }
            Array.Copy(this.m_receiveBuffer, 0, this.m_backingBuffer, this.m_backingBufferBytes, nBytes);
            this.m_backingBufferBytes = 0;
            this.BytesReceived(this.m_backingBuffer, num, 0);
        }
        else
        {
            this.BytesReceived(this.m_receiveBuffer, nBytes, 0);
        }
    }

    private void BytesReceived(byte[] bytes, int nBytes, int offset)
    {
        while (nBytes > 0)
        {
            if (this.m_currentPacket == null)
            {
                this.m_currentPacket = new BattleNetPacket();
            }
            int num = this.m_currentPacket.Decode(bytes, offset, nBytes);
            nBytes -= num;
            offset += num;
            if (this.m_currentPacket.IsLoaded())
            {
                ConnectionEvent item = new ConnectionEvent {
                    Type = ConnectionEventTypes.OnPacketCompleted,
                    Packet = this.m_currentPacket
                };
                List<ConnectionEvent> connectionEvents = this.m_connectionEvents;
                lock (connectionEvents)
                {
                    this.m_connectionEvents.Add(item);
                }
                this.m_currentPacket = null;
            }
            else
            {
                Array.Copy(bytes, offset, this.m_backingBuffer, 0, nBytes);
                this.m_backingBufferBytes = nBytes;
                return;
            }
        }
        this.m_backingBufferBytes = 0;
    }

    public void Connect(string host, int port)
    {
        this.m_hostAddress = host;
        this.m_hostPort = port;
        this.Disconnect();
        this.m_sslSocket = new SslSocket();
        this.m_connectionState = ConnectionState.Connecting;
        try
        {
            this.m_sslSocket.BeginConnect(host, port, this.m_bundleInfo, new SslSocket.BeginConnectDelegate(this.ConnectCallback));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not connect to " + (this.m_hostAddress + ":" + this.m_hostPort) + " -- " + exception.Message);
            this.m_connectionState = ConnectionState.ConnectionFailed;
            this.TriggerOnConnectandler(BattleNetErrors.ERROR_RPC_PEER_UNKNOWN);
        }
        this.m_bundleInfo = null;
    }

    private void ConnectCallback(bool connectFailed, bool isEncrypted, bool isSigned)
    {
        if (!connectFailed)
        {
            try
            {
                this.m_sslSocket.BeginReceive(this.m_receiveBuffer, RECEIVE_BUFFER_SIZE, new SslSocket.BeginReceiveDelegate(this.ReceiveCallback));
            }
            catch (Exception)
            {
                connectFailed = true;
            }
        }
        if (connectFailed || !this.m_sslSocket.Connected)
        {
            this.TriggerOnConnectandler(BattleNetErrors.ERROR_RPC_PEER_UNAVAILABLE);
        }
        else
        {
            this.TriggerOnConnectandler(BattleNetErrors.ERROR_OK);
        }
    }

    public void Disconnect()
    {
        if (this.m_sslSocket != null)
        {
            this.m_sslSocket.Close();
            this.m_sslSocket = null;
        }
        this.m_connectionState = ConnectionState.Disconnected;
    }

    ~SslClientConnection()
    {
        if (this.m_sslSocket != null)
        {
            this.Disconnect();
        }
    }

    public void QueuePacket(BattleNetPacket packet)
    {
        this.m_outQueue.Enqueue(packet);
    }

    private void ReceiveCallback(int bytesReceived)
    {
        if ((bytesReceived == 0) || !this.m_sslSocket.Connected)
        {
            Debug.Log("disconnected");
            this.TriggerOnDisconnectHandler(BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED);
        }
        else if ((this.m_sslSocket != null) && this.m_sslSocket.Connected)
        {
            try
            {
                if (bytesReceived > 0)
                {
                    this.BytesReceived(bytesReceived);
                    this.m_sslSocket.BeginReceive(this.m_receiveBuffer, RECEIVE_BUFFER_SIZE, new SslSocket.BeginReceiveDelegate(this.ReceiveCallback));
                }
            }
            catch (Exception exception)
            {
                Debug.Log("disconnected " + exception);
                this.TriggerOnDisconnectHandler(BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED);
            }
        }
    }

    public bool RemoveConnectHandler(ConnectHandler handler)
    {
        return this.m_connectHandlers.Remove(handler);
    }

    public bool RemoveDisconnectHandler(DisconnectHandler handler)
    {
        return this.m_disconnectHandlers.Remove(handler);
    }

    public void RemoveListener(ISslConnectionListener listener)
    {
        while (this.m_listeners.Remove(listener))
        {
        }
    }

    private void SendBytes(byte[] bytes)
    {
        if (bytes.Length > 0)
        {
            <SendBytes>c__AnonStorey319 storey = new <SendBytes>c__AnonStorey319 {
                <>f__this = this,
                block = this.BlockOnSend,
                blockLock = new object()
            };
            this.m_sslSocket.BeginSend(bytes, new SslSocket.BeginSendDelegate(storey.<>m__137));
            if (this.BlockOnSend)
            {
                float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
                while ((UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup) < 1f)
                {
                    object blockLock = storey.blockLock;
                    lock (blockLock)
                    {
                        if (!storey.block)
                        {
                            break;
                        }
                        continue;
                    }
                }
            }
        }
    }

    public void SendPacket(BattleNetPacket packet)
    {
        byte[] bytes = packet.Encode();
        this.SendBytes(bytes);
    }

    private void TriggerOnConnectandler(BattleNetErrors error)
    {
        ConnectionEvent item = new ConnectionEvent {
            Type = ConnectionEventTypes.OnConnected,
            Error = error
        };
        List<ConnectionEvent> connectionEvents = this.m_connectionEvents;
        lock (connectionEvents)
        {
            this.m_connectionEvents.Add(item);
        }
    }

    private void TriggerOnDisconnectHandler(BattleNetErrors error)
    {
        ConnectionEvent item = new ConnectionEvent {
            Type = ConnectionEventTypes.OnDisconnected,
            Error = error
        };
        List<ConnectionEvent> connectionEvents = this.m_connectionEvents;
        lock (connectionEvents)
        {
            this.m_connectionEvents.Add(item);
        }
    }

    public void Update()
    {
        SslSocket.Process();
        List<ConnectionEvent> connectionEvents = this.m_connectionEvents;
        lock (connectionEvents)
        {
            if (this.m_connectionEvents.Count > 0)
            {
                foreach (ConnectionEvent event2 in this.m_connectionEvents)
                {
                    ConnectHandler[] handlerArray;
                    int num3;
                    ISslConnectionListener listener;
                    switch (event2.Type)
                    {
                        case ConnectionEventTypes.OnConnected:
                            if (event2.Error == BattleNetErrors.ERROR_OK)
                            {
                                break;
                            }
                            this.Disconnect();
                            this.m_connectionState = ConnectionState.ConnectionFailed;
                            goto Label_0080;

                        case ConnectionEventTypes.OnDisconnected:
                        {
                            if (event2.Error != BattleNetErrors.ERROR_OK)
                            {
                                this.Disconnect();
                            }
                            foreach (DisconnectHandler handler2 in this.m_disconnectHandlers.ToArray())
                            {
                                handler2(event2.Error);
                            }
                            continue;
                        }
                        case ConnectionEventTypes.OnPacketCompleted:
                            num3 = 0;
                            goto Label_0147;

                        default:
                        {
                            continue;
                        }
                    }
                    this.m_connectionState = ConnectionState.Connected;
                Label_0080:
                    handlerArray = this.m_connectHandlers.ToArray();
                    for (int i = 0; i < handlerArray.Length; i++)
                    {
                        ConnectHandler handler = handlerArray[i];
                        handler(event2.Error);
                    }
                    continue;
                Label_0114:
                    listener = this.m_listeners[num3];
                    object state = this.m_listenerStates[num3];
                    listener.PacketReceived(event2.Packet, state);
                    num3++;
                Label_0147:
                    if (num3 < this.m_listeners.Count)
                    {
                        goto Label_0114;
                    }
                }
                this.m_connectionEvents.Clear();
            }
        }
        if ((this.m_sslSocket != null) && (this.m_connectionState == ConnectionState.Connected))
        {
            while (this.m_outQueue.Count > 0)
            {
                if (this.OnlyOneSend && !this.m_sslSocket.m_canSend)
                {
                    return;
                }
                BattleNetPacket packet = this.m_outQueue.Dequeue();
                this.SendPacket(packet);
            }
        }
    }

    public bool Active
    {
        get
        {
            return this.m_sslSocket.Connected;
        }
    }

    public bool BlockOnSend { get; set; }

    public bool OnlyOneSend { get; set; }

    [CompilerGenerated]
    private sealed class <SendBytes>c__AnonStorey319
    {
        internal SslClientConnection <>f__this;
        internal bool block;
        internal object blockLock;

        internal void <>m__137(bool wasSent)
        {
            if (!wasSent)
            {
                this.<>f__this.TriggerOnDisconnectHandler(BattleNetErrors.ERROR_RPC_CONNECTION_TIMED_OUT);
            }
            object blockLock = this.blockLock;
            lock (blockLock)
            {
                this.block = false;
            }
        }
    }

    public delegate void ConnectHandler(BattleNetErrors error);

    private class ConnectionEvent
    {
        public BattleNetErrors Error { get; set; }

        public BattleNetPacket Packet { get; set; }

        public SslClientConnection.ConnectionEventTypes Type { get; set; }
    }

    private enum ConnectionEventTypes
    {
        OnConnected,
        OnDisconnected,
        OnPacketCompleted
    }

    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        ConnectionFailed,
        Connected
    }

    public delegate void DisconnectHandler(BattleNetErrors error);
}

