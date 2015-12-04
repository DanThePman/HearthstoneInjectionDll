using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TurnStartManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<Card> <>f__am$cacheB;
    private bool m_blockingInput;
    private List<Card> m_cardsToDraw = new List<Card>();
    private List<CardChange> m_exhaustedChangesToHandle = new List<CardChange>();
    private bool m_listeningForTurnEvents;
    private int m_manaCrystalsFilled;
    private int m_manaCrystalsGained;
    private SpellController m_spellController;
    private TurnStartIndicator m_turnStartInstance;
    public TurnStartIndicator m_turnStartPrefab;
    private bool m_twoScoopsDisplayed;
    private static TurnStartManager s_instance;

    private bool AreDrawnCardsReady(Card[] cardsToDraw)
    {
        if (<>f__am$cacheB == null)
        {
            <>f__am$cacheB = card => !card.IsActorReady();
        }
        return (Array.Find<Card>(cardsToDraw, <>f__am$cacheB) == 0);
    }

    private void Awake()
    {
        s_instance = this;
        this.m_turnStartInstance = UnityEngine.Object.Instantiate<TurnStartIndicator>(this.m_turnStartPrefab);
        this.m_turnStartInstance.transform.parent = base.transform;
        GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
    }

    public void BeginListeningForTurnEvents()
    {
        this.m_cardsToDraw.Clear();
        this.m_exhaustedChangesToHandle.Clear();
        this.m_manaCrystalsGained = 0;
        this.m_manaCrystalsFilled = 0;
        this.m_twoScoopsDisplayed = false;
        this.m_listeningForTurnEvents = true;
        this.m_blockingInput = true;
    }

    public void BeginPlayingTurnEvents()
    {
        base.StartCoroutine(this.RunTurnEventsWithTiming());
    }

    private void DisplayTwoScoops()
    {
        if (!this.m_twoScoopsDisplayed)
        {
            this.m_twoScoopsDisplayed = true;
            this.m_turnStartInstance.SetReminderText(GameState.Get().GetGameEntity().GetTurnStartReminderText());
            this.m_turnStartInstance.Show();
            SoundManager.Get().LoadAndPlay("ALERT_YourTurn_0v2");
        }
    }

    public static TurnStartManager Get()
    {
        return s_instance;
    }

    public List<Card> GetCardsToDraw()
    {
        return this.m_cardsToDraw;
    }

    public int GetNumCardsToDraw()
    {
        return this.m_cardsToDraw.Count;
    }

    public SpellController GetSpellController()
    {
        return this.m_spellController;
    }

    private void HandleExhaustedChanges()
    {
        foreach (CardChange change in this.m_exhaustedChangesToHandle)
        {
            Card card = change.m_card;
            switch (card.GetEntity().GetZone())
            {
                case TAG_ZONE.PLAY:
                case TAG_ZONE.SECRET:
                    card.ShowExhaustedChange(change.m_tagDelta.newValue);
                    break;
            }
        }
        this.m_exhaustedChangesToHandle.Clear();
    }

    private bool HasActionsAfterCardDraw()
    {
        if (this.m_spellController != null)
        {
            return true;
        }
        Network.EntityChoices friendlyEntityChoices = GameState.Get().GetFriendlyEntityChoices();
        return ((friendlyEntityChoices != null) && (friendlyEntityChoices.ChoiceType == CHOICE_TYPE.GENERAL));
    }

    public bool IsBlockingInput()
    {
        return this.m_blockingInput;
    }

    public bool IsCardDrawHandled(Card card)
    {
        if (card == null)
        {
            return false;
        }
        return this.m_cardsToDraw.Contains(card);
    }

    public bool IsListeningForTurnEvents()
    {
        return this.m_listeningForTurnEvents;
    }

    public void NotifyOfCardDrawn(Entity drawnEntity)
    {
        this.m_cardsToDraw.Add(drawnEntity.GetCard());
    }

    public void NotifyOfExhaustedChange(Card card, TagDelta tagChange)
    {
        CardChange item = new CardChange {
            m_card = card,
            m_tagDelta = tagChange
        };
        this.m_exhaustedChangesToHandle.Add(item);
    }

    public void NotifyOfManaCrystalFilled(int amount)
    {
        this.m_manaCrystalsFilled += amount;
    }

    public void NotifyOfManaCrystalGained(int amount)
    {
        this.m_manaCrystalsGained += amount;
    }

    public void NotifyOfSpellController(SpellController spellController)
    {
        this.m_spellController = spellController;
        this.BeginPlayingTurnEvents();
    }

    public void NotifyOfTriggerVisual()
    {
        this.DisplayTwoScoops();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnGameOver(object userData)
    {
        base.StopAllCoroutines();
    }

    [DebuggerHidden]
    private IEnumerator RunTurnEventsWithTiming()
    {
        return new <RunTurnEventsWithTiming>c__IteratorC8 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <RunTurnEventsWithTiming>c__IteratorC8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card[] <$s_638>__3;
        internal int <$s_639>__4;
        internal TurnStartManager <>f__this;
        internal Card <card>__5;
        internal Card[] <cardsToDraw>__1;
        internal Player <friendlyPlayer>__0;
        internal ZoneHand <handZone>__2;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    if (this.<>f__this.IsListeningForTurnEvents())
                    {
                        this.<>f__this.m_listeningForTurnEvents = false;
                        if (GameMgr.Get().IsAI() && !this.<>f__this.m_twoScoopsDisplayed)
                        {
                            this.$current = new WaitForSeconds(1f);
                            this.$PC = 1;
                            goto Label_030F;
                        }
                        break;
                    }
                    goto Label_030D;

                case 1:
                    break;

                case 2:
                    goto Label_0120;

                case 3:
                    goto Label_01BB;

                case 4:
                    goto Label_020F;

                case 5:
                    goto Label_0251;

                case 6:
                    goto Label_028E;

                default:
                    goto Label_030D;
            }
            this.<>f__this.DisplayTwoScoops();
            this.<friendlyPlayer>__0 = GameState.Get().GetFriendlySidePlayer();
            this.<friendlyPlayer>__0.ReadyManaCrystal(this.<>f__this.m_manaCrystalsFilled);
            this.<friendlyPlayer>__0.AddManaCrystal(this.<>f__this.m_manaCrystalsGained, true);
            this.<friendlyPlayer>__0.UpdateManaCounter();
            this.<>f__this.HandleExhaustedChanges();
            if (this.<>f__this.m_turnStartInstance.IsShown())
            {
                this.$current = new WaitForSeconds(1f);
                this.$PC = 2;
                goto Label_030F;
            }
        Label_0120:
            if (this.<>f__this.m_cardsToDraw.Count <= 0)
            {
                goto Label_0251;
            }
            this.<cardsToDraw>__1 = this.<>f__this.m_cardsToDraw.ToArray();
            this.<>f__this.m_cardsToDraw.Clear();
            this.<handZone>__2 = this.<friendlyPlayer>__0.GetHandZone();
            this.<handZone>__2.UpdateLayout();
            this.<$s_638>__3 = this.<cardsToDraw>__1;
            this.<$s_639>__4 = 0;
            while (this.<$s_639>__4 < this.<$s_638>__3.Length)
            {
                this.<card>__5 = this.<$s_638>__3[this.<$s_639>__4];
            Label_01BB:
                while (this.<card>__5.IsActorLoading())
                {
                    this.$current = null;
                    this.$PC = 3;
                    goto Label_030F;
                }
                this.<card>__5.DrawFriendlyCard();
                this.<$s_639>__4++;
            }
        Label_020F:
            while (!this.<>f__this.AreDrawnCardsReady(this.<cardsToDraw>__1))
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_030F;
            }
            if (this.<>f__this.HasActionsAfterCardDraw())
            {
                this.$current = new WaitForSeconds(0.35f);
                this.$PC = 5;
                goto Label_030F;
            }
        Label_0251:
            if (this.<>f__this.m_spellController == null)
            {
                goto Label_02AF;
            }
            this.<>f__this.m_spellController.DoPowerTaskList();
        Label_028E:
            while (this.<>f__this.m_spellController.IsProcessingTaskList())
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_030F;
            }
            this.<>f__this.m_spellController = null;
        Label_02AF:
            if (GameState.Get().IsFriendlySidePlayerTurn())
            {
                GameState.Get().GetGameEntity().NotifyOfStartOfTurnEventsFinished();
                this.<>f__this.m_blockingInput = false;
                EndTurnButton.Get().OnTurnStartManagerFinished();
                if (GameState.Get().IsInMainOptionMode())
                {
                    GameState.Get().EnterMainOptionMode();
                }
                TurnTimer.Get().OnTurnStartManagerFinished();
            }
            this.$PC = -1;
        Label_030D:
            return false;
        Label_030F:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    private class CardChange
    {
        public Card m_card;
        public TagDelta m_tagDelta;
    }
}

