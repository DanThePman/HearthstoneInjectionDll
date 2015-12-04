using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ManaFilterTab : PegUIElement
{
    public static readonly int ALL_TAB_IDX = -1;
    public UberText m_costText;
    public ManaCrystal m_crystal;
    private FilterState m_filterState;
    private int m_manaID;
    private AudioSource m_mouseOverSound;
    public UberText m_otherText;
    public static readonly int SEVEN_PLUS_TAB_IDX = 7;

    protected override void Awake()
    {
        this.m_crystal.MarkAsNotInGame();
        base.Awake();
    }

    public int GetManaID()
    {
        return this.m_manaID;
    }

    private void ManaCrystalSoundCallback(AudioSource source, object userData)
    {
        <ManaCrystalSoundCallback>c__AnonStorey2E4 storeye = new <ManaCrystalSoundCallback>c__AnonStorey2E4 {
            source = source
        };
        if (this.m_mouseOverSound != null)
        {
            SoundManager.Get().Stop(this.m_mouseOverSound);
        }
        this.m_mouseOverSound = storeye.source;
        SoundManager.Get().SetVolume(storeye.source, 0f);
        if (this.m_crystal.state != ManaCrystal.State.PROPOSED)
        {
            SoundManager.Get().Stop(this.m_mouseOverSound);
        }
        Action<object> action = new Action<object>(storeye.<>m__C7);
        object[] args = new object[] { "from", 0f, "to", 1f, "time", 0.5f, "easetype", iTween.EaseType.linear, "onupdate", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Stop(base.gameObject);
        iTween.ValueTo(base.gameObject, hashtable);
    }

    public void NotifyMousedOut()
    {
        Action<object> action = amount => SoundManager.Get().SetVolume(this.m_mouseOverSound, (float) amount);
        object[] args = new object[] { "from", 1f, "to", 0f, "time", 0.5f, "easetype", iTween.EaseType.linear, "onupdate", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Stop(base.gameObject);
        iTween.ValueTo(base.gameObject, hashtable);
        if (this.m_filterState != FilterState.ON)
        {
            this.m_crystal.state = ManaCrystal.State.READY;
        }
    }

    public void NotifyMousedOver()
    {
        if (this.m_filterState != FilterState.ON)
        {
            this.m_crystal.state = ManaCrystal.State.PROPOSED;
            SoundManager.Get().LoadAndPlay("mana_crystal_highlight_lp", base.gameObject, 1f, new SoundManager.LoadedCallback(this.ManaCrystalSoundCallback));
        }
    }

    public void SetFilterState(FilterState state)
    {
        this.m_filterState = state;
        switch (this.m_filterState)
        {
            case FilterState.ON:
                this.m_crystal.state = ManaCrystal.State.PROPOSED;
                break;

            case FilterState.OFF:
                this.m_crystal.state = ManaCrystal.State.READY;
                break;

            case FilterState.DISABLED:
                this.m_crystal.state = ManaCrystal.State.USED;
                break;
        }
    }

    public void SetManaID(int manaID)
    {
        this.m_manaID = manaID;
        this.UpdateManaText();
    }

    private void UpdateManaText()
    {
        string str = string.Empty;
        string str2 = string.Empty;
        if (this.m_manaID == ALL_TAB_IDX)
        {
            str2 = GameStrings.Get("GLUE_COLLECTION_ALL");
        }
        else
        {
            str = this.m_manaID.ToString();
            if (this.m_manaID == SEVEN_PLUS_TAB_IDX)
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    str = str + GameStrings.Get("GLUE_COLLECTION_PLUS");
                }
                else
                {
                    str2 = GameStrings.Get("GLUE_COLLECTION_PLUS");
                }
            }
        }
        if (this.m_costText != null)
        {
            this.m_costText.Text = str;
        }
        if (this.m_otherText != null)
        {
            this.m_otherText.Text = str2;
        }
    }

    [CompilerGenerated]
    private sealed class <ManaCrystalSoundCallback>c__AnonStorey2E4
    {
        internal AudioSource source;

        internal void <>m__C7(object amount)
        {
            SoundManager.Get().SetVolume(this.source, (float) amount);
        }
    }

    public enum FilterState
    {
        ON,
        OFF,
        DISABLED
    }
}

