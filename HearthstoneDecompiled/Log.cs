using System;
using System.IO;

public class Log
{
    public static Logger Achievements = new Logger("Achievements");
    public static Logger AdTracking = new Logger("AdTracking");
    public static Logger Arena = new Logger("Arena");
    public static Logger Asset = new Logger("Asset");
    public static Logger BattleNet = new Logger("BattleNet");
    public static Logger Becca = new Logger("Becca");
    public static Logger Ben = new Logger("Ben");
    public static Logger BIReport = new Logger("BIReport");
    public static Logger Bob = new Logger("Bob");
    public static Logger Brian = new Logger("Brian");
    public static Logger BugReporter = new Logger("BugReporter");
    public static Logger Cameron = new Logger("Cameron");
    public static Logger CardbackMgr = new Logger("CardbackMgr");
    public static Logger ClientRequestManager = new Logger("ClientRequestManager");
    private const string CONFIG_FILE_NAME = "log.config";
    public static Logger ConfigFile = new Logger("ConfigFile");
    public static Logger DbfXml = new Logger("DbfXml");
    private readonly LogInfo[] DEFAULT_LOG_INFOS = new LogInfo[0];
    public static Logger Derek = new Logger("Derek");
    public static Logger DeviceEmulation = new Logger("DeviceEmulation");
    public static Logger Downloader = new Logger("Downloader");
    public static Logger EndOfGame = new Logger("EndOfGame");
    public static Logger EventTiming = new Logger("EventTiming");
    public static Logger FaceDownCard = new Logger("FaceDownCard");
    public static Logger FullScreenFX = new Logger("FullScreenFX");
    public static Logger GameMgr = new Logger("GameMgr");
    public static Logger Graphics = new Logger("Graphics");
    public static Logger Hand = new Logger("Hand");
    public static Logger HealthyGaming = new Logger("HealthyGaming");
    public static Logger Henry = new Logger("Henry");
    public static Logger InnKeepersSpecial = new Logger("InnKeepersSpecial");
    public static Logger Jay = new Logger("Jay");
    public static Logger JMac = new Logger("JMac");
    public static Logger Josh = new Logger("Josh");
    public static Logger Kyle = new Logger("Kyle");
    public static Logger LoadingScreen = new Logger("LoadingScreen");
    private Map<string, LogInfo> m_logInfos = new Map<string, LogInfo>();
    public static Logger Mike = new Logger("Mike");
    public static Logger MikeH = new Logger("MikeH");
    public static Logger MissingAssets = new Logger("MissingAssets");
    public static Logger Net = new Logger("Net");
    public static Logger Packets = new Logger("Packet");
    public static Logger Party = new Logger("Party");
    public static Logger PlayErrors = new Logger("PlayErrors");
    public static Logger Power = new Logger("Power");
    public static Logger Rachelle = new Logger("Rachelle");
    public static Logger Reset = new Logger("Reset");
    public static Logger Robin = new Logger("Robin");
    public static Logger Ryan = new Logger("Ryan");
    private static Log s_instance;
    public static Logger Sound = new Logger("Sound");
    public static Logger Spectator = new Logger("Spectator");
    public static Logger UpdateManager = new Logger("UpdateManager");
    public static Logger Yim = new Logger("Yim");
    public static Logger Zone = new Logger("Zone");

    public static Log Get()
    {
        if (s_instance == null)
        {
            s_instance = new Log();
            s_instance.Initialize();
        }
        return s_instance;
    }

    public LogInfo GetLogInfo(string name)
    {
        LogInfo info = null;
        this.m_logInfos.TryGetValue(name, out info);
        return info;
    }

    private void Initialize()
    {
        this.Load();
    }

    public void Load()
    {
        string path = string.Format("{0}/{1}", FileUtils.PersistentDataPath, "log.config");
        if (System.IO.File.Exists(path))
        {
            this.m_logInfos.Clear();
            this.LoadConfig(path);
        }
        foreach (LogInfo info in this.DEFAULT_LOG_INFOS)
        {
            if (!this.m_logInfos.ContainsKey(info.m_name))
            {
                this.m_logInfos.Add(info.m_name, info);
            }
        }
        ConfigFile.Print("log.config location: " + path, new object[0]);
    }

    private void LoadConfig(string path)
    {
        ConfigFile file = new ConfigFile();
        if (file.LightLoad(path))
        {
            foreach (ConfigFile.Line line in file.GetLines())
            {
                LogInfo info;
                string sectionName = line.m_sectionName;
                string lineKey = line.m_lineKey;
                string strVal = line.m_value;
                if (!this.m_logInfos.TryGetValue(sectionName, out info))
                {
                    info = new LogInfo {
                        m_name = sectionName
                    };
                    this.m_logInfos.Add(info.m_name, info);
                }
                if (lineKey.Equals("ConsolePrinting", StringComparison.OrdinalIgnoreCase))
                {
                    info.m_consolePrinting = GeneralUtils.ForceBool(strVal);
                }
                else if (lineKey.Equals("ScreenPrinting", StringComparison.OrdinalIgnoreCase))
                {
                    info.m_screenPrinting = GeneralUtils.ForceBool(strVal);
                }
                else if (lineKey.Equals("FilePrinting", StringComparison.OrdinalIgnoreCase))
                {
                    info.m_filePrinting = GeneralUtils.ForceBool(strVal);
                }
                else if (lineKey.Equals("MinLevel", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        LogLevel level = EnumUtils.GetEnum<LogLevel>(strVal, StringComparison.OrdinalIgnoreCase);
                        info.m_minLevel = level;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                else if (lineKey.Equals("DefaultLevel", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        LogLevel level2 = EnumUtils.GetEnum<LogLevel>(strVal, StringComparison.OrdinalIgnoreCase);
                        info.m_defaultLevel = level2;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                else if (lineKey.Equals("Verbose", StringComparison.OrdinalIgnoreCase))
                {
                    info.m_verbose = GeneralUtils.ForceBool(strVal);
                }
            }
        }
    }
}

