using System;
using System.Collections.Generic;
using HearthstoneInjectionDll;
using BotPlay = HearthstoneInjectionDll.Lib.BotPlay;
// ReSharper disable LoopCanBeConvertedToQuery
using VCard = HearthstoneInjectionDll.Lib.VirtualCard;

// ReSharper disable once CheckNamespace
namespace Behaviour
{
    public class EntryPoint
    {
        public static List<BotPlay> OnQueryBotPlays()
        {
            var rawPlays = CreateRawBotPlays();
            List<BotPlay[]> perms = CreatePermutations(rawPlays);
            List<BotPlaysResult> validPerms = GetValidPermutations(perms);

            BotPlaysResult bestPlayResult = GetBestPerm(validPerms);
            return bestPlayResult != null ? bestPlayResult.BotPlayList : new List<BotPlay>();
        }

        public static Card OnQueryCardChoice()
        {
            return ChoiceCardMgr.Get().GetFriendlyCards()[0];
        }

        public static Card OnQueryUnexpectedTarget()
        {
            return Lib.Collector.OwnBoardCards[0];
        }

        /// <summary>
        /// Ignore if valid
        /// </summary>
        /// <returns></returns>
        static List<BotPlay> CreateRawBotPlays()
        {
            List<BotPlay> list = new List<BotPlay>();
            var myHand = Lib.Collector.HandCards;

            foreach (var card in myHand)
            {
                //Play
                var e = card.GetEntity();
                if (e.IsMinion())
                {
                    list.Add(new BotPlay { Source = card });
                }

                if (Lib.HasBattleCryTargetAction(card) && e.IsMinion())
                {
                    foreach (var enemyCard in Lib.GetPotentialBoardTargets(Lib.GetBattleCryTargetAction(card)))
                    {
                        list.Add(new BotPlay { Source = card, Target = enemyCard });
                    }
                }

                bool hastargets = false;
                if (e.IsSpell() && !hastargets)
                {
                    list.Add(new BotPlay { Source = card });
                }
                else if (e.IsSpell() && hastargets)
                {
                    foreach (var enemyCard in Lib.Collector.EnemyBoardCards)
                    {
                        list.Add(new BotPlay { Source = card, Target = enemyCard });
                    }
                }
                //Play

                //Attack Afterwards ? (can get charge?)
                if (e.IsMinion())
                {
                    foreach (var enemyCard in Lib.Collector.EnemyBoardCards)
                    {
                        list.Add(new BotPlay { Source = card, Target = enemyCard });
                    }
                }
                //Attack Afterwards
            }

            foreach (var card in Lib.Collector.OwnBoardCards)
            {
                foreach (var enemyCard in Lib.Collector.EnemyBoardCards)
                {
                    list.Add(new BotPlay {Source = card, Target = enemyCard});
                }
            }

            return list;
        }

        static List<T[]> CreatePermutations<T>(List<T> originalArray, int max = 3000)
        {
            List<T[]> subsets = new List<T[]>();

            for (int i = 0; i < originalArray.Count; i++)
            {
                int subsetCount = subsets.Count;
                subsets.Add(new T[] { originalArray[i] });

                for (int j = 0; j < subsetCount; j++)
                {
                    T[] newSubset = new T[subsets[j].Length + 1];
                    subsets[j].CopyTo(newSubset, 0);
                    newSubset[newSubset.Length - 1] = originalArray[i];

                    if (subsets.Count < max)
                        subsets.Add(newSubset);
                    else break;
                }
            }

            return subsets;
        }

        static List<VCard> GetVirtualBoard()
        {
            List<VCard> r = new List<VCard>();
            foreach (var bCard in Lib.Collector.BoardCards)
            {
                r.Add(new VCard(bCard));
            }
            return r;
        }

        static List<VCard> GetVirtualHand()
        {
            List<VCard> r = new List<VCard>();
            foreach (var hCard in Lib.Collector.HandCards)
            {
                r.Add(new VCard(hCard));
            }
            return r;
        }

        static List<VCard> GetVirtualSecretZone()
        {
            List<VCard> r = new List<VCard>();
            foreach (var hCard in Lib.Collector.SecretZoneCards)
            {
                r.Add(new VCard(hCard));
            }
            return r;
        }


        static VCard GetVCardInList(List<VCard> list, Card realCard)
        {
            if (realCard == null)
                return null;

            foreach (var virtualCard in list)
            {
                if (virtualCard.CardID == realCard.GetEntity().GetCardId() &&
                    virtualCard.IsAlly == realCard.GetController().IsFriendlySide())
                    return virtualCard;
            }
            return null;
        }

        static bool Found(List<VCard> virtualList, Card realCard)
        {
            if (realCard == null)
                return false;

            foreach (var virtualCard in virtualList)
            {
                if (virtualCard.CardID == realCard.GetEntity().GetCardId() &&
                    virtualCard.IsAlly == realCard.GetController().IsFriendlySide())
                    return true;
            }
            return false;
        }

        static bool IsVAttackValid(List<VCard> vBoard, BotPlay play)
        {
            Card Target = play.Target;
            var vTarget = GetVCardInList(vBoard, Target);

            if (vTarget.IsImmune)
                return false;

            if (play.Source.GetEntity().IsHero() && Lib.GetWeapon(true) == null)
                return false;

            var enemyTaunts = new List<VCard>();
            foreach (var vCard in vBoard)
            {
                if (vCard.IsAlly)
                    continue;

                if (vCard.HasTaunt)
                    enemyTaunts.Add(vCard);
            }

            if (enemyTaunts.Count == 0)
                return true;

            foreach (var virtualCard in enemyTaunts)
            {
                if (virtualCard == vTarget)
                    return true;
            }

            return false;
        }

        static int AllyMinionsCount(List<VCard> vboard)
        {
            int i = 0;
            foreach (var virtualCard in vboard)
            {
                if (virtualCard.IsAlly)
                    i++;
            }
            return i;
        }

        delegate VCard _Func();

        static void UpdateVBoardCard(ref List<VCard> vBoard, _Func updateFunc)
        {
            VCard updatedVCard = updateFunc.Invoke();
            List<VCard> updatedList = new List<VCard>();
            foreach (var virtualCard in vBoard)
            {
                if (virtualCard.RealCard.GetEntity().GetCardId() == updatedVCard.RealCard.GetEntity().GetCardId() &&
                    virtualCard.IsAlly == updatedVCard.IsAlly)
                {
                    if (updatedVCard.Health > 0)
                        updatedList.Add(updatedVCard);
                }
                else updatedList.Add(virtualCard);
            }
            vBoard = updatedList;
        }

        static int GetSpellPower(List<VCard> vBoard)
        {
            int dmg = 0;
            foreach (var virtualCard in vBoard)
            {
                if (!virtualCard.IsAlly)
                    continue;
                if (virtualCard.IsSilenced)
                    continue;

                dmg += virtualCard.SpellPower;
            }
            return dmg;
        }

        static bool IsHoldingDragon(List<VCard> vHand)
        {
            foreach (var virtualCard in vHand)
            {
                if (virtualCard.RealCard.GetEntity().GetRace() == TAG_RACE.DRAGON)
                    return true;
            }
            return false;
        }

        static bool IsSecretActive(List<VCard> vSecretZone)
        {
            return vSecretZone.Count > 0;
        }

        class BotPlaysResult
        {
            public List<VCard> VHand;
            public List<VCard> VBoard;
            /// <summary>
            /// Valid
            /// </summary>
            public List<BotPlay> BotPlayList;
        }

        static bool AlreadyInList(List<BotPlaysResult> validPerms, List<BotPlay> newPlays)
        {
            bool anySame = false;
            foreach (var botPlaysResult in validPerms)
            {
                bool same = true;
                if (botPlaysResult.BotPlayList.Count != newPlays.Count)
                {
                    same = false;
                    goto skip;
                }

                for (int i = 0; i < botPlaysResult.BotPlayList.Count; i++)
                {
                    if (botPlaysResult.BotPlayList[i].ToString() != newPlays[i].ToString())
                    {
                        same = false;
                        break;
                    }
                }
                
                skip:
                if (same)
                {
                    anySame = true;
                    break;
                }
            }
            return anySame;
        }

        private static List<BotPlaysResult> GetValidPermutations(List<BotPlay[]> perms)
        {
            List<BotPlaysResult> validPerms = new List<BotPlaysResult>();
            foreach (BotPlay[] playList in perms)
            {
                int mana = Lib.Collector.ManaMgr.GetSpendableManaCrystals();
                List<VCard> vBoard = GetVirtualBoard();
                List<VCard> vHand = GetVirtualHand();
                List<VCard> vSectetZone = GetVirtualSecretZone();
                List<BotPlay> validPlayList = new List<BotPlay>();

                foreach (BotPlay play in playList)
                {
                    VCard vCard = GetVCardInList(vHand, play.Source) ?? GetVCardInList(vBoard, play.Source);
                    VCard targetVCard = GetVCardInList(vBoard, play.Target);

                    if ((vCard == null && play.Source != null) || (targetVCard == null && play.Target != null))
                        continue;

                    if (play.IsSummonOnly && Found(vHand, play.Source))//inHand
                    {
                        if (vCard.Cost > mana || AllyMinionsCount(vBoard) >= 7)
                            continue;

                        vHand.Remove(vCard);

                        if (play.Source.GetEntity().IsMinion())
                        {
                            vCard.PlayedThisTurn = true;
                            vBoard.Add(vCard);
                        }
                        else if (play.Source.GetEntity().IsSecret())
                            vSectetZone.Add(vCard);
                        else if (play.Source.GetEntity().IsSpell())
                        {
                            if (play.Source.GetEntity().GetCardId() == "GAME_005")//coin
                                mana++;
                            else if (play.Source.GetEntity().GetCardId() == "EX1_169") //innvervate
                                mana += 2;
                        }
                        mana -= vCard.Cost;
                        validPlayList.Add(play);
                    }

                    if (play.IsTargeted && Found(vBoard, play.Target)) //onBoard
                    {
                        if (Found(vHand, play.Source) && play.Source.GetEntity().IsSpell()) //spell in hand
                        {
                            if (vCard.Cost > mana || targetVCard.IsImmune || targetVCard.IsStealthed)
                                continue;

                            var board = vBoard;
                            _Func f1 = delegate
                            {
                                int spellPower = GetSpellPower(board);
                                vCard.Health -= play.Source.GetEntity().GetDamage() + spellPower;
                                return vCard;
                            };
                            UpdateVBoardCard(ref vBoard, f1);

                            vHand.Remove(vCard);
                            validPlayList.Add(play);
                        }
                        else if (Found(vBoard, play.Source) && Found(vBoard, play.Target) &&
                            IsVAttackValid(vBoard, play) && vCard.CanAttack)//minion attack
                        {
                            if (targetVCard.IsImmune || targetVCard.IsStealthed)
                                continue;

                            _Func targetUpdate = delegate
                            {
                                targetVCard.Health -= vCard.Attack;
                                if (vCard.RealCard.GetEntity().IsHero() && Lib.GetWeapon(true) != null)
                                    targetVCard.Health -= Lib.GetWeapon(true).GetEntity().GetRealTimeAttack();
                                return targetVCard;
                            };
                            _Func allyUpdate = delegate
                            {
                                vCard.Health -= targetVCard.Attack;
                                return targetVCard;
                            };
                            UpdateVBoardCard(ref vBoard, allyUpdate);
                            UpdateVBoardCard(ref vBoard, targetUpdate);

                            vCard.VAttack();
                            validPlayList.Add(play);
                        }
                    }

                    if (play.IsTargetOnlyAction && Found(vBoard, play.Target))
                    {
                        var lastPlay = validPlayList[validPlayList.Count - 1];
                        if (lastPlay.IsSummonOnly && Lib.HasBattleCryTargetAction(lastPlay.Source))
                        {
                            #region HandleBattleCryAction

                            //Apply play on target
                            Lib.BattleCryTargetAction action = Lib.GetBattleCryTargetAction(play.Source);
                            if (!Lib.CorrespondsToBattleCryAction(play.Source, action))
                                continue;

                            if (targetVCard.IsImmune && !targetVCard.IsAlly)
                                continue;

                            if (action.OnlyIfHoldingDragon && !IsHoldingDragon(vHand))
                                continue;

                            if (action.OnlyIfSectretExists && !IsSecretActive(vSectetZone))
                                continue;

                            if (action.BecomeCopy)
                            {
                                vBoard.Remove(GetVCardInList(vBoard, play.Source));
                                var copy = targetVCard;
                                copy.IsAlly = true;
                                vBoard.Add(copy);
                            }
                            if (action.CopyDeathrattle)
                            {
                            }
                            //todo:add CopyDeathrattle
                            if (action.CopyStats)
                            {
                                var card = vCard;
                                var card1 = targetVCard;
                                _Func allyFunc = delegate
                                {
                                    card.Attack = card1.Attack;
                                    card.Health = card1.Health;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, allyFunc);
                            }
                            if (action.DestroyLegendary && play.Target.GetEntity().GetRarity() == TAG_RARITY.LEGENDARY)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.Health = -1;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }
                            else continue;

                            if (action.DestroyTaunt && targetVCard.HasTaunt && !targetVCard.IsSilenced)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    card.Health = -1;
                                    // ReSharper disable once AccessToModifiedClosure
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }
                            else continue;

                            if (action.Freeze)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.IsFrozen = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.GiveDivineShield)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.HasDivineShield = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.GiveImmune)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.IsImmune = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.GiveStealth)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.IsStealthed = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.GiveTaunt)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.HasTaunt = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.GiveWindfury)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.HasWindFury = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.MakeCopy)
                            {
                                var copy = targetVCard;
                                copy.IsAlly = true;
                                vBoard.Add(copy);
                            }

                            if (action.Make_1_1_CopyForHand)
                            {
                                var copy = targetVCard;
                                copy.IsAlly = true;
                                copy.Health = 1;
                                copy.Attack = 1;
                                copy.IsBuffed = false;
                                copy.Cost = 1;
                                vHand.Add(copy);
                            }

                            if (action.ReturnTargetToHand)
                            {
                                if (targetVCard.IsAlly)
                                {
                                    vBoard.Remove(targetVCard);
                                    vHand.Add(targetVCard);
                                }
                                else
                                    vBoard.Remove(targetVCard);
                            }

                            if (action.SetAttackToExactValue)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.Attack = action.ExactAttack;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.SetHealthToExactValue)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.Attack = action.ExactHealth;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.Silence)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    card.IsSilenced = true;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (vCard != null && action.SwapHealth)
                            {
                                var card = targetVCard;
                                var card1 = vCard;
                                _Func targetFunc = delegate
                                {
                                    card.Health = card1.Health;
                                    return card;
                                };
                                _Func allyFunc = delegate
                                {
                                    card1.Health = card.Health;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, allyFunc);
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.SwapStatsOfTargetItself)
                            {
                                var card = targetVCard;
                                _Func targetFunc = delegate
                                {
                                    int h = card.Health;
                                    int a = card.Attack;
                                    card.Attack = h;
                                    card.Health = a;
                                    return card;
                                };
                                UpdateVBoardCard(ref vBoard, targetFunc);
                            }

                            if (action.TransfromMinionInOneMoreCost)
                            {
                            }
                            //todo:Add TransfromMinionInOneMoreCost
                            if (action.TakeControl)
                            {
                                vBoard.Remove(targetVCard);
                                var c = targetVCard;
                                c.IsAlly = true;
                                vBoard.Add(c);
                            }

                            #endregion

                            validPlayList.Add(play);
                        }
                    }
                }

                if (validPlayList.Count > 0 && !AlreadyInList(validPerms, validPlayList))
                    validPerms.Add(new BotPlaysResult { BotPlayList = validPlayList, VBoard = vBoard, VHand = vHand });
            }
            return validPerms;
        }

        static float GetMissingHeroHpValue(int hp)
        {
            if (hp == 0)
                return float.MaxValue;

            int missingHp = 30 - hp;
            float k = (float) Math.Log(20)/29;
            return (float)Math.Pow((float) Math.E, k*missingHp);
        }
        //todo: add hero power (in general)
        private static BotPlaysResult GetBestPerm(List<BotPlaysResult> validPerms)
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
            overloadM = -0.829f,
            
            frozenM = -1,
            silencedM = -4 / 10f;//-4 is max: 0-10 scala => 10*x=-4 <=> -4/10

            float bestValue = -float.MaxValue;
            BotPlaysResult bestPerm = null;

            foreach (var perm in validPerms)
            {
                float value = 0;
                foreach (var vCard in perm.VBoard)
                {
                    float cardValue = 0;
                    int sign = vCard.IsAlly ? 1 : -1;

                    if (!vCard.RealCard.GetEntity().IsHero())
                        cardValue += vCard.Attack*attackM;
                    if (!vCard.RealCard.GetEntity().IsHero())
                        cardValue += vCard.Health * healthM;
                    else
                    {
                        cardValue -= GetMissingHeroHpValue(vCard.Health);
                    }
                    //todo: durability
                    if (vCard.HasDivineShield)
                        cardValue += divineM;
                    if (vCard.HasWindFury)
                        cardValue += windFuryM;
                    if (vCard.RealCard.GetEntity().IsFreeze())
                        cardValue += freezeM;
                    if (vCard.IsFrozen)
                        cardValue -= frozenM;
                    if (vCard.IsSilenced)
                        cardValue -= Lib.GetSilenceValue(vCard)*silencedM;
                    if (vCard.HasTaunt)
                        cardValue += tauntM;
                    if (vCard.IsStealthed)
                        cardValue += stealthM;
                    if (vCard.SpellPower > 0)
                        cardValue += vCard.SpellPower*spellPowerM;
                    if (vCard.RealCard.GetEntity().HasOverload())
                        cardValue += Lib.GetOverload(vCard)*overloadM;
                    if (vCard.RealCard.GetEntity().IsWeapon())
                        cardValue += vCard.Durablility * durabilityM;

                    value += sign*cardValue;
                }

                if (value > bestValue)
                {
                    bestValue = value;
                    bestPerm = perm;
                }
            }

            return bestPerm;
        }
    }
}
