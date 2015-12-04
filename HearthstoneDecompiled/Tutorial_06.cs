using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tutorial_06 : TutorialEntity
{
    private Notification endTurnNotifier;
    private bool m_choSpeaking;
    private bool victory;

    protected override Spell BlowUpHero(Card card, SpellType spellType)
    {
        if (card.GetEntity().GetCardId() != "TU4f_001")
        {
            return base.BlowUpHero(card, spellType);
        }
        Spell spell = card.ActivateActorSpell(SpellType.CHODEATH);
        Gameplay.Get().StartCoroutine(base.HideOtherElements(card));
        return spell;
    }

    public override float GetAdditionalTimeToWaitForSpells()
    {
        return 1.5f;
    }

    public override List<RewardData> GetCustomRewards()
    {
        if (!this.victory)
        {
            return null;
        }
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("CS2_124", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator1A7 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator1A6 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    public override bool IsKeywordHelpDelayOverridden()
    {
        return true;
    }

    public override void NotifyOfDefeatCoinAnimation()
    {
        if (this.victory)
        {
            base.PlaySound("VO_TUTORIAL_06_JAINA_05_53", 1f, true, false);
        }
    }

    public override bool NotifyOfEndTurnButtonPushed()
    {
        Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
        if (((optionsPacket != null) && (optionsPacket.List != null)) && (optionsPacket.List.Count != 1))
        {
            for (int i = 0; i < optionsPacket.List.Count; i++)
            {
                Network.Options.Option option = optionsPacket.List[i];
                if ((option.Type == Network.Options.Option.OptionType.POWER) && (GameState.Get().GetEntity(option.Main.ID).GetZone() == TAG_ZONE.PLAY))
                {
                    if (this.endTurnNotifier != null)
                    {
                        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.endTurnNotifier);
                    }
                    Vector3 position = EndTurnButton.Get().transform.position;
                    Vector3 vector2 = new Vector3(position.x - 3f, position.y, position.z);
                    this.endTurnNotifier = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL_NO_ENDTURN_ATK"), true);
                    NotificationManager.Get().DestroyNotification(this.endTurnNotifier, 2.5f);
                    return false;
                }
            }
        }
        return true;
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            this.victory = true;
            base.SetTutorialProgress(TutorialProgress.CHO_COMPLETE);
            base.PlaySound("VO_TUTORIAL_06_CHO_22_19", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL_06_CHO_22_19", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            base.SetTutorialLostProgress(TutorialProgress.CHO_COMPLETE);
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL_06_CHO_15_15");
        base.PreloadSound("VO_TUTORIAL_06_CHO_09_13");
        base.PreloadSound("VO_TUTORIAL_06_CHO_17_16");
        base.PreloadSound("VO_TUTORIAL_06_CHO_05_09");
        base.PreloadSound("VO_TUTORIAL_06_JAINA_03_51");
        base.PreloadSound("VO_TUTORIAL_06_CHO_06_10");
        base.PreloadSound("VO_TUTORIAL_06_CHO_21_18");
        base.PreloadSound("VO_TUTORIAL_06_CHO_20_17");
        base.PreloadSound("VO_TUTORIAL_06_CHO_07_11");
        base.PreloadSound("VO_TUTORIAL_06_JAINA_04_52");
        base.PreloadSound("VO_TUTORIAL_06_CHO_04_08");
        base.PreloadSound("VO_TUTORIAL_06_CHO_12_14");
        base.PreloadSound("VO_TUTORIAL_06_CHO_01_05");
        base.PreloadSound("VO_TUTORIAL_06_JAINA_01_49");
        base.PreloadSound("VO_TUTORIAL_06_CHO_02_06");
        base.PreloadSound("VO_TUTORIAL_06_JAINA_02_50");
        base.PreloadSound("VO_TUTORIAL_06_CHO_03_07");
        base.PreloadSound("VO_TUTORIAL_06_CHO_22_19");
        base.PreloadSound("VO_TUTORIAL_06_JAINA_05_53");
    }

    [DebuggerHidden]
    private IEnumerator Wait(float seconds)
    {
        return new <Wait>c__Iterator1A8 { seconds = seconds, <$>seconds = seconds };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator1A7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_06 <>f__this;
        internal GameObject <actorObject>__10;
        internal Notification <battlecryNotification>__9;
        internal string <bodyText>__11;
        internal List<Card> <cardsInEnemyField>__6;
        internal Actor <enemyActor>__0;
        internal Spell <enemyAttackSpell>__3;
        internal Spell <enemyAttackSpell2>__5;
        internal Card <enemyHero>__2;
        internal Card <enemyHero2>__4;
        internal Actor <jainaActor>__1;
        internal Card <voodooDoctor>__7;
        internal Vector3 <voodooDoctorLocation>__8;
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
                            goto Label_07B5;

                        case 2:
                            GameState.Get().SetBusy(true);
                            goto Label_0112;

                        case 6:
                            GameState.Get().SetBusy(true);
                            this.<enemyHero>__2 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard();
                            this.<enemyAttackSpell>__3 = this.<enemyHero>__2.GetActorSpell(SpellType.CHOFLOAT);
                            this.<enemyAttackSpell>__3.ActivateState(SpellStateType.BIRTH);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_07_11", "TUTORIAL06_CHO_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 3;
                            goto Label_07B7;

                        case 8:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_04_08", "TUTORIAL06_CHO_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            goto Label_07B5;

                        case 9:
                            this.<enemyHero2>__4 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard();
                            this.<enemyAttackSpell2>__5 = this.<enemyHero2>__4.GetActorSpell(SpellType.CHOFLOAT);
                            this.<enemyAttackSpell2>__5.ActivateState(SpellStateType.CANCEL);
                            this.<>f__this.m_choSpeaking = true;
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_12_14", "TUTORIAL06_CHO_12", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 4;
                            goto Label_07B7;

                        case 10:
                            this.<cardsInEnemyField>__6 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards();
                            if (this.<cardsInEnemyField>__6.Count != 0)
                            {
                                GameState.Get().SetBusy(true);
                                this.<voodooDoctor>__7 = this.<cardsInEnemyField>__6[this.<cardsInEnemyField>__6.Count - 1];
                                this.<voodooDoctorLocation>__8 = this.<voodooDoctor>__7.transform.position;
                                this.<battlecryNotification>__9 = NotificationManager.Get().CreatePopupText(new Vector3(this.<voodooDoctorLocation>__8.x + 3f, this.<voodooDoctorLocation>__8.y, this.<voodooDoctorLocation>__8.z), TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL06_HELP_03"), true);
                                this.<battlecryNotification>__9.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                                NotificationManager.Get().DestroyNotification(this.<battlecryNotification>__9, 5f);
                                this.$current = new WaitForSeconds(5f);
                                this.$PC = 5;
                                goto Label_07B7;
                            }
                            goto Label_07B5;
                    }
                    break;

                case 1:
                    goto Label_0112;

                case 2:
                    GameState.Get().SetBusy(false);
                    goto Label_07B5;

                case 3:
                    GameState.Get().SetBusy(false);
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_JAINA_04_52", "TUTORIAL06_JAINA_04", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    goto Label_07B5;

                case 4:
                    this.<>f__this.m_choSpeaking = false;
                    goto Label_07B5;

                case 5:
                    GameState.Get().SetBusy(false);
                    goto Label_07B5;

                case 6:
                    this.<actorObject>__10 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__10.GetComponent<Notification>();
                    this.<bodyText>__11 = this.<>f__this.DidLoseTutorial(TutorialProgress.CHO_COMPLETE) ? GameStrings.Get("TUTORIAL06_HELP_04") : GameStrings.Get("TUTORIAL06_HELP_02");
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL06_HELP_01"), this.<bodyText>__11, GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0f, 0.5f);
                    goto Label_07B5;

                case 7:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(0.5f));
                    this.$PC = 8;
                    goto Label_07B7;

                case 8:
                    this.<>f__this.FadeInHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_JAINA_01_49", "TUTORIAL06_JAINA_01", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 9;
                    goto Label_07B7;

                case 9:
                    this.<>f__this.FadeOutHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(0.5f));
                    this.$PC = 10;
                    goto Label_07B7;

                case 10:
                    this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_02_06", "TUTORIAL06_CHO_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 11;
                    goto Label_07B7;

                case 11:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(0.25f));
                    this.$PC = 12;
                    goto Label_07B7;

                case 12:
                    this.<>f__this.FadeInHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_JAINA_02_50", "TUTORIAL06_JAINA_02", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 13;
                    goto Label_07B7;

                case 13:
                    this.<>f__this.FadeOutHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(0.25f));
                    this.$PC = 14;
                    goto Label_07B7;

                case 14:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_03_07", "TUTORIAL06_CHO_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    goto Label_07B5;

                default:
                    goto Label_07B5;
            }
            switch (missionEvent)
            {
                case 0x36:
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 6;
                    goto Label_07B7;

                case 0x37:
                    MulliganManager.Get().BeginMulligan();
                    this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_01_05", "TUTORIAL06_CHO_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_07B7;

                default:
                    goto Label_07B5;
            }
        Label_0112:
            if (this.<>f__this.m_choSpeaking)
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_17_16", "TUTORIAL06_CHO_17", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                this.$PC = 2;
            }
            goto Label_07B7;
            this.$PC = -1;
        Label_07B5:
            return false;
        Label_07B7:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator1A6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal Tutorial_06 <>f__this;
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
            int turn;
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    turn = this.turn;
                    switch (turn)
                    {
                        case 2:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.CHO_COMPLETE))
                            {
                                GameState.Get().SetBusy(true);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_15_15", "TUTORIAL06_CHO_15", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.$PC = 1;
                                goto Label_03F9;
                            }
                            goto Label_03F7;

                        case 4:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.CHO_COMPLETE))
                            {
                                GameState.Get().SetBusy(true);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_09_13", "TUTORIAL06_CHO_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.$PC = 2;
                                goto Label_03F9;
                            }
                            goto Label_03F7;
                    }
                    break;

                case 1:
                    GameState.Get().SetBusy(false);
                    goto Label_03F7;

                case 2:
                    GameState.Get().SetBusy(false);
                    goto Label_03F7;

                case 3:
                    goto Label_01D3;

                case 4:
                    this.<>f__this.m_choSpeaking = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_05_09", "TUTORIAL06_CHO_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 5;
                    goto Label_03F9;

                case 5:
                    this.<>f__this.m_choSpeaking = false;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_JAINA_03_51", "TUTORIAL06_JAINA_03", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 6;
                    goto Label_03F9;

                case 6:
                    this.<>f__this.m_choSpeaking = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_06_10", "TUTORIAL06_CHO_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_03F9;

                case 7:
                    this.<>f__this.m_choSpeaking = false;
                    goto Label_03F7;

                case 8:
                    goto Label_032F;

                case 9:
                    this.<>f__this.m_choSpeaking = false;
                    goto Label_03F7;

                default:
                    goto Label_03F7;
            }
            switch (turn)
            {
                case 14:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.CHO_COMPLETE))
                    {
                        goto Label_032F;
                    }
                    goto Label_03F7;

                case 15:
                    if (!this.<>f__this.DidLoseTutorial(TutorialProgress.CHO_COMPLETE))
                    {
                        break;
                    }
                    goto Label_03F7;

                case 0x10:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_20_17", "TUTORIAL06_CHO_20", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 10;
                    goto Label_03F9;

                default:
                    goto Label_03F7;
            }
        Label_01D3:
            while (this.<>f__this.m_choSpeaking)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_03F9;
            }
            this.$current = new WaitForSeconds(0.5f);
            this.$PC = 4;
            goto Label_03F9;
        Label_032F:
            while (this.<>f__this.m_choSpeaking)
            {
                this.$current = null;
                this.$PC = 8;
                goto Label_03F9;
            }
            this.<>f__this.m_choSpeaking = true;
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_06_CHO_21_18", "TUTORIAL06_CHO_21", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
            this.$PC = 9;
            goto Label_03F9;
            this.$PC = -1;
        Label_03F7:
            return false;
        Label_03F9:
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
    private sealed class <Wait>c__Iterator1A8 : IDisposable, IEnumerator, IEnumerator<object>
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

