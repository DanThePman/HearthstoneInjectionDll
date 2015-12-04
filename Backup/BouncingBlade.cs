using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class BouncingBlade : SuperSpell
{
    private const float BLADE_ANIMATION_SPEED = 50f;
    private const float BLADE_BIRTH_TIME = 0.3f;
    private const float DAMAGE_SPLAT_DELAY = 0f;
    private bool m_Animating;
    public GameObject m_Blade;
    public AudioSource m_BladeHitBoardCorner;
    public AudioSource m_BladeHitMinion;
    public AudioSource m_BladeHitOffScreen;
    public GameObject m_BladeRoot;
    public AudioSource m_BladeSpinning;
    public AudioSource m_BladeSpinningContinuous;
    public float m_BladeSpinningMaxVol = 1f;
    public float m_BladeSpinningMinVol;
    public float m_BladeSpinningRampTime = 0.3f;
    public ParticleSystem m_EndBigSparkParticles;
    public AudioSource m_EndSound;
    public ParticleSystem m_EndSparkParticles;
    public List<HitBonesType> m_HitBones;
    public GameObject m_HitBonesRoot;
    private bool m_isDone;
    private Vector3? m_NextPosition;
    private Vector3 m_OrgBladeScale;
    private HitBonesType m_PreviousHitBone;
    private bool m_Running;
    public List<ParticleSystem> m_SparkParticles;
    public AudioSource m_StartSound;
    private List<Target> m_TargetQueue = new List<Target>();
    public GameObject m_Trail;
    private const int OFFSCREEN_HIT_PERCENT = 5;

    private Vector3 AcquireRandomBoardTarget(out bool offscreen)
    {
        offscreen = false;
        if (UnityEngine.Random.Range(1, 100) < 5)
        {
            offscreen = true;
        }
        List<HitBonesType> list = new List<HitBonesType>();
        if (offscreen)
        {
            foreach (HitBonesType type in this.m_HitBones)
            {
                if (((((type.Direction != HIT_DIRECTIONS.E) && (type.Direction != HIT_DIRECTIONS.NE)) && ((type.Direction != HIT_DIRECTIONS.NW) && (type.Direction != HIT_DIRECTIONS.SE))) && (type.Direction != HIT_DIRECTIONS.SW)) && (type.Direction != this.m_PreviousHitBone.Direction))
                {
                    list.Add(type);
                }
            }
        }
        else
        {
            foreach (HitBonesType type2 in this.m_HitBones)
            {
                if (((((type2.Direction != HIT_DIRECTIONS.E_OFFSCREEN) && (type2.Direction != HIT_DIRECTIONS.N_OFFSCREEN)) && (type2.Direction != HIT_DIRECTIONS.S_OFFSCREEN)) && (type2.Direction != HIT_DIRECTIONS.W_OFFSCREEN)) && (type2.Direction != this.m_PreviousHitBone.Direction))
                {
                    list.Add(type2);
                }
            }
        }
        int num = UnityEngine.Random.Range(0, list.Count);
        this.m_PreviousHitBone = list[num];
        return list[num].GetPosition();
    }

    private void AnimateSparks()
    {
        foreach (ParticleSystem system in this.m_SparkParticles)
        {
            system.Play();
        }
    }

    private void AnimateToNextTarget(Target target)
    {
        this.m_Animating = true;
        object[] args = new object[] { "position", target.TargetPosition, "speed", 50f, "orienttopath", true, "easetype", iTween.EaseType.linear, "oncompletetarget", base.gameObject, "oncomplete", "AnimationComplete", "oncompleteparams", target };
        iTween.MoveTo(this.m_BladeRoot, iTween.Hash(args));
    }

    private void AnimationComplete(Target target)
    {
        AudioSource bladeHitMinion;
        this.m_Animating = false;
        this.AnimateSparks();
        if (!target.LastBlock && !target.LastTarget)
        {
            this.RampBladeVolume();
        }
        if (target.isMinion)
        {
            bladeHitMinion = this.m_BladeHitMinion;
        }
        else if (target.Offscreen)
        {
            bladeHitMinion = this.m_BladeHitOffScreen;
        }
        else
        {
            bladeHitMinion = this.m_BladeHitBoardCorner;
        }
        if (bladeHitMinion != null)
        {
            bladeHitMinion.gameObject.transform.position = target.TargetPosition;
            SoundManager.Get().Play(bladeHitMinion);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Log.Kyle.Print("Awake()", new object[0]);
        this.m_BladeRoot.SetActive(false);
        this.m_PreviousHitBone = this.m_HitBones[this.m_HitBones.Count - 1];
        this.m_OrgBladeScale = this.m_BladeRoot.transform.localScale;
        this.m_BladeRoot.transform.localScale = Vector3.zero;
    }

    [DebuggerHidden]
    private IEnumerator BladeRunner()
    {
        return new <BladeRunner>c__Iterator204 { <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        if (base.m_targets.Count == 0)
        {
            this.m_isDone = true;
            this.m_BladeRoot.SetActive(false);
            base.m_effectsPendingFinish--;
            base.FinishIfPossible();
        }
        else
        {
            if (!this.m_Running)
            {
                this.m_BladeRoot.SetActive(true);
                this.m_Blade.SetActive(false);
                this.m_Trail.SetActive(false);
                this.m_Running = true;
                base.StartCoroutine(this.BladeRunner());
            }
            this.m_BladeRoot.transform.localScale = this.m_OrgBladeScale;
            this.m_isDone = false;
            bool flag = base.IsHandlingLastTaskList();
            for (int i = 0; i < base.m_targets.Count; i++)
            {
                GameObject obj2 = base.m_targets[i];
                int metaDataIndexForTarget = base.GetMetaDataIndexForTarget(i);
                Target item = new Target {
                    VisualTarget = obj2,
                    TargetPosition = obj2.transform.position,
                    MetaDataIdx = metaDataIndexForTarget,
                    isMinion = true
                };
                if (i == (base.m_targets.Count - 1))
                {
                    item.LastTarget = true;
                }
                if (flag)
                {
                    item.LastBlock = true;
                }
                this.m_TargetQueue.Add(item);
                if (!item.LastTarget)
                {
                    Target target2;
                    target2 = new Target {
                        TargetPosition = this.AcquireRandomBoardTarget(out target2.Offscreen),
                        isMinion = false,
                        LastTarget = false
                    };
                    this.m_TargetQueue.Add(target2);
                }
            }
        }
    }

    private void RampBladeVolume()
    {
        iTween.StopByName(this.m_BladeSpinning.gameObject, "BladeSpinningSound");
        SoundManager.Get().SetVolume(this.m_BladeSpinning, this.m_BladeSpinningMinVol);
        Action<object> action = amount => SoundManager.Get().SetVolume(this.m_BladeSpinning, (float) amount);
        object[] args = new object[] { "name", "BladeSpinningSound", "from", this.m_BladeSpinningMinVol, "to", this.m_BladeSpinningMaxVol, "time", this.m_BladeSpinningRampTime, "easetype", iTween.EaseType.linear, "onupdate", action, "onupdatetarget", this.m_BladeSpinning.gameObject };
        iTween.ValueTo(this.m_BladeSpinning.gameObject, iTween.Hash(args));
    }

    private void SetupBounceLocations()
    {
        Vector3 position = Board.Get().FindBone("CenterPointBone").transform.position;
        Vector3 localPosition = this.m_HitBonesRoot.transform.localPosition;
        this.m_HitBonesRoot.transform.position = position;
        foreach (HitBonesType type in this.m_HitBones)
        {
            type.SetPosition(type.Bone.transform.position);
        }
        this.m_HitBonesRoot.transform.localPosition = localPosition;
    }

    protected override void Start()
    {
        base.Start();
        Log.Kyle.Print("Start()", new object[0]);
        this.SetupBounceLocations();
    }

    [CompilerGenerated]
    private sealed class <BladeRunner>c__Iterator204 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BouncingBlade <>f__this;
        internal int <metaDataIdx>__1;
        internal BouncingBlade.Target <randomTarget>__2;
        internal BouncingBlade.Target <target>__0;

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
                    goto Label_04B5;

                case 1:
                    this.<>f__this.AnimateToNextTarget(this.<target>__0);
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0228;

                case 4:
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
                    this.<>f__this.m_BladeRoot.SetActive(true);
                    goto Label_04CC;

                case 5:
                    goto Label_03EE;

                case 6:
                    goto Label_0422;

                case 7:
                    goto Label_04A5;

                default:
                    goto Label_04CC;
            }
            if (this.<>f__this.m_Animating)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_04CE;
            }
            if (this.<metaDataIdx>__1 > 0)
            {
                this.$current = this.<>f__this.StartCoroutine(this.<>f__this.CompleteTasksFromMetaData(this.<metaDataIdx>__1, 0f));
                this.$PC = 3;
                goto Label_04CE;
            }
        Label_0228:
            if (this.<target>__0.LastBlock && this.<target>__0.LastTarget)
            {
                Log.Kyle.Print("BladeRunner() - Finish Spell", new object[0]);
                GameState.Get().SetUsingFastActorTriggers(false);
                this.<>f__this.m_EndSparkParticles.Play();
                this.<>f__this.m_EndBigSparkParticles.Play();
                this.<>f__this.m_Blade.SetActive(false);
                if (this.<>f__this.m_BladeSpinning != null)
                {
                    SoundManager.Get().Stop(this.<>f__this.m_BladeSpinning);
                }
                if (this.<>f__this.m_BladeSpinningContinuous != null)
                {
                    SoundManager.Get().Stop(this.<>f__this.m_BladeSpinningContinuous);
                }
                if (this.<>f__this.m_EndSound != null)
                {
                    SoundManager.Get().Play(this.<>f__this.m_EndSound);
                }
                this.$current = new WaitForSeconds(0.8f);
                this.$PC = 4;
                goto Label_04CE;
            }
            if (!this.<target>__0.LastBlock && this.<target>__0.LastTarget)
            {
                Log.Kyle.Print("BladeRunner() - Finish Action", new object[0]);
                this.<>f__this.m_effectsPendingFinish--;
                this.<>f__this.FinishIfPossible();
            }
            goto Label_03FE;
        Label_03EE:
            while (this.<>f__this.m_Animating)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_04CE;
            }
        Label_03FE:
            this.<>f__this.m_TargetQueue.RemoveAt(0);
            this.$current = null;
            this.$PC = 6;
            goto Label_04CE;
        Label_0422:
            if (this.<>f__this.m_TargetQueue.Count > 0)
            {
                if (!this.<>f__this.m_Blade.activeSelf)
                {
                    GameState.Get().SetUsingFastActorTriggers(true);
                    this.<>f__this.m_Blade.SetActive(true);
                    if (this.<>f__this.m_BladeSpinning != null)
                    {
                        this.<>f__this.m_BladeSpinning.gameObject.SetActive(true);
                        SoundManager.Get().Play(this.<>f__this.m_BladeSpinning);
                    }
                    if (this.<>f__this.m_BladeSpinningContinuous != null)
                    {
                        this.<>f__this.m_BladeSpinningContinuous.gameObject.SetActive(true);
                        SoundManager.Get().Play(this.<>f__this.m_BladeSpinningContinuous);
                    }
                    if (this.<>f__this.m_StartSound != null)
                    {
                        SoundManager.Get().Play(this.<>f__this.m_StartSound);
                    }
                }
                if (!this.<>f__this.m_Trail.activeSelf)
                {
                    this.<>f__this.m_Trail.SetActive(true);
                }
                this.<target>__0 = this.<>f__this.m_TargetQueue[0];
                if (!this.<target>__0.isMinion)
                {
                    this.<>f__this.AnimateToNextTarget(this.<target>__0);
                    goto Label_03EE;
                }
                this.<metaDataIdx>__1 = this.<target>__0.MetaDataIdx;
                this.$current = this.<>f__this.StartCoroutine(this.<>f__this.CompleteTasksUntilMetaData(this.<metaDataIdx>__1));
                this.$PC = 1;
                goto Label_04CE;
            }
            this.<randomTarget>__2 = new BouncingBlade.Target();
            this.<randomTarget>__2.TargetPosition = this.<>f__this.AcquireRandomBoardTarget(out this.<randomTarget>__2.Offscreen);
            this.<randomTarget>__2.isMinion = false;
            this.<randomTarget>__2.LastTarget = false;
            this.<>f__this.AnimateToNextTarget(this.<randomTarget>__2);
        Label_04A5:
            while (this.<>f__this.m_Animating)
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_04CE;
            }
        Label_04B5:
            if (!this.<>f__this.m_isDone)
            {
                goto Label_0422;
            }
            this.$PC = -1;
        Label_04CC:
            return false;
        Label_04CE:
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

    public enum HIT_DIRECTIONS
    {
        NW,
        NE,
        E,
        SW,
        SE,
        N_OFFSCREEN,
        E_OFFSCREEN,
        W_OFFSCREEN,
        S_OFFSCREEN
    }

    [Serializable]
    public class HitBonesType
    {
        public GameObject Bone;
        public BouncingBlade.HIT_DIRECTIONS Direction;
        private Vector3 m_Position;

        public Vector3 GetPosition()
        {
            return this.m_Position;
        }

        public void SetPosition(Vector3 pos)
        {
            this.m_Position = pos;
        }
    }

    [Serializable]
    public class Target
    {
        public bool isMinion;
        public bool LastBlock;
        public bool LastTarget;
        public int MetaDataIdx;
        public bool Offscreen;
        public Vector3 TargetPosition;
        public GameObject VisualTarget;
    }
}

