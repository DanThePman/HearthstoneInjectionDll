using System;

public class TurnTimerUpdate
{
    private float m_endTimestamp;
    private float m_secondsRemaining;
    private bool m_show;

    public float GetEndTimestamp()
    {
        return this.m_endTimestamp;
    }

    public float GetSecondsRemaining()
    {
        return this.m_secondsRemaining;
    }

    public void SetEndTimestamp(float timestamp)
    {
        this.m_endTimestamp = timestamp;
    }

    public void SetSecondsRemaining(float sec)
    {
        this.m_secondsRemaining = sec;
    }

    public void SetShow(bool show)
    {
        this.m_show = show;
    }

    public bool ShouldShow()
    {
        return this.m_show;
    }
}

