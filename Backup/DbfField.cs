using System;

public class DbfField
{
    private string m_columnName;
    public uint? m_locID;
    private object m_value;

    public string GetColumnName()
    {
        return this.m_columnName;
    }

    public object GetValue()
    {
        return this.m_value;
    }

    public void SetColumnName(string columnName)
    {
        this.m_columnName = columnName;
    }

    public void SetValue(object val)
    {
        this.m_value = val;
    }

    public override string ToString()
    {
        return string.Format("columnName={0} value={1}", this.m_columnName, this.m_value);
    }
}

