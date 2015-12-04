using System;
using System.Collections.Generic;
using UnityEngine;

public class ManaFilterTabManager : MonoBehaviour
{
    private int m_currentFilterValue = ManaFilterTab.ALL_TAB_IDX;
    public ManaFilterTab m_dynamicManaFilterPrefab;
    public MultiSliceElement m_manaCrystalContainer;
    public ManaFilterTab m_singleManaFilterPrefab;
    private List<ManaFilterTab> m_tabs = new List<ManaFilterTab>();
    private bool m_tabsActive;

    public void ActivateTabs(bool active)
    {
        this.m_tabsActive = active;
        this.UpdateFilterStates();
        if (active)
        {
            this.m_manaCrystalContainer.UpdateSlices();
        }
    }

    private void Awake()
    {
    }

    public void ClearFilter()
    {
        this.UpdateCurrentFilterValue(ManaFilterTab.ALL_TAB_IDX);
    }

    private void CreateNewTab(ManaFilterTab tabPrefab, int index)
    {
        ManaFilterTab item = (ManaFilterTab) GameUtils.Instantiate(tabPrefab, this.m_manaCrystalContainer.gameObject, false);
        item.SetManaID(index);
        item.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnTabPressed));
        item.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnTabMousedOver));
        item.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnTabMousedOut));
        item.SetFilterState(ManaFilterTab.FilterState.DISABLED);
        if (UniversalInputManager.Get().IsTouchMode())
        {
            item.SetReceiveReleaseWithoutMouseDown(true);
        }
        this.m_tabs.Add(item);
        this.m_manaCrystalContainer.AddSlice(item.gameObject);
    }

    public void Enable(bool enabled)
    {
        foreach (ManaFilterTab tab in this.m_tabs)
        {
            tab.SetEnabled(enabled);
            ManaFilterTab.FilterState dISABLED = ManaFilterTab.FilterState.DISABLED;
            if (enabled && this.m_tabsActive)
            {
                dISABLED = (tab.GetManaID() != this.m_currentFilterValue) ? ManaFilterTab.FilterState.OFF : ManaFilterTab.FilterState.ON;
            }
            tab.SetFilterState(dISABLED);
            if (tab.m_costText != null)
            {
                tab.m_costText.gameObject.SetActive(enabled);
            }
        }
    }

    private void OnTabMousedOut(UIEvent e)
    {
        if (this.m_tabsActive)
        {
            ((ManaFilterTab) e.GetElement()).NotifyMousedOut();
        }
    }

    private void OnTabMousedOver(UIEvent e)
    {
        if (this.m_tabsActive)
        {
            ((ManaFilterTab) e.GetElement()).NotifyMousedOver();
        }
    }

    private void OnTabPressed(UIEvent e)
    {
        if (this.m_tabsActive)
        {
            ManaFilterTab element = (ManaFilterTab) e.GetElement();
            if ((UniversalInputManager.UsePhoneUI == null) && !Options.Get().GetBool(Option.HAS_CLICKED_MANA_TAB, false))
            {
                Options.Get().SetBool(Option.HAS_CLICKED_MANA_TAB, true);
                this.ShowManaTabHint(element);
            }
            if (element.GetManaID() == this.m_currentFilterValue)
            {
                this.UpdateCurrentFilterValue(ManaFilterTab.ALL_TAB_IDX);
            }
            else
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    Navigation.GoBack();
                }
                this.UpdateCurrentFilterValue(element.GetManaID());
                if (element.GetManaID() == ManaFilterTab.ALL_TAB_IDX)
                {
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_MANA_FILTER_OFF);
                }
                else
                {
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_CM_MANA_FILTER_CLICKED);
                }
            }
        }
    }

    public void SetUpTabs()
    {
        for (int i = 0; i <= (ManaFilterTab.SEVEN_PLUS_TAB_IDX - 1); i++)
        {
            this.CreateNewTab(this.m_singleManaFilterPrefab, i);
        }
        this.CreateNewTab(this.m_dynamicManaFilterPrefab, ManaFilterTab.SEVEN_PLUS_TAB_IDX);
        this.m_manaCrystalContainer.UpdateSlices();
    }

    private void ShowManaTabHint(ManaFilterTab tabButton)
    {
        Notification notification = NotificationManager.Get().CreatePopupText(tabButton.transform.position + new Vector3(0f, 0f, 7f), TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("GLUE_COLLECTION_MANAGER_MANA_TAB_FIRST_CLICK"), true);
        notification.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
        NotificationManager.Get().DestroyNotification(notification, 3f);
    }

    private void UpdateCurrentFilterValue(int filterValue)
    {
        if (filterValue != this.m_currentFilterValue)
        {
            SoundManager.Get().LoadAndPlay("mana_crystal_refresh");
            CollectionManagerDisplay.Get().FilterByManaCost(filterValue);
        }
        this.m_currentFilterValue = filterValue;
        this.UpdateFilterStates();
    }

    private void UpdateFilterStates()
    {
        foreach (ManaFilterTab tab in this.m_tabs)
        {
            ManaFilterTab.FilterState dISABLED = ManaFilterTab.FilterState.DISABLED;
            if (this.m_tabsActive)
            {
                dISABLED = (tab.GetManaID() != this.m_currentFilterValue) ? ManaFilterTab.FilterState.OFF : ManaFilterTab.FilterState.ON;
            }
            tab.SetFilterState(dISABLED);
        }
    }
}

