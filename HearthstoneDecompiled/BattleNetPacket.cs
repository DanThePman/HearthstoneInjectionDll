using bnet.protocol;
using System;
using System.IO;

public class BattleNetPacket : PacketFormat
{
    private object body;
    private int bodySize;
    private bnet.protocol.Header header;
    private int headerSize;

    public BattleNetPacket()
    {
        this.headerSize = -1;
        this.bodySize = -1;
        this.header = null;
        this.body = null;
    }

    public BattleNetPacket(bnet.protocol.Header h, IProtoBuf b)
    {
        this.headerSize = -1;
        this.bodySize = -1;
        this.header = h;
        this.body = b;
    }

    public override int Decode(byte[] bytes, int offset, int available)
    {
        int num = 0;
        if (this.headerSize < 0)
        {
            if (available < 2)
            {
                return num;
            }
            this.headerSize = (bytes[offset] << 8) + bytes[offset + 1];
            available -= 2;
            num += 2;
            offset += 2;
        }
        if (this.header == null)
        {
            if (available < this.headerSize)
            {
                return num;
            }
            this.header = new bnet.protocol.Header();
            this.header.Deserialize(new MemoryStream(bytes, offset, this.headerSize));
            this.bodySize = !this.header.HasSize ? 0 : ((int) this.header.Size);
            if (this.header == null)
            {
                throw new Exception("failed to parse BattleNet packet header");
            }
            available -= this.headerSize;
            num += this.headerSize;
            offset += this.headerSize;
        }
        if (this.body != null)
        {
            return num;
        }
        if (available < this.bodySize)
        {
            return num;
        }
        byte[] destinationArray = new byte[this.bodySize];
        Array.Copy(bytes, offset, destinationArray, 0, this.bodySize);
        this.body = destinationArray;
        return (num + this.bodySize);
    }

    public override byte[] Encode()
    {
        if (!(this.body is IProtoBuf))
        {
            return null;
        }
        IProtoBuf body = (IProtoBuf) this.body;
        int serializedSize = (int) this.header.GetSerializedSize();
        int count = (int) body.GetSerializedSize();
        byte[] buffer = new byte[(2 + serializedSize) + count];
        buffer[0] = (byte) ((serializedSize >> 8) & 0xff);
        buffer[1] = (byte) (serializedSize & 0xff);
        this.header.Serialize(new MemoryStream(buffer, 2, serializedSize));
        body.Serialize(new MemoryStream(buffer, 2 + serializedSize, count));
        return buffer;
    }

    public object GetBody()
    {
        return this.body;
    }

    public bnet.protocol.Header GetHeader()
    {
        return this.header;
    }

    public override bool IsLoaded()
    {
        return ((this.header != null) && (this.body != null));
    }
}

