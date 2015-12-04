using System;
using UnityEngine;

public class InactivePlayerKicker : MonoBehaviour
{
    private const float DEFAULT_KICK_SEC = 1800f;
    private bool m_activityDetected;
    private bool m_checkingForInactivity;
    private float m_inactivityStartTimestamp;
    private float m_kickSec = 1800f;
    private bool m_shouldCheckForInactivity = true;
    private static InactivePlayerKicker s_instance;

    private void Awake()
    {
        s_instance = this;
        ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
    }

    private bool CanCheckForInactivity()
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            return false;
        }
        if (!Network.IsLoggedIn())
        {
            return false;
        }
        if (!this.m_shouldCheckForInactivity)
        {
            return false;
        }
        if (ApplicationMgr.IsInternal() && !Options.Get().GetBool(Option.IDLE_KICKER))
        {
            return false;
        }
        return true;
    }

    private void CheckActivity()
    {
        if (this.IsCheckingForInactivity())
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseDrag:
                case EventType.KeyDown:
                case EventType.KeyUp:
                case EventType.ScrollWheel:
                    this.m_activityDetected = true;
                    return;
            }
            if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
            {
                this.m_activityDetected = true;
            }
        }
    }

    private void CheckInactivity()
    {
        if (this.IsCheckingForInactivity())
        {
            if (this.m_activityDetected)
            {
                this.m_inactivityStartTimestamp = UnityEngine.Time.realtimeSinceStartup;
                this.m_activityDetected = false;
            }
            else
            {
                float num = UnityEngine.Time.realtimeSinceStartup - this.m_inactivityStartTimestamp;
                if (num >= this.m_kickSec)
                {
                    Error.AddFatalLoc("GLOBAL_ERROR_INACTIVITY_KICK", new object[0]);
                    if (ApplicationMgr.AllowResetFromFatalError == null)
                    {
                        UnityEngine.Object.Destroy(this);
                    }
                }
            }
        }
    }

    public static InactivePlayerKicker Get()
    {
        return s_instance;
    }

    public float GetKickSec()
    {
        return this.m_kickSec;
    }

    public bool IsCheckingForInactivity()
    {
        return this.m_checkingForInactivity;
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(s_instance.WillReset);
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        }
        if (ApplicationMgr.IsInternal())
        {
            Options.Get().UnregisterChangedListener(Option.IDLE_KICK_TIME, new Options.ChangedCallback(this.OnOptionChanged));
            Options.Get().UnregisterChangedListener(Option.IDLE_KICKER, new Options.ChangedCallback(this.OnOptionChanged));
        }
        s_instance = null;
    }

    private void OnGUI()
    {
        this.CheckActivity();
    }

    public void OnLoggedIn()
    {
        this.UpdateIdleKickTimeOption();
        this.UpdateCheckForInactivity();
    }

    private void OnOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        Option option2 = option;
        if (option2 == Option.IDLE_KICKER)
        {
            this.UpdateCheckForInactivity();
        }
        else if (option2 == Option.IDLE_KICK_TIME)
        {
            this.UpdateIdleKickTimeOption();
        }
        else
        {
            object[] messageArgs = new object[] { option };
            Error.AddDevFatal("InactivePlayerKicker.OnOptionChanged() - unhandled option {0}", messageArgs);
        }
    }

    private void OnScenePreUnload(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FATAL_ERROR)
        {
            if (ApplicationMgr.AllowResetFromFatalError != null)
            {
                this.SetShouldCheckForInactivity(false);
            }
            else
            {
                UnityEngine.Object.Destroy(this);
            }
        }
    }

    public void SetKickSec(float sec)
    {
        this.m_kickSec = sec;
    }

    public bool SetKickTimeStr(string timeStr)
    {
        float num;
        if (!TimeUtils.TryParseDevSecFromElapsedTimeString(timeStr, out num))
        {
            return false;
        }
        this.SetKickSec(num);
        return true;
    }

    public void SetShouldCheckForInactivity(bool check)
    {
        if (this.m_shouldCheckForInactivity != check)
        {
            this.m_shouldCheckForInactivity = check;
            this.UpdateCheckForInactivity();
        }
    }

    public bool ShouldCheckForInactivity()
    {
        return this.m_shouldCheckForInactivity;
    }

    private void Start()
    {
        SceneMgr.Get().RegisterScenePreUnloadEvent(new SceneMgr.ScenePreUnloadCallback(this.OnScenePreUnload));
        if (ApplicationMgr.IsInternal())
        {
            Options.Get().RegisterChangedListener(Option.IDLE_KICK_TIME, new Options.ChangedCallback(this.OnOptionChanged));
            Options.Get().RegisterChangedListener(Option.IDLE_KICKER, new Options.ChangedCallback(this.OnOptionChanged));
        }
    }

    private void StartCheckForInactivity()
    {
        this.m_activityDetected = false;
        this.m_inactivityStartTimestamp = UnityEngine.Time.realtimeSinceStartup;
    }

    private void Update()
    {
        this.CheckInactivity();
    }

    private void UpdateCheckForInactivity()
    {
        bool checkingForInactivity = this.m_checkingForInactivity;
        this.m_checkingForInactivity = this.CanCheckForInactivity();
        if (this.m_checkingForInactivity && !checkingForInactivity)
        {
            this.StartCheckForInactivity();
        }
    }

    private void UpdateIdleKickTimeOption()
    {
        if (ApplicationMgr.IsInternal())
        {
            this.SetKickTimeStr(Options.Get().GetString(Option.IDLE_KICK_TIME));
        }
    }

    private void WillReset()
    {
        this.SetShouldCheckForInactivity(true);
    }
}

