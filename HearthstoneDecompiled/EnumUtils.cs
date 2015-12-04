using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class EnumUtils
{
    private static Map<System.Type, Map<string, object>> s_enumCache = new Map<System.Type, Map<string, object>>();

    public static T GetEnum<T>(string str)
    {
        return GetEnum<T>(str, StringComparison.Ordinal);
    }

    public static T GetEnum<T>(string str, StringComparison comparisonType)
    {
        T local;
        if (!TryGetEnum<T>(str, comparisonType, out local))
        {
            throw new ArgumentException(string.Format("EnumUtils.GetEnum() - \"{0}\" has no matching value in enum {1}", str, typeof(T)));
        }
        return local;
    }

    public static string GetString<T>(T enumVal)
    {
        string name = enumVal.ToString();
        DescriptionAttribute[] customAttributes = (DescriptionAttribute[]) enumVal.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (customAttributes.Length > 0)
        {
            return customAttributes[0].Description;
        }
        return name;
    }

    public static int Length<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }

    public static T Parse<T>(string str)
    {
        return (T) Enum.Parse(typeof(T), str);
    }

    public static T SafeParse<T>(string str)
    {
        try
        {
            return (T) Enum.Parse(typeof(T), str);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    public static bool TryCast<T>(object inVal, out T outVal)
    {
        outVal = default(T);
        try
        {
            outVal = (T) inVal;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool TryGetEnum<T>(string str, out T outVal)
    {
        return TryGetEnum<T>(str, StringComparison.Ordinal, out outVal);
    }

    public static bool TryGetEnum<T>(string str, StringComparison comparisonType, out T result)
    {
        Map<string, object> map;
        object obj2;
        System.Type key = typeof(T);
        s_enumCache.TryGetValue(key, out map);
        if ((map != null) && map.TryGetValue(str, out obj2))
        {
            result = (T) obj2;
            return true;
        }
        IEnumerator enumerator = Enum.GetValues(key).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                T current = (T) enumerator.Current;
                bool flag = false;
                if (GetString<T>(current).Equals(str, comparisonType))
                {
                    flag = true;
                    result = current;
                }
                else
                {
                    System.Type type = current.GetType();
                    DescriptionAttribute[] customAttributes = (DescriptionAttribute[]) type.GetField(current.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                    for (int i = 0; i < customAttributes.Length; i++)
                    {
                        if (customAttributes[i].Description.Equals(str, comparisonType))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    if (map == null)
                    {
                        map = new Map<string, object>();
                        s_enumCache.Add(key, map);
                    }
                    if (!map.ContainsKey(str))
                    {
                        map.Add(str, current);
                    }
                    result = current;
                    return true;
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
        result = default(T);
        return false;
    }
}

