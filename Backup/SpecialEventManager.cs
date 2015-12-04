using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class SpecialEventManager
{
    private Map<SpecialEventType, EventTiming> m_eventTimings = new Map<SpecialEventType, EventTiming>();
    private HashSet<SpecialEventType> m_forcedActiveEvents;
    private static SpecialEventManager s_instance;

    private SpecialEventManager()
    {
    }

    private bool ForceEventActive(SpecialEventType eventType)
    {
        if (!ApplicationMgr.IsInternal())
        {
            return false;
        }
        if (this.m_forcedActiveEvents == null)
        {
            this.m_forcedActiveEvents = new HashSet<SpecialEventType>();
            string str = Vars.Key("Events.ForceActive").GetStr(null);
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            char[] separator = new char[] { ' ', ',', ';' };
            string[] strArray = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strArray.Length; i++)
            {
                SpecialEventType type;
                if (EnumUtils.TryGetEnum<SpecialEventType>(strArray[i], StringComparison.OrdinalIgnoreCase, out type))
                {
                    this.m_forcedActiveEvents.Add(type);
                }
            }
        }
        return this.m_forcedActiveEvents.Contains(eventType);
    }

    public static SpecialEventManager Get()
    {
        if (s_instance == null)
        {
            s_instance = new SpecialEventManager();
        }
        return s_instance;
    }

    public DateTime? GetEventLocalEndTime(SpecialEventType eventType)
    {
        if (!this.m_eventTimings.ContainsKey(eventType))
        {
            return null;
        }
        return this.m_eventTimings[eventType].EndTime;
    }

    public DateTime? GetEventLocalStartTime(SpecialEventType eventType)
    {
        if (!this.m_eventTimings.ContainsKey(eventType))
        {
            return null;
        }
        return this.m_eventTimings[eventType].StartTime;
    }

    public bool HasEventEnded(SpecialEventType eventType)
    {
        if (this.ForceEventActive(eventType))
        {
            return false;
        }
        return (!this.m_eventTimings.ContainsKey(eventType) || this.m_eventTimings[eventType].HasEnded());
    }

    public bool HasEventStarted(SpecialEventType eventType)
    {
        if (this.ForceEventActive(eventType))
        {
            return true;
        }
        if (!this.m_eventTimings.ContainsKey(eventType))
        {
            return false;
        }
        return this.m_eventTimings[eventType].HasStarted();
    }

    public void InitEventTiming(IList<SpecialEventTiming> serverEventTimingList)
    {
        if (this.m_eventTimings.Count > 0)
        {
            Debug.LogWarning("SpecialEventManager.InitEventTiming(): m_eventTimings was not empty; clearing it first");
            this.m_eventTimings.Clear();
        }
        DateTime now = DateTime.Now;
        bool flag = false;
        for (int i = 0; i < serverEventTimingList.Count; i++)
        {
            SpecialEventType type;
            SpecialEventTiming timing = serverEventTimingList[i];
            try
            {
                type = EnumUtils.GetEnum<SpecialEventType>(timing.Event);
            }
            catch (ArgumentException exception)
            {
                Error.AddDevWarning("GetEnum Error", exception.Message, new object[0]);
                flag = true;
                continue;
            }
            if (this.m_eventTimings.ContainsKey(type))
            {
                Debug.LogWarning(string.Format("SpecialEventManager.InitEventTiming duplicate entry for event {0} received", type));
                flag = true;
            }
            else
            {
                DateTime? startTime = null;
                if (timing.HasStart)
                {
                    if (timing.Start > 0L)
                    {
                        startTime = new DateTime?(now.AddSeconds((double) timing.Start));
                    }
                    else
                    {
                        startTime = new DateTime?(now);
                    }
                }
                DateTime? endTime = null;
                if (timing.HasEnd)
                {
                    if (timing.End > 0L)
                    {
                        endTime = new DateTime?(now.AddSeconds((double) timing.End));
                    }
                    else
                    {
                        endTime = new DateTime?(now);
                    }
                }
                this.m_eventTimings[type] = new EventTiming(startTime, endTime);
            }
        }
        if (flag && ApplicationMgr.IsInternal())
        {
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < serverEventTimingList.Count; j++)
            {
                SpecialEventTiming timing2 = serverEventTimingList[j];
                builder.Append("\n   serverEvent=").Append(timing2.Event);
                builder.Append(" start=").Append(!timing2.HasStart ? "null" : timing2.Start.ToString());
                builder.Append(" end=").Append(!timing2.HasEnd ? "null" : timing2.End.ToString());
            }
            foreach (SpecialEventType type2 in this.m_eventTimings.Keys)
            {
                EventTiming timing3 = this.m_eventTimings[type2];
                builder.Append("\n   mgrEvent=").Append(type2);
                builder.Append(" start=").Append(!timing3.StartTime.HasValue ? "none" : timing3.StartTime.Value.ToString());
                builder.Append(" end=").Append(!timing3.EndTime.HasValue ? "none" : timing3.EndTime.Value.ToString());
            }
            Debug.LogWarning(string.Format("EventTiming dump: {0}", builder.ToString()));
        }
    }

    public bool IsEventActive(SpecialEventType eventType, bool activeIfDoesNotExist)
    {
        if (this.ForceEventActive(eventType))
        {
            return true;
        }
        if (!this.m_eventTimings.ContainsKey(eventType))
        {
            return activeIfDoesNotExist;
        }
        return this.m_eventTimings[eventType].IsActiveNow();
    }

    public bool IsEventActive(string eventName, bool activeIfDoesNotExist)
    {
        SpecialEventType type;
        if ("always" == eventName)
        {
            return true;
        }
        if (!EnumUtils.TryGetEnum<SpecialEventType>(eventName, out type))
        {
            Debug.LogWarning(string.Format("SpecialEventManager.IsEventActive could not find SpecialEventType record for event '{0}'", eventName));
            return activeIfDoesNotExist;
        }
        return Get().IsEventActive(type, true);
    }

    private void OnReset()
    {
        this.m_eventTimings.Clear();
        this.m_forcedActiveEvents.Clear();
    }

    private class EventTiming
    {
        public EventTiming(DateTime? startTime, DateTime? endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public bool HasEnded()
        {
            if (!this.EndTime.HasValue)
            {
                return false;
            }
            return (DateTime.Now > this.EndTime.Value);
        }

        public bool HasStarted()
        {
            return (!this.StartTime.HasValue || (DateTime.Now >= this.StartTime.Value));
        }

        public bool IsActiveNow()
        {
            if ((this.StartTime.HasValue && this.EndTime.HasValue) && (this.EndTime.Value < this.StartTime.Value))
            {
                return false;
            }
            return (this.HasStarted() && !this.HasEnded());
        }

        public DateTime? EndTime { get; private set; }

        public DateTime? StartTime { get; private set; }
    }
}

