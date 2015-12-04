using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class TimeUtils
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map42;
    public const string DEFAULT_TIME_UNITS_STR = "sec";
    public static readonly DateTime EPOCH_TIME = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public const int MS_PER_HOUR = 0x36ee80;
    public const int MS_PER_MINUTE = 0xea60;
    public const int MS_PER_SEC = 0x3e8;
    public const int SEC_PER_DAY = 0x15180;
    public const int SEC_PER_HOUR = 0xe10;
    public const int SEC_PER_MINUTE = 60;
    public const int SEC_PER_WEEK = 0x93a80;
    public static readonly ElapsedStringSet SPLASHSCREEN_DATETIME_STRINGSET;

    static TimeUtils()
    {
        ElapsedStringSet set = new ElapsedStringSet {
            m_seconds = "GLOBAL_DATETIME_SPLASHSCREEN_SECONDS",
            m_minutes = "GLOBAL_DATETIME_SPLASHSCREEN_MINUTES",
            m_hours = "GLOBAL_DATETIME_SPLASHSCREEN_HOURS",
            m_yesterday = "GLOBAL_DATETIME_SPLASHSCREEN_DAY",
            m_days = "GLOBAL_DATETIME_SPLASHSCREEN_DAYS",
            m_weeks = "GLOBAL_DATETIME_SPLASHSCREEN_WEEKS",
            m_monthAgo = "GLOBAL_DATETIME_SPLASHSCREEN_MONTH"
        };
        SPLASHSCREEN_DATETIME_STRINGSET = set;
    }

    private static void AppendDevTimeUnitsString(string formatString, int msPerUnit, StringBuilder builder, ref long ms, ref int unitCount)
    {
        long num = ms / ((long) msPerUnit);
        if (num > 0L)
        {
            if (unitCount > 0)
            {
                builder.Append(' ');
            }
            builder.AppendFormat(formatString, num);
            unitCount++;
        }
        ms -= num * msPerUnit;
    }

    private static void AppendDevTimeUnitsString(string formatString, float secPerUnit, StringBuilder builder, ref float sec, ref int unitCount)
    {
        float num = Mathf.Floor(sec / secPerUnit);
        if (num > 0f)
        {
            if (unitCount > 0)
            {
                builder.Append(' ');
            }
            builder.AppendFormat(formatString, num);
            unitCount++;
        }
        sec -= num * secPerUnit;
    }

    public static long BinaryStamp()
    {
        return DateTime.UtcNow.ToBinary();
    }

    public static DateTime ConvertEpochMicrosecToDateTime(ulong microsec)
    {
        return EPOCH_TIME.AddMilliseconds(((double) microsec) / 1000.0);
    }

    public static float ForceDevSecFromElapsedTimeString(string timeStr)
    {
        float num;
        TryParseDevSecFromElapsedTimeString(timeStr, out num);
        return num;
    }

    public static string GetDevElapsedTimeString(long ms)
    {
        StringBuilder builder = new StringBuilder();
        int unitCount = 0;
        if (ms >= 0x36ee80L)
        {
            AppendDevTimeUnitsString("{0}h", 0x36ee80, builder, ref ms, ref unitCount);
        }
        if (ms >= 0xea60L)
        {
            AppendDevTimeUnitsString("{0}m", 0xea60, builder, ref ms, ref unitCount);
        }
        if (ms >= 0x3e8L)
        {
            AppendDevTimeUnitsString("{0}s", 0x3e8, builder, ref ms, ref unitCount);
        }
        if (unitCount <= 1)
        {
            if (unitCount > 0)
            {
                builder.Append(' ');
            }
            builder.AppendFormat("{0}ms", ms);
        }
        return builder.ToString();
    }

    public static string GetDevElapsedTimeString(float sec)
    {
        StringBuilder builder = new StringBuilder();
        int unitCount = 0;
        if (sec >= 3600f)
        {
            AppendDevTimeUnitsString("{0}h", 3600f, builder, ref sec, ref unitCount);
        }
        if (sec >= 60f)
        {
            AppendDevTimeUnitsString("{0}m", 60f, builder, ref sec, ref unitCount);
        }
        if (sec >= 1f)
        {
            AppendDevTimeUnitsString("{0}s", 1f, builder, ref sec, ref unitCount);
        }
        if (unitCount <= 1)
        {
            if (unitCount > 0)
            {
                builder.Append(' ');
            }
            float num2 = sec * 1000f;
            if (num2 > 0f)
            {
                builder.AppendFormat("{0:f0}ms", num2);
            }
            else
            {
                builder.AppendFormat("{0}ms", num2);
            }
        }
        return builder.ToString();
    }

    public static string GetDevElapsedTimeString(TimeSpan span)
    {
        return GetDevElapsedTimeString((long) span.TotalMilliseconds);
    }

    public static void GetElapsedTime(int seconds, out ElapsedTimeType timeType, out int time)
    {
        time = 0;
        if (seconds < 60)
        {
            timeType = ElapsedTimeType.SECONDS;
            time = seconds;
        }
        else if (seconds < 0xe10)
        {
            timeType = ElapsedTimeType.MINUTES;
            time = seconds / 60;
        }
        else
        {
            int num = seconds / 0x15180;
            switch (num)
            {
                case 0:
                    timeType = ElapsedTimeType.HOURS;
                    time = seconds / 0xe10;
                    return;

                case 1:
                    timeType = ElapsedTimeType.YESTERDAY;
                    return;
            }
            int num2 = seconds / 0x93a80;
            if (num2 == 0)
            {
                timeType = ElapsedTimeType.DAYS;
                time = num;
            }
            else if (num2 < 4)
            {
                timeType = ElapsedTimeType.WEEKS;
                time = num2;
            }
            else
            {
                timeType = ElapsedTimeType.MONTH_AGO;
            }
        }
    }

    public static TimeSpan GetElapsedTimeSinceEpoch(DateTime? endDateTime = new DateTime?())
    {
        DateTime time = !endDateTime.HasValue ? DateTime.UtcNow : endDateTime.Value;
        return (TimeSpan) (time - EPOCH_TIME);
    }

    public static string GetElapsedTimeString(int seconds, ElapsedStringSet stringSet)
    {
        ElapsedTimeType type;
        int num;
        GetElapsedTime(seconds, out type, out num);
        return GetElapsedTimeString(type, num, stringSet);
    }

    public static string GetElapsedTimeString(ElapsedTimeType timeType, int time, ElapsedStringSet stringSet)
    {
        switch (timeType)
        {
            case ElapsedTimeType.SECONDS:
            {
                object[] args = new object[] { time };
                return GameStrings.Format(stringSet.m_seconds, args);
            }
            case ElapsedTimeType.MINUTES:
            {
                object[] objArray2 = new object[] { time };
                return GameStrings.Format(stringSet.m_minutes, objArray2);
            }
            case ElapsedTimeType.HOURS:
            {
                object[] objArray3 = new object[] { time };
                return GameStrings.Format(stringSet.m_hours, objArray3);
            }
            case ElapsedTimeType.YESTERDAY:
            {
                if (stringSet.m_yesterday != null)
                {
                    return GameStrings.Get(stringSet.m_yesterday);
                }
                object[] objArray4 = new object[] { 1 };
                return GameStrings.Format(stringSet.m_days, objArray4);
            }
            case ElapsedTimeType.DAYS:
            {
                object[] objArray5 = new object[] { time };
                return GameStrings.Format(stringSet.m_days, objArray5);
            }
            case ElapsedTimeType.WEEKS:
            {
                object[] objArray6 = new object[] { time };
                return GameStrings.Format(stringSet.m_weeks, objArray6);
            }
        }
        return GameStrings.Get(stringSet.m_monthAgo);
    }

    public static string GetElapsedTimeStringFromEpochMicrosec(ulong microsec, ElapsedStringSet stringSet)
    {
        DateTime time = ConvertEpochMicrosecToDateTime(microsec);
        TimeSpan span = (TimeSpan) (DateTime.UtcNow - time);
        int totalSeconds = (int) span.TotalSeconds;
        return GetElapsedTimeString(totalSeconds, stringSet);
    }

    private static string ParseTimeUnitsStr(string unitsStr)
    {
        if (unitsStr != null)
        {
            unitsStr = unitsStr.ToLowerInvariant();
            string key = unitsStr;
            if (key != null)
            {
                int num;
                if (<>f__switch$map42 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(13);
                    dictionary.Add("s", 0);
                    dictionary.Add("sec", 0);
                    dictionary.Add("secs", 0);
                    dictionary.Add("second", 0);
                    dictionary.Add("seconds", 0);
                    dictionary.Add("m", 1);
                    dictionary.Add("min", 1);
                    dictionary.Add("mins", 1);
                    dictionary.Add("minute", 1);
                    dictionary.Add("minutes", 1);
                    dictionary.Add("h", 2);
                    dictionary.Add("hour", 2);
                    dictionary.Add("hours", 2);
                    <>f__switch$map42 = dictionary;
                }
                if (<>f__switch$map42.TryGetValue(key, out num))
                {
                    switch (num)
                    {
                        case 0:
                            return "sec";

                        case 1:
                            return "min";

                        case 2:
                            return "hour";
                    }
                }
            }
        }
        return "sec";
    }

    public static bool TryParseDevSecFromElapsedTimeString(string timeStr, out float sec)
    {
        sec = 0f;
        MatchCollection matchs = Regex.Matches(timeStr, @"(?<number>(?:[0-9]+,)*[0-9]+)\s*(?<units>[a-zA-Z]+)");
        if (matchs.Count == 0)
        {
            return false;
        }
        Match match = matchs[0];
        if (!match.Groups[0].Success)
        {
            return false;
        }
        System.Text.RegularExpressions.Group group = match.Groups["number"];
        System.Text.RegularExpressions.Group group2 = match.Groups["units"];
        if (!group.Success || !group2.Success)
        {
            return false;
        }
        string s = group.Value;
        string unitsStr = group2.Value;
        if (!float.TryParse(s, out sec))
        {
            return false;
        }
        switch (ParseTimeUnitsStr(unitsStr))
        {
            case "min":
                sec *= 60f;
                break;

            case "hour":
                sec *= 3600f;
                break;
        }
        return true;
    }

    public class ElapsedStringSet
    {
        public string m_days;
        public string m_hours;
        public string m_minutes;
        public string m_monthAgo;
        public string m_seconds;
        public string m_weeks;
        public string m_yesterday;
    }

    public enum ElapsedTimeType
    {
        SECONDS,
        MINUTES,
        HOURS,
        YESTERDAY,
        DAYS,
        WEEKS,
        MONTH_AGO
    }
}

