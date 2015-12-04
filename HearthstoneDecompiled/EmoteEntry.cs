using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class EmoteEntry
{
    private string m_emoteGameStringKey;
    private CardSoundSpell m_emoteSoundSpell;
    private string m_emoteSoundSpellPath;
    private EmoteType m_emoteType;
    private InitSpellFunc m_initSpellFunc;

    public EmoteEntry(EmoteType type, string path, string stringKey, InitSpellFunc func)
    {
        this.m_emoteType = type;
        this.m_emoteSoundSpellPath = path;
        this.m_emoteGameStringKey = stringKey;
        this.m_initSpellFunc = func;
    }

    public void Clear()
    {
        if (this.m_emoteSoundSpell != null)
        {
            UnityEngine.Object.Destroy(this.m_emoteSoundSpell.gameObject);
            this.m_emoteSoundSpell = null;
        }
    }

    public EmoteType GetEmoteType()
    {
        return this.m_emoteType;
    }

    public string GetGameStringKey()
    {
        return this.m_emoteGameStringKey;
    }

    public CardSoundSpell GetSpell(bool loadIfNeeded = true)
    {
        if ((this.m_emoteSoundSpell == null) && loadIfNeeded)
        {
            this.LoadSpell();
        }
        return this.m_emoteSoundSpell;
    }

    public CardSoundSpell GetSpellIfLoaded()
    {
        return this.m_emoteSoundSpell;
    }

    private void LoadSpell()
    {
        if (string.IsNullOrEmpty(this.m_emoteSoundSpellPath))
        {
            Debug.LogWarning(string.Format("Could not load emote of type {0} as no sound asset was specified", this.m_emoteType));
        }
        else
        {
            GameObject obj2 = AssetLoader.Get().LoadSpell(this.m_emoteSoundSpellPath, true, false);
            if (obj2 != null)
            {
                this.m_emoteSoundSpell = obj2.GetComponent<CardSoundSpell>();
            }
            if (this.m_emoteSoundSpell == null)
            {
                if (AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS == null)
                {
                    object[] messageArgs = new object[] { this.m_emoteSoundSpellPath, this.m_emoteType };
                    Error.AddDevFatal("EmoteEntry.LoadSpell() - FAILED TO LOAD \"{0}\" (emoteType {1})", messageArgs);
                }
            }
            else if (this.m_initSpellFunc != null)
            {
                this.m_initSpellFunc(this.m_emoteSoundSpell);
            }
        }
    }

    public delegate void InitSpellFunc(Spell spell);
}

