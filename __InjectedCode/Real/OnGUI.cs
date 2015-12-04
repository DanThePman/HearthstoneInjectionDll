#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
// - Assembly-CSharp v0.0.0.0
#endregion 

class Gameplay : Scene
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void OnGUI()
    {
        HearthstoneInjectionDll.DrawingGUI.ExtendGUI();

        this.LayoutProgressGUI();
        if ((GameState.Get() != null) && Options.Get().GetBool(Option.HUD))
        {
            this.LayoutSpellControllerKillTimeGUI();
        }
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    Gameplay()
    {
    }

    void Awake()
    {
    }

    void OnDestroy()
    {
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void LayoutProgressGUI()
    {
    }

    void LayoutSpellControllerKillTimeGUI()
    {
    }

    static Gameplay Get()
    {
        return default(Gameplay);
    }

    void PreUnload()
    {
    }

    bool IsUnloading()
    {
        return default(bool);
    }

    void Unload()
    {
    }

    void RemoveClassNames()
    {
    }

    void RemoveNameBanners()
    {
    }

    void AddGamePlayNameBannerPhone()
    {
    }

    void RemoveGamePlayNameBannerPhone()
    {
    }

    void UpdateFriendlySideMedalChange(MedalInfoTranslator medalInfo)
    {
    }

    void UpdateEnemySideNameBannerName(string newName)
    {
    }

    Actor GetCardDrawStandIn()
    {
        return default(Actor);
    }

    void SetGameStateBusy(bool busy, float delay)
    {
    }

    void SwapCardBacks()
    {
    }

    void ProcessGameSetupPacket()
    {
    }

    bool IsHandlingNetworkProblem()
    {
        return default(bool);
    }

    bool ShouldHandleDisconnect()
    {
        return default(bool);
    }

    void OnDisconnect(BattleNetErrors error)
    {
    }

    void HandleDisconnect()
    {
    }

    bool IsDoneUpdatingGame()
    {
        return default(bool);
    }

    bool OnBnetError(BnetErrorInfo info, object userData)
    {
        return default(bool);
    }

    void HandleLastFatalBnetError()
    {
    }

    void OnPowerHistory()
    {
    }

    void OnAllOptions()
    {
    }

    void OnEntityChoices()
    {
    }

    void OnEntitiesChosen()
    {
    }

    void OnUserUI()
    {
    }

    void OnOptionRejected()
    {
    }

    void OnTurnTimerUpdate()
    {
    }

    void OnSpectatorNotify()
    {
    }

    bool AreCriticalAssetsLoaded()
    {
        return default(bool);
    }

    bool CheckCriticalAssetLoads()
    {
        return default(bool);
    }

    void InitCardBacks()
    {
    }

    System.Collections.IEnumerator NotifyPlayersOfBoardLoad()
    {
        return default(System.Collections.IEnumerator);
    }

    void OnBoardLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnBoardProgressUpdate(string name, float progress, object callbackData)
    {
    }

    void OnBoardCamerasLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnBoardStandardGameLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnBoardTutorialLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnEndTurnButtonLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnTurnTimerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnRemoteActionHandlerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnChoiceCardMgrLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnInputManagerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnMulliganManagerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnTurnStartManagerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnTargetReticleManagerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnGameplayErrorManagerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnPlayerBannerLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnCardDrawStandinLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void OnTransitionFinished(bool cutoff, object userData)
    {
    }

    void ProcessQueuedPowerHistory()
    {
    }

    bool IsLeavingGameUnfinished()
    {
        return default(bool);
    }

    System.Collections.IEnumerator SetGameStateBusyWithDelay(bool busy, float delay)
    {
        return default(System.Collections.IEnumerator);
    }

    bool OnProcessCheat_saveme(string func, string[] args, string rawArgs)
    {
        return default(bool);
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static float TIME_TO_SET_ASYNC;
    static Gameplay s_instance;
    bool m_unloading;
    BnetErrorInfo m_lastFatalBnetErrorInfo;
    bool m_handleLastFatalBnetErrorNow;
    float m_boardProgress;
    System.Collections.Generic.List<NameBanner> m_nameBanners;
    NameBannerGamePlayPhone m_nameBannerGamePlayPhone;
    Actor m_cardDrawStandIn;
    bool m_criticalAssetsLoaded;
    #endregion

}
