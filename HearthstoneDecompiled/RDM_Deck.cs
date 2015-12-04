using System;
using System.Collections.Generic;

public class RDM_Deck
{
    public TAG_CLASS classType;
    public List<RDMDeckEntry> deckList;
    public EntityDef heroCard;

    public RDM_Deck()
    {
        this.deckList = new List<RDMDeckEntry>();
    }

    public RDM_Deck(EntityDef hero)
    {
        this.deckList = new List<RDMDeckEntry>();
        this.heroCard = hero;
        this.classType = hero.GetClass();
    }

    public void DebugPrintDeck()
    {
        foreach (RDMDeckEntry entry in this.deckList)
        {
            Log.Ben.Print(string.Format("Choice: {0} (flair {1})", entry.EntityDef.GetDebugName(), entry.Flair), new object[0]);
        }
    }

    public List<int> GetDeckListForServer()
    {
        List<int> list = new List<int>();
        int item = GameUtils.TranslateCardIdToDbId(this.heroCard.GetCardId());
        list.Add(item);
        foreach (RDMDeckEntry entry in this.deckList)
        {
            int num2 = GameUtils.TranslateCardIdToDbId(entry.EntityDef.GetCardId());
            list.Add(num2);
        }
        return list;
    }
}

