using System;
using UnityEngine;

public class RarityGem : MonoBehaviour
{
    public void SetRarityGem(TAG_RARITY rarity, TAG_CARD_SET cardSet)
    {
        if (cardSet == TAG_CARD_SET.CORE)
        {
            base.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            base.GetComponent<Renderer>().enabled = true;
            switch (rarity)
            {
                case TAG_RARITY.RARE:
                    base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.118f, 0f);
                    return;

                case TAG_RARITY.EPIC:
                    base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.239f, 0f);
                    return;

                case TAG_RARITY.LEGENDARY:
                    base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.3575f, 0f);
                    return;
            }
            base.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f, 0f);
        }
    }
}

