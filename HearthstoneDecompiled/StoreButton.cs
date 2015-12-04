using System;
using UnityEngine;

public class StoreButton : PegUIElement
{
    public GameObject m_highlight;
    public HighlightState m_highlightState;
    public GameObject m_storeClosed;
    public UberText m_storeClosedText;
    public UberText m_storeText;

    protected override void Awake()
    {
        base.Awake();
        this.m_storeText.Text = GameStrings.Get("GLUE_STORE_OPEN_BUTTON_TEXT");
        this.m_storeClosedText.Text = GameStrings.Get("GLUE_STORE_CLOSED_BUTTON_TEXT");
    }

    public bool IsVisualClosed()
    {
        return ((this.m_storeClosed != null) && this.m_storeClosed.activeInHierarchy);
    }

    private void OnButtonOut(UIEvent e)
    {
        if (this.m_highlightState != null)
        {
            this.m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        }
        if (this.m_highlight != null)
        {
            this.m_highlight.SetActive(false);
        }
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.HideTooltip();
        }
    }

    private void OnButtonOver(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("store_button_mouse_over", base.gameObject);
        if (this.m_highlightState != null)
        {
            this.m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
        }
        if (this.m_highlight != null)
        {
            this.m_highlight.SetActive(true);
        }
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            component.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_STORE_HEADLINE"), GameStrings.Get("GLUE_TOOLTIP_BUTTON_STORE_DESC"));
        }
    }

    private void OnStoreStatusChanged(bool isOpen, object userData)
    {
        if (this.m_storeClosed != null)
        {
            this.m_storeClosed.SetActive(!isOpen);
        }
    }

    private void Start()
    {
        this.m_storeClosed.SetActive(!StoreManager.Get().IsOpen());
        StoreManager.Get().RegisterStatusChangedListener(new StoreManager.StatusChangedCallback(this.OnStoreStatusChanged));
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnButtonOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnButtonOut));
        SoundManager.Get().Load("store_button_mouse_over");
    }

    public void Unload()
    {
        base.SetEnabled(false);
        StoreManager.Get().RemoveStatusChangedListener(new StoreManager.StatusChangedCallback(this.OnStoreStatusChanged));
    }
}

