using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DeckTrayContent : MonoBehaviour
{
    private bool m_isModeActive;
    private bool m_isModeTrying;

    public virtual bool AnimateContentEntranceEnd()
    {
        return true;
    }

    public virtual bool AnimateContentEntranceStart()
    {
        return true;
    }

    public virtual bool AnimateContentExitEnd()
    {
        return true;
    }

    public virtual bool AnimateContentExitStart()
    {
        return true;
    }

    public virtual void Hide(bool hideAll = false)
    {
    }

    public virtual bool IsContentLoaded()
    {
        return true;
    }

    public bool IsModeActive()
    {
        return this.m_isModeActive;
    }

    public bool IsModeTryingOrActive()
    {
        return (this.m_isModeTrying || this.m_isModeActive);
    }

    public virtual void OnContentLoaded()
    {
    }

    public virtual void OnTaggedDeckChanged(CollectionManager.DeckTag tag, CollectionDeck newDeck, CollectionDeck oldDeck, bool isNewDeck)
    {
    }

    public virtual bool PostAnimateContentEntrance()
    {
        return true;
    }

    public virtual bool PostAnimateContentExit()
    {
        return true;
    }

    public virtual bool PreAnimateContentEntrance()
    {
        return true;
    }

    public virtual bool PreAnimateContentExit()
    {
        return true;
    }

    public void SetModeActive(bool active)
    {
        this.m_isModeActive = active;
    }

    public void SetModeTrying(bool trying)
    {
        this.m_isModeTrying = trying;
    }

    public virtual void Show(bool showAll = false)
    {
    }
}

