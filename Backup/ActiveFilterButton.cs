using System;
using UnityEngine;

public class ActiveFilterButton : MonoBehaviour
{
    public PegUIElement m_activeFilterButton;
    public SlidingTray m_activeFilterTray;
    public Material m_disabledMaterial;
    public Material m_enabledMaterial;
    private bool m_filtersShown;
    public PegUIElement m_inactiveFilterButton;
    public MeshRenderer m_inactiveFilterButtonRenderer;
    public GameObject m_inactiveFilterButtonText;
    public ManaFilterTabManager m_manaFilter;
    private bool m_manaFilterActive;
    public GameObject m_manaFilterIcon;
    public UberText m_manaFilterText;
    private string m_manaFilterValue = string.Empty;
    public PegUIElement m_offClickCatcher;
    public CollectionSearch m_search;
    private bool m_searchFilterActive;
    private string m_searchFilterValue = string.Empty;
    public UberText m_searchText;

    protected void Awake()
    {
        if (this.m_inactiveFilterButton != null)
        {
            this.m_inactiveFilterButton.AddEventListener(UIEventType.RELEASE, e => this.ShowFilters());
        }
        if (this.m_activeFilterButton != null)
        {
            this.m_activeFilterButton.AddEventListener(UIEventType.RELEASE, e => this.ClearFilters());
        }
        if (this.m_offClickCatcher != null)
        {
            this.m_offClickCatcher.AddEventListener(UIEventType.RELEASE, e => this.OffClickPressed());
        }
        CollectionManagerDisplay.Get().RegisterManaFilterListener(new CollectionManagerDisplay.FilterStateListener(this.ManaFilterUpdate));
        CollectionManagerDisplay.Get().RegisterSearchFilterListener(new CollectionManagerDisplay.FilterStateListener(this.SearchFilterUpdate));
        this.FiltersUpdated();
    }

    public void ClearFilters()
    {
        this.m_manaFilter.ClearFilter();
        this.m_search.ClearFilter(true);
    }

    private void FiltersUpdated()
    {
        bool flag = this.m_manaFilterActive || this.m_searchFilterActive;
        if (this.m_inactiveFilterButton != null)
        {
            this.m_activeFilterButton.gameObject.SetActive(flag);
            this.m_inactiveFilterButton.gameObject.SetActive(!flag);
        }
        else
        {
            if (this.m_filtersShown != flag)
            {
                Vector3 euler = !flag ? new Vector3(0f, 0f, 0f) : new Vector3(180f, 0f, 0f);
                float num = !flag ? -0.5f : 0.5f;
                iTween.Stop(this.m_activeFilterButton.gameObject);
                this.m_activeFilterButton.gameObject.transform.localRotation = Quaternion.Euler(euler);
                object[] args = new object[] { "x", num, "time", 0.25f, "easetype", iTween.EaseType.easeInOutExpo };
                iTween.RotateBy(this.m_activeFilterButton.gameObject, iTween.Hash(args));
            }
            this.m_filtersShown = flag;
        }
        if (this.m_searchFilterActive)
        {
            this.m_manaFilterIcon.SetActive(false);
            this.m_searchText.Text = this.m_searchFilterValue;
        }
        else
        {
            this.m_searchText.Text = string.Empty;
        }
        this.m_manaFilterIcon.SetActive(this.m_manaFilterActive);
        this.m_manaFilterText.Text = this.m_manaFilterValue;
    }

    public bool HideFilters()
    {
        this.m_activeFilterTray.ToggleTraySlider(false, null, true);
        if (this.m_offClickCatcher != null)
        {
            this.m_offClickCatcher.gameObject.SetActive(false);
        }
        return true;
    }

    public void ManaFilterUpdate(bool state, object description)
    {
        this.m_manaFilterActive = state;
        if (description == null)
        {
            this.m_manaFilterValue = string.Empty;
        }
        else
        {
            this.m_manaFilterValue = (string) description;
        }
        this.FiltersUpdated();
    }

    public void OffClickPressed()
    {
        Navigation.GoBack();
    }

    public void OnDestroy()
    {
        CollectionManagerDisplay display = CollectionManagerDisplay.Get();
        if (display != null)
        {
            display.UnregisterManaFilterListener(new CollectionManagerDisplay.FilterStateListener(this.ManaFilterUpdate));
            display.UnregisterSearchFilterListener(new CollectionManagerDisplay.FilterStateListener(this.SearchFilterUpdate));
        }
    }

    public void SearchFilterUpdate(bool state, object description)
    {
        this.m_searchFilterActive = state;
        if (description == null)
        {
            this.m_searchFilterValue = string.Empty;
        }
        else
        {
            this.m_searchFilterValue = (string) description;
        }
        this.FiltersUpdated();
    }

    public void SetEnabled(bool enabled)
    {
        this.m_inactiveFilterButton.SetEnabled(enabled);
        this.m_inactiveFilterButtonText.SetActive(enabled);
        this.m_inactiveFilterButtonRenderer.sharedMaterial = !enabled ? this.m_disabledMaterial : this.m_enabledMaterial;
    }

    public void ShowFilters()
    {
        CollectionManagerDisplay.Get().HideDeckHelpPopup();
        Navigation.Push(new Navigation.NavigateBackHandler(this.HideFilters));
        this.m_activeFilterTray.ToggleTraySlider(true, null, true);
        if (this.m_offClickCatcher != null)
        {
            this.m_offClickCatcher.gameObject.SetActive(true);
        }
        this.m_manaFilter.m_manaCrystalContainer.UpdateSlices();
    }
}

