using System.Collections.Generic;

namespace HearthstoneInjectionDll
{
    public class Enemy
    {
        private static List<Card> playedCards = new List<Card>();
        private static List<Card> knownHandCards = new List<Card>();

        public static void Reset()
        {
            playedCards = new List<Card>();
            knownHandCards = new List<Card>();
        }

        public static List<Card> GetKnownHandCards()
        {
            return knownHandCards;
        }

        public static List<Card> GetKnownPlayedCards()
        {
            return playedCards;
        }

        public static void AddHand(string cardId)
        {
            foreach (var enemyHandCard in knownHandCards)
            {
                if (enemyHandCard.ID == cardId)
                {
                    enemyHandCard.Count++;
                    return;
                }
            }

            knownHandCards.Add(new Card(cardId));
        }

        public static void AddPlayed(string cardId)
        {
            foreach (var enemyPlayedCard in playedCards)
            {
                if (enemyPlayedCard.ID == cardId)
                {
                    enemyPlayedCard.Count++;
                    return;
                }
            }

            playedCards.Add(new Card(cardId));
        }

        public static void RemoveOrReduceHandCard(string id)
        {
            foreach (var enemyHandCard in knownHandCards)
            {
                if (enemyHandCard.ID == id)
                {
                    if (enemyHandCard.Count > 1)
                    {
                        enemyHandCard.Count--;
                        return;
                    }

                    knownHandCards.Remove(enemyHandCard); //remove potentially not existing
                    return;
                }
            }
        }
    }
}