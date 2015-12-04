using System;

public class QuestLogButton : PegUIElement
{
    public HighlightState m_highlight;

    private void OnButtonOut(UIEvent e)
    {
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.HideTooltip();
        }
    }

    private void OnButtonOver(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("quest_log_button_mouse_over", base.gameObject);
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_QUESTLOG_HEADLINE"), GameStrings.Get("GLUE_TOOLTIP_BUTTON_QUESTLOG_DESC"));
        }
    }

    private void Start()
    {
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnButtonOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnButtonOut));
        SoundManager.Get().Load("quest_log_button_mouse_over");
    }
}

