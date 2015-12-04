using System;

public class DefeatScreen : EndGameScreen
{
    protected override void Awake()
    {
        base.Awake();
        if (base.ShouldMakeUtilRequests())
        {
            NetCache.Get().RegisterScreenEndOfGame(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        }
    }

    protected override void ShowStandardFlow()
    {
        base.ShowStandardFlow();
        if (GameMgr.Get().IsTutorial() && !GameMgr.Get().IsSpectator())
        {
            base.m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_TutorialProgress));
        }
        else
        {
            base.m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_PrevMode));
        }
    }
}

