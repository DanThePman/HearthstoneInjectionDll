using System;

public class PlayerChatInfo
{
    private float m_lastFocusTime;
    private BnetWhisper m_lastSeenWhisper;
    private BnetPlayer m_player;

    public float GetLastFocusTime()
    {
        return this.m_lastFocusTime;
    }

    public BnetWhisper GetLastSeenWhisper()
    {
        return this.m_lastSeenWhisper;
    }

    public BnetPlayer GetPlayer()
    {
        return this.m_player;
    }

    public void SetLastFocusTime(float time)
    {
        this.m_lastFocusTime = time;
    }

    public void SetLastSeenWhisper(BnetWhisper whisper)
    {
        this.m_lastSeenWhisper = whisper;
    }

    public void SetPlayer(BnetPlayer player)
    {
        this.m_player = player;
    }
}

