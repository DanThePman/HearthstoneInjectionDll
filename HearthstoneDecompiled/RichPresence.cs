using System;

public class RichPresence
{
    public const uint FIELD_INDEX_START = 0x70000;
    public static readonly Map<System.Type, FourCC> s_streamIds;
    public static readonly FourCC SCENARIOS_STREAMID = new FourCC("scen");
    public static readonly FourCC STATUS_STREAMID = new FourCC("stat");
    public static readonly FourCC TUTORIAL_STREAMID = new FourCC("tut");

    static RichPresence()
    {
        Map<System.Type, FourCC> map = new Map<System.Type, FourCC>();
        map.Add(typeof(PresenceStatus), STATUS_STREAMID);
        map.Add(typeof(PresenceTutorial), TUTORIAL_STREAMID);
        map.Add(typeof(ScenarioDbId), SCENARIOS_STREAMID);
        s_streamIds = map;
    }
}

