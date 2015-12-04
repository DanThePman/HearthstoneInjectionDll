using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tutorial_05 : TutorialEntity
{
    private bool heroPowerHasNotBeenUsed = true;
    private int numTimesRemindedAboutGoal;
    private bool victory;
    private int weaponsPlayed;

    public override List<RewardData> GetCustomRewards()
    {
        if (!this.victory)
        {
            return null;
        }
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("EX1_277", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator1A4 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator1A3 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    public override bool IsKeywordHelpDelayOverridden()
    {
        return true;
    }

    public override bool NotifyOfEndTurnButtonPushed()
    {
        NotificationManager.Get().DestroyAllArrows();
        return true;
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            this.victory = true;
            base.SetTutorialProgress(TutorialProgress.ILLIDAN_COMPLETE);
            FixedRewardsMgr.Get().CheckForTutorialComplete();
            if (Network.ShouldBeConnectedToAurora())
            {
                BnetPresenceMgr.Get().SetGameField(15, 1);
            }
            base.ResetTutorialLostProgress();
            base.PlaySound("VO_TUTORIAL_05_ILLIDAN_12_12", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL_05_ILLIDAN_12_12", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            base.SetTutorialLostProgress(TutorialProgress.ILLIDAN_COMPLETE);
        }
    }

    public override string[] NotifyOfKeywordHelpPanelDisplay(Entity entity)
    {
        if (!(entity.GetCardId() == "TU4e_004") && !(entity.GetCardId() == "TU4e_007"))
        {
            return null;
        }
        return new string[] { GameStrings.Get("TUTORIAL05_WEAPON_HEADLINE"), GameStrings.Get("TUTORIAL05_WEAPON_DESC") };
    }

    public override bool NotifyOfTooltipDisplay(TooltipZone specificZone)
    {
        return false;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_12_12");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_03_39");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_11_11");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_02_03");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_04_05");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_08_08");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_03_04");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_05_06");
        base.PreloadSound("VO_TUTORIAL_05_JAINA_02_46");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_06_07");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_09_09");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_10_10");
        base.PreloadSound("VO_TUTORIAL_05_JAINA_05_47");
        base.PreloadSound("VO_TUTORIAL_05_JAINA_06_48");
        base.PreloadSound("VO_TUTORIAL_05_ILLIDAN_01_02");
        base.PreloadSound("VO_TUTORIAL_05_JAINA_01_45");
        base.PreloadSound("VO_INNKEEPER_TUT_COMPLETE_05");
    }

    private void ShowEndTurnBouncingArrow()
    {
        if (!EndTurnButton.Get().IsInWaitingState())
        {
            Vector3 position = EndTurnButton.Get().transform.position;
            Vector3 vector2 = new Vector3(position.x - 2f, position.y, position.z);
            NotificationManager.Get().CreateBouncingArrow(vector2, new Vector3(0f, -90f, 0f));
        }
    }

    [DebuggerHidden]
    private IEnumerator Wait(float seconds)
    {
        return new <Wait>c__Iterator1A5 { seconds = seconds, <$>seconds = seconds };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator1A4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_05 <>f__this;
        internal GameObject <actorObject>__5;
        internal string <bodyText>__6;
        internal Actor <enemyActor>__0;
        internal Actor <jainaActor>__1;
        internal Vector3 <popUpPos>__3;
        internal Notification <weaponHelp>__4;
        internal Vector3 <weaponPosition>__2;
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
                            goto Label_089F;

                        case 2:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                            {
                                GameState.Get().SetBusy(true);
                                this.<>f__this.weaponsPlayed++;
                                if (this.<>f__this.weaponsPlayed == 1)
                                {
                                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_02_03", "TUTORIAL05_ILLIDAN_02", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                                    this.$PC = 1;
                                }
                                else if (this.<>f__this.weaponsPlayed == 2)
                                {
                                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_04_05", "TUTORIAL05_ILLIDAN_04", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                                    this.$PC = 2;
                                }
                                else
                                {
                                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_08_08", "TUTORIAL05_ILLIDAN_08", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                                    this.$PC = 3;
                                }
                                goto Label_08A1;
                            }
                            goto Label_089F;

                        case 3:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_03_04", "TUTORIAL05_ILLIDAN_03", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                            }
                            goto Label_089F;

                        case 4:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                            {
                                GameState.Get().SetBusy(true);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_05_06", "TUTORIAL05_ILLIDAN_05", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.$PC = 4;
                                goto Label_08A1;
                            }
                            goto Label_089F;

                        case 5:
                            if (!this.<>f__this.heroPowerHasNotBeenUsed)
                            {
                                goto Label_089F;
                            }
                            this.<>f__this.heroPowerHasNotBeenUsed = false;
                            GameState.Get().SetBusy(true);
                            this.$current = new WaitForSeconds(2f);
                            this.$PC = 5;
                            goto Label_08A1;

                        case 6:
                        case 7:
                            goto Label_089F;

                        case 8:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_09_09", "TUTORIAL05_ILLIDAN_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            }
                            goto Label_089F;

                        case 9:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                            {
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_10_10", "TUTORIAL05_ILLIDAN_10", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.$PC = 8;
                                goto Label_08A1;
                            }
                            goto Label_089F;

                        case 10:
                            if (this.<>f__this.numTimesRemindedAboutGoal != 0)
                            {
                                if (this.<>f__this.numTimesRemindedAboutGoal == 1)
                                {
                                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_JAINA_06_48", "TUTORIAL05_JAINA_06", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                                    this.$PC = 10;
                                    goto Label_08A1;
                                }
                                goto Label_0545;
                            }
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_JAINA_05_47", "TUTORIAL05_JAINA_05", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                            this.$PC = 9;
                            goto Label_08A1;

                        case 12:
                            GameState.Get().SetBusy(true);
                            this.<weaponPosition>__2 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().transform.position;
                            this.<popUpPos>__3 = new Vector3(this.<weaponPosition>__2.x - 1.55f, this.<weaponPosition>__2.y, this.<weaponPosition>__2.z - 2.721f);
                            this.<weaponHelp>__4 = NotificationManager.Get().CreatePopupText(this.<popUpPos>__3, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL05_HELP_01"), true);
                            this.<weaponHelp>__4.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
                            NotificationManager.Get().DestroyNotification(this.<weaponHelp>__4, 5f);
                            this.$current = new WaitForSeconds(5.5f);
                            this.$PC = 11;
                            goto Label_08A1;
                    }
                    break;

                case 1:
                case 2:
                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_089F;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_089F;

                case 5:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_JAINA_02_46", "TUTORIAL05_JAINA_02", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 6;
                    goto Label_08A1;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_06_07", "TUTORIAL05_ILLIDAN_06", Notification.SpeechBubbleDirection.TopLeft, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_08A1;

                case 7:
                    GameState.Get().SetBusy(false);
                    goto Label_089F;

                case 9:
                case 10:
                    goto Label_0545;

                case 11:
                    GameState.Get().SetBusy(false);
                    goto Label_089F;

                case 12:
                    this.<actorObject>__5 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__5.GetComponent<Notification>();
                    this.<bodyText>__6 = this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE) ? GameStrings.Get("TUTORIAL05_HELP_04") : GameStrings.Get("TUTORIAL05_HELP_03");
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL05_HELP_02"), this.<bodyText>__6, GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material = this.<>f__this.startingPopup.swapMaterial;
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0.5f, 0f);
                    goto Label_089F;

                case 13:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 14;
                    goto Label_08A1;

                case 14:
                    this.<>f__this.FadeInHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_JAINA_01_45", "TUTORIAL05_JAINA_01", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 15;
                    goto Label_08A1;

                case 15:
                    goto Label_0844;

                case 0x10:
                    this.<>f__this.FadeOutHeroActor(this.<jainaActor>__1);
                    goto Label_089F;

                default:
                    goto Label_089F;
            }
            switch (missionEvent)
            {
                case 0x36:
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 12;
                    goto Label_08A1;

                case 0x37:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                    {
                        this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_01_02", "TUTORIAL05_ILLIDAN_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                        this.$PC = 13;
                        goto Label_08A1;
                    }
                    goto Label_0844;

                default:
                    goto Label_089F;
            }
        Label_0545:
            this.<>f__this.numTimesRemindedAboutGoal++;
            goto Label_089F;
        Label_0844:
            MulliganManager.Get().BeginMulligan();
            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
            {
                this.$current = new WaitForSeconds(2.3f);
                this.$PC = 0x10;
                goto Label_08A1;
                this.$PC = -1;
            }
        Label_089F:
            return false;
        Label_08A1:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator1A3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal Tutorial_05 <>f__this;
        internal Actor <enemyActor>__0;
        internal Actor <jainaActor>__1;
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
                    if (GameState.Get().GetOpposingSidePlayer().HasWeapon())
                    {
                        GameState.Get().GetOpposingSidePlayer().GetWeaponCard().GetActorSpell(SpellType.DEATH).m_BlockServerEvents = true;
                    }
                    if (this.turn != 2)
                    {
                        goto Label_017D;
                    }
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_03_39", "TUTORIAL04_JAINA_03", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 1;
                    goto Label_017F;

                case 1:
                    if (this.<>f__this.DidLoseTutorial(TutorialProgress.ILLIDAN_COMPLETE))
                    {
                        break;
                    }
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_05_ILLIDAN_11_11", "TUTORIAL05_ILLIDAN_11", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 2;
                    goto Label_017F;

                case 2:
                    break;

                default:
                    goto Label_017D;
            }
            if ((this.<>f__this.GetTag(GAME_TAG.TURN) == 2) && EndTurnButton.Get().IsInNMPState())
            {
                this.<>f__this.ShowEndTurnBouncingArrow();
                goto Label_017D;
                this.$PC = -1;
            }
        Label_017D:
            return false;
        Label_017F:
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
    private sealed class <Wait>c__Iterator1A5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal float seconds;

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
                    this.$current = new WaitForSeconds(this.seconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.$PC = -1;
                    break;
            }
            return false;
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

