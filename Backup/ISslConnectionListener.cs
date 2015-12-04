using System;

public interface ISslConnectionListener
{
    void PacketReceived(BattleNetPacket p, object state);
}

