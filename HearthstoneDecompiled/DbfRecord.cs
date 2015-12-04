using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class DbfRecord
{
    private int m_cachedId = -1;
    private Map<string, DbfField> m_fieldMap = new Map<string, DbfField>();
    private List<DbfField> m_fields = new List<DbfField>();

    public void AddField(DbfField field)
    {
        this.m_fields.Add(field);
        this.m_fieldMap.Add(DbfUtils.MakeFieldKey(field), field);
    }

    public string GetAssetName(string column)
    {
        string str;
        this.TryGetAssetName(column, out str);
        return str;
    }

    public string GetAssetPath(string column)
    {
        return this.GetString(column);
    }

    public bool GetBool(string column)
    {
        return this.GetValue<bool>(column);
    }

    public DbfField GetField(string column)
    {
        DbfField field = null;
        this.TryGetField(column, out field);
        return field;
    }

    public int GetFieldCount()
    {
        return this.m_fields.Count;
    }

    public Map<string, DbfField> GetFieldMap()
    {
        return this.m_fieldMap;
    }

    public List<DbfField> GetFields()
    {
        return this.m_fields;
    }

    public float GetFloat(string column)
    {
        return this.GetValue<float>(column);
    }

    public int GetId()
    {
        DbfField field;
        if (this.m_cachedId >= 0)
        {
            return this.m_cachedId;
        }
        if (!this.m_fieldMap.TryGetValue("ID", out field))
        {
            return 0;
        }
        int num = Convert.ToInt32(field.GetValue());
        this.m_cachedId = num;
        return num;
    }

    public int GetInt(string column)
    {
        return this.GetValue<int>(column);
    }

    public string GetLocString(string column)
    {
        string str;
        if (this.TryGetLocValue<string>(column, out str))
        {
            return str;
        }
        return string.Format("ID={0} COLUMN={1}", this.GetId(), column);
    }

    public string GetLocString(Locale locale, string column)
    {
        string str;
        if (this.TryGetLocValue<string>(locale, column, out str))
        {
            return str;
        }
        return string.Format("ID={0} COLUMN={1}", this.GetId(), column);
    }

    public T GetLocValue<T>(string column)
    {
        T local;
        this.TryGetLocValue<T>(column, out local);
        return local;
    }

    public T GetLocValue<T>(Locale locale, string column)
    {
        T local;
        this.TryGetLocValue<T>(locale, column, out local);
        return local;
    }

    public long GetLong(string column)
    {
        return this.GetValue<long>(column);
    }

    public string GetString(string column)
    {
        return this.GetValue<string>(column);
    }

    public ulong GetULong(string column)
    {
        return this.GetValue<ulong>(column);
    }

    public T GetValue<T>(string column)
    {
        T local;
        this.TryGetValue<T>(column, out local);
        return local;
    }

    public bool HasField(string column)
    {
        if (column == null)
        {
            return false;
        }
        return this.m_fieldMap.ContainsKey(column);
    }

    public override string ToString()
    {
        return string.Format("id={0} fields={1}", this.GetId(), this.m_fields.Count);
    }

    public bool TryGetAssetName(string column, out string val)
    {
        if (!this.TryGetString(column, out val))
        {
            return false;
        }
        val = FileUtils.GameAssetPathToName(val);
        return true;
    }

    public bool TryGetAssetPath(string column, out string val)
    {
        return this.TryGetString(column, out val);
    }

    public bool TryGetBool(string column, out bool val)
    {
        return this.TryGetValue<bool>(column, out val);
    }

    public bool TryGetField(string column, out DbfField field)
    {
        field = null;
        if (column == null)
        {
            return false;
        }
        return this.m_fieldMap.TryGetValue(column, out field);
    }

    public bool TryGetFloat(string column, out float val)
    {
        return this.TryGetValue<float>(column, out val);
    }

    public bool TryGetInt(string column, out int val)
    {
        return this.TryGetValue<int>(column, out val);
    }

    public bool TryGetLocString(string column, out string val)
    {
        return this.TryGetLocValue<string>(column, out val);
    }

    public bool TryGetLocString(Locale locale, string column, out string val)
    {
        return this.TryGetLocValue<string>(locale, column, out val);
    }

    public bool TryGetLocValue<T>(string column, out T val)
    {
        DbfField field;
        val = default(T);
        if (this.TryGetField(column, out field))
        {
            List<DbfLocValue> list = (List<DbfLocValue>) field.GetValue();
            foreach (Locale locale in Localization.GetLoadOrder(false))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DbfLocValue value2 = list[i];
                    if (value2.GetLocale() == locale)
                    {
                        val = (T) value2.GetValue();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool TryGetLocValue<T>(Locale locale, string column, out T val)
    {
        DbfField field;
        val = default(T);
        if (this.TryGetField(column, out field))
        {
            List<DbfLocValue> list = (List<DbfLocValue>) field.GetValue();
            for (int i = 0; i < list.Count; i++)
            {
                DbfLocValue value2 = list[i];
                if (value2.GetLocale() == locale)
                {
                    val = (T) value2.GetValue();
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryGetLong(string column, out long val)
    {
        return this.TryGetValue<long>(column, out val);
    }

    public bool TryGetString(string column, out string val)
    {
        return this.TryGetValue<string>(column, out val);
    }

    public bool TryGetULong(string column, out ulong val)
    {
        return this.TryGetValue<ulong>(column, out val);
    }

    public bool TryGetValue<T>(string column, out T val)
    {
        DbfField field;
        val = default(T);
        if (!this.TryGetField(column, out field))
        {
            return false;
        }
        object obj2 = field.GetValue();
        if ((obj2 != null) && (!(obj2 is string) || !string.IsNullOrEmpty((string) obj2)))
        {
            val = (T) field.GetValue();
        }
        return true;
    }
}

