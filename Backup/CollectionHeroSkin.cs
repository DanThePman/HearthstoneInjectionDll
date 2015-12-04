using System;
using UnityEngine;

public class CollectionHeroSkin : MonoBehaviour
{
    public MeshRenderer m_classIcon;
    public UberText m_collectionManagerName;
    public GameObject m_favoriteBanner;
    public UberText m_favoriteBannerText;
    public UberText m_name;
    public GameObject m_nameShadow;
    public GameObject m_shadow;
    public Spell m_socketFX;

    public void Awake()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Actor component = base.gameObject.GetComponent<Actor>();
            if (component != null)
            {
                component.OverrideNameText(null);
            }
            this.m_nameShadow.SetActive(false);
        }
    }

    public void SetClass(TAG_CLASS classTag)
    {
        if (this.m_classIcon != null)
        {
            Vector2 offset = CollectionPageManager.s_classTextureOffsets[classTag];
            this.m_classIcon.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
        }
        if (this.m_favoriteBannerText != null)
        {
            object[] args = new object[] { GameStrings.GetClassName(classTag) };
            this.m_favoriteBannerText.Text = GameStrings.Format("GLUE_COLLECTION_MANAGER_FAVORITE_DEFAULT_TEXT", args);
        }
    }

    public void ShowCollectionManagerText()
    {
        Actor component = base.gameObject.GetComponent<Actor>();
        if (component != null)
        {
            component.OverrideNameText(this.m_collectionManagerName);
        }
    }

    public void ShowFavoriteBanner(bool show)
    {
        if (this.m_favoriteBanner != null)
        {
            this.m_favoriteBanner.SetActive(show);
        }
    }

    public void ShowShadow(bool show)
    {
        if (this.m_shadow != null)
        {
            this.m_shadow.SetActive(show);
        }
    }

    public void ShowSocketFX()
    {
        if ((this.m_socketFX != null) && this.m_socketFX.gameObject.activeInHierarchy)
        {
            this.m_socketFX.gameObject.SetActive(true);
            this.m_socketFX.Activate();
        }
    }
}

