using System;
using UnityEngine;

public class ClassFilterButton : PegUIElement
{
    private TAG_CLASS? m_class;
    public GameObject m_disabled;
    public GameObject m_newCardCount;
    public UberText m_newCardCountText;
    public CollectionManagerDisplay.ViewMode m_tabViewMode;

    protected override void Awake()
    {
        this.AddEventListener(UIEventType.RELEASE, e => this.HandleRelease());
        base.Awake();
    }

    public void HandleRelease()
    {
        switch (this.m_tabViewMode)
        {
            case CollectionManagerDisplay.ViewMode.CARDS:
                if (this.m_class.HasValue)
                {
                    CollectionManagerDisplay.Get().m_pageManager.JumpToCollectionClassPage(this.m_class.Value);
                }
                break;

            case CollectionManagerDisplay.ViewMode.HERO_SKINS:
                CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.HERO_SKINS, null);
                break;

            case CollectionManagerDisplay.ViewMode.CARD_BACKS:
                CollectionManagerDisplay.Get().SetViewMode(CollectionManagerDisplay.ViewMode.CARD_BACKS, null);
                break;
        }
        Navigation.GoBack();
    }

    public void SetClass(TAG_CLASS? classTag, Material material)
    {
        this.m_class = classTag;
        base.GetComponent<Renderer>().material = material;
        bool flag = !this.m_class.HasValue;
        base.GetComponent<Renderer>().enabled = !flag;
        if (this.m_newCardCount != null)
        {
            this.m_newCardCount.SetActive(!flag);
        }
        if (this.m_disabled != null)
        {
            this.m_disabled.SetActive(flag);
        }
    }

    public void SetNewCardCount(int count)
    {
        if (this.m_newCardCount != null)
        {
            this.m_newCardCount.SetActive(count > 0);
        }
        if ((count > 0) && (this.m_newCardCountText != null))
        {
            object[] args = new object[] { count };
            this.m_newCardCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", args);
        }
    }
}

