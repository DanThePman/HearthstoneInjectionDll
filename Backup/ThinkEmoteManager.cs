using System;
using UnityEngine;

public class ThinkEmoteManager : MonoBehaviour
{
    private float m_secondsSinceAction;
    private static ThinkEmoteManager s_instance;
    public const float SECONDS_BEFORE_EMOTE = 20f;

    private void Awake()
    {
        s_instance = this;
    }

    public static ThinkEmoteManager Get()
    {
        return s_instance;
    }

    public void NotifyOfActivity()
    {
        this.m_secondsSinceAction = 0f;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void PlayThinkEmote()
    {
        this.m_secondsSinceAction = 0f;
        GameState.Get().GetGameEntity().OnPlayThinkEmote();
    }

    private void Update()
    {
        GameState state = GameState.Get();
        if ((state != null) && state.IsMainPhase())
        {
            this.m_secondsSinceAction += UnityEngine.Time.deltaTime;
            if (this.m_secondsSinceAction > 20f)
            {
                this.PlayThinkEmote();
            }
        }
    }
}

