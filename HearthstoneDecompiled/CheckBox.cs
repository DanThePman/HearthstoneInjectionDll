using System;
using UnityEngine;

[CustomEditClass]
public class CheckBox : PegUIElement
{
    private int m_buttonID;
    public GameObject m_check;
    private bool m_checked;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_checkOffSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_checkOnSound;
    public TextMesh m_text;
    public UberText m_uberText;

    public int GetButtonID()
    {
        return this.m_buttonID;
    }

    public bool IsChecked()
    {
        return this.m_checked;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.SetState(PegUIElement.InteractionState.Up);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.SetState(PegUIElement.InteractionState.Over);
        }
    }

    protected override void OnPress()
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.SetState(PegUIElement.InteractionState.Down);
        }
    }

    protected override void OnRelease()
    {
        if (base.gameObject.activeInHierarchy)
        {
            this.ToggleChecked();
            if (this.m_checked && !string.IsNullOrEmpty(this.m_checkOnSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_checkOnSound));
            }
            else if (!this.m_checked && !string.IsNullOrEmpty(this.m_checkOffSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_checkOffSound));
            }
            this.SetState(PegUIElement.InteractionState.Over);
        }
    }

    public void SetButtonID(int id)
    {
        this.m_buttonID = id;
    }

    public void SetButtonText(string s)
    {
        if (this.m_text != null)
        {
            this.m_text.text = s;
        }
        if (this.m_uberText != null)
        {
            this.m_uberText.Text = s;
        }
    }

    public virtual void SetChecked(bool isChecked)
    {
        this.m_checked = isChecked;
        if (this.m_check != null)
        {
            this.m_check.SetActive(this.m_checked);
        }
    }

    public void SetState(PegUIElement.InteractionState state)
    {
        base.SetEnabled(true);
    }

    private bool ToggleChecked()
    {
        this.SetChecked(!this.m_checked);
        return this.m_checked;
    }
}

