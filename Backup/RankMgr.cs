using System;
using System.Collections.Generic;

public class RankMgr
{
    public const int INVALID_RANK = -1;
    private TranslatedMedalInfo m_medalInfo;
    private static RankMgr s_instance;

    public static RankMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new RankMgr();
        }
        return s_instance;
    }

    public MedalInfoTranslator GetRankPresenceField(BnetGameAccount gameAccount)
    {
        byte[] buffer;
        if ((gameAccount == null) || !gameAccount.TryGetGameFieldBytes(0x12, out buffer))
        {
            return null;
        }
        if (buffer == null)
        {
            return null;
        }
        if (buffer.Length < 3)
        {
            return null;
        }
        int rank = Convert.ToInt32(buffer[0]);
        return new MedalInfoTranslator(rank, BitConverter.ToUInt16(buffer, 1));
    }

    public MedalInfoTranslator GetRankPresenceField(BnetPlayer player)
    {
        return this.GetRankPresenceField(player.GetHearthstoneGameAccount());
    }

    public bool SetRankPresenceField(NetCache.NetCacheMedalInfo medalInfo)
    {
        TranslatedMedalInfo currentMedal = new MedalInfoTranslator(medalInfo).GetCurrentMedal();
        List<byte> list = new List<byte>();
        byte item = Convert.ToByte(currentMedal.rank);
        int legendIndex = currentMedal.legendIndex;
        list.Add(item);
        byte[] bytes = BitConverter.GetBytes(legendIndex);
        list.Add(bytes[0]);
        list.Add(bytes[1]);
        byte[] val = list.ToArray();
        return BnetPresenceMgr.Get().SetGameField(0x12, val);
    }
}

