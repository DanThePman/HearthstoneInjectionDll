using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSettings
{
    public static InputCategory s_input;
    public static bool s_isDeviceSupported = true;
    public static MemoryCategory s_memory = MemoryCategory.High;
    public static OSCategory s_os = OSCategory.PC;
    public static ScreenCategory s_screen = ScreenCategory.PC;
    public static ScreenDensityCategory s_screenDensity = ScreenDensityCategory.High;

    static PlatformSettings()
    {
        RecomputeDeviceSettings();
    }

    private static bool EmulateMobileDevice()
    {
        ConfigFile config = new ConfigFile();
        if (!config.FullLoad(Vars.GetClientConfigPath()))
        {
            Blizzard.Log.Warning("Failed to read DeviceEmulation from client.config");
            return false;
        }
        DevicePreset preset = new DevicePreset();
        preset.ReadFromConfig(config);
        if (preset.name == "No Emulation")
        {
            return false;
        }
        if (!config.Get("Emulation.emulateOnDevice", false))
        {
            return false;
        }
        s_os = preset.os;
        s_input = preset.input;
        s_screen = preset.screen;
        s_screenDensity = preset.screenDensity;
        Log.DeviceEmulation.Print("Emulating an " + preset.name, new object[0]);
        return true;
    }

    public static int GetBestScreenMatch(List<ScreenCategory> categories)
    {
        int num = 0;
        int num2 = 4 - Screen;
        for (int i = 1; i < categories.Count; i++)
        {
            int num4 = ((ScreenCategory) categories[i]) - Screen;
            if ((num4 >= 0) && (num4 < num2))
            {
                num = i;
                num2 = num4;
            }
        }
        return num;
    }

    private static void RecomputeDeviceSettings()
    {
        if (!EmulateMobileDevice())
        {
            s_os = OSCategory.PC;
            s_input = InputCategory.Mouse;
            s_screen = ScreenCategory.PC;
            s_screenDensity = ScreenDensityCategory.High;
            s_os = OSCategory.PC;
            int systemMemorySize = SystemInfo.systemMemorySize;
            if (systemMemorySize < 500)
            {
                Debug.LogWarning("Low Memory Warning: Device has only " + systemMemorySize + "MBs of system memory");
                s_memory = MemoryCategory.Low;
            }
            else if (systemMemorySize < 0x3e8)
            {
                s_memory = MemoryCategory.Low;
            }
            else if (systemMemorySize < 0x5dc)
            {
                s_memory = MemoryCategory.Medium;
            }
            else
            {
                s_memory = MemoryCategory.High;
            }
        }
    }

    public static void Refresh()
    {
        RecomputeDeviceSettings();
    }

    private static void SetAndroidSettings()
    {
        s_os = OSCategory.Android;
        s_input = InputCategory.Touch;
    }

    private static void SetIOSSettings()
    {
    }

    public static string DeviceName
    {
        get
        {
            if (string.IsNullOrEmpty(SystemInfo.deviceModel))
            {
                return "unknown";
            }
            return SystemInfo.deviceModel;
        }
    }

    public static InputCategory Input
    {
        get
        {
            return s_input;
        }
    }

    public static MemoryCategory Memory
    {
        get
        {
            return s_memory;
        }
    }

    public static OSCategory OS
    {
        get
        {
            return s_os;
        }
    }

    public static ScreenCategory Screen
    {
        get
        {
            return s_screen;
        }
    }

    public static ScreenDensityCategory ScreenDensity
    {
        get
        {
            return s_screenDensity;
        }
    }
}

