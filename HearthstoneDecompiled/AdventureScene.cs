using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureScene : Scene
{
    [CompilerGenerated]
    private static Comparison<AdventureDef> <>f__am$cache18;
    private Map<AdventureDbId, AdventureDef> m_adventureDefs = new Map<AdventureDbId, AdventureDef>();
    [CustomEditField(Sections="Music Settings")]
    public List<AdventureModeMusic> m_AdventureModeMusic = new List<AdventureModeMusic>();
    private GameObject m_CurrentSubScene;
    [CustomEditField(Sections="Transition Motions")]
    public float m_DefaultTransitionAnimationTime = 1f;
    private bool m_MusicStopped;
    private bool m_ReverseTransition;
    [CustomEditField(Sections="Transition Sounds", T=EditType.SOUND_PREFAB)]
    public string m_SlideInSound;
    [CustomEditField(Sections="Transition Sounds", T=EditType.SOUND_PREFAB)]
    public string m_SlideOutSound;
    private int m_StartupAssetLoads;
    [CustomEditField(Sections="Adventure Subscene Prefabs")]
    public List<AdventureSubSceneDef> m_SubSceneDefs = new List<AdventureSubSceneDef>();
    [CustomEditField(Sections="Transition Motions")]
    public Vector3 m_SubScenePosition = Vector3.zero;
    private int m_SubScenesLoaded;
    [CustomEditField(Sections="Transition Blocker")]
    public GameObject m_transitionClickBlocker;
    [CustomEditField(Sections="Transition Motions")]
    public TransitionDirection m_TransitionDirection;
    [CustomEditField(Sections="Transition Motions")]
    public iTween.EaseType m_TransitionEaseType = iTween.EaseType.easeInOutSine;
    private GameObject m_TransitionOutSubScene;
    private bool m_Unloading;
    private Map<WingDbId, AdventureWingDef> m_wingDefs = new Map<WingDbId, AdventureWingDef>();
    public static readonly Vector3 REWARD_LOCAL_POS = new Vector3(0.1438589f, 31.27692f, 12.97332f);
    public static PlatformDependentValue<Vector3> REWARD_PUNCH_SCALE;
    public static PlatformDependentValue<Vector3> REWARD_SCALE;
    private static AdventureScene s_instance;
    private const AdventureSubScenes s_StartMode = AdventureSubScenes.Chooser;

    static AdventureScene()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(10f, 10f, 10f),
            Phone = new Vector3(7f, 7f, 7f)
        };
        REWARD_SCALE = value2;
        value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(10.2f, 10.2f, 10.2f),
            Phone = new Vector3(7.1f, 7.1f, 7.1f)
        };
        REWARD_PUNCH_SCALE = value2;
    }

    protected override void Awake()
    {
        base.Awake();
        s_instance = this;
        this.m_CurrentSubScene = null;
        this.m_TransitionOutSubScene = null;
        AdventureConfig config = AdventureConfig.Get();
        config.OnAdventureSceneAwake();
        config.AddSubSceneChangeListener(new AdventureConfig.SubSceneChange(this.OnSubSceneChange));
        config.AddSelectedModeChangeListener(new AdventureConfig.SelectedModeChange(this.OnSelectedModeChanged));
        config.AddAdventureModeChangeListener(new AdventureConfig.AdventureModeChange(this.OnAdventureModeChanged));
        this.m_StartupAssetLoads++;
        bool @bool = Options.Get().GetBool(Option.HAS_SEEN_NAXX);
        if (!@bool)
        {
            this.m_StartupAssetLoads++;
        }
        this.LoadSubScene(config.GetCurrentSubScene(), new AssetLoader.GameObjectCallback(this.OnFirstSubSceneLoaded));
        if (!@bool)
        {
            SoundManager.Get().Load("VO_KT_INTRO_39");
            AssetLoader.Get().LoadGameObject("KT_Quote", new AssetLoader.GameObjectCallback(this.OnKTQuoteLoaded), null, false);
        }
        Options.Get().SetBool(Option.BUNDLE_JUST_PURCHASE_IN_HUB, false);
        if (ApplicationMgr.IsInternal())
        {
            CheatMgr.Get().RegisterCheatHandler("advdev", new CheatMgr.ProcessCheatCallback(this.OnDevCheat), null, null, null);
        }
        this.InitializeAllDefs();
    }

    private void CompleteTransition()
    {
        AdventureSubScene component = this.m_CurrentSubScene.GetComponent<AdventureSubScene>();
        if (component != null)
        {
            component.NotifyTransitionComplete();
            this.UpdateAdventureModeMusic();
        }
        this.EnableTransitionBlocker(false);
    }

    private void DestroyTransitioningSubScene(GameObject destroysubscene)
    {
        if (destroysubscene != null)
        {
            UnityEngine.Object.DestroyObject(destroysubscene);
        }
    }

    private void DoSubSceneTransition(AdventureSubScene subscene)
    {
        <DoSubSceneTransition>c__AnonStorey2A5 storeya = new <DoSubSceneTransition>c__AnonStorey2A5 {
            <>f__this = this
        };
        this.m_CurrentSubScene.transform.localPosition = this.m_SubScenePosition;
        if (this.m_TransitionOutSubScene == null)
        {
            this.CompleteTransition();
        }
        else
        {
            float num = (subscene != null) ? subscene.m_TransitionAnimationTime : this.m_DefaultTransitionAnimationTime;
            Vector3 moveDirection = this.GetMoveDirection();
            storeya.delobj = this.m_TransitionOutSubScene;
            if (this.m_ReverseTransition)
            {
                AdventureSubScene component = this.m_TransitionOutSubScene.GetComponent<AdventureSubScene>();
                Vector3 vector2 = (component != null) ? ((Vector3) component.m_SubSceneBounds) : TransformUtil.GetBoundsOfChildren(this.m_TransitionOutSubScene).size;
                Vector3 localPosition = this.m_TransitionOutSubScene.transform.localPosition;
                localPosition.x -= vector2.x * moveDirection.x;
                localPosition.y -= vector2.y * moveDirection.y;
                localPosition.z -= vector2.z * moveDirection.z;
                object[] args = new object[] { "islocal", true, "position", localPosition, "time", num, "easeType", this.m_TransitionEaseType, "oncomplete", new Action<object>(storeya.<>m__33), "oncompletetarget", base.gameObject };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(this.m_TransitionOutSubScene, hashtable);
                if (!string.IsNullOrEmpty(this.m_SlideOutSound))
                {
                    SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_SlideOutSound));
                }
                this.CompleteTransition();
            }
            else
            {
                AdventureSubScene scene2 = this.m_CurrentSubScene.GetComponent<AdventureSubScene>();
                Vector3 vector4 = (scene2 != null) ? ((Vector3) scene2.m_SubSceneBounds) : TransformUtil.GetBoundsOfChildren(this.m_CurrentSubScene).size;
                Vector3 vector5 = this.m_CurrentSubScene.transform.localPosition;
                Vector3 vector6 = this.m_CurrentSubScene.transform.localPosition;
                vector6.x -= vector4.x * moveDirection.x;
                vector6.y -= vector4.y * moveDirection.y;
                vector6.z -= vector4.z * moveDirection.z;
                this.m_CurrentSubScene.transform.localPosition = vector6;
                object[] objArray2 = new object[] { "islocal", true, "position", vector5, "time", num, "easeType", this.m_TransitionEaseType, "oncomplete", new Action<object>(storeya.<>m__34), "oncompletetarget", base.gameObject };
                Hashtable hashtable2 = iTween.Hash(objArray2);
                iTween.MoveTo(this.m_CurrentSubScene, hashtable2);
                if (!string.IsNullOrEmpty(this.m_SlideInSound))
                {
                    SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_SlideInSound));
                }
            }
            this.m_TransitionOutSubScene = null;
        }
    }

    private void EnableTransitionBlocker(bool block)
    {
        if (this.m_transitionClickBlocker != null)
        {
            this.m_transitionClickBlocker.SetActive(block);
        }
    }

    public static AdventureScene Get()
    {
        return s_instance;
    }

    public AdventureDef GetAdventureDef(AdventureDbId advId)
    {
        AdventureDef def = null;
        this.m_adventureDefs.TryGetValue(advId, out def);
        return def;
    }

    private Vector3 GetMoveDirection()
    {
        float num = 1f;
        if (this.m_TransitionDirection >= TransitionDirection.NX)
        {
            num *= -1f;
        }
        Vector3 zero = Vector3.zero;
        zero[(int) (this.m_TransitionDirection % TransitionDirection.NX)] = num;
        return zero;
    }

    public List<AdventureDef> GetSortedAdventureDefs()
    {
        List<AdventureDef> list = new List<AdventureDef>(this.m_adventureDefs.Values);
        if (<>f__am$cache18 == null)
        {
            <>f__am$cache18 = (l, r) => r.GetSortOrder() - l.GetSortOrder();
        }
        list.Sort(<>f__am$cache18);
        return list;
    }

    public AdventureWingDef GetWingDef(WingDbId wingId)
    {
        AdventureWingDef def = null;
        this.m_wingDefs.TryGetValue(wingId, out def);
        return def;
    }

    public List<AdventureWingDef> GetWingDefsFromAdventure(AdventureDbId advId)
    {
        List<AdventureWingDef> list = new List<AdventureWingDef>();
        foreach (KeyValuePair<WingDbId, AdventureWingDef> pair in this.m_wingDefs)
        {
            if (pair.Value.GetAdventureId() == advId)
            {
                list.Add(pair.Value);
            }
        }
        return list;
    }

    private void InitializeAllDefs()
    {
        List<DbfRecord> adventureRecordsWithDefPrefab = GameUtils.GetAdventureRecordsWithDefPrefab();
        List<DbfRecord> adventureDataRecordsWithSubDefPrefab = GameUtils.GetAdventureDataRecordsWithSubDefPrefab();
        foreach (DbfRecord record in adventureRecordsWithDefPrefab)
        {
            AdventureDef def = GameUtils.LoadDbfGameObjectWithComponent<AdventureDef>(record, "ADVENTURE_DEF_PREFAB");
            if (def != null)
            {
                def.Init(record, adventureDataRecordsWithSubDefPrefab);
                this.m_adventureDefs.Add(def.GetAdventureId(), def);
            }
        }
        foreach (DbfRecord record2 in GameDbf.Wing.GetRecords())
        {
            AdventureWingDef def2 = GameUtils.LoadDbfGameObjectWithComponent<AdventureWingDef>(record2, "ADVENTURE_WING_DEF_PREFAB");
            if (def2 != null)
            {
                def2.Init(record2);
                this.m_wingDefs.Add(def2.GetWingId(), def2);
            }
        }
    }

    public bool IsAdventureOpen(AdventureDbId advId)
    {
        bool flag = true;
        foreach (KeyValuePair<WingDbId, AdventureWingDef> pair in this.m_wingDefs)
        {
            if (pair.Value.GetAdventureId() == advId)
            {
                if (AdventureProgressMgr.Get().IsWingOpen((int) pair.Value.GetWingId()))
                {
                    return true;
                }
                flag = false;
            }
        }
        return flag;
    }

    public bool IsInitialScreen()
    {
        return (this.m_SubScenesLoaded <= 1);
    }

    public override bool IsUnloading()
    {
        return this.m_Unloading;
    }

    private void LoadSubScene(AdventureSubScenes subscene)
    {
        this.LoadSubScene(subscene, new AssetLoader.GameObjectCallback(this.OnSubSceneLoaded));
    }

    private void LoadSubScene(AdventureSubScenes subscene, AssetLoader.GameObjectCallback callback)
    {
        <LoadSubScene>c__AnonStorey2A4 storeya = new <LoadSubScene>c__AnonStorey2A4 {
            subscene = subscene,
            <>f__this = this
        };
        AdventureSubSceneDef def = this.m_SubSceneDefs.Find(new Predicate<AdventureSubSceneDef>(storeya.<>m__31));
        if (def == null)
        {
            UnityEngine.Debug.LogError(string.Format("Subscene {0} prefab not defined in m_SubSceneDefs", storeya.subscene));
        }
        else
        {
            this.EnableTransitionBlocker(true);
            storeya.runCallback = callback;
            AssetLoader.Get().LoadUIScreen(FileUtils.GameAssetPathToName((string) def.m_Prefab), new AssetLoader.GameObjectCallback(storeya.<>m__32), null, false);
        }
    }

    private void OnAdventureModeChanged(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        if (modeId == AdventureModeDbId.HEROIC)
        {
            this.ShowHeroicWarning();
        }
        if ((adventureId == AdventureDbId.NAXXRAMAS) && !Options.Get().GetBool(Option.HAS_ENTERED_NAXX))
        {
            NotificationManager.Get().CreateKTQuote("VO_KT_INTRO2_40", "VO_KT_INTRO2_40", true);
            Options.Get().SetBool(Option.HAS_ENTERED_NAXX, true);
        }
        this.UpdateAdventureModeMusic();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private bool OnDevCheat(string func, string[] args, string rawArgs)
    {
        if (ApplicationMgr.IsInternal())
        {
            this.IsDevMode = true;
            if (args.Length > 0)
            {
                int result = 1;
                if (int.TryParse(args[0], out result) && (result > 0))
                {
                    this.IsDevMode = true;
                    this.DevModeSetting = result;
                }
            }
            if (UIStatus.Get() != null)
            {
                UIStatus.Get().AddInfo(string.Format("{0}: IsDevMode={1} DevModeSetting={2}", func, this.IsDevMode, this.DevModeSetting));
            }
        }
        return true;
    }

    private void OnFirstSubSceneLoaded(string name, GameObject screen, object callbackData)
    {
        this.ShowExpertAIUnlockTip();
        this.OnSubSceneLoaded(name, screen, callbackData);
        this.OnStartupAssetLoaded();
    }

    private void OnKTQuoteLoaded(string name, GameObject go, object userData)
    {
        if (go != null)
        {
            UnityEngine.Object.Destroy(go);
        }
        this.OnStartupAssetLoaded();
    }

    private void OnSelectedModeChanged(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        if (AdventureConfig.Get().CanPlayMode(adventureId, modeId))
        {
            if (adventureId == AdventureDbId.NAXXRAMAS)
            {
                if (!Options.Get().GetBool(Option.HAS_SEEN_NAXX))
                {
                    this.OnSelectedModeChanged_CreateKTIntroConversation();
                }
            }
            else if (adventureId == AdventureDbId.BRM)
            {
                if (((Get() != null) && Get().IsDevMode) || !Options.Get().GetBool(Option.HAS_SEEN_BRM))
                {
                    this.OnSelectedModeChanged_CreateIntroConversation(0, InitialConversationLines.BRM_INITIAL_CONVO_LINES, Option.HAS_SEEN_BRM);
                }
            }
            else if ((adventureId == AdventureDbId.LOE) && (((Get() != null) && Get().IsDevMode) || !Options.Get().GetBool(Option.HAS_SEEN_LOE)))
            {
                this.OnSelectedModeChanged_CreateIntroConversation(0, InitialConversationLines.LOE_INITIAL_CONVO_LINES, Option.HAS_SEEN_LOE);
            }
        }
        this.UpdateAdventureModeMusic();
    }

    private void OnSelectedModeChanged_CreateIntroConversation(int index, string[][] convoLines, Option hasSeen)
    {
        <OnSelectedModeChanged_CreateIntroConversation>c__AnonStorey2A6 storeya = new <OnSelectedModeChanged_CreateIntroConversation>c__AnonStorey2A6 {
            index = index,
            convoLines = convoLines,
            hasSeen = hasSeen,
            <>f__this = this
        };
        System.Action finishCallback = null;
        if (storeya.index < (storeya.convoLines.Length - 1))
        {
            finishCallback = new System.Action(storeya.<>m__35);
        }
        bool flag = (Get() != null) && Get().IsDevMode;
        if ((storeya.index >= (storeya.convoLines.Length - 1)) && !flag)
        {
            Options.Get().SetBool(storeya.hasSeen, true);
        }
        string text = GameStrings.Get(storeya.convoLines[storeya.index][1]);
        bool allowRepeatDuringSession = flag;
        NotificationManager.Get().CreateCharacterQuote(storeya.convoLines[storeya.index][0], NotificationManager.DEFAULT_CHARACTER_POS, text, storeya.convoLines[storeya.index][1], allowRepeatDuringSession, 0f, finishCallback, CanvasAnchor.BOTTOM_LEFT);
    }

    private void OnSelectedModeChanged_CreateKTIntroConversation()
    {
        NotificationManager.Get().CreateKTQuote("VO_KT_INTRO_39", "VO_KT_INTRO_39", true);
        Options.Get().SetBool(Option.HAS_SEEN_NAXX, true);
    }

    private void OnStartupAssetLoaded()
    {
        this.m_StartupAssetLoads--;
        if (this.m_StartupAssetLoads <= 0)
        {
            this.UpdateAdventureModeMusic();
            SceneMgr.Get().NotifySceneLoaded();
        }
    }

    private void OnSubSceneChange(AdventureSubScenes newscene, bool forward)
    {
        this.m_ReverseTransition = !forward;
        this.LoadSubScene(newscene);
    }

    private void OnSubSceneLoaded(string name, GameObject screen, object callbackData)
    {
        this.m_TransitionOutSubScene = this.m_CurrentSubScene;
        this.m_CurrentSubScene = screen;
        this.m_CurrentSubScene.transform.position = new Vector3(-500f, 0f, 0f);
        Vector3 localScale = this.m_CurrentSubScene.transform.localScale;
        this.m_CurrentSubScene.transform.parent = base.transform;
        this.m_CurrentSubScene.transform.localScale = localScale;
        AdventureSubScene component = this.m_CurrentSubScene.GetComponent<AdventureSubScene>();
        this.m_SubScenesLoaded++;
        if (component == null)
        {
            this.DoSubSceneTransition(component);
        }
        else
        {
            base.StartCoroutine(this.WaitForSubSceneToLoad());
        }
    }

    private void ShowExpertAIUnlockTip()
    {
        if ((AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO, false).Count <= 0) && !Options.Get().GetBool(Option.HAS_SEEN_EXPERT_AI_UNLOCK, false))
        {
            NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_EXPERT_AI_10"), "VO_INNKEEPER_EXPERT_AI_10", 0f, null);
            Options.Get().SetBool(Option.HAS_SEEN_EXPERT_AI_UNLOCK, true);
        }
    }

    private void ShowHeroicWarning()
    {
        if (!Options.Get().GetBool(Option.HAS_SEEN_HEROIC_WARNING))
        {
            Options.Get().SetBool(Option.HAS_SEEN_HEROIC_WARNING, true);
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_HEROIC_WARNING_TITLE"),
                m_text = GameStrings.Get("GLUE_HEROIC_WARNING"),
                m_showAlertIcon = true,
                m_responseDisplay = AlertPopup.ResponseDisplay.OK
            };
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void Start()
    {
        AdventureConfig.Get().UpdatePresence();
    }

    public override void Unload()
    {
        this.m_Unloading = true;
        AdventureConfig config = AdventureConfig.Get();
        config.ClearBossDefs();
        DeckPickerTray.Get().Unload();
        config.RemoveAdventureModeChangeListener(new AdventureConfig.AdventureModeChange(this.OnAdventureModeChanged));
        config.RemoveSelectedModeChangeListener(new AdventureConfig.SelectedModeChange(this.OnSelectedModeChanged));
        config.RemoveSubSceneChangeListener(new AdventureConfig.SubSceneChange(this.OnSubSceneChange));
        config.OnAdventureSceneUnload();
        CheatMgr.Get().UnregisterCheatHandler("advdev", new CheatMgr.ProcessCheatCallback(this.OnDevCheat));
        this.m_Unloading = false;
    }

    private void Update()
    {
        Network.Get().ProcessNetwork();
    }

    private void UpdateAdventureModeMusic()
    {
        AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
        AdventureSubScenes currentSubScene = AdventureConfig.Get().GetCurrentSubScene();
        AdventureModeMusic music = null;
        foreach (AdventureModeMusic music2 in this.m_AdventureModeMusic)
        {
            if ((music2.m_subsceneId == currentSubScene) && (music2.m_adventureId == selectedAdventure))
            {
                music = music2;
                break;
            }
            if ((music2.m_subsceneId == currentSubScene) && (music2.m_adventureId == AdventureDbId.INVALID))
            {
                music = music2;
            }
        }
        if (music != null)
        {
            MusicManager.Get().StartPlaylist(music.m_playlist);
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForSubSceneToLoad()
    {
        return new <WaitForSubSceneToLoad>c__IteratorC { <>f__this = this };
    }

    public int DevModeSetting { get; set; }

    public bool IsDevMode { get; set; }

    [CompilerGenerated]
    private sealed class <DoSubSceneTransition>c__AnonStorey2A5
    {
        internal AdventureScene <>f__this;
        internal GameObject delobj;

        internal void <>m__33(object e)
        {
            this.<>f__this.DestroyTransitioningSubScene(this.delobj);
        }

        internal void <>m__34(object e)
        {
            this.<>f__this.DestroyTransitioningSubScene(this.delobj);
            this.<>f__this.CompleteTransition();
        }
    }

    [CompilerGenerated]
    private sealed class <LoadSubScene>c__AnonStorey2A4
    {
        internal AdventureScene <>f__this;
        internal AssetLoader.GameObjectCallback runCallback;
        internal AdventureSubScenes subscene;

        internal bool <>m__31(AdventureScene.AdventureSubSceneDef item)
        {
            return (item.m_SubScene == this.subscene);
        }

        internal void <>m__32(string name, GameObject go, object data)
        {
            if (this.runCallback != null)
            {
                this.runCallback(name, go, data);
            }
            this.<>f__this.UpdateAdventureModeMusic();
        }
    }

    [CompilerGenerated]
    private sealed class <OnSelectedModeChanged_CreateIntroConversation>c__AnonStorey2A6
    {
        internal AdventureScene <>f__this;
        internal string[][] convoLines;
        internal Option hasSeen;
        internal int index;

        internal void <>m__35()
        {
            if ((SceneMgr.Get() != null) && (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE))
            {
                this.<>f__this.OnSelectedModeChanged_CreateIntroConversation(this.index + 1, this.convoLines, this.hasSeen);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitForSubSceneToLoad>c__IteratorC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureScene <>f__this;
        internal AdventureSubScene <subscene>__0;

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
                    this.<subscene>__0 = this.<>f__this.m_CurrentSubScene.GetComponent<AdventureSubScene>();
                    break;

                case 1:
                    break;

                default:
                    goto Label_0077;
            }
            if (!this.<subscene>__0.IsLoaded())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.DoSubSceneTransition(this.<subscene>__0);
            this.$PC = -1;
        Label_0077:
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

    [Serializable]
    public class AdventureModeMusic
    {
        public AdventureDbId m_adventureId;
        public MusicPlaylistType m_playlist;
        public AdventureSubScenes m_subsceneId;
    }

    [Serializable]
    public class AdventureSubSceneDef
    {
        [CustomEditField(T=EditType.GAME_OBJECT)]
        public String_MobileOverride m_Prefab;
        [CustomEditField(ListSortable=true)]
        public AdventureSubScenes m_SubScene;
    }

    public enum TransitionDirection
    {
        X,
        Y,
        Z,
        NX,
        NY,
        NZ
    }
}

