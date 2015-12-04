using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class ClientConnection<PacketType> where PacketType: PacketFormat, new()
{
    private static int BACKING_BUFFER_SIZE;
    private byte[] m_backingBuffer;
    private int m_backingBufferBytes;
    private List<ConnectHandler<PacketType>> m_connectHandlers;
    private List<ConnectionEvent<PacketType>> m_connectionEvents;
    private ConnectionState<PacketType> m_connectionState;
    private PacketType m_currentPacket;
    private List<DisconnectHandler<PacketType>> m_disconnectHandlers;
    private string m_hostAddress;
    private int m_hostPort;
    private List<IConnectionListener<PacketType>> m_listeners;
    private List<object> m_listenerStates;
    private object m_mutex;
    private int m_outPacketsInFlight;
    private Queue<PacketType> m_outQueue;
    private byte[] m_receiveBuffer;
    private Socket m_socket;
    private bool m_stolenSocket;
    private static int RECEIVE_BUFFER_SIZE;

    static ClientConnection()
    {
        ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE = 0x10000;
        ClientConnection<PacketType>.BACKING_BUFFER_SIZE = 0x40000;
    }

    public ClientConnection()
    {
        this.m_connectHandlers = new List<ConnectHandler<PacketType>>();
        this.m_disconnectHandlers = new List<DisconnectHandler<PacketType>>();
        this.m_outQueue = new Queue<PacketType>();
        this.m_listeners = new List<IConnectionListener<PacketType>>();
        this.m_listenerStates = new List<object>();
        this.m_connectionEvents = new List<ConnectionEvent<PacketType>>();
        this.m_mutex = new object();
        this.m_connectionState = ConnectionState<PacketType>.Disconnected;
        this.m_receiveBuffer = new byte[ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE];
        this.m_backingBuffer = new byte[ClientConnection<PacketType>.BACKING_BUFFER_SIZE];
        this.m_stolenSocket = false;
    }

    public ClientConnection(Socket socket)
    {
        this.m_connectHandlers = new List<ConnectHandler<PacketType>>();
        this.m_disconnectHandlers = new List<DisconnectHandler<PacketType>>();
        this.m_outQueue = new Queue<PacketType>();
        this.m_listeners = new List<IConnectionListener<PacketType>>();
        this.m_listenerStates = new List<object>();
        this.m_connectionEvents = new List<ConnectionEvent<PacketType>>();
        this.m_mutex = new object();
        this.m_socket = socket;
        this.m_connectionState = ConnectionState<PacketType>.Connected;
        this.m_receiveBuffer = new byte[ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE];
        this.m_stolenSocket = true;
        this.m_hostAddress = "local";
        this.m_hostPort = 0;
    }

    private void AddConnectEvent(BattleNetErrors error, Exception exception = null)
    {
        ConnectionEvent<PacketType> item = new ConnectionEvent<PacketType> {
            Type = ConnectionEventTypes<PacketType>.OnConnected,
            Error = error,
            Exception = exception
        };
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.m_connectionEvents.Add(item);
        }
    }

    public bool AddConnectHandler(ConnectHandler<PacketType> handler)
    {
        if (this.m_connectHandlers.Contains(handler))
        {
            return false;
        }
        this.m_connectHandlers.Add(handler);
        return true;
    }

    private void AddDisconnectEvent(BattleNetErrors error)
    {
        ConnectionEvent<PacketType> item = new ConnectionEvent<PacketType> {
            Type = ConnectionEventTypes<PacketType>.OnDisconnected,
            Error = error
        };
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.m_connectionEvents.Add(item);
        }
    }

    public bool AddDisconnectHandler(DisconnectHandler<PacketType> handler)
    {
        if (this.m_disconnectHandlers.Contains(handler))
        {
            return false;
        }
        this.m_disconnectHandlers.Add(handler);
        return true;
    }

    public void AddListener(IConnectionListener<PacketType> listener, object state)
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
                int num2 = ((num + ClientConnection<PacketType>.BACKING_BUFFER_SIZE) - 1) / ClientConnection<PacketType>.BACKING_BUFFER_SIZE;
                byte[] destinationArray = new byte[num2 * ClientConnection<PacketType>.BACKING_BUFFER_SIZE];
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
                this.m_currentPacket = Activator.CreateInstance<PacketType>();
            }
            int num = this.m_currentPacket.Decode(bytes, offset, nBytes);
            nBytes -= num;
            offset += num;
            if (this.m_currentPacket.IsLoaded())
            {
                ConnectionEvent<PacketType> item = new ConnectionEvent<PacketType> {
                    Type = ConnectionEventTypes<PacketType>.OnPacketCompleted,
                    Packet = this.m_currentPacket
                };
                object mutex = this.m_mutex;
                lock (mutex)
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
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(host), port);
        this.DisconnectSocket();
        this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.m_connectionState = ConnectionState<PacketType>.Connecting;
        try
        {
            this.m_socket.BeginConnect(remoteEP, new AsyncCallback(this.ConnectCallback), null);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not connect to " + this.HostAddress + " -- " + exception.Message);
            this.m_connectionState = ConnectionState<PacketType>.ConnectionFailed;
            this.AddConnectEvent(BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, null);
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        bool flag = false;
        Exception exception = null;
        try
        {
            this.m_socket.EndConnect(ar);
            this.m_socket.BeginReceive(this.m_receiveBuffer, 0, ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
        }
        catch (Exception exception2)
        {
            exception = exception2;
            flag = true;
        }
        if (flag || !this.m_socket.Connected)
        {
            this.AddConnectEvent(BattleNetErrors.ERROR_RPC_PEER_UNAVAILABLE, exception);
        }
        else
        {
            this.AddConnectEvent(BattleNetErrors.ERROR_OK, null);
        }
    }

    public void Disconnect()
    {
        this.DisconnectSocket();
        this.m_connectionState = ConnectionState<PacketType>.Disconnected;
    }

    private void DisconnectSocket()
    {
        if ((this.m_socket != null) && this.m_socket.Connected)
        {
            this.m_socket.Close();
        }
    }

    ~ClientConnection()
    {
        if (this.m_socket != null)
        {
            this.m_socket.Close();
        }
    }

    public bool HasEvents()
    {
        return (this.m_connectionEvents.Count > 0);
    }

    public bool HasOutPacketsInFlight()
    {
        return (this.m_outPacketsInFlight > 0);
    }

    public bool HasQueuedPackets()
    {
        return (this.m_outQueue.Count > 0);
    }

    private void OnSendBytes(IAsyncResult ar)
    {
        try
        {
            this.m_socket.EndSend(ar);
        }
        catch (Exception)
        {
            this.AddDisconnectEvent(BattleNetErrors.ERROR_RPC_CONNECTION_TIMED_OUT);
        }
    }

    private void OnSendPacket(IAsyncResult ar)
    {
        this.OnSendBytes(ar);
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.m_outPacketsInFlight--;
        }
    }

    private void PrintConnectionException(ConnectionEvent<PacketType> connectionEvent)
    {
        Exception exception = connectionEvent.Exception;
        if (exception != null)
        {
            object[] args = new object[] { exception.Message, this.m_hostAddress, this.m_hostPort, exception.StackTrace };
            Debug.LogError(string.Format("ClientConnection Exception - {0} - {1}:{2}\n{3}", args));
        }
    }

    public void QueuePacket(PacketType packet)
    {
        this.m_outQueue.Enqueue(packet);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        if (this.m_socket.Connected)
        {
            try
            {
                int nBytes = this.m_socket.EndReceive(ar);
                if (nBytes > 0)
                {
                    this.BytesReceived(nBytes);
                    this.m_socket.BeginReceive(this.m_receiveBuffer, 0, ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
                }
                else
                {
                    this.AddDisconnectEvent(BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED);
                }
            }
            catch (Exception exception)
            {
                Log.Net.Print(exception.ToString(), new object[0]);
                this.AddDisconnectEvent(BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED);
            }
        }
        else
        {
            this.AddDisconnectEvent(BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED);
        }
    }

    public bool RemoveConnectHandler(ConnectHandler<PacketType> handler)
    {
        return this.m_connectHandlers.Remove(handler);
    }

    public bool RemoveDisconnectHandler(DisconnectHandler<PacketType> handler)
    {
        return this.m_disconnectHandlers.Remove(handler);
    }

    public void RemoveListener(IConnectionListener<PacketType> listener)
    {
        while (this.m_listeners.Remove(listener))
        {
        }
    }

    public bool SendBytes(byte[] bytes, AsyncCallback callback, object userData)
    {
        if (bytes.Length == 0)
        {
            return false;
        }
        if (!this.m_socket.Connected)
        {
            return false;
        }
        bool flag = false;
        try
        {
            this.m_socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, callback, userData);
            flag = true;
        }
        catch (Exception)
        {
        }
        return flag;
    }

    public bool SendPacket(PacketType packet)
    {
        byte[] buffer = packet.Encode();
        if (buffer.Length == 0)
        {
            return false;
        }
        if (!this.m_socket.Connected)
        {
            return false;
        }
        object mutex = this.m_mutex;
        lock (mutex)
        {
            this.m_outPacketsInFlight++;
        }
        bool flag = false;
        try
        {
            this.m_socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnSendPacket), null);
            flag = true;
        }
        catch (Exception)
        {
            object obj3 = this.m_mutex;
            lock (obj3)
            {
                this.m_outPacketsInFlight--;
            }
        }
        return flag;
    }

    public void SendQueuedPackets()
    {
        while (this.m_outQueue.Count > 0)
        {
            PacketType packet = this.m_outQueue.Dequeue();
            this.SendPacket(packet);
        }
    }

    public bool SendString(string str)
    {
        byte[] bytes = new ASCIIEncoding().GetBytes(str);
        return this.SendBytes(bytes, new AsyncCallback(this.OnSendBytes), null);
    }

    public void StartReceiving()
    {
        if (!this.m_stolenSocket)
        {
            Debug.LogError("StartReceiving should only be called on sockets created with ClientConnection(Socket)");
        }
        else
        {
            try
            {
                this.m_socket.BeginReceive(this.m_receiveBuffer, 0, ClientConnection<PacketType>.RECEIVE_BUFFER_SIZE, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception exception)
            {
                Debug.LogError("error receiving from local connection: " + exception.Message);
            }
        }
    }

    public void Update()
    {
        object mutex = this.m_mutex;
        lock (mutex)
        {
            foreach (ConnectionEvent<PacketType> event2 in this.m_connectionEvents)
            {
                ConnectHandler<PacketType>[] handlerArray;
                int num3;
                IConnectionListener<PacketType> listener;
                this.PrintConnectionException(event2);
                switch (event2.Type)
                {
                    case ConnectionEventTypes<PacketType>.OnConnected:
                        if (event2.Error == BattleNetErrors.ERROR_OK)
                        {
                            break;
                        }
                        this.DisconnectSocket();
                        this.m_connectionState = ConnectionState<PacketType>.ConnectionFailed;
                        goto Label_0071;

                    case ConnectionEventTypes<PacketType>.OnDisconnected:
                    {
                        if (event2.Error != BattleNetErrors.ERROR_OK)
                        {
                            this.Disconnect();
                        }
                        foreach (DisconnectHandler<PacketType> handler2 in this.m_disconnectHandlers.ToArray())
                        {
                            handler2(event2.Error);
                        }
                        continue;
                    }
                    case ConnectionEventTypes<PacketType>.OnPacketCompleted:
                        num3 = 0;
                        goto Label_0138;

                    default:
                    {
                        continue;
                    }
                }
                this.m_connectionState = ConnectionState<PacketType>.Connected;
            Label_0071:
                handlerArray = this.m_connectHandlers.ToArray();
                for (int i = 0; i < handlerArray.Length; i++)
                {
                    ConnectHandler<PacketType> handler = handlerArray[i];
                    handler(event2.Error);
                }
                continue;
            Label_0105:
                listener = this.m_listeners[num3];
                object state = this.m_listenerStates[num3];
                listener.PacketReceived(event2.Packet, state);
                num3++;
            Label_0138:
                if (num3 < this.m_listeners.Count)
                {
                    goto Label_0105;
                }
            }
            this.m_connectionEvents.Clear();
        }
        if ((this.m_socket != null) && (this.m_connectionState == ConnectionState<PacketType>.Connected))
        {
            this.SendQueuedPackets();
        }
    }

    public bool Active
    {
        get
        {
            if (this.m_socket == null)
            {
                return false;
            }
            return (this.m_socket.Connected || (this.m_connectionState == ConnectionState<PacketType>.Connecting));
        }
    }

    public string HostAddress
    {
        get
        {
            return (this.m_hostAddress + ":" + this.m_hostPort);
        }
    }

    public ConnectionState<PacketType> State
    {
        get
        {
            return this.m_connectionState;
        }
    }

    public delegate void ConnectHandler(BattleNetErrors error);

    private class ConnectionEvent
    {
        public BattleNetErrors Error { get; set; }

        public System.Exception Exception { get; set; }

        public PacketType Packet { get; set; }

        public ClientConnection<PacketType>.ConnectionEventTypes Type { get; set; }
    }

    private enum ConnectionEventTypes
    {
        public const ClientConnection<PacketType>.ConnectionEventTypes OnConnected = ClientConnection<PacketType>.ConnectionEventTypes.OnConnected;,
        public const ClientConnection<PacketType>.ConnectionEventTypes OnDisconnected = ClientConnection<PacketType>.ConnectionEventTypes.OnDisconnected;,
        public const ClientConnection<PacketType>.ConnectionEventTypes OnPacketCompleted = ClientConnection<PacketType>.ConnectionEventTypes.OnPacketCompleted;
    }

    public enum ConnectionState
    {
        public const ClientConnection<PacketType>.ConnectionState Connected = ClientConnection<PacketType>.ConnectionState.Connected;,
        public const ClientConnection<PacketType>.ConnectionState Connecting = ClientConnection<PacketType>.ConnectionState.Connecting;,
        public const ClientConnection<PacketType>.ConnectionState ConnectionFailed = ClientConnection<PacketType>.ConnectionState.ConnectionFailed;,
        public const ClientConnection<PacketType>.ConnectionState Disconnected = ClientConnection<PacketType>.ConnectionState.Disconnected;
    }

    public delegate void DisconnectHandler(BattleNetErrors error);
}

