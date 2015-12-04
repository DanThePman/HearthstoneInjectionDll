using System;

public class Vars
{
    public const string CONFIG_FILE_NAME = "client.config";

    public static string GetClientConfigPath()
    {
        return "client.config";
    }

    public static VarKey Key(string key)
    {
        return new VarKey(key);
    }

    public static void RefreshVars()
    {
        VarsInternal.RefreshVars();
    }
}

