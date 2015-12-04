using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileDeviceLocale
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3D;
    public const string ptrServerVersion = "BETA";
    private static Map<string, int> s_countryCodeToRegionId;
    private static Network.BnetRegion s_defaultDevRegion;
    private static ConnectionData s_defaultProdIP;
    private static int s_defaultRegionId;
    private static Map<string, Locale> s_languageCodeToLocale;
    public static Map<Network.BnetRegion, ConnectionData> s_regionIdToDevIP;
    private static Map<Network.BnetRegion, ConnectionData> s_regionIdToProdIP;

    static MobileDeviceLocale()
    {
        Map<string, Locale> map = new Map<string, Locale>();
        map.Add("en", Locale.enUS);
        map.Add("fr", Locale.frFR);
        map.Add("de", Locale.deDE);
        map.Add("ko", Locale.koKR);
        map.Add("ru", Locale.ruRU);
        map.Add("it", Locale.itIT);
        map.Add("pt", Locale.ptBR);
        map.Add("pl", Locale.plPL);
        map.Add("ja", Locale.jaJP);
        map.Add("en-AU", Locale.enUS);
        map.Add("en-GB", Locale.enUS);
        map.Add("fr-CA", Locale.frFR);
        map.Add("es-MX", Locale.esMX);
        map.Add("zh-Hans", Locale.zhCN);
        map.Add("zh-Hant", Locale.zhTW);
        map.Add("pt-PT", Locale.ptBR);
        s_languageCodeToLocale = map;
        Map<string, int> map2 = new Map<string, int>();
        map2.Add("AD", 2);
        map2.Add("AE", 2);
        map2.Add("AG", 1);
        map2.Add("AL", 2);
        map2.Add("AM", 2);
        map2.Add("AO", 2);
        map2.Add("AR", 1);
        map2.Add("AT", 2);
        map2.Add("AU", 1);
        map2.Add("AZ", 2);
        map2.Add("BA", 2);
        map2.Add("BB", 1);
        map2.Add("BD", 1);
        map2.Add("BE", 2);
        map2.Add("BF", 2);
        map2.Add("BG", 2);
        map2.Add("BH", 2);
        map2.Add("BI", 2);
        map2.Add("BJ", 2);
        map2.Add("BM", 2);
        map2.Add("BN", 1);
        map2.Add("BO", 1);
        map2.Add("BR", 1);
        map2.Add("BS", 1);
        map2.Add("BT", 1);
        map2.Add("BW", 2);
        map2.Add("BY", 2);
        map2.Add("BZ", 1);
        map2.Add("CA", 1);
        map2.Add("CD", 2);
        map2.Add("CF", 2);
        map2.Add("CG", 2);
        map2.Add("CH", 2);
        map2.Add("CI", 2);
        map2.Add("CL", 1);
        map2.Add("CM", 2);
        map2.Add("CN", 3);
        map2.Add("CO", 1);
        map2.Add("CR", 1);
        map2.Add("CU", 1);
        map2.Add("CV", 2);
        map2.Add("CY", 2);
        map2.Add("CZ", 2);
        map2.Add("DE", 2);
        map2.Add("DJ", 2);
        map2.Add("DK", 2);
        map2.Add("DM", 1);
        map2.Add("DO", 1);
        map2.Add("DZ", 2);
        map2.Add("EC", 1);
        map2.Add("EE", 2);
        map2.Add("EG", 2);
        map2.Add("ER", 2);
        map2.Add("ES", 2);
        map2.Add("ET", 2);
        map2.Add("FI", 2);
        map2.Add("FJ", 1);
        map2.Add("FK", 2);
        map2.Add("FO", 2);
        map2.Add("FR", 2);
        map2.Add("GA", 2);
        map2.Add("GB", 2);
        map2.Add("GD", 1);
        map2.Add("GE", 2);
        map2.Add("GL", 2);
        map2.Add("GM", 2);
        map2.Add("GN", 2);
        map2.Add("GQ", 2);
        map2.Add("GR", 2);
        map2.Add("GS", 2);
        map2.Add("GT", 1);
        map2.Add("GW", 2);
        map2.Add("GY", 1);
        map2.Add("HK", 3);
        map2.Add("HN", 1);
        map2.Add("HR", 2);
        map2.Add("HT", 1);
        map2.Add("HU", 2);
        map2.Add("ID", 1);
        map2.Add("IE", 2);
        map2.Add("IL", 2);
        map2.Add("IM", 2);
        map2.Add("IN", 1);
        map2.Add("IQ", 2);
        map2.Add("IR", 2);
        map2.Add("IS", 2);
        map2.Add("IT", 2);
        map2.Add("JM", 1);
        map2.Add("JO", 2);
        map2.Add("JP", 3);
        map2.Add("KE", 2);
        map2.Add("KG", 2);
        map2.Add("KH", 2);
        map2.Add("KI", 1);
        map2.Add("KM", 2);
        map2.Add("KP", 1);
        map2.Add("KR", 3);
        map2.Add("KW", 2);
        map2.Add("KY", 2);
        map2.Add("KZ", 2);
        map2.Add("LA", 1);
        map2.Add("LB", 2);
        map2.Add("LC", 1);
        map2.Add("LI", 2);
        map2.Add("LK", 1);
        map2.Add("LR", 2);
        map2.Add("LS", 2);
        map2.Add("LT", 2);
        map2.Add("LU", 2);
        map2.Add("LV", 2);
        map2.Add("LY", 2);
        map2.Add("MA", 2);
        map2.Add("MC", 2);
        map2.Add("MD", 2);
        map2.Add("ME", 2);
        map2.Add("MG", 2);
        map2.Add("MK", 2);
        map2.Add("ML", 2);
        map2.Add("MM", 1);
        map2.Add("MN", 2);
        map2.Add("MO", 3);
        map2.Add("MR", 2);
        map2.Add("MT", 2);
        map2.Add("MU", 2);
        map2.Add("MV", 2);
        map2.Add("MW", 2);
        map2.Add("MX", 1);
        map2.Add("MY", 1);
        map2.Add("MZ", 2);
        map2.Add("NA", 2);
        map2.Add("NC", 2);
        map2.Add("NE", 2);
        map2.Add("NG", 2);
        map2.Add("NI", 1);
        map2.Add("NL", 2);
        map2.Add("NO", 2);
        map2.Add("NP", 1);
        map2.Add("NR", 1);
        map2.Add("NZ", 1);
        map2.Add("OM", 2);
        map2.Add("PA", 1);
        map2.Add("PE", 1);
        map2.Add("PF", 1);
        map2.Add("PG", 1);
        map2.Add("PH", 1);
        map2.Add("PK", 2);
        map2.Add("PL", 2);
        map2.Add("PT", 2);
        map2.Add("PY", 1);
        map2.Add("QA", 2);
        map2.Add("RO", 2);
        map2.Add("RS", 2);
        map2.Add("RU", 2);
        map2.Add("RW", 2);
        map2.Add("SA", 2);
        map2.Add("SB", 1);
        map2.Add("SC", 2);
        map2.Add("SD", 2);
        map2.Add("SE", 2);
        map2.Add("SG", 1);
        map2.Add("SH", 2);
        map2.Add("SI", 2);
        map2.Add("SK", 2);
        map2.Add("SL", 2);
        map2.Add("SN", 2);
        map2.Add("SO", 2);
        map2.Add("SR", 2);
        map2.Add("ST", 2);
        map2.Add("SV", 1);
        map2.Add("SY", 2);
        map2.Add("SZ", 2);
        map2.Add("TD", 2);
        map2.Add("TG", 2);
        map2.Add("TH", 1);
        map2.Add("TJ", 2);
        map2.Add("TL", 1);
        map2.Add("TM", 2);
        map2.Add("TN", 2);
        map2.Add("TO", 1);
        map2.Add("TR", 2);
        map2.Add("TT", 1);
        map2.Add("TV", 1);
        map2.Add("TW", 3);
        map2.Add("TZ", 2);
        map2.Add("UA", 2);
        map2.Add("UG", 2);
        map2.Add("US", 1);
        map2.Add("UY", 1);
        map2.Add("UZ", 2);
        map2.Add("VA", 2);
        map2.Add("VC", 1);
        map2.Add("VE", 1);
        map2.Add("VN", 1);
        map2.Add("VU", 1);
        map2.Add("WS", 1);
        map2.Add("YE", 2);
        map2.Add("YU", 2);
        map2.Add("ZA", 2);
        map2.Add("ZM", 2);
        map2.Add("ZW", 2);
        s_countryCodeToRegionId = map2;
        s_defaultRegionId = 1;
        Map<Network.BnetRegion, ConnectionData> map3 = new Map<Network.BnetRegion, ConnectionData>();
        ConnectionData data = new ConnectionData {
            address = "us.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_UNKNOWN, data);
        ConnectionData data2 = new ConnectionData {
            address = "us.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_US, data2);
        ConnectionData data3 = new ConnectionData {
            address = "eu.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_EU, data3);
        ConnectionData data4 = new ConnectionData {
            address = "kr.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_KR, data4);
        ConnectionData data5 = new ConnectionData {
            address = "kr.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_TW, data5);
        ConnectionData data6 = new ConnectionData {
            address = "cn.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        map3.Add(Network.BnetRegion.REGION_CN, data6);
        ConnectionData data7 = new ConnectionData {
            address = "beta.actual.battle.net",
            port = 0x45f,
            version = "LOC"
        };
        map3.Add(Network.BnetRegion.REGION_PTR_LOC, data7);
        s_regionIdToProdIP = map3;
        ConnectionData data8 = new ConnectionData {
            address = "us.actual.battle.net",
            port = 0x45f,
            version = "product"
        };
        s_defaultProdIP = data8;
        map3 = new Map<Network.BnetRegion, ConnectionData>();
        ConnectionData data9 = new ConnectionData {
            name = "bn11-dev",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "dev11",
            tutorialPort = 0xafcd
        };
        map3.Add(Network.BnetRegion.REGION_US, data9);
        ConnectionData data10 = new ConnectionData {
            name = "bn11-qa1",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "qa1",
            tutorialPort = 0xafd0
        };
        map3.Add(Network.BnetRegion.REGION_DEV, data10);
        ConnectionData data11 = new ConnectionData {
            name = "bn11-qa2",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "qa2",
            tutorialPort = 0xafd1
        };
        map3.Add(Network.BnetRegion.REGION_DEV | Network.BnetRegion.REGION_US, data11);
        ConnectionData data12 = new ConnectionData {
            name = "bn11-qa3",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "qa3",
            tutorialPort = 0xafee
        };
        map3.Add(Network.BnetRegion.REGION_DEV | Network.BnetRegion.REGION_EU, data12);
        ConnectionData data13 = new ConnectionData {
            name = "bn11-eu",
            address = "bn11-01.battle.net",
            port = 0x2b72,
            version = "eu11",
            tutorialPort = 0xafce
        };
        map3.Add(Network.BnetRegion.REGION_DEV | Network.BnetRegion.REGION_KR, data13);
        ConnectionData data14 = new ConnectionData {
            name = "bn11-kr",
            address = "bn11-01.battle.net",
            port = 0x2b73,
            version = "kr11",
            tutorialPort = 0xafcf
        };
        map3.Add((Network.BnetRegion) 0x40, data14);
        ConnectionData data15 = new ConnectionData {
            name = "bn11-rc",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "product",
            tutorialPort = 0xafe1
        };
        map3.Add((Network.BnetRegion) 0x41, data15);
        ConnectionData data16 = new ConnectionData {
            name = "bn12-dev",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "dev12",
            tutorialPort = 0xafd4
        };
        map3.Add((Network.BnetRegion) 0x42, data16);
        ConnectionData data17 = new ConnectionData {
            name = "bn12-qa4",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "qa4",
            tutorialPort = 0xafd5
        };
        map3.Add((Network.BnetRegion) 0x43, data17);
        ConnectionData data18 = new ConnectionData {
            name = "bn12-qa5",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "qa5",
            tutorialPort = 0xafd6
        };
        map3.Add((Network.BnetRegion) 0x44, data18);
        ConnectionData data19 = new ConnectionData {
            name = "bn12-qa6",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "qa6",
            tutorialPort = 0xafef
        };
        map3.Add((Network.BnetRegion) 0x45, data19);
        ConnectionData data20 = new ConnectionData {
            name = "bn12-rc",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "product",
            tutorialPort = 0xafe4
        };
        map3.Add((Network.BnetRegion) 70, data20);
        ConnectionData data21 = new ConnectionData {
            name = "bn8-dev",
            address = "bn8-01.battle.net",
            port = 0x45f,
            version = "dev8",
            tutorialPort = 0xafdd
        };
        map3.Add((Network.BnetRegion) 0x47, data21);
        ConnectionData data22 = new ConnectionData {
            name = "bn8-qa7",
            address = "bn8-01.battle.net",
            port = 0x45f,
            version = "qa7",
            tutorialPort = 0xafde
        };
        map3.Add((Network.BnetRegion) 0x48, data22);
        ConnectionData data23 = new ConnectionData {
            name = "bn8-qa8",
            address = "bn8-01.battle.net",
            port = 0x45f,
            version = "qa8",
            tutorialPort = 0xafec
        };
        map3.Add((Network.BnetRegion) 0x49, data23);
        ConnectionData data24 = new ConnectionData {
            name = "bn8-qa9",
            address = "bn8-01.battle.net",
            port = 0x45f,
            version = "qa9",
            tutorialPort = 0xafed
        };
        map3.Add((Network.BnetRegion) 0x4a, data24);
        ConnectionData data25 = new ConnectionData {
            name = "bn8-eu",
            address = "bn8-01.battle.net",
            port = 0x2b72,
            version = "eu8",
            tutorialPort = 0xafc8
        };
        map3.Add((Network.BnetRegion) 0x4b, data25);
        ConnectionData data26 = new ConnectionData {
            name = "bn8-kr",
            address = "bn8-01.battle.net",
            port = 0x2b73,
            version = "kr8",
            tutorialPort = 0xafc9
        };
        map3.Add((Network.BnetRegion) 0x4c, data26);
        ConnectionData data27 = new ConnectionData {
            name = "bn8-rc",
            address = "bn8-01.battle.net",
            port = 0x45f,
            version = "product",
            tutorialPort = 0xafdf
        };
        map3.Add((Network.BnetRegion) 0x4d, data27);
        ConnectionData data28 = new ConnectionData {
            name = "bn11-mschweitzer",
            address = "bn11-01.battle.net",
            port = 0x45f,
            version = "mschweitzer"
        };
        map3.Add(Network.BnetRegion.REGION_MSCHWEITZER_BN11, data28);
        ConnectionData data29 = new ConnectionData {
            name = "bn12-mschweitzer",
            address = "bn12-01.battle.net",
            port = 0x45f,
            version = "mschweitzer"
        };
        map3.Add(Network.BnetRegion.REGION_MSCHWEITZER_BN12, data29);
        s_regionIdToDevIP = map3;
        s_defaultDevRegion = Network.BnetRegion.REGION_US;
    }

    public static Network.BnetRegion FindDevRegionByServerVersion(string version)
    {
        foreach (Network.BnetRegion region in s_regionIdToDevIP.Keys)
        {
            ConnectionData data = s_regionIdToDevIP[region];
            if (version == data.version)
            {
                return region;
            }
        }
        return Network.BnetRegion.REGION_UNINITIALIZED;
    }

    public static Locale GetBestGuessForLocale()
    {
        Locale enUS = Locale.enUS;
        bool flag = false;
        string languageCode = GetLanguageCode();
        try
        {
            flag = s_languageCodeToLocale.TryGetValue(languageCode, out enUS);
        }
        catch (Exception)
        {
        }
        if (!flag)
        {
            languageCode = languageCode.Substring(0, 2);
            try
            {
                flag = s_languageCodeToLocale.TryGetValue(languageCode, out enUS);
            }
            catch (Exception)
            {
            }
        }
        if (flag)
        {
            return enUS;
        }
        int num = 1;
        string countryCode = GetCountryCode();
        try
        {
            s_countryCodeToRegionId.TryGetValue(countryCode, out num);
        }
        catch (Exception)
        {
        }
        string key = languageCode;
        if (key != null)
        {
            int num2;
            if (<>f__switch$map3D == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                dictionary.Add("es", 0);
                dictionary.Add("zh", 1);
                <>f__switch$map3D = dictionary;
            }
            if (<>f__switch$map3D.TryGetValue(key, out num2))
            {
                if (num2 == 0)
                {
                    if (num == 1)
                    {
                        return Locale.esMX;
                    }
                    return Locale.esES;
                }
                if (num2 == 1)
                {
                    if (countryCode == "CN")
                    {
                        return Locale.zhCN;
                    }
                    return Locale.zhTW;
                }
            }
        }
        return Locale.enUS;
    }

    public static ConnectionData GetConnectionDataFromRegionId(Network.BnetRegion region, bool isDev)
    {
        ConnectionData data;
        if (isDev)
        {
            if (!s_regionIdToDevIP.TryGetValue(region, out data) && !s_regionIdToDevIP.TryGetValue(s_defaultDevRegion, out data))
            {
                Debug.LogError("Invalid region set for s_defaultDevRegion!  This should never happen!!!");
            }
            return data;
        }
        if (!s_regionIdToProdIP.TryGetValue(region, out data))
        {
            data = s_defaultProdIP;
        }
        return data;
    }

    public static string GetCountryCode()
    {
        return GetLocaleCountryCode();
    }

    public static Network.BnetRegion GetCurrentRegionId()
    {
        int @int = Options.Get().GetInt(Option.PREFERRED_REGION);
        if (@int < 0)
        {
            if (UseClientConfigForEnv())
            {
                Network.BnetRegion region = FindDevRegionByServerVersion(Vars.Key("Aurora.Version.String").GetStr(string.Empty));
                Log.JMac.Print("Battle.net region from client.config version: " + region, new object[0]);
                if (region != Network.BnetRegion.REGION_UNINITIALIZED)
                {
                    return region;
                }
            }
            try
            {
                if (!s_countryCodeToRegionId.TryGetValue(GetCountryCode(), out @int))
                {
                    @int = s_defaultRegionId;
                }
            }
            catch (Exception)
            {
            }
        }
        return (Network.BnetRegion) @int;
    }

    public static string GetLanguageCode()
    {
        return GetLocaleLanguageCode();
    }

    private static string GetLocaleCountryCode()
    {
        return string.Empty;
    }

    private static string GetLocaleLanguageCode()
    {
        return string.Empty;
    }

    public static List<string> GetRegionCodesForCurrentRegionId()
    {
        List<string> list = new List<string>();
        int currentRegionId = (int) GetCurrentRegionId();
        foreach (KeyValuePair<string, int> pair in s_countryCodeToRegionId)
        {
            if (pair.Value == currentRegionId)
            {
                list.Add(pair.Key);
            }
        }
        return list;
    }

    public static bool UseClientConfigForEnv()
    {
        bool flag = Vars.Key("Aurora.Env.Override").GetInt(0) != 0;
        if (Vars.Key("Aurora.Env.DisableOverrideOnDevices").GetInt(0) != 0)
        {
            flag = false;
        }
        string str = Vars.Key("Aurora.Env").GetStr(string.Empty);
        bool flag3 = (str != null) && (str != string.Empty);
        return (flag && flag3);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectionData
    {
        public string address;
        public int port;
        public string version;
        public string name;
        public int tutorialPort;
    }
}

