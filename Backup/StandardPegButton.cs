using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class StandardPegButton : PegUIElement
{
    public NewThreeSliceElement m_border;
    private BoxCollider m_boxCollider;
    public NewThreeSliceElement m_button;
    public UberText m_buttonText;
    public GameObject m_downBone;
    public bool m_ExecuteInEditMode;
    public NewThreeSliceElement m_highlight;
    private bool m_highlightLocked;
    public float m_highlightScaleFudgeFactor = 1f;
    public Vector2 m_middleScale;
    public Vector3 m_textOffset = Vector3.zero;
    public float m_textWidthFudgeFactor = 1f;
    public GameObject m_upBone;

    public void Disable()
    {
        this.m_button.transform.localRotation = Quaternion.Euler(new Vector3(180f, 180f, 0f));
        base.SetEnabled(false);
    }

    public void Enable()
    {
        this.m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
        base.SetEnabled(true);
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }

    public void LockHighlight()
    {
        this.m_highlight.gameObject.SetActive(true);
        this.m_highlightLocked = true;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        if (!this.m_highlightLocked)
        {
            this.m_highlight.gameObject.SetActive(false);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (!this.m_highlightLocked)
        {
            object[] args = new object[] { "amount", new Vector3(90f, 0f, 0f), "time", 0.5f, "name", "rotation" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(this.m_button.gameObject, "rotation");
            this.m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
            iTween.PunchRotation(this.m_button.gameObject, hashtable);
            this.m_highlight.gameObject.SetActive(true);
            if ((SoundManager.Get() != null) && (SoundManager.Get().GetConfig() != null))
            {
                SoundManager.Get().LoadAndPlay("Small_Mouseover");
            }
        }
    }

    protected override void OnPress()
    {
        this.m_button.transform.localPosition = this.m_downBone.transform.localPosition;
        this.m_button.transform.localScale = this.m_downBone.transform.localScale;
        if ((SoundManager.Get() != null) && (SoundManager.Get().GetConfig() != null))
        {
            SoundManager.Get().LoadAndPlay("Back_Click");
        }
    }

    protected override void OnRelease()
    {
        this.m_button.transform.localPosition = this.m_upBone.transform.localPosition;
        this.m_button.transform.localScale = this.m_upBone.transform.localScale;
    }

    public void SetSize(Vector2 middleScale)
    {
        this.m_button.SetSize((Vector3) middleScale);
        this.m_border.SetSize((Vector3) middleScale);
        this.m_highlight.SetSize((Vector3) new Vector2(middleScale.x * this.m_highlightScaleFudgeFactor, middleScale.y));
        this.m_buttonText.Width = middleScale.x * this.m_textWidthFudgeFactor;
        Vector3 selfUnitAnchor = new Vector3(0.5f, 0f, 0.5f);
        TransformUtil.SetPoint(this.m_buttonText, selfUnitAnchor, this.m_button.m_middle, selfUnitAnchor, this.m_textOffset);
        SceneUtils.ResizeBoxCollider(base.gameObject, TransformUtil.GetBoundsOfChildren(this.m_button));
    }

    public void SetText(string t)
    {
        this.m_buttonText.Text = t;
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
    }

    public void UnlockHighlight()
    {
        this.m_highlight.gameObject.SetActive(false);
        this.m_highlightLocked = false;
    }
}

