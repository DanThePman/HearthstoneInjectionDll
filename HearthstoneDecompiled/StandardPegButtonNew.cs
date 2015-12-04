using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class StandardPegButtonNew : PegUIElement
{
    private const float GRAY_FRAME_SCALE = 0.88f;
    private const float HIGHLIGHT_SCALE = 1.2f;
    public ThreeSliceElement m_border;
    public ThreeSliceElement m_button;
    public UberText m_buttonText;
    public float m_buttonWidth;
    public GameObject m_downBone;
    public bool m_ExecuteInEditMode;
    public ThreeSliceElement m_highlight;
    private bool m_highlightLocked;
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
        this.m_button.transform.localPosition = this.m_upBone.transform.localPosition;
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
        if ((SoundManager.Get() != null) && (SoundManager.Get().GetConfig() != null))
        {
            SoundManager.Get().LoadAndPlay("Back_Click");
        }
    }

    protected override void OnRelease()
    {
        this.m_button.transform.localPosition = this.m_upBone.transform.localPosition;
    }

    public void Reset()
    {
        iTween.StopByName(this.m_button.gameObject, "rotation");
        this.m_button.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
    }

    public void SetText(string t)
    {
        this.m_buttonText.Text = t;
    }

    public void SetWidth(float globalWidth)
    {
        this.m_button.SetWidth(globalWidth * 0.88f);
        if (this.m_border != null)
        {
            this.m_border.SetWidth(globalWidth);
        }
        Quaternion rotation = base.transform.rotation;
        base.transform.rotation = Quaternion.Euler(Vector3.zero);
        Vector3 size = this.m_button.GetSize();
        Vector3 vector2 = TransformUtil.ComputeWorldScale(base.transform);
        Vector3 vector3 = new Vector3(size.x / vector2.x, size.z / vector2.z, size.y / vector2.y);
        base.GetComponent<BoxCollider>().size = vector3;
        base.transform.rotation = rotation;
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

