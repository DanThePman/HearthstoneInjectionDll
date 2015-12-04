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
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class GameState
{
    public enum CreateGamePhase
    {
        INVALID,
        CREATING,
        CREATED
    }
    private CreateGamePhase m_createGamePhase;
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void OnRealTimeCreateGame(System.Collections.Generic.List<Network.PowerHistory> powerList, int index, Network.HistCreateGame createGame)
    {
        HearthstoneInjectionDll.Player.OnGameStart();

        this.m_gameEntity = GameMgr.Get().CreateGameEntity();
        this.m_gameEntity.SetTags(createGame.Game.Tags);
        this.AddEntity(this.m_gameEntity);
        foreach (Network.HistCreateGame.PlayerData data in createGame.Players)
        {
            Player player = new Player();
            player.InitPlayer(data);
            this.AddPlayer(player);
        }
        this.m_createGamePhase = CreateGamePhase.CREATING;
        this.FireCreateGameEvent();
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

    void Update()
    {
    }

    PowerProcessor GetPowerProcessor()
    {
        return default(PowerProcessor);
    }

    bool IsProcessingPowers()
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

    bool IsMulliganManagerActive()
    {
        return default(bool);
    }

    bool IsMulliganBlockingPowers()
    {
        return default(bool);
    }

    void SetMulliganPowerBlocker(bool on)
    {
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

    bool IsConcedingAllowed()
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

    bool IsBlockingServer()
    {
        return default(bool);
    }

    void OnSpellControllerPassedMaxWaitSec(SpellController spellController)
    {
    }

    System.Nullable<System.DateTime> GetLatestSpellControllerKillTime()
    {
        return default(System.Nullable<System.DateTime>);
    }

    bool IsBlockingServerImpl()
    {
        return default(bool);
    }

    string BuildBlockingServerCausesString()
    {
        return default(string);
    }

    void AppendBlockingServerSpell(System.Text.StringBuilder builder, Spell spell)
    {
    }

    void AppendBlockingServerSpellController(System.Text.StringBuilder builder, SpellController spellController)
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

    void FireTurnTimerUpdateEvent(TurnTimerUpdate update)
    {
    }

    void FireCurrentPlayerChangedEvent(Player player)
    {
    }

    void FireTurnChangedEvent(int oldTurn, int newTurn)
    {
    }

    void FireSpectatorNotifyEvent(PegasusGame.SpectatorNotify notify)
    {
    }

    void FireGameOverEvent()
    {
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

    Options GetOptionsPacket()
    {
        return default(Options);
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

    Option GetSelectedNetworkOption()
    {
        return default(Option);
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

    void UpdateOptionHighlights(Options options)
    {
    }

    void UpdateSubOptionHighlights(Option option)
    {
    }

    Options GetLastOptions()
    {
        return default(Options);
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

    bool OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
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

    void OnTaskListEnded(PowerTaskList taskList)
    {
    }

    void OnReceivedEarlyConcede()
    {
    }

    void OnAllOptions(Options options)
    {
    }

    void ProcessAllQueuedChoices()
    {
    }

    void OnOptionRejected(int optionId)
    {
    }

    void OnSpectatorNotifyEvent(PegasusGame.SpectatorNotify notify)
    {
    }

    void SendChoices()
    {
    }

    void SendOption()
    {
    }

    void OnTurnChanged_TurnTimer(int oldTurn, int newTurn)
    {
    }

    void TriggerTurnTimerUpdate(TurnTimerUpdate update)
    {
    }

    void DebugPrintTags(Logger logger, string callerName, string indentation, Entity netEntity)
    {
    }

    void DebugPrintOptions()
    {
    }

    object GetPrintableEntity(int id)
    {
        return default(object);
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static int DEFAULT_SUBOPTION;
    static float SPELL_CONTROLLER_MAX_WAIT_SEC;
    static string INDENT;
    static float BLOCK_SEC;
    static float BLOCK_REPORT_INTERVAL_SEC;
    static GameState s_instance;
    Map<int, Entity> m_entityMap;
    Map<int, Player> m_playerMap;
    GameEntity m_gameEntity;
    bool m_gameOver;
    bool m_concedeRequested;
    int m_maxSecretsPerPlayer;
    int m_maxFriendlyMinionsPerPlayer;
    System.Collections.Generic.List<Entity> m_chosenEntities;
    Options m_options;
    Options m_lastOptions;
    bool m_busy;
    bool m_mulliganPowerBlock;
    bool m_coinHasSpawned;
    Card m_friendlyCardBeingDrawn;
    Card m_opponentCardBeingDrawn;
    int m_lastTurnRemindedOfFullHand;
    bool m_usingFastActorTriggers;
    PowerProcessor m_powerProcessor;
    System.Collections.Generic.List<Spell> m_serverBlockingSpells;
    System.Collections.Generic.List<SpellController> m_serverBlockingSpellControllers;
    System.Nullable<System.DateTime> m_latestSpellControllerKillTime;
    float m_lastTimeUnblocked;
    float m_lastBlockedReport;
    Map<int, TurnTimerUpdate> m_turnTimerUpdates;
    #endregion

}
