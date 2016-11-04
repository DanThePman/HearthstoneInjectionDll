using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using HearthstoneInjectionDll.Behaviour;

namespace HearthstoneInjectionDll
{
    [AttributeUsage(AttributeTargets.All)]
    class ExternCall : Attribute { }

    /// <summary>
    /// Certain commands CAUSES BUGS (board fly out)
    /// BoardCards cant be "addranged" otherwise it bugs
    /// </summary>
    public static class Core
    {
        public static void OnGameInit()
        {
            new Thread(new ThreadStart(delegate
            {
                Actions.EnableCustomMousePosition();
                Actions.EnableCustomMouseButton();

                while (SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN) { }

                while (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
                {
                    Actions.SetMousePosition(Actions.GetScreenMidLeft());
                    Thread.Sleep(100);
                    Actions.MouseClick();
                    Thread.Sleep(2000);
                }


            })).Start();
        }
        public static void OnGameStart()
        {
            new Thread(new ThreadStart(delegate
            {
                IsMulliganOver = false;

                while (true)
                {
                    try
                    {
                        OnUpdate();
                    }
                    catch (Exception ex)
                    {
                        Debug.AppendDesktopLog("CoreUpdateError", ex.Message + "\n" + ex.StackTrace, false);
                        Debug.ShowIngameMessage("CoreUpdateError");
                    }
                }
            })).Start();
        }

        private static bool IsPrivatelyBusy, IsPubliclyBusy, IsMulliganOver;

        /// <summary>
        /// Collects data | called of assembly csharp
        /// </summary>
        public static void OnPublicUpdate()
        {
            if (IsPrivatelyBusy)
                return;

            IsPubliclyBusy = true;
            var gs = GameState.Get();
            Lib.Collector.HandCards = gs.GetFriendlySidePlayer().GetHandZone().GetCards();
            Lib.Collector.HeroCards = new List<Card> { gs.GetFriendlySidePlayer().GetHeroCard(), gs.GetOpposingSidePlayer().GetHeroCard() };

            Lib.Collector.FriendlyWeaponCards = new List<Card> { gs.GetFriendlySidePlayer().GetWeaponCard() };
            Lib.Collector.EnemyWeaponCards = new List<Card> { gs.GetOpposingSidePlayer().GetWeaponCard() };

            Lib.Collector.AllWeaponCards = Lib.Collector.FriendlyWeaponCards;
            foreach (var enemyWeaponCard in Lib.Collector.EnemyWeaponCards)
            {
                Lib.Collector.AllWeaponCards.Add(enemyWeaponCard);
            }

            Lib.Collector.OwnBoardCards = gs.GetFriendlySidePlayer().GetBattlefieldZone().GetCards();
            Lib.Collector.EnemyBoardCards = gs.GetOpposingSidePlayer().GetBattlefieldZone().GetCards();

            Lib.Collector.ManaMgr = ManaCrystalMgr.Get();
            Lib.Collector.InputManager = InputManager.Get();
            Lib.Collector.EndTurnButton = EndTurnButton.Get();
            Lib.Collector.GameState = gs;
            IsPubliclyBusy = false;
        }

        /// <summary>
        /// Gets called in a loop via thread
        /// </summary>
        public static void OnUpdate()
        {
            var gameState = Lib.Collector.GameState;
            if (gameState == null)
                return;
            if (gameState.GetPowerProcessor().IsBuildingTaskList())
                return;
            if (IsPubliclyBusy)
                return;

            if (File.Exists(@"C:\Users\Daniel\Desktop\Neues Textdokument.txt"))
            {
                Actions.DisableCustomMousePosition();
                Actions.DisableCustomMouseButton();
                return;
            }
            Actions.EnableCustomMousePosition();
            Actions.EnableCustomMouseButton();

            CheckTurn();

            if (!gameState.IsFriendlySidePlayerTurn() && EndTurn)
                EndTurn = false;/*reset EndTurn-bool at enemy turn*/
            if (Lib.Collector.GameState.IsGameOverNowOrPending())
            {
                Actions.DisableCustomMousePosition();
                Actions.DisableCustomMouseButton();
            }

            /*hover*/
            if (IsMulliganOver)
                DoHovering();

            if (!IsMulliganOver)
                if (gameState.IsMulliganPhaseNowOrPending())
                    CheckMulligan();

            try
            {
                if (Lib.Collector.GameState.IsFriendlySidePlayerTurn())
                {
                    DoPlays();
                }

                if (Lib.Collector.EndTurnButton.HasNoMorePlays())
                {
                    Actions.EndTurn();
                }

            }
            catch (Exception ex)
            {
                Debug.AppendDesktopLog("PlayErrorA", ex.Message + "\n" + ex.StackTrace, false);
            }
        }

        private static int LastTurn;
        private static void CheckTurn()
        {
            var currentTurn = Lib.Collector.GameState.GetTurn();
            if (currentTurn != LastTurn)
            {
                LastTurn = currentTurn;
                OnTurnChangd();
            }
        }

        private static int TurnChangedTick;
        private static void OnTurnChangd()
        {
            Lib.WasHeroPowerUsed = false;
            TurnChangedTick = Environment.TickCount;
        }

        private static void CheckMulligan()
        {
            if (IsPubliclyBusy)
                return;

            var gameState = Lib.Collector.GameState;

            Thread.Sleep(17500);

            var allCards = gameState.GetFriendlySidePlayer().GetHandZone().GetCards();
            List<Card> selectedCards = Mulligan.EntryPoint.OnQueryMulligan();

            foreach (var card in allCards)
            {
                if (card.GetEntity().GetCardId() == "GAME_005")
                    continue;

                if (!selectedCards.Contains(card))
                {
                    Actions.SetMousePosition(card);
                    Thread.Sleep(10);
                    Actions.MouseClick();
                }

                Thread.Sleep(500);
            }

            Thread.Sleep(1000);
            Actions.HadToHandleMulligan = true;
            Actions.EndMulligan();
            IsMulliganOver = true;
            Thread.Sleep(9000);
        }

        private static bool EndTurn;
        private static void DoPlays()
        {
            if (IsPubliclyBusy)
                return;
            if (Environment.TickCount - TurnChangedTick <= 5000) return;
            IsPrivatelyBusy = true;

            if (Lib.Collector.GameState.IsInChoiceMode())
            {
                CheckChoiceMode();
            }
            if (Lib.Collector.GameState.IsInTargetMode())
            {
                CheckUnexpectedTargetingMode();
            }

            Lib.BotPlay play = Behaviour2.OnQueryBotPlay();
            if (play != null)
            {
                HandleBotPlay(play);
            }

            IsPrivatelyBusy = false;
        }

        private static void HandleBotPlay(Lib.BotPlay play)
        {
            if (play.IsSummonOnly)//play
                Actions.PlayCard(play.Source);
            else if (play.IsTargeted && Lib.IsInHand(play.Source))//play with taget
                Actions.PlayCard(play.Source, play.Target, play.PlayBeforeTargeting);
            else if (play.IsTargeted && !Lib.IsInHand(play.Source))//attack
                Actions.Attack(play.Source, play.Target);
            else if (play.IsTargetOnlyAction) //target action after battlecrys
            {
                Actions.TargetOnly(play.Target);
                Debug.AppendDesktopLog("tagetOnly", "of " + play.Source.GetEntity().GetName());
            }
            else if (play.IsUseHeroPowerPlay)
                Actions.UseHeroPower();
        }

        private static void CheckChoiceMode()
        {
            var choice = Behaviour2.OnQueryCardChoice();
            Actions.DoChoice(choice.GetEntity());
        }

        private static void CheckUnexpectedTargetingMode()
        {
            var target = Behaviour2.OnQueryUnexpectedTarget();
            Actions.TargetOnly(target);
            Thread.Sleep(Actions.GetDelay);
        }

        static int GetHoverFrequency { get { return Actions.RandomNumber(2000, 4000); } }
        private static int LastMouseHoverTick;

        private static void DoHovering()
        {
            try
            {
                if (IsPubliclyBusy)
                    return;

                if (Environment.TickCount - LastMouseHoverTick <= GetHoverFrequency) return;
                LastMouseHoverTick = Environment.TickCount;

                if (Environment.TickCount - TurnChangedTick <= 3000) return;

                var player = Lib.Collector.GameState.GetFriendlySidePlayer();

                var handCards = player.GetHandZone().GetCards();

                Actions.HoverCard(handCards[new Random().Next(0, handCards.Count - 1)]);
                Thread.Sleep(Actions.GetHoverDelay);
                Actions.UnHoverLastCard();
            }
            catch { }
        }
    }
}
