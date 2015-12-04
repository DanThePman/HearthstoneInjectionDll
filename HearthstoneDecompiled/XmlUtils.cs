using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class XmlUtils
{
    public static string EscapeXPathSearchString(string search)
    {
        char[] anyOf = new char[] { '\'', '"' };
        StringBuilder builder = new StringBuilder();
        int num = search.IndexOfAny(anyOf);
        if (num == -1)
        {
            builder.Append('\'');
            builder.Append(search);
            builder.Append('\'');
        }
        else
        {
            builder.Append("concat(");
            int startIndex = 0;
            while (num != -1)
            {
                string str3;
                builder.Append('\'');
                builder.Append(search, startIndex, num - startIndex);
                builder.Append("', ");
                if (search[num] == '\'')
                {
                    str3 = "\"'\", ";
                }
                else
                {
                    str3 = "'\"', ";
                }
                builder.Append(str3);
                startIndex = num + 1;
                num = search.IndexOfAny(anyOf, num + 1);
            }
            builder.Append('\'');
            builder.Append(search, startIndex, search.Length - startIndex);
            builder.Append("')");
        }
        return builder.ToString();
    }

    public static XmlDocument LoadXmlDocFromTextAsset(TextAsset asset)
    {
        string xml = null;
        using (StringReader reader = new StringReader(asset.text))
        {
            xml = reader.ReadToEnd();
        }
        if (xml == null)
        {
            return null;
        }
        XmlDocument document = new XmlDocument();
        document.LoadXml(xml);
        return document;
    }

    public static void RemoveAllChildNodes(XmlNode node)
    {
        if (node != null)
        {
            while (node.HasChildNodes)
            {
                node.RemoveChild(node.FirstChild);
            }
        }
    }
}

