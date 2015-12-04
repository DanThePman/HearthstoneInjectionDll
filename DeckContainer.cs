using System.Collections.Generic;

namespace HearthstoneInjectionDll
{
    public class DeckContainer
    {
        private static List<string> loadedCards = new List<string>();

        public static long currentDeckId;
        public static string currentDeckName;

        public static void SetDeckInfo(string name, long id)
        {
            currentDeckName = name;
            currentDeckId = id;
        }

        public static void LoadCard(string cardId)
        {
            if (loadedCards.Contains(cardId))
            {
                loadedCards.Remove(cardId);
                loadedCards.Add(cardId + " x2");
            }
            else
                loadedCards.Add(cardId);
        }

        public static List<string> GetLoadedCards()
        {
            return loadedCards;
        }

        public static void ResetLoadedCards()
        {
            loadedCards = new List<string>();
        }
    }
}