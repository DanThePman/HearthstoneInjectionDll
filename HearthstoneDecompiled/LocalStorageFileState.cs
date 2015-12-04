using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

internal class LocalStorageFileState
{
    public ContentHandle CH;
    public byte[] CompressedData;
    public byte[] FileData;
    public string Host;
    private int m_ID;
    public byte[] ReceiveBuffer;
    public Socket TcpSocket;

    public LocalStorageFileState(int id)
    {
        this.m_ID = id;
    }

    public override string ToString()
    {
        object[] args = new object[] { this.CH.Region, this.CH.Usage, this.CH.Sha256Digest, this.m_ID };
        return string.Format("[Region={0} Usage={1} SHA256={2} ID={3}]", args);
    }

    public LocalStorageAPI.DownloadCompletedCallback Callback { get; set; }

    public string FailureMessage { get; set; }

    public int ID
    {
        get
        {
            return this.m_ID;
        }
    }
}

