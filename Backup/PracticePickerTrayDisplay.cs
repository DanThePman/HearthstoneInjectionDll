using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class PracticePickerTrayDisplay : MonoBehaviour
{
    [SerializeField]
    private float m_AIButtonHeight = 5f;
    [CustomEditField(Sections="AI Button Settings")]
    public PracticeAIButton m_AIButtonPrefab;
    [CustomEditField(Sections="AI Button Settings")]
    public GameObject m_AIButtonsContainer;
    [CustomEditField(Sections="UI")]
    public StandardPegButtonNew m_backButton;
    private bool m_buttonsCreated;
    private bool m_buttonsReady;
    private Map<string, FullDef> m_heroDefs = new Map<string, FullDef>();
    private int m_heroDefsToLoad;
    private bool m_heroesLoaded;
    private List<Achievement> m_lockedHeroes;
    [CustomEditField(Sections="UI")]
    public PlayButton m_playButton;
    private List<PracticeAIButton> m_practiceAIButtons = new List<PracticeAIButton>();
    private PracticeAIButton m_selectedPracticeAIButton;
    private bool m_shown;
    private List<DbfRecord> m_sortedMissionRecords = new List<DbfRecord>();
    [CustomEditField(Sections="Animation Settings")]
    public float m_trayAnimationTime = 0.5f;
    [CustomEditField(Sections="Animation Settings")]
    public iTween.EaseType m_trayInEaseType = iTween.EaseType.easeOutBounce;
    [CustomEditField(Sections="UI")]
    public UberText m_trayLabel;
    private List<TrayLoaded> m_TrayLoadedListeners = new List<TrayLoaded>();
    [CustomEditField(Sections="Animation Settings")]
    public iTween.EaseType m_trayOutEaseType = iTween.EaseType.easeOutCubic;
    private static readonly int NUM_AI_BUTTONS_TO_SHOW = 14;
    private const float PRACTICE_TRAY_MATERIAL_Y_OFFSET = -0.045f;
    private static PracticePickerTrayDisplay s_instance;

    public void AddTrayLoadedListener(TrayLoaded dlg)
    {
        this.m_TrayLoadedListeners.Add(dlg);
    }

    private void AIButtonPressed(UIEvent e)
    {
        PracticeAIButton element = (PracticeAIButton) e.GetElement();
        this.SetSelectedButton(element);
        this.m_playButton.Enable();
        element.Select();
    }

    private void Awake()
    {
        s_instance = this;
        this.InitMissionRecords();
        foreach (Transform transform in base.gameObject.GetComponents<Transform>())
        {
            transform.gameObject.SetActive(false);
        }
        base.gameObject.SetActive(true);
        if (this.m_backButton != null)
        {
            this.m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
            this.m_backButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.BackButtonReleased));
        }
        this.m_trayLabel.Text = GameStrings.Get("GLUE_CHOOSE_OPPONENT");
        this.m_playButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.PlayGameButtonRelease));
        this.m_heroDefsToLoad = this.m_sortedMissionRecords.Count;
        foreach (DbfRecord record in this.m_sortedMissionRecords)
        {
            string missionHeroCardId = GameUtils.GetMissionHeroCardId(record.GetId());
            DefLoader.Get().LoadFullDef(missionHeroCardId, new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
        }
        SoundManager.Get().Load("choose_opponent_panel_slide_on");
        SoundManager.Get().Load("choose_opponent_panel_slide_off");
        this.SetupHeroAchieves();
        base.StartCoroutine(this.NotifyWhenTrayLoaded());
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    private void BackButtonReleased(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void DisableAIButtons()
    {
        for (int i = 0; i < this.m_practiceAIButtons.Count; i++)
        {
            this.m_practiceAIButtons[i].SetEnabled(false);
        }
    }

    [DebuggerHidden]
    private IEnumerator DoPickHeroLines()
    {
        return new <DoPickHeroLines>c__Iterator1B1 { <>f__this = this };
    }

    private void EnableAIButtons()
    {
        for (int i = 0; i < this.m_practiceAIButtons.Count; i++)
        {
            this.m_practiceAIButtons[i].SetEnabled(true);
        }
    }

    private void FireTrayLoadedEvent()
    {
        foreach (TrayLoaded loaded in this.m_TrayLoadedListeners.ToArray())
        {
            loaded();
        }
    }

    public static PracticePickerTrayDisplay Get()
    {
        return s_instance;
    }

    public void Hide()
    {
        this.m_shown = false;
        iTween.Stop(base.gameObject);
        object[] args = new object[] { "position", PracticeDisplay.Get().GetPracticePickerHidePosition(), "isLocal", true, "time", this.m_trayAnimationTime, "easetype", this.m_trayOutEaseType, "oncomplete", e => base.gameObject.SetActive(false), "delay", 0.001f };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(base.gameObject, hashtable);
        SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_off");
    }

    public void Init()
    {
        int num = Mathf.Min(NUM_AI_BUTTONS_TO_SHOW, this.m_sortedMissionRecords.Count);
        for (int i = 0; i < num; i++)
        {
            PracticeAIButton c = (PracticeAIButton) GameUtils.Instantiate(this.m_AIButtonPrefab, this.m_AIButtonsContainer, false);
            SceneUtils.SetLayer(c, this.m_AIButtonsContainer.gameObject.layer);
            this.m_practiceAIButtons.Add(c);
        }
        this.UpdateAIButtonPositions();
        foreach (PracticeAIButton button2 in this.m_practiceAIButtons)
        {
            button2.SetOriginalLocalPosition();
            button2.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.AIButtonPressed));
        }
        this.m_buttonsCreated = true;
    }

    [DebuggerHidden]
    private IEnumerator InitButtonsWhenReady()
    {
        return new <InitButtonsWhenReady>c__Iterator1B2 { <>f__this = this };
    }

    private void InitMissionRecords()
    {
        int num = 2;
        int selectedMode = (int) AdventureConfig.Get().GetSelectedMode();
        foreach (DbfRecord record in GameDbf.Scenario.GetRecords())
        {
            if ((record.GetInt("ADVENTURE_ID") == num) && (record.GetInt("MODE_ID") == selectedMode))
            {
                this.m_sortedMissionRecords.Add(record);
            }
        }
        this.m_sortedMissionRecords.Sort(new Comparison<DbfRecord>(GameUtils.MissionSortComparison));
    }

    public bool IsLoaded()
    {
        return this.m_buttonsReady;
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    [DebuggerHidden]
    private IEnumerator NotifyWhenTrayLoaded()
    {
        return new <NotifyWhenTrayLoaded>c__Iterator1B3 { <>f__this = this };
    }

    private void OnDestroy()
    {
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        s_instance = null;
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        if (eventData.m_state == FindGameState.INVALID)
        {
            this.EnableAIButtons();
        }
        return false;
    }

    private void OnFullDefLoaded(string cardId, FullDef def, object userData)
    {
        this.m_heroDefs[cardId] = def;
        this.m_heroDefsToLoad--;
        if (this.m_heroDefsToLoad <= 0)
        {
            this.m_heroesLoaded = true;
        }
    }

    public void OnGameDenied()
    {
        this.UpdateAIButtons();
    }

    private bool OnNavigateBack()
    {
        this.Hide();
        DeckPickerTrayDisplay.Get().ResetCurrentMode();
        return true;
    }

    private void PlayGameButtonRelease(UIEvent e)
    {
        SceneUtils.SetLayer(PracticeDisplay.Get().gameObject, GameLayer.Default);
        long selectedDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
        if (selectedDeckID == 0)
        {
            UnityEngine.Debug.LogError("Trying to play practice game with deck ID 0!");
        }
        else
        {
            e.GetElement().SetEnabled(false);
            this.DisableAIButtons();
            Network.TrackWhat what = !DeckPickerTrayDisplay.Get().IsShowingCustomDecks() ? Network.TrackWhat.TRACK_PLAY_PRACTICE_WITH_PRECON_DECK : Network.TrackWhat.TRACK_PLAY_PRACTICE_WITH_CUSTOM_DECK;
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, what);
            if ((AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.EXPERT) && !Options.Get().GetBool(Option.HAS_PLAYED_EXPERT_AI, false))
            {
                Options.Get().SetBool(Option.HAS_PLAYED_EXPERT_AI, true);
            }
            GameMgr.Get().FindGame(GameType.GT_VS_AI, this.m_selectedPracticeAIButton.GetMissionID(), selectedDeckID, 0L);
        }
    }

    public void RemoveTrayLoadedListener(TrayLoaded dlg)
    {
        this.m_TrayLoadedListeners.Remove(dlg);
    }

    private void SetSelectedButton(PracticeAIButton button)
    {
        if (this.m_selectedPracticeAIButton != null)
        {
            this.m_selectedPracticeAIButton.Deselect();
        }
        this.m_selectedPracticeAIButton = button;
    }

    private void SetupHeroAchieves()
    {
        this.m_lockedHeroes = AchieveManager.Get().GetAchievesInGroup(Achievement.Group.UNLOCK_HERO, false);
        if ((this.m_lockedHeroes.Count <= 7) && !Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_MODE, false))
        {
            Options.Get().SetBool(Option.HAS_SEEN_PRACTICE_MODE, true);
        }
        base.StartCoroutine(this.InitButtonsWhenReady());
    }

    public void Show()
    {
        this.m_shown = true;
        iTween.Stop(base.gameObject);
        foreach (Transform transform in base.gameObject.GetComponents<Transform>())
        {
            transform.gameObject.SetActive(true);
        }
        base.gameObject.SetActive(true);
        object[] args = new object[] { "position", PracticeDisplay.Get().GetPracticePickerShowPosition(), "isLocal", true, "time", this.m_trayAnimationTime, "easetype", this.m_trayInEaseType, "delay", 0.001f };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(base.gameObject, hashtable);
        SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_on");
        if (!Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_TRAY, false))
        {
            Options.Get().SetBool(Option.HAS_SEEN_PRACTICE_TRAY, true);
            base.StartCoroutine(this.DoPickHeroLines());
        }
        if (this.m_selectedPracticeAIButton != null)
        {
            this.m_playButton.Enable();
        }
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    private void Start()
    {
        this.m_playButton.SetText(GameStrings.Get("GLOBAL_PLAY"));
        this.m_playButton.SetOriginalLocalPosition();
        this.m_playButton.Disable();
    }

    private void UpdateAIButtonPositions()
    {
        int num = 0;
        foreach (PracticeAIButton button in this.m_practiceAIButtons)
        {
            TransformUtil.SetLocalPosZ(button, -this.m_AIButtonHeight * num++);
        }
    }

    private void UpdateAIButtons()
    {
        this.UpdateAIDeckButtons();
        if (this.m_selectedPracticeAIButton == null)
        {
            this.m_playButton.Disable();
        }
        else
        {
            this.m_playButton.Enable();
        }
    }

    private void UpdateAIDeckButtons()
    {
        bool flag = AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.EXPERT;
        for (int i = 0; i < this.m_sortedMissionRecords.Count; i++)
        {
            DbfRecord record = this.m_sortedMissionRecords[i];
            int id = record.GetId();
            string missionHeroCardId = GameUtils.GetMissionHeroCardId(id);
            FullDef def = this.m_heroDefs[missionHeroCardId];
            EntityDef entityDef = def.GetEntityDef();
            CardDef cardDef = def.GetCardDef();
            TAG_CLASS buttonClass = entityDef.GetClass();
            string locString = record.GetLocString("SHORT_NAME");
            PracticeAIButton button = this.m_practiceAIButtons[i];
            button.SetInfo(locString, buttonClass, cardDef, id, false);
            bool shown = false;
            foreach (Achievement achievement in this.m_lockedHeroes)
            {
                if (((TAG_CLASS) achievement.ClassRequirement.Value) == buttonClass)
                {
                    shown = true;
                    break;
                }
            }
            button.ShowQuestBang(shown);
            if (button == this.m_selectedPracticeAIButton)
            {
                button.Select();
            }
            else
            {
                button.Deselect();
            }
        }
        bool @bool = Options.Get().GetBool(Option.HAS_SEEN_EXPERT_AI, false);
        if (flag && !@bool)
        {
            Options.Get().SetBool(Option.HAS_SEEN_EXPERT_AI, true);
            @bool = true;
        }
    }

    [CustomEditField(Sections="AI Button Settings")]
    public float AIButtonHeight
    {
        get
        {
            return this.m_AIButtonHeight;
        }
        set
        {
            this.m_AIButtonHeight = value;
            this.UpdateAIButtonPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <DoPickHeroLines>c__Iterator1B1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PracticePickerTrayDisplay <>f__this;
        internal Notification <firstPart>__0;

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
                    this.<firstPart>__0 = NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_PRACTICE_INST1_07"), "VO_INNKEEPER_UNLOCK_HEROES", 0f, null);
                    break;

                case 1:
                    break;

                case 2:
                    this.$current = new WaitForSeconds(6f);
                    this.$PC = 3;
                    goto Label_0116;

                case 3:
                    if (!this.<>f__this.m_playButton.IsEnabled() && !GameMgr.Get().IsTransitionPopupShown())
                    {
                        NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_INNKEEPER_PRACTICE_INST2_08"), "VO_INNKEEPER_PRACTICE_INST2_08", 2f, null);
                        this.$PC = -1;
                    }
                    goto Label_0114;

                default:
                    goto Label_0114;
            }
            if (this.<firstPart>__0.GetAudio() == null)
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.$current = new WaitForSeconds(this.<firstPart>__0.GetAudio().clip.length);
                this.$PC = 2;
            }
            goto Label_0116;
        Label_0114:
            return false;
        Label_0116:
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
    private sealed class <InitButtonsWhenReady>c__Iterator1B2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PracticePickerTrayDisplay <>f__this;

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
                    if (!this.<>f__this.m_buttonsCreated)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0095;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0093;
            }
            while (!this.<>f__this.m_heroesLoaded)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0095;
            }
            this.<>f__this.UpdateAIButtons();
            this.<>f__this.m_buttonsReady = true;
            this.$PC = -1;
        Label_0093:
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
    private sealed class <NotifyWhenTrayLoaded>c__Iterator1B3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PracticePickerTrayDisplay <>f__this;

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
                    if (!this.<>f__this.m_buttonsReady)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.FireTrayLoadedEvent();
                    Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_BUTTON_PRACTICE);
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

    public delegate void TrayLoaded();
}

