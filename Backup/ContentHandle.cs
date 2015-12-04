using bnet.protocol;
using System;
using System.Runtime.CompilerServices;
using System.Text;

public class ContentHandle
{
    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder builder = new StringBuilder(ba.Length * 2);
        foreach (byte num in ba)
        {
            builder.AppendFormat("{0:x2}", num);
        }
        return builder.ToString();
    }

    public static ContentHandle FromProtocol(bnet.protocol.ContentHandle contentHandle)
    {
        if ((contentHandle == null) || !contentHandle.IsInitialized)
        {
            return null;
        }
        return new ContentHandle { Region = new FourCC(contentHandle.Region).ToString(), Usage = new FourCC(contentHandle.Usage).ToString(), Sha256Digest = ByteArrayToString(contentHandle.Hash) };
    }

    public override string ToString()
    {
        return string.Format("Region={0} Usage={1} Sha256={2}", this.Region, this.Usage, this.Sha256Digest);
    }

    public string Region { get; set; }

    public string Sha256Digest { get; set; }

    public string Usage { get; set; }
}

