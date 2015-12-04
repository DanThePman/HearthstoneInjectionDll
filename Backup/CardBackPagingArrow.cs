using System;

public class CardBackPagingArrow : CardBackPagingArrowBase
{
    public ArrowModeButton button;

    public override void AddEventListener(UIEventType eventType, UIEvent.Handler handler)
    {
        this.button.AddEventListener(eventType, handler);
    }

    public override void EnablePaging(bool enable)
    {
        this.button.Activate(enable);
    }
}

