using System.Collections.Generic;

namespace HearthstoneInjectionDll.Behaviour
{
    class Behaviour2
    {
        public static Lib.BotPlay OnQueryBotPlay()
        {
            if (Lib.IsInHand("GAME_005"))
                return new Lib.BotPlay {Source = Lib.GetHandCardById("GAME_005") };

            int mana = Lib.Collector.ManaMgr.GetSpendableManaCrystals();
            var handMinions = Lib.GetMinions(mana);

            var handMin = GetBestMinion(handMinions);
            if (handMin != null && Lib.Collector.OwnBoardCards.Count < 7)
                return new Lib.BotPlay { Source = handMin };

            var myBoard = Lib.Collector.OwnBoardCards;
            if (myBoard.Count > 0)
            {
                var bestBoardMinion = GetBestMinion(myBoard, true);
                if (bestBoardMinion != null)
                {
                    var taunt = Lib.GetEnemyTaunt();
                    if (taunt != null)
                        return new Lib.BotPlay { Source = bestBoardMinion, Target = taunt };

                    return new Lib.BotPlay { Source = bestBoardMinion, Target = Lib.GetHeroCard(false) };
                }
            }

            if (Lib.GetHeroCard(true).GetHeroPower().GetRealTimeCost() <= mana && !Lib.WasHeroPowerUsed)
                return new Lib.BotPlay { IsUseHeroPowerPlay = true };

            var maGun = Lib.GetWeapon(true);
            var hero = Lib.GetHeroCard(true);
            /*Own Hero got attack points but no weapon*/
            if (hero.GetEntity().CanAttack() && !hero.GetEntity().IsFrozen() && hero.GetEntity().GetNumAttacksThisTurn() == 0 &&
                hero.GetEntity().GetRealTimeAttack() > 0 &&
                maGun == null)
            {
                var taunt = Lib.GetEnemyTaunt();
                if (taunt != null)
                    return new Lib.BotPlay { Source = hero, Target = taunt };

                return new Lib.BotPlay { Source = hero, Target = Lib.GetHeroCard(false) };
            }

            if (maGun != null)
            {
                int maxAA = maGun.GetEntity().HasWindfury() ? 2 : 1;
                if (hero.GetEntity().GetNumAttacksThisTurn() < maxAA)
                {
                    var taunt = Lib.GetEnemyTaunt();
                    if (taunt != null)
                        return new Lib.BotPlay { Source = hero, Target = taunt };

                    return new Lib.BotPlay { Source = hero, Target = Lib.GetHeroCard(false) };
                }
            }

            return null;
        }


        static Card GetBestMinion(List<Card> minions, bool attack = false)
        {
            float
                attackM = 0.571f,
                healthM = 0.407f,
                divineM = 1.407f,
                windFuryM = 1.195f,
                freezeM = 1.027f,
                stealthM = 0.604f,
                durabilityM = 0.591f,
                tauntM = 0.512f,
                spellPowerM = 0.46f,
                overloadM = -0.829f;

            float bestVal = -float.MaxValue;
            Card bestCard = null;

            for (int i = 0; i < minions.Count; i++)
            {
                var min = minions[i];
                var e = min.GetEntity();

                float cardValue = 0;

                cardValue += e.GetATK() * attackM;
                cardValue += e.GetHealth() * healthM;
                   
                if (e.HasDivineShield())
                    cardValue += divineM;
                if (e.HasWindfury())
                    cardValue += windFuryM;
                if (e.IsFreeze())
                    cardValue += freezeM;
                if (e.HasTaunt())
                    cardValue += tauntM;
                if (e.ReferencesStealth())
                    cardValue += stealthM;
                if (e.HasSpellPower())
                    cardValue += e.GetSpellPower() * spellPowerM;
                if (e.HasOverload())
                    cardValue += Lib.GetOverload(new Lib.VirtualCard(min, true)) * overloadM;
                if (e.IsWeapon())
                    cardValue += e.GetDurability() * durabilityM;

                int maxAttacks = e.HasWindfury() ? 2 : 1;
                bool cantAttack = e.GetNumAttacksThisTurn() >= maxAttacks || !e.CanAttack() || e.IsAsleep();
                bool valid = !attack || (attack && !cantAttack);
                if (cardValue > bestVal && valid)
                {
                    bestVal = cardValue;
                    bestCard = min;
                }
            }

            return bestCard;
        }

        public static Card OnQueryCardChoice()
        {
            return ChoiceCardMgr.Get().GetFriendlyCards()[0];
        }

        public static Card OnQueryUnexpectedTarget()
        {
            return Lib.Collector.OwnBoardCards[0];
        }
    }
}
