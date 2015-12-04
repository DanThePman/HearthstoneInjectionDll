using System;
using UnityEngine;

public class CardTypeBanner : MonoBehaviour
{
    private Actor m_actor;
    public GameObject m_minionBanner;
    public GameObject m_root;
    public GameObject m_spellBanner;
    public UberText m_text;
    public GameObject m_weaponBanner;
    private readonly Color MINION_COLOR = new Color(0.1529412f, 0.1254902f, 0.03529412f);
    private static CardTypeBanner s_instance;
    private readonly Color SPELL_COLOR = new Color(0.8745098f, 0.7882353f, 0.5254902f);
    private readonly Color WEAPON_COLOR = new Color(0.8745098f, 0.7882353f, 0.5254902f);

    private void Awake()
    {
        s_instance = this;
    }

    public static CardTypeBanner Get()
    {
        return s_instance;
    }

    public CardDef GetCardDef()
    {
        if (this.m_actor != null)
        {
            return this.m_actor.GetCardDef();
        }
        return null;
    }

    public void Hide()
    {
        this.m_actor = null;
        this.HideImpl();
    }

    public void Hide(Actor actor)
    {
        if (this.m_actor == actor)
        {
            this.Hide();
        }
    }

    private void HideImpl()
    {
        this.m_root.gameObject.SetActive(false);
    }

    public bool IsShown()
    {
        return (bool) this.m_actor;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void Show(Actor a)
    {
        this.m_actor = a;
        this.ShowImpl();
    }

    private void ShowImpl()
    {
        this.m_root.gameObject.SetActive(true);
        TAG_CARDTYPE cardType = this.m_actor.GetEntity().GetCardType();
        this.m_text.gameObject.SetActive(true);
        this.m_text.Text = GameStrings.GetCardTypeName(cardType);
        switch (cardType)
        {
            case TAG_CARDTYPE.MINION:
                this.m_text.TextColor = this.MINION_COLOR;
                this.m_minionBanner.SetActive(true);
                break;

            case TAG_CARDTYPE.SPELL:
                this.m_text.TextColor = this.SPELL_COLOR;
                this.m_spellBanner.SetActive(true);
                break;

            case TAG_CARDTYPE.WEAPON:
                this.m_text.TextColor = this.WEAPON_COLOR;
                this.m_weaponBanner.SetActive(true);
                break;
        }
        this.UpdatePosition();
    }

    private void Update()
    {
        if (this.m_actor != null)
        {
            this.UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        GameObject cardTypeBannerAnchor = this.m_actor.GetCardTypeBannerAnchor();
        this.m_root.transform.position = cardTypeBannerAnchor.transform.position;
    }
}

