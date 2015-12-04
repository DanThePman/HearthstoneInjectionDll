using PegasusShared;
using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DraftManager
{
    private Network.RewardChest m_chest;
    private int m_currentSlot;
    private bool m_deckActiveDuringSession;
    private CollectionDeck m_draftDeck;
    private List<DraftDeckSet> m_draftDeckSetListeners = new List<DraftDeckSet>();
    private bool m_isNewKey;
    private int m_losses;
    private int m_maxWins = 0x7fffffff;
    private int m_validSlot;
    private int m_wins;
    private static DraftManager s_instance;

    private void ClearDeckInfo()
    {
        this.m_draftDeck = null;
        this.m_losses = 0;
        this.m_wins = 0;
        this.m_maxWins = 0x7fffffff;
        this.m_isNewKey = false;
        this.m_chest = null;
        this.m_deckActiveDuringSession = false;
    }

    public bool DeckWasActiveDuringSession()
    {
        return this.m_deckActiveDuringSession;
    }

    public void FindGame()
    {
        GameMgr.Get().FindGame(GameType.GT_ARENA, 2, 0L, 0L);
    }

    private void FireDraftDeckSetEvent()
    {
        foreach (DraftDeckSet set in this.m_draftDeckSetListeners.ToArray())
        {
            set(this.m_draftDeck);
        }
    }

    public static DraftManager Get()
    {
        if (s_instance == null)
        {
            s_instance = new DraftManager();
        }
        return s_instance;
    }

    public CollectionDeck GetDraftDeck()
    {
        return this.m_draftDeck;
    }

    public bool GetIsNewKey()
    {
        return this.m_isNewKey;
    }

    public int GetLosses()
    {
        return this.m_losses;
    }

    public int GetMaxWins()
    {
        return this.m_maxWins;
    }

    public List<RewardData> GetRewards()
    {
        if (this.m_chest != null)
        {
            return this.m_chest.Rewards;
        }
        return new List<RewardData>();
    }

    public int GetSlot()
    {
        return this.m_currentSlot;
    }

    public int GetWins()
    {
        return this.m_wins;
    }

    private void InformDraftDisplayOfChoices(List<NetCache.CardDefinition> choices)
    {
        DraftDisplay display = DraftDisplay.Get();
        if (display != null)
        {
            if (choices.Count == 0)
            {
                DraftDisplay.DraftMode mode;
                if (this.m_chest == null)
                {
                    mode = DraftDisplay.DraftMode.ACTIVE_DRAFT_DECK;
                    this.m_deckActiveDuringSession = true;
                }
                else
                {
                    mode = DraftDisplay.DraftMode.IN_REWARDS;
                }
                display.SetDraftMode(mode);
            }
            else
            {
                display.SetDraftMode(DraftDisplay.DraftMode.DRAFTING);
                display.AcceptNewChoices(choices);
            }
        }
    }

    public void MakeChoice(int choiceNum)
    {
        if (this.m_draftDeck == null)
        {
            Debug.LogWarning("DraftManager.MakeChoice(): Trying to make a draft choice while the draft deck is null");
        }
        else if (this.m_validSlot == this.m_currentSlot)
        {
            this.m_validSlot++;
            Network.MakeDraftChoice(this.m_draftDeck.ID, this.m_currentSlot, choiceNum);
        }
    }

    public void NotifyOfFinalGame(bool wonFinalGame)
    {
        if (wonFinalGame)
        {
            this.m_wins++;
        }
        else
        {
            this.m_losses++;
        }
    }

    private void OnAckRewards()
    {
        object[] objArray1 = new object[] { this.m_wins, ",", this.m_losses, ",1" };
        BnetPresenceMgr.Get().SetGameField(3, string.Concat(objArray1));
        if (!Options.Get().GetBool(Option.HAS_ACKED_ARENA_REWARDS, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_ARENA_1ST_REWARD"), "VO_INNKEEPER_ARENA_1ST_REWARD", 0f, null);
            Options.Get().SetBool(Option.HAS_ACKED_ARENA_REWARDS, true);
        }
        Network.GetRewardsAckDraftID();
        this.ClearDeckInfo();
    }

    private void OnBegin()
    {
        BnetPresenceMgr.Get().SetGameField(3, "0,0,0");
        Network.BeginDraft newDraftDeckID = Network.GetNewDraftDeckID();
        CollectionDeck deck = new CollectionDeck {
            ID = newDraftDeckID.DeckID,
            Type = DeckType.DRAFT_DECK
        };
        this.m_draftDeck = deck;
        this.m_currentSlot = 0;
        this.m_validSlot = 0;
        Log.Arena.Print(string.Format("DraftManager.OnBegin - Got new draft deck with ID: {0}", this.m_draftDeck.ID), new object[0]);
        this.InformDraftDisplayOfChoices(newDraftDeckID.Heroes);
        this.FireDraftDeckSetEvent();
    }

    private void OnChoicesAndContents()
    {
        Network.DraftChoicesAndContents draftChoicesAndContents = Network.GetDraftChoicesAndContents();
        this.m_currentSlot = draftChoicesAndContents.Slot;
        this.m_validSlot = draftChoicesAndContents.Slot;
        CollectionDeck deck = new CollectionDeck {
            ID = draftChoicesAndContents.DeckInfo.Deck,
            Type = DeckType.DRAFT_DECK,
            HeroCardID = draftChoicesAndContents.Hero.Name,
            HeroCardFlair = new CardFlair(draftChoicesAndContents.Hero.Premium)
        };
        this.m_draftDeck = deck;
        Log.Arena.Print(string.Format("DraftManager.OnChoicesAndContents - Draft Deck ID: {0}, Hero Card = {1}", this.m_draftDeck.ID, this.m_draftDeck.HeroCardID), new object[0]);
        foreach (Network.CardUserData data in draftChoicesAndContents.DeckInfo.Cards)
        {
            string str = (data.DbId != 0) ? GameUtils.TranslateDbIdToCardId(data.DbId) : string.Empty;
            Log.Arena.Print(string.Format("DraftManager.OnChoicesAndContents - Draft deck contains card {0}", str), new object[0]);
            if (!this.m_draftDeck.AddCard(str, data.Premium))
            {
                Debug.LogWarning(string.Format("DraftManager.OnChoicesAndContents() - Card {0} could not be added to draft deck", str));
            }
        }
        this.m_losses = draftChoicesAndContents.Losses;
        if (draftChoicesAndContents.Wins > this.m_wins)
        {
            this.m_isNewKey = true;
        }
        else
        {
            this.m_isNewKey = false;
        }
        this.m_wins = draftChoicesAndContents.Wins;
        this.m_maxWins = draftChoicesAndContents.MaxWins;
        this.m_chest = draftChoicesAndContents.Chest;
        if ((this.m_losses > 0) && DemoMgr.Get().ArenaIs1WinMode())
        {
            Network.RetireDraftDeck(this.GetDraftDeck().ID, this.GetSlot());
        }
        else
        {
            if ((this.m_wins == 5) && (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2013))
            {
                DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_5_WINS"), false, false);
            }
            else if ((this.m_losses == 3) && !Options.Get().GetBool(Option.HAS_LOST_IN_ARENA, false))
            {
                NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_ARENA_3RD_LOSS"), "VO_INNKEEPER_ARENA_3RD_LOSS", 0f, null);
                Options.Get().SetBool(Option.HAS_LOST_IN_ARENA, true);
            }
            this.InformDraftDisplayOfChoices(draftChoicesAndContents.Choices);
        }
    }

    private void OnChosen()
    {
        Network.DraftChosen chosenAndNext = Network.GetChosenAndNext();
        if (this.m_currentSlot == 0)
        {
            Log.Arena.Print(string.Format("DraftManager.OnChosen(): hero={0} premium={1}", chosenAndNext.ChosenCard.Name, chosenAndNext.ChosenCard.Premium), new object[0]);
            this.m_draftDeck.HeroCardID = chosenAndNext.ChosenCard.Name;
            this.m_draftDeck.HeroCardFlair = new CardFlair(chosenAndNext.ChosenCard.Premium);
        }
        else
        {
            this.m_draftDeck.AddCard(chosenAndNext.ChosenCard.Name, chosenAndNext.ChosenCard.Premium);
        }
        this.m_currentSlot++;
        if ((this.m_currentSlot > 30) && (DraftDisplay.Get() != null))
        {
            DraftDisplay.Get().DoDeckCompleteAnims();
        }
        this.InformDraftDisplayOfChoices(chosenAndNext.NextChoices);
    }

    private void OnDraftPurchaseAck(Network.Bundle bundle, PaymentMethod paymentMethod, object userData)
    {
        if (this.m_draftDeck != null)
        {
            StoreManager.Get().HideArenaStore();
        }
        else
        {
            this.RequestDraftStart();
        }
    }

    private void OnError()
    {
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.DRAFT))
        {
            Network.DraftError draftError = Network.GetDraftError();
            DraftDisplay display = DraftDisplay.Get();
            switch (draftError)
            {
                case Network.DraftError.DE_UNKNOWN:
                    Debug.LogError("DraftManager.OnError - UNKNOWN EXCEPTION - Talk to Brode or Fitch.");
                    return;

                case Network.DraftError.DE_NO_LICENSE:
                    Debug.LogWarning("DraftManager.OnError - No License.  What does this mean???");
                    return;

                case Network.DraftError.DE_RETIRE_FIRST:
                    Debug.LogError("DraftManager.OnError - You cannot start a new draft while one is in progress.");
                    return;

                case Network.DraftError.DE_NOT_IN_DRAFT:
                    if (display != null)
                    {
                        display.SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
                    }
                    return;

                case Network.DraftError.DE_NOT_IN_DRAFT_BUT_COULD_BE:
                    if (!Options.Get().GetBool(Option.HAS_SEEN_FORGE, false))
                    {
                        DraftDisplay.Get().SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
                        return;
                    }
                    this.RequestDraftStart();
                    return;

                case Network.DraftError.DE_FEATURE_DISABLED:
                    Debug.LogError("DraftManager.OnError - The Arena is currently disabled. Returning to the hub.");
                    if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
                    {
                        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                        Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_FORGE", new object[0]);
                    }
                    return;
            }
            Debug.LogError("DraftManager.onError - UNHANDLED ERROR - please send this to Brode. ERROR: " + draftError.ToString());
        }
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_CANCELED:
            case FindGameState.CLIENT_ERROR:
                DraftDisplay.Get().HandleGameStartupFailure();
                break;

            case FindGameState.CLIENT_CANCELED:
                Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CANCEL_MATCHMAKER);
                DraftDisplay.Get().HandleGameStartupFailure();
                break;
        }
        return false;
    }

    private void OnRetire()
    {
        Network.DraftRetired retiredDraft = Network.GetRetiredDraft();
        Log.Arena.Print(string.Format("DraftManager.OnRetire deckID={0}", retiredDraft.Deck), new object[0]);
        this.m_chest = retiredDraft.Chest;
        this.InformDraftDisplayOfChoices(new List<NetCache.CardDefinition>());
    }

    public void RegisterDraftDeckSetListener(DraftDeckSet dlg)
    {
        this.m_draftDeckSetListeners.Add(dlg);
    }

    public void RegisterMatchmakerHandlers()
    {
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    public void RegisterNetHandlers()
    {
        Network network = Network.Get();
        network.RegisterNetHandler(DraftBeginning.PacketID.ID, new Network.NetHandler(this.OnBegin), null);
        network.RegisterNetHandler(PegasusUtil.DraftRetired.PacketID.ID, new Network.NetHandler(this.OnRetire), null);
        network.RegisterNetHandler(DraftRewardsAcked.PacketID.ID, new Network.NetHandler(this.OnAckRewards), null);
        network.RegisterNetHandler(PegasusUtil.DraftChoicesAndContents.PacketID.ID, new Network.NetHandler(this.OnChoicesAndContents), null);
        network.RegisterNetHandler(PegasusUtil.DraftChosen.PacketID.ID, new Network.NetHandler(this.OnChosen), null);
        network.RegisterNetHandler(PegasusUtil.DraftError.PacketID.ID, new Network.NetHandler(this.OnError), null);
    }

    public void RegisterStoreHandlers()
    {
        StoreManager.Get().RegisterSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.OnDraftPurchaseAck));
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            StoreManager.Get().RegisterSuccessfulPurchaseListener(new StoreManager.SuccessfulPurchaseCallback(this.OnDraftPurchaseAck));
        }
    }

    public void RemoveDraftDeckSetListener(DraftDeckSet dlg)
    {
        this.m_draftDeckSetListeners.Remove(dlg);
    }

    public void RemoveMatchmakerHandlers()
    {
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    public void RemoveNetHandlers()
    {
        Network network = Network.Get();
        network.RemoveNetHandler(DraftBeginning.PacketID.ID, new Network.NetHandler(this.OnBegin));
        network.RemoveNetHandler(PegasusUtil.DraftRetired.PacketID.ID, new Network.NetHandler(this.OnRetire));
        network.RemoveNetHandler(DraftRewardsAcked.PacketID.ID, new Network.NetHandler(this.OnAckRewards));
        network.RemoveNetHandler(PegasusUtil.DraftChoicesAndContents.PacketID.ID, new Network.NetHandler(this.OnChoicesAndContents));
        network.RemoveNetHandler(PegasusUtil.DraftChosen.PacketID.ID, new Network.NetHandler(this.OnChosen));
        network.RemoveNetHandler(PegasusUtil.DraftError.PacketID.ID, new Network.NetHandler(this.OnError));
    }

    public void RemoveStoreHandlers()
    {
        StoreManager.Get().RemoveSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.OnDraftPurchaseAck));
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            StoreManager.Get().RemoveSuccessfulPurchaseListener(new StoreManager.SuccessfulPurchaseCallback(this.OnDraftPurchaseAck));
        }
    }

    public void RequestDraftStart()
    {
        Network.StartANewDraft();
    }

    public delegate void DraftDeckSet(CollectionDeck deck);
}

