using bnet.protocol.config;
using RPCServices;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class RPCConnectionMetering
{
    private MeteringData m_data;
    private BattleNetLogSource m_log = new BattleNetLogSource("ConnectionMetering");

    public bool AllowRPCCall(uint serviceID, uint methodID)
    {
        if (this.m_data == null)
        {
            return true;
        }
        RuntimeData runtimedData = this.GetRuntimedData(serviceID, methodID);
        if (runtimedData == null)
        {
            return true;
        }
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if ((this.m_data.StartupPeriodEnd > 0f) && (realtimeSinceStartup < this.m_data.StartupPeriodEnd))
        {
            float num2 = this.m_data.StartupPeriodEnd - realtimeSinceStartup;
            object[] objArray1 = new object[] { num2, runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
            this.m_log.LogDebug("Allow (STARTUP PERIOD {0}) {1} ({2}:{3})", objArray1);
            return true;
        }
        if (runtimedData.AlwaysAllow)
        {
            object[] objArray2 = new object[] { runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
            this.m_log.LogDebug("Allow (ALWAYS ALLOW) {0} ({1}:{2})", objArray2);
            return true;
        }
        if (runtimedData.AlwaysDeny)
        {
            object[] objArray3 = new object[] { runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
            this.m_log.LogDebug("Deny (ALWAYS DENY) {0} ({1}:{2})", objArray3);
            return false;
        }
        if (runtimedData.FiniteCallsLeft != uint.MaxValue)
        {
            if (runtimedData.FiniteCallsLeft > 0)
            {
                object[] objArray4 = new object[] { runtimedData.FiniteCallsLeft, runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
                this.m_log.LogDebug("Allow (FINITE CALLS LEFT {0}) {1} ({2}:{3})", objArray4);
                runtimedData.FiniteCallsLeft--;
                return true;
            }
            object[] objArray5 = new object[] { runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
            this.m_log.LogDebug("Deny (FINITE CALLS LEFT 0) {0} ({1}:{2})", objArray5);
            return false;
        }
        bool flag = runtimedData.CanCall(realtimeSinceStartup);
        object[] args = new object[] { !flag ? "Deny" : "Allow", runtimedData.GetServiceAndMethodNames(), serviceID, methodID };
        this.m_log.LogDebug("{0} (TRACKER) {1} ({2}:{3})", args);
        return flag;
    }

    private RuntimeData GetRuntimedData(uint serviceID, uint methodID)
    {
        uint id = (serviceID * 0x3e8) + methodID;
        RuntimeData runtimeData = this.m_data.GetRuntimeData(id);
        if (runtimeData == null)
        {
            runtimeData = new RuntimeData();
            this.m_data.RuntimeData[id] = runtimeData;
            StaticData globalDefault = null;
            foreach (KeyValuePair<string, StaticData> pair in this.m_data.MethodDefaults)
            {
                if ((pair.Value.ServiceId == serviceID) && (pair.Value.MethodId == methodID))
                {
                    globalDefault = pair.Value;
                    break;
                }
            }
            if (globalDefault == null)
            {
                foreach (KeyValuePair<string, StaticData> pair2 in this.m_data.ServiceDefaults)
                {
                    if (pair2.Value.ServiceId == serviceID)
                    {
                        globalDefault = pair2.Value;
                        break;
                    }
                }
            }
            if ((globalDefault == null) && (this.m_data.GlobalDefault != null))
            {
                globalDefault = this.m_data.GlobalDefault;
            }
            if (globalDefault == null)
            {
                object[] args = new object[] { serviceID, methodID };
                this.m_log.LogDebug("Always allowing ServiceId={0} MethodId={1}", args);
                runtimeData.AlwaysAllow = true;
                return runtimeData;
            }
            runtimeData.StaticData = globalDefault;
            if (globalDefault.RateLimitCount == 0)
            {
                runtimeData.AlwaysDeny = true;
                return runtimeData;
            }
            if (globalDefault.RateLimitSeconds == 0)
            {
                runtimeData.FiniteCallsLeft = globalDefault.RateLimitCount;
                return runtimeData;
            }
            runtimeData.Tracker = new CallTracker(globalDefault.RateLimitCount, globalDefault.RateLimitSeconds);
        }
        return runtimeData;
    }

    private void InitializeInternalState(RPCMeterConfig config, ServiceCollectionHelper serviceHelper)
    {
        List<string> list = new List<string>();
        List<string> list2 = new List<string>();
        int methodCount = config.MethodCount;
        for (int i = 0; i < methodCount; i++)
        {
            RPCMethodConfig method = config.Method[i];
            StaticData data = new StaticData();
            data.FromProtocol(method);
            if (!method.HasServiceName)
            {
                if (this.m_data.GlobalDefault == null)
                {
                    this.m_data.GlobalDefault = data;
                    object[] args = new object[] { data };
                    this.m_log.LogDebug("Adding global default {0}", args);
                }
                else
                {
                    this.m_log.LogWarning("Static data has two defaults, ignoring additional ones.");
                }
            }
            else
            {
                string serviceName = method.ServiceName;
                ServiceDescriptor importedServiceByName = serviceHelper.GetImportedServiceByName(serviceName);
                if (importedServiceByName == null)
                {
                    if (!list2.Contains(serviceName))
                    {
                        object[] objArray2 = new object[] { serviceName };
                        this.m_log.LogDebug("Ignoring not imported service {0}", objArray2);
                        list2.Add(serviceName);
                    }
                }
                else
                {
                    data.ServiceId = importedServiceByName.Id;
                    if (method.HasMethodName)
                    {
                        string methodName = method.MethodName;
                        string name = string.Format("{0}.{1}", serviceName, methodName);
                        MethodDescriptor methodDescriptorByName = importedServiceByName.GetMethodDescriptorByName(name);
                        if (methodDescriptorByName == null)
                        {
                            object[] objArray3 = new object[] { methodName };
                            this.m_log.LogDebug("Configuration specifies an unused method {0}, ignoring.", objArray3);
                            goto Label_0231;
                        }
                        if (this.m_data.MethodDefaults.ContainsKey(name))
                        {
                            object[] objArray4 = new object[] { name };
                            this.m_log.LogWarning("Default for method {0} already exists, ignoring extras.", objArray4);
                            goto Label_0231;
                        }
                        data.MethodId = methodDescriptorByName.Id;
                        this.m_data.MethodDefaults[name] = data;
                        object[] objArray5 = new object[] { data };
                        this.m_log.LogDebug("Adding Method default {0}", objArray5);
                    }
                    else
                    {
                        if (this.m_data.ServiceDefaults.ContainsKey(serviceName))
                        {
                            object[] objArray6 = new object[] { serviceName };
                            this.m_log.LogWarning("Default for service {0} already exists, ignoring extras.", objArray6);
                            goto Label_0231;
                        }
                        this.m_data.ServiceDefaults[serviceName] = data;
                        object[] objArray7 = new object[] { data };
                        this.m_log.LogDebug("Adding Service default {0}", objArray7);
                    }
                    list.Add(serviceName);
                Label_0231:;
                }
            }
        }
        foreach (KeyValuePair<uint, ServiceDescriptor> pair in serviceHelper.ImportedServices)
        {
            if (!list.Contains(pair.Value.Name) && (this.m_data.GlobalDefault == null))
            {
                object[] objArray8 = new object[] { pair.Value.Name };
                this.m_log.LogDebug("Configuration for service {0} was not found and will not be metered.", objArray8);
            }
        }
    }

    public void ResetStartupPeriod()
    {
        if (this.m_data != null)
        {
            this.m_data.StartupPeriodEnd = UnityEngine.Time.realtimeSinceStartup + this.m_data.StartupPeriodDuration;
        }
    }

    public void SetConnectionMeteringData(byte[] data, ServiceCollectionHelper serviceHelper)
    {
        this.m_data = new MeteringData();
        if (((data == null) || (data.Length == 0)) || (serviceHelper == null))
        {
            this.m_log.LogError("Unable to retrieve Connection Metering data");
        }
        else
        {
            try
            {
                RPCMeterConfig config = RPCMeterConfigParser.ParseConfig(Encoding.ASCII.GetString(data));
                if ((config == null) || !config.IsInitialized)
                {
                    this.m_data = null;
                    throw new Exception("Unable to parse metering config protocol buffer.");
                }
                this.UpdateConfigStats(config);
                if (config.HasStartupPeriod)
                {
                    this.m_data.StartupPeriodDuration = config.StartupPeriod;
                    this.m_data.StartupPeriodEnd = UnityEngine.Time.realtimeSinceStartup + config.StartupPeriod;
                    object[] args = new object[] { config.StartupPeriod };
                    this.m_log.LogDebug("StartupPeriod={0}", args);
                    object[] objArray2 = new object[] { this.m_data.StartupPeriodEnd };
                    this.m_log.LogDebug("StartupPeriodEnd={0}", objArray2);
                }
                this.InitializeInternalState(config, serviceHelper);
            }
            catch (Exception exception)
            {
                this.m_data = null;
                object[] objArray3 = new object[] { exception.Message, exception.StackTrace };
                this.m_log.LogError("EXCEPTION = {0} {1}", objArray3);
            }
            if (this.m_data == null)
            {
                this.m_log.LogError("Unable to parse Connection Metering data");
            }
        }
    }

    private bool UpdateConfigStats(RPCMeterConfig config)
    {
        int methodCount = config.MethodCount;
        for (int i = 0; i < methodCount; i++)
        {
            RPCMethodConfig method = config.Method[i];
            this.UpdateMethodStats(method);
        }
        Stats stats = this.m_data.Stats;
        this.m_log.LogDebug("Config Stats:");
        object[] args = new object[] { stats.MethodCount };
        this.m_log.LogDebug("  MethodCount={0}", args);
        object[] objArray2 = new object[] { stats.ServiceNameCount };
        this.m_log.LogDebug("  ServiceNameCount={0}", objArray2);
        object[] objArray3 = new object[] { stats.MethodNameCount };
        this.m_log.LogDebug("  MethodNameCount={0}", objArray3);
        object[] objArray4 = new object[] { stats.FixedCalledCostCount };
        this.m_log.LogDebug("  FixedCalledCostCount={0}", objArray4);
        object[] objArray5 = new object[] { stats.FixedPacketSizeCount };
        this.m_log.LogDebug("  FixedPacketSizeCount={0}", objArray5);
        object[] objArray6 = new object[] { stats.VariableMultiplierCount };
        this.m_log.LogDebug("  VariableMultiplierCount={0}", objArray6);
        object[] objArray7 = new object[] { stats.MultiplierCount };
        this.m_log.LogDebug("  MultiplierCount={0}", objArray7);
        object[] objArray8 = new object[] { stats.RateLimitCountCount };
        this.m_log.LogDebug("  RateLimitCountCount={0}", objArray8);
        object[] objArray9 = new object[] { stats.RateLimitSecondsCount };
        this.m_log.LogDebug("  RateLimitSecondsCount={0}", objArray9);
        object[] objArray10 = new object[] { stats.MaxPacketSizeCount };
        this.m_log.LogDebug("  MaxPacketSizeCount={0}", objArray10);
        object[] objArray11 = new object[] { stats.MaxEncodedSizeCount };
        this.m_log.LogDebug("  MaxEncodedSizeCount={0}", objArray11);
        object[] objArray12 = new object[] { stats.TimeoutCount };
        this.m_log.LogDebug("  TimeoutCount={0}", objArray12);
        object[] objArray13 = new object[] { stats.AggregatedRateLimitCountCount };
        this.m_log.LogDebug("  AggregatedRateLimitCountCount={0}", objArray13);
        return true;
    }

    private void UpdateMethodStats(RPCMethodConfig method)
    {
        Stats stats = this.m_data.Stats;
        stats.MethodCount++;
        if (method.HasServiceName)
        {
            Stats stats2 = this.m_data.Stats;
            stats2.ServiceNameCount++;
        }
        if (method.HasMethodName)
        {
            Stats stats3 = this.m_data.Stats;
            stats3.MethodNameCount++;
        }
        if (method.HasFixedCallCost)
        {
            Stats stats4 = this.m_data.Stats;
            stats4.FixedCalledCostCount++;
        }
        if (method.HasFixedPacketSize)
        {
            Stats stats5 = this.m_data.Stats;
            stats5.FixedPacketSizeCount++;
        }
        if (method.HasVariableMultiplier)
        {
            Stats stats6 = this.m_data.Stats;
            stats6.VariableMultiplierCount++;
        }
        if (method.HasMultiplier)
        {
            Stats stats7 = this.m_data.Stats;
            stats7.MultiplierCount++;
        }
        if (method.HasRateLimitCount)
        {
            Stats stats8 = this.m_data.Stats;
            stats8.RateLimitCountCount++;
            Stats stats9 = this.m_data.Stats;
            stats9.AggregatedRateLimitCountCount += method.RateLimitCount;
        }
        if (method.HasRateLimitSeconds)
        {
            Stats stats10 = this.m_data.Stats;
            stats10.RateLimitSecondsCount++;
        }
        if (method.HasMaxPacketSize)
        {
            Stats stats11 = this.m_data.Stats;
            stats11.MaxPacketSizeCount++;
        }
        if (method.HasMaxEncodedSize)
        {
            Stats stats12 = this.m_data.Stats;
            stats12.MaxEncodedSizeCount++;
        }
        if (method.HasTimeout)
        {
            Stats stats13 = this.m_data.Stats;
            stats13.TimeoutCount++;
        }
    }

    private class CallTracker
    {
        private int m_callIndex;
        private float[] m_calls;
        private float m_numberOfSeconds;

        public CallTracker(uint maxCalls, uint timePeriodInSeconds)
        {
            if ((maxCalls != 0) && (timePeriodInSeconds != 0))
            {
                this.m_calls = new float[maxCalls];
                this.m_numberOfSeconds = timePeriodInSeconds;
            }
        }

        public bool CanCall(float now)
        {
            if ((this.m_calls == null) || (this.m_calls.Length == 0))
            {
                return false;
            }
            if (this.m_callIndex < this.m_calls.Length)
            {
                this.m_calls[this.m_callIndex++] = now;
                return true;
            }
            float num = now - this.m_calls[0];
            if (num <= this.m_numberOfSeconds)
            {
                return false;
            }
            if (this.m_calls.Length == 1)
            {
                this.m_calls[0] = now;
                this.m_callIndex = 1;
                return true;
            }
            int num2 = 0;
            while (((num2 + 1) < this.m_calls.Length) && ((now - this.m_calls[num2 + 1]) > this.m_numberOfSeconds))
            {
                num2++;
            }
            int length = this.m_calls.Length - (num2 + 1);
            Array.Copy(this.m_calls, num2 + 1, this.m_calls, 0, length);
            this.m_callIndex = length;
            this.m_calls[this.m_callIndex++] = now;
            return true;
        }
    }

    private class MeteringData
    {
        private RPCConnectionMetering.StaticData m_globalDefault;
        private Map<string, RPCConnectionMetering.StaticData> m_methodDefaults = new Map<string, RPCConnectionMetering.StaticData>();
        private Map<uint, RPCConnectionMetering.RuntimeData> m_runtimeData = new Map<uint, RPCConnectionMetering.RuntimeData>();
        private Map<string, RPCConnectionMetering.StaticData> m_serviceDefaults = new Map<string, RPCConnectionMetering.StaticData>();
        private RPCConnectionMetering.Stats m_staticDataStats = new RPCConnectionMetering.Stats();

        public RPCConnectionMetering.RuntimeData GetRuntimeData(uint id)
        {
            RPCConnectionMetering.RuntimeData data;
            if (this.m_runtimeData.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }

        public RPCConnectionMetering.StaticData GlobalDefault
        {
            get
            {
                return this.m_globalDefault;
            }
            set
            {
                this.m_globalDefault = value;
            }
        }

        public Map<string, RPCConnectionMetering.StaticData> MethodDefaults
        {
            get
            {
                return this.m_methodDefaults;
            }
        }

        public Map<uint, RPCConnectionMetering.RuntimeData> RuntimeData
        {
            get
            {
                return this.m_runtimeData;
            }
        }

        public Map<string, RPCConnectionMetering.StaticData> ServiceDefaults
        {
            get
            {
                return this.m_serviceDefaults;
            }
        }

        public float StartupPeriodDuration { get; set; }

        public float StartupPeriodEnd { get; set; }

        public RPCConnectionMetering.Stats Stats
        {
            get
            {
                return this.m_staticDataStats;
            }
        }
    }

    private class RuntimeData
    {
        private RPCConnectionMetering.CallTracker m_callTracker;
        private uint m_finiteCallsLeft = uint.MaxValue;

        public bool CanCall(float now)
        {
            return ((this.m_callTracker == null) || this.m_callTracker.CanCall(now));
        }

        public string GetServiceAndMethodNames()
        {
            string str = ((this.StaticData == null) || (this.StaticData.ServiceName == null)) ? "<null>" : this.StaticData.ServiceName;
            string str2 = ((this.StaticData == null) || (this.StaticData.MethodName == null)) ? "<null>" : this.StaticData.MethodName;
            return string.Format("{0}.{1}", str, str2);
        }

        public bool AlwaysAllow { get; set; }

        public bool AlwaysDeny { get; set; }

        public uint FiniteCallsLeft
        {
            get
            {
                return this.m_finiteCallsLeft;
            }
            set
            {
                this.m_finiteCallsLeft = value;
            }
        }

        public RPCConnectionMetering.StaticData StaticData { get; set; }

        public RPCConnectionMetering.CallTracker Tracker
        {
            get
            {
                return this.m_callTracker;
            }
            set
            {
                this.m_callTracker = value;
            }
        }
    }

    private class StaticData
    {
        private uint m_methodId = uint.MaxValue;
        private uint m_serviceId = uint.MaxValue;

        public void FromProtocol(RPCMethodConfig method)
        {
            if (method.HasServiceName)
            {
                this.ServiceName = method.ServiceName;
            }
            if (method.HasMethodName)
            {
                this.MethodName = method.MethodName;
            }
            if (method.HasFixedCallCost)
            {
                this.FixedCallCost = method.FixedCallCost;
            }
            if (method.HasRateLimitCount)
            {
                this.RateLimitCount = method.RateLimitCount;
            }
            if (method.HasRateLimitSeconds)
            {
                this.RateLimitSeconds = method.RateLimitSeconds;
            }
        }

        public override string ToString()
        {
            string str = !string.IsNullOrEmpty(this.ServiceName) ? this.ServiceName : "<null>";
            string str2 = !string.IsNullOrEmpty(this.MethodName) ? this.MethodName : "<null>";
            object[] args = new object[] { str, str2, this.RateLimitCount, this.RateLimitSeconds, this.FixedCallCost };
            return string.Format("ServiceName={0} MethodName={1} RateLimitCount={2} RateLimitSeconds={3} FixedCallCost={4}", args);
        }

        public uint FixedCallCost { get; set; }

        public uint MethodId
        {
            get
            {
                return this.m_methodId;
            }
            set
            {
                this.m_methodId = value;
            }
        }

        public string MethodName { get; set; }

        public uint RateLimitCount { get; set; }

        public uint RateLimitSeconds { get; set; }

        public uint ServiceId
        {
            get
            {
                return this.m_serviceId;
            }
            set
            {
                this.m_serviceId = value;
            }
        }

        public string ServiceName { get; set; }
    }

    private class Stats
    {
        public uint AggregatedRateLimitCountCount { get; set; }

        public uint FixedCalledCostCount { get; set; }

        public uint FixedPacketSizeCount { get; set; }

        public uint MaxEncodedSizeCount { get; set; }

        public uint MaxPacketSizeCount { get; set; }

        public uint MethodCount { get; set; }

        public uint MethodNameCount { get; set; }

        public uint MultiplierCount { get; set; }

        public uint RateLimitCountCount { get; set; }

        public uint RateLimitSecondsCount { get; set; }

        public uint ServiceNameCount { get; set; }

        public uint TimeoutCount { get; set; }

        public uint VariableMultiplierCount { get; set; }
    }
}

