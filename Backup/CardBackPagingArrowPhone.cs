using System;

public class CardBackPagingArrowPhone : CardBackPagingArrowBase
{
    public PegUIElement button;

    public override void AddEventListener(UIEventType eventType, UIEvent.Handler handler)
    {
        this.button.AddEventListener(eventType, handler);
    }

    public override void EnablePaging(bool enable)
    {
        this.button.gameObject.SetActive(enable);
    }

    private void OnButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("deck_select_button_press");
    }

    private void Start()
    {
        this.button.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnButtonReleased));
    }
}

