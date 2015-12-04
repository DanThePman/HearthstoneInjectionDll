using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TutorialProgressScreen : MonoBehaviour
{
    private PlatformDependentValue<Vector3> FINAL_POS;
    private PlatformDependentValue<Vector3> FINAL_POS_OVER_BOX;
    private Vector3 FINAL_SCALE;
    private Vector3 FINAL_SCALE_OVER_BOX;
    private Vector3 HERO_COIN_START;
    private const float HERO_SPACING = -0.2f;
    private bool IS_TESTING;
    public HeroCoin m_coinPrefab;
    private HeroCoin.CoinPressCallback m_coinPressCallback;
    public GameObject m_currentLessonBone;
    public PegUIElement m_exitButton;
    public UberText m_exitButtonLabel;
    private List<HeroCoin> m_heroCoins = new List<HeroCoin>();
    public UberText m_lessonTitle;
    private readonly Map<ScenarioDbId, LessonAsset> m_missionIdToLessonAssetMap;
    public UberText m_missionProgressTitle;
    private readonly Map<TutorialProgress, ScenarioDbId> m_progressToNextMissionIdMap;
    private bool m_showProgressSavedMessage;
    private List<DbfRecord> m_sortedMissionRecords;
    private static TutorialProgressScreen s_instance;
    private Vector3 START_SCALE;
    private const float START_SCALE_VAL = 0.5f;

    public TutorialProgressScreen()
    {
        Map<TutorialProgress, ScenarioDbId> map = new Map<TutorialProgress, ScenarioDbId>();
        map.Add(TutorialProgress.NOTHING_COMPLETE, ScenarioDbId.TUTORIAL_HOGGER);
        map.Add(TutorialProgress.HOGGER_COMPLETE, ScenarioDbId.TUTORIAL_MILLHOUSE);
        map.Add(TutorialProgress.MILLHOUSE_COMPLETE, ScenarioDbId.TUTORIAL_CHO);
        map.Add(TutorialProgress.CHO_COMPLETE, ScenarioDbId.TUTORIAL_MUKLA);
        map.Add(TutorialProgress.MUKLA_COMPLETE, ScenarioDbId.TUTORIAL_NESINGWARY);
        map.Add(TutorialProgress.NESINGWARY_COMPLETE, ScenarioDbId.TUTORIAL_ILLIDAN);
        this.m_progressToNextMissionIdMap = map;
        Map<ScenarioDbId, LessonAsset> map2 = new Map<ScenarioDbId, LessonAsset>();
        map2.Add(ScenarioDbId.TUTORIAL_HOGGER, null);
        LessonAsset asset = new LessonAsset {
            m_asset = "Tutorial_Lesson1"
        };
        map2.Add(ScenarioDbId.TUTORIAL_MILLHOUSE, asset);
        asset = new LessonAsset {
            m_asset = "Tutorial_Lesson2",
            m_phoneAsset = "Tutorial_Lesson2_phone"
        };
        map2.Add(ScenarioDbId.TUTORIAL_CHO, asset);
        asset = new LessonAsset {
            m_asset = "Tutorial_Lesson3"
        };
        map2.Add(ScenarioDbId.TUTORIAL_MUKLA, asset);
        asset = new LessonAsset {
            m_asset = "Tutorial_Lesson4"
        };
        map2.Add(ScenarioDbId.TUTORIAL_NESINGWARY, asset);
        asset = new LessonAsset {
            m_asset = "Tutorial_Lesson5"
        };
        map2.Add(ScenarioDbId.TUTORIAL_ILLIDAN, asset);
        this.m_missionIdToLessonAssetMap = map2;
        this.m_sortedMissionRecords = new List<DbfRecord>();
        this.START_SCALE = new Vector3(0.5f, 0.5f, 0.5f);
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(7f, 1f, 7f),
            Phone = new Vector3(6.1f, 1f, 6.1f)
        };
        this.FINAL_SCALE = (Vector3) value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(92.5f, 14f, 92.5f),
            Phone = new Vector3(106f, 16f, 106f)
        };
        this.FINAL_SCALE_OVER_BOX = (Vector3) value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(-8f, 5f, -5f),
            Phone = new Vector3(-8f, 5f, -4.58f)
        };
        this.FINAL_POS = value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(0f, 5f, -0.2f),
            Phone = new Vector3(0f, 5f, -2.06f)
        };
        this.FINAL_POS_OVER_BOX = value2;
    }

    private void Awake()
    {
        s_instance = this;
        FullScreenFXMgr.Get().Vignette(1f, 0.5f, iTween.EaseType.easeInOutQuad, null);
        this.m_lessonTitle.Text = GameStrings.Get("TUTORIAL_PROGRESS_LESSON_TITLE");
        this.m_missionProgressTitle.Text = GameStrings.Get("TUTORIAL_PROGRESS_TITLE");
        this.m_exitButton.gameObject.SetActive(false);
        this.InitMissionRecords();
    }

    private void ExitButtonPress(UIEvent e)
    {
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        FullScreenFXMgr.Get().Vignette(0f, 0.5f, iTween.EaseType.easeInOutQuad, null);
    }

    public static TutorialProgressScreen Get()
    {
        return s_instance;
    }

    private void Hide()
    {
        object[] args = new object[] { "scale", this.START_SCALE, "time", 0.5f, "oncomplete", "OnHideAnimComplete", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ScaleTo(base.gameObject, hashtable);
        object[] objArray2 = new object[] { "alpha", 0f, "time", 0.25f, "delay", 0.25f };
        hashtable = iTween.Hash(objArray2);
        iTween.FadeTo(base.gameObject, hashtable);
    }

    private void InitMissionRecords()
    {
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            if (record.GetInt("ADVENTURE_ID") == 1)
            {
                int id = record.GetId();
                if (Enum.IsDefined(typeof(ScenarioDbId), id))
                {
                    this.m_sortedMissionRecords.Add(record);
                }
            }
        }
        this.m_sortedMissionRecords.Sort(new Comparison<DbfRecord>(GameUtils.MissionSortComparison));
    }

    private void LoadAllTutorialHeroEntities()
    {
        for (int i = 0; i < this.m_sortedMissionRecords.Count; i++)
        {
            string missionHeroCardId = GameUtils.GetMissionHeroCardId(this.m_sortedMissionRecords[i].GetId());
            if (DefLoader.Get().GetEntityDef(missionHeroCardId) == null)
            {
                UnityEngine.Debug.LogError(string.Format("TutorialProgress.OnTutorialHeroEntityDefLoaded() - failed to load {0}", missionHeroCardId));
            }
        }
        this.SetupCoins();
        this.Show();
    }

    private void OnDestroy()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.UpdateProgress));
        s_instance = null;
    }

    private void OnHideAnimComplete()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void OnScaleAnimComplete()
    {
        if (this.IS_TESTING)
        {
            this.UpdateProgress();
        }
        else
        {
            NetCache.Get().RegisterTutorialEndGameScreen(new NetCache.NetCacheCallback(this.UpdateProgress), new NetCache.ErrorCallback(NetCache.DefaultErrorHandler));
        }
        foreach (HeroCoin coin in this.m_heroCoins)
        {
            coin.FinishIntroScaling();
        }
    }

    private void OnTutorialImageLoaded(string name, GameObject go, object callbackData)
    {
        this.SetupTutorialImage(go);
    }

    [DebuggerHidden]
    private IEnumerator SetActiveToDefeated(HeroCoin coin)
    {
        return new <SetActiveToDefeated>c__Iterator1ED { coin = coin, <$>coin = coin };
    }

    public void SetCoinPressCallback(HeroCoin.CoinPressCallback callback)
    {
        <SetCoinPressCallback>c__AnonStorey34B storeyb = new <SetCoinPressCallback>c__AnonStorey34B {
            callback = callback,
            <>f__this = this
        };
        if (storeyb.callback != null)
        {
            this.m_coinPressCallback = new HeroCoin.CoinPressCallback(storeyb.<>m__1AD);
        }
    }

    [DebuggerHidden]
    private IEnumerator SetUnrevealedToActive(HeroCoin coin)
    {
        return new <SetUnrevealedToActive>c__Iterator1EE { coin = coin, <$>coin = coin };
    }

    private void SetupCoins()
    {
        this.HERO_COIN_START = new Vector3(0.5f, 0.1f, 0.32f);
        Vector3 zero = Vector3.zero;
        for (int i = 0; i < this.m_sortedMissionRecords.Count; i++)
        {
            Vector2 vector2;
            LessonAsset asset;
            int id = this.m_sortedMissionRecords[i].GetId();
            HeroCoin item = UnityEngine.Object.Instantiate<HeroCoin>(this.m_coinPrefab);
            item.transform.parent = base.transform;
            item.gameObject.SetActive(false);
            item.SetCoinPressCallback(this.m_coinPressCallback);
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 1:
                    vector2 = new Vector2(0.25f, -1f);
                    break;

                case 2:
                    vector2 = new Vector2(0.5f, -1f);
                    break;

                default:
                    vector2 = new Vector2(0f, -1f);
                    break;
            }
            if (i == 0)
            {
                item.transform.localPosition = this.HERO_COIN_START;
            }
            else
            {
                item.transform.localPosition = new Vector3(zero.x + -0.2f, zero.y, zero.z);
            }
            string phoneAsset = null;
            this.m_missionIdToLessonAssetMap.TryGetValue((ScenarioDbId) id, out asset);
            if (asset != null)
            {
                if ((UniversalInputManager.UsePhoneUI != null) && !string.IsNullOrEmpty(asset.m_phoneAsset))
                {
                    phoneAsset = asset.m_phoneAsset;
                }
                else
                {
                    phoneAsset = asset.m_asset;
                }
            }
            if (!string.IsNullOrEmpty(phoneAsset))
            {
                item.SetLessonAssetName(phoneAsset);
            }
            this.m_heroCoins.Add(item);
            Vector2 goldTexture = Vector2.zero;
            Vector2 grayTexture = Vector2.zero;
            int num4 = id;
            switch (num4)
            {
                case 3:
                    goldTexture = new Vector2(0f, -0.25f);
                    grayTexture = new Vector2(0.25f, -0.25f);
                    break;

                case 4:
                    goldTexture = new Vector2(0.5f, 0f);
                    grayTexture = new Vector2(0.75f, 0f);
                    break;

                case 0xf8:
                    goldTexture = new Vector2(0f, -0.5f);
                    grayTexture = new Vector2(0.25f, -0.5f);
                    break;

                case 0xf9:
                    goldTexture = new Vector2(0.5f, -0.5f);
                    grayTexture = new Vector2(0.75f, -0.5f);
                    break;

                case 0xb5:
                    goldTexture = new Vector2(0.5f, -0.25f);
                    grayTexture = new Vector2(0.75f, -0.25f);
                    break;

                default:
                    if (num4 == 0xc9)
                    {
                        goldTexture = new Vector2(0f, 0f);
                        grayTexture = new Vector2(0.25f, 0f);
                    }
                    break;
            }
            item.SetCoinInfo(goldTexture, grayTexture, vector2, id);
            zero = item.transform.localPosition;
        }
        SceneUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
    }

    private void SetupTutorialImage(GameObject go)
    {
        SceneUtils.SetLayer(go, GameLayer.IgnoreFullScreenEffects);
        go.transform.parent = this.m_currentLessonBone.transform;
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = Vector3.zero;
    }

    private void Show()
    {
        iTween.FadeTo(base.gameObject, 1f, 0.25f);
        bool flag = SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY;
        base.transform.position = !flag ? ((Vector3) this.FINAL_POS_OVER_BOX) : ((Vector3) this.FINAL_POS);
        base.transform.localScale = this.START_SCALE;
        object[] args = new object[] { "scale", !flag ? this.FINAL_SCALE_OVER_BOX : this.FINAL_SCALE, "time", 0.5f, "oncomplete", "OnScaleAnimComplete", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ScaleTo(base.gameObject, hashtable);
    }

    public void StartTutorialProgress()
    {
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
        {
            if (((TAG_PLAYSTATE) GameState.Get().GetFriendlySidePlayer().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE)) == TAG_PLAYSTATE.WON)
            {
                GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActorSpell(SpellType.ENDGAME_WIN).ActivateState(SpellStateType.DEATH);
                this.m_showProgressSavedMessage = true;
            }
            Gameplay.Get().RemoveGamePlayNameBannerPhone();
        }
        this.LoadAllTutorialHeroEntities();
    }

    private void UpdateProgress()
    {
        <UpdateProgress>c__AnonStorey34C storeyc = new <UpdateProgress>c__AnonStorey34C();
        if (this.IS_TESTING)
        {
            storeyc.nextMissionId = this.m_progressToNextMissionIdMap[TutorialProgress.HOGGER_COMPLETE];
        }
        else
        {
            NetCache.NetCacheProfileProgress netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
            storeyc.nextMissionId = this.m_progressToNextMissionIdMap[netObject.CampaignProgress];
        }
        int num = this.m_heroCoins.FindIndex(new Predicate<HeroCoin>(storeyc.<>m__1AE));
        for (int i = 0; i < this.m_heroCoins.Count; i++)
        {
            HeroCoin coin = this.m_heroCoins[i];
            if (i == (num - 1))
            {
                base.StartCoroutine(this.SetActiveToDefeated(coin));
            }
            else if (i < num)
            {
                coin.SetProgress(HeroCoin.CoinStatus.DEFEATED);
            }
            else if (i == num)
            {
                base.StartCoroutine(this.SetUnrevealedToActive(coin));
                string lessonAssetName = coin.GetLessonAssetName();
                if (!string.IsNullOrEmpty(lessonAssetName))
                {
                    AssetLoader.Get().LoadGameObject(lessonAssetName, new AssetLoader.GameObjectCallback(this.OnTutorialImageLoaded), null, false);
                }
            }
            else
            {
                coin.SetProgress(HeroCoin.CoinStatus.UNREVEALED);
            }
        }
        if (this.m_showProgressSavedMessage)
        {
            UIStatus.Get().AddInfo(GameStrings.Get("TUTORIAL_PROGRESS_SAVED"));
            this.m_showProgressSavedMessage = false;
        }
    }

    [CompilerGenerated]
    private sealed class <SetActiveToDefeated>c__Iterator1ED : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroCoin <$>coin;
        internal HeroCoin coin;

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
                    this.coin.SetProgress(HeroCoin.CoinStatus.ACTIVE);
                    this.coin.m_inputEnabled = false;
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.coin.SetProgress(HeroCoin.CoinStatus.ACTIVE_TO_DEFEATED);
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
    private sealed class <SetCoinPressCallback>c__AnonStorey34B
    {
        internal TutorialProgressScreen <>f__this;
        internal HeroCoin.CoinPressCallback callback;

        internal void <>m__1AD()
        {
            this.<>f__this.Hide();
            this.callback();
        }
    }

    [CompilerGenerated]
    private sealed class <SetUnrevealedToActive>c__Iterator1EE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroCoin <$>coin;
        internal HeroCoin coin;

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
                    this.coin.SetProgress(HeroCoin.CoinStatus.UNREVEALED);
                    this.coin.m_inputEnabled = false;
                    this.$current = new WaitForSeconds(2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.coin.SetProgress(HeroCoin.CoinStatus.UNREVEALED_TO_ACTIVE);
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
    private sealed class <UpdateProgress>c__AnonStorey34C
    {
        internal ScenarioDbId nextMissionId;

        internal bool <>m__1AE(HeroCoin coin)
        {
            return (coin.GetMissionId() == this.nextMissionId);
        }
    }

    private class LessonAsset
    {
        public string m_asset;
        public string m_phoneAsset;
    }
}

