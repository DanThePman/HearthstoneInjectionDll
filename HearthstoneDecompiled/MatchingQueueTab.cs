using System;
using UnityEngine;

public class MatchingQueueTab : MonoBehaviour
{
    public UberText m_queueTime;
    private float m_timeInQueue;
    private TimeUtils.ElapsedStringSet m_timeStringSet;
    public UberText m_waitTime;
    private const int SUPPRESS_TIME = 30;
    private const string TIME_RANGE_STRING = "GLOBAL_APPROXIMATE_DATETIME_RANGE";
    private const string TIME_STRING = "GLOBAL_APPROXIMATE_DATETIME";

    private string GetElapsedTimeString(int minSeconds, int maxSeconds)
    {
        TimeUtils.ElapsedTimeType type;
        int num;
        TimeUtils.ElapsedTimeType type2;
        int num2;
        TimeUtils.GetElapsedTime(minSeconds, out type, out num);
        if (minSeconds == maxSeconds)
        {
            object[] objArray1 = new object[] { TimeUtils.GetElapsedTimeString(minSeconds, this.m_timeStringSet) };
            return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME", objArray1);
        }
        TimeUtils.GetElapsedTime(maxSeconds, out type2, out num2);
        if (type == type2)
        {
            switch (type)
            {
                case TimeUtils.ElapsedTimeType.SECONDS:
                {
                    object[] objArray2 = new object[2];
                    objArray2[0] = num;
                    object[] objArray3 = new object[] { num2 };
                    objArray2[1] = GameStrings.Format(this.m_timeStringSet.m_seconds, objArray3);
                    return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", objArray2);
                }
                case TimeUtils.ElapsedTimeType.MINUTES:
                {
                    object[] objArray4 = new object[2];
                    objArray4[0] = num;
                    object[] objArray5 = new object[] { num2 };
                    objArray4[1] = GameStrings.Format(this.m_timeStringSet.m_minutes, objArray5);
                    return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", objArray4);
                }
                case TimeUtils.ElapsedTimeType.HOURS:
                {
                    object[] objArray6 = new object[2];
                    objArray6[0] = num;
                    object[] objArray7 = new object[] { num2 };
                    objArray6[1] = GameStrings.Format(this.m_timeStringSet.m_hours, objArray7);
                    return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", objArray6);
                }
                case TimeUtils.ElapsedTimeType.YESTERDAY:
                    return GameStrings.Get(this.m_timeStringSet.m_yesterday);

                case TimeUtils.ElapsedTimeType.DAYS:
                {
                    object[] objArray8 = new object[2];
                    objArray8[0] = num;
                    object[] objArray9 = new object[] { num2 };
                    objArray8[1] = GameStrings.Format(this.m_timeStringSet.m_days, objArray9);
                    return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", objArray8);
                }
                case TimeUtils.ElapsedTimeType.WEEKS:
                {
                    object[] objArray10 = new object[] { num, num2 };
                    return GameStrings.Format(this.m_timeStringSet.m_weeks, objArray10);
                }
            }
            return GameStrings.Get(this.m_timeStringSet.m_monthAgo);
        }
        string str = TimeUtils.GetElapsedTimeString(type, num, this.m_timeStringSet);
        string str2 = TimeUtils.GetElapsedTimeString(type2, num2, this.m_timeStringSet);
        object[] args = new object[] { str, str2 };
        return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", args);
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }

    private void InitTimeStringSet()
    {
        if (this.m_timeStringSet == null)
        {
            TimeUtils.ElapsedStringSet set = new TimeUtils.ElapsedStringSet {
                m_seconds = "GLOBAL_DATETIME_SPINNER_SECONDS",
                m_minutes = "GLOBAL_DATETIME_SPINNER_MINUTES",
                m_hours = "GLOBAL_DATETIME_SPINNER_HOURS",
                m_yesterday = "GLOBAL_DATETIME_SPINNER_DAY",
                m_days = "GLOBAL_DATETIME_SPINNER_DAYS",
                m_weeks = "GLOBAL_DATETIME_SPINNER_WEEKS",
                m_monthAgo = "GLOBAL_DATETIME_SPINNER_MONTH"
            };
            this.m_timeStringSet = set;
        }
    }

    public void ResetTimer()
    {
        this.m_timeInQueue = 0f;
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
    }

    private void Update()
    {
        this.InitTimeStringSet();
        this.m_timeInQueue += UnityEngine.Time.deltaTime;
        this.m_waitTime.Text = TimeUtils.GetElapsedTimeString(Mathf.RoundToInt(this.m_timeInQueue), this.m_timeStringSet);
    }

    public void UpdateDisplay(int minSeconds, int maxSeconds)
    {
        this.InitTimeStringSet();
        int num = Mathf.RoundToInt(this.m_timeInQueue);
        maxSeconds += num;
        if (maxSeconds <= 30)
        {
            this.Hide();
        }
        else
        {
            this.m_queueTime.Text = this.GetElapsedTimeString(minSeconds + num, maxSeconds);
            this.Show();
        }
    }
}

