#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class GameMgr
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void FindGame(PegasusShared.GameType type, int missionId, long deckId, long aiDeckId)
    {
        HearthstoneInjectionDll.Player.currentDeckId = deckId;

        this.m_lastEnterGameError = 0;
        this.ChangeFindGameState(FindGameState.CLIENT_STARTED);
        if (!AdventureProgressMgr.Get().CanPlayScenario(missionId))
        {
            //Debug.LogError(string.Format("GameMgr.FindGame() - You do not have the required adventure progress to play scenario {0}", missionId));
            this.ChangeFindGameState(FindGameState.CLIENT_ERROR);
        }
        else
        {
            this.m_nextGameType = type;
            this.m_nextMissionId = missionId;
            string popupName = this.DetermineTransitionPopupForFindGame(type, missionId);
            if (popupName != null)
            {
                this.ShowTransitionPopup(popupName);
            }
            Network.Get().FindGame(type, missionId, deckId, aiDeckId);
        }
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    GameMgr()
    {
    }

    static GameMgr()
    {
    }

    static GameMgr Get()
    {
        return default(GameMgr);
    }

    void Initialize()
    {
    }

    void OnLoggedIn()
    {
    }

    PegasusShared.GameType GetGameType()
    {
        return default(PegasusShared.GameType);
    }

    PegasusShared.GameType GetPreviousGameType()
    {
        return default(PegasusShared.GameType);
    }

    PegasusShared.GameType GetNextGameType()
    {
        return default(PegasusShared.GameType);
    }

    int GetMissionId()
    {
        return default(int);
    }

    int GetPreviousMissionId()
    {
        return default(int);
    }

    int GetNextMissionId()
    {
        return default(int);
    }

    ReconnectType GetReconnectType()
    {
        return default(ReconnectType);
    }

    ReconnectType GetPreviousReconnectType()
    {
        return default(ReconnectType);
    }

    ReconnectType GetNextReconnectType()
    {
        return default(ReconnectType);
    }

    bool IsReconnect()
    {
        return default(bool);
    }

    bool IsPreviousReconnect()
    {
        return default(bool);
    }

    bool IsNextReconnect()
    {
        return default(bool);
    }

    bool IsSpectator()
    {
        return default(bool);
    }

    bool WasSpectator()
    {
        return default(bool);
    }

    bool IsNextSpectator()
    {
        return default(bool);
    }

    uint GetLastEnterGameError()
    {
        return default(uint);
    }

    bool IsPendingAutoConcede()
    {
        return default(bool);
    }

    void SetPendingAutoConcede(bool pendingAutoConcede)
    {
    }

    FindGameState GetFindGameState()
    {
        return default(FindGameState);
    }

    bool IsFindingGame()
    {
        return default(bool);
    }

    bool IsAboutToStopFindingGame()
    {
        return default(bool);
    }

    void WaitForFriendChallengeToStart(int missionId)
    {
    }

    void SpectateGame(SpectatorProto.JoinInfo joinInfo)
    {
    }

    void OnGameServerDisconnect(BattleNetErrors error)
    {
    }

    bool CancelFindGame()
    {
        return default(bool);
    }

    GameEntity CreateGameEntity()
    {
        return default(GameEntity);
    }

    bool IsAI()
    {
        return default(bool);
    }

    bool WasAI()
    {
        return default(bool);
    }

    bool IsNextAI()
    {
        return default(bool);
    }

    bool IsTutorial()
    {
        return default(bool);
    }

    bool WasTutorial()
    {
        return default(bool);
    }

    bool IsNextTutorial()
    {
        return default(bool);
    }

    bool IsPractice()
    {
        return default(bool);
    }

    bool WasPractice()
    {
        return default(bool);
    }

    bool IsNextPractice()
    {
        return default(bool);
    }

    bool IsClassicMission()
    {
        return default(bool);
    }

    bool WasClassicMission()
    {
        return default(bool);
    }

    bool IsNextClassicMission()
    {
        return default(bool);
    }

    bool IsClassChallengeMission()
    {
        return default(bool);
    }

    bool IsExpansionMission()
    {
        return default(bool);
    }

    bool WasExpansionMission()
    {
        return default(bool);
    }

    bool IsNextExpansionMission()
    {
        return default(bool);
    }

    bool IsPlay()
    {
        return default(bool);
    }

    bool WasPlay()
    {
        return default(bool);
    }

    bool IsNextPlay()
    {
        return default(bool);
    }

    bool IsRankedPlay()
    {
        return default(bool);
    }

    bool WasRankedPlay()
    {
        return default(bool);
    }

    bool IsNextRankedPlay()
    {
        return default(bool);
    }

    bool IsUnrankedPlay()
    {
        return default(bool);
    }

    bool WasUnrankedPlay()
    {
        return default(bool);
    }

    bool IsNextUnrankedPlay()
    {
        return default(bool);
    }

    bool IsArena()
    {
        return default(bool);
    }

    bool WasArena()
    {
        return default(bool);
    }

    bool IsNextArena()
    {
        return default(bool);
    }

    bool IsFriendly()
    {
        return default(bool);
    }

    bool WasFriendly()
    {
        return default(bool);
    }

    bool IsNextFriendly()
    {
        return default(bool);
    }

    bool IsTavernBrawl()
    {
        return default(bool);
    }

    bool IsNextTavernBrawl()
    {
        return default(bool);
    }

    bool IsTransitionPopupShown()
    {
        return default(bool);
    }

    TransitionPopup GetTransitionPopup()
    {
        return default(TransitionPopup);
    }

    void UpdatePresence()
    {
    }

    void WillReset()
    {
    }

    void OnServerResult()
    {
    }

    void OnServerResult_Retry(object userData)
    {
    }

    void ChangeBoardIfNecessary()
    {
    }

    void PreloadTransitionPopup()
    {
    }

    string DetermineTransitionPopupForFindGame(PegasusShared.GameType gameType, int missionId)
    {
        return default(string);
    }

    void LoadTransitionPopup(string popupName)
    {
    }

    void ShowTransitionPopup(string popupName)
    {
    }

    void OnTransitionPopupCanceled()
    {
    }

    void HideTransitionPopup()
    {
    }

    void DestroyTransitionPopup()
    {
    }

    void OnGameSetup()
    {
    }

    void OnGameCanceled()
    {
    }

    bool OnBnetError(BnetErrorInfo info, object userData)
    {
        return default(bool);
    }

    void HandleGameCanceled()
    {
    }

    bool OnReconnectTimeout(object userData)
    {
        return default(bool);
    }

    bool ChangeFindGameState(FindGameState state)
    {
        return default(bool);
    }

    bool FireFindGameEvent(FindGameEventData eventData)
    {
        return default(bool);
    }

    void DoDefaultFindGameEventBehavior(FindGameEventData eventData)
    {
    }

    void FinalizeState(FindGameEventData eventData)
    {
    }

    void OnGameEnded()
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static string MATCHING_POPUP_PC_NAME;
    static string MATCHING_POPUP_PHONE_NAME;
    static string LOADING_POPUP_NAME;
    PlatformDependentValue<string> MATCHING_POPUP_NAME;
    Map<string, System.Type> s_transitionPopupNameToType;
    static GameMgr s_instance;
    PegasusShared.GameType m_gameType;
    PegasusShared.GameType m_prevGameType;
    PegasusShared.GameType m_nextGameType;
    int m_missionId;
    int m_prevMissionId;
    int m_nextMissionId;
    ReconnectType m_reconnectType;
    ReconnectType m_prevReconnectType;
    ReconnectType m_nextReconnectType;
    bool m_spectator;
    bool m_prevSpectator;
    bool m_nextSpectator;
    uint m_lastEnterGameError;
    bool m_pendingAutoConcede;
    FindGameState m_findGameState;
    TransitionPopup m_transitionPopup;
    UnityEngine.Vector3 m_initialTransitionPopupPos;
    static Map<Type, System.Nullable<FindGameState>> s_bnetToFindGameResultMap;
    #endregion

}
