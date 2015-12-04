using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[CustomEditClass]
public class AdventurePurchaseScreen : Store
{
    [CustomEditField(Sections="UI")]
    public PegUIElement m_BuyDungeonButton;
    private List<PurchaseListener> m_PurchaseListeners = new List<PurchaseListener>();

    public void AddPurchaseListener(Purchase dlg, object userdata)
    {
        PurchaseListener item = new PurchaseListener();
        item.SetCallback(dlg);
        item.SetUserData(userdata);
        this.m_PurchaseListeners.Add(item);
    }

    protected override void Awake()
    {
        base.Awake();
        base.m_buyWithMoneyButton.AddEventListener(UIEventType.RELEASE, e => this.BuyWithMoney());
        base.m_buyWithGoldButton.AddEventListener(UIEventType.RELEASE, e => this.BuyWithGold());
        this.m_BuyDungeonButton.AddEventListener(UIEventType.RELEASE, e => this.SendToStore());
    }

    private void BuyWithGold()
    {
        bool success = true;
        this.FirePurchaseEvent(success);
    }

    private void BuyWithMoney()
    {
        bool success = true;
        this.FirePurchaseEvent(success);
    }

    private void FirePurchaseEvent(bool success)
    {
        foreach (PurchaseListener listener in this.m_PurchaseListeners.ToArray())
        {
            listener.Fire(success);
        }
    }

    public void RemovePurchaseListener(Purchase dlg)
    {
        foreach (PurchaseListener listener in this.m_PurchaseListeners)
        {
            if (listener.GetCallback() == dlg)
            {
                this.m_PurchaseListeners.Remove(listener);
                break;
            }
        }
    }

    private void SendToStore()
    {
        bool success = false;
        this.FirePurchaseEvent(success);
    }

    public delegate void Purchase(bool success, object userdata);

    public class PurchaseListener : EventListener<AdventurePurchaseScreen.Purchase>
    {
        public void Fire(bool success)
        {
            base.m_callback(success, base.m_userData);
        }
    }
}

