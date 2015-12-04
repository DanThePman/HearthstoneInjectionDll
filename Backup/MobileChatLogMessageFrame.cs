using System;
using UnityEngine;

public class MobileChatLogMessageFrame : MonoBehaviour, ITouchListItem
{
    private Bounds localBounds;
    public GameObject m_Background;
    public UberText text;

    GameObject ITouchListItem.get_gameObject()
    {
        return base.gameObject;
    }

    Transform ITouchListItem.get_transform()
    {
        return base.transform;
    }

    T ITouchListItem.GetComponent<T>()
    {
        return base.GetComponent<T>();
    }

    private void UpdateLocalBounds()
    {
        Bounds textBounds = this.text.GetTextBounds();
        Vector3 size = textBounds.size;
        this.localBounds.center = base.transform.InverseTransformPoint(textBounds.center) + ((Vector3) (10f * Vector3.up));
        this.localBounds.size = size;
    }

    public UnityEngine.Color Color
    {
        get
        {
            return this.text.TextColor;
        }
        set
        {
            this.text.TextColor = value;
        }
    }

    public bool IsHeader
    {
        get
        {
            return false;
        }
    }

    public Bounds LocalBounds
    {
        get
        {
            return this.localBounds;
        }
    }

    public string Message
    {
        get
        {
            return this.text.Text;
        }
        set
        {
            this.text.Text = value;
            this.text.UpdateNow();
            this.UpdateLocalBounds();
        }
    }

    public bool Visible
    {
        get
        {
            return true;
        }
        set
        {
        }
    }

    public float Width
    {
        get
        {
            return this.text.Width;
        }
        set
        {
            this.text.Width = value;
            if (this.m_Background != null)
            {
                float x = this.m_Background.GetComponent<MeshFilter>().mesh.bounds.size.x;
                this.m_Background.transform.localScale = new Vector3(value / x, this.m_Background.transform.localScale.y, 1f);
                this.m_Background.transform.localPosition = new Vector3(-value / (0.5f * x), 0f, 0f);
            }
        }
    }
}

