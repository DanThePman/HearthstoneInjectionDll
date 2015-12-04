using System;
using UnityEngine;

public class StoreMiniSummary : MonoBehaviour
{
    public UberText m_headlineText;
    public UberText m_itemsHeadlineText;
    public UberText m_itemsText;

    private void Awake()
    {
        this.m_headlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_HEADLINE");
        this.m_itemsHeadlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_ITEMS_ORDERED_HEADLINE");
    }

    private string GetItemsText(string productID, int quantity)
    {
        string productName;
        Network.Bundle bundle = StoreManager.Get().GetBundle(productID);
        if (bundle == null)
        {
            productName = GameStrings.Get("GLUE_STORE_PRODUCT_NAME_MOBILE_UNKNOWN");
        }
        else
        {
            productName = StoreManager.Get().GetProductName(bundle.Items);
        }
        object[] args = new object[] { quantity, productName };
        return GameStrings.Format("GLUE_STORE_SUMMARY_ITEM_ORDERED", args);
    }

    public void SetDetails(string productID, int quantity)
    {
        this.m_itemsText.Text = this.GetItemsText(productID, quantity);
    }
}

