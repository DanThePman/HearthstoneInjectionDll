using System;

public class DeckPickerTray
{
    private bool m_registeredHandlers;
    private static DeckPickerTray s_instance;

    public static DeckPickerTray Get()
    {
        if (s_instance == null)
        {
            s_instance = new DeckPickerTray();
        }
        return s_instance;
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.CLIENT_CANCELED:
                if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
                {
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CANCEL_MATCHMAKER);
                }
                DeckPickerTrayDisplay.Get().HandleGameStartupFailure();
                break;

            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_ERROR:
                DeckPickerTrayDisplay.Get().HandleGameStartupFailure();
                break;

            case FindGameState.SERVER_GAME_STARTED:
                DeckPickerTrayDisplay.Get().OnServerGameStarted();
                break;

            case FindGameState.SERVER_GAME_CANCELED:
                DeckPickerTrayDisplay.Get().OnServerGameCanceled();
                break;
        }
        return false;
    }

    public void RegisterHandlers()
    {
        if (!this.m_registeredHandlers)
        {
            GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
            this.m_registeredHandlers = true;
        }
    }

    public void Unload()
    {
        this.UnregisterHandlers();
        DefLoader.Get().ClearCardDefs();
    }

    public void UnregisterHandlers()
    {
        if (this.m_registeredHandlers)
        {
            GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
            this.m_registeredHandlers = false;
        }
    }
}

