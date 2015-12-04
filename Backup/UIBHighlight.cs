using System;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class UIBHighlight : MonoBehaviour
{
    [CustomEditField(Sections="Allow Selection", Label="Enable")]
    public bool m_AllowSelection;
    [SerializeField]
    private bool m_AlwaysOver;
    [SerializeField]
    private bool m_EnableResponse = true;
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_HideMouseOverOnPress;
    [CustomEditField(Sections="Highlight Objects")]
    public GameObject m_MouseDownHighlight;
    [CustomEditField(Sections="Highlight Sounds", T=EditType.SOUND_PREFAB)]
    public string m_MouseDownSound = "Assets/Game/Sounds/Interface/Small_Click";
    [CustomEditField(Sections="Highlight Sounds", T=EditType.SOUND_PREFAB)]
    public string m_MouseOutSound;
    [CustomEditField(Sections="Highlight Objects")]
    public GameObject m_MouseOverHighlight;
    [CustomEditField(Parent="m_AllowSelection")]
    public GameObject m_MouseOverSelectedHighlight;
    [CustomEditField(Sections="Highlight Sounds", T=EditType.SOUND_PREFAB)]
    public string m_MouseOverSound = "Assets/Game/Sounds/Interface/Small_Mouseover";
    [CustomEditField(Sections="Highlight Objects")]
    public GameObject m_MouseUpHighlight;
    [CustomEditField(Sections="Highlight Sounds", T=EditType.SOUND_PREFAB)]
    public string m_MouseUpSound;
    private PegUIElement m_PegUIElement;
    [CustomEditField(Parent="m_AllowSelection")]
    public GameObject m_SelectedHighlight;
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_SelectOnRelease;

    private void Awake()
    {
        PegUIElement component = base.gameObject.GetComponent<PegUIElement>();
        if (component != null)
        {
            component.AddEventListener(UIEventType.ROLLOVER, e => this.OnRollOver(false));
            component.AddEventListener(UIEventType.PRESS, e => this.OnPress(true));
            component.AddEventListener(UIEventType.RELEASE, e => this.OnRelease());
            component.AddEventListener(UIEventType.ROLLOUT, e => this.OnRollOut(false));
            this.ResetState();
        }
    }

    public void HighlightOnce()
    {
        this.OnRollOver(true);
    }

    private void OnPress()
    {
        this.OnPress(true);
    }

    private void OnPress(bool playSound)
    {
        if (this.m_EnableResponse)
        {
            if (playSound)
            {
                this.PlaySound(this.m_MouseDownSound);
            }
            if (this.m_AllowSelection && !this.m_SelectOnRelease)
            {
                this.ShowHighlightObject(this.m_SelectedHighlight, true);
                this.ShowHighlightObject(this.m_MouseOverSelectedHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, false);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
            }
            else
            {
                this.ShowHighlightObject(this.m_MouseDownHighlight, true);
                this.ShowHighlightObject(this.m_MouseOverHighlight, this.m_AlwaysOver || !this.m_HideMouseOverOnPress);
                this.ShowHighlightObject(this.m_MouseUpHighlight, !this.m_AlwaysOver);
            }
        }
    }

    private void OnRelease()
    {
        this.OnRelease(true);
    }

    private void OnRelease(bool playSound)
    {
        if (this.m_EnableResponse)
        {
            if (playSound)
            {
                this.PlaySound(this.m_MouseUpSound);
            }
            if (this.m_AllowSelection && this.m_SelectOnRelease)
            {
                this.ShowHighlightObject(this.m_SelectedHighlight, true);
                this.ShowHighlightObject(this.m_MouseOverSelectedHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, false);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
            }
            else
            {
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, true);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
            }
        }
    }

    private void OnRollOut(bool force = false)
    {
        if (this.m_EnableResponse || force)
        {
            this.PlaySound(this.m_MouseOutSound);
            if (this.m_AllowSelection && ((this.m_MouseOverSelectedHighlight == null) || this.m_MouseOverSelectedHighlight.activeSelf))
            {
                this.ShowHighlightObject(this.m_SelectedHighlight, true);
                this.ShowHighlightObject(this.m_MouseOverSelectedHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, false);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
            }
            else
            {
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, this.m_AlwaysOver);
                this.ShowHighlightObject(this.m_MouseUpHighlight, !this.m_AlwaysOver);
            }
        }
    }

    private void OnRollOver(bool force = false)
    {
        if (this.m_EnableResponse || force)
        {
            if (!this.m_AlwaysOver)
            {
                this.PlaySound(this.m_MouseOverSound);
            }
            if (this.m_AllowSelection && ((this.m_SelectedHighlight == null) || this.m_SelectedHighlight.activeSelf))
            {
                this.ShowHighlightObject(this.m_MouseOverSelectedHighlight, true);
                this.ShowHighlightObject(this.m_SelectedHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, false);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
            }
            else
            {
                this.ShowHighlightObject(this.m_MouseDownHighlight, false);
                this.ShowHighlightObject(this.m_MouseOverHighlight, true);
                this.ShowHighlightObject(this.m_MouseUpHighlight, false);
            }
        }
    }

    private void PlaySound(string soundFilePath)
    {
        if (!string.IsNullOrEmpty(soundFilePath))
        {
            string soundName = FileUtils.GameAssetPathToName(soundFilePath);
            SoundManager.Get().LoadAndPlay(soundName);
        }
    }

    public void Reset()
    {
        this.ResetState();
        this.ShowHighlightObject(this.m_SelectedHighlight, false);
        this.ShowHighlightObject(this.m_MouseOverSelectedHighlight, false);
        this.ShowHighlightObject(this.m_MouseOverHighlight, false);
    }

    private void ResetState()
    {
        if (this.m_AlwaysOver)
        {
            this.OnRollOver(true);
        }
        else
        {
            this.OnRollOut(true);
        }
    }

    public void Select()
    {
        if (this.m_SelectOnRelease)
        {
            this.OnRelease(true);
        }
        else
        {
            this.OnPress(true);
        }
    }

    public void SelectNoSound()
    {
        if (this.m_SelectOnRelease)
        {
            this.OnRelease(false);
        }
        else
        {
            this.OnPress(false);
        }
    }

    private void ShowHighlightObject(GameObject obj, bool show)
    {
        if ((obj != null) && (obj.activeSelf != show))
        {
            obj.SetActive(show);
        }
    }

    [CustomEditField(Sections="Behavior Settings")]
    public bool AlwaysOver
    {
        get
        {
            return this.m_AlwaysOver;
        }
        set
        {
            this.m_AlwaysOver = value;
            this.ResetState();
        }
    }

    [CustomEditField(Sections="Behavior Settings")]
    public bool EnableResponse
    {
        get
        {
            return this.m_EnableResponse;
        }
        set
        {
            this.m_EnableResponse = value;
            this.ResetState();
        }
    }
}

