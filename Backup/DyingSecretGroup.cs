using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class DyingSecretGroup
{
    [CompilerGenerated]
    private static Predicate<Card> <>f__am$cache3;
    private List<Actor> m_actors = new List<Actor>();
    private List<Card> m_cards = new List<Card>();
    private Card m_mainCard;

    public void AddCard(Card card)
    {
        if (this.m_mainCard == null)
        {
            Zone zone = card.GetZone();
            if (<>f__am$cache3 == null)
            {
                <>f__am$cache3 = currCard => currCard.IsShown();
            }
            this.m_mainCard = zone.GetCards().Find(<>f__am$cache3);
        }
        this.m_cards.Add(card);
        this.m_actors.Add(card.GetActor());
    }

    public List<Actor> GetActors()
    {
        return this.m_actors;
    }

    public List<Card> GetCards()
    {
        return this.m_cards;
    }

    public Card GetMainCard()
    {
        return this.m_mainCard;
    }
}

