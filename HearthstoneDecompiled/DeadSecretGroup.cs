using System;
using System.Collections.Generic;

public class DeadSecretGroup
{
    private List<Card> m_cards = new List<Card>();
    private Card m_mainCard;

    public void AddCard(Card card)
    {
        this.m_cards.Add(card);
    }

    public List<Card> GetCards()
    {
        return this.m_cards;
    }

    public Card GetMainCard()
    {
        return this.m_mainCard;
    }

    public void SetMainCard(Card card)
    {
        this.m_mainCard = card;
    }
}

