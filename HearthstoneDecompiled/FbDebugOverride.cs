using System;

public static class FbDebugOverride
{
    private static bool allowLogging = true;

    public static void Error(string msg)
    {
        if (allowLogging)
        {
            FbDebug.Error(msg);
        }
    }

    public static void Info(string msg)
    {
        if (allowLogging)
        {
            FbDebug.Info(msg);
        }
    }

    public static void Log(string msg)
    {
        if (allowLogging)
        {
            FbDebug.Log(msg);
        }
    }

    public static void Warn(string msg)
    {
        if (allowLogging)
        {
            FbDebug.Warn(msg);
        }
    }
}

