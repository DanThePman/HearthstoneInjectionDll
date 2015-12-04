using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PoisonSeeds : SuperSpell
{
    private float m_AnimTime;
    public Spell m_CustomDeathSpell;
    public Spell m_CustomSpawnSpell;
    public ParticleSystem m_DustParticles;
    public AnimationCurve m_HeightCurve;
    private float m_HeightCurveLength;
    public ParticleSystem m_ImpactParticles;
    public float m_RotationDriftAmount;
    public AnimationCurve m_RotationDriftCurve;
    private AudioSource m_Sound;
    public float m_StartDeathSpellAdjustment = 0.01f;
    private SpellTargetType m_TargetType;

    public override bool AddPowerTargets()
    {
        base.m_visualToTargetIndexMap.Clear();
        base.m_targetToMetaDataMap.Clear();
        base.m_targets.Clear();
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        for (int i = 0; i < taskList.Count; i++)
        {
            PowerTask task = taskList[i];
            Card targetCardFromPowerTask = this.GetTargetCardFromPowerTask(i, task);
            if (targetCardFromPowerTask != null)
            {
                base.m_targets.Add(targetCardFromPowerTask.gameObject);
            }
        }
        return (base.m_targets.Count > 0);
    }

    [DebuggerHidden]
    private IEnumerator AnimateDeathEffect(List<MinionData> minions)
    {
        return new <AnimateDeathEffect>c__Iterator21F { minions = minions, <$>minions = minions, <>f__this = this };
    }

    protected override void Awake()
    {
        this.m_Sound = base.GetComponent<AudioSource>();
        base.Awake();
    }

    [DebuggerHidden]
    private IEnumerator CreateEffect()
    {
        return new <CreateEffect>c__Iterator21E { <>f__this = this };
    }

    private void DeathEffect()
    {
        if (this.m_HeightCurve.length == 0)
        {
            UnityEngine.Debug.LogWarning("PoisonSeeds Spell height animation curve in not defined");
        }
        else if (this.m_RotationDriftCurve.length == 0)
        {
            UnityEngine.Debug.LogWarning("PoisonSeeds Spell rotation drift animation curve in not defined");
        }
        else
        {
            if (this.m_CustomDeathSpell != null)
            {
                foreach (GameObject obj2 in base.GetTargets())
                {
                    if (obj2 != null)
                    {
                        obj2.GetComponent<Card>().OverrideCustomDeathSpell(UnityEngine.Object.Instantiate<Spell>(this.m_CustomDeathSpell));
                    }
                }
            }
            this.m_HeightCurveLength = this.m_HeightCurve.keys[this.m_HeightCurve.length - 1].time;
            List<MinionData> minions = new List<MinionData>();
            foreach (GameObject obj3 in base.GetTargets())
            {
                MinionData item = new MinionData {
                    card = obj3.GetComponent<Card>(),
                    gameObject = obj3,
                    orgLocPos = obj3.transform.localPosition,
                    orgLocRot = obj3.transform.localRotation
                };
                float x = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value);
                float y = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value) * 0.1f;
                float z = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value);
                item.rotationDrift = new Vector3(x, y, z);
                minions.Add(item);
            }
            base.StartCoroutine(this.AnimateDeathEffect(minions));
        }
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        int id = 0;
        Network.PowerHistory power = task.GetPower();
        if (power.Type == Network.PowerType.FULL_ENTITY)
        {
            this.m_TargetType = SpellTargetType.Create;
            Network.HistFullEntity entity = power as Network.HistFullEntity;
            id = entity.Entity.ID;
        }
        else
        {
            Network.HistTagChange change = power as Network.HistTagChange;
            if ((change != null) && (change.Tag == 360))
            {
                this.m_TargetType = SpellTargetType.Death;
                id = change.Entity;
            }
            else
            {
                return null;
            }
        }
        Entity entity2 = GameState.Get().GetEntity(id);
        if (entity2 == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, id));
            return null;
        }
        return entity2.GetCard();
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        if (this.m_TargetType == SpellTargetType.Death)
        {
            this.DeathEffect();
        }
        else if (this.m_TargetType == SpellTargetType.Create)
        {
            base.StartCoroutine(this.CreateEffect());
        }
        else
        {
            base.m_effectsPendingFinish--;
            base.FinishIfPossible();
        }
    }

    private void ShakeCamera()
    {
        CameraShakeMgr.Shake(Camera.main, new Vector3(0.15f, 0.15f, 0.15f), 0.9f);
    }

    [CompilerGenerated]
    private sealed class <AnimateDeathEffect>c__Iterator21F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<PoisonSeeds.MinionData> <$>minions;
        internal List<PoisonSeeds.MinionData>.Enumerator <$s_1243>__1;
        internal List<PoisonSeeds.MinionData>.Enumerator <$s_1244>__8;
        internal List<PoisonSeeds.MinionData>.Enumerator <$s_1245>__10;
        internal List<ParticleSystem>.Enumerator <$s_1246>__12;
        internal PoisonSeeds <>f__this;
        internal bool <finished>__5;
        internal float <height>__6;
        internal List<ParticleSystem> <impactParticles>__0;
        internal ParticleSystem <ips>__13;
        internal PoisonSeeds.MinionData <minion>__11;
        internal PoisonSeeds.MinionData <minion>__2;
        internal PoisonSeeds.MinionData <minion>__9;
        internal GameObject <newDustParticle>__4;
        internal GameObject <newImpactParticle>__3;
        internal float <rotAmount>__7;
        internal List<PoisonSeeds.MinionData> minions;

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
                    if (this.<>f__this.m_Sound != null)
                    {
                        SoundManager.Get().Play(this.<>f__this.m_Sound);
                    }
                    this.<impactParticles>__0 = new List<ParticleSystem>();
                    this.<$s_1243>__1 = this.minions.GetEnumerator();
                    try
                    {
                        while (this.<$s_1243>__1.MoveNext())
                        {
                            this.<minion>__2 = this.<$s_1243>__1.Current;
                            this.<newImpactParticle>__3 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.m_ImpactParticles.gameObject);
                            this.<newImpactParticle>__3.transform.parent = this.<>f__this.transform;
                            this.<newImpactParticle>__3.transform.position = this.<minion>__2.gameObject.transform.position;
                            this.<impactParticles>__0.Add(this.<newImpactParticle>__3.GetComponentInChildren<ParticleSystem>());
                            this.<newDustParticle>__4 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.m_DustParticles.gameObject);
                            this.<newDustParticle>__4.transform.parent = this.<>f__this.transform;
                            this.<newDustParticle>__4.transform.position = this.<minion>__2.gameObject.transform.position;
                            this.<newDustParticle>__4.GetComponent<ParticleSystem>().Play();
                        }
                    }
                    finally
                    {
                        this.<$s_1243>__1.Dispose();
                    }
                    this.<>f__this.m_AnimTime = 0f;
                    this.<finished>__5 = false;
                    break;

                case 1:
                    break;

                default:
                    goto Label_0431;
            }
            while (this.<>f__this.m_AnimTime < this.<>f__this.m_HeightCurveLength)
            {
                this.<>f__this.m_AnimTime += UnityEngine.Time.deltaTime;
                this.<height>__6 = this.<>f__this.m_HeightCurve.Evaluate(this.<>f__this.m_AnimTime);
                this.<rotAmount>__7 = this.<>f__this.m_RotationDriftCurve.Evaluate(this.<>f__this.m_AnimTime);
                this.<$s_1244>__8 = this.minions.GetEnumerator();
                try
                {
                    while (this.<$s_1244>__8.MoveNext())
                    {
                        this.<minion>__9 = this.<$s_1244>__8.Current;
                        this.<minion>__9.gameObject.transform.localPosition = new Vector3(this.<minion>__9.orgLocPos.x, this.<minion>__9.orgLocPos.y + this.<height>__6, this.<minion>__9.orgLocPos.z);
                        this.<minion>__9.gameObject.transform.localRotation = this.<minion>__9.orgLocRot;
                        this.<minion>__9.gameObject.transform.Rotate((Vector3) (this.<minion>__9.rotationDrift * this.<rotAmount>__7), Space.Self);
                    }
                }
                finally
                {
                    this.<$s_1244>__8.Dispose();
                }
                if ((this.<>f__this.m_AnimTime > (this.<>f__this.m_HeightCurveLength - this.<>f__this.m_StartDeathSpellAdjustment)) && !this.<finished>__5)
                {
                    this.<$s_1245>__10 = this.minions.GetEnumerator();
                    try
                    {
                        while (this.<$s_1245>__10.MoveNext())
                        {
                            this.<minion>__11 = this.<$s_1245>__10.Current;
                            this.<minion>__11.card.GetActor().DeactivateAllPreDeathSpells();
                        }
                    }
                    finally
                    {
                        this.<$s_1245>__10.Dispose();
                    }
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
                    this.<finished>__5 = true;
                }
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<$s_1246>__12 = this.<impactParticles>__0.GetEnumerator();
            try
            {
                while (this.<$s_1246>__12.MoveNext())
                {
                    this.<ips>__13 = this.<$s_1246>__12.Current;
                    this.<ips>__13.Play();
                }
            }
            finally
            {
                this.<$s_1246>__12.Dispose();
            }
            this.<>f__this.ShakeCamera();
            this.$PC = -1;
        Label_0431:
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
    private sealed class <CreateEffect>c__Iterator21E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<GameObject>.Enumerator <$s_1239>__0;
        internal List<GameObject>.Enumerator <$s_1240>__4;
        internal PoisonSeeds <>f__this;
        internal Card <card>__6;
        internal GameObject <target>__1;
        internal GameObject <target>__5;
        internal Card <targetCard>__2;
        internal ZonePlay <zone>__3;
        internal ZonePlay <zone>__7;

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
                    this.<$s_1239>__0 = this.<>f__this.GetTargets().GetEnumerator();
                    try
                    {
                        while (this.<$s_1239>__0.MoveNext())
                        {
                            this.<target>__1 = this.<$s_1239>__0.Current;
                            if (this.<target>__1 != null)
                            {
                                this.<targetCard>__2 = this.<target>__1.GetComponent<Card>();
                                if (this.<targetCard>__2 != null)
                                {
                                    this.<targetCard>__2.OverrideCustomSpawnSpell(UnityEngine.Object.Instantiate<Spell>(this.<>f__this.m_CustomSpawnSpell));
                                    this.<zone>__3 = (ZonePlay) this.<targetCard>__2.GetZone();
                                    if (this.<zone>__3 != null)
                                    {
                                        this.<zone>__3.SetTransitionTime(0.01f);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_1239>__0.Dispose();
                    }
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
                    this.<>f__this.ShakeCamera();
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<$s_1240>__4 = this.<>f__this.GetTargets().GetEnumerator();
                    try
                    {
                        while (this.<$s_1240>__4.MoveNext())
                        {
                            this.<target>__5 = this.<$s_1240>__4.Current;
                            this.<card>__6 = this.<target>__5.GetComponent<Card>();
                            if (this.<card>__6 != null)
                            {
                                this.<zone>__7 = (ZonePlay) this.<card>__6.GetZone();
                                if (this.<zone>__7 != null)
                                {
                                    this.<zone>__7.ResetTransitionTime();
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_1240>__4.Dispose();
                    }
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

    public class MinionData
    {
        public Card card;
        public GameObject gameObject;
        public Vector3 orgLocPos;
        public Quaternion orgLocRot;
        public Vector3 rotationDrift;
    }

    private enum SpellTargetType
    {
        None,
        Death,
        Create
    }
}

