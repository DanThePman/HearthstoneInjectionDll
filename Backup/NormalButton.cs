using System;
using UnityEngine;

[CustomEditClass]
public class NormalButton : PegUIElement
{
    private int buttonID;
    [CustomEditField(Sections="Button Properties")]
    public GameObject m_button;
    [CustomEditField(Sections="Button Properties")]
    public TextMesh m_buttonText;
    [CustomEditField(Sections="Button Properties")]
    public UberText m_buttonUberText;
    [CustomEditField(Sections="Mouse Over Settings")]
    public GameObject m_mouseOverBone;
    private Vector3 m_originalButtonPosition;
    [CustomEditField(Sections="Mouse Over Settings")]
    public float m_userOverYOffset = -0.05f;

    protected override void Awake()
    {
        this.SetOriginalButtonPosition();
    }

    public float GetBottom()
    {
        Bounds bounds = base.GetComponent<BoxCollider>().bounds;
        return (bounds.center.y - bounds.extents.y);
    }

    public int GetButtonID()
    {
        return this.buttonID;
    }

    public GameObject GetButtonTextGO()
    {
        if (this.m_buttonUberText == null)
        {
            return this.m_buttonText.gameObject;
        }
        return this.m_buttonUberText.gameObject;
    }

    public UberText GetButtonUberText()
    {
        return this.m_buttonUberText;
    }

    public float GetLeft()
    {
        Bounds bounds = base.GetComponent<BoxCollider>().bounds;
        return (bounds.center.x - bounds.extents.x);
    }

    public float GetRight()
    {
        return base.GetComponent<BoxCollider>().bounds.max.x;
    }

    public string GetText()
    {
        if (this.m_buttonUberText == null)
        {
            return this.m_buttonText.text;
        }
        return this.m_buttonUberText.Text;
    }

    public float GetTextHeight()
    {
        if (this.m_buttonUberText == null)
        {
            return (this.m_buttonText.GetComponent<Renderer>().bounds.extents.y * 2f);
        }
        return this.m_buttonUberText.Height;
    }

    public float GetTextWidth()
    {
        if (this.m_buttonUberText == null)
        {
            return (this.m_buttonText.GetComponent<Renderer>().bounds.extents.x * 2f);
        }
        return this.m_buttonUberText.Width;
    }

    public float GetTop()
    {
        Bounds bounds = base.GetComponent<BoxCollider>().bounds;
        return (bounds.center.y + bounds.extents.y);
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_button.gameObject.transform.localPosition = this.m_originalButtonPosition;
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (this.m_mouseOverBone != null)
        {
            this.m_button.transform.position = this.m_mouseOverBone.transform.position;
        }
        else
        {
            TransformUtil.SetLocalPosY(this.m_button.gameObject, this.m_originalButtonPosition.y + this.m_userOverYOffset);
        }
    }

    public void SetButtonID(int newID)
    {
        this.buttonID = newID;
    }

    public void SetOriginalButtonPosition()
    {
        this.m_originalButtonPosition = this.m_button.transform.localPosition;
    }

    public void SetText(string t)
    {
        if (this.m_buttonUberText == null)
        {
            this.m_buttonText.text = t;
        }
        else
        {
            this.m_buttonUberText.Text = t;
        }
    }

    public void SetUserOverYOffset(float userOverYOffset)
    {
        this.m_userOverYOffset = userOverYOffset;
    }
}

