// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable All

using System;
using System.IO;
using UnityEngine;

namespace HearthstoneInjectionDll
{
    public class Card
    {
        private int _count = 1;

        public Card(string id)
        {
            ID = id;

            var US_translationFile = LanguageManager.GetLanguageJsonContent("enUS");
            var myLang_translationFile = LanguageManager.GetLanguageJsonContent();

            var US_cardIdEndPos = US_translationFile.IndexOf("\"" + ID + "\"") + ID.Length + 1;
            var mayLang_cardIdEndPos = myLang_translationFile.IndexOf("\"" + ID + "\"") + ID.Length + 1;

            var nameStartPos = US_translationFile.IndexOf("name", US_cardIdEndPos) + 4 + 3;
            var nameEndPos = US_translationFile.IndexOf("\"", nameStartPos);

            var typeStartPos = US_translationFile.IndexOf("type", US_cardIdEndPos) + 4 + 3;
            var typeEndPos = US_translationFile.IndexOf("\"", typeStartPos);

            var costStartPos = US_translationFile.IndexOf("cost", US_cardIdEndPos) + 4 + 2;
            var costEndPos = US_translationFile.IndexOf("\"", costStartPos) - 1;

            var realCardName = US_translationFile.Substring(nameStartPos, nameEndPos - nameStartPos);
            imageName = realCardName.ToLower().Replace(" ", "-").Replace("'", "-").Replace(".", "");
            Type = US_translationFile.Substring(typeStartPos, typeEndPos - typeStartPos);

            var costSTR = US_translationFile.Substring(costStartPos, costEndPos - costStartPos);
            Cost = Convert.ToInt32(costSTR);

            if (Type == "Minion")
            {
                var atkStartPos = US_translationFile.IndexOf("attack", US_cardIdEndPos) + 6 + 2;
                var atkEndPos = US_translationFile.IndexOf("\"", atkStartPos) - 1;

                var hpStartPos = US_translationFile.IndexOf("health", US_cardIdEndPos) + 6 + 2;
                var hpEndPos = US_translationFile.IndexOf("\"", hpStartPos) - 1;

                var atkSTR = US_translationFile.Substring(atkStartPos, atkEndPos - atkStartPos);
                Atk = Convert.ToInt32(atkSTR);

                var hpSTR = US_translationFile.Substring(hpStartPos, hpEndPos - hpStartPos);
                Hp = Convert.ToInt32(hpSTR);
            }
            else
            {
                Atk = 0;
                Hp = 0;
            }


            var myLang_endPosOfIdBlock = myLang_translationFile.IndexOf("}", mayLang_cardIdEndPos);
            var myLang_descriptionStartPos = myLang_translationFile.IndexOf("text", mayLang_cardIdEndPos) + 4 + 3;
            var myLang_descriptionEndPos = myLang_translationFile.IndexOf("\"", myLang_descriptionStartPos);

            /*if no desctiption the next one after the current block will be taken =>  its pos > end block*/
            if (myLang_descriptionStartPos > myLang_endPosOfIdBlock)
            {
                Description = "No Description";
            }
            else
            {
                var descriptionRaw = myLang_translationFile.Substring(myLang_descriptionStartPos,
                    myLang_descriptionEndPos - myLang_descriptionStartPos);
                Description = descriptionRaw.Replace("<b>", "").Replace("</b>", "").Replace(".", ".\n").
                    Replace("$", "");
            }

            CardTexture = GetTextureByName(imageName);
        }

        public Texture CardTexture { get; private set; }

        public Texture CountTexture { get; private set; } = null;
        public string imageName { get; private set; }
        public string ID { get; private set; }
        public string Type { get; private set; }
        public int Atk { get; private set; }
        public int Hp { get; private set; }
        public int Cost { get; private set; }
        public string Description { get; private set; }

        public int Count
        {
            get { return _count; }
            set
            {
                CountTexture = GetTextureByName("frame_" + value);
                _count = value;
            }
        }

        private Texture GetTextureByName(string imageName)
        {
            if (imageName == "frame_1")
                return null;

            var mainDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              DeckManager.directoryName;

            if (!File.Exists(mainDirPath + @"\Images\" + imageName + ".png"))
            {
                Debug.AppendDesktopLog("noCardImageFound", imageName);
                imageName = "unknown";
            }

            var imgPath = "file:///" + mainDirPath + @"\Images\" + imageName + ".png";
            imgPath = imgPath.Replace("\\", "/");
            var w = new WWW(imgPath);

            return w.texture;
        }
    }
}