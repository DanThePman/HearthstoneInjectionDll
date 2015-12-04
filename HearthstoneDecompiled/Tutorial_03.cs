using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tutorial_03 : TutorialEntity
{
    private bool defenselessVoPlayed;
    private bool enemyPlayedBigBrother;
    private bool monkeyLinePlayedOnce;
    private bool needATaunterVOPlayed;
    private int numTauntGorillasPlayed;
    private bool overrideMouseOver;
    private bool seenTheBrother;
    private Notification thatsABadPlayPopup;
    private bool victory;
    private bool warnedAgainstAttackingGorilla;

    private void DialogLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
    }

    public override List<RewardData> GetCustomRewards()
    {
        if (!this.victory)
        {
            return null;
        }
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("CS2_022", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    [DebuggerHidden]
    private IEnumerator GetReadyForBro()
    {
        return new <GetReadyForBro>c__Iterator19C { <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator19D { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator19B { turn = turn, <$>turn = turn, <>f__this = this };
    }

    public override bool IsKeywordHelpDelayOverridden()
    {
        return true;
    }

    public override bool IsMouseOverDelayOverriden()
    {
        return this.overrideMouseOver;
    }

    public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
    {
        if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
        {
            return false;
        }
        if ((wasInTargetMode && (clickedEntity.GetCardId() == "TU4c_007")) && !this.warnedAgainstAttackingGorilla)
        {
            this.warnedAgainstAttackingGorilla = true;
            base.HandleMissionEvent(11);
            return false;
        }
        return true;
    }

    public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
    {
        this.overrideMouseOver = false;
    }

    public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
    {
        if (mousedOverEntity.HasTaunt())
        {
            this.overrideMouseOver = true;
        }
    }

    public override void NotifyOfDefeatCoinAnimation()
    {
        if (this.victory)
        {
            base.PlaySound("VO_TUTORIAL_03_JAINA_20_36", 1f, true, false);
        }
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            this.victory = true;
            base.SetTutorialProgress(TutorialProgress.MUKLA_COMPLETE);
            base.PlaySound("VO_TUTORIAL_03_MUKLA_07_07", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL_03_MUKLA_07_07", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            base.SetTutorialLostProgress(TutorialProgress.MUKLA_COMPLETE);
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL_03_JAINA_17_33");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_18_34");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_01_24");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_05_25");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_07_26");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_12_28");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_13_29");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_16_32");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_14_30");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_15_31");
        base.PreloadSound("VO_TUTORIAL_03_JAINA_20_36");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_01_01");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_03_03");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_04_04");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_05_05");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_06_06");
        base.PreloadSound("VO_TUTORIAL_03_MUKLA_07_07");
    }

    public override bool ShouldShowCrazyKeywordTooltip()
    {
        return true;
    }

    private void ShowDontFireballYourselfPopup(Vector3 origin)
    {
        if (this.thatsABadPlayPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.thatsABadPlayPopup);
        }
        Vector3 position = new Vector3(origin.x - 3f, origin.y, origin.z);
        this.thatsABadPlayPopup = NotificationManager.Get().CreatePopupText(position, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_07"), true);
        NotificationManager.Get().DestroyNotification(this.thatsABadPlayPopup, 2.5f);
    }

    private void ShowDontPolymorphYourselfPopup(Vector3 origin)
    {
        if (this.thatsABadPlayPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.thatsABadPlayPopup);
        }
        Vector3 position = new Vector3(origin.x - 3f, origin.y, origin.z);
        this.thatsABadPlayPopup = NotificationManager.Get().CreatePopupText(position, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_07"), true);
        NotificationManager.Get().DestroyNotification(this.thatsABadPlayPopup, 2.5f);
    }

    [CompilerGenerated]
    private sealed class <GetReadyForBro>c__Iterator19C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_03 <>f__this;
        internal Actor <jainaActor>__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<jainaActor>__0 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    this.<>f__this.seenTheBrother = true;
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_12_28", "TUTORIAL03_JAINA_12", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                    this.$PC = 1;
                    goto Label_0102;

                case 1:
                    GameState.Get().SetBusy(false);
                    this.$current = new WaitForSeconds(3.2f);
                    this.$PC = 2;
                    goto Label_0102;

                case 2:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_13_29", "TUTORIAL03_JAINA_13", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0102:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator19D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_03 <>f__this;
        internal GameObject <actorObject>__7;
        internal Actor <enemyActor>__0;
        internal ZonePlay <enemyBattlefield>__5;
        internal Vector3 <gorillaPosition>__2;
        internal Vector3 <help04Position>__3;
        internal string <helpString>__8;
        internal Actor <jainaActor>__1;
        internal Vector3 <oldPosition>__6;
        internal Notification <tauntHelp>__4;
        internal int missionEvent;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            int missionEvent;
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    missionEvent = this.missionEvent;
                    switch (missionEvent)
                    {
                        case 1:
                            this.<>f__this.HandleGameStartEvent();
                            AssetLoader.Get().LoadActor("TutorialKeywordManager", false, false);
                            goto Label_05E1;

                        case 2:
                        case 8:
                        case 9:
                            goto Label_05E1;

                        case 4:
                            this.<>f__this.numTauntGorillasPlayed++;
                            if (this.<>f__this.numTauntGorillasPlayed != 1)
                            {
                                if ((this.<>f__this.numTauntGorillasPlayed == 2) && !this.<>f__this.DidLoseTutorial(TutorialProgress.MUKLA_COMPLETE))
                                {
                                    GameState.Get().SetBusy(true);
                                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_MUKLA_06_06", "TUTORIAL03_MUKLA_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                    this.$PC = 3;
                                    goto Label_05E3;
                                    this.$PC = -1;
                                }
                                goto Label_05E1;
                            }
                            this.$current = new WaitForSeconds(1f);
                            this.$PC = 1;
                            goto Label_05E3;

                        case 10:
                            this.<>f__this.enemyPlayedBigBrother = true;
                            this.<enemyBattlefield>__5 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone();
                            this.<enemyBattlefield>__5.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                            this.<oldPosition>__6 = this.<enemyBattlefield>__5.transform.position;
                            this.<enemyBattlefield>__5.transform.position = new Vector3(this.<oldPosition>__6.x + 2.393164f, this.<oldPosition>__6.y, this.<oldPosition>__6.z + 0.7f);
                            this.<enemyBattlefield>__5.transform.localEulerAngles = new Vector3(356.22f, 0f, 0f);
                            if (!GameState.Get().IsFriendlySidePlayerTurn())
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.GetReadyForBro());
                            }
                            goto Label_05E1;

                        case 11:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_16_32", "TUTORIAL03_JAINA_16", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            goto Label_05E1;

                        case 12:
                            if (!this.<>f__this.monkeyLinePlayedOnce)
                            {
                                this.<>f__this.monkeyLinePlayedOnce = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_14_30", "TUTORIAL03_JAINA_14", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            }
                            else if (!this.<>f__this.DidLoseTutorial(TutorialProgress.MUKLA_COMPLETE))
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_15_31", "TUTORIAL03_JAINA_15", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            }
                            goto Label_05E1;
                    }
                    break;

                case 1:
                    if (GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards().Count != 0)
                    {
                        this.<gorillaPosition>__2 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards()[0].transform.position;
                        this.<help04Position>__3 = new Vector3(this.<gorillaPosition>__2.x - 3f, this.<gorillaPosition>__2.y, this.<gorillaPosition>__2.z);
                        this.$current = new WaitForSeconds(2f);
                        this.$PC = 2;
                        goto Label_05E3;
                    }
                    goto Label_05E1;

                case 2:
                    this.<tauntHelp>__4 = NotificationManager.Get().CreatePopupText(this.<help04Position>__3, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL03_HELP_01"), true);
                    this.<tauntHelp>__4.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                    NotificationManager.Get().DestroyNotification(this.<tauntHelp>__4, 6f);
                    goto Label_05E1;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_05E1;

                case 4:
                    this.<actorObject>__7 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__7.GetComponent<Notification>();
                    this.<helpString>__8 = "TUTORIAL03_HELP_03";
                    if (UniversalInputManager.Get().IsTouchMode())
                    {
                        this.<helpString>__8 = "TUTORIAL03_HELP_03_TOUCH";
                    }
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL03_HELP_02"), GameStrings.Get(this.<helpString>__8), GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0.5f, 0.5f);
                    goto Label_05E1;

                case 5:
                    MulliganManager.Get().BeginMulligan();
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    goto Label_05E1;

                default:
                    goto Label_05E1;
            }
            switch (missionEvent)
            {
                case 0x36:
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 4;
                    goto Label_05E3;

                case 0x37:
                    this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_MUKLA_01_01", "TUTORIAL03_MUKLA_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 5;
                    goto Label_05E3;
            }
        Label_05E1:
            return false;
        Label_05E3:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator19B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal List<Card>.Enumerator <$s_713>__4;
        internal Tutorial_03 <>f__this;
        internal Actor <enemyActor>__0;
        internal Actor <jainaActor>__1;
        internal Card <minion>__5;
        internal List<Card> <myMinions>__3;
        internal bool <noTaunters>__2;
        internal int turn;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    if (!this.<>f__this.enemyPlayedBigBrother)
                    {
                        break;
                    }
                    if (!GameState.Get().IsFriendlySidePlayerTurn())
                    {
                        if (!this.<>f__this.seenTheBrother)
                        {
                            Gameplay.Get().StartCoroutine(this.<>f__this.GetReadyForBro());
                        }
                        break;
                    }
                    if (GameState.Get().GetOpposingSidePlayer().GetNumMinionsInPlay() <= 0)
                    {
                        break;
                    }
                    if (this.<>f__this.needATaunterVOPlayed)
                    {
                        if (!this.<>f__this.defenselessVoPlayed)
                        {
                            this.<noTaunters>__2 = true;
                            this.<myMinions>__3 = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards();
                            this.<$s_713>__4 = this.<myMinions>__3.GetEnumerator();
                            try
                            {
                                while (this.<$s_713>__4.MoveNext())
                                {
                                    this.<minion>__5 = this.<$s_713>__4.Current;
                                    if (this.<minion>__5.GetEntity().HasTaunt())
                                    {
                                        this.<noTaunters>__2 = false;
                                    }
                                }
                            }
                            finally
                            {
                                this.<$s_713>__4.Dispose();
                            }
                            if (this.<noTaunters>__2)
                            {
                                this.<>f__this.defenselessVoPlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_18_34", "TUTORIAL03_JAINA_18", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            }
                        }
                        break;
                    }
                    if (!GameState.Get().GetFriendlySidePlayer().HasATauntMinion())
                    {
                        this.<>f__this.needATaunterVOPlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_17_33", "TUTORIAL03_JAINA_17", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    }
                    goto Label_0438;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_MUKLA_03_03", "TUTORIAL03_MUKLA_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    goto Label_0438;

                case 2:
                    GameState.Get().SetBusy(false);
                    goto Label_0438;

                default:
                    goto Label_0438;
            }
            switch (this.turn)
            {
                case 1:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.MUKLA_COMPLETE))
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_01_24", "TUTORIAL03_JAINA_01", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                        break;
                    }
                    break;

                case 2:
                case 3:
                case 4:
                case 7:
                case 8:
                case 10:
                case 11:
                case 12:
                case 13:
                    break;

                case 5:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.MUKLA_COMPLETE))
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_05_25", "TUTORIAL03_JAINA_05", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                        this.$PC = 1;
                        goto Label_043A;
                    }
                    break;

                case 6:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.MUKLA_COMPLETE))
                    {
                        GameState.Get().SetBusy(true);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_MUKLA_04_04", "TUTORIAL03_MUKLA_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                        this.$PC = 2;
                        goto Label_043A;
                    }
                    break;

                case 9:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_JAINA_07_26", "TUTORIAL03_JAINA_07", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    break;

                case 14:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_03_MUKLA_05_05", "TUTORIAL03_MUKLA_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
        Label_0438:
            return false;
        Label_043A:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

