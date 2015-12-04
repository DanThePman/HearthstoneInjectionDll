using System;
using UnityEngine;

public class SocialToast : MonoBehaviour
{
    private const float FUDGE_FACTOR = 0.95f;
    public UberText m_text;

    public void SetText(string text)
    {
        this.m_text.Text = text;
        base.GetComponent<ThreeSliceElement>().SetMiddleWidth(this.m_text.GetTextWorldSpaceBounds().size.x * 0.95f);
    }
}

