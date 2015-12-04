using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class DebugUtils
{
    [Conditional("UNITY_EDITOR")]
    public static void Assert(bool test)
    {
        if (!test)
        {
            UnityEngine.Debug.Break();
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void Assert(bool test, string message)
    {
        if (!test)
        {
            UnityEngine.Debug.LogWarning(message);
            UnityEngine.Debug.Break();
        }
    }

    public static int CountParents(GameObject go)
    {
        int num = 0;
        if (go != null)
        {
            for (Transform transform = go.transform.parent; transform != null; transform = transform.transform.parent)
            {
                num++;
            }
        }
        return num;
    }

    public static string GetHierarchyPath(UnityEngine.Object obj)
    {
        StringBuilder b = new StringBuilder();
        GetHierarchyPath_Internal(b, obj);
        return b.ToString();
    }

    private static bool GetHierarchyPath_Internal(StringBuilder b, UnityEngine.Object obj)
    {
        if (obj == null)
        {
            return false;
        }
        Transform transform = !(obj is GameObject) ? (!(obj is Component) ? null : ((Component) obj).transform) : ((GameObject) obj).transform;
        if ((transform != null) && (transform.parent != null))
        {
            Transform parent = transform.parent;
            if (GetHierarchyPath_Internal(b, parent))
            {
                b.Append(".");
            }
        }
        b.Append(obj.name);
        return true;
    }

    public static string GetHierarchyPathAndType(UnityEngine.Object obj)
    {
        StringBuilder b = new StringBuilder();
        b.Append("[Type]=").Append(obj.GetType().FullName).Append(" [Path]=");
        GetHierarchyPath_Internal(b, obj);
        return b.ToString();
    }

    public static string HashtableToString(Hashtable table)
    {
        string str = string.Empty;
        IDictionaryEnumerator enumerator = table.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                string str2 = str;
                object[] objArray1 = new object[] { str2, current.Key, " = ", current.Value, "\n" };
                str = string.Concat(objArray1);
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
        return str;
    }
}

