using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using UnityEngine;

public static class DbfXml
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map2F;
    private const string dateFormatString = "MM/dd/yyyy HH:mm:ss";
    private static DbfField s_currentField;
    private static DbfRecord s_currentRecord;
    private static bool s_skipRead;

    public static bool Load(Dbf dbf, string xml)
    {
        return LoadDbf(dbf, xml);
    }

    public static bool Load(Dbf dbf, string path, bool editor)
    {
        return LoadDbf(dbf, path, editor);
    }

    private static void LoadColumn(Dbf dbf, XmlReader reader, bool editor)
    {
        DbfColumn column = new DbfColumn();
        string name = reader["name"];
        string str = reader["type"];
        if ((name == null) || (str == null))
        {
            Debug.LogError(string.Format("DbfXml.LoadColumn() - failed to load column for DBF {1}", dbf.GetName()));
        }
        column.SetName(name);
        column.SetDataType(EnumUtils.GetEnum<DbfDataType>(str));
        if (editor)
        {
            bool flag;
            string str3 = reader["client"];
            if ((str3 != null) && bool.TryParse(str3, out flag))
            {
                column.SetClient(flag);
            }
            string comment = reader["comment"];
            if (comment != null)
            {
                column.SetComment(comment);
            }
        }
        dbf.AddColumn(column);
    }

    private static bool LoadDbf(Dbf dbf, string xml)
    {
        bool flag;
        try
        {
            using (StringReader reader = new StringReader(xml))
            {
                using (XmlReader reader2 = XmlReader.Create(reader))
                {
                    return LoadDbf(dbf, reader2, false);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("DbfXml.LoadDbf() - failed to load. Exception=\"{0}\"", exception.Message));
            flag = false;
        }
        return flag;
    }

    private static bool LoadDbf(Dbf dbf, string path, bool editor)
    {
        bool flag;
        try
        {
            using (XmlReader reader = XmlReader.Create(path))
            {
                return LoadDbf(dbf, reader, editor);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("DbfXml.LoadDbf() - failed to load. Exception=\"{0}\"\n{1}", exception.Message, exception.StackTrace));
            flag = false;
        }
        return flag;
    }

    private static bool LoadDbf(Dbf dbf, XmlReader reader, bool editor)
    {
        s_currentRecord = null;
        while (s_skipRead || reader.Read())
        {
            s_skipRead = false;
            if (reader.NodeType == XmlNodeType.Element)
            {
                string name = reader.Name;
                if (name != null)
                {
                    int num;
                    if (<>f__switch$map2F == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                        dictionary.Add("Column", 0);
                        dictionary.Add("Record", 1);
                        dictionary.Add("Field", 2);
                        <>f__switch$map2F = dictionary;
                    }
                    if (<>f__switch$map2F.TryGetValue(name, out num))
                    {
                        switch (num)
                        {
                            case 0:
                                LoadColumn(dbf, reader, editor);
                                break;

                            case 1:
                                LoadRecord(dbf, reader, editor);
                                break;

                            case 2:
                                LoadField(dbf, reader, editor);
                                break;
                        }
                    }
                }
            }
        }
        if (s_currentRecord != null)
        {
            dbf.AddRecord(s_currentRecord);
        }
        return true;
    }

    private static void LoadField(Dbf dbf, XmlReader reader, bool editor)
    {
        DbfField field = new DbfField();
        string column = reader["column"];
        if (column == null)
        {
            Debug.LogError("DbfXml.LoadField() - failed to load field, has no column specified");
        }
        else if (s_currentRecord.HasField(column))
        {
            Debug.LogError(string.Format("DbfXml.LoadField() - failed to load field, duplicate column definition for {0}", column));
        }
        else
        {
            field.SetColumnName(column);
            DbfDataType dataType = dbf.GetDataType(column);
            if (dataType == DbfDataType.LOC_STRING)
            {
                string str2 = reader["loc_ID"];
                if (string.IsNullOrEmpty(str2) || (str2.Trim() == string.Empty))
                {
                    field.m_locID = null;
                }
                else
                {
                    int num;
                    if (!GeneralUtils.TryParseInt(str2, out num))
                    {
                        Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse loc_ID as int for field {0}", column));
                        return;
                    }
                    if (num < 0)
                    {
                        Debug.LogError(string.Format("DbfXml.LoadField() - negative number found when parsing loc_ID for field {0}", column));
                        return;
                    }
                    field.m_locID = new uint?((uint) num);
                }
                Log.DbfXml.Print("Load Field.LocalizedString " + column, new object[0]);
                List<DbfLocValue> val = LoadLocalizedString(dbf, reader, editor);
                field.SetValue(val);
                s_currentRecord.AddField(field);
                Log.DbfXml.Print("Finished Localized String " + val.Count, new object[0]);
            }
            else
            {
                string str3 = reader.ReadElementContentAsString();
                if (string.IsNullOrEmpty(str3))
                {
                    field.SetValue(str3);
                    s_currentRecord.AddField(field);
                }
                else
                {
                    switch (dataType)
                    {
                        case DbfDataType.INT:
                            int num2;
                            if (GeneralUtils.TryParseInt(str3, out num2))
                            {
                                field.SetValue(num2);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse an int for field {0}", column));
                            return;

                        case DbfDataType.LONG:
                            long num3;
                            if (GeneralUtils.TryParseLong(str3, out num3))
                            {
                                field.SetValue(num3);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse a long for field {0}", column));
                            return;

                        case DbfDataType.ULONG:
                            ulong num4;
                            if (GeneralUtils.TryParseULong(str3, out num4))
                            {
                                field.SetValue(num4);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse a ulong for field {0}", column));
                            return;

                        case DbfDataType.FLOAT:
                            float num5;
                            if (GeneralUtils.TryParseFloat(str3, out num5))
                            {
                                field.SetValue(num5);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse a float for field {0}", column));
                            return;

                        case DbfDataType.BOOL:
                            bool flag;
                            if (GeneralUtils.TryParseBool(str3, out flag))
                            {
                                field.SetValue(flag);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse a bool for field {0}", column));
                            return;

                        case DbfDataType.STRING:
                            field.SetValue(str3);
                            break;

                        case DbfDataType.ASSET_PATH:
                            field.SetValue(str3);
                            break;

                        case DbfDataType.DATE:
                            DateTime time;
                            if (DateTime.TryParseExact(str3, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowLeadingWhite, out time))
                            {
                                field.SetValue(time);
                                break;
                            }
                            Debug.LogError(string.Format("DbfXml.LoadField() - failed to parse '{1}' as a date, using format string '{2}' for field {0}", column, str3, "MM/dd/yyyy HH:mm:ss"));
                            return;

                        default:
                            field.SetValue(str3);
                            break;
                    }
                    s_currentRecord.AddField(field);
                }
            }
        }
    }

    private static List<DbfLocValue> LoadLocalizedString(Dbf dbf, XmlReader reader, bool editor)
    {
        List<DbfLocValue> list = new List<DbfLocValue>();
        int depth = reader.Depth;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                Locale locale;
                if (reader.Depth <= depth)
                {
                    break;
                }
                string name = reader.Name;
                string val = reader.ReadElementContentAsString();
                try
                {
                    locale = EnumUtils.GetEnum<Locale>(name);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                DbfLocValue item = new DbfLocValue();
                item.SetLocale(locale);
                if (editor)
                {
                    item.SetValue(val);
                }
                else
                {
                    item.SetValue(TextUtils.DecodeWhitespaces(val));
                }
                list.Add(item);
            }
        }
        s_skipRead = true;
        return list;
    }

    private static void LoadRecord(Dbf dbf, XmlReader reader, bool editor)
    {
        if (s_currentRecord != null)
        {
            dbf.AddRecord(s_currentRecord);
        }
        s_currentRecord = new DbfRecord();
    }

    public static bool Save(Dbf dbf, string path)
    {
        return SaveDbf(dbf, path);
    }

    private static void SaveColumn(Dbf dbf, DbfColumn column, XmlDocument doc, XmlElement columnElement)
    {
        XmlAttribute node = doc.CreateAttribute("name");
        node.Value = column.GetName();
        columnElement.Attributes.Append(node);
        XmlAttribute attribute2 = doc.CreateAttribute("type");
        attribute2.Value = EnumUtils.GetString<DbfDataType>(column.GetDataType());
        columnElement.Attributes.Append(attribute2);
        if (column.HasClientProperty())
        {
            XmlAttribute attribute3 = doc.CreateAttribute("client");
            attribute3.Value = column.IsClient().ToString();
            columnElement.Attributes.Append(attribute3);
        }
        string comment = column.GetComment();
        if (!string.IsNullOrEmpty(comment))
        {
            XmlAttribute attribute4 = doc.CreateAttribute("comment");
            attribute4.Value = comment;
            columnElement.Attributes.Append(attribute4);
        }
    }

    private static void SaveColumns(Dbf dbf, XmlDocument doc, XmlElement rootElement)
    {
        List<DbfColumn> columns = dbf.GetColumns();
        for (int i = 0; i < columns.Count; i++)
        {
            DbfColumn column = columns[i];
            XmlElement newChild = doc.CreateElement("Column");
            rootElement.AppendChild(newChild);
            SaveColumn(dbf, column, doc, newChild);
        }
    }

    private static bool SaveDbf(Dbf dbf, string path)
    {
        XmlWriter writer;
        XmlDocument doc = new XmlDocument();
        XmlDeclaration newChild = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(newChild);
        XmlWriterSettings settings = new XmlWriterSettings {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "\t",
            NewLineChars = "\n"
        };
        try
        {
            writer = XmlWriter.Create(path, settings);
        }
        catch (Exception exception)
        {
            Debug.LogError(string.Format("DbfXml.SaveDbf() - failed to save {0}. Exception=\"{1}\"", path, exception.Message));
            return false;
        }
        XmlElement element = doc.CreateElement("Dbf");
        doc.AppendChild(element);
        SaveColumns(dbf, doc, element);
        SaveRecords(dbf, doc, element);
        try
        {
            doc.Save(writer);
        }
        catch (Exception exception2)
        {
            Debug.LogError(string.Format("DbfXml.SaveDbf() - failed to save {0}. Exception=\"{1}\"", path, exception2.Message));
            return false;
        }
        finally
        {
            writer.Close();
        }
        return true;
    }

    private static void SaveField(Dbf dbf, DbfField field, XmlDocument doc, XmlElement fieldElement)
    {
        string columnName = field.GetColumnName();
        XmlAttribute node = doc.CreateAttribute("column");
        node.Value = columnName;
        fieldElement.Attributes.Append(node);
        DbfDataType dataType = dbf.GetDataType(columnName);
        object obj2 = field.GetValue();
        switch (dataType)
        {
            case DbfDataType.INT:
                if (obj2 is int)
                {
                    fieldElement.InnerText = ((int) obj2).ToString();
                }
                return;

            case DbfDataType.LONG:
                if (obj2 is long)
                {
                    fieldElement.InnerText = ((long) obj2).ToString();
                }
                return;

            case DbfDataType.ULONG:
                if (obj2 is ulong)
                {
                    fieldElement.InnerText = ((ulong) obj2).ToString();
                }
                return;

            case DbfDataType.FLOAT:
                if (!(obj2 is float))
                {
                    if (obj2 is double)
                    {
                        fieldElement.InnerText = ((double) obj2).ToString();
                    }
                    return;
                }
                fieldElement.InnerText = ((float) obj2).ToString();
                return;

            case DbfDataType.BOOL:
                if (obj2 is bool)
                {
                    fieldElement.InnerText = ((bool) obj2).ToString();
                }
                return;

            case DbfDataType.STRING:
                if (obj2 is string)
                {
                    fieldElement.InnerText = (string) obj2;
                }
                return;

            case DbfDataType.LOC_STRING:
            {
                List<DbfLocValue> list = obj2 as List<DbfLocValue>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        DbfLocValue value2 = list[i];
                        XmlElement newChild = doc.CreateElement(value2.GetLocale().ToString());
                        newChild.InnerText = TextUtils.EncodeWhitespaces((string) value2.GetValue());
                        fieldElement.AppendChild(newChild);
                    }
                }
                break;
            }
            case DbfDataType.ASSET_PATH:
                if (obj2 is string)
                {
                    fieldElement.InnerText = (string) obj2;
                }
                return;

            case DbfDataType.DATE:
                if (obj2 is DateTime)
                {
                    fieldElement.InnerText = ((DateTime) obj2).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                return;

            default:
                if (obj2 is string)
                {
                    fieldElement.InnerText = (string) obj2;
                }
                return;
        }
        if (field.m_locID.HasValue)
        {
            fieldElement.SetAttribute("loc_ID", TextUtils.EncodeWhitespaces(field.m_locID.ToString()));
        }
        else
        {
            fieldElement.RemoveAttribute("loc_ID");
        }
    }

    private static void SaveFields(Dbf dbf, DbfRecord record, XmlDocument doc, XmlElement recordElement)
    {
        List<DbfField> fields = record.GetFields();
        for (int i = 0; i < fields.Count; i++)
        {
            DbfField field = fields[i];
            XmlElement newChild = doc.CreateElement("Field");
            recordElement.AppendChild(newChild);
            SaveField(dbf, field, doc, newChild);
        }
    }

    private static void SaveRecord(Dbf dbf, DbfRecord record, XmlDocument doc, XmlElement recordElement)
    {
        SaveFields(dbf, record, doc, recordElement);
    }

    private static void SaveRecords(Dbf dbf, XmlDocument doc, XmlElement rootElement)
    {
        List<DbfRecord> records = dbf.GetRecords();
        for (int i = 0; i < records.Count; i++)
        {
            DbfRecord record = records[i];
            XmlElement newChild = doc.CreateElement("Record");
            rootElement.AppendChild(newChild);
            SaveRecord(dbf, record, doc, newChild);
        }
    }
}

