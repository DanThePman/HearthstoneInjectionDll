using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    class TGT
    {
        private static List<string> WhiteList = new List<string>
        {
            "AT_133",//Gadgetzan Jouster
            "AT_082",//Lowly Squire
            "AT_097",//Tournament Attendee
            "AT_089",//Boneguard Lieutenant
            "AT_094",//Flame Juggler
            "AT_080",//Garrison Commander
        };

        private static List<string> BlackList = new List<string>
        {
            "AT_092", //Ice Rager
        };

        public static void SetTGT_WhiteAndBlackList()
        {
            foreach (var whiteListedCard in WhiteList)
            {
                MainLists.whiteList.Add(whiteListedCard);
            }

            foreach (var blackListedCard in BlackList)
            {
                MainLists.whiteList.Add(blackListedCard);
            }
        }
    }
}
