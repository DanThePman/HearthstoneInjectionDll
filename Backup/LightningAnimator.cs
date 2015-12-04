using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class LightningAnimator : MonoBehaviour
{
    public List<int> m_FrameList;
    public float m_FrameTime = 0.01f;
    private Material m_material;
    public string m_MatFrameProperty = "_Frame";
    private float m_matGlowIntensity;
    public bool m_SetAlphaToZeroOnStart = true;
    public Transform m_SourceJount;
    public Vector3 m_SourceMaxRotation = new Vector3(0f, 10f, 0f);
    public Vector3 m_SourceMinRotation = new Vector3(0f, -10f, 0f);
    public float m_StartDelayMax;
    public float m_StartDelayMin;
    public bool m_StartOnEnable;
    public Transform m_TargetJoint;
    public Vector3 m_TargetMaxRotation = new Vector3(0f, 20f, 0f);
    public Vector3 m_TargetMinRotation = new Vector3(0f, -20f, 0f);

    [DebuggerHidden]
    private IEnumerator AnimateMaterial()
    {
        return new <AnimateMaterial>c__Iterator284 { <>f__this = this };
    }

    private void OnEnable()
    {
        if (this.m_StartOnEnable)
        {
            this.StartAnimation();
        }
    }

    private void RandomJointRotation()
    {
        if (this.m_SourceJount != null)
        {
            Vector3 eulerAngles = Vector3.Lerp(this.m_SourceMinRotation, this.m_SourceMaxRotation, UnityEngine.Random.value);
            this.m_SourceJount.Rotate(eulerAngles);
        }
        if (this.m_TargetJoint != null)
        {
            Vector3 vector2 = Vector3.Lerp(this.m_TargetMinRotation, this.m_TargetMaxRotation, UnityEngine.Random.value);
            this.m_TargetJoint.Rotate(vector2);
        }
    }

    private void Start()
    {
        this.m_material = base.GetComponent<Renderer>().material;
        if (this.m_material == null)
        {
            base.enabled = false;
        }
        if (this.m_SetAlphaToZeroOnStart)
        {
            Color color = this.m_material.color;
            color.a = 0f;
            this.m_material.color = color;
        }
        if (this.m_material.HasProperty("_GlowIntensity"))
        {
            this.m_matGlowIntensity = this.m_material.GetFloat("_GlowIntensity");
        }
    }

    public void StartAnimation()
    {
        base.StartCoroutine(this.AnimateMaterial());
    }

    [CompilerGenerated]
    private sealed class <AnimateMaterial>c__Iterator284 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<int>.Enumerator <$s_1813>__1;
        internal LightningAnimator <>f__this;
        internal int <frame>__2;
        internal Color <matColor>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 2:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1813>__1.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    this.<>f__this.RandomJointRotation();
                    this.<matColor>__0 = this.<>f__this.m_material.color;
                    this.<matColor>__0.a = 0f;
                    this.<>f__this.m_material.color = this.<matColor>__0;
                    this.$current = new WaitForSeconds(UnityEngine.Random.Range(this.<>f__this.m_StartDelayMin, this.<>f__this.m_StartDelayMax));
                    this.$PC = 1;
                    goto Label_0224;

                case 1:
                    this.<matColor>__0 = this.<>f__this.m_material.color;
                    this.<matColor>__0.a = 1f;
                    this.<>f__this.m_material.color = this.<matColor>__0;
                    if (this.<>f__this.m_material.HasProperty("_GlowIntensity"))
                    {
                        this.<>f__this.m_material.SetFloat("_GlowIntensity", this.<>f__this.m_matGlowIntensity);
                    }
                    this.<$s_1813>__1 = this.<>f__this.m_FrameList.GetEnumerator();
                    num = 0xfffffffd;
                    break;

                case 2:
                    break;

                default:
                    goto Label_0222;
            }
            try
            {
                while (this.<$s_1813>__1.MoveNext())
                {
                    this.<frame>__2 = this.<$s_1813>__1.Current;
                    this.<>f__this.m_material.SetFloat(this.<>f__this.m_MatFrameProperty, (float) this.<frame>__2);
                    this.$current = new WaitForSeconds(this.<>f__this.m_FrameTime);
                    this.$PC = 2;
                    flag = true;
                    goto Label_0224;
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1813>__1.Dispose();
            }
            this.<matColor>__0.a = 0f;
            this.<>f__this.m_material.color = this.<matColor>__0;
            if (this.<>f__this.m_material.HasProperty("_GlowIntensity"))
            {
                this.<>f__this.m_material.SetFloat("_GlowIntensity", 0f);
            }
            this.$PC = -1;
        Label_0222:
            return false;
        Label_0224:
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

