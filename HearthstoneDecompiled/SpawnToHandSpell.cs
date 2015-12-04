using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpawnToHandSpell : SuperSpell
{
    public float m_CardDelay = 1f;
    public float m_CardStaggerMax;
    public float m_CardStaggerMin;
    public float m_CardStartScale = 0.1f;
    [Tooltip("Only meaningful if you have a stagger delay.")]
    public bool m_RandomizeOrder;
    public bool m_Shake = true;
    public float m_ShakeDelay;
    public ShakeMinionIntensity m_ShakeIntensity = ShakeMinionIntensity.MediumShake;
    public Spell m_SpellPrefab;

    private void AddTransitionDelays()
    {
        int num = base.m_targets.Count - 1;
        if ((num > 1) && ((this.m_CardStaggerMin > 0f) || (this.m_CardStaggerMax > 0f)))
        {
            float delay = 0f;
            if (this.m_RandomizeOrder)
            {
                List<int> list = new List<int>(base.m_targets.Count);
                for (int i = 0; i < base.m_targets.Count; i++)
                {
                    list.Add(i);
                }
                while (num > 0)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    int num5 = list[index];
                    list.RemoveAt(index);
                    Card component = base.m_targets[num5].GetComponent<Card>();
                    float num6 = UnityEngine.Random.Range(this.m_CardStaggerMin, this.m_CardStaggerMax);
                    delay += num6;
                    component.SetTransitionDelay(delay);
                    num--;
                }
            }
            else
            {
                for (int j = 0; j < base.m_targets.Count; j++)
                {
                    Card card2 = base.m_targets[j].GetComponent<Card>();
                    float num8 = UnityEngine.Random.Range(this.m_CardStaggerMin, this.m_CardStaggerMax);
                    delay += num8;
                    card2.SetTransitionDelay(delay);
                }
            }
        }
    }

    [DebuggerHidden]
    protected virtual IEnumerator DoEffectWithTiming(Card sourceCard, GameObject sourceActorGo)
    {
        return new <DoEffectWithTiming>c__Iterator20E { sourceActorGo = sourceActorGo, <$>sourceActorGo = sourceActorGo, <>f__this = this };
    }

    protected virtual Vector3 GetOriginForTarget(int targetIndex = 0)
    {
        return base.transform.position;
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        Network.PowerHistory power = task.GetPower();
        if (power.Type != Network.PowerType.FULL_ENTITY)
        {
            return null;
        }
        Network.HistFullEntity entity = power as Network.HistFullEntity;
        Network.Entity entity2 = entity.Entity;
        Entity entity3 = GameState.Get().GetEntity(entity2.ID);
        if (entity3 == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, entity2.ID));
            return null;
        }
        if (entity3.GetZone() != TAG_ZONE.HAND)
        {
            return null;
        }
        return entity3.GetCard();
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        Card sourceCard = base.GetSourceCard();
        GameObject gameObject = sourceCard.GetActor().gameObject;
        if (gameObject == null)
        {
            UnityEngine.Debug.LogError(string.Format("{0}.OnAction() - unable to get source actor", this));
            base.m_effectsPendingFinish--;
            base.FinishIfPossible();
        }
        else
        {
            base.StartCoroutine(this.DoEffectWithTiming(sourceCard, gameObject));
        }
    }

    [CompilerGenerated]
    private sealed class <DoEffectWithTiming>c__Iterator20E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>sourceActorGo;
        internal SpawnToHandSpell <>f__this;
        internal int <i>__1;
        internal GameObject <sourceObject>__0;
        internal Spell <startSpell>__4;
        internal Card <targetCard>__3;
        internal GameObject <targetObject>__2;
        internal GameObject sourceActorGo;

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
                    if (this.<>f__this.m_Shake)
                    {
                        MinionShake.ShakeObject(this.sourceActorGo, ShakeMinionType.RandomDirection, this.sourceActorGo.transform.position, this.<>f__this.m_ShakeIntensity, 0.1f, 0f, this.<>f__this.m_ShakeDelay, true);
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_CardDelay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<sourceObject>__0 = this.<>f__this.GetSource();
                    this.<i>__1 = 0;
                    while (this.<i>__1 < this.<>f__this.m_targets.Count)
                    {
                        this.<targetObject>__2 = this.<>f__this.m_targets[this.<i>__1];
                        this.<targetCard>__3 = this.<targetObject>__2.GetComponent<Card>();
                        this.<targetCard>__3.transform.position = this.<>f__this.GetOriginForTarget(this.<i>__1);
                        if (this.<>f__this.m_SpellPrefab != null)
                        {
                            this.<startSpell>__4 = this.<>f__this.CloneSpell(this.<>f__this.m_SpellPrefab);
                            this.<startSpell>__4.SetSource(this.<sourceObject>__0);
                            this.<startSpell>__4.AddTarget(this.<targetObject>__2);
                            this.<startSpell>__4.SetPosition(this.<targetCard>__3.transform.position);
                            this.<startSpell>__4.Activate();
                        }
                        this.<targetCard>__3.transform.localScale = new Vector3(this.<>f__this.m_CardStartScale, this.<>f__this.m_CardStartScale, this.<>f__this.m_CardStartScale);
                        this.<targetCard>__3.SetTransitionStyle(ZoneTransitionStyle.VERY_SLOW);
                        this.<targetCard>__3.SetDoNotWarpToNewZone(true);
                        this.<i>__1++;
                    }
                    this.<>f__this.AddTransitionDelays();
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
}

