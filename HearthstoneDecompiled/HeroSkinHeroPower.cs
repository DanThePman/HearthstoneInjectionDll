using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroSkinHeroPower : MonoBehaviour
{
    public Actor m_Actor;
    public Texture m_OriginalBackTexture;
    public Texture m_OriginalFrontTexture;

    public Texture GetBackTexture()
    {
        return base.GetComponent<Renderer>().materials[2].mainTexture;
    }

    public Texture GetFrontTexture()
    {
        return base.GetComponent<Renderer>().materials[0].mainTexture;
    }

    [DebuggerHidden]
    private IEnumerator HeroSkinCustomHeroPowerTextures()
    {
        return new <HeroSkinCustomHeroPowerTextures>c__Iterator25D { <>f__this = this };
    }

    private void OnBackTextureLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        Texture tex = obj as Texture2D;
        this.SetBackTexture(tex);
    }

    private void OnFrontTextureLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        Texture tex = obj as Texture2D;
        this.SetFrontTexture(tex);
    }

    public void RestoreOriginalTextures()
    {
        this.SetFrontTexture(this.m_OriginalFrontTexture);
        this.SetBackTexture(this.m_OriginalBackTexture);
    }

    public void SetBackTexture(Texture tex)
    {
        Renderer component = base.GetComponent<Renderer>();
        Material[] materials = component.materials;
        materials[1].SetTexture("_SecondTex", tex);
        materials[2].mainTexture = tex;
        component.materials = materials;
    }

    public void SetBackTextureFromPath(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            AssetLoader.Get().LoadTexture(path, new AssetLoader.ObjectCallback(this.OnBackTextureLoaded), null, false);
        }
    }

    public void SetFrontTexture(Texture tex)
    {
        Renderer component = base.GetComponent<Renderer>();
        Material[] materials = component.materials;
        materials[0].mainTexture = tex;
        component.materials = materials;
    }

    public void SetFrontTextureFromPath(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            AssetLoader.Get().LoadTexture(path, new AssetLoader.ObjectCallback(this.OnFrontTextureLoaded), null, false);
        }
    }

    private void Start()
    {
        if (SceneMgr.Get().IsInGame())
        {
            base.StartCoroutine(this.HeroSkinCustomHeroPowerTextures());
        }
    }

    [CompilerGenerated]
    private sealed class <HeroSkinCustomHeroPowerTextures>c__Iterator25D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroSkinHeroPower <>f__this;
        internal Card <card>__0;
        internal Card <heroCard>__1;
        internal CardDef <heroCardDef>__2;

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
                    this.<card>__0 = this.<>f__this.m_Actor.GetCard();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00BE;

                default:
                    goto Label_0107;
            }
            if (this.<card>__0 == null)
            {
                this.<card>__0 = this.<>f__this.m_Actor.GetCard();
                this.$current = 0;
                this.$PC = 1;
                goto Label_0109;
            }
            this.<heroCard>__1 = this.<card>__0.GetHeroCard();
        Label_00BE:
            while (this.<heroCard>__1 == null)
            {
                this.<heroCard>__1 = this.<card>__0.GetHeroCard();
                this.$current = 0;
                this.$PC = 2;
                goto Label_0109;
            }
            this.<heroCardDef>__2 = this.<heroCard>__1.GetCardDef();
            if (this.<heroCardDef>__2 == null)
            {
                UnityEngine.Debug.LogWarning("HeroSkinHeroPower: heroCardDef is null!");
            }
            else
            {
                this.$PC = -1;
            }
        Label_0107:
            return false;
        Label_0109:
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

