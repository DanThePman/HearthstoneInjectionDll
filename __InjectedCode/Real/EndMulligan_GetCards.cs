#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class MulliganManager : MonoBehaviour
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void EndMulligan()
    {
        foreach (var handZoneCard in friendlySideHandZone.GetCards())
        {
            HearthstoneInjectionDll.Player.AddShownCard(handZoneCard.name);
        }    

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

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    MulliganManager()
    {
    }

    void Awake()
    {
    }

    void OnDestroy()
    {
    }

    void Start()
    {
    }

    static MulliganManager Get()
    {
        return default(MulliganManager);
    }

    bool IsMulliganActive()
    {
        return default(bool);
    }

    void ForceMulliganActive(bool active)
    {
    }

    System.Collections.IEnumerator WaitForBoardThenLoadButton()
    {
        return default(System.Collections.IEnumerator);
    }

    void OnMulliganButtonLoaded(string name, UnityEngine.GameObject go, object userData)
    {
    }

    void OnVersusVoLoaded(string name, UnityEngine.GameObject go, object userData)
    {
    }

    void OnVersusTextLoaded(string name, UnityEngine.GameObject go, object userData)
    {
    }

    System.Collections.IEnumerator WaitForHeroesAndStartAnimations()
    {
        return default(System.Collections.IEnumerator);
    }

    void BeginMulligan()
    {
    }

    void HandleGameStart()
    {
    }

    System.Collections.IEnumerator ResumeMulligan()
    {
        return default(System.Collections.IEnumerator);
    }

    void OnMulliganTimerUpdate(TurnTimerUpdate update, object userData)
    {
    }

    void OnGameOver(object userData)
    {
    }

    System.Collections.IEnumerator ContinueMulliganWhenBoardLoads()
    {
        return default(System.Collections.IEnumerator);
    }

    bool ShouldWaitForMulliganCardsToBeProcessed()
    {
        return default(bool);
    }

    bool IsTaskListPuttingUsPastMulligan(PowerTaskList taskList)
    {
        return default(bool);
    }

    void GetStartingLists()
    {
    }

    System.Collections.IEnumerator PlayStartingTaunts()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator DealStartingCards()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator WaitAFrameBeforeSendingEventToMulliganButton()
    {
        return default(System.Collections.IEnumerator);
    }

    void BeginMulliganCountdown(float endTimeStamp)
    {
    }

    NormalButton GetMulliganButton()
    {
        return default(NormalButton);
    }

    void CoinTossTextCallback(string actorName, UnityEngine.GameObject actorObject, object callbackData)
    {
    }

    System.Collections.IEnumerator AnimateCoinTossText()
    {
        return default(System.Collections.IEnumerator);
    }

    MulliganReplaceLabel CreateNewUILabelAtCardPosition(MulliganReplaceLabel prefab, int cardPosition)
    {
        return default(MulliganReplaceLabel);
    }

    void SetAllMulliganCardsToHold()
    {
    }

    void ToggleHoldState(int startingCardsIndex)
    {
    }

    void DestroyXobjects()
    {
    }

    void DestroyChooseBanner()
    {
    }

    void DestroyMulliganTimer()
    {
    }

    void ToggleHoldState(Card toggleCard)
    {
    }

    void ServerHasDealtReplacementCards(bool isFriendlySide)
    {
    }

    void AutomaticContinueMulligan()
    {
    }

    void OnMulliganButtonReleased(UIEvent e)
    {
    }

    void BeginDealNewCards()
    {
    }

    System.Collections.IEnumerator RemoveOldCardsAnimation()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator WaitForOpponentToFinishMulligan()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator RemoveUIButtons()
    {
        return default(System.Collections.IEnumerator);
    }

    void DestroyButton(UnityEngine.Object buttonToDestroy)
    {
    }

    void HandleGameOverDuringMulligan()
    {
    }

    System.Collections.IEnumerator EndMulliganWithTiming()
    {
        return default(System.Collections.IEnumerator);
    }

    System.Collections.IEnumerator HandleCoinCard()
    {
        return default(System.Collections.IEnumerator);
    }

    bool IsCoinCard(Card card)
    {
        return default(bool);
    }

    Card GetCoinCardFromFriendlyHand()
    {
        return default(Card);
    }

    void PutCoinCardInSpawnPosition(Card coinCard)
    {
    }

    bool ShouldHandleCoinCard()
    {
        return default(bool);
    }

    void CoinCardSummonFinishedCallback(Spell spell, object userData)
    {
    }

    System.Collections.IEnumerator EnableHandCollidersAfterCardsAreDealt()
    {
        return default(System.Collections.IEnumerator);
    }

    void SkipCardChoosing()
    {
    }

    void SkipMulliganForDev()
    {
    }

    System.Collections.IEnumerator SkipMulliganForResume()
    {
        return default(System.Collections.IEnumerator);
    }

    void SkipMulligan()
    {
    }

    System.Collections.IEnumerator SkipMulliganWhenIntroComplete()
    {
        return default(System.Collections.IEnumerator);
    }

    void FadeOutMulliganMusicAndStartGameplayMusic()
    {
    }

    System.Collections.IEnumerator WaitForBoardAnimToCompleteThenStartTurn()
    {
        return default(System.Collections.IEnumerator);
    }

    void ShuffleDeck()
    {
    }

    void SlideCard(UnityEngine.GameObject topCard)
    {
    }

    System.Collections.IEnumerator SampleAnimFrame(UnityEngine.Animation animToUse, string animName, float startSec)
    {
        return default(System.Collections.IEnumerator);
    }

    void SortHand(Zone zone)
    {
    }

    System.Collections.IEnumerator ShrinkStartingHandBanner(UnityEngine.GameObject banner)
    {
        return default(System.Collections.IEnumerator);
    }

    void FadeHeroPowerIn(Card heroPowerCard)
    {
    }

    void LoadMyHeroSkinSocketInEffect(CardDef cardDef)
    {
    }

    void OnMyHeroSkinSocketInEffectLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void LoadHisHeroSkinSocketInEffect(CardDef cardDef)
    {
    }

    void OnHisHeroSkinSocketInEffectLoaded(string name, UnityEngine.GameObject go, object callbackData)
    {
    }

    void DestoryHeroSkinSocketInEffects()
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    static float PHONE_HEIGHT_OFFSET;
    static float PHONE_CARD_Z_OFFSET;
    static float PHONE_CARD_SCALE;
    static float PHONE_ZONE_SIZE_ADJUST;
    static float ANIMATION_TIME_DEAL_CARD;
    static float DEFAULT_STARTING_TAUNT_DURATION;
    UnityEngine.AnimationClip cardAnimatesFromBoardToDeck;
    UnityEngine.AnimationClip cardAnimatesFromBoardToDeck_iPhone;
    UnityEngine.AnimationClip cardAnimatesFromTableToSky;
    UnityEngine.AnimationClip cardAnimatesFromDeckToBoard;
    UnityEngine.AnimationClip shuffleDeck;
    UnityEngine.AnimationClip myheroAnimatesToPosition;
    UnityEngine.AnimationClip hisheroAnimatesToPosition;
    UnityEngine.AnimationClip myheroAnimatesToPosition_iPhone;
    UnityEngine.AnimationClip hisheroAnimatesToPosition_iPhone;
    UnityEngine.GameObject coinPrefab;
    UnityEngine.GameObject weldPrefab;
    UnityEngine.GameObject mulliganChooseBannerPrefab;
    UnityEngine.GameObject mulliganKeepLabelPrefab;
    MulliganReplaceLabel mulliganReplaceLabelPrefab;
    UnityEngine.GameObject mulliganXlabelPrefab;
    UnityEngine.GameObject mulliganTimerPrefab;
    UnityEngine.GameObject heroLabelPrefab;
    static MulliganManager s_instance;
    bool mulliganActive;
    MulliganTimer m_mulliganTimer;
    NormalButton mulliganButton;
    UnityEngine.GameObject myWeldEffect;
    UnityEngine.GameObject hisWeldEffect;
    UnityEngine.GameObject coinObject;
    UnityEngine.GameObject startingHandZone;
    UnityEngine.GameObject coinTossText;
    ZoneHand friendlySideHandZone;
    ZoneHand opposingSideHandZone;
    ZoneDeck friendlySideDeck;
    ZoneDeck opposingSideDeck;
    Actor myHeroCardActor;
    Actor hisHeroCardActor;
    Actor myHeroPowerCardActor;
    Actor hisHeroPowerCardActor;
    bool waitingForVersusText;
    UnityEngine.GameObject versusTextObject;
    bool waitingForVersusVo;
    UnityEngine.AudioSource versusVo;
    bool introComplete;
    bool skipCardChoosing;
    System.Collections.Generic.List<Card> m_startingCards;
    System.Collections.Generic.List<Card> m_startingOppCards;
    int m_coinCardIndex;
    int m_bonusCardIndex;
    UnityEngine.GameObject mulliganChooseBanner;
    System.Collections.Generic.List<MulliganReplaceLabel> m_replaceLabels;
    UnityEngine.GameObject[] m_xLabels;
    bool[] m_handCardsMarkedForReplace;
    UnityEngine.Vector3 coinLocation;
    bool friendlyPlayerGoesFirst;
    HeroLabel myheroLabel;
    HeroLabel hisheroLabel;
    Spell m_MyCustomSocketInSpell;
    Spell m_HisCustomSocketInSpell;
    bool m_isLoadingMyCustomSocketIn;
    bool m_isLoadingHisCustomSocketIn;
    bool friendlyPlayerHasReplacementCards;
    bool opponentPlayerHasReplacementCards;
    bool m_waitingForUserInput;
    Notification innkeeperMulliganDialog;
    bool m_resuming;
    #endregion

}
