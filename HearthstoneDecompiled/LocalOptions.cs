using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalOptions
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map41;
    private const int LOAD_LINE_FAIL_THRESHOLD = 4;
    private int m_currentLineVersion;
    private bool m_dirty;
    private LineParser[] m_lineParsers;
    private LoadResult m_loadResult;
    private Map<string, object> m_options;
    private string m_path;
    private List<string> m_sortedKeys;
    private const string OPTIONS_FILE_NAME = "options.txt";
    private static LocalOptions s_instance;

    public LocalOptions()
    {
        LineParser[] parserArray1 = new LineParser[2];
        LineParser parser = new LineParser {
            m_pattern = @"(?<key>[^\:]+):(?<type>[bilds])=(?<value>.*)",
            m_callback = new ParseLineCallback(LocalOptions.ParseLine_V1)
        };
        parserArray1[0] = parser;
        parser = new LineParser {
            m_pattern = "(?<key>[^=]+)=(?<value>.*)",
            m_callback = new ParseLineCallback(LocalOptions.ParseLine_V2)
        };
        parserArray1[1] = parser;
        this.m_lineParsers = parserArray1;
        this.m_options = new Map<string, object>();
        this.m_sortedKeys = new List<string>();
    }

    public void Clear()
    {
        this.m_dirty = false;
        this.m_options.Clear();
        this.m_sortedKeys.Clear();
    }

    public void Delete(string key)
    {
        if (this.m_options.Remove(key))
        {
            this.m_sortedKeys.Remove(key);
            this.m_dirty = true;
            this.Save(key);
        }
    }

    public static LocalOptions Get()
    {
        if (s_instance == null)
        {
            s_instance = new LocalOptions();
        }
        return s_instance;
    }

    public T Get<T>(string key)
    {
        object obj2;
        if (!this.m_options.TryGetValue(key, out obj2))
        {
            return default(T);
        }
        return (T) obj2;
    }

    public bool GetBool(string key)
    {
        return this.Get<bool>(key);
    }

    public float GetFloat(string key)
    {
        return this.Get<float>(key);
    }

    public int GetInt(string key)
    {
        return this.Get<int>(key);
    }

    public long GetLong(string key)
    {
        return this.Get<long>(key);
    }

    public string GetString(string key)
    {
        return this.Get<string>(key);
    }

    public ulong GetULong(string key)
    {
        return this.Get<ulong>(key);
    }

    public bool Has(string key)
    {
        return this.m_options.ContainsKey(key);
    }

    public void Initialize()
    {
        this.m_path = string.Format("{0}/{1}", FileUtils.PersistentDataPath, "options.txt");
        foreach (LineParser parser in this.m_lineParsers)
        {
            parser.m_regex = new Regex(parser.m_pattern, RegexOptions.IgnoreCase);
        }
        this.m_currentLineVersion = this.m_lineParsers.Length;
        if (this.Load())
        {
            OptionsMigration.UpgradeClientOptions();
        }
    }

    private int KeyComparison(string key1, string key2)
    {
        return string.Compare(key1, key2, true);
    }

    private bool Load()
    {
        string[] strArray;
        this.Clear();
        if (!System.IO.File.Exists(this.m_path))
        {
            this.m_loadResult = LoadResult.SUCCESS;
            return true;
        }
        if (!this.LoadFile(out strArray))
        {
            this.m_loadResult = LoadResult.FAIL;
            return false;
        }
        bool formatChanged = false;
        if (!this.LoadAllLines(strArray, out formatChanged))
        {
            this.m_loadResult = LoadResult.FAIL;
            return false;
        }
        foreach (string str in this.m_options.Keys)
        {
            this.m_sortedKeys.Add(str);
        }
        this.SortKeys();
        this.m_loadResult = LoadResult.SUCCESS;
        if (formatChanged)
        {
            this.m_dirty = true;
            this.Save();
        }
        return true;
    }

    private bool LoadAllLines(string[] lines, out bool formatChanged)
    {
        formatChanged = false;
        int num = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            line = line.Trim();
            if ((line.Length != 0) && !line.StartsWith("#"))
            {
                int num3;
                string str2;
                object obj2;
                bool flag;
                if (!this.LoadLine(line, out num3, out str2, out obj2, out flag))
                {
                    Debug.LogError(string.Format("LocalOptions.LoadAllLines() - Failed to load line {0}.", i + 1));
                    num++;
                    if (num > 4)
                    {
                        this.m_loadResult = LoadResult.FAIL;
                        return false;
                    }
                }
                else
                {
                    this.m_options[str2] = obj2;
                    formatChanged = (formatChanged || (num3 != this.m_currentLineVersion)) || flag;
                }
            }
        }
        return true;
    }

    private bool LoadFile(out string[] lines)
    {
        try
        {
            lines = System.IO.File.ReadAllLines(this.m_path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("LocalOptions.LoadFile() - Failed to read {0}. Exception={1}", this.m_path, exception.Message));
            lines = null;
            return false;
        }
    }

    private bool LoadLine(string line, out int version, out string key, out object val, out bool formatChanged)
    {
        bool flag;
        version = 0;
        key = null;
        val = null;
        formatChanged = false;
        int num = 0;
        string str = null;
        string str2 = null;
        for (int i = 0; i < this.m_lineParsers.Length; i++)
        {
            LineParser parser = this.m_lineParsers[i];
            if (parser.m_callback(parser.m_regex, line, out str, out str2))
            {
                num = i + 1;
                break;
            }
        }
        if (num != 0)
        {
            int num3;
            Locale locale;
            Option iNVALID = Option.INVALID;
            try
            {
                iNVALID = EnumUtils.GetEnum<Option>(str, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                version = num;
                key = str;
                val = str2;
                return true;
            }
            flag = false;
            if (((iNVALID == Option.LOCALE) && GeneralUtils.TryParseInt(str2, out num3)) && EnumUtils.TryCast<Locale>(num3, out locale))
            {
                str2 = locale.ToString();
                flag = true;
            }
            System.Type type = OptionDataTables.s_typeMap[iNVALID];
            if (type == typeof(bool))
            {
                val = GeneralUtils.ForceBool(str2);
                goto Label_01A8;
            }
            if (type == typeof(int))
            {
                val = GeneralUtils.ForceInt(str2);
                goto Label_01A8;
            }
            if (type == typeof(long))
            {
                val = GeneralUtils.ForceLong(str2);
                goto Label_01A8;
            }
            if (type == typeof(ulong))
            {
                val = GeneralUtils.ForceULong(str2);
                goto Label_01A8;
            }
            if (type == typeof(float))
            {
                val = GeneralUtils.ForceFloat(str2);
                goto Label_01A8;
            }
            if (type == typeof(string))
            {
                val = str2;
                goto Label_01A8;
            }
        }
        return false;
    Label_01A8:
        version = num;
        key = str;
        formatChanged = flag;
        return true;
    }

    private static bool ParseLine_V1(Regex regex, string line, out string key, out string val)
    {
        key = null;
        val = null;
        Match match = regex.Match(line);
        if (match.Success)
        {
            GroupCollection groups = match.Groups;
            key = groups["key"].Value;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            string str = groups["type"].Value;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            string str2 = str;
            if (str2 != null)
            {
                int num;
                if (<>f__switch$map41 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(5);
                    dictionary.Add("b", 0);
                    dictionary.Add("i", 0);
                    dictionary.Add("l", 0);
                    dictionary.Add("d", 0);
                    dictionary.Add("s", 0);
                    <>f__switch$map41 = dictionary;
                }
                if (<>f__switch$map41.TryGetValue(str2, out num) && (num == 0))
                {
                    val = groups["value"].Value;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool ParseLine_V2(Regex regex, string line, out string key, out string val)
    {
        key = null;
        val = null;
        Match match = regex.Match(line);
        if (!match.Success)
        {
            return false;
        }
        GroupCollection groups = match.Groups;
        key = groups["key"].Value;
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        val = groups["value"].Value;
        return true;
    }

    private bool Save()
    {
        if (!this.m_dirty)
        {
            return true;
        }
        List<string> list = new List<string>();
        for (int i = 0; i < this.m_sortedKeys.Count; i++)
        {
            string str = this.m_sortedKeys[i];
            object obj2 = this.m_options[str];
            string item = string.Format("{0}={1}", str, obj2);
            list.Add(item);
        }
        try
        {
            System.IO.File.WriteAllLines(this.m_path, list.ToArray(), new UTF8Encoding());
            this.m_dirty = false;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("LocalOptions.Save() - Failed to save {0}. Exception={1}", this.m_path, exception.Message));
            return false;
        }
    }

    private bool Save(string triggerKey)
    {
        switch (this.m_loadResult)
        {
            case LoadResult.INVALID:
            case LoadResult.FAIL:
                return false;
        }
        return this.Save();
    }

    public void Set(string key, object val)
    {
        object obj2;
        if (this.m_options.TryGetValue(key, out obj2))
        {
            if (obj2 == val)
            {
                return;
            }
            if ((obj2 != null) && obj2.Equals(val))
            {
                return;
            }
        }
        else
        {
            this.m_sortedKeys.Add(key);
            this.SortKeys();
        }
        this.m_options[key] = val;
        this.m_dirty = true;
        this.Save(key);
    }

    private void SortKeys()
    {
        this.m_sortedKeys.Sort(new Comparison<string>(this.KeyComparison));
    }

    private class LineParser
    {
        public LocalOptions.ParseLineCallback m_callback;
        public string m_pattern;
        public Regex m_regex;
    }

    private enum LoadResult
    {
        INVALID,
        SUCCESS,
        FAIL
    }

    private delegate bool ParseLineCallback(Regex regex, string line, out string key, out string val);
}

