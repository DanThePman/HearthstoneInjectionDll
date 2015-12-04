using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CraftingModeButton : UIBButton
{
    public GameObject m_activeGlow;
    public Material m_disabledMaterial;
    public GameObject m_dustBottle;
    public ParticleSystem m_dustShower;
    public Material m_enabledMaterial;
    private bool m_isGlowEnabled;
    private bool m_isJiggling;
    public Vector3 m_jarJiggleRotation = new Vector3(0f, 30f, 0f);
    public MeshRenderer m_mainMesh;
    private bool m_showDustBottle;
    public GameObject m_textObject;

    private void BottleJiggle()
    {
        base.StartCoroutine(this.Jiggle());
    }

    public void Enable(bool enabled)
    {
        base.SetEnabled(enabled);
        this.m_activeGlow.SetActive(enabled && this.m_isGlowEnabled);
        this.m_textObject.SetActive(enabled);
        this.m_dustShower.gameObject.SetActive(enabled);
        if (enabled)
        {
            this.m_dustBottle.SetActive(this.m_showDustBottle);
        }
        else
        {
            this.m_dustBottle.SetActive(false);
        }
        this.m_mainMesh.sharedMaterial = !enabled ? this.m_disabledMaterial : this.m_enabledMaterial;
    }

    [DebuggerHidden]
    private IEnumerator Jiggle()
    {
        return new <Jiggle>c__Iterator46 { <>f__this = this };
    }

    public void ShowActiveGlow(bool show)
    {
        this.m_isGlowEnabled = show;
        this.m_activeGlow.SetActive(show);
    }

    public void ShowDustBottle(bool show)
    {
        this.m_showDustBottle = show;
        this.m_dustBottle.SetActive(show);
        if (show)
        {
            this.StartBottleJiggle();
        }
    }

    private void StartBottleJiggle()
    {
        base.StopCoroutine("Jiggle");
        iTween.Stop(this.m_dustBottle.gameObject);
        this.BottleJiggle();
    }

    [CompilerGenerated]
    private sealed class <Jiggle>c__Iterator46 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingModeButton <>f__this;
        internal Hashtable <args>__0;

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
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    this.<>f__this.m_dustShower.Play();
                    object[] args = new object[] { "amount", this.<>f__this.m_jarJiggleRotation, "time", 0.5f, "oncomplete", "BottleJiggle", "oncompletetarget", this.<>f__this.gameObject };
                    this.<args>__0 = iTween.Hash(args);
                    iTween.PunchRotation(this.<>f__this.m_dustBottle.gameObject, this.<args>__0);
                    this.$PC = -1;
                    break;
                }
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

