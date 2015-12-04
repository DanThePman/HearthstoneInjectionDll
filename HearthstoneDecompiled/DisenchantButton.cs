using System;
using System.Collections.Generic;
using UnityEngine;

public class DisenchantButton : CraftingButton
{
    private string m_lastwarnedCard;

    private void DoDisenchant()
    {
        CraftingManager.Get().DisenchantButtonPressed();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.GetComponent<Animation>().Play("CardExchange_ButtonPress1_phone");
        }
        else
        {
            base.GetComponent<Animation>().Play("CardExchange_ButtonPress1");
        }
    }

    public override void EnableButton()
    {
        if (CraftingManager.Get().GetNumTransactions() > 0)
        {
            base.EnterUndoMode();
        }
        else
        {
            base.labelText.Text = GameStrings.Get("GLUE_CRAFTING_DISENCHANT");
            base.EnableButton();
        }
    }

    private List<string> GetPostDisenchantInvalidDeckNames()
    {
        Actor shownActor = CraftingManager.Get().GetShownActor();
        string cardId = shownActor.GetEntityDef().GetCardId();
        CardFlair cardFlair = shownActor.GetCardFlair();
        CollectionCardStack.ArtStack collectionArtStack = CollectionManager.Get().GetCollectionArtStack(cardId, cardFlair);
        int num = Mathf.Max(0, collectionArtStack.Count - 1);
        SortedDictionary<long, CollectionDeck> decks = CollectionManager.Get().GetDecks();
        List<string> list = new List<string>();
        foreach (CollectionDeck deck in decks.Values)
        {
            if (deck.GetCardCount(cardId, cardFlair) > num)
            {
                list.Add(deck.Name);
                Log.Rachelle.Print(string.Format("Disenchanting will invalidate deck '{0}'", deck.Name), new object[0]);
            }
        }
        return list;
    }

    private void OnConfirmDisenchantResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            this.DoDisenchant();
        }
    }

    private void OnReadyToStartDisenchant()
    {
        List<string> postDisenchantInvalidDeckNames = this.GetPostDisenchantInvalidDeckNames();
        if (postDisenchantInvalidDeckNames.Count == 0)
        {
            EntityDef entityDef = CraftingManager.Get().GetShownActor().GetEntityDef();
            string cardId = entityDef.GetCardId();
            int num = CraftingManager.Get().GetNumOwnedCopies(cardId, TAG_PREMIUM.GOLDEN, false);
            int num2 = CraftingManager.Get().GetNumOwnedCopies(cardId, TAG_PREMIUM.NORMAL, true);
            int num3 = num + num2;
            if (((CraftingManager.Get().GetNumTransactions() <= 0) && (this.m_lastwarnedCard != cardId)) && ((!entityDef.IsElite() && (num3 <= 2)) || (entityDef.IsElite() && (num3 <= 1))))
            {
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_HEADER"),
                    m_text = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM2_DESC"),
                    m_showAlertIcon = true,
                    m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                    m_responseCallback = new AlertPopup.ResponseCallback(this.OnConfirmDisenchantResponse)
                };
                this.m_lastwarnedCard = cardId;
                DialogManager.Get().ShowPopup(info);
            }
            else
            {
                this.DoDisenchant();
            }
        }
        else
        {
            string str2 = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_DESC");
            foreach (string str3 in postDisenchantInvalidDeckNames)
            {
                str2 = str2 + "\n" + str3;
            }
            AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_HEADER"),
                m_text = str2,
                m_showAlertIcon = false,
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_responseCallback = new AlertPopup.ResponseCallback(this.OnConfirmDisenchantResponse)
            };
            DialogManager.Get().ShowPopup(info2);
        }
    }

    protected override void OnRelease()
    {
        if (CraftingManager.Get().GetPendingTransaction() == null)
        {
            if (CraftingManager.Get().GetNumTransactions() > 0)
            {
                this.DoDisenchant();
            }
            else
            {
                CollectionManager.Get().LoadAllDeckContents(new CollectionManager.DelOnAllDeckContents(this.OnReadyToStartDisenchant));
            }
        }
    }
}

