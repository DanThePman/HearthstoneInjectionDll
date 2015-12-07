using System;
using System.Collections.Generic;
using System.IO;

namespace HearthstoneInjectionDll
{
    public class Player
    {
        protected static List<Card> shownCards = new List<Card>();
        protected static List<Card> currentDeckCards = new List<Card>();
        public static long currentDeckId;

        private static int gangUpRepeatCount; //gang up spams 4 times on casting

        protected static int lastTurnStartTick;

        /// <summary>
        /// Of OnBirth of Spell.cs (target not available for Gang up)
        /// </summary>
        /// <param name="cardNameInfinitive"></param>
        /// <param name="isFriendly"></param>
        public static void OnSpellCast(string cardNameInfinitive, bool isFriendly)
        {
            string sourceId = ConvertFromInfinitiveToId(cardNameInfinitive);

            if (isFriendly) //without gang up
            {
                if (sourceId == "LOE_002")
                {
                    OnForgottenTorchCast();
                }
            }
            else
            {
                Enemy.AddPlayed(sourceId);
                Enemy.RemoveOrReduceHandCard(sourceId);

                if (sourceId == "AT_035")
                {
                    OnBeneathTheGroundsCast();
                }
            }

        }

        public static void OnGameStart()
        {
            ResetCards();
            Enemy.Reset();

            CompareDeckIdsAndLoadDeckCards();
        }

        /// <summary>
        /// For minions only
        /// </summary>
        /// <param name="prevZone"></param>
        /// <param name="newZone"></param>
        /// <param name="cardNameInfinitive"></param>
        public static void OnCardZoneChanged(string prevZone, string newZone, 
            string cardNameInfinitive)
        {
            if (cardNameInfinitive.Contains("Hidden Entity"))
                return;

            string cardName = ConvertFromInfinitiveToId(cardNameInfinitive);

            if (prevZone == "OPPOSING HAND" && newZone == "OPPOSING PLAY")//always minion => play
            {
                Enemy.RemoveOrReduceHandCard(cardName);
                Enemy.AddPlayed(cardName);
            }

            /*doesn't handle forgotten torch + gang up*/
            if (prevZone == "OPPOSING PLAY" && newZone == "OPPOSING HAND")//always minion => sap
                Enemy.AddHand(cardName);

            if (prevZone == "OPPOSING DECK" && newZone == "OPPOSING PLAY")//alyways minion => play
                Enemy.AddHand(cardName);

            if (prevZone == "OPPOSING DECK" && newZone == "OPPOSING HAND")//probably both
                Enemy.AddHand(cardName);
            
        }
        /// <summary>
        /// No spam, triggers only once per real discard
        /// </summary>
        /// <param name="targetInfinitive"></param>
        public static void OnEnemyDiscard(string targetInfinitive)
        {
            string target = ConvertFromInfinitiveToId(targetInfinitive);
            Enemy.RemoveOrReduceHandCard(target);
            Enemy.AddPlayed(target);
        }
        public static void OnNewTurn()
        {
            lastTurnStartTick = Environment.TickCount;
        }

        /// <summary>
        /// On Object Activate (Spams)
        /// </summary>
        /// <param name="callerInfinitive">Source or spell name</param>
        /// <param name="targetInfinitive">Target or  nothing  </param>
        public static void OnAction(string callerInfinitive, string targetInfinitive)
        {
            /*target exists => caller exists as card*/ //Spams 4 times
            if (targetInfinitive != "0")
            {
                var caller = ConvertFromInfinitiveToId(callerInfinitive);
                var target = ConvertFromInfinitiveToId(targetInfinitive);
                var playerIdOfCaller = ConvertFromInfinitiveToPlayerId(callerInfinitive);

                CheckGangUpCast(caller, target, playerIdOfCaller);
            }
        }

        private static void OnForgottenTorchCast()
        {
            foreach (var deckCard in currentDeckCards)
            {
                if (deckCard.ID == "LOE_002t") //Roaring Torch to add
                {
                    deckCard.Count++;
                    return;
                }
            }

            var gangUpTargetCard = new Card("LOE_002t");
            currentDeckCards.Add(gangUpTargetCard);
        }

        private static void OnBeneathTheGroundsCast()
        {
            foreach (var deckCard in currentDeckCards)
            {
                if (deckCard.ID == "AT_036t")
                {
                    deckCard.Count += 3;
                    return;
                }
            }

            var nerubianCard = new Card("AT_036t") {Count = 3};
            currentDeckCards.Add(nerubianCard);
        }

        private static void CheckGangUpCast(string caller, string target, string playerIdOfCaller)
        {
            /*Gang up casted of the player itself*/
            if (caller == "BRM_007" && playerIdOfCaller == "1")
            {
                /*add new cards to deck*/
                if (gangUpRepeatCount == 0)
                {
                    foreach (var deckCard in currentDeckCards)
                    {
                        if (deckCard.ID == target)
                        {
                            deckCard.Count += 3;
                            ++gangUpRepeatCount;
                            return;
                        }
                    }

                    var gangUpTargetCard = new Card(target) {Count = 3};
                    currentDeckCards.Add(gangUpTargetCard);
                }
                ++gangUpRepeatCount;
            }

            if (gangUpRepeatCount == 4)
                gangUpRepeatCount = 0;
        }

        private static string ConvertFromInfinitiveToId(string infinitiveName)
        {
            var idStartPos = infinitiveName.IndexOf("cardId=", StringComparison.Ordinal) + 7;
            var idEndPos = infinitiveName.IndexOf(" ", idStartPos, StringComparison.Ordinal);

            return infinitiveName.Substring(idStartPos, idEndPos - idStartPos);
        }

        private static string ConvertFromInfinitiveToPlayerId(string infinitiveName)
        {
            var idStartPos = infinitiveName.IndexOf("player=", StringComparison.Ordinal) + 7;
            var idEndPos = infinitiveName.IndexOf("]", idStartPos, StringComparison.Ordinal);

            return infinitiveName.Substring(idStartPos, idEndPos - idStartPos);
        }

        private static void CompareDeckIdsAndLoadDeckCards()
        {
            string deckFileWithId = null;
            var deckFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                               @"\HearthstoneBrainDotNet");

            foreach (var deckFile in deckFiles)
            {
                if (!deckFile.Contains(".txt"))
                    continue;

                var sr = new StreamReader(deckFile);
                var deckID = Convert.ToInt64(sr.ReadLine());
                sr.Close();

                if (deckID == currentDeckId)
                {
                    deckFileWithId = deckFile;
                    break;
                }
            }

            if (deckFileWithId == null) return;

            /*cardID, isDoubled*/
            var cardIdsOfCurrentDeck = DeckManager.GetDeckIds(deckFileWithId);

            foreach (var cardId in cardIdsOfCurrentDeck)
            {
                var c = new Card(cardId.Key);
                if (cardId.Value) //twice in deck
                    c.Count = 2;

                currentDeckCards.Add(c);
            }
        }

        private static void ResetCards()
        {
            shownCards = new List<Card>();
            currentDeckCards = new List<Card>();

            gangUpRepeatCount = 0;

            lastTurnStartTick = 0;
        }

        public static void AddShownCard(string nameInfinitive)
        {
            var cardId = ConvertFromInfinitiveToId(nameInfinitive);


            var alreadyExisted = false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var shownCard in shownCards)
            {
                if (shownCard.ID == cardId)
                {
                    shownCard.Count++;
                    alreadyExisted = true;
                    break;
                }
            }

            if (!alreadyExisted)
            {
                var newCard = new Card(cardId);
                shownCards.Add(newCard);
            }

            UpdateCardCounts(cardId);
        }

        private static void UpdateCardCounts(string cardIdOfNewCard)
        {
            foreach (var deckCard in currentDeckCards)
            {
                if (deckCard.ID.Equals(cardIdOfNewCard))
                {
                    if (deckCard.Count > 1)
                        deckCard.Count--;
                    else
                    {
                        currentDeckCards.Remove(deckCard);
                    }
                    break;
                }
            }
        }

        public static void UpdateHandCards(List<string> currentHandCardsListInfinitive)
        {
        }
    }
}