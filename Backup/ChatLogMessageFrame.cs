using System;
using UnityEngine;

public class ChatLogMessageFrame : MonoBehaviour
{
    public GameObject m_Background;
    private float m_initialBackgroundHeight;
    private float m_initialBackgroundLocalScaleY;
    private float m_initialPadding;
    public UberText m_Text;

    private void Awake()
    {
        Bounds bounds = this.m_Background.GetComponent<Collider>().bounds;
        Bounds textWorldSpaceBounds = this.m_Text.GetTextWorldSpaceBounds();
        this.m_initialPadding = bounds.size.y - textWorldSpaceBounds.size.y;
        this.m_initialBackgroundHeight = bounds.size.y;
        this.m_initialBackgroundLocalScaleY = this.m_Background.transform.localScale.y;
    }

    public Color GetColor()
    {
        return this.m_Text.TextColor;
    }

    public string GetMessage()
    {
        return this.m_Text.Text;
    }

    public void SetColor(Color color)
    {
        this.m_Text.TextColor = color;
    }

    public void SetMessage(string message)
    {
        this.m_Text.Text = message;
        this.UpdateText();
        this.UpdateBackground();
    }

    private void UpdateBackground()
    {
        float num = this.m_Text.GetTextWorldSpaceBounds().size.y + this.m_initialPadding;
        float initialBackgroundLocalScaleY = this.m_initialBackgroundLocalScaleY;
        if (num > this.m_initialBackgroundHeight)
        {
            initialBackgroundLocalScaleY *= num / this.m_initialBackgroundHeight;
        }
        TransformUtil.SetLocalScaleY(this.m_Background, initialBackgroundLocalScaleY);
    }

    private void UpdateText()
    {
        this.m_Text.UpdateNow();
    }
}

