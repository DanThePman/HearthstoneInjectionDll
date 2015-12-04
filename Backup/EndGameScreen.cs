using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    protected bool m_achievesReady;
    protected bool m_animationReadyToSkip;
    protected List<Achievement> m_completedQuests = new List<Achievement>();
    public UberText m_continueText;
    private Reward m_currentlyShowingReward;
    protected bool m_goldenHeroEventReady;
    private bool m_hasAlreadySetMode;
    private bool m_haveShownTwoScoop;
    public PegUIElement m_hitbox;
    protected bool m_netCacheReady;
    private int m_numRewardsToLoad;
    private List<Reward> m_rewards = new List<Reward>();
    private bool m_rewardsLoaded;
    protected bool m_shown;
    public EndGameTwoScoop m_twoScoop;
    public static OnTwoScoopsShownHandler OnTwoScoopsShown;
    private static readonly PlatformDependentValue<Vector3> REWARD_LOCAL_POS;
    private static readonly PlatformDependentValue<Vector3> REWARD_PUNCH_SCALE;
    private static readonly PlatformDependentValue<Vector3> REWARD_SCALE;
    private static EndGameScreen s_instance;

    static EndGameScreen()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = Vector3.one,
            Phone = new Vector3(0.8f, 0.8f, 0.8f)
        };
        REWARD_SCALE = value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(1.2f, 1.2f, 1.2f),
            Phone = new Vector3(1.25f, 1.25f, 1.25f)
        };
        REWARD_PUNCH_SCALE = value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(-7.628078f, 8.371922f, -3.883112f),
            Phone = new Vector3(-7.628078f, 8.371922f, -3.94f)
        };
        REWARD_LOCAL_POS = value2;
    }

    protected virtual void Awake()
    {
        s_instance = this;
        CollectionManager.Get().RegisterAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        AchieveManager.Get().TriggerLaunchDayEvent();
        AchieveManager.Get().UpdateActiveAchieves(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnAchievesUpdated));
        this.m_hitbox.gameObject.SetActive(false);
        string key = "GLOBAL_CLICK_TO_CONTINUE";
        if (UniversalInputManager.Get().IsTouchMode())
        {
            key = "GLOBAL_CLICK_TO_CONTINUE_TOUCH";
        }
        this.m_continueText.Text = GameStrings.Get(key);
        this.m_continueText.gameObject.SetActive(false);
        PegUI.Get().SetInputCamera(CameraUtils.FindFirstByLayer(GameLayer.IgnoreFullScreenEffects));
        SceneUtils.SetLayer(this.m_hitbox.gameObject, GameLayer.IgnoreFullScreenEffects);
        SceneUtils.SetLayer(this.m_continueText.gameObject, GameLayer.IgnoreFullScreenEffects);
        if (!Network.ShouldBeConnectedToAurora())
        {
            this.UpdateRewards();
        }
    }

    protected void BackToMode(SceneMgr.Mode mode)
    {
        CollectionManager.Get().RemoveAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        this.HideTwoScoop();
        if (!this.m_hasAlreadySetMode)
        {
            this.m_hasAlreadySetMode = true;
            base.StartCoroutine(this.ToMode(mode));
            Navigation.Clear();
        }
    }

    protected void ContinueButtonPress_Common()
    {
        LoadingScreen.Get().AddTransitionObject(this);
    }

    protected void ContinueButtonPress_PrevMode(UIEvent e)
    {
        this.ContinueEvents();
    }

    protected void ContinueButtonPress_TutorialProgress(UIEvent e)
    {
        this.ContinueTutorialEvents();
    }

    private bool ContinueDefaultEvents()
    {
        if (!this.m_haveShownTwoScoop)
        {
            return true;
        }
        if (!this.m_animationReadyToSkip)
        {
            return true;
        }
        this.HideTwoScoop();
        return ((this.ShowGoldenHeroEvent() && this.m_goldenHeroEventReady) || (this.ShowFixedRewards() || (this.ShowNextCompletedQuest() || this.ShowNextReward())));
    }

    public bool ContinueEvents()
    {
        if (this.ContinueDefaultEvents())
        {
            return true;
        }
        PlayMakerFSM component = this.m_twoScoop.GetComponent<PlayMakerFSM>();
        if (component != null)
        {
            component.SendEvent("Death");
        }
        this.ContinueButtonPress_Common();
        this.m_hitbox.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_PrevMode));
        this.ReturnToPreviousMode();
        return false;
    }

    public void ContinueTutorialEvents()
    {
        if (!this.ContinueDefaultEvents())
        {
            this.ContinueButtonPress_Common();
            this.m_hitbox.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ContinueButtonPress_TutorialProgress));
            this.m_continueText.gameObject.SetActive(false);
            this.ShowTutorialProgress();
        }
    }

    public static EndGameScreen Get()
    {
        return s_instance;
    }

    protected bool HasShownScoops()
    {
        return this.m_haveShownTwoScoop;
    }

    protected void HideTwoScoop()
    {
        if (this.m_twoScoop.IsShown())
        {
            this.m_twoScoop.Hide();
            this.OnTwoScoopHidden();
            if (OnTwoScoopsShown != null)
            {
                OnTwoScoopsShown(false, this.m_twoScoop);
            }
            if (InputManager.Get() != null)
            {
                InputManager.Get().EnableInput();
            }
        }
    }

    protected bool InitIfReady()
    {
        if (!this.IsReady() && (this.ShouldMakeUtilRequests() || !this.m_shown))
        {
            return false;
        }
        if ((!GameMgr.Get().IsSpectator() && GameMgr.Get().IsPlay()) && Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE))
        {
            AssetLoader.Get().LoadGameObject("RankChangeTwoScoop", new AssetLoader.GameObjectCallback(this.OnRankChangeLoaded), null, false);
        }
        else
        {
            this.ShowStandardFlow();
        }
        return true;
    }

    protected bool IsReady()
    {
        return (((this.m_shown && this.m_netCacheReady) && this.m_achievesReady) && this.m_rewardsLoaded);
    }

    protected virtual bool JustEarnedGoldenHero()
    {
        return false;
    }

    protected virtual void LoadGoldenHeroEvent()
    {
    }

    [DebuggerHidden]
    private IEnumerator LoadTutorialProgress()
    {
        return new <LoadTutorialProgress>c__Iterator6E { <>f__this = this };
    }

    private void MaybeUpdateRewards()
    {
        if (this.m_achievesReady && this.m_netCacheReady)
        {
            this.UpdateRewards();
            this.InitIfReady();
        }
    }

    [DebuggerHidden]
    private IEnumerator NotifyEndGameScreenOfAnimComplete()
    {
        return new <NotifyEndGameScreenOfAnimComplete>c__Iterator72();
    }

    public void NotifyOfAnimComplete()
    {
        this.m_animationReadyToSkip = true;
    }

    public void NotifyOfRewardAnimComplete()
    {
        this.m_animationReadyToSkip = true;
    }

    private void OnAchievesUpdated(object userData)
    {
        base.StartCoroutine(this.WaitForAchieveManager());
    }

    private void OnCollectionAchievesCompleted(List<Achievement> achievements)
    {
        if (GameUtils.AreAllTutorialsComplete())
        {
            <OnCollectionAchievesCompleted>c__AnonStorey2EF storeyef = new <OnCollectionAchievesCompleted>c__AnonStorey2EF();
            using (List<Achievement>.Enumerator enumerator = achievements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storeyef.achieve = enumerator.Current;
                    if ((storeyef.achieve.RewardTiming == RewardVisualTiming.IMMEDIATE) && (this.m_completedQuests.Find(new Predicate<Achievement>(storeyef.<>m__E6)) == null))
                    {
                        this.m_completedQuests.Add(storeyef.achieve);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (AchieveManager.Get() != null)
        {
            AchieveManager.Get().RemoveActiveAchievesUpdatedListener(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnAchievesUpdated));
        }
        if (OnTwoScoopsShown != null)
        {
            OnTwoScoopsShown(false, this.m_twoScoop);
        }
        s_instance = null;
    }

    protected virtual void OnNetCacheReady()
    {
        this.m_netCacheReady = true;
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        this.MaybeUpdateRewards();
    }

    private void OnRankChangeLoaded(string name, GameObject go, object callbackData)
    {
        NetCache.NetCacheMedalInfo netObject = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
        RankChangeTwoScoop component = go.GetComponent<RankChangeTwoScoop>();
        MedalInfoTranslator medalInfoTranslator = new MedalInfoTranslator(netObject, netObject.PreviousMedalInfo);
        component.Initialize(medalInfoTranslator, new RankChangeTwoScoop.RankChangeClosed(this.ShowStandardFlow));
    }

    private void OnRewardHidden(Reward reward)
    {
        reward.Hide(false);
    }

    private void OnTutorialProgressScreenCallback(string name, GameObject go, object callbackData)
    {
        go.transform.parent = base.transform;
        go.GetComponent<TutorialProgressScreen>().StartTutorialProgress();
    }

    protected virtual void OnTwoScoopHidden()
    {
    }

    protected virtual void OnTwoScoopShown()
    {
    }

    private void PositionReward(Reward reward)
    {
        reward.transform.parent = base.transform;
        reward.transform.localRotation = Quaternion.identity;
        reward.transform.localPosition = (Vector3) REWARD_LOCAL_POS;
    }

    private void ReturnToPreviousMode()
    {
        SceneMgr.Mode postGameSceneMode = GameMgr.Get().GetPostGameSceneMode();
        GameMgr.Get().PreparePostGameSceneMode(postGameSceneMode);
        this.BackToMode(postGameSceneMode);
    }

    private void RewardObjectLoaded(Reward reward, object callbackData)
    {
        reward.Hide(false);
        this.PositionReward(reward);
        this.m_rewards.Add(reward);
        this.m_numRewardsToLoad--;
        if (this.m_numRewardsToLoad <= 0)
        {
            RewardUtils.SortRewards(ref this.m_rewards);
            this.m_rewardsLoaded = true;
            this.InitIfReady();
        }
    }

    protected void SetGoldenHeroEventReady(bool isReady)
    {
        this.m_goldenHeroEventReady = isReady;
    }

    protected bool ShouldMakeUtilRequests()
    {
        if (!Network.ShouldBeConnectedToAurora())
        {
            return false;
        }
        return true;
    }

    public virtual void Show()
    {
        this.m_shown = true;
        Network.DisconnectFromGameServer();
        InputManager.Get().DisableInput();
        this.m_hitbox.gameObject.SetActive(true);
        FullScreenFXMgr.Get().SetBlurDesaturation(0.5f);
        FullScreenFXMgr.Get().Blur(1f, 0.5f, iTween.EaseType.easeInCirc, null);
        if ((GameState.Get() != null) && (GameState.Get().GetFriendlySidePlayer() != null))
        {
            GameState.Get().GetFriendlySidePlayer().GetHandZone().UpdateLayout(-1);
        }
        this.InitIfReady();
    }

    protected bool ShowFixedRewards()
    {
        HashSet<RewardVisualTiming> rewardVisualTimings = new HashSet<RewardVisualTiming> { 1 };
        return FixedRewardsMgr.Get().ShowFixedRewards(rewardVisualTimings, delegate (object userData) {
            this.ContinueEvents();
        }, new FixedRewardsMgr.DelPositionNonToastReward(this.PositionReward), (Vector3) REWARD_PUNCH_SCALE, (Vector3) REWARD_SCALE);
    }

    protected virtual bool ShowGoldenHeroEvent()
    {
        return false;
    }

    protected bool ShowNextCompletedQuest()
    {
        if (QuestToast.IsQuestActive())
        {
            QuestToast.GetCurrentToast().CloseQuestToast();
        }
        if (this.m_completedQuests.Count == 0)
        {
            return false;
        }
        Achievement quest = this.m_completedQuests[0];
        this.m_completedQuests.RemoveAt(0);
        QuestToast.ShowQuestToast(new QuestToast.DelOnCloseQuestToast(this.ShowQuestToastCallback), true, quest);
        return true;
    }

    protected bool ShowNextReward()
    {
        if (this.m_currentlyShowingReward != null)
        {
            this.m_currentlyShowingReward.Hide(true);
            this.m_currentlyShowingReward = null;
        }
        if (this.m_rewards.Count == 0)
        {
            return false;
        }
        this.m_animationReadyToSkip = false;
        this.m_currentlyShowingReward = this.m_rewards[0];
        this.m_rewards.RemoveAt(0);
        this.ShowReward(this.m_currentlyShowingReward);
        return true;
    }

    protected void ShowQuestToastCallback(object userData)
    {
        this.ContinueEvents();
    }

    private void ShowReward(Reward reward)
    {
        RewardUtils.ShowReward(reward, true, (Vector3) REWARD_PUNCH_SCALE, (Vector3) REWARD_SCALE, string.Empty, null, null);
        base.StartCoroutine(this.NotifyEndGameScreenOfAnimComplete());
    }

    protected virtual void ShowStandardFlow()
    {
        this.ShowTwoScoop();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_continueText.gameObject.SetActive(true);
        }
    }

    private void ShowTutorialProgress()
    {
        this.HideTwoScoop();
        base.StartCoroutine(this.LoadTutorialProgress());
    }

    private void ShowTwoScoop()
    {
        base.StartCoroutine(this.ShowTwoScoopWhenReady());
    }

    [DebuggerHidden]
    private IEnumerator ShowTwoScoopWhenReady()
    {
        return new <ShowTwoScoopWhenReady>c__Iterator70 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ToMode(SceneMgr.Mode mode)
    {
        return new <ToMode>c__Iterator6F { mode = mode, <$>mode = mode };
    }

    private void UpdateRewards()
    {
        bool flag = true;
        if (GameMgr.Get().IsTutorial())
        {
            flag = GameUtils.AreAllTutorialsComplete();
        }
        List<RewardData> rewardsToShow = null;
        if (flag)
        {
            List<RewardData> rewards = RewardUtils.GetRewards(NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices);
            HashSet<RewardVisualTiming> rewardTimings = new HashSet<RewardVisualTiming> { 1 };
            RewardUtils.GetViewableRewards(rewards, rewardTimings, ref rewardsToShow, ref this.m_completedQuests);
        }
        else
        {
            rewardsToShow = new List<RewardData>();
        }
        if (this.JustEarnedGoldenHero())
        {
            this.LoadGoldenHeroEvent();
        }
        if (!GameMgr.Get().IsSpectator())
        {
            List<RewardData> customRewards = GameState.Get().GetGameEntity().GetCustomRewards();
            if (customRewards != null)
            {
                rewardsToShow.AddRange(customRewards);
            }
        }
        this.m_numRewardsToLoad = rewardsToShow.Count;
        if (this.m_numRewardsToLoad == 0)
        {
            this.m_rewardsLoaded = true;
        }
        else
        {
            foreach (RewardData data in rewardsToShow)
            {
                data.LoadRewardObject(new Reward.DelOnRewardLoaded(this.RewardObjectLoaded));
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForAchieveManager()
    {
        return new <WaitForAchieveManager>c__Iterator71 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <LoadTutorialProgress>c__Iterator6E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;

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
                    this.$current = new WaitForSeconds(0.25f);
                    this.$PC = 1;
                    return true;

                case 1:
                    AssetLoader.Get().LoadActor("TutorialProgressScreen", new AssetLoader.GameObjectCallback(this.<>f__this.OnTutorialProgressScreenCallback), null, false);
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
    private sealed class <NotifyEndGameScreenOfAnimComplete>c__Iterator72 : IDisposable, IEnumerator, IEnumerator<object>
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
                    this.$current = new WaitForSeconds(0.35f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (EndGameScreen.Get() != null)
                    {
                        EndGameScreen.Get().NotifyOfRewardAnimComplete();
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
    private sealed class <OnCollectionAchievesCompleted>c__AnonStorey2EF
    {
        internal Achievement achieve;

        internal bool <>m__E6(Achievement obj)
        {
            return (this.achieve.ID == obj.ID);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowTwoScoopWhenReady>c__Iterator70 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;

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
                    if (!this.<>f__this.ShouldMakeUtilRequests())
                    {
                        goto Label_00A9;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0081;

                case 3:
                    goto Label_00A9;

                case 4:
                    goto Label_00D1;

                case 5:
                    goto Label_0118;

                default:
                    goto Label_0176;
            }
            if (!this.<>f__this.m_netCacheReady)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0178;
            }
        Label_0081:
            while (!this.<>f__this.m_achievesReady)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0178;
            }
        Label_00A9:
            while (!this.<>f__this.m_rewardsLoaded)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0178;
            }
        Label_00D1:
            while (!this.<>f__this.m_twoScoop.IsLoaded())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0178;
            }
        Label_0118:
            while (this.<>f__this.JustEarnedGoldenHero())
            {
                if (this.<>f__this.m_goldenHeroEventReady)
                {
                    break;
                }
                this.$current = null;
                this.$PC = 5;
                goto Label_0178;
            }
            this.<>f__this.m_twoScoop.Show();
            this.<>f__this.OnTwoScoopShown();
            this.<>f__this.m_haveShownTwoScoop = true;
            if (EndGameScreen.OnTwoScoopsShown != null)
            {
                EndGameScreen.OnTwoScoopsShown(true, this.<>f__this.m_twoScoop);
            }
            this.$PC = -1;
        Label_0176:
            return false;
        Label_0178:
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
    private sealed class <ToMode>c__Iterator6F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SceneMgr.Mode <$>mode;
        internal SceneMgr.Mode mode;

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
                    SceneMgr.Get().SetNextMode(this.mode);
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
    private sealed class <WaitForAchieveManager>c__Iterator71 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndGameScreen <>f__this;

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
                    if (!AchieveManager.Get().IsReady())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_achievesReady = true;
                    this.<>f__this.MaybeUpdateRewards();
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

    public delegate void OnTwoScoopsShownHandler(bool shown, EndGameTwoScoop twoScoops);
}

