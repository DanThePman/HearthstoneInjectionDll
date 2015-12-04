using PegasusGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class GameState
{
    private const float BLOCK_REPORT_INTERVAL_SEC = 3f;
    private const float BLOCK_SEC = 5f;
    public const int DEFAULT_SUBOPTION = -1;
    private const string INDENT = "    ";
    private bool m_busy;
    private Map<int, Network.EntityChoices> m_choicesMap = new Map<int, Network.EntityChoices>();
    private List<Entity> m_chosenEntities = new List<Entity>();
    private bool m_coinHasSpawned;
    private bool m_concedeRequested;
    private List<CreateGameListener> m_createGameListeners = new List<CreateGameListener>();
    private CreateGamePhase m_createGamePhase;
    private List<CurrentPlayerChangedListener> m_currentPlayerChangedListeners = new List<CurrentPlayerChangedListener>();
    private List<EntitiesChosenReceivedListener> m_entitiesChosenReceivedListeners = new List<EntitiesChosenReceivedListener>();
    private List<EntityChoicesReceivedListener> m_entityChoicesReceivedListeners = new List<EntityChoicesReceivedListener>();
    private Map<int, Entity> m_entityMap = new Map<int, Entity>();
    private Card m_friendlyCardBeingDrawn;
    private GameEntity m_gameEntity;
    private bool m_gameOver;
    private List<GameOverListener> m_gameOverListeners = new List<GameOverListener>();
    private float m_lastBlockedReport;
    private Network.Options m_lastOptions;
    private SelectedOption m_lastSelectedOption;
    private float m_lastTimeUnblocked;
    private int m_lastTurnRemindedOfFullHand;
    private DateTime? m_latestSpellControllerKillTime;
    private int m_maxFriendlyMinionsPerPlayer;
    private int m_maxSecretsPerPlayer;
    private bool m_mulliganPowerBlock;
    private List<TurnTimerUpdateListener> m_mulliganTimerUpdateListeners = new List<TurnTimerUpdateListener>();
    private Card m_opponentCardBeingDrawn;
    private Network.Options m_options;
    private List<OptionsReceivedListener> m_optionsReceivedListeners = new List<OptionsReceivedListener>();
    private Map<int, Player> m_playerMap = new Map<int, Player>();
    private PowerProcessor m_powerProcessor = new PowerProcessor();
    private Queue<QueuedChoice> m_queuedChoices = new Queue<QueuedChoice>();
    private Network.HistTagChange m_realTimeGameOverTagChange;
    private ResponseMode m_responseMode;
    private SelectedOption m_selectedOption = new SelectedOption();
    private List<SpellController> m_serverBlockingSpellControllers = new List<SpellController>();
    private List<Spell> m_serverBlockingSpells = new List<Spell>();
    private List<SpectatorNotifyListener> m_spectatorNotifyListeners = new List<SpectatorNotifyListener>();
    private List<TurnChangedListener> m_turnChangedListeners = new List<TurnChangedListener>();
    private List<TurnTimerUpdateListener> m_turnTimerUpdateListeners = new List<TurnTimerUpdateListener>();
    private Map<int, TurnTimerUpdate> m_turnTimerUpdates = new Map<int, TurnTimerUpdate>();
    private bool m_usingFastActorTriggers;
    private static List<GameStateInitializedListener> s_gameStateInitializedListeners;
    private static GameState s_instance;
    public const float SPELL_CONTROLLER_MAX_WAIT_SEC = 7f;

    public bool AddChosenEntity(Entity entity)
    {
        if (this.m_chosenEntities.Contains(entity))
        {
            return false;
        }
        this.m_chosenEntities.Add(entity);
        Card card = entity.GetCard();
        if (card != null)
        {
            card.UpdateActorState();
        }
        return true;
    }

    public void AddEntity(Entity entity)
    {
        this.m_entityMap.Add(entity.GetEntityId(), entity);
    }

    public void AddPlayer(Player player)
    {
        this.m_playerMap.Add(player.GetPlayerId(), player);
        this.m_entityMap.Add(player.GetEntityId(), player);
    }

    public void AddServerBlockingSpell(Spell spell)
    {
        if ((spell != null) && !this.m_serverBlockingSpells.Contains(spell))
        {
            this.m_serverBlockingSpells.Add(spell);
        }
    }

    public void AddServerBlockingSpellController(SpellController spellController)
    {
        if ((spellController != null) && !this.m_serverBlockingSpellControllers.Contains(spellController))
        {
            this.m_serverBlockingSpellControllers.Add(spellController);
        }
    }

    private void AppendBlockingServerSection<T>(StringBuilder builder, string sectionPrefix, List<T> items, AppendBlockingServerItemCallback<T> itemCallback, ref int sectionCount) where T: Component
    {
        if (items.Count != 0)
        {
            if (sectionCount > 0)
            {
                builder.Append(' ');
            }
            builder.Append('{');
            builder.Append(sectionPrefix);
            for (int i = 0; i < items.Count; i++)
            {
                builder.Append(' ');
                if (itemCallback == null)
                {
                    T local = items[i];
                    builder.Append(local.name);
                }
                else
                {
                    itemCallback(builder, items[i]);
                }
            }
            builder.Append('}');
            sectionCount++;
        }
    }

    private void AppendBlockingServerSpell(StringBuilder builder, Spell spell)
    {
        builder.Append('[');
        builder.Append(spell.name);
        builder.Append(' ');
        builder.AppendFormat("Source: {0}", spell.GetSource());
        builder.Append(' ');
        builder.Append("Targets:");
        List<GameObject> targets = spell.GetTargets();
        if (targets.Count == 0)
        {
            builder.Append(' ');
            builder.Append("none");
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                builder.Append(' ');
                builder.Append(targets[i].ToString());
            }
        }
        builder.Append(']');
    }

    private void AppendBlockingServerSpellController(StringBuilder builder, SpellController spellController)
    {
        builder.Append('[');
        builder.Append(spellController.name);
        builder.Append(' ');
        builder.AppendFormat("Source: {0}", spellController.GetSource());
        builder.Append(' ');
        builder.Append("Targets:");
        List<Card> targets = spellController.GetTargets();
        if (targets.Count == 0)
        {
            builder.Append(' ');
            builder.Append("none");
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                builder.Append(' ');
                builder.Append(targets[i].ToString());
            }
        }
        builder.Append(']');
    }

    private string BuildBlockingServerCausesString()
    {
        StringBuilder builder = new StringBuilder();
        int sectionCount = 0;
        this.AppendBlockingServerSection<Spell>(builder, "Spells:", this.m_serverBlockingSpells, new AppendBlockingServerItemCallback<Spell>(this.AppendBlockingServerSpell), ref sectionCount);
        this.AppendBlockingServerSection<SpellController>(builder, "SpellControllers:", this.m_serverBlockingSpellControllers, new AppendBlockingServerItemCallback<SpellController>(this.AppendBlockingServerSpellController), ref sectionCount);
        return builder.ToString();
    }

    public void CancelCurrentOptionMode()
    {
        if (this.IsInTargetMode())
        {
            this.GetGameEntity().NotifyOfTargetModeCancelled();
        }
        this.CancelSelectedOptionProposedMana();
        this.EnterMainOptionMode();
    }

    private void CancelSelectedOptionProposedMana()
    {
        Network.Options.Option selectedNetworkOption = this.GetSelectedNetworkOption();
        if (selectedNetworkOption != null)
        {
            this.GetFriendlySidePlayer().CancelAllProposedMana(this.GetEntity(selectedNetworkOption.Main.ID));
        }
    }

    private bool CanProcessEntitiesChosen(Network.EntitiesChosen chosen)
    {
        Network.EntityChoices choices;
        int playerId = chosen.PlayerId;
        if (!this.m_playerMap.ContainsKey(playerId))
        {
            return false;
        }
        foreach (int num2 in chosen.Entities)
        {
            if (!this.m_entityMap.ContainsKey(num2))
            {
                return false;
            }
        }
        if (this.m_choicesMap.TryGetValue(playerId, out choices) && (choices.ID != chosen.ID))
        {
            return false;
        }
        return true;
    }

    private bool CanProcessEntityChoices(Network.EntityChoices choices)
    {
        int playerId = choices.PlayerId;
        if (!this.m_playerMap.ContainsKey(playerId))
        {
            return false;
        }
        foreach (int num2 in choices.Entities)
        {
            if (!this.m_entityMap.ContainsKey(num2))
            {
                return false;
            }
        }
        if (this.m_choicesMap.ContainsKey(playerId))
        {
            return false;
        }
        return true;
    }

    public bool ClearCardBeingDrawn(Card card)
    {
        if (card == this.m_friendlyCardBeingDrawn)
        {
            this.m_friendlyCardBeingDrawn = null;
            return true;
        }
        if (card == this.m_opponentCardBeingDrawn)
        {
            this.m_opponentCardBeingDrawn = null;
            return true;
        }
        return false;
    }

    private void ClearFriendlyChoices()
    {
        this.m_chosenEntities.Clear();
        int friendlyPlayerId = this.GetFriendlyPlayerId();
        this.m_choicesMap.Remove(friendlyPlayerId);
    }

    private void ClearLastOptions()
    {
        this.m_lastOptions = null;
        this.m_lastSelectedOption = null;
    }

    private void ClearOptions()
    {
        this.m_options = null;
        this.m_selectedOption.Clear();
    }

    public void ClearResponseMode()
    {
        Log.Hand.Print("ClearResponseMode", new object[0]);
        this.m_responseMode = ResponseMode.NONE;
        if (this.m_options != null)
        {
            for (int i = 0; i < this.m_options.List.Count; i++)
            {
                Network.Options.Option option = this.m_options.List[i];
                if (option.Type == Network.Options.Option.OptionType.POWER)
                {
                    Entity entity = this.GetEntity(option.Main.ID);
                    if (entity != null)
                    {
                        entity.ClearBattlecryFlag();
                    }
                }
            }
            this.UpdateHighlightsBasedOnSelection();
            this.UpdateOptionHighlights(this.m_options);
        }
        else if (this.GetFriendlyEntityChoices() != null)
        {
            this.UpdateChoiceHighlights();
        }
    }

    public void Concede()
    {
        if (!this.m_concedeRequested)
        {
            this.m_concedeRequested = true;
            Network.Concede();
        }
    }

    public PlayErrors.GameStateInfo ConvertToGameStateInfo()
    {
        PlayErrors.GameStateInfo info = new PlayErrors.GameStateInfo();
        GameState state = Get();
        info.currentStep = (TAG_STEP) state.GetGameEntity().GetTag(GAME_TAG.STEP);
        return info;
    }

    public void DebugNukeServerBlocks()
    {
        while (this.m_serverBlockingSpells.Count > 0)
        {
            this.m_serverBlockingSpells[0].OnSpellFinished();
        }
        while (this.m_serverBlockingSpellControllers.Count > 0)
        {
            this.m_serverBlockingSpellControllers[0].OnFinishedTaskList();
        }
    }

    private void DebugPrintEntitiesChosen(Network.EntitiesChosen chosen)
    {
        if (Log.Power.CanPrint())
        {
            object printableEntity = this.GetPrintableEntity(this.GetPlayer(chosen.PlayerId).GetEntityId());
            object[] args = new object[] { chosen.ID, printableEntity, chosen.Entities.Count };
            Log.Power.Print("GameState.DebugPrintEntitiesChosen() - id={0} Player={1} EntitiesCount={2}", args);
            for (int i = 0; i < chosen.Entities.Count; i++)
            {
                object obj3 = this.GetPrintableEntity(chosen.Entities[i]);
                object[] objArray2 = new object[] { i, obj3 };
                Log.Power.Print("GameState.DebugPrintEntitiesChosen() -   Entities[{0}]={1}", objArray2);
            }
        }
    }

    private void DebugPrintEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
        if (Log.Power.CanPrint())
        {
            object printableEntity = this.GetPrintableEntity(this.GetPlayer(choices.PlayerId).GetEntityId());
            object id = null;
            if (preChoiceTaskList != null)
            {
                id = preChoiceTaskList.GetId();
            }
            object[] args = new object[] { choices.ID, printableEntity, id, choices.ChoiceType, choices.CountMin, choices.CountMax };
            Log.Power.Print("GameState.DebugPrintEntityChoices() - id={0} Player={1} TaskList={2} ChoiceType={3} CountMin={4} CountMax={5}", args);
            object obj4 = this.GetPrintableEntity(choices.Source);
            object[] objArray2 = new object[] { obj4 };
            Log.Power.Print("GameState.DebugPrintEntityChoices() -   Source={0}", objArray2);
            for (int i = 0; i < choices.Entities.Count; i++)
            {
                object obj5 = this.GetPrintableEntity(choices.Entities[i]);
                object[] objArray3 = new object[] { i, obj5 };
                Log.Power.Print("GameState.DebugPrintEntityChoices() -   Entities[{0}]={1}", objArray3);
            }
        }
    }

    private void DebugPrintOptions()
    {
        if (Log.Power.CanPrint())
        {
            object[] args = new object[] { this.m_options.ID };
            Log.Power.Print("GameState.DebugPrintOptions() - id={0}", args);
            for (int i = 0; i < this.m_options.List.Count; i++)
            {
                Network.Options.Option option = this.m_options.List[i];
                Entity entity = this.GetEntity(option.Main.ID);
                object[] objArray2 = new object[] { i, option.Type, entity };
                Log.Power.Print("GameState.DebugPrintOptions() -   option {0} type={1} mainEntity={2}", objArray2);
                if (option.Main.Targets != null)
                {
                    for (int k = 0; k < option.Main.Targets.Count; k++)
                    {
                        int id = option.Main.Targets[k];
                        Entity entity2 = this.GetEntity(id);
                        object[] objArray3 = new object[] { k, entity2 };
                        Log.Power.Print("GameState.DebugPrintOptions() -     target {0} entity={1}", objArray3);
                    }
                }
                for (int j = 0; j < option.Subs.Count; j++)
                {
                    Network.Options.Option.SubOption option2 = option.Subs[j];
                    Entity entity3 = this.GetEntity(option2.ID);
                    object[] objArray4 = new object[] { j, entity3 };
                    Log.Power.Print("GameState.DebugPrintOptions() -     subOption {0} entity={1}", objArray4);
                    if (option2.Targets != null)
                    {
                        for (int m = 0; m < option2.Targets.Count; m++)
                        {
                            int num6 = option2.Targets[m];
                            Entity entity4 = this.GetEntity(num6);
                            object[] objArray5 = new object[] { m, entity4 };
                            Log.Power.Print("GameState.DebugPrintOptions() -       target {0} entity={1}", objArray5);
                        }
                    }
                }
            }
        }
    }

    public void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power)
    {
        string indentation = string.Empty;
        this.DebugPrintPower(logger, callerName, power, ref indentation);
    }

    public void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power, ref string indentation)
    {
        if (Log.Power.CanPrint())
        {
            Network.Entity entity2;
            switch (power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                {
                    Network.HistFullEntity entity = (Network.HistFullEntity) power;
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
                    Network.HistShowEntity entity4 = (Network.HistShowEntity) power;
                    Network.Entity netEntity = entity4.Entity;
                    object printableEntity = this.GetPrintableEntity(netEntity.ID);
                    object[] objArray9 = new object[] { callerName, indentation, printableEntity, netEntity.CardID };
                    logger.Print("{0}.DebugPrintPower() - {1}SHOW_ENTITY - Updating Entity={2} CardID={3}", objArray9);
                    this.DebugPrintTags(logger, callerName, indentation, netEntity);
                    return;
                }
                case Network.PowerType.HIDE_ENTITY:
                {
                    Network.HistHideEntity entity6 = (Network.HistHideEntity) power;
                    object obj6 = this.GetPrintableEntity(entity6.Entity);
                    object[] objArray10 = new object[] { callerName, indentation, obj6, Tags.DebugTag(0x31, entity6.Zone) };
                    logger.Print("{0}.DebugPrintPower() - {1}HIDE_ENTITY - Entity={2} {3}", objArray10);
                    return;
                }
                case Network.PowerType.TAG_CHANGE:
                {
                    Network.HistTagChange change = (Network.HistTagChange) power;
                    object obj4 = this.GetPrintableEntity(change.Entity);
                    object[] objArray5 = new object[] { callerName, indentation, obj4, Tags.DebugTag(change.Tag, change.Value) };
                    logger.Print("{0}.DebugPrintPower() - {1}TAG_CHANGE Entity={2} {3}", objArray5);
                    return;
                }
                case Network.PowerType.ACTION_START:
                {
                    Network.HistActionStart start = (Network.HistActionStart) power;
                    object obj2 = this.GetPrintableEntity(start.Entity);
                    object obj3 = this.GetPrintableEntity(start.Target);
                    object[] objArray1 = new object[] { callerName, indentation, obj2, start.BlockType, start.Index, obj3 };
                    logger.Print("{0}.DebugPrintPower() - {1}ACTION_START Entity={2} BlockType={3} Index={4} Target={5}", objArray1);
                    indentation = indentation + "    ";
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
                    Network.HistCreateGame game = (Network.HistCreateGame) power;
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
                    Network.HistMetaData data2 = (Network.HistMetaData) power;
                    object obj7 = data2.Data;
                    if (data2.MetaType == HistoryMeta.Type.JOUST)
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

    private void DebugPrintPowerList(List<Network.PowerHistory> powerList)
    {
        if (Log.Power.CanPrint())
        {
            string indentation = string.Empty;
            Log.Power.Print(string.Format("GameState.DebugPrintPowerList() - Count={0}", powerList.Count), new object[0]);
            for (int i = 0; i < powerList.Count; i++)
            {
                Network.PowerHistory power = powerList[i];
                this.DebugPrintPower(Log.Power, "GameState", power, ref indentation);
            }
        }
    }

    private void DebugPrintTags(Logger logger, string callerName, string indentation, Network.Entity netEntity)
    {
        if (Log.Power.CanPrint())
        {
            if (indentation != null)
            {
                indentation = indentation + "    ";
            }
            for (int i = 0; i < netEntity.Tags.Count; i++)
            {
                Network.Entity.Tag tag = netEntity.Tags[i];
                object[] args = new object[] { callerName, indentation, Tags.DebugTag(tag.Name, tag.Value) };
                logger.Print("{0}.DebugPrintPower() - {1}{2}", args);
            }
        }
    }

    [Conditional("UNITY_EDITOR")]
    public void DebugSetGameEntity(GameEntity gameEntity)
    {
        this.m_gameEntity = gameEntity;
    }

    public void EnterChoiceMode()
    {
        this.m_responseMode = ResponseMode.CHOICE;
        this.UpdateOptionHighlights();
        this.UpdateChoiceHighlights();
    }

    public void EnterMainOptionMode()
    {
        ResponseMode responseMode = this.m_responseMode;
        this.m_responseMode = ResponseMode.OPTION;
        switch (responseMode)
        {
            case ResponseMode.SUB_OPTION:
            {
                Network.Options.Option option = this.m_options.List[this.m_selectedOption.m_main];
                this.UpdateSubOptionHighlights(option);
                break;
            }
            case ResponseMode.OPTION_TARGET:
            {
                Network.Options.Option option2 = this.m_options.List[this.m_selectedOption.m_main];
                this.UpdateTargetHighlights(option2.Main);
                if (this.m_selectedOption.m_sub != -1)
                {
                    Network.Options.Option.SubOption subOption = option2.Subs[this.m_selectedOption.m_sub];
                    this.UpdateTargetHighlights(subOption);
                }
                break;
            }
        }
        this.UpdateOptionHighlights(this.m_lastOptions);
        this.UpdateOptionHighlights();
        this.m_selectedOption.Clear();
    }

    public void EnterOptionTargetMode()
    {
        if (this.m_responseMode == ResponseMode.OPTION)
        {
            this.m_responseMode = ResponseMode.OPTION_TARGET;
            this.UpdateOptionHighlights();
            Network.Options.Option option = this.m_options.List[this.m_selectedOption.m_main];
            this.UpdateTargetHighlights(option.Main);
        }
        else if (this.m_responseMode == ResponseMode.SUB_OPTION)
        {
            this.m_responseMode = ResponseMode.OPTION_TARGET;
            Network.Options.Option option2 = this.m_options.List[this.m_selectedOption.m_main];
            this.UpdateSubOptionHighlights(option2);
            Network.Options.Option.SubOption subOption = option2.Subs[this.m_selectedOption.m_sub];
            this.UpdateTargetHighlights(subOption);
        }
    }

    public void EnterSubOptionMode()
    {
        Network.Options.Option option = this.m_options.List[this.m_selectedOption.m_main];
        if (this.m_responseMode == ResponseMode.OPTION)
        {
            this.m_responseMode = ResponseMode.SUB_OPTION;
            this.UpdateOptionHighlights();
        }
        else if (this.m_responseMode == ResponseMode.OPTION_TARGET)
        {
            this.m_responseMode = ResponseMode.SUB_OPTION;
            Network.Options.Option.SubOption subOption = option.Subs[this.m_selectedOption.m_sub];
            this.UpdateTargetHighlights(subOption);
        }
        this.UpdateSubOptionHighlights(option);
    }

    public bool EntityHasSubOptions(Entity entity)
    {
        int entityId = entity.GetEntityId();
        Network.Options optionsPacket = this.GetOptionsPacket();
        if (optionsPacket != null)
        {
            for (int i = 0; i < optionsPacket.List.Count; i++)
            {
                Network.Options.Option option = optionsPacket.List[i];
                if ((option.Type == Network.Options.Option.OptionType.POWER) && (option.Main.ID == entityId))
                {
                    return ((option.Subs != null) && (option.Subs.Count > 0));
                }
            }
        }
        return false;
    }

    public bool EntityHasTargets(Entity entity)
    {
        return this.EntityHasTargets(entity, false);
    }

    private bool EntityHasTargets(Entity entity, bool isSubEntity)
    {
        int entityId = entity.GetEntityId();
        Network.Options optionsPacket = this.GetOptionsPacket();
        if (optionsPacket != null)
        {
            for (int i = 0; i < optionsPacket.List.Count; i++)
            {
                Network.Options.Option option = optionsPacket.List[i];
                if (option.Type == Network.Options.Option.OptionType.POWER)
                {
                    if (isSubEntity)
                    {
                        if (option.Subs != null)
                        {
                            for (int j = 0; j < option.Subs.Count; j++)
                            {
                                Network.Options.Option.SubOption option2 = option.Subs[j];
                                if (option2.ID == entityId)
                                {
                                    return ((option2.Targets != null) && (option2.Targets.Count > 0));
                                }
                            }
                        }
                    }
                    else if (option.Main.ID == entityId)
                    {
                        return ((option.Main.Targets != null) && (option.Main.Targets.Count > 0));
                    }
                }
            }
        }
        return false;
    }

    private void FireCreateGameEvent()
    {
        foreach (CreateGameListener listener in this.m_createGameListeners.ToArray())
        {
            listener.Fire(this.m_createGamePhase);
        }
    }

    private void FireCurrentPlayerChangedEvent(Player player)
    {
        foreach (CurrentPlayerChangedListener listener in this.m_currentPlayerChangedListeners.ToArray())
        {
            listener.Fire(player);
        }
    }

    private bool FireEntitiesChosenReceivedEvent(Network.EntitiesChosen chosen)
    {
        EntitiesChosenReceivedListener[] listenerArray = this.m_entitiesChosenReceivedListeners.ToArray();
        Network.EntityChoices entityChoices = this.GetEntityChoices(chosen.PlayerId);
        bool flag = false;
        foreach (EntitiesChosenReceivedListener listener in listenerArray)
        {
            flag = listener.Fire(chosen, entityChoices) || flag;
        }
        return flag;
    }

    private void FireEntityChoicesReceivedEvent(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
        foreach (EntityChoicesReceivedListener listener in this.m_entityChoicesReceivedListeners.ToArray())
        {
            listener.Fire(choices, preChoiceTaskList);
        }
    }

    private void FireGameOverEvent()
    {
        foreach (GameOverListener listener in this.m_gameOverListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private static void FireGameStateInitializedEvent()
    {
        if (s_gameStateInitializedListeners != null)
        {
            foreach (GameStateInitializedListener listener in s_gameStateInitializedListeners.ToArray())
            {
                listener.Fire(s_instance);
            }
        }
    }

    private void FireOptionsReceivedEvent()
    {
        foreach (OptionsReceivedListener listener in this.m_optionsReceivedListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void FireSpectatorNotifyEvent(SpectatorNotify notify)
    {
        foreach (SpectatorNotifyListener listener in this.m_spectatorNotifyListeners.ToArray())
        {
            listener.Fire(notify);
        }
    }

    private void FireTurnChangedEvent(int oldTurn, int newTurn)
    {
        foreach (TurnChangedListener listener in this.m_turnChangedListeners.ToArray())
        {
            listener.Fire(oldTurn, newTurn);
        }
    }

    private void FireTurnTimerUpdateEvent(TurnTimerUpdate update)
    {
        TurnTimerUpdateListener[] listenerArray = null;
        if (this.IsMulliganManagerActive())
        {
            listenerArray = this.m_mulliganTimerUpdateListeners.ToArray();
        }
        else
        {
            listenerArray = this.m_turnTimerUpdateListeners.ToArray();
        }
        foreach (TurnTimerUpdateListener listener in listenerArray)
        {
            listener.Fire(update);
        }
    }

    public bool FriendlyHeroIsTargetable()
    {
        if (this.m_responseMode == ResponseMode.OPTION_TARGET)
        {
            Network.Options.Option option = this.m_options.List[this.m_selectedOption.m_main];
            Network.Options.Option.SubOption option2 = (this.m_selectedOption.m_sub == -1) ? option.Main : option.Subs[this.m_selectedOption.m_sub];
            foreach (int num in option2.Targets)
            {
                Entity entity = this.GetEntity(num);
                if (((entity != null) && (entity.GetCard() != null)) && (entity.IsHero() && entity.IsControlledByFriendlySidePlayer()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static GameState Get()
    {
        return s_instance;
    }

    public List<Entity> GetChosenEntities()
    {
        return this.m_chosenEntities;
    }

    public CreateGamePhase GetCreateGamePhase()
    {
        return this.m_createGamePhase;
    }

    public Player GetCurrentPlayer()
    {
        foreach (Player player in this.m_playerMap.Values)
        {
            if (player.IsCurrentPlayer())
            {
                return player;
            }
        }
        return null;
    }

    public Entity GetEntity(int id)
    {
        Entity entity;
        this.m_entityMap.TryGetValue(id, out entity);
        return entity;
    }

    public Network.EntityChoices GetEntityChoices(int playerId)
    {
        Network.EntityChoices choices;
        this.m_choicesMap.TryGetValue(playerId, out choices);
        return choices;
    }

    public Map<int, Network.EntityChoices> GetEntityChoicesMap()
    {
        return this.m_choicesMap;
    }

    public Map<int, Entity> GetEntityMap()
    {
        return this.m_entityMap;
    }

    public Player GetFirstOpponentPlayer(Player player)
    {
        foreach (Player player2 in this.m_playerMap.Values)
        {
            if (player2.GetSide() != player.GetSide())
            {
                return player2;
            }
        }
        return null;
    }

    public Card GetFriendlyCardBeingDrawn()
    {
        return this.m_friendlyCardBeingDrawn;
    }

    public Network.EntityChoices GetFriendlyEntityChoices()
    {
        int friendlyPlayerId = this.GetFriendlyPlayerId();
        return this.GetEntityChoices(friendlyPlayerId);
    }

    public int GetFriendlyPlayerId()
    {
        Player friendlySidePlayer = this.GetFriendlySidePlayer();
        if (friendlySidePlayer == null)
        {
            return 0;
        }
        return friendlySidePlayer.GetPlayerId();
    }

    public Player GetFriendlySidePlayer()
    {
        foreach (Player player in this.m_playerMap.Values)
        {
            if (player.IsFriendlySide())
            {
                return player;
            }
        }
        return null;
    }

    public GameEntity GetGameEntity()
    {
        return this.m_gameEntity;
    }

    public Network.Options GetLastOptions()
    {
        return this.m_lastOptions;
    }

    public int GetLastTurnRemindedOfFullHand()
    {
        return this.m_lastTurnRemindedOfFullHand;
    }

    public DateTime? GetLatestSpellControllerKillTime()
    {
        return this.m_latestSpellControllerKillTime;
    }

    public int GetMaxFriendlyMinionsPerPlayer()
    {
        return this.m_maxFriendlyMinionsPerPlayer;
    }

    public int GetMaxSecretsPerPlayer()
    {
        return this.m_maxSecretsPerPlayer;
    }

    public Card GetOpponentCardBeingDrawn()
    {
        return this.m_opponentCardBeingDrawn;
    }

    public Network.EntityChoices GetOpponentEntityChoices()
    {
        int opposingPlayerId = this.GetOpposingPlayerId();
        return this.GetEntityChoices(opposingPlayerId);
    }

    public int GetOpposingPlayerId()
    {
        Player opposingSidePlayer = this.GetOpposingSidePlayer();
        if (opposingSidePlayer == null)
        {
            return 0;
        }
        return opposingSidePlayer.GetPlayerId();
    }

    public Player GetOpposingSidePlayer()
    {
        foreach (Player player in this.m_playerMap.Values)
        {
            if (player.IsOpposingSide())
            {
                return player;
            }
        }
        return null;
    }

    public Network.Options GetOptionsPacket()
    {
        return this.m_options;
    }

    public Player GetPlayer(int id)
    {
        Player player;
        this.m_playerMap.TryGetValue(id, out player);
        return player;
    }

    public Map<int, Player> GetPlayerMap()
    {
        return this.m_playerMap;
    }

    public PowerProcessor GetPowerProcessor()
    {
        return this.m_powerProcessor;
    }

    public object GetPrintableEntity(int id)
    {
        Entity entity = this.GetEntity(id);
        if (entity == null)
        {
            return id;
        }
        return entity;
    }

    public Network.HistTagChange GetRealTimeGameOverTagChange()
    {
        return this.m_realTimeGameOverTagChange;
    }

    public ResponseMode GetResponseMode()
    {
        return this.m_responseMode;
    }

    public Network.Options.Option GetSelectedNetworkOption()
    {
        if (this.m_selectedOption.m_main < 0)
        {
            return null;
        }
        return this.m_options.List[this.m_selectedOption.m_main];
    }

    public Network.Options.Option.SubOption GetSelectedNetworkSubOption()
    {
        if (this.m_selectedOption.m_main < 0)
        {
            return null;
        }
        Network.Options.Option option = this.m_options.List[this.m_selectedOption.m_main];
        if (this.m_selectedOption.m_sub == -1)
        {
            return option.Main;
        }
        return option.Subs[this.m_selectedOption.m_sub];
    }

    public int GetSelectedOption()
    {
        return this.m_selectedOption.m_main;
    }

    public int GetSelectedOptionPosition()
    {
        return this.m_selectedOption.m_position;
    }

    public int GetSelectedOptionTarget()
    {
        return this.m_selectedOption.m_target;
    }

    public int GetSelectedSubOption()
    {
        return this.m_selectedOption.m_sub;
    }

    public void GetTauntCounts(Player player, out int minionCount, out int heroCount)
    {
        minionCount = 0;
        heroCount = 0;
        List<Zone> zones = ZoneMgr.Get().GetZones();
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            if ((zone.m_ServerTag == TAG_ZONE.PLAY) && (player == zone.GetController()))
            {
                List<Card> cards = zone.GetCards();
                for (int j = 0; j < cards.Count; j++)
                {
                    Entity entity = cards[j].GetEntity();
                    if (entity.HasTaunt() && !entity.IsStealthed())
                    {
                        switch (entity.GetCardType())
                        {
                            case TAG_CARDTYPE.HERO:
                                heroCount++;
                                break;

                            case TAG_CARDTYPE.MINION:
                                minionCount++;
                                break;
                        }
                    }
                }
            }
        }
    }

    public int GetTurn()
    {
        return ((this.m_gameEntity != null) ? this.m_gameEntity.GetTag(GAME_TAG.TURN) : 0);
    }

    public bool HasHandPlays()
    {
        if (this.m_options != null)
        {
            foreach (Network.Options.Option option in this.m_options.List)
            {
                if (option.Type == Network.Options.Option.OptionType.POWER)
                {
                    Entity entity = this.GetEntity(option.Main.ID);
                    if (entity != null)
                    {
                        Card card = entity.GetCard();
                        if (card != null)
                        {
                            ZoneHand zone = card.GetZone() as ZoneHand;
                            if (zone != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool HasResponse(Entity entity)
    {
        switch (this.GetResponseMode())
        {
            case ResponseMode.OPTION:
                return this.IsOption(entity);

            case ResponseMode.SUB_OPTION:
                return this.IsSubOption(entity);

            case ResponseMode.OPTION_TARGET:
                return this.IsOptionTarget(entity);

            case ResponseMode.CHOICE:
                return this.IsChoice(entity);
        }
        return false;
    }

    public bool HasSubOptions(Entity entity)
    {
        if (this.IsEntityInputEnabled(entity))
        {
            int entityId = entity.GetEntityId();
            Network.Options optionsPacket = this.GetOptionsPacket();
            for (int i = 0; i < optionsPacket.List.Count; i++)
            {
                Network.Options.Option option = optionsPacket.List[i];
                if ((option.Type == Network.Options.Option.OptionType.POWER) && (option.Main.ID == entityId))
                {
                    return (option.Subs.Count > 0);
                }
            }
        }
        return false;
    }

    public bool HasTheCoinBeenSpawned()
    {
        return this.m_coinHasSpawned;
    }

    public static GameState Initialize()
    {
        if (s_instance == null)
        {
            s_instance = new GameState();
            FireGameStateInitializedEvent();
        }
        return s_instance;
    }

    public bool IsBeginPhase()
    {
        if (this.m_gameEntity == null)
        {
            return false;
        }
        return GameUtils.IsBeginPhase(this.m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP));
    }

    public bool IsBeingDrawn(Card card)
    {
        return ((card == this.m_friendlyCardBeingDrawn) || (card == this.m_opponentCardBeingDrawn));
    }

    public bool IsBlockingServer()
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if (!this.IsBlockingServerImpl())
        {
            this.m_lastTimeUnblocked = UnityEngine.Time.realtimeSinceStartup;
            return false;
        }
        float sec = realtimeSinceStartup - this.m_lastTimeUnblocked;
        if (sec >= 5f)
        {
            float num3 = realtimeSinceStartup - this.m_lastBlockedReport;
            if (num3 < 3f)
            {
                return true;
            }
            this.m_lastBlockedReport = realtimeSinceStartup;
            string devElapsedTimeString = TimeUtils.GetDevElapsedTimeString(sec);
            string str2 = this.BuildBlockingServerCausesString();
            UnityEngine.Debug.LogWarning(string.Format("GameState.IsBlockingServer() - blocked for {0}. {1}", devElapsedTimeString, str2));
        }
        return true;
    }

    private bool IsBlockingServerImpl()
    {
        return ((this.m_serverBlockingSpells.Count > 0) || (this.m_serverBlockingSpellControllers.Count > 0));
    }

    public bool IsBusy()
    {
        return this.m_busy;
    }

    public bool IsChoice(Entity entity)
    {
        if (!this.IsEntityInputEnabled(entity))
        {
            return false;
        }
        if (!this.IsChoosableEntity(entity))
        {
            return false;
        }
        if (this.IsChosenEntity(entity))
        {
            return false;
        }
        return true;
    }

    public bool IsChoosableEntity(Entity entity)
    {
        Network.EntityChoices friendlyEntityChoices = this.GetFriendlyEntityChoices();
        if (friendlyEntityChoices == null)
        {
            return false;
        }
        return friendlyEntityChoices.Entities.Contains(entity.GetEntityId());
    }

    public bool IsChosenEntity(Entity entity)
    {
        if (this.GetFriendlyEntityChoices() == null)
        {
            return false;
        }
        return this.m_chosenEntities.Contains(entity);
    }

    public bool IsConcedingAllowed()
    {
        if (GameMgr.Get().IsTutorial())
        {
            return false;
        }
        return true;
    }

    public bool IsEntityInputEnabled(Entity entity)
    {
        if (this.IsResponsePacketBlocked())
        {
            return false;
        }
        if (entity.IsBusy())
        {
            return false;
        }
        Card card = entity.GetCard();
        if (card != null)
        {
            if (!card.IsInputEnabled())
            {
                return false;
            }
            Zone zone = card.GetZone();
            if ((zone != null) && !zone.IsInputEnabled())
            {
                return false;
            }
        }
        return true;
    }

    public bool IsFriendlySidePlayerTurn()
    {
        Player friendlySidePlayer = this.GetFriendlySidePlayer();
        if (friendlySidePlayer == null)
        {
            return false;
        }
        return friendlySidePlayer.IsCurrentPlayer();
    }

    public bool IsGameCreated()
    {
        return (this.m_createGamePhase == CreateGamePhase.CREATED);
    }

    public bool IsGameCreatedOrCreating()
    {
        return (this.IsGameCreated() || this.IsGameCreating());
    }

    public bool IsGameCreating()
    {
        return (this.m_createGamePhase == CreateGamePhase.CREATING);
    }

    public bool IsGameOver()
    {
        return this.m_gameOver;
    }

    public bool IsGameOverNowOrPending()
    {
        return (this.IsGameOver() || this.IsGameOverPending());
    }

    public bool IsGameOverPending()
    {
        return (this.m_realTimeGameOverTagChange != null);
    }

    public bool IsInChoiceMode()
    {
        return (this.m_responseMode == ResponseMode.CHOICE);
    }

    public bool IsInMainOptionMode()
    {
        return (this.m_responseMode == ResponseMode.OPTION);
    }

    public bool IsInSubOptionMode()
    {
        return (this.m_responseMode == ResponseMode.SUB_OPTION);
    }

    public bool IsInTargetMode()
    {
        return (this.m_responseMode == ResponseMode.OPTION_TARGET);
    }

    public bool IsMainPhase()
    {
        if (this.m_gameEntity == null)
        {
            return false;
        }
        return GameUtils.IsMainPhase(this.m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP));
    }

    public bool IsMulliganBlockingPowers()
    {
        return this.m_mulliganPowerBlock;
    }

    public bool IsMulliganManagerActive()
    {
        if (MulliganManager.Get() == null)
        {
            return false;
        }
        return MulliganManager.Get().IsMulliganActive();
    }

    public bool IsMulliganPhase()
    {
        if (this.m_gameEntity == null)
        {
            return false;
        }
        return (((TAG_STEP) this.m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP)) == TAG_STEP.BEGIN_MULLIGAN);
    }

    public bool IsMulliganPhaseNowOrPending()
    {
        return (this.IsMulliganPhase() || this.IsMulliganPhasePending());
    }

    public bool IsMulliganPhasePending()
    {
        <IsMulliganPhasePending>c__AnonStorey2F9 storeyf = new <IsMulliganPhasePending>c__AnonStorey2F9();
        if (this.m_gameEntity == null)
        {
            return false;
        }
        if (((TAG_STEP) this.m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.NEXT_STEP)) == TAG_STEP.BEGIN_MULLIGAN)
        {
            return true;
        }
        storeyf.foundMulliganStep = false;
        storeyf.gameEntityId = this.m_gameEntity.GetEntityId();
        this.m_powerProcessor.ForEachTaskList(new Action<int, PowerTaskList>(storeyf.<>m__F8));
        return storeyf.foundMulliganStep;
    }

    public bool IsOption(Entity entity)
    {
        if (this.IsEntityInputEnabled(entity))
        {
            int entityId = entity.GetEntityId();
            Network.Options optionsPacket = this.GetOptionsPacket();
            if (optionsPacket == null)
            {
                return false;
            }
            for (int i = 0; i < optionsPacket.List.Count; i++)
            {
                Network.Options.Option option = optionsPacket.List[i];
                if ((option.Type == Network.Options.Option.OptionType.POWER) && (option.Main.ID == entityId))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsOptionTarget(Entity entity)
    {
        if (this.IsEntityInputEnabled(entity))
        {
            int entityId = entity.GetEntityId();
            Network.Options.Option.SubOption selectedNetworkSubOption = this.GetSelectedNetworkSubOption();
            if (selectedNetworkSubOption.Targets == null)
            {
                return false;
            }
            for (int i = 0; i < selectedNetworkSubOption.Targets.Count; i++)
            {
                int num3 = selectedNetworkSubOption.Targets[i];
                if (num3 == entityId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsPastBeginPhase()
    {
        if (this.m_gameEntity == null)
        {
            return false;
        }
        return GameUtils.IsPastBeginPhase(this.m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP));
    }

    public bool IsProcessingPowers()
    {
        return this.m_powerProcessor.IsBusy();
    }

    public bool IsResponsePacketBlocked()
    {
        if (!this.IsMulliganManagerActive())
        {
            if (!this.IsFriendlySidePlayerTurn())
            {
                return true;
            }
            if (this.IsTurnStartManagerBlockingInput())
            {
                return true;
            }
            if (EndTurnButton.Get().IsInWaitingState())
            {
                return true;
            }
            switch (this.m_responseMode)
            {
                case ResponseMode.NONE:
                    return true;

                case ResponseMode.OPTION:
                case ResponseMode.SUB_OPTION:
                case ResponseMode.OPTION_TARGET:
                    if (this.m_options != null)
                    {
                        break;
                    }
                    return true;

                case ResponseMode.CHOICE:
                    if (this.GetFriendlyEntityChoices() != null)
                    {
                        break;
                    }
                    return true;

                default:
                    UnityEngine.Debug.LogWarning(string.Format("GameState.IsResponsePacketBlocked() - unhandled response mode {0}", this.m_responseMode));
                    break;
            }
        }
        return false;
    }

    public bool IsSelectedOptionFriendlyHero()
    {
        Entity hero = this.GetFriendlySidePlayer().GetHero();
        Network.Options.Option selectedNetworkOption = this.GetSelectedNetworkOption();
        return ((selectedNetworkOption != null) && (selectedNetworkOption.Main.ID == hero.GetEntityId()));
    }

    public bool IsSubOption(Entity entity)
    {
        if (this.IsEntityInputEnabled(entity))
        {
            int entityId = entity.GetEntityId();
            Network.Options.Option selectedNetworkOption = this.GetSelectedNetworkOption();
            for (int i = 0; i < selectedNetworkOption.Subs.Count; i++)
            {
                Network.Options.Option.SubOption option2 = selectedNetworkOption.Subs[i];
                if (option2.ID == entityId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsTurnStartManagerActive()
    {
        if (TurnStartManager.Get() == null)
        {
            return false;
        }
        return TurnStartManager.Get().IsListeningForTurnEvents();
    }

    public bool IsTurnStartManagerBlockingInput()
    {
        if (TurnStartManager.Get() == null)
        {
            return false;
        }
        return TurnStartManager.Get().IsBlockingInput();
    }

    public bool IsUsingFastActorTriggers()
    {
        return this.m_usingFastActorTriggers;
    }

    public bool IsValidOptionTarget(Entity entity)
    {
        Network.Options.Option.SubOption selectedNetworkSubOption = this.GetSelectedNetworkSubOption();
        return (((selectedNetworkSubOption != null) && (selectedNetworkSubOption.Targets != null)) && selectedNetworkSubOption.Targets.Contains(entity.GetEntityId()));
    }

    public void NotifyOfCoinSpawn()
    {
        this.m_coinHasSpawned = true;
    }

    public void OnAllOptions(Network.Options options)
    {
        this.m_responseMode = ResponseMode.OPTION;
        this.m_chosenEntities.Clear();
        if ((this.m_options != null) && ((this.m_lastOptions == null) || (this.m_lastOptions.ID < this.m_options.ID)))
        {
            this.m_lastOptions = new Network.Options();
            this.m_lastOptions.CopyFrom(this.m_options);
        }
        this.m_options = options;
        foreach (Network.Options.Option option in this.m_options.List)
        {
            if (option.Type == Network.Options.Option.OptionType.POWER)
            {
                Entity entity = this.GetEntity(option.Main.ID);
                if ((entity != null) && ((option.Main.Targets != null) && (option.Main.Targets.Count > 0)))
                {
                    entity.UpdateUseBattlecryFlag(true);
                }
            }
        }
        this.DebugPrintOptions();
        this.EnterMainOptionMode();
        this.FireOptionsReceivedEvent();
    }

    private void OnCurrentPlayerChanged(Player player)
    {
        this.FireCurrentPlayerChangedEvent(player);
    }

    public bool OnEarlyConcedeHideEntity(Network.HistHideEntity hideEntity)
    {
        Entity entity = this.GetEntity(hideEntity.Entity);
        if (entity == null)
        {
            object[] args = new object[] { hideEntity.Entity, hideEntity.Zone };
            UnityEngine.Debug.LogWarningFormat("GameState.OnEarlyConcedeHideEntity() - WARNING entity {0} DOES NOT EXIST! zone={1}", args);
            return false;
        }
        entity.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
        return true;
    }

    public bool OnEarlyConcedeShowEntity(Network.HistShowEntity showEntity)
    {
        Network.Entity entity = showEntity.Entity;
        Entity entity2 = this.GetEntity(entity.ID);
        if (entity2 == null)
        {
            object[] args = new object[] { entity.ID };
            UnityEngine.Debug.LogWarningFormat("GameState.OnEarlyConcedeShowEntity() - WARNING entity {0} DOES NOT EXIST!", args);
            return false;
        }
        entity2.SetTags(entity.Tags);
        return true;
    }

    public bool OnEarlyConcedeTagChange(Network.HistTagChange netChange)
    {
        Entity entity = this.GetEntity(netChange.Entity);
        if (entity == null)
        {
            object[] args = new object[] { netChange.Entity };
            UnityEngine.Debug.LogWarningFormat("GameState.OnEarlyConcedeTagChange() - WARNING Entity {0} does not exist", args);
            return false;
        }
        TagDelta change = new TagDelta {
            tag = netChange.Tag,
            oldValue = entity.GetTag(netChange.Tag),
            newValue = netChange.Value
        };
        entity.SetTag(change.tag, change.newValue);
        this.PreprocessEarlyConcedeTagChange(entity, change);
        this.ProcessEarlyConcedeTagChange(entity, change);
        return true;
    }

    public void OnEntitiesChosen(Network.EntitiesChosen chosen)
    {
        if (!this.CanProcessEntitiesChosen(chosen))
        {
            object[] args = new object[] { chosen.ID, chosen.PlayerId };
            Log.Power.Print("GameState.OnEntitiesChosen() - id={0} playerId={1} queued", args);
            QueuedChoice item = new QueuedChoice {
                m_type = QueuedChoice.PacketType.ENTITIES_CHOSEN,
                m_packet = chosen
            };
            this.m_queuedChoices.Enqueue(item);
        }
        else
        {
            this.ProcessEntitiesChosen(chosen);
        }
    }

    public void OnEntitiesChosenProcessed(Network.EntitiesChosen chosen)
    {
        int playerId = chosen.PlayerId;
        int friendlyPlayerId = this.GetFriendlyPlayerId();
        if (playerId == friendlyPlayerId)
        {
            this.ClearResponseMode();
            this.ClearFriendlyChoices();
        }
        else
        {
            this.m_choicesMap.Remove(playerId);
        }
        this.ProcessAllQueuedChoices();
    }

    public void OnEntityChoices(Network.EntityChoices choices)
    {
        PowerTaskList lastTaskList = this.m_powerProcessor.GetLastTaskList();
        if (!this.CanProcessEntityChoices(choices))
        {
            object[] args = new object[] { choices.ID, choices.PlayerId };
            Log.Power.Print("GameState.OnEntityChoices() - id={0} playerId={1} queued", args);
            QueuedChoice item = new QueuedChoice {
                m_packet = choices,
                m_eventData = lastTaskList
            };
            this.m_queuedChoices.Enqueue(item);
        }
        else
        {
            this.ProcessEntityChoices(choices, lastTaskList);
        }
    }

    public bool OnFullEntity(Network.HistFullEntity fullEntity)
    {
        Network.Entity entity = fullEntity.Entity;
        Entity entity2 = this.GetEntity(entity.ID);
        if (entity2 == null)
        {
            object[] args = new object[] { entity.ID };
            UnityEngine.Debug.LogWarningFormat("GameState.OnFullEntity() - WARNING entity {0} DOES NOT EXIST!", args);
            return false;
        }
        entity2.OnFullEntity(fullEntity);
        return true;
    }

    private void OnGameOver(TAG_PLAYSTATE playState)
    {
        this.m_gameOver = true;
        this.m_realTimeGameOverTagChange = null;
        this.m_gameEntity.NotifyOfGameOver(playState);
        this.FireGameOverEvent();
    }

    public void OnGameSetup(Network.GameSetup setup)
    {
        this.m_maxSecretsPerPlayer = setup.MaxSecretsPerPlayer;
        this.m_maxFriendlyMinionsPerPlayer = setup.MaxFriendlyMinionsPerPlayer;
    }

    public bool OnHideEntity(Network.HistHideEntity hideEntity)
    {
        Entity entity = this.GetEntity(hideEntity.Entity);
        if (entity == null)
        {
            object[] args = new object[] { hideEntity.Entity, hideEntity.Zone };
            UnityEngine.Debug.LogWarningFormat("GameState.OnHideEntity() - WARNING entity {0} DOES NOT EXIST! zone={1}", args);
            return false;
        }
        entity.SetTagAndHandleChange<int>(GAME_TAG.ZONE, hideEntity.Zone);
        return true;
    }

    public bool OnMetaData(Network.HistMetaData metaData)
    {
        foreach (int num in metaData.Info)
        {
            Entity entity = this.GetEntity(num);
            if (entity == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("GameState.OnMetaData() - WARNING Entity {0} does not exist", num));
                return false;
            }
            entity.OnMetaData(metaData);
        }
        return true;
    }

    public void OnOptionRejected(int optionId)
    {
        if (this.m_lastSelectedOption == null)
        {
            UnityEngine.Debug.LogError("GameState.OnOptionRejected() - got an option rejection without a last selected option");
        }
        else if (this.m_lastOptions.ID != optionId)
        {
            UnityEngine.Debug.LogError(string.Format("GameState.OnOptionRejected() - rejected option id ({0}) does not match last option id ({1})", optionId, this.m_lastOptions.ID));
        }
        else
        {
            Network.Options.Option option = this.m_lastOptions.List[this.m_lastSelectedOption.m_main];
            if (option.Type == Network.Options.Option.OptionType.POWER)
            {
                Entity triggerEntity = this.GetEntity(option.Main.ID);
                triggerEntity.GetCard().NotifyTargetingCanceled();
                ZoneMgr.Get().OnLocalZoneChangeRejected(triggerEntity);
                InputManager.Get().ReverseManaUpdate(triggerEntity);
            }
            string message = GameStrings.Get("GAMEPLAY_ERROR_PLAY_REJECTED");
            GameplayErrorManager.Get().DisplayMessage(message);
            this.ClearLastOptions();
        }
    }

    public void OnPowerHistory(List<Network.PowerHistory> powerList)
    {
        this.DebugPrintPowerList(powerList);
        bool flag = this.m_powerProcessor.HasEarlyConcedeTaskList();
        this.m_powerProcessor.OnPowerHistory(powerList);
        this.ProcessAllQueuedChoices();
        bool flag2 = this.m_powerProcessor.HasEarlyConcedeTaskList();
        if (!flag && flag2)
        {
            this.OnReceivedEarlyConcede();
        }
    }

    public void OnRealTimeCreateGame(List<Network.PowerHistory> powerList, int index, Network.HistCreateGame createGame)
    {
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

    public bool OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
    {
        Entity entity = new Entity();
        entity.OnRealTimeFullEntity(fullEntity);
        this.AddEntity(entity);
        return true;
    }

    private void OnRealTimeGameOver(Network.HistTagChange change)
    {
        this.m_realTimeGameOverTagChange = change;
        if (Network.ShouldBeConnectedToAurora())
        {
            BnetPresenceMgr.Get().SetGameFieldBlob(0x15, null);
        }
        SpectatorManager.Get().OnRealTimeGameOver();
    }

    public bool OnRealTimeShowEntity(Network.HistShowEntity showEntity)
    {
        Entity entity = this.GetEntity(showEntity.Entity.ID);
        if (entity == null)
        {
            return false;
        }
        entity.OnRealTimeShowEntity(showEntity);
        return true;
    }

    public bool OnRealTimeTagChange(Network.HistTagChange change)
    {
        Entity entity = Get().GetEntity(change.Entity);
        if (entity == null)
        {
            return false;
        }
        this.PreprocessRealTimeTagChange(entity, change);
        entity.OnRealTimeTagChanged(change);
        return true;
    }

    private void OnReceivedEarlyConcede()
    {
        this.ClearResponseMode();
        this.ClearLastOptions();
        this.ClearOptions();
    }

    private void OnSelectedOptionsSent()
    {
        this.ClearResponseMode();
        this.m_lastOptions = new Network.Options();
        this.m_lastOptions.CopyFrom(this.m_options);
        this.m_lastSelectedOption = new SelectedOption();
        this.m_lastSelectedOption.CopyFrom(this.m_selectedOption);
        this.ClearOptions();
    }

    public bool OnShowEntity(Network.HistShowEntity showEntity)
    {
        Network.Entity entity = showEntity.Entity;
        Entity entity2 = this.GetEntity(entity.ID);
        if (entity2 == null)
        {
            object[] args = new object[] { entity.ID };
            UnityEngine.Debug.LogWarningFormat("GameState.OnShowEntity() - WARNING entity {0} DOES NOT EXIST!", args);
            return false;
        }
        entity2.OnShowEntity(showEntity);
        return true;
    }

    public void OnSpectatorNotifyEvent(SpectatorNotify notify)
    {
        this.FireSpectatorNotifyEvent(notify);
    }

    public void OnSpellControllerPassedMaxWaitSec(SpellController spellController)
    {
        this.m_latestSpellControllerKillTime = new DateTime?(DateTime.Now);
        PowerTaskList powerTaskList = spellController.GetPowerTaskList();
        if (powerTaskList != null)
        {
            powerTaskList.DebugDump();
        }
    }

    public bool OnTagChange(Network.HistTagChange netChange)
    {
        Entity entity = this.GetEntity(netChange.Entity);
        if (entity == null)
        {
            object[] args = new object[] { netChange.Entity };
            UnityEngine.Debug.LogWarningFormat("GameState.OnTagChange() - WARNING Entity {0} does not exist", args);
            return false;
        }
        TagDelta change = new TagDelta {
            tag = netChange.Tag,
            oldValue = entity.GetTag(netChange.Tag),
            newValue = netChange.Value
        };
        entity.SetTag(change.tag, change.newValue);
        this.PreprocessTagChange(entity, change);
        entity.OnTagChanged(change);
        return true;
    }

    public void OnTaskListEnded(PowerTaskList taskList)
    {
        if (taskList != null)
        {
            foreach (PowerTask task in taskList.GetTaskList())
            {
                if (task.GetPower().Type == Network.PowerType.CREATE_GAME)
                {
                    this.m_createGamePhase = CreateGamePhase.CREATED;
                    this.FireCreateGameEvent();
                    this.m_createGameListeners.Clear();
                }
            }
        }
    }

    private void OnTimeout()
    {
        if (this.m_responseMode != ResponseMode.NONE)
        {
            this.ClearResponseMode();
            this.ClearLastOptions();
            this.ClearOptions();
        }
    }

    private void OnTurnChanged(int oldTurn, int newTurn)
    {
        this.OnTurnChanged_TurnTimer(oldTurn, newTurn);
        this.FireTurnChangedEvent(oldTurn, newTurn);
    }

    private void OnTurnChanged_TurnTimer(int oldTurn, int newTurn)
    {
        TurnTimerUpdate update;
        if ((this.m_turnTimerUpdates.Count != 0) && this.m_turnTimerUpdates.TryGetValue(newTurn, out update))
        {
            float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            float endTimestamp = update.GetEndTimestamp();
            float sec = Mathf.Max((float) 0f, (float) (endTimestamp - realtimeSinceStartup));
            update.SetSecondsRemaining(sec);
            this.TriggerTurnTimerUpdate(update);
            this.m_turnTimerUpdates.Remove(newTurn);
        }
    }

    public void OnTurnTimerUpdate(Network.TurnTimerInfo info)
    {
        TurnTimerUpdate update = new TurnTimerUpdate();
        update.SetSecondsRemaining(info.Seconds);
        update.SetEndTimestamp(UnityEngine.Time.realtimeSinceStartup + info.Seconds);
        update.SetShow(info.Show);
        int num = (this.m_gameEntity != null) ? this.m_gameEntity.GetTag(GAME_TAG.TURN) : 0;
        if (info.Turn > num)
        {
            this.m_turnTimerUpdates[info.Turn] = update;
        }
        else
        {
            this.TriggerTurnTimerUpdate(update);
        }
    }

    private void PreprocessEarlyConcedeTagChange(Entity entity, TagDelta change)
    {
        if (change.tag == 0x11)
        {
            Player player = (Player) entity;
            if (GameUtils.IsGameOverTag(player, change.tag, change.newValue))
            {
                this.OnGameOver((TAG_PLAYSTATE) change.newValue);
            }
        }
    }

    private void PreprocessRealTimeTagChange(Entity entity, Network.HistTagChange change)
    {
        if ((change.Tag == 0x11) && GameUtils.IsGameOverTag(change.Entity, change.Tag, change.Value))
        {
            this.OnRealTimeGameOver(change);
        }
    }

    private void PreprocessTagChange(Entity entity, TagDelta change)
    {
        GAME_TAG tag = (GAME_TAG) change.tag;
        switch (tag)
        {
            case GAME_TAG.PLAYSTATE:
            {
                Player player2 = (Player) entity;
                if (GameUtils.IsGameOverTag(player2, change.tag, change.newValue))
                {
                    this.OnGameOver((TAG_PLAYSTATE) change.newValue);
                }
                return;
            }
            case GAME_TAG.TURN:
                this.OnTurnChanged(change.oldValue, change.newValue);
                return;
        }
        if ((tag == GAME_TAG.CURRENT_PLAYER) && (change.newValue == 1))
        {
            Player player = (Player) entity;
            this.OnCurrentPlayerChanged(player);
        }
    }

    private void ProcessAllQueuedChoices()
    {
        while (this.m_queuedChoices.Count > 0)
        {
            QueuedChoice choice = this.m_queuedChoices.Peek();
            switch (choice.m_type)
            {
                case QueuedChoice.PacketType.ENTITY_CHOICES:
                {
                    Network.EntityChoices packet = (Network.EntityChoices) choice.m_packet;
                    if (!this.CanProcessEntityChoices(packet))
                    {
                        return;
                    }
                    this.m_queuedChoices.Dequeue();
                    PowerTaskList eventData = (PowerTaskList) choice.m_eventData;
                    this.ProcessEntityChoices(packet, eventData);
                    break;
                }
                case QueuedChoice.PacketType.ENTITIES_CHOSEN:
                {
                    Network.EntitiesChosen chosen = (Network.EntitiesChosen) choice.m_packet;
                    if (!this.CanProcessEntitiesChosen(chosen))
                    {
                        return;
                    }
                    this.m_queuedChoices.Dequeue();
                    this.ProcessEntitiesChosen(chosen);
                    continue;
                }
            }
        }
    }

    private void ProcessEarlyConcedeTagChange(Entity entity, TagDelta change)
    {
        if (change.tag == 0x11)
        {
            entity.OnTagChanged(change);
        }
    }

    private void ProcessEntitiesChosen(Network.EntitiesChosen chosen)
    {
        this.DebugPrintEntitiesChosen(chosen);
        if (!this.m_powerProcessor.HasEarlyConcedeTaskList() && !this.FireEntitiesChosenReceivedEvent(chosen))
        {
            this.OnEntitiesChosenProcessed(chosen);
        }
    }

    private void ProcessEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
    {
        this.DebugPrintEntityChoices(choices, preChoiceTaskList);
        if (!this.m_powerProcessor.HasEarlyConcedeTaskList())
        {
            int playerId = choices.PlayerId;
            this.m_choicesMap[playerId] = choices;
            int friendlyPlayerId = this.GetFriendlyPlayerId();
            if (playerId == friendlyPlayerId)
            {
                this.m_responseMode = ResponseMode.CHOICE;
                this.m_chosenEntities.Clear();
                this.EnterChoiceMode();
            }
            this.FireEntityChoicesReceivedEvent(choices, preChoiceTaskList);
        }
    }

    private void QuickGameFlipHeroesCheat(List<Network.PowerHistory> powerList)
    {
    }

    public bool RegisterCreateGameListener(CreateGameCallback callback)
    {
        return this.RegisterCreateGameListener(callback, null);
    }

    public bool RegisterCreateGameListener(CreateGameCallback callback, object userData)
    {
        CreateGameListener item = new CreateGameListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_createGameListeners.Contains(item))
        {
            return false;
        }
        this.m_createGameListeners.Add(item);
        return true;
    }

    public bool RegisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback)
    {
        return this.RegisterCurrentPlayerChangedListener(callback, null);
    }

    public bool RegisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback, object userData)
    {
        CurrentPlayerChangedListener item = new CurrentPlayerChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_currentPlayerChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_currentPlayerChangedListeners.Add(item);
        return true;
    }

    public bool RegisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback)
    {
        return this.RegisterEntitiesChosenReceivedListener(callback, null);
    }

    public bool RegisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback, object userData)
    {
        EntitiesChosenReceivedListener item = new EntitiesChosenReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_entitiesChosenReceivedListeners.Contains(item))
        {
            return false;
        }
        this.m_entitiesChosenReceivedListeners.Add(item);
        return true;
    }

    public bool RegisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback)
    {
        return this.RegisterEntityChoicesReceivedListener(callback, null);
    }

    public bool RegisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback, object userData)
    {
        EntityChoicesReceivedListener item = new EntityChoicesReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_entityChoicesReceivedListeners.Contains(item))
        {
            return false;
        }
        this.m_entityChoicesReceivedListeners.Add(item);
        return true;
    }

    public bool RegisterGameOverListener(GameOverCallback callback, object userData = null)
    {
        GameOverListener item = new GameOverListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_gameOverListeners.Contains(item))
        {
            return false;
        }
        this.m_gameOverListeners.Add(item);
        return true;
    }

    public static bool RegisterGameStateInitializedListener(GameStateInitializedCallback callback, object userData = null)
    {
        if (callback == null)
        {
            return false;
        }
        GameStateInitializedListener item = new GameStateInitializedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (s_gameStateInitializedListeners == null)
        {
            s_gameStateInitializedListeners = new List<GameStateInitializedListener>();
        }
        else if (s_gameStateInitializedListeners.Contains(item))
        {
            return false;
        }
        s_gameStateInitializedListeners.Add(item);
        return true;
    }

    public bool RegisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback)
    {
        return this.RegisterMulliganTimerUpdateListener(callback, null);
    }

    public bool RegisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
    {
        TurnTimerUpdateListener item = new TurnTimerUpdateListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_mulliganTimerUpdateListeners.Contains(item))
        {
            return false;
        }
        this.m_mulliganTimerUpdateListeners.Add(item);
        return true;
    }

    public bool RegisterOptionsReceivedListener(OptionsReceivedCallback callback)
    {
        return this.RegisterOptionsReceivedListener(callback, null);
    }

    public bool RegisterOptionsReceivedListener(OptionsReceivedCallback callback, object userData)
    {
        OptionsReceivedListener item = new OptionsReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_optionsReceivedListeners.Contains(item))
        {
            return false;
        }
        this.m_optionsReceivedListeners.Add(item);
        return true;
    }

    public bool RegisterSpectatorNotifyListener(SpectatorNotifyEventCallback callback, object userData = null)
    {
        SpectatorNotifyListener item = new SpectatorNotifyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_spectatorNotifyListeners.Contains(item))
        {
            return false;
        }
        this.m_spectatorNotifyListeners.Add(item);
        return true;
    }

    public bool RegisterTurnChangedListener(TurnChangedCallback callback)
    {
        return this.RegisterTurnChangedListener(callback, null);
    }

    public bool RegisterTurnChangedListener(TurnChangedCallback callback, object userData)
    {
        TurnChangedListener item = new TurnChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_turnChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_turnChangedListeners.Add(item);
        return true;
    }

    public bool RegisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback)
    {
        return this.RegisterTurnTimerUpdateListener(callback, null);
    }

    public bool RegisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
    {
        TurnTimerUpdateListener item = new TurnTimerUpdateListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_turnTimerUpdateListeners.Contains(item))
        {
            return false;
        }
        this.m_turnTimerUpdateListeners.Add(item);
        return true;
    }

    public bool RemoveChosenEntity(Entity entity)
    {
        if (!this.m_chosenEntities.Remove(entity))
        {
            return false;
        }
        Card card = entity.GetCard();
        if (card != null)
        {
            card.UpdateActorState();
        }
        return true;
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.IsPlayer())
        {
            this.RemovePlayer(entity as Player);
        }
        else if (entity.IsGame())
        {
            this.m_gameEntity = null;
        }
        else
        {
            if (entity.IsAttached())
            {
                Entity entity2 = this.GetEntity(entity.GetAttached());
                if (entity2 != null)
                {
                    entity2.RemoveAttachment(entity);
                }
            }
            if (entity.IsHero())
            {
                Player player = this.GetPlayer(entity.GetControllerId());
                if (player != null)
                {
                    player.SetHero(null);
                }
            }
            else if (entity.IsHeroPower())
            {
                Player player2 = this.GetPlayer(entity.GetControllerId());
                if (player2 != null)
                {
                    player2.SetHeroPower(null);
                }
            }
            entity.Destroy();
        }
    }

    public void RemovePlayer(Player player)
    {
        player.Destroy();
        this.m_playerMap.Remove(player.GetPlayerId());
        this.m_entityMap.Remove(player.GetEntityId());
    }

    public bool RemoveServerBlockingSpell(Spell spell)
    {
        return this.m_serverBlockingSpells.Remove(spell);
    }

    public bool RemoveServerBlockingSpellController(SpellController spellController)
    {
        return this.m_serverBlockingSpellControllers.Remove(spellController);
    }

    public void SendChoices()
    {
        if (this.m_responseMode == ResponseMode.CHOICE)
        {
            Network.EntityChoices friendlyEntityChoices = this.GetFriendlyEntityChoices();
            if (friendlyEntityChoices != null)
            {
                object[] args = new object[] { friendlyEntityChoices.ID, friendlyEntityChoices.ChoiceType };
                Log.Power.Print("GameState.SendChoices() - id={0} ChoiceType={1}", args);
                List<int> picks = new List<int>();
                for (int i = 0; i < this.m_chosenEntities.Count; i++)
                {
                    Entity entity = this.m_chosenEntities[i];
                    int entityId = entity.GetEntityId();
                    object[] objArray2 = new object[] { i, entity };
                    Log.Power.Print("GameState.SendChoices() -   m_chosenEntities[{0}]={1}", objArray2);
                    picks.Add(entityId);
                }
                if (!GameMgr.Get().IsSpectator())
                {
                    Network.Get().SendChoices(friendlyEntityChoices.ID, picks);
                }
                this.ClearResponseMode();
                this.ClearFriendlyChoices();
                this.ProcessAllQueuedChoices();
            }
        }
    }

    public void SendOption()
    {
        if (!GameMgr.Get().IsSpectator())
        {
            Network.Get().SendOption(this.m_options.ID, this.m_selectedOption.m_main, this.m_selectedOption.m_target, this.m_selectedOption.m_sub, this.m_selectedOption.m_position);
            object[] args = new object[] { this.m_selectedOption.m_main, this.m_selectedOption.m_sub, this.m_selectedOption.m_target, this.m_selectedOption.m_position };
            Log.Power.Print("GameState.SendOption() - selectedOption={0} selectedSubOption={1} selectedTarget={2} selectedPosition={3}", args);
        }
        this.CancelSelectedOptionProposedMana();
        this.OnSelectedOptionsSent();
    }

    public void SetBusy(bool busy)
    {
        this.m_busy = busy;
    }

    public void SetChosenEntities(ChooseEntities packet)
    {
        this.m_chosenEntities.Clear();
        foreach (int num in packet.Entities)
        {
            Entity item = this.GetEntity(num);
            if (item != null)
            {
                this.m_chosenEntities.Add(item);
            }
        }
    }

    public void SetFriendlyCardBeingDrawn(Card card)
    {
        this.m_friendlyCardBeingDrawn = card;
    }

    public void SetLastTurnRemindedOfFullHand(int turn)
    {
        this.m_lastTurnRemindedOfFullHand = turn;
    }

    public void SetMulliganPowerBlocker(bool on)
    {
        this.m_mulliganPowerBlock = on;
    }

    public void SetOpponentCardBeingDrawn(Card card)
    {
        this.m_opponentCardBeingDrawn = card;
    }

    public void SetSelectedOption(ChooseOption packet)
    {
        this.m_selectedOption.m_main = packet.Index;
        this.m_selectedOption.m_sub = packet.SubOption;
        this.m_selectedOption.m_target = packet.Target;
        this.m_selectedOption.m_position = packet.Position;
    }

    public void SetSelectedOption(int index)
    {
        this.m_selectedOption.m_main = index;
    }

    public void SetSelectedOptionPosition(int position)
    {
        this.m_selectedOption.m_position = position;
    }

    public void SetSelectedOptionTarget(int target)
    {
        this.m_selectedOption.m_target = target;
    }

    public void SetSelectedSubOption(int index)
    {
        this.m_selectedOption.m_sub = index;
    }

    public void SetUsingFastActorTriggers(bool enable)
    {
        this.m_usingFastActorTriggers = enable;
    }

    public void ShowEnemyTauntCharacters()
    {
        List<Zone> zones = ZoneMgr.Get().GetZones();
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            if ((zone.m_ServerTag == TAG_ZONE.PLAY) && (zone.m_Side == Player.Side.OPPOSING))
            {
                List<Card> cards = zone.GetCards();
                for (int j = 0; j < cards.Count; j++)
                {
                    Card card = cards[j];
                    Entity entity = card.GetEntity();
                    if (entity.HasTaunt() && !entity.IsStealthed())
                    {
                        card.DoTauntNotification();
                    }
                }
            }
        }
    }

    public static void Shutdown()
    {
        if (s_instance != null)
        {
            s_instance = null;
        }
    }

    public bool SubEntityHasTargets(Entity subEntity)
    {
        return this.EntityHasTargets(subEntity, true);
    }

    private void TriggerTurnTimerUpdate(TurnTimerUpdate update)
    {
        this.FireTurnTimerUpdateEvent(update);
        if (update.GetSecondsRemaining() <= Mathf.Epsilon)
        {
            this.OnTimeout();
        }
    }

    public bool UnregisterCreateGameListener(CreateGameCallback callback)
    {
        return this.UnregisterCreateGameListener(callback, null);
    }

    public bool UnregisterCreateGameListener(CreateGameCallback callback, object userData)
    {
        CreateGameListener item = new CreateGameListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_createGameListeners.Remove(item);
    }

    public bool UnregisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback)
    {
        return this.UnregisterCurrentPlayerChangedListener(callback, null);
    }

    public bool UnregisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback, object userData)
    {
        CurrentPlayerChangedListener item = new CurrentPlayerChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_currentPlayerChangedListeners.Remove(item);
    }

    public bool UnregisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback)
    {
        return this.UnregisterEntitiesChosenReceivedListener(callback, null);
    }

    public bool UnregisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback, object userData)
    {
        EntitiesChosenReceivedListener item = new EntitiesChosenReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_entitiesChosenReceivedListeners.Remove(item);
    }

    public bool UnregisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback)
    {
        return this.UnregisterEntityChoicesReceivedListener(callback, null);
    }

    public bool UnregisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback, object userData)
    {
        EntityChoicesReceivedListener item = new EntityChoicesReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_entityChoicesReceivedListeners.Remove(item);
    }

    public bool UnregisterGameOverListener(GameOverCallback callback, object userData = null)
    {
        GameOverListener item = new GameOverListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_gameOverListeners.Remove(item);
    }

    public static bool UnregisterGameStateInitializedListener(GameStateInitializedCallback callback, object userData = null)
    {
        if ((callback == null) || (s_gameStateInitializedListeners == null))
        {
            return false;
        }
        GameStateInitializedListener item = new GameStateInitializedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return s_gameStateInitializedListeners.Remove(item);
    }

    public bool UnregisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback)
    {
        return this.UnregisterMulliganTimerUpdateListener(callback, null);
    }

    public bool UnregisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
    {
        TurnTimerUpdateListener item = new TurnTimerUpdateListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_mulliganTimerUpdateListeners.Remove(item);
    }

    public bool UnregisterOptionsReceivedListener(OptionsReceivedCallback callback)
    {
        return this.UnregisterOptionsReceivedListener(callback, null);
    }

    public bool UnregisterOptionsReceivedListener(OptionsReceivedCallback callback, object userData)
    {
        OptionsReceivedListener item = new OptionsReceivedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_optionsReceivedListeners.Remove(item);
    }

    public bool UnregisterSpectatorNotifyListener(SpectatorNotifyEventCallback callback, object userData = null)
    {
        SpectatorNotifyListener item = new SpectatorNotifyListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_spectatorNotifyListeners.Remove(item);
    }

    public bool UnregisterTurnChangedListener(TurnChangedCallback callback)
    {
        return this.UnregisterTurnChangedListener(callback, null);
    }

    public bool UnregisterTurnChangedListener(TurnChangedCallback callback, object userData)
    {
        TurnChangedListener item = new TurnChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_turnChangedListeners.Remove(item);
    }

    public bool UnregisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback)
    {
        return this.UnregisterTurnTimerUpdateListener(callback, null);
    }

    public bool UnregisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
    {
        TurnTimerUpdateListener item = new TurnTimerUpdateListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_turnTimerUpdateListeners.Remove(item);
    }

    public void Update()
    {
        if (!this.IsBlockingServer())
        {
            this.m_powerProcessor.ProcessPowerQueue();
        }
    }

    public void UpdateChoiceHighlights()
    {
        foreach (Network.EntityChoices choices in this.m_choicesMap.Values)
        {
            Entity entity = this.GetEntity(choices.Source);
            if (entity != null)
            {
                Card card = entity.GetCard();
                if (card != null)
                {
                    card.UpdateActorState();
                }
            }
            foreach (int num in choices.Entities)
            {
                Entity entity2 = this.GetEntity(num);
                if (entity2 != null)
                {
                    Card card2 = entity2.GetCard();
                    if (card2 != null)
                    {
                        card2.UpdateActorState();
                    }
                }
            }
        }
        foreach (Entity entity3 in this.m_chosenEntities)
        {
            Card card3 = entity3.GetCard();
            if (card3 != null)
            {
                card3.UpdateActorState();
            }
        }
    }

    private void UpdateHighlightsBasedOnSelection()
    {
        if (this.m_selectedOption.m_target != 0)
        {
            Network.Options.Option.SubOption selectedNetworkSubOption = this.GetSelectedNetworkSubOption();
            this.UpdateTargetHighlights(selectedNetworkSubOption);
        }
        else if (this.m_selectedOption.m_sub >= 0)
        {
            Network.Options.Option selectedNetworkOption = this.GetSelectedNetworkOption();
            this.UpdateSubOptionHighlights(selectedNetworkOption);
        }
    }

    public void UpdateOptionHighlights()
    {
        this.UpdateOptionHighlights(this.m_options);
    }

    public void UpdateOptionHighlights(Network.Options options)
    {
        if ((options != null) && (options.List != null))
        {
            for (int i = 0; i < options.List.Count; i++)
            {
                Network.Options.Option option = options.List[i];
                if (option.Type == Network.Options.Option.OptionType.POWER)
                {
                    Entity entity = this.GetEntity(option.Main.ID);
                    if (entity != null)
                    {
                        Card card = entity.GetCard();
                        if (card != null)
                        {
                            card.UpdateActorState();
                        }
                    }
                }
            }
        }
    }

    private void UpdateSubOptionHighlights(Network.Options.Option option)
    {
        Entity entity = this.GetEntity(option.Main.ID);
        if (entity != null)
        {
            Card card = entity.GetCard();
            if (card != null)
            {
                card.UpdateActorState();
            }
        }
        foreach (Network.Options.Option.SubOption option2 in option.Subs)
        {
            Entity entity2 = this.GetEntity(option2.ID);
            if (entity2 != null)
            {
                Card card2 = entity2.GetCard();
                if (card2 != null)
                {
                    card2.UpdateActorState();
                }
            }
        }
    }

    private void UpdateTargetHighlights(Network.Options.Option.SubOption subOption)
    {
        Entity entity = this.GetEntity(subOption.ID);
        if (entity != null)
        {
            Card card = entity.GetCard();
            if (card != null)
            {
                card.UpdateActorState();
            }
        }
        foreach (int num in subOption.Targets)
        {
            Entity entity2 = this.GetEntity(num);
            if (entity2 != null)
            {
                Card card2 = entity2.GetCard();
                if (card2 != null)
                {
                    card2.UpdateActorState();
                }
            }
        }
    }

    public bool WasConcedeRequested()
    {
        return this.m_concedeRequested;
    }

    public bool WasGameCreated()
    {
        return (this.m_gameEntity != null);
    }

    [CompilerGenerated]
    private sealed class <IsMulliganPhasePending>c__AnonStorey2F9
    {
        internal bool foundMulliganStep;
        internal int gameEntityId;

        internal void <>m__F8(int queueIndex, PowerTaskList taskList)
        {
            List<PowerTask> list = taskList.GetTaskList();
            for (int i = 0; i < list.Count; i++)
            {
                PowerTask task = list[i];
                Network.HistTagChange power = task.GetPower() as Network.HistTagChange;
                if ((power != null) && (power.Entity == this.gameEntityId))
                {
                    GAME_TAG tag = (GAME_TAG) power.Tag;
                    if (((tag == GAME_TAG.STEP) || (tag == GAME_TAG.NEXT_STEP)) && (power.Value == 4))
                    {
                        this.foundMulliganStep = true;
                        return;
                    }
                }
            }
        }
    }

    private delegate void AppendBlockingServerItemCallback<T>(StringBuilder builder, T item);

    public delegate void CreateGameCallback(GameState.CreateGamePhase phase, object userData);

    private class CreateGameListener : EventListener<GameState.CreateGameCallback>
    {
        public void Fire(GameState.CreateGamePhase phase)
        {
            base.m_callback(phase, base.m_userData);
        }
    }

    public enum CreateGamePhase
    {
        INVALID,
        CREATING,
        CREATED
    }

    public delegate void CurrentPlayerChangedCallback(Player player, object userData);

    private class CurrentPlayerChangedListener : EventListener<GameState.CurrentPlayerChangedCallback>
    {
        public void Fire(Player player)
        {
            base.m_callback(player, base.m_userData);
        }
    }

    public delegate bool EntitiesChosenReceivedCallback(Network.EntitiesChosen chosen, Network.EntityChoices choices, object userData);

    private class EntitiesChosenReceivedListener : EventListener<GameState.EntitiesChosenReceivedCallback>
    {
        public bool Fire(Network.EntitiesChosen chosen, Network.EntityChoices choices)
        {
            return base.m_callback(chosen, choices, base.m_userData);
        }
    }

    public delegate void EntityChoicesReceivedCallback(Network.EntityChoices choices, PowerTaskList preChoiceTaskList, object userData);

    private class EntityChoicesReceivedListener : EventListener<GameState.EntityChoicesReceivedCallback>
    {
        public void Fire(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
        {
            base.m_callback(choices, preChoiceTaskList, base.m_userData);
        }
    }

    public delegate void GameOverCallback(object userData);

    private class GameOverListener : EventListener<GameState.GameOverCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void GameStateInitializedCallback(GameState instance, object userData);

    private class GameStateInitializedListener : EventListener<GameState.GameStateInitializedCallback>
    {
        public void Fire(GameState instance)
        {
            base.m_callback(instance, base.m_userData);
        }
    }

    public delegate void OptionsReceivedCallback(object userData);

    private class OptionsReceivedListener : EventListener<GameState.OptionsReceivedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    private class QueuedChoice
    {
        public object m_eventData;
        public object m_packet;
        public PacketType m_type;

        public enum PacketType
        {
            ENTITY_CHOICES,
            ENTITIES_CHOSEN
        }
    }

    public enum ResponseMode
    {
        NONE,
        OPTION,
        SUB_OPTION,
        OPTION_TARGET,
        CHOICE
    }

    private class SelectedOption
    {
        public int m_main = -1;
        public int m_position;
        public int m_sub = -1;
        public int m_target;

        public void Clear()
        {
            this.m_main = -1;
            this.m_sub = -1;
            this.m_target = 0;
            this.m_position = 0;
        }

        public void CopyFrom(GameState.SelectedOption original)
        {
            this.m_main = original.m_main;
            this.m_sub = original.m_sub;
            this.m_target = original.m_target;
            this.m_position = original.m_position;
        }
    }

    public delegate void SpectatorNotifyEventCallback(SpectatorNotify notify, object userData);

    private class SpectatorNotifyListener : EventListener<GameState.SpectatorNotifyEventCallback>
    {
        public void Fire(SpectatorNotify notify)
        {
            base.m_callback(notify, base.m_userData);
        }
    }

    public delegate void TurnChangedCallback(int oldTurn, int newTurn, object userData);

    private class TurnChangedListener : EventListener<GameState.TurnChangedCallback>
    {
        public void Fire(int oldTurn, int newTurn)
        {
            base.m_callback(oldTurn, newTurn, base.m_userData);
        }
    }

    public delegate void TurnTimerUpdateCallback(TurnTimerUpdate update, object userData);

    private class TurnTimerUpdateListener : EventListener<GameState.TurnTimerUpdateCallback>
    {
        public void Fire(TurnTimerUpdate update)
        {
            base.m_callback(update, base.m_userData);
        }
    }
}

