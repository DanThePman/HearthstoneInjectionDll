using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameToastMgr : MonoBehaviour
{
    private const float FADE_IN_TIME = 0.25f;
    private const float FADE_OUT_TIME = 0.5f;
    private const float HOLD_TIME = 4f;
    private int m_numToastsShown;
    public QuestProgressToast m_questProgressToastPrefab;
    public SeasonTimeRemainingToast m_seasonTimeRemainingToastPrefab;
    private List<GameToast> m_toasts = new List<GameToast>();
    private const int MAX_DAYS_TO_SHOW_TIME_REMAINING = 5;
    private static GameToastMgr s_instance;

    public void AddQuestProgressToast(string questName, string questDescription, int progress, int maxProgress)
    {
        if (progress != maxProgress)
        {
            QuestProgressToast toast = UnityEngine.Object.Instantiate<QuestProgressToast>(this.m_questProgressToastPrefab);
            toast.UpdateDisplay(questName, questDescription, progress, maxProgress);
            this.AddToast(toast);
        }
    }

    public void AddSeasonTimeRemainingToast()
    {
        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
        if (netObject != null)
        {
            int num2;
            TimeSpan span = (TimeSpan) (DateTime.FromFileTimeUtc(netObject.SeasonEndDate) - DateTime.UtcNow);
            int totalSeconds = (int) span.TotalSeconds;
            if (totalSeconds < 0x15180)
            {
                num2 = 1;
            }
            else
            {
                num2 = totalSeconds / 0x15180;
            }
            int @int = Options.Get().GetInt(Option.SEASON_END_THRESHOLD);
            if (num2 != @int)
            {
                int val = -1;
                switch (num2)
                {
                    case 1:
                        val = 1;
                        break;

                    case 2:
                        val = 2;
                        break;

                    case 3:
                        val = 3;
                        break;

                    case 4:
                        val = 4;
                        break;

                    case 5:
                        val = 5;
                        break;

                    case 10:
                        val = 10;
                        break;
                }
                if (val != -1)
                {
                    string elapsedTimeString;
                    Options.Get().SetInt(Option.SEASON_END_THRESHOLD, val);
                    if (num2 < 5)
                    {
                        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT)
                        {
                            return;
                        }
                        if (!Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE))
                        {
                            return;
                        }
                    }
                    if (totalSeconds < 0x15180)
                    {
                        elapsedTimeString = TimeUtils.GetElapsedTimeString(totalSeconds, TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET);
                    }
                    else
                    {
                        object[] args = new object[] { num2 };
                        elapsedTimeString = GameStrings.Format(TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET.m_days, args);
                    }
                    DbfRecord record = GameDbf.Season.GetRecord(netObject.Season);
                    if (record != null)
                    {
                        string locString = record.GetLocString("NAME");
                        SeasonTimeRemainingToast toast = UnityEngine.Object.Instantiate<SeasonTimeRemainingToast>(this.m_seasonTimeRemainingToastPrefab);
                        object[] objArray2 = new object[] { elapsedTimeString };
                        toast.UpdateDisplay(locString, GameStrings.Format("GLOBAL_REMAINING_DATETIME", objArray2));
                        this.AddToast(toast);
                    }
                }
            }
        }
    }

    public void AddToast(GameToast toast)
    {
        toast.transform.parent = BnetBar.Get().m_questProgressToastBone.transform;
        toast.transform.localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));
        toast.transform.localScale = new Vector3(450f, 1f, 450f);
        toast.transform.localPosition = Vector3.zero;
        this.m_toasts.Add(toast);
        RenderUtils.SetAlpha(toast.gameObject, 0f);
        this.UpdateToastPositions();
        object[] args = new object[] { "amount", 1f, "time", 0.25f, "delay", 0.25f, "easeType", iTween.EaseType.easeInCubic, "oncomplete", "FadeOutToast", "oncompletetarget", base.gameObject, "oncompleteparams", toast };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(toast.gameObject, hashtable);
    }

    public bool AreToastsActive()
    {
        return (this.m_toasts.Count > 0);
    }

    private void Awake()
    {
        s_instance = this;
    }

    private void DeactivateToast(GameToast toast)
    {
        toast.gameObject.SetActive(false);
        this.m_toasts.Remove(toast);
        this.UpdateToastPositions();
    }

    private void FadeOutToast(GameToast toast)
    {
        object[] args = new object[] { "amount", 0f, "delay", 4f, "time", 0.25f, "easeType", iTween.EaseType.easeInCubic, "oncomplete", "DeactivateToast", "oncompletetarget", base.gameObject, "oncompleteparams", toast };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(toast.gameObject, hashtable);
    }

    public static GameToastMgr Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void UpdateQuestProgressToasts()
    {
        foreach (Achievement achievement in AchieveManager.Get().GetNewlyProgressedQuests())
        {
            this.AddQuestProgressToast(achievement.Name, achievement.Description, achievement.Progress, achievement.MaxProgress);
            achievement.AckCurrentProgressAndRewardNotices(true);
        }
    }

    private void UpdateToastPositions()
    {
        int num = 0;
        foreach (GameToast toast in this.m_toasts)
        {
            if (num > 0)
            {
                TransformUtil.SetPoint(toast.gameObject, Anchor.BOTTOM, this.m_toasts[num - 1].gameObject, Anchor.TOP, new Vector3(0f, 5f, 0f));
            }
            num++;
        }
    }

    public enum SEASON_TOAST_THRESHHOLDS
    {
        FIVE = 5,
        FOUR = 4,
        NONE = 0,
        ONE = 1,
        TEN = 10,
        THREE = 3,
        TWO = 2
    }

    public enum TOAST_TYPE
    {
        NORMAL,
        NO_COUNT
    }
}

