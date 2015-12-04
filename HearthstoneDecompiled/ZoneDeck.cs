using System;
using UnityEngine;

public class ZoneDeck : Zone
{
    public Spell m_DeckFatigueGlow;
    private bool m_suppressEmotes;
    public GameObject m_Thickness1;
    public GameObject m_Thickness25;
    public GameObject m_Thickness50;
    public GameObject m_Thickness75;
    public GameObject m_ThicknessFull;
    private bool m_warnedAboutLastCard;
    private bool m_warnedAboutNoCards;
    private bool m_wasFatigued;
    private const int MAX_THICKNESS_CARD_COUNT = 0x1a;

    public bool AreEmotesSuppressed()
    {
        return this.m_suppressEmotes;
    }

    public void DoFatigueGlow()
    {
        if (this.m_DeckFatigueGlow != null)
        {
            this.m_DeckFatigueGlow.ActivateState(SpellStateType.ACTION);
        }
    }

    public GameObject GetActiveThickness()
    {
        if (this.m_ThicknessFull.GetComponent<Renderer>().enabled)
        {
            return this.m_ThicknessFull;
        }
        if (this.m_Thickness75.GetComponent<Renderer>().enabled)
        {
            return this.m_Thickness75;
        }
        if (this.m_Thickness50.GetComponent<Renderer>().enabled)
        {
            return this.m_Thickness50;
        }
        if (this.m_Thickness25.GetComponent<Renderer>().enabled)
        {
            return this.m_Thickness25;
        }
        if (this.m_Thickness1.GetComponent<Renderer>().enabled)
        {
            return this.m_Thickness1;
        }
        return null;
    }

    public GameObject GetThicknessForLayout()
    {
        GameObject activeThickness = this.GetActiveThickness();
        if (activeThickness != null)
        {
            return activeThickness;
        }
        return this.m_Thickness1;
    }

    public bool IsFatigued()
    {
        return (base.m_cards.Count == 0);
    }

    public void SetCardToInDeckState(Card card)
    {
        card.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
        card.transform.position = base.transform.position;
        card.transform.localScale = new Vector3(0.88f, 0.88f, 0.88f);
        card.EnableTransitioningZones(false);
    }

    public void SetSuppressEmotes(bool suppress)
    {
        this.m_suppressEmotes = suppress;
    }

    public void SetVisibility(bool visible)
    {
        base.gameObject.SetActive(visible);
    }

    private void UpdateDeckStateEmotes()
    {
        if (!GameState.Get().IsMulliganManagerActive() && !this.m_suppressEmotes)
        {
            int count = base.m_cards.Count;
            if ((count <= 0) && !this.m_warnedAboutNoCards)
            {
                this.m_warnedAboutNoCards = true;
                this.m_warnedAboutLastCard = true;
                base.m_controller.GetHeroCard().PlayEmote(EmoteType.NO_CARDS);
            }
            else if ((count == 1) && !this.m_warnedAboutLastCard)
            {
                this.m_warnedAboutLastCard = true;
                base.m_controller.GetHeroCard().PlayEmote(EmoteType.LOW_CARDS);
            }
            else
            {
                if (this.m_warnedAboutLastCard && (count > 1))
                {
                    this.m_warnedAboutLastCard = false;
                }
                if (this.m_warnedAboutNoCards && (count > 0))
                {
                    this.m_warnedAboutNoCards = false;
                }
            }
        }
    }

    public override void UpdateLayout()
    {
        base.m_updatingLayout = true;
        if (base.IsBlockingLayout())
        {
            base.UpdateLayoutFinished();
        }
        else
        {
            this.UpdateThickness();
            this.UpdateDeckStateEmotes();
            for (int i = 0; i < base.m_cards.Count; i++)
            {
                Card card = base.m_cards[i];
                if (!card.IsDoNotSort())
                {
                    card.HideCard();
                    this.SetCardToInDeckState(card);
                }
            }
            base.UpdateLayoutFinished();
        }
    }

    private void UpdateThickness()
    {
        this.m_ThicknessFull.GetComponent<Renderer>().enabled = false;
        this.m_Thickness75.GetComponent<Renderer>().enabled = false;
        this.m_Thickness50.GetComponent<Renderer>().enabled = false;
        this.m_Thickness25.GetComponent<Renderer>().enabled = false;
        this.m_Thickness1.GetComponent<Renderer>().enabled = false;
        int count = base.m_cards.Count;
        if (count == 0)
        {
            this.m_DeckFatigueGlow.ActivateState(SpellStateType.BIRTH);
            this.m_wasFatigued = true;
        }
        else
        {
            if (this.m_wasFatigued)
            {
                this.m_DeckFatigueGlow.ActivateState(SpellStateType.DEATH);
                this.m_wasFatigued = false;
            }
            if (count == 1)
            {
                this.m_Thickness1.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                float num2 = ((float) count) / 26f;
                if (num2 > 0.75f)
                {
                    this.m_ThicknessFull.GetComponent<Renderer>().enabled = true;
                }
                else if (num2 > 0.5f)
                {
                    this.m_Thickness75.GetComponent<Renderer>().enabled = true;
                }
                else if (num2 > 0.25f)
                {
                    this.m_Thickness50.GetComponent<Renderer>().enabled = true;
                }
                else if (num2 > 0f)
                {
                    this.m_Thickness25.GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }
}

