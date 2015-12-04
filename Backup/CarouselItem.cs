using System;
using UnityEngine;

public abstract class CarouselItem
{
    protected CarouselItem()
    {
    }

    public abstract void Clear();
    public abstract GameObject GetGameObject();
    public abstract void Hide();
    public abstract bool IsLoaded();
    public abstract void Show(Carousel parent);
}

