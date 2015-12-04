using System;
using System.Runtime.CompilerServices;

public static class OptionsMigration
{
    public const int LATEST_CLIENT_VERSION = 2;
    public const int LATEST_SERVER_VERSION = 2;
    private static readonly Map<int, UpgradeCallback> s_clientUpgradeCallbacks = new Map<int, UpgradeCallback>();
    private static readonly Map<int, UpgradeCallback> s_serverUpgradeCallbacks = new Map<int, UpgradeCallback>();

    public static bool UpgradeClientOptions()
    {
        int @int = Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION);
        if (!Options.Get().HasOption(Option.CLIENT_OPTIONS_VERSION))
        {
            if (!UpgradeClientOptions_V2())
            {
                return false;
            }
            @int = 2;
        }
        while (@int < 2)
        {
            UpgradeCallback callback;
            if (!s_clientUpgradeCallbacks.TryGetValue(@int, out callback))
            {
                object[] messageArgs = new object[] { @int, @int + 1, 2 };
                Error.AddDevFatal("OptionsMigration.UpgradeClientOptions() - Current version is {0} and there is no function to upgrade to {1}. Latest is {2}.", messageArgs);
                return false;
            }
            if (!callback())
            {
                return false;
            }
            @int++;
        }
        return true;
    }

    private static bool UpgradeClientOptions_V2()
    {
        Options.Get().SetInt(Option.CLIENT_OPTIONS_VERSION, 2);
        return (Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION) == 2);
    }

    public static bool UpgradeServerOptions()
    {
        int @int = Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION);
        if (!Options.Get().HasOption(Option.SERVER_OPTIONS_VERSION))
        {
            if (!UpgradeServerOptions_V2())
            {
                return false;
            }
            @int = 2;
        }
        while (@int < 2)
        {
            UpgradeCallback callback;
            if (!s_serverUpgradeCallbacks.TryGetValue(@int, out callback))
            {
                object[] messageArgs = new object[] { @int, @int + 1, 2 };
                Error.AddDevFatal("OptionsMigration.UpgradeServerOptions() - Current version is {0} and there is no function to upgrade to {1}. Latest is {2}.", messageArgs);
                return false;
            }
            if (!callback())
            {
                return false;
            }
            @int++;
        }
        return true;
    }

    private static bool UpgradeServerOptions_V2()
    {
        Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 2);
        return (Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 2);
    }

    private delegate bool UpgradeCallback();
}

