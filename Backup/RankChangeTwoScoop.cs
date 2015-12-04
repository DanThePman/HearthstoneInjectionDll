using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class RankChangeTwoScoop : MonoBehaviour
{
    protected Vector3 AFTER_PUNCH_SCALE;
    protected Vector3 END_SCALE;
    public GameObject m_bannerBottom;
    public GameObject m_bannerTop;
    private int m_chestRank;
    private List<RankChangeStar> m_chestStars = new List<RankChangeStar>();
    private RankChangeClosed m_closedCallback;
    private TranslatedMedalInfo m_currMedalInfo;
    public PegUIElement m_debugClickCatcher;
    public List<Transform> m_evenStarBones;
    public Material m_legendaryMaterial;
    public UberText m_legendIndex;
    public PlayMakerFSM m_mainFSM;
    public GameObject m_medalContainer;
    private MedalInfoTranslator m_medalInfoTranslator;
    public UberText m_name;
    private int m_numTexturesLoading;
    public List<Transform> m_oddStarBones;
    private TranslatedMedalInfo m_prevMedalInfo;
    public GameObject m_rankMedalBottom;
    public GameObject m_rankMedalTop;
    public UberText m_rankNumberBottom;
    public UberText m_rankNumberTop;
    public RankedRewardChest m_rewardChest;
    public MeshRenderer m_rewardChestGlow;
    public UberText m_rewardChestInstructions;
    public GameObject m_scrubRankDesc;
    public RankChangeStar m_starPrefab;
    private List<RankChangeStar> m_stars = new List<RankChangeStar>();
    private bool m_validPrevMedal;
    public GameObject m_winStreakParent;
    private const float STAR_ACTION_DELAY = 0.2f;
    protected Vector3 START_SCALE;

    public void AnimateRankChange()
    {
        float delay = 0f;
        switch (this.m_medalInfoTranslator.GetChangeType())
        {
            case RankChangeType.RANK_UP:
                delay = this.IncreaseStars(this.m_prevMedalInfo.earnedStars, this.m_prevMedalInfo.totalStars);
                if (!this.m_medalInfoTranslator.ShowRewardChest())
                {
                    base.StartCoroutine(this.PlayLevelUp(delay + 0.6f));
                    break;
                }
                base.StartCoroutine(this.PlayChestLevelUp(delay + 0.6f));
                break;

            case RankChangeType.RANK_DOWN:
                delay = this.DecreaseStars(0, this.m_prevMedalInfo.earnedStars);
                base.StartCoroutine(this.PlayLevelDown(delay + 0.2f));
                break;

            case RankChangeType.RANK_SAME:
                if (this.m_currMedalInfo.IsLegendRank())
                {
                    if (this.m_currMedalInfo.legendIndex == 0)
                    {
                        this.m_legendIndex.gameObject.SetActive(false);
                    }
                    else
                    {
                        this.m_legendIndex.gameObject.SetActive(true);
                    }
                    this.UpdateLegendText(this.m_currMedalInfo.legendIndex);
                    break;
                }
                if (this.m_currMedalInfo.earnedStars <= this.m_prevMedalInfo.earnedStars)
                {
                    delay = this.DecreaseStars(this.m_currMedalInfo.earnedStars, this.m_prevMedalInfo.earnedStars);
                    break;
                }
                delay = this.IncreaseStars(this.m_prevMedalInfo.earnedStars, this.m_currMedalInfo.earnedStars);
                break;
        }
        base.StartCoroutine(this.EnableHitboxOnAnimFinished(delay));
    }

    private void Awake()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1.2f,
            Phone = 0.8f
        };
        float x = 1.25f * value2;
        this.START_SCALE = new Vector3(0.01f, 0.01f, 0.01f);
        this.END_SCALE = new Vector3(x, x, x);
        this.AFTER_PUNCH_SCALE = new Vector3((float) value2, (float) value2, (float) value2);
        for (int i = 0; i < 5; i++)
        {
            this.m_stars.Add((RankChangeStar) GameUtils.Instantiate(this.m_starPrefab, base.gameObject, false));
            this.m_chestStars.Add((RankChangeStar) GameUtils.Instantiate(this.m_starPrefab, base.gameObject, false));
        }
        for (int j = 0; j < this.m_chestStars.Count; j++)
        {
            this.m_chestStars[j].GetComponent<UberFloaty>().enabled = true;
            this.m_chestStars[j].gameObject.SetActive(false);
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_name.Width *= 1.2f;
        }
        this.m_winStreakParent.SetActive(false);
        this.m_medalContainer.SetActive(false);
        this.m_debugClickCatcher.gameObject.SetActive(false);
    }

    public void CheatRankUp(string[] args)
    {
        bool flag = false;
        bool flag2 = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower() == "winstreak")
            {
                flag = true;
            }
            if (args[i].ToLower() == "chest")
            {
                flag2 = true;
            }
        }
        this.m_debugClickCatcher.gameObject.SetActive(true);
        this.m_debugClickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.Hide));
        this.m_medalInfoTranslator = new MedalInfoTranslator();
        TranslatedMedalInfo prevMedal = new TranslatedMedalInfo();
        TranslatedMedalInfo currMedal = new TranslatedMedalInfo();
        int result = 14;
        int num3 = 15;
        int num4 = 3;
        int num5 = 1;
        if (args.Length >= 2)
        {
            int.TryParse(args[0], out result);
            int.TryParse(args[1], out num3);
        }
        if (args.Length >= 4)
        {
            int.TryParse(args[2], out num4);
            int.TryParse(args[3], out num5);
        }
        prevMedal.earnedStars = num4;
        prevMedal.totalStars = 3;
        if (result <= 15)
        {
            prevMedal.totalStars = 4;
        }
        if (result <= 10)
        {
            prevMedal.totalStars = 5;
        }
        prevMedal.canLoseStars = result <= 20;
        prevMedal.canLoseLevel = result < 20;
        prevMedal.name = string.Format("Rank {0}", result);
        prevMedal.nextMedalName = string.Format("Rank {0}", num3);
        prevMedal.rank = result;
        prevMedal.textureName = string.Format("Medal_Ranked_{0}", result);
        currMedal.earnedStars = num5;
        currMedal.totalStars = 3;
        if (num3 <= 15)
        {
            currMedal.totalStars = 4;
        }
        if (num3 <= 10)
        {
            currMedal.totalStars = 5;
        }
        if (num3 == 0)
        {
            currMedal.legendIndex = 0x539;
        }
        currMedal.canLoseStars = num3 <= 20;
        currMedal.canLoseLevel = num3 < 20;
        currMedal.name = string.Format("Rank {0}", num3);
        currMedal.nextMedalName = string.Format("Rank {0}", num3);
        currMedal.rank = num3;
        currMedal.textureName = string.Format("Medal_Ranked_{0}", num3);
        if (flag)
        {
            currMedal.winStreak = 3;
        }
        if (flag2)
        {
            prevMedal.bestRank = prevMedal.rank;
            currMedal.bestRank = currMedal.rank;
        }
        this.m_medalInfoTranslator.TestSetMedalInfo(currMedal, prevMedal);
    }

    private float DecreaseStars(int lastWipeIndex, int firstWipeIndex)
    {
        bool flag = EndGameScreen.Get() is DefeatScreen;
        bool flag2 = false;
        if (flag && this.m_validPrevMedal)
        {
            if (!this.m_currMedalInfo.canLoseStars)
            {
                flag2 = true;
            }
            else if ((this.m_currMedalInfo.IsHighestRankThatCannotBeLost() && (this.m_currMedalInfo.earnedStars == 0)) && ((this.m_prevMedalInfo.rank == this.m_currMedalInfo.rank) && (this.m_prevMedalInfo.earnedStars == 0)))
            {
                flag2 = true;
            }
        }
        this.m_scrubRankDesc.SetActive(flag2);
        float delay = 0f;
        for (int i = this.m_stars.Count - 1; i >= 0; i--)
        {
            delay = (this.m_stars.Count - i) * 0.2f;
            RankChangeStar star = this.m_stars[i];
            if ((star.gameObject.activeInHierarchy && (i < firstWipeIndex)) && (i >= lastWipeIndex))
            {
                star.Wipe(delay);
            }
        }
        return delay;
    }

    private void DespawnOldStars()
    {
        this.m_winStreakParent.SetActive(false);
        for (int i = 0; i < this.m_stars.Count; i++)
        {
            if (this.m_stars[i].gameObject.activeSelf)
            {
                this.m_stars[i].GetComponent<PlayMakerFSM>().SendEvent("DeSpawn");
            }
        }
    }

    private void DestroyRankChange()
    {
        if (this.m_closedCallback != null)
        {
            this.m_closedCallback();
        }
        UnityEngine.Object.Destroy(base.gameObject);
    }

    [DebuggerHidden]
    private IEnumerator EnableHitboxOnAnimFinished(float delay)
    {
        return new <EnableHitboxOnAnimFinished>c__Iterator78 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    private void Hide(UIEvent e)
    {
        if (EndGameScreen.Get() != null)
        {
            EndGameScreen.Get().m_hitbox.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.Hide));
        }
        if (base.gameObject != null)
        {
            AnimationUtil.ScaleFade(base.gameObject, new Vector3(0.1f, 0.1f, 0.1f), "DestroyRankChange");
        }
        if (SoundManager.Get() != null)
        {
            SoundManager.Get().LoadAndPlay("rank_window_shrink");
        }
    }

    private float IncreaseStars(int blinkToThisIndex, int burstToThisIndex)
    {
        if (!this.m_currMedalInfo.IsLegendRank())
        {
            int num = 0;
            if (this.m_prevMedalInfo.rank != this.m_currMedalInfo.rank)
            {
                int num2 = this.m_prevMedalInfo.totalStars - this.m_prevMedalInfo.earnedStars;
                int earnedStars = this.m_currMedalInfo.earnedStars;
                num = num2 + earnedStars;
            }
            else
            {
                num = this.m_currMedalInfo.earnedStars - this.m_prevMedalInfo.earnedStars;
            }
            if ((this.m_currMedalInfo.winStreak >= 3) && (num > 1))
            {
                this.m_winStreakParent.SetActive(true);
            }
        }
        float delay = 0f;
        for (int i = 0; i < this.m_stars.Count; i++)
        {
            delay = (i + 1) * 0.2f;
            RankChangeStar star = this.m_stars[i];
            if (i < blinkToThisIndex)
            {
                star.Blink(delay);
            }
            else if (i < burstToThisIndex)
            {
                star.Burst(delay);
            }
        }
        return delay;
    }

    public void Initialize(MedalInfoTranslator medalInfoTranslator, RankChangeClosed callback)
    {
        if (medalInfoTranslator != null)
        {
            this.m_medalInfoTranslator = medalInfoTranslator;
        }
        this.m_currMedalInfo = this.m_medalInfoTranslator.GetCurrentMedal();
        this.m_prevMedalInfo = this.m_medalInfoTranslator.GetPreviousMedal();
        this.m_validPrevMedal = this.m_medalInfoTranslator.IsPreviousMedalValid();
        this.m_closedCallback = callback;
        this.InitMedalsAndStars();
    }

    private void InitMedalsAndStars()
    {
        switch (this.m_medalInfoTranslator.GetChangeType())
        {
            case RankChangeType.RANK_UP:
                this.m_rankNumberTop.Text = this.m_prevMedalInfo.rank.ToString();
                this.m_name.Text = this.m_prevMedalInfo.name;
                this.m_numTexturesLoading = 2;
                AssetLoader.Get().LoadTexture(this.m_prevMedalInfo.textureName, new AssetLoader.ObjectCallback(this.OnTopTextureLoaded), null, false);
                AssetLoader.Get().LoadTexture(this.m_currMedalInfo.textureName, new AssetLoader.ObjectCallback(this.OnBottomTextureLoaded), null, false);
                this.InitStars(this.m_prevMedalInfo.earnedStars, this.m_prevMedalInfo.totalStars, false);
                return;

            case RankChangeType.RANK_DOWN:
                this.m_rankNumberBottom.Text = this.m_currMedalInfo.rank.ToString();
                this.m_rankNumberTop.Text = this.m_prevMedalInfo.rank.ToString();
                this.m_name.Text = this.m_prevMedalInfo.name;
                this.m_numTexturesLoading = 2;
                AssetLoader.Get().LoadTexture(this.m_prevMedalInfo.textureName, new AssetLoader.ObjectCallback(this.OnTopTextureLoaded), null, false);
                AssetLoader.Get().LoadTexture(this.m_currMedalInfo.textureName, new AssetLoader.ObjectCallback(this.OnBottomTextureLoaded), null, false);
                this.InitStars(this.m_prevMedalInfo.earnedStars, this.m_prevMedalInfo.totalStars, false);
                return;

            case RankChangeType.RANK_SAME:
                if (this.m_currMedalInfo.rank != 0)
                {
                    this.m_rankNumberTop.Text = this.m_currMedalInfo.rank.ToString();
                    this.m_bannerBottom.SetActive(false);
                    this.m_rankNumberBottom.gameObject.SetActive(false);
                    break;
                }
                this.m_bannerTop.SetActive(false);
                this.m_bannerBottom.SetActive(false);
                this.m_rankMedalBottom.SetActive(false);
                break;

            default:
                return;
        }
        this.m_name.Text = this.m_currMedalInfo.name;
        this.m_numTexturesLoading = 1;
        AssetLoader.Get().LoadTexture(this.m_currMedalInfo.textureName, new AssetLoader.ObjectCallback(this.OnTopTextureLoaded), null, false);
        this.InitStars(this.m_prevMedalInfo.earnedStars, this.m_prevMedalInfo.totalStars, false);
    }

    private void InitStars(int numEarned, int numTotal, bool fadeIn = false)
    {
        List<Transform> evenStarBones;
        int num = 0;
        if ((numTotal % 2) == 0)
        {
            evenStarBones = this.m_evenStarBones;
            if (numTotal == 2)
            {
                num = 1;
            }
        }
        else
        {
            evenStarBones = this.m_oddStarBones;
            if (numTotal == 3)
            {
                num = 1;
            }
            else if (numTotal == 1)
            {
                num = 2;
            }
        }
        for (int i = 0; i < numTotal; i++)
        {
            RankChangeStar star = this.m_stars[i];
            star.gameObject.SetActive(true);
            star.transform.localScale = new Vector3(0.22f, 0.1f, 0.22f);
            star.transform.position = evenStarBones[i + num].position;
            star.Reset();
            if (i >= numEarned)
            {
                star.BlackOut();
            }
            else
            {
                star.UnBlackOut();
            }
            if (fadeIn)
            {
                star.FadeIn();
            }
        }
        for (int j = numTotal; j < this.m_stars.Count; j++)
        {
            RankChangeStar star2 = this.m_stars[j];
            star2.gameObject.SetActive(false);
        }
    }

    [DebuggerHidden]
    private IEnumerator LegendaryChanges()
    {
        return new <LegendaryChanges>c__Iterator7E { <>f__this = this };
    }

    private void LevelUpChanges()
    {
        this.m_rankMedalTop.GetComponent<Renderer>().material.mainTexture = this.m_rankMedalBottom.GetComponent<Renderer>().material.mainTexture;
        this.m_rankNumberTop.Text = this.m_currMedalInfo.rank.ToString();
        this.m_name.Text = this.m_currMedalInfo.name;
        if (Gameplay.Get() != null)
        {
            Gameplay.Get().UpdateFriendlySideMedalChange(this.m_medalInfoTranslator);
        }
    }

    private void OnBottomTextureLoaded(string assetName, UnityEngine.Object asset, object callbackData)
    {
        if (asset == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("RankChangeTwoScoop.OnBottomTextureLoaded(): asset for {0} is null!", assetName));
        }
        else
        {
            Texture texture = asset as Texture;
            if (texture == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("RankChangeTwoScoop.OnBottomTextureLoaded(): medalTexture for {0} is null (asset is not a texture)!", assetName));
            }
            else
            {
                this.m_rankMedalBottom.GetComponent<Renderer>().material.mainTexture = texture;
                this.m_numTexturesLoading--;
                this.Show();
            }
        }
    }

    private void OnDestroy()
    {
        foreach (RankChangeStar star in this.m_stars)
        {
            if (star.gameObject != null)
            {
                UnityEngine.Object.Destroy(star.gameObject);
            }
        }
        if (EndGameScreen.Get() != null)
        {
            EndGameScreen.Get().m_hitbox.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.Hide));
        }
    }

    private void OnTopTextureLoaded(string assetName, UnityEngine.Object asset, object callbackData)
    {
        if (asset == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("RankChangeTwoScoop.OnTopTextureLoaded(): asset for {0} is null!", assetName));
        }
        else
        {
            Texture texture = asset as Texture;
            if (texture == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("RankChangeTwoScoop.OnTopTextureLoaded(): medalTexture for {0} is null (asset is not a texture)!", assetName));
            }
            else
            {
                this.m_rankMedalTop.GetComponent<Renderer>().material.mainTexture = texture;
                this.m_numTexturesLoading--;
                this.Show();
            }
        }
    }

    private void PlayChestChange()
    {
        if (this.m_prevMedalInfo.CanGetRankedRewardChest() && this.m_rewardChest.DoesChestVisualChange(this.m_chestRank, this.m_currMedalInfo.bestRank))
        {
            this.m_rewardChest.GetChestVisualFromRank(this.m_currMedalInfo.bestRank).m_glowMesh.gameObject.SetActive(true);
            Animator component = base.GetComponent<Animator>();
            string chestChangeAnimation = this.m_rewardChest.GetChestVisualFromRank(this.m_currMedalInfo.bestRank).chestChangeAnimation;
            Log.EndOfGame.Print("playing chest change animation " + chestChangeAnimation, new object[0]);
            component.Play(chestChangeAnimation);
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayChestLevelUp(float delay)
    {
        return new <PlayChestLevelUp>c__Iterator7A { delay = delay, <$>delay = delay, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PlayLevelDown(float delay)
    {
        return new <PlayLevelDown>c__Iterator7D { delay = delay, <$>delay = delay, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator PlayLevelUp(float delay)
    {
        return new <PlayLevelUp>c__Iterator79 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    private void Show()
    {
        if (this.m_numTexturesLoading <= 0)
        {
            this.m_medalContainer.SetActive(true);
            AnimationUtil.ShowWithPunch(base.gameObject, this.START_SCALE, this.END_SCALE, this.AFTER_PUNCH_SCALE, "AnimateRankChange", true, null, null, null);
            SoundManager.Get().LoadAndPlay("rank_window_expand");
        }
    }

    private void StartChestGlow()
    {
        if (SoundManager.Get() != null)
        {
            SoundManager.Get().LoadAndPlay("tutorial_intro_box_opens");
        }
    }

    private void UpdateChestAfterLevelUp()
    {
        if (!this.m_rewardChest.DoesChestVisualChange(this.m_chestRank, this.m_currMedalInfo.bestRank))
        {
            this.UpdateToFinalChest();
        }
    }

    private void UpdateLegendText(int legendIndex)
    {
        if (legendIndex == -1)
        {
            this.m_legendIndex.Text = string.Empty;
        }
        else if (legendIndex == 0)
        {
            this.m_legendIndex.Text = string.Empty;
        }
        else
        {
            this.m_legendIndex.Text = legendIndex.ToString();
        }
    }

    private void UpdateToCurrentChest()
    {
        base.StartCoroutine(this.UpdateToCurrentChestCoroutine());
    }

    [DebuggerHidden]
    private IEnumerator UpdateToCurrentChestCoroutine()
    {
        return new <UpdateToCurrentChestCoroutine>c__Iterator7B { <>f__this = this };
    }

    private void UpdateToFinalChest()
    {
        if ((SoundManager.Get() != null) && this.m_prevMedalInfo.CanGetRankedRewardChest())
        {
            SoundManager.Get().LoadAndPlay("level_up");
        }
        Log.EndOfGame.Print("Updating to final chest..", new object[0]);
        this.m_rewardChest.SetRank(this.m_currMedalInfo.bestRank);
        this.m_rewardChestInstructions.gameObject.SetActive(true);
        this.m_rewardChestInstructions.TextAlpha = 0f;
        iTween.FadeTo(this.m_rewardChestInstructions.gameObject, 1f, 1f);
    }

    [DebuggerHidden]
    private IEnumerator WaitAndPlayRankedStarImpact(float delay)
    {
        return new <WaitAndPlayRankedStarImpact>c__Iterator7C { delay = delay, <$>delay = delay };
    }

    [CompilerGenerated]
    private sealed class <EnableHitboxOnAnimFinished>c__Iterator78 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeTwoScoop <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (EndGameScreen.Get() != null)
                    {
                        EndGameScreen.Get().m_hitbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.<>f__this.Hide));
                    }
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
    private sealed class <LegendaryChanges>c__Iterator7E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<RankChangeStar>.Enumerator <$s_492>__0;
        internal RankChangeTwoScoop <>f__this;
        internal RankChangeStar <star>__1;

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
                    this.<>f__this.m_legendIndex.gameObject.SetActive(true);
                    this.<>f__this.m_legendIndex.Text = string.Empty;
                    this.$current = new WaitForSeconds(0.3f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_rankMedalTop.GetComponent<Renderer>().material = this.<>f__this.m_legendaryMaterial;
                    this.<>f__this.UpdateLegendText(this.<>f__this.m_currMedalInfo.legendIndex);
                    this.<>f__this.m_name.Text = this.<>f__this.m_currMedalInfo.name;
                    this.<>f__this.m_bannerBottom.SetActive(false);
                    this.<$s_492>__0 = this.<>f__this.m_stars.GetEnumerator();
                    try
                    {
                        while (this.<$s_492>__0.MoveNext())
                        {
                            this.<star>__1 = this.<$s_492>__0.Current;
                            this.<star>__1.gameObject.SetActive(false);
                        }
                    }
                    finally
                    {
                        this.<$s_492>__0.Dispose();
                    }
                    if (Gameplay.Get() != null)
                    {
                        Gameplay.Get().UpdateFriendlySideMedalChange(this.<>f__this.m_medalInfoTranslator);
                    }
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
    private sealed class <PlayChestLevelUp>c__Iterator7A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal List<ChestVisual>.Enumerator <$s_491>__7;
        internal RankChangeTwoScoop <>f__this;
        internal Animator <animator>__0;
        internal Vector3 <away>__3;
        internal ChestVisual <chestVisual>__8;
        internal Hashtable <glowArgs>__6;
        internal int <i>__1;
        internal int <i>__2;
        internal Hashtable <moveArgs>__5;
        internal Vector3 <starPosition>__4;
        internal float delay;

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
                    this.<>f__this.gameObject.GetComponent<Animator>().enabled = true;
                    this.<>f__this.m_winStreakParent.transform.localPosition += new Vector3(0f, 0f, 0.558f);
                    this.<animator>__0 = this.<>f__this.GetComponent<Animator>();
                    this.$current = new WaitForSeconds(this.delay + 0.25f);
                    this.$PC = 1;
                    goto Label_0754;

                case 1:
                    if (this.<>f__this.m_prevMedalInfo.CanGetRankedRewardChest())
                    {
                        if (SoundManager.Get() != null)
                        {
                            SoundManager.Get().LoadAndPlay("tutorial_intro_hub_circle_360");
                        }
                        this.<>f__this.m_chestStars.RemoveRange(this.<>f__this.m_prevMedalInfo.totalStars - 1, this.<>f__this.m_chestStars.Count - this.<>f__this.m_prevMedalInfo.totalStars);
                        this.<i>__1 = 0;
                        while (this.<i>__1 < this.<>f__this.m_chestStars.Count)
                        {
                            TransformUtil.CopyLocal(this.<>f__this.m_chestStars[this.<i>__1], this.<>f__this.m_stars[this.<i>__1]);
                            this.<>f__this.m_chestStars[this.<i>__1].gameObject.SetActive(true);
                            this.<>f__this.m_stars[this.<i>__1].gameObject.SetActive(false);
                            this.<i>__1++;
                        }
                        this.<i>__2 = 0;
                        while (this.<i>__2 < this.<>f__this.m_chestStars.Count)
                        {
                            this.<away>__3 = this.<>f__this.m_stars[this.<i>__2].gameObject.transform.position - this.<>f__this.m_medalContainer.transform.position;
                            this.<away>__3.Normalize();
                            this.<away>__3 += (Vector3) (10f * Vector3.forward);
                            this.<away>__3.Normalize();
                            this.<starPosition>__4 = this.<>f__this.m_stars[this.<i>__2].gameObject.transform.position + ((Vector3) (0.4f * this.<away>__3));
                            this.<starPosition>__4.y = this.<>f__this.m_rewardChest.m_rankNumber.transform.position.y;
                            object[] args = new object[] { "islocal", false, "position", this.<starPosition>__4, "time", 2f, "easetype", iTween.EaseType.easeOutElastic };
                            this.<moveArgs>__5 = iTween.Hash(args);
                            this.<>f__this.m_chestStars[this.<i>__2].m_topGlowRenderer.enabled = true;
                            iTween.ColorTo(this.<>f__this.m_chestStars[this.<i>__2].m_bottomGlowRenderer.gameObject, new Color(0.1f, 0.1f, 0.1f), 0.5f);
                            object[] objArray2 = new object[] { "islocal", false, "position", this.<starPosition>__4, "color", new Color(0.36f, 0.25f, 0.25f), "time", 5f, "easetype", iTween.EaseType.easeOutBack };
                            this.<glowArgs>__6 = iTween.Hash(objArray2);
                            iTween.ColorTo(this.<>f__this.m_chestStars[this.<i>__2].m_topGlowRenderer.gameObject, this.<glowArgs>__6);
                            iTween.MoveTo(this.<>f__this.m_chestStars[this.<i>__2].gameObject, this.<moveArgs>__5);
                            iTween.ScaleBy(this.<>f__this.m_chestStars[this.<i>__2].gameObject, (Vector3) (0.85f * Vector3.one), 0.5f);
                            this.$current = new WaitForSeconds(0.1f);
                            this.$PC = 2;
                            goto Label_0754;
                        Label_04B5:
                            this.<i>__2++;
                        }
                    }
                    this.$current = new WaitForSeconds(0.4f);
                    this.$PC = 3;
                    goto Label_0754;

                case 2:
                    goto Label_04B5;

                case 3:
                    this.<>f__this.m_mainFSM.SendEvent("Wipe");
                    break;

                case 4:
                    break;

                case 5:
                    if (this.<>f__this.m_prevMedalInfo.CanGetRankedRewardChest())
                    {
                        this.<animator>__0.Play("MedalRanked_to_RewardChest");
                    }
                    else
                    {
                        this.<animator>__0.Play("MedalRanked_ChestChange_spawn");
                    }
                    this.$PC = -1;
                    goto Label_0752;

                default:
                    goto Label_0752;
            }
            if (this.<>f__this.m_mainFSM.ActiveStateName != "MedalWipeDone")
            {
                this.$current = new WaitForEndOfFrame();
                this.$PC = 4;
            }
            else
            {
                this.<>f__this.m_chestRank = this.<>f__this.m_prevMedalInfo.CanGetRankedRewardChest() ? this.<>f__this.m_prevMedalInfo.rank : this.<>f__this.m_currMedalInfo.rank;
                this.<$s_491>__7 = this.<>f__this.m_rewardChest.m_chests.GetEnumerator();
                try
                {
                    while (this.<$s_491>__7.MoveNext())
                    {
                        this.<chestVisual>__8 = this.<$s_491>__7.Current;
                        Log.EndOfGame.Print("setting chest visual inactive " + this.<chestVisual>__8.m_glowMesh, new object[0]);
                        this.<chestVisual>__8.m_glowMesh.gameObject.SetActive(false);
                    }
                }
                finally
                {
                    this.<$s_491>__7.Dispose();
                }
                this.<>f__this.m_rewardChest.GetChestVisualFromRank(this.<>f__this.m_chestRank).m_glowMesh.gameObject.SetActive(true);
                if (this.<>f__this.m_currMedalInfo.earnedStars > 0)
                {
                    this.<>f__this.m_prevMedalInfo.rank = this.<>f__this.m_currMedalInfo.rank;
                    this.<>f__this.m_prevMedalInfo.earnedStars = 0;
                    this.<>f__this.m_prevMedalInfo.totalStars = this.<>f__this.m_currMedalInfo.totalStars;
                }
                this.<>f__this.LevelUpChanges();
                this.<>f__this.InitStars(this.<>f__this.m_prevMedalInfo.earnedStars, this.<>f__this.m_prevMedalInfo.totalStars, true);
                this.<>f__this.AnimateRankChange();
                this.$current = new WaitForSeconds(2f);
                this.$PC = 5;
            }
            goto Label_0754;
        Label_0752:
            return false;
        Label_0754:
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
    private sealed class <PlayLevelDown>c__Iterator7D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeTwoScoop <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    goto Label_0172;

                case 1:
                    this.<>f__this.m_mainFSM.SendEvent("Wipe");
                    if (this.<>f__this.m_currMedalInfo.earnedStars > this.<>f__this.m_currMedalInfo.totalStars)
                    {
                        break;
                    }
                    this.<>f__this.m_prevMedalInfo.rank = this.<>f__this.m_currMedalInfo.rank;
                    this.<>f__this.m_prevMedalInfo.earnedStars = this.<>f__this.m_currMedalInfo.totalStars;
                    this.<>f__this.m_prevMedalInfo.totalStars = this.<>f__this.m_currMedalInfo.totalStars;
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 2;
                    goto Label_0172;

                case 2:
                    this.<>f__this.m_name.Text = this.<>f__this.m_currMedalInfo.name;
                    this.<>f__this.m_rankNumberTop.Text = this.<>f__this.m_currMedalInfo.rank.ToString();
                    this.<>f__this.InitStars(this.<>f__this.m_currMedalInfo.earnedStars, this.<>f__this.m_currMedalInfo.totalStars, false);
                    break;

                default:
                    goto Label_0170;
            }
            this.$PC = -1;
        Label_0170:
            return false;
        Label_0172:
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
    private sealed class <PlayLevelUp>c__Iterator79 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeTwoScoop <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    goto Label_016A;

                case 1:
                    if ((this.<>f__this.m_prevMedalInfo.rank != 1) || (this.<>f__this.m_currMedalInfo.rank != 0))
                    {
                        this.<>f__this.m_mainFSM.SendEvent("Burst");
                        this.$current = new WaitForSeconds(0.45f);
                        this.$PC = 3;
                    }
                    else
                    {
                        this.<>f__this.m_mainFSM.SendEvent("Burst2");
                        this.$current = this.<>f__this.StartCoroutine(this.<>f__this.LegendaryChanges());
                        this.$PC = 2;
                    }
                    goto Label_016A;

                case 3:
                    if (this.<>f__this.m_currMedalInfo.earnedStars > 0)
                    {
                        this.<>f__this.m_prevMedalInfo.rank = this.<>f__this.m_currMedalInfo.rank;
                        this.<>f__this.m_prevMedalInfo.earnedStars = 0;
                        this.<>f__this.m_prevMedalInfo.totalStars = this.<>f__this.m_currMedalInfo.totalStars;
                        this.<>f__this.LevelUpChanges();
                        this.<>f__this.InitMedalsAndStars();
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_016A:
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
    private sealed class <UpdateToCurrentChestCoroutine>c__Iterator7B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        private static Action<object> <>f__am$cache8;
        internal RankChangeTwoScoop <>f__this;
        internal Animator <animator>__0;
        internal int <i>__2;
        internal string <levelUpAnimation>__1;
        internal Hashtable <moveArgs>__4;
        internal RankChangeStar <star>__3;

        private static void <>m__E8(object s)
        {
            ((RankChangeStar) s).gameObject.SetActive(false);
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
                    this.<animator>__0 = this.<>f__this.GetComponent<Animator>();
                    this.<>f__this.m_rewardChest.SetRank(this.<>f__this.m_chestRank);
                    if (this.<>f__this.m_prevMedalInfo.CanGetRankedRewardChest())
                    {
                        this.$current = new WaitForSeconds(0.8f);
                        this.$PC = 1;
                        goto Label_024E;
                    }
                    goto Label_024C;

                case 1:
                    this.<levelUpAnimation>__1 = this.<>f__this.m_rewardChest.GetChestVisualFromRank(this.<>f__this.m_currMedalInfo.bestRank).levelUpAnimation;
                    this.<animator>__0.Play(this.<levelUpAnimation>__1);
                    Log.EndOfGame.Print("playing level up animation " + this.<levelUpAnimation>__1, new object[0]);
                    this.<i>__2 = 0;
                    break;

                case 2:
                    this.<i>__2++;
                    break;

                default:
                    goto Label_024C;
            }
            if (this.<i>__2 < this.<>f__this.m_chestStars.Count)
            {
                this.<star>__3 = this.<>f__this.m_chestStars[(this.<>f__this.m_chestStars.Count - 1) - this.<i>__2];
                object[] args = new object[12];
                args[0] = "islocal";
                args[1] = false;
                args[2] = "position";
                args[3] = this.<>f__this.m_rewardChest.m_rankNumber.transform.position;
                args[4] = "time";
                args[5] = 0.5f;
                args[6] = "easetype";
                args[7] = iTween.EaseType.easeInBack;
                args[8] = "oncompleteparams";
                args[9] = this.<star>__3;
                args[10] = "oncomplete";
                if (<>f__am$cache8 == null)
                {
                    <>f__am$cache8 = new Action<object>(RankChangeTwoScoop.<UpdateToCurrentChestCoroutine>c__Iterator7B.<>m__E8);
                }
                args[11] = <>f__am$cache8;
                this.<moveArgs>__4 = iTween.Hash(args);
                this.<>f__this.StartCoroutine(this.<>f__this.WaitAndPlayRankedStarImpact(0.5f));
                iTween.MoveTo(this.<star>__3.gameObject, this.<moveArgs>__4);
                this.$current = new WaitForSeconds(0.1f);
                this.$PC = 2;
                goto Label_024E;
            }
            this.$PC = -1;
        Label_024C:
            return false;
        Label_024E:
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
    private sealed class <WaitAndPlayRankedStarImpact>c__Iterator7C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (SoundManager.Get() != null)
                    {
                        SoundManager.Get().LoadAndPlay("rank_star_gain");
                    }
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

    public delegate void RankChangeClosed();
}

