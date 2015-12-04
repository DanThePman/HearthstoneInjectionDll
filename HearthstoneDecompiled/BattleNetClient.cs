using System;
using System.Diagnostics;
using System.IO;

public class BattleNetClient
{
    public static void quitHearthstoneAndRun()
    {
        Blizzard.Log.Warning("Hearthstone was not run from Battle.net Client");
        if (!bootstrapper.Exists)
        {
            Blizzard.Log.Warning("Hearthstone could not find Battle.net client");
            Error.AddFatalLoc("GLUE_CANNOT_FIND_BATTLENET_CLIENT", new object[0]);
        }
        else
        {
            try
            {
                Process process2 = new Process {
                    StartInfo = { UseShellExecute = false, FileName = bootstrapper.FullName, Arguments = "-uid hs_beta" },
                    EnableRaisingEvents = true
                };
                process2.Start();
                Blizzard.Log.Warning("Hearthstone ran Battle.net Client.  Exiting.");
                ApplicationMgr.Get().Exit();
            }
            catch (Exception exception)
            {
                Error.AddFatalLoc("GLUE_CANNOT_RUN_BATTLENET_CLIENT", new object[0]);
                object[] args = new object[] { exception.Message };
                Blizzard.Log.Warning("Hearthstone could not launch Battle.net client: {0}", args);
            }
        }
    }

    private static FileInfo bootstrapper
    {
        get
        {
            return new FileInfo("Hearthstone Beta Launcher.exe");
        }
    }

    private static bool launchedHearthstone
    {
        get
        {
            foreach (string str in Environment.GetCommandLineArgs())
            {
                if (str.Equals("-launch", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool needsToRun
    {
        get
        {
            return (usedOnThisPlatform && !launchedHearthstone);
        }
    }

    private static bool usedOnThisPlatform
    {
        get
        {
            return true;
        }
    }
}

