using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CardEffect
{
    private InitSpellFunc m_initSoundSpell;
    private InitSpellFunc m_initSpell;
    private List<string> m_soundSpellPathes;
    private List<CardSoundSpell> m_soundSpells;
    private Spell m_spell;
    private string m_spellPath;

    public CardEffect(Spell spell)
    {
        this.OverrideSpell(spell);
    }

    public CardEffect(CardEffectDef def, InitSpellFunc spellFunc = null, InitSpellFunc soundSpellFunc = null)
    {
        this.m_spellPath = def.m_SpellPath;
        this.m_soundSpellPathes = def.m_SoundSpellPaths;
        this.m_initSpell = spellFunc;
        this.m_initSoundSpell = soundSpellFunc;
        if (this.m_soundSpellPathes != null)
        {
            this.m_soundSpells = new List<CardSoundSpell>(this.m_soundSpellPathes.Count);
            for (int i = 0; i < this.m_soundSpellPathes.Count; i++)
            {
                this.m_soundSpells.Add(null);
            }
        }
    }

    public CardEffect(string path, InitSpellFunc spellFunc = null, InitSpellFunc soundSpellFunc = null)
    {
        this.m_spellPath = path;
        this.m_initSpell = spellFunc;
        this.m_initSoundSpell = soundSpellFunc;
    }

    public CardEffect(string path, List<string> soundPathes, InitSpellFunc spellFunc = null, InitSpellFunc soundSpellFunc = null)
    {
        this.m_spellPath = path;
        this.m_soundSpellPathes = soundPathes;
        this.m_initSpell = spellFunc;
        this.m_initSoundSpell = soundSpellFunc;
        if (this.m_soundSpellPathes != null)
        {
            this.m_soundSpells = new List<CardSoundSpell>(this.m_soundSpellPathes.Count);
            for (int i = 0; i < this.m_soundSpellPathes.Count; i++)
            {
                this.m_soundSpells.Add(null);
            }
        }
    }

    public void Clear()
    {
        if (this.m_spell != null)
        {
            UnityEngine.Object.Destroy(this.m_spell.gameObject);
            this.m_spell = null;
        }
        if (this.m_soundSpells != null)
        {
            for (int i = 0; i < this.m_soundSpells.Count; i++)
            {
                Spell spell = this.m_soundSpells[i];
                if (spell != null)
                {
                    UnityEngine.Object.Destroy(spell.gameObject);
                    this.m_soundSpells[i] = null;
                }
            }
        }
    }

    public List<CardSoundSpell> GetSoundSpells(bool loadIfNeeded = true)
    {
        if (this.m_soundSpells == null)
        {
            return null;
        }
        if (loadIfNeeded)
        {
            for (int i = 0; i < this.m_soundSpells.Count; i++)
            {
                this.LoadSoundSpell(i);
            }
        }
        return this.m_soundSpells;
    }

    public Spell GetSpell(bool loadIfNeeded = true)
    {
        if (((this.m_spell == null) && !string.IsNullOrEmpty(this.m_spellPath)) && loadIfNeeded)
        {
            this.LoadSpell();
        }
        return this.m_spell;
    }

    public void LoadAll()
    {
        this.GetSpell(true);
        if (this.m_soundSpellPathes != null)
        {
            for (int i = 0; i < this.m_soundSpellPathes.Count; i++)
            {
                this.LoadSoundSpell(i);
            }
        }
    }

    public void LoadSoundSpell(int index)
    {
        if ((((index >= 0) && (this.m_soundSpellPathes != null)) && ((index < this.m_soundSpellPathes.Count) && !string.IsNullOrEmpty(this.m_soundSpellPathes[index]))) && (this.m_soundSpells[index] == null))
        {
            string name = this.m_soundSpellPathes[index];
            GameObject obj2 = AssetLoader.Get().LoadSpell(name, true, false);
            if (obj2 == null)
            {
                if (AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS == null)
                {
                    object[] messageArgs = new object[] { this.m_spellPath, index };
                    Error.AddDevFatal("CardEffect.LoadSoundSpell() - FAILED TO LOAD \"{0}\" (index {1})", messageArgs);
                }
            }
            else
            {
                this.m_soundSpells[index] = obj2.GetComponent<CardSoundSpell>();
                if (this.m_soundSpells[index] == null)
                {
                    if (AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS == null)
                    {
                        object[] objArray2 = new object[] { this.m_spellPath, index };
                        Error.AddDevFatal("CardEffect.LoadSoundSpell() - FAILED TO LOAD \"{0}\" (index {1})", objArray2);
                    }
                }
                else if (this.m_initSoundSpell != null)
                {
                    this.m_initSoundSpell(this.m_soundSpells[index]);
                }
            }
        }
    }

    private void LoadSpell()
    {
        this.m_spell = AssetLoader.Get().LoadSpell(this.m_spellPath, true, false).GetComponent<Spell>();
        if (this.m_spell == null)
        {
            object[] messageArgs = new object[] { this.m_spellPath };
            Error.AddDevFatal("CardEffect.LoadSpell() - FAILED TO LOAD \"{0}\"", messageArgs);
        }
        else if (this.m_initSpell != null)
        {
            this.m_initSpell(this.m_spell);
        }
    }

    public void OverrideSpell(Spell spell)
    {
        if (this.m_spell != null)
        {
            UnityEngine.Object.Destroy(this.m_spell);
        }
        this.m_spell = spell;
    }

    public void PurgeInactiveSpells()
    {
        this.TryPurge(this.m_spell);
        if (this.m_soundSpells != null)
        {
            for (int i = 0; i < this.m_soundSpells.Count; i++)
            {
                this.TryPurge(this.m_soundSpells[i]);
            }
        }
    }

    private void TryPurge(Spell spell)
    {
        if ((spell != null) && !spell.IsActive())
        {
            UnityEngine.Object.Destroy(spell);
        }
    }

    public delegate void InitSpellFunc(Spell spell);
}

