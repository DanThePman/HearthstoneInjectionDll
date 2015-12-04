using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionPageDisplay : MonoBehaviour
{
    public GameObject m_cardStartPositionEightCards;
    public GameObject m_classFlavorHeader;
    public UberText m_classNameText;
    private List<CollectionCardVisual> m_collectionCardVisuals = new List<CollectionCardVisual>();
    public GameObject m_heroSkinsDecor;
    public UberText m_noMatchesFoundText;
    public UberText m_pageCountText;
    public static readonly Color TAUPE_COLOR = new Color(0.388f, 0.337f, 0.271f);

    public void ActivatePageCountText(bool active)
    {
        if (this.m_pageCountText != null)
        {
            this.m_pageCountText.gameObject.SetActive(active);
        }
    }

    private void Awake()
    {
        this.m_noMatchesFoundText.Text = GameStrings.Get("GLUE_COLLECTION_NO_MATCHES");
    }

    public CollectionCardVisual GetCardVisual(string cardID, CardFlair cardFlair)
    {
        foreach (CollectionCardVisual visual in this.m_collectionCardVisuals)
        {
            if (visual.IsShown())
            {
                Actor actor = visual.GetActor();
                if (actor.GetEntityDef().GetCardId().Equals(cardID) && actor.GetCardFlair().Equals(cardFlair))
                {
                    return visual;
                }
            }
        }
        return null;
    }

    private CollectionCardVisual GetCollectionCardVisual(int index)
    {
        CollectionCardVisual visual;
        CollectionPageLayoutSettings.Variables currentPageLayoutSettings = CollectionManagerDisplay.Get().GetCurrentPageLayoutSettings();
        float columnSpacing = currentPageLayoutSettings.m_ColumnSpacing;
        int columnCount = currentPageLayoutSettings.m_ColumnCount;
        float num3 = columnSpacing * (columnCount - 1);
        float scale = currentPageLayoutSettings.m_Scale;
        float rowSpacing = currentPageLayoutSettings.m_RowSpacing;
        Vector3 position = this.m_cardStartPositionEightCards.transform.localPosition + currentPageLayoutSettings.m_Offset;
        int rowNum = index / columnCount;
        position.x += ((index % columnCount) * columnSpacing) - (num3 * 0.5f);
        position.z -= rowSpacing * rowNum;
        if (index == this.m_collectionCardVisuals.Count)
        {
            visual = (CollectionCardVisual) GameUtils.Instantiate(CollectionManagerDisplay.Get().GetCardVisualPrefab(), base.gameObject, false);
            this.m_collectionCardVisuals.Insert(index, visual);
        }
        else
        {
            visual = this.m_collectionCardVisuals[index];
        }
        visual.SetCMRow(rowNum);
        visual.transform.localScale = new Vector3(scale, scale, scale);
        visual.transform.position = base.transform.TransformPoint(position);
        return visual;
    }

    public TAG_CLASS? GetFirstCardClass()
    {
        if (this.m_collectionCardVisuals.Count == 0)
        {
            return null;
        }
        CollectionCardVisual visual = this.m_collectionCardVisuals[0];
        if (!visual.IsShown())
        {
            return null;
        }
        Actor actor = visual.GetActor();
        if (!actor.IsShown())
        {
            return null;
        }
        EntityDef entityDef = actor.GetEntityDef();
        if (entityDef == null)
        {
            return null;
        }
        return new TAG_CLASS?(entityDef.GetClass());
    }

    public static int GetMaxNumCards()
    {
        CollectionPageLayoutSettings.Variables currentPageLayoutSettings = CollectionManagerDisplay.Get().GetCurrentPageLayoutSettings();
        return (currentPageLayoutSettings.m_ColumnCount * currentPageLayoutSettings.m_RowCount);
    }

    public void HideHeroSkinsDecor()
    {
        if (this.m_heroSkinsDecor != null)
        {
            this.m_heroSkinsDecor.SetActive(false);
        }
    }

    public void SetCardBacks()
    {
        this.SetClassNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_CARD_BACKS_TITLE"));
        this.SetClassFlavorTextures(HEADER_CLASS.CARDBACKS);
    }

    public void SetClass(TAG_CLASS? classTag)
    {
        if (!classTag.HasValue)
        {
            this.SetClassNameText(string.Empty);
            this.ShowClassFlavorObjects(false);
        }
        else
        {
            TAG_CLASS tag = classTag.Value;
            this.SetClassNameText(GameStrings.GetClassName(tag));
            this.SetClassFlavorTextures(this.TagClassToHeaderClass(tag));
        }
    }

    private void SetClassFlavorTextures(HEADER_CLASS headerClass)
    {
        if (this.m_classFlavorHeader != null)
        {
            int num = (int) headerClass;
            float x = (num >= 8f) ? 0.5f : 0f;
            float y = -((float) num) / 8f;
            this.m_classFlavorHeader.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(x, y));
            this.ShowClassFlavorObjects(true);
        }
    }

    private void SetClassNameText(string className)
    {
        if (this.m_classNameText != null)
        {
            this.m_classNameText.Text = className;
        }
    }

    public void SetHeroSkins(TAG_CLASS classTag)
    {
        this.SetClassNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE"));
        this.SetClassFlavorTextures(HEADER_CLASS.HEROSKINS);
    }

    public void SetPageCountText(string text)
    {
        if (this.m_pageCountText != null)
        {
            this.m_pageCountText.Text = text;
        }
    }

    private void ShowClassFlavorObjects(bool show)
    {
        if (this.m_classFlavorHeader != null)
        {
            this.m_classFlavorHeader.SetActive(show);
        }
    }

    public void ShowNoMatchesFound(bool show)
    {
        this.m_noMatchesFoundText.gameObject.SetActive(show);
    }

    private HEADER_CLASS TagClassToHeaderClass(TAG_CLASS classTag)
    {
        string str = classTag.ToString();
        if (Enum.IsDefined(typeof(HEADER_CLASS), str))
        {
            return (HEADER_CLASS) ((int) Enum.Parse(typeof(HEADER_CLASS), str));
        }
        return HEADER_CLASS.INVALID;
    }

    public void UpdateCollectionCards(List<Actor> actorList, CollectionManagerDisplay.ViewMode mode, bool isMassDisenchanting)
    {
        int index = 0;
        while (index < actorList.Count)
        {
            if (index >= GetMaxNumCards())
            {
                break;
            }
            Actor actor = actorList[index];
            CollectionCardVisual collectionCardVisual = this.GetCollectionCardVisual(index);
            collectionCardVisual.SetActor(actor, mode);
            collectionCardVisual.Show();
            if (mode == CollectionManagerDisplay.ViewMode.HERO_SKINS)
            {
                collectionCardVisual.SetHeroSkinBoxCollider();
            }
            else
            {
                collectionCardVisual.SetDefaultBoxCollider();
            }
            index++;
        }
        for (int i = index; i < this.m_collectionCardVisuals.Count; i++)
        {
            CollectionCardVisual visual2 = this.GetCollectionCardVisual(i);
            visual2.SetActor(null, CollectionManagerDisplay.ViewMode.CARDS);
            visual2.Hide();
        }
        this.UpdateFavoriteCardBack(mode);
        this.UpdateFavoriteHeroSkins(mode, isMassDisenchanting);
        this.UpdateHeroSkinNames(mode);
        this.UpdateCurrentPageCardLocks(false);
    }

    public void UpdateCurrentPageCardLocks(bool playSound = false)
    {
        if (CollectionDeckTray.Get().GetCurrentContentType() != CollectionDeckTray.DeckContentTypes.Cards)
        {
            foreach (CollectionCardVisual visual in this.m_collectionCardVisuals)
            {
                if (visual.IsShown())
                {
                    visual.ShowLock(CollectionCardVisual.LockType.NONE);
                }
            }
        }
        else
        {
            CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
            foreach (CollectionCardVisual visual2 in this.m_collectionCardVisuals)
            {
                if (visual2.GetVisualType() != CollectionManagerDisplay.ViewMode.CARD_BACKS)
                {
                    if (!visual2.IsShown())
                    {
                        visual2.ShowLock(CollectionCardVisual.LockType.NONE);
                    }
                    else
                    {
                        Actor actor = visual2.GetActor();
                        string cardId = actor.GetEntityDef().GetCardId();
                        CardFlair cardFlair = actor.GetCardFlair();
                        CollectionCardStack.ArtStack collectionArtStack = CollectionManager.Get().GetCollectionArtStack(cardId, cardFlair);
                        if (collectionArtStack.Count <= 0)
                        {
                            visual2.ShowLock(CollectionCardVisual.LockType.NONE);
                        }
                        else
                        {
                            CollectionCardVisual.LockType nONE = CollectionCardVisual.LockType.NONE;
                            if (taggedDeck != null)
                            {
                                if (CollectionDeckValidator.GetDeckViolationCardIdOverflow(taggedDeck, cardId, true) != null)
                                {
                                    nONE = CollectionCardVisual.LockType.MAX_COPIES_IN_DECK;
                                }
                                if (((nONE == CollectionCardVisual.LockType.NONE) && (collectionArtStack.Count > 0)) && (taggedDeck.GetCardCount(cardId, cardFlair) >= collectionArtStack.Count))
                                {
                                    nONE = CollectionCardVisual.LockType.NO_MORE_INSTANCES;
                                }
                            }
                            visual2.ShowLock(nONE, playSound);
                        }
                    }
                }
                else
                {
                    visual2.ShowLock(CollectionCardVisual.LockType.NONE);
                }
            }
        }
    }

    public void UpdateFavoriteCardBack(CollectionManagerDisplay.ViewMode mode)
    {
        if (mode == CollectionManagerDisplay.ViewMode.CARD_BACKS)
        {
            int defaultCardBack = -1;
            if (!CollectionManager.Get().IsInEditMode())
            {
                defaultCardBack = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>().DefaultCardBack;
            }
            foreach (CollectionCardVisual visual in this.m_collectionCardVisuals)
            {
                if (visual.IsShown())
                {
                    CollectionCardBack component = visual.GetActor().GetComponent<CollectionCardBack>();
                    if (component != null)
                    {
                        component.ShowFavoriteBanner(defaultCardBack == component.GetCardBackId());
                    }
                }
            }
        }
    }

    public void UpdateFavoriteHeroSkins(CollectionManagerDisplay.ViewMode mode, bool isMassDisenchanting)
    {
        bool flag = mode == CollectionManagerDisplay.ViewMode.HERO_SKINS;
        if (this.m_heroSkinsDecor != null)
        {
            this.m_heroSkinsDecor.SetActive(flag && !isMassDisenchanting);
        }
        if (flag)
        {
            bool flag2 = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing) == null;
            foreach (CollectionCardVisual visual in this.m_collectionCardVisuals)
            {
                if (visual.IsShown())
                {
                    Actor actor = visual.GetActor();
                    CollectionHeroSkin component = actor.GetComponent<CollectionHeroSkin>();
                    if (component != null)
                    {
                        component.ShowShadow(actor.IsShown());
                        EntityDef entityDef = actor.GetEntityDef();
                        if (entityDef != null)
                        {
                            component.SetClass(entityDef.GetClass());
                            bool show = false;
                            if (flag2)
                            {
                                NetCache.CardDefinition favoriteHero = CollectionManager.Get().GetFavoriteHero(entityDef.GetClass());
                                if (favoriteHero != null)
                                {
                                    show = ((CollectionManager.Get().GetBestHeroesIOwn(entityDef.GetClass()).Count > 1) && !string.IsNullOrEmpty(favoriteHero.Name)) && (favoriteHero.Name == entityDef.GetCardId());
                                }
                            }
                            component.ShowFavoriteBanner(show);
                        }
                    }
                }
            }
        }
    }

    public void UpdateHeroSkinNames(CollectionManagerDisplay.ViewMode mode)
    {
        if ((UniversalInputManager.UsePhoneUI != null) && (mode == CollectionManagerDisplay.ViewMode.HERO_SKINS))
        {
            foreach (CollectionCardVisual visual in this.m_collectionCardVisuals)
            {
                if (visual.IsShown())
                {
                    CollectionHeroSkin component = visual.GetActor().GetComponent<CollectionHeroSkin>();
                    if (component != null)
                    {
                        component.ShowCollectionManagerText();
                    }
                }
            }
        }
    }

    public enum HEADER_CLASS
    {
        INVALID,
        SHAMAN,
        PALADIN,
        MAGE,
        DRUID,
        HUNTER,
        ROGUE,
        WARRIOR,
        PRIEST,
        WARLOCK,
        HEROSKINS,
        CARDBACKS
    }
}

