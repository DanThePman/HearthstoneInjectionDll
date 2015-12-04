using System;
using UnityEngine;

public class ClassFilterHeaderButton : PegUIElement
{
    private ClassFilterButton[] m_buttons;
    public SlidingTray m_classFilterTray;
    public ClassFilterButtonContainer m_container;
    public UberText m_headerText;
    public Transform m_showTwoRowsBone;

    protected override void Awake()
    {
        this.AddEventListener(UIEventType.RELEASE, e => this.HandleRelease());
        base.Awake();
    }

    public void HandleRelease()
    {
        CollectionManagerDisplay.Get().HideDeckHelpPopup();
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        bool flag = taggedDeck != null;
        if (this.m_buttons == null)
        {
            this.m_buttons = this.m_classFilterTray.GetComponentsInChildren<ClassFilterButton>();
        }
        if (!flag)
        {
            this.m_container.SetDefaults();
        }
        else
        {
            TAG_CLASS classTag = taggedDeck.GetClass();
            this.m_container.SetClass(classTag);
        }
        this.m_classFilterTray.ToggleTraySlider(true, this.m_showTwoRowsBone, true);
    }

    public void SetMode(CollectionManagerDisplay.ViewMode mode, TAG_CLASS? classTag)
    {
        if (mode == CollectionManagerDisplay.ViewMode.CARD_BACKS)
        {
            this.m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_CARD_BACKS_TITLE");
        }
        else if (mode == CollectionManagerDisplay.ViewMode.HERO_SKINS)
        {
            this.m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE");
        }
        else if (classTag.HasValue)
        {
            this.m_headerText.Text = GameStrings.GetClassName(classTag.Value);
        }
        else
        {
            this.m_headerText.Text = string.Empty;
        }
    }
}

