using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NydusLink
{
    private static string DevHost = "https://nydus-qa.web.blizzard.net";
    private static string ProdHost = "https://nydus.battle.net";
    private static readonly Map<string, string> TargetServerToRegion;

    static NydusLink()
    {
        Map<string, string> map = new Map<string, string>();
        map.Add("us.actual.battle.net", "US");
        map.Add("eu.actual.battle.net", "EU");
        map.Add("kr.actual.battle.net", "KR");
        map.Add("cn.actual.battle.net", "CN");
        map.Add("bn11-01.battle.net", "US");
        TargetServerToRegion = map;
    }

    public static string GetAccountCreationLink()
    {
        return GetLink("creation", true);
    }

    public static string GetBreakingNewsLink()
    {
        return GetLink("alert", true);
    }

    public static string GetLink(string linkType, bool isMobile)
    {
        string str;
        string str2;
        string str3;
        GetLocalizedLinkVars(out str, out str2, out str3);
        string str4 = !isMobile ? string.Empty : "-mobile";
        object[] args = new object[] { str, str2, str4, linkType, str3 };
        return string.Format("{0}/WTCG/{1}/client{2}/{3}?targetRegion={4}", args);
    }

    private static void GetLocalizedLinkVars(out string baseUrl, out string localeString, out string regionString)
    {
        localeString = Localization.GetLocaleName();
        bool isDev = (ApplicationMgr.GetMobileEnvironment() == MobileEnv.DEVELOPMENT) || ApplicationMgr.IsInternal();
        MobileDeviceLocale.ConnectionData connectionDataFromRegionId = MobileDeviceLocale.GetConnectionDataFromRegionId(MobileDeviceLocale.GetCurrentRegionId(), isDev);
        try
        {
            regionString = TargetServerToRegion[connectionDataFromRegionId.address];
        }
        catch (KeyNotFoundException)
        {
            Debug.LogWarning("No matching region found for " + connectionDataFromRegionId.address + " to get Nydus Link");
            regionString = "US";
        }
        baseUrl = !isDev ? ProdHost : DevHost;
    }

    public static string GetSupportLink(string linkType, bool isMobile)
    {
        string str;
        string str2;
        string str3;
        GetLocalizedLinkVars(out str, out str2, out str3);
        string str4 = !isMobile ? string.Empty : "-mobile";
        object[] args = new object[] { str, str2, str4, linkType, str3 };
        return string.Format("{0}/WTCG/{1}/client{2}/support/{3}?targetRegion={4}", args);
    }
}

