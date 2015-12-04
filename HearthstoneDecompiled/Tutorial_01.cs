using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Tutorial_01 : TutorialEntity
{
    private bool announcerIsFinishedYapping;
    private KeywordHelpPanel attackHelpPanel;
    private GameObject attackLabel;
    private Notification attackWithYourMinion;
    private GameObject costLabel;
    private Notification crushThisGnoll;
    private Notification endTurnNotifier;
    private bool firstAttackFinished;
    private Card firstMurlocCard;
    private Card firstRaptorCard;
    private Notification freeCardsPopup;
    private Notification handBounceArrow;
    private Notification handFadeArrow;
    private KeywordHelpPanel healthHelpPanel;
    private GameObject healthLabel;
    private PlatformDependentValue<Vector3> m_attackTooltipPosition;
    private PlatformDependentValue<float> m_gemScale;
    private PlatformDependentValue<Vector3> m_healthTooltipPosition;
    private PlatformDependentValue<Vector3> m_heroHealthTooltipPosition;
    private bool m_isShowingAttackHelpPanel;
    private bool m_jainaSpeaking;
    private Card mousedOverCard;
    private Notification noFireballPopup;
    private int numTimesTextSwapStarted;
    private bool packOpened;
    private GameObject startingPack;
    private string textToShowForAttackTip = GameStrings.Get("TUTORIAL01_HELP_02");
    private bool tooltipsDisabled = true;

    public Tutorial_01()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1.75f,
            Phone = 1.2f
        };
        this.m_gemScale = value2;
        PlatformDependentValue<Vector3> value3 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(-2.15f, 0f, -0.62f),
            Phone = new Vector3(-3.5f, 0f, -0.62f)
        };
        this.m_attackTooltipPosition = value3;
        value3 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(2.05f, 0f, -0.62f),
            Phone = new Vector3(3.25f, 0f, -0.62f)
        };
        this.m_healthTooltipPosition = value3;
        value3 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(2.4f, 0.3f, -0.8f),
            Phone = new Vector3(3.5f, 0.3f, 0.6f)
        };
        this.m_heroHealthTooltipPosition = value3;
        MulliganManager.Get().ForceMulliganActive(true);
        Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_START_TUTORIAL);
    }

    public override bool AreTooltipsDisabled()
    {
        return this.tooltipsDisabled;
    }

    private void AttackLabelLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (!this.m_isShowingAttackHelpPanel)
        {
            Card card = (Card) callbackData;
            GameObject attackTextObject = card.GetActor().GetAttackTextObject();
            if (attackTextObject == null)
            {
                UnityEngine.Object.Destroy(actorObject);
            }
            else
            {
                this.attackLabel = actorObject;
                actorObject.transform.parent = attackTextObject.transform;
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.transform.localPosition = new Vector3(-0.2f, -0.3039344f, 0f);
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.GetComponent<UberText>().Text = GameStrings.Get("GLOBAL_ATTACK");
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator BeginFlashingMinionLoop(Card minionToFlash)
    {
        return new <BeginFlashingMinionLoop>c__Iterator191 { minionToFlash = minionToFlash, <$>minionToFlash = minionToFlash, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ContinueFinishingCustomIntro()
    {
        return new <ContinueFinishingCustomIntro>c__Iterator195 { <>f__this = this };
    }

    public override bool DoAlternateMulliganIntro()
    {
        AssetLoader.Get().LoadActor("GameOpen_Pack", new AssetLoader.GameObjectCallback(this.PackLoadedCallback), null, false);
        return true;
    }

    private void EnsureCardGemsAreOnTheCorrectLayer()
    {
        List<Card> list = new List<Card>();
        list.AddRange(GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards());
        list.AddRange(GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards());
        list.Add(GameState.Get().GetFriendlySidePlayer().GetHeroCard());
        list.Add(GameState.Get().GetOpposingSidePlayer().GetHeroCard());
        foreach (Card card in list)
        {
            if ((card != null) && (card.GetActor() != null))
            {
                if (card.GetActor().GetAttackObject() != null)
                {
                    SceneUtils.SetLayer(card.GetActor().GetAttackObject().gameObject, GameLayer.Default);
                }
                if (card.GetActor().GetHealthObject() != null)
                {
                    SceneUtils.SetLayer(card.GetActor().GetHealthObject().gameObject, GameLayer.Default);
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator FlashMinionUntilAttackBegins(Card minionToFlash)
    {
        return new <FlashMinionUntilAttackBegins>c__Iterator190 { minionToFlash = minionToFlash, <$>minionToFlash = minionToFlash, <>f__this = this };
    }

    public override List<RewardData> GetCustomRewards()
    {
        List<RewardData> list = new List<RewardData>();
        CardRewardData item = new CardRewardData("CS2_023", TAG_PREMIUM.NORMAL, 2);
        item.MarkAsDummyReward();
        list.Add(item);
        return list;
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator18E { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator18D { turn = turn, <$>turn = turn, <>f__this = this };
    }

    private void HealthLabelLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (!this.m_isShowingAttackHelpPanel)
        {
            Card card = (Card) callbackData;
            GameObject healthTextObject = card.GetActor().GetHealthTextObject();
            if (healthTextObject == null)
            {
                UnityEngine.Object.Destroy(actorObject);
            }
            else
            {
                this.healthLabel = actorObject;
                actorObject.transform.parent = healthTextObject.transform;
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.transform.localPosition = new Vector3(0.21f, -0.31f, 0f);
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.GetComponent<UberText>().Text = GameStrings.Get("GLOBAL_HEALTH");
            }
        }
    }

    private void HideFadeArrow()
    {
        if (this.handFadeArrow != null)
        {
            NotificationManager.Get().DestroyNotification(this.handFadeArrow, 0f);
            this.handFadeArrow = null;
        }
    }

    public override bool IsMouseOverDelayOverriden()
    {
        return true;
    }

    private void ManaLabelLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (!this.m_isShowingAttackHelpPanel)
        {
            Card card = (Card) callbackData;
            GameObject costTextObject = card.GetActor().GetCostTextObject();
            if (costTextObject == null)
            {
                UnityEngine.Object.Destroy(actorObject);
            }
            else
            {
                this.costLabel = actorObject;
                actorObject.transform.parent = costTextObject.transform;
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.transform.localPosition = new Vector3(-0.017f, 0.3512533f, 0f);
                actorObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                actorObject.GetComponent<UberText>().Text = GameStrings.Get("GLOBAL_COST");
            }
        }
    }

    public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
    {
        if (base.GetTag(GAME_TAG.TURN) == 4)
        {
            if (clickedEntity.GetCardId() == "CS2_168")
            {
                if (!wasInTargetMode && !this.firstAttackFinished)
                {
                    if (this.crushThisGnoll != null)
                    {
                        NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.crushThisGnoll);
                    }
                    NotificationManager.Get().DestroyAllPopUps();
                    Vector3 position = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetFirstCard().transform.position;
                    Vector3 vector2 = new Vector3(position.x - 3f, position.y, position.z);
                    this.crushThisGnoll = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_03"), true);
                    this.crushThisGnoll.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                    this.numTimesTextSwapStarted++;
                    Gameplay.Get().StartCoroutine(this.WaitAndThenHide(this.numTimesTextSwapStarted));
                }
            }
            else if ((clickedEntity.GetCardId() == "TU4a_002") && wasInTargetMode)
            {
                if (this.crushThisGnoll != null)
                {
                    NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.crushThisGnoll);
                }
                NotificationManager.Get().DestroyAllPopUps();
                this.firstAttackFinished = true;
            }
        }
        else if (((base.GetTag(GAME_TAG.TURN) == 6) && (clickedEntity.GetCardId() == "TU4a_001")) && wasInTargetMode)
        {
            NotificationManager.Get().DestroyAllPopUps();
        }
        if ((wasInTargetMode && (InputManager.Get().GetHeldCard() != null)) && (InputManager.Get().GetHeldCard().GetEntity().GetCardId() == "CS2_029"))
        {
            if (clickedEntity.IsControlledByLocalUser())
            {
                this.ShowDontFireballYourselfPopup(clickedEntity.GetCard().transform.position);
                return false;
            }
            if ((clickedEntity.GetCardId() == "TU4a_003") && (base.GetTag(GAME_TAG.TURN) >= 8))
            {
                if (this.noFireballPopup != null)
                {
                    NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.noFireballPopup);
                }
                Vector3 vector3 = clickedEntity.GetCard().transform.position;
                Vector3 vector4 = new Vector3(vector3.x - 3f, vector3.y, vector3.z);
                this.noFireballPopup = NotificationManager.Get().CreatePopupText(vector4, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_08"), true);
                NotificationManager.Get().DestroyNotification(this.noFireballPopup, 3f);
                return false;
            }
        }
        return true;
    }

    public override void NotifyOfCardDropped(Entity entity)
    {
        if ((base.GetTag(GAME_TAG.TURN) == 2) || (entity.GetCardId() == "CS2_025"))
        {
            BoardTutorial.Get().EnableHighlight(false);
        }
    }

    public override void NotifyOfCardGrabbed(Entity entity)
    {
        if ((base.GetTag(GAME_TAG.TURN) == 2) || (entity.GetCardId() == "CS2_025"))
        {
            BoardTutorial.Get().EnableHighlight(true);
            if (base.GetTag(GAME_TAG.TURN) == 2)
            {
            }
        }
        this.NukeNumberLabels();
    }

    public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
    {
        if (this.ShouldShowArrowOnCardInHand(mousedOffEntity))
        {
            Gameplay.Get().StartCoroutine(this.ShowArrowInSeconds(0.5f));
        }
        this.NukeNumberLabels();
    }

    public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
    {
        if (this.ShouldShowArrowOnCardInHand(mousedOverEntity))
        {
            NotificationManager.Get().DestroyAllArrows();
        }
        if (mousedOverEntity.GetZone() == TAG_ZONE.HAND)
        {
            this.mousedOverCard = mousedOverEntity.GetCard();
            AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.ManaLabelLoadedCallback), this.mousedOverCard, false);
            AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.AttackLabelLoadedCallback), this.mousedOverCard, false);
            AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.HealthLabelLoadedCallback), this.mousedOverCard, false);
        }
    }

    public override void NotifyOfCardTooltipDisplayHide(Card card)
    {
        if (this.attackHelpPanel != null)
        {
            if (card != null)
            {
                GemObject attackObject = card.GetActor().GetAttackObject();
                SceneUtils.SetLayer(attackObject.gameObject, GameLayer.Default);
                attackObject.Shrink();
            }
            UnityEngine.Object.Destroy(this.attackHelpPanel.gameObject);
            this.m_isShowingAttackHelpPanel = false;
        }
        if (this.healthHelpPanel != null)
        {
            if (card != null)
            {
                GemObject healthObject = card.GetActor().GetHealthObject();
                SceneUtils.SetLayer(healthObject.gameObject, GameLayer.Default);
                healthObject.Shrink();
            }
            UnityEngine.Object.Destroy(this.healthHelpPanel.gameObject);
        }
    }

    public override bool NotifyOfCardTooltipDisplayShow(Card card)
    {
        if (!GameState.Get().IsGameOver())
        {
            Entity entity = card.GetEntity();
            if (entity.IsMinion())
            {
                if (this.attackHelpPanel == null)
                {
                    this.m_isShowingAttackHelpPanel = true;
                    this.ShowAttackTooltip(card);
                    Gameplay.Get().StartCoroutine(this.ShowHealthTooltipAfterWait(card));
                }
                return false;
            }
            if (!entity.IsHero())
            {
                return true;
            }
            if (this.healthHelpPanel == null)
            {
                this.ShowHealthTooltip(card);
            }
        }
        return false;
    }

    public override void NotifyOfCustomIntroFinished()
    {
        Card heroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
        Card card2 = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
        heroCard.SetDoNotSort(false);
        card2.GetActor().TurnOnCollider();
        heroCard.GetActor().TurnOnCollider();
        heroCard.transform.parent = null;
        card2.transform.parent = null;
        SceneUtils.SetLayer(heroCard.GetActor().GetRootObject(), GameLayer.CardRaycast);
        Gameplay.Get().StartCoroutine(this.ContinueFinishingCustomIntro());
    }

    public override void NotifyOfDefeatCoinAnimation()
    {
        base.PlaySound("VO_TUTORIAL_01_JAINA_13_10", 1f, true, false);
    }

    public override bool NotifyOfEndTurnButtonPushed()
    {
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
        string key = "TUTORIAL_NO_ENDTURN_ATK";
        if (!GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
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
        if (this.attackHelpPanel != null)
        {
            UnityEngine.Object.Destroy(this.attackHelpPanel.gameObject);
            this.attackHelpPanel = null;
        }
        if (this.healthHelpPanel != null)
        {
            UnityEngine.Object.Destroy(this.healthHelpPanel.gameObject);
            this.healthHelpPanel = null;
        }
        this.EnsureCardGemsAreOnTheCorrectLayer();
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            base.SetTutorialProgress(TutorialProgress.HOGGER_COMPLETE);
            base.PlaySound("VO_TUTORIAL_01_HOGGER_11_11", 1f, true, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            base.PlaySound("VO_TUTORIAL_01_HOGGER_11_11", 1f, true, false);
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            InputManager.Get().RemovePhoneHandShownListener(new InputManager.PhoneHandShownCallback(this.OnPhoneHandShown));
            InputManager.Get().RemovePhoneHandHiddenListener(new InputManager.PhoneHandHiddenCallback(this.OnPhoneHandHidden));
        }
    }

    public override void NotifyOfGamePackOpened()
    {
        this.packOpened = true;
        if (this.freeCardsPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.freeCardsPopup);
        }
    }

    public override bool NotifyOfPlayError(PlayErrors.ErrorType error, Entity errorSource)
    {
        return ((error == PlayErrors.ErrorType.REQ_ATTACK_GREATER_THAN_0) && (errorSource.GetCardId() == "TU4a_006"));
    }

    public override void NotifyOfTargetModeCancelled()
    {
        if (this.crushThisGnoll != null)
        {
            NotificationManager.Get().DestroyAllPopUps();
            if ((this.firstRaptorCard != null) && (this.firstRaptorCard.GetZone() is ZonePlay))
            {
                this.ShowAttackWithYourMinionPopup();
            }
        }
    }

    private void NukeNumberLabels()
    {
        this.mousedOverCard = null;
        if (this.costLabel != null)
        {
            UnityEngine.Object.Destroy(this.costLabel);
        }
        if (this.attackLabel != null)
        {
            UnityEngine.Object.Destroy(this.attackLabel);
        }
        if (this.healthLabel != null)
        {
            UnityEngine.Object.Destroy(this.healthLabel);
        }
    }

    private void OnPhoneHandHidden(object userData)
    {
        this.HideFadeArrow();
        this.ShowHandBouncingArrow();
    }

    private void OnPhoneHandShown(object userData)
    {
        if (this.handBounceArrow != null)
        {
            NotificationManager.Get().DestroyNotification(this.handBounceArrow, 0f);
            this.handBounceArrow = null;
        }
        this.ShowHandFadeArrow();
    }

    private void PackLoadedCallback(string actorName, GameObject actorObject, object callbackData)
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.Misc_Tutorial01);
        Card heroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
        Card card2 = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
        this.startingPack = actorObject;
        Transform transform = SceneUtils.FindChildBySubstring(this.startingPack, "Hero_Dummy").transform;
        heroCard.transform.parent = transform;
        heroCard.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        heroCard.transform.localPosition = new Vector3(0f, 0f, 0f);
        SceneUtils.SetLayer(heroCard.GetActor().GetRootObject(), GameLayer.IgnoreFullScreenEffects);
        Transform transform2 = SceneUtils.FindChildBySubstring(this.startingPack, "HeroEnemy_Dummy").transform;
        card2.transform.parent = transform2;
        card2.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        card2.transform.localPosition = new Vector3(0f, 0f, 0f);
        heroCard.SetDoNotSort(true);
        Transform transform3 = Board.Get().FindBone("Tutorial1HeroStart");
        actorObject.transform.position = transform3.position;
        heroCard.GetActor().GetHealthObject().Hide();
        card2.GetActor().GetHealthObject().Hide();
        card2.GetActor().Hide();
        heroCard.GetActor().Hide();
        SceneMgr.Get().NotifySceneLoaded();
        Gameplay.Get().StartCoroutine(this.UpdatePresence());
        Gameplay.Get().StartCoroutine(this.ShowPackOpeningArrow(transform3.position));
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_TUTORIAL_01_ANNOUNCER_01");
        base.PreloadSound("VO_TUTORIAL_01_ANNOUNCER_02");
        base.PreloadSound("VO_TUTORIAL_01_ANNOUNCER_03");
        base.PreloadSound("VO_TUTORIAL_01_ANNOUNCER_04");
        base.PreloadSound("VO_TUTORIAL_01_ANNOUNCER_05");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_13_10");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_01_01");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_02_02");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_03_03");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_20_16");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_05_05");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_06_06");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_07_07");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_21_17");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_09_08");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_15_11");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_16_12");
        base.PreloadSound("VO_TUTORIAL_JAINA_02_55_ALT2");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_10_09");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_17_13");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_18_14");
        base.PreloadSound("VO_TUTORIAL_01_JAINA_19_15");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_01_01");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_02_02");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_03_03");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_04_04");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_06_06_ALT");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_08_08_ALT");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_09_09_ALT");
        base.PreloadSound("VO_TUTORIAL_01_HOGGER_11_11");
    }

    public override bool ShouldDoAlternateMulliganIntro()
    {
        return true;
    }

    private bool ShouldShowArrowOnCardInHand(Entity entity)
    {
        if (entity.GetZone() != TAG_ZONE.HAND)
        {
            return false;
        }
        int tag = base.GetTag(GAME_TAG.TURN);
        return ((tag == 2) || ((tag == 4) && (GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count == 0)));
    }

    public override bool ShouldShowBigCard()
    {
        return (base.GetTag(GAME_TAG.TURN) > 8);
    }

    public override bool ShouldShowHeroTooltips()
    {
        return true;
    }

    [DebuggerHidden]
    private IEnumerator ShowArrowInSeconds(float seconds)
    {
        return new <ShowArrowInSeconds>c__Iterator18C { seconds = seconds, <$>seconds = seconds, <>f__this = this };
    }

    private void ShowAttackTooltip(Card card)
    {
        SceneUtils.SetLayer(card.GetActor().GetAttackObject().gameObject, GameLayer.Tooltip);
        Vector3 position = card.transform.position;
        Vector3 attackTooltipPosition = (Vector3) this.m_attackTooltipPosition;
        Vector3 vector3 = new Vector3(position.x + attackTooltipPosition.x, position.y + attackTooltipPosition.y, position.z + attackTooltipPosition.z);
        this.attackHelpPanel = KeywordHelpPanelManager.Get().CreateKeywordPanel(0);
        this.attackHelpPanel.Reset();
        this.attackHelpPanel.SetScale((float) KeywordHelpPanel.GAMEPLAY_SCALE);
        this.attackHelpPanel.Initialize(GameStrings.Get("GLOBAL_ATTACK"), GameStrings.Get("TUTORIAL01_HELP_12"));
        this.attackHelpPanel.transform.position = vector3;
        RenderUtils.SetAlpha(this.attackHelpPanel.gameObject, 0f);
        object[] args = new object[] { "alpha", 1, "time", 0.25f };
        iTween.FadeTo(this.attackHelpPanel.gameObject, iTween.Hash(args));
        card.GetActor().GetAttackObject().Enlarge((float) this.m_gemScale);
    }

    private void ShowAttackWithYourMinionPopup()
    {
        if (this.attackWithYourMinion != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.attackWithYourMinion);
        }
        if (!this.firstAttackFinished && (this.firstMurlocCard != null))
        {
            this.firstMurlocCard.GetActor().ToggleForceIdle(false);
            this.firstMurlocCard.GetActor().SetActorState(ActorStateType.CARD_PLAYABLE);
            Vector3 position = this.firstMurlocCard.transform.position;
            if (!this.firstMurlocCard.GetEntity().IsExhausted() && (this.firstMurlocCard.GetZone() is ZonePlay))
            {
                Vector3 vector2;
                if ((this.firstRaptorCard != null) && (this.firstMurlocCard.GetZonePosition() < this.firstRaptorCard.GetZonePosition()))
                {
                    vector2 = new Vector3(position.x - 3f, position.y, position.z);
                    this.attackWithYourMinion = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, this.textToShowForAttackTip, true);
                    this.attackWithYourMinion.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                }
                else
                {
                    vector2 = new Vector3(position.x + 3f, position.y, position.z);
                    this.attackWithYourMinion = NotificationManager.Get().CreatePopupText(vector2, TutorialEntity.HELP_POPUP_SCALE, this.textToShowForAttackTip, true);
                    this.attackWithYourMinion.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                }
                Card firstCard = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetFirstCard();
                this.ShowFadeArrow(this.firstMurlocCard, firstCard);
                Gameplay.Get().StartCoroutine(this.SwapHelpTextAndFlashMinion());
            }
        }
    }

    private void ShowDontFireballYourselfPopup(Vector3 origin)
    {
        if (this.noFireballPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.noFireballPopup);
        }
        Vector3 position = new Vector3(origin.x - 3f, origin.y, origin.z);
        this.noFireballPopup = NotificationManager.Get().CreatePopupText(position, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_07"), true);
        NotificationManager.Get().DestroyNotification(this.noFireballPopup, 2.5f);
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

    private void ShowFadeArrow(Card card, Card target = null)
    {
        if (this.handFadeArrow == null)
        {
            Vector3 vector2;
            Vector3 position = card.transform.position;
            Vector3 rotation = new Vector3(0f, 180f, 0f);
            if (target != null)
            {
                Vector3 vector4 = target.transform.position - position;
                vector2 = new Vector3(position.x, position.y + 0.47f, position.z + 0.27f);
                float num = Vector3.Angle(target.transform.position - vector2, new Vector3(0f, 0f, -1f));
                rotation = new Vector3(0f, -Mathf.Sign(vector4.x) * num, 0f);
                vector2 += (Vector3) (0.3f * vector4);
            }
            else
            {
                vector2 = new Vector3(position.x, position.y + 0.047f, position.z + 0.95f);
            }
            UnityEngine.Debug.Log("fade arrow rotation is " + rotation);
            this.handFadeArrow = NotificationManager.Get().CreateFadeArrow(vector2, rotation);
            if (target != null)
            {
                this.handFadeArrow.transform.localScale = (Vector3) (1.25f * Vector3.one);
            }
            this.handFadeArrow.transform.parent = card.transform;
        }
    }

    private void ShowHandBouncingArrow()
    {
        if (this.handBounceArrow == null)
        {
            List<Card> cards = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
            if (cards.Count != 0)
            {
                Vector3 vector2;
                Card card = cards[0];
                Vector3 position = card.transform.position;
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    vector2 = new Vector3(position.x - 0.08f, position.y + 0.2f, position.z + 1.2f);
                }
                else
                {
                    vector2 = new Vector3(position.x, position.y, position.z + 2f);
                }
                this.handBounceArrow = NotificationManager.Get().CreateBouncingArrow(vector2, new Vector3(0f, 0f, 0f));
                this.handBounceArrow.transform.parent = card.transform;
            }
        }
    }

    private void ShowHandFadeArrow()
    {
        List<Card> cards = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
        if (cards.Count != 0)
        {
            this.ShowFadeArrow(cards[0], null);
        }
    }

    private void ShowHealthTooltip(Card card)
    {
        SceneUtils.SetLayer(card.GetActor().GetHealthObject().gameObject, GameLayer.Tooltip);
        Vector3 position = card.transform.position;
        Vector3 healthTooltipPosition = (Vector3) this.m_healthTooltipPosition;
        if (card.GetEntity().IsHero())
        {
            healthTooltipPosition = (Vector3) this.m_heroHealthTooltipPosition;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                if (!card.GetEntity().IsControlledByLocalUser())
                {
                    healthTooltipPosition.z -= 0.75f;
                }
                else if (Localization.GetLocale() == Locale.ruRU)
                {
                    healthTooltipPosition.z++;
                }
            }
        }
        Vector3 vector3 = new Vector3(position.x + healthTooltipPosition.x, position.y + healthTooltipPosition.y, position.z + healthTooltipPosition.z);
        this.healthHelpPanel = KeywordHelpPanelManager.Get().CreateKeywordPanel(0);
        this.healthHelpPanel.Reset();
        this.healthHelpPanel.SetScale((float) KeywordHelpPanel.GAMEPLAY_SCALE);
        this.healthHelpPanel.Initialize(GameStrings.Get("GLOBAL_HEALTH"), GameStrings.Get("TUTORIAL01_HELP_13"));
        this.healthHelpPanel.transform.position = vector3;
        RenderUtils.SetAlpha(this.healthHelpPanel.gameObject, 0f);
        object[] args = new object[] { "alpha", 1, "time", 0.25f };
        iTween.FadeTo(this.healthHelpPanel.gameObject, iTween.Hash(args));
        card.GetActor().GetHealthObject().Enlarge((float) this.m_gemScale);
    }

    [DebuggerHidden]
    private IEnumerator ShowHealthTooltipAfterWait(Card card)
    {
        return new <ShowHealthTooltipAfterWait>c__Iterator18B { card = card, <$>card = card, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ShowPackOpeningArrow(Vector3 packSpot)
    {
        return new <ShowPackOpeningArrow>c__Iterator194 { packSpot = packSpot, <$>packSpot = packSpot, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SwapHelpTextAndFlashMinion()
    {
        return new <SwapHelpTextAndFlashMinion>c__Iterator18F { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UpdatePresence()
    {
        return new <UpdatePresence>c__Iterator193();
    }

    [DebuggerHidden]
    private IEnumerator WaitAndThenHide(int numTimesStarted)
    {
        return new <WaitAndThenHide>c__Iterator18A { numTimesStarted = numTimesStarted, <$>numTimesStarted = numTimesStarted, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForBoardLoadedAndStartGame()
    {
        return new <WaitForBoardLoadedAndStartGame>c__Iterator192();
    }

    [CompilerGenerated]
    private sealed class <BeginFlashingMinionLoop>c__Iterator191 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>minionToFlash;
        internal Tutorial_01 <>f__this;
        internal Card minionToFlash;

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
                    if (this.minionToFlash != null)
                    {
                        if ((!this.minionToFlash.GetEntity().IsExhausted() && (this.minionToFlash.GetActor().GetActorStateType() != ActorStateType.CARD_IDLE)) && (this.minionToFlash.GetActor().GetActorStateType() != ActorStateType.CARD_MOUSE_OVER))
                        {
                            this.minionToFlash.GetActorSpell(SpellType.WIGGLE).Activate();
                            this.$current = new WaitForSeconds(1.5f);
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    break;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.BeginFlashingMinionLoop(this.minionToFlash));
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

    [CompilerGenerated]
    private sealed class <ContinueFinishingCustomIntro>c__Iterator195 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_01 <>f__this;

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
                    this.$current = new WaitForSeconds(3f);
                    this.$PC = 1;
                    return true;

                case 1:
                    UnityEngine.Object.Destroy(this.<>f__this.startingPack);
                    GameState.Get().SetBusy(false);
                    MulliganManager.Get().SkipMulligan();
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

    [CompilerGenerated]
    private sealed class <FlashMinionUntilAttackBegins>c__Iterator190 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>minionToFlash;
        internal Tutorial_01 <>f__this;
        internal Card minionToFlash;

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
                    this.$current = new WaitForSeconds(8f);
                    this.$PC = 1;
                    return true;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.BeginFlashingMinionLoop(this.minionToFlash));
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

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator18E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal Tutorial_01 <>f__this;
        internal Notification <battlebegin>__17;
        internal Notification.PopUpArrowDirection <direction>__6;
        internal Collider <dragPlane>__16;
        internal List<Card> <enemyCards>__14;
        internal Vector3 <healthPopUpPosition>__5;
        internal Vector3 <help04Position>__9;
        internal Actor <hoggerActor>__1;
        internal Notification.PopUpArrowDirection <hoggerHealthPopupDirection>__13;
        internal Vector3 <hoggerHealthPopupPosition>__12;
        internal Vector3 <hoggerPosition>__11;
        internal Notification <innkeeperLine>__20;
        internal Actor <jainaActor>__0;
        internal Vector3 <jainaPos>__4;
        internal Card <lastCard>__15;
        internal Vector3 <middleSpot>__19;
        internal Vector3 <northHero>__18;
        internal Notification <notification>__7;
        internal AudioSource <prevLine>__3;
        internal Notification <raptorHelp>__10;
        internal Vector3 <raptorPosition>__8;
        internal int <turn>__2;
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
                    this.<jainaActor>__0 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    this.<hoggerActor>__1 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    missionEvent = this.missionEvent;
                    switch (missionEvent)
                    {
                        case 1:
                            Gameplay.Get().StartCoroutine(this.<>f__this.WaitForBoardLoadedAndStartGame());
                            goto Label_104B;

                        case 2:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_01_01", "TUTORIAL01_JAINA_01", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            Gameplay.Get().SetGameStateBusy(false, 2.2f);
                            goto Label_104B;

                        case 3:
                            this.<turn>__2 = GameState.Get().GetTurn();
                            this.$current = new WaitForSeconds(2f);
                            this.$PC = 1;
                            goto Label_1054;

                        case 4:
                            GameState.Get().SetBusy(true);
                            this.<prevLine>__3 = this.<>f__this.GetPreloadedSound("VO_TUTORIAL_01_JAINA_03_03");
                            goto Label_0295;

                        case 5:
                            this.<>f__this.HideFadeArrow();
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_05_05", "TUTORIAL01_JAINA_05", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            goto Label_104B;

                        case 6:
                        case 9:
                        case 10:
                        case 0x15:
                            goto Label_104B;

                        case 7:
                            NotificationManager.Get().DestroyAllPopUps();
                            this.$current = new WaitForSeconds(1.7f);
                            this.$PC = 5;
                            goto Label_1054;

                        case 8:
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_03_03", "TUTORIAL01_HOGGER_05", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
                            this.$PC = 8;
                            goto Label_1054;

                        case 12:
                            this.$current = new WaitForSeconds(1f);
                            this.$PC = 10;
                            goto Label_1054;

                        case 13:
                            goto Label_07F0;

                        case 14:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_08_08_ALT", "TUTORIAL01_HOGGER_08", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
                            this.<hoggerPosition>__11 = this.<hoggerActor>__1.transform.position;
                            this.<hoggerHealthPopupPosition>__12 = new Vector3(this.<hoggerPosition>__11.x + 3.3f, this.<hoggerPosition>__11.y + 0.5f, this.<hoggerPosition>__11.z - 1f);
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                this.<hoggerHealthPopupPosition>__12 = new Vector3(this.<hoggerPosition>__11.x + 3f, this.<hoggerPosition>__11.y + 0.5f, this.<hoggerPosition>__11.z - 0.75f);
                            }
                            this.<hoggerHealthPopupDirection>__13 = Notification.PopUpArrowDirection.Left;
                            this.<notification>__7 = NotificationManager.Get().CreatePopupText(this.<hoggerHealthPopupPosition>__12, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_09"), true);
                            this.<notification>__7.ShowPopUpArrow(this.<hoggerHealthPopupDirection>__13);
                            NotificationManager.Get().DestroyNotification(this.<notification>__7, 5f);
                            if ((this.<>f__this.GetTag(GAME_TAG.TURN) != 6) || !EndTurnButton.Get().IsInNMPState())
                            {
                                goto Label_104B;
                            }
                            this.$current = new WaitForSeconds(9f);
                            this.$PC = 12;
                            goto Label_1054;

                        case 15:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_JAINA_02_55_ALT2", string.Empty, Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            goto Label_104B;

                        case 20:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_10_09", "TUTORIAL01_JAINA_10", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            this.$current = new WaitForSeconds(1.5f);
                            this.$PC = 13;
                            goto Label_1054;

                        case 0x16:
                            GameState.Get().SetBusy(true);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_09_09_ALT", "TUTORIAL01_HOGGER_02", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
                            Gameplay.Get().SetGameStateBusy(false, 2f);
                            goto Label_104B;
                    }
                    break;

                case 1:
                    if (this.<turn>__2 == GameState.Get().GetTurn())
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_03_03", "TUTORIAL01_JAINA_03", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                        if ((this.<>f__this.GetTag(GAME_TAG.TURN) == 2) && !EndTurnButton.Get().IsInWaitingState())
                        {
                            this.<>f__this.ShowEndTurnBouncingArrow();
                        }
                        goto Label_104B;
                    }
                    goto Label_1052;

                case 2:
                    goto Label_0295;

                case 3:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_06_06_ALT", "TUTORIAL01_HOGGER_07", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
                    this.$PC = 4;
                    goto Label_1054;

                case 4:
                    this.<jainaPos>__4 = this.<jainaActor>__0.transform.position;
                    this.<healthPopUpPosition>__5 = new Vector3(this.<jainaPos>__4.x + 3.3f, this.<jainaPos>__4.y + 0.5f, this.<jainaPos>__4.z - 0.85f);
                    this.<direction>__6 = Notification.PopUpArrowDirection.Left;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<healthPopUpPosition>__5 = new Vector3(this.<jainaPos>__4.x + 3f, this.<jainaPos>__4.y + 0.5f, this.<jainaPos>__4.z + 0.85f);
                        this.<direction>__6 = Notification.PopUpArrowDirection.LeftDown;
                    }
                    this.<notification>__7 = NotificationManager.Get().CreatePopupText(this.<healthPopUpPosition>__5, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_01"), true);
                    this.<notification>__7.ShowPopUpArrow(this.<direction>__6);
                    NotificationManager.Get().DestroyNotification(this.<notification>__7, 5f);
                    Gameplay.Get().SetGameStateBusy(false, 5.2f);
                    goto Label_104B;

                case 5:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_07_07", "TUTORIAL01_JAINA_07", Notification.SpeechBubbleDirection.BottomRight, this.<jainaActor>__0, 1f, true, false, 3f));
                    if (this.<>f__this.firstRaptorCard != null)
                    {
                        this.<raptorPosition>__8 = this.<>f__this.firstRaptorCard.transform.position;
                        if ((this.<>f__this.firstMurlocCard == null) || (this.<>f__this.firstRaptorCard.GetZonePosition() <= this.<>f__this.firstMurlocCard.GetZonePosition()))
                        {
                            this.<help04Position>__9 = new Vector3(this.<raptorPosition>__8.x - 3f, this.<raptorPosition>__8.y, this.<raptorPosition>__8.z);
                            this.<raptorHelp>__10 = NotificationManager.Get().CreatePopupText(this.<help04Position>__9, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_04"), true);
                            this.<raptorHelp>__10.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                        }
                        else
                        {
                            this.<help04Position>__9 = new Vector3(this.<raptorPosition>__8.x + 3f, this.<raptorPosition>__8.y, this.<raptorPosition>__8.z);
                            this.<raptorHelp>__10 = NotificationManager.Get().CreatePopupText(this.<help04Position>__9, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_04"), true);
                            this.<raptorHelp>__10.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                        }
                        NotificationManager.Get().DestroyNotification(this.<raptorHelp>__10, 4f);
                    }
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 6;
                    goto Label_1054;

                case 6:
                    if ((GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count > 1) && !GameState.Get().IsInTargetMode())
                    {
                        this.<>f__this.ShowAttackWithYourMinionPopup();
                    }
                    if ((this.<>f__this.GetTag(GAME_TAG.TURN) != 4) || !EndTurnButton.Get().IsInNMPState())
                    {
                        goto Label_104B;
                    }
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 7;
                    goto Label_1054;

                case 7:
                    this.<>f__this.ShowEndTurnBouncingArrow();
                    goto Label_104B;

                case 8:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_21_17", "TUTORIAL01_JAINA_21", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                    this.$PC = 9;
                    goto Label_1054;

                case 9:
                    GameState.Get().SetBusy(false);
                    goto Label_104B;

                case 10:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_15_11", "TUTORIAL01_JAINA_15", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                    goto Label_104B;

                case 11:
                    goto Label_07F0;

                case 12:
                    this.<>f__this.ShowEndTurnBouncingArrow();
                    goto Label_104B;

                case 13:
                    GameState.Get().SetBusy(false);
                    this.<enemyCards>__14 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards();
                    this.<lastCard>__15 = this.<enemyCards>__14[this.<enemyCards>__14.Count - 1];
                    this.<lastCard>__15.GetActor().GetAttackObject().Jiggle();
                    goto Label_104B;

                case 14:
                    goto Label_0B3C;

                case 15:
                    NotificationManager.Get().DestroyNotification(this.<battlebegin>__17, 0f);
                    goto Label_0C29;

                case 0x10:
                    goto Label_0C29;

                case 0x11:
                    GameState.Get().SetBusy(false);
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 0x12;
                    goto Label_1054;

                case 0x12:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_04_04", "TUTORIAL01_HOGGER_06", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
                    goto Label_104B;

                case 0x13:
                    NotificationManager.Get().DestroyNotification(this.<innkeeperLine>__20, 0f);
                    goto Label_0DD8;

                case 20:
                    goto Label_0DD8;

                case 0x15:
                    if (SoundUtils.CanDetectVolume())
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_02", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 0x17;
                    }
                    else
                    {
                        this.<innkeeperLine>__20 = NotificationManager.Get().CreateInnkeeperQuote(this.<middleSpot>__19, GameStrings.Get("VO_TUTORIAL_01_ANNOUNCER_02"), string.Empty, 15f, null);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_02", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 0x16;
                    }
                    goto Label_1054;

                case 0x16:
                    NotificationManager.Get().DestroyNotification(this.<innkeeperLine>__20, 0f);
                    goto Label_0EC4;

                case 0x17:
                    goto Label_0EC4;

                case 0x18:
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        Gameplay.Get().AddGamePlayNameBannerPhone();
                    }
                    if (!SoundUtils.CanDetectVolume())
                    {
                        this.<innkeeperLine>__20 = NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_TUTORIAL_01_ANNOUNCER_03"), string.Empty, 15f, null);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_03", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 0x19;
                    }
                    else
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_03", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 0x1a;
                    }
                    goto Label_1054;

                case 0x19:
                    NotificationManager.Get().DestroyNotification(this.<innkeeperLine>__20, 0f);
                    goto Label_0FEB;

                case 0x1a:
                    goto Label_0FEB;

                case 0x1b:
                    this.<>f__this.announcerIsFinishedYapping = true;
                    goto Label_104B;

                default:
                    goto Label_1052;
            }
            switch (missionEvent)
            {
                case 0x37:
                    this.<>f__this.tooltipsDisabled = false;
                    this.<dragPlane>__16 = Board.Get().FindCollider("DragPlane");
                    this.<dragPlane>__16.enabled = true;
                    goto Label_0B3C;

                case 0x42:
                    this.<northHero>__18 = new Vector3(136f, NotificationManager.DEPTH, 131f);
                    this.<middleSpot>__19 = new Vector3(136f, NotificationManager.DEPTH, 80f);
                    this.<innkeeperLine>__20 = null;
                    if (!SoundUtils.CanDetectVolume())
                    {
                        this.<innkeeperLine>__20 = NotificationManager.Get().CreateInnkeeperQuote(this.<northHero>__18, GameStrings.Get("VO_TUTORIAL_01_ANNOUNCER_01"), string.Empty, 15f, null);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_01", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 0x13;
                    }
                    else
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_01", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 20;
                    }
                    goto Label_1054;

                default:
                    UnityEngine.Debug.LogWarning("WARNING - Mission fired an event that we are not listening for.");
                    goto Label_104B;
            }
        Label_0295:
            if (SoundManager.Get().IsPlaying(this.<prevLine>__3))
            {
                this.$current = null;
                this.$PC = 2;
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_20_16", "TUTORIAL01_JAINA_20", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                this.$PC = 3;
            }
            goto Label_1054;
        Label_07F0:
            while (this.<>f__this.m_jainaSpeaking)
            {
                this.$current = null;
                this.$PC = 11;
                goto Label_1054;
            }
            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_16_12", "TUTORIAL01_JAINA_16", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
            goto Label_104B;
        Label_0B3C:
            while (!this.<>f__this.announcerIsFinishedYapping)
            {
                this.$current = null;
                this.$PC = 14;
                goto Label_1054;
            }
            if (!SoundUtils.CanDetectVolume())
            {
                this.<battlebegin>__17 = NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 84.8f), GameStrings.Get("VO_TUTORIAL_01_ANNOUNCER_05"), string.Empty, 15f, null);
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_05", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                this.$PC = 15;
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_05", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                this.$PC = 0x10;
            }
            goto Label_1054;
        Label_0C29:
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_01_01", "TUTORIAL01_HOGGER_01", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
            this.$PC = 0x11;
            goto Label_1054;
        Label_0DD8:
            this.$current = new WaitForSeconds(0.5f);
            this.$PC = 0x15;
            goto Label_1054;
        Label_0EC4:
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_HOGGER_02_02", "TUTORIAL01_HOGGER_04", Notification.SpeechBubbleDirection.TopRight, this.<hoggerActor>__1, 1f, true, false, 3f));
            this.$PC = 0x18;
            goto Label_1054;
        Label_0FEB:
            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_ANNOUNCER_04", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
            this.$PC = 0x1b;
            goto Label_1054;
        Label_104B:
            this.$PC = -1;
        Label_1052:
            return false;
        Label_1054:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator18D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal Tutorial_01 <>f__this;
        internal GameObject <actorObject>__4;
        internal List<Card> <cardsInDeck>__2;
        internal List<Card> <cardsInHand>__6;
        internal Collider <dragPlane>__3;
        internal Actor <hoggerActor>__1;
        internal Actor <jainaActor>__0;
        internal UberText <wantedText>__5;
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
                    this.<jainaActor>__0 = GameState.Get().GetFriendlySidePlayer().GetHero().GetCard().GetActor();
                    this.<hoggerActor>__1 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    switch (this.turn)
                    {
                        case 1:
                            this.<cardsInDeck>__2 = GameState.Get().GetFriendlySidePlayer().GetDeckZone().GetCards();
                            this.<>f__this.firstMurlocCard = this.<cardsInDeck>__2[this.<cardsInDeck>__2.Count - 1];
                            this.<>f__this.firstRaptorCard = this.<cardsInDeck>__2[this.<cardsInDeck>__2.Count - 2];
                            GameState.Get().SetBusy(true);
                            this.<dragPlane>__3 = Board.Get().FindCollider("DragPlane");
                            this.<dragPlane>__3.enabled = false;
                            this.$current = new WaitForSeconds(1.25f);
                            this.$PC = 1;
                            goto Label_0573;

                        case 2:
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                InputManager.Get().RegisterPhoneHandShownListener(new InputManager.PhoneHandShownCallback(this.<>f__this.OnPhoneHandShown));
                                InputManager.Get().RegisterPhoneHandHiddenListener(new InputManager.PhoneHandHiddenCallback(this.<>f__this.OnPhoneHandHidden));
                            }
                            this.$current = new WaitForSeconds(1f);
                            this.$PC = 2;
                            goto Label_0573;

                        case 3:
                            if (UniversalInputManager.UsePhoneUI != null)
                            {
                                InputManager.Get().RemovePhoneHandShownListener(new InputManager.PhoneHandShownCallback(this.<>f__this.OnPhoneHandShown));
                                InputManager.Get().RemovePhoneHandHiddenListener(new InputManager.PhoneHandHiddenCallback(this.<>f__this.OnPhoneHandHidden));
                            }
                            goto Label_056A;

                        case 4:
                            this.<hoggerActor>__1.TurnOffCollider();
                            this.<hoggerActor>__1.SetActorState(ActorStateType.CARD_IDLE);
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_06_06", "TUTORIAL01_JAINA_06", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            if (this.<>f__this.firstMurlocCard != null)
                            {
                                this.<>f__this.firstMurlocCard.GetActor().ToggleForceIdle(true);
                                this.<>f__this.firstMurlocCard.GetActor().SetActorState(ActorStateType.CARD_IDLE);
                            }
                            goto Label_056A;

                        case 5:
                            this.<hoggerActor>__1.TurnOnCollider();
                            goto Label_056A;

                        case 6:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_17_13", "TUTORIAL01_JAINA_17", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            goto Label_056A;

                        case 8:
                            this.<>f__this.m_jainaSpeaking = true;
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_18_14", "TUTORIAL01_JAINA_18", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            this.$PC = 3;
                            goto Label_0573;

                        case 10:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_19_15", "TUTORIAL01_JAINA_19", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                            goto Label_056A;
                    }
                    break;

                case 1:
                    this.<actorObject>__4 = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false);
                    this.<>f__this.startingPopup = this.<actorObject>__4.GetComponent<Notification>();
                    NotificationManager.Get().CreatePopupDialogFromObject(this.<>f__this.startingPopup, GameStrings.Get("TUTORIAL01_HELP_14"), GameStrings.Get("TUTORIAL01_HELP_15"), GameStrings.Get("TUTORIAL01_HELP_16"));
                    this.<>f__this.startingPopup.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.UserPressedStartButton));
                    this.<>f__this.startingPopup.artOverlay.material.mainTextureOffset = new Vector2(0f, 0f);
                    this.<wantedText>__5 = this.<>f__this.startingPopup.transform.FindChild("WantedText").GetComponent<UberText>();
                    this.<wantedText>__5.Text = GameStrings.Get("MISSION_PRE_TUTORIAL_WANTED");
                    this.<wantedText>__5.gameObject.SetActive(true);
                    break;

                case 2:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_TUTORIAL_01_JAINA_02_02", "TUTORIAL01_JAINA_02", Notification.SpeechBubbleDirection.BottomLeft, this.<jainaActor>__0, 1f, true, false, 3f));
                    this.<cardsInHand>__6 = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
                    if (((this.<>f__this.GetTag(GAME_TAG.TURN) == 2) && (this.<cardsInHand>__6.Count == 1)) && ((InputManager.Get().GetHeldCard() == null) && !this.<cardsInHand>__6[0].IsMousedOver()))
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.ShowArrowInSeconds(0f));
                    }
                    break;

                case 3:
                    this.<>f__this.m_jainaSpeaking = false;
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 4;
                    goto Label_0573;

                case 4:
                    Gameplay.Get().StartCoroutine(this.<>f__this.FlashMinionUntilAttackBegins(this.<>f__this.firstRaptorCard));
                    break;

                default:
                    goto Label_0571;
            }
        Label_056A:
            this.$PC = -1;
        Label_0571:
            return false;
        Label_0573:
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
    private sealed class <ShowArrowInSeconds>c__Iterator18C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal Tutorial_01 <>f__this;
        internal Card <cardInHand>__1;
        internal List<Card> <handCards>__0;
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
                    goto Label_00F9;

                case 1:
                    this.<handCards>__0 = GameState.Get().GetFriendlySidePlayer().GetHandZone().GetCards();
                    if (this.<handCards>__0.Count != 0)
                    {
                        this.<cardInHand>__1 = this.<handCards>__0[0];
                        break;
                    }
                    goto Label_00F7;

                case 2:
                    break;

                default:
                    goto Label_00F7;
            }
            while (iTween.Count(this.<cardInHand>__1.gameObject) > 0)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00F9;
            }
            if (!this.<cardInHand>__1.IsMousedOver() && (InputManager.Get().GetHeldCard() != this.<cardInHand>__1))
            {
                this.<>f__this.ShowHandBouncingArrow();
                this.$PC = -1;
            }
        Label_00F7:
            return false;
        Label_00F9:
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
    private sealed class <ShowHealthTooltipAfterWait>c__Iterator18B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal Tutorial_01 <>f__this;
        internal Card card;

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
                    this.$current = new WaitForSeconds(0.05f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (InputManager.Get().GetMousedOverCard() == this.card)
                    {
                        this.<>f__this.ShowHealthTooltip(this.card);
                        this.$PC = -1;
                        break;
                    }
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

    [CompilerGenerated]
    private sealed class <ShowPackOpeningArrow>c__Iterator194 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Vector3 <$>packSpot;
        internal Tutorial_01 <>f__this;
        internal Vector3 <popUpPosition>__0;
        internal Vector3 packSpot;

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
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!this.<>f__this.packOpened)
                    {
                        this.<popUpPosition>__0 = new Vector3(this.packSpot.x + 4.014574f, this.packSpot.y, this.packSpot.z + 0.2307634f);
                        this.<>f__this.freeCardsPopup = NotificationManager.Get().CreatePopupText(this.<popUpPosition>__0, TutorialEntity.HELP_POPUP_SCALE, GameStrings.Get("TUTORIAL01_HELP_18"), true);
                        this.<>f__this.freeCardsPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                        this.$PC = -1;
                        break;
                    }
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

    [CompilerGenerated]
    private sealed class <SwapHelpTextAndFlashMinion>c__Iterator18F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Tutorial_01 <>f__this;
        internal Vector3 <cardInBattlefieldPosition>__0;
        internal Vector3 <help02Position>__1;

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
                    if (this.<>f__this.firstMurlocCard != null)
                    {
                        Gameplay.Get().StartCoroutine(this.<>f__this.BeginFlashingMinionLoop(this.<>f__this.firstMurlocCard));
                        this.$current = new WaitForSeconds(4f);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    if (!(this.<>f__this.textToShowForAttackTip == GameStrings.Get("TUTORIAL01_HELP_10")))
                    {
                        if (((!this.<>f__this.firstMurlocCard.GetEntity().IsExhausted() && (this.<>f__this.firstMurlocCard.GetActor().GetActorStateType() != ActorStateType.CARD_IDLE)) && ((this.<>f__this.firstMurlocCard.GetActor().GetActorStateType() != ActorStateType.CARD_MOUSE_OVER) && (this.<>f__this.firstMurlocCard.GetZone() is ZonePlay))) && !this.<>f__this.firstAttackFinished)
                        {
                            this.<cardInBattlefieldPosition>__0 = this.<>f__this.firstMurlocCard.transform.position;
                            this.<>f__this.textToShowForAttackTip = GameStrings.Get("TUTORIAL01_HELP_10");
                            if (this.<>f__this.attackWithYourMinion != null)
                            {
                                NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.<>f__this.attackWithYourMinion);
                            }
                            if ((this.<>f__this.firstRaptorCard != null) && (this.<>f__this.firstMurlocCard.GetZonePosition() < this.<>f__this.firstRaptorCard.GetZonePosition()))
                            {
                                this.<help02Position>__1 = new Vector3(this.<cardInBattlefieldPosition>__0.x - 3f, this.<cardInBattlefieldPosition>__0.y, this.<cardInBattlefieldPosition>__0.z);
                                this.<>f__this.attackWithYourMinion = NotificationManager.Get().CreatePopupText(this.<help02Position>__1, TutorialEntity.HELP_POPUP_SCALE, this.<>f__this.textToShowForAttackTip, true);
                                this.<>f__this.attackWithYourMinion.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
                            }
                            else
                            {
                                this.<help02Position>__1 = new Vector3(this.<cardInBattlefieldPosition>__0.x + 3f, this.<cardInBattlefieldPosition>__0.y, this.<cardInBattlefieldPosition>__0.z);
                                this.<>f__this.attackWithYourMinion = NotificationManager.Get().CreatePopupText(this.<help02Position>__1, TutorialEntity.HELP_POPUP_SCALE, this.<>f__this.textToShowForAttackTip, true);
                                this.<>f__this.attackWithYourMinion.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
                            }
                            this.$PC = -1;
                        }
                        break;
                    }
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

    [CompilerGenerated]
    private sealed class <UpdatePresence>c__Iterator193 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                case 1:
                    if (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    GameMgr.Get().UpdatePresence();
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

    [CompilerGenerated]
    private sealed class <WaitAndThenHide>c__Iterator18A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>numTimesStarted;
        internal Tutorial_01 <>f__this;
        internal Card <firstCard>__0;
        internal int numTimesStarted;

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
                    this.$current = new WaitForSeconds(6f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.crushThisGnoll != null)
                    {
                        if (this.numTimesStarted == this.<>f__this.numTimesTextSwapStarted)
                        {
                            this.<firstCard>__0 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetFirstCard();
                            if (this.<firstCard>__0 != null)
                            {
                                NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.<>f__this.crushThisGnoll);
                                this.$PC = -1;
                            }
                        }
                        break;
                    }
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

    [CompilerGenerated]
    private sealed class <WaitForBoardLoadedAndStartGame>c__Iterator192 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                case 1:
                    if (ZoneMgr.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    GameState.Get().SetBusy(true);
                    HistoryManager.Get().DisableHistory();
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

