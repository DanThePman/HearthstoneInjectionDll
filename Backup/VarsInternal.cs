using System;

internal class VarsInternal
{
    private Map<string, string> m_vars = new Map<string, string>();
    private static VarsInternal s_instance = new VarsInternal();

    private VarsInternal()
    {
        string clientConfigPath = Vars.GetClientConfigPath();
        if (!this.LoadConfig(clientConfigPath))
        {
        }
    }

    public bool Contains(string key)
    {
        return this.m_vars.ContainsKey(key);
    }

    public static VarsInternal Get()
    {
        return s_instance;
    }

    private bool LoadConfig(string path)
    {
        ConfigFile file = new ConfigFile();
        if (!file.LightLoad(path))
        {
            return false;
        }
        foreach (ConfigFile.Line line in file.GetLines())
        {
            this.m_vars[line.m_fullKey] = line.m_value;
        }
        return true;
    }

    public static void RefreshVars()
    {
        s_instance = new VarsInternal();
    }

    public void Set(string key, string value)
    {
        this.m_vars[key] = value;
    }

    public string Value(string key)
    {
        return this.m_vars[key];
    }
}

