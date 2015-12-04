using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class QuestLog : UIBPopup
{
    private ArenaMedal m_arenaMedal;
    public Transform m_arenaMedalBone;
    public ArenaMedal m_arenaMedalPrefab;
    public List<ClassProgressBar> m_classProgressBars;
    public List<ClassProgressInfo> m_classProgressInfos;
    public ClassProgressBar m_classProgressPrefab;
    public UIBButton m_closeButton;
    private TournamentMedal m_currentMedal;
    private List<QuestTile> m_currentQuests;
    public UberText m_forgeRecordCountText;
    private int m_justCanceledQuestID;
    public Transform m_medalBone;
    public TournamentMedal m_medalPrefab;
    public UberText m_noQuestText;
    public PegUIElement m_offClickCatcher;
    private Enum[] m_presencePrevStatus;
    public List<Transform> m_questBones;
    public GameObject m_questTilePrefab;
    public RankedRewardChest2D m_rewardChest;
    public GameObject m_root;
    public UberText m_totalLevelsText;
    public UberText m_winsCountText;
    [CustomEditField(Sections="Aspect Ratio Positioning")]
    public float m_yPosRoot16to9 = 0.475f;
    [CustomEditField(Sections="Aspect Ratio Positioning")]
    public float m_yPosRoot3to2;
    private static QuestLog s_instance;

    private void AddCurrentQuestTile(Achievement achieveQuest, int slot)
    {
        GameObject go = (GameObject) GameUtils.Instantiate(this.m_questTilePrefab, this.m_questBones[slot].gameObject, true);
        SceneUtils.SetLayer(go, this.m_questBones[slot].gameObject.layer);
        go.transform.localScale = Vector3.one;
        QuestTile component = go.GetComponent<QuestTile>();
        component.SetupTile(achieveQuest);
        component.SetCanShowCancelButton(true);
        this.m_currentQuests.Add(component);
    }

    private void Awake()
    {
        s_instance = this;
        this.m_presencePrevStatus = PresenceMgr.Get().GetStatus();
        Enum[] args = new Enum[] { PresenceStatus.QUESTLOG };
        PresenceMgr.Get().SetStatus(args);
        if (this.m_closeButton != null)
        {
            this.m_closeButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCloseButtonReleased));
        }
    }

    private void DeleteQuests()
    {
        if ((this.m_currentQuests != null) && (this.m_currentQuests.Count != 0))
        {
            for (int i = 0; i < this.m_currentQuests.Count; i++)
            {
                if (this.m_currentQuests[i] != null)
                {
                    UnityEngine.Object.Destroy(this.m_currentQuests[i].gameObject);
                }
            }
        }
    }

    public static QuestLog Get()
    {
        return s_instance;
    }

    protected override void Hide(bool animate)
    {
        if (this.m_presencePrevStatus == null)
        {
            this.m_presencePrevStatus = new Enum[] { PresenceStatus.HUB };
        }
        PresenceMgr.Get().SetStatus(this.m_presencePrevStatus);
        if (ShownUIMgr.Get() != null)
        {
            ShownUIMgr.Get().ClearShownUI();
        }
        base.DoHideAnimation(!animate, delegate {
            AchieveManager.Get().RemoveQuestCanceledListener(new AchieveManager.AchieveCanceledCallback(this.OnQuestCanceled));
            this.DeleteQuests();
            FullScreenFXMgr.Get().EndStandardBlurVignette(0.1f, null);
            base.m_shown = false;
        });
    }

    private void OnActiveAchievesUpdated(object userData)
    {
        <OnActiveAchievesUpdated>c__AnonStorey321 storey = new <OnActiveAchievesUpdated>c__AnonStorey321 {
            justCanceledQuest = this.m_justCanceledQuestID
        };
        this.m_justCanceledQuestID = 0;
        QuestTile tile = this.m_currentQuests.Find(new Predicate<QuestTile>(storey.<>m__14F));
        if (tile == null)
        {
            Debug.LogError(string.Format("QuestLog.OnActiveAchievesUpdated(): could not find tile for just canceled quest (quest ID {0})", storey.justCanceledQuest));
            this.Hide();
        }
        else
        {
            List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests(true);
            if ((activeQuests.Count < 1) || (((activeQuests.Count > 1) && !Vars.Key("Quests.CanCancelManyTimes").GetBool(false)) && !Vars.Key("Quests.CancelGivesManyNewQuests").GetBool(false)))
            {
                Debug.LogError(string.Format("QuestLog.OnActiveAchievesUpdated(): expecting ONE new active quest after a quest cancel but received {0}", activeQuests.Count));
                this.Hide();
            }
            else
            {
                tile.SetupTile(activeQuests[0]);
                tile.PlayBirth();
                for (int i = 1; i < activeQuests.Count; i++)
                {
                    int count = this.m_currentQuests.Count;
                    if (count >= this.m_questBones.Count)
                    {
                        break;
                    }
                    this.AddCurrentQuestTile(activeQuests[i], count);
                }
                foreach (QuestTile tile2 in this.m_currentQuests)
                {
                    tile2.UpdateCancelButtonVisibility();
                }
            }
        }
    }

    private void OnCloseButtonReleased(UIEvent e)
    {
        this.OnNavigateBack();
    }

    private void OnDestroy()
    {
        if (ShownUIMgr.Get() != null)
        {
            ShownUIMgr.Get().ClearShownUI();
        }
        if (AchieveManager.Get() != null)
        {
            AchieveManager.Get().RemoveActiveAchievesUpdatedListener(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
            AchieveManager.Get().RemoveQuestCanceledListener(new AchieveManager.AchieveCanceledCallback(this.OnQuestCanceled));
        }
        if (Network.IsRunning())
        {
            PresenceMgr.Get().SetPrevStatus();
        }
        s_instance = null;
    }

    private bool OnNavigateBack()
    {
        this.Hide(true);
        return true;
    }

    private void OnQuestCanceled(int achieveID, bool canceled, object userData)
    {
        object[] args = new object[] { achieveID, canceled };
        Log.Rachelle.Print("QuestLog.OnQuestCanceled({0},{1})", args);
        if (canceled)
        {
            this.m_justCanceledQuestID = achieveID;
            AchieveManager.Get().UpdateActiveAchieves(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnActiveAchievesUpdated));
        }
    }

    private void OnQuestLogCloseEvent(UIEvent e)
    {
        Navigation.GoBack();
    }

    public override void Show()
    {
        AchieveManager.Get().RegisterQuestCanceledListener(new AchieveManager.AchieveCanceledCallback(this.OnQuestCanceled));
        this.UpdateData();
        FullScreenFXMgr.Get().StartStandardBlurVignette(0.1f);
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
        base.Show();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.WIDTH);
        }
    }

    private void Start()
    {
        if (this.m_classProgressPrefab != null)
        {
            for (int i = 0; i < this.m_classProgressInfos.Count; i++)
            {
                ClassProgressInfo info = this.m_classProgressInfos[i];
                TAG_CLASS tag_class = info.m_class;
                ClassProgressBar c = (ClassProgressBar) GameUtils.Instantiate(this.m_classProgressPrefab, info.m_bone, true);
                SceneUtils.SetLayer(c, info.m_bone.layer);
                TransformUtil.Identity(c.transform);
                c.m_class = tag_class;
                c.m_classIcon.GetComponent<Renderer>().material = info.m_iconMaterial;
                c.Init();
                this.m_classProgressBars.Add(c);
            }
        }
        this.m_offClickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnQuestLogCloseEvent));
    }

    private void UpdateActiveQuests()
    {
        List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests(false);
        Log.Ben.Print(string.Format("Found {0} activeQuests! I should do something awesome with them here.", activeQuests.Count), new object[0]);
        this.m_currentQuests = new List<QuestTile>();
        for (int i = 0; i < activeQuests.Count; i++)
        {
            if (i < 3)
            {
                this.AddCurrentQuestTile(activeQuests[i], i);
            }
        }
        if (this.m_currentQuests.Count == 0)
        {
            this.m_noQuestText.gameObject.SetActive(true);
            if (AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.DAILY_QUESTS))
            {
                this.m_noQuestText.Text = GameStrings.Get("GLUE_QUEST_LOG_NO_QUESTS_DAILIES_UNLOCKED");
                if (!Options.Get().GetBool(Option.HAS_RUN_OUT_OF_QUESTS, false))
                {
                    NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_OUT_OF_QUESTS"), "VO_INNKEEPER_OUT_OF_QUESTS", 0f, null);
                    Options.Get().SetBool(Option.HAS_RUN_OUT_OF_QUESTS, true);
                }
            }
            else
            {
                this.m_noQuestText.Text = GameStrings.Get("GLUE_QUEST_LOG_NO_QUESTS");
            }
        }
        else
        {
            this.m_noQuestText.gameObject.SetActive(false);
        }
    }

    private void UpdateBestArenaMedal()
    {
        NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
        if (this.m_arenaMedal == null)
        {
            this.m_arenaMedal = (ArenaMedal) GameUtils.Instantiate(this.m_arenaMedalPrefab, this.m_arenaMedalBone.gameObject, true);
            SceneUtils.SetLayer(this.m_arenaMedal, this.m_arenaMedalBone.gameObject.layer);
            this.m_arenaMedal.transform.localScale = Vector3.one;
        }
        if (netObject.LastForgeDate != 0)
        {
            this.m_arenaMedal.gameObject.SetActive(true);
            this.m_arenaMedal.SetMedal(netObject.BestForgeWins);
        }
        else
        {
            this.m_arenaMedal.gameObject.SetActive(false);
        }
    }

    private void UpdateClassProgress()
    {
        if (this.m_classProgressBars.Count != 0)
        {
            int num = 0;
            List<Achievement> achievesInGroup = AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO, true);
            NetCache.NetCacheHeroLevels netObject = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
            <UpdateClassProgress>c__AnonStorey320 storey = new <UpdateClassProgress>c__AnonStorey320();
            using (List<ClassProgressBar>.Enumerator enumerator = this.m_classProgressBars.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storey.classProgress = enumerator.Current;
                    if (storey.classProgress.m_classLockedGO != null)
                    {
                        if (achievesInGroup.Find(new Predicate<Achievement>(storey.<>m__14D)) != null)
                        {
                            storey.classProgress.m_classLockedGO.SetActive(false);
                            NetCache.HeroLevel level = netObject.Levels.Find(new Predicate<NetCache.HeroLevel>(storey.<>m__14E));
                            storey.classProgress.m_levelText.Text = level.CurrentLevel.Level.ToString();
                            if (level.CurrentLevel.IsMaxLevel())
                            {
                                storey.classProgress.m_progressBar.SetProgressBar(1f);
                            }
                            else
                            {
                                storey.classProgress.m_progressBar.SetProgressBar(((float) level.CurrentLevel.XP) / ((float) level.CurrentLevel.MaxXP));
                            }
                            storey.classProgress.SetNextReward(level.NextReward);
                            num += level.CurrentLevel.Level;
                        }
                        else
                        {
                            storey.classProgress.m_levelText.Text = string.Empty;
                            storey.classProgress.m_classLockedGO.SetActive(true);
                        }
                    }
                }
            }
            if (this.m_totalLevelsText != null)
            {
                this.m_totalLevelsText.Text = string.Format(GameStrings.Get("GLUE_QUEST_LOG_TOTAL_LEVELS"), num);
            }
        }
    }

    private void UpdateCurrentMedal()
    {
        NetCache.NetCacheMedalInfo netObject = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
        if (this.m_currentMedal == null)
        {
            this.m_currentMedal = (TournamentMedal) GameUtils.Instantiate(this.m_medalPrefab, this.m_medalBone.gameObject, true);
            SceneUtils.SetLayer(this.m_currentMedal, base.gameObject.layer);
            this.m_currentMedal.transform.localScale = Vector3.one;
        }
        this.m_currentMedal.SetMedal(netObject, false);
        int rank = 0x1a - netObject.BestStarLevel;
        this.m_rewardChest.SetRank(rank);
    }

    private void UpdateData()
    {
        this.UpdateClassProgress();
        this.UpdateActiveQuests();
        this.UpdateCurrentMedal();
        this.UpdateBestArenaMedal();
        this.UpdateTotalWins();
    }

    private void UpdateTotalWins()
    {
        int num = 0;
        int num2 = 0;
        foreach (NetCache.PlayerRecord record in NetCache.Get().GetNetObject<NetCache.NetCachePlayerRecords>().Records)
        {
            if (record.Data != 0)
            {
                continue;
            }
            GameType recordType = record.RecordType;
            switch (recordType)
            {
                case GameType.GT_ARENA:
                {
                    num2 += record.Wins;
                    continue;
                }
                case GameType.GT_RANKED:
                case GameType.GT_UNRANKED:
                    break;

                default:
                    if (recordType != GameType.GT_TAVERNBRAWL)
                    {
                        continue;
                    }
                    break;
            }
            num += record.Wins;
        }
        this.m_winsCountText.Text = num.ToString();
        this.m_forgeRecordCountText.Text = num2.ToString();
    }

    [CompilerGenerated]
    private sealed class <OnActiveAchievesUpdated>c__AnonStorey321
    {
        internal int justCanceledQuest;

        internal bool <>m__14F(QuestTile obj)
        {
            return (obj.GetQuestID() == this.justCanceledQuest);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateClassProgress>c__AnonStorey320
    {
        internal ClassProgressBar classProgress;

        internal bool <>m__14D(Achievement obj)
        {
            return (obj.ClassRequirement.HasValue && (((TAG_CLASS) obj.ClassRequirement.Value) == this.classProgress.m_class));
        }

        internal bool <>m__14E(NetCache.HeroLevel obj)
        {
            return (obj.Class == this.classProgress.m_class);
        }
    }
}

