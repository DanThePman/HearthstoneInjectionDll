using System;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable ConvertNullableToShortForm

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    class MainLists
    {
        public static List<string> whiteList = new List<string> { "GAME_005" /*Coin*/ };
        public static List<string> blackList = new List<string>();
        public static List<string> chosenCards = new List<string>();

        public static TAG_CLASS OwnClass => GameState.Get().GetFriendlySidePlayer().GetClass();
        public static TAG_CLASS OpponentClass = GameState.Get().GetOpposingSidePlayer().GetClass();

        //public static DeckTypeDetector.DeckType CurrentDeckType = DeckTypeDetector.DeckType.UNKNOWN;

        public static int MaxManaCost
        {
            get
            {
                if (GameMgr.Get().IsArena())
                    return 4;

                if (OwnClass == TAG_CLASS.HUNTER ||OwnClass == TAG_CLASS.WARLOCK)
                    return 3;

                return 4;
            }
        }

        static Nullable<Int64> GetDeckId()
        {
            GameMgr mgr = GameMgr.Get();
            FieldInfo field = mgr.GetType().GetField("m_lastDeckId", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Nullable<Int64>) field.GetValue(mgr);
        }

        public static bool DeckFound => GetDeckId() != null;
        public static List<EntityDef> Deck
        {
            get
            {
                var id = GetDeckId();
                var deck = CollectionManager.Get().GetDeck(id ?? 0);
                var l = new List<EntityDef>();
                foreach (var slot in deck.GetSlots())
                {
                    for (int i = 0; i < slot.Count; i++)
                    {
                        var entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
                        l.Add(entityDef);
                    }
                }
                return l;
            }
        }

        public static List<Card> HandCards
        {
            get
            {
                var l = new List<Card>();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var card in GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards())
                {
                    if (card.GetEntity().GetCardId() != "GAME_005")
                        l.Add(card);
                }
                return l;
            }
        }

        public static DeckTypeDetector.DeckType CurrentDeckType { get; set; }
    }
}
