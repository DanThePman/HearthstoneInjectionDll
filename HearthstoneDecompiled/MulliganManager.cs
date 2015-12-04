using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MulliganManager : MonoBehaviour
{
    private const float ANIMATION_TIME_DEAL_CARD = 1.5f;
    public AnimationClip cardAnimatesFromBoardToDeck;
    public AnimationClip cardAnimatesFromBoardToDeck_iPhone;
    public AnimationClip cardAnimatesFromDeckToBoard;
    public AnimationClip cardAnimatesFromTableToSky;
    private Vector3 coinLocation;
    private GameObject coinObject;
    public GameObject coinPrefab;
    private GameObject coinTossText;
    private const float DEFAULT_STARTING_TAUNT_DURATION = 2.5f;
    private bool friendlyPlayerGoesFirst;
    private bool friendlyPlayerHasReplacementCards;
    private ZoneDeck friendlySideDeck;
    private ZoneHand friendlySideHandZone;
    public GameObject heroLabelPrefab;
    public AnimationClip hisheroAnimatesToPosition;
    public AnimationClip hisheroAnimatesToPosition_iPhone;
    private Actor hisHeroCardActor;
    private HeroLabel hisheroLabel;
    private Actor hisHeroPowerCardActor;
    private GameObject hisWeldEffect;
    private Notification innkeeperMulliganDialog;
    private bool introComplete;
    private int m_bonusCardIndex = -1;
    private int m_coinCardIndex = -1;
    private bool[] m_handCardsMarkedForReplace = new bool[4];
    private Spell m_HisCustomSocketInSpell;
    private bool m_isLoadingHisCustomSocketIn;
    private bool m_isLoadingMyCustomSocketIn;
    private MulliganTimer m_mulliganTimer;
    private Spell m_MyCustomSocketInSpell;
    private List<MulliganReplaceLabel> m_replaceLabels;
    private bool m_resuming;
    private List<Card> m_startingCards;
    private List<Card> m_startingOppCards;
    private bool m_waitingForUserInput;
    private GameObject[] m_xLabels;
    private bool mulliganActive;
    private NormalButton mulliganButton;
    private GameObject mulliganChooseBanner;
    public GameObject mulliganChooseBannerPrefab;
    public GameObject mulliganKeepLabelPrefab;
    public MulliganReplaceLabel mulliganReplaceLabelPrefab;
    public GameObject mulliganTimerPrefab;
    public GameObject mulliganXlabelPrefab;
    public AnimationClip myheroAnimatesToPosition;
    public AnimationClip myheroAnimatesToPosition_iPhone;
    private Actor myHeroCardActor;
    private HeroLabel myheroLabel;
    private Actor myHeroPowerCardActor;
    private GameObject myWeldEffect;
    private bool opponentPlayerHasReplacementCards;
    private ZoneDeck opposingSideDeck;
    private ZoneHand opposingSideHandZone;
    private const float PHONE_CARD_SCALE = 0.9f;
    private const float PHONE_CARD_Z_OFFSET = 0.2f;
    private const float PHONE_HEIGHT_OFFSET = 7f;
    private const float PHONE_ZONE_SIZE_ADJUST = 0.55f;
    private static MulliganManager s_instance;
    public AnimationClip shuffleDeck;
    private bool skipCardChoosing;
    private GameObject startingHandZone;
    private GameObject versusTextObject;
    private AudioSource versusVo;
    private bool waitingForVersusText;
    private bool waitingForVersusVo;
    public GameObject weldPrefab;

    [DebuggerHidden]
    private IEnumerator AnimateCoinTossText()
    {
        return new <AnimateCoinTossText>c__IteratorB4 { <>f__this = this };
    }

    public void AutomaticContinueMulligan()
    {
        if (this.mulliganButton != null)
        {
            this.mulliganButton.SetEnabled(false);
        }
        this.DestroyMulliganTimer();
        this.BeginDealNewCards();
    }

    private void Awake()
    {
        s_instance = this;
    }

    private void BeginDealNewCards()
    {
        base.StartCoroutine("RemoveOldCardsAnimation");
    }

    public void BeginMulligan()
    {
        bool mulliganActive = this.mulliganActive;
        this.mulliganActive = true;
        if (GameState.Get().WasConcedeRequested())
        {
            this.HandleGameOverDuringMulligan();
        }
        else if (!mulliganActive || !SpectatorManager.Get().IsSpectatingOpposingSide())
        {
            base.StartCoroutine(this.ContinueMulliganWhenBoardLoads());
        }
    }

    private void BeginMulliganCountdown(float endTimeStamp)
    {
        if (this.m_waitingForUserInput)
        {
            GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.mulliganTimerPrefab);
            this.m_mulliganTimer = obj2.GetComponent<MulliganTimer>();
            if (this.m_mulliganTimer == null)
            {
                UnityEngine.Object.Destroy(obj2);
            }
            if (!this.m_waitingForUserInput)
            {
                this.DestroyMulliganTimer();
            }
            this.m_mulliganTimer.SetEndTime(endTimeStamp);
        }
    }

    private void CoinCardSummonFinishedCallback(Spell spell, object userData)
    {
        Card card = SceneUtils.FindComponentInParents<Card>(spell);
        card.RefreshActor();
        card.UpdateActorComponents();
        card.SetDoNotSort(false);
        UnityEngine.Object.Destroy(this.coinObject);
        card.SetTransitionStyle(ZoneTransitionStyle.VERY_SLOW);
        this.friendlySideHandZone.UpdateLayout(-1, true);
    }

    private void CoinTossTextCallback(string actorName, GameObject actorObject, object callbackData)
    {
        string str;
        this.coinTossText = actorObject;
        RenderUtils.SetAlpha(actorObject, 1f);
        actorObject.transform.position = this.coinLocation + new Vector3(0f, 0f, -1f);
        actorObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        UberText componentInChildren = actorObject.transform.GetComponentInChildren<UberText>();
        if (this.friendlyPlayerGoesFirst)
        {
            str = GameStrings.Get("GAMEPLAY_COIN_TOSS_WON");
        }
        else
        {
            str = GameStrings.Get("GAMEPLAY_COIN_TOSS_LOST");
        }
        componentInChildren.Text = str;
        GameState.Get().GetGameEntity().NotifyOfCoinFlipResult();
        base.StartCoroutine(this.AnimateCoinTossText());
    }

    [DebuggerHidden]
    private IEnumerator ContinueMulliganWhenBoardLoads()
    {
        return new <ContinueMulliganWhenBoardLoads>c__IteratorB0 { <>f__this = this };
    }

    private MulliganReplaceLabel CreateNewUILabelAtCardPosition(MulliganReplaceLabel prefab, int cardPosition)
    {
        MulliganReplaceLabel label = UnityEngine.Object.Instantiate<MulliganReplaceLabel>(prefab);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            label.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            label.transform.position = new Vector3(this.m_startingCards[cardPosition].transform.position.x, this.m_startingCards[cardPosition].transform.position.y + 0.3f, this.m_startingCards[cardPosition].transform.position.z - 1.1f);
            return label;
        }
        label.transform.position = new Vector3(this.m_startingCards[cardPosition].transform.position.x, this.m_startingCards[cardPosition].transform.position.y + 0.3f, this.m_startingCards[cardPosition].transform.position.z - (this.startingHandZone.GetComponent<Collider>().bounds.size.z / 2.6f));
        return label;
    }

    [DebuggerHidden]
    private IEnumerator DealStartingCards()
    {
        return new <DealStartingCards>c__IteratorB2 { <>f__this = this };
    }

    private void DestoryHeroSkinSocketInEffects()
    {
        if (this.m_MyCustomSocketInSpell != null)
        {
            UnityEngine.Object.Destroy(this.m_MyCustomSocketInSpell.gameObject);
        }
        if (this.m_HisCustomSocketInSpell != null)
        {
            UnityEngine.Object.Destroy(this.m_HisCustomSocketInSpell.gameObject);
        }
    }

    private void DestroyButton(UnityEngine.Object buttonToDestroy)
    {
        UnityEngine.Object.Destroy(buttonToDestroy);
    }

    private void DestroyChooseBanner()
    {
        if (this.mulliganChooseBanner != null)
        {
            UnityEngine.Object.Destroy(this.mulliganChooseBanner);
        }
    }

    private void DestroyMulliganTimer()
    {
        if (this.m_mulliganTimer != null)
        {
            this.m_mulliganTimer.SelfDestruct();
            this.m_mulliganTimer = null;
        }
    }

    private void DestroyXobjects()
    {
        if (this.m_xLabels != null)
        {
            for (int i = 0; i < this.m_xLabels.Length; i++)
            {
                UnityEngine.Object.Destroy(this.m_xLabels[i]);
            }
            this.m_xLabels = null;
        }
    }

    [DebuggerHidden]
    private IEnumerator EnableHandCollidersAfterCardsAreDealt()
    {
        return new <EnableHandCollidersAfterCardsAreDealt>c__IteratorBA { <>f__this = this };
    }

    public void EndMulligan()
    {
        this.m_waitingForUserInput = false;
        if (this.m_replaceLabels != null)
        {
            for (int i = 0; i < this.m_replaceLabels.Count; i++)
            {
                UnityEngine.Object.Destroy(this.m_replaceLabels[i]);
            }
        }
        if (this.mulliganButton != null)
        {
            UnityEngine.Object.Destroy(this.mulliganButton.gameObject);
        }
        this.DestroyXobjects();
        this.DestroyChooseBanner();
        if (this.versusTextObject != null)
        {
            UnityEngine.Object.Destroy(this.versusTextObject);
        }
        if (this.versusVo != null)
        {
            SoundManager.Get().Destroy(this.versusVo);
        }
        if (this.coinTossText != null)
        {
            UnityEngine.Object.Destroy(this.coinTossText);
        }
        if (this.hisheroLabel != null)
        {
            this.hisheroLabel.FadeOut();
        }
        if (this.myheroLabel != null)
        {
            this.myheroLabel.FadeOut();
        }
        this.DestoryHeroSkinSocketInEffects();
        this.myHeroCardActor.transform.localPosition = new Vector3(0f, 0f, 0f);
        this.hisHeroCardActor.transform.localPosition = new Vector3(0f, 0f, 0f);
        if (!GameState.Get().IsGameOver())
        {
            this.myHeroCardActor.GetHealthObject().Show();
            this.hisHeroCardActor.GetHealthObject().Show();
            this.friendlySideHandZone.ForceStandInUpdate();
            this.friendlySideHandZone.SetDoNotUpdateLayout(false);
            this.friendlySideHandZone.UpdateLayout();
            if ((this.m_startingOppCards != null) && (this.m_startingOppCards.Count > 0))
            {
                this.m_startingOppCards[this.m_startingOppCards.Count - 1].SetDoNotSort(false);
            }
            this.opposingSideHandZone.SetDoNotUpdateLayout(false);
            this.opposingSideHandZone.UpdateLayout();
            this.friendlySideDeck.SetSuppressEmotes(false);
            this.opposingSideDeck.SetSuppressEmotes(false);
            Board.Get().SplitSurface();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                Gameplay.Get().RemoveNameBanners();
                Gameplay.Get().AddGamePlayNameBannerPhone();
            }
            if (this.m_MyCustomSocketInSpell != null)
            {
                UnityEngine.Object.Destroy(this.m_MyCustomSocketInSpell);
            }
            if (this.m_HisCustomSocketInSpell != null)
            {
                UnityEngine.Object.Destroy(this.m_HisCustomSocketInSpell);
            }
            base.StartCoroutine(this.EndMulliganWithTiming());
        }
    }

    [DebuggerHidden]
    private IEnumerator EndMulliganWithTiming()
    {
        return new <EndMulliganWithTiming>c__IteratorB8 { <>f__this = this };
    }

    private void FadeHeroPowerIn(Card heroPowerCard)
    {
        if (heroPowerCard != null)
        {
            Actor actor = heroPowerCard.GetActor();
            if (actor != null)
            {
                actor.TurnOnCollider();
            }
        }
    }

    private void FadeOutMulliganMusicAndStartGameplayMusic()
    {
        GameState.Get().GetGameEntity().StartGameplaySoundtracks();
    }

    public void ForceMulliganActive(bool active)
    {
        this.mulliganActive = active;
    }

    public static MulliganManager Get()
    {
        return s_instance;
    }

    private Card GetCoinCardFromFriendlyHand()
    {
        List<Card> cards = this.friendlySideHandZone.GetCards();
        return cards[cards.Count - 1];
    }

    public NormalButton GetMulliganButton()
    {
        return this.mulliganButton;
    }

    private void GetStartingLists()
    {
        int count;
        List<Card> cards = this.friendlySideHandZone.GetCards();
        List<Card> list2 = this.opposingSideHandZone.GetCards();
        if (this.ShouldHandleCoinCard())
        {
            if (this.friendlyPlayerGoesFirst)
            {
                count = cards.Count;
                this.m_bonusCardIndex = list2.Count - 2;
                this.m_coinCardIndex = list2.Count - 1;
            }
            else
            {
                count = cards.Count - 1;
                this.m_bonusCardIndex = cards.Count - 2;
            }
        }
        else
        {
            count = cards.Count;
            if (this.friendlyPlayerGoesFirst)
            {
                this.m_bonusCardIndex = list2.Count - 1;
            }
            else
            {
                this.m_bonusCardIndex = cards.Count - 1;
            }
        }
        this.m_startingCards = new List<Card>();
        for (int i = 0; i < count; i++)
        {
            this.m_startingCards.Add(cards[i]);
        }
        this.m_startingOppCards = new List<Card>();
        for (int j = 0; j < list2.Count; j++)
        {
            this.m_startingOppCards.Add(list2[j]);
        }
    }

    [DebuggerHidden]
    private IEnumerator HandleCoinCard()
    {
        return new <HandleCoinCard>c__IteratorB9 { <>f__this = this };
    }

    private void HandleGameOverDuringMulligan()
    {
        base.StopCoroutine("WaitForBoardThenLoadButton");
        base.StopCoroutine("WaitForHeroesAndStartAnimations");
        base.StopCoroutine("DealStartingCards");
        base.StopCoroutine("RemoveOldCardsAnimation");
        base.StopCoroutine("PlayStartingTaunts");
        this.m_waitingForUserInput = false;
        this.DestroyXobjects();
        this.DestroyChooseBanner();
        this.DestroyMulliganTimer();
        if (this.coinObject != null)
        {
            UnityEngine.Object.Destroy(this.coinObject);
        }
        if (this.versusTextObject != null)
        {
            UnityEngine.Object.Destroy(this.versusTextObject);
        }
        if (this.versusVo != null)
        {
            SoundManager.Get().Destroy(this.versusVo);
        }
        if (this.coinTossText != null)
        {
            UnityEngine.Object.Destroy(this.coinTossText);
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Gameplay.Get().RemoveNameBanners();
        }
        else
        {
            Gameplay.Get().RemoveClassNames();
        }
        base.StartCoroutine(this.RemoveUIButtons());
        if (this.mulliganButton != null)
        {
            this.mulliganButton.SetEnabled(false);
        }
        this.DestoryHeroSkinSocketInEffects();
        if (this.myheroLabel != null)
        {
            this.myheroLabel.FadeOut();
        }
        if (this.hisheroLabel != null)
        {
            this.hisheroLabel.FadeOut();
        }
        if (this.friendlySideHandZone != null)
        {
            foreach (Card card in this.friendlySideHandZone.GetCards())
            {
                Actor actor = card.GetActor();
                actor.SetActorState(ActorStateType.CARD_IDLE);
                actor.ToggleForceIdle(true);
            }
            if (!this.friendlyPlayerGoesFirst)
            {
                Card coinCardFromFriendlyHand = this.GetCoinCardFromFriendlyHand();
                coinCardFromFriendlyHand.SetDoNotSort(false);
                coinCardFromFriendlyHand.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
                this.PutCoinCardInSpawnPosition(coinCardFromFriendlyHand);
                coinCardFromFriendlyHand.GetActor().Show();
            }
            this.friendlySideHandZone.ForceStandInUpdate();
            this.friendlySideHandZone.SetDoNotUpdateLayout(false);
            this.friendlySideHandZone.UpdateLayout();
        }
        Board board = Board.Get();
        if (board != null)
        {
            board.RaiseTheLightsQuickly();
        }
        Animation component = this.myHeroCardActor.gameObject.GetComponent<Animation>();
        if (component != null)
        {
            component.Stop();
        }
        Animation animation2 = this.hisHeroCardActor.gameObject.GetComponent<Animation>();
        if (animation2 != null)
        {
            animation2.Stop();
        }
        this.myHeroCardActor.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).transform.position;
        this.hisHeroCardActor.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).transform.position;
    }

    private void HandleGameStart()
    {
        object[] args = new object[] { GameState.Get().IsPastBeginPhase() };
        Log.LoadingScreen.Print("MulliganManager.HandleGameStart() - IsPastBeginPhase()={0}", args);
        if (GameState.Get().IsPastBeginPhase())
        {
            base.StartCoroutine(this.SkipMulliganForResume());
        }
        else
        {
            if (!GameState.Get().GetGameEntity().ShouldDoAlternateMulliganIntro())
            {
                this.m_xLabels = new GameObject[4];
                this.coinObject = UnityEngine.Object.Instantiate<GameObject>(this.coinPrefab);
                this.coinObject.SetActive(false);
                if (!Cheats.Get().QuickGameSkipMulligan())
                {
                    if (Cheats.Get().IsLaunchingQuickGame())
                    {
                        UnityEngine.Time.timeScale = SceneDebugger.GetDevTimescale();
                    }
                    this.waitingForVersusVo = true;
                    AssetLoader.Get().LoadSound("VO_ANNOUNCER_VERSUS_21", new AssetLoader.GameObjectCallback(this.OnVersusVoLoaded), null, false, null);
                }
                this.waitingForVersusText = true;
                AssetLoader.Get().LoadGameObject("GameStart_VS_Letters", new AssetLoader.GameObjectCallback(this.OnVersusTextLoaded), null, false);
                base.StartCoroutine("WaitForBoardThenLoadButton");
            }
            base.StartCoroutine("WaitForHeroesAndStartAnimations");
            object[] objArray2 = new object[] { GameState.Get().IsMulliganPhase() };
            Log.LoadingScreen.Print("MulliganManager.HandleGameStart() - IsMulliganPhase()={0}", objArray2);
            if (GameState.Get().IsMulliganPhase())
            {
                base.StartCoroutine(this.ResumeMulligan());
            }
        }
    }

    private void InitZones()
    {
        foreach (Zone zone in ZoneMgr.Get().GetZones())
        {
            if (zone is ZoneHand)
            {
                if (zone.m_Side == Player.Side.FRIENDLY)
                {
                    this.friendlySideHandZone = (ZoneHand) zone;
                }
                else
                {
                    this.opposingSideHandZone = (ZoneHand) zone;
                }
            }
            if (zone is ZoneDeck)
            {
                if (zone.m_Side == Player.Side.FRIENDLY)
                {
                    this.friendlySideDeck = (ZoneDeck) zone;
                    this.friendlySideDeck.SetSuppressEmotes(true);
                    this.friendlySideDeck.UpdateLayout();
                }
                else
                {
                    this.opposingSideDeck = (ZoneDeck) zone;
                    this.opposingSideDeck.SetSuppressEmotes(true);
                    this.opposingSideDeck.UpdateLayout();
                }
            }
        }
    }

    private bool IsCoinCard(Card card)
    {
        return (card.GetEntity().GetCardId() == "GAME_005");
    }

    public bool IsMulliganActive()
    {
        return this.mulliganActive;
    }

    private bool IsTaskListPuttingUsPastMulligan(PowerTaskList taskList)
    {
        foreach (PowerTask task in taskList.GetTaskList())
        {
            Network.PowerHistory power = task.GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = power as Network.HistTagChange;
                if ((change.Tag == 0xc6) && GameUtils.IsPastBeginPhase((TAG_STEP) change.Value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void LoadHisHeroSkinSocketInEffect(CardDef cardDef)
    {
        if ((!string.IsNullOrEmpty(cardDef.m_SocketInEffectOpponent) || (UniversalInputManager.UsePhoneUI != null)) && (!string.IsNullOrEmpty(cardDef.m_SocketInEffectOpponentPhone) || (UniversalInputManager.UsePhoneUI == null)))
        {
            this.m_isLoadingHisCustomSocketIn = true;
            string socketInEffectOpponent = cardDef.m_SocketInEffectOpponent;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                socketInEffectOpponent = cardDef.m_SocketInEffectOpponentPhone;
            }
            AssetLoader.Get().LoadSpell(socketInEffectOpponent, new AssetLoader.GameObjectCallback(this.OnHisHeroSkinSocketInEffectLoaded), null, false);
        }
    }

    private void LoadMyHeroSkinSocketInEffect(CardDef cardDef)
    {
        if ((!string.IsNullOrEmpty(cardDef.m_SocketInEffectFriendly) || (UniversalInputManager.UsePhoneUI != null)) && (!string.IsNullOrEmpty(cardDef.m_SocketInEffectFriendlyPhone) || (UniversalInputManager.UsePhoneUI == null)))
        {
            this.m_isLoadingMyCustomSocketIn = true;
            string socketInEffectFriendly = cardDef.m_SocketInEffectFriendly;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                socketInEffectFriendly = cardDef.m_SocketInEffectFriendlyPhone;
            }
            AssetLoader.Get().LoadSpell(socketInEffectFriendly, new AssetLoader.GameObjectCallback(this.OnMyHeroSkinSocketInEffectLoaded), null, false);
        }
    }

    private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
    {
        GameState.Get().UnregisterCreateGameListener(new GameState.CreateGameCallback(this.OnCreateGame));
        this.HandleGameStart();
    }

    private void OnDestroy()
    {
        if (GameState.Get() != null)
        {
            GameState.Get().UnregisterCreateGameListener(new GameState.CreateGameCallback(this.OnCreateGame));
            GameState.Get().UnregisterMulliganTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnMulliganTimerUpdate));
            GameState.Get().UnregisterEntitiesChosenReceivedListener(new GameState.EntitiesChosenReceivedCallback(this.OnEntitiesChosenReceived));
            GameState.Get().UnregisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        }
        s_instance = null;
    }

    private bool OnEntitiesChosenReceived(Network.EntitiesChosen chosen, Network.EntityChoices choices, object userData)
    {
        if (GameMgr.Get().IsSpectator())
        {
            int playerId = chosen.PlayerId;
            int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
            if (playerId == friendlyPlayerId)
            {
                base.StartCoroutine(this.Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen(chosen));
                return true;
            }
        }
        return false;
    }

    private void OnGameOver(object userData)
    {
        this.HandleGameOverDuringMulligan();
    }

    private void OnHisHeroSkinSocketInEffectLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError("Failed to load His custom hero socket in effect!");
            this.m_isLoadingHisCustomSocketIn = false;
        }
        else
        {
            go.transform.position = Board.Get().FindBone("CustomSocketIn_Opposing").position;
            Spell component = go.GetComponent<Spell>();
            if (component == null)
            {
                UnityEngine.Debug.LogError("Faild to locate Spell on custom socket in effect!");
                this.m_isLoadingHisCustomSocketIn = false;
            }
            else
            {
                this.m_HisCustomSocketInSpell = component;
                if (this.m_HisCustomSocketInSpell.HasUsableState(SpellStateType.IDLE))
                {
                    this.m_HisCustomSocketInSpell.ActivateState(SpellStateType.IDLE);
                }
                else
                {
                    this.m_HisCustomSocketInSpell.gameObject.SetActive(false);
                }
                this.m_isLoadingHisCustomSocketIn = false;
            }
        }
    }

    private void OnMulliganButtonLoaded(string name, GameObject go, object userData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("MulliganManager.OnMulliganButtonLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.mulliganButton = go.GetComponent<NormalButton>();
            if (this.mulliganButton == null)
            {
                UnityEngine.Debug.LogError(string.Format("MulliganManager.OnMulliganButtonLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(NormalButton)));
            }
            else
            {
                this.mulliganButton.SetText(GameStrings.Get("GLOBAL_CONFIRM"));
            }
        }
    }

    private void OnMulliganButtonReleased(UIEvent e)
    {
        if (!GameMgr.Get().IsSpectator())
        {
            ((NormalButton) e.GetElement()).SetEnabled(false);
            this.BeginDealNewCards();
        }
    }

    private void OnMulliganTimerUpdate(TurnTimerUpdate update, object userData)
    {
        if (update.GetSecondsRemaining() > Mathf.Epsilon)
        {
            if (update.ShouldShow())
            {
                this.BeginMulliganCountdown(update.GetEndTimestamp());
            }
        }
        else
        {
            GameState.Get().UnregisterMulliganTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnMulliganTimerUpdate));
        }
    }

    private void OnMyHeroSkinSocketInEffectLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError("Failed to load My custom hero socket in effect!");
            this.m_isLoadingMyCustomSocketIn = false;
        }
        else
        {
            go.transform.position = Board.Get().FindBone("CustomSocketIn_Friendly").position;
            Spell component = go.GetComponent<Spell>();
            if (component == null)
            {
                UnityEngine.Debug.LogError("Faild to locate Spell on custom socket in effect!");
                this.m_isLoadingMyCustomSocketIn = false;
            }
            else
            {
                this.m_MyCustomSocketInSpell = component;
                if (this.m_MyCustomSocketInSpell.HasUsableState(SpellStateType.IDLE))
                {
                    this.m_MyCustomSocketInSpell.ActivateState(SpellStateType.IDLE);
                }
                else
                {
                    this.m_MyCustomSocketInSpell.gameObject.SetActive(false);
                }
                this.m_isLoadingMyCustomSocketIn = false;
            }
        }
    }

    private void OnVersusTextLoaded(string name, GameObject go, object userData)
    {
        this.waitingForVersusText = false;
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("MulliganManager.OnVersusTextLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.versusTextObject = go;
        }
    }

    private void OnVersusVoLoaded(string name, GameObject go, object userData)
    {
        this.waitingForVersusVo = false;
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("MulliganManager.OnVersusVoLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.versusVo = go.GetComponent<AudioSource>();
            if (this.versusVo == null)
            {
                UnityEngine.Debug.LogError(string.Format("MulliganManager.OnVersusVoLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(AudioSource)));
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayStartingTaunts()
    {
        return new <PlayStartingTaunts>c__IteratorB1 { <>f__this = this };
    }

    private void PutCoinCardInSpawnPosition(Card coinCard)
    {
        coinCard.transform.position = Board.Get().FindBone("MulliganCoinCardSpawnPosition").position;
        coinCard.transform.localScale = Board.Get().FindBone("MulliganCoinCardSpawnPosition").localScale;
    }

    [DebuggerHidden]
    private IEnumerator RemoveOldCardsAnimation()
    {
        return new <RemoveOldCardsAnimation>c__IteratorB5 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator RemoveUIButtons()
    {
        return new <RemoveUIButtons>c__IteratorB7 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ResumeMulligan()
    {
        return new <ResumeMulligan>c__IteratorAE { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SampleAnimFrame(Animation animToUse, string animName, float startSec)
    {
        return new <SampleAnimFrame>c__IteratorBE { animToUse = animToUse, animName = animName, startSec = startSec, <$>animToUse = animToUse, <$>animName = animName, <$>startSec = startSec };
    }

    public void ServerHasDealtReplacementCards(bool isFriendlySide)
    {
        if (isFriendlySide)
        {
            this.friendlyPlayerHasReplacementCards = true;
            if (GameState.Get().IsFriendlySidePlayerTurn())
            {
                TurnStartManager.Get().BeginListeningForTurnEvents();
            }
        }
        else
        {
            this.opponentPlayerHasReplacementCards = true;
        }
    }

    public void SetAllMulliganCardsToHold()
    {
        foreach (Card card in this.friendlySideHandZone.GetCards())
        {
            InputManager.Get().DoNetworkResponse(card.GetEntity(), true);
        }
    }

    private bool ShouldHandleCoinCard()
    {
        if (!GameState.Get().IsMulliganPhase())
        {
            return false;
        }
        if (!GameState.Get().GetGameEntity().ShouldHandleCoin())
        {
            return false;
        }
        return true;
    }

    private bool ShouldWaitForMulliganCardsToBeProcessed()
    {
        <ShouldWaitForMulliganCardsToBeProcessed>c__AnonStorey2FA storeyfa = new <ShouldWaitForMulliganCardsToBeProcessed>c__AnonStorey2FA {
            <>f__this = this
        };
        PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
        storeyfa.receivedEndOfMulligan = false;
        powerProcessor.ForEachTaskList(new Action<int, PowerTaskList>(storeyfa.<>m__100));
        if (storeyfa.receivedEndOfMulligan)
        {
            return false;
        }
        return powerProcessor.HasTaskLists();
    }

    [DebuggerHidden]
    private IEnumerator ShrinkStartingHandBanner(GameObject banner)
    {
        return new <ShrinkStartingHandBanner>c__IteratorBF { banner = banner, <$>banner = banner };
    }

    private void ShuffleDeck()
    {
        SoundManager.Get().LoadAndPlay("FX_MulliganCoin09_DeckShuffle", this.friendlySideDeck.gameObject);
        Animation component = this.friendlySideDeck.gameObject.GetComponent<Animation>();
        if (component == null)
        {
            component = this.friendlySideDeck.gameObject.AddComponent<Animation>();
        }
        component.AddClip(this.shuffleDeck, "shuffleDeckAnim");
        component.Play("shuffleDeckAnim");
        component = this.opposingSideDeck.gameObject.GetComponent<Animation>();
        if (component == null)
        {
            component = this.opposingSideDeck.gameObject.AddComponent<Animation>();
        }
        component.AddClip(this.shuffleDeck, "shuffleDeckAnim");
        component.Play("shuffleDeckAnim");
    }

    public void SkipCardChoosing()
    {
        this.skipCardChoosing = true;
    }

    public void SkipMulligan()
    {
        base.StartCoroutine(this.SkipMulliganWhenIntroComplete());
    }

    public void SkipMulliganForDev()
    {
        base.StopCoroutine("WaitForBoardThenLoadButton");
        base.StopCoroutine("WaitForHeroesAndStartAnimations");
        base.StopCoroutine("DealStartingCards");
        this.EndMulligan();
    }

    [DebuggerHidden]
    private IEnumerator SkipMulliganForResume()
    {
        return new <SkipMulliganForResume>c__IteratorBB { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator SkipMulliganWhenIntroComplete()
    {
        return new <SkipMulliganWhenIntroComplete>c__IteratorBC { <>f__this = this };
    }

    private void SlideCard(GameObject topCard)
    {
        object[] args = new object[] { "position", new Vector3(topCard.transform.position.x - 0.5f, topCard.transform.position.y, topCard.transform.position.z), "time", 0.5f, "easetype", iTween.EaseType.linear };
        iTween.MoveTo(topCard, iTween.Hash(args));
    }

    private void SortHand(Zone zone)
    {
        zone.GetCards().Sort(new Comparison<Card>(Zone.CardSortComparison));
    }

    [DebuggerHidden]
    private IEnumerator Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen(Network.EntitiesChosen chosen)
    {
        return new <Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen>c__IteratorAF { chosen = chosen, <$>chosen = chosen, <>f__this = this };
    }

    private void Start()
    {
        if (GameState.Get().IsGameCreatedOrCreating())
        {
            this.HandleGameStart();
        }
        else
        {
            GameState.Get().RegisterCreateGameListener(new GameState.CreateGameCallback(this.OnCreateGame));
        }
        GameState.Get().RegisterMulliganTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnMulliganTimerUpdate));
        GameState.Get().RegisterEntitiesChosenReceivedListener(new GameState.EntitiesChosenReceivedCallback(this.OnEntitiesChosenReceived));
        GameState.Get().RegisterGameOverListener(new GameState.GameOverCallback(this.OnGameOver), null);
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.myheroAnimatesToPosition = this.myheroAnimatesToPosition_iPhone;
            this.hisheroAnimatesToPosition = this.hisheroAnimatesToPosition_iPhone;
            this.cardAnimatesFromBoardToDeck = this.cardAnimatesFromBoardToDeck_iPhone;
        }
    }

    public void ToggleHoldState(Card toggleCard)
    {
        for (int i = 0; i < this.m_startingCards.Count; i++)
        {
            if (this.m_startingCards[i] == toggleCard)
            {
                this.ToggleHoldState(i);
                return;
            }
        }
    }

    private void ToggleHoldState(int startingCardsIndex)
    {
        if ((startingCardsIndex < this.m_startingCards.Count) && InputManager.Get().DoNetworkResponse(this.m_startingCards[startingCardsIndex].GetEntity(), true))
        {
            this.m_handCardsMarkedForReplace[startingCardsIndex] = !this.m_handCardsMarkedForReplace[startingCardsIndex];
            if (!this.m_handCardsMarkedForReplace[startingCardsIndex])
            {
                SoundManager.Get().LoadAndPlay("GM_ChatWarning");
                if (this.m_xLabels[startingCardsIndex] != null)
                {
                    UnityEngine.Object.Destroy(this.m_xLabels[startingCardsIndex]);
                }
                UnityEngine.Object.Destroy(this.m_replaceLabels[startingCardsIndex].gameObject);
            }
            else
            {
                SoundManager.Get().LoadAndPlay("HeroDropItem1");
                if (this.m_xLabels[startingCardsIndex] != null)
                {
                    UnityEngine.Object.Destroy(this.m_xLabels[startingCardsIndex]);
                }
                GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.mulliganXlabelPrefab);
                obj2.transform.position = this.m_startingCards[startingCardsIndex].transform.position;
                obj2.transform.rotation = this.m_startingCards[startingCardsIndex].transform.rotation;
                this.m_xLabels[startingCardsIndex] = obj2;
                this.m_replaceLabels[startingCardsIndex] = this.CreateNewUILabelAtCardPosition(this.mulliganReplaceLabelPrefab, startingCardsIndex);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitAFrameBeforeSendingEventToMulliganButton()
    {
        return new <WaitAFrameBeforeSendingEventToMulliganButton>c__IteratorB3 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForBoardAnimToCompleteThenStartTurn()
    {
        return new <WaitForBoardAnimToCompleteThenStartTurn>c__IteratorBD { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForBoardThenLoadButton()
    {
        return new <WaitForBoardThenLoadButton>c__IteratorAC { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForHeroesAndStartAnimations()
    {
        return new <WaitForHeroesAndStartAnimations>c__IteratorAD { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForOpponentToFinishMulligan()
    {
        return new <WaitForOpponentToFinishMulligan>c__IteratorB6 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <AnimateCoinTossText>c__IteratorB4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;

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
                    this.$current = new WaitForSeconds(1.8f);
                    this.$PC = 1;
                    goto Label_016E;

                case 1:
                    if (this.<>f__this.coinTossText != null)
                    {
                        iTween.FadeTo(this.<>f__this.coinTossText, 1f, 0.25f);
                        iTween.MoveTo(this.<>f__this.coinTossText, this.<>f__this.coinTossText.transform.position + new Vector3(0f, 0.5f, 0f), 2f);
                        this.$current = new WaitForSeconds(1.9f);
                        this.$PC = 2;
                        goto Label_016E;
                    }
                    break;

                case 2:
                case 3:
                    if (GameState.Get().IsBusy())
                    {
                        this.$current = null;
                        this.$PC = 3;
                    }
                    else
                    {
                        if (this.<>f__this.coinTossText == null)
                        {
                            break;
                        }
                        iTween.FadeTo(this.<>f__this.coinTossText, 0f, 1f);
                        this.$current = new WaitForSeconds(0.1f);
                        this.$PC = 4;
                    }
                    goto Label_016E;

                case 4:
                    UnityEngine.Object.Destroy(this.<>f__this.coinTossText);
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_016E:
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
    private sealed class <ContinueMulliganWhenBoardLoads>c__IteratorB0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal Board <board>__0;
        internal Collider <dragPlane>__1;

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
                        goto Label_012E;
                    }
                    this.<board>__0 = Board.Get();
                    this.<>f__this.startingHandZone = this.<board>__0.FindBone("StartingHandZone").gameObject;
                    this.<>f__this.InitZones();
                    if (!this.<>f__this.m_resuming)
                    {
                        goto Label_00BB;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_012C;
            }
            while (this.<>f__this.ShouldWaitForMulliganCardsToBeProcessed())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_012E;
            }
        Label_00BB:
            this.<>f__this.SortHand(this.<>f__this.friendlySideHandZone);
            this.<>f__this.SortHand(this.<>f__this.opposingSideHandZone);
            this.<board>__0.CombinedSurface();
            this.<dragPlane>__1 = this.<board>__0.FindCollider("DragPlane");
            this.<dragPlane>__1.enabled = false;
            this.<>f__this.StartCoroutine("DealStartingCards");
            this.$PC = -1;
        Label_012C:
            return false;
        Label_012E:
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
    private sealed class <DealStartingCards>c__IteratorB2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_591>__1;
        internal List<Card>.Enumerator <$s_592>__24;
        internal MulliganManager <>f__this;
        internal GameObject <bonusCard>__20;
        internal Card <card>__2;
        internal float <cardHeightOffset>__11;
        internal float <cardZpos>__12;
        internal Transform <coinSpawnLocation>__19;
        internal Vector3[] <drawPath>__16;
        internal Vector3[] <drawPath>__21;
        internal Player <friendlyPlayer>__0;
        internal int <i>__14;
        internal int <i>__22;
        internal int <i>__26;
        internal float <leftSideOfZone>__7;
        internal Vector3 <mulliganChooseBannerPosition>__17;
        internal int <numCardsToDealExcludingBonusCard>__10;
        internal float <rightSideOfZone>__8;
        internal float <spaceForEachCard>__4;
        internal float <spaceForEachCardPre4th>__5;
        internal float <spacingToUse>__6;
        internal Card <startCard>__25;
        internal Vector3 <startingScale>__18;
        internal float <timingBonus>__9;
        internal GameObject <topCard>__15;
        internal GameObject <topCard>__23;
        internal float <xOffset>__13;
        internal float <zoneWidth>__3;

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
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    goto Label_110D;

                case 1:
                case 2:
                    if (!this.<>f__this.introComplete)
                    {
                        this.$current = null;
                        this.$PC = 2;
                    }
                    else
                    {
                        this.$current = this.<>f__this.StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterIntroBeforeMulligan());
                        this.$PC = 3;
                    }
                    goto Label_110D;

                case 3:
                    if (GameState.Get().GetGameEntity().ShouldDoOpeningTaunts() && !Cheats.Get().QuickGameSkipMulligan())
                    {
                        this.<>f__this.StartCoroutine("PlayStartingTaunts");
                    }
                    this.<friendlyPlayer>__0 = GameState.Get().GetFriendlySidePlayer();
                    this.<>f__this.friendlyPlayerGoesFirst = this.<friendlyPlayer>__0.HasTag(GAME_TAG.FIRST_PLAYER);
                    this.<>f__this.GetStartingLists();
                    this.<$s_591>__1 = this.<>f__this.m_startingCards.GetEnumerator();
                    try
                    {
                        while (this.<$s_591>__1.MoveNext())
                        {
                            this.<card>__2 = this.<$s_591>__1.Current;
                            this.<card>__2.GetActor().SetActorState(ActorStateType.CARD_IDLE);
                            this.<card>__2.GetActor().TurnOffCollider();
                            this.<card>__2.GetActor().GetMeshRenderer().gameObject.layer = 8;
                        }
                    }
                    finally
                    {
                        this.<$s_591>__1.Dispose();
                    }
                    this.<zoneWidth>__3 = this.<>f__this.startingHandZone.GetComponent<Collider>().bounds.size.x;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<zoneWidth>__3 *= 0.55f;
                    }
                    this.<spaceForEachCard>__4 = this.<zoneWidth>__3 / ((float) this.<>f__this.m_startingCards.Count);
                    this.<spaceForEachCardPre4th>__5 = this.<zoneWidth>__3 / ((float) (this.<>f__this.m_startingCards.Count + 1));
                    this.<spacingToUse>__6 = this.<spaceForEachCardPre4th>__5;
                    this.<leftSideOfZone>__7 = this.<>f__this.startingHandZone.transform.position.x - (this.<zoneWidth>__3 / 2f);
                    this.<rightSideOfZone>__8 = this.<>f__this.startingHandZone.transform.position.x + (this.<zoneWidth>__3 / 2f);
                    this.<timingBonus>__9 = 0.1f;
                    this.<numCardsToDealExcludingBonusCard>__10 = this.<>f__this.m_startingCards.Count;
                    if (!this.<>f__this.friendlyPlayerGoesFirst)
                    {
                        this.<numCardsToDealExcludingBonusCard>__10 = this.<>f__this.m_bonusCardIndex;
                        this.<spacingToUse>__6 = this.<spaceForEachCard>__4;
                    }
                    else if (this.<>f__this.m_startingOppCards.Count > 0)
                    {
                        this.<>f__this.m_startingOppCards[this.<>f__this.m_bonusCardIndex].SetDoNotSort(true);
                        if (this.<>f__this.m_coinCardIndex >= 0)
                        {
                            this.<>f__this.m_startingOppCards[this.<>f__this.m_coinCardIndex].SetDoNotSort(true);
                        }
                    }
                    this.<>f__this.opposingSideHandZone.SetDoNotUpdateLayout(false);
                    this.<>f__this.opposingSideHandZone.UpdateLayout(-1, true, 3);
                    this.<cardHeightOffset>__11 = 0f;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<cardHeightOffset>__11 = 7f;
                    }
                    this.<cardZpos>__12 = this.<>f__this.startingHandZone.transform.position.z - 0.3f;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<cardZpos>__12 = this.<>f__this.startingHandZone.transform.position.z - 0.2f;
                    }
                    this.<xOffset>__13 = this.<spacingToUse>__6 / 2f;
                    this.<i>__14 = 0;
                    while (this.<i>__14 < this.<numCardsToDealExcludingBonusCard>__10)
                    {
                        this.<topCard>__15 = this.<>f__this.m_startingCards[this.<i>__14].gameObject;
                        iTween.Stop(this.<topCard>__15);
                        this.<drawPath>__16 = new Vector3[] { this.<topCard>__15.transform.position, new Vector3(this.<topCard>__15.transform.position.x, this.<topCard>__15.transform.position.y + 3.6f, this.<topCard>__15.transform.position.z), new Vector3(this.<leftSideOfZone>__7 + this.<xOffset>__13, this.<>f__this.friendlySideHandZone.transform.position.y + this.<cardHeightOffset>__11, this.<cardZpos>__12) };
                        object[] objArray1 = new object[] { "path", this.<drawPath>__16, "time", 1.5f, "easetype", iTween.EaseType.easeInSineOutExpo };
                        iTween.MoveTo(this.<topCard>__15, iTween.Hash(objArray1));
                        if (UniversalInputManager.UsePhoneUI != null)
                        {
                            iTween.ScaleTo(this.<topCard>__15, new Vector3(0.9f, 1.1f, 0.9f), 1.5f);
                        }
                        else
                        {
                            iTween.ScaleTo(this.<topCard>__15, new Vector3(1.1f, 1.1f, 1.1f), 1.5f);
                        }
                        object[] objArray2 = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", 1.5f, "delay", 0.09375f };
                        iTween.RotateTo(this.<topCard>__15, iTween.Hash(objArray2));
                        this.$current = new WaitForSeconds(0.04f);
                        this.$PC = 4;
                        goto Label_110D;
                    Label_06BA:
                        this.<timingBonus>__9 = 0f;
                        this.<i>__14++;
                    }
                    if (this.<>f__this.skipCardChoosing)
                    {
                        this.<>f__this.mulliganChooseBanner = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.mulliganChooseBannerPrefab);
                        this.<>f__this.mulliganChooseBanner.GetComponent<Banner>().SetText(GameStrings.Get("GAMEPLAY_MULLIGAN_STARTING_HAND"));
                        this.<mulliganChooseBannerPosition>__17 = Board.Get().FindBone("ChoiceBanner").position;
                        this.<>f__this.mulliganChooseBanner.transform.position = this.<mulliganChooseBannerPosition>__17;
                        this.<startingScale>__18 = this.<>f__this.mulliganChooseBanner.transform.localScale;
                        this.<>f__this.mulliganChooseBanner.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                        iTween.ScaleTo(this.<>f__this.mulliganChooseBanner, this.<startingScale>__18, 0.5f);
                        this.<>f__this.StartCoroutine(this.<>f__this.ShrinkStartingHandBanner(this.<>f__this.mulliganChooseBanner));
                    }
                    this.$current = new WaitForSeconds(1.1f);
                    this.$PC = 6;
                    goto Label_110D;

                case 4:
                    SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable", this.<topCard>__15);
                    this.<xOffset>__13 += this.<spacingToUse>__6;
                    this.$current = new WaitForSeconds(0.05f + this.<timingBonus>__9);
                    this.$PC = 5;
                    goto Label_110D;

                case 5:
                    goto Label_06BA;

                case 6:
                    this.<coinSpawnLocation>__19 = Board.Get().FindBone("MulliganCoinPosition");
                    this.<>f__this.coinObject.transform.position = this.<coinSpawnLocation>__19.position;
                    this.<>f__this.coinObject.transform.localEulerAngles = this.<coinSpawnLocation>__19.localEulerAngles;
                    this.<>f__this.coinObject.SetActive(true);
                    this.<>f__this.coinObject.GetComponent<CoinEffect>().DoAnim(this.<>f__this.friendlyPlayerGoesFirst);
                    SoundManager.Get().LoadAndPlay("FX_MulliganCoin03_CoinFlip", this.<>f__this.coinObject);
                    this.<>f__this.coinLocation = this.<coinSpawnLocation>__19.position;
                    AssetLoader.Get().LoadActor("MulliganResultText", new AssetLoader.GameObjectCallback(this.<>f__this.CoinTossTextCallback), null, false);
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 7;
                    goto Label_110D;

                case 7:
                {
                    if (this.<>f__this.friendlyPlayerGoesFirst)
                    {
                        if (this.<>f__this.m_startingOppCards.Count > 0)
                        {
                            this.<>f__this.m_startingOppCards[this.<>f__this.m_bonusCardIndex].SetDoNotSort(false);
                            this.<>f__this.opposingSideHandZone.UpdateLayout(-1, true, 4);
                        }
                        goto Label_0B9B;
                    }
                    this.<bonusCard>__20 = this.<>f__this.m_startingCards[this.<>f__this.m_bonusCardIndex].gameObject;
                    this.<drawPath>__21 = new Vector3[] { this.<bonusCard>__20.transform.position, new Vector3(this.<bonusCard>__20.transform.position.x, this.<bonusCard>__20.transform.position.y + 3.6f, this.<bonusCard>__20.transform.position.z), new Vector3(this.<leftSideOfZone>__7 + this.<xOffset>__13, this.<>f__this.friendlySideHandZone.transform.position.y + this.<cardHeightOffset>__11, this.<cardZpos>__12) };
                    object[] objArray3 = new object[] { "path", this.<drawPath>__21, "time", 1.5f, "easetype", iTween.EaseType.easeInSineOutExpo };
                    iTween.MoveTo(this.<bonusCard>__20, iTween.Hash(objArray3));
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        iTween.ScaleTo(this.<bonusCard>__20, new Vector3(1.1f, 1.1f, 1.1f), 1.5f);
                        break;
                    }
                    iTween.ScaleTo(this.<bonusCard>__20, new Vector3(0.9f, 1.1f, 0.9f), 1.5f);
                    break;
                }
                case 8:
                    SoundManager.Get().LoadAndPlay("FX_GameStart20_CardDealSingle", this.<bonusCard>__20);
                    goto Label_0B9B;

                case 9:
                case 10:
                    while (GameState.Get().IsBusy())
                    {
                        this.$current = null;
                        this.$PC = 10;
                        goto Label_110D;
                    }
                    if (this.<>f__this.friendlyPlayerGoesFirst)
                    {
                        this.<xOffset>__13 = 0f;
                        this.<i>__22 = this.<>f__this.m_startingCards.Count - 1;
                        while (this.<i>__22 >= 0)
                        {
                            this.<topCard>__23 = this.<>f__this.m_startingCards[this.<i>__22].gameObject;
                            iTween.Stop(this.<topCard>__23);
                            object[] objArray5 = new object[] { "position", new Vector3(((this.<rightSideOfZone>__8 - this.<spaceForEachCard>__4) - this.<xOffset>__13) + (this.<spaceForEachCard>__4 / 2f), this.<>f__this.friendlySideHandZone.transform.position.y + this.<cardHeightOffset>__11, this.<cardZpos>__12), "time", 0.9333333f, "easetype", iTween.EaseType.easeInOutCubic };
                            iTween.MoveTo(this.<topCard>__23, iTween.Hash(objArray5));
                            this.<xOffset>__13 += this.<spaceForEachCard>__4;
                            this.<i>__22--;
                        }
                    }
                    this.$current = new WaitForSeconds(0.6f);
                    this.$PC = 11;
                    goto Label_110D;

                case 11:
                    if (!this.<>f__this.skipCardChoosing)
                    {
                        this.<$s_592>__24 = this.<>f__this.m_startingCards.GetEnumerator();
                        try
                        {
                            while (this.<$s_592>__24.MoveNext())
                            {
                                this.<startCard>__25 = this.<$s_592>__24.Current;
                                this.<startCard>__25.GetActor().TurnOnCollider();
                            }
                        }
                        finally
                        {
                            this.<$s_592>__24.Dispose();
                        }
                        this.<>f__this.mulliganChooseBanner = (GameObject) UnityEngine.Object.Instantiate(this.<>f__this.mulliganChooseBannerPrefab, Board.Get().FindBone("ChoiceBanner").position, new Quaternion(0f, 0f, 0f, 0f));
                        this.<>f__this.mulliganChooseBanner.GetComponent<Banner>().SetText(GameStrings.Get("GAMEPLAY_MULLIGAN_STARTING_HAND"), GameStrings.Get("GAMEPLAY_MULLIGAN_SUBTITLE"));
                        this.<>f__this.m_replaceLabels = new List<MulliganReplaceLabel>();
                        this.<i>__26 = 0;
                        while (this.<i>__26 < this.<>f__this.m_startingCards.Count)
                        {
                            InputManager.Get().DoNetworkResponse(this.<>f__this.m_startingCards[this.<i>__26].GetEntity(), true);
                            this.<>f__this.m_replaceLabels.Add(null);
                            this.<i>__26++;
                        }
                        goto Label_0F1A;
                    }
                    if (!GameState.Get().IsMulliganPhase())
                    {
                        this.$current = new WaitForSeconds(2f);
                        this.$PC = 12;
                        goto Label_110D;
                    }
                    if (GameState.Get().IsFriendlySidePlayerTurn())
                    {
                        TurnStartManager.Get().BeginListeningForTurnEvents();
                    }
                    this.<>f__this.StartCoroutine(this.<>f__this.WaitForOpponentToFinishMulligan());
                    goto Label_110B;

                case 12:
                    this.<>f__this.EndMulligan();
                    goto Label_110B;

                case 13:
                    goto Label_0F1A;

                case 14:
                    goto Label_10BE;

                default:
                    goto Label_110B;
            }
            object[] args = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", 1.5f, "delay", 0.1875f };
            iTween.RotateTo(this.<bonusCard>__20, iTween.Hash(args));
            this.$current = new WaitForSeconds(0.04f);
            this.$PC = 8;
            goto Label_110D;
        Label_0B9B:
            this.$current = new WaitForSeconds(1.75f);
            this.$PC = 9;
            goto Label_110D;
        Label_0F1A:
            while (this.<>f__this.mulliganButton == null)
            {
                this.$current = null;
                this.$PC = 13;
                goto Label_110D;
            }
            this.<>f__this.mulliganButton.transform.position = new Vector3(this.<>f__this.startingHandZone.transform.position.x, this.<>f__this.friendlySideHandZone.transform.position.y, this.<>f__this.myHeroCardActor.transform.position.z);
            this.<>f__this.mulliganButton.transform.localEulerAngles = new Vector3(90f, 90f, 90f);
            this.<>f__this.mulliganButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.OnMulliganButtonReleased));
            this.<>f__this.StartCoroutine(this.<>f__this.WaitAFrameBeforeSendingEventToMulliganButton());
            if (!GameMgr.Get().IsSpectator() && !Options.Get().GetBool(Option.HAS_SEEN_MULLIGAN, false))
            {
                this.<>f__this.innkeeperMulliganDialog = NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_MULLIGAN_13"), "VO_INNKEEPER_MULLIGAN_13", 0f, null);
                Options.Get().SetBool(Option.HAS_SEEN_MULLIGAN, true);
                this.<>f__this.mulliganButton.GetComponent<Collider>().enabled = false;
            }
            MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_MulliganSoft);
            this.<>f__this.m_waitingForUserInput = true;
        Label_10BE:
            while (this.<>f__this.innkeeperMulliganDialog != null)
            {
                this.$current = null;
                this.$PC = 14;
                goto Label_110D;
            }
            this.<>f__this.mulliganButton.GetComponent<Collider>().enabled = true;
            if (Cheats.Get().QuickGameSkipMulligan())
            {
                this.<>f__this.BeginDealNewCards();
            }
            this.$PC = -1;
        Label_110B:
            return false;
        Label_110D:
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
    private sealed class <EnableHandCollidersAfterCardsAreDealt>c__IteratorBA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_598>__0;
        internal MulliganManager <>f__this;
        internal Card <card>__1;

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
                    if (!this.<>f__this.friendlyPlayerHasReplacementCards)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<$s_598>__0 = this.<>f__this.friendlySideHandZone.GetCards().GetEnumerator();
                    try
                    {
                        while (this.<$s_598>__0.MoveNext())
                        {
                            this.<card>__1 = this.<$s_598>__0.Current;
                            this.<card>__1.GetActor().TurnOnCollider();
                        }
                    }
                    finally
                    {
                        this.<$s_598>__0.Dispose();
                    }
                    break;

                default:
                    break;
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
    private sealed class <EndMulliganWithTiming>c__IteratorB8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_597>__0;
        internal MulliganManager <>f__this;
        internal Card <card>__1;
        internal Collider <dragPlane>__2;

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
                    if (!this.<>f__this.ShouldHandleCoinCard())
                    {
                        UnityEngine.Object.Destroy(this.<>f__this.coinObject);
                        break;
                    }
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.HandleCoinCard());
                    this.$PC = 1;
                    return true;

                case 1:
                    break;

                default:
                    goto Label_01E4;
            }
            this.<>f__this.myHeroCardActor.TurnOnCollider();
            this.<>f__this.hisHeroCardActor.TurnOnCollider();
            this.<>f__this.FadeOutMulliganMusicAndStartGameplayMusic();
            this.<$s_597>__0 = this.<>f__this.friendlySideHandZone.GetCards().GetEnumerator();
            try
            {
                while (this.<$s_597>__0.MoveNext())
                {
                    this.<card>__1 = this.<$s_597>__0.Current;
                    this.<card>__1.GetActor().TurnOnCollider();
                    this.<card>__1.GetActor().ToggleForceIdle(false);
                }
            }
            finally
            {
                this.<$s_597>__0.Dispose();
            }
            if (!this.<>f__this.friendlyPlayerHasReplacementCards)
            {
                this.<>f__this.StartCoroutine(this.<>f__this.EnableHandCollidersAfterCardsAreDealt());
            }
            this.<dragPlane>__2 = Board.Get().FindCollider("DragPlane");
            this.<dragPlane>__2.enabled = true;
            this.<>f__this.mulliganActive = false;
            Board.Get().RaiseTheLights();
            this.<>f__this.FadeHeroPowerIn(GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard());
            this.<>f__this.FadeHeroPowerIn(GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard());
            InputManager.Get().OnMulliganEnded();
            EndTurnButton.Get().OnMulliganEnded();
            GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
            this.<>f__this.StartCoroutine(this.<>f__this.WaitForBoardAnimToCompleteThenStartTurn());
            this.$PC = -1;
        Label_01E4:
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
    private sealed class <HandleCoinCard>c__IteratorB9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal Card <coinCard>__1;
        internal PlayMakerFSM <coinFSM>__0;

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
                    if (this.<>f__this.friendlyPlayerGoesFirst)
                    {
                        UnityEngine.Object.Destroy(this.<>f__this.coinObject);
                        this.<>f__this.m_startingOppCards[this.<>f__this.m_coinCardIndex].SetDoNotSort(false);
                        this.<>f__this.opposingSideHandZone.UpdateLayout();
                        goto Label_01B0;
                    }
                    if (!this.<>f__this.coinObject.activeSelf)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    goto Label_01B9;

                case 1:
                    this.<coinFSM>__0 = this.<>f__this.coinObject.GetComponentInChildren<PlayMakerFSM>();
                    this.<coinFSM>__0.SendEvent("Birth");
                    this.$current = new WaitForSeconds(0.1f);
                    this.$PC = 2;
                    goto Label_01B9;

                case 2:
                    break;

                case 3:
                    goto Label_01B0;

                default:
                    goto Label_01B7;
            }
            if (!GameMgr.Get().IsSpectator() && !Options.Get().GetBool(Option.HAS_SEEN_THE_COIN, false))
            {
                NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_COIN_INTRO"), "VO_INNKEEPER_COIN_INTRO", 0f, null);
                Options.Get().SetBool(Option.HAS_SEEN_THE_COIN, true);
            }
            this.<coinCard>__1 = this.<>f__this.GetCoinCardFromFriendlyHand();
            this.<>f__this.PutCoinCardInSpawnPosition(this.<coinCard>__1);
            this.<coinCard>__1.ActivateActorSpell(SpellType.SUMMON_IN, new Spell.FinishedCallback(this.<>f__this.CoinCardSummonFinishedCallback));
            this.$current = new WaitForSeconds(1f);
            this.$PC = 3;
            goto Label_01B9;
        Label_01B0:
            this.$PC = -1;
        Label_01B7:
            return false;
        Label_01B9:
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
    private sealed class <PlayStartingTaunts>c__IteratorB1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal CardSoundSpell <emoteSpell>__2;
        internal EmoteType <emoteToPlay>__5;
        internal Card <heroCard>__0;
        internal Card <heroPowerCard>__1;
        internal Card <myHeroCard>__3;
        internal Card <myHeroPowerCard>__4;

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
                    this.<heroCard>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
                    this.<heroPowerCard>__1 = GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard();
                    iTween.StopByName(this.<>f__this.gameObject, "HisHeroLightBlend");
                    if (this.<heroPowerCard>__1 != null)
                    {
                        GameState.Get().GetGameEntity().FadeInActor(this.<heroPowerCard>__1.GetActor(), 0.4f);
                    }
                    GameState.Get().GetGameEntity().FadeInHeroActor(this.<heroCard>__0.GetActor());
                    this.<emoteSpell>__2 = this.<heroCard>__0.PlayEmote(EmoteType.START);
                    if (this.<emoteSpell>__2 == null)
                    {
                        this.$current = new WaitForSeconds(2.5f);
                        this.$PC = 2;
                        goto Label_02FE;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0122;

                case 3:
                    goto Label_027F;

                case 4:
                    goto Label_02B0;

                default:
                    goto Label_02FC;
            }
            while (this.<emoteSpell>__2.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_02FE;
            }
        Label_0122:
            GameState.Get().GetGameEntity().FadeOutHeroActor(this.<heroCard>__0.GetActor());
            if (this.<heroPowerCard>__1 != null)
            {
                GameState.Get().GetGameEntity().FadeOutActor(this.<heroPowerCard>__1.GetActor());
            }
            this.<myHeroCard>__3 = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
            this.<myHeroPowerCard>__4 = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
            if (MulliganManager.Get() == null)
            {
                goto Label_02FC;
            }
            iTween.StopByName(this.<>f__this.gameObject, "MyHeroLightBlend");
            if (this.<myHeroPowerCard>__4 != null)
            {
                GameState.Get().GetGameEntity().FadeInActor(this.<myHeroPowerCard>__4.GetActor(), 0.4f);
            }
            this.<emoteToPlay>__5 = EmoteType.START;
            if (this.<myHeroCard>__3.GetEntity().GetCardId() == this.<heroCard>__0.GetEntity().GetCardId())
            {
                this.<emoteToPlay>__5 = EmoteType.MIRROR_START;
            }
            GameState.Get().GetGameEntity().FadeInHeroActor(this.<myHeroCard>__3.GetActor());
            this.<emoteSpell>__2 = this.<myHeroCard>__3.PlayEmote(this.<emoteToPlay>__5, Notification.SpeechBubbleDirection.BottomRight);
            if (this.<emoteSpell>__2 == null)
            {
                this.$current = new WaitForSeconds(2.5f);
                this.$PC = 4;
                goto Label_02FE;
            }
        Label_027F:
            while (this.<emoteSpell>__2.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_02FE;
            }
        Label_02B0:
            GameState.Get().GetGameEntity().FadeOutHeroActor(this.<myHeroCard>__3.GetActor());
            if (this.<myHeroPowerCard>__4 != null)
            {
                GameState.Get().GetGameEntity().FadeOutActor(this.<myHeroPowerCard>__4.GetActor());
            }
            this.$PC = -1;
        Label_02FC:
            return false;
        Label_02FE:
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
    private sealed class <RemoveOldCardsAnimation>c__IteratorB5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_594>__1;
        internal List<Card>.Enumerator <$s_595>__9;
        internal MulliganManager <>f__this;
        internal string <animName>__21;
        internal Card <card>__10;
        internal Card <card>__2;
        internal Animation <cardAnim>__20;
        internal Animation <cardAnim>__7;
        internal float <cardHeightOffset>__15;
        internal GameObject <cardObject>__5;
        internal float <cardZpos>__16;
        internal Vector3[] <drawPath>__19;
        internal Vector3[] <drawPath>__6;
        internal List<Card> <handZoneCards>__8;
        internal int <i>__17;
        internal int <i>__4;
        internal float <leftSideOfZone>__13;
        internal Vector3 <mulliganedCardsPosition>__0;
        internal float <spaceForEachCard>__12;
        internal float <TO_DECK_ANIMATION_TIME>__3;
        internal GameObject <topCard>__18;
        internal float <xOffset>__14;
        internal float <zoneWidth>__11;

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
                    this.<>f__this.m_waitingForUserInput = false;
                    this.<>f__this.DestroyMulliganTimer();
                    SoundManager.Get().LoadAndPlay("FX_GameStart28_CardDismissWoosh2_v2");
                    this.<mulliganedCardsPosition>__0 = Board.Get().FindBone("MulliganedCardsPosition").position;
                    this.<>f__this.DestroyXobjects();
                    this.<>f__this.DestroyChooseBanner();
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        Gameplay.Get().RemoveClassNames();
                    }
                    this.<$s_594>__1 = this.<>f__this.m_startingCards.GetEnumerator();
                    try
                    {
                        while (this.<$s_594>__1.MoveNext())
                        {
                            this.<card>__2 = this.<$s_594>__1.Current;
                            this.<card>__2.GetActor().SetActorState(ActorStateType.CARD_IDLE);
                            this.<card>__2.GetActor().ToggleForceIdle(true);
                            this.<card>__2.GetActor().TurnOffCollider();
                        }
                    }
                    finally
                    {
                        this.<$s_594>__1.Dispose();
                    }
                    this.<>f__this.StartCoroutine(this.<>f__this.RemoveUIButtons());
                    this.<TO_DECK_ANIMATION_TIME>__3 = 1.5f;
                    this.<i>__4 = 0;
                    while (this.<i>__4 < this.<>f__this.m_startingCards.Count)
                    {
                        if (this.<>f__this.m_handCardsMarkedForReplace[this.<i>__4])
                        {
                            this.<cardObject>__5 = this.<>f__this.m_startingCards[this.<i>__4].gameObject;
                            this.<drawPath>__6 = new Vector3[] { this.<cardObject>__5.transform.position, new Vector3(this.<cardObject>__5.transform.position.x + 2f, this.<cardObject>__5.transform.position.y - 1.7f, this.<cardObject>__5.transform.position.z), new Vector3(this.<mulliganedCardsPosition>__0.x, this.<mulliganedCardsPosition>__0.y, this.<mulliganedCardsPosition>__0.z), this.<>f__this.friendlySideDeck.transform.position };
                            object[] args = new object[] { "path", this.<drawPath>__6, "time", this.<TO_DECK_ANIMATION_TIME>__3, "easetype", iTween.EaseType.easeOutCubic };
                            iTween.MoveTo(this.<cardObject>__5, iTween.Hash(args));
                            this.<cardAnim>__7 = this.<cardObject>__5.GetComponent<Animation>();
                            if (this.<cardAnim>__7 == null)
                            {
                                this.<cardAnim>__7 = this.<cardObject>__5.AddComponent<Animation>();
                            }
                            this.<cardAnim>__7.AddClip(this.<>f__this.cardAnimatesFromBoardToDeck, "putCardBack");
                            this.<cardAnim>__7.Play("putCardBack");
                            this.$current = new WaitForSeconds(0.5f);
                            this.$PC = 1;
                            goto Label_09FA;
                        }
                    Label_0347:
                        this.<i>__4++;
                    }
                    InputManager.Get().DoEndTurnButton();
                    break;

                case 1:
                    goto Label_0347;

                case 2:
                    break;

                case 3:
                    goto Label_0904;

                case 4:
                    this.<>f__this.ShuffleDeck();
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 5;
                    goto Label_09FA;

                case 5:
                    if (!this.<>f__this.opponentPlayerHasReplacementCards)
                    {
                        this.<>f__this.StartCoroutine(this.<>f__this.WaitForOpponentToFinishMulligan());
                    }
                    else
                    {
                        this.<>f__this.EndMulligan();
                    }
                    this.$PC = -1;
                    goto Label_09F8;

                default:
                    goto Label_09F8;
            }
            while (!this.<>f__this.friendlyPlayerHasReplacementCards)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_09FA;
            }
            this.<>f__this.SortHand(this.<>f__this.friendlySideHandZone);
            this.<handZoneCards>__8 = this.<>f__this.friendlySideHandZone.GetCards();
            this.<$s_595>__9 = this.<handZoneCards>__8.GetEnumerator();
            try
            {
                while (this.<$s_595>__9.MoveNext())
                {
                    this.<card>__10 = this.<$s_595>__9.Current;
                    if (!this.<>f__this.IsCoinCard(this.<card>__10))
                    {
                        this.<card>__10.GetActor().SetActorState(ActorStateType.CARD_IDLE);
                        this.<card>__10.GetActor().ToggleForceIdle(true);
                    }
                }
            }
            finally
            {
                this.<$s_595>__9.Dispose();
            }
            this.<zoneWidth>__11 = this.<>f__this.startingHandZone.GetComponent<Collider>().bounds.size.x;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<zoneWidth>__11 *= 0.55f;
            }
            this.<spaceForEachCard>__12 = this.<zoneWidth>__11 / ((float) this.<>f__this.m_startingCards.Count);
            this.<leftSideOfZone>__13 = this.<>f__this.startingHandZone.transform.position.x - (this.<zoneWidth>__11 / 2f);
            this.<xOffset>__14 = 0f;
            this.<cardHeightOffset>__15 = 0f;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<cardHeightOffset>__15 = 7f;
            }
            this.<cardZpos>__16 = this.<>f__this.startingHandZone.transform.position.z - 0.3f;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<cardZpos>__16 = this.<>f__this.startingHandZone.transform.position.z - 0.2f;
            }
            this.<i>__17 = 0;
            while (this.<i>__17 < this.<>f__this.m_startingCards.Count)
            {
                if (!this.<>f__this.m_handCardsMarkedForReplace[this.<i>__17])
                {
                    goto Label_093B;
                }
                this.<topCard>__18 = this.<handZoneCards>__8[this.<i>__17].gameObject;
                iTween.Stop(this.<topCard>__18);
                object[] objArray2 = new object[] { "position", new Vector3(((this.<leftSideOfZone>__13 + this.<spaceForEachCard>__12) + this.<xOffset>__14) - (this.<spaceForEachCard>__12 / 2f), this.<>f__this.friendlySideHandZone.GetComponent<Collider>().bounds.center.y, this.<>f__this.startingHandZone.transform.position.z), "time", 3f };
                iTween.MoveTo(this.<topCard>__18, iTween.Hash(objArray2));
                this.<drawPath>__19 = new Vector3[4];
                this.<drawPath>__19[0] = this.<topCard>__18.transform.position;
                this.<drawPath>__19[1] = new Vector3(this.<mulliganedCardsPosition>__0.x, this.<mulliganedCardsPosition>__0.y, this.<mulliganedCardsPosition>__0.z);
                this.<drawPath>__19[3] = new Vector3(((this.<leftSideOfZone>__13 + this.<spaceForEachCard>__12) + this.<xOffset>__14) - (this.<spaceForEachCard>__12 / 2f), this.<>f__this.friendlySideHandZone.GetComponent<Collider>().bounds.center.y + this.<cardHeightOffset>__15, this.<cardZpos>__16);
                this.<drawPath>__19[2] = new Vector3(this.<drawPath>__19[3].x + 2f, this.<drawPath>__19[3].y - 1.7f, this.<drawPath>__19[3].z);
                object[] objArray3 = new object[] { "path", this.<drawPath>__19, "time", this.<TO_DECK_ANIMATION_TIME>__3, "easetype", iTween.EaseType.easeInCubic };
                iTween.MoveTo(this.<topCard>__18, iTween.Hash(objArray3));
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    iTween.ScaleTo(this.<topCard>__18, new Vector3(0.9f, 1.1f, 0.9f), 1.5f);
                }
                else
                {
                    iTween.ScaleTo(this.<topCard>__18, new Vector3(1.1f, 1.1f, 1.1f), 1.5f);
                }
                this.<cardAnim>__20 = this.<topCard>__18.GetComponent<Animation>();
                if (this.<cardAnim>__20 == null)
                {
                    this.<cardAnim>__20 = this.<topCard>__18.AddComponent<Animation>();
                }
                this.<animName>__21 = "putCardBack";
                this.<cardAnim>__20.AddClip(this.<>f__this.cardAnimatesFromBoardToDeck, this.<animName>__21);
                this.<cardAnim>__20[this.<animName>__21].normalizedTime = 1f;
                this.<cardAnim>__20[this.<animName>__21].speed = -1f;
                this.<cardAnim>__20.Play(this.<animName>__21);
                this.$current = new WaitForSeconds(0.5f);
                this.$PC = 3;
                goto Label_09FA;
            Label_0904:
                if (this.<topCard>__18.GetComponent<AudioSource>() == null)
                {
                    this.<topCard>__18.AddComponent<AudioSource>();
                }
                SoundManager.Get().LoadAndPlay("FX_GameStart30_CardReplaceSingle", this.<topCard>__18);
            Label_093B:
                this.<xOffset>__14 += this.<spaceForEachCard>__12;
                this.<i>__17++;
            }
            this.$current = new WaitForSeconds(1f);
            this.$PC = 4;
            goto Label_09FA;
        Label_09F8:
            return false;
        Label_09FA:
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
    private sealed class <RemoveUIButtons>c__IteratorB7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal int <i>__0;

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
                    if (this.<>f__this.mulliganButton != null)
                    {
                        this.<>f__this.mulliganButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
                    }
                    if (this.<>f__this.m_replaceLabels != null)
                    {
                        this.<i>__0 = 0;
                        while (this.<i>__0 < this.<>f__this.m_replaceLabels.Count)
                        {
                            if (this.<>f__this.m_replaceLabels[this.<i>__0] != null)
                            {
                                object[] args = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", 0.5f, "easetype", iTween.EaseType.easeInExpo };
                                iTween.RotateTo(this.<>f__this.m_replaceLabels[this.<i>__0].gameObject, iTween.Hash(args));
                                object[] objArray2 = new object[] { "scale", new Vector3(0.001f, 0.001f, 0.001f), "time", 0.5f, "easetype", iTween.EaseType.easeInExpo, "oncomplete", "DestroyButton", "oncompletetarget", this.<>f__this.gameObject, "oncompleteparams", this.<>f__this.m_replaceLabels[this.<i>__0] };
                                iTween.ScaleTo(this.<>f__this.m_replaceLabels[this.<i>__0].gameObject, iTween.Hash(objArray2));
                                this.$current = new WaitForSeconds(0.05f);
                                this.$PC = 1;
                                goto Label_0263;
                            }
                        Label_01EA:
                            this.<i>__0++;
                        }
                    }
                    this.$current = new WaitForSeconds(3.5f);
                    this.$PC = 2;
                    goto Label_0263;

                case 1:
                    goto Label_01EA;

                case 2:
                    if (this.<>f__this.mulliganButton != null)
                    {
                        UnityEngine.Object.Destroy(this.<>f__this.mulliganButton.gameObject);
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0263:
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
    private sealed class <ResumeMulligan>c__IteratorAE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_588>__0;
        internal MulliganManager <>f__this;
        internal TAG_MULLIGAN <mulliganState>__2;
        internal Player <player>__1;

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
                    this.<>f__this.m_resuming = true;
                    this.<$s_588>__0 = GameState.Get().GetPlayerMap().Values.GetEnumerator();
                    try
                    {
                        while (this.<$s_588>__0.MoveNext())
                        {
                            this.<player>__1 = this.<$s_588>__0.Current;
                            this.<mulliganState>__2 = this.<player>__1.GetTag<TAG_MULLIGAN>(GAME_TAG.MULLIGAN_STATE);
                            if (this.<mulliganState>__2 == TAG_MULLIGAN.DONE)
                            {
                                if (this.<player>__1.IsFriendlySide())
                                {
                                    this.<>f__this.friendlyPlayerHasReplacementCards = true;
                                }
                                else
                                {
                                    this.<>f__this.opponentPlayerHasReplacementCards = true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_588>__0.Dispose();
                    }
                    if (this.<>f__this.friendlyPlayerHasReplacementCards)
                    {
                        this.<>f__this.skipCardChoosing = true;
                        goto Label_011B;
                    }
                    break;

                case 1:
                    break;

                default:
                    goto Label_012D;
            }
            while (GameState.Get().GetResponseMode() != GameState.ResponseMode.CHOICE)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_011B:
            this.<>f__this.BeginMulligan();
            this.$PC = -1;
        Label_012D:
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
    private sealed class <SampleAnimFrame>c__IteratorBE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>animName;
        internal Animation <$>animToUse;
        internal float <$>startSec;
        internal AnimationState <state>__0;
        internal string animName;
        internal Animation animToUse;
        internal float startSec;

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
                    this.<state>__0 = this.animToUse[this.animName];
                    this.<state>__0.enabled = true;
                    this.<state>__0.time = this.startSec;
                    this.animToUse.Play(this.animName);
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<state>__0.enabled = false;
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
    private sealed class <ShouldWaitForMulliganCardsToBeProcessed>c__AnonStorey2FA
    {
        internal MulliganManager <>f__this;
        internal bool receivedEndOfMulligan;

        internal void <>m__100(int index, PowerTaskList taskList)
        {
            if (this.<>f__this.IsTaskListPuttingUsPastMulligan(taskList))
            {
                this.receivedEndOfMulligan = true;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShrinkStartingHandBanner>c__IteratorBF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>banner;
        internal GameObject banner;

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
                    goto Label_0095;

                case 1:
                    iTween.ScaleTo(this.banner, new Vector3(0f, 0f, 0f), 0.5f);
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 2;
                    goto Label_0095;

                case 2:
                    UnityEngine.Object.Destroy(this.banner);
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0095:
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
    private sealed class <SkipMulliganForResume>c__IteratorBB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal IEnumerator <$s_599>__1;
        internal MulliganManager <>f__this;
        internal SoundDuckedCategoryDef <categoryDef>__3;
        internal Collider <dragPlane>__4;
        internal SoundDucker <ducker>__0;
        internal SoundCategory <soundCategory>__2;

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
                    this.<>f__this.introComplete = true;
                    this.<ducker>__0 = null;
                    if (!GameMgr.Get().IsSpectator())
                    {
                        this.<ducker>__0 = this.<>f__this.gameObject.AddComponent<SoundDucker>();
                        this.<ducker>__0.m_DuckedCategoryDefs = new List<SoundDuckedCategoryDef>();
                        this.<$s_599>__1 = Enum.GetValues(typeof(SoundCategory)).GetEnumerator();
                        try
                        {
                            while (this.<$s_599>__1.MoveNext())
                            {
                                this.<soundCategory>__2 = (SoundCategory) ((int) this.<$s_599>__1.Current);
                                if ((this.<soundCategory>__2 != SoundCategory.AMBIENCE) && (this.<soundCategory>__2 != SoundCategory.MUSIC))
                                {
                                    this.<categoryDef>__3 = new SoundDuckedCategoryDef();
                                    this.<categoryDef>__3.m_Category = this.<soundCategory>__2;
                                    this.<categoryDef>__3.m_Volume = 0f;
                                    this.<categoryDef>__3.m_RestoreSec = 5f;
                                    this.<categoryDef>__3.m_BeginSec = 0f;
                                    this.<ducker>__0.m_DuckedCategoryDefs.Add(this.<categoryDef>__3);
                                }
                            }
                        }
                        finally
                        {
                            IDisposable disposable = this.<$s_599>__1 as IDisposable;
                            if (disposable == null)
                            {
                            }
                            disposable.Dispose();
                        }
                        this.<ducker>__0.StartDucking();
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_01B3;

                case 3:
                    goto Label_0249;

                case 4:
                    goto Label_0270;

                case 5:
                    goto Label_02B0;

                default:
                    goto Label_03BC;
            }
            while (Board.Get() == null)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_03BE;
            }
            Board.Get().RaiseTheLightsQuickly();
        Label_01B3:
            while (ZoneMgr.Get() == null)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_03BE;
            }
            this.<>f__this.InitZones();
            this.<dragPlane>__4 = Board.Get().FindCollider("DragPlane");
            this.<>f__this.friendlySideHandZone.SetDoNotUpdateLayout(false);
            this.<>f__this.opposingSideHandZone.SetDoNotUpdateLayout(false);
            this.<dragPlane>__4.enabled = false;
            this.<>f__this.friendlySideHandZone.AddInputBlocker();
            this.<>f__this.opposingSideHandZone.AddInputBlocker();
        Label_0249:
            while (!GameState.Get().IsGameCreated())
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_03BE;
            }
        Label_0270:
            while (ZoneMgr.Get().HasActiveServerChange())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_03BE;
            }
            GameState.Get().GetGameEntity().NotifyOfMulliganInitialized();
            SceneMgr.Get().NotifySceneLoaded();
        Label_02B0:
            while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_03BE;
            }
            if (this.<ducker>__0 != null)
            {
                this.<ducker>__0.StopDucking();
                UnityEngine.Object.Destroy(this.<ducker>__0);
            }
            if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY)
            {
                this.<>f__this.FadeOutMulliganMusicAndStartGameplayMusic();
            }
            this.<dragPlane>__4.enabled = true;
            this.<>f__this.friendlySideHandZone.RemoveInputBlocker();
            this.<>f__this.opposingSideHandZone.RemoveInputBlocker();
            this.<>f__this.friendlySideDeck.SetSuppressEmotes(false);
            this.<>f__this.opposingSideDeck.SetSuppressEmotes(false);
            if (GameState.Get().GetResponseMode() == GameState.ResponseMode.CHOICE)
            {
                GameState.Get().UpdateChoiceHighlights();
            }
            GameMgr.Get().UpdatePresence();
            InputManager.Get().OnMulliganEnded();
            EndTurnButton.Get().OnMulliganEnded();
            GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
            UnityEngine.Object.Destroy(this.<>f__this.gameObject);
            this.$PC = -1;
        Label_03BC:
            return false;
        Label_03BE:
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
    private sealed class <SkipMulliganWhenIntroComplete>c__IteratorBC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal Collider <dragPlane>__0;

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
                    this.<>f__this.m_waitingForUserInput = false;
                    break;

                case 1:
                    break;

                default:
                    goto Label_01C5;
            }
            if (!this.<>f__this.introComplete)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.myHeroCardActor.TurnOnCollider();
            this.<>f__this.hisHeroCardActor.TurnOnCollider();
            this.<>f__this.FadeOutMulliganMusicAndStartGameplayMusic();
            this.<>f__this.myHeroCardActor.GetHealthObject().Show();
            this.<>f__this.hisHeroCardActor.GetHealthObject().Show();
            this.<dragPlane>__0 = Board.Get().FindCollider("DragPlane");
            this.<dragPlane>__0.enabled = true;
            Board.Get().SplitSurface();
            this.<>f__this.FadeHeroPowerIn(GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard());
            this.<>f__this.FadeHeroPowerIn(GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard());
            this.<>f__this.mulliganActive = false;
            this.<>f__this.InitZones();
            this.<>f__this.friendlySideHandZone.SetDoNotUpdateLayout(false);
            this.<>f__this.friendlySideHandZone.UpdateLayout();
            this.<>f__this.opposingSideHandZone.SetDoNotUpdateLayout(false);
            this.<>f__this.opposingSideHandZone.UpdateLayout();
            this.<>f__this.friendlySideDeck.SetSuppressEmotes(false);
            this.<>f__this.opposingSideDeck.SetSuppressEmotes(false);
            InputManager.Get().OnMulliganEnded();
            EndTurnButton.Get().OnMulliganEnded();
            GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
            this.<>f__this.StartCoroutine(this.<>f__this.WaitForBoardAnimToCompleteThenStartTurn());
            this.$PC = -1;
        Label_01C5:
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
    private sealed class <Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen>c__IteratorAF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Network.EntitiesChosen <$>chosen;
        internal MulliganManager <>f__this;
        internal Card <card>__1;
        internal int <cardIndex>__0;
        internal int <entityId>__2;
        internal bool <spectateeMarkedReplaced>__3;
        internal Network.EntitiesChosen chosen;

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
                    if (!this.<>f__this.m_waitingForUserInput)
                    {
                        if (!GameState.Get().IsGameOver() && !this.<>f__this.skipCardChoosing)
                        {
                            this.$current = null;
                            this.$PC = 1;
                            return true;
                        }
                        break;
                    }
                    this.<cardIndex>__0 = 0;
                    while (this.<cardIndex>__0 < this.<>f__this.m_startingCards.Count)
                    {
                        this.<card>__1 = this.<>f__this.m_startingCards[this.<cardIndex>__0];
                        this.<entityId>__2 = this.<card>__1.GetEntity().GetEntityId();
                        this.<spectateeMarkedReplaced>__3 = !this.chosen.Entities.Contains(this.<entityId>__2);
                        if (this.<>f__this.m_handCardsMarkedForReplace[this.<cardIndex>__0] != this.<spectateeMarkedReplaced>__3)
                        {
                            this.<>f__this.ToggleHoldState(this.<cardIndex>__0);
                        }
                        this.<cardIndex>__0++;
                    }
                    GameState.Get().OnEntitiesChosenProcessed(this.chosen);
                    this.<>f__this.BeginDealNewCards();
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
    private sealed class <WaitAFrameBeforeSendingEventToMulliganButton>c__IteratorB3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;

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
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.mulliganButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Birth");
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
    private sealed class <WaitForBoardAnimToCompleteThenStartTurn>c__IteratorBD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;

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
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    GameState.Get().SetMulliganPowerBlocker(false);
                    UnityEngine.Object.Destroy(this.<>f__this.gameObject);
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
    private sealed class <WaitForBoardThenLoadButton>c__IteratorAC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;

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
                    if (BoardStandardGame.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    AssetLoader.Get().LoadActor("MulliganButton", new AssetLoader.GameObjectCallback(this.<>f__this.OnMulliganButtonLoaded), null, false);
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
    private sealed class <WaitForHeroesAndStartAnimations>c__IteratorAD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal bool <alternateIntro>__10;
        internal Animation <cardAnim>__16;
        internal string <className>__15;
        internal TAG_CLASS <classTag>__14;
        internal Player <friendlyPlayer>__0;
        internal Card <heroCard>__2;
        internal Card <heroCard>__3;
        internal GameObject <heroLabelCopy>__13;
        internal Card <heroPowerCard>__4;
        internal Card <heroPowerCard>__5;
        internal Vector3 <hisActorScale>__22;
        internal GameObject <hisHero>__12;
        internal Material <hisHeroFrameMat>__9;
        internal AudioSource <hisHeroLine>__19;
        internal Material <hisHeroMat>__8;
        internal GameObject <hisHeroSocketBone>__23;
        internal Hashtable <hisLightBlendArgs>__27;
        internal Vector3 <myActorScale>__20;
        internal GameObject <myHero>__11;
        internal Material <myHeroFrameMat>__7;
        internal AudioSource <myHeroLine>__18;
        internal Material <myHeroMat>__6;
        internal GameObject <myHeroSocketBone>__21;
        internal Hashtable <myLightBlendArgs>__25;
        internal Action<object> <OnHisLightBlendUpdate>__26;
        internal Action<object> <OnMyLightBlendUpdate>__24;
        internal Animation <oppCardAnim>__17;
        internal Player <opposingPlayer>__1;

        internal void <>m__101(Spell spell, SpellStateType prevStateType, object userData)
        {
            this.<>f__this.myHeroCardActor.transform.position = this.<myHeroSocketBone>__21.transform.position;
            this.<>f__this.myHeroCardActor.transform.localScale = Vector3.one;
        }

        internal void <>m__102(Spell spell, SpellStateType prevStateType, object userData)
        {
            this.<>f__this.hisHeroCardActor.transform.position = this.<hisHeroSocketBone>__23.transform.position;
            this.<>f__this.hisHeroCardActor.transform.localScale = Vector3.one;
        }

        internal void <>m__103(object amount)
        {
            this.<myHeroMat>__6.SetFloat("_LightingBlend", (float) amount);
            this.<myHeroFrameMat>__7.SetFloat("_LightingBlend", (float) amount);
        }

        internal void <>m__104(object amount)
        {
            this.<hisHeroMat>__8.SetFloat("_LightingBlend", (float) amount);
            this.<hisHeroFrameMat>__9.SetFloat("_LightingBlend", (float) amount);
        }

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
                    Log.LoadingScreen.Print("MulliganManager.WaitForHeroesAndStartAnimations()", new object[0]);
                    this.<friendlyPlayer>__0 = GameState.Get().GetFriendlySidePlayer();
                    this.<opposingPlayer>__1 = GameState.Get().GetOpposingSidePlayer();
                    break;

                case 1:
                    this.<friendlyPlayer>__0 = GameState.Get().GetFriendlySidePlayer();
                    break;

                case 2:
                    goto Label_00FD;

                case 3:
                    goto Label_0168;

                case 4:
                    goto Label_01CE;

                case 5:
                    goto Label_025A;

                case 6:
                    goto Label_02F6;

                case 7:
                    goto Label_0334;

                case 8:
                    goto Label_036A;

                case 9:
                    goto Label_039E;

                case 10:
                    goto Label_0408;

                case 11:
                    goto Label_0651;

                case 12:
                    goto Label_06A9;

                case 13:
                    goto Label_0A8C;

                case 14:
                    goto Label_0AF5;

                case 15:
                    SoundManager.Get().PlayPreloaded(this.<>f__this.versusVo);
                    goto Label_0B55;

                case 0x10:
                    goto Label_0B55;

                case 0x11:
                    this.<hisHeroLine>__19 = this.<>f__this.hisHeroCardActor.GetCard().GetAnnouncerLine();
                    if ((this.<hisHeroLine>__19 == null) || (this.<hisHeroLine>__19.clip == null))
                    {
                        goto Label_0C0D;
                    }
                    SoundManager.Get().Play(this.<hisHeroLine>__19);
                    goto Label_0BF8;

                case 0x12:
                    goto Label_0BF8;

                case 0x13:
                    this.<>f__this.myheroLabel.transform.parent = null;
                    this.<>f__this.hisheroLabel.transform.parent = null;
                    this.<>f__this.myheroLabel.FadeOut();
                    this.<>f__this.hisheroLabel.FadeOut();
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 20;
                    goto Label_148A;

                case 20:
                    if (this.<>f__this.m_MyCustomSocketInSpell == null)
                    {
                        this.<cardAnim>__16["hisHeroAnimateToPosition"].enabled = true;
                    }
                    else
                    {
                        this.<>f__this.m_MyCustomSocketInSpell.m_Location = SpellLocation.NONE;
                        this.<>f__this.m_MyCustomSocketInSpell.gameObject.SetActive(true);
                        if (this.<>f__this.myHeroCardActor.GetCardDef().m_SocketInParentEffectToHero)
                        {
                            this.<myActorScale>__20 = this.<>f__this.myHeroCardActor.transform.localScale;
                            this.<>f__this.myHeroCardActor.transform.localScale = Vector3.one;
                            this.<>f__this.m_MyCustomSocketInSpell.transform.parent = this.<>f__this.myHeroCardActor.transform;
                            this.<>f__this.m_MyCustomSocketInSpell.transform.localPosition = Vector3.zero;
                            this.<>f__this.myHeroCardActor.transform.localScale = this.<myActorScale>__20;
                        }
                        this.<>f__this.m_MyCustomSocketInSpell.SetSource(this.<>f__this.myHeroCardActor.GetCard().gameObject);
                        this.<>f__this.m_MyCustomSocketInSpell.RemoveAllTargets();
                        this.<myHeroSocketBone>__21 = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).gameObject;
                        this.<>f__this.m_MyCustomSocketInSpell.AddTarget(this.<myHeroSocketBone>__21);
                        this.<>f__this.m_MyCustomSocketInSpell.ActivateState(SpellStateType.BIRTH);
                        this.<>f__this.m_MyCustomSocketInSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>m__101));
                        if (!this.<>f__this.myHeroCardActor.GetCardDef().m_SocketInOverrideHeroAnimation)
                        {
                            this.<cardAnim>__16["hisHeroAnimateToPosition"].enabled = true;
                        }
                    }
                    if (this.<>f__this.m_HisCustomSocketInSpell == null)
                    {
                        this.<oppCardAnim>__17["myHeroAnimateToPosition"].enabled = true;
                        goto Label_1083;
                    }
                    if (this.<>f__this.m_MyCustomSocketInSpell != null)
                    {
                        SoundUtils.SetSourceVolumes(this.<>f__this.m_HisCustomSocketInSpell, 0f, false);
                    }
                    this.<>f__this.m_HisCustomSocketInSpell.m_Location = SpellLocation.NONE;
                    if (this.<>f__this.hisHeroCardActor.GetCardDef().m_SocketInOverrideHeroAnimation)
                    {
                        this.$current = new WaitForSeconds(0.25f);
                        this.$PC = 0x15;
                        goto Label_148A;
                    }
                    goto Label_0EEB;

                case 0x15:
                    goto Label_0EEB;

                case 0x16:
                    this.<>f__this.versusTextObject.GetComponentInChildren<Animation>().Play();
                    this.$current = new WaitForSeconds(0.32f);
                    this.$PC = 0x17;
                    goto Label_148A;

                case 0x17:
                    goto Label_110C;

                case 0x18:
                {
                    object[] args = new object[] { "time", 0.6f, "amount", new Vector3(0.03f, 0.01f, 0.03f) };
                    iTween.ShakePosition(Camera.main.gameObject, iTween.Hash(args));
                    this.<OnMyLightBlendUpdate>__24 = new Action<object>(this.<>m__103);
                    this.<OnMyLightBlendUpdate>__24(0f);
                    object[] objArray2 = new object[] { "time", 1f, "from", 0f, "to", 1f, "delay", 2f, "onupdate", this.<OnMyLightBlendUpdate>__24, "onupdatetarget", this.<>f__this.gameObject, "name", "MyHeroLightBlend" };
                    this.<myLightBlendArgs>__25 = iTween.Hash(objArray2);
                    iTween.ValueTo(this.<>f__this.gameObject, this.<myLightBlendArgs>__25);
                    this.<OnHisLightBlendUpdate>__26 = new Action<object>(this.<>m__104);
                    this.<OnHisLightBlendUpdate>__26(0f);
                    object[] objArray3 = new object[] { "time", 1f, "from", 0f, "to", 1f, "delay", 2f, "onupdate", this.<OnHisLightBlendUpdate>__26, "onupdatetarget", this.<>f__this.gameObject, "name", "HisHeroLightBlend" };
                    this.<hisLightBlendArgs>__27 = iTween.Hash(objArray3);
                    iTween.ValueTo(this.<>f__this.gameObject, this.<hisLightBlendArgs>__27);
                    this.<>f__this.introComplete = true;
                    GameState.Get().GetGameEntity().NotifyOfHeroesFinishedAnimatingInMulligan();
                    this.$PC = -1;
                    goto Label_1488;
                }
                default:
                    goto Label_1488;
            }
            if (this.<friendlyPlayer>__0 == null)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_148A;
            }
            while (this.<opposingPlayer>__1 == null)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_148A;
            Label_00FD:
                this.<opposingPlayer>__1 = GameState.Get().GetOpposingSidePlayer();
            }
        Label_0168:
            while (this.<>f__this.myHeroCardActor == null)
            {
                this.<heroCard>__2 = this.<friendlyPlayer>__0.GetHeroCard();
                if (this.<heroCard>__2 != null)
                {
                    this.<>f__this.myHeroCardActor = this.<heroCard>__2.GetActor();
                }
                this.$current = null;
                this.$PC = 3;
                goto Label_148A;
            }
        Label_01CE:
            while (this.<>f__this.hisHeroCardActor == null)
            {
                this.<heroCard>__3 = this.<opposingPlayer>__1.GetHeroCard();
                if (this.<heroCard>__3 != null)
                {
                    this.<>f__this.hisHeroCardActor = this.<heroCard>__3.GetActor();
                }
                this.$current = null;
                this.$PC = 4;
                goto Label_148A;
            }
        Label_025A:
            while ((this.<friendlyPlayer>__0.GetHeroPower() != null) && (this.<>f__this.myHeroPowerCardActor == null))
            {
                this.<heroPowerCard>__4 = this.<friendlyPlayer>__0.GetHeroPowerCard();
                if (this.<heroPowerCard>__4 != null)
                {
                    this.<>f__this.myHeroPowerCardActor = this.<heroPowerCard>__4.GetActor();
                    if (this.<>f__this.myHeroPowerCardActor != null)
                    {
                        this.<>f__this.myHeroPowerCardActor.TurnOffCollider();
                    }
                }
                this.$current = null;
                this.$PC = 5;
                goto Label_148A;
            }
        Label_02F6:
            while ((this.<opposingPlayer>__1.GetHeroPower() != null) && (this.<>f__this.hisHeroPowerCardActor == null))
            {
                this.<heroPowerCard>__5 = this.<opposingPlayer>__1.GetHeroPowerCard();
                if (this.<heroPowerCard>__5 != null)
                {
                    this.<>f__this.hisHeroPowerCardActor = this.<heroPowerCard>__5.GetActor();
                    if (this.<>f__this.hisHeroPowerCardActor != null)
                    {
                        this.<>f__this.hisHeroPowerCardActor.TurnOffCollider();
                    }
                }
                this.$current = null;
                this.$PC = 6;
                goto Label_148A;
            }
        Label_0334:
            while ((GameState.Get() == null) || GameState.Get().GetGameEntity().IsPreloadingAssets())
            {
                this.$current = null;
                this.$PC = 7;
                goto Label_148A;
            }
        Label_036A:
            while (this.<>f__this.myHeroCardActor.GetCardDef() == null)
            {
                this.$current = null;
                this.$PC = 8;
                goto Label_148A;
            }
        Label_039E:
            while (this.<>f__this.hisHeroCardActor.GetCardDef() == null)
            {
                this.$current = null;
                this.$PC = 9;
                goto Label_148A;
            }
            this.<>f__this.LoadMyHeroSkinSocketInEffect(this.<>f__this.myHeroCardActor.GetCardDef());
            this.<>f__this.LoadHisHeroSkinSocketInEffect(this.<>f__this.hisHeroCardActor.GetCardDef());
        Label_0408:
            while (this.<>f__this.m_isLoadingMyCustomSocketIn || this.<>f__this.m_isLoadingHisCustomSocketIn)
            {
                this.$current = null;
                this.$PC = 10;
                goto Label_148A;
            }
            this.<myHeroMat>__6 = this.<>f__this.myHeroCardActor.m_portraitMesh.GetComponent<Renderer>().materials[this.<>f__this.myHeroCardActor.m_portraitMatIdx];
            this.<myHeroFrameMat>__7 = this.<>f__this.myHeroCardActor.m_portraitMesh.GetComponent<Renderer>().materials[this.<>f__this.myHeroCardActor.m_portraitFrameMatIdx];
            if ((this.<myHeroMat>__6 != null) && this.<myHeroMat>__6.HasProperty("_LightingBlend"))
            {
                this.<myHeroMat>__6.SetFloat("_LightingBlend", 0f);
            }
            if ((this.<myHeroFrameMat>__7 != null) && this.<myHeroFrameMat>__7.HasProperty("_LightingBlend"))
            {
                this.<myHeroFrameMat>__7.SetFloat("_LightingBlend", 0f);
            }
            this.<hisHeroMat>__8 = this.<>f__this.hisHeroCardActor.m_portraitMesh.GetComponent<Renderer>().materials[this.<>f__this.hisHeroCardActor.m_portraitMatIdx];
            this.<hisHeroFrameMat>__9 = this.<>f__this.hisHeroCardActor.m_portraitMesh.GetComponent<Renderer>().materials[this.<>f__this.hisHeroCardActor.m_portraitFrameMatIdx];
            if ((this.<hisHeroMat>__8 != null) && this.<hisHeroMat>__8.HasProperty("_LightingBlend"))
            {
                this.<hisHeroMat>__8.SetFloat("_LightingBlend", 0f);
            }
            if ((this.<hisHeroFrameMat>__9 != null) && this.<hisHeroFrameMat>__9.HasProperty("_LightingBlend"))
            {
                this.<hisHeroFrameMat>__9.SetFloat("_LightingBlend", 0f);
            }
            this.<>f__this.myHeroCardActor.TurnOffCollider();
            this.<>f__this.hisHeroCardActor.TurnOffCollider();
            GameState.Get().GetGameEntity().NotifyOfMulliganInitialized();
            this.<alternateIntro>__10 = GameState.Get().GetGameEntity().DoAlternateMulliganIntro();
            if (this.<alternateIntro>__10)
            {
                this.<>f__this.introComplete = true;
                goto Label_1488;
            }
        Label_0651:
            while (this.<>f__this.waitingForVersusText || this.<>f__this.waitingForVersusVo)
            {
                this.$current = null;
                this.$PC = 11;
                goto Label_148A;
            }
            Log.LoadingScreen.Print("MulliganManager.WaitForHeroesAndStartAnimations() - NotifySceneLoaded()", new object[0]);
            SceneMgr.Get().NotifySceneLoaded();
        Label_06A9:
            while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
            {
                this.$current = null;
                this.$PC = 12;
                goto Label_148A;
            }
            GameMgr.Get().UpdatePresence();
            this.<myHero>__11 = this.<>f__this.myHeroCardActor.gameObject;
            this.<hisHero>__12 = this.<>f__this.hisHeroCardActor.gameObject;
            this.<>f__this.myHeroCardActor.GetHealthObject().Hide();
            this.<>f__this.hisHeroCardActor.GetHealthObject().Hide();
            if (this.<>f__this.versusTextObject != null)
            {
                this.<>f__this.versusTextObject.transform.position = Board.Get().FindBone("VS_Position").position;
            }
            this.<heroLabelCopy>__13 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.heroLabelPrefab);
            this.<>f__this.myheroLabel = this.<heroLabelCopy>__13.GetComponent<HeroLabel>();
            this.<>f__this.myheroLabel.transform.parent = this.<>f__this.myHeroCardActor.GetMeshRenderer().transform;
            this.<>f__this.myheroLabel.transform.localPosition = new Vector3(0f, 0f, 0f);
            this.<classTag>__14 = this.<>f__this.myHeroCardActor.GetEntity().GetClass();
            this.<className>__15 = string.Empty;
            if ((this.<classTag>__14 != TAG_CLASS.INVALID) && !GameMgr.Get().IsTutorial())
            {
                this.<className>__15 = GameStrings.GetClassName(this.<classTag>__14).ToUpper();
            }
            this.<>f__this.myheroLabel.UpdateText(this.<>f__this.myHeroCardActor.GetEntity().GetName(), this.<className>__15);
            this.<heroLabelCopy>__13 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.heroLabelPrefab);
            this.<>f__this.hisheroLabel = this.<heroLabelCopy>__13.GetComponent<HeroLabel>();
            this.<>f__this.hisheroLabel.transform.parent = this.<>f__this.hisHeroCardActor.GetMeshRenderer().transform;
            this.<>f__this.hisheroLabel.transform.localPosition = new Vector3(0f, 0f, 0f);
            this.<classTag>__14 = this.<>f__this.hisHeroCardActor.GetEntity().GetClass();
            this.<className>__15 = string.Empty;
            if ((this.<classTag>__14 != TAG_CLASS.INVALID) && !GameMgr.Get().IsTutorial())
            {
                this.<className>__15 = GameStrings.GetClassName(this.<classTag>__14).ToUpper();
            }
            this.<>f__this.hisheroLabel.UpdateText(this.<>f__this.hisHeroCardActor.GetEntity().GetName(), this.<className>__15);
            if (GameState.Get().WasConcedeRequested())
            {
                goto Label_1488;
            }
            MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_Mulligan);
            this.<cardAnim>__16 = this.<myHero>__11.GetComponent<Animation>();
            if (this.<cardAnim>__16 == null)
            {
                this.<cardAnim>__16 = this.<myHero>__11.AddComponent<Animation>();
            }
            this.<cardAnim>__16.AddClip(this.<>f__this.hisheroAnimatesToPosition, "hisHeroAnimateToPosition");
            this.<>f__this.StartCoroutine(this.<>f__this.SampleAnimFrame(this.<cardAnim>__16, "hisHeroAnimateToPosition", 0f));
            this.<oppCardAnim>__17 = this.<hisHero>__12.GetComponent<Animation>();
            if (this.<oppCardAnim>__17 == null)
            {
                this.<oppCardAnim>__17 = this.<hisHero>__12.AddComponent<Animation>();
            }
            this.<oppCardAnim>__17.AddClip(this.<>f__this.myheroAnimatesToPosition, "myHeroAnimateToPosition");
            this.<>f__this.StartCoroutine(this.<>f__this.SampleAnimFrame(this.<oppCardAnim>__17, "myHeroAnimateToPosition", 0f));
        Label_0A8C:
            while (LoadingScreen.Get().IsTransitioning())
            {
                this.$current = null;
                this.$PC = 13;
                goto Label_148A;
            }
            if (this.<>f__this.versusVo == null)
            {
                goto Label_0C0D;
            }
            this.<myHeroLine>__18 = this.<>f__this.myHeroCardActor.GetCard().GetAnnouncerLine();
            SoundManager.Get().Play(this.<myHeroLine>__18);
        Label_0AF5:
            while (SoundManager.Get().IsActive(this.<myHeroLine>__18))
            {
                this.$current = null;
                this.$PC = 14;
                goto Label_148A;
            }
            this.$current = new WaitForSeconds(0.05f);
            this.$PC = 15;
            goto Label_148A;
        Label_0B55:
            while (SoundManager.Get().IsActive(this.<>f__this.versusVo))
            {
                this.$current = null;
                this.$PC = 0x10;
                goto Label_148A;
            }
            this.$current = new WaitForSeconds(0.05f);
            this.$PC = 0x11;
            goto Label_148A;
        Label_0BF8:
            while (SoundManager.Get().IsActive(this.<hisHeroLine>__19))
            {
                this.$current = null;
                this.$PC = 0x12;
                goto Label_148A;
            }
        Label_0C0D:
            this.$current = this.<>f__this.StartCoroutine(GameState.Get().GetGameEntity().PlayMissionIntroLineAndWait());
            this.$PC = 0x13;
            goto Label_148A;
        Label_0EEB:
            this.<>f__this.m_HisCustomSocketInSpell.gameObject.SetActive(true);
            if (this.<>f__this.hisHeroCardActor.GetCardDef().m_SocketInParentEffectToHero)
            {
                this.<hisActorScale>__22 = this.<>f__this.hisHeroCardActor.transform.localScale;
                this.<>f__this.hisHeroCardActor.transform.localScale = Vector3.one;
                this.<>f__this.m_HisCustomSocketInSpell.transform.parent = this.<>f__this.hisHeroCardActor.transform;
                this.<>f__this.m_HisCustomSocketInSpell.transform.localPosition = Vector3.zero;
                this.<>f__this.hisHeroCardActor.transform.localScale = this.<hisActorScale>__22;
            }
            this.<>f__this.m_HisCustomSocketInSpell.SetSource(this.<>f__this.hisHeroCardActor.GetCard().gameObject);
            this.<>f__this.m_HisCustomSocketInSpell.RemoveAllTargets();
            this.<hisHeroSocketBone>__23 = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).gameObject;
            this.<>f__this.m_HisCustomSocketInSpell.AddTarget(this.<hisHeroSocketBone>__23);
            this.<>f__this.m_HisCustomSocketInSpell.ActivateState(SpellStateType.BIRTH);
            this.<>f__this.m_HisCustomSocketInSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>m__102));
            if (!this.<>f__this.hisHeroCardActor.GetCardDef().m_SocketInOverrideHeroAnimation)
            {
                this.<oppCardAnim>__17["myHeroAnimateToPosition"].enabled = true;
            }
        Label_1083:
            SoundManager.Get().LoadAndPlay("FX_MulliganCoin01_HeroCoinDrop", this.<>f__this.hisHeroCardActor.GetCard().gameObject);
            if (this.<>f__this.versusTextObject != null)
            {
                this.$current = new WaitForSeconds(0.1f);
                this.$PC = 0x16;
                goto Label_148A;
            }
        Label_110C:
            if (this.<>f__this.m_MyCustomSocketInSpell == null)
            {
                this.<>f__this.myWeldEffect = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.weldPrefab);
                this.<>f__this.myWeldEffect.transform.position = this.<myHero>__11.transform.position;
                if (this.<>f__this.m_HisCustomSocketInSpell != null)
                {
                    SoundUtils.SetSourceVolumes(this.<>f__this.myWeldEffect, 0f, false);
                }
                this.<>f__this.myWeldEffect.GetComponent<HeroWeld>().DoAnim();
            }
            if (this.<>f__this.m_HisCustomSocketInSpell == null)
            {
                this.<>f__this.hisWeldEffect = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.weldPrefab);
                this.<>f__this.hisWeldEffect.transform.position = this.<hisHero>__12.transform.position;
                if (this.<>f__this.m_MyCustomSocketInSpell != null)
                {
                    SoundUtils.SetSourceVolumes(this.<>f__this.hisWeldEffect, 0f, false);
                }
                this.<>f__this.hisWeldEffect.GetComponent<HeroWeld>().DoAnim();
            }
            this.$current = new WaitForSeconds(0.05f);
            this.$PC = 0x18;
            goto Label_148A;
        Label_1488:
            return false;
        Label_148A:
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
    private sealed class <WaitForOpponentToFinishMulligan>c__IteratorB6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MulliganManager <>f__this;
        internal Vector3 <endScale>__2;
        internal Vector3 <mulliganBannerPosition>__0;
        internal Vector3 <position>__1;

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
                    this.<>f__this.DestroyChooseBanner();
                    this.<mulliganBannerPosition>__0 = Board.Get().FindBone("ChoiceBanner").position;
                    if (UniversalInputManager.UsePhoneUI == null)
                    {
                        this.<position>__1 = new Vector3(this.<mulliganBannerPosition>__0.x, this.<>f__this.friendlySideHandZone.transform.position.y, this.<>f__this.myHeroCardActor.transform.position.z + 0.4f);
                        this.<endScale>__2 = new Vector3(1.4f, 1.4f, 1.4f);
                        break;
                    }
                    this.<position>__1 = new Vector3(this.<mulliganBannerPosition>__0.x, this.<>f__this.friendlySideHandZone.transform.position.y + 1f, this.<>f__this.myHeroCardActor.transform.position.z + 6.8f);
                    this.<endScale>__2 = new Vector3(2.5f, 2.5f, 2.5f);
                    break;

                case 1:
                    goto Label_0210;

                default:
                    goto Label_0241;
            }
            this.<>f__this.mulliganChooseBanner = (GameObject) UnityEngine.Object.Instantiate(this.<>f__this.mulliganChooseBannerPrefab, this.<position>__1, new Quaternion(0f, 0f, 0f, 0f));
            this.<>f__this.mulliganChooseBanner.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            iTween.ScaleTo(this.<>f__this.mulliganChooseBanner, this.<endScale>__2, 0.4f);
            this.<>f__this.mulliganChooseBanner.GetComponent<Banner>().SetText(GameStrings.Get("GAMEPLAY_MULLIGAN_WAITING"));
            this.<>f__this.mulliganChooseBanner.GetComponent<Banner>().MoveGlowForBottomPlacement();
        Label_0210:
            while (!this.<>f__this.opponentPlayerHasReplacementCards && !GameState.Get().IsGameOver())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.EndMulligan();
            this.$PC = -1;
        Label_0241:
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

