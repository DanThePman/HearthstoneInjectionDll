using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    public enum CardValue
    {
        Bad, Med, Good, Excellent
    }

    static class MinionHelper
    {
        public static Card GetBestValue(List<Card> mins)
        {
            CardValue best = CardValue.Bad;
            Card bestC = null;
            foreach (var card in mins)
            {
                var min = new CustomMinion(card);
                if (min.ThisCardValue > best)
                {
                    best = min.ThisCardValue;
                    bestC = card;
                }
                else if (bestC != null && min.ThisCardValue == best &&
                         card.GetEntity().GetClass() != TAG_CLASS.NEUTRAL &&
                         bestC.GetEntity().GetClass() == TAG_CLASS.NEUTRAL)
                {
                    best = min.ThisCardValue;
                    bestC = card;
                }
                else if (bestC != null && min.ThisCardValue == best &&
                         card.GetEntity().GetClass() == MainLists.OwnClass &&
                         bestC.GetEntity().GetClass() != MainLists.OwnClass)
                {
                    best = min.ThisCardValue;
                    bestC = card;
                }
            }

            return bestC;
        }
    }
    /// <summary>
    /// Class which includes the quality of a neutral minion
    /// </summary> 
    class CustomMinion
    {
        public Card MinionCard { get; private set; }
        public CardValue ThisCardValue { get; private set; }

        public CustomMinion(Card NeutralMinionMulliganCard)
        {
            MinionCard = NeutralMinionMulliganCard;

            SetCardValue();

            if (MinionCard != null && MainLists.whiteList.Contains(MinionCard.GetEntity().GetCardId()))
                ThisCardValue = CardValue.Excellent;
        }

        private void SetCardValue()
        {
            var e = MinionCard.GetEntity();
            float rawValue = (e.GetATK() + e.GetHealth()) / 2f;
            float resultingValueFloat = rawValue - e.GetCost();

            if (resultingValueFloat > 0)
                ThisCardValue = CardValue.Good;
            else if (resultingValueFloat == 0)
                ThisCardValue = CardValue.Med;
            else
                ThisCardValue = CardValue.Bad;

            if (CardEffects.HasBadEffect(MinionCard))
                ThisCardValue--;

            if (CardEffects.HasGoodEffect(MinionCard))
                ThisCardValue++;
        }

        public void ManageNeutralMinion()
        {
            Card card = MinionCard; 
            //<= max mana
            if (card.GetEntity().GetCost() > MainLists.MaxManaCost)
                return;

            if (card.GetEntity().GetRarity() >= TAG_RARITY.EPIC) //epic by default
                MainLists.chosenCards.Add(card.GetEntity().GetCardId());
            else
            {
                //card quality not hight enough but <= max mana
                var minionCard = new CustomMinion(card);
                CardValue requiredMinNeutralMinionValue = minionCard.IsMaxManaCard
                        ? CardValue.Excellent
                        : CardValue.Good;

                if (minionCard.ThisCardValue >= requiredMinNeutralMinionValue)
                    MainLists.chosenCards.Add(card.GetEntity().GetCardId());
            }
        }

        public bool WouldTake()
        {
            Card card = MinionCard;
            //<= max mana
            if (card.GetEntity().GetCost() > MainLists.MaxManaCost)
                return false;

            if (card.GetEntity().GetRarity() >= TAG_RARITY.EPIC) //epic by default
                return true;

            //card quality not hight enough but <= max mana
            var minionCard = new CustomMinion(card);
            CardValue requiredMinNeutralMinionValue = minionCard.IsMaxManaCard
                    ? CardValue.Excellent
                    : CardValue.Good;

            if (minionCard.ThisCardValue >= requiredMinNeutralMinionValue)
                return true;

            return false;
        }

        public bool IsMaxManaCard => MinionCard.GetEntity().GetCost() == MainLists.MaxManaCost;
    }
}
