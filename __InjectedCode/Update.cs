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
// - UnityEngine.Analytics v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
// - HearthstoneInjectionDll v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class GameState
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void Update()
    {
        if (!this.CheckBlockingPowerProcessor())
        {
            this.m_powerProcessor.ProcessPowerQueue();
        }
        HearthstoneInjectionDll.Core.OnPublicUpdate();
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    GameState()
    {
    }

    static GameState()
    {
    }

    static GameState Get()
    {
        return default(GameState);
    }

    static GameState Initialize()
    {
        return default(GameState);
    }

    static void Shutdown()
    {
    }

    PowerProcessor GetPowerProcessor()
    {
        return default(PowerProcessor);
    }

    bool HasPowersToProcess()
    {
        return default(bool);
    }

    Entity GetEntity(int id)
    {
        return default(Entity);
    }

    Player GetPlayer(int id)
    {
        return default(Player);
    }

    GameEntity GetGameEntity()
    {
        return default(GameEntity);
    }

    void DebugSetGameEntity(GameEntity gameEntity)
    {
    }

    bool WasGameCreated()
    {
        return default(bool);
    }

    Player GetFriendlySidePlayer()
    {
        return default(Player);
    }

    Player GetOpposingSidePlayer()
    {
        return default(Player);
    }

    int GetFriendlyPlayerId()
    {
        return default(int);
    }

    int GetOpposingPlayerId()
    {
        return default(int);
    }

    bool IsFriendlySidePlayerTurn()
    {
        return default(bool);
    }

    Player GetCurrentPlayer()
    {
        return default(Player);
    }

    Player GetFirstOpponentPlayer(Player player)
    {
        return default(Player);
    }

    int GetNumFriendlyMinionsInPlay(bool includeUntouchables)
    {
        return default(int);
    }

    int GetNumEnemyMinionsInPlay(bool includeUntouchables)
    {
        return default(int);
    }

    int GetNumMinionsInPlay(Player player, bool includeUntouchables)
    {
        return default(int);
    }

    int GetTurn()
    {
        return default(int);
    }

    bool IsResponsePacketBlocked()
    {
        return default(bool);
    }

    Map<int, Entity> GetEntityMap()
    {
        return default(Map<int, Entity>);
    }

    Map<int, Player> GetPlayerMap()
    {
        return default(Map<int, Player>);
    }

    void AddPlayer(Player player)
    {
    }

    void RemovePlayer(Player player)
    {
    }

    void AddEntity(Entity entity)
    {
    }

    void RemoveEntity(Entity entity)
    {
    }

    int GetMaxSecretsPerPlayer()
    {
        return default(int);
    }

    int GetMaxFriendlyMinionsPerPlayer()
    {
        return default(int);
    }

    bool IsBusy()
    {
        return default(bool);
    }

    void SetBusy(bool busy)
    {
    }

    bool IsMulliganBusy()
    {
        return default(bool);
    }

    void SetMulliganBusy(bool busy)
    {
    }

    bool IsMulliganManagerActive()
    {
        return default(bool);
    }

    bool IsTurnStartManagerActive()
    {
        return default(bool);
    }

    bool IsTurnStartManagerBlockingInput()
    {
        return default(bool);
    }

    bool HasTheCoinBeenSpawned()
    {
        return default(bool);
    }

    void NotifyOfCoinSpawn()
    {
    }

    bool IsBeginPhase()
    {
        return default(bool);
    }

    bool IsPastBeginPhase()
    {
        return default(bool);
    }

    bool IsMainPhase()
    {
        return default(bool);
    }

    bool IsMulliganPhase()
    {
        return default(bool);
    }

    bool IsMulliganPhasePending()
    {
        return default(bool);
    }

    bool IsMulliganPhaseNowOrPending()
    {
        return default(bool);
    }

    bool IsGameCreating()
    {
        return default(bool);
    }

    bool IsGameCreated()
    {
        return default(bool);
    }

    bool IsGameCreatedOrCreating()
    {
        return default(bool);
    }

    bool WasConcedeRequested()
    {
        return default(bool);
    }

    void Concede()
    {
    }

    bool WasRestartRequested()
    {
        return default(bool);
    }

    void Restart()
    {
    }

    void CheckRestartOnRealTimeGameOver()
    {
    }

    bool IsGameOver()
    {
        return default(bool);
    }

    bool IsGameOverPending()
    {
        return default(bool);
    }

    bool IsGameOverNowOrPending()
    {
        return default(bool);
    }

    Network.HistTagChange GetRealTimeGameOverTagChange()
    {
        return default(Network.HistTagChange);
    }

    void ShowEnemyTauntCharacters()
    {
    }

    void GetTauntCounts(Player player, ref int minionCount, ref int heroCount)
    {
    }

    Card GetFriendlyCardBeingDrawn()
    {
        return default(Card);
    }

    void SetFriendlyCardBeingDrawn(Card card)
    {
    }

    Card GetOpponentCardBeingDrawn()
    {
        return default(Card);
    }

    void SetOpponentCardBeingDrawn(Card card)
    {
    }

    bool IsBeingDrawn(Card card)
    {
        return default(bool);
    }

    bool ClearCardBeingDrawn(Card card)
    {
        return default(bool);
    }

    int GetLastTurnRemindedOfFullHand()
    {
        return default(int);
    }

    void SetLastTurnRemindedOfFullHand(int turn)
    {
    }

    bool IsUsingFastActorTriggers()
    {
        return default(bool);
    }

    void SetUsingFastActorTriggers(bool enable)
    {
    }

    bool HasHandPlays()
    {
        return default(bool);
    }

    bool CanShowScoreScreen()
    {
        return default(bool);
    }

    void PreprocessRealTimeTagChange(Entity entity, Network.HistTagChange change)
    {
    }

    void PreprocessTagChange(Entity entity, TagDelta change)
    {
    }

    void PreprocessEarlyConcedeTagChange(Entity entity, TagDelta change)
    {
    }

    void ProcessEarlyConcedeTagChange(Entity entity, TagDelta change)
    {
    }

    void OnRealTimeGameOver(Network.HistTagChange change)
    {
    }

    void OnGameOver(TAG_PLAYSTATE playState)
    {
    }

    void OnCurrentPlayerChanged(Player player)
    {
    }

    void OnTurnChanged(int oldTurn, int newTurn)
    {
    }

    void AddServerBlockingSpell(Spell spell)
    {
    }

    bool RemoveServerBlockingSpell(Spell spell)
    {
        return default(bool);
    }

    void AddServerBlockingSpellController(SpellController spellController)
    {
    }

    bool RemoveServerBlockingSpellController(SpellController spellController)
    {
        return default(bool);
    }

    void DebugNukeServerBlocks()
    {
    }

    bool IsBlockingPowerProcessor()
    {
        return default(bool);
    }

    bool CanProcessPowerQueue()
    {
        return default(bool);
    }

    bool CheckBlockingPowerProcessor()
    {
        return default(bool);
    }

    bool ReconnectIfStuck()
    {
        return default(bool);
    }

    void ReportStuck()
    {
    }

    string BuildServerBlockingCausesString()
    {
        return default(string);
    }

    void AppendServerBlockingSpell(System.Text.StringBuilder builder, Spell spell)
    {
    }

    void AppendServerBlockingSpellController(System.Text.StringBuilder builder, SpellController spellController)
    {
    }

    void AppendServerBlockingHistory(System.Text.StringBuilder builder, ref int sectionCount)
    {
    }

    static void FireGameStateInitializedEvent()
    {
    }

    void FireCreateGameEvent()
    {
    }

    void FireOptionsReceivedEvent()
    {
    }

    void FireOptionsSentEvent(Network.Options.Option option)
    {
    }

    void FireOptionRejectedEvent(Network.Options.Option option)
    {
    }

    void FireEntityChoicesReceivedEvent(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
    }

    bool FireEntitiesChosenReceivedEvent(Network.EntitiesChosen chosen)
    {
        return default(bool);
    }

    void FireTurnChangedEvent(int oldTurn, int newTurn)
    {
    }

    void FireFriendlyTurnStartedEvent()
    {
    }

    void FireTurnTimerUpdateEvent(TurnTimerUpdate update)
    {
    }

    void FireCurrentPlayerChangedEvent(Player player)
    {
    }

    void FireSpectatorNotifyEvent(PegasusGame.SpectatorNotify notify)
    {
    }

    void FireGameOverEvent(TAG_PLAYSTATE playState)
    {
    }

    void FireHeroChangedEvent(Player player)
    {
    }


    Network.EntityChoices GetFriendlyEntityChoices()
    {
        return default(Network.EntityChoices);
    }

    Network.EntityChoices GetOpponentEntityChoices()
    {
        return default(Network.EntityChoices);
    }

    Network.EntityChoices GetEntityChoices(int playerId)
    {
        return default(Network.EntityChoices);
    }

    Map<int, Network.EntityChoices> GetEntityChoicesMap()
    {
        return default(Map<int, Network.EntityChoices>);
    }

    bool IsChoosableEntity(Entity entity)
    {
        return default(bool);
    }

    bool IsChosenEntity(Entity entity)
    {
        return default(bool);
    }

    bool IsValidOptionTarget(Entity entity)
    {
        return default(bool);
    }

    bool AddChosenEntity(Entity entity)
    {
        return default(bool);
    }

    bool RemoveChosenEntity(Entity entity)
    {
        return default(bool);
    }

    System.Collections.Generic.List<Entity> GetChosenEntities()
    {
        return default(System.Collections.Generic.List<Entity>);
    }

    Network.Options GetOptionsPacket()
    {
        return default(Network.Options);
    }

    void EnterChoiceMode()
    {
    }

    void EnterMainOptionMode()
    {
    }

    void EnterSubOptionMode()
    {
    }

    void EnterOptionTargetMode()
    {
    }

    void CancelCurrentOptionMode()
    {
    }

    bool IsInMainOptionMode()
    {
        return default(bool);
    }

    bool IsInSubOptionMode()
    {
        return default(bool);
    }

    bool IsInTargetMode()
    {
        return default(bool);
    }

    bool IsInChoiceMode()
    {
        return default(bool);
    }

    void SetSelectedOption(PegasusGame.ChooseOption packet)
    {
    }

    void SetChosenEntities(PegasusGame.ChooseEntities packet)
    {
    }

    void SetSelectedOption(int index)
    {
    }

    int GetSelectedOption()
    {
        return default(int);
    }

    void SetSelectedSubOption(int index)
    {
    }

    int GetSelectedSubOption()
    {
        return default(int);
    }

    void SetSelectedOptionTarget(int target)
    {
    }

    int GetSelectedOptionTarget()
    {
        return default(int);
    }

    bool IsSelectedOptionFriendlyHero()
    {
        return default(bool);
    }

    void SetSelectedOptionPosition(int position)
    {
    }

    int GetSelectedOptionPosition()
    {
        return default(int);
    }

    Network.Options.Option GetSelectedNetworkOption()
    {
        return default(Network.Options.Option);
    }

    Network.Options.Option.SubOption GetSelectedNetworkSubOption()
    {
        return default(Network.Options.Option.SubOption);
    }

    bool EntityHasSubOptions(Entity entity)
    {
        return default(bool);
    }

    bool EntityHasTargets(Entity entity)
    {
        return default(bool);
    }

    bool SubEntityHasTargets(Entity subEntity)
    {
        return default(bool);
    }

    bool HasSubOptions(Entity entity)
    {
        return default(bool);
    }

    bool HasResponse(Entity entity)
    {
        return default(bool);
    }

    bool IsChoice(Entity entity)
    {
        return default(bool);
    }

    bool IsOption(Entity entity)
    {
        return default(bool);
    }

    bool IsSubOption(Entity entity)
    {
        return default(bool);
    }

    bool IsOptionTarget(Entity entity)
    {
        return default(bool);
    }

    bool IsEntityInputEnabled(Entity entity)
    {
        return default(bool);
    }

    bool EntityHasTargets(Entity entity, bool isSubEntity)
    {
        return default(bool);
    }

    void CancelSelectedOptionProposedMana()
    {
    }

    void ClearResponseMode()
    {
    }

    void UpdateChoiceHighlights()
    {
    }

    void UpdateHighlightsBasedOnSelection()
    {
    }

    void UpdateOptionHighlights()
    {
    }

    void UpdateOptionHighlights(Network.Options options)
    {
    }

    void UpdateSubOptionHighlights(Network.Options.Option option)
    {
    }

    void UpdateTargetHighlights(Network.Options.Option.SubOption subOption)
    {
    }

    Network.Options GetLastOptions()
    {
        return default(Network.Options);
    }

    bool FriendlyHeroIsTargetable()
    {
        return default(bool);
    }

    void ClearLastOptions()
    {
    }

    void ClearOptions()
    {
    }

    void ClearFriendlyChoices()
    {
    }

    void OnSelectedOptionsSent()
    {
    }

    void OnTimeout()
    {
    }

    void OnRealTimeCreateGame(System.Collections.Generic.List<Network.PowerHistory> powerList, int index, Network.HistCreateGame createGame)
    {
    }

    bool OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
    {
        return default(bool);
    }

    bool OnFullEntity(Network.HistFullEntity fullEntity)
    {
        return default(bool);
    }

    bool OnRealTimeShowEntity(Network.HistShowEntity showEntity)
    {
        return default(bool);
    }

    bool OnShowEntity(Network.HistShowEntity showEntity)
    {
        return default(bool);
    }

    bool OnEarlyConcedeShowEntity(Network.HistShowEntity showEntity)
    {
        return default(bool);
    }

    bool OnHideEntity(Network.HistHideEntity hideEntity)
    {
        return default(bool);
    }

    bool OnEarlyConcedeHideEntity(Network.HistHideEntity hideEntity)
    {
        return default(bool);
    }

    bool OnRealTimeChangeEntity(Network.HistChangeEntity changeEntity)
    {
        return default(bool);
    }

    bool OnChangeEntity(Network.HistChangeEntity changeEntity)
    {
        return default(bool);
    }

    bool OnEarlyConcedeChangeEntity(Network.HistChangeEntity changeEntity)
    {
        return default(bool);
    }

    bool OnRealTimeTagChange(Network.HistTagChange change)
    {
        return default(bool);
    }

    bool OnTagChange(Network.HistTagChange netChange)
    {
        return default(bool);
    }

    bool OnEarlyConcedeTagChange(Network.HistTagChange netChange)
    {
        return default(bool);
    }

    bool OnMetaData(Network.HistMetaData metaData)
    {
        return default(bool);
    }

    void OnTaskListEnded(PowerTaskList taskList)
    {
    }

    void OnPowerHistory(System.Collections.Generic.List<Network.PowerHistory> powerList)
    {
    }

    void OnReceivedEarlyConcede()
    {
    }

    void OnAllOptions(Network.Options options)
    {
    }

    void OnEntityChoices(Network.EntityChoices choices)
    {
    }

    void OnEntitiesChosen(Network.EntitiesChosen chosen)
    {
    }

    bool CanProcessEntityChoices(Network.EntityChoices choices)
    {
        return default(bool);
    }

    bool CanProcessEntitiesChosen(Network.EntitiesChosen chosen)
    {
        return default(bool);
    }

    void ProcessAllQueuedChoices()
    {
    }

    void ProcessEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
    }

    void ProcessEntitiesChosen(Network.EntitiesChosen chosen)
    {
    }

    void OnGameSetup(Network.GameSetup setup)
    {
    }

    void OnOptionRejected(int optionId)
    {
    }

    void OnTurnTimerUpdate(Network.TurnTimerInfo info)
    {
    }

    void OnSpectatorNotifyEvent(PegasusGame.SpectatorNotify notify)
    {
    }

    void SendChoices()
    {
    }

    void OnEntitiesChosenProcessed(Network.EntitiesChosen chosen)
    {
    }

    void SendOption()
    {
    }

    PlayErrors.GameStateInfo ConvertToGameStateInfo()
    {
        return default(PlayErrors.GameStateInfo);
    }

    void OnTurnChanged_TurnTimer(int oldTurn, int newTurn)
    {
    }

    void TriggerTurnTimerUpdate(TurnTimerUpdate update)
    {
    }

    void DebugPrintPowerList(System.Collections.Generic.List<Network.PowerHistory> powerList)
    {
    }

    void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power)
    {
    }

    void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power, ref string indentation)
    {
    }

    void DebugPrintTags(Logger logger, string callerName, string indentation, Network.Entity netEntity)
    {
    }

    void DebugPrintOptions()
    {
    }

    void DebugPrintEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
    }

    void DebugPrintEntitiesChosen(Network.EntitiesChosen chosen)
    {
    }

    object GetPrintableEntity(int id)
    {
        return default(object);
    }

    void PrintBlockingTaskList(System.Text.StringBuilder builder, PowerTaskList taskList)
    {
    }

    void QuickGameFlipHeroesCheat(System.Collections.Generic.List<Network.PowerHistory> powerList)
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static int DEFAULT_SUBOPTION;
    static string INDENT;
    static float BLOCK_REPORT_START_SEC;
    static float BLOCK_REPORT_INTERVAL_SEC;
    static GameState s_instance;
    //static System.Collections.Generic.List<GameState.GameStateInitializedListener> s_gameStateInitializedListeners;
    Map<int, Entity> m_entityMap;
    Map<int, Player> m_playerMap;
    GameEntity m_gameEntity;
    //GameState.CreateGamePhase m_createGamePhase;
    Network.HistTagChange m_realTimeGameOverTagChange;
    bool m_gameOver;
    bool m_concedeRequested;
    bool m_restartRequested;
    int m_maxSecretsPerPlayer;
    int m_maxFriendlyMinionsPerPlayer;
    //GameState.ResponseMode m_responseMode;
    Map<int, Network.EntityChoices> m_choicesMap;
    //System.Collections.Generic.Queue<GameState.QueuedChoice> m_queuedChoices;
    System.Collections.Generic.List<Entity> m_chosenEntities;
    Network.Options m_options;
    //GameState.SelectedOption m_selectedOption;
    Network.Options m_lastOptions;
    //GameState.SelectedOption m_lastSelectedOption;
    bool m_coinHasSpawned;
    Card m_friendlyCardBeingDrawn;
    Card m_opponentCardBeingDrawn;
    int m_lastTurnRemindedOfFullHand;
    bool m_usingFastActorTriggers;

    PowerProcessor m_powerProcessor;
    float m_blockedSec;
    float m_lastBlockedReportTimestamp;
    bool m_busy;
    bool m_mulliganBusy;
    System.Collections.Generic.List<Spell> m_serverBlockingSpells;
    System.Collections.Generic.List<SpellController> m_serverBlockingSpellControllers;
    //System.Collections.Generic.List<GameState.TurnTimerUpdateListener> m_turnTimerUpdateListeners;
    //System.Collections.Generic.List<GameState.TurnTimerUpdateListener> m_mulliganTimerUpdateListeners;
    Map<int, TurnTimerUpdate> m_turnTimerUpdates;
    #endregion

}
