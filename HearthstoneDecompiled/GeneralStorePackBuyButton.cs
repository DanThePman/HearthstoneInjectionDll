using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePackBuyButton : PegUIElement
{
    public List<Renderer> m_buttonRenderers = new List<Renderer>();
    public UberText m_costText;
    public UberText m_fullText;
    public Vector2 m_goldBtnDownMatOffset;
    public Vector2 m_goldBtnMatOffset;
    public Color m_goldCostTextColor;
    public GameObject m_goldIcon;
    public Color m_goldQuantityTextColor;
    private bool m_isGold;
    public int m_materialIndex;
    public string m_materialPropName = "_MainTex";
    public Vector2 m_moneyBtnDownMatOffset;
    public Vector2 m_moneyBtnMatOffset;
    public Color m_moneyCostTextColor;
    public Color m_moneyQuantityTextColor;
    public UberText m_quantityText;
    private bool m_selected;
    public GameObject m_selectGlow;

    public bool IsSelected()
    {
        return this.m_selected;
    }

    protected override void OnDoubleClick()
    {
    }

    public void Select()
    {
        if (!this.m_selected)
        {
            this.m_selected = true;
            this.UpdateButtonState();
        }
    }

    public void SetGoldValue(long goldCost, string quantityText)
    {
        if (this.m_fullText != null)
        {
            this.m_quantityText.gameObject.SetActive(true);
            this.m_costText.gameObject.SetActive(true);
            this.m_fullText.gameObject.SetActive(false);
        }
        this.m_costText.Text = goldCost.ToString();
        this.m_costText.TextColor = this.m_goldCostTextColor;
        this.m_quantityText.Text = quantityText;
        this.m_quantityText.TextColor = this.m_goldQuantityTextColor;
        this.m_isGold = true;
        this.UpdateButtonState();
    }

    public void SetMoneyValue(Network.Bundle bundle, string quantityText)
    {
        if ((bundle != null) && !StoreManager.Get().IsProductAlreadyOwned(bundle))
        {
            if (this.m_fullText != null)
            {
                this.m_quantityText.gameObject.SetActive(true);
                this.m_costText.gameObject.SetActive(true);
                this.m_fullText.gameObject.SetActive(false);
            }
            this.m_costText.Text = StoreManager.Get().FormatCostBundle(bundle);
            this.m_costText.TextColor = this.m_moneyCostTextColor;
            this.m_quantityText.Text = quantityText;
            this.m_quantityText.TextColor = this.m_moneyQuantityTextColor;
        }
        else
        {
            this.m_costText.Text = string.Empty;
            UberText fullText = this.m_quantityText;
            if (this.m_fullText != null)
            {
                this.m_quantityText.gameObject.SetActive(false);
                this.m_costText.gameObject.SetActive(false);
                this.m_fullText.gameObject.SetActive(true);
                fullText = this.m_fullText;
            }
            fullText.Text = GameStrings.Get("GLUE_STORE_PACK_BUTTON_TEXT_PURCHASED");
        }
        this.m_isGold = false;
        this.UpdateButtonState();
    }

    public void Unselect()
    {
        if (this.m_selected)
        {
            this.m_selected = false;
            this.UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        if (this.m_goldIcon != null)
        {
            this.m_goldIcon.SetActive(this.m_isGold);
        }
        Vector2 zero = Vector2.zero;
        if (this.m_isGold)
        {
            zero = !this.m_selected ? this.m_goldBtnMatOffset : this.m_goldBtnDownMatOffset;
        }
        else
        {
            zero = !this.m_selected ? this.m_moneyBtnMatOffset : this.m_moneyBtnDownMatOffset;
        }
        foreach (Renderer renderer in this.m_buttonRenderers)
        {
            renderer.materials[this.m_materialIndex].SetTextureOffset(this.m_materialPropName, zero);
        }
        if (this.m_selectGlow != null)
        {
            this.m_selectGlow.SetActive(this.m_selected);
        }
    }

    public void UpdateFromGTAPP(NoGTAPPTransactionData noGTAPPGoldPrice)
    {
        long goldCostNoGTAPP = StoreManager.Get().GetGoldCostNoGTAPP(noGTAPPGoldPrice);
        string quantityText = StoreManager.Get().GetProductQuantityText(noGTAPPGoldPrice.Product, noGTAPPGoldPrice.ProductData, noGTAPPGoldPrice.Quantity);
        this.SetGoldValue(goldCostNoGTAPP, quantityText);
    }

    public void UpdateFromMoneyBundle(Network.Bundle bundle)
    {
        Network.BundleItem item = bundle.Items[0];
        string quantityText = StoreManager.Get().GetProductQuantityText(item.Product, item.ProductData, item.Quantity);
        this.SetMoneyValue(bundle, quantityText);
    }
}

