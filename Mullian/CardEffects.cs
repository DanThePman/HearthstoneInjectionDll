using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    class CardEffects
    {
        private static List<string> CardsWithBadProperties = new List<string>
        {
            //Toxitron
            "EX1_577",//The Beast
            "CS2_227",//Venture Co. Mercenary
            "EX1_045",//Ancient Watcher
            "NEW1_030",//Deathwing
            "FP1_001",//Zombie Chow
            //"TU4c_001",//King Mukla
            "NEW1_021",//Doomsayer
            "GVG_076",//Explosive Sheep
            "EX1_097",//Abomination
            "FP1_024",//Unstable Ghoul
            "NEW1_020",//Wild Pyromancer
            "EX1_616",//Mana Wraith
            "FP1_017",//Nerub'ar Weblord
            "EX1_308",//Soulfire
        };

        public static bool HasBadEffect(Card card)
        {
            return CardsWithBadProperties.Contains(card.GetEntity().GetCardId()) 
                || card.GetEntity().HasOverload() || MainLists.blackList.Contains(card.GetEntity().GetCardId());
        }

        public static bool HasGoodEffect(Card _card)
        {
            var card = _card.GetEntity();
            return card.HasCharge() || card.HasDivineShield() || card.HasTag(GAME_TAG.FREEZE) ||
                card.HasTag(GAME_TAG.STEALTH) || card.HasTaunt() || card.HasWindfury() || card.GetSpellPower() > 0 ||
                MainLists.whiteList.Contains(card.GetCardId());
        }

        public static bool AnyBadEffect(List<Card> list)
        {
            if (list.Count == 0)
                return true;

            foreach (var card in list)
            {
                if (CardsWithBadProperties.Contains(card.GetEntity().GetCardId()))
                    return true;
            }
            return false;
        }

        public static List<Card> GetNoBadEffects(List<Card> l)
        {
            var r = new List<Card>();
            foreach (var card in l)
            {
                if (!HasBadEffect(card) && !MainLists.blackList.Contains(card.GetEntity().GetCardId()))
                    r.Add(card);
            }
            return r;
        }

        public static bool AnyNoBadEffect(List<Card> l)
        {
            return GetNoBadEffects(l).Count > 0;
        }
    }
}
