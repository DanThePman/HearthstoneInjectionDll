using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VictoryScreen : EndGameScreen
{
    public GamesWonIndicator m_gamesWonIndicator;
    private Achievement m_goldenHeroAchievement;
    private CardDef m_goldenHeroCardDef;
    private bool m_goldenHeroCardDefReady;
    private GoldenHeroEvent m_goldenHeroEvent;
    public Transform m_goldenHeroEventBone;
    private bool m_hasParsedCompletedQuests;
    public UberText m_noGoldRewardText;
    private bool m_showGoldenHeroEvent;
    private bool m_showNoGoldRewardText;
    private bool m_showWinProgress;
    private const string NO_GOLDEN_HERO = "none";

    protected override void Awake()
    {
        base.Awake();
        this.m_gamesWonIndicator.Hide();
        this.m_noGoldRewardText.gameObject.SetActive(false);
        if (base.ShouldMakeUtilRequests())
        {
            if (GameMgr.Get().IsTutorial() && !GameMgr.Get().IsSpectator())
            {
                NetCache.Get().RegisterTutorialEndGameScreen(new NetCache.NetCacheCallback(this.OnNetCacheReady));
            }
            else
            {
                NetCache.Get().RegisterScreenEndOfGame(new NetCache.NetCacheCallback(this.OnNetCacheReady));
            }
        }
    }

    protected void ContinueButtonPress_FirstTimeHub(UIEvent e)
    {
        if (base.HasShownScoops())
        {
            base.HideTwoScoop();
            if (base.ShowNextReward())
            {
                SoundManager.Get().LoadAndPlay("VO_INNKEEPER_TUT_COMPLETE_05");
            }
            else if (!base.ShowNextCompletedQuest())
            {
                base.ContinueButtonPress_Common();
                base.m_hitbox.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_FirstTimeHub));
                if (Network.ShouldBeConnectedToAurora())
                {
                    base.BackToMode(SceneMgr.Mode.HUB);
                }
                else
                {
                    Notification component = AssetLoader.Get().LoadActor("TutorialIntroDialog", true, false).GetComponent<Notification>();
                    NotificationManager.Get().CreatePopupDialogFromObject(component, GameStrings.Get("GLOBAL_MEDAL_REWARD_CONGRATULATIONS"), GameStrings.Get("TUTORIAL_MOBILE_COMPLETE_CONGRATS"), GameStrings.Get("GLOBAL_OKAY"));
                    component.ButtonStart.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.UserPressedStartButton));
                    component.artOverlay.material = component.swapMaterial;
                    component.artOverlay.material.mainTextureOffset = new Vector2(0.5f, 0f);
                    base.m_hitbox.gameObject.SetActive(false);
                    base.m_continueText.gameObject.SetActive(false);
                }
            }
        }
    }

    private string GetGoldenHeroCardID()
    {
        int index = 0;
        foreach (Achievement achievement in base.m_completedQuests)
        {
            if (achievement.AchieveGroup == Achievement.Group.UNLOCK_GOLDEN_HERO)
            {
                this.m_goldenHeroAchievement = achievement;
                foreach (RewardData data in achievement.Rewards)
                {
                    if (data.RewardType != Reward.Type.CARD)
                    {
                        index++;
                    }
                    else
                    {
                        CardRewardData cardReward = data as CardRewardData;
                        CollectionManager.Get().AddCardReward(cardReward, false);
                        base.m_completedQuests.RemoveAt(index);
                        return cardReward.CardID;
                    }
                }
            }
        }
        return "none";
    }

    private void InitGoldRewardUI()
    {
        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
        TAG_GOLD_REWARD_STATE tag = GameState.Get().GetFriendlySidePlayer().GetTag<TAG_GOLD_REWARD_STATE>(GAME_TAG.GOLD_REWARD_STATE);
        if (tag == TAG_GOLD_REWARD_STATE.ELIGIBLE)
        {
            NetCache.NetCacheGamesPlayed played = NetCache.Get().GetNetObject<NetCache.NetCacheGamesPlayed>();
            Log.Rachelle.Print(string.Format("EndGameTwoScoop.UpdateData(): {0}/{1} wins towards {2} gold", played.FreeRewardProgress, netObject.WinsPerGold, netObject.GoldPerReward), new object[0]);
            this.m_showWinProgress = true;
            this.m_gamesWonIndicator.Init(Reward.Type.GOLD, netObject.GoldPerReward, netObject.WinsPerGold, played.FreeRewardProgress, GamesWonIndicator.InnKeeperTrigger.NONE);
        }
        else
        {
            Log.Rachelle.Print(string.Format("VictoryScreen.InitGoldRewardUI(): goldRewardState = {0}", tag), new object[0]);
            string str = string.Empty;
            switch (tag)
            {
                case TAG_GOLD_REWARD_STATE.ALREADY_CAPPED:
                {
                    object[] args = new object[] { netObject.MaxGoldPerDay };
                    str = GameStrings.Format("GLOBAL_GOLD_REWARD_ALREADY_CAPPED", args);
                    break;
                }
                case TAG_GOLD_REWARD_STATE.BAD_RATING:
                    str = GameStrings.Get("GLOBAL_GOLD_REWARD_BAD_RATING");
                    break;

                case TAG_GOLD_REWARD_STATE.SHORT_GAME:
                    str = GameStrings.Get("GLOBAL_GOLD_REWARD_SHORT_GAME");
                    break;

                case TAG_GOLD_REWARD_STATE.OVER_CAIS:
                    str = GameStrings.Get("GLOBAL_GOLD_REWARD_OVER_CAIS");
                    break;
            }
            if (!string.IsNullOrEmpty(str))
            {
                this.m_showNoGoldRewardText = true;
                this.m_noGoldRewardText.Text = str;
            }
        }
    }

    protected override bool JustEarnedGoldenHero()
    {
        if (!this.m_hasParsedCompletedQuests)
        {
            string goldenHeroCardID = this.GetGoldenHeroCardID();
            if (goldenHeroCardID != "none")
            {
                CardPortraitQuality quality = new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN);
                DefLoader.Get().LoadCardDef(goldenHeroCardID, new DefLoader.LoadDefCallback<CardDef>(this.OnGoldenHeroCardDefLoaded), new object(), quality);
            }
            this.m_hasParsedCompletedQuests = true;
            this.m_showGoldenHeroEvent = goldenHeroCardID != "none";
        }
        return this.m_showGoldenHeroEvent;
    }

    protected override void LoadGoldenHeroEvent()
    {
        AssetLoader.Get().LoadGameObject("Hero2GoldHero", new AssetLoader.GameObjectCallback(this.OnGoldenHeroEventLoaded), null, false);
    }

    public void NotifyOfGoldenHeroAnimComplete()
    {
        base.m_animationReadyToSkip = true;
        this.m_goldenHeroEvent.RemoveAnimationDoneListener(new GoldenHeroEvent.AnimationDoneListener(this.NotifyOfGoldenHeroAnimComplete));
    }

    private void OnGoldenHeroCardDefLoaded(string cardId, CardDef def, object userData)
    {
        this.m_goldenHeroCardDef = def;
        this.m_goldenHeroCardDefReady = true;
    }

    private void OnGoldenHeroEventLoaded(string name, GameObject go, object callbackData)
    {
        base.StartCoroutine(this.WaitUntilTwoScoopLoaded(name, go, callbackData));
    }

    protected override void OnTwoScoopHidden()
    {
        if (this.m_showNoGoldRewardText)
        {
            this.m_noGoldRewardText.gameObject.SetActive(false);
        }
        if (this.m_showWinProgress)
        {
            this.m_gamesWonIndicator.Hide();
        }
    }

    protected override void OnTwoScoopShown()
    {
        if (BnetBar.Get() != null)
        {
            BnetBar.Get().SuppressLoginTooltip(true);
        }
        if (base.ShouldMakeUtilRequests())
        {
            this.InitGoldRewardUI();
        }
        if (this.m_showNoGoldRewardText)
        {
            this.m_noGoldRewardText.gameObject.SetActive(true);
        }
        if (this.m_showWinProgress)
        {
            this.m_gamesWonIndicator.Show();
        }
    }

    protected override bool ShowGoldenHeroEvent()
    {
        if (!this.JustEarnedGoldenHero())
        {
            return false;
        }
        if (this.m_goldenHeroEvent.gameObject.activeInHierarchy)
        {
            this.m_goldenHeroEvent.Hide();
            this.m_showGoldenHeroEvent = false;
            return false;
        }
        this.m_goldenHeroAchievement.AckCurrentProgressAndRewardNotices();
        base.m_animationReadyToSkip = false;
        this.m_goldenHeroEvent.RegisterAnimationDoneListener(new GoldenHeroEvent.AnimationDoneListener(this.NotifyOfGoldenHeroAnimComplete));
        base.m_twoScoop.StopAnimating();
        this.m_goldenHeroEvent.Show();
        base.m_twoScoop.m_heroActor.transform.parent = this.m_goldenHeroEvent.m_heroBone;
        base.m_twoScoop.m_heroActor.transform.localPosition = Vector3.zero;
        base.m_twoScoop.m_heroActor.transform.localScale = new Vector3(1.375f, 1.375f, 1.375f);
        return true;
    }

    protected override void ShowStandardFlow()
    {
        base.ShowStandardFlow();
        if (!GameMgr.Get().IsTutorial() || GameMgr.Get().IsSpectator())
        {
            base.m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_PrevMode));
        }
        else if (GameUtils.AreAllTutorialsComplete())
        {
            LoadingScreen.Get().SetFadeColor(Color.white);
            base.m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_FirstTimeHub));
        }
        else if ((DemoMgr.Get().GetMode() == DemoMode.APPLE_STORE) && (GameUtils.GetNextTutorial() == 0))
        {
            base.StartCoroutine(DemoMgr.Get().CompleteAppleStoreDemo());
        }
        else
        {
            base.m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_TutorialProgress));
        }
    }

    protected void UserPressedStartButton(UIEvent e)
    {
        WebAuth.ClearLoginData();
        base.BackToMode(SceneMgr.Mode.RESET);
    }

    [DebuggerHidden]
    private IEnumerator WaitUntilTwoScoopLoaded(string name, GameObject go, object callbackData)
    {
        return new <WaitUntilTwoScoopLoaded>c__Iterator7F { go = go, <$>go = go, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitUntilTwoScoopLoaded>c__Iterator7F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>go;
        internal VictoryScreen <>f__this;
        internal Texture <heroTexture>__0;
        internal GameObject go;

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
                    if ((this.<>f__this.m_twoScoop == null) || !this.<>f__this.m_twoScoop.IsLoaded())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_012E;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_012C;
            }
            while (!this.<>f__this.m_goldenHeroCardDefReady)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_012E;
            }
            this.go.SetActive(false);
            TransformUtil.AttachAndPreserveLocalTransform(this.go.transform, this.<>f__this.m_goldenHeroEventBone);
            this.<>f__this.m_goldenHeroEvent = this.go.GetComponent<GoldenHeroEvent>();
            this.<heroTexture>__0 = this.<>f__this.m_goldenHeroCardDef.GetPortraitTexture();
            this.<>f__this.m_goldenHeroEvent.SetHeroBurnAwayTexture(this.<heroTexture>__0);
            this.<>f__this.m_goldenHeroEvent.SetVictoryTwoScoop((VictoryTwoScoop) this.<>f__this.m_twoScoop);
            this.<>f__this.SetGoldenHeroEventReady(true);
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
}

