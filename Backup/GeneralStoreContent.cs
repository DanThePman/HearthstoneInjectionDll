using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GeneralStoreContent : MonoBehaviour
{
    private List<BundleChanged> m_bundleChangedListeners = new List<BundleChanged>();
    private NoGTAPPTransactionData m_currentGoldBundle;
    private Network.Bundle m_currentMoneyBundle;
    private bool m_isContentActive;
    protected GeneralStore m_parentStore;
    protected ProductType m_productType;

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

    private void FireBundleChangedEvent()
    {
        if (this.IsContentActive())
        {
            foreach (BundleChanged changed in this.m_bundleChangedListeners.ToArray())
            {
                changed(this.m_currentGoldBundle, this.m_currentMoneyBundle);
            }
        }
    }

    public NoGTAPPTransactionData GetCurrentGoldBundle()
    {
        return this.m_currentGoldBundle;
    }

    public Network.Bundle GetCurrentMoneyBundle()
    {
        return this.m_currentMoneyBundle;
    }

    public virtual string GetMoneyDisplayOwnedText()
    {
        return string.Empty;
    }

    public ProductType GetProductType()
    {
        return this.m_productType;
    }

    public bool HasBundleSet()
    {
        return ((this.m_currentMoneyBundle != null) || (this.m_currentGoldBundle != null));
    }

    public bool IsContentActive()
    {
        return (this.m_isContentActive && ((this.m_parentStore == null) || !this.m_parentStore.IsCovered()));
    }

    public virtual bool IsPurchaseDisabled()
    {
        return false;
    }

    protected virtual void OnBundleChanged(NoGTAPPTransactionData goldBundle, Network.Bundle moneyBundle)
    {
    }

    public virtual void OnCoverStateChanged(bool coverActive)
    {
    }

    protected virtual void OnRefresh()
    {
    }

    public virtual void PostStoreFlipIn(bool animatedFlipIn)
    {
    }

    public virtual void PostStoreFlipOut()
    {
    }

    public virtual void PreStoreFlipIn()
    {
    }

    public virtual void PreStoreFlipOut()
    {
    }

    public void Refresh()
    {
        this.OnRefresh();
    }

    public void RegisterCurrentBundleChanged(BundleChanged dlg)
    {
        this.m_bundleChangedListeners.Add(dlg);
    }

    public void SetContentActive(bool active)
    {
        this.m_isContentActive = active;
    }

    public void SetCurrentGoldBundle(NoGTAPPTransactionData bundle)
    {
        if (this.m_currentGoldBundle != bundle)
        {
            this.m_currentMoneyBundle = null;
            this.m_currentGoldBundle = bundle;
            this.OnBundleChanged(this.m_currentGoldBundle, this.m_currentMoneyBundle);
            this.FireBundleChangedEvent();
        }
    }

    public void SetCurrentMoneyBundle(Network.Bundle bundle, bool force = false)
    {
        if ((force || (this.m_currentMoneyBundle != bundle)) || (bundle == null))
        {
            this.m_currentGoldBundle = null;
            this.m_currentMoneyBundle = bundle;
            this.OnBundleChanged(this.m_currentGoldBundle, this.m_currentMoneyBundle);
            this.FireBundleChangedEvent();
        }
    }

    public void SetParentStore(GeneralStore parentStore)
    {
        this.m_parentStore = parentStore;
    }

    public virtual void StoreHidden(bool isCurrent)
    {
    }

    public virtual void StoreShown(bool isCurrent)
    {
    }

    public virtual void TryBuyWithGold(BuyEvent successBuyCB = null, BuyEvent failedBuyCB = null)
    {
        if (successBuyCB != null)
        {
            successBuyCB();
        }
    }

    public virtual void TryBuyWithMoney(BuyEvent successBuyCB = null, BuyEvent failedBuyCB = null)
    {
        if (successBuyCB != null)
        {
            successBuyCB();
        }
    }

    public void UnregisterCurrentBundleChanged(BundleChanged dlg)
    {
        this.m_bundleChangedListeners.Remove(dlg);
    }

    public delegate void BundleChanged(NoGTAPPTransactionData newGoldBundle, Network.Bundle newMoneyBundle);

    public delegate void BuyEvent();
}

