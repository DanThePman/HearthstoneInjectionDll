using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Options
{
    private const string DEPRECATED_NAME_PREFIX = "DEPRECATED";
    private const int FLAG_BIT_COUNT = 0x40;
    private const string FLAG_NAME_PREFIX = "FLAGS";
    private Map<Option, List<ChangedListener>> m_changedListeners = new Map<Option, List<ChangedListener>>();
    private Map<Option, string> m_clientOptionMap;
    private List<ChangedListener> m_globalChangedListeners = new List<ChangedListener>();
    private Map<Option, ServerOptionFlag> m_serverOptionFlagMap;
    private Map<Option, ServerOption> m_serverOptionMap;
    private const float RCP_FLAG_BIT_COUNT = 0.015625f;
    private static Options s_instance;
    private static readonly ServerOption[] s_serverFlagContainers = new ServerOption[] { ServerOption.FLAGS1, ServerOption.FLAGS2, ServerOption.FLAGS3, ServerOption.FLAGS4, ServerOption.FLAGS5, ServerOption.FLAGS6, ServerOption.FLAGS7, ServerOption.FLAGS8, ServerOption.FLAGS9, ServerOption.FLAGS10 };

    private void BuildClientOptionMap(Map<string, Option> options)
    {
        this.m_clientOptionMap = new Map<Option, string>();
        IEnumerator enumerator = Enum.GetValues(typeof(ClientOption)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                ClientOption current = (ClientOption) ((int) enumerator.Current);
                if (current != ClientOption.INVALID)
                {
                    Option option2;
                    string key = current.ToString();
                    if (!options.TryGetValue(key, out option2))
                    {
                        Debug.LogError(string.Format("Options.BuildClientOptionMap() - ClientOption {0} is not mirrored in the Option enum", current));
                    }
                    else
                    {
                        System.Type type;
                        if (!OptionDataTables.s_typeMap.TryGetValue(option2, out type))
                        {
                            Debug.LogError(string.Format("Options.BuildClientOptionMap() - ClientOption {0} has no type. Please add its type to the type map.", current));
                            continue;
                        }
                        string str2 = EnumUtils.GetString<Option>(option2);
                        this.m_clientOptionMap.Add(option2, str2);
                    }
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
    }

    private void BuildServerOptionFlagMap(Map<string, Option> options)
    {
        this.m_serverOptionFlagMap = new Map<Option, ServerOptionFlag>();
        IEnumerator enumerator = Enum.GetValues(typeof(ServerOptionFlag)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                ServerOptionFlag current = (ServerOptionFlag) ((int) enumerator.Current);
                if (current != ServerOptionFlag.LIMIT)
                {
                    string key = current.ToString();
                    if (!key.StartsWith("DEPRECATED"))
                    {
                        Option option;
                        if (!options.TryGetValue(key, out option))
                        {
                            Debug.LogError(string.Format("Options.BuildServerOptionFlagMap() - ServerOptionFlag {0} is not mirrored in the Option enum", current));
                        }
                        else
                        {
                            this.m_serverOptionFlagMap.Add(option, current);
                        }
                    }
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
    }

    private void BuildServerOptionMap(Map<string, Option> options)
    {
        this.m_serverOptionMap = new Map<Option, ServerOption>();
        IEnumerator enumerator = Enum.GetValues(typeof(ServerOption)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                ServerOption current = (ServerOption) ((int) enumerator.Current);
                switch (current)
                {
                    case ServerOption.INVALID:
                    case ServerOption.LIMIT:
                    {
                        continue;
                    }
                }
                string key = current.ToString();
                if (!key.StartsWith("FLAGS") && !key.StartsWith("DEPRECATED"))
                {
                    Option option2;
                    if (!options.TryGetValue(key, out option2))
                    {
                        Debug.LogError(string.Format("Options.BuildServerOptionMap() - ServerOption {0} is not mirrored in the Option enum", current));
                    }
                    else
                    {
                        System.Type type;
                        if (!OptionDataTables.s_typeMap.TryGetValue(option2, out type))
                        {
                            Debug.LogError(string.Format("Options.BuildServerOptionMap() - ServerOption {0} has no type. Please add its type to the type map.", current));
                            continue;
                        }
                        if (type == typeof(bool))
                        {
                            Debug.LogError(string.Format("Options.BuildServerOptionMap() - ServerOption {0} is a bool. You should convert it to a ServerOptionFlag.", current));
                            continue;
                        }
                        this.m_serverOptionMap.Add(option2, current);
                    }
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
    }

    private void DeleteClientOption(Option option, string optionName)
    {
        if (LocalOptions.Get().Has(optionName))
        {
            object clientOption = this.GetClientOption(option, optionName);
            LocalOptions.Get().Delete(optionName);
            this.RemoveListeners(option, clientOption);
        }
    }

    public void DeleteOption(Option option)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            this.DeleteClientOption(option, str);
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                this.DeleteServerOption(option, option2);
            }
            else
            {
                ServerOptionFlag flag;
                if (this.m_serverOptionFlagMap.TryGetValue(option, out flag))
                {
                    this.DeleteServerOptionFlag(option, flag);
                }
            }
        }
    }

    private void DeleteServerOption(Option option, ServerOption serverOption)
    {
        if (NetCache.Get().ClientOptionExists(serverOption))
        {
            object prevVal = this.GetServerOption(option, serverOption);
            NetCache.Get().DeleteClientOption(serverOption);
            this.RemoveListeners(option, prevVal);
        }
    }

    private void DeleteServerOptionFlag(Option option, ServerOptionFlag serverOptionFlag)
    {
        ServerOption option2;
        ulong num;
        ulong num2;
        this.GetServerOptionFlagInfo(serverOptionFlag, out option2, out num, out num2);
        ulong uLongOption = NetCache.Get().GetULongOption(option2);
        if ((uLongOption & num2) != 0L)
        {
            bool prevVal = (uLongOption & num) != 0L;
            uLongOption &= ~num2;
            NetCache.Get().SetULongOption(option2, uLongOption);
            this.RemoveListeners(option, prevVal);
        }
    }

    private void FireChangedEvent(Option option, object prevVal, bool existed)
    {
        List<ChangedListener> list;
        if (this.m_changedListeners.TryGetValue(option, out list))
        {
            ChangedListener[] listenerArray = list.ToArray();
            for (int j = 0; j < listenerArray.Length; j++)
            {
                listenerArray[j].Fire(option, prevVal, existed);
            }
        }
        ChangedListener[] listenerArray2 = this.m_globalChangedListeners.ToArray();
        for (int i = 0; i < listenerArray2.Length; i++)
        {
            listenerArray2[i].Fire(option, prevVal, existed);
        }
    }

    public static Options Get()
    {
        if (s_instance == null)
        {
            s_instance = new Options();
            s_instance.Initialize();
        }
        return s_instance;
    }

    public bool GetBool(Option option)
    {
        bool flag;
        object obj2;
        if (this.GetBoolImpl(option, out flag))
        {
            return flag;
        }
        return (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2) && ((bool) obj2));
    }

    public bool GetBool(Option option, bool defaultVal)
    {
        bool flag;
        if (this.GetBoolImpl(option, out flag))
        {
            return flag;
        }
        return defaultVal;
    }

    private bool GetBoolImpl(Option option, out bool val)
    {
        object obj2;
        val = false;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (bool) obj2;
            return true;
        }
        return false;
    }

    private object GetClientOption(Option option, string optionName)
    {
        System.Type optionType = this.GetOptionType(option);
        if (optionType == typeof(bool))
        {
            return LocalOptions.Get().GetBool(optionName);
        }
        if (optionType == typeof(int))
        {
            return LocalOptions.Get().GetInt(optionName);
        }
        if (optionType == typeof(long))
        {
            return LocalOptions.Get().GetLong(optionName);
        }
        if (optionType == typeof(ulong))
        {
            return LocalOptions.Get().GetULong(optionName);
        }
        if (optionType == typeof(float))
        {
            return LocalOptions.Get().GetFloat(optionName);
        }
        if (optionType == typeof(string))
        {
            return LocalOptions.Get().GetString(optionName);
        }
        object[] messageArgs = new object[] { option, optionType };
        Error.AddDevFatal("Options.GetClientOption() - option {0} has unsupported underlying type {1}", messageArgs);
        return null;
    }

    public Map<Option, string> GetClientOptions()
    {
        return this.m_clientOptionMap;
    }

    public T GetEnum<T>(Option option)
    {
        T local;
        object obj2;
        if (this.GetEnumImpl<T>(option, out local))
        {
            return local;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2) && this.TranslateEnumVal<T>(option, obj2, out local))
        {
            return local;
        }
        return default(T);
    }

    public T GetEnum<T>(Option option, T defaultVal)
    {
        T local;
        if (this.GetEnumImpl<T>(option, out local))
        {
            return local;
        }
        return defaultVal;
    }

    private bool GetEnumImpl<T>(Option option, out T val)
    {
        object obj2;
        val = default(T);
        return (this.GetOptionImpl(option, out obj2) && this.TranslateEnumVal<T>(option, obj2, out val));
    }

    public float GetFloat(Option option)
    {
        float num;
        object obj2;
        if (this.GetFloatImpl(option, out num))
        {
            return num;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2))
        {
            return (float) obj2;
        }
        return 0f;
    }

    public float GetFloat(Option option, float defaultVal)
    {
        float num;
        if (this.GetFloatImpl(option, out num))
        {
            return num;
        }
        return defaultVal;
    }

    private bool GetFloatImpl(Option option, out float val)
    {
        object obj2;
        val = 0f;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (float) obj2;
            return true;
        }
        return false;
    }

    public int GetInt(Option option)
    {
        int num;
        object obj2;
        if (this.GetIntImpl(option, out num))
        {
            return num;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2))
        {
            return (int) obj2;
        }
        return 0;
    }

    public int GetInt(Option option, int defaultVal)
    {
        int num;
        if (this.GetIntImpl(option, out num))
        {
            return num;
        }
        return defaultVal;
    }

    private bool GetIntImpl(Option option, out int val)
    {
        object obj2;
        val = 0;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (int) obj2;
            return true;
        }
        return false;
    }

    public long GetLong(Option option)
    {
        long num;
        object obj2;
        if (this.GetLongImpl(option, out num))
        {
            return num;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2))
        {
            return (long) obj2;
        }
        return 0L;
    }

    public long GetLong(Option option, long defaultVal)
    {
        long num;
        if (this.GetLongImpl(option, out num))
        {
            return num;
        }
        return defaultVal;
    }

    private bool GetLongImpl(Option option, out long val)
    {
        object obj2;
        val = 0L;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (long) obj2;
            return true;
        }
        return false;
    }

    public object GetOption(Option option)
    {
        object obj2;
        object obj3;
        if (this.GetOptionImpl(option, out obj2))
        {
            return obj2;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj3))
        {
            return obj3;
        }
        return null;
    }

    public object GetOption(Option option, object defaultVal)
    {
        object obj2;
        if (this.GetOptionImpl(option, out obj2))
        {
            return obj2;
        }
        return defaultVal;
    }

    private bool GetOptionImpl(Option option, out object val)
    {
        string str;
        val = null;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            if (LocalOptions.Get().Has(str))
            {
                val = this.GetClientOption(option, str);
            }
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                if (NetCache.Get().ClientOptionExists(option2))
                {
                    val = this.GetServerOption(option, option2);
                }
            }
            else
            {
                ServerOptionFlag flag;
                if (this.m_serverOptionFlagMap.TryGetValue(option, out flag))
                {
                    ulong num;
                    ulong num2;
                    this.GetServerOptionFlagInfo(flag, out option2, out num, out num2);
                    ulong uLongOption = NetCache.Get().GetULongOption(option2);
                    if ((uLongOption & num2) != 0L)
                    {
                        val = (uLongOption & num) != 0L;
                    }
                }
            }
        }
        return (val != null);
    }

    public System.Type GetOptionType(Option option)
    {
        System.Type type;
        if (OptionDataTables.s_typeMap.TryGetValue(option, out type))
        {
            return type;
        }
        if (this.m_serverOptionFlagMap.ContainsKey(option))
        {
            return typeof(bool);
        }
        Debug.LogError(string.Format("Options.GetOptionType() - {0} does not have an option type!", option));
        return null;
    }

    private object GetServerOption(Option option, ServerOption serverOption)
    {
        System.Type optionType = this.GetOptionType(option);
        if (optionType == typeof(int))
        {
            return NetCache.Get().GetIntOption(serverOption);
        }
        if (optionType == typeof(long))
        {
            return NetCache.Get().GetLongOption(serverOption);
        }
        if (optionType == typeof(float))
        {
            return NetCache.Get().GetFloatOption(serverOption);
        }
        if (optionType == typeof(ulong))
        {
            return NetCache.Get().GetULongOption(serverOption);
        }
        object[] messageArgs = new object[] { option, optionType };
        Error.AddDevFatal("Options.GetServerOption() - option {0} has unsupported underlying type {1}", messageArgs);
        return null;
    }

    private void GetServerOptionFlagInfo(ServerOptionFlag flag, out ServerOption container, out ulong flagBit, out ulong existenceBit)
    {
        int num = (int) (ServerOptionFlag.HAS_SEEN_TOURNAMENT * flag);
        int index = Mathf.FloorToInt(num * 0.015625f);
        int num3 = num % 0x40;
        int num4 = 1 + num3;
        container = s_serverFlagContainers[index];
        flagBit = ((ulong) 1L) << num3;
        existenceBit = ((ulong) 1L) << num4;
    }

    public Map<Option, ServerOption> GetServerOptions()
    {
        return this.m_serverOptionMap;
    }

    public string GetString(Option option)
    {
        string str;
        object obj2;
        if (this.GetStringImpl(option, out str))
        {
            return str;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2))
        {
            return (string) obj2;
        }
        return string.Empty;
    }

    public string GetString(Option option, string defaultVal)
    {
        string str;
        if (this.GetStringImpl(option, out str))
        {
            return str;
        }
        return defaultVal;
    }

    private bool GetStringImpl(Option option, out string val)
    {
        object obj2;
        val = string.Empty;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (string) obj2;
            return true;
        }
        return false;
    }

    public ulong GetULong(Option option)
    {
        ulong num;
        object obj2;
        if (this.GetULongImpl(option, out num))
        {
            return num;
        }
        if (OptionDataTables.s_defaultsMap.TryGetValue(option, out obj2))
        {
            return (ulong) obj2;
        }
        return 0L;
    }

    public ulong GetULong(Option option, ulong defaultVal)
    {
        ulong num;
        if (this.GetULongImpl(option, out num))
        {
            return num;
        }
        return defaultVal;
    }

    private bool GetULongImpl(Option option, out ulong val)
    {
        object obj2;
        val = 0L;
        if (this.GetOptionImpl(option, out obj2))
        {
            val = (ulong) obj2;
            return true;
        }
        return false;
    }

    public bool HasOption(Option option)
    {
        string str;
        ServerOption option2;
        ServerOptionFlag flag;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            return LocalOptions.Get().Has(str);
        }
        if (this.m_serverOptionMap.TryGetValue(option, out option2))
        {
            return NetCache.Get().ClientOptionExists(option2);
        }
        return (this.m_serverOptionFlagMap.TryGetValue(option, out flag) && this.HasServerOptionFlag(flag));
    }

    private bool HasServerOptionFlag(ServerOptionFlag serverOptionFlag)
    {
        ServerOption option;
        ulong num;
        ulong num2;
        this.GetServerOptionFlagInfo(serverOptionFlag, out option, out num, out num2);
        return ((NetCache.Get().GetULongOption(option) & num2) != 0L);
    }

    private void Initialize()
    {
        Array values = Enum.GetValues(typeof(Option));
        Map<string, Option> options = new Map<string, Option>();
        IEnumerator enumerator = values.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Option current = (Option) ((int) enumerator.Current);
                if (current != Option.INVALID)
                {
                    string key = current.ToString();
                    options.Add(key, current);
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
        this.BuildClientOptionMap(options);
        this.BuildServerOptionMap(options);
        this.BuildServerOptionFlagMap(options);
    }

    public bool IsClientOption(Option option)
    {
        return this.m_clientOptionMap.ContainsKey(option);
    }

    public bool IsServerOption(Option option)
    {
        return this.m_serverOptionMap.ContainsKey(option);
    }

    public bool RegisterChangedListener(Option option, ChangedCallback callback)
    {
        return this.RegisterChangedListener(option, callback, null);
    }

    public bool RegisterChangedListener(Option option, ChangedCallback callback, object userData)
    {
        List<ChangedListener> list;
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_changedListeners.TryGetValue(option, out list))
        {
            list = new List<ChangedListener>();
            this.m_changedListeners.Add(option, list);
        }
        else if (list.Contains(item))
        {
            return false;
        }
        list.Add(item);
        return true;
    }

    public bool RegisterGlobalChangedListener(ChangedCallback callback)
    {
        return this.RegisterGlobalChangedListener(callback, null);
    }

    public bool RegisterGlobalChangedListener(ChangedCallback callback, object userData)
    {
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_globalChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_globalChangedListeners.Add(item);
        return true;
    }

    private void RemoveListeners(Option option, object prevVal)
    {
        this.FireChangedEvent(option, prevVal, true);
        this.m_changedListeners.Remove(option);
    }

    public void SetBool(Option option, bool val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            bool @bool = LocalOptions.Get().GetBool(str);
            if (!existed || (@bool != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, @bool, existed);
            }
        }
        else
        {
            ServerOptionFlag flag3;
            if (this.m_serverOptionFlagMap.TryGetValue(option, out flag3))
            {
                ServerOption option2;
                ulong num;
                ulong num2;
                this.GetServerOptionFlagInfo(flag3, out option2, out num, out num2);
                ulong uLongOption = NetCache.Get().GetULongOption(option2);
                bool prevVal = (uLongOption & num) != 0L;
                bool flag5 = (uLongOption & num2) != 0L;
                if (!flag5 || (prevVal != val))
                {
                    ulong num4 = !val ? (uLongOption & ~num) : (uLongOption | num);
                    num4 |= num2;
                    NetCache.Get().SetULongOption(option2, num4);
                    this.FireChangedEvent(option, prevVal, flag5);
                }
            }
        }
    }

    public void SetEnum<T>(Option option, T val)
    {
        if (!Enum.IsDefined(typeof(T), val))
        {
            object[] messageArgs = new object[] { val, typeof(T), option };
            Error.AddDevFatal("Options.SetEnum() - {0} is not convertible to enum type {1} for option {2}", messageArgs);
        }
        else
        {
            System.Type optionType = this.GetOptionType(option);
            if (optionType == typeof(int))
            {
                this.SetInt(option, Convert.ToInt32(val));
            }
            else if (optionType == typeof(long))
            {
                this.SetLong(option, Convert.ToInt64(val));
            }
            else
            {
                object[] objArray2 = new object[] { option, optionType };
                Error.AddDevFatal("Options.SetEnum() - option {0} has unsupported underlying type {1}", objArray2);
            }
        }
    }

    public void SetFloat(Option option, float val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            float @float = LocalOptions.Get().GetFloat(str);
            if (!existed || (@float != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, @float, existed);
            }
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                float num2;
                bool floatOption = NetCache.Get().GetFloatOption(option2, out num2);
                if (!floatOption || (num2 != val))
                {
                    NetCache.Get().SetFloatOption(option2, val);
                    this.FireChangedEvent(option, num2, floatOption);
                }
            }
        }
    }

    public void SetInt(Option option, int val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            int @int = LocalOptions.Get().GetInt(str);
            if (!existed || (@int != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, @int, existed);
            }
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                int num2;
                bool intOption = NetCache.Get().GetIntOption(option2, out num2);
                if (!intOption || (num2 != val))
                {
                    NetCache.Get().SetIntOption(option2, val);
                    this.FireChangedEvent(option, num2, intOption);
                }
            }
        }
    }

    public void SetLong(Option option, long val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            long @long = LocalOptions.Get().GetLong(str);
            if (!existed || (@long != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, @long, existed);
            }
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                long num2;
                bool longOption = NetCache.Get().GetLongOption(option2, out num2);
                if (!longOption || (num2 != val))
                {
                    NetCache.Get().SetLongOption(option2, val);
                    this.FireChangedEvent(option, num2, longOption);
                }
            }
        }
    }

    public void SetOption(Option option, object val)
    {
        System.Type optionType = this.GetOptionType(option);
        if (optionType == typeof(bool))
        {
            this.SetBool(option, (bool) val);
        }
        else if (optionType == typeof(int))
        {
            this.SetInt(option, (int) val);
        }
        else if (optionType == typeof(long))
        {
            this.SetLong(option, (long) val);
        }
        else if (optionType == typeof(float))
        {
            this.SetFloat(option, (float) val);
        }
        else if (optionType == typeof(string))
        {
            this.SetString(option, (string) val);
        }
        else if (optionType == typeof(ulong))
        {
            this.SetULong(option, (ulong) val);
        }
    }

    public void SetString(Option option, string val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            string prevVal = LocalOptions.Get().GetString(str);
            if (!existed || (prevVal != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, prevVal, existed);
            }
        }
    }

    public void SetULong(Option option, ulong val)
    {
        string str;
        if (this.m_clientOptionMap.TryGetValue(option, out str))
        {
            bool existed = LocalOptions.Get().Has(str);
            ulong uLong = LocalOptions.Get().GetULong(str);
            if (!existed || (uLong != val))
            {
                LocalOptions.Get().Set(str, val);
                this.FireChangedEvent(option, uLong, existed);
            }
        }
        else
        {
            ServerOption option2;
            if (this.m_serverOptionMap.TryGetValue(option, out option2))
            {
                ulong num2;
                bool uLongOption = NetCache.Get().GetULongOption(option2, out num2);
                if (!uLongOption || (num2 != val))
                {
                    NetCache.Get().SetULongOption(option2, val);
                    this.FireChangedEvent(option, num2, uLongOption);
                }
            }
        }
    }

    private bool TranslateEnumVal<T>(Option option, object genericVal, out T val)
    {
        val = default(T);
        System.Type type = typeof(T);
        try
        {
            val = (T) genericVal;
            return true;
        }
        catch (Exception)
        {
            Debug.LogError(string.Format("Options.TranslateEnumVal() - option {0} has value {1}, which cannot be converted to type {2}", option, genericVal, type));
            return false;
        }
    }

    public bool UnregisterChangedListener(Option option, ChangedCallback callback)
    {
        return this.UnregisterChangedListener(option, callback, null);
    }

    public bool UnregisterChangedListener(Option option, ChangedCallback callback, object userData)
    {
        List<ChangedListener> list;
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_changedListeners.TryGetValue(option, out list))
        {
            return false;
        }
        if (!list.Remove(item))
        {
            return false;
        }
        if (list.Count == 0)
        {
            this.m_changedListeners.Remove(option);
        }
        return true;
    }

    public bool UnregisterGlobalChangedListener(ChangedCallback callback)
    {
        return this.UnregisterGlobalChangedListener(callback, null);
    }

    public bool UnregisterGlobalChangedListener(ChangedCallback callback, object userData)
    {
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_globalChangedListeners.Remove(item);
    }

    public delegate void ChangedCallback(Option option, object prevValue, bool existed, object userData);

    private class ChangedListener : EventListener<Options.ChangedCallback>
    {
        public void Fire(Option option, object prevValue, bool didExist)
        {
            base.m_callback(option, prevValue, didExist, base.m_userData);
        }
    }
}

