using System.Collections.Generic;

// ReSharper disable LoopCanBeConvertedToQuery

namespace HearthstoneInjectionDll
{
    public class CollectorInst
    {
        public List<Card>
            HandCards = new List<Card>(), 
            OwnBoardCards = new List<Card>(), 
            EnemyBoardCards = new List<Card>(),
            SecretZoneCards = new List<Card>();

        public List<Card>
            HeroCards = new List<Card>(),
            FriendlyWeaponCards = new List<Card>(),
            EnemyWeaponCards = new List<Card>(),
            AllWeaponCards = new List<Card>();

        public ManaCrystalMgr ManaMgr { get; set; }

        public InputManager InputManager { get; set; }
        public EndTurnButton EndTurnButton { get; set; }
        public GameState GameState { get; set; }

        public List<Card> BoardCards
        {
            get
            {
                var list = OwnBoardCards;
                foreach (var enemyBoardCard in EnemyBoardCards)
                {
                    list.Add(enemyBoardCard);
                }
                return list;
            }
        }
    }

    public static class Lib
    {
        public static CollectorInst Collector = new CollectorInst();
        public static bool WasHeroPowerUsed;

        public class VirtualCard
        {
            // ReSharper disable once ConvertNullableToShortForm
            public VirtualCard(Card realCard, bool? isAlly = null)
            {
                var e = realCard.GetEntity();
                CardID = e.GetCardId();
                RealCard = realCard;
                IsBuffed = e.GetRealTimeAttack() > BaseAttack ||
                           e.GetRealTimeRemainingHP() > BaseHealth;

                IsStealthed = e.IsStealthed();
                IsFrozen = e.IsFrozen();
                IsSilenced = e.IsSilenced();
                IsImmune = e.IsImmune();

                HasDivineShield = e.HasDivineShield();

                _attack = e.GetRealTimeAttack();
                _health = e.IsWeapon() ? e.GetCurrentDurability() : e.GetRealTimeRemainingHP();
                Cost = e.GetRealTimeCost();
                SpellPower = e.GetSpellPower();

                BaseAttack = RealCard.GetEntity().GetATK();
                BaseHealth = RealCard.GetEntity().GetHealth();

                IsAlly = isAlly ?? e.IsControlledByFriendlySidePlayer();

                HasTaunt = e.HasTaunt();
                HasCharge = e.HasCharge();
                HasDivineShield = e.HasDivineShield();
            }

            public Card RealCard;
            public bool IsAlly;
            public string CardID;

            private int _health;
            public int Health
            {
                get
                {
                    if (!IsSilenced)
                        return _health;

                    return BaseHealth;
                }
                set
                {
                    if (IsSilenced)
                        BaseHealth = value;
                    else _health = value;
                }
            }

            private int _attack;
            public int Attack
            {
                get
                {
                    if (!IsSilenced)
                        return _attack;

                    return BaseAttack;
                }
                set
                {
                    if (IsSilenced)
                        BaseAttack = value;
                    else _attack = value;
                }
            }

            public int SpellPower;
            public int Cost;
            public int BaseAttack;
            public int BaseHealth;

            public bool IsStealthed, IsFrozen, IsSilenced, IsImmune, IsBuffed;
            public bool HasTaunt, HasDivineShield/*, HasInspire*/, HasCharge, HasWindFury;
            public bool PlayedThisTurn;

            private int AttackCount = 0;
            public void VAttack() { AttackCount++;}

            public bool CanAttack
            {
                get
                {
                    if ((PlayedThisTurn && HasCharge) || !PlayedThisTurn)
                    {
                        int maxAttacks = !IsSilenced && HasWindFury ? 2 : 1;
                        return AttackCount < maxAttacks;
                    }
                    return false;
                }
            }

            public float Durablility { get; internal set; }
        }

        public class BotPlay
        {
            public Card Source, Target;
            public bool IsTargeted => Target != null && Source != null;
            public bool IsSummonOnly => Target == null && Source != null;

            public bool IsUseHeroPowerPlay;

            /// <summary>
            /// Source card already exists on the battlefield
            /// </summary>
            public bool IsTargetOnlyAction => Source == null && Target != null;

            public bool PlayBeforeTargeting;

            public override string ToString()
            {
                string a = Target != null ? "TARGETED" : (Source != null ? "SUMMON" : "BATTLE_CRY_ACTION");
                string sourceName = Source != null ? Source.GetEntity().GetName() : "NULL";
                string targetName = Target != null ? Target.GetEntity().GetName() : "NULL";

                return "Action: "+ a + " - Source: " + sourceName + " | Target: " + targetName;
            }
        }

        public static int CountContains(List<EntityDef> list, List<string> stringList)
        {
            int count = 0;
            foreach (var l in list)
            {
                var element = l;

                if (stringList.Contains(element.GetCardId()))
                    count++;
            }

            return count;
        }

        public static int CountContains(List<Card> list, List<string> stringList)
        {
            int count = 0;
            foreach (var l in list)
            {
                var element = l;

                if (stringList.Contains(element.GetEntity().GetCardId()))
                    count++;
            }

            return count;
        }

        public static int Count(List<EntityDef> list, string cardid)
        {
            int c = 0;
            foreach (EntityDef def in list)
            {
                if (def.GetCardId() == cardid)
                    c++;
            }
            return c;
        }

        public static int Count(List<Card> list, string cardid)
        {
            int c = 0;
            foreach (Card cc in list)
            {
                if (cc.GetEntity().GetCardId() == cardid)
                    c++;
            }
            return c;
        }

        public static bool Any(List<EntityDef> list, string cardid)
        {
            return Count(list, cardid) > 0;
        }

        public static bool Any(List<Card> list, string cardid)
        {
            return Count(list, cardid) > 0;
        }

        public static bool AnyWeapon(List<Card> list)
        {
            foreach (var card in list)
            {
                if (card.GetEntity().IsWeapon())
                    return true;
            }
            return false;
        }

        public static Card GetEnemyTaunt()
        {
            foreach (var enemyBoardCard in Lib.Collector.EnemyBoardCards)
            {
                if (enemyBoardCard.GetEntity().HasTaunt() && !enemyBoardCard.GetEntity().IsImmune() &&
                    !enemyBoardCard.GetEntity().IsSilenced())
                    return enemyBoardCard;
            }

            return null;
        }

        public static List<Card> OrderByCost(List<Card> l)
        {
            int[] ar = new int[l.Count];

            int z = 0;
            foreach (var card in l)
            {
                ar[z] = card.GetEntity().GetCost();
                z++;
            }

            for (int i = 1; i < ar.Length; i++)
            {
                int index = ar[i]; int j = i;
                while (j > 0 && ar[j - 1] > index)
                {
                    ar[j] = ar[j - 1];
                    j--;
                }
                ar[j] = index;
            }

            List<Card> ordered = new List<Card>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var i in ar)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var card in l)
                {
                    if (card.GetEntity().GetCost() == i)
                        ordered.Add(card);
                }
            }

            return ordered;
        }

        public static List<Card> TakeWeapons(List<Card> l)
        {
            var r = new List<Card>();
            foreach (var card in l)
            {
                if (card.GetEntity().IsWeapon())
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetMinionsByCostM(int maxCost)
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().IsMinion() && card.GetEntity().GetCost() <= maxCost)
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetSpellsByCostM(int maxCost)
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().IsSpell() && card.GetEntity().GetCost() <= maxCost)
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetWeaponsByCostM(int maxCost)
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().IsWeapon() && card.GetEntity().GetCost() <= maxCost)
                    r.Add(card);
            }
            return r;
        }

        public static bool HasOneTwoThreeMurlocComboM()
        {
            bool one = false, two = false, three = false;
            foreach (var card in Collector.HandCards)
            {
                var e = card.GetEntity();
                if (!e.IsMinion() || e.GetRace() != TAG_RACE.MURLOC)
                    continue;

                if (e.GetCost() == 1) one = true;
                if (e.GetCost() == 2) two = true;
                if (e.GetCost() == 3) three = true;
            }
            return one && two && three;
        }

        public static bool HasOneTwoThreeMurlocComboM(out List<string> ids)
        {
            List<string> ids2 = new List<string>();
            bool one = false, two = false, three = false;
            foreach (var card in Collector.HandCards)
            {
                var e = card.GetEntity();
                if (!e.IsMinion() || e.GetRace() != TAG_RACE.MURLOC)
                    continue;

                if (e.GetCost() == 1)
                {
                    one = true;
                    ids2.Add(e.GetCardId());
                }
                if (e.GetCost() == 2)
                {
                    two = true;
                    ids2.Add(e.GetCardId());
                }
                if (e.GetCost() == 3)
                {
                    three = true;
                    ids2.Add(e.GetCardId());
                }
            }
            ids = ids2;
            return one && two && three;
        }

        public static List<Card> GetMinionsM()
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().IsMinion())
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetMinions(int maxMana = -1)
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (maxMana > -1 && card.GetEntity().GetRealTimeCost() > maxMana)
                    continue;

                if (card.GetEntity().IsMinion())
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> SortOutM(List<Card> list, string sortOutId)
        {
            var r = new List<Card>();
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().GetCardId() != sortOutId)
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetOfRaceM(List<Card> list, TAG_RACE race)
        {
            var r = new List<Card>();
            foreach (var card in list)
            {
                if (card.GetEntity().GetRace() == race)
                    r.Add(card);
            }
            return r;
        }

        public static List<Card> GetOfClassM(List<Card> list, TAG_CLASS cclass)
        {
            var r = new List<Card>();
            foreach (var card in list)
            {
                if (card.GetEntity().GetClass() == cclass)
                    r.Add(card);
            }
            return r;
        }

        public static float AverageManaM()
        {
            float f = 0;
            foreach (var card in Collector.HandCards)
            {
                f += card.GetEntity().GetCost();
            }
            f /= Collector.HandCards.Count;
            return f;
        }


        /// <summary>
        /// For targeting-scards after battlecry
        /// </summary>
        [ExternCall]
        public class BattleCryTargetAction
        {
            public string CardID;
            public int AttackBonus;
            public int HealthBonus;

            public int ExactAttack
            {
                get { return _exactAttack; }
                set
                {
                    SetAttackToExactValue = true;
                    _exactAttack = value;
                }
            }

            public int ExactHealth
            {
                get { return _exactHealth; }
                set
                {
                    SetHealthToExactValue = true;
                    _exactHealth = value;
                }
            }


            public bool GiveTaunt, GiveDivineShield;
            public bool SetHealthToExactValue, SetAttackToExactValue;


            public bool EnemyOnly, AllyOnly;
            public bool TwoWay => !EnemyOnly && !AllyOnly;

            public bool OnlyIfHoldingDragon;
            public bool XAttackOrLess, XAttackOrMore;
            
            private int _maxAttack, _minAttack;
            private bool _takeControl;

            /// <summary>
            /// Refereing to XAttackOrLess
            /// </summary>
            public int MaxAttack
            {
                get { return _maxAttack; }
                set
                {
                    XAttackOrLess = true;
                    _maxAttack = value;
                }
            }

            /// <summary>
            /// Refereing to _minAttack
            /// </summary>
            public int MinAttack
            {
                get { return _minAttack; }
                set
                {
                    XAttackOrMore = true;
                    _minAttack = value;
                }
            }


            public bool TakeControl
            {
                get { return _takeControl; }
                set
                {
                    EnemyOnly = true;
                    _takeControl = value;
                }
            }

            public bool Freeze;
            public bool CanTargetHero;
            private bool _canTargetHeroOnly;

            public bool CanTargetHeroOnly
            {
                get { return _canTargetHeroOnly; }
                set
                {
                    CanTargetHero = true;
                    _canTargetHeroOnly = value;
                }
            }

            public bool MakeCopy, BecomeCopy, Make_1_1_CopyForHand, CopyStats;
            public bool DestroyTaunt, DestroyLegendary;
            public TAG_RACE RaceOnly = TAG_RACE.INVALID;
            public bool SwapStats, SwapHealth;
            public bool ReturnTargetToHand;
            private int _exactAttack;
            private int _exactHealth;
            public bool GiveStealth;
            public bool TransfromMinionInOneMoreCost;
            public bool Silence;
            public bool GiveWindfury;
            public int HealthBonusInCombo;
            public bool GiveImmune;
            public bool CopyDeathrattle;
            public bool SwapStatsOfTargetItself;
            public bool OnlyIfSectretExists;
        }

//http://www.hearthpwn.com/cards?filter-battlecry=1
private static List<BattleCryTargetAction> BattleCryActionsCardDataBase = new List<BattleCryTargetAction>
{
new BattleCryTargetAction { CardID = "EX1_561", ExactHealth = 15, CanTargetHeroOnly = true},//Alextrasa
new BattleCryTargetAction {CardID = "OG_282", HealthBonus = -int.MaxValue},//Blade of C'Thun
new BattleCryTargetAction {CardID = "AT_103", HealthBonus = -4, CanTargetHero = true},//North Sea Kraken
new BattleCryTargetAction {CardID = "BRM_029", HealthBonus = -int.MaxValue, OnlyIfHoldingDragon = true, DestroyLegendary = true},//Rend Blackhand (!)
new BattleCryTargetAction {CardID = "KAR_033", HealthBonus = -int.MaxValue, EnemyOnly = true, OnlyIfHoldingDragon =true, MaxAttack = 3},
new BattleCryTargetAction {CardID = "EX1_091", TakeControl = true, MaxAttack = 2},//Cabal Shadow Priest
new BattleCryTargetAction {CardID = "CS2_042", HealthBonus = -3},//Fire Elemental
new BattleCryTargetAction {CardID = "EX1_283", Freeze = true},//Frost Elemental
new BattleCryTargetAction {CardID = "KAR_065", AllyOnly = true, MakeCopy = true},//Menagerie Warden
new BattleCryTargetAction {CardID = "KAR_041", HealthBonus = -int.MaxValue},//Moat Lurker
new BattleCryTargetAction {CardID = "EX1_623", HealthBonus = 3, AllyOnly = true},//Temple Enforcer
new BattleCryTargetAction {CardID = "EX1_002", HealthBonus = -int.MaxValue, DestroyTaunt = true},//The Black Knight
new BattleCryTargetAction {CardID = "EX1_005", HealthBonus = -int.MaxValue, MinAttack = 7},//Big Game Hunter
new BattleCryTargetAction {CardID = "BRM_034", HealthBonus = -3, CanTargetHero = true, OnlyIfHoldingDragon = true},//Blackwing Corruptor
new BattleCryTargetAction {CardID = "AT_096", HealthBonus = 1, AttackBonus = 1, AllyOnly = true, RaceOnly = TAG_RACE.MECHANICAL},//Clockwork Knight
new BattleCryTargetAction {CardID = "OG_102", AllyOnly = true, SwapStats = true},//Darkspeaker
new BattleCryTargetAction {CardID = "EX1_564", BecomeCopy = true},//Faceless Manipulator
new BattleCryTargetAction {CardID = "GVG_120", HealthBonus = -int.MaxValue, RaceOnly = TAG_RACE.PET},//Hemet Nesingwary
new BattleCryTargetAction {CardID = "OG_291", Make_1_1_CopyForHand = true, AllyOnly = true},//Shadowcaster
new BattleCryTargetAction {CardID = "CS2_150", HealthBonus = -2, CanTargetHero = true},//Stormpike Commando
new BattleCryTargetAction {CardID = "GVG_083", HealthBonus = 4, RaceOnly = TAG_RACE.MECHANICAL},//Upgraded Repair Bot
new BattleCryTargetAction {CardID = "GVG_014", SwapHealth = true},//Vol'jin
new BattleCryTargetAction {CardID = "EX1_057", ReturnTargetToHand = true, AllyOnly = true},//Ancient Brewmaster
new BattleCryTargetAction {CardID = "OG_174", CopyStats = true, AllyOnly = true},//Faceless Shambler
new BattleCryTargetAction {CardID = "OG_282", HealthBonus = 2, AttackBonus = 2, AllyOnly = true, RaceOnly = TAG_RACE.PET},//Houndmaster
new BattleCryTargetAction {CardID = "LOE_017", ExactHealth = 3, ExactAttack = 3},//Keeper of Uldaman
new BattleCryTargetAction {CardID = "NEW1_014", AllyOnly = true, GiveStealth = true},//Master of Disguise	
new BattleCryTargetAction {CardID = "OG_328", TransfromMinionInOneMoreCost = true, AllyOnly = true},//Master of Evolution
new BattleCryTargetAction {CardID = "KAR_A02_06", HealthBonus = 2, AttackBonus = 2},//Pitcher
new BattleCryTargetAction {CardID = "GVG_055",HealthBonus = 2, AttackBonus = 2, AllyOnly = true, RaceOnly = TAG_RACE.MECHANICAL},//Screwjank Clunker
new BattleCryTargetAction {CardID = "EX1_048", Silence = true},//Spellbreaker
new BattleCryTargetAction {CardID = "AT_040", HealthBonus = 3, RaceOnly = TAG_RACE.PET, AllyOnly = true},//Wildwalker
new BattleCryTargetAction {CardID = "EX1_587", AllyOnly = true, GiveWindfury = true},//Windspeaker
new BattleCryTargetAction {CardID = "EX1_133", HealthBonus = -1, HealthBonusInCombo = -2, CanTargetHero = true},//Perdition's Blade
new BattleCryTargetAction {CardID = "EX1_382", ExactHealth = 1, EnemyOnly = true},//Aldor Peacekeeper
new BattleCryTargetAction {CardID = "OG_162", HealthBonus = -2, CanTargetHero = true},//Disciple of C'Thun
new BattleCryTargetAction {CardID = "CS2_117", HealthBonus = 3, CanTargetHero = true},//Earthen Ring Farseer
new BattleCryTargetAction {CardID = "CS2_203", Silence = true},//Ironbeak Owl
new BattleCryTargetAction {CardID = "CS2_141", HealthBonus = -1, CanTargetHero = true},////Ironforge Rifleman
new BattleCryTargetAction {CardID = "AT_106", Silence = true, RaceOnly = TAG_RACE.DEMON},//Light's Champion
new BattleCryTargetAction {CardID = "EX1_019", HealthBonus = 1, AttackBonus = 1, AllyOnly = true},//Shattered Sun Cleric
new BattleCryTargetAction {CardID = "BRMC_91", HealthBonus = -6, CanTargetHero = true},//Son of the Flame (NA)
new BattleCryTargetAction {CardID = "AT_057", GiveImmune = true, RaceOnly = TAG_RACE.PET},//Stablemaster
new BattleCryTargetAction {CardID = "LOE_019", AllyOnly = true, CopyDeathrattle = true},//Unearthed Raptor
new BattleCryTargetAction {CardID = "EX1_362", GiveDivineShield = true, AllyOnly = true},//Argent Protector
new BattleCryTargetAction {CardID = "OG_282", SwapStatsOfTargetItself = true},//Crazed Alchemist
new BattleCryTargetAction {CardID = "EX1_603", HealthBonus = -1, AttackBonus = 2},//Cruel Taskmaster
new BattleCryTargetAction {CardID = "AT_084", AttackBonus = 2, AllyOnly = true},//Lance Carrier
new BattleCryTargetAction {CardID = "KAR_092", HealthBonus = -2, OnlyIfSectretExists = true},//Medivh's Valet
new BattleCryTargetAction {CardID = "GVG_011", AttackBonus = -2},//Shrinkmeister
new BattleCryTargetAction {CardID = "LOEA09_6", HealthBonus = -1, CanTargetHero = true},//Slithering Archer
new BattleCryTargetAction {CardID = "AT_069", GiveTaunt = true},//Sparring Partner
new BattleCryTargetAction {CardID = "EX1_049", ReturnTargetToHand = true, AllyOnly = true},//Youthful Brewmaster
new BattleCryTargetAction {CardID = "CS2_188", AttackBonus = 2},//Abusive Sergeant
new BattleCryTargetAction {CardID = "CS2_189", HealthBonus = -1, CanTargetHero = true},//Elven Archer
new BattleCryTargetAction {CardID = "NEW1_017", HealthBonus = -int.MaxValue, RaceOnly = TAG_RACE.MURLOC},//Hungry Crab
new BattleCryTargetAction {CardID = "EX1_011", HealthBonus = 2, CanTargetHero = true},//Voodoo Doctor
};

        public static bool HasBattleCryTargetAction(Card c)
        {
            foreach (var entry in BattleCryActionsCardDataBase)
            {
                if (entry.CardID == c.GetEntity().GetCardId())
                    return true;
            }
            return false;
        }

        public static bool CorrespondsToBattleCryAction(Card c, BattleCryTargetAction action)
        {
            return action.CardID == c.GetEntity().GetCardId();
        }

        public static BattleCryTargetAction GetBattleCryTargetAction(Card c)
        {
            foreach (var entry in BattleCryActionsCardDataBase)
            {
                if (entry.CardID == c.GetEntity().GetCardId())
                    return entry;
            }
            return null;
        }

        public static Card GetHeroCard(bool ally)
        {
            return Collector.HeroCards[ally ? 0 : 1];
        }

        public static Card GetWeapon(bool ally)
        {
            var cards = ally ? Collector.FriendlyWeaponCards : Collector.EnemyWeaponCards;
            if (cards.Count > 0)
                return cards[0];

            return null;
        }

        public static List<Card> GetPotentialBoardTargets(BattleCryTargetAction action)
        {
            List<Card> boardCards = new List<Card>();
            bool allyTarget = action.AllyOnly || action.TwoWay;
            if (allyTarget)
                foreach (var c in Collector.OwnBoardCards)
                    boardCards.Add(c);

            bool enemyTarget = action.EnemyOnly || action.TwoWay;
            if (enemyTarget)
                foreach (var c in Collector.EnemyBoardCards)
                    boardCards.Add(c);

            if (action.CanTargetHero)
            {
                //if (allyTarget)
                //    boardCards.Add(GetHeroCard(true));
                //if (enemyTarget)
                //    boardCards.Add(GetHeroCard(false));
            }

            if (action.CopyDeathrattle)
            {
                List<Card> _boardCards = new List<Card>();
                foreach (var card in boardCards)
                {
                    if (card.GetEntity().HasDeathrattle())
                        _boardCards.Add(card);
                }
                boardCards = _boardCards;
            }

            if (action.XAttackOrLess)
            {
                List<Card> _boardCards = new List<Card>();
                foreach (var card in boardCards)
                {
                    if (card.GetEntity().GetRealTimeAttack() <= action.MaxAttack)
                        _boardCards.Add(card);
                }
                boardCards = _boardCards;
            }

            if (action.XAttackOrMore)
            {
                List<Card> _boardCards = new List<Card>();
                foreach (var card in boardCards)
                {
                    if (card.GetEntity().GetRealTimeAttack() >= action.MaxAttack)
                        _boardCards.Add(card);
                }
                boardCards = _boardCards;
            }

            if (action.DestroyTaunt)
            {
                List<Card> _boardCards = new List<Card>();
                foreach (var card in boardCards)
                {
                    if (card.GetEntity().HasTaunt())
                        _boardCards.Add(card);
                }
                boardCards = _boardCards;
            }

            if (action.RaceOnly != TAG_RACE.INVALID)
            {
                List<Card> _boardCards = new List<Card>();
                foreach (var card in boardCards)
                {
                    if (card.GetEntity().GetRace() == action.RaceOnly)
                        _boardCards.Add(card);
                }
                boardCards = _boardCards;
            }

            if (action.OnlyIfSectretExists && Collector.SecretZoneCards.Count == 0)
                return null;

            int numDragonsHand = 0;
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().GetRace() == TAG_RACE.DRAGON)
                    numDragonsHand++;
            }

            if (action.OnlyIfHoldingDragon && numDragonsHand == 0)
                return null;

            //Cant trigger enemy stealth
            List<Card> __boardCards = new List<Card>();
            foreach (var card in boardCards)
            {
                bool cantTarget = card.GetEntity().IsStealthed() 
                    && !card.GetEntity().GetController().IsFriendlySide();
                if (!cantTarget)
                    __boardCards.Add(card);
            }
            boardCards = __boardCards;

            return boardCards;
        }

        public static bool IsInHand(Card c)
        {
            foreach (var card in Collector.HandCards)
            {
                if (card == c)
                    return true;
            }
            return false;
        }

        public static bool IsInHand(string CardId)
        {
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().GetCardId() == CardId)
                    return true;
            }
            return false;
        }

        public static Card GetHandCardById(string CardId)
        {
            foreach (var card in Collector.HandCards)
            {
                if (card.GetEntity().GetCardId() == CardId)
                    return card;
            }
            return null;
        }

        /// <summary>
        /// 0-10
        /// </summary>
        /// <returns></returns>
        public static float GetSilenceValue(VirtualCard vc)
        {
            return 1;
        }

        public static int GetOverload(VirtualCard vc)
        {
            return 1;
        }
    }
}
