using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameStringTable
{
    public const string KEY_FIELD_NAME = "TAG";
    private GameStringCategory m_category;
    private Map<string, string> m_table = new Map<string, string>();
    public const string VALUE_FIELD_NAME = "TEXT";

    private void BuildTable(string path, List<Entry> entries)
    {
        int count = entries.Count;
        this.m_table = new Map<string, string>(count);
        if (count != 0)
        {
            if (ApplicationMgr.IsInternal())
            {
                CheckConflicts(path, entries);
            }
            foreach (Entry entry in entries)
            {
                this.m_table[entry.m_key] = entry.m_value;
            }
        }
    }

    private void BuildTable(string path, List<Entry> entries, string audioPath, List<Entry> audioEntries)
    {
        int count = entries.Count + audioEntries.Count;
        this.m_table = new Map<string, string>(count);
        if (count != 0)
        {
            if (ApplicationMgr.IsInternal())
            {
                CheckConflicts(path, entries);
            }
            foreach (Entry entry in entries)
            {
                this.m_table[entry.m_key] = entry.m_value;
            }
            foreach (Entry entry2 in audioEntries)
            {
                this.m_table[entry2.m_key] = entry2.m_value;
            }
        }
    }

    private static void CheckConflicts(string path, List<Entry> entries)
    {
        if (entries.Count != 0)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                string key = entries[i].m_key;
                for (int j = i + 1; j < entries.Count; j++)
                {
                    string b = entries[j].m_key;
                    if (string.Equals(key, b, StringComparison.OrdinalIgnoreCase))
                    {
                        string message = string.Format("GameStringTable.CheckConflicts() - Tag {0} appears more than once in {1}. All tags must be unique.", key, path);
                        Error.AddDevWarning("GameStrings Error", message, new object[0]);
                    }
                }
            }
        }
    }

    private static void CheckConflicts(string path1, List<Entry> entries1, string path2, List<Entry> entries2)
    {
        if (entries1.Count != 0)
        {
            CheckConflicts(path1, entries1);
            if (entries2.Count != 0)
            {
                CheckConflicts(path2, entries2);
                for (int i = 0; i < entries1.Count; i++)
                {
                    string key = entries1[i].m_key;
                    for (int j = 0; j < entries2.Count; j++)
                    {
                        string b = entries2[j].m_key;
                        if (string.Equals(key, b, StringComparison.OrdinalIgnoreCase))
                        {
                            string message = string.Format("GameStringTable.CheckConflicts() - Tag {0} is used in {1} and {2}. All tags must be unique.", key, path1, path2);
                            Error.AddDevWarning("GameStrings Error", message, new object[0]);
                        }
                    }
                }
            }
        }
    }

    public string Get(string key)
    {
        string str;
        this.m_table.TryGetValue(key, out str);
        return str;
    }

    public Map<string, string> GetAll()
    {
        return this.m_table;
    }

    private static string GetAudioFilePathFromCategory(GameStringCategory cat, Locale locale)
    {
        string fileName = string.Format("{0}_AUDIO.txt", cat);
        return GameStrings.GetAssetPath(locale, fileName);
    }

    public GameStringCategory GetCategory()
    {
        return this.m_category;
    }

    private static string GetFilePathFromCategory(GameStringCategory cat, Locale locale)
    {
        string fileName = string.Format("{0}.txt", cat);
        return GameStrings.GetAssetPath(locale, fileName);
    }

    private static string GetFilePathWithLoadOrder(GameStringCategory cat, FilePathFromCategoryCallback pathCallback)
    {
        Locale[] loadOrder = Localization.GetLoadOrder(false);
        for (int i = 0; i < loadOrder.Length; i++)
        {
            string path = pathCallback(cat, loadOrder[i]);
            if (System.IO.File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    public bool Load(GameStringCategory cat)
    {
        string filePathWithLoadOrder = GetFilePathWithLoadOrder(cat, new FilePathFromCategoryCallback(GameStringTable.GetFilePathFromCategory));
        string audioPath = GetFilePathWithLoadOrder(cat, new FilePathFromCategoryCallback(GameStringTable.GetAudioFilePathFromCategory));
        return this.Load(cat, filePathWithLoadOrder, audioPath);
    }

    public bool Load(GameStringCategory cat, Locale locale)
    {
        string filePathFromCategory = GetFilePathFromCategory(cat, locale);
        string audioFilePathFromCategory = GetAudioFilePathFromCategory(cat, locale);
        return this.Load(cat, filePathFromCategory, audioFilePathFromCategory);
    }

    public bool Load(GameStringCategory cat, string path, string audioPath)
    {
        Header header;
        Header header2;
        this.m_category = GameStringCategory.INVALID;
        this.m_table.Clear();
        List<Entry> entries = null;
        List<Entry> list2 = null;
        if (System.IO.File.Exists(path) && !LoadFile(path, out header, out entries))
        {
            object[] messageArgs = new object[] { path, cat };
            Error.AddDevWarning("GameStrings Error", "GameStringTable.Load() - Failed to load {0} for cat {1}.", messageArgs);
            return false;
        }
        if (System.IO.File.Exists(audioPath) && !LoadFile(audioPath, out header2, out list2))
        {
            object[] objArray2 = new object[] { audioPath, cat };
            Error.AddDevWarning("GameStrings Error", "GameStringTable.Load() - Failed to load {0} for cat {1}.", objArray2);
            return false;
        }
        if ((entries != null) && (list2 != null))
        {
            this.BuildTable(path, entries, audioPath, list2);
        }
        else if (entries != null)
        {
            this.BuildTable(path, entries);
        }
        else if (list2 != null)
        {
            this.BuildTable(audioPath, list2);
        }
        else
        {
            object[] objArray3 = new object[] { cat };
            Error.AddDevWarning("GameStrings Error", "GameStringTable.Load() - There are no entries for cat {0}.", objArray3);
            return false;
        }
        this.m_category = cat;
        return true;
    }

    public static bool LoadFile(string path, out Header header, out List<Entry> entries)
    {
        header = null;
        entries = null;
        string[] lines = null;
        try
        {
            lines = System.IO.File.ReadAllLines(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(string.Format("GameStringTable.LoadFile() - Failed to read \"{0}\".\n\nException: {1}", path, exception.Message));
            return false;
        }
        header = LoadFileHeader(lines);
        if (header == null)
        {
            Debug.LogWarning(string.Format("GameStringTable.LoadFile() - \"{0}\" had a malformed header.", path));
            return false;
        }
        entries = LoadFileEntries(path, header, lines);
        return true;
    }

    private static List<Entry> LoadFileEntries(string path, Header header, string[] lines)
    {
        List<Entry> list = new List<Entry>(lines.Length);
        int num = Mathf.Max(header.m_keyIndex, header.m_valueIndex);
        for (int i = header.m_entryStartIndex; i < lines.Length; i++)
        {
            string str = lines[i];
            if ((str.Length != 0) && !str.StartsWith("#"))
            {
                char[] separator = new char[] { '\t' };
                string[] strArray = str.Split(separator);
                if (strArray.Length <= num)
                {
                    object[] messageArgs = new object[] { i + 1, path };
                    Error.AddDevWarning("GameStrings Error", "GameStringTable.LoadFileEntries() - line {0} in \"{1}\" is malformed", messageArgs);
                }
                else
                {
                    Entry item = new Entry {
                        m_key = strArray[header.m_keyIndex],
                        m_value = TextUtils.DecodeWhitespaces(strArray[header.m_valueIndex])
                    };
                    list.Add(item);
                }
            }
        }
        return list;
    }

    private static Header LoadFileHeader(string[] lines)
    {
        Header header = new Header();
        for (int i = 0; i < lines.Length; i++)
        {
            string str = lines[i];
            if ((str.Length == 0) || str.StartsWith("#"))
            {
                continue;
            }
            char[] separator = new char[] { '\t' };
            string[] strArray = str.Split(separator);
            for (int j = 0; j < strArray.Length; j++)
            {
                switch (strArray[j])
                {
                    case "TAG":
                        header.m_keyIndex = j;
                        if (header.m_valueIndex < 0)
                        {
                            break;
                        }
                        goto Label_00BF;

                    case "TEXT":
                        header.m_valueIndex = j;
                        if (header.m_keyIndex >= 0)
                        {
                            goto Label_00BF;
                        }
                        break;
                }
            }
        Label_00BF:
            if ((header.m_keyIndex < 0) && (header.m_valueIndex < 0))
            {
                return null;
            }
            header.m_entryStartIndex = i + 1;
            return header;
        }
        return null;
    }

    public class Entry
    {
        public string m_key;
        public string m_value;
    }

    private delegate string FilePathFromCategoryCallback(GameStringCategory cat, Locale locale);

    public class Header
    {
        public int m_entryStartIndex = -1;
        public int m_keyIndex = -1;
        public int m_valueIndex = -1;
    }
}

