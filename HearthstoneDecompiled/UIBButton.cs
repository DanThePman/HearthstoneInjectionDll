using System;
using System.Collections;
using UnityEngine;

[CustomEditClass]
public class UIBButton : PegUIElement
{
    [CustomEditField(Sections="Text Object")]
    public UberText m_ButtonText;
    [CustomEditField(Sections="Click Depress Behavior")]
    public Vector3 m_ClickDownOffset = new Vector3(0f, -0.05f, 0f);
    private bool m_Depressed;
    [CustomEditField(Sections="Roll Over Depress Behavior")]
    public bool m_DepressOnOver;
    [CustomEditField(Sections="Click Depress Behavior")]
    public float m_DepressTime = 0.1f;
    [CustomEditField(Sections="Flip Enable Behavior")]
    public Vector3 m_DisabledRotation = Vector3.zero;
    [CustomEditField(Sections="Click Depress Behavior")]
    public float m_RaiseTime = 0.1f;
    [CustomEditField(Sections="Button Objects")]
    public GameObject m_RootObject;
    private Vector3? m_RootObjectOriginalPosition;
    private Vector3? m_RootObjectOriginalRotation;
    [CustomEditField(Sections="Wiggle Behavior")]
    public Vector3 m_WiggleAmount = new Vector3(90f, 0f, 0f);
    [CustomEditField(Sections="Wiggle Behavior")]
    public float m_WiggleTime = 0.5f;

    private void Depress()
    {
        if (((this.m_RootObject != null) && !this.m_Depressed) && (UniversalInputManager.UsePhoneUI == null))
        {
            this.InitOriginalPosition();
            this.m_Depressed = true;
            iTween.StopByName(this.m_RootObject, "depress");
            Vector3 vector = this.m_RootObjectOriginalPosition.Value + this.m_ClickDownOffset;
            if (this.m_DepressTime > 0f)
            {
                object[] args = new object[] { "position", vector, "time", this.m_DepressTime, "easeType", iTween.EaseType.linear, "isLocal", true, "name", "depress" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(this.m_RootObject, hashtable);
            }
            else
            {
                this.m_RootObject.transform.localPosition = vector;
            }
        }
    }

    public void Deselect()
    {
        this.Raise();
    }

    public void Flip(bool faceUp)
    {
        if (this.m_RootObject != null)
        {
            this.InitOriginalRotation();
            this.m_RootObject.transform.localEulerAngles = !faceUp ? (this.m_RootObjectOriginalRotation.Value + this.m_DisabledRotation) : this.m_RootObjectOriginalRotation.Value;
        }
    }

    public string GetText()
    {
        return (!this.m_ButtonText.GameStringLookup ? this.m_ButtonText.Text : GameStrings.Get(this.m_ButtonText.Text));
    }

    private void InitOriginalPosition()
    {
        if ((this.m_RootObject != null) && !this.m_RootObjectOriginalPosition.HasValue)
        {
            this.m_RootObjectOriginalPosition = new Vector3?(this.m_RootObject.transform.localPosition);
        }
    }

    private void InitOriginalRotation()
    {
        if ((this.m_RootObject != null) && !this.m_RootObjectOriginalRotation.HasValue)
        {
            this.m_RootObjectOriginalRotation = new Vector3?(this.m_RootObject.transform.localEulerAngles);
        }
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        if (this.m_Depressed)
        {
            this.Raise();
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (this.m_DepressOnOver)
        {
            this.Depress();
        }
        this.Wiggle();
    }

    protected override void OnPress()
    {
        if (!this.m_DepressOnOver)
        {
            this.Depress();
        }
    }

    protected override void OnRelease()
    {
        if (!this.m_DepressOnOver)
        {
            this.Raise();
        }
    }

    private void Raise()
    {
        if ((this.m_RootObject != null) && this.m_Depressed)
        {
            this.m_Depressed = false;
            iTween.StopByName(this.m_RootObject, "depress");
            if (this.m_RaiseTime > 0f)
            {
                object[] args = new object[] { "position", this.m_RootObjectOriginalPosition, "time", this.m_RaiseTime, "easeType", iTween.EaseType.linear, "isLocal", true, "name", "depress" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(this.m_RootObject, hashtable);
            }
            else
            {
                this.m_RootObject.transform.localPosition = this.m_RootObjectOriginalPosition.Value;
            }
        }
    }

    public void Select()
    {
        this.Depress();
    }

    public void SetText(string text)
    {
        if (this.m_ButtonText != null)
        {
            this.m_ButtonText.Text = text;
        }
    }

    private void Wiggle()
    {
        if (((this.m_RootObject != null) && (this.m_WiggleAmount.sqrMagnitude != 0f)) && ((this.m_WiggleTime > 0f) && (UniversalInputManager.UsePhoneUI == null)))
        {
            this.InitOriginalRotation();
            object[] args = new object[] { "amount", this.m_WiggleAmount, "time", this.m_WiggleTime, "name", "wiggle" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(this.m_RootObject, "wiggle");
            this.m_RootObject.transform.localEulerAngles = this.m_RootObjectOriginalRotation.Value;
            iTween.PunchRotation(this.m_RootObject, hashtable);
        }
    }
}

