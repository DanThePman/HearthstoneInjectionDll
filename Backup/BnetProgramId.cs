using System;

[Serializable]
public class BnetProgramId : FourCC
{
    public static readonly BnetProgramId BNET = new BnetProgramId("BN");
    public static readonly BnetProgramId DIABLO3 = new BnetProgramId("D3");
    public static readonly BnetProgramId HEARTHSTONE = new BnetProgramId("WTCG");
    public static readonly BnetProgramId HEROES = new BnetProgramId("Hero");
    public static readonly BnetProgramId OVERWATCH = new BnetProgramId("Pro");
    public static readonly BnetProgramId PHOENIX = new BnetProgramId("App");
    public static readonly BnetProgramId PHOENIX_OLD = new BnetProgramId("CLNT");
    private static readonly Map<BnetProgramId, string> s_nameStringTagMap;
    private static readonly Map<BnetProgramId, string> s_textureNameMap;
    public static readonly BnetProgramId STARCRAFT2 = new BnetProgramId("S2");
    public static readonly BnetProgramId WOW = new BnetProgramId("WoW");

    static BnetProgramId()
    {
        Map<BnetProgramId, string> map = new Map<BnetProgramId, string>();
        map.Add(HEARTHSTONE, "HS");
        map.Add(WOW, "WOW");
        map.Add(DIABLO3, "D3");
        map.Add(STARCRAFT2, "SC2");
        map.Add(PHOENIX, "BN");
        map.Add(PHOENIX_OLD, "BN");
        map.Add(HEROES, "Heroes");
        map.Add(OVERWATCH, "Overwatch");
        s_textureNameMap = map;
        map = new Map<BnetProgramId, string>();
        map.Add(HEARTHSTONE, "GLOBAL_PROGRAMNAME_HEARTHSTONE");
        map.Add(WOW, "GLOBAL_PROGRAMNAME_WOW");
        map.Add(DIABLO3, "GLOBAL_PROGRAMNAME_DIABLO3");
        map.Add(STARCRAFT2, "GLOBAL_PROGRAMNAME_STARCRAFT2");
        map.Add(PHOENIX, "GLOBAL_PROGRAMNAME_PHOENIX");
        map.Add(PHOENIX_OLD, "GLOBAL_PROGRAMNAME_PHOENIX");
        map.Add(HEROES, "GLOBAL_PROGRAMNAME_HEROES");
        map.Add(OVERWATCH, "GLOBAL_PROGRAMNAME_OVERWATCH");
        s_nameStringTagMap = map;
    }

    public BnetProgramId()
    {
    }

    public BnetProgramId(string stringVal) : base(stringVal)
    {
    }

    public BnetProgramId(uint val) : base(val)
    {
    }

    public BnetProgramId Clone()
    {
        return (BnetProgramId) base.MemberwiseClone();
    }

    public static string GetName(BnetProgramId programId)
    {
        if (programId != null)
        {
            string str = null;
            if (s_nameStringTagMap.TryGetValue(programId, out str))
            {
                return GameStrings.Get(str);
            }
        }
        return null;
    }

    public static string GetNameTag(BnetProgramId programId)
    {
        if (programId == null)
        {
            return null;
        }
        string str = null;
        s_nameStringTagMap.TryGetValue(programId, out str);
        return str;
    }

    public static string GetTextureName(BnetProgramId programId)
    {
        if (programId == null)
        {
            return null;
        }
        string str = null;
        s_textureNameMap.TryGetValue(programId, out str);
        return str;
    }

    public bool IsGame()
    {
        return ((this != PHOENIX) && (this != PHOENIX_OLD));
    }

    public bool IsPhoenix()
    {
        return ((this == PHOENIX) || (this == PHOENIX_OLD));
    }
}

