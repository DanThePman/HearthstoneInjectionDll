using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Dbf
{
    private List<DbfColumn> m_columns;
    private string m_name;
    private RecordAddedListener m_recordAddedListener;
    private List<DbfRecord> m_records;

    public Dbf()
    {
        this.m_columns = new List<DbfColumn>();
        this.m_records = new List<DbfRecord>();
    }

    public Dbf(RecordAddedListener listener)
    {
        this.m_columns = new List<DbfColumn>();
        this.m_records = new List<DbfRecord>();
        this.m_recordAddedListener = listener;
    }

    public void AddColumn(DbfColumn column)
    {
        this.m_columns.Add(column);
    }

    public void AddRecord(DbfRecord record)
    {
        if (this.m_recordAddedListener != null)
        {
            this.m_recordAddedListener(record);
        }
        this.m_records.Add(record);
    }

    public void Clear()
    {
        this.m_columns.Clear();
        this.m_records.Clear();
    }

    public void ClearRecords()
    {
        this.m_records.Clear();
    }

    private string GetAssetPath()
    {
        return FileUtils.GetAssetPath(string.Format("DBF/{0}.xml", this.m_name));
    }

    public DbfColumn GetColumn(string columnName)
    {
        if (columnName != null)
        {
            for (int i = 0; i < this.m_columns.Count; i++)
            {
                DbfColumn column = this.m_columns[i];
                if (columnName.Equals(column.GetName(), StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }
        }
        return null;
    }

    public List<DbfColumn> GetColumns()
    {
        return this.m_columns;
    }

    public DbfDataType GetDataType(string columnName)
    {
        DbfColumn column = this.GetColumn(columnName);
        if (column == null)
        {
            return DbfDataType.INVALID;
        }
        return column.GetDataType();
    }

    public string GetName()
    {
        return this.m_name;
    }

    public DbfRecord GetRecord(int id)
    {
        for (int i = 0; i < this.m_records.Count; i++)
        {
            DbfRecord record = this.m_records[i];
            if (record.GetId() == id)
            {
                return record;
            }
        }
        return null;
    }

    public DbfRecord GetRecord(string column, object comparedata)
    {
        for (int i = 0; i < this.m_records.Count; i++)
        {
            DbfRecord record = this.m_records[i];
            object obj2 = record.GetField(column).GetValue();
            if ((obj2 != null) && obj2.Equals(comparedata))
            {
                return record;
            }
        }
        return null;
    }

    public List<DbfRecord> GetRecords()
    {
        return this.m_records;
    }

    public List<DbfRecord> GetRecords(IEnumerable<int> ids)
    {
        if (ids == null)
        {
            return null;
        }
        List<DbfRecord> list = new List<DbfRecord>();
        for (int i = 0; i < this.m_records.Count; i++)
        {
            DbfRecord item = this.m_records[i];
            IEnumerator<int> enumerator = ids.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == item.GetId())
                    {
                        list.Add(item);
                        continue;
                    }
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }
        return list;
    }

    public bool HasColumn(string columnName)
    {
        return (this.GetColumn(columnName) != null);
    }

    public bool HasRecord(int id)
    {
        return (this.GetRecord(id) != null);
    }

    public bool Load()
    {
        string assetPath = this.GetAssetPath();
        return this.Load(assetPath, false);
    }

    public static Dbf Load(string name, RecordAddedListener listener = null)
    {
        Dbf dbf = new Dbf(listener);
        dbf.SetName(name);
        if (!dbf.Load())
        {
            Debug.LogError(string.Format("Dbf.Load() - failed to load {0}", name));
        }
        return dbf;
    }

    public bool Load(string path, bool editor)
    {
        this.Clear();
        if (!DbfXml.Load(this, path, editor))
        {
            this.Clear();
            return false;
        }
        return true;
    }

    public static Dbf Load(string name, string xml, RecordAddedListener listener = null)
    {
        Dbf dbf = new Dbf(listener);
        dbf.SetName(name);
        dbf.Clear();
        if (!DbfXml.Load(dbf, xml))
        {
            dbf.Clear();
            Debug.LogError(string.Format("Dbf.Load() - failed to load {0}", name));
        }
        return dbf;
    }

    public static Dbf Load(string name, string path, bool editor)
    {
        Dbf dbf = new Dbf();
        dbf.SetName(name);
        dbf.Clear();
        if (!DbfXml.Load(dbf, path, editor))
        {
            dbf.Clear();
            Debug.LogError(string.Format("Dbf.Load() - failed to load {0}", name));
        }
        return dbf;
    }

    public void RelaceRecord(DbfRecord record)
    {
        if (record != null)
        {
            int id = record.GetId();
            for (int i = 0; i < this.m_records.Count; i++)
            {
                DbfRecord record2 = this.m_records[i];
                if (record2.GetId() == id)
                {
                    this.m_records[i] = record;
                    return;
                }
            }
            this.AddRecord(record);
        }
    }

    public bool Save()
    {
        string assetPath = this.GetAssetPath();
        return this.Save(assetPath);
    }

    public bool Save(string path)
    {
        return DbfXml.Save(this, path);
    }

    public void SetName(string name)
    {
        this.m_name = name;
    }

    public override string ToString()
    {
        if (this.m_name == null)
        {
            return "UNKNOWN";
        }
        return this.m_name;
    }

    public delegate void RecordAddedListener(DbfRecord record);
}

