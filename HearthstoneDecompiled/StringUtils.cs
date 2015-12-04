using System;
using System.Text.RegularExpressions;

public class StringUtils
{
    private static readonly string[] SPLIT_LINES_CHARS = new string[] { "\n", "\r" };

    public static bool CompareIgnoreCase(string a, string b)
    {
        return (string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0);
    }

    public static string[] SplitLines(string str)
    {
        return str.Split(SPLIT_LINES_CHARS, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string StripNewlines(string str)
    {
        return Regex.Replace(str, @"[\r\n]", string.Empty);
    }

    public static string StripNonNumbers(string str)
    {
        return Regex.Replace(str, "[^0-9]", string.Empty);
    }
}

