using System;

public class ScrollBarThumb : PegUIElement
{
    private bool m_isDragging;

    public bool IsDragging()
    {
        return this.m_isDragging;
    }

    protected override void OnHold()
    {
        this.StartDragging();
    }

    public void StartDragging()
    {
        this.m_isDragging = true;
    }

    public void StopDragging()
    {
        this.m_isDragging = false;
    }

    private void Update()
    {
        if (this.IsDragging() && UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            this.StopDragging();
        }
    }
}

