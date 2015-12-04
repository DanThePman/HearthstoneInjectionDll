using bgs;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class SslSocket
{
    private string m_address;
    private BeginConnectDelegate m_beginConnectDelegate;
    private SslCertBundleInfo m_bundleInfo;
    public bool m_canSend = true;
    private string m_resolvedAddress;
    private Socket m_socket;
    private SslStream m_sslStream;
    private const int PUBKEY_EXP_SIZE_BYTES = 4;
    private const int PUBKEY_MODULUS_SIZE_BITS = 0x800;
    private const int PUBKEY_MODULUS_SIZE_BYTES = 0x100;
    private static LogThreadHelper s_log;
    private static string s_magicBundleSignaturePreamble;
    private static X509Certificate2 s_rootCertificate;
    private static byte[] s_standardPublicExponent;
    private static byte[] s_standardPublicModulus;
    private static Map<SslStream, SslStreamValidationContext> s_streamValidationContexts;

    static SslSocket()
    {
        string str;
        s_magicBundleSignaturePreamble = "NGIS";
        byte[] buffer1 = new byte[4];
        buffer1[0] = 1;
        buffer1[2] = 1;
        s_standardPublicExponent = buffer1;
        s_standardPublicModulus = new byte[] { 
            0x35, 0xff, 0x17, 0xe7, 0x33, 0xc4, 0xd3, 0xd4, 240, 0x37, 0xa4, 0xb5, 0x7c, 0x1b, 240, 0x4e, 
            0x31, 0xe8, 0xff, 0xb3, 12, 30, 0x88, 0x10, 0x4d, 0xaf, 0x13, 11, 0x58, 0x56, 0x58, 0x19, 
            0x58, 0x37, 0x15, 0xf9, 0xeb, 0xec, 0x98, 0xcb, 0x9d, 0xcc, 0xfd, 0x18, 0xf1, 0x47, 9, 0x1b, 
            0xe3, 0x7b, 0x38, 40, 0x9e, 14, 0x9b, 0x1f, 0x9f, 0x95, 0xda, 0x9d, 0x61, 0x75, 0xf2, 0x1f, 
            160, 0x3d, 0xa2, 0x99, 0xbd, 0xb2, 0x1d, 14, 0x69, 0xca, 0xbc, 0x73, 0x1b, 0xe5, 0xeb, 15, 
            0xe7, 0xfb, 0x2b, 0x7b, 0xb2, 0x35, 5, 0x8f, 0xf5, 0xb5, 0x9a, 0x3b, 0x12, 0xad, 0xa1, 0xa4, 
            140, 0xf7, 0x90, 0x66, 0x88, 0x17, 0xd6, 0x1f, 0x93, 0x84, 0x10, 0xae, 0xf2, 0xef, 0x2a, 0x7a, 
            0x5f, 0x41, 0x7b, 0x5c, 0x80, 210, 0x5e, 0x1a, 0xfd, 0xdb, 0x10, 0x76, 0x93, 0xbc, 0x8b, 0xd5, 
            230, 0xb2, 80, 0xf5, 0x51, 0x9b, 3, 0xe2, 0x53, 0x9b, 0xa8, 0xb0, 0xb1, 0x37, 0xd5, 0x25, 
            0x66, 0x45, 8, 0x81, 0x20, 15, 0x88, 0x61, 0xae, 0xbb, 0xf5, 0x44, 0xf5, 0x84, 0x9e, 0x76, 
            0x27, 0x15, 0x74, 0x17, 0xc6, 0xb7, 0x8f, 0xe0, 0x2d, 0x37, 0x5c, 0xf8, 0x52, 0x31, 50, 0x3f, 
            250, 0x44, 0x7f, 0xef, 0x24, 0x3d, 0x5b, 0x59, 0xf9, 0xfd, 80, 80, 0xca, 160, 0x36, 0x4d, 
            0x62, 0xd9, 0x44, 13, 0x69, 0xa6, 0xef, 0x2b, 0xce, 0xcc, 0xc2, 0xa3, 0xbc, 0xf5, 0xa2, 0x1c, 
            0xee, 0x77, 0x45, 0xe4, 0x33, 240, 0x57, 0x20, 0xbf, 0x2e, 7, 0x86, 0x2b, 0x95, 0xbb, 0x3a, 
            0xfc, 4, 60, 0x45, 0x3f, 0, 0x34, 11, 0x36, 0xbb, 0x4b, 0xc1, 15, 0x95, 0x18, 0xc3, 
            0xd9, 250, 0x36, 0x42, 0xca, 150, 170, 0xec, 0x7a, 0x2e, 0x88, 130, 60, 0x1d, 0x98, 0x94
         };
        s_log = new LogThreadHelper("SslSocket");
        s_streamValidationContexts = new Map<SslStream, SslStreamValidationContext>();
        if (ApplicationMgr.GetMobileEnvironment() == MobileEnv.PRODUCTION)
        {
            str = "-----BEGIN CERTIFICATE-----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-----END CERTIFICATE-----";
        }
        else
        {
            str = "-----BEGIN CERTIFICATE-----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-----END CERTIFICATE-----";
        }
        s_rootCertificate = new X509Certificate2(Encoding.ASCII.GetBytes(str));
    }

    public void BeginConnect(string address, int port, SslCertBundleInfo bundleInfo, BeginConnectDelegate connectDelegate)
    {
        try
        {
            string str;
            string str2;
            try
            {
                UriUtils.GetHostAddress(address, out str);
            }
            catch (Exception exception)
            {
                s_log.LogError("Exception within GetHostAddress: " + exception.Message);
                return;
            }
            this.m_address = address;
            if (UriUtils.GetHostAddressAsIp(address, out str2))
            {
                this.m_address = "::ffff:" + address;
            }
            this.m_resolvedAddress = "::ffff:" + str;
            object[] args = new object[] { str, port };
            s_log.LogDebug("Connecting to {0}:{1}", args);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(str), port);
            this.m_beginConnectDelegate = connectDelegate;
            this.m_bundleInfo = bundleInfo;
            this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_socket.BeginConnect(remoteEP, new AsyncCallback(this.ConnectCallback), address);
        }
        catch (Exception exception2)
        {
            object[] objArray2 = new object[] { exception2 };
            s_log.LogWarning("Exception while trying to call BeginConnect. {0}", objArray2);
            this.ExecuteBeginConnectDelegate(true);
        }
    }

    public void BeginReceive(byte[] buffer, int size, BeginReceiveDelegate beginReceiveDelegate)
    {
        try
        {
            if (this.m_sslStream == null)
            {
                throw new NullReferenceException("m_sslStream is null!");
            }
            this.m_sslStream.BeginRead(buffer, 0, size, new AsyncCallback(this.ReadCallback), beginReceiveDelegate);
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception };
            s_log.LogWarning("Exception while trying to call BeginRead. {0}", args);
            if (beginReceiveDelegate != null)
            {
                beginReceiveDelegate(0);
            }
        }
    }

    public void BeginSend(byte[] bytes, BeginSendDelegate sendDelegate)
    {
        try
        {
            if (this.m_sslStream == null)
            {
                throw new NullReferenceException("m_sslStream is null!");
            }
            this.m_canSend = false;
            this.m_sslStream.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(this.WriteCallback), sendDelegate);
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception };
            s_log.LogWarning("Exception while trying to call BeginWrite. {0}", args);
            if (sendDelegate != null)
            {
                sendDelegate(false);
            }
        }
    }

    public void Close()
    {
        Socket socket = this.m_socket;
        SslStream sslStream = this.m_sslStream;
        this.m_socket = null;
        this.m_sslStream = null;
        try
        {
            if (socket != null)
            {
                socket.Close();
            }
            if (sslStream != null)
            {
                sslStream.Close();
            }
        }
        catch (Exception)
        {
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            this.m_socket.EndConnect(ar);
            if (!this.m_socket.Connected)
            {
                s_log.LogWarning("Failed to connect.");
                this.ExecuteBeginConnectDelegate(true);
            }
            else
            {
                RemoteCertificateValidationCallback userCertificateValidationCallback = new RemoteCertificateValidationCallback(SslSocket.OnValidateServerCertificate);
                this.m_sslStream = new SslStream(new NetworkStream(this.m_socket, true), false, userCertificateValidationCallback);
                SslStreamValidationContext context = new SslStreamValidationContext {
                    m_socket = this
                };
                s_streamValidationContexts.Add(this.m_sslStream, context);
                this.m_sslStream.BeginAuthenticateAsClient(this.m_address, new AsyncCallback(this.OnAuthenticateAsClient), null);
            }
        }
        catch (Exception exception)
        {
            object[] args = new object[] { exception };
            s_log.LogWarning("Exception while trying to authenticate. {0}", args);
            this.ExecuteBeginConnectDelegate(true);
        }
    }

    private void ExecuteBeginConnectDelegate(bool connectFailed)
    {
        this.m_bundleInfo = null;
        if (this.m_beginConnectDelegate != null)
        {
            bool isEncrypted = false;
            bool isSigned = false;
            if (this.m_sslStream != null)
            {
                isEncrypted = this.m_sslStream.IsEncrypted;
                isSigned = this.m_sslStream.IsSigned;
            }
            this.m_beginConnectDelegate(connectFailed, isEncrypted, isSigned);
            this.m_beginConnectDelegate = null;
            object[] args = new object[] { !connectFailed, isEncrypted, isSigned };
            s_log.LogDebug("Connected={0} isEncrypted={1} isSigned={2}", args);
        }
    }

    private static bool GetBundleInfo(byte[] unsignedBundleBytes, out List<byte[]> bundleKeyHashs, out List<string> bundleUris)
    {
        bundleKeyHashs = new List<byte[]>();
        bundleUris = new List<string>();
        string str = null;
        string aJSON = Encoding.ASCII.GetString(unsignedBundleBytes);
        try
        {
            IEnumerator enumerator = JSON.Parse(aJSON)["Certificates"].AsArray.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    JSONNode current = (JSONNode) enumerator.Current;
                    string item = current["Uri"].Value;
                    string hex = current["ShaHashPublicKeyInfo"].Value;
                    byte[] outBytes = null;
                    HexStrToBytesError enumVal = HexStrToBytes(hex, out outBytes);
                    if (enumVal != HexStrToBytesError.OK)
                    {
                        str = EnumUtils.GetString<HexStrToBytesError>(enumVal);
                        goto Label_00E4;
                    }
                    bundleKeyHashs.Add(outBytes);
                    bundleUris.Add(item);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
        }
        catch (Exception exception)
        {
            str = exception.ToString();
        }
    Label_00E4:
        if (str != null)
        {
            object[] args = new object[] { str };
            s_log.LogWarning("Exception while trying to parse certificate bundle. {0}", args);
            return false;
        }
        return true;
    }

    private static List<string> GetCommonNamesFromCertSubject(string certSubject)
    {
        List<string> list = new List<string>();
        char[] separator = new char[] { ',' };
        foreach (string str in certSubject.Split(separator))
        {
            string str2 = str.Trim();
            if (str2.StartsWith("CN="))
            {
                string item = str2.Substring(3);
                list.Add(item);
            }
        }
        return list;
    }

    private static byte[] GetUnsignedBundleBytes(byte[] signedBundleBytes)
    {
        int length = signedBundleBytes.Length - (s_magicBundleSignaturePreamble.Length + 0x100);
        if (length <= 0)
        {
            return null;
        }
        byte[] destinationArray = new byte[length];
        Array.Copy(signedBundleBytes, destinationArray, length);
        return destinationArray;
    }

    private static HexStrToBytesError HexStrToBytes(string hex, out byte[] outBytes)
    {
        outBytes = null;
        int length = hex.Length;
        if ((length % 2) == 1)
        {
            return HexStrToBytesError.ODD_NUMBER_OF_DIGITS;
        }
        outBytes = new byte[length / 2];
        int startIndex = 0;
        for (int i = 0; startIndex < length; i++)
        {
            string str = hex.Substring(startIndex, 2);
            outBytes[i] = Convert.ToByte(str, 0x10);
            startIndex += 2;
        }
        return HexStrToBytesError.OK;
    }

    private static bool IsCertSignedByBlizzard(X509Certificate cert)
    {
        string issuer = cert.Issuer;
        char[] separator = new char[] { ',' };
        string[] strArray = issuer.Split(separator);
        for (int i = 0; i < strArray.Length; i++)
        {
            strArray[i] = strArray[i].Trim();
        }
        HashSet<string> set = new HashSet<string> { "CN=Battle.net Certificate Authority" };
        foreach (string str2 in strArray)
        {
            set.Remove(str2);
        }
        return (set.Count == 0);
    }

    private static bool IsWhitelistedInCertBundle(byte[] unsignedBundleBytes, string uri, byte[] publicKey)
    {
        List<byte[]> list;
        List<string> list2;
        if (GetBundleInfo(unsignedBundleBytes, out list, out list2))
        {
            byte[] first = SHA256.Create().ComputeHash(publicKey);
            for (int i = 0; i < list.Count; i++)
            {
                byte[] second = list[i];
                if (first.SequenceEqual<byte>(second))
                {
                    string str = list2[i];
                    if (str.Equals(uri))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool MakePKCS1SignatureBlock(byte[] hash, int hashSize, byte[] id, int idSize, byte[] signature, int signatureSize)
    {
        byte[] buffer = signature;
        int num = (3 + idSize) + hashSize;
        if (num > signatureSize)
        {
            return false;
        }
        int num2 = signatureSize - num;
        int num3 = 0;
        for (int i = 0; i < hashSize; i++)
        {
            buffer[num3++] = hash[(hashSize - i) - 1];
        }
        for (int j = 0; j < idSize; j++)
        {
            buffer[num3++] = id[(idSize - j) - 1];
        }
        buffer[num3++] = 0;
        for (int k = 0; k < num2; k++)
        {
            buffer[num3++] = 0xff;
        }
        buffer[num3++] = 1;
        buffer[num3++] = 0;
        if (num3 != signatureSize)
        {
            return false;
        }
        return true;
    }

    private void OnAuthenticateAsClient(IAsyncResult ar)
    {
        bool connectFailed = false;
        try
        {
            if (this.m_sslStream == null)
            {
                throw new NullReferenceException("m_sslStream is null!");
            }
            this.m_sslStream.EndAuthenticateAsClient(ar);
            object[] args = new object[] { this.m_sslStream.IsEncrypted, this.m_sslStream.IsSigned };
            s_log.LogDebug("Authentication completed IsEncrypted = {0}, IsSigned = {1}", args);
        }
        catch (Exception exception)
        {
            object[] objArray2 = new object[] { exception };
            s_log.LogWarning("Exception while ending client authentication. {0}", objArray2);
            connectFailed = true;
        }
        this.ExecuteBeginConnectDelegate(connectFailed);
    }

    private static bool OnValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        SslStream stream = (SslStream) sender;
        SslStreamValidationContext context = s_streamValidationContexts[stream];
        SslSocket socket = context.m_socket;
        List<string> commonNamesFromCertSubject = GetCommonNamesFromCertSubject(certificate.Subject);
        bool flag = false;
        if ((socket.m_bundleInfo != null) && socket.m_bundleInfo.isUsingCertBundle)
        {
            byte[] certBundleBytes = socket.m_bundleInfo.certBundleBytes;
            if (socket.m_bundleInfo.isCertBundleSigned)
            {
                if (!VerifyBundleSignature(socket.m_bundleInfo.certBundleBytes))
                {
                    return false;
                }
                certBundleBytes = GetUnsignedBundleBytes(socket.m_bundleInfo.certBundleBytes);
            }
            byte[] publicKey = certificate.GetPublicKey();
            foreach (string str in commonNamesFromCertSubject)
            {
                if (IsWhitelistedInCertBundle(certBundleBytes, str, publicKey))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }
        }
        bool flag3 = IsCertSignedByBlizzard(certificate);
        bool flag4 = false;
        flag4 = true;
        bool flag5 = (!flag3 && flag4) && (chain.ChainElements.Count == 1);
        try
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                SslPolicyErrors errors = (!flag3 && !flag5) ? SslPolicyErrors.None : (SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable);
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != SslPolicyErrors.None)
                {
                    string resolvedAddress = socket.m_resolvedAddress;
                    foreach (string str3 in commonNamesFromCertSubject)
                    {
                        if (str3.Equals(resolvedAddress))
                        {
                            errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
                            break;
                        }
                    }
                }
                if ((sslPolicyErrors & ~errors) != SslPolicyErrors.None)
                {
                    object[] args = new object[] { sslPolicyErrors };
                    s_log.LogWarning("Failed policy check. {0}", args);
                    return false;
                }
            }
            if (chain.ChainElements == null)
            {
                s_log.LogWarning("ChainElements is null");
                return false;
            }
            X509ChainElementEnumerator enumerator = chain.ChainElements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509ChainElement current = enumerator.Current;
                object[] objArray2 = new object[] { current.Certificate.Thumbprint };
                s_log.LogDebug("Certificate Thumbprint: {0}", objArray2);
                foreach (X509ChainStatus status in current.ChainElementStatus)
                {
                    object[] objArray3 = new object[] { status.Status };
                    s_log.LogDebug("  Certificate Status: {0}", objArray3);
                }
            }
            bool flag6 = false;
            if (flag3 && (chain.ChainElements.Count == 1))
            {
                chain.ChainPolicy.ExtraStore.Add(s_rootCertificate);
                chain.Build(new X509Certificate2(certificate));
                flag6 = true;
            }
            int num2 = !flag3 ? 3 : 2;
            if (flag4 && !flag6)
            {
                num2 = 1;
            }
            if (chain.ChainElements.Count != num2)
            {
                object[] objArray4 = new object[] { chain.ChainElements.Count };
                s_log.LogWarning("ChainElements.Count is {0}", objArray4);
                return false;
            }
            for (int i = 0; i < num2; i++)
            {
                if (chain.ChainElements[i] == null)
                {
                    s_log.LogWarning("ChainElements[" + i + "] is null");
                    return false;
                }
            }
            if (flag3)
            {
                string str4;
                if (ApplicationMgr.GetMobileEnvironment() == MobileEnv.PRODUCTION)
                {
                    str4 = "673D9D1072B625CAD95CB47BF0F0F512233E39FD";
                }
                else
                {
                    str4 = "C0805E3CF51F1A56CE9E6E35CB4F4901B68128B7";
                }
                if (chain.ChainElements[1].Certificate.Thumbprint != str4)
                {
                    s_log.LogWarning("Root certificate thumb print check failure");
                    object[] objArray5 = new object[] { str4 };
                    s_log.LogWarning("  expected: {0}", objArray5);
                    object[] objArray6 = new object[] { chain.ChainElements[1].Certificate.Thumbprint };
                    s_log.LogWarning("  received: {0}", objArray6);
                    return false;
                }
            }
            for (int j = 0; j < num2; j++)
            {
                if (DateTime.Now > chain.ChainElements[j].Certificate.NotAfter)
                {
                    s_log.LogWarning("ChainElements[" + j + "] certificate is expired.");
                    return false;
                }
            }
            X509ChainElementEnumerator enumerator4 = chain.ChainElements.GetEnumerator();
            while (enumerator4.MoveNext())
            {
                foreach (X509ChainStatus status2 in enumerator4.Current.ChainElementStatus)
                {
                    if ((!flag3 && !flag6) || (status2.Status != X509ChainStatusFlags.UntrustedRoot))
                    {
                        object[] objArray7 = new object[] { status2.Status };
                        s_log.LogWarning("Found unexpected chain error={0}.", objArray7);
                        return false;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            object[] objArray8 = new object[] { exception };
            s_log.LogWarning("Exception while trying to validate certificate. {0}", objArray8);
            return false;
        }
        return true;
    }

    public static void Process()
    {
        s_log.Process();
    }

    private void ReadCallback(IAsyncResult ar)
    {
        BeginReceiveDelegate asyncState = (BeginReceiveDelegate) ar.AsyncState;
        if ((this.m_socket == null) || (this.m_sslStream == null))
        {
            if (asyncState != null)
            {
                asyncState(0);
            }
        }
        else
        {
            try
            {
                int bytesReceived = this.m_sslStream.EndRead(ar);
                if (asyncState != null)
                {
                    asyncState(bytesReceived);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception };
                s_log.LogWarning("Exception while trying to call EndRead. {0}", args);
                if (asyncState != null)
                {
                    asyncState(0);
                }
            }
        }
    }

    private static bool VerifyBundleSignature(byte[] signedBundleData)
    {
        int inputCount = signedBundleData.Length - (s_magicBundleSignaturePreamble.Length + 0x100);
        if (inputCount <= 0)
        {
            return false;
        }
        byte[] bytes = Encoding.ASCII.GetBytes(s_magicBundleSignaturePreamble);
        for (int i = 0; i < bytes.Length; i++)
        {
            if (signedBundleData[inputCount + i] != bytes[i])
            {
                return false;
            }
        }
        SHA256 sha = SHA256.Create();
        sha.Initialize();
        sha.TransformBlock(signedBundleData, 0, inputCount, null, 0);
        string s = "Blizzard Certificate Bundle";
        byte[] inputBuffer = Encoding.ASCII.GetBytes(s);
        sha.TransformBlock(inputBuffer, 0, inputBuffer.Length, null, 0);
        sha.TransformFinalBlock(new byte[1], 0, 0);
        byte[] hash = sha.Hash;
        byte[] destinationArray = new byte[0x100];
        Array.Copy(signedBundleData, inputCount + s_magicBundleSignaturePreamble.Length, destinationArray, 0, 0x100);
        List<RSAParameters> list = new List<RSAParameters>();
        RSAParameters item = new RSAParameters {
            Modulus = s_standardPublicModulus,
            Exponent = s_standardPublicExponent
        };
        list.Add(item);
        for (int j = 0; j < list.Count; j++)
        {
            RSAParameters key = list[j];
            if (VerifySignedHash(key, hash, destinationArray))
            {
                return true;
            }
        }
        return false;
    }

    private static bool VerifySignedHash(RSAParameters key, byte[] hash, byte[] signature)
    {
        byte[] destinationArray = new byte[key.Modulus.Length];
        byte[] buffer2 = new byte[key.Exponent.Length];
        byte[] buffer3 = new byte[signature.Length];
        Array.Copy(key.Modulus, destinationArray, key.Modulus.Length);
        Array.Copy(key.Exponent, buffer2, key.Exponent.Length);
        Array.Copy(signature, buffer3, signature.Length);
        Array.Reverse(destinationArray);
        Array.Reverse(buffer2);
        Array.Reverse(buffer3);
        BigInteger mod = new BigInteger(destinationArray);
        BigInteger exp = new BigInteger(buffer2);
        BigInteger b = new BigInteger(buffer3);
        BigInteger integer4 = BigInteger.PowMod(b, exp, mod);
        byte[] buffer4 = new byte[key.Modulus.Length];
        byte[] id = new byte[] { 
            0x30, 0x31, 0x30, 13, 6, 9, 0x60, 0x86, 0x48, 1, 0x65, 3, 4, 2, 1, 5, 
            0, 4, 0x20, 0
         };
        if (!MakePKCS1SignatureBlock(hash, hash.Length, id, id.Length, buffer4, key.Modulus.Length))
        {
            return false;
        }
        byte[] buffer6 = new byte[buffer4.Length];
        Array.Copy(buffer4, buffer6, buffer4.Length);
        Array.Reverse(buffer6);
        BigInteger integer5 = new BigInteger(buffer6);
        return (integer5.CompareTo(integer4) == 0);
    }

    private void WriteCallback(IAsyncResult ar)
    {
        BeginSendDelegate asyncState = (BeginSendDelegate) ar.AsyncState;
        if ((this.m_socket == null) || (this.m_sslStream == null))
        {
            if (asyncState != null)
            {
                asyncState(false);
            }
        }
        else
        {
            try
            {
                this.m_sslStream.EndWrite(ar);
                this.m_canSend = true;
                if (asyncState != null)
                {
                    asyncState(true);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception };
                s_log.LogWarning("Exception while trying to call EndWrite. {0}", args);
                if (asyncState != null)
                {
                    asyncState(false);
                }
            }
        }
    }

    public bool Connected
    {
        get
        {
            return ((this.m_socket != null) && this.m_socket.Connected);
        }
    }

    public delegate void BeginConnectDelegate(bool connectFailed, bool isEncrypted, bool isSigned);

    public delegate void BeginReceiveDelegate(int bytesReceived);

    public delegate void BeginSendDelegate(bool wasSent);

    private enum HexStrToBytesError
    {
        [Description("Hex string has an odd number of digits")]
        ODD_NUMBER_OF_DIGITS = 1,
        [Description("OK")]
        OK = 0,
        [Description("Unknown error parsing hex string")]
        UNKNOWN = 2
    }

    private class SslStreamValidationContext
    {
        public SslSocket m_socket;
    }
}

