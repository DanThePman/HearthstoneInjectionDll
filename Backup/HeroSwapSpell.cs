using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroSwapSpell : Spell
{
    public float m_FinishDelay;
    protected Card m_newHeroCard;
    public Spell m_NewHeroFX;
    protected Card m_oldHeroCard;
    public Spell m_OldHeroFX;
    public bool removeOldStats;

    public override bool AddPowerTargets()
    {
        this.m_oldHeroCard = null;
        this.m_newHeroCard = null;
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.FULL_ENTITY)
            {
                Network.HistFullEntity entity = (Network.HistFullEntity) power;
                int iD = entity.Entity.ID;
                Entity entity2 = GameState.Get().GetEntity(iD);
                if (entity2 == null)
                {
                    UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING encountered HistFullEntity where entity id={1} but there is no entity with that id", this, iD));
                    return false;
                }
                if (entity2.IsHero())
                {
                    this.m_newHeroCard = entity2.GetCard();
                }
            }
            else if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = (Network.HistTagChange) power;
                if ((change.Tag == 0x31) && (change.Value == 6))
                {
                    int id = change.Entity;
                    Entity entity3 = GameState.Get().GetEntity(id);
                    if (entity3 == null)
                    {
                        UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING encountered HistTagChange where entity id={1} but there is no entity with that id", this, id));
                        return false;
                    }
                    if (entity3.IsHero())
                    {
                        this.m_oldHeroCard = entity3.GetCard();
                    }
                }
            }
        }
        if (this.m_oldHeroCard == null)
        {
            this.m_newHeroCard = null;
            return false;
        }
        if (this.m_newHeroCard == null)
        {
            this.m_oldHeroCard = null;
            return false;
        }
        return true;
    }

    public virtual void CustomizeFXProcess(Actor heroActor)
    {
    }

    private PowerTask FindFullEntityTask()
    {
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            if (task.GetPower().Type == Network.PowerType.FULL_ENTITY)
            {
                return task;
            }
        }
        return null;
    }

    private void Finish()
    {
        this.m_newHeroCard.GetActor().TurnOnCollider();
        this.m_newHeroCard.ShowCard();
        this.OnSpellFinished();
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        base.StartCoroutine(this.SetupHero());
    }

    [DebuggerHidden]
    private IEnumerator PlaySwapFx(Spell heroFX, Card heroCard)
    {
        return new <PlaySwapFx>c__Iterator214 { heroCard = heroCard, heroFX = heroFX, <$>heroCard = heroCard, <$>heroFX = heroFX, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SetupHero()
    {
        return new <SetupHero>c__Iterator213 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <PlaySwapFx>c__Iterator214 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>heroCard;
        internal Spell <$>heroFX;
        internal HeroSwapSpell <>f__this;
        internal Actor <heroActor>__0;
        internal Spell <swapSpell>__1;
        internal Card heroCard;
        internal Spell heroFX;

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
                    this.<heroActor>__0 = this.heroCard.GetActor();
                    this.<>f__this.CustomizeFXProcess(this.<heroActor>__0);
                    this.<swapSpell>__1 = UnityEngine.Object.Instantiate<Spell>(this.heroFX);
                    SpellUtils.SetCustomSpellParent(this.<swapSpell>__1, this.<heroActor>__0);
                    this.<swapSpell>__1.SetSource(this.heroCard.gameObject);
                    this.<swapSpell>__1.Activate();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00CA;

                default:
                    goto Label_00F1;
            }
            if (!this.<swapSpell>__1.IsFinished())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_00F3;
            }
        Label_00CA:
            while (this.<swapSpell>__1.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00F3;
            }
            UnityEngine.Object.Destroy(this.<swapSpell>__1.gameObject);
            this.$PC = -1;
        Label_00F1:
            return false;
        Label_00F3:
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

    [CompilerGenerated]
    private sealed class <SetupHero>c__Iterator213 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroSwapSpell <>f__this;
        internal PowerTask <fullEntityTask>__1;
        internal Zone <heroZone>__2;
        internal Entity <newHeroEntity>__0;
        internal Actor <oldActor>__3;

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
                    this.<newHeroEntity>__0 = this.<>f__this.m_newHeroCard.GetEntity();
                    this.<fullEntityTask>__1 = this.<>f__this.FindFullEntityTask();
                    this.<fullEntityTask>__1.DoTask();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00D7;

                case 3:
                    this.<>f__this.Finish();
                    this.$PC = -1;
                    goto Label_0230;

                default:
                    goto Label_0230;
            }
            if (this.<newHeroEntity>__0.IsLoadingAssets())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0232;
            }
            this.<>f__this.m_newHeroCard.HideCard();
            this.<heroZone>__2 = ZoneMgr.Get().FindZoneForEntity(this.<newHeroEntity>__0);
            this.<>f__this.m_newHeroCard.TransitionToZone(this.<heroZone>__2);
        Label_00D7:
            while (this.<>f__this.m_newHeroCard.IsActorLoading())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0232;
            }
            this.<>f__this.m_newHeroCard.GetActor().TurnOffCollider();
            this.<>f__this.m_newHeroCard.transform.position = this.<>f__this.m_newHeroCard.GetZone().transform.position;
            if (this.<>f__this.m_OldHeroFX != null)
            {
                if (this.<>f__this.removeOldStats)
                {
                    this.<oldActor>__3 = this.<>f__this.m_oldHeroCard.GetActor();
                    UnityEngine.Object.Destroy(this.<oldActor>__3.m_healthObject);
                    UnityEngine.Object.Destroy(this.<oldActor>__3.m_attackObject);
                }
                this.<>f__this.StartCoroutine(this.<>f__this.PlaySwapFx(this.<>f__this.m_OldHeroFX, this.<>f__this.m_oldHeroCard));
            }
            if (this.<>f__this.m_NewHeroFX != null)
            {
                this.<>f__this.StartCoroutine(this.<>f__this.PlaySwapFx(this.<>f__this.m_NewHeroFX, this.<>f__this.m_newHeroCard));
            }
            this.$current = new WaitForSeconds(this.<>f__this.m_FinishDelay);
            this.$PC = 3;
            goto Label_0232;
        Label_0230:
            return false;
        Label_0232:
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
}

