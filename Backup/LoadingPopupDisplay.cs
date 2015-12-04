using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoadingPopupDisplay : TransitionPopup
{
    public static readonly Vector3 END_POS = new Vector3(-0.0152f, 0.0368f, 0.0226f);
    private const float LOWER_TIME = 0.25f;
    private bool m_animationStopped;
    private bool m_barAnimating;
    public List<LoadingbarTexture> m_barTextures = new List<LoadingbarTexture>();
    public GameObject m_cancelButtonParent;
    public GameObject m_loadingTile;
    private AudioSource m_loopSound;
    public ProgressBar m_progressBar;
    private List<string> m_spectatorTaskNameMap = new List<string>();
    private bool m_stopAnimating;
    private Map<AdventureDbId, List<string>> m_taskNameMap = new Map<AdventureDbId, List<string>>();
    public UberText m_tipOfTheDay;
    public static readonly Vector3 MID_POS = new Vector3(-0.0152f, -0.0894f, 0.0226f);
    public static readonly Vector3 OFFSCREEN_POS = new Vector3(-0.0152f, -0.0894f, 0.13f);
    private const float RAISE_TIME = 0.5f;
    private const float ROTATION_DELAY = 0.5f;
    private const float ROTATION_DURATION = 0.5f;
    private const float SHOW_CANCEL_BUTTON_THRESHOLD = 30f;
    private const string SHOW_CANCEL_BUTTON_TWEEN_NAME = "ShowCancelButton";
    private const float SLIDE_IN_TIME = 0.5f;
    private const float SLIDE_OUT_TIME = 0.25f;
    public static readonly Vector3 START_POS = new Vector3(-0.0152f, -0.0894f, -0.0837f);
    private const int TASK_DURATION_VARIATION = 2;

    [DebuggerHidden]
    private IEnumerator AnimateBar()
    {
        return new <AnimateBar>c__IteratorD3 { <>f__this = this };
    }

    protected override void AnimateHide()
    {
        SoundManager.Get().LoadAndPlay("StartGame_window_shrink_down");
        iTween.StopByName(base.gameObject, "ShowCancelButton");
        if (this.m_barAnimating)
        {
            base.StopCoroutine("AnimateBar");
            this.m_barAnimating = false;
            this.StopLoopingSound();
        }
        base.AnimateHide();
    }

    private void AnimateInLoadingTile()
    {
        if (this.m_stopAnimating)
        {
            this.m_animationStopped = true;
        }
        else
        {
            this.m_loadingTile.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            this.m_loadingTile.transform.localPosition = START_POS;
            this.m_progressBar.SetProgressBar(0f);
            object[] args = new object[] { "position", MID_POS, "isLocal", true, "time", 0.5f, "easetype", iTween.EaseType.easeOutBounce };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(this.m_loadingTile, hashtable);
            SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_move_down_and_forward");
            object[] objArray2 = new object[] { "position", END_POS, "isLocal", true, "time", 0.5f, "delay", 0.5f, "easetype", iTween.EaseType.easeOutCubic };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.MoveTo(this.m_loadingTile, hashtable2);
            object[] objArray3 = new object[] { "amount", new Vector3(180f, 0f, 0f), "time", 0.5f, "delay", 0.8f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self, "name", "flip" };
            Hashtable hashtable3 = iTween.Hash(objArray3);
            iTween.RotateAdd(this.m_loadingTile, hashtable3);
            this.m_progressBar.SetLabel(this.GetRandomTaskName());
            base.StartCoroutine("AnimateBar");
        }
    }

    private void AnimateOutLoadingTile()
    {
        object[] args = new object[] { "position", MID_POS, "isLocal", true, "time", 0.25f, "easetype", iTween.EaseType.easeOutBounce };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_loadingTile, hashtable);
        SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_drop");
        object[] objArray2 = new object[] { "position", OFFSCREEN_POS, "isLocal", true, "time", 0.25f, "delay", 0.25f, "easetype", iTween.EaseType.easeOutCubic, "oncomplete", "AnimateInLoadingTile", "oncompletetarget", base.gameObject };
        Hashtable hashtable2 = iTween.Hash(objArray2);
        iTween.MoveTo(this.m_loadingTile, hashtable2);
    }

    protected override void AnimateShow()
    {
        object[] args = new object[] { "name", "ShowCancelButton", "time", 30f, "ignoretimescale", true, "oncomplete", new Action<object>(this.OnCancelButtonShowTimerCompleted), "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Timer(base.gameObject, hashtable);
        this.SetTipOfTheDay();
        this.SetLoadingBarTexture();
        SoundManager.Get().LoadAndPlay("StartGame_window_expand_up");
        base.AnimateShow();
        this.m_stopAnimating = false;
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    protected override void Awake()
    {
        base.Awake();
        this.GenerateTaskNameMap();
        base.m_title.Text = GameStrings.Get("GLUE_STARTING_GAME");
        base.gameObject.transform.localPosition = new Vector3(-0.05f, 8.2f, 3.908f);
        SoundManager.Get().Load("StartGame_window_expand_up");
        SoundManager.Get().Load("StartGame_window_shrink_down");
        SoundManager.Get().Load("StartGame_window_loading_bar_move_down_and_forward");
        SoundManager.Get().Load("StartGame_window_loading_bar_flip");
        SoundManager.Get().Load("StartGame_window_bar_filling_loop");
        SoundManager.Get().Load("StartGame_window_loading_bar_drop");
        this.DisableCancelButton();
    }

    protected override void DisableCancelButton()
    {
        base.DisableCancelButton();
        this.m_cancelButtonParent.SetActive(false);
    }

    protected override void EnableCancelButton()
    {
        this.m_cancelButtonParent.SetActive(true);
        base.EnableCancelButton();
    }

    protected override bool EnableCancelButtonIfPossible()
    {
        if (!base.EnableCancelButtonIfPossible())
        {
            return false;
        }
        TransformUtil.SetLocalPosX(base.m_queueTab, -0.3234057f);
        return true;
    }

    private void GenerateTaskNameMap()
    {
        this.GenerateTaskNamesForAdventure(AdventureDbId.INVALID, "GLUE_LOADING_BAR_TASK_");
        this.GenerateTaskNamesForAdventure(AdventureDbId.NAXXRAMAS, "GLUE_NAXX_LOADING_BAR_TASK_");
        this.GenerateTaskNamesForAdventure(AdventureDbId.BRM, "GLUE_BRM_LOADING_BAR_TASK_");
        this.GenerateTaskNamesForAdventure(AdventureDbId.LOE, "GLUE_LOE_LOADING_BAR_TASK_");
        this.GenerateTaskNamesForPrefix(this.m_spectatorTaskNameMap, "GLUE_SPECTATOR_LOADING_BAR_TASK_");
    }

    private void GenerateTaskNamesForAdventure(AdventureDbId adventureId, string prefix)
    {
        List<string> taskNames = new List<string>();
        this.GenerateTaskNamesForPrefix(taskNames, prefix);
        this.m_taskNameMap[adventureId] = taskNames;
    }

    private void GenerateTaskNamesForPrefix(List<string> taskNames, string prefix)
    {
        taskNames.Clear();
        for (int i = 1; i < 100; i++)
        {
            string key = prefix + i;
            string item = GameStrings.Get(key);
            if (item == key)
            {
                break;
            }
            taskNames.Add(item);
        }
    }

    private float GetRandomTaskDuration()
    {
        return (1f + (UnityEngine.Random.value * 2f));
    }

    private string GetRandomTaskName()
    {
        List<string> spectatorTaskNameMap;
        if (GameMgr.Get().IsSpectator())
        {
            spectatorTaskNameMap = this.m_spectatorTaskNameMap;
        }
        else if (!this.m_taskNameMap.TryGetValue(base.m_adventureId, out spectatorTaskNameMap))
        {
            spectatorTaskNameMap = this.m_taskNameMap[AdventureDbId.INVALID];
        }
        if (spectatorTaskNameMap.Count == 0)
        {
            return "ERROR - OUT OF TASK NAMES!!!";
        }
        int num = UnityEngine.Random.Range(0, spectatorTaskNameMap.Count);
        return spectatorTaskNameMap[num];
    }

    private void LoopingSoundLoadedCallback(AudioSource source, object userData)
    {
        this.StopLoopingSound();
        if (this.m_barAnimating)
        {
            this.m_loopSound = source;
        }
        else
        {
            SoundManager.Get().Stop(source);
        }
    }

    protected override void OnAnimateShowFinished()
    {
        base.OnAnimateShowFinished();
        this.AnimateInLoadingTile();
    }

    protected override void OnCancelButtonReleased(UIEvent e)
    {
        base.OnCancelButtonReleased(e);
        Navigation.GoBack();
    }

    protected void OnCancelButtonShowTimerCompleted(object userData)
    {
        this.EnableCancelButtonIfPossible();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (SoundManager.Get() != null)
        {
            this.StopLoopingSound();
        }
    }

    protected override void OnGameCanceled(FindGameEventData eventData)
    {
        Navigation.PopUnique(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    protected override void OnGameError(FindGameEventData eventData)
    {
        Navigation.PopUnique(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    protected override void OnGameplaySceneLoaded()
    {
        base.StartCoroutine(this.StopLoading());
        Navigation.Clear();
    }

    private bool OnNavigateBack()
    {
        if (!this.m_cancelButtonParent.gameObject.activeSelf)
        {
            return false;
        }
        base.StartCoroutine(this.StopLoading());
        base.FireMatchCanceledEvent();
        return true;
    }

    private void SetLoadingBarTexture()
    {
        Texture texture = this.m_barTextures[0].texture;
        foreach (LoadingbarTexture texture2 in this.m_barTextures)
        {
            if (texture2.adventureID == base.m_adventureId)
            {
                texture = texture2.texture;
                this.m_progressBar.m_barIntensity = texture2.m_barIntensity;
                this.m_progressBar.m_barIntensityIncreaseMax = texture2.m_barIntensityIncreaseMax;
            }
        }
        this.m_progressBar.SetBarTexture(texture);
    }

    private void SetTipOfTheDay()
    {
        switch (base.m_adventureId)
        {
            case AdventureDbId.PRACTICE:
                this.m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.PRACTICE, Options.Get().GetInt(Option.TIP_PRACTICE_PROGRESS, 0), TipCategory.DEFAULT);
                break;

            case AdventureDbId.NAXXRAMAS:
            case AdventureDbId.BRM:
                this.m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.ADVENTURE);
                break;

            default:
                this.m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.DEFAULT);
                break;
        }
    }

    [DebuggerHidden]
    private IEnumerator StopLoading()
    {
        return new <StopLoading>c__IteratorD4 { <>f__this = this };
    }

    private void StopLoopingSound()
    {
        SoundManager.Get().Stop(this.m_loopSound);
        this.m_loopSound = null;
    }

    [CompilerGenerated]
    private sealed class <AnimateBar>c__IteratorD3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal LoadingPopupDisplay <>f__this;
        internal float <timeToAnimate>__0;

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
                    this.<>f__this.m_barAnimating = true;
                    this.$current = new WaitForSeconds(0.8f);
                    this.$PC = 1;
                    goto Label_012B;

                case 1:
                    SoundManager.Get().LoadAndPlay("StartGame_window_loading_bar_flip");
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 2;
                    goto Label_012B;

                case 2:
                    this.<timeToAnimate>__0 = this.<>f__this.GetRandomTaskDuration();
                    this.<>f__this.m_progressBar.m_increaseAnimTime = this.<timeToAnimate>__0;
                    this.<>f__this.m_progressBar.AnimateProgress(0f, 1f);
                    SoundManager.Get().LoadAndPlay("StartGame_window_bar_filling_loop", null, 1f, new SoundManager.LoadedCallback(this.<>f__this.LoopingSoundLoadedCallback));
                    this.$current = new WaitForSeconds(this.<timeToAnimate>__0);
                    this.$PC = 3;
                    goto Label_012B;

                case 3:
                    this.<>f__this.StopLoopingSound();
                    this.<>f__this.AnimateOutLoadingTile();
                    this.<>f__this.m_barAnimating = false;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_012B:
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
    private sealed class <StopLoading>c__IteratorD4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal LoadingPopupDisplay <>f__this;
        internal int <practiceProgress>__0;

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
                    this.<>f__this.m_stopAnimating = true;
                    break;

                case 1:
                    break;

                default:
                    goto Label_009F;
            }
            if (!this.<>f__this.m_animationStopped)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (this.<>f__this.m_adventureId == AdventureDbId.PRACTICE)
            {
                this.<practiceProgress>__0 = Options.Get().GetInt(Option.TIP_PRACTICE_PROGRESS, 0);
                Options.Get().SetInt(Option.TIP_PRACTICE_PROGRESS, this.<practiceProgress>__0 + 1);
            }
            this.<>f__this.AnimateHide();
            this.$PC = -1;
        Label_009F:
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
    public class LoadingbarTexture
    {
        public AdventureDbId adventureID;
        public float m_barIntensity = 1.2f;
        public float m_barIntensityIncreaseMax = 3f;
        public Texture texture;
    }
}

