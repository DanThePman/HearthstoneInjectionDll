using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using UnityEngine;

public class DebugConsoleBroadcaster
{
    private static readonly TimeSpan Interval = new TimeSpan(0, 0, UnityEngine.Random.Range(7, 10));
    private IPEndPoint m_RemoteEndPoint;
    private byte[] m_RequestBytes;
    private Socket m_Socket;
    private bool m_started;
    private System.Timers.Timer m_Timer;

    private void OnSendTo(IAsyncResult ar)
    {
        try
        {
            this.m_Socket.EndSendTo(ar);
        }
        catch (Exception exception)
        {
            Debug.LogError("error debug broadcast: " + exception.Message);
        }
    }

    private void OnTimerTick(object sender, ElapsedEventArgs args)
    {
        this.m_Socket.BeginSendTo(this.m_RequestBytes, 0, this.m_RequestBytes.Length, SocketFlags.None, this.m_RemoteEndPoint, new AsyncCallback(this.OnSendTo), this);
    }

    public void Start(int destinationPort, string broadCastResponse)
    {
        this.m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        this.m_Socket.EnableBroadcast = true;
        this.m_RequestBytes = new ASCIIEncoding().GetBytes(broadCastResponse);
        this.m_RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, destinationPort);
        this.m_Timer = new System.Timers.Timer(Interval.TotalMilliseconds);
        this.m_Timer.Elapsed += new ElapsedEventHandler(this.OnTimerTick);
        this.m_Timer.Start();
        this.m_started = true;
    }

    public void Stop()
    {
        if (this.m_started)
        {
            this.m_Timer.Stop();
            this.m_Socket.Close();
        }
    }
}

