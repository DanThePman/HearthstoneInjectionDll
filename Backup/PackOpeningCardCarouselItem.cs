using System;
using UnityEngine;

public class PackOpeningCardCarouselItem : CarouselItem
{
    private PackOpeningCard m_card;

    public PackOpeningCardCarouselItem(PackOpeningCard card)
    {
        this.m_card = card;
    }

    public override void Clear()
    {
        this.m_card = null;
    }

    public override GameObject GetGameObject()
    {
        return this.m_card.gameObject;
    }

    public override void Hide()
    {
    }

    public override bool IsLoaded()
    {
        return true;
    }

    public override void Show(Carousel card)
    {
    }
}

