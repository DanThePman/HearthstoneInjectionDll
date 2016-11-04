using System.Collections.Generic;
using HearthstoneInjectionDll;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Mulligan
{
    class DeckTypeDetector
    {
        private static List<string> MECH_Cards = new List<string>
        {
            "GVG_085",//Annoy-o-Tron
            "GVG_030",//Anodized Robo Cub
            "GVG_069",//Antique Healbot
            "GVG_082",//Clockwork Gnome
            "AT_096",//Clockwork Knight
            "GVG_076",//Explosive Sheep
            "GVG_084",//Flying Machine
            "GVG_079",//Force-Tank MAX
            "GVG_023",//Goblin Auto-Barber
            "EX1_556",//Harvest Golem
            "GVG_078",//Mechanical Yeti
            "GVG_006",//Mechwarper
            "GVG_103",//Micro Machine
            "GVG_096",//Piloted Shredder
            "GVG_058",//Shielded Minibot
            "GVG_002",//Snowchugger
            "GVG_044",//Spider Tank
            "GVG_051",//Warbot
            "GVG_037",//Whirling Zap-o-matic
            "EX1_006",//Alarm-o-Bot
            "GVG_091",//Arcane Nullifier X-21
            "GVG_062",//Cobalt Guardian
            "EX1_102",//Demolisher
            "GVG_020",//Fel Cannon
            "GVG_027",//Iron Sensei
            "GVG_094",//Jeeves
            "GVG_034",//Mech-Bear-Cat
            "GVG_048",//Metaltooth Leaper
            "GVG_055",//Screwjank Clunker
            "GVG_072",//Shadowboxer
            "GVG_086",//Siege Engine
            "GVG_123",//Soot Spewer
            "GVG_093",//Target Dummy
            "GVG_083",//Upgraded Repair Bot
            "GVG_077",//Anima Golem
            "GVG_121",//Clockwork Giant
            "GVG_107",//Enhance-o Mechano
            "GVG_106",//Junkbot
            "GVG_119",//Blingtron 3000
            "GVG_007",//Flame Leviathan
            "GVG_016",//Fel Reaver
            "GVG_105",//Piloted Sky Golem
            "GVG_113",//Foe Reaper 4000
            "GVG_056",//Iron Juggernaut
            "GVG_116",//Mekgineer Thermaplugg
            "GVG_114",//Sneed's Old Shredder
            "GVG_111"//Mimiron's Head
        };

        private static List<string> OTK_WARRIOR_Cards = new List<string>
        {
            "EX1_604",//Frothing Berserker   
            "CS2_104",//Rampage
            "EX1_391",//Slam
            "EX1_603",//Cruel Taskmaster
            "CS2_103",//Charge
            "EX1_407",//Brawl
            "CS2_203",//Ironbeak Owl
            "EX1_096",//Loot Hoarder
            "EX1_007",//Acolyte of Pain
            "EX1_412",//Raging Worgen
            "CS2_147",//Gnomish Inventor
        };

        private static List<string> MIRACLE_ROGUE_Cards = new List<string>
        {
            "CS2_073",//Cold Blood
            "EX1_613",//Edwin VanCleef
            "EX1_012",//Bloodmage Thalnos
            "CS2_117",//Earthen Ring Farseer
            "EX1_116",//Leeroy Jenkins
            "EX1_095",//Gadgetzan Auctioneer
            "EX1_128",//Conceal
            "EX1_145",//Preparation
            "EX1_581",//Sap
            "EX1_278",//Shiv
            "EX1_284",//Azure Drake
            "CS2_075"//Sinister Strike
        };

        private static List<string> PIRATE_ROGUE_Cards = new List<string>
        {
            "AT_029",//Buccaneer
            "GVG_025",//One-eyed Cheat
            "CS2_146",//Southsea Deckhand
            "EX1_096",//Loot Hoarder
            "GVG_075",//Ship's Cannon
            "AT_033",//Burgle
            "AT_032",//Shady Dealer
            "EX1_134",//SI:7 Agent
            "CS2_077",//Sprint
            "EX1_012",//Bloodmage Thalnos
            "NEW1_018",//Bloodsail Raider
            "NEW1_027",//Southsea Captain
            "NEW1_022",//Dread Corsair
            "AT_070",//Skycap'n Kragg
            "GVG_022"//Tinker's Sharpsword Oil
        };

        private static List<string> MURLOC_Cards = new List<string>
        {
            "EX1_506",//Murloc Tidehunter
            "EX1_507",//Murloc Warleader
            "EX1_509",//Murloc Tidecaller
            "EX1_029",//Leper Gnome
            "EX1_554",//Snake Trap
            "EX1_319",//Flame Imp
            "CS2_065",//Voidwalker
            "EX1_508",//Grimscale Oracle
            "EX1_004",//Young Priestess
            "GVG_064",//Puddlestomper
            "NEW1_019",//Knife Juggler
            "CS2_059",//Blood Imp
            "EX1_366",//Sword of Justice
            "CS2_203",//Ironbeak Owl
            "FP1_004",//Mad Scientist
            "EX1_062",//Old Murk-Eye
            "EX1_116",//Leeroy Jenkins
            "CS2_173",//Bluegill Warrior
            "EX1_538",//Unleash the Hounds
            "EX1_539",//Kill Command
            "NEW1_031",//Animal Companion
            "EX1_536",//Eaglehorn Bow
            "BRM_013",//Quick Shot
            "EX1_610",//Explosive Trap
            "EX1_316",//Power Overwhelming
            "EX1_308",//Soulfire
            "EX1_089",//Arcane Golem
            "EX1_103",//Coldlight Seer
            "CS2_188",//Abusive Sergeant
            "EX1_162",//Dire Wolf Alpha
            "EX1_093",//Defender of Argus
            "EX1_136",//Redemption
            "AT_122"//Gormok the Impaler
        };

        private static List<string> HANDLOCK_WARLOCK_Cards = new List<string>
        {
            "EX1_302",//Mortal Coil
            "GVG_015",//Darkbomb
            "CS2_062",//Hellfire
            "FP1_001",//Zombie Chow
            "EX1_043",//Twilight Drake
            "EX1_066",//Acidic Swamp Ooze
            "EX1_105",//Mountain Giant
            "EX1_005",//Big Game Hunter
            "EX1_045",//Ancient Watcher
            "EX1_058",//Sunfury Protector
        };
        public enum DeckType
        {
            UNKNOWN,
            MECH,
            /// <summary>
            /// missing
            /// </summary>
            OTK_WARRIOR,
            GRIM_PATRON_WARRIOR,
            /// <summary>
            /// bot has problems running it
            /// </summary>
            FREEZE_MAGE,
            MIRACLE_ROGUE,//otk
            PIRAT_ROGUE,
            MURLOC,
            HANDLOCK_WAROCK,
        }

        public static DeckType SearchForDeckType()
        {
            try
            {
                DeckType thisDeckType = DeckType.UNKNOWN;
                var deckCards = MainLists.Deck;

                //cards are probably doubled included
                if (Lib.CountContains(deckCards, MECH_Cards) >= 7)
                {
                    thisDeckType = DeckType.MECH;
                }
                else if (Lib.CountContains(deckCards, OTK_WARRIOR_Cards) >= 10 &&
                    MainLists.OwnClass == TAG_CLASS.WARRIOR &&
                        !Lib.Any(deckCards, "BRM_019"))//Grim Patron
                {
                    thisDeckType = DeckType.OTK_WARRIOR;
                }
                else if (Lib.CountContains(deckCards, MIRACLE_ROGUE_Cards) >= 10 &&
                    MainLists.OwnClass == TAG_CLASS.ROGUE &&
                        Lib.Any(deckCards, "EX1_095"))//Gadgetzan Auctioneer
                {
                    thisDeckType = DeckType.MIRACLE_ROGUE;
                }
                else if (Lib.CountContains(deckCards, PIRATE_ROGUE_Cards) >= 10)
                {
                    thisDeckType = DeckType.PIRAT_ROGUE;
                }
                else if (Lib.CountContains(deckCards, MURLOC_Cards) >= 10)
                {
                    thisDeckType = DeckType.MURLOC;
                }
                else if (Lib.CountContains(deckCards, HANDLOCK_WARLOCK_Cards) >= 10)
                {
                    thisDeckType = DeckType.HANDLOCK_WAROCK;
                }

                return thisDeckType;
            }
            catch
            {
                return DeckType.UNKNOWN;
            }
        }

        public static void GenerateWhiteAndBlackListForSpecialDeck(DeckType typeOfDeck)
        {
            switch (typeOfDeck)
            {
                case DeckType.MECH:
                    MainLists.whiteList.Add("GVG_085");//Annoy-o-Tron
                    MainLists.whiteList.Add("GVG_030");//Anodized Robo Cub
                    MainLists.whiteList.Add("GVG_082");//Clockwork Gnome
                    MainLists.whiteList.Add("GVG_023");//Goblin Auto-Barber
                    MainLists.whiteList.Add("GVG_006");//Mechwarper
                    MainLists.whiteList.Add("GVG_103");//Micro Machine
                    MainLists.whiteList.Add("GVG_058");//Shielded Minibot
                    MainLists.whiteList.Add("GVG_002");//Snowchugger
                    MainLists.whiteList.Add("GVG_051");//Warbot
                    MainLists.whiteList.Add("GVG_037");//Whirling Zap-o-matic
                    MainLists.whiteList.Add("GVG_072");//Shadowboxer
                    MainLists.whiteList.Add("GVG_048");//Metaltooth Leaper
                    MainLists.whiteList.Add("GVG_093");//Target Dummy
                    MainLists.whiteList.Add("GVG_107");//Enhance-o Mechano
                    MainLists.whiteList.Add("GVG_013");//Cogmaster
                    MainLists.whiteList.Add("GVG_102");//Tinkertown Technician

                    MainLists.blackList.Add("GVG_069");//Antique Healbot
                    MainLists.blackList.Add("AT_096");//Clockwork Knight
                    MainLists.blackList.Add("GVG_076");//Explosive Sheep
                    MainLists.blackList.Add("GVG_079");//Force-Tank MAX
                    MainLists.blackList.Add("GVG_078");//Mechanical Yeti
                    MainLists.blackList.Add("GVG_096");//Piloted Shredder
                    MainLists.blackList.Add("GVG_044");//Spider Tank
                    MainLists.blackList.Add("EX1_006");//Alarm-o-Bot
                    MainLists.blackList.Add("GVG_091");//Arcane Nullifier X-21
                    MainLists.blackList.Add("GVG_062");//Cobalt Guardian
                    MainLists.blackList.Add("EX1_102");//Demolisher
                    MainLists.blackList.Add("GVG_020");//Fel Cannon
                    MainLists.blackList.Add("GVG_027");//Iron Sensei
                    MainLists.blackList.Add("GVG_094");//Jeeves
                    MainLists.blackList.Add("GVG_034");//Mech-Bear-Cat
                    MainLists.blackList.Add("GVG_055");//Screwjank Clunker
                    MainLists.blackList.Add("GVG_086");//Siege Engine
                    MainLists.blackList.Add("GVG_123");//Soot Spewer
                    MainLists.blackList.Add("GVG_083");//Upgraded Repair Bot
                    MainLists.blackList.Add("GVG_077");//Anima Golem
                    MainLists.blackList.Add("GVG_121");//Clockwork Giant
                    MainLists.blackList.Add("GVG_107");//Enhance-o Mechano
                    MainLists.blackList.Add("GVG_106");//Junkbot
                    MainLists.blackList.Add("GVG_119");//Blingtron 3000
                    MainLists.blackList.Add("GVG_007");//Flame Leviathan
                    MainLists.blackList.Add("GVG_016");//Fel Reaver
                    MainLists.blackList.Add("GVG_105");//Piloted Sky Golem
                    MainLists.blackList.Add("GVG_113");//Foe Reaper 4000

                    //Warrior cards
                    MainLists.whiteList.Add("CS2_106");//Fiery War Axe                      
                    MainLists.whiteList.Add("EX1_402");//Armorsmith
                    MainLists.whiteList.Add("FP1_021");//Death's Bite

                    MainLists.blackList.Add("CS2_105");//Heroic Strike
                    MainLists.blackList.Add("CS2_112");//Arcanite Reaper
                    MainLists.blackList.Add("GVG_055");//Screwjank Clunker
                    MainLists.blackList.Add("NEW1_011");//Kor'kron Elite
                    MainLists.blackList.Add("EX1_408");//Mortal Strike
                    MainLists.blackList.Add("GVG_110");//Dr. Boom

                    //Mage cards
                    MainLists.whiteList.Add("EX1_277");//Arcane Missiles
                    MainLists.whiteList.Add("NEW1_012");//Mana Wyrm
                    MainLists.whiteList.Add("CS2_024");//Frostbolt
                    MainLists.whiteList.Add("GVG_002");//Snowchugger

                    MainLists.blackList.Add("BRM_002");//Flamewaker
                    MainLists.blackList.Add("CS2_029");//Fireball
                    MainLists.blackList.Add("GVG_004");//Goblin Blastmage
                    MainLists.blackList.Add("EX1_559");//Archmage Antonidas
                    MainLists.blackList.Add("FP1_030");//Loatheb
                    break;
                case DeckType.OTK_WARRIOR:
                    if (Lib.Any(MainLists.HandCards, "EX1_412"))//Raging Worgen
                        MainLists.whiteList.Add("EX1_607");//Inner Rage
                    else
                        MainLists.blackList.Add("EX1_607");//Inner Rage
                    MainLists.whiteList.Add("FP1_024");//Unstable Ghoul
                    MainLists.whiteList.Add("CS2_106");//Fiery War Axe
                    MainLists.whiteList.Add("CS2_104");//Rampage
                    MainLists.whiteList.Add("FP1_021");//Death's Bite
                    MainLists.whiteList.Add("CS2_103");//Charge
                    MainLists.whiteList.Add("EX1_096");//Loot Hoarder
                    MainLists.whiteList.Add("EX1_412");//Raging Worgen
                    MainLists.whiteList.Add("EX1_400");//Whirlwind
                    MainLists.whiteList.Add("EX1_007");//Acolyte of Pain

                    if (MainLists.OpponentClass == TAG_CLASS.HUNTER)
                        MainLists.whiteList.Add("EX1_606");//Shield Block    
                    else
                        MainLists.blackList.Add("EX1_606");//Shield Block    

                    MainLists.blackList.Add("EX1_049");//Youthful Brewmaster
                    MainLists.blackList.Add("EX1_603");//Cruel Taskmaster
                    MainLists.blackList.Add("CS2_108");//Execute
                    MainLists.blackList.Add("EX1_410");//Shield Slam
                    MainLists.blackList.Add("EX1_391");//Slam                 
                    MainLists.blackList.Add("EX1_604");//Frothing Berserker              
                    MainLists.blackList.Add("EX1_407");//Brawl
                    MainLists.blackList.Add("CS2_203");//Ironbeak Owl
                    MainLists.blackList.Add("CS2_147");//Gnomish Inventor
                    MainLists.blackList.Add("BRM_028");//Emperor Thaurissan
                    MainLists.blackList.Add("GVG_069");//Antique Healbot
                    break;
                case DeckType.MIRACLE_ROGUE:
                    MainLists.whiteList.Add("CS2_072");//Backstab
                    MainLists.whiteList.Add("EX1_613");//Edwin VanCleef
                    MainLists.whiteList.Add("EX1_134");//SI:7 Agent
                    MainLists.whiteList.Add("EX1_131");//Defias Ringleader

                    if (Lib.Any(MainLists.HandCards, "CS2_233") &&//Blade Flurry
                        Lib.Any(MainLists.HandCards, "CS2_074")) //Deadly Poison
                    {
                        MainLists.whiteList.Add("CS2_074"); //Deadly Poison
                        MainLists.whiteList.Add("CS2_233"); //Blade Flurry
                    }
                    else
                    {
                        if (MainLists.OpponentClass == TAG_CLASS.PRIEST)
                            MainLists.whiteList.Add("CS2_074"); //Deadly Poison
                        else
                            MainLists.blackList.Add("CS2_074"); //Deadly Poison
                        MainLists.blackList.Add("CS2_233"); //Blade Flurry
                    }
                    if (MainLists.OpponentClass == TAG_CLASS.HUNTER)
                        MainLists.blackList.Add("EX1_129");//Fan of Knives
                    else
                        MainLists.blackList.Add("EX1_129");//Fan of Knives

                    MainLists.blackList.Add("EX1_124");//Eviscerate
                    MainLists.blackList.Add("CS2_075");//Sinister Strike
                    MainLists.blackList.Add("CS2_233");//Blade Flurry
                    MainLists.blackList.Add("EX1_012");//Bloodmage Thalnos
                    MainLists.blackList.Add("EX1_144");//Shadowstep
                    MainLists.blackList.Add("EX1_095");//Gadgetzan Auctioneer
                    MainLists.blackList.Add("CS2_073");//Cold Blood
                    MainLists.blackList.Add("EX1_116");//Leeroy Jenkins
                    MainLists.blackList.Add("CS2_117");//Earthen Ring Farseer
                    MainLists.blackList.Add("EX1_128");//Conceal
                    MainLists.blackList.Add("EX1_145");//Preparation
                    MainLists.blackList.Add("EX1_581");//Sap
                    MainLists.blackList.Add("EX1_278");//Shiv

                    MainLists.blackList.Add("EX1_284");//Azure Drake
                    MainLists.blackList.Add("CS2_080");//Assassin's Blade
                    break;
                case DeckType.PIRAT_ROGUE:
                    if (MainLists.OpponentClass != TAG_CLASS.MAGE)
                    {
                        MainLists.whiteList.Add("AT_029"); //Buccaneer
                        MainLists.whiteList.Add("GVG_025"); //One-eyed Cheat
                        MainLists.whiteList.Add("CS2_146"); //Southsea Deckhand
                        MainLists.whiteList.Add("EX1_096"); //Loot Hoarder
                    }
                    else
                    {
                        MainLists.blackList.Add("AT_029"); //Buccaneer
                        MainLists.blackList.Add("GVG_025"); //One-eyed Cheat
                        MainLists.blackList.Add("CS2_146"); //Southsea Deckhand
                        MainLists.blackList.Add("EX1_096"); //Loot Hoarder
                    }
                    if (Lib.Any(MainLists.HandCards, "CS2_074"))//Deadly Poison
                        MainLists.whiteList.Add("NEW1_022");//Dread Corsair
                    else
                        MainLists.blackList.Add("NEW1_022");//Dread Corsair
                    MainLists.whiteList.Add("CS2_074");//Deadly Poison
                    MainLists.whiteList.Add("EX1_134");//SI:7 Agent
                    MainLists.whiteList.Add("GVG_075");//Ship's Cannon
                    MainLists.whiteList.Add("EX1_124");//Eviscerate

                    MainLists.blackList.Add("AT_033");//Burgle
                    MainLists.blackList.Add("AT_032");//Shady Dealer
                    MainLists.blackList.Add("CS2_233");//Blade Flurry
                    MainLists.blackList.Add("CS2_077");//Sprint
                    MainLists.blackList.Add("EX1_012");//Bloodmage Thalnos
                    MainLists.blackList.Add("NEW1_018");//Bloodsail Raider
                    MainLists.blackList.Add("NEW1_027");//Southsea Captain
                    MainLists.blackList.Add("AT_070");//Skycap'n Kragg
                    MainLists.blackList.Add("EX1_284");//Azure Drake
                    MainLists.blackList.Add("CS2_080");//Assassin's Blade
                    MainLists.blackList.Add("GVG_022");//Tinker's Sharpsword Oil                   
                    break;
                case DeckType.MURLOC:
                    if (MainLists.OpponentClass != TAG_CLASS.MAGE)
                    {
                        MainLists.whiteList.Add("EX1_506"); //Murloc Tidehunter
                        MainLists.whiteList.Add("EX1_029");//Leper Gnome
                        MainLists.whiteList.Add("EX1_508");//Grimscale Oracle
                    }
                    else
                    {
                        MainLists.blackList.Add("EX1_506"); //Murloc Tidehunter
                        MainLists.blackList.Add("EX1_029");//Leper Gnome
                        MainLists.blackList.Add("EX1_508");//Grimscale Oracle
                    }

                    if (Lib.Count(MainLists.HandCards, "EX1_302") >= 2)
                    {
                        MainLists.chosenCards.Add("EX1_302");
                        MainLists.blackList.Add("EX1_302");
                    }
                    else
                        MainLists.whiteList.Add("EX1_302");//Mortal Coil
                    MainLists.whiteList.Add("EX1_507");//Murloc Warleader
                    MainLists.whiteList.Add("EX1_509");//Murloc Tidecaller                    
                    MainLists.whiteList.Add("EX1_554");//Snake Trap
                    MainLists.whiteList.Add("EX1_319");//Flame Imp
                    MainLists.whiteList.Add("CS2_065");//Voidwalker
                    MainLists.whiteList.Add("EX1_508");//Grimscale Oracle
                    MainLists.whiteList.Add("EX1_004");//Young Priestess
                    MainLists.whiteList.Add("GVG_064");//Puddlestomper
                    MainLists.whiteList.Add("NEW1_019");//Knife Juggler
                    MainLists.whiteList.Add("CS2_059");//Blood Imp

                    bool havingEnoughLowDropMinions = Lib.GetMinionsByCostM(2).Count >= 2;

                    if (havingEnoughLowDropMinions)
                        MainLists.whiteList.Add("EX1_366");//Sword of Justice
                    else
                        MainLists.blackList.Add("EX1_366");//Sword of Justice

                    MainLists.blackList.Add("CS2_168");//Murloc Raider
                    MainLists.blackList.Add("GVG_040");//Siltfin Spiritwalker
                    MainLists.blackList.Add("CS2_173");//Bluegill Warrior
                    MainLists.blackList.Add("CS2_203");//Ironbeak Owl
                    MainLists.blackList.Add("FP1_004");//Mad Scientist
                    MainLists.blackList.Add("EX1_062");//Old Murk-Eye
                    MainLists.blackList.Add("EX1_116");//Leeroy Jenkins
                    MainLists.blackList.Add("EX1_538");//Unleash the Hounds
                    MainLists.blackList.Add("EX1_539");//Kill Command
                    MainLists.blackList.Add("NEW1_031");//Animal Companion
                    MainLists.blackList.Add("EX1_536");//Eaglehorn Bow
                    MainLists.blackList.Add("BRM_013");//Quick Shot
                    MainLists.blackList.Add("EX1_610");//Explosive Trap
                    MainLists.blackList.Add("EX1_316");//Power Overwhelming
                    MainLists.blackList.Add("EX1_308");//Soulfire
                    MainLists.blackList.Add("EX1_089");//Arcane Golem
                    MainLists.blackList.Add("EX1_103");//Coldlight Seer
                    MainLists.blackList.Add("CS2_188");//Abusive Sergeant
                    MainLists.blackList.Add("EX1_162");//Dire Wolf Alpha
                    MainLists.blackList.Add("EX1_093");//Defender of Argus
                    MainLists.blackList.Add("EX1_136");//Redemption
                    MainLists.blackList.Add("AT_122");//Gormok the Impaler
                    MainLists.blackList.Add("AT_076");//Murloc Knight
                    MainLists.blackList.Add("NEW1_017");//Hungry Crab
                    MainLists.blackList.Add("EX1_310");//Doomguard
                    MainLists.blackList.Add("GVG_045");//Imp-losion

                    List<string> ids;
                    if (Lib.HasOneTwoThreeMurlocComboM(out ids))
                    {
                        foreach (var id in ids)
                        {
                            MainLists.whiteList.Add(id);
                        }
                    }
                    break;
                case DeckType.HANDLOCK_WAROCK:
                    /*whitelist > blacklist*/
                    foreach (var handCard in MainLists.HandCards)
                    {
                        MainLists.blackList.Add(handCard.GetEntity().GetCardId());
                    }

                    MainLists.whiteList.Add("EX1_302");//Mortal Coil
                    MainLists.whiteList.Add("GVG_015");//Darkbomb
                    MainLists.whiteList.Add("CS2_062");//Hellfire
                    MainLists.whiteList.Add("FP1_001");//Zombie Chow
                    MainLists.whiteList.Add("EX1_043");//Twilight Drake

                    if (MainLists.OpponentClass == TAG_CLASS.ROGUE ||
                        MainLists.OpponentClass == TAG_CLASS.WARRIOR)
                        MainLists.whiteList.Add("EX1_066");//Acidic Swamp Ooze

                    if (MainLists.OpponentClass != TAG_CLASS.HUNTER && MainLists.OpponentClass != TAG_CLASS.MAGE)
                        MainLists.whiteList.Add("EX1_105");//Mountain Giant
                    break;
            }
        }
    }
}
