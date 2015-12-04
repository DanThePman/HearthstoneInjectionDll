using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DrawMorphedCardSpell : SuperSpell
{
    [CompilerGenerated]
    private static Predicate<Network.Entity.Tag> <>f__am$cache5;
    private Card m_newCard;
    public float m_NewCardHoldTime;
    private Card m_oldCard;
    public float m_OldCardHoldTime;
    private int m_revealTaskIndex = -1;

    public override bool AddPowerTargets()
    {
        this.m_revealTaskIndex = -1;
        if (!base.CanAddPowerTargets())
        {
            return false;
        }
        this.FindOldAndNewCards();
        return ((this.m_oldCard != null) && (this.m_newCard != null));
    }

    private void BeginEffects()
    {
        base.DoActionNow();
        base.StartCoroutine(this.HoldOldCard());
    }

    protected override void DoActionNow()
    {
        if (this.m_revealTaskIndex < 0)
        {
            this.BeginEffects();
        }
        else
        {
            base.m_taskList.DoTasks(0, this.m_revealTaskIndex + 1, new PowerTaskList.CompleteCallback(this.OnRevealTasksComplete));
        }
    }

    private void FindOldAndNewCards()
    {
        int num = -1;
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        for (int i = 0; i < taskList.Count; i++)
        {
            Network.PowerHistory power = taskList[i].GetPower();
            switch (power.Type)
            {
                case Network.PowerType.FULL_ENTITY:
                {
                    Network.HistFullEntity entity = (Network.HistFullEntity) power;
                    Entity entity2 = GameState.Get().GetEntity(entity.Entity.ID);
                    if (entity2 != null)
                    {
                        Card card = entity2.GetCard();
                        if (card != null)
                        {
                            this.m_newCard = card;
                        }
                    }
                    break;
                }
                case Network.PowerType.SHOW_ENTITY:
                {
                    Network.HistShowEntity entity3 = (Network.HistShowEntity) power;
                    Entity entity4 = GameState.Get().GetEntity(entity3.Entity.ID);
                    if (entity4 != null)
                    {
                        Card card2 = entity4.GetCard();
                        if ((card2 != null) && (entity4.GetZone() == TAG_ZONE.DECK))
                        {
                            if (<>f__am$cache5 == null)
                            {
                                <>f__am$cache5 = delegate (Network.Entity.Tag tag) {
                                    if (tag.Name != 0x31)
                                    {
                                        return false;
                                    }
                                    if (tag.Value != 3)
                                    {
                                        return false;
                                    }
                                    return true;
                                };
                            }
                            if (entity3.Entity.Tags.Find(<>f__am$cache5) != null)
                            {
                                this.m_oldCard = card2;
                                num = i;
                            }
                        }
                    }
                    break;
                }
            }
        }
        if ((this.m_oldCard != null) && (this.m_newCard != null))
        {
            this.m_revealTaskIndex = num;
            this.AddTarget(this.m_oldCard.gameObject);
        }
    }

    [DebuggerHidden]
    private IEnumerator HoldNewCard()
    {
        return new <HoldNewCard>c__Iterator20D { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator HoldOldCard()
    {
        return new <HoldOldCard>c__Iterator20C { <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        this.m_oldCard.SetHoldingForLinkedCardSwitch(true);
        this.m_newCard.SetHoldingForLinkedCardSwitch(true);
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
    }

    private void OnAllTasksComplete(PowerTaskList taskList, int startIndex, int count, object userData)
    {
        base.StartCoroutine(this.HoldNewCard());
    }

    private void OnRevealTasksComplete(PowerTaskList taskList, int startIndex, int count, object userData)
    {
        this.BeginEffects();
    }

    [CompilerGenerated]
    private sealed class <HoldNewCard>c__Iterator20D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DrawMorphedCardSpell <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_NewCardHoldTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_oldCard.SetHoldingForLinkedCardSwitch(false);
                    this.<>f__this.m_newCard.SetHoldingForLinkedCardSwitch(false);
                    this.<>f__this.m_oldCard = null;
                    this.<>f__this.m_newCard = null;
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
                    this.$PC = -1;
                    break;
            }
            return false;
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

    [CompilerGenerated]
    private sealed class <HoldOldCard>c__Iterator20C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DrawMorphedCardSpell <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_OldCardHoldTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_taskList.DoAllTasks(new PowerTaskList.CompleteCallback(this.<>f__this.OnAllTasksComplete));
                    this.$PC = -1;
                    break;
            }
            return false;
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
}

