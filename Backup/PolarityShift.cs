using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PolarityShift : SuperSpell
{
    private float m_AnimTime;
    public float m_CleanupTime = 2f;
    public ParticleSystem m_GlowParticle;
    public AnimationCurve m_HeightCurve;
    private float m_HeightCurveLength;
    public ParticleSystem m_ImpactParticle;
    public ParticleSystem m_LightningParticle;
    public ParticleEffects m_ParticleEffects;
    public float m_ParticleHeightOffset = 0.1f;
    public float m_RotationDriftAmount;
    public AnimationCurve m_RotationDriftCurve;
    private AudioSource m_Sound;
    public float m_SpellFinishTime = 2f;

    protected override void Awake()
    {
        this.m_Sound = base.GetComponent<AudioSource>();
        base.Awake();
    }

    [DebuggerHidden]
    private IEnumerator DoSpellFinished()
    {
        return new <DoSpellFinished>c__Iterator220 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator MinionAnimation(List<MinionData> minions)
    {
        return new <MinionAnimation>c__Iterator221 { minions = minions, <$>minions = minions, <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        if (this.m_HeightCurve.length == 0)
        {
            UnityEngine.Debug.LogWarning("PolarityShift Spell height animation curve in not defined");
            base.OnAction(prevStateType);
        }
        else if (this.m_RotationDriftCurve.length == 0)
        {
            UnityEngine.Debug.LogWarning("PolarityShift Spell rotation drift animation curve in not defined");
            base.OnAction(prevStateType);
        }
        else
        {
            base.m_effectsPendingFinish++;
            base.OnAction(prevStateType);
            this.m_HeightCurveLength = this.m_HeightCurve.keys[this.m_HeightCurve.length - 1].time;
            this.m_ParticleEffects.m_ParticleSystems.Clear();
            List<MinionData> minions = new List<MinionData>();
            foreach (GameObject obj2 in base.GetTargets())
            {
                MinionData item = new MinionData {
                    gameObject = obj2,
                    orgLocPos = obj2.transform.localPosition,
                    orgLocRot = obj2.transform.localRotation
                };
                float x = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value);
                float y = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value) * 0.1f;
                float z = Mathf.Lerp(-this.m_RotationDriftAmount, this.m_RotationDriftAmount, UnityEngine.Random.value);
                item.rotationDrift = new Vector3(x, y, z);
                item.glowParticle = UnityEngine.Object.Instantiate<ParticleSystem>(this.m_GlowParticle);
                item.glowParticle.transform.position = obj2.transform.position;
                item.glowParticle.transform.Translate(0f, this.m_ParticleHeightOffset, 0f, Space.World);
                item.lightningParticle = UnityEngine.Object.Instantiate<ParticleSystem>(this.m_LightningParticle);
                item.lightningParticle.transform.position = obj2.transform.position;
                item.lightningParticle.transform.Translate(0f, this.m_ParticleHeightOffset, 0f, Space.World);
                item.impactParticle = UnityEngine.Object.Instantiate<ParticleSystem>(this.m_ImpactParticle);
                item.impactParticle.transform.position = obj2.transform.position;
                item.impactParticle.transform.Translate(0f, this.m_ParticleHeightOffset, 0f, Space.World);
                this.m_ParticleEffects.m_ParticleSystems.Add(item.lightningParticle);
                if (this.m_Sound != null)
                {
                    SoundManager.Get().Play(this.m_Sound);
                }
                minions.Add(item);
            }
            base.StartCoroutine(this.DoSpellFinished());
            base.StartCoroutine(this.MinionAnimation(minions));
        }
    }

    private void ShakeCamera()
    {
        CameraShakeMgr.Shake(Camera.main, new Vector3(0.1f, 0.1f, 0.1f), 0.75f);
    }

    [CompilerGenerated]
    private sealed class <DoSpellFinished>c__Iterator220 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PolarityShift <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_SpellFinishTime);
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <MinionAnimation>c__Iterator221 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<PolarityShift.MinionData> <$>minions;
        internal List<PolarityShift.MinionData>.Enumerator <$s_1248>__0;
        internal List<PolarityShift.MinionData>.Enumerator <$s_1249>__4;
        internal List<PolarityShift.MinionData>.Enumerator <$s_1250>__6;
        internal List<PolarityShift.MinionData>.Enumerator <$s_1251>__10;
        internal PolarityShift <>f__this;
        internal FullScreenEffects <fsfx>__9;
        internal float <height>__2;
        internal Camera <mainCamera>__8;
        internal PolarityShift.MinionData <minion>__1;
        internal PolarityShift.MinionData <minion>__11;
        internal PolarityShift.MinionData <minion>__5;
        internal PolarityShift.MinionData <minion>__7;
        internal float <rotAmount>__3;
        internal List<PolarityShift.MinionData> minions;

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
                    this.<$s_1248>__0 = this.minions.GetEnumerator();
                    try
                    {
                        while (this.<$s_1248>__0.MoveNext())
                        {
                            this.<minion>__1 = this.<$s_1248>__0.Current;
                            this.<minion>__1.glowParticle.Play();
                        }
                    }
                    finally
                    {
                        this.<$s_1248>__0.Dispose();
                    }
                    this.<>f__this.m_AnimTime = 0f;
                    break;

                case 1:
                    break;

                case 2:
                    this.<fsfx>__9.BlendToColorAmount = 0.67f;
                    this.$current = null;
                    this.$PC = 3;
                    goto Label_04B5;

                case 3:
                    this.<fsfx>__9.BlendToColorAmount = 0.33f;
                    this.$current = null;
                    this.$PC = 4;
                    goto Label_04B5;

                case 4:
                    this.<fsfx>__9.BlendToColorAmount = 0f;
                    this.<fsfx>__9.BlendToColorEnable = false;
                    this.<fsfx>__9.Disable();
                    goto Label_03CD;

                case 5:
                    this.<>f__this.m_ParticleEffects.m_ParticleSystems.Clear();
                    this.<$s_1251>__10 = this.minions.GetEnumerator();
                    try
                    {
                        while (this.<$s_1251>__10.MoveNext())
                        {
                            this.<minion>__11 = this.<$s_1251>__10.Current;
                            UnityEngine.Object.Destroy(this.<minion>__11.glowParticle.gameObject);
                            UnityEngine.Object.Destroy(this.<minion>__11.lightningParticle.gameObject);
                            UnityEngine.Object.Destroy(this.<minion>__11.impactParticle.gameObject);
                        }
                    }
                    finally
                    {
                        this.<$s_1251>__10.Dispose();
                    }
                    goto Label_04A1;

                default:
                    goto Label_04B3;
            }
            while (this.<>f__this.m_AnimTime < this.<>f__this.m_HeightCurveLength)
            {
                this.<>f__this.m_AnimTime += UnityEngine.Time.deltaTime;
                this.<height>__2 = this.<>f__this.m_HeightCurve.Evaluate(this.<>f__this.m_AnimTime);
                this.<rotAmount>__3 = this.<>f__this.m_RotationDriftCurve.Evaluate(this.<>f__this.m_AnimTime);
                this.<$s_1249>__4 = this.minions.GetEnumerator();
                try
                {
                    while (this.<$s_1249>__4.MoveNext())
                    {
                        this.<minion>__5 = this.<$s_1249>__4.Current;
                        this.<minion>__5.gameObject.transform.localPosition = new Vector3(this.<minion>__5.orgLocPos.x, this.<minion>__5.orgLocPos.y + this.<height>__2, this.<minion>__5.orgLocPos.z);
                        this.<minion>__5.gameObject.transform.localRotation = this.<minion>__5.orgLocRot;
                        this.<minion>__5.gameObject.transform.Rotate((Vector3) (this.<minion>__5.rotationDrift * this.<rotAmount>__3), Space.Self);
                    }
                }
                finally
                {
                    this.<$s_1249>__4.Dispose();
                }
                this.$current = null;
                this.$PC = 1;
                goto Label_04B5;
            }
            this.<$s_1250>__6 = this.minions.GetEnumerator();
            try
            {
                while (this.<$s_1250>__6.MoveNext())
                {
                    this.<minion>__7 = this.<$s_1250>__6.Current;
                    this.<minion>__7.impactParticle.Play();
                    this.<minion>__7.lightningParticle.Play();
                    MinionShake.ShakeObject(this.<minion>__7.gameObject, ShakeMinionType.RandomDirection, this.<minion>__7.gameObject.transform.position, ShakeMinionIntensity.MediumShake, 0f, 0f, 0f);
                }
            }
            finally
            {
                this.<$s_1250>__6.Dispose();
            }
            this.<mainCamera>__8 = Camera.main;
            if ((this.<mainCamera>__8 != null) && (this.minions.Count > 0))
            {
                this.<>f__this.ShakeCamera();
                this.<fsfx>__9 = this.<mainCamera>__8.GetComponent<FullScreenEffects>();
                if ((this.<fsfx>__9 != null) && !this.<fsfx>__9.isActive())
                {
                    this.<fsfx>__9.BlendToColorEnable = true;
                    this.<fsfx>__9.BlendToColorAmount = 1f;
                    this.<fsfx>__9.BlendToColor = Color.white;
                    this.$current = null;
                    this.$PC = 2;
                    goto Label_04B5;
                }
            }
        Label_03CD:
            if (this.minions.Count > 0)
            {
                this.$current = new WaitForSeconds(this.<>f__this.m_CleanupTime);
                this.$PC = 5;
                goto Label_04B5;
            }
        Label_04A1:
            this.<>f__this.OnStateFinished();
            this.$PC = -1;
        Label_04B3:
            return false;
        Label_04B5:
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

    public class MinionData
    {
        public GameObject gameObject;
        public ParticleSystem glowParticle;
        public ParticleSystem impactParticle;
        public ParticleSystem lightningParticle;
        public Vector3 orgLocPos;
        public Quaternion orgLocRot;
        public Vector3 rotationDrift;
    }
}

