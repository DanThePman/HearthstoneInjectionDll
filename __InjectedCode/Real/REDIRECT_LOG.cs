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
// - HearthstoneInjectionDll v1.0.0.0
// - Assembly-CSharp v0.0.0.0
// - mscorlib v4.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - HearthstoneInjectionDll v0.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class GameState
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power, ref string indentation)
    {
        if (Log.Power.CanPrint())
        {
            Network.Entity entity2;
            switch (power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                    {
                        Network.HistFullEntity entity = (Network.HistFullEntity)power;
                        entity2 = entity.Entity;
                        Entity entity3 = this.GetEntity(entity2.ID);
                        if (entity3 != null)
                        {
                            object[] objArray4 = new object[] { callerName, indentation, entity3, entity2.CardID };
                            logger.Print("{0}.DebugPrintPower() - {1}FULL_ENTITY - Updating {2} CardID={3}", objArray4);
                            break;
                        }
                        object[] args = new object[] { callerName, indentation, entity2.ID, entity2.CardID };
                        logger.Print("{0}.DebugPrintPower() - {1}FULL_ENTITY - Creating ID={2} CardID={3}", args);
                        break;
                    }
                case Network.PowerType.SHOW_ENTITY:
                    {
                        Network.HistShowEntity entity4 = (Network.HistShowEntity)power;
                        Network.Entity netEntity = entity4.Entity;
                        object printableEntity = this.GetPrintableEntity(netEntity.ID);
                        object[] objArray9 = new object[] { callerName, indentation, printableEntity, netEntity.CardID };
                        logger.Print("{0}.DebugPrintPower() - {1}SHOW_ENTITY - Updating Entity={2} CardID={3}", objArray9);
                        this.DebugPrintTags(logger, callerName, indentation, netEntity);
                        return;
                    }
                case Network.PowerType.HIDE_ENTITY:
                    {
                        Network.HistHideEntity entity6 = (Network.HistHideEntity)power;
                        object obj6 = this.GetPrintableEntity(entity6.Entity);
                        object[] objArray10 = new object[] { callerName, indentation, obj6, Tags.DebugTag(0x31, entity6.Zone) };
                        logger.Print("{0}.DebugPrintPower() - {1}HIDE_ENTITY - Entity={2} {3}", objArray10);
                        return;
                    }
                case Network.PowerType.TAG_CHANGE:
                    {
                        Network.HistTagChange change = (Network.HistTagChange)power;
                        object obj4 = this.GetPrintableEntity(change.Entity);
                        object[] objArray5 = new object[] { callerName, indentation, obj4, Tags.DebugTag(change.Tag, change.Value) };
                        logger.Print("{0}.DebugPrintPower() - {1}TAG_CHANGE Entity={2} {3}", objArray5);
                        return;
                    }
                case Network.PowerType.ACTION_START:
                    {
                        Network.HistActionStart start = (Network.HistActionStart)power;
                        object obj2 = this.GetPrintableEntity(start.Entity);
                        object obj3 = this.GetPrintableEntity(start.Target);
                        object[] objArray1 = new object[] { callerName, indentation, obj2, start.BlockType, start.Index, obj3 };
                        logger.Print("{0}.DebugPrintPower() - {1}ACTION_START Entity={2} BlockType={3} Index={4} Target={5}", objArray1);
                        indentation = indentation + "    ";
                        HearthstoneInjectionDll.Player.OnAction(obj2.ToString(), obj3.ToString());
                        return;
                    }
                case Network.PowerType.ACTION_END:
                    {
                        if (indentation.Length >= "    ".Length)
                        {
                            indentation = indentation.Remove(indentation.Length - "    ".Length);
                        }
                        object[] objArray2 = new object[] { callerName, indentation };
                        logger.Print("{0}.DebugPrintPower() - {1}ACTION_END", objArray2);
                        return;
                    }
                case Network.PowerType.CREATE_GAME:
                    {
                        Network.HistCreateGame game = (Network.HistCreateGame)power;
                        object[] objArray6 = new object[] { callerName, indentation };
                        logger.Print("{0}.DebugPrintPower() - {1}CREATE_GAME", objArray6);
                        indentation = indentation + "    ";
                        object[] objArray7 = new object[] { callerName, indentation, game.Game.ID };
                        logger.Print("{0}.DebugPrintPower() - {1}GameEntity EntityID={2}", objArray7);
                        this.DebugPrintTags(logger, callerName, indentation, game.Game);
                        foreach (Network.HistCreateGame.PlayerData data in game.Players)
                        {
                            object[] objArray8 = new object[] { callerName, indentation, data.Player.ID, data.ID, data.GameAccountId };
                            logger.Print("{0}.DebugPrintPower() - {1}Player EntityID={2} PlayerID={3} GameAccountId={4}", objArray8);
                            this.DebugPrintTags(logger, callerName, indentation, data.Player);
                        }
                        indentation = indentation.Remove(indentation.Length - "    ".Length);
                        return;
                    }
                case Network.PowerType.META_DATA:
                    {
                        Network.HistMetaData data2 = (Network.HistMetaData)power;
                        object obj7 = data2.Data;
                        if ((int)data2.MetaType == 3)
                        {
                            obj7 = this.GetPrintableEntity(data2.Data);
                        }
                        object[] objArray11 = new object[] { callerName, indentation, data2.MetaType, obj7, data2.Info.Count };
                        logger.Print("{0}.DebugPrintPower() - {1}META_DATA - Meta={2} Data={3} Info={4}", objArray11);
                        if ((data2.Info.Count > 0) && logger.IsVerbose())
                        {
                            indentation = indentation + "    ";
                            for (int i = 0; i < data2.Info.Count; i++)
                            {
                                int id = data2.Info[i];
                                object obj8 = this.GetPrintableEntity(id);
                                object[] objArray12 = new object[] { callerName, indentation, i, obj8 };
                                logger.Print(true, "{0}.DebugPrintPower() - {1}Info[{2}] = {3}", objArray12);
                            }
                            indentation = indentation.Remove(indentation.Length - "    ".Length);
                        }
                        return;
                    }
                default:
                    {
                        object[] objArray13 = new object[] { callerName, power.Type };
                        logger.Print("{0}.DebugPrintPower() - ERROR: unhandled PowType {1}", objArray13);
                        return;
                    }
            }
            this.DebugPrintTags(logger, callerName, indentation, entity2);
        }
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

    Network.HistTagChange GetRealTimeGameOverTagChange()
    {
        return default(Network.HistTagChange);
    }

    void ShowEnemyTauntMinions()
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

    void PreprocessPowerHistory(System.Collections.Generic.List<Network.PowerHistory> powerList)
    {
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

    bool FireEntitiesChosenReceivedEvent(Network.EntitiesChosen chosen)
    {
        return default(bool);
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

    void OnRealTimeCreateGame(Network.HistCreateGame createGame)
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

    void OnAllOptions(Options options)
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

    void OnOptionRejected(int optionId)
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

    void QuickGameFlipHeroesCheat(System.Collections.Generic.List<Network.PowerHistory> powerList)
    {
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
