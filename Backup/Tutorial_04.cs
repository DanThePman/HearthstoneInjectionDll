using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tutorial_04 : TutorialEntity
{
    private Notification endTurnNotifier;
    private Notification handBounceArrow;
    private Notification heroPowerHelp;
    private bool m_hemetSpeaking;
    private GameObject m_heroPowerCostLabel;
    private bool m_isBogSheeped;
    private bool m_isPolymorphGrabbed;
    private bool m_shouldSignalPolymorph;
    private Notification noSheepPopup;
    private int numBeastsPlayed;
    private int numComplaintsMade;
    private Notification sheepTheBog;
    private Notification thatsABadPlayPopup;
    private bool victory;

    private bool AllowEndTurn()
    {
        if (this.m_shouldSignalPolymorph && (!this.m_shouldSignalPolymorph || !this.m_isBogSheeped))
        {
            return false;
        }
        return true;
    }

    public override List<RewardData> GetCustomRewards()
    {
        if (!this.victory)
        {
            return null;
        }
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("CS2_213", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    [DebuggerHidden]
    private IEnumerator HandleCoinFlip()
    {
        return new <HandleCoinFlip>c__Iterator1A0 { <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator19F { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator19E { turn = turn, <$>turn = turn, <>f__this = this };
    }

    public override bool IsKeywordHelpDelayOverridden()
    {
        return true;
    }

    private void ManaLabelLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        Card card = (Card) callbackData;
        GameObject costTextObject = card.GetActor().GetCostTextObject();
        if (costTextObject == null)
        {
            UnityEngine.Object.Destroy(actorObject);
        }
        else
        {
            this.m_heroPowerCostLabel = actorObject;
            SceneUtils.SetLayer(actorObject, GameLayer.Default);
            actorObject.transform.parent = costTextObject.transform;
            actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            actorObject.transform.localPosition = new Vector3(-0.02f, 0.38f, 0f);
            actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            actorObject.transform.localScale = new Vector3(actorObject.transform.localScale.x, actorObject.transform.localScale.x, actorObject.transform.localScale.x);
            actorObject.GetComponent<UberText>().Text = GameStrings.Get("GLOBAL_COST");
        }
    }

    public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
    {
        if (this.m_shouldSignalPolymorph)
        {
            if ((clickedEntity.GetCardId() != "CS1_069") || !wasInTargetMode)
            {
                if (this.m_isPolymorphGrabbed && wasInTargetMode)
                {
                    if (this.noSheepPopup != null)
                    {
                        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.noSheepPopup);
                    }
                    Vector3 position = clickedEntity.GetCard().transform.position;
                    Vector3 vector2 = new Vector3(position.x + 2.5f, position.y, position.z);
                    this.noSheepPopup = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL04_HELP_03"), true);
                    NotificationManager.Get().DestroyNotification(this.noSheepPopup, 3f);
                }
                return false;
            }
            if (this.sheepTheBog != null)
            {
                NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.sheepTheBog);
            }
            NotificationManager.Get().DestroyAllPopUps();
            this.m_shouldSignalPolymorph = false;
            this.m_isBogSheeped = true;
        }
        return true;
    }

    public override void NotifyOfCardDropped(Entity entity)
    {
        this.m_isPolymorphGrabbed = false;
        if (this.m_shouldSignalPolymorph)
        {
            if (this.sheepTheBog != null)
            {
                NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.sheepTheBog);
            }
            NotificationManager.Get().DestroyAllPopUps();
            if (this.ShouldShowArrowOnCardInHand(entity))
            {
                Gameplay.Get().StartCoroutine(this.ShowArrowInSeconds(0.5f));
            }
        }
    }

    public override void NotifyOfCardGrabbed(Entity entity)
    {
        if (this.m_shouldSignalPolymorph)
        {
            if (entity.GetCardId() == "CS2_022")
            {
                this.m_isPolymorphGrabbed = true;
                if (this.sheepTheBog != null)
                {
                    NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.sheepTheBog);
                }
                if (this.handBounceArrow != null)
                {
                    NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.handBounceArrow);
                }
                NotificationManager.Get().DestroyAllPopUps();
                Vector3 position = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetFirstCard().transform.position;
                Vector3 vector2 = new Vector3(position.x - 3f, position.y, position.z);
                this.sheepTheBog = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL04_HELP_02"), true);
                this.sheepTheBog.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
            }
            else
            {
                if (this.sheepTheBog != null)
                {
                    NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.sheepTheBog);
                }
                NotificationManager.Get().DestroyAllPopUps();
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    InputManager.Get().ReturnHeldCardToHand();
                }
                else
                {
                    InputManager.Get().DropHeldCard();
                }
            }
        }
    }

    public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
    {
        if (this.ShouldShowArrowOnCardInHand(mousedOffEntity))
        {
            Gameplay.Get().StartCoroutine(this.ShowArrowInSeconds(0.5f));
        }
    }

    public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
    {
        if (this.ShouldShowArrowOnCardInHand(mousedOverEntity))
        {
            NotificationManager.Get().DestroyAllArrows();
        }
    }

    public override void NotifyOfCoinFlipResult()
    {
        Gameplay.Get().StartCoroutine(this.HandleCoinFlip());
    }

    public override void NotifyOfDefeatCoinAnimation()
    {
        if (this.victory)
        {
            base.PlaySound("VO_TUTORIAL_04_JAINA_10_44", 1f, true, false);
        }
    }

    public override bool NotifyOfEndTurnButtonPushed()
    {
        if ((base.GetTag(GAME_TAG.TURN) != 4) && this.AllowEndTurn())
        {
            NotificationManager.Get().DestroyAllArrows();
            return true;
        }
        Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
        if (((optionsPacket != null) && (optionsPacket.List != null)) && (optionsPacket.List.Count == 1))
        {
            NotificationManager.Get().DestroyAllArrows();
            return true;
        }
        if (this.endTurnNotifier != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.endTurnNotifier);
        }
        Vector3 position = EndTurnButton.Get().transform.position;
        Vector3 vector2 = new Vector3(position.x - 3f, position.y, position.z);
        string key = "TUTORIAL_NO_ENDTURN_HP";
        if (GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
        {
            key = "TUTORIAL_NO_ENDTURN_ATK";
        }
        if (this.m_shouldSignalPolymorph && !this.m_isBogSheeped)
        {
            key = "TUTORIAL_NO_ENDTURN";
        }
        this.endTurnNotifier = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get(key), true);
        NotificationManager.Get().DestroyNotification(this.endTurnNotifier, 2.5f);
        return false;
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        if (this.m_heroPowerCostLabel != null)
        {
            UnityEngine.Object.Destroy(this.m_heroPowerCostLabel);
        }
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            this.victory = true;
            base.SetTutorialProgress(TutorialProgress.NESINGWARY_COMPLETE);
            base.PlaySound("VO_TUTORIAL04_HEMET_23_21", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL04_HEMET_23_21", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            base.SetTutorialLostProgress(TutorialProgress.NESINGWARY_COMPLETE);
        }
    }

    public override void NotifyOfTargetModeCancelled()
    {
        if (this.sheepTheBog != null)
        {
            NotificationManager.Get().DestroyAllPopUps();
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL04_HEMET_23_21");
        base.PreloadSound("VO_TUTORIAL04_HEMET_15_13");
        base.PreloadSound("VO_TUTORIAL04_HEMET_20_18");
        base.PreloadSound("VO_TUTORIAL04_HEMET_16_14");
        base.PreloadSound("VO_TUTORIAL04_HEMET_13_12");
        base.PreloadSound("VO_TUTORIAL04_HEMET_19_17");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_09_43");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_10_44");
        base.PreloadSound("VO_TUTORIAL04_HEMET_06_05");
        base.PreloadSound("VO_TUTORIAL04_HEMET_07_06_ALT");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_04_40");
        base.PreloadSound("VO_TUTORIAL04_HEMET_08_07");
        base.PreloadSound("VO_TUTORIAL04_HEMET_09_08");
        base.PreloadSound("VO_TUTORIAL04_HEMET_10_09");
        base.PreloadSound("VO_TUTORIAL04_HEMET_11_10");
        base.PreloadSound("VO_TUTORIAL04_HEMET_12_11");
        base.PreloadSound("VO_TUTORIAL04_HEMET_12_11_ALT");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_08_42");
        base.PreloadSound("VO_TUTORIAL04_HEMET_01_01");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_01_37");
        base.PreloadSound("VO_TUTORIAL04_HEMET_02_02");
        base.PreloadSound("VO_TUTORIAL04_HEMET_03_03");
        base.PreloadSound("VO_TUTORIAL_04_JAINA_02_38");
        base.PreloadSound("VO_TUTORIAL04_HEMET_04_04_ALT");
    }

    public override bool ShouldAllowCardGrab(Entity entity)
    {
        if (this.m_shouldSignalPolymorph && (entity.GetCardId() != "CS2_022"))
        {
            return false;
        }
        return true;
    }

    private bool ShouldShowArrowOnCardInHand(Entity entity)
    {
        if (entity.GetZone() != TAG_ZONE.HAND)
        {
            return false;
        }
        return (this.m_shouldSignalPolymorph && (entity.GetCardId() == "CS2_022"));
    }

    [DebuggerHidden]
    private IEnumerator ShowArrowInSeconds(float seconds)
    {
        return new <ShowArrowInSeconds>c__Iterator1A2 { seconds = seconds, <$>seconds = seconds, <>f__this = this };
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

    private void ShowHandBouncingArrow()
    {
        if (this.handBounceArrow == null)
        {
            List<Card> cards = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
            if (cards.Count != 0)
            {
                Card card = null;
                foreach (Card card2 in cards)
                {
                    if (card2.GetEntity().GetCardId() == "CS2_022")
                    {
                        card = card2;
                    }
                }
                if ((card != null) && !this.m_isPolymorphGrabbed)
                {
                    Vector3 position = card.transform.position;
                    Vector3 vector2 = new Vector3(position.x, position.y, position.z + 2f);
                    this.handBounceArrow = NotificationManager.Get().CreateBouncingArrow(vector2, new Vector3(0f, 0f, 0f));
                    this.handBounceArrow.transform.parent = card.transform;
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator Wait(float seconds)
    {
        return new <Wait>c__Iterator1A1 { seconds = seconds, <$>seconds = seconds };
    }

    [CompilerGenerated]
    private sealed class <HandleCoinFlip>c__Iterator1A0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_04 <>f__this;
        internal Actor <enemyActor>__0;
        internal Actor <jainaActor>__1;

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
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(1f));
                    this.$PC = 1;
                    goto Label_0286;

                case 1:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    break;

                case 2:
                    break;

                case 3:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.$current = new WaitForSeconds(0.3f);
                    this.$PC = 4;
                    goto Label_0286;

                case 4:
                    this.<>f__this.FadeInHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_02_38", "TUTORIAL04_JAINA_02", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 5;
                    goto Label_0286;

                case 5:
                    this.<>f__this.FadeOutHeroActor(this.<jainaActor>__1);
                    this.$current = new WaitForSeconds(0.25f);
                    this.$PC = 6;
                    goto Label_0286;

                case 6:
                    if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                    {
                        goto Label_0272;
                    }
                    this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_04_04_ALT", "TUTORIAL04_HEMET_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_0286;

                case 7:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.$current = new WaitForSeconds(0.4f);
                    this.$PC = 8;
                    goto Label_0286;

                case 8:
                    goto Label_0272;

                default:
                    goto Label_0284;
            }
            if (this.<>f__this.m_hemetSpeaking)
            {
                this.$current = null;
                this.$PC = 2;
            }
            else
            {
                this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_03_03", "TUTORIAL04_HEMET_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                this.$PC = 3;
            }
            goto Label_0286;
        Label_0272:
            GameState.Get().SetBusy(false);
            this.$PC = -1;
        Label_0284:
            return false;
        Label_0286:
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator19F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_04 <>f__this;
        internal GameObject <actorObject>__2;
        internal string <bodyText>__3;
        internal Actor <enemyActor>__0;
        internal Actor <jainaActor>__1;
        internal int missionEvent;

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
                    switch (this.missionEvent)
                    {
                        case 1:
                            this.<>f__this.HandleGameStartEvent();
                            break;

                        case 2:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_06_05", "TUTORIAL04_HEMET_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 1;
                            goto Label_071F;

                        case 3:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(2f));
                            this.$PC = 3;
                            goto Label_071F;

                        case 4:
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                InputManager.Get().GetFriendlyHand().SetHandEnlarged(false);
                            }
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_04_40", "TUTORIAL04_JAINA_04", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            this.$PC = 4;
                            goto Label_071F;

                        case 5:
                            if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                break;
                            }
                            switch (this.<>f__this.numBeastsPlayed)
                            {
                                case 0:
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_08_07", "TUTORIAL04_HEMET_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                    break;

                                case 1:
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_09_08", "TUTORIAL04_HEMET_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                    break;

                                case 2:
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_10_09", "TUTORIAL04_HEMET_10", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                    break;

                                case 3:
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_11_10", "TUTORIAL04_HEMET_11", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                    break;
                            }
                            goto Label_0370;

                        case 6:
                            if (this.<>f__this.numComplaintsMade == 0)
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_12_11", "TUTORIAL04_HEMET_12a", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.<>f__this.numComplaintsMade++;
                            }
                            else
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_12_11_ALT", "TUTORIAL04_HEMET_12b", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            }
                            break;

                        case 7:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_08_42", "TUTORIAL04_JAINA_08", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                            break;

                        case 0x36:
                            this.$current = new WaitForSeconds(2f);
                            this.$PC = 5;
                            goto Label_071F;

                        case 0x37:
                            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_01_01", "TUTORIAL04_HEMET_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                                this.$PC = 6;
                                goto Label_071F;
                            }
                            goto Label_0671;
                    }
                    goto Label_071D;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_07_06_ALT", "TUTORIAL04_HEMET_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.Wait(1f));
                    this.$PC = 2;
                    goto Label_071F;

                case 2:
                    GameState.Get().SetBusy(false);
                    goto Label_071D;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_071D;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_071D;

                case 5:
                    this.<actorObject>__2 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__2.GetComponent<Notification>();
                    this.<bodyText>__3 = this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE) ? GameStrings.Get("TUTORIAL04_HELP_16") : GameStrings.Get("TUTORIAL04_HELP_15");
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL04_HELP_14"), this.<bodyText>__3, GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material = this.<>f__this.startingPopup.swapMaterial;
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0f, 0f);
                    goto Label_071D;

                case 6:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.<>f__this.FadeInHeroActor(this.<jainaActor>__1);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_01_37", "TUTORIAL04_JAINA_01", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_071F;

                case 7:
                    this.<>f__this.FadeOutHeroActor(this.<jainaActor>__1);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 8;
                    goto Label_071F;

                case 8:
                    goto Label_0671;

                case 9:
                    this.<>f__this.FadeOutHeroActor(this.<enemyActor>__0);
                    this.<>f__this.m_hemetSpeaking = false;
                    goto Label_071D;

                default:
                    goto Label_071D;
            }
        Label_0370:
            this.<>f__this.numBeastsPlayed++;
            goto Label_071D;
        Label_0671:
            MulliganManager.Get().BeginMulligan();
            if (!this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
            {
                this.<>f__this.m_hemetSpeaking = true;
                this.<>f__this.FadeInHeroActor(this.<enemyActor>__0);
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_02_02", "TUTORIAL04_HEMET_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                this.$PC = 9;
                goto Label_071F;
                this.$PC = -1;
            }
        Label_071D:
            return false;
        Label_071F:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator19E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal List<Card>.Enumerator <$s_714>__7;
        internal Tutorial_04 <>f__this;
        internal Card <card>__8;
        internal List<Card> <cardsInHand>__5;
        internal Actor <enemyActor>__0;
        internal Vector3 <help04Position>__3;
        internal Vector3 <help04Position>__4;
        internal Vector3 <heroPowerPosition>__2;
        internal Actor <jainaActor>__1;
        internal Card <polyMorph>__6;
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
                    this.<>f__this.m_shouldSignalPolymorph = false;
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    switch (this.turn)
                    {
                        case 1:
                            if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                break;
                            }
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_15_13", "TUTORIAL04_HEMET_15", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 1;
                            goto Label_05CE;

                        case 4:
                            this.$current = new WaitForSeconds(1f);
                            this.$PC = 2;
                            goto Label_05CE;

                        case 5:
                            NotificationManager.Get().DestroyNotification(this.<>f__this.heroPowerHelp, 0f);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_20_18", "TUTORIAL04_HEMET_20", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            goto Label_05CC;

                        case 7:
                            if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                break;
                            }
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_16_14", "TUTORIAL04_HEMET_16", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 3;
                            goto Label_05CE;

                        case 9:
                            if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                break;
                            }
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_13_12", "TUTORIAL04_HEMET_13", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 4;
                            goto Label_05CE;

                        case 11:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().SetGameStateBusy(false, 3f);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL04_HEMET_19_17", "TUTORIAL04_HEMET_19", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false, 3f));
                            this.$PC = 5;
                            goto Label_05CE;

                        case 12:
                            if (this.<>f__this.DidLoseTutorial(TutorialProgress.NESINGWARY_COMPLETE))
                            {
                                this.<>f__this.m_shouldSignalPolymorph = true;
                                this.<cardsInHand>__5 = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
                                if (InputManager.Get().GetHeldCard() == null)
                                {
                                    this.<polyMorph>__6 = null;
                                    this.<$s_714>__7 = this.<cardsInHand>__5.GetEnumerator();
                                    try
                                    {
                                        while (this.<$s_714>__7.MoveNext())
                                        {
                                            this.<card>__8 = this.<$s_714>__7.Current;
                                            if (this.<card>__8.GetEntity().GetCardId() == "CS2_022")
                                            {
                                                this.<polyMorph>__6 = this.<card>__8;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        this.<$s_714>__7.Dispose();
                                    }
                                    if ((this.<polyMorph>__6 != null) && !this.<polyMorph>__6.IsMousedOver())
                                    {
                                        Gameplay.Get().StartCoroutine(this.<>f__this.ShowArrowInSeconds(0f));
                                        break;
                                        this.$PC = -1;
                                    }
                                }
                            }
                            goto Label_05CC;
                    }
                    break;

                case 1:
                    GameState.Get().SetBusy(false);
                    break;

                case 2:
                    this.<heroPowerPosition>__2 = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard().transform.position;
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        this.<help04Position>__4 = new Vector3(this.<heroPowerPosition>__2.x + 3f, this.<heroPowerPosition>__2.y, this.<heroPowerPosition>__2.z);
                        this.<>f__this.heroPowerHelp = NotificationManager.Get().CreatePopupText(this.<help04Position>__4, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL04_HELP_01"), true);
                        this.<>f__this.heroPowerHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                        AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.<>f__this.ManaLabelLoadedCallback), GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard(), false);
                        break;
                    }
                    this.<help04Position>__3 = new Vector3(this.<heroPowerPosition>__2.x, this.<heroPowerPosition>__2.y, this.<heroPowerPosition>__2.z + 2.3f);
                    this.<>f__this.heroPowerHelp = NotificationManager.Get().CreatePopupText(this.<help04Position>__3, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL04_HELP_01"), true);
                    this.<>f__this.heroPowerHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
                    break;

                case 3:
                    GameState.Get().SetBusy(false);
                    break;

                case 4:
                    GameState.Get().SetBusy(false);
                    break;

                case 5:
                    this.$current = new WaitForSeconds(0.7f);
                    this.$PC = 6;
                    goto Label_05CE;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_04_JAINA_09_43", "TUTORIAL04_JAINA_09", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_05CE;
            }
        Label_05CC:
            return false;
        Label_05CE:
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
    private sealed class <ShowArrowInSeconds>c__Iterator1A2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal List<Card>.Enumerator <$s_715>__2;
        internal Tutorial_04 <>f__this;
        internal Card <card>__3;
        internal List<Card> <handCards>__0;
        internal Card <polyMorph>__1;
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
                    goto Label_018C;

                case 1:
                    this.<handCards>__0 = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
                    if ((this.<handCards>__0.Count != 0) && !this.<>f__this.m_isPolymorphGrabbed)
                    {
                        this.<polyMorph>__1 = null;
                        this.<$s_715>__2 = this.<handCards>__0.GetEnumerator();
                        try
                        {
                            while (this.<$s_715>__2.MoveNext())
                            {
                                this.<card>__3 = this.<$s_715>__2.Current;
                                if (this.<card>__3.GetEntity().GetCardId() == "CS2_022")
                                {
                                    this.<polyMorph>__1 = this.<card>__3;
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_715>__2.Dispose();
                        }
                        if (this.<polyMorph>__1 != null)
                        {
                            break;
                        }
                    }
                    goto Label_018A;

                case 2:
                    break;

                default:
                    goto Label_018A;
            }
            while (iTween.Count(this.<polyMorph>__1.gameObject) > 0)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_018C;
            }
            if (!this.<polyMorph>__1.IsMousedOver() && (InputManager.Get().GetHeldCard() != this.<polyMorph>__1))
            {
                this.<>f__this.ShowHandBouncingArrow();
                this.$PC = -1;
            }
        Label_018A:
            return false;
        Label_018C:
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
    private sealed class <Wait>c__Iterator1A1 : IDisposable, IEnumerator, IEnumerator<object>
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

