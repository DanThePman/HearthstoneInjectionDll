using System;
using UnityEngine;

[Serializable]
public class DisenchantBar
{
    private int m_amount;
    public GameObject m_amountBar;
    public UberText m_amountText;
    public MeshRenderer m_barFrameMesh;
    public GameObject m_dustJar;
    public GameObject m_glow;
    private int m_numCards;
    public UberText m_numCardsText;
    private int m_numGoldCards;
    public UberText m_numGoldText;
    public TAG_PREMIUM m_premiumType;
    public TAG_RARITY m_rarity;
    public GameObject m_rarityGem;
    public UberText m_typeText;
    public const float NUM_CARDS_TEXT_CENTER_X = 2.902672f;
    public const float NUM_CARDS_TEXT_OFFSET_X = 7.638979f;

    public void AddCards(int count, int sellAmount, TAG_PREMIUM premiumType)
    {
        this.m_numCards += count;
        if (this.m_premiumType != premiumType)
        {
            this.m_numGoldCards += count;
        }
        this.m_amount += sellAmount;
    }

    public int GetAmountDust()
    {
        return this.m_amount;
    }

    public int GetNumCards()
    {
        return this.m_numCards;
    }

    public void Init()
    {
        if (this.m_typeText != null)
        {
            string rarityText = GameStrings.GetRarityText(this.m_rarity);
            this.m_typeText.Text = (this.m_premiumType != TAG_PREMIUM.GOLDEN) ? rarityText : GameStrings.Format("GLUE_MASS_DISENCHANT_PREMIUM_TITLE", new object[] { rarityText });
        }
    }

    public void Reset()
    {
        this.m_numCards = 0;
        this.m_amount = 0;
        this.m_numGoldCards = 0;
    }

    public void UpdateVisuals(int totalNumCards)
    {
        this.m_numCardsText.Text = this.m_numCards.ToString();
        this.m_amountText.Text = this.m_amount.ToString();
        if (this.m_numGoldText != null)
        {
            if (this.m_numGoldCards > 0)
            {
                this.m_numGoldText.gameObject.SetActive(true);
                object[] args = new object[] { this.m_numGoldCards.ToString() };
                this.m_numGoldText.Text = GameStrings.Format("GLUE_MASS_DISENCHANT_NUM_GOLDEN_CARDS", args);
                TransformUtil.SetLocalPosX(this.m_numCardsText, 7.638979f);
                this.m_barFrameMesh.GetComponent<MeshFilter>().mesh = MassDisenchant.Get().m_rarityBarGoldMesh;
                this.m_barFrameMesh.material = MassDisenchant.Get().m_rarityBarGoldMaterial;
            }
            else
            {
                this.m_numGoldText.gameObject.SetActive(false);
                TransformUtil.SetLocalPosX(this.m_numCardsText, 2.902672f);
                this.m_barFrameMesh.GetComponent<MeshFilter>().mesh = MassDisenchant.Get().m_rarityBarNormalMesh;
                this.m_barFrameMesh.material = MassDisenchant.Get().m_rarityBarNormalMaterial;
            }
        }
        float num = (totalNumCards <= 0f) ? 0f : (((float) this.m_numCards) / ((float) totalNumCards));
        this.m_amountBar.GetComponent<Renderer>().material.SetFloat("_Percent", num);
    }
}

