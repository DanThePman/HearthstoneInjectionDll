using System;

public class UIHeader : ThreeSliceElement
{
    public UberText m_headerUberText;

    public void SetText(string t)
    {
        this.m_headerUberText.Text = t;
        base.SetMiddleWidth(this.m_headerUberText.GetTextWorldSpaceBounds().size.x);
    }
}

