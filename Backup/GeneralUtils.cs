using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public static class GeneralUtils
{
    public const float DEVELOPMENT_BUILD_TEXT_WIDTH = 115f;

    public static bool AreArraysEqual<T>(T[] arr1, T[] arr2)
    {
        if (arr1 != arr2)
        {
            if (arr1 == null)
            {
                return false;
            }
            if (arr2 == null)
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static bool AreBytesEqual(byte[] bytes1, byte[] bytes2)
    {
        return AreArraysEqual<byte>(bytes1, bytes2);
    }

    public static void CleanDeadObjectsFromList(List<GameObject> list)
    {
        int index = 0;
        while (index < list.Count)
        {
            GameObject obj2 = list[index];
            if (obj2 != null)
            {
                index++;
            }
            else
            {
                list.RemoveAt(index);
            }
        }
    }

    public static void CleanDeadObjectsFromList<T>(List<T> list) where T: Component
    {
        int index = 0;
        while (index < list.Count)
        {
            T local = list[index];
            if (local != null)
            {
                index++;
            }
            else
            {
                list.RemoveAt(index);
            }
        }
    }

    public static void CleanNullObjectsFromList<T>(List<T> list)
    {
        int index = 0;
        while (index < list.Count)
        {
            T local = list[index];
            if (local == null)
            {
                list.RemoveAt(index);
            }
            else
            {
                index++;
            }
        }
    }

    private static object CloneClass(object obj, System.Type objType)
    {
        object obj2 = CreateNewType(objType);
        foreach (FieldInfo info in objType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            info.SetValue(obj2, CloneValue(info.GetValue(obj), info.FieldType));
        }
        return obj2;
    }

    private static object CloneValue(object src, System.Type type)
    {
        if (((src == null) || (type == typeof(string))) || !type.IsClass)
        {
            return src;
        }
        if (!type.IsGenericType)
        {
            return CloneClass(src, type);
        }
        if (src is IDictionary)
        {
            IDictionary dictionary = src as IDictionary;
            IDictionary dictionary2 = CreateNewType(type) as IDictionary;
            System.Type type2 = type.GetGenericArguments()[0];
            System.Type type3 = type.GetGenericArguments()[1];
            IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    object key = CloneValue(current.Key, type2);
                    dictionary2.Add(key, CloneValue(current.Value, type3));
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
            return dictionary2;
        }
        if (!(src is IList))
        {
            return src;
        }
        IList list = src as IList;
        IList list2 = CreateNewType(type) as IList;
        System.Type type4 = type.GetGenericArguments()[0];
        IEnumerator enumerator2 = list.GetEnumerator();
        try
        {
            while (enumerator2.MoveNext())
            {
                object obj2 = enumerator2.Current;
                list2.Add(CloneValue(obj2, type4));
            }
        }
        finally
        {
            IDisposable disposable2 = enumerator2 as IDisposable;
            if (disposable2 == null)
            {
            }
            disposable2.Dispose();
        }
        return list2;
    }

    public static T[] Combine<T>(T[] arr1, T[] arr2)
    {
        T[] destinationArray = new T[arr1.Length + arr2.Length];
        Array.Copy(arr1, 0, destinationArray, 0, arr1.Length);
        Array.Copy(arr1, 0, destinationArray, arr1.Length, arr2.Length);
        return destinationArray;
    }

    public static bool CompleteProcess(Process proc)
    {
        proc.WaitForExit();
        return LogCompletedProcess(proc);
    }

    public static bool CompleteProcess(Process proc, int millisecondTimout)
    {
        if (!proc.WaitForExit(millisecondTimout))
        {
            UnityEngine.Debug.LogError(string.Concat(new object[] { System.IO.Path.GetFileNameWithoutExtension(proc.StartInfo.FileName), " timed out after ", millisecondTimout, "milliseconds" }));
            return false;
        }
        return LogCompletedProcess(proc);
    }

    public static bool Contains(this string str, string val, StringComparison comparison)
    {
        return (str.IndexOf(val, comparison) >= 0);
    }

    private static object CreateNewType(System.Type type)
    {
        object obj2 = Activator.CreateInstance(type);
        if (obj2 == null)
        {
            throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", type.Name));
        }
        return obj2;
    }

    public static T DeepClone<T>(T obj)
    {
        return (T) CloneValue(obj, obj.GetType());
    }

    public static void DeepReset<T>(T obj)
    {
        System.Type type = typeof(T);
        T local = Activator.CreateInstance<T>();
        if (local == null)
        {
            throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", type.Name));
        }
        foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            info.SetValue(obj, info.GetValue(local));
        }
    }

    public static void ExitApplication()
    {
        Application.Quit();
    }

    public static bool ForceBool(string strVal)
    {
        string str = strVal.ToLowerInvariant().Trim();
        if ((!(str == "on") && !(str == "1")) && !(str == "true"))
        {
            return false;
        }
        return true;
    }

    public static float ForceFloat(string str)
    {
        float val = 0f;
        TryParseFloat(str, out val);
        return val;
    }

    public static int ForceInt(string str)
    {
        int val = 0;
        TryParseInt(str, out val);
        return val;
    }

    public static long ForceLong(string str)
    {
        long val = 0L;
        TryParseLong(str, out val);
        return val;
    }

    public static ulong ForceULong(string str)
    {
        ulong val = 0L;
        TryParseULong(str, out val);
        return val;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
    {
        if (enumerable != null)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    func(current);
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
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> func)
    {
        if (enumerable != null)
        {
            int num = 0;
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    func(current, num);
                    num++;
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
    }

    public static void ForEachReassign<T>(this T[] array, Func<T, T> func)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i]);
            }
        }
    }

    public static void ForEachReassign<T>(this T[] array, Func<T, int, T> func)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i], i);
            }
        }
    }

    public static string GetPatchDir()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        currentDirectory = currentDirectory.Substring(0, currentDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
        return currentDirectory.Substring(0, currentDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
    }

    public static bool IsCallbackValid(Delegate callback)
    {
        bool flag = true;
        if (callback == null)
        {
            return false;
        }
        if (!callback.Method.IsStatic)
        {
            flag = IsObjectAlive(callback.Target);
            if (!flag)
            {
                UnityEngine.Debug.LogError(string.Format("Target for callback {0} is null.", callback.Method.Name));
            }
        }
        return flag;
    }

    public static bool IsDevelopmentBuildTextVisible()
    {
        return UnityEngine.Debug.isDebugBuild;
    }

    public static bool IsEditorPlaying()
    {
        return false;
    }

    public static bool IsEven(int n)
    {
        return ((n & 1) == 0);
    }

    public static bool IsObjectAlive(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (!(obj is UnityEngine.Object))
        {
            return true;
        }
        UnityEngine.Object obj2 = (UnityEngine.Object) obj;
        return (bool) obj2;
    }

    public static bool IsOdd(int n)
    {
        return ((n & 1) == 1);
    }

    public static bool IsOverriddenMethod(MethodInfo childMethod, MethodInfo ancestorMethod)
    {
        if (childMethod == null)
        {
            return false;
        }
        if (ancestorMethod == null)
        {
            return false;
        }
        if (childMethod.Equals(ancestorMethod))
        {
            return false;
        }
        MethodInfo baseDefinition = childMethod.GetBaseDefinition();
        while (!baseDefinition.Equals(childMethod) && !baseDefinition.Equals(ancestorMethod))
        {
            MethodInfo info2 = baseDefinition;
            baseDefinition = baseDefinition.GetBaseDefinition();
            if (baseDefinition.Equals(info2))
            {
                return false;
            }
        }
        return baseDefinition.Equals(ancestorMethod);
    }

    public static void ListMove<T>(IList<T> list, int srcIndex, int dstIndex)
    {
        if (srcIndex != dstIndex)
        {
            T item = list[srcIndex];
            list.RemoveAt(srcIndex);
            if (dstIndex > srcIndex)
            {
                dstIndex--;
            }
            list.Insert(dstIndex, item);
        }
    }

    public static void ListSwap<T>(IList<T> list, int indexA, int indexB)
    {
        T local = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = local;
    }

    public static bool LogCompletedProcess(Process proc)
    {
        return (proc.ExitCode == 0);
    }

    public static bool RandomBool()
    {
        return (UnityEngine.Random.Range(0, 2) == 0);
    }

    public static float RandomSign()
    {
        return (!RandomBool() ? 1f : -1f);
    }

    public static Process RunPegasusCommonScriptWithParams(string scriptName, params string[] scriptParams)
    {
        try
        {
            string patchDir = GetPatchDir();
            UnityEngine.Debug.Log(string.Format("Running {0} on {1}", scriptName, patchDir));
            string str2 = string.Join(" ", scriptParams);
            string str3 = "bat";
            string str4 = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(patchDir, "Pegasus"), "Common"), string.Format("{0}.{1}", scriptName, str3));
            Process process = new Process {
                StartInfo = { FileName = str4, Arguments = str2 }
            };
            process.Start();
            object[] args = new object[] { scriptName };
            UnityEngine.Debug.LogFormat("{0} started...", args);
            return process;
        }
        catch (Exception exception)
        {
            object[] objArray2 = new object[] { scriptName, exception.Message };
            Blizzard.Log.Error("Failed to run {0}: {1}", objArray2);
            return null;
        }
    }

    public static string SafeFormat(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            return format;
        }
        return string.Format(format, args);
    }

    public static T[] Slice<T>(this T[] arr)
    {
        return arr.Slice<T>(0, arr.Length);
    }

    public static T[] Slice<T>(this T[] arr, int start)
    {
        return arr.Slice<T>(start, arr.Length);
    }

    public static T[] Slice<T>(this T[] arr, int start, int end)
    {
        int length = arr.Length;
        if (start < 0)
        {
            start = length + start;
        }
        if (end < 0)
        {
            end = length + end;
        }
        int num2 = end - start;
        if (num2 <= 0)
        {
            return new T[0];
        }
        int num3 = length - start;
        if (num2 > num3)
        {
            num2 = num3;
        }
        T[] destinationArray = new T[num2];
        Array.Copy(arr, start, destinationArray, 0, num2);
        return destinationArray;
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T local = a;
        a = b;
        b = local;
    }

    public static bool TryParseBool(string strVal, out bool boolVal)
    {
        if (bool.TryParse(strVal, out boolVal))
        {
            return true;
        }
        switch (strVal.ToLowerInvariant().Trim())
        {
            case "off":
            case "0":
            case "false":
                boolVal = false;
                return true;

            case "on":
            case "1":
            case "true":
                boolVal = true;
                return true;
        }
        boolVal = false;
        return false;
    }

    public static bool TryParseFloat(string str, out float val)
    {
        return float.TryParse(str, NumberStyles.Any, (IFormatProvider) null, out val);
    }

    public static bool TryParseInt(string str, out int val)
    {
        return int.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static bool TryParseLong(string str, out long val)
    {
        return long.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static bool TryParseULong(string str, out ulong val)
    {
        return ulong.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static int UnsignedMod(int x, int y)
    {
        int num = x % y;
        if (num < 0)
        {
            num += y;
        }
        return num;
    }
}

