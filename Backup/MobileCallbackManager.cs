using System;
using UnityEngine;

public class MobileCallbackManager : MonoBehaviour
{
    private const string CHINESE_COUNTRY_CODE = "CN";
    private const string CHINESE_CURRENCY_CODE = "CNY";
    public bool m_wasBreakingNewsShown;
    private static MobileCallbackManager s_Instance;

    public static bool AreMotionEffectsEnabled()
    {
        return true;
    }

    private void Awake()
    {
        s_Instance = this;
    }

    public static MobileCallbackManager Get()
    {
        return s_Instance;
    }

    public static uint GetMemoryUsage()
    {
        return Profiler.GetTotalAllocatedMemory();
    }

    public static bool IsAndroidDeviceTabletSized()
    {
        return true;
    }

    private static bool IsDevice(string deviceModel)
    {
        return false;
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }

    private static void RegisterForPushNotifications()
    {
    }

    public static void RegisterPushNotifications()
    {
        Log.Yim.Print("MobileCallbackManager - RegisterPushNotifications()", new object[0]);
        RegisterForPushNotifications();
    }

    private static void SetBattleNetRegionAndGameLocale(int gameRegion, string gameLocale)
    {
    }

    public static void SetRegionAndLocale(int gameRegion, string gameLocale)
    {
        Log.Yim.Print(string.Concat(new object[] { "SetRegionAndLocale(", gameRegion, ", ", gameLocale, ")" }), new object[0]);
        SetBattleNetRegionAndGameLocale(gameRegion, gameLocale);
    }
}

