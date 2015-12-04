using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StageTransistion : MonoBehaviour
{
    private bool amountchange;
    private bool colorchange;
    public Color endColor;
    public GameObject entireObj;
    public GameObject flash;
    private bool flashchange;
    public Color flashendColor;
    public float fxATime = 1f;
    public GameObject fxEmitterA;
    public float FxEmitterAKillTime = 1f;
    private bool fxEmitterAScale;
    public float FxEmitterATimer = 2f;
    public float FxEmitterAWaitTime = 1f;
    public GameObject fxEmitterB;
    private bool fxmovefwd = true;
    private bool FxStartAnim;
    private bool FxStartStop;
    public GameObject hlBase;
    public GameObject hlEdge;
    public GameObject inplayObj;
    private bool powerchange;
    public GameObject rays;
    private bool rayschange;
    private bool raysdone;
    public float RayTime = 10f;
    private Shader shaderBucket;
    private int stage;
    private bool turnon;

    [DebuggerHidden]
    private IEnumerator destroyParticle()
    {
        return new <destroyParticle>c__Iterator28F { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator fxOnExit()
    {
        return new <fxOnExit>c__Iterator291 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator fxStartEnd()
    {
        return new <fxStartEnd>c__Iterator290 { <>f__this = this };
    }

    private void ManaUse()
    {
        this.colorchange = true;
    }

    private void OnGUI()
    {
        if (Event.current.isKey)
        {
            this.amountchange = true;
        }
    }

    private void OnMouseDown()
    {
        switch (this.stage)
        {
            case 0:
                this.ManaUse();
                break;

            case 1:
                this.RaysOn();
                break;
        }
        this.stage++;
    }

    private void OnMouseEnter()
    {
        if (!this.FxStartAnim)
        {
            base.StopCoroutine("fxOnExit");
            this.FxStartStop = false;
            this.FxStartAnim = true;
            this.fxmovefwd = true;
            base.StartCoroutine(this.fxStartEnd());
            this.fxEmitterA.GetComponent<ParticleEmitter>().emit = true;
            this.powerchange = true;
            this.fxEmitterAScale = true;
        }
    }

    private void OnMouseExit()
    {
        if (!this.FxStartStop)
        {
            base.StopCoroutine("fxStartEnd");
            this.FxStartAnim = false;
            this.FxStartStop = true;
            this.fxmovefwd = false;
            this.fxEmitterA.GetComponent<ParticleEmitter>().emit = true;
            base.StartCoroutine(this.fxOnExit());
        }
    }

    private void OnSelected()
    {
        base.StartCoroutine(this.destroyParticle());
    }

    private void RaysOn()
    {
        this.rays.SetActive(true);
        this.flash.SetActive(true);
        this.rayschange = true;
    }

    private void Start()
    {
        this.rays.SetActive(false);
        this.flash.SetActive(false);
        this.entireObj.SetActive(true);
        this.inplayObj.SetActive(false);
        this.hlBase.GetComponent<Renderer>().material.SetFloat("_Amount", 0f);
        this.hlEdge.GetComponent<Renderer>().material.SetFloat("_Amount", 0f);
    }

    private void Update()
    {
        if (this.amountchange)
        {
            float num = UnityEngine.Time.deltaTime / 0.5f;
            float num2 = num * 0.6954f;
            float num3 = num * 0.6954f;
            UnityEngine.Debug.Log("amount edge " + (this.hlEdge.GetComponent<Renderer>().material.GetFloat("_Amount") + num3));
            this.hlBase.GetComponent<Renderer>().material.SetFloat("_Amount", this.hlBase.GetComponent<Renderer>().material.GetFloat("_Amount") + num2);
            if (this.hlBase.GetComponent<Renderer>().material.GetFloat("_Amount") >= 0.6954f)
            {
                this.amountchange = false;
            }
            this.hlEdge.GetComponent<Renderer>().material.SetFloat("_Amount", this.hlEdge.GetComponent<Renderer>().material.GetFloat("_Amount") + num3);
        }
        if (this.colorchange)
        {
            float t = UnityEngine.Time.deltaTime / 0.5f;
            Color a = this.hlBase.GetComponent<Renderer>().material.color;
            this.hlBase.GetComponent<Renderer>().material.color = Color.Lerp(a, this.endColor, t);
        }
        if (this.powerchange)
        {
            float num6 = UnityEngine.Time.deltaTime / 0.5f;
            float num7 = num6 * 18f;
            float num8 = num6 * 0.6954f;
            this.hlBase.GetComponent<Renderer>().material.SetFloat("_power", this.hlBase.GetComponent<Renderer>().material.GetFloat("_power") + num7);
            if (this.hlBase.GetComponent<Renderer>().material.GetFloat("_power") >= 29f)
            {
                this.powerchange = false;
            }
            this.hlBase.GetComponent<Renderer>().material.SetFloat("_Amount", this.hlBase.GetComponent<Renderer>().material.GetFloat("_Amount") + num8);
            if (this.hlBase.GetComponent<Renderer>().material.GetFloat("_Amount") >= 1.12f)
            {
                this.amountchange = false;
            }
        }
        if (this.rayschange)
        {
            float num9 = UnityEngine.Time.deltaTime / 0.5f;
            float y = num9 * this.RayTime;
            Transform transform = this.rays.transform;
            transform.localScale += new Vector3(0f, y, 0f);
            if (!this.raysdone && (this.rays.transform.localScale.y >= 20f))
            {
                this.rays.SetActive(false);
                base.GetComponent<Renderer>().enabled = false;
                this.inplayObj.SetActive(true);
                this.inplayObj.GetComponent<Animation>().Play();
                this.fxEmitterB.GetComponent<ParticleEmitter>().emit = true;
                this.fxEmitterA.SetActive(false);
                this.raysdone = true;
            }
        }
        if (this.raysdone)
        {
            float num11 = this.flash.GetComponent<Renderer>().material.GetFloat("_InvFade") - UnityEngine.Time.deltaTime;
            this.flash.GetComponent<Renderer>().material.SetFloat("_InvFade", num11);
            UnityEngine.Debug.Log("InvFade " + num11);
            if (num11 <= 0.01f)
            {
                this.entireObj.SetActive(false);
            }
        }
        if (this.FxStartAnim || this.FxStartStop)
        {
            if (this.fxmovefwd)
            {
                UnityEngine.Particle[] particles = this.fxEmitterA.GetComponent<ParticleEmitter>().particles;
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].position += (Vector3) ((particles[i].position * UnityEngine.Time.deltaTime) / 0.2f);
                }
                this.fxEmitterA.GetComponent<ParticleEmitter>().particles = particles;
            }
            else
            {
                UnityEngine.Particle[] particleArray2 = this.fxEmitterA.GetComponent<ParticleEmitter>().particles;
                for (int j = 0; j < particleArray2.Length; j++)
                {
                    particleArray2[j].position -= (Vector3) ((particleArray2[j].position * UnityEngine.Time.deltaTime) / 0.2f);
                }
                this.fxEmitterA.GetComponent<ParticleEmitter>().particles = particleArray2;
            }
        }
        if (this.fxEmitterAScale)
        {
            float num14 = UnityEngine.Time.deltaTime / 0.5f;
            float x = num14 * this.fxATime;
            Transform transform2 = this.fxEmitterA.transform;
            transform2.localScale += new Vector3(x, x, x);
        }
    }

    [CompilerGenerated]
    private sealed class <destroyParticle>c__Iterator28F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal StageTransistion <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.FxEmitterAKillTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.fxEmitterA.GetComponent<ParticleEmitter>().emit = false;
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
    private sealed class <fxOnExit>c__Iterator291 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal StageTransistion <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.FxEmitterATimer);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.FxStartStop = false;
                    this.<>f__this.fxEmitterA.GetComponent<ParticleEmitter>().emit = false;
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
    private sealed class <fxStartEnd>c__Iterator290 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal StageTransistion <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.FxEmitterATimer);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.FxStartAnim = false;
                    this.<>f__this.fxEmitterA.GetComponent<ParticleEmitter>().emit = false;
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

