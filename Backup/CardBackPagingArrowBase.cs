using System;
using UnityEngine;

public abstract class CardBackPagingArrowBase : MonoBehaviour
{
    protected CardBackPagingArrowBase()
    {
    }

    public abstract void AddEventListener(UIEventType eventType, UIEvent.Handler handler);
    public abstract void EnablePaging(bool enable);
}

