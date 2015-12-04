using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tutorial_02 : TutorialEntity
{
    private GameObject costLabel;
    private Notification endTurnNotifier;
    private Notification handBounceArrow;
    private Notification manaNotifier;
    private Notification manaNotifier2;
    private int numManaThisTurn = 1;

    private void FadeInManaSpotlight()
    {
        Gameplay.Get().StartCoroutine(this.StartManaSpotFade());
    }

    public override List<RewardData> GetCustomRewards()
    {
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("EX1_015", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    public override string GetTurnStartReminderText()
    {
        object[] args = new object[] { this.numManaThisTurn };
        return GameStrings.Format("TUTORIAL02_HELP_04", args);
    }

    [DebuggerHidden]
    private IEnumerator HandleCoinFlip()
    {
        return new <HandleCoinFlip>c__Iterator198 { <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator197 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator196 { turn = turn, <$>turn = turn, <>f__this = this };
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
            if (this.costLabel != null)
            {
                UnityEngine.Object.Destroy(this.costLabel);
            }
            this.costLabel = actorObject;
            actorObject.transform.parent = costTextObject.transform;
            actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            actorObject.transform.localPosition = new Vector3(-0.025f, 0.28f, 0f);
            actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            actorObject.GetComponent<UberText>().Text = GameStrings.Get("GLOBAL_COST");
        }
    }

    public override void NotifyOfCardDropped(Entity entity)
    {
        if (entity.GetCardId() == "CS2_023")
        {
            BoardTutorial.Get().EnableFullHighlight(false);
        }
    }

    public override void NotifyOfCardGrabbed(Entity entity)
    {
        if ((entity.GetCardId() == "CS2_023") && (GameState.Get().GetFriendlySidePlayer().GetNumAvailableResources() >= entity.GetCost()))
        {
            BoardTutorial.Get().EnableFullHighlight(true);
        }
        if (this.costLabel != null)
        {
            UnityEngine.Object.Destroy(this.costLabel);
        }
    }

    public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
    {
        if (this.costLabel != null)
        {
            UnityEngine.Object.Destroy(this.costLabel);
        }
    }

    public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
    {
        if ((mousedOverEntity.GetZone() == TAG_ZONE.HAND) && (base.GetTag(GAME_TAG.TURN) <= 7))
        {
            AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.ManaLabelLoadedCallback), mousedOverEntity.GetCard(), false);
        }
    }

    public override void NotifyOfCoinFlipResult()
    {
        Gameplay.Get().StartCoroutine(this.HandleCoinFlip());
    }

    public override void NotifyOfDefeatCoinAnimation()
    {
        Gameplay.Get().StartCoroutine(this.PlayGoingOutSound());
    }

    public override bool NotifyOfEndTurnButtonPushed()
    {
        Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
        if ((optionsPacket != null) && (optionsPacket.List != null))
        {
            if (optionsPacket.List.Count == 1)
            {
                NotificationManager.Get().DestroyAllArrows();
                return true;
            }
            if (optionsPacket.List.Count == 2)
            {
                for (int i = 0; i < optionsPacket.List.Count; i++)
                {
                    Network.Options.Option option = optionsPacket.List[i];
                    if ((option.Type == Network.Options.Option.OptionType.POWER) && (GameState.Get().GetEntity(option.Main.ID).GetCardId() == "CS2_025"))
                    {
                        return true;
                    }
                }
            }
        }
        if (this.endTurnNotifier != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.endTurnNotifier);
        }
        Vector3 position = EndTurnButton.Get().transform.position;
        Vector3 vector2 = new Vector3(position.x - 3f, position.y, position.z);
        string key = "TUTORIAL_NO_ENDTURN";
        if (GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
        {
            key = "TUTORIAL_NO_ENDTURN_ATK";
        }
        this.endTurnNotifier = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get(key), true);
        NotificationManager.Get().DestroyNotification(this.endTurnNotifier, 2.5f);
        return false;
    }

    public override void NotifyOfEnemyManaCrystalSpawned()
    {
        AssetLoader.Get().LoadActor("plus1", new AssetLoader.GameObjectCallback(this.Plus1ActorLoadedCallbackEnemy), null, false);
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            base.SetTutorialProgress(TutorialProgress.MILLHOUSE_COMPLETE);
            base.PlaySound("VO_TUTORIAL02_MILLHOUSE_20_22_ALT", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL02_MILLHOUSE_20_22_ALT", 1f, true, false);
        }
    }

    public override string[] NotifyOfKeywordHelpPanelDisplay(Entity entity)
    {
        if (entity.GetCardId() == "CS2_122")
        {
            return new string[] { GameStrings.Get("TUTORIAL_RAID_LEADER_HEADLINE"), GameStrings.Get("TUTORIAL_RAID_LEADER_DESCRIPTION") };
        }
        if (entity.GetCardId() == "CS2_023")
        {
            return new string[] { GameStrings.Get("TUTORIAL_ARCANE_INTELLECT_HEADLINE"), GameStrings.Get("TUTORIAL_ARCANE_INTELLECT_DESCRIPTION") };
        }
        return null;
    }

    public override void NotifyOfManaCrystalSpawned()
    {
        AssetLoader.Get().LoadActor("plus1", new AssetLoader.GameObjectCallback(this.Plus1ActorLoadedCallback), null, false);
        if (base.GetTag(GAME_TAG.TURN) == 3)
        {
            Actor actor = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
            Gameplay.Get().StartCoroutine(base.PlaySoundAndWait("VO_TUTORIAL_02_JAINA_08_22", "TUTORIAL02_JAINA_08", Notification.SpeechBubbleDirection.BottomLeft, actor, 1f, true, false, 3f));
        }
        this.FadeInManaSpotlight();
    }

    protected override void NotifyOfManaError()
    {
        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.manaNotifier);
        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.manaNotifier2);
    }

    public override void NotifyOfTooltipZoneMouseOver(TooltipZone tooltip)
    {
        if (tooltip.targetObject.GetComponent<ManaCrystalMgr>() != null)
        {
            if (this.manaNotifier != null)
            {
                UnityEngine.Object.Destroy(this.manaNotifier.gameObject);
            }
            if (this.manaNotifier2 != null)
            {
                UnityEngine.Object.Destroy(this.manaNotifier2.gameObject);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayGoingOutSound()
    {
        return new <PlayGoingOutSound>c__Iterator19A { <>f__this = this };
    }

    private void Plus1ActorLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        Vector3 manaCrystalSpawnPosition = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
        Vector3 vector2 = new Vector3(manaCrystalSpawnPosition.x - 0.02f, manaCrystalSpawnPosition.y + 0.2f, manaCrystalSpawnPosition.z);
        actorObject.transform.position = vector2;
        actorObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        Vector3 localScale = actorObject.transform.localScale;
        actorObject.transform.localScale = new Vector3(1f, 1f, 1f);
        iTween.MoveTo(actorObject, new Vector3(vector2.x, vector2.y, vector2.z + 2f), 3f);
        float num = 2.5f;
        iTween.ScaleTo(actorObject, new Vector3(localScale.x * num, localScale.y * num, localScale.z * num), 3f);
        iTween.RotateTo(actorObject, new Vector3(0f, 170f, 0f), 3f);
        iTween.FadeTo(actorObject, 0f, 2.75f);
    }

    private void Plus1ActorLoadedCallbackEnemy(string actorName, GameObject actorObject, object callbackData)
    {
        Vector3 position = SceneUtils.FindChildBySubstring(Board.Get().gameObject, "ManaCounter_Opposing").transform.position;
        Vector3 vector2 = new Vector3(position.x, position.y + 0.2f, position.z);
        actorObject.transform.position = vector2;
        actorObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        Vector3 localScale = actorObject.transform.localScale;
        actorObject.transform.localScale = new Vector3(1f, 1f, 1f);
        iTween.MoveTo(actorObject, new Vector3(vector2.x, vector2.y, vector2.z - 2f), 3f);
        float num = 2.5f;
        iTween.ScaleTo(actorObject, new Vector3(localScale.x * num, localScale.y * num, localScale.z * num), 3f);
        iTween.RotateTo(actorObject, new Vector3(0f, 170f, 0f), 3f);
        iTween.FadeTo(actorObject, 0f, 2.75f);
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_02_05");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_01_04");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_04_07");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_05_08");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_07_10");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_11_14");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_13_16");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_15_17");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_06_09");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_03_06");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_17_19");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_08_11");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_09_12");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_10_13");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_16_18");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_20_22_ALT");
        base.PreloadSound("VO_TUTORIAL_02_JAINA_08_22");
        base.PreloadSound("VO_TUTORIAL_02_JAINA_03_18");
        base.PreloadSound("VO_TUTORIAL02_MILLHOUSE_19_21");
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
    private IEnumerator StartManaSpotFade()
    {
        return new <StartManaSpotFade>c__Iterator199();
    }

    [CompilerGenerated]
    private sealed class <HandleCoinFlip>c__Iterator198 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_02 <>f__this;
        internal Actor <millhouseActor>__0;

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
                    this.$current = new WaitForSeconds(3.5f);
                    this.$PC = 1;
                    goto Label_0105;

                case 1:
                    this.<millhouseActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<>f__this.FadeInHeroActor(this.<millhouseActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_01_04", "TUTORIAL02_MILLHOUSE_01", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    this.$PC = 2;
                    goto Label_0105;

                case 2:
                    GameState.Get().SetBusy(false);
                    this.$current = new WaitForSeconds(0.175f);
                    this.$PC = 3;
                    goto Label_0105;

                case 3:
                    this.<>f__this.FadeOutHeroActor(this.<millhouseActor>__0);
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0105:
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator197 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_02 <>f__this;
        internal GameObject <actorObject>__5;
        internal AudioSource <feelslikeLine>__2;
        internal Actor <jainaActor>__1;
        internal Actor <millhouseActor>__0;
        internal AudioSource <whatLine>__3;
        internal AudioSource <winngingLine>__4;
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
                    this.<millhouseActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.<jainaActor>__1 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    missionEvent = this.missionEvent;
                    switch (missionEvent)
                    {
                        case 1:
                            this.<>f__this.HandleGameStartEvent();
                            goto Label_0590;

                        case 2:
                            GameState.Get().SetBusy(true);
                            this.$current = new WaitForSeconds(1.5f);
                            this.$PC = 1;
                            goto Label_0592;

                        case 3:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_17_19", "TUTORIAL02_MILLHOUSE_17", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                            goto Label_0590;

                        case 4:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_08_11", "TUTORIAL02_MILLHOUSE_08", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                            this.$PC = 4;
                            goto Label_0592;

                        case 5:
                            GameState.Get().SetBusy(true);
                            this.<feelslikeLine>__2 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL02_MILLHOUSE_08_11");
                            goto Label_02D8;

                        case 6:
                            if (EndTurnButton.Get().IsInNMPState())
                            {
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_16_18", "TUTORIAL02_MILLHOUSE_16", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                            }
                            goto Label_0590;

                        case 9:
                            goto Label_0590;
                    }
                    break;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_03_06", "TUTORIAL02_MILLHOUSE_03", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    this.$PC = 2;
                    goto Label_0592;

                case 2:
                    GameState.Get().SetBusy(false);
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 3;
                    goto Label_0592;

                case 3:
                    if ((this.<>f__this.GetTag(GAME_TAG.TURN) == 1) && !EndTurnButton.Get().IsInWaitingState())
                    {
                        this.<>f__this.ShowEndTurnBouncingArrow();
                    }
                    goto Label_0590;

                case 4:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_02_JAINA_03_18", "TUTORIAL02_JAINA_03", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__1, 1f, true, false, 3f));
                    this.$PC = 5;
                    goto Label_0592;

                case 5:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_09_12", "TUTORIAL02_MILLHOUSE_09", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    goto Label_0590;

                case 6:
                    goto Label_02D8;

                case 7:
                    goto Label_031B;

                case 8:
                    goto Label_035E;

                case 9:
                    GameState.Get().SetBusy(false);
                    goto Label_0590;

                case 10:
                    this.<actorObject>__5 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__5.GetComponent<Notification>();
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL02_HELP_06"), GameStrings.Get("TUTORIAL02_HELP_07"), GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0.5f, 0f);
                    goto Label_0590;

                case 11:
                    HistoryManager.Get().DisableHistory();
                    MulliganManager.Get().BeginMulligan();
                    this.$current = new WaitForSeconds(1.1f);
                    this.$PC = 12;
                    goto Label_0592;

                case 12:
                    this.<>f__this.FadeOutHeroActor(this.<millhouseActor>__0);
                    goto Label_0590;

                default:
                    goto Label_0590;
            }
            switch (missionEvent)
            {
                case 0x36:
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 10;
                    goto Label_0592;

                case 0x37:
                    this.<>f__this.FadeInHeroActor(this.<millhouseActor>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_02_05", "TUTORIAL02_MILLHOUSE_02", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    this.$PC = 11;
                    goto Label_0592;

                default:
                    goto Label_0590;
            }
        Label_02D8:
            if (SoundManager.Get().IsPlaying(this.<feelslikeLine>__2))
            {
                this.$current = null;
                this.$PC = 6;
                goto Label_0592;
            }
            this.<whatLine>__3 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL_02_JAINA_03_18");
        Label_031B:
            while (SoundManager.Get().IsPlaying(this.<whatLine>__3))
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_0592;
            }
            this.<winngingLine>__4 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL02_MILLHOUSE_09_12");
        Label_035E:
            while (SoundManager.Get().IsPlaying(this.<winngingLine>__4))
            {
                this.$current = null;
                this.$PC = 8;
                goto Label_0592;
            }
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_10_13", "TUTORIAL02_MILLHOUSE_10", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
            this.$PC = 9;
            goto Label_0592;
            this.$PC = -1;
        Label_0590:
            return false;
        Label_0592:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator196 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal Tutorial_02 <>f__this;
        internal AudioSource <comeOnLine>__8;
        internal Notification.PopUpArrowDirection <direction>__3;
        internal Notification.PopUpArrowDirection <direction2>__6;
        internal Vector3 <manaPopupPosition>__2;
        internal Vector3 <manaPopupPosition2>__5;
        internal Vector3 <manaPosition>__1;
        internal Vector3 <manaPosition2>__4;
        internal Actor <millhouseActor>__0;
        internal AudioSource <previousLine>__7;
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
                    if (GameState.Get().IsFriendlySidePlayerTurn())
                    {
                        this.<>f__this.numManaThisTurn++;
                    }
                    this.<millhouseActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    switch (this.turn)
                    {
                        case 1:
                            this.<manaPosition>__1 = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
                            if (UniversalInputManager.UsePhoneUI == null)
                            {
                                this.<manaPopupPosition>__2 = new Vector3(this.<manaPosition>__1.x - 0.02f, this.<manaPosition>__1.y + 0.2f, this.<manaPosition>__1.z + 1.8f);
                                this.<direction>__3 = Notification.PopUpArrowDirection.Down;
                            }
                            else
                            {
                                this.<manaPopupPosition>__2 = new Vector3(this.<manaPosition>__1.x - 0.7f, this.<manaPosition>__1.y + 1.14f, this.<manaPosition>__1.z + 4.33f);
                                this.<direction>__3 = Notification.PopUpArrowDirection.RightDown;
                            }
                            goto Label_0188;

                        case 2:
                            GameState.Get().SetBusy(true);
                            this.$current = new WaitForSeconds(0.5f);
                            this.$PC = 4;
                            goto Label_08CF;

                        case 3:
                            this.<manaPosition2>__4 = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
                            if (UniversalInputManager.UsePhoneUI == null)
                            {
                                this.<manaPopupPosition2>__5 = new Vector3(this.<manaPosition2>__4.x - 0.02f, this.<manaPosition2>__4.y + 0.2f, this.<manaPosition2>__4.z + 1.7f);
                                this.<direction2>__6 = Notification.PopUpArrowDirection.Down;
                            }
                            else
                            {
                                this.<manaPopupPosition2>__5 = new Vector3(this.<manaPosition2>__4.x - 0.7f, this.<manaPosition2>__4.y + 1.14f, this.<manaPosition2>__4.z + 4.33f);
                                this.<direction2>__6 = Notification.PopUpArrowDirection.RightDown;
                            }
                            this.<>f__this.manaNotifier2 = NotificationManager.Get().CreatePopupText(this.<manaPopupPosition2>__5, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL02_HELP_03"), true);
                            this.<>f__this.manaNotifier2.ShowPopUpArrow(this.<direction2>__6);
                            this.$current = new WaitForSeconds(4.5f);
                            this.$PC = 8;
                            goto Label_08CF;

                        case 4:
                            if (this.<>f__this.manaNotifier2 != null)
                            {
                                NotificationManager.Get().DestroyNotification(this.<>f__this.manaNotifier2, 0f);
                            }
                            GameState.Get().SetBusy(true);
                            this.<previousLine>__7 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL02_MILLHOUSE_17_19");
                            goto Label_069E;

                        case 6:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_11_14", "TUTORIAL02_MILLHOUSE_11", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                            this.$PC = 12;
                            goto Label_08CF;

                        case 8:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_13_16", "TUTORIAL02_MILLHOUSE_13", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                            this.$PC = 13;
                            goto Label_08CF;

                        case 9:
                            this.$current = new WaitForSeconds(0.5f);
                            this.$PC = 14;
                            goto Label_08CF;

                        case 10:
                            GameState.Get().SetBusy(true);
                            this.<comeOnLine>__8 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL02_MILLHOUSE_16_18");
                            goto Label_0857;
                    }
                    goto Label_08CD;

                case 1:
                {
                    if (this.<>f__this.manaNotifier == null)
                    {
                        goto Label_08CD;
                    }
                    object[] args = new object[] { "amount", new Vector3(1f, 1f, 1f), "time", 1f };
                    iTween.PunchScale(this.<>f__this.manaNotifier.gameObject, iTween.Hash(args));
                    this.$current = new WaitForSeconds(4.5f);
                    this.$PC = 2;
                    goto Label_08CF;
                }
                case 2:
                {
                    if (this.<>f__this.manaNotifier == null)
                    {
                        goto Label_08CD;
                    }
                    object[] objArray2 = new object[] { "amount", new Vector3(1f, 1f, 1f), "time", 1f };
                    iTween.PunchScale(this.<>f__this.manaNotifier.gameObject, iTween.Hash(objArray2));
                    this.$current = new WaitForSeconds(4.5f);
                    this.$PC = 3;
                    goto Label_08CF;
                }
                case 3:
                    if (this.<>f__this.manaNotifier != null)
                    {
                        NotificationManager.Get().DestroyNotification(this.<>f__this.manaNotifier, 0f);
                    }
                    goto Label_08CD;

                case 4:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_04_07", "TUTORIAL02_MILLHOUSE_04", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    this.$PC = 5;
                    goto Label_08CF;

                case 5:
                    this.$current = new WaitForSeconds(0.3f);
                    this.$PC = 6;
                    goto Label_08CF;

                case 6:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_05_08", "TUTORIAL02_MILLHOUSE_05", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    this.$PC = 7;
                    goto Label_08CF;

                case 7:
                    GameState.Get().SetBusy(false);
                    goto Label_08CD;

                case 8:
                {
                    if (this.<>f__this.manaNotifier2 == null)
                    {
                        goto Label_08CD;
                    }
                    object[] objArray3 = new object[] { "amount", new Vector3(1f, 1f, 1f), "time", 1f };
                    iTween.PunchScale(this.<>f__this.manaNotifier2.gameObject, iTween.Hash(objArray3));
                    this.$current = new WaitForSeconds(4.5f);
                    this.$PC = 9;
                    goto Label_08CF;
                }
                case 9:
                    if (this.<>f__this.manaNotifier2 != null)
                    {
                        object[] objArray4 = new object[] { "amount", new Vector3(1f, 1f, 1f), "time", 1f };
                        iTween.PunchScale(this.<>f__this.manaNotifier2.gameObject, iTween.Hash(objArray4));
                    }
                    goto Label_08CD;

                case 10:
                    goto Label_069E;

                case 11:
                    GameState.Get().SetBusy(false);
                    goto Label_08CD;

                case 12:
                    GameState.Get().SetBusy(false);
                    goto Label_08CD;

                case 13:
                    GameState.Get().SetBusy(false);
                    goto Label_08CD;

                case 14:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_15_17", "TUTORIAL02_MILLHOUSE_15", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                    goto Label_08CD;

                case 15:
                    goto Label_0857;

                case 0x10:
                    GameState.Get().SetBusy(false);
                    goto Label_08CD;

                default:
                    goto Label_08CD;
            }
        Label_0188:
            this.<>f__this.manaNotifier = NotificationManager.Get().CreatePopupText(this.<manaPopupPosition>__2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL02_HELP_01"), true);
            this.<>f__this.manaNotifier.ShowPopUpArrow(this.<direction>__3);
            this.$current = new WaitForSeconds(4.5f);
            this.$PC = 1;
            goto Label_08CF;
        Label_069E:
            while (SoundManager.Get().IsPlaying(this.<previousLine>__7))
            {
                this.$current = null;
                this.$PC = 10;
                goto Label_08CF;
            }
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_07_10", "TUTORIAL02_MILLHOUSE_07", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
            this.$PC = 11;
            goto Label_08CF;
        Label_0857:
            if (SoundManager.Get().IsPlaying(this.<comeOnLine>__8))
            {
                this.$current = null;
                this.$PC = 15;
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL02_MILLHOUSE_06_09", "TUTORIAL02_MILLHOUSE_06", Notification.SpeechBubbleDirection.TopRight, this.<millhouseActor>__0, 1f, true, false, 3f));
                this.$PC = 0x10;
            }
            goto Label_08CF;
            this.$PC = -1;
        Label_08CD:
            return false;
        Label_08CF:
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
    private sealed class <PlayGoingOutSound>c__Iterator19A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_02 <>f__this;
        internal AudioSource <deathLine>__0;

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
                    this.<deathLine>__0 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL02_MILLHOUSE_20_22_ALT");
                    break;

                case 1:
                    break;

                default:
                    goto Label_008E;
            }
            if ((this.<deathLine>__0 != null) && this.<deathLine>__0.isPlaying)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.PlaySound("VO_TUTORIAL02_MILLHOUSE_19_21", 1f, true, false);
            this.$PC = -1;
        Label_008E:
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

    [CompilerGenerated]
    private sealed class <StartManaSpotFade>c__Iterator199 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Light <manaSpot>__0;
        internal float <TARGET_INTENSITY>__1;

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
                    this.<manaSpot>__0 = BoardTutorial.Get().m_ManaSpotlight;
                    this.<manaSpot>__0.enabled = true;
                    this.<manaSpot>__0.spotAngle = 179f;
                    this.<manaSpot>__0.intensity = 0f;
                    this.<TARGET_INTENSITY>__1 = 0.6f;
                    break;

                case 1:
                    break;

                case 2:
                case 3:
                    while (this.<manaSpot>__0.intensity > 0.05f)
                    {
                        this.<manaSpot>__0.intensity = Mathf.Lerp(this.<manaSpot>__0.intensity, 0f, UnityEngine.Time.deltaTime * 10f);
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_0184;
                    }
                    this.<manaSpot>__0.enabled = false;
                    this.$PC = -1;
                    goto Label_0182;

                default:
                    goto Label_0182;
            }
            if (this.<manaSpot>__0.intensity < (this.<TARGET_INTENSITY>__1 * 0.95f))
            {
                this.<manaSpot>__0.intensity = Mathf.Lerp(this.<manaSpot>__0.intensity, this.<TARGET_INTENSITY>__1, UnityEngine.Time.deltaTime * 5f);
                this.<manaSpot>__0.spotAngle = Mathf.Lerp(this.<manaSpot>__0.spotAngle, 80f, UnityEngine.Time.deltaTime * 5f);
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.$current = new WaitForSeconds(2f);
                this.$PC = 2;
            }
            goto Label_0184;
        Label_0182:
            return false;
        Label_0184:
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

