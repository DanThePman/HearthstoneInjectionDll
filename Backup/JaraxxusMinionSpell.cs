using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JaraxxusMinionSpell : Spell
{
    public float m_MoveToHeroSpotDelay = 3.5f;
    public float m_MoveToHeroSpotDuration = 0.3f;
    public iTween.EaseType m_MoveToHeroSpotEaseType = iTween.EaseType.linear;
    public float m_MoveToLocationDelay;
    public float m_MoveToLocationDuration = 1.5f;
    public iTween.EaseType m_MoveToLocationEaseType = iTween.EaseType.linear;

    public override bool AddPowerTargets()
    {
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.FULL_ENTITY)
            {
                Network.HistFullEntity entity = power as Network.HistFullEntity;
                int iD = entity.Entity.ID;
                Entity entity2 = GameState.Get().GetEntity(iD);
                if (entity2 == null)
                {
                    UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING encountered HistFullEntity where entity id={1} but there is no entity with that id", this, iD));
                    return false;
                }
                if (!entity2.IsHero())
                {
                    UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING HistFullEntity where entity id={1} is not a hero", this, iD));
                    return false;
                }
                this.AddTarget(entity2.GetCard().gameObject);
                return true;
            }
        }
        return false;
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
        base.GetSourceCard().GetActor().TurnOnCollider();
        Card targetCard = base.GetTargetCard();
        targetCard.GetActor().TurnOnCollider();
        targetCard.ShowCard();
        this.OnSpellFinished();
    }

    private void MoveToHeroSpot(Card minionCard, Card heroCard, Zone heroZone)
    {
        object[] args = new object[] { "position", heroZone.transform.position, "time", this.m_MoveToHeroSpotDuration, "easetype", this.m_MoveToHeroSpotEaseType };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(minionCard.gameObject, hashtable);
        object[] objArray2 = new object[] { "position", heroZone.transform.position, "time", this.m_MoveToHeroSpotDuration, "easetype", this.m_MoveToHeroSpotEaseType, "oncomplete", "OnMoveToHeroSpotComplete", "oncompletetarget", base.gameObject };
        Hashtable hashtable2 = iTween.Hash(objArray2);
        iTween.MoveTo(heroCard.gameObject, hashtable2);
    }

    private void MoveToSpellLocation(Card minionCard, Card heroCard)
    {
        object[] args = new object[] { "position", base.transform.position, "time", this.m_MoveToLocationDuration, "easetype", this.m_MoveToLocationEaseType };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(minionCard.gameObject, hashtable);
        object[] objArray2 = new object[] { "position", base.transform.position, "time", this.m_MoveToLocationDuration, "easetype", this.m_MoveToLocationEaseType };
        Hashtable hashtable2 = iTween.Hash(objArray2);
        iTween.MoveTo(heroCard.gameObject, hashtable2);
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        base.StartCoroutine(this.SetupHero());
    }

    private void OnMoveToHeroSpotComplete()
    {
        this.Finish();
    }

    [DebuggerHidden]
    private IEnumerator PlaySummoningSpells(Card minionCard, Card heroCard)
    {
        return new <PlaySummoningSpells>c__Iterator1D0 { minionCard = minionCard, heroCard = heroCard, <$>minionCard = minionCard, <$>heroCard = heroCard, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SetupHero()
    {
        return new <SetupHero>c__Iterator1CF { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <PlaySummoningSpells>c__Iterator1D0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>heroCard;
        internal Card <$>minionCard;
        internal JaraxxusMinionSpell <>f__this;
        internal Card heroCard;
        internal Card minionCard;

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
                    this.heroCard.transform.position = this.minionCard.transform.position;
                    this.minionCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS);
                    this.heroCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS);
                    this.$current = new WaitForSeconds(this.<>f__this.m_MoveToLocationDelay);
                    this.$PC = 1;
                    goto Label_00E7;

                case 1:
                    this.<>f__this.MoveToSpellLocation(this.minionCard, this.heroCard);
                    this.$current = new WaitForSeconds(this.<>f__this.m_MoveToHeroSpotDelay);
                    this.$PC = 2;
                    goto Label_00E7;

                case 2:
                    this.<>f__this.MoveToHeroSpot(this.minionCard, this.heroCard, this.heroCard.GetZone());
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00E7:
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
    private sealed class <SetupHero>c__Iterator1CF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JaraxxusMinionSpell <>f__this;
        internal PowerTask <fullEntityTask>__3;
        internal Card <heroCard>__1;
        internal Entity <heroEntity>__2;
        internal Zone <heroZone>__4;
        internal Card <minionCard>__0;

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
                    this.<minionCard>__0 = this.<>f__this.GetSourceCard();
                    this.<heroCard>__1 = this.<>f__this.GetTargetCard();
                    this.<heroEntity>__2 = this.<heroCard>__1.GetEntity();
                    this.<minionCard>__0.SuppressDeathEffects(true);
                    this.<minionCard>__0.GetActor().TurnOffCollider();
                    this.<fullEntityTask>__3 = this.<>f__this.FindFullEntityTask();
                    this.<fullEntityTask>__3.DoTask();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0102;

                default:
                    goto Label_014C;
            }
            if (this.<heroEntity>__2.IsLoadingAssets())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_014E;
            }
            this.<heroCard>__1.HideCard();
            this.<heroZone>__4 = ZoneMgr.Get().FindZoneForEntity(this.<heroEntity>__2);
            this.<heroCard>__1.TransitionToZone(this.<heroZone>__4);
        Label_0102:
            while (this.<heroCard>__1.IsActorLoading())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_014E;
            }
            this.<heroCard>__1.GetActor().TurnOffCollider();
            this.<>f__this.StartCoroutine(this.<>f__this.PlaySummoningSpells(this.<minionCard>__0, this.<heroCard>__1));
            this.$PC = -1;
        Label_014C:
            return false;
        Label_014E:
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

