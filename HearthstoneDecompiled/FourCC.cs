using System;
using System.Text;

[Serializable]
public class FourCC
{
    protected uint m_value;

    public FourCC()
    {
    }

    public FourCC(string stringVal)
    {
        this.SetString(stringVal);
    }

    public FourCC(uint value)
    {
        this.m_value = value;
    }

    public FourCC Clone()
    {
        FourCC rcc = new FourCC();
        rcc.CopyFrom(this);
        return rcc;
    }

    public void CopyFrom(FourCC other)
    {
        this.m_value = other.m_value;
    }

    public bool Equals(FourCC other)
    {
        if (other == null)
        {
            return false;
        }
        return (this.m_value == other.m_value);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        FourCC rcc = obj as FourCC;
        if (rcc == null)
        {
            return false;
        }
        return (this.m_value == rcc.m_value);
    }

    public override int GetHashCode()
    {
        return this.m_value.GetHashCode();
    }

    public string GetString()
    {
        StringBuilder builder = new StringBuilder(4);
        for (int i = 0x18; i >= 0; i -= 8)
        {
            char ch = (char) ((this.m_value >> i) & 0xff);
            if (ch != '\0')
            {
                builder.Append(ch);
            }
        }
        return builder.ToString();
    }

    public uint GetValue()
    {
        return this.m_value;
    }

    public static bool operator ==(FourCC a, FourCC b)
    {
        return (object.ReferenceEquals(a, b) || (((a != null) && (b != null)) && (a.m_value == b.m_value)));
    }

    public static bool operator ==(FourCC fourCC, uint val)
    {
        if (fourCC == null)
        {
            return false;
        }
        return (fourCC.m_value == val);
    }

    public static bool operator ==(uint val, FourCC fourCC)
    {
        if (fourCC == null)
        {
            return false;
        }
        return (val == fourCC.m_value);
    }

    public static implicit operator FourCC(uint val)
    {
        return new FourCC(val);
    }

    public static bool operator !=(FourCC a, FourCC b)
    {
        return !(a == b);
    }

    public static bool operator !=(FourCC fourCC, uint val)
    {
        return (fourCC != val);
    }

    public static bool operator !=(uint val, FourCC fourCC)
    {
        return (val != fourCC);
    }

    public void SetString(string str)
    {
        this.m_value = 0;
        for (int i = 0; (i < str.Length) && (i < 4); i++)
        {
            this.m_value = (this.m_value << 8) | ((byte) str[i]);
        }
    }

    public void SetValue(uint val)
    {
        this.m_value = val;
    }

    public override string ToString()
    {
        return this.GetString();
    }
}

