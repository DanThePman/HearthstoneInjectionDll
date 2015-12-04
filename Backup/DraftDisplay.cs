using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class DraftDisplay : MonoBehaviour
{
    private static readonly Vector3 CHOICE_ACTOR_LOCAL_SCALE = new Vector3(7.2f, 7.2f, 7.2f);
    private static readonly Vector3 CHOICE_ACTOR_LOCAL_SCALE_PHONE = new Vector3(14.5f, 14.5f, 14.5f);
    private static readonly Vector3 HERO_ACTOR_LOCAL_SCALE = new Vector3(8.285825f, 8.285825f, 8.285825f);
    private static readonly Vector3 HERO_ACTOR_LOCAL_SCALE_PHONE = new Vector3(15.5f, 15.5f, 15.5f);
    private static readonly Vector3 HERO_CONFIRM_BUTTON_POSITION = new Vector3(0.03024703f, 107.4205f, -6.346496f);
    private static readonly Vector3 HERO_CONFIRM_BUTTON_POSITION_PHONE = new Vector3(-4.27f, 113f, -6f);
    private static readonly Vector3 HERO_CONFIRM_BUTTON_SCALE = new Vector3(3.28f, 3.28f, 3.28f);
    private static readonly Vector3 HERO_CONFIRM_BUTTON_SCALE_PHONE = new Vector3(4f, 4f, 4f);
    private static readonly Vector3 HERO_LABEL_SCALE = new Vector3(8f, 8f, 8f);
    private static readonly Vector3 HERO_LABEL_SCALE_PHONE = new Vector3(15f, 15f, 15f);
    private static readonly Vector3 HERO_POWER_POSITION = new Vector3(1.40873f, 0f, -0.3410472f);
    private static readonly Vector3 HERO_POWER_POSITION_PHONE = new Vector3(1.07f, 0.3f, -0.15f);
    private static readonly Vector3 HERO_POWER_SCALE = new Vector3(0.3419997f, 0.3419997f, 0.3419997f);
    private static readonly Vector3 HERO_POWER_SCALE_PHONE = new Vector3(0.5f, 0.5f, 0.5f);
    private static readonly Vector3 HERO_POWER_START_POSITION = new Vector3(0f, 0f, -0.3410472f);
    private static readonly Vector3 HERO_POWER_START_POSITION_PHONE = new Vector3(1.6f, 0.3f, -0.15f);
    private bool m_animationsComplete = true;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_backButton;
    [CustomEditField(Sections="Bones")]
    public Transform m_bigHeroBone;
    private List<DraftChoice> m_choices = new List<DraftChoice>();
    private Actor m_chosenHero;
    private NormalButton m_confirmButton;
    private List<HeroLabel> m_currentLabels = new List<HeroLabel>();
    private DraftMode m_currentMode;
    public float m_DeckCardBarFlareUpDelay;
    public Spell m_DeckCompleteSpell;
    public DraftDeckTray m_draftDeckTray;
    private bool m_firstTimeIntroComplete;
    public UberText m_forgeLabel;
    public PegUIElement m_heroClickCatcher;
    private CardSoundSpell[] m_heroEmotes = new CardSoundSpell[3];
    public GameObject m_heroLabel;
    private Actor m_heroPower;
    private FullDef[] m_heroPowerDefs = new FullDef[3];
    public UberText m_instructionText;
    private bool m_isHeroAnimating;
    public DraftManaCurve m_manaCurve;
    private bool m_netCacheReady;
    private List<Achievement> m_newlyCompletedAchieves = new List<Achievement>();
    public GameObject m_Phone3WayButtonRoot;
    public Transform m_PhoneBackButtonBone;
    public GameObject m_PhoneChooseHero;
    public ArenaPhoneControl m_PhoneDeckControl;
    public Transform m_PhoneDeckTrayHiddenBone;
    public GameObject m_PhoneLargeViewDeckButton;
    [CustomEditField(Sections="Phone")]
    public GameObject m_PhonePlayButtonTray;
    public Collider m_pickArea;
    public PlayButton m_playButton;
    private bool m_questsHandled;
    public StandardPegButtonNew m_retireButton;
    private bool m_skipHeroEmotes;
    public Transform m_socketHeroBone;
    private bool m_wasDrafting;
    private DraftCardVisual m_zoomedHero;
    private static DraftDisplay s_instance;

    public void AcceptNewChoices(List<NetCache.CardDefinition> choices)
    {
        this.DestroyOldChoices();
        this.UpdateInstructionText();
        base.StartCoroutine(this.WaitForAnimsToFinishAndThenDisplayNewChoices(choices));
    }

    public void AddCardToManaCurve(EntityDef entityDef)
    {
        if (entityDef == null)
        {
            UnityEngine.Debug.LogWarning("DraftDisplay.AddCardToManaCurve() - entityDef is null");
        }
        else if (this.m_manaCurve == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.AddCardToManaCurve({0}) - m_manaCurve is null", entityDef));
        }
        else
        {
            this.m_manaCurve.AddCardOfCost(entityDef.GetCost());
        }
    }

    private void Awake()
    {
        s_instance = this;
        AssetLoader.Get().LoadActor("DraftHeroChooseButton", new AssetLoader.GameObjectCallback(this.OnConfirmButtonLoaded), null, false);
        AssetLoader.Get().LoadActor("History_HeroPower", new AssetLoader.GameObjectCallback(this.LoadHeroPowerCallback), null, false);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            AssetLoader.Get().LoadGameObject("BackButton_phone", new AssetLoader.GameObjectCallback(this.OnPhoneBackButtonLoaded), null, false);
        }
        DraftManager.Get().RegisterNetHandlers();
        DraftManager.Get().RegisterMatchmakerHandlers();
        DraftManager.Get().RegisterStoreHandlers();
        this.m_forgeLabel.Text = GameStrings.Get("GLUE_TOOLTIP_BUTTON_FORGE_HEADLINE");
        this.m_instructionText.Text = string.Empty;
        this.m_pickArea.enabled = false;
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, false);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, false);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE, true);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, true);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, false);
        }
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    private void BackButtonPress(UIEvent e)
    {
        Navigation.GoBack();
    }

    private bool CanAutoDraft()
    {
        if (!ApplicationMgr.IsInternal())
        {
            return false;
        }
        if (!Vars.Key("Arena.AutoDraft").GetBool(false))
        {
            return false;
        }
        return true;
    }

    public void CancelFindGame()
    {
        GameMgr.Get().CancelFindGame();
        this.HandleGameStartupFailure();
    }

    [DebuggerHidden]
    private IEnumerator CompleteAnims()
    {
        return new <CompleteAnims>c__Iterator68 { <>f__this = this };
    }

    private void DeckHeaderOut(UIEvent e)
    {
        this.m_draftDeckTray.GetTooltipZone().HideTooltip();
    }

    private void DeckHeaderOver(UIEvent e)
    {
        this.m_draftDeckTray.GetTooltipZone().ShowTooltip(GameStrings.Get("GLUE_ARENA_DECK_TOOLTIP_HEADER"), GameStrings.Get("GLUE_ARENA_DECK_TOOLTIP"), (float) KeywordHelpPanel.FORGE_SCALE, true);
    }

    private void DestroyChoiceOnSpellFinish(Spell spell, object actorObject)
    {
        Actor actor = (Actor) actorObject;
        base.StartCoroutine(this.DestroyObjectAfterDelay(actor.gameObject));
    }

    [DebuggerHidden]
    private IEnumerator DestroyObjectAfterDelay(GameObject gameObjectToDestroy)
    {
        return new <DestroyObjectAfterDelay>c__Iterator69 { gameObjectToDestroy = gameObjectToDestroy, <$>gameObjectToDestroy = gameObjectToDestroy };
    }

    private void DestroyOldChoices()
    {
        this.m_animationsComplete = false;
        foreach (DraftChoice choice in this.m_choices)
        {
            Actor userData = choice.m_actor;
            if (userData != null)
            {
                DraftCardVisual component = userData.GetCollider().gameObject.GetComponent<DraftCardVisual>();
                userData.TurnOffCollider();
                Spell spell = userData.GetSpell(this.GetSpellTypeForRarity(userData.GetEntityDef().GetRarity()));
                if (component.IsChosen())
                {
                    if (!userData.GetEntityDef().IsHero())
                    {
                        this.AddCardToManaCurve(userData.GetEntityDef());
                        userData.GetSpell(SpellType.SUMMON_OUT_FORGE).AddFinishedCallback(new Spell.FinishedCallback(this.DestroyChoiceOnSpellFinish), userData);
                        userData.ActivateSpell(SpellType.SUMMON_OUT_FORGE);
                        spell.ActivateState(SpellStateType.DEATH);
                        SoundManager.Get().LoadAndPlay("forge_select_card_1");
                        this.m_draftDeckTray.GetCardsContent().UpdateCardList(choice.m_cardID, true, userData);
                    }
                    else
                    {
                        foreach (HeroLabel label in this.m_currentLabels)
                        {
                            label.FadeOut();
                        }
                    }
                }
                else
                {
                    SoundManager.Get().LoadAndPlay("unselected_cards_dissipate");
                    userData.GetSpell(SpellType.BURN).AddFinishedCallback(new Spell.FinishedCallback(this.DestroyChoiceOnSpellFinish), userData);
                    userData.ActivateSpell(SpellType.BURN);
                    if (userData.GetEntityDef().IsHero())
                    {
                        userData.Hide(true);
                    }
                    if (spell != null)
                    {
                        spell.ActivateState(SpellStateType.DEATH);
                    }
                }
            }
        }
        base.StartCoroutine(this.CompleteAnims());
        this.m_choices.Clear();
    }

    public void DoDeckCompleteAnims()
    {
        SoundManager.Get().LoadAndPlay("forge_commit_deck");
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_DeckCompleteSpell.Activate();
            if (this.m_draftDeckTray != null)
            {
                this.m_draftDeckTray.GetCardsContent().ShowDeckCompleteEffects();
            }
        }
    }

    private void DoFirstTimeIntro()
    {
        Box.Get().SetToIgnoreFullScreenEffects(true);
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        this.m_retireButton.Disable();
        if (this.m_manaCurve != null)
        {
            this.m_manaCurve.ResetBars();
        }
        if (UniversalInputManager.UsePhoneUI == null)
        {
            StoreManager.Get().StartArenaTransaction(new Store.ExitCallback(this.OnStoreBackButtonPressed), null, true);
        }
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_ARENA_1ST_TIME_HEADER"),
            m_text = GameStrings.Get("GLUE_ARENA_1ST_TIME_DESC"),
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK,
            m_responseCallback = new AlertPopup.ResponseCallback(this.OnFirstTimeIntroOkButtonPressed)
        };
        DialogManager.Get().ShowPopup(info);
        SoundManager.Get().LoadAndPlay("VO_INNKEEPER_ARENA_INTRO2");
    }

    public void DoHeroCancelAnimation()
    {
        this.RemoveListeners();
        this.m_heroPower.Hide();
        DraftChoice choice = this.m_choices[this.m_zoomedHero.GetChoiceNum() - 1];
        SceneUtils.SetLayer(choice.m_actor.gameObject, GameLayer.Default);
        choice.m_actor.TurnOnCollider();
        this.FadeEffectsOut();
        UniversalInputManager.Get().SetGameDialogActive(false);
        this.m_isHeroAnimating = false;
        this.m_pickArea.enabled = true;
        iTween.MoveTo(this.m_zoomedHero.GetActor().gameObject, this.GetCardPosition(this.m_zoomedHero.GetChoiceNum() - 1, true), 0.25f);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            iTween.ScaleTo(choice.m_actor.gameObject, HERO_ACTOR_LOCAL_SCALE_PHONE, 0.25f);
        }
        else
        {
            iTween.ScaleTo(choice.m_actor.gameObject, HERO_ACTOR_LOCAL_SCALE, 0.25f);
        }
        this.m_pickArea.enabled = false;
        this.m_zoomedHero = null;
    }

    private void DoHeroSelectAnimation()
    {
        this.RemoveListeners();
        this.m_heroPower.Hide();
        this.FadeEffectsOut();
        UniversalInputManager.Get().SetGameDialogActive(false);
        this.m_chosenHero = this.m_zoomedHero.GetActor();
        this.m_zoomedHero.SetChosenFlag(true);
        DraftManager.Get().MakeChoice(this.m_zoomedHero.GetChoiceNum());
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_zoomedHero.GetActor().transform.parent = this.m_socketHeroBone;
            object[] args = new object[] { "position", Vector3.zero, "time", 0.25f, "isLocal", true, "easeType", iTween.EaseType.easeInCubic, "oncomplete", "PhoneHeroAnimationFinished", "oncompletetarget", base.gameObject };
            iTween.MoveTo(this.m_zoomedHero.GetActor().gameObject, iTween.Hash(args));
            object[] objArray2 = new object[] { "scale", Vector3.one, "time", 0.25f, "easeType", iTween.EaseType.easeInCubic };
            iTween.ScaleTo(this.m_zoomedHero.GetActor().gameObject, iTween.Hash(objArray2));
        }
        else
        {
            this.m_zoomedHero.GetActor().ActivateSpell(SpellType.CONSTRUCT);
            this.m_zoomedHero = null;
            this.m_isHeroAnimating = false;
        }
        SoundManager.Get().LoadAndPlay("forge_hero_portrait_plate_descend_and_impact");
        if (!Options.Get().GetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_INST2_20"), "VO_INNKEEPER_FORGE_INST2_20", 3f, null);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE, true);
        }
    }

    private void EnableBackButton(bool buttonEnabled)
    {
        this.m_backButton.enabled = buttonEnabled;
    }

    private void ExitDraftScene()
    {
        GameMgr.Get().CancelFindGame();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        StoreManager.Get().HideArenaStore();
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.SetBlurBrightness(1f);
        mgr.SetBlurDesaturation(0f);
        mgr.Vignette(0.8f, 0.4f, iTween.EaseType.easeOutCirc, null);
        mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, new FullScreenFXMgr.EffectListener(this.OnFadeFinished));
        mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
    }

    public static DraftDisplay Get()
    {
        return s_instance;
    }

    private Vector3 GetCardPosition(int cardChoice, bool isHero)
    {
        float num = this.m_pickArea.bounds.center.x - this.m_pickArea.bounds.extents.x;
        float num3 = this.m_pickArea.bounds.size.x / 3f;
        float num4 = (this.m_choices.Count != 2) ? (-num3 / 2f) : 0f;
        float num5 = 0f;
        if (isHero)
        {
            num5 = 1f;
        }
        return new Vector3((num + ((cardChoice + 1) * num3)) + num4, this.m_pickArea.transform.position.y, this.m_pickArea.transform.position.z + num5);
    }

    public List<DraftCardVisual> GetCardVisuals()
    {
        List<DraftCardVisual> list = new List<DraftCardVisual>();
        foreach (DraftChoice choice in this.m_choices)
        {
            if (choice.m_actor == null)
            {
                return null;
            }
            DraftCardVisual component = choice.m_actor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
            if (component == null)
            {
                return null;
            }
            list.Add(component);
        }
        return list;
    }

    public DraftMode GetDraftMode()
    {
        return this.m_currentMode;
    }

    private SpellType GetSpellTypeForRarity(TAG_RARITY rarity)
    {
        switch (rarity)
        {
            case TAG_RARITY.RARE:
                return SpellType.BURST_RARE;

            case TAG_RARITY.EPIC:
                return SpellType.BURST_EPIC;

            case TAG_RARITY.LEGENDARY:
                return SpellType.BURST_LEGENDARY;
        }
        return SpellType.BURST_COMMON;
    }

    public void HandleGameStartupFailure()
    {
        this.m_playButton.Enable();
        this.ShowPhonePlayButton(true);
        PresenceMgr.Get().SetPrevStatus();
    }

    private bool HaveActorsForAllChoices()
    {
        foreach (DraftChoice choice in this.m_choices)
        {
            if (choice.m_actor == null)
            {
                return false;
            }
        }
        return true;
    }

    private void InitializeDraftScreen()
    {
        if (!this.m_firstTimeIntroComplete && !Options.Get().GetBool(Option.HAS_SEEN_FORGE, false))
        {
            Enum[] args = new Enum[] { PresenceStatus.ARENA_PURCHASE };
            PresenceMgr.Get().SetStatus(args);
            this.m_firstTimeIntroComplete = true;
            this.DoFirstTimeIntro();
        }
        else
        {
            switch (this.m_currentMode)
            {
                case DraftMode.NO_ACTIVE_DRAFT:
                {
                    Enum[] enumArray2 = new Enum[] { PresenceStatus.ARENA_PURCHASE };
                    PresenceMgr.Get().SetStatus(enumArray2);
                    this.ShowPurchaseScreen();
                    break;
                }
                case DraftMode.DRAFTING:
                {
                    if (StoreManager.Get().HasOutstandingPurchaseNotices(ProductType.PRODUCT_TYPE_DRAFT))
                    {
                        this.ShowPurchaseScreen();
                    }
                    Enum[] enumArray3 = new Enum[] { PresenceStatus.ARENA_FORGE };
                    PresenceMgr.Get().SetStatus(enumArray3);
                    this.ShowCurrentlyDraftingScreen();
                    break;
                }
                case DraftMode.ACTIVE_DRAFT_DECK:
                {
                    Enum[] enumArray4 = new Enum[] { PresenceStatus.ARENA_IDLE };
                    PresenceMgr.Get().SetStatus(enumArray4);
                    base.StartCoroutine(this.ShowActiveDraftScreen());
                    break;
                }
                case DraftMode.IN_REWARDS:
                {
                    Enum[] enumArray5 = new Enum[] { PresenceStatus.ARENA_REWARD };
                    PresenceMgr.Get().SetStatus(enumArray5);
                    this.ShowDraftRewardsScreen();
                    break;
                }
                default:
                    UnityEngine.Debug.LogError(string.Format("DraftDisplay.InitializeDraftScreen(): don't know how to handle m_currentMode = {0}", this.m_currentMode));
                    break;
            }
        }
    }

    private void InitManaCurve()
    {
        CollectionDeck draftDeck = DraftManager.Get().GetDraftDeck();
        if (draftDeck != null)
        {
            foreach (CollectionDeckSlot slot in draftDeck.GetSlots())
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
                for (int i = 0; i < slot.Count; i++)
                {
                    this.AddCardToManaCurve(entityDef);
                }
            }
        }
    }

    private bool IsHeroEmoteSpellReady(int index)
    {
        return ((this.m_heroEmotes[index] != null) || this.m_skipHeroEmotes);
    }

    public bool IsInHeroSelectMode()
    {
        return (this.m_zoomedHero != null);
    }

    private void LastArenaWinsLabelLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        int num = (int) callbackData;
        actorObject.GetComponent<UberText>().Text = "Last Arena: " + num + " Wins";
        actorObject.transform.position = new Vector3(11.40591f, 1.341853f, 29.28797f);
        actorObject.transform.localScale = new Vector3(15f, 15f, 15f);
    }

    private void LoadAndPositionHeroCard()
    {
        if (this.m_chosenHero == null)
        {
            if (DraftManager.Get().GetDraftDeck() == null)
            {
                Log.Rachelle.Print("bug 8052, null exception", new object[0]);
            }
            else
            {
                string heroCardID = DraftManager.Get().GetDraftDeck().HeroCardID;
                if (!string.IsNullOrEmpty(heroCardID))
                {
                    DefLoader.Get().LoadFullDef(heroCardID, new DefLoader.LoadDefCallback<FullDef>(this.OnFullHeroDefLoaded), DraftManager.Get().GetDraftDeck().HeroCardFlair);
                }
            }
        }
    }

    private void LoadHeroPowerCallback(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                component.TurnOffCollider();
                SceneUtils.SetLayer(component.gameObject, GameLayer.IgnoreFullScreenEffects);
                this.m_heroPower = component;
                component.Hide();
            }
        }
    }

    private void ManaCurveOut(UIEvent e)
    {
        this.m_manaCurve.GetComponent<TooltipZone>().HideTooltip();
    }

    private void ManaCurveOver(UIEvent e)
    {
        this.m_manaCurve.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_FORGE_MANATIP_HEADER"), GameStrings.Get("GLUE_FORGE_MANATIP_DESC"), (UniversalInputManager.UsePhoneUI == null) ? ((float) KeywordHelpPanel.FORGE_SCALE) : ((float) KeywordHelpPanel.BOX_SCALE), true);
    }

    [DebuggerHidden]
    private IEnumerator NotifySceneLoadedWhenReady()
    {
        return new <NotifySceneLoadedWhenReady>c__Iterator66 { <>f__this = this };
    }

    private void OnActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        <OnActorLoaded>c__AnonStorey2EE storeyee = new <OnActorLoaded>c__AnonStorey2EE();
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.OnActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                ChoiceCallback callback = (ChoiceCallback) callbackData;
                FullDef fullDef = callback.fullDef;
                storeyee.entityDef = fullDef.GetEntityDef();
                CardDef cardDef = fullDef.GetCardDef();
                DraftChoice choice = this.m_choices.Find(new Predicate<DraftChoice>(storeyee.<>m__E5));
                if (choice == null)
                {
                    UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.OnActorLoaded(): Could not find draft choice {0} (cardID = {1}) in m_choices.", storeyee.entityDef.GetName(), storeyee.entityDef.GetCardId()));
                    UnityEngine.Object.Destroy(component);
                }
                else
                {
                    choice.m_actor = component;
                    choice.m_actor.SetCardFlair(choice.m_cardFlair);
                    choice.m_actor.SetEntityDef(storeyee.entityDef);
                    choice.m_actor.SetCardDef(cardDef);
                    choice.m_actor.UpdateAllComponents();
                    choice.m_actor.gameObject.name = cardDef.name + "_actor";
                    choice.m_actor.ContactShadow(true);
                    DraftCardVisual visual = choice.m_actor.GetCollider().gameObject.AddComponent<DraftCardVisual>();
                    visual.SetActor(choice.m_actor);
                    visual.SetChoiceNum(callback.choiceID);
                    if (this.HaveActorsForAllChoices())
                    {
                        this.PositionAndShowChoices();
                    }
                    else
                    {
                        choice.m_actor.Hide();
                    }
                }
            }
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        if (GameMgr.Get().IsFindingGame())
        {
            this.CancelFindGame();
        }
    }

    private void OnCancelButtonClicked(UIEvent e)
    {
        if (this.IsInHeroSelectMode())
        {
            this.DoHeroCancelAnimation();
        }
        else
        {
            Navigation.GoBack();
        }
    }

    private void OnCardDefLoaded(string cardID, GameObject cardObject, object callbackData)
    {
        foreach (EmoteEntryDef def2 in cardObject.GetComponent<CardDef>().m_EmoteDefs)
        {
            if (def2.m_emoteType == EmoteType.PICKED)
            {
                AssetLoader.Get().LoadSpell(def2.m_emoteSoundSpellPath, new AssetLoader.GameObjectCallback(this.OnStartEmoteLoaded), callbackData, false);
            }
        }
    }

    private void OnConfirmButtonClicked(UIEvent e)
    {
        if (!GameUtils.IsAnyTransitionActive())
        {
            this.EnableBackButton(false);
            this.DoHeroSelectAnimation();
        }
    }

    private void OnConfirmButtonLoaded(string name, GameObject go, object callbackData)
    {
        this.m_confirmButton = go.GetComponent<NormalButton>();
        this.m_confirmButton.SetText(GameStrings.Get("GLUE_CHOOSE"));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_confirmButton.transform.position = HERO_CONFIRM_BUTTON_POSITION_PHONE;
        }
        else
        {
            this.m_confirmButton.transform.position = HERO_CONFIRM_BUTTON_POSITION;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_confirmButton.transform.localScale = HERO_CONFIRM_BUTTON_SCALE_PHONE;
        }
        else
        {
            this.m_confirmButton.transform.localScale = HERO_CONFIRM_BUTTON_SCALE;
        }
        this.m_confirmButton.gameObject.SetActive(false);
        SceneUtils.SetLayer(go, GameLayer.IgnoreFullScreenEffects);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnFadeFinished()
    {
        if (this.m_chosenHero != null)
        {
            SceneUtils.SetLayer(this.m_chosenHero.gameObject, GameLayer.Default);
        }
    }

    private void OnFirstTimeIntroOkButtonPressed(AlertPopup.Response response, object userData)
    {
        StoreManager.Get().HideArenaStore();
        DraftManager.Get().RequestDraftStart();
        Options.Get().SetBool(Option.HAS_SEEN_FORGE, true);
    }

    private void OnFullDefLoaded(string cardID, FullDef def, object userData)
    {
        ChoiceCallback callbackData = (ChoiceCallback) userData;
        callbackData.fullDef = def;
        if (def.GetEntityDef().IsHero())
        {
            AssetLoader.Get().LoadActor(ActorNames.GetZoneActor(def.GetEntityDef(), TAG_ZONE.PLAY), new AssetLoader.GameObjectCallback(this.OnActorLoaded), callbackData, false);
            AssetLoader.Get().LoadCardPrefab(def.GetEntityDef().GetCardId(), new AssetLoader.GameObjectCallback(this.OnCardDefLoaded), callbackData.choiceID, false);
            string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(def.GetEntityDef().GetCardId());
            DefLoader.Get().LoadFullDef(heroPowerCardIdFromHero, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroPowerFullDefLoaded), callbackData.choiceID);
        }
        else
        {
            AssetLoader.Get().LoadActor(ActorNames.GetHandActor(def.GetEntityDef()), new AssetLoader.GameObjectCallback(this.OnActorLoaded), callbackData, false);
        }
    }

    private void OnFullHeroDefLoaded(string cardID, FullDef def, object userData)
    {
        CardFlair flair = userData as CardFlair;
        LoadHeroActorCallbackInfo callbackData = new LoadHeroActorCallbackInfo {
            heroFullDef = def,
            heroCardFlair = flair
        };
        AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnHeroActorLoaded), callbackData, false);
    }

    private void OnHeroActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.OnHeroActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                LoadHeroActorCallbackInfo info = callbackData as LoadHeroActorCallbackInfo;
                FullDef heroFullDef = info.heroFullDef;
                EntityDef entityDef = heroFullDef.GetEntityDef();
                CardDef cardDef = heroFullDef.GetCardDef();
                this.m_chosenHero = component;
                this.m_chosenHero.SetCardFlair(info.heroCardFlair);
                this.m_chosenHero.SetEntityDef(entityDef);
                this.m_chosenHero.SetCardDef(cardDef);
                this.m_chosenHero.UpdateAllComponents();
                this.m_chosenHero.gameObject.name = cardDef.name + "_actor";
                this.m_chosenHero.transform.parent = this.m_socketHeroBone.transform;
                this.m_chosenHero.transform.localPosition = Vector3.zero;
                this.m_chosenHero.transform.localScale = Vector3.one;
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    SceneUtils.SetLayer(this.m_chosenHero.gameObject, GameLayer.IgnoreFullScreenEffects);
                }
                this.m_chosenHero.GetHealthObject().Hide();
            }
        }
    }

    public void OnHeroClicked(int heroChoice)
    {
        SoundManager.Get().LoadAndPlay("tournament_screen_select_hero");
        this.m_isHeroAnimating = true;
        DraftChoice choice = this.m_choices[heroChoice - 1];
        this.m_zoomedHero = choice.m_actor.GetCollider().gameObject.GetComponent<DraftCardVisual>();
        choice.m_actor.SetUnlit();
        iTween.MoveTo(choice.m_actor.gameObject, this.m_bigHeroBone.position, 0.25f);
        iTween.ScaleTo(choice.m_actor.gameObject, this.m_bigHeroBone.localScale, 0.25f);
        SoundManager.Get().LoadAndPlay("forge_hero_portrait_plate_rises");
        this.FadeEffectsIn();
        SceneUtils.SetLayer(choice.m_actor.gameObject, GameLayer.IgnoreFullScreenEffects);
        UniversalInputManager.Get().SetGameDialogActive(true);
        this.m_confirmButton.gameObject.SetActive(true);
        this.m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Birth");
        this.m_confirmButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnConfirmButtonClicked));
        this.m_heroClickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelButtonClicked));
        this.m_heroClickCatcher.gameObject.SetActive(true);
        choice.m_actor.TurnOffCollider();
        choice.m_actor.SetActorState(ActorStateType.CARD_IDLE);
        bool flag = this.IsHeroEmoteSpellReady(heroChoice - 1);
        base.StartCoroutine(this.WaitForSpellToLoadAndPlay(heroChoice - 1));
        this.ShowHeroPowerBigCard();
        if (this.CanAutoDraft() && flag)
        {
            this.OnConfirmButtonClicked(null);
        }
    }

    private void OnHeroPowerFullDefLoaded(string cardID, FullDef def, object userData)
    {
        int num = (int) userData;
        this.m_heroPowerDefs[num - 1] = def;
    }

    private bool OnNavigateBack()
    {
        if (this.IsInHeroSelectMode())
        {
            this.DoHeroCancelAnimation();
            return false;
        }
        if (ArenaTrayDisplay.Get() == null)
        {
            return false;
        }
        ArenaTrayDisplay.Get().KeyFXCancel();
        this.ExitDraftScene();
        return true;
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Forge)
        {
            if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
            {
                SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_FORGE", new object[0]);
            }
        }
        else
        {
            this.m_netCacheReady = true;
        }
    }

    private void OnNewQuestsShown()
    {
        this.m_newlyCompletedAchieves = AchieveManager.Get().GetNewCompletedAchieves();
        this.ShowNextCompletedQuestToast(null);
    }

    public void OnOpenRewardsComplete()
    {
        this.ExitDraftScene();
    }

    private void OnPhoneBackButtonLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError("Phone Back Button failed to load!");
        }
        else
        {
            this.m_backButton = go.GetComponent<UIBButton>();
            this.m_backButton.transform.parent = this.m_PhoneBackButtonBone;
            this.m_backButton.transform.position = this.m_PhoneBackButtonBone.position;
            this.m_backButton.transform.localScale = this.m_PhoneBackButtonBone.localScale;
            this.m_backButton.transform.rotation = Quaternion.identity;
            SceneUtils.SetLayer(go, GameLayer.Default);
            this.SetupBackButton();
        }
    }

    private void OnRetirePopupResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_draftDeckTray.gameObject.GetComponent<SlidingTray>().HideTray();
            }
            DraftManager manager = DraftManager.Get();
            this.m_retireButton.Disable();
            Network.RetireDraftDeck(manager.GetDraftDeck().ID, manager.GetSlot());
        }
    }

    private void OnStartEmoteLoaded(string name, GameObject go, object callbackData)
    {
        CardSoundSpell component = null;
        if (go != null)
        {
            component = go.GetComponent<CardSoundSpell>();
        }
        this.m_skipHeroEmotes |= component == null;
        if (this.m_skipHeroEmotes)
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            int num = (int) callbackData;
            this.m_heroEmotes[num - 1] = component;
        }
    }

    private void OnStoreBackButtonPressed(bool authorizationBackButtonPressed, object userData)
    {
        this.ExitDraftScene();
    }

    private void PhoneHeroAnimationFinished()
    {
        Log.Arena.Print("Phone Hero animation complete", new object[0]);
        this.m_zoomedHero = null;
        this.m_isHeroAnimating = false;
    }

    private void PlayButtonPress(UIEvent e)
    {
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        DraftManager.Get().FindGame();
        Enum[] args = new Enum[] { PresenceStatus.ARENA_QUEUE };
        PresenceMgr.Get().SetStatus(args);
    }

    private void PositionAndShowChoices()
    {
        this.m_pickArea.enabled = true;
        for (int i = 0; i < this.m_choices.Count; i++)
        {
            DraftChoice choice = this.m_choices[i];
            if (choice.m_actor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DraftDisplay.PositionAndShowChoices(): WARNING found choice with null actor (cardID = {0}). Skipping...", choice.m_cardID));
                continue;
            }
            bool isHero = choice.m_actor.GetEntityDef().IsHero();
            choice.m_actor.transform.position = this.GetCardPosition(i, isHero);
            choice.m_actor.Show();
            choice.m_actor.ActivateSpell(SpellType.SUMMON_IN_FORGE);
            TAG_RARITY rarity = choice.m_actor.GetEntityDef().GetRarity();
            choice.m_actor.ActivateSpell(this.GetSpellTypeForRarity(rarity));
            switch (rarity)
            {
                case TAG_RARITY.COMMON:
                case TAG_RARITY.FREE:
                    SoundManager.Get().LoadAndPlay("forge_normal_card_appears");
                    break;

                case TAG_RARITY.RARE:
                case TAG_RARITY.EPIC:
                case TAG_RARITY.LEGENDARY:
                    SoundManager.Get().LoadAndPlay("forge_rarity_card_appears");
                    break;
            }
            if (isHero)
            {
                if ((i == 0) && DemoMgr.Get().ArenaIs1WinMode())
                {
                    DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), false, true);
                }
                choice.m_actor.GetHealthObject().Hide();
                GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_heroLabel);
                obj2.transform.position = choice.m_actor.GetMeshRenderer().transform.position;
                HeroLabel component = obj2.GetComponent<HeroLabel>();
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    choice.m_actor.transform.localScale = HERO_ACTOR_LOCAL_SCALE_PHONE;
                    obj2.transform.localScale = HERO_LABEL_SCALE_PHONE;
                }
                else
                {
                    choice.m_actor.transform.localScale = HERO_ACTOR_LOCAL_SCALE;
                    obj2.transform.localScale = HERO_LABEL_SCALE;
                }
                component.UpdateText(choice.m_actor.GetEntityDef().GetName(), GameStrings.GetClassName(choice.m_actor.GetEntityDef().GetClass()).ToUpper());
                this.m_currentLabels.Add(component);
            }
            else if (UniversalInputManager.UsePhoneUI != null)
            {
                choice.m_actor.transform.localScale = CHOICE_ACTOR_LOCAL_SCALE_PHONE;
            }
            else
            {
                choice.m_actor.transform.localScale = CHOICE_ACTOR_LOCAL_SCALE;
            }
        }
        this.EnableBackButton(true);
        base.StartCoroutine(this.RunAutoDraftCheat());
        this.m_pickArea.enabled = false;
    }

    private void RemoveListeners()
    {
        this.m_confirmButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnConfirmButtonClicked));
        this.m_confirmButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
        this.m_confirmButton.gameObject.SetActive(false);
        this.m_heroClickCatcher.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelButtonClicked));
        this.m_heroClickCatcher.gameObject.SetActive(false);
    }

    [DebuggerHidden]
    private IEnumerator RestartArena()
    {
        return new <RestartArena>c__Iterator6B { <>f__this = this };
    }

    private void RetireButtonPress(UIEvent e)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_FORGE_RETIRE_WARNING_HEADER"),
            m_text = GameStrings.Get("GLUE_FORGE_RETIRE_WARNING_DESC"),
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
            m_responseCallback = new AlertPopup.ResponseCallback(this.OnRetirePopupResponse)
        };
        DialogManager.Get().ShowPopup(info);
    }

    [DebuggerHidden]
    public IEnumerator RunAutoDraftCheat()
    {
        return new <RunAutoDraftCheat>c__Iterator6C { <>f__this = this };
    }

    public void SetDraftMode(DraftMode mode)
    {
        bool flag = this.m_currentMode != mode;
        this.m_currentMode = mode;
        if (flag)
        {
            Log.Arena.Print("SetDraftMode - " + this.m_currentMode, new object[0]);
            this.InitializeDraftScreen();
        }
    }

    private void SetupBackButton()
    {
        if (DemoMgr.Get().CantExitArena())
        {
            this.m_backButton.SetText(string.Empty);
        }
        else
        {
            this.m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
            this.m_backButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.BackButtonPress));
        }
    }

    private void SetupRetireButton()
    {
        if (DemoMgr.Get().CantExitArena())
        {
            this.m_retireButton.SetText(string.Empty);
        }
        else
        {
            this.m_retireButton.SetText(GameStrings.Get("GLUE_DRAFT_RETIRE_BUTTON"));
            this.m_retireButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.RetireButtonPress));
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowActiveDraftScreen()
    {
        return new <ShowActiveDraftScreen>c__Iterator6A { <>f__this = this };
    }

    private void ShowCurrentlyDraftingScreen()
    {
        this.m_wasDrafting = true;
        ArenaTrayDisplay.Get().ShowPlainPaperBackground();
        StoreManager.Get().HideArenaStore();
        this.UpdateInstructionText();
        this.m_retireButton.Disable();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        this.LoadAndPositionHeroCard();
    }

    private void ShowDraftRewardsScreen()
    {
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        this.m_retireButton.Disable();
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            base.StartCoroutine(this.RestartArena());
        }
        else
        {
            if (DraftManager.Get().DeckWasActiveDuringSession())
            {
                int maxWins = DraftManager.Get().GetMaxWins();
                if ((DraftManager.Get().GetWins() >= maxWins) && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_MAX_WIN, false))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_MAX_ARENA_WINS_04"), "VO_INNKEEPER_MAX_ARENA_WINS_04", 0f, null);
                    Options.Get().SetBool(Option.HAS_SEEN_FORGE_MAX_WIN, true);
                }
                ArenaTrayDisplay.Get().UpdateTray(false);
                ArenaTrayDisplay.Get().ActivateKey();
                if (this.m_PhoneDeckControl != null)
                {
                    this.m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.Rewards);
                }
            }
            else
            {
                ArenaTrayDisplay.Get().ShowRewardsOpenAtStart();
            }
            this.LoadAndPositionHeroCard();
        }
    }

    private void ShowHeroPowerBigCard()
    {
        if (this.m_heroPower != null)
        {
            SceneUtils.SetLayer(this.m_heroPower.gameObject, GameLayer.IgnoreFullScreenEffects);
            this.m_heroPower.gameObject.transform.parent = this.m_zoomedHero.GetActor().transform;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_heroPower.gameObject.transform.localPosition = HERO_POWER_START_POSITION_PHONE;
                this.m_heroPower.gameObject.transform.localScale = HERO_POWER_SCALE_PHONE;
            }
            else
            {
                this.m_heroPower.gameObject.transform.localPosition = HERO_POWER_START_POSITION;
                this.m_heroPower.gameObject.transform.localScale = HERO_POWER_SCALE;
            }
            base.StartCoroutine(this.ShowHeroPowerWhenDefIsLoaded());
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowHeroPowerWhenDefIsLoaded()
    {
        return new <ShowHeroPowerWhenDefIsLoaded>c__Iterator67 { <>f__this = this };
    }

    private void ShowNewQuests()
    {
        if (!this.m_questsHandled && (this.m_currentMode != DraftMode.IN_REWARDS))
        {
            this.m_questsHandled = true;
            if (AchieveManager.Get().HasActiveQuests(true))
            {
                WelcomeQuests.Show(false, new WelcomeQuests.DelOnWelcomeQuestsClosed(this.OnNewQuestsShown), false);
            }
            else
            {
                this.OnNewQuestsShown();
                GameToastMgr.Get().UpdateQuestProgressToasts();
            }
        }
    }

    private void ShowNextCompletedQuestToast(object userData)
    {
        if (this.m_newlyCompletedAchieves.Count != 0)
        {
            Achievement quest = this.m_newlyCompletedAchieves[0];
            this.m_newlyCompletedAchieves.RemoveAt(0);
            QuestToast.ShowQuestToast(new QuestToast.DelOnCloseQuestToast(this.ShowNextCompletedQuestToast), true, quest);
        }
    }

    private void ShowPhonePlayButton(bool show)
    {
        if (this.m_PhonePlayButtonTray != null)
        {
            SlidingTray component = this.m_PhonePlayButtonTray.GetComponent<SlidingTray>();
            if (component != null)
            {
                component.ToggleTraySlider(show, null, true);
            }
        }
    }

    private void ShowPurchaseScreen()
    {
        Box.Get().SetToIgnoreFullScreenEffects(true);
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_playButton.Disable();
        }
        this.ShowPhonePlayButton(false);
        this.m_retireButton.Disable();
        if (this.m_manaCurve != null)
        {
            this.m_manaCurve.ResetBars();
        }
        if (DemoMgr.Get().ArenaIs1WinMode())
        {
            Network.PurchaseViaGold(1, ProductType.PRODUCT_TYPE_DRAFT, 0);
        }
        else
        {
            StoreManager.Get().StartArenaTransaction(new Store.ExitCallback(this.OnStoreBackButtonPressed), null, false);
        }
    }

    private void Start()
    {
        NetCache.Get().RegisterScreenForge(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        this.SetupRetireButton();
        this.m_playButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.PlayButtonPress));
        this.m_manaCurve.GetComponent<PegUIElement>().AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.ManaCurveOver));
        this.m_manaCurve.GetComponent<PegUIElement>().AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.ManaCurveOut));
        this.m_playButton.SetText(GameStrings.Get("GLOBAL_PLAY"));
        this.ShowPhonePlayButton(false);
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.SetupBackButton();
        }
        Network.FindOutCurrentDraftState();
        MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Arena);
        base.StartCoroutine(this.NotifySceneLoadedWhenReady());
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_draftDeckTray.gameObject.SetActive(true);
        }
        ArenaTrayDisplay.Get().ShowPlainPaperBackground();
    }

    public void Unload()
    {
        Box.Get().SetToIgnoreFullScreenEffects(false);
        if (CollectionCardCache.Get() != null)
        {
            CollectionCardCache.Get().Unload();
        }
        if (this.m_confirmButton != null)
        {
            UnityEngine.Object.Destroy(this.m_confirmButton.gameObject);
        }
        if (this.m_heroPower != null)
        {
            this.m_heroPower.Destroy();
        }
        if (this.m_chosenHero != null)
        {
            this.m_chosenHero.Destroy();
        }
        DraftManager.Get().RemoveNetHandlers();
        DraftManager.Get().RemoveMatchmakerHandlers();
        DraftManager.Get().RemoveStoreHandlers();
        DraftInputManager.Get().Unload();
        DefLoader.Get().ClearCardDefs();
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }

    private void UpdateInstructionText()
    {
        if (this.GetDraftMode() == DraftMode.DRAFTING)
        {
            if (DraftManager.Get().GetSlot() == 0)
            {
                if (!Options.Get().GetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, false))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_INST1_19"), "VO_INNKEEPER_FORGE_INST1_19", 3f, null);
                    Options.Get().SetBool(Option.HAS_SEEN_FORGE_HERO_CHOICE, true);
                }
            }
            else if ((DraftManager.Get().GetSlot() == 2) && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, false))
            {
                NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_INST3_21"), "VO_INNKEEPER_FORGE_INST3_21", 3f, null);
                Options.Get().SetBool(Option.HAS_SEEN_FORGE_CARD_CHOICE2, true);
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                if (DraftManager.Get().GetSlot() == 0)
                {
                    this.m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ChooseHero);
                }
                else if (DraftManager.Get().GetDraftDeck().GetTotalCardCount() > 0)
                {
                    this.m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.CardCountViewDeck);
                }
                else
                {
                    this.m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ChooseCard);
                }
            }
            else
            {
                string str = (DraftManager.Get().GetSlot() != 0) ? GameStrings.Get("GLUE_DRAFT_INSTRUCTIONS") : GameStrings.Get("GLUE_DRAFT_HERO_INSTRUCTIONS");
                this.m_instructionText.Text = str;
            }
        }
        else if (this.GetDraftMode() == DraftMode.ACTIVE_DRAFT_DECK)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_PhoneDeckControl.SetMode(ArenaPhoneControl.ControlMode.ViewDeck);
            }
            else
            {
                this.m_instructionText.Text = GameStrings.Get("GLUE_DRAFT_MATCH_PROG");
            }
        }
        else
        {
            this.m_instructionText.Text = string.Empty;
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForAnimsToFinishAndThenDisplayNewChoices(List<NetCache.CardDefinition> choices)
    {
        return new <WaitForAnimsToFinishAndThenDisplayNewChoices>c__Iterator64 { choices = choices, <$>choices = choices, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForSpellToLoadAndPlay(int index)
    {
        return new <WaitForSpellToLoadAndPlay>c__Iterator65 { index = index, <$>index = index, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <CompleteAnims>c__Iterator68 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;

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
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_animationsComplete = true;
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
    private sealed class <DestroyObjectAfterDelay>c__Iterator69 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>gameObjectToDestroy;
        internal GameObject gameObjectToDestroy;

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
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    UnityEngine.Object.Destroy(this.gameObjectToDestroy);
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
    private sealed class <NotifySceneLoadedWhenReady>c__Iterator66 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;
        internal PegUIElement <deckHeader>__0;

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
                    if (this.<>f__this.m_confirmButton == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0199;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_00A5;

                case 4:
                    goto Label_00CD;

                case 5:
                    goto Label_00F5;

                default:
                    goto Label_0197;
            }
            while (this.<>f__this.m_heroPower == null)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0199;
            }
        Label_00A5:
            while (this.<>f__this.m_currentMode == DraftDisplay.DraftMode.INVALID)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0199;
            }
        Label_00CD:
            while (!this.<>f__this.m_netCacheReady)
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0199;
            }
        Label_00F5:
            while (!AchieveManager.Get().IsReady())
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_0199;
            }
            this.<>f__this.InitManaCurve();
            this.<>f__this.m_draftDeckTray.Initialize();
            this.<deckHeader>__0 = this.<>f__this.m_draftDeckTray.GetTooltipZone().gameObject.GetComponent<PegUIElement>();
            this.<deckHeader>__0.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.<>f__this.DeckHeaderOver));
            this.<deckHeader>__0.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.<>f__this.DeckHeaderOut));
            this.<>f__this.ShowNewQuests();
            SceneMgr.Get().NotifySceneLoaded();
            this.$PC = -1;
        Label_0197:
            return false;
        Label_0199:
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
    private sealed class <OnActorLoaded>c__AnonStorey2EE
    {
        internal EntityDef entityDef;

        internal bool <>m__E5(DraftDisplay.DraftChoice obj)
        {
            return obj.m_cardID.Equals(this.entityDef.GetCardId());
        }
    }

    [CompilerGenerated]
    private sealed class <RestartArena>c__Iterator6B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;
        internal DraftManager <draftMgr>__1;
        internal int <wins>__0;

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
                    UnityEngine.Debug.LogWarning("Restarting");
                    this.<wins>__0 = DraftManager.Get().GetWins();
                    if (this.<wins>__0 >= 5)
                    {
                        if (this.<wins>__0 < 9)
                        {
                            DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_PRIZE"), true);
                        }
                        else if (this.<wins>__0 == 9)
                        {
                            DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_GRAND_PRIZE"), true);
                        }
                        break;
                    }
                    DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_NO_PRIZE"), true);
                    break;

                case 1:
                    this.<>f__this.SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 2;
                    goto Label_0246;

                case 2:
                    this.<draftMgr>__1 = DraftManager.Get();
                    Network.AckDraftRewards(this.<draftMgr>__1.GetDraftDeck().ID, this.<draftMgr>__1.GetSlot());
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 3;
                    goto Label_0246;

                case 3:
                    ArenaTrayDisplay.Get().UpdateTray();
                    if (this.<>f__this.m_chosenHero != null)
                    {
                        UnityEngine.Object.Destroy(this.<>f__this.m_chosenHero.gameObject);
                    }
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 4;
                    goto Label_0246;

                case 4:
                    Network.PurchaseViaGold(1, ProductType.PRODUCT_TYPE_DRAFT, 0);
                    this.$current = new WaitForSeconds(15f);
                    this.$PC = 5;
                    goto Label_0246;

                case 5:
                    if (this.<wins>__0 < 5)
                    {
                        DemoMgr.Get().RemoveDemoTextDialog();
                        DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA"), false, true);
                    }
                    else
                    {
                        DemoMgr.Get().MakeDemoTextClickable(true);
                        DemoMgr.Get().NextDemoTipIsNewArenaMatch();
                    }
                    this.$PC = -1;
                    goto Label_0244;

                default:
                    goto Label_0244;
            }
            AssetLoader.Get().LoadActor("NumberLabel", new AssetLoader.GameObjectCallback(this.<>f__this.LastArenaWinsLabelLoaded), this.<wins>__0, false);
            this.<>f__this.m_currentLabels = new List<HeroLabel>();
            this.$current = new WaitForSeconds(6f);
            this.$PC = 1;
            goto Label_0246;
        Label_0244:
            return false;
        Label_0246:
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
    private sealed class <RunAutoDraftCheat>c__Iterator6C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;
        internal List<DraftCardVisual> <draftChoices>__1;
        internal int <frameStart>__0;
        internal string <info>__4;
        internal int <pickedIndex>__2;
        internal DraftCardVisual <visual>__3;

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
                    if (this.<>f__this.CanAutoDraft())
                    {
                        this.<frameStart>__0 = UnityEngine.Time.frameCount;
                        break;
                    }
                    goto Label_01A4;

                case 1:
                    break;

                case 2:
                    goto Label_0109;

                default:
                    goto Label_01A4;
            }
            while (GameUtils.IsAnyTransitionActive() && ((UnityEngine.Time.frameCount - this.<frameStart>__0) < 120))
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01A6;
            }
            this.<draftChoices>__1 = this.<>f__this.GetCardVisuals();
            if ((this.<draftChoices>__1 == null) || (this.<draftChoices>__1.Count <= 0))
            {
                goto Label_019D;
            }
            UnityEngine.Time.timeScale = SceneDebugger.Get().m_MaxTimeScale;
            this.<pickedIndex>__2 = UnityEngine.Random.Range(0, this.<draftChoices>__1.Count - 1);
            this.<visual>__3 = this.<draftChoices>__1[this.<pickedIndex>__2];
            this.<frameStart>__0 = UnityEngine.Time.frameCount;
        Label_0109:
            while ((this.<visual>__3.GetActor() == null) && ((UnityEngine.Time.frameCount - this.<frameStart>__0) < 120))
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01A6;
            }
            if (this.<visual>__3.GetActor() != null)
            {
                this.<info>__4 = string.Format("autodraft'ing {0}\nto stop, use cmd 'autodraft off'", this.<visual>__3.GetActor().GetEntityDef().GetStringTag(GAME_TAG.CARDNAME));
                UIStatus.Get().AddInfo(this.<info>__4, (float) 2f);
                this.<draftChoices>__1[this.<pickedIndex>__2].ChooseThisCard();
            }
        Label_019D:
            this.$PC = -1;
        Label_01A4:
            return false;
        Label_01A6:
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
    private sealed class <ShowActiveDraftScreen>c__Iterator6A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;
        internal int <losses>__0;

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
                    StoreManager.Get().HideArenaStore();
                    this.<losses>__0 = DraftManager.Get().GetLosses();
                    this.<>f__this.DestroyOldChoices();
                    this.<>f__this.m_retireButton.Enable();
                    this.<>f__this.m_playButton.Enable();
                    this.<>f__this.ShowPhonePlayButton(true);
                    this.<>f__this.UpdateInstructionText();
                    this.<>f__this.LoadAndPositionHeroCard();
                    if (!this.<>f__this.m_wasDrafting)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(0.3f);
                    this.$PC = 1;
                    goto Label_0208;

                case 1:
                    break;

                case 2:
                    goto Label_01AA;

                default:
                    goto Label_0206;
            }
            ArenaTrayDisplay.Get().UpdateTray();
            if (!Options.Get().GetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, false))
            {
                if ((DraftManager.Get().GetWins() == 0) && (this.<losses>__0 == 0))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_COMPLETE_22"), "VO_INNKEEPER_ARENA_COMPLETE", 0f, null);
                    Options.Get().SetBool(Option.HAS_SEEN_FORGE_PLAY_MODE, true);
                }
                goto Label_0206;
            }
            if ((this.<losses>__0 == 2) && !Options.Get().GetBool(Option.HAS_SEEN_FORGE_2LOSS, false))
            {
                NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_FORGE_2LOSS_25"), "VO_INNKEEPER_FORGE_2LOSS_25", 3f, null);
                Options.Get().SetBool(Option.HAS_SEEN_FORGE_2LOSS, true);
                goto Label_0206;
            }
            if ((DraftManager.Get().GetWins() != 1) || Options.Get().GetBool(Option.HAS_SEEN_FORGE_1WIN, false))
            {
                goto Label_0206;
            }
        Label_01AA:
            while (GameToastMgr.Get().AreToastsActive())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0208;
            }
            NotificationManager.Get().CreateInnkeeperQuote(new Vector3(133.1f, NotificationManager.DEPTH, 54.2f), GameStrings.Get("VO_INNKEEPER_FORGE_1WIN"), "VO_INNKEEPER_ARENA_1WIN", 0f, null);
            Options.Get().SetBool(Option.HAS_SEEN_FORGE_1WIN, true);
            goto Label_0206;
            this.$PC = -1;
        Label_0206:
            return false;
        Label_0208:
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
    private sealed class <ShowHeroPowerWhenDefIsLoaded>c__Iterator67 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DraftDisplay <>f__this;
        internal FullDef <def>__0;

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
                    if (this.<>f__this.m_zoomedHero != null)
                    {
                        break;
                    }
                    goto Label_01C2;

                case 1:
                    break;

                default:
                    goto Label_01C2;
            }
            while (this.<>f__this.m_heroPowerDefs[this.<>f__this.m_zoomedHero.GetChoiceNum() - 1] == null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<def>__0 = this.<>f__this.m_heroPowerDefs[this.<>f__this.m_zoomedHero.GetChoiceNum() - 1];
            this.<>f__this.m_heroPower.SetCardDef(this.<def>__0.GetCardDef());
            this.<>f__this.m_heroPower.SetEntityDef(this.<def>__0.GetEntityDef());
            this.<>f__this.m_heroPower.UpdateAllComponents();
            this.<>f__this.m_heroPower.Show();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                object[] args = new object[] { "position", DraftDisplay.HERO_POWER_POSITION_PHONE, "isLocal", true, "time", 0.5f };
                iTween.MoveTo(this.<>f__this.m_heroPower.gameObject, iTween.Hash(args));
            }
            else
            {
                object[] objArray2 = new object[] { "position", DraftDisplay.HERO_POWER_POSITION, "isLocal", true, "time", 0.5f };
                iTween.MoveTo(this.<>f__this.m_heroPower.gameObject, iTween.Hash(objArray2));
            }
            this.$PC = -1;
        Label_01C2:
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
    private sealed class <WaitForAnimsToFinishAndThenDisplayNewChoices>c__Iterator64 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<NetCache.CardDefinition> <$>choices;
        internal DraftDisplay <>f__this;
        internal DraftDisplay.ChoiceCallback <callbackInfo>__6;
        internal NetCache.CardDefinition <cardDefinition>__1;
        internal DraftDisplay.DraftChoice <choice>__5;
        internal int <currentDraftSlot>__3;
        internal int <i>__0;
        internal int <i>__4;
        internal DraftDisplay.DraftChoice <newChoice>__2;
        internal List<NetCache.CardDefinition> choices;

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
                    if (!this.<>f__this.m_animationsComplete)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_01D7;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01D5;
            }
            while (this.<>f__this.m_isHeroAnimating)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01D7;
            }
            this.<i>__0 = 0;
            while (this.<i>__0 < this.choices.Count)
            {
                this.<cardDefinition>__1 = this.choices[this.<i>__0];
                DraftDisplay.DraftChoice choice = new DraftDisplay.DraftChoice {
                    m_cardID = this.<cardDefinition>__1.Name,
                    m_cardFlair = new CardFlair(this.<cardDefinition>__1.Premium)
                };
                this.<newChoice>__2 = choice;
                this.<>f__this.m_choices.Add(this.<newChoice>__2);
                this.<i>__0++;
            }
            this.<currentDraftSlot>__3 = DraftManager.Get().GetSlot();
            this.<>f__this.m_skipHeroEmotes = false;
            this.<i>__4 = 0;
            while (this.<i>__4 < this.<>f__this.m_choices.Count)
            {
                this.<choice>__5 = this.<>f__this.m_choices[this.<i>__4];
                this.<callbackInfo>__6 = new DraftDisplay.ChoiceCallback();
                this.<callbackInfo>__6.choiceID = this.<i>__4 + 1;
                this.<callbackInfo>__6.slot = this.<currentDraftSlot>__3;
                DefLoader.Get().LoadFullDef(this.<choice>__5.m_cardID, new DefLoader.LoadDefCallback<FullDef>(this.<>f__this.OnFullDefLoaded), this.<callbackInfo>__6);
                this.<i>__4++;
            }
            this.$PC = -1;
        Label_01D5:
            return false;
        Label_01D7:
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
    private sealed class <WaitForSpellToLoadAndPlay>c__Iterator65 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>index;
        internal DraftDisplay <>f__this;
        internal CardSoundSpell <emote>__1;
        internal bool <wasEmoteAlreadyReady>__0;
        internal int index;

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
                    this.<wasEmoteAlreadyReady>__0 = this.<>f__this.IsHeroEmoteSpellReady(this.index);
                    break;

                case 1:
                    break;

                default:
                    goto Label_00C7;
            }
            if (!this.<>f__this.IsHeroEmoteSpellReady(this.index))
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (!this.<>f__this.m_skipHeroEmotes)
            {
                this.<emote>__1 = this.<>f__this.m_heroEmotes[this.index];
                this.<emote>__1.Reactivate();
            }
            if (this.<>f__this.CanAutoDraft() && !this.<wasEmoteAlreadyReady>__0)
            {
                this.<>f__this.OnConfirmButtonClicked(null);
            }
            this.$PC = -1;
        Label_00C7:
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

    private class ChoiceCallback
    {
        public int choiceID;
        public FullDef fullDef;
        public int slot;
    }

    private class DraftChoice
    {
        public Actor m_actor;
        public CardFlair m_cardFlair = new CardFlair(TAG_PREMIUM.NORMAL);
        public string m_cardID = string.Empty;
    }

    public enum DraftMode
    {
        INVALID,
        NO_ACTIVE_DRAFT,
        DRAFTING,
        ACTIVE_DRAFT_DECK,
        IN_REWARDS
    }

    private class LoadHeroActorCallbackInfo
    {
        public CardFlair heroCardFlair;
        public FullDef heroFullDef;
    }
}

