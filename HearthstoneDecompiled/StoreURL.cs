using System;
using UnityEngine;

public class StoreURL
{
    private string m_format;
    private Param m_param1;
    private Param m_param2;
    private static readonly Map<Network.BnetRegion, string> s_regionToStrMap;

    static StoreURL()
    {
        Map<Network.BnetRegion, string> map = new Map<Network.BnetRegion, string>();
        map.Add(Network.BnetRegion.REGION_US, "US");
        map.Add(Network.BnetRegion.REGION_EU, "EU");
        map.Add(Network.BnetRegion.REGION_KR, "KR");
        map.Add(Network.BnetRegion.REGION_TW, "TW");
        map.Add(Network.BnetRegion.REGION_CN, "CN");
        map.Add(Network.BnetRegion.REGION_PTR, "US");
        s_regionToStrMap = map;
    }

    public StoreURL(string format, Param param1, Param param2)
    {
        this.m_format = format;
        this.m_param1 = param1;
        this.m_param2 = param2;
    }

    private string GetParamString(Param paramType)
    {
        switch (paramType)
        {
            case Param.LOCALE:
                return Localization.GetLocale().ToString();

            case Param.REGION:
            {
                Network.BnetRegion accountRegion = BattleNet.GetAccountRegion();
                if (!s_regionToStrMap.ContainsKey(accountRegion))
                {
                    Debug.LogError(string.Format("StoreURL unrecognized region {0}", accountRegion));
                    return s_regionToStrMap[Network.BnetRegion.REGION_US];
                }
                return s_regionToStrMap[accountRegion];
            }
        }
        return string.Empty;
    }

    public string GetURL()
    {
        return string.Format(this.m_format, this.GetParamString(this.m_param1), this.GetParamString(this.m_param2));
    }

    public enum Param
    {
        NONE,
        LOCALE,
        REGION
    }
}

