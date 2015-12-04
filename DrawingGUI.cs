using System;
using UnityEngine;

namespace HearthstoneInjectionDll
{
    public class DrawingGUI : Player
    {
        private static readonly GUIStyle turnTimerStyle = new GUIStyle
        {
            fontSize = 20
        };

        /// <summary>
        ///     Draws from Gameplay.cs OnGUI
        /// </summary>
        public static void ExtendGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 40), "By DanThePman");
            var lastColor = GUI.color;

            DrawOwnCards(lastColor);
            DrawEnemyHand(lastColor);


            DrawTurnTimeLeft();
        }

        private static void DrawEnemyHand(Color lastColor)
        {
            var cardHeightScaling = Screen.height*0.0009057971f;

            GUI.Label(new Rect(10, 70, 200, 40), "Known Enemy Hand:"); //absoulte position

            float j = 0;
            foreach (var enemyCard in Enemy.GetKnownHandCards())
            {
                float xStart = 10;
                float xEnd = enemyCard.CardTexture.width; //end pos everytime in steps
                var yStart = 70 + 40 + j*(enemyCard.CardTexture.height*cardHeightScaling);
                var yEnd = enemyCard.CardTexture.height*cardHeightScaling;

                var drawingRect = new Rect(xStart, yStart, xEnd, yEnd);

                /*Draw enemy card hand image*/
                GUI.DrawTexture(drawingRect, enemyCard.CardTexture);

                /*draw x2 or more*/
                if (enemyCard.Count > 1)
                {
                    var frameRect = new Rect(xStart + xEnd - enemyCard.CountTexture.width,
                        yStart, enemyCard.CountTexture.width, yEnd);

                    GUI.DrawTexture(frameRect, enemyCard.CountTexture);
                }

                /*On mouse over texture*/
                if (drawingRect.Contains(Event.current.mousePosition))
                {
                    var cardStats = enemyCard.Type == "Minion"
                        ? "DMG: " +
                          enemyCard.Atk + " HP: " + enemyCard.Hp +
                          " Cost: " + enemyCard.Cost + "\n\n"
                        : string.Empty;

                    GUI.color = Color.white;
                    GUI.Label(new Rect(
                        xStart + xEnd + 10, yStart, 190, 500), cardStats + enemyCard.Description);
                    GUI.color = lastColor;
                }

                j++;
            }

            var yStartGeneral = 70 + 40 + j*(Enemy.GetKnownPlayedCards()[0].CardTexture.height*cardHeightScaling);
            GUI.Label(new Rect(10, yStartGeneral, 200, 40), "Known Enemy Played:");
            j++;

            float k =  j;
            foreach (var enemyCard in Enemy.GetKnownPlayedCards())
            {
                float xStart = 10;
                float xEnd = enemyCard.CardTexture.width; //end pos everytime in steps
                var yStart = 70 + 40 + 10 + k * (enemyCard.CardTexture.height * cardHeightScaling);
                var yEnd = enemyCard.CardTexture.height * cardHeightScaling;

                var drawingRect = new Rect(xStart, yStart, xEnd, yEnd);

                /*Draw enemy card hand image*/
                GUI.DrawTexture(drawingRect, enemyCard.CardTexture);

                /*draw x2 or more*/
                if (enemyCard.Count > 1)
                {
                    var frameRect = new Rect(xStart + xEnd - enemyCard.CountTexture.width,
                        yStart, enemyCard.CountTexture.width, yEnd);

                    GUI.DrawTexture(frameRect, enemyCard.CountTexture);
                }

                /*On mouse over texture*/
                if (drawingRect.Contains(Event.current.mousePosition))
                {
                    var cardStats = enemyCard.Type == "Minion"
                        ? "DMG: " +
                          enemyCard.Atk + " HP: " + enemyCard.Hp +
                          " Cost: " + enemyCard.Cost + "\n\n"
                        : string.Empty;

                    GUI.color = Color.white;
                    GUI.Label(new Rect(
                        xStart + xEnd + 10, yStart, 190, 500), cardStats + enemyCard.Description);
                    GUI.color = lastColor;
                }

                k++;
            }

            GUI.color = lastColor;
        }

        private static void DrawTurnTimeLeft()
        {
            if (lastTurnStartTick != 0)
            {
                var elapsedTime = (Environment.TickCount - lastTurnStartTick)/1000;

                var yPos = Screen.height*0.846875f;
                var xPos = Screen.width*0.56796875f;

                GUI.Label(new Rect(xPos, yPos, 20, 20), (75 - elapsedTime).ToString(), turnTimerStyle);
            }
        }

        private static void DrawOwnCards(Color lastColor)
        {
            var cardHeightScaling = Screen.height*0.0009057971f;

            var transparentColor = new Color(lastColor.r, lastColor.g, lastColor.b, 0.5f);

            float j = 0;
            foreach (var shownCard in shownCards)
            {
                float xStart = Screen.width - 10 - shownCard.CardTexture.width;
                float xEnd = shownCard.CardTexture.width; //end pos everytime in steps
                var yStart = 20 + j*(shownCard.CardTexture.height*cardHeightScaling);
                var yEnd = shownCard.CardTexture.height*cardHeightScaling;

                var drawingRect = new Rect(xStart, yStart, xEnd, yEnd);

                /*Draw card image*/
                GUI.DrawTexture(drawingRect, shownCard.CardTexture);

                /*draw x2 or more*/
                if (shownCard.Count > 1)
                {
                    var frameRect = new Rect(xStart + xEnd - shownCard.CountTexture.width,
                        yStart, shownCard.CountTexture.width, yEnd);

                    GUI.DrawTexture(frameRect, shownCard.CountTexture);
                }

                /*On mouse over texture*/
                if (drawingRect.Contains(Event.current.mousePosition))
                {
                    var cardStats = shownCard.Type == "Minion"
                        ? "DMG: " +
                          shownCard.Atk + " HP: " + shownCard.Hp +
                          " Cost: " + shownCard.Cost + "\n\n"
                        : string.Empty;

                    GUI.color = Color.white;
                    GUI.Label(new Rect(xStart - 200, yStart, 190, 500), cardStats + shownCard.Description);
                    GUI.color = lastColor;
                }
                j += 1;
            }

            //===============================Deck Cards=============================================

            GUI.color = transparentColor;

            var k = j;
            foreach (var deckCard in currentDeckCards)
            {
                float xStart = Screen.width - 10 - deckCard.CardTexture.width;
                float xEnd = deckCard.CardTexture.width; //end pos everytime in steps
                var yStart = 20 + k*(deckCard.CardTexture.height*cardHeightScaling);
                var yEnd = deckCard.CardTexture.height*cardHeightScaling;

                var drawingRect = new Rect(xStart, yStart, xEnd, yEnd);

                /*Draw card image*/
                GUI.DrawTexture(drawingRect, deckCard.CardTexture);

                /*draw x2 or more */
                if (deckCard.Count > 1)
                {
                    var frameRect = new Rect(xStart + xEnd - deckCard.CountTexture.width,
                        yStart, deckCard.CountTexture.width, yEnd);

                    GUI.DrawTexture(frameRect, deckCard.CountTexture);
                }

                /*On mouse over texture*/
                if (drawingRect.Contains(Event.current.mousePosition))
                {
                    var cardStats = deckCard.Type == "Minion"
                        ? "DMG: " +
                          deckCard.Atk + " HP: " + deckCard.Hp +
                          " Cost: " + deckCard.Cost + "\n\n"
                        : string.Empty;

                    GUI.color = Color.white; //anti transparency                    
                    GUI.Label(new Rect(xStart - 200, yStart, 190, 500), cardStats + deckCard.Description);
                    GUI.color = transparentColor;
                }
                k += 1;
            }

            GUI.color = lastColor;
        }
    }
}