using bnet.protocol;
using bnet.protocol.connection;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class RPCConnection : IConnectionListener<BattleNetPacket>
{
    private ClientConnection<BattleNetPacket> Connection;
    private Queue<BattleNetPacket> incomingPackets = new Queue<BattleNetPacket>();
    private BattleNetLogSource m_cmLogSource = new BattleNetLogSource("ConnectionMetering");
    private RPCConnectionMetering m_connMetering = new RPCConnectionMetering();
    private BattleNetLogSource m_logSource = new BattleNetLogSource("Network");
    private OnConnectHandler m_onConnectHandler;
    private OnDisconectHandler m_onDisconnectHandler;
    private List<BattleNetPacket> m_pendingOutboundPackets = new List<BattleNetPacket>();
    private Stopwatch m_stopWatch;
    private static uint nextToken;
    private Queue<BattleNetPacket> outBoundPackets = new Queue<BattleNetPacket>();
    public ServiceCollectionHelper serviceHelper = new ServiceCollectionHelper();
    private object tokenLock = new object();
    private Map<uint, RPCContext> waitingForResponse = new Map<uint, RPCContext>();

    public void BeginAuth()
    {
        this.m_connMetering.ResetStartupPeriod();
    }

    public void Connect(string host, int port, SslParameters sslParams)
    {
        this.m_stopWatch = new Stopwatch();
        this.Connection = new ClientConnection<BattleNetPacket>();
        this.Connection.AddListener(this, null);
        this.Connection.AddConnectHandler(new ClientConnection<BattleNetPacket>.ConnectHandler(this.OnConnectCallback));
        this.Connection.AddDisconnectHandler(new ClientConnection<BattleNetPacket>.DisconnectHandler(this.OnDisconnectCallback));
        this.Connection.Connect(host, port);
    }

    protected bnet.protocol.Header CreateHeader(uint serviceId, uint methodId, uint objectId, uint token, uint size)
    {
        bnet.protocol.Header header = new bnet.protocol.Header();
        header.SetServiceId(serviceId);
        header.SetMethodId(methodId);
        if (objectId != 0)
        {
            header.SetObjectId((ulong) objectId);
        }
        header.SetToken(token);
        header.SetSize(size);
        return header;
    }

    public void Disconnect()
    {
        this.Update();
        this.Connection.Disconnect();
    }

    private void DownloadCompletedCallback(byte[] data)
    {
        if (data == null)
        {
            this.m_cmLogSource.LogWarning("Downloading of the connection metering data failed!");
        }
        else
        {
            object[] args = new object[] { data.Length };
            this.m_cmLogSource.LogDebug("Connection metering file downloaded. Length={0}", args);
            this.m_connMetering.SetConnectionMeteringData(data, this.serviceHelper);
        }
    }

    private ServiceDescriptor GetExportedServiceDescriptor(uint serviceId)
    {
        return this.serviceHelper.GetExportedServiceById(serviceId);
    }

    public uint GetExportedServiceNameHash(uint serviceId)
    {
        ServiceDescriptor exportedServiceDescriptor = this.GetExportedServiceDescriptor(serviceId);
        if (exportedServiceDescriptor != null)
        {
            return exportedServiceDescriptor.Hash;
        }
        return uint.MaxValue;
    }

    private ServiceDescriptor GetImportedServiceDescriptor(uint serviceId)
    {
        return this.serviceHelper.GetImportedServiceById(serviceId);
    }

    public uint GetImportedServiceNameHash(uint serviceId)
    {
        ServiceDescriptor importedServiceDescriptor = this.GetImportedServiceDescriptor(serviceId);
        if (importedServiceDescriptor != null)
        {
            return importedServiceDescriptor.Hash;
        }
        return uint.MaxValue;
    }

    private void LogOutgoingPacket(BattleNetPacket packet, bool wasMetered)
    {
        if (this.m_logSource == null)
        {
            UnityEngine.Debug.LogWarning("tried to log with null log source, skipping");
        }
        else
        {
            bool flag = false;
            IProtoBuf body = (IProtoBuf) packet.GetBody();
            bnet.protocol.Header header = packet.GetHeader();
            uint serviceId = header.ServiceId;
            uint methodId = header.MethodId;
            string str = !wasMetered ? "QueueRequest" : "QueueRequest (METERED)";
            if (!string.IsNullOrEmpty(body.ToString()))
            {
                ServiceDescriptor importedServiceById = this.serviceHelper.GetImportedServiceById(serviceId);
                object[] args = new object[] { str, (importedServiceById != null) ? importedServiceById.GetMethodName(methodId) : "null", header.ToString(), body.ToString() };
                this.m_logSource.LogDebug("{0}: type = {1}, header = {2}, request = {3}", args);
            }
            else
            {
                ServiceDescriptor descriptor2 = this.serviceHelper.GetImportedServiceById(serviceId);
                string str2 = (descriptor2 != null) ? descriptor2.GetMethodName(methodId) : null;
                if ((str2 != "bnet.protocol.connection.ConnectionService.KeepAlive") && (str2 != null))
                {
                    object[] objArray2 = new object[] { str, str2, header.ToString() };
                    this.m_logSource.LogDebug("{0}: type = {1}, header = {2}", objArray2);
                }
                else
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                this.m_logSource.LogDebugStackTrace("LogOutgoingPacket: ", 0x20, 1);
            }
        }
    }

    private void OnConnectCallback(BattleNetErrors error)
    {
        if (this.m_onConnectHandler != null)
        {
            this.m_onConnectHandler(error);
        }
    }

    private void OnDisconnectCallback(BattleNetErrors error)
    {
        if (this.m_onDisconnectHandler != null)
        {
            this.m_onDisconnectHandler(error);
        }
    }

    public void PacketReceived(BattleNetPacket p, object state)
    {
        Queue<BattleNetPacket> incomingPackets = this.incomingPackets;
        lock (incomingPackets)
        {
            this.incomingPackets.Enqueue(p);
        }
    }

    private void PrintHeader(bnet.protocol.Header h)
    {
        object[] args = new object[] { h.ServiceId, h.MethodId, h.Token, h.Size, (BattleNetErrors) h.Status };
        string message = string.Format("Packet received: Header = [ ServiceId: {0}, MethodId: {1} Token: {2} Size: {3} Status: {4}", args);
        if (h.ErrorCount > 0)
        {
            message = message + " Error:[";
            foreach (bnet.protocol.ErrorInfo info in h.ErrorList)
            {
                string str2 = message;
                object[] objArray2 = new object[] { str2, " ErrorInfo{ ", info.ObjectAddress.Host.Label, "/", info.ObjectAddress.Host.Epoch, "}" };
                message = string.Concat(objArray2);
            }
            message = message + "]";
        }
        message = message + "]";
        this.m_logSource.LogDebug(message);
    }

    private void ProcessPendingOutboundPackets()
    {
        if (this.m_pendingOutboundPackets.Count > 0)
        {
            List<BattleNetPacket> list = new List<BattleNetPacket>();
            foreach (BattleNetPacket packet in this.m_pendingOutboundPackets)
            {
                bnet.protocol.Header header = packet.GetHeader();
                uint serviceId = header.ServiceId;
                uint methodId = header.MethodId;
                if (this.m_connMetering.AllowRPCCall(serviceId, methodId))
                {
                    this.QueuePacket(packet);
                }
                else
                {
                    list.Add(packet);
                }
            }
            this.m_pendingOutboundPackets = list;
        }
    }

    protected void QueuePacket(BattleNetPacket packet)
    {
        this.LogOutgoingPacket(packet, false);
        Queue<BattleNetPacket> outBoundPackets = this.outBoundPackets;
        lock (outBoundPackets)
        {
            this.outBoundPackets.Enqueue(packet);
            this.m_stopWatch.Reset();
            this.m_stopWatch.Start();
        }
    }

    public RPCContext QueueRequest(uint serviceId, uint methodId, IProtoBuf message, RPCContextDelegate callback = null, uint objectId = 0)
    {
        uint nextToken;
        if (message == null)
        {
            return null;
        }
        object tokenLock = this.tokenLock;
        lock (tokenLock)
        {
            nextToken = RPCConnection.nextToken;
            RPCConnection.nextToken++;
        }
        RPCContext context = new RPCContext();
        if (callback != null)
        {
            context.Callback = callback;
            this.waitingForResponse.Add(nextToken, context);
        }
        bnet.protocol.Header h = this.CreateHeader(serviceId, methodId, objectId, nextToken, message.GetSerializedSize());
        BattleNetPacket item = new BattleNetPacket(h, message);
        context.Header = h;
        context.Request = message;
        if (!this.m_connMetering.AllowRPCCall(serviceId, methodId))
        {
            this.m_pendingOutboundPackets.Add(item);
            this.LogOutgoingPacket(item, true);
            return context;
        }
        this.QueuePacket(item);
        return context;
    }

    public void QueueResponse(RPCContext context, IProtoBuf message)
    {
        if ((message == null) || (context.Header == null))
        {
            this.m_logSource.LogError("QueueResponse: invalid response");
        }
        else if (this.serviceHelper.GetImportedServiceById(context.Header.ServiceId) == null)
        {
            this.m_logSource.LogError("QueueResponse: error, unrecognized service id: " + context.Header.ServiceId);
        }
        else
        {
            this.m_logSource.LogDebug(string.Concat(new object[] { "QueueResponse: type=", this.serviceHelper.GetImportedServiceById(context.Header.ServiceId).GetMethodName(context.Header.MethodId), " data=", message }));
            bnet.protocol.Header header = context.Header;
            header.SetServiceId(0xfe);
            header.SetMethodId(0);
            header.SetSize(message.GetSerializedSize());
            context.Header = header;
            BattleNetPacket packet = new BattleNetPacket(context.Header, message);
            this.QueuePacket(packet);
        }
    }

    public void RegisterServiceMethodListener(uint serviceId, uint methodId, RPCContextDelegate callback)
    {
        ServiceDescriptor exportedServiceDescriptor = this.GetExportedServiceDescriptor(serviceId);
        if (exportedServiceDescriptor != null)
        {
            exportedServiceDescriptor.RegisterMethodListener(methodId, callback);
        }
    }

    public void SetConnectionMeteringContentHandles(ConnectionMeteringContentHandles handles, LocalStorageAPI localStorage)
    {
        if (((handles == null) || !handles.IsInitialized) || (handles.ContentHandleCount == 0))
        {
            this.m_cmLogSource.LogWarning("Invalid connection metering content handle received.");
        }
        else
        {
            if (handles.ContentHandleCount != 1)
            {
                this.m_cmLogSource.LogWarning("More than 1 connection metering content handle specified!");
            }
            bnet.protocol.ContentHandle contentHandle = handles.ContentHandle[0];
            if ((contentHandle == null) || !contentHandle.IsInitialized)
            {
                this.m_cmLogSource.LogWarning("The content handle received is not valid!");
            }
            else
            {
                this.m_cmLogSource.LogDebug("Received request to enable connection metering.");
                ContentHandle ch = ContentHandle.FromProtocol(contentHandle);
                object[] args = new object[] { ch };
                this.m_cmLogSource.LogDebug("Requesting file from local storage. ContentHandle={0}", args);
                localStorage.GetFile(ch, new LocalStorageAPI.DownloadCompletedCallback(this.DownloadCompletedCallback));
            }
        }
    }

    public void SetOnConnectHandler(OnConnectHandler handler)
    {
        this.m_onConnectHandler = handler;
    }

    public void SetOnDisconnectHandler(OnDisconectHandler handler)
    {
        this.m_onDisconnectHandler = handler;
    }

    public void Update()
    {
        this.ProcessPendingOutboundPackets();
        if (this.outBoundPackets.Count > 0)
        {
            Queue<BattleNetPacket> queue;
            Queue<BattleNetPacket> outBoundPackets = this.outBoundPackets;
            lock (outBoundPackets)
            {
                queue = new Queue<BattleNetPacket>(this.outBoundPackets.ToArray());
                this.outBoundPackets.Clear();
            }
            while (queue.Count > 0)
            {
                BattleNetPacket packet = queue.Dequeue();
                if (this.Connection != null)
                {
                    this.Connection.QueuePacket(packet);
                }
                else
                {
                    this.m_logSource.LogError("##Client Connection object does not exists!##");
                }
            }
        }
        if (this.Connection != null)
        {
            this.Connection.Update();
        }
        if (this.incomingPackets.Count > 0)
        {
            Queue<BattleNetPacket> queue3;
            Queue<BattleNetPacket> incomingPackets = this.incomingPackets;
            lock (incomingPackets)
            {
                queue3 = new Queue<BattleNetPacket>(this.incomingPackets.ToArray());
                this.incomingPackets.Clear();
            }
            while (queue3.Count > 0)
            {
                BattleNetPacket packet2 = queue3.Dequeue();
                bnet.protocol.Header h = packet2.GetHeader();
                this.PrintHeader(h);
                byte[] body = (byte[]) packet2.GetBody();
                if (h.ServiceId == 0xfe)
                {
                    RPCContext context;
                    if (this.waitingForResponse.TryGetValue(h.Token, out context))
                    {
                        ServiceDescriptor importedServiceById = this.serviceHelper.GetImportedServiceById(context.Header.ServiceId);
                        MethodDescriptor.ParseMethod parser = null;
                        if (importedServiceById != null)
                        {
                            parser = importedServiceById.GetParser(context.Header.MethodId);
                        }
                        if (parser == null)
                        {
                            if (importedServiceById != null)
                            {
                                object[] args = new object[] { importedServiceById.Name, context.Header.MethodId };
                                this.m_logSource.LogWarning("Incoming Response: Unable to find method for serviceName={0} method id={1}", args);
                                int methodCount = importedServiceById.GetMethodCount();
                                object[] objArray2 = new object[] { methodCount };
                                this.m_logSource.LogDebug("  Found {0} methods", objArray2);
                                for (int i = 0; i < methodCount; i++)
                                {
                                    MethodDescriptor methodDescriptor = importedServiceById.GetMethodDescriptor((uint) i);
                                    if ((methodDescriptor == null) && (i != 0))
                                    {
                                        object[] objArray3 = new object[] { i, "<null>" };
                                        this.m_logSource.LogDebug("  Found method id={0} name={1}", objArray3);
                                    }
                                    else
                                    {
                                        object[] objArray4 = new object[] { i, methodDescriptor.Name };
                                        this.m_logSource.LogDebug("  Found method id={0} name={1}", objArray4);
                                    }
                                }
                            }
                            else
                            {
                                object[] objArray5 = new object[] { context.Header.ServiceId };
                                this.m_logSource.LogWarning("Incoming Response: Unable to identify service id={0}", objArray5);
                            }
                        }
                        context.Header = h;
                        context.Payload = body;
                        context.ResponseReceived = true;
                        if (context.Callback != null)
                        {
                            context.Callback(context);
                        }
                        this.waitingForResponse.Remove(h.Token);
                    }
                }
                else
                {
                    ServiceDescriptor exportedServiceDescriptor = this.GetExportedServiceDescriptor(h.ServiceId);
                    if (exportedServiceDescriptor != null)
                    {
                        if (this.serviceHelper.GetExportedServiceById(h.ServiceId).GetParser(h.MethodId) == null)
                        {
                            this.m_logSource.LogDebug("Incoming Packet: NULL TYPE service=" + this.serviceHelper.GetExportedServiceById(h.ServiceId).Name + ", method=" + this.serviceHelper.GetExportedServiceById(h.ServiceId).GetMethodName(h.MethodId));
                        }
                        if (exportedServiceDescriptor.HasMethodListener(h.MethodId))
                        {
                            RPCContext context2 = new RPCContext {
                                Header = h,
                                Payload = body,
                                ResponseReceived = true
                            };
                            exportedServiceDescriptor.NotifyMethodListener(context2);
                        }
                        else
                        {
                            string str = ((exportedServiceDescriptor == null) || string.IsNullOrEmpty(exportedServiceDescriptor.Name)) ? "<null>" : exportedServiceDescriptor.Name;
                            this.m_logSource.LogError(string.Concat(new object[] { "[!]Unhandled Server Request Received (Service Name: ", str, " Service id:", h.ServiceId, " Method id:", h.MethodId, ")" }));
                        }
                    }
                    else
                    {
                        this.m_logSource.LogError(string.Concat(new object[] { "[!]Server Requested an Unsupported (Service id:", h.ServiceId, " Method id:", h.MethodId, ")" }));
                    }
                }
            }
        }
    }

    public long MillisecondsSinceLastPacketSent
    {
        get
        {
            return this.m_stopWatch.ElapsedMilliseconds;
        }
    }

    public delegate void OnConnectHandler(BattleNetErrors error);

    public delegate void OnDisconectHandler(BattleNetErrors error);
}

