using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Shadowform : SuperSpell
{
    public float m_Contrast = -0.29f;
    public float m_Desaturate = 0.8f;
    public float m_FadeInTime = 1f;
    public float m_FxIntensity = 4f;
    public float m_Intensity = 0.85f;
    public int m_MaterialIndex = 1;
    private Material m_MaterialInstance;
    public Material m_ShadowformMaterial;
    public Color m_Tint = new Color(0.6914063f, 0.328125f, 0.8046875f, 1f);

    protected override void OnBirth(SpellStateType prevStateType)
    {
        <OnBirth>c__AnonStorey351 storey = new <OnBirth>c__AnonStorey351();
        base.OnBirth(prevStateType);
        if (this.m_ShadowformMaterial != null)
        {
            Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(this);
            actor.SetShadowform(true);
            this.m_MaterialInstance = new Material(this.m_ShadowformMaterial);
            Texture portraitTexture = actor.GetPortraitTexture();
            this.m_MaterialInstance.mainTexture = portraitTexture;
            actor.SetPortraitMaterial(this.m_MaterialInstance);
            this.OnSpellFinished();
            GameObject portraitMesh = actor.GetPortraitMesh();
            storey.mat = portraitMesh.GetComponent<Renderer>().materials[actor.m_portraitMatIdx];
            Action<object> action = new Action<object>(storey.<>m__1D1);
            object[] args = new object[] { "time", this.m_FadeInTime, "from", 0f, "to", this.m_Desaturate, "onupdate", action, "onupdatetarget", actor.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(actor.gameObject, hashtable);
            Action<object> action2 = new Action<object>(storey.<>m__1D2);
            object[] objArray2 = new object[] { "time", this.m_FadeInTime, "from", Color.white, "to", this.m_Tint, "onupdate", action2, "onupdatetarget", actor.gameObject };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.ValueTo(actor.gameObject, hashtable2);
            Action<object> action3 = new Action<object>(storey.<>m__1D3);
            object[] objArray3 = new object[] { "time", this.m_FadeInTime, "from", 0f, "to", this.m_Contrast, "onupdate", action3, "onupdatetarget", actor.gameObject };
            Hashtable hashtable3 = iTween.Hash(objArray3);
            iTween.ValueTo(actor.gameObject, hashtable3);
            Action<object> action4 = new Action<object>(storey.<>m__1D4);
            object[] objArray4 = new object[] { "time", this.m_FadeInTime, "from", 1f, "to", this.m_Intensity, "onupdate", action4, "onupdatetarget", actor.gameObject };
            Hashtable hashtable4 = iTween.Hash(objArray4);
            iTween.ValueTo(actor.gameObject, hashtable4);
            Action<object> action5 = new Action<object>(storey.<>m__1D5);
            object[] objArray5 = new object[] { "time", this.m_FadeInTime, "from", 0f, "to", this.m_FxIntensity, "onupdate", action5, "onupdatetarget", actor.gameObject };
            Hashtable hashtable5 = iTween.Hash(objArray5);
            iTween.ValueTo(actor.gameObject, hashtable5);
        }
    }

    protected override void OnDeath(SpellStateType prevStateType)
    {
        base.OnDeath(prevStateType);
        Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(this);
        actor.SetShadowform(false);
        actor.UpdateAllComponents();
    }

    [CompilerGenerated]
    private sealed class <OnBirth>c__AnonStorey351
    {
        internal Material mat;

        internal void <>m__1D1(object desat)
        {
            this.mat.SetFloat("_Desaturate", (float) desat);
        }

        internal void <>m__1D2(object col)
        {
            this.mat.SetColor("_Color", (Color) col);
        }

        internal void <>m__1D3(object desat)
        {
            this.mat.SetFloat("_Contrast", (float) desat);
        }

        internal void <>m__1D4(object desat)
        {
            this.mat.SetFloat("_Intensity", (float) desat);
        }

        internal void <>m__1D5(object desat)
        {
            this.mat.SetFloat("_FxIntensity", (float) desat);
        }
    }
}

