using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class CollectionDeck
{
    public int CardBackID;
    public bool CardBackOverridden;
    public CardFlair HeroCardFlair;
    public string HeroCardID = string.Empty;
    public bool HeroOverridden;
    public long ID;
    public bool IsTourneyValid;
    private bool m_isBeingDeleted;
    private bool m_isSavingContentChanges;
    private bool m_isSavingNameChanges;
    private string m_name;
    private bool m_netContentsLoaded;
    private List<CollectionDeckSlot> m_slots = new List<CollectionDeckSlot>();
    public int SeasonId;
    public DeckType Type = DeckType.NORMAL_DECK;

    public bool AddCard(EntityDef cardEntityDef, TAG_PREMIUM premium)
    {
        return this.AddCard(cardEntityDef.GetCardId(), premium);
    }

    public bool AddCard(string cardID, TAG_PREMIUM premium)
    {
        if (!this.CanInsertCard(cardID, premium))
        {
            return false;
        }
        bool owned = this.CanAddOwnedCard(cardID, premium);
        CollectionDeckSlot slot = this.FindSlotByCardIdPremiumAndOwned(cardID, premium, owned);
        if (slot != null)
        {
            slot.Count++;
            return true;
        }
        slot = new CollectionDeckSlot {
            CardID = cardID,
            Count = 1,
            Premium = premium,
            Owned = owned
        };
        this.InsertSlotByDefaultSort(slot);
        return true;
    }

    public bool CanAddOwnedCard(string cardID, TAG_PREMIUM premium)
    {
        int standard = 0;
        int golden = 0;
        CollectionManager.Get().GetOwnedCardCount(cardID, out standard, out golden);
        int num3 = (premium != TAG_PREMIUM.NORMAL) ? golden : standard;
        int num4 = this.GetOwnedCardCount(cardID, premium, true);
        return (num3 > num4);
    }

    private bool CanInsertCard(string cardID, TAG_PREMIUM premium)
    {
        if (this.Type != DeckType.DRAFT_DECK)
        {
            EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
            EntityDef def2 = DefLoader.Get().GetEntityDef(this.HeroCardID);
            TAG_CLASS tag_class = entityDef.GetClass();
            TAG_CLASS tag_class2 = def2.GetClass();
            if ((tag_class != TAG_CLASS.INVALID) && (tag_class != tag_class2))
            {
                Debug.LogWarning(string.Format("CollectionDeck.CanInsertCard(): {0} (class {1}) has invalid class for deck with hero class {2}", entityDef.GetName(), tag_class, tag_class2));
                return false;
            }
            int cardIdCount = this.GetCardIdCount(cardID);
            int num2 = !entityDef.IsElite() ? 2 : 1;
            if ((cardIdCount + 1) > num2)
            {
                Debug.LogWarning(string.Format("CollectionDeck.CanInsertCard(): already have {0} copies of {1} (max allowed copies = {2})", cardIdCount, entityDef.GetName(), num2));
                return false;
            }
        }
        return true;
    }

    private Network.CardUserData CardUserDataFromSlot(CollectionDeckSlot deckSlot, bool deleted)
    {
        return new Network.CardUserData { DbId = GameUtils.TranslateCardIdToDbId(deckSlot.CardID), Count = !deleted ? deckSlot.Count : 0, Premium = deckSlot.Premium };
    }

    public void ClearSlotContents()
    {
        this.m_slots.Clear();
    }

    public void CopyFrom(CollectionDeck otherDeck)
    {
        this.ID = otherDeck.ID;
        this.Type = otherDeck.Type;
        this.m_name = otherDeck.m_name;
        this.HeroCardID = otherDeck.HeroCardID;
        this.HeroCardFlair = otherDeck.HeroCardFlair;
        this.HeroOverridden = otherDeck.HeroOverridden;
        this.CardBackID = otherDeck.CardBackID;
        this.CardBackOverridden = otherDeck.CardBackOverridden;
        this.IsTourneyValid = otherDeck.IsTourneyValid;
        this.SeasonId = otherDeck.SeasonId;
        this.m_slots.Clear();
        for (int i = 0; i < otherDeck.GetSlotCount(); i++)
        {
            CollectionDeckSlot slotByIndex = otherDeck.GetSlotByIndex(i);
            CollectionDeckSlot item = new CollectionDeckSlot();
            item.CopyFrom(slotByIndex);
            this.m_slots.Add(item);
        }
    }

    public RDM_Deck CreateRDMFromDeckString(string deckString)
    {
        RDM_Deck deck = new RDM_Deck();
        char[] separator = new char[] { '\n' };
        foreach (string str in deckString.Split(separator))
        {
            string str2 = str.Trim();
            if (!str2.StartsWith("#"))
            {
                try
                {
                    char[] chArray2 = new char[] { ';' };
                    foreach (string str3 in str2.Split(chArray2))
                    {
                        try
                        {
                            int num3;
                            char[] chArray3 = new char[] { ',' };
                            string[] strArray4 = str3.Split(chArray3);
                            if ((int.TryParse(strArray4[0], out num3) && (num3 >= 0)) && (num3 <= 10))
                            {
                                string cardId = strArray4[1];
                                EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
                                if (entityDef != null)
                                {
                                    CardFlair cardFlair = new CardFlair(TAG_PREMIUM.NORMAL);
                                    for (int i = 0; i < num3; i++)
                                    {
                                        RDMDeckEntry item = new RDMDeckEntry(entityDef, cardFlair);
                                        deck.deckList.Add(item);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
        }
        return deck;
    }

    private CollectionDeckSlot FindSlotByCardIdAndPremium(string cardID, TAG_PREMIUM premium)
    {
        <FindSlotByCardIdAndPremium>c__AnonStorey2C2 storeyc = new <FindSlotByCardIdAndPremium>c__AnonStorey2C2 {
            cardID = cardID,
            premium = premium
        };
        return this.m_slots.Find(new Predicate<CollectionDeckSlot>(storeyc.<>m__89));
    }

    private CollectionDeckSlot FindSlotByCardIdPremiumAndOwned(string cardID, TAG_PREMIUM premium, bool owned)
    {
        <FindSlotByCardIdPremiumAndOwned>c__AnonStorey2C3 storeyc = new <FindSlotByCardIdPremiumAndOwned>c__AnonStorey2C3 {
            cardID = cardID,
            premium = premium,
            owned = owned
        };
        if (!this.IsCardOwnershipUnique())
        {
            return this.FindSlotByCardIdAndPremium(storeyc.cardID, storeyc.premium);
        }
        return this.m_slots.Find(new Predicate<CollectionDeckSlot>(storeyc.<>m__8A));
    }

    private bool GenerateCardBackDiff(CollectionDeck baseDeck, out int cardBackID)
    {
        cardBackID = ConnectAPI.SEND_DECK_DATA_NO_CARD_BACK_CHANGE;
        if (!this.CardBackOverridden)
        {
            return false;
        }
        bool flag = this.CardBackID == baseDeck.CardBackID;
        if (baseDeck.CardBackOverridden && flag)
        {
            return false;
        }
        cardBackID = this.CardBackID;
        return true;
    }

    private List<Network.CardUserData> GenerateContentChanges(CollectionDeck baseDeck)
    {
        SortedDictionary<int, CollectionDeckSlot> dictionary = new SortedDictionary<int, CollectionDeckSlot>();
        foreach (CollectionDeckSlot slot in baseDeck.GetSlots())
        {
            CollectionDeckSlot slot2 = null;
            if (dictionary.TryGetValue(slot.UID, out slot2))
            {
                slot2.Count += slot.Count;
            }
            else
            {
                slot2 = new CollectionDeckSlot();
                slot2.CopyFrom(slot);
                dictionary.Add(slot2.UID, slot2);
            }
        }
        SortedDictionary<int, CollectionDeckSlot> dictionary2 = new SortedDictionary<int, CollectionDeckSlot>();
        foreach (CollectionDeckSlot slot3 in this.GetSlots())
        {
            CollectionDeckSlot slot4 = null;
            if (dictionary2.TryGetValue(slot3.UID, out slot4))
            {
                slot4.Count += slot3.Count;
            }
            else
            {
                slot4 = new CollectionDeckSlot();
                slot4.CopyFrom(slot3);
                dictionary2.Add(slot4.UID, slot4);
            }
        }
        SortedDictionary<int, CollectionDeckSlot>.Enumerator enumerator = dictionary.GetEnumerator();
        SortedDictionary<int, CollectionDeckSlot>.Enumerator enumerator4 = dictionary2.GetEnumerator();
        List<Network.CardUserData> list = new List<Network.CardUserData>();
        bool flag = enumerator.MoveNext();
        bool flag2 = enumerator4.MoveNext();
        while (flag && flag2)
        {
            KeyValuePair<int, CollectionDeckSlot> current = enumerator.Current;
            CollectionDeckSlot deckSlot = current.Value;
            KeyValuePair<int, CollectionDeckSlot> pair2 = enumerator4.Current;
            CollectionDeckSlot slot6 = pair2.Value;
            if (deckSlot.GetUID(this.Type) == slot6.GetUID(this.Type))
            {
                if (deckSlot.Count != slot6.Count)
                {
                    list.Add(this.CardUserDataFromSlot(slot6, 0 == slot6.Count));
                }
                flag = enumerator.MoveNext();
                flag2 = enumerator4.MoveNext();
            }
            else if (deckSlot.GetUID(this.Type) < slot6.GetUID(this.Type))
            {
                list.Add(this.CardUserDataFromSlot(deckSlot, true));
                flag = enumerator.MoveNext();
            }
            else
            {
                list.Add(this.CardUserDataFromSlot(slot6, false));
                flag2 = enumerator4.MoveNext();
            }
        }
        while (flag)
        {
            KeyValuePair<int, CollectionDeckSlot> pair3 = enumerator.Current;
            CollectionDeckSlot slot7 = pair3.Value;
            list.Add(this.CardUserDataFromSlot(slot7, true));
            flag = enumerator.MoveNext();
        }
        while (flag2)
        {
            KeyValuePair<int, CollectionDeckSlot> pair4 = enumerator4.Current;
            CollectionDeckSlot slot8 = pair4.Value;
            list.Add(this.CardUserDataFromSlot(slot8, false));
            flag2 = enumerator4.MoveNext();
        }
        return list;
    }

    private bool GenerateHeroDiff(CollectionDeck baseDeck, out int heroAssetID, out TAG_PREMIUM heroCardPremium)
    {
        heroAssetID = ConnectAPI.SEND_DECK_DATA_NO_HERO_ASSET_CHANGE;
        heroCardPremium = TAG_PREMIUM.NORMAL;
        if (!this.HeroOverridden)
        {
            return false;
        }
        bool flag = (this.HeroCardID == baseDeck.HeroCardID) && (this.HeroCardFlair.Premium == baseDeck.HeroCardFlair.Premium);
        if (baseDeck.HeroOverridden && flag)
        {
            return false;
        }
        heroAssetID = GameUtils.TranslateCardIdToDbId(this.HeroCardID);
        heroCardPremium = this.HeroCardFlair.Premium;
        return true;
    }

    private void GenerateNameDiff(CollectionDeck baseDeck, out string deckName)
    {
        deckName = null;
        if (!this.Name.Equals(baseDeck.Name))
        {
            deckName = this.Name;
        }
    }

    public int GetCardCount(string cardID, CardFlair flair)
    {
        CollectionDeckSlot slot = this.FindSlotByCardIdAndPremium(cardID, flair.Premium);
        return ((slot != null) ? slot.Count : 0);
    }

    public int GetCardCount(string cardID, TAG_PREMIUM type)
    {
        CollectionDeckSlot slot = this.FindSlotByCardIdAndPremium(cardID, type);
        return ((slot != null) ? slot.Count : 0);
    }

    public int GetCardIdCount(string cardID)
    {
        int num = 0;
        foreach (CollectionDeckSlot slot in this.m_slots)
        {
            if (slot.CardID.Equals(cardID))
            {
                num += slot.Count;
            }
        }
        return num;
    }

    public TAG_CLASS GetClass()
    {
        return DefLoader.Get().GetEntityDef(this.HeroCardID).GetClass();
    }

    private int GetInsertionIdxByDefaultSort(CollectionDeckSlot slot)
    {
        EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
        if (entityDef == null)
        {
            Log.Rachelle.Print(string.Format("CollectionDeck.GetInsertionIdxByDefaultSort(): could not get entity def for {0}", slot.CardID), new object[0]);
            return -1;
        }
        int slotIndex = 0;
        while (slotIndex < this.GetSlotCount())
        {
            CollectionDeckSlot slotByIndex = this.GetSlotByIndex(slotIndex);
            EntityDef def2 = DefLoader.Get().GetEntityDef(slotByIndex.CardID);
            if (def2 == null)
            {
                Log.Rachelle.Print(string.Format("CollectionDeck.GetInsertionIdxByDefaultSort(): entityDef is null at slot index {0}", slotIndex), new object[0]);
                return slotIndex;
            }
            int num2 = CollectionManager.Get().EntityDefSortComparison(entityDef, def2);
            if (num2 < 0)
            {
                return slotIndex;
            }
            if (((num2 <= 0) && (slot.Premium <= slotByIndex.Premium)) && (!this.IsCardOwnershipUnique() || (slot.Owned == slotByIndex.Owned)))
            {
                return slotIndex;
            }
            slotIndex++;
        }
        return slotIndex;
    }

    public int GetOwnedCardCount(string cardID, CardFlair flair, bool owned)
    {
        CollectionDeckSlot slot = this.FindSlotByCardIdPremiumAndOwned(cardID, flair.Premium, owned);
        return ((slot != null) ? slot.Count : 0);
    }

    public int GetOwnedCardCount(string cardID, TAG_PREMIUM type, bool owned)
    {
        CollectionDeckSlot slot = this.FindSlotByCardIdPremiumAndOwned(cardID, type, owned);
        return ((slot != null) ? slot.Count : 0);
    }

    public CollectionDeckSlot GetSlotByIndex(int slotIndex)
    {
        if ((slotIndex >= 0) && (slotIndex < this.GetSlotCount()))
        {
            return this.m_slots[slotIndex];
        }
        return null;
    }

    public CollectionDeckSlot GetSlotByUID(int uid)
    {
        <GetSlotByUID>c__AnonStorey2C1 storeyc = new <GetSlotByUID>c__AnonStorey2C1 {
            uid = uid,
            <>f__this = this
        };
        return this.m_slots.Find(new Predicate<CollectionDeckSlot>(storeyc.<>m__88));
    }

    public int GetSlotCount()
    {
        return this.m_slots.Count;
    }

    public List<CollectionDeckSlot> GetSlots()
    {
        return this.m_slots;
    }

    public int GetSortedInsertionIndex(string cardID, TAG_PREMIUM premium, out bool slotAlreadyExists)
    {
        slotAlreadyExists = false;
        if (!this.CanInsertCard(cardID, premium))
        {
            return -1;
        }
        bool owned = this.CanAddOwnedCard(cardID, premium);
        CollectionDeckSlot slot = this.FindSlotByCardIdPremiumAndOwned(cardID, premium, owned);
        if (slot != null)
        {
            slotAlreadyExists = true;
            return slot.Index;
        }
        slot = new CollectionDeckSlot {
            CardID = cardID,
            Premium = premium,
            Owned = owned
        };
        return this.GetInsertionIdxByDefaultSort(slot);
    }

    public int GetTotalCardCount()
    {
        int num = 0;
        foreach (CollectionDeckSlot slot in this.m_slots)
        {
            num += slot.Count;
        }
        return num;
    }

    public int GetTotalOwnedCardCount()
    {
        if (!this.IsCardOwnershipUnique())
        {
            return this.GetTotalCardCount();
        }
        int num = 0;
        foreach (CollectionDeckSlot slot in this.m_slots)
        {
            if (slot.Owned)
            {
                num += slot.Count;
            }
        }
        return num;
    }

    private bool InsertSlot(int slotIndex, CollectionDeckSlot slot)
    {
        if ((slotIndex < 0) || (slotIndex > this.GetSlotCount()))
        {
            Log.Rachelle.Print(string.Format("CollectionDeck.InsertSlot(): inserting slot {0} failed; invalid slot index {1}.", slot, slotIndex), new object[0]);
            return false;
        }
        if (this.GetSlotByUID(slot.GetUID(this.Type)) != null)
        {
            Log.Rachelle.Print(string.Format("CollectionDeck.InsertSlot(): slot with uid {0} already exists in deck {1}!", slot.GetUID(this.Type), this.ID), new object[0]);
            return false;
        }
        slot.OnSlotEmptied = (CollectionDeckSlot.DelOnSlotEmptied) Delegate.Combine(slot.OnSlotEmptied, new CollectionDeckSlot.DelOnSlotEmptied(this.OnSlotEmptied));
        slot.Index = slotIndex;
        this.m_slots.Insert(slotIndex, slot);
        this.UpdateSlotIndices(slotIndex, this.GetSlotCount() - 1);
        return true;
    }

    public bool InsertSlotByDefaultSort(CollectionDeckSlot slot)
    {
        return this.InsertSlot(this.GetInsertionIdxByDefaultSort(slot), slot);
    }

    public bool IsBeingDeleted()
    {
        return this.m_isBeingDeleted;
    }

    private bool IsCardOwnershipUnique()
    {
        return (this.Type == DeckType.NORMAL_DECK);
    }

    public bool IsSavingChanges()
    {
        return (this.m_isSavingNameChanges || this.m_isSavingContentChanges);
    }

    public void MarkBeingDeleted()
    {
        this.m_isBeingDeleted = true;
    }

    public void MarkNetworkContentsLoaded()
    {
        this.m_netContentsLoaded = true;
    }

    public bool NetworkContentsLoaded()
    {
        return this.m_netContentsLoaded;
    }

    public void OnContentChangesComplete()
    {
        this.m_isSavingContentChanges = false;
    }

    public void OnNameChangeComplete()
    {
        this.m_isSavingNameChanges = false;
    }

    private void OnSlotEmptied(CollectionDeckSlot slot)
    {
        if (this.GetSlotByUID(slot.GetUID(this.Type)) == null)
        {
            Log.Rachelle.Print(string.Format("CollectionDeck.OnSlotCountUpdated(): Trying to remove slot {0}, but it does not exist in deck {1}", slot, this), new object[0]);
        }
        else
        {
            this.RemoveSlot(slot);
        }
    }

    public bool RemoveCard(string cardID, TAG_PREMIUM premium, bool owned)
    {
        CollectionDeckSlot slot = this.FindSlotByCardIdPremiumAndOwned(cardID, premium, owned);
        if (slot == null)
        {
            return false;
        }
        slot.Count--;
        return true;
    }

    private void RemoveSlot(CollectionDeckSlot slot)
    {
        slot.OnSlotEmptied = (CollectionDeckSlot.DelOnSlotEmptied) Delegate.Remove(slot.OnSlotEmptied, new CollectionDeckSlot.DelOnSlotEmptied(this.OnSlotEmptied));
        int index = slot.Index;
        this.m_slots.RemoveAt(index);
        this.UpdateSlotIndices(index, this.GetSlotCount() - 1);
    }

    public void SendChanges()
    {
        CollectionDeck baseDeck = CollectionManager.Get().GetBaseDeck(this.ID);
        if (this == baseDeck)
        {
            Debug.LogError(string.Format("CollectionDeck.Send() - {0} is a base deck. You cannot send a base deck to the network.", baseDeck));
        }
        else
        {
            string str;
            int num;
            TAG_PREMIUM tag_premium;
            int num2;
            this.GenerateNameDiff(baseDeck, out str);
            List<Network.CardUserData> cards = this.GenerateContentChanges(baseDeck);
            bool flag = this.GenerateHeroDiff(baseDeck, out num, out tag_premium);
            bool flag2 = this.GenerateCardBackDiff(baseDeck, out num2);
            Network network = Network.Get();
            if (str != null)
            {
                this.m_isSavingNameChanges = true;
                network.RenameDeck(this.ID, str);
            }
            if (((cards.Count > 0) || flag) || flag2)
            {
                this.m_isSavingContentChanges = true;
                network.SetDeckContents(this.ID, cards, num, tag_premium, num2);
            }
        }
    }

    public string ToDeckString()
    {
        StringBuilder builder = new StringBuilder();
        StringBuilder builder2 = new StringBuilder();
        EntityDef entityDef = DefLoader.Get().GetEntityDef(this.HeroCardID);
        string name = BnetPresenceMgr.Get().GetMyPlayer().GetBattleTag().GetName();
        string str2 = entityDef.GetClass().ToString().ToLower();
        str2 = str2.Substring(0, 1).ToUpper() + str2.Substring(1, str2.Length - 1);
        builder.Append(string.Format("# Hearthstone {0} Deck: \"{1}\" saved by {2}\n", str2, this.Name, name));
        builder.Append("#\n");
        foreach (CollectionDeckSlot slot in this.m_slots)
        {
            EntityDef def2 = DefLoader.Get().GetEntityDef(slot.CardID);
            builder.Append(string.Format("# {0}x ({1}) {2}\n", slot.Count, def2.GetCost(), def2.GetName()));
            builder2.Append(string.Format("{0},{1};", slot.Count, slot.CardID));
        }
        builder.Append("#\n");
        builder.Append(builder2.ToString());
        return builder.ToString();
    }

    public override string ToString()
    {
        object[] args = new object[] { this.ID, this.Name, this.HeroCardID, this.HeroCardFlair, this.CardBackID, this.CardBackOverridden, this.HeroOverridden, this.IsTourneyValid, this.GetSlotCount() };
        return string.Format("Deck [id={0} name=\"{1}\" heroCardId={2} heroCardFlair={3} cardBackId={4} cardBackOverridden={5} heroOverridden={6} isValid={7} slotCount={8}]", args);
    }

    private void UpdateSlotIndices(int indexA, int indexB)
    {
        if (this.GetSlotCount() != 0)
        {
            int num;
            int num2;
            if (indexA < indexB)
            {
                num = indexA;
                num2 = indexB;
            }
            else
            {
                num = indexB;
                num2 = indexA;
            }
            num = Math.Max(0, num);
            num2 = Math.Min(num2, this.GetSlotCount() - 1);
            for (int i = num; i <= num2; i++)
            {
                this.GetSlotByIndex(i).Index = i;
            }
        }
    }

    public string Name
    {
        get
        {
            return this.m_name;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError(string.Format("CollectionDeck.SetName() - null name given for deck {0}", this));
            }
            else if (!value.Equals(this.m_name, StringComparison.InvariantCultureIgnoreCase))
            {
                this.m_name = value;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <FindSlotByCardIdAndPremium>c__AnonStorey2C2
    {
        internal string cardID;
        internal TAG_PREMIUM premium;

        internal bool <>m__89(CollectionDeckSlot slot)
        {
            return (slot.CardID.Equals(this.cardID) && (slot.Premium == this.premium));
        }
    }

    [CompilerGenerated]
    private sealed class <FindSlotByCardIdPremiumAndOwned>c__AnonStorey2C3
    {
        internal string cardID;
        internal bool owned;
        internal TAG_PREMIUM premium;

        internal bool <>m__8A(CollectionDeckSlot slot)
        {
            return ((slot.CardID.Equals(this.cardID) && (slot.Premium == this.premium)) && (slot.Owned == this.owned));
        }
    }

    [CompilerGenerated]
    private sealed class <GetSlotByUID>c__AnonStorey2C1
    {
        internal CollectionDeck <>f__this;
        internal int uid;

        internal bool <>m__88(CollectionDeckSlot slot)
        {
            return (slot.GetUID(this.<>f__this.Type) == this.uid);
        }
    }
}

