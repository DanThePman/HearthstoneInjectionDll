using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace HearthstoneInjectionDll
{
    public class DeckManager
    {
        public static string directoryName = @"\HearthstoneBrainDotNet";

        /// <summary>
        ///     Creates a text file with the information of DeckLoader class
        /// </summary>
        public static void CreateDeckFile()
        {
            var mainDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              directoryName;

            var deckName = DeckContainer.currentDeckName;
            var deckId = DeckContainer.currentDeckId;

            using (var sw = new StreamWriter($"{mainDirPath}\\{deckName}.txt"))
            {
                sw.WriteLine(deckId);

                foreach (var deckCard in DeckContainer.GetLoadedCards())
                {
                    sw.WriteLine(deckCard);
                }
                sw.Close();
            }
            DeckContainer.ResetLoadedCards();
        }

        /// <summary>
        ///     Converts card ids to card names when creating the game
        /// </summary>
        /// <param name="deckPath"></param>
        /// <returns></returns>
        public static Dictionary<string, bool> GetDeckIds(string deckPath)
        {
            var cardIds = new Dictionary<string, bool>();
            var firstLine = true;

            using (var sr = new StreamReader(deckPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    if (firstLine)
                        firstLine = false;
                    else
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (line.Contains(" x2"))
                        {
                            cardIds.Add(line.Substring(0, line.IndexOf(" x2")), true);
                        }
                        else
                            cardIds.Add(line, false);
                    }
                }
                sr.Close();
            }

            return cardIds;
        }
    }
}