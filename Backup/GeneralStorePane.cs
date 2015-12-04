using System;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePane : MonoBehaviour
{
    public GameObject m_paneContainer;
    public GeneralStoreContent m_parentContent;

    public virtual bool AnimateEntranceEnd()
    {
        return true;
    }

    public virtual bool AnimateEntranceStart()
    {
        return true;
    }

    public virtual bool AnimateExitEnd()
    {
        return true;
    }

    public virtual bool AnimateExitStart()
    {
        return true;
    }

    public virtual void OnPurchaseFinished()
    {
    }

    protected virtual void OnRefresh()
    {
    }

    public virtual void PostPaneSwappedIn()
    {
    }

    public virtual void PostPaneSwappedOut()
    {
    }

    public virtual void PrePaneSwappedIn()
    {
    }

    public virtual void PrePaneSwappedOut()
    {
    }

    public void Refresh()
    {
        this.OnRefresh();
    }

    public virtual void StoreHidden(bool isCurrent)
    {
    }

    public virtual void StoreShown(bool isCurrent)
    {
    }
}

