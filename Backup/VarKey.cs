using System;

public class VarKey
{
    private string m_key;

    public VarKey(string key)
    {
        this.m_key = key;
    }

    public VarKey(string key, string subKey)
    {
        this.m_key = key + "." + subKey;
    }

    public bool GetBool(bool def)
    {
        if (VarsInternal.Get().Contains(this.m_key))
        {
            return GeneralUtils.ForceBool(VarsInternal.Get().Value(this.m_key));
        }
        return def;
    }

    public float GetFloat(float def)
    {
        if (VarsInternal.Get().Contains(this.m_key))
        {
            return GeneralUtils.ForceFloat(VarsInternal.Get().Value(this.m_key));
        }
        return def;
    }

    public int GetInt(int def)
    {
        if (VarsInternal.Get().Contains(this.m_key))
        {
            return GeneralUtils.ForceInt(VarsInternal.Get().Value(this.m_key));
        }
        return def;
    }

    public string GetStr(string def)
    {
        if (VarsInternal.Get().Contains(this.m_key))
        {
            return VarsInternal.Get().Value(this.m_key);
        }
        return def;
    }

    public VarKey Key(string subKey)
    {
        return new VarKey(this.m_key, subKey);
    }
}

