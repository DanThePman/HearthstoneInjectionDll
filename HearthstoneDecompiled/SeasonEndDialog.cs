using System;
using System.Collections.Generic;
using UnityEngine;

public class SeasonEndDialog : DialogBase
{
    public GameObject m_bonusStar;
    public GameObject m_bonusStarFlourish;
    public GameObject m_bonusStarItems;
    public UberText m_bonusStarLabel;
    private int m_bonusStars;
    public UberText m_bonusStarText;
    public UberText m_bonusStarTitle;
    public GameObject m_boostedMedalBone;
    public GameObject m_boostedMedalLeftFiligreeBone;
    public GameObject m_boostedMedalRightFiligreeBone;
    private bool m_chestOpened;
    private MODE m_currentMode;
    private bool m_earnedRewardChest;
    public UberText m_header;
    public GameObject m_leftFiligree;
    public GameObject m_legendaryGem;
    public Vector3 m_loadPosition;
    public TournamentMedal m_medal;
    public PlayMakerFSM m_medalPlayMaker;
    public GameObject m_nameFlourish;
    public StandardPegButtonNew m_okayButton;
    public UberText m_rankAchieved;
    public UberText m_rankName;
    public UberText m_rankPercentile;
    public GameObject m_rewardBoxesBone;
    public PegUIElement m_rewardChest;
    public UberText m_rewardChestHeader;
    public UberText m_rewardChestInstructions;
    public GameObject m_rewardChestLeftFiligreeBone;
    public GameObject m_rewardChestPage;
    public GameObject m_rewardChestRightFiligreeBone;
    public List<PegUIElement> m_rewardChests;
    public GameObject m_ribbon;
    public GameObject m_rightFiligree;
    private SeasonEndInfo m_seasonEndInfo;
    public GameObject m_shieldIcon;
    public Vector3 m_showPosition;
    public PlayMakerFSM m_starPlayMaker;
    public Material m_transparentMaterial;
    public UberText m_welcomeDetails;
    public GameObject m_welcomeItems;
    public UberText m_welcomeTitle;
    private static string[] s_percentiles = new string[] { 
        "0.25", "0.33", "0.5", "1", "2", "3", "4", "5", "7", "9", "12", "15", "20", "25", "30", "40", 
        "45", "50"
     };
    private bool TESTING;

    protected override void Awake()
    {
        base.Awake();
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.m_rewardChestInstructions.Text = GameStrings.Format("GLOBAL_SEASON_END_CHEST_INSTRUCTIONS_TOUCH", new object[0]);
        }
        if (!this.TESTING)
        {
            this.m_okayButton.SetText(GameStrings.Get("GLOBAL_BUTTON_NEXT"));
            this.m_okayButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OkayButtonReleased));
            this.m_medal.SetEnabled(false);
        }
    }

    private void ChestButtonReleased(UIEvent e)
    {
        if (!this.m_chestOpened)
        {
            this.m_chestOpened = true;
            this.m_rewardChest.GetComponent<PlayMakerFSM>().SendEvent("StartAnim");
        }
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.SetBlurBrightness(1f);
        mgr.SetBlurDesaturation(0f);
        mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
        mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
        mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
    }

    private string GetInlineSeasonName(int seasonID)
    {
        DbfRecord record = GameDbf.Season.GetRecord(seasonID);
        if (record == null)
        {
            Debug.LogError(string.Format("SeasonEndDialog.GetInlineSeasonName() - There is no Season DBF record for ID {0}", seasonID));
            return "NO RECORD FOUND";
        }
        return record.GetLocString("SEASON_START_NAME");
    }

    public static string GetRankPercentile(int rank)
    {
        int index = rank - 1;
        if (index < 0)
        {
            index = 0;
        }
        if (index >= s_percentiles.Length)
        {
            return string.Empty;
        }
        if (Localization.DoesLocaleUseDecimalPoint(Localization.GetLocale()))
        {
            return s_percentiles[index];
        }
        return s_percentiles[index].Replace(".", ",");
    }

    private string GetSeasonName(int seasonID)
    {
        DbfRecord record = GameDbf.Season.GetRecord(seasonID);
        if (record == null)
        {
            Debug.LogError(string.Format("SeasonEndDialog.GetSeasonName() - There is no Season DBF record for ID {0}", seasonID));
            return "NO RECORD FOUND";
        }
        return record.GetLocString("NAME");
    }

    public void GotoBonusStars()
    {
        this.m_currentMode = MODE.BONUS_STARS;
        this.m_medalPlayMaker.SendEvent("PageTear");
        this.m_rewardChestPage.SetActive(false);
        this.m_welcomeItems.SetActive(false);
        this.m_bonusStarItems.SetActive(true);
        this.m_bonusStarText.Text = this.m_bonusStars.ToString();
        GameStrings.PluralNumber[] numberArray1 = new GameStrings.PluralNumber[1];
        GameStrings.PluralNumber number = new GameStrings.PluralNumber {
            m_number = this.m_bonusStars
        };
        numberArray1[0] = number;
        GameStrings.PluralNumber[] pluralNumbers = numberArray1;
        this.m_bonusStarLabel.Text = GameStrings.FormatPlurals("GLOBAL_SEASON_END_BONUS_STARS_LABEL", pluralNumbers, new object[0]);
        this.m_bonusStarTitle.Text = GameStrings.Get("GLOBAL_SEASON_END_BONUS_STAR_TITLE");
    }

    public void GotoBonusStarsOrWelcome()
    {
        if (this.m_bonusStars > 0)
        {
            this.GotoBonusStars();
        }
        else
        {
            this.GotoSeasonWelcome();
        }
    }

    public void GotoBoostedMedal()
    {
        this.m_currentMode = MODE.BOOSTED_WELCOME;
        this.m_starPlayMaker.SendEvent("Burst Big");
        this.m_medal.transform.position = this.m_boostedMedalBone.transform.position;
        this.m_medal.SetMedal(new MedalInfoTranslator(this.m_seasonEndInfo.m_boostedRank, 0), false);
        this.m_leftFiligree.transform.position = this.m_boostedMedalLeftFiligreeBone.transform.position;
        this.m_rightFiligree.transform.position = this.m_boostedMedalRightFiligreeBone.transform.position;
    }

    public void GotoSeasonWelcome()
    {
        this.m_currentMode = MODE.SEASON_WELCOME;
        this.m_medalPlayMaker.SendEvent("PageTear");
        this.m_welcomeItems.SetActive(true);
        this.m_bonusStarItems.SetActive(false);
        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
        string seasonName = this.GetSeasonName(netObject.Season);
        this.m_header.Text = seasonName;
        object[] args = new object[] { seasonName };
        this.m_welcomeDetails.Text = GameStrings.Format("GLOBAL_SEASON_END_NEW_SEASON", args);
    }

    public override void Hide()
    {
        base.Hide();
        this.FadeEffectsOut();
        SoundManager.Get().LoadAndPlay("rank_window_shrink");
    }

    public void HideBonusStarText()
    {
        this.m_bonusStarText.gameObject.SetActive(false);
    }

    public void HideMedal()
    {
        this.m_medal.gameObject.SetActive(false);
    }

    public void HideRewardChest()
    {
        this.m_rewardChestPage.SetActive(false);
    }

    public void Init(SeasonEndInfo info)
    {
        this.m_seasonEndInfo = info;
        this.m_header.Text = this.GetSeasonName(info.m_seasonID);
        if ((info.m_rankedRewards != null) && (info.m_rankedRewards.Count > 0))
        {
            this.m_earnedRewardChest = true;
        }
        else
        {
            this.m_earnedRewardChest = false;
        }
        this.m_medal.SetMedal(new MedalInfoTranslator(info.m_rank, info.m_legendIndex), false);
        this.m_rankName.Text = this.m_medal.GetMedal().name;
        this.m_bonusStars = info.m_bonusStars;
        string rankPercentile = GetRankPercentile(this.m_seasonEndInfo.m_rank);
        if (rankPercentile.Length > 0)
        {
            this.m_rankPercentile.gameObject.SetActive(true);
            object[] args = new object[] { rankPercentile };
            this.m_rankPercentile.Text = GameStrings.Format("GLOBAL_SEASON_END_PERCENTILE_LABEL", args);
        }
        else
        {
            this.m_rankPercentile.gameObject.SetActive(false);
        }
        foreach (PegUIElement element in this.m_rewardChests)
        {
            element.gameObject.SetActive(false);
        }
        int chestIndexFromRank = RankedRewardChest.GetChestIndexFromRank(this.m_seasonEndInfo.m_chestRank);
        if (chestIndexFromRank >= 0)
        {
            this.m_rewardChest = this.m_rewardChests[chestIndexFromRank];
            this.m_rewardChest.gameObject.SetActive(true);
            this.m_medalPlayMaker.FsmVariables.GetFsmGameObject("RankChest").Value = this.m_rewardChest.gameObject;
            UberText[] componentsInChildren = this.m_rewardChest.GetComponentsInChildren<UberText>(true);
            if (componentsInChildren.Length > 0)
            {
                componentsInChildren[0].Text = info.m_chestRank.ToString();
            }
            this.m_rewardChestHeader.Text = RankedRewardChest.GetChestEarnedFromRank(this.m_seasonEndInfo.m_chestRank);
        }
        this.m_rewardChest.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ChestButtonReleased));
    }

    public void MedalAnimationFinished()
    {
        if (this.m_earnedRewardChest)
        {
            this.m_currentMode = MODE.CHEST_EARNED;
            this.m_medalPlayMaker.SendEvent("RevealRewardChest");
            iTween.FadeTo(this.m_rankAchieved.gameObject, 0f, 0.5f);
        }
        else
        {
            this.GotoBonusStarsOrWelcome();
            this.m_okayButton.Enable();
        }
    }

    public void MedalInFinished()
    {
        this.m_okayButton.SetText("GLOBAL_DONE");
        this.m_okayButton.Enable();
    }

    private void OkayButtonReleased(UIEvent e)
    {
        if ((this.m_currentMode == MODE.SEASON_WELCOME) || (this.m_currentMode == MODE.BOOSTED_WELCOME))
        {
            this.Hide();
            foreach (long num in this.m_seasonEndInfo.m_noticesToAck)
            {
                Network.AckNotice(num);
            }
            this.m_okayButton.Disable();
        }
        else if (this.m_currentMode == MODE.RANK_EARNED)
        {
            this.m_ribbon.GetComponent<Renderer>().material = this.m_transparentMaterial;
            this.m_nameFlourish.GetComponent<Renderer>().material = this.m_transparentMaterial;
            iTween.FadeTo(this.m_nameFlourish.gameObject, 0f, 0.5f);
            iTween.FadeTo(this.m_rankName.gameObject, 0f, 0.5f);
            iTween.FadeTo(this.m_rankAchieved.gameObject, 0f, 0.5f);
            iTween.FadeTo(this.m_leftFiligree.gameObject, 0f, 0.5f);
            iTween.FadeTo(this.m_rightFiligree.gameObject, 0f, 0.5f);
            if (this.m_medal.GetMedal().rank == 0)
            {
                this.m_medalPlayMaker.SendEvent("JustMedal");
            }
            else
            {
                this.m_medalPlayMaker.SendEvent("MedalBanner");
            }
            this.m_okayButton.Disable();
            this.m_rankPercentile.gameObject.SetActive(false);
        }
        else if (this.m_currentMode == MODE.BONUS_STARS)
        {
            this.GotoBoostedMedal();
        }
    }

    public void OnDestroy()
    {
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
    }

    protected override void OnHideAnimFinished()
    {
        UniversalInputManager.Get().SetGameDialogActive(false);
        base.OnHideAnimFinished();
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (mode != SceneMgr.Mode.HUB)
        {
            this.Hide();
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void OpenRewards()
    {
        AssetLoader.GameObjectCallback callback = delegate (string name, GameObject go, object callbackData) {
            if (SoundManager.Get() != null)
            {
                SoundManager.Get().LoadAndPlay("card_turn_over_legendary");
            }
            RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
            component.SetRewards(this.m_seasonEndInfo.m_rankedRewards);
            component.m_playBoxFlyoutSound = false;
            component.SetLayer(GameLayer.PerspectiveUI);
            component.UseDarkeningClickCatcher(true);
            component.RegisterDoneCallback(() => this.m_rewardChest.GetComponent<PlayMakerFSM>().SendEvent("SummonOut"));
            component.transform.localPosition = this.m_rewardBoxesBone.transform.localPosition;
            component.transform.localRotation = this.m_rewardBoxesBone.transform.localRotation;
            component.transform.localScale = this.m_rewardBoxesBone.transform.localScale;
            component.AnimateRewards();
        };
        AssetLoader.Get().LoadGameObject("RewardBoxes", callback, null, false);
        iTween.FadeTo(this.m_rewardChestInstructions.gameObject, 0f, 0.5f);
    }

    public void PageTearFinished()
    {
        if (this.m_currentMode == MODE.SEASON_WELCOME)
        {
            this.m_okayButton.SetText("GLOBAL_DONE");
        }
        this.m_okayButton.Enable();
    }

    public override void Show()
    {
        this.FadeEffectsIn();
        base.Show();
        base.DoShowAnimation();
        UniversalInputManager.Get().SetGameDialogActive(true);
        SoundManager.Get().LoadAndPlay("rank_window_expand");
    }

    public void ShowMedal()
    {
        this.m_medal.gameObject.SetActive(true);
    }

    public void ShowRewardChest()
    {
        if (this.m_seasonEndInfo.m_rank == 0)
        {
            this.m_legendaryGem.SetActive(true);
        }
        this.m_rewardChestPage.SetActive(true);
        this.m_leftFiligree.transform.position = this.m_rewardChestLeftFiligreeBone.transform.position;
        this.m_rightFiligree.transform.position = this.m_rewardChestRightFiligreeBone.transform.position;
        iTween.FadeTo(this.m_leftFiligree.gameObject, 1f, 0.5f);
        iTween.FadeTo(this.m_rightFiligree.gameObject, 1f, 0.5f);
    }

    public void StarBurstFinished()
    {
        if (this.m_medal.GetMedal().rank == 0)
        {
            this.m_medalPlayMaker.SendEvent("JustMedalIn");
        }
        else
        {
            this.m_medalPlayMaker.SendEvent("MedalBannerIn");
        }
        this.m_bonusStarText.gameObject.SetActive(false);
        this.m_bonusStarLabel.Text = this.m_medal.GetMedal().name;
        NetCache.NetCacheRewardProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
        string inlineSeasonName = this.GetInlineSeasonName(netObject.Season);
        object[] args = new object[] { inlineSeasonName };
        this.m_bonusStarTitle.Text = GameStrings.Format("GLOBAL_SEASON_END_NEW_SEASON", args);
    }

    private enum MODE
    {
        RANK_EARNED,
        CHEST_EARNED,
        SEASON_WELCOME,
        BONUS_STARS,
        BOOSTED_WELCOME
    }

    public class SeasonEndInfo
    {
        public int m_bonusStars;
        public int m_boostedRank;
        public int m_chestRank;
        public bool m_isFake;
        public int m_legendIndex;
        public List<long> m_noticesToAck = new List<long>();
        public int m_rank;
        public List<RewardData> m_rankedRewards;
        public int m_seasonID;
    }
}

