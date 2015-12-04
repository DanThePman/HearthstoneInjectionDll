using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class ConfigFile
{
    private List<Line> m_lines = new List<Line>();
    private string m_path;

    public void Clear()
    {
        this.m_lines.Clear();
    }

    public bool Delete(string key, bool removeEmptySections = true)
    {
        int index = this.FindEntryIndex(key);
        if (index < 0)
        {
            return false;
        }
        this.m_lines.RemoveAt(index);
        if (removeEmptySections)
        {
            int num2 = index - 1;
            while (num2 >= 0)
            {
                Line line = this.m_lines[num2];
                if (line.m_type == LineType.SECTION)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(line.m_raw.Trim()))
                {
                    return true;
                }
                num2--;
            }
            int num3 = index;
            while (num3 < this.m_lines.Count)
            {
                Line line2 = this.m_lines[num3];
                if (line2.m_type == LineType.SECTION)
                {
                    break;
                }
                if (!string.IsNullOrEmpty(line2.m_raw.Trim()))
                {
                    return true;
                }
                num3++;
            }
            int count = num3 - num2;
            this.m_lines.RemoveRange(num2, count);
        }
        return true;
    }

    private Line FindEntry(string fullKey)
    {
        int num = this.FindEntryIndex(fullKey);
        if (num < 0)
        {
            return null;
        }
        return this.m_lines[num];
    }

    private int FindEntryIndex(string fullKey)
    {
        for (int i = 0; i < this.m_lines.Count; i++)
        {
            Line line = this.m_lines[i];
            if ((line.m_type == LineType.ENTRY) && line.m_fullKey.Equals(fullKey, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private int FindSectionIndex(string sectionName)
    {
        for (int i = 0; i < this.m_lines.Count; i++)
        {
            Line line = this.m_lines[i];
            if ((line.m_type == LineType.SECTION) && line.m_sectionName.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    public bool FullLoad(string path)
    {
        return this.Load(path, false);
    }

    public string GenerateText()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < this.m_lines.Count; i++)
        {
            Line line = this.m_lines[i];
            LineType type = line.m_type;
            if (type != LineType.SECTION)
            {
                if (type == LineType.ENTRY)
                {
                    goto Label_004B;
                }
                goto Label_0090;
            }
            builder.AppendFormat("[{0}]", line.m_sectionName);
            goto Label_00A2;
        Label_004B:
            if (line.m_quoteValue)
            {
                builder.AppendFormat("{0} = \"{1}\"", line.m_lineKey, line.m_value);
            }
            else
            {
                builder.AppendFormat("{0} = {1}", line.m_lineKey, line.m_value);
            }
            goto Label_00A2;
        Label_0090:
            builder.Append(line.m_raw);
        Label_00A2:
            builder.AppendLine();
        }
        return builder.ToString();
    }

    public bool Get(string key, bool defaultVal = false)
    {
        Line line = this.FindEntry(key);
        if (line == null)
        {
            return defaultVal;
        }
        return GeneralUtils.ForceBool(line.m_value);
    }

    public int Get(string key, int defaultVal = 0)
    {
        Line line = this.FindEntry(key);
        if (line == null)
        {
            return defaultVal;
        }
        return GeneralUtils.ForceInt(line.m_value);
    }

    public float Get(string key, float defaultVal = 0f)
    {
        Line line = this.FindEntry(key);
        if (line == null)
        {
            return defaultVal;
        }
        return GeneralUtils.ForceFloat(line.m_value);
    }

    public string Get(string key, string defaultVal = "")
    {
        Line line = this.FindEntry(key);
        if (line == null)
        {
            return defaultVal;
        }
        return line.m_value;
    }

    public List<Line> GetLines()
    {
        return this.m_lines;
    }

    public string GetPath()
    {
        return this.m_path;
    }

    public bool Has(string key)
    {
        return (this.FindEntry(key) != null);
    }

    public bool LightLoad(string path)
    {
        return this.Load(path, true);
    }

    private bool Load(string path, bool ignoreUselessLines)
    {
        this.m_path = null;
        this.m_lines.Clear();
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("Error loading config file " + path);
            return false;
        }
        int num = 1;
        using (StreamReader reader = System.IO.File.OpenText(path))
        {
            string str = string.Empty;
            while (reader.Peek() != -1)
            {
                string str2 = reader.ReadLine();
                string str3 = str2.Trim();
                if (!ignoreUselessLines || (str3.Length > 0))
                {
                    bool flag = (str3.Length > 0) && (str3[0] == ';');
                    if (!ignoreUselessLines || !flag)
                    {
                        Line item = new Line {
                            m_raw = str2,
                            m_sectionName = str
                        };
                        if (flag)
                        {
                            item.m_type = LineType.COMMENT;
                        }
                        else if (str3.Length > 0)
                        {
                            if (str3[0] == '[')
                            {
                                if ((str3.Length < 2) || (str3[str3.Length - 1] != ']'))
                                {
                                    Debug.LogWarning(string.Format("ConfigFile.Load() - invalid section \"{0}\" on line {1} in file {2}", str2, num, path));
                                    if (!ignoreUselessLines)
                                    {
                                        this.m_lines.Add(item);
                                    }
                                }
                                else
                                {
                                    item.m_type = LineType.SECTION;
                                    item.m_sectionName = str = str3.Substring(1, str3.Length - 2);
                                    this.m_lines.Add(item);
                                }
                                continue;
                            }
                            int index = str3.IndexOf('=');
                            if (index < 0)
                            {
                                Debug.LogWarning(string.Format("ConfigFile.Load() - invalid entry \"{0}\" on line {1} in file {2}", str2, num, path));
                                if (!ignoreUselessLines)
                                {
                                    this.m_lines.Add(item);
                                }
                                continue;
                            }
                            string str4 = str3.Substring(0, index).Trim();
                            string str5 = str3.Substring(index + 1, (str3.Length - index) - 1).Trim();
                            if (str5.Length > 2)
                            {
                                int num3 = str5.Length - 1;
                                if ((((str5[0] == '"') || (str5[0] == '“')) || (str5[0] == '”')) && (((str5[num3] == '"') || (str5[num3] == '“')) || (str5[num3] == '”')))
                                {
                                    str5 = str5.Substring(1, str5.Length - 2);
                                    item.m_quoteValue = true;
                                }
                            }
                            item.m_type = LineType.ENTRY;
                            item.m_fullKey = !string.IsNullOrEmpty(str) ? string.Format("{0}.{1}", str, str4) : str4;
                            item.m_lineKey = str4;
                            item.m_value = str5;
                        }
                        this.m_lines.Add(item);
                    }
                }
            }
        }
        this.m_path = path;
        return true;
    }

    private Line RegisterEntry(string fullKey)
    {
        if (string.IsNullOrEmpty(fullKey))
        {
            return null;
        }
        int index = fullKey.IndexOf('.');
        if (index < 0)
        {
            return null;
        }
        string sectionName = fullKey.Substring(0, index);
        string str2 = string.Empty;
        if (fullKey.Length > (index + 1))
        {
            str2 = fullKey.Substring(index + 1, (fullKey.Length - index) - 1);
        }
        Line item = null;
        int num2 = this.FindSectionIndex(sectionName);
        if (num2 < 0)
        {
            Line line2 = new Line();
            if (this.m_lines.Count > 0)
            {
                line2.m_sectionName = this.m_lines[this.m_lines.Count - 1].m_sectionName;
            }
            this.m_lines.Add(line2);
            Line line3 = new Line {
                m_type = LineType.SECTION,
                m_sectionName = sectionName
            };
            this.m_lines.Add(line3);
            item = new Line {
                m_type = LineType.ENTRY,
                m_sectionName = sectionName,
                m_lineKey = str2,
                m_fullKey = fullKey
            };
            this.m_lines.Add(item);
            return item;
        }
        int num3 = num2 + 1;
        while (num3 < this.m_lines.Count)
        {
            Line line4 = this.m_lines[num3];
            if (line4.m_type == LineType.SECTION)
            {
                break;
            }
            if ((line4.m_type == LineType.ENTRY) && line4.m_lineKey.Equals(str2, StringComparison.OrdinalIgnoreCase))
            {
                item = line4;
                break;
            }
            num3++;
        }
        if (item == null)
        {
            item = new Line {
                m_type = LineType.ENTRY,
                m_sectionName = sectionName,
                m_lineKey = str2,
                m_fullKey = fullKey
            };
            this.m_lines.Insert(num3, item);
        }
        return item;
    }

    public bool Save(string path = null)
    {
        if (path == null)
        {
            path = this.m_path;
        }
        if (path == null)
        {
            Debug.LogError("ConfigFile.Save() - no path given");
            return false;
        }
        string contents = this.GenerateText();
        try
        {
            FileUtils.SetFileWritableFlag(path, true);
            System.IO.File.WriteAllText(path, contents);
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("ConfigFile.Save() - Failed to write file at {0}. Exception={1}", path, exception.Message));
            return false;
        }
        this.m_path = path;
        return true;
    }

    public bool Set(string key, bool val)
    {
        string str = !val ? "false" : "true";
        return this.Set(key, str);
    }

    public bool Set(string key, object val)
    {
        string str = (val != null) ? val.ToString() : string.Empty;
        return this.Set(key, str);
    }

    public bool Set(string key, string val)
    {
        Line line = this.RegisterEntry(key);
        if (line == null)
        {
            return false;
        }
        line.m_value = val;
        return true;
    }

    public class Line
    {
        public string m_fullKey = string.Empty;
        public string m_lineKey = string.Empty;
        public bool m_quoteValue;
        public string m_raw = string.Empty;
        public string m_sectionName = string.Empty;
        public ConfigFile.LineType m_type;
        public string m_value = string.Empty;
    }

    public enum LineType
    {
        UNKNOWN,
        COMMENT,
        SECTION,
        ENTRY
    }
}

