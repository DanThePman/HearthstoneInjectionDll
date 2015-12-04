using System;
using UnityEngine;

public class ClassFilterButtonContainer : MonoBehaviour
{
    public PegUIElement m_cardBacksButton;
    public GameObject m_cardBacksDisabled;
    public ClassFilterButton[] m_classButtons;
    public Material[] m_classMaterials;
    public TAG_CLASS[] m_classTags;
    public PegUIElement m_heroSkinsButton;
    public GameObject m_heroSkinsDisabled;
    public Material m_inactiveMaterial;
    private int m_neutralIndex;
    public int m_rowSize = 5;

    public void Awake()
    {
        this.m_neutralIndex = this.GetIndex(TAG_CLASS.INVALID);
    }

    private int GetIndex(TAG_CLASS classTag)
    {
        for (int i = 0; i < this.m_classTags.Length; i++)
        {
            if (this.m_classTags[i] == classTag)
            {
                return i;
            }
        }
        return 0;
    }

    public int GetNumVisibleClasses()
    {
        int num = 0;
        CollectionPageManager pageManager = CollectionManagerDisplay.Get().m_pageManager;
        for (int i = 0; i < this.m_classTags.Length; i++)
        {
            if (pageManager.GetNumPagesForClass(this.m_classTags[i]) > 0)
            {
                num++;
            }
        }
        return num;
    }

    private void SetCardBacksEnabled(bool enabled)
    {
        this.m_cardBacksButton.SetEnabled(enabled);
        this.m_cardBacksDisabled.SetActive(!enabled);
    }

    public void SetClass(TAG_CLASS classTag)
    {
        int count = CardBackManager.Get().GetCardBacksOwned().Count;
        int num2 = CollectionManager.Get().GetBestHeroesIOwn(classTag).Count;
        this.SetCardBacksEnabled(count > 1);
        this.SetHeroSkinsEnabled(num2 > 1);
        int index = this.GetIndex(classTag);
        for (int i = 0; i < this.m_classTags.Length; i++)
        {
            TAG_CLASS? nullable = null;
            this.m_classButtons[i].SetClass(nullable, this.m_inactiveMaterial);
            this.m_classButtons[i].SetNewCardCount(0);
        }
        this.m_classButtons[0].SetClass(new TAG_CLASS?(classTag), this.m_classMaterials[index]);
        this.m_classButtons[0].SetNewCardCount(CollectionManagerDisplay.Get().m_pageManager.GetNumNewCardsForClass(classTag));
        this.m_classButtons[1].SetClass(0, this.m_classMaterials[this.m_neutralIndex]);
        this.m_classButtons[1].SetNewCardCount(CollectionManagerDisplay.Get().m_pageManager.GetNumNewCardsForClass(TAG_CLASS.INVALID));
    }

    public void SetDefaults()
    {
        this.SetCardBacksEnabled(true);
        this.SetHeroSkinsEnabled(true);
        for (int i = 0; i < this.m_classTags.Length; i++)
        {
            TAG_CLASS? classTag = null;
            this.m_classButtons[i].SetClass(classTag, this.m_inactiveMaterial);
        }
        CollectionPageManager pageManager = CollectionManagerDisplay.Get().m_pageManager;
        int index = 0;
        for (int j = 0; j < this.m_classTags.Length; j++)
        {
            if (pageManager.GetNumPagesForClass(this.m_classTags[j]) > 0)
            {
                this.m_classButtons[index].SetClass(new TAG_CLASS?(this.m_classTags[j]), this.m_classMaterials[j]);
                int numNewCardsForClass = CollectionManagerDisplay.Get().m_pageManager.GetNumNewCardsForClass(this.m_classTags[j]);
                this.m_classButtons[index].SetNewCardCount(numNewCardsForClass);
                index++;
            }
        }
    }

    private void SetHeroSkinsEnabled(bool enabled)
    {
        this.m_heroSkinsButton.SetEnabled(enabled);
        this.m_heroSkinsDisabled.SetActive(!enabled);
    }
}

