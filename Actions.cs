using System;
using System.Threading;
using UnityEngine;

namespace HearthstoneInjectionDll
{
    public static class Actions
    {
        public static int GetDelay
        {
            get { return RandomNumber(500, 1500); }
        }

        public static int GetHoverDelay
        {
            get { return RandomNumber(500, 2500); }
        }

        public static int RandomNumber(int min, int max)
        {
            try
            {
                return min + Environment.TickCount % (max + 1);
            }
            catch
            {
                return min;
            }
        }

        public static bool HadToHandleMulligan { get; set; }

        public static void EndTurn()
        {
            Lib.Collector.InputManager.DoEndTurnButton();
            Thread.Sleep(2000);
        }

        public static bool IsPlayingCard { get; set; }
        public static Card LastHoveredCard { get; set; }

        public static bool Hovering => LastHoveredCard != null;

        public static void HoverCard(Card c, bool playingFromHand = false)
        {
            if (c.IsAttacking() ||c.IsActorLoading() || c.IsTransitioningZones() || !c.IsActorReady())
                return;

            LastHoveredCard = c;
            SetMousePosition(c.transform.position);
            Thread.Sleep(10);
        }

        public static void UnHoverLastCard()
        {
            SetMousePosition(EndTurnButton.Get().transform.position);
            Thread.Sleep(10);
            LastHoveredCard = null;
        }

        public static void UseHeroPower()
        {
            SetMousePosition(Lib.GetHeroCard(true).GetHeroPower().GetCard().transform.position);
            MouseClick();
            Lib.WasHeroPowerUsed = true;
        }

        public static void PlayCard(Card c, Card target = null, bool targetAfterPlay = false)
        {
            while (c.GetEntity().IsBusy()) { }
            if (target != null)
                while (target.GetEntity().IsBusy()) { }

            if (c.GetEntity().GetRealTimeCost() > Lib.Collector.ManaMgr.GetSpendableManaCrystals() && target == null)
            {
                Debug.AppendDesktopLog("PlayErrorMana", "Not enough mana for " + c.GetEntity().GetName(), false);
                return;
            }

            if (!targetAfterPlay)
            {
                SetMousePosition(c);
                Thread.Sleep(500);
                MouseClick();
                Thread.Sleep(100);
                // ReSharper disable once MergeConditionalExpression
                SimulateMouseMovement(c.transform.position,
                    target == null ? GetBoardPos() : target.transform.position);
                Thread.Sleep(500);
                MouseClick();
            }
            else //playFirst
            {
                SetMousePosition(c);
                Thread.Sleep(100);
                MouseClick();
                Thread.Sleep(100);
                SimulateMouseMovement(c.transform.position, GetBoardPos());
                Thread.Sleep(100);
                MouseClick();

                Thread.Sleep(2000);

                SetMousePosition(c.transform.position);
                Thread.Sleep(100);
                SimulateMouseMovement(c.transform.position, target.transform.position);
                Thread.Sleep(100);
                MouseClick();
            }

            Thread.Sleep(4000);
        }

        public static void WeaponAttack(Card attackee)
        {
            while (attackee.GetEntity().IsBusy()) { }

            Card pCard = null;
            foreach (var zone in ZoneMgr.Get().GetZones())
            {
                if (!(zone is ZoneHero))
                    continue;

                var z = (ZoneHero)zone;
                if (!z.GetController().Equals(GameState.Get().GetFriendlySidePlayer()))
                    continue;

                foreach (Card ally in zone.GetCards())
                {
                    if (ally.GetEntity().IsCharacter())
                    {
                        pCard = ally.GetEntity().GetCard();
                        break;
                    }
                }
            }

            Attack(pCard, attackee);
        }

        public static void Attack(Card attacker, Card attackee)
        {
            //while (attacker.GetEntity().IsBusy()) { }
            //while (attackee.GetEntity().IsBusy()) { }

            int maxAttacks = attacker.GetEntity().HasWindfury() ? 2 : 1;
            if (attacker.GetEntity().GetNumAttacksThisTurn() >= maxAttacks || !attacker.GetEntity().CanAttack() ||
                attacker.GetEntity().IsAsleep())
            {
                Debug.AppendDesktopLog("AttackError", "Card " + attacker.GetEntity().GetName() + " cannot attack (anymore?)");
                return;
            }

            PlayCard(attacker, attackee);
        }

        public static void TargetOnly(Card target)
        {
            while (target.GetEntity().IsBusy()) { }

            SimulateMouseMovement(Camera.main.ScreenToWorldPoint(_MousePosition), target.transform.position);
            Thread.Sleep(50);
            MouseClick();
        }

        public static void EndMulligan()
        {
            MulliganManager.Get().GetMulliganButton().TriggerRelease();
        }

        public static Card GetEnemyHero()
        {
            Card pCard = null;
            foreach (var zone in ZoneMgr.Get().GetZones())
            {
                if (!(zone is ZoneHero))
                    continue;

                var z = (ZoneHero)zone;
                if (z.GetController().Equals(GameState.Get().GetFriendlySidePlayer()))
                    continue;

                foreach (Card ally in zone.GetCards())
                {
                    if (ally.GetEntity().IsCharacter())
                    {
                        pCard = ally.GetEntity().GetCard();
                        break;
                    }
                }
            }
            return pCard;
        }


        public static void DoChoice(Entity e)
        {
            SetMousePosition(e.GetCard());
            Thread.Sleep(50);
            MouseClick();
        }

        private static Vector3 _MousePosition = Vector3.zero;
        private static bool UseDllMousePosition = true;

        public static void SetMousePosition(Vector3 v, bool WorldToScreen = true)
        {
            _MousePosition = WorldToScreen ? Camera.main.WorldToScreenPoint(v) : v;
            UseDllMousePosition = true;
        }

        public static void SetMousePosition(Card c)
        {
            _MousePosition = Camera.main.WorldToScreenPoint(c.transform.position);
            UseDllMousePosition = true;
        }

        public static void DisableCustomMousePosition() => UseDllMousePosition = false;
        public static void EnableCustomMousePosition() => UseDllMousePosition = true;

        [ExternCall]
        public static Vector3 GetMousePosition()
        {
            return UseDllMousePosition ? _MousePosition : Input.mousePosition;
        }

        private static bool UseDllMouseButton, IsLeftMouseDown, IsLeftMouseUp;
        public static void DisableCustomMouseButton() => UseDllMouseButton = false;
        public static void EnableCustomMouseButton() => UseDllMouseButton = true;

        [ExternCall]
        public static bool IsLeftMouseButtonDown()
        {
            return UseDllMouseButton ? IsLeftMouseDown : Input.GetMouseButtonDown(0);
        }

        [ExternCall]
        public static bool IsLeftMouseButtonUp()
        {
            return UseDllMouseButton ? IsLeftMouseUp : Input.GetMouseButtonUp(0);
        }

        private static Vector3 GetBoardPos()
        {
            return EndTurnButton.Get().transform.position;
        }

        public static Vector3 GetScreenMid()
        {
            var cam = Camera.main;
            return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        }

        public static Vector3 GetScreenMidLeft()
        {
            var cam = Camera.main;
            return cam.ViewportToWorldPoint(new Vector3(0.8f, 0.5f, cam.nearClipPlane));
        }

        private static void SimulateMouseMovement(Vector3 _world_Start, Vector3 _world_End)
        {
            int delay = RandomNumber(10, 30);
            for (float i = 0; i <= 1; i += 0.1f)
            {
                Vector3 dest = _world_Start + (_world_End - _world_Start) * i;
                SetMousePosition(dest);
                Thread.Sleep(delay);
            }
        }

        public static void MouseClick(int extraDelay = 0)
        {
            IsLeftMouseDown = true;
            Thread.Sleep(32 + extraDelay);
            IsLeftMouseDown = false;
            Thread.Sleep(46);
            IsLeftMouseUp = true;
            Thread.Sleep(32);
            IsLeftMouseUp = false;
        }

        //public static void WaitFor(Func<bool> condition)
        //{
        //    while (!condition.Invoke()) { }
        //}
    }
}
