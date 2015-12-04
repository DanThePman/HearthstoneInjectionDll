using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class LocalStorageAPI : BattleNetAPI
{
    private static readonly int m_bufferSize = 0x400;
    private static List<LocalStorageFileState> m_completedDownloads = new List<LocalStorageFileState>();
    private static int m_depotport = 0x45f;
    private static int m_downloadId = 0;
    private static string m_rootPath = Application.temporaryCachePath;
    private static LogThreadHelper s_log = new LogThreadHelper("LocalStorage");

    public LocalStorageAPI(BattleNetCSharp battlenet) : base(battlenet, "LocalStorage")
    {
    }

    private static void CompleteDownload(LocalStorageFileState state)
    {
        bool flag = true;
        object[] args = new object[] { state };
        s_log.LogDebug("Download completed for State={0}", args);
        HTTPHeader header = ParseHTTPHeader(state.FileData);
        if (header == null)
        {
            object[] objArray2 = new object[] { state };
            s_log.LogWarning("Parsinig of HTTP header failed for State={0}", objArray2);
        }
        else
        {
            byte[] destinationArray = new byte[header.ContentLength];
            Array.Copy(state.FileData, header.ContentStart, destinationArray, 0, header.ContentLength);
            if (ComputeSHA256(destinationArray) != state.CH.Sha256Digest)
            {
                object[] objArray3 = new object[] { state };
                s_log.LogWarning("Integrity check failed for State={0}", objArray3);
            }
            else
            {
                flag = false;
                DecompressStateIfPossible(state, destinationArray);
            }
        }
        if (flag || (state.FileData == null))
        {
            ExecuteFailedDownload(state);
        }
        else
        {
            ExecuteSucessfulDownload(state);
        }
    }

    private static string ComputeSHA256(byte[] bytes)
    {
        byte[] buffer = SHA256.Create().ComputeHash(bytes, 0, bytes.Length);
        StringBuilder builder = new StringBuilder();
        foreach (byte num in buffer)
        {
            builder.Append(num.ToString("x2"));
        }
        return builder.ToString();
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            LocalStorageFileState asyncState = (LocalStorageFileState) ar.AsyncState;
            object[] args = new object[] { asyncState };
            s_log.LogDebug("ConnectCallback called for State={0}", args);
            asyncState.TcpSocket.EndConnect(ar);
            asyncState.ReceiveBuffer = new byte[m_bufferSize];
            asyncState.TcpSocket.BeginReceive(asyncState.ReceiveBuffer, 0, m_bufferSize, SocketFlags.None, new AsyncCallback(LocalStorageAPI.ReceiveCallback), asyncState);
            string s = ((string.Format("GET /{0}.{1} HTTP/1.1\r\n", asyncState.CH.Sha256Digest, asyncState.CH.Usage) + "Host: " + asyncState.Host + "\r\n") + "User-Agent: HearthStone\r\n") + "Connection: close\r\n" + "\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            asyncState.TcpSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
        }
        catch (Exception exception)
        {
            object[] objArray2 = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION: {0}", objArray2);
        }
    }

    private static void DecompressStateIfPossible(LocalStorageFileState state, byte[] data)
    {
        ulong num;
        if (IsCompressedStream(data, out num))
        {
            state.CompressedData = data;
            MemoryStream ms = new MemoryStream(data, 0x10, data.Length - 0x10);
            state.FileData = Inflate(ms, (int) num);
        }
        else
        {
            state.FileData = data;
        }
    }

    private void DownloadFromDepot(LocalStorageFileState state)
    {
        string hostNameOrAddress = string.Format("{0}.depot.battle.net", state.CH.Region);
        state.Host = hostNameOrAddress;
        object[] args = new object[] { hostNameOrAddress };
        s_log.LogDebug("Starting download from {0}", args);
        IPAddress address = Dns.GetHostEntry(hostNameOrAddress).AddressList[0];
        state.TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        state.TcpSocket.BeginConnect(new IPEndPoint(address, this.DepotPort), new AsyncCallback(LocalStorageAPI.ConnectCallback), state);
    }

    private static void ExecuteFailedDownload(LocalStorageFileState state)
    {
        state.FileData = null;
        FinalizeDownload(state);
    }

    private static void ExecuteSucessfulDownload(LocalStorageFileState state)
    {
        StoreStateToDrive(state);
        FinalizeDownload(state);
    }

    private static void FinalizeDownload(LocalStorageFileState state)
    {
        if (state.TcpSocket != null)
        {
            state.TcpSocket.Close();
        }
        state.ReceiveBuffer = null;
        FinalizeState(state);
    }

    private static void FinalizeState(LocalStorageFileState state)
    {
        List<LocalStorageFileState> completedDownloads = m_completedDownloads;
        lock (completedDownloads)
        {
            m_completedDownloads.Add(state);
        }
    }

    public bool GetFile(ContentHandle ch, DownloadCompletedCallback cb)
    {
        try
        {
            LocalStorageFileState state = new LocalStorageFileState(m_downloadId) {
                CH = ch,
                Callback = cb
            };
            object[] args = new object[] { state };
            s_log.LogDebug("Starting GetFile State={0}", args);
            if (!this.LoadStateFromDrive(state))
            {
                object[] objArray2 = new object[] { state };
                s_log.LogDebug("Unable to load file from disk, starting a download. State={0}", objArray2);
                this.DownloadFromDepot(state);
            }
        }
        catch (Exception exception)
        {
            object[] objArray3 = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION (GetFile): {0}", objArray3);
            return false;
        }
        return true;
    }

    private static byte[] Inflate(MemoryStream ms, int length)
    {
        ms.Seek(0L, SeekOrigin.Begin);
        Inflater inf = new Inflater(false);
        InflaterInputStream stream = new InflaterInputStream(ms, inf);
        byte[] buffer = new byte[length];
        int offset = 0;
        int count = buffer.Length;
        try
        {
            while (true)
            {
                int num3 = stream.Read(buffer, offset, count);
                if (num3 <= 0)
                {
                    goto Label_0084;
                }
                offset += num3;
                count -= num3;
            }
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION (Inflate): {0}", args);
            return null;
        }
    Label_0084:
        if (offset != length)
        {
            s_log.LogWarning("Decompressed size does not equal expected size.");
            return null;
        }
        return buffer;
    }

    private static bool IsCompressedStream(byte[] data, out ulong decompressedLength)
    {
        decompressedLength = 0L;
        try
        {
            if (data.Length < 0x10)
            {
                return false;
            }
            if (BitConverter.ToUInt32(data, 0) != 0x436d705a)
            {
                return false;
            }
            if (BitConverter.ToUInt32(data, 4) != 0)
            {
                return false;
            }
            decompressedLength = BitConverter.ToUInt64(data, 8);
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION: {0}", args);
            return false;
        }
        return true;
    }

    private bool LoadStateFromDrive(LocalStorageFileState state)
    {
        try
        {
            object[] args = new object[] { state };
            s_log.LogDebug("LoadState State={0}", args);
            string path = MakeFullPathFromState(state);
            object[] objArray2 = new object[] { path };
            s_log.LogDebug("Attempting to load {0}", objArray2);
            if (!System.IO.File.Exists(path))
            {
                s_log.LogDebug("File does not exist, unable to load from disk.");
                return false;
            }
            FileStream stream = System.IO.File.OpenRead(path);
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            stream.Close();
            if (ComputeSHA256(array) != state.CH.Sha256Digest)
            {
                s_log.LogDebug("File was loaded but integrity check failed, attempting to delete ...");
                System.IO.File.Delete(path);
                return false;
            }
            DecompressStateIfPossible(state, array);
            s_log.LogDebug("Loading completed");
            FinalizeState(state);
        }
        catch (Exception exception)
        {
            object[] objArray3 = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION: {0}", objArray3);
            return false;
        }
        return true;
    }

    private static string MakeFullPathFromState(LocalStorageFileState state)
    {
        string rootPath = m_rootPath;
        string str2 = state.CH.Sha256Digest + "." + state.CH.Usage;
        return System.IO.Path.Combine(rootPath, str2);
    }

    private static HTTPHeader ParseHTTPHeader(byte[] data)
    {
        try
        {
            int count = SearchForBytePattern(data, new byte[] { 13, 10, 13, 10 });
            if (count == -1)
            {
                return null;
            }
            int num2 = count + 1;
            if (num2 >= data.Length)
            {
                return null;
            }
            string input = Encoding.ASCII.GetString(data, 0, count);
            if (input.IndexOf("200 OK") == -1)
            {
                return null;
            }
            Match match = new Regex(@"(?<=Content-Length:\s)\d+", RegexOptions.IgnoreCase).Match(input);
            if (!match.Success)
            {
                return null;
            }
            int num3 = (int) uint.Parse(match.Value);
            int num4 = data.Length - num2;
            if (num3 != num4)
            {
                return null;
            }
            return new HTTPHeader { ContentLength = num3, ContentStart = num2 };
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION (ParseHTTPHeader): {0}", args);
        }
        return null;
    }

    public override void Process()
    {
        List<LocalStorageFileState> completedDownloads = m_completedDownloads;
        lock (completedDownloads)
        {
            if (m_completedDownloads.Count > 0)
            {
                foreach (LocalStorageFileState state in m_completedDownloads)
                {
                    if (state.FileData != null)
                    {
                        object[] args = new object[] { state };
                        s_log.LogDebug("Request completed State={0}", args);
                    }
                    else
                    {
                        object[] objArray2 = new object[] { state };
                        s_log.LogWarning("Request failed State={0}", objArray2);
                    }
                    s_log.Process();
                    state.Callback(state.FileData);
                }
                m_completedDownloads.Clear();
            }
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        LocalStorageFileState asyncState = (LocalStorageFileState) ar.AsyncState;
        try
        {
            object[] args = new object[] { asyncState };
            s_log.LogDebug("ReceiveCallback called for State={0}", args);
            int count = asyncState.TcpSocket.EndReceive(ar);
            if (count > 0)
            {
                int num2 = (asyncState.FileData != null) ? asyncState.FileData.Length : 0;
                int num3 = num2 + count;
                MemoryStream stream = new MemoryStream(new byte[num3], 0, num3, true, true);
                if (asyncState.FileData != null)
                {
                    stream.Write(asyncState.FileData, 0, asyncState.FileData.Length);
                }
                stream.Write(asyncState.ReceiveBuffer, 0, count);
                asyncState.FileData = stream.GetBuffer();
                asyncState.TcpSocket.BeginReceive(asyncState.ReceiveBuffer, 0, m_bufferSize, SocketFlags.None, new AsyncCallback(LocalStorageAPI.ReceiveCallback), asyncState);
            }
            else
            {
                CompleteDownload(asyncState);
            }
        }
        catch (Exception exception)
        {
            object[] objArray2 = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION: {0}", objArray2);
            asyncState.FailureMessage = exception.Message;
            ExecuteFailedDownload(asyncState);
        }
    }

    private static int SearchForBytePattern(byte[] source, byte[] pattern)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if ((pattern[0] == source[i]) && ((source.Length - i) >= pattern.Length))
            {
                bool flag = true;
                for (int j = 1; (j < pattern.Length) && flag; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    return (i + (pattern.Length - 1));
                }
            }
        }
        return -1;
    }

    private static void StoreStateToDrive(LocalStorageFileState state)
    {
        try
        {
            object[] args = new object[] { state };
            s_log.LogDebug("StoreState State={0}", args);
            string path = MakeFullPathFromState(state);
            object[] objArray2 = new object[] { path };
            s_log.LogDebug("Attempting to save {0}", objArray2);
            if (System.IO.File.Exists(path))
            {
                s_log.LogDebug("Unable to save the file, it already exists");
            }
            else
            {
                FileStream stream = System.IO.File.Create(path, state.FileData.Length);
                if (state.CompressedData == null)
                {
                    s_log.LogDebug("Writing uncompressed file to disk");
                    stream.Write(state.FileData, 0, state.FileData.Length);
                }
                else
                {
                    s_log.LogDebug("Writing compressed file to disk");
                    stream.Write(state.CompressedData, 0, state.CompressedData.Length);
                }
                stream.Flush();
                stream.Close();
                s_log.LogDebug("Writing completed");
            }
        }
        catch (Exception exception)
        {
            object[] objArray3 = new object[] { exception.Message };
            s_log.LogWarning("EXCEPTION (StoreStateToDrive): {0}", objArray3);
        }
    }

    public int DepotPort
    {
        get
        {
            return m_depotport;
        }
        set
        {
            m_depotport = value;
        }
    }

    public delegate void DownloadCompletedCallback(byte[] data);
}

