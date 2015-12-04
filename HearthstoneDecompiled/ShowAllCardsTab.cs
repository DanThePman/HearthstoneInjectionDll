using System;
using UnityEngine;

public class ShowAllCardsTab : MonoBehaviour
{
    public CheckBox m_includePremiumsCheckBox;
    public CheckBox m_showAllCardsCheckBox;

    private void Awake()
    {
        this.m_showAllCardsCheckBox.SetButtonText(GameStrings.Get("GLUE_COLLECTION_SHOW_ALL_CARDS"));
        this.m_includePremiumsCheckBox.SetButtonText(GameStrings.Get("GLUE_COLLECTION_INCLUDE_PREMIUMS"));
    }

    public bool IsShowAllChecked()
    {
        return this.m_showAllCardsCheckBox.IsChecked();
    }

    private void Start()
    {
        this.m_includePremiumsCheckBox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleIncludePremiums));
        this.m_showAllCardsCheckBox.SetChecked(false);
        this.m_includePremiumsCheckBox.SetChecked(false);
        this.m_includePremiumsCheckBox.gameObject.SetActive(false);
    }

    private void ToggleIncludePremiums(UIEvent e)
    {
        bool show = this.m_includePremiumsCheckBox.IsChecked();
        CollectionManagerDisplay.Get().ShowPremiumCardsNotOwned(show);
        if (show)
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_on", base.gameObject);
        }
        else
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_off", base.gameObject);
        }
    }
}

