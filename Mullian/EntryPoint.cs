using System.Collections.Generic;
using HearthstoneInjectionDll;
// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    public class EntryPoint
    {
        public static List<Card> OnQueryMulligan()
        {
            LoadGeneralBlackListEntries();

            var detectedDeckType = DeckTypeDetector.SearchForDeckType();
            MainLists.CurrentDeckType = detectedDeckType;
            if (detectedDeckType == DeckTypeDetector.DeckType.UNKNOWN)
            {
                CreateDefaultLists();
            }
            else //particular deck type
            {
                DeckTypeDetector.GenerateWhiteAndBlackListForSpecialDeck(detectedDeckType);
            }

            ManageTwins();

            /*====================================main comparison===========================================*/
            /*prefers whitelist over blacklist*/
            var notChosen = new List<Card>();
            foreach (var handCard in MainLists.HandCards)
            {
                if (!MainLists.chosenCards.Contains(handCard.GetEntity().GetCardId()))
                    notChosen.Add(handCard);
            }
            foreach (Card card in notChosen)
            {
                string id = card.GetEntity().GetCardId();
                if (MainLists.whiteList.Contains(id))
                    MainLists.chosenCards.Add(id);
                else if (MainLists.blackList.Contains(id))
                    // ReSharper disable once RedundantJumpStatement
                    continue;
                else if (card.GetEntity().GetCost() <= MainLists.MaxManaCost &&
                    card.GetEntity().GetClass() == TAG_CLASS.NEUTRAL)
                {
                    var min = new CustomMinion(card);
                    min.ManageNeutralMinion();
                }
            }

            return FinalCards;
        }

        private static List<Card> FinalCards
        {
            get
            {
                var cards = new List<Card>();
                foreach (var chosenCardID in MainLists.chosenCards)
                {
                    foreach (var handCard in MainLists.HandCards)
                    {
                        if (handCard.GetEntity().GetCardId() == chosenCardID)
                        {
                            cards.Add(handCard);
                        }
                    }
                }
                return cards;
            }
        } 

        private static void ManageTwins()
        {
            bool allowTwins = true;
            int dontAllowAtMana = 3;
            var HC = MainLists.HandCards;

            for (int i = 0; i < HC.Count; i++)
            {
                for (int j = 0; j < HC.Count; j++)
                {
                    if (i != j && HC[i].GetEntity().GetCardId().Equals(HC[j].GetEntity().GetCardId()))
                    {
                        if (!allowTwins || HC[i].GetEntity().GetCost() >= dontAllowAtMana)
                        {
                            string id = HC[i].GetEntity().GetCardId();
                            if (!MainLists.chosenCards.Contains(id))
                            {
                                MainLists.chosenCards.Add(id);
                            }
                            MainLists.blackList.Add(id);
                            for (int z = 0; z < 10; z++)
                            {
                                if (MainLists.whiteList.Contains(id))
                                    MainLists.whiteList.Remove(id);
                                else break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Additionally organizates neutral minions with the whitelist instead of only the value
        /// 2 Mana or below
        /// </summary>
        private static void LoadGeneralWhiteListEntriesForNeutralMinions()
        {
            List<Card> HC = MainLists.HandCards;

            //Chromatic Dragonkin
            //Lance Bearer
            //Chromatic Prototype
            //Deathcharger

            MainLists.whiteList.Add("EX1_066");//Acidic Swamp Ooze
            MainLists.whiteList.Add("EX1_393");//Amani Berserker
            MainLists.whiteList.Add("GVG_085");//Annoy-o-Tron
            MainLists.whiteList.Add("AT_109");//Argent Watchman
            MainLists.whiteList.Add("CS2_172");//Bloodfen Raptor
            MainLists.whiteList.Add("EX1_012");//Bloodmage Thalnos
            MainLists.whiteList.Add("NEW1_018");//Bloodsail Raider

            if (MainLists.CurrentDeckType == DeckTypeDetector.DeckType.MURLOC)
            {
                MainLists.blackList.Add("CS2_173"); //Bluegill Warrior
            }
            else
            {
                MainLists.whiteList.Add("CS2_173"); //Bluegill Warrior
            }

            //HandCards_BoardCards.Count(x => x.Type == Card.CType.MINION && x.Id.ToString() != "EX1_162" && x.Cost <= 2) >= 2
            if (Lib.SortOutM(Lib.GetMinionsByCostM(2), "EX1_162").Count >= 2)
            {
                MainLists.whiteList.Add("EX1_162"); //Dire Wolf Alpha
                foreach (var minion in Lib.GetMinionsByCostM(2))
                {
                    //no bad effect?
                    if (CardEffects.HasBadEffect(minion))
                        continue;

                    MainLists.whiteList.Add(minion.GetEntity().GetCardId());
                }
            }
            else
                MainLists.blackList.Add("EX1_162"); //Dire Wolf Alpha

            if (Lib.Any(HC, "CS2_026")) //Frost Nova
            {
                MainLists.whiteList.Add("NEW1_021"); //Doomsayer
                MainLists.whiteList.Add("CS2_026");//Frost Nova
            }
            else
                MainLists.blackList.Add("NEW1_021"); //Doomsayer


            MainLists.whiteList.Add("NEW1_016");//Captain's Parrot
            MainLists.whiteList.Add("NEW1_023");//Faerie Dragon
            MainLists.whiteList.Add("CS2_121");//Frostwolf Grunt
            MainLists.whiteList.Add("GVG_081");//Gilblin Stalker
            MainLists.whiteList.Add("FP1_002");//Haunted Creeper
            MainLists.whiteList.Add("NEW1_019");//Knife Juggler
            MainLists.whiteList.Add("CS2_142");//Kobold Geomancer
            MainLists.whiteList.Add("EX1_096");//Loot Hoarder
            MainLists.whiteList.Add("EX1_100");//Lorewalker Cho
            MainLists.whiteList.Add("EX1_082");//Mad Bomber

            if (!CardEffects.AnyBadEffect(Lib.GetSpellsByCostM(2)))
            {
                MainLists.whiteList.Add("EX1_055"); //Mana Addict
                foreach (var spell in Lib.GetSpellsByCostM(2))
                {
                    MainLists.whiteList.Add(spell.GetEntity().GetCardId());
                }
            }
            else
                MainLists.blackList.Add("EX1_055"); //Mana Addict

            if (!CardEffects.AnyBadEffect(Lib.GetMinionsByCostM(3)))
            {
                MainLists.whiteList.Add("NEW1_037"); //Master Swordsmith
                MainLists.whiteList.Add(MinionHelper.GetBestValue(Lib.GetMinionsByCostM(3)).GetEntity().GetCardId());
            }
            else
                MainLists.blackList.Add("NEW1_037"); //Master Swordsmith

            MainLists.whiteList.Add("GVG_006");//Mechwarper
            MainLists.whiteList.Add("EX1_506");//Murloc Tidehunter
            MainLists.whiteList.Add("EX1_557");//Nat Pagle

            List<string> ok1 = new List<string> { "EX1_093" , "EX1_058", "GVG_076" };
            if (Lib.CountContains(HC, ok1) > 0)
            //Defender of Argus 
            //Sunfury Protector 
            //Explosive Sheep
            {
                MainLists.whiteList.Add("FP1_007"); //Nerubian Egg
                bool containsArgus = Lib.Any(HC, "EX1_093");
                bool containsProtector = Lib.Any(HC, "EX1_058");

                if (containsArgus)
                    MainLists.whiteList.Add("EX1_093");
                else if (containsProtector)
                    MainLists.whiteList.Add("EX1_058");
                else //must contain the sheep
                    MainLists.whiteList.Add("GVG_076");
            }
            else
                MainLists.blackList.Add("FP1_007"); //Nerubian Egg

            MainLists.whiteList.Add("EX1_015");//Novice Engineer
            MainLists.whiteList.Add("EX1_076");//Pint-Sized Summoner
            MainLists.whiteList.Add("GVG_064");//Puddlestomper
            MainLists.whiteList.Add("CS2_120");//River Crocolisk
            MainLists.whiteList.Add("GVG_075");//Ship's Cannon
            MainLists.whiteList.Add("GVG_067");//Stonesplinter Trogg
            MainLists.whiteList.Add("EX1_058");//Sunfury Protector
            MainLists.whiteList.Add("FP1_024");//Unstable Ghoul

            MainLists.whiteList.Add("NEW1_020");//Wild Pyromancer

            if (CardEffects.AnyNoBadEffect(Lib.GetMinionsByCostM(3)))
            {
                MainLists.whiteList.Add("CS2_188"); //Abusive Sergeant
                MainLists.whiteList.Add(
                    MinionHelper.GetBestValue(CardEffects.GetNoBadEffects(Lib.GetMinionsByCostM(3))).
                    GetEntity().GetCardId());
            }
            //dont force adding to blacklist => calculate minion value later

            MainLists.whiteList.Add("EX1_008");//Argent Squire
            MainLists.whiteList.Add("GVG_082");//Clockwork Gnome
            
            if (CardEffects.AnyNoBadEffect(Lib.GetOfRaceM(Lib.GetMinionsByCostM(3), TAG_RACE.MECHANICAL)))
            {
                MainLists.whiteList.Add("GVG_013"); //Cogmaster
                var mechsCost3 = CardEffects.GetNoBadEffects(Lib.GetOfRaceM(Lib.GetMinionsByCostM(3), TAG_RACE.MECHANICAL));
                MainLists.whiteList.Add(MinionHelper.GetBestValue(mechsCost3).GetEntity().GetCardId());
            }
            else
                MainLists.blackList.Add("GVG_013"); //Cogmaster

            MainLists.whiteList.Add("CS2_189");//Elven Archer

            if (MainLists.CurrentDeckType == DeckTypeDetector.DeckType.MURLOC)
                MainLists.whiteList.Add("EX1_508");//Grimscale Oracle
            //dont force blacklist
            MainLists.whiteList.Add("EX1_508");//Grimscale Oracle

            MainLists.whiteList.Add("NEW1_017");//Hungry Crab
            MainLists.whiteList.Add("EX1_029");//Leper Gnome
            MainLists.whiteList.Add("EX1_001");//Lightwarden
            MainLists.whiteList.Add("AT_082");//Lowly Squire
            MainLists.whiteList.Add("EX1_080");//Secretkeeper

            if (MainLists.CurrentDeckType != DeckTypeDetector.DeckType.MURLOC)
            {
                MainLists.whiteList.Add("CS2_168"); //Murloc Raider
                MainLists.blackList.Add("EX1_509");//Murloc Tidecaller
            }
            else
            {
                MainLists.blackList.Add("CS2_168"); //Murloc Raider
                MainLists.whiteList.Add("EX1_509");//Murloc Tidecaller
            }
            MainLists.whiteList.Add("CS2_168"); //Murloc Raider
            MainLists.whiteList.Add("EX1_509");//Murloc Tidecaller

            MainLists.whiteList.Add("EX1_405");//Shieldbearer
            MainLists.whiteList.Add("CS2_146");//Southsea Deckhand
            MainLists.whiteList.Add("CS2_171");//Stonetusk Boar
            MainLists.whiteList.Add("FP1_028");//Undertaker
            MainLists.whiteList.Add("EX1_011");//Voodoo Doctor
            MainLists.whiteList.Add("EX1_010");//Worgen Infiltrator
            MainLists.whiteList.Add("EX1_004");//Young Priestess
            MainLists.whiteList.Add("FP1_001");//Zombie Chow
            MainLists.whiteList.Add("GVG_093");//Target Dummy
            MainLists.whiteList.Add("EX1_301");//Felguard
        }

        private static void CreateDefaultLists()
        {
            LoadGeneralWhiteListEntriesForNeutralMinions();
            TGT.SetTGT_WhiteAndBlackList();

            #region ListManaging
            switch (MainLists.OpponentClass)
            {
                case TAG_CLASS.DRUID:
                    MainLists.whiteList.Add("EX1_591");//Auchenai Priest
                    MainLists.whiteList.Add("EX1_339");//Thoughtsteal
                    MainLists.whiteList.Add("EX1_621");//Circle of Healing
                    break;
                case TAG_CLASS.WARRIOR:
                    MainLists.whiteList.Add("EX1_591");//Auchenai Priest
                    MainLists.whiteList.Add("EX1_339");//Thoughtsteal
                    MainLists.whiteList.Add("EX1_621");//Circle of Healing
                    MainLists.whiteList.Add("CS2_179");//Sen'jin Shieldmasta
                    break;
                case TAG_CLASS.ROGUE:
                    MainLists.whiteList.Add("EX1_591");//Auchenai Priest
                    MainLists.whiteList.Add("EX1_621");//Circle of Healing
                    MainLists.whiteList.Add("EX1_339");//Thoughtsteal
                    MainLists.whiteList.Add("FP1_030");//Loatheb
                    MainLists.whiteList.Add("CS2_179");//Sen'jin Shieldmasta
                    break;
                case TAG_CLASS.PRIEST:
                    MainLists.whiteList.Add("EX1_591");//Auchenai Priest
                    MainLists.whiteList.Add("EX1_339");//Thoughtsteal
                    MainLists.whiteList.Add("EX1_621");//Circle of Healing
                    MainLists.whiteList.Add("FP1_030");//Loatheb
                    break;
            }

            switch (MainLists.OwnClass)
            {
                case TAG_CLASS.DRUID:
                    MainLists.blackList.Add("CS2_012");//Swipe
                    if (Lib.GetMinionsM().Count >= 2)
                        MainLists.blackList.Add("CS2_011");//Savage Roar
                    MainLists.whiteList.Add("EX1_169");//Innervate
                    MainLists.whiteList.Add("EX1_154");//Wrath
                    MainLists.whiteList.Add("CS2_005");//Claw

                    if (Lib.Any(MainLists.HandCards, "EX1_169")  /*already in whitelist*/
                        || Lib.Any(MainLists.HandCards, "CS2_013"))
                    {
                        MainLists.whiteList.Add("FP1_005");//Shade of Naxxramas
                        MainLists.whiteList.Add("CS2_013");//Wild Growth

                        MainLists.whiteList.Add("EX1_085");//Mind Control Tech
                        MainLists.whiteList.Add("GVG_096");//Piloted Shredder
                    }

                    if (Lib.Any(MainLists.HandCards, "CS2_002") /*already in whitelist*/)//Claw
                    {
                        MainLists.whiteList.Add("EX1_578"); //Savagery
                    }
                    else
                        MainLists.blackList.Add("EX1_578"); //Savagery
                    break;
                case TAG_CLASS.HUNTER:
                    MainLists.whiteList.Add("NEW1_031");//Animal Companion
                    MainLists.whiteList.Add("EX1_617");//Deadly Shot
                    MainLists.whiteList.Add("GVG_043");//Glaivezooka
                    MainLists.whiteList.Add("GVG_087");//Steamwheedle Sniper
                    MainLists.whiteList.Add("BRM_013");//Quick Shot
                    MainLists.whiteList.Add("FP1_011");//Webspinner
                    MainLists.whiteList.Add("DS1_184");//Tracking
                    MainLists.whiteList.Add("DS1_185");//Arcane Shot
                    MainLists.blackList.Add("GVG_026");//Feign Death
                    MainLists.blackList.Add("EX1_544");//Flare

                    if (CardEffects.GetNoBadEffects(Lib.GetMinionsByCostM(3)).Count >= 2)
                    {
                        MainLists.whiteList.Add("EX1_611"); //Freezing Trap
                        var minCost3 = CardEffects.GetNoBadEffects(Lib.GetMinionsByCostM(3));
                        MainLists.whiteList.Add(MinionHelper.GetBestValue(minCost3).GetEntity().GetCardId());
                    }
                    else //better get minions
                        MainLists.blackList.Add("EX1_611"); //Freezing Trap                                        

                    if (CardEffects.GetNoBadEffects(Lib.GetOfClassM(Lib.GetMinionsByCostM(3), TAG_CLASS.HUNTER)).Count >= 3)
                        MainLists.whiteList.Add("DS1_175");//Timber Wolf
                    else
                        MainLists.blackList.Add("DS1_175");//Timber Wolf

                    if (MainLists.HandCards.Count > 3)
                        MainLists.whiteList.Add("EX1_014");//Mukla
                    break;
                case TAG_CLASS.MAGE:
                    MainLists.whiteList.Add("EX1_608");//Sorcerer's Apprentice
                    MainLists.whiteList.Add("GVG_002");//Snowchugger
                    MainLists.whiteList.Add("CS2_024");//Frostbolt
                    MainLists.whiteList.Add("GVG_001");//Flamecannon
                    MainLists.whiteList.Add("CS2_mirror");//Mirror Image
                    MainLists.whiteList.Add("NEW1_012");//Mana Wyrm
                    MainLists.whiteList.Add("EX1_277");//Arcane Missiles
                    MainLists.whiteList.Add("FP1_004");//Mad Scientist
                    break;
                case TAG_CLASS.PALADIN:
                    MainLists.whiteList.Add("EX1_366");//Sword of Justice
                    MainLists.whiteList.Add("GVG_061");//Muster for Battle
                    MainLists.whiteList.Add("EX1_382");//Aldor Peacekeeper
                    MainLists.whiteList.Add("GVG_058");//Shielded Minibot
                    MainLists.whiteList.Add("EX1_362");//Argent Protector
                    MainLists.whiteList.Add("EX1_130");//Noble Sacrifice
                    MainLists.whiteList.Add("CS2_091");//Light's Justice
                    MainLists.whiteList.Add("EX1_363");//Blessing of Wisdom
                    MainLists.whiteList.Add("CS2_087");//Blessing of Might
                    MainLists.whiteList.Add("EX1_136");//Redemption
                    MainLists.whiteList.Add("FP1_020");//Avenges

                    if (MainLists.OpponentClass == TAG_CLASS.DRUID || MainLists.OpponentClass == TAG_CLASS.WARLOCK)
                        MainLists.whiteList.Add("GVG_101");//Scarlet Purifier
                    break;
                case TAG_CLASS.PRIEST:
                    MainLists.whiteList.Add("EX1_339");//Thoughtsteal
                    MainLists.whiteList.Add("GVG_072");//Shadowboxer
                    MainLists.whiteList.Add("CS2_234");//Shadow Word: Pain
                    MainLists.whiteList.Add("GVG_009");//Shadowbomber
                    MainLists.whiteList.Add("CS2_004");//Power Word: Shield
                    MainLists.whiteList.Add("CS2_235");//Northshire Cleric
                    MainLists.whiteList.Add("EX1_332");//Silence
                    MainLists.whiteList.Add("CS1_130");//Holy Smite
                    MainLists.whiteList.Add("CS1_129");//Inner Fire
                    MainLists.whiteList.Add("CS2_181");//Injured Blademaster

                    if (CardEffects.GetNoBadEffects(Lib.GetMinionsByCostM(3)).Count >= 0)
                        MainLists.whiteList.Add("CS2_236");//Divine Spirit
                    else
                        MainLists.blackList.Add("CS2_236");//Divine Spirit
                    if (Lib.Any(MainLists.HandCards, "CS2_181"))
                        MainLists.whiteList.Add("EX1_621"); // Circle of Healing
                    else
                        MainLists.blackList.Add("EX1_621");

                    if (MainLists.HandCards.Count > 3)
                    {
                        MainLists.whiteList.Add("CS2_004");//Power Word: Shield
                        MainLists.whiteList.Add("EX1_621");//Circle of Healing
                    }

                    if (MainLists.OpponentClass == TAG_CLASS.WARRIOR || MainLists.OpponentClass == TAG_CLASS.PALADIN)
                        MainLists.whiteList.Add("EX1_588");
                    else
                        MainLists.blackList.Add("EX1_588");

                    var ok1 = new List<string> { "FP1_001", "CS2_235", "GVG_081" };/*already in whitelist*/
                    if (Lib.CountContains(MainLists.HandCards, ok1)  > 1)
                    {
                        MainLists.whiteList.Add("FP1_001");
                        MainLists.whiteList.Add("CS2_004");//Power Word: Shield
                        MainLists.whiteList.Add("GVG_010");//Velen's Chosen    
                        MainLists.whiteList.Add("FP1_009");//Deathlord  
                    }
                    break;
                case TAG_CLASS.ROGUE:
                    MainLists.whiteList.Add("GVG_023"); //Goblinbarbier-o-Mat
                    MainLists.whiteList.Add("EX1_522"); //Geduldiger Attentäter
                    MainLists.whiteList.Add("EX1_124"); //Ausweiden
                    MainLists.whiteList.Add("CS2_074"); //Tödliches Gift
                    MainLists.whiteList.Add("CS2_073"); //Kaltblütigkeit
                    MainLists.whiteList.Add("CS2_072"); //Meucheln
                    MainLists.whiteList.Add("CS2_075"); //Finsterer Stoß
                    MainLists.whiteList.Add("EX1_145"); //Vorbereitung
                    MainLists.whiteList.Add("EX1_131");//Defias Ringleader
                    MainLists.whiteList.Add("EX1_129"); //Dolchfächer
                    MainLists.whiteList.Add("EX1_126"); //Verrat

                    bool b1 = CardEffects.GetNoBadEffects(Lib.GetSpellsByCostM(1)).Count > 0;
                    bool b2 = CardEffects.GetNoBadEffects(Lib.GetWeaponsByCostM(1)).Count > 0;
                    if (b1 || b2) //got no Defias Ringleader
                    {
                        MainLists.whiteList.Add("EX1_134"); //SI:7-Agent
                        MainLists.whiteList.Add("EX1_131");

                        if (b1)
                            MainLists.whiteList.Add(CardEffects.GetNoBadEffects(Lib.GetSpellsByCostM(1))[0].GetEntity().GetCardId());
                        if (b2)
                            MainLists.whiteList.Add(CardEffects.GetNoBadEffects(Lib.GetWeaponsByCostM(1))[0].GetEntity().GetCardId());
                    }
                    break;
                case TAG_CLASS.SHAMAN:
                    MainLists.whiteList.Add("EX1_248"); //Wildgeist
                    MainLists.whiteList.Add("EX1_575"); //Manafluttotem
                    MainLists.whiteList.Add("EX1_259"); //Gewittersturm
                    MainLists.whiteList.Add("EX1_258"); //Entfesselter Elementar
                    MainLists.whiteList.Add("GVG_037"); //Wirbelnder Zapp-o-Mat
                    MainLists.whiteList.Add("CS2_039"); //Windzorn
                    MainLists.whiteList.Add("EX1_247");//Stormforged Axe
                    MainLists.whiteList.Add("FP1_025");//Reincarnate
                    MainLists.whiteList.Add("EX1_565");//Flametongue Totem
                    MainLists.whiteList.Add("GVG_038");//Crackle
                    MainLists.whiteList.Add("CS2_045");//Rockbiter Weapon
                    MainLists.whiteList.Add("EX1_238");//Lightning Bolt
                    MainLists.whiteList.Add("CS2_037");//Frost Shock
                    MainLists.whiteList.Add("EX1_251");//Forked Lightning
                    MainLists.whiteList.Add("EX1_245");//Earth Shock
                    MainLists.whiteList.Add("EX1_243");//Dust Devil
                    MainLists.whiteList.Add("EX1_244");//Totemic Might
                    MainLists.whiteList.Add("CS2_041");//Ancestral Healing
                    break;
                case TAG_CLASS.WARLOCK:
                    MainLists.whiteList.Add("BRM_005");//Demonwrath
                    MainLists.whiteList.Add("CS2_065");//Voidwalker
                    MainLists.whiteList.Add("EX1_306");//Succubus
                    MainLists.whiteList.Add("EX1_596");//Demonfire
                    MainLists.whiteList.Add("GVG_015");//Darkbomb
                    MainLists.whiteList.Add("EX1_302");//Mortal Coil
                    MainLists.whiteList.Add("EX1_319");//Flame Imp
                    MainLists.whiteList.Add("CS2_059");//Blood Imp

                    MainLists.blackList.Add("EX1_308");//Soulfire
                    break;
                case TAG_CLASS.WARRIOR:
                    MainLists.whiteList.Add("EX1_604");//Frothing Berserker

                    var minCost3_ = CardEffects.GetNoBadEffects(Lib.GetMinionsByCostM(3));

                    if (CardEffects.AnyNoBadEffect(Lib.GetMinionsByCostM(3)))
                    {
                        MainLists.whiteList.Add("EX1_402"); //Armorsmith
                        MainLists.blackList.Add(MinionHelper.GetBestValue(minCost3_).GetEntity().GetCardId());
                    }
                    else
                        MainLists.blackList.Add("EX1_402"); //Armorsmith

                    bool anyOver1Hp = false;
                    foreach (var card in minCost3_)
                    {
                        if (card.GetEntity().GetHealth() > 1)
                        {
                            anyOver1Hp = true;
                            break;
                        }
                    }
                    //INNER RAGE
                    if (CardEffects.AnyNoBadEffect(minCost3_) && anyOver1Hp)
                        MainLists.whiteList.Add("CS2_104");//Rampage
                    if (Lib.Any(MainLists.HandCards, "EX1_607")) //Inner Rage
                    {
                        var strL = new List<string> {"EX1_007", "EX1_393", "BRM_019", "EX1_412" };
                        if (Lib.CountContains(MainLists.HandCards, strL) > 0)
                        {
                            MainLists.whiteList.Add("EX1_007"); //Acolyte
                            MainLists.whiteList.Add("EX1_393"); //Amani Berserker
                            MainLists.whiteList.Add("BRM_019");//Grim Patron
                            MainLists.whiteList.Add("EX1_412");//Raging Worgen
                            MainLists.whiteList.Add("EX1_607"); //Inner Rage
                        }
                        else
                        {
                            MainLists.blackList.Add("EX1_607");
                        }
                    }

                    //WHIRLWIND
                    if (Lib.Any(MainLists.HandCards, "EX1_400")) //Whirlwind
                    {
                        var strL = new List<string> { "EX1_007", "EX1_393", "BRM_019", "EX1_412" };
                        if (Lib.CountContains(MainLists.HandCards, strL) > 0)
                        {
                            MainLists.whiteList.Add("EX1_007"); //Acolyte
                            MainLists.whiteList.Add("EX1_393"); //Amani Berserker
                            MainLists.whiteList.Add("BRM_019");//Grim Patron
                            MainLists.whiteList.Add("EX1_412");//Raging Worgen
                            MainLists.whiteList.Add("EX1_400"); //Whirlwind
                        }
                        else
                        {
                            MainLists.blackList.Add("EX1_400"); //Whirlwind
                        }
                    }

                    float averageMana = Lib.AverageManaM();
                    if (averageMana < 4f)
                        MainLists.whiteList.Add("GVG_050");//Bouncing Blade

                    switch (MainLists.OpponentClass)
                    {
                        case TAG_CLASS.WARLOCK:
                            MainLists.whiteList.Add("EX1_402");//Armorsmith
                            MainLists.whiteList.Add("FP1_021");//Death's Bite
                            break;
                        case TAG_CLASS.SHAMAN:
                            MainLists.whiteList.Add("EX1_402");//Armorsmith
                            MainLists.whiteList.Add("FP1_021");//Death's Bite
                            break;
                        case TAG_CLASS.PRIEST:
                            MainLists.whiteList.Add("EX1_402");//Armorsmith
                            MainLists.whiteList.Add("EX1_606");//Shield Block
                            MainLists.whiteList.Add("EX1_410");//Shield Slam
                            MainLists.whiteList.Add("EX1_007");//Acolyte of Pain
                            MainLists.whiteList.Add("FP1_021");//Death's Bite
                            break;
                        case TAG_CLASS.ROGUE:
                            MainLists.whiteList.Add("EX1_606");//Shield Block
                            MainLists.whiteList.Add("EX1_410");//Shield Slam	
                            MainLists.whiteList.Add("FP1_021");//Death's Bite
                            break;
                    }

                    MainLists.whiteList.Add("EX1_391");//Slam
                    MainLists.whiteList.Add("CS2_105");//Heroic Strike
                    MainLists.whiteList.Add("CS2_106");//Fiery War Axe
                    MainLists.whiteList.Add("EX1_603");//Cruel Taskmaster
                    MainLists.whiteList.Add("NEW1_036");//Commanding Shout
                    MainLists.whiteList.Add("CS2_114");//Cleave
                    MainLists.whiteList.Add("GVG_051");//Warbot
                    if (Lib.AnyWeapon(MainLists.HandCards))
                    {
                        MainLists.whiteList.Add("EX1_409"); //Upgrade!
                        var ordered = Lib.OrderByCost(MainLists.HandCards);
                        var weapons = Lib.TakeWeapons(ordered);
                        MainLists.whiteList.Add(weapons[0].GetEntity().GetCardId());
                    }
                    MainLists.whiteList.Add("EX1_410");//Shield Slam
                    MainLists.whiteList.Add("CS2_108");//Execute
                    break;

            }
            #endregion ListManaging
        }

        private static void LoadGeneralBlackListEntries()
        {
            TAG_CLASS opponentClass = MainLists.OpponentClass;
            TAG_CLASS myClass = MainLists.OwnClass;
            if (opponentClass != TAG_CLASS.WARRIOR && opponentClass != TAG_CLASS.HUNTER &&
                opponentClass != TAG_CLASS.ROGUE)
            {
                MainLists.blackList.Add("EX1_007");//Acolyte of Pain
            }
            if (opponentClass != TAG_CLASS.HUNTER)
                MainLists.blackList.Add("TU4d_001");//Hemet Nesingwary

            if (opponentClass != TAG_CLASS.WARLOCK)
                MainLists.blackList.Add("AT_106");//Light's Champion

            MainLists.blackList.Add("AT_115");//Fencing Coach
            MainLists.blackList.Add("EX1_304");//Void Terror
            MainLists.blackList.Add("FP1_025");//Reincarnate
            MainLists.blackList.Add("CS2_038");//Ancestral Spirit

            MainLists.blackList.Add("EX1_349");//Divine Favor
            MainLists.blackList.Add("CS2_023");//Arcane Intellect
            MainLists.blackList.Add("CS2_011");//Savage roar
            MainLists.blackList.Add("EX1_622");//Shadow Word Death
            MainLists.blackList.Add("EX1_625");//Shadow Form
            MainLists.blackList.Add("DS1_233");//Mind Blast
            MainLists.blackList.Add("CS2_108");//Execute
            MainLists.blackList.Add("EX1_391");//Slam
            MainLists.blackList.Add("EX1_005");//BGH
            MainLists.blackList.Add("CS2_007");//Healing Touch
            MainLists.blackList.Add("EX1_246");//Hex 
            MainLists.blackList.Add("EX1_575");//Mana Tide Totem
            MainLists.blackList.Add("EX1_539");//Kill Command
            MainLists.blackList.Add("CS2_203");//Ironbeak Owl

            MainLists.blackList.Add("CS2_118");//Magma Rager
            MainLists.blackList.Add("EX1_132"); //Eye for an Eye
            MainLists.blackList.Add("CS2_231"); //Wisp

            MainLists.blackList.Add("EX1_294");//Mirror entity

            if (opponentClass != TAG_CLASS.WARLOCK)
                MainLists.blackList.Add("EX1_238");//Lightning Bolt

            MainLists.blackList.Add("EX1_565");//Flametongue Totem
            MainLists.blackList.Add("EX1_059");//Crazed Alchemist
            MainLists.blackList.Add("FP1_003");//Echoing Ooze
            MainLists.blackList.Add("GVG_108");//Recombobulator
            MainLists.blackList.Add("EX1_049");//Youthful Brewmaster
            MainLists.blackList.Add("NEW1_025");//Bloodsail Corsair
            MainLists.blackList.Add("CS2_169");//Young Dragonhawk
            MainLists.blackList.Add("EX1_408");//Mortal Strike
        }
    }
}
