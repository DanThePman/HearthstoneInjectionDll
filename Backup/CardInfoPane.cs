using System;
using UnityEngine;

public class CardInfoPane : MonoBehaviour
{
    public UberText m_artistName;
    public UberText m_flavorText;
    public RarityGem m_rarityGem;
    public UberText m_rarityLabel;
    public UberText m_setName;

    private void AssignRarityColors(TAG_RARITY rarity, TAG_CARD_SET cardSet)
    {
        if (cardSet == TAG_CARD_SET.CORE)
        {
            this.m_rarityLabel.TextColor = new Color(0.53f, 0.52f, 0.51f, 1f);
        }
        else
        {
            switch (rarity)
            {
                case TAG_RARITY.RARE:
                    this.m_rarityLabel.TextColor = new Color(0.11f, 0.33f, 0.8f, 1f);
                    return;

                case TAG_RARITY.EPIC:
                    this.m_rarityLabel.TextColor = new Color(0.77f, 0.03f, 1f, 1f);
                    return;

                case TAG_RARITY.LEGENDARY:
                    this.m_rarityLabel.TextColor = new Color(1f, 0.56f, 0f, 1f);
                    return;
            }
            this.m_rarityLabel.TextColor = Color.white;
        }
    }

    public void UpdateText()
    {
        EntityDef def;
        CardFlair flair;
        if (CraftingManager.Get().GetShownCardInfo(out def, out flair))
        {
            TAG_RARITY rarity = def.GetRarity();
            TAG_CARD_SET cardSet = def.GetCardSet();
            if (cardSet == TAG_CARD_SET.CORE)
            {
                this.m_rarityLabel.Text = string.Empty;
            }
            else
            {
                this.m_rarityLabel.Text = GameStrings.GetRarityText(rarity);
            }
            this.AssignRarityColors(rarity, cardSet);
            this.m_rarityGem.SetRarityGem(rarity, cardSet);
            this.m_setName.Text = GameStrings.GetCardSetName(cardSet);
            object[] args = new object[] { def.GetArtistName() };
            this.m_artistName.Text = GameStrings.Format("GLUE_COLLECTION_ARTIST", args);
            string str = "<color=#000000ff>" + def.GetFlavorText() + "</color>";
            NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(def.GetCardId(), flair.Premium);
            if ((cardValue != null) && cardValue.Nerfed)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str + "\n\n";
                }
                str = str + GameStrings.Get("GLUE_COLLECTION_RECENTLY_NERFED");
            }
            this.m_flavorText.Text = str;
        }
    }
}

