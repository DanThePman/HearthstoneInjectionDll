using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroCoin : PegUIElement
{
    public GameObject m_coin;
    public UberText m_coinLabel;
    private CoinPressCallback m_coinPressCallback;
    public GameObject m_coinX;
    public GameObject m_cracks;
    private UnityEngine.Vector2 m_crackTexture;
    private CoinStatus m_currentStatus;
    public GameObject m_explosionPrefab;
    private UnityEngine.Vector2 m_goldTexture;
    private Material m_grayMaterial;
    private UnityEngine.Vector2 m_grayTexture;
    public HighlightState m_highlight;
    public bool m_inputEnabled;
    public GameObject m_leftCap;
    private string m_lessonAssetName;
    private UnityEngine.Vector2 m_lessonCoords;
    private Material m_material;
    public GameObject m_middle;
    private int m_missionID;
    private bool m_nextTutorialStarted;
    private float m_originalMiddleWidth;
    private Vector3 m_originalPosition;
    private Vector3 m_originalXPosition;
    public GameObject m_rightCap;
    public GameObject m_tooltip;
    public UberText m_tooltipText;

    protected override void Awake()
    {
        base.Awake();
        this.m_tooltip.SetActive(false);
    }

    public void EnableInput()
    {
        this.m_inputEnabled = true;
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
    }

    public void FinishIntroScaling()
    {
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnOut));
        this.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPress));
        this.m_originalMiddleWidth = this.m_middle.GetComponent<Renderer>().bounds.size.x;
    }

    public string GetLessonAssetName()
    {
        return this.m_lessonAssetName;
    }

    public int GetMissionId()
    {
        return this.m_missionID;
    }

    public void HideTooltip()
    {
        this.m_tooltip.SetActive(false);
    }

    private void OnDestroy()
    {
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnGameLoaded));
        }
    }

    private void OnGameLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        base.GetComponentInChildren<PlayMakerFSM>().SendEvent("Death");
        SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnGameLoaded));
        base.StartCoroutine(this.WaitThenTransition());
    }

    private void OnOut(UIEvent e)
    {
        this.HideTooltip();
        if (!this.m_nextTutorialStarted && this.m_inputEnabled)
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
    }

    private void OnOver(UIEvent e)
    {
        if (!this.m_nextTutorialStarted)
        {
            this.ShowTooltip();
            if (this.m_inputEnabled)
            {
                this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_MOUSE_OVER);
                SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_mouse_over", base.gameObject);
            }
        }
    }

    private void OnPress(UIEvent e)
    {
        if (this.m_inputEnabled && !this.m_nextTutorialStarted)
        {
            this.m_inputEnabled = false;
            SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_play_select", base.gameObject);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            if (this.m_coinPressCallback != null)
            {
                this.m_coinPressCallback();
            }
            else
            {
                LoadingScreen.Get().AddTransitionBlocker();
                LoadingScreen.Get().AddTransitionObject(base.gameObject);
                SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnGameLoaded));
                this.StartNextTutorial();
                base.GetComponentInChildren<PlayMakerFSM>().SendEvent("Action");
            }
        }
    }

    public void OnUpdateAlphaVal(float val)
    {
        this.m_material.SetColor("_Color", new Color(1f, 1f, 1f, val));
    }

    public void SetCoinInfo(UnityEngine.Vector2 goldTexture, UnityEngine.Vector2 grayTexture, UnityEngine.Vector2 crackTexture, int missionID)
    {
        this.m_material = base.GetComponent<Renderer>().materials[0];
        this.m_grayMaterial = base.GetComponent<Renderer>().materials[1];
        this.m_goldTexture = goldTexture;
        this.m_material.mainTextureOffset = this.m_goldTexture;
        this.m_grayTexture = grayTexture;
        this.m_grayMaterial.mainTextureOffset = this.m_grayTexture;
        this.m_crackTexture = crackTexture;
        this.m_cracks.GetComponent<Renderer>().material.mainTextureOffset = this.m_crackTexture;
        this.m_missionID = missionID;
        this.m_tooltipText.Text = GameUtils.GetMissionHeroName(missionID);
        this.m_originalPosition = base.transform.localPosition;
        this.m_originalXPosition = this.m_coinX.transform.localPosition;
    }

    public void SetCoinPressCallback(CoinPressCallback callback)
    {
        this.m_coinPressCallback = callback;
    }

    public void SetLessonAssetName(string lessonAssetName)
    {
        this.m_lessonAssetName = lessonAssetName;
    }

    public void SetProgress(CoinStatus status)
    {
        base.gameObject.SetActive(true);
        this.m_currentStatus = status;
        if (status == CoinStatus.DEFEATED)
        {
            this.m_material.mainTextureOffset = this.m_grayTexture;
            this.m_cracks.SetActive(true);
            this.m_coinX.SetActive(true);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            this.m_inputEnabled = false;
            this.m_coinLabel.gameObject.SetActive(false);
        }
        else if (status == CoinStatus.ACTIVE)
        {
            this.m_cracks.SetActive(false);
            this.m_coinX.SetActive(false);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            this.m_inputEnabled = true;
        }
        else if (status == CoinStatus.UNREVEALED_TO_ACTIVE)
        {
            this.OnUpdateAlphaVal(0f);
            object[] args = new object[] { "y", 3f, "z", this.m_originalPosition.z - 0.2f, "time", 0.5f, "isLocal", true, "easetype", iTween.EaseType.easeOutCubic };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(base.gameObject, hashtable);
            object[] objArray2 = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", 1f, "isLocal", true, "delay", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.RotateTo(base.gameObject, hashtable2);
            object[] objArray3 = new object[] { "y", this.m_originalPosition.y, "z", this.m_originalPosition.z, "time", 0.5f, "isLocal", true, "delay", 1.75f, "easetype", iTween.EaseType.easeOutCubic };
            Hashtable hashtable3 = iTween.Hash(objArray3);
            iTween.MoveTo(base.gameObject, hashtable3);
            object[] objArray4 = new object[] { "from", 0, "to", 1, "time", 0.25f, "delay", 1.5f, "easetype", iTween.EaseType.easeOutCirc, "onupdate", "OnUpdateAlphaVal", "oncomplete", "EnableInput", "oncompletetarget", base.gameObject };
            Hashtable hashtable4 = iTween.Hash(objArray4);
            iTween.ValueTo(base.gameObject, hashtable4);
            SoundManager.Get().LoadAndPlay("tutorial_mission_hero_coin_rises", base.gameObject);
            base.StartCoroutine(this.ShowCoinText());
            this.m_inputEnabled = false;
        }
        else if (status == CoinStatus.ACTIVE_TO_DEFEATED)
        {
            this.m_coinX.transform.localPosition = new Vector3(0f, 10f, (UniversalInputManager.UsePhoneUI == null) ? -0.23f : 0f);
            this.m_cracks.SetActive(true);
            this.m_coinX.SetActive(true);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            this.m_inputEnabled = false;
            RenderUtils.SetAlpha(this.m_coinX, 0f);
            RenderUtils.SetAlpha(this.m_cracks, 0f);
            object[] objArray5 = new object[] { "y", this.m_originalXPosition.y, "z", this.m_originalXPosition.z, "time", 0.25f, "isLocal", true, "easetype", iTween.EaseType.easeInCirc };
            Hashtable hashtable5 = iTween.Hash(objArray5);
            iTween.MoveTo(this.m_coinX, hashtable5);
            object[] objArray6 = new object[] { "amount", 1, "delay", 0, "time", 0.25f, "easeType", iTween.EaseType.easeInCirc };
            Hashtable hashtable6 = iTween.Hash(objArray6);
            iTween.FadeTo(this.m_coinX, hashtable6);
            object[] objArray7 = new object[] { "amount", 1, "delay", 0.15f, "time", 0.25f, "easeType", iTween.EaseType.easeInCirc };
            Hashtable hashtable7 = iTween.Hash(objArray7);
            iTween.FadeTo(this.m_cracks, hashtable7);
            SoundManager.Get().LoadAndPlay("tutorial_mission_x_descend", base.gameObject);
            base.StartCoroutine(this.SwitchCoinToGray());
            if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
            {
                GameState.Get().GetGameEntity().NotifyOfDefeatCoinAnimation();
            }
        }
        else
        {
            base.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            this.m_coinLabel.gameObject.SetActive(false);
            this.m_cracks.SetActive(false);
            this.m_coinX.SetActive(false);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            this.m_inputEnabled = false;
        }
    }

    [DebuggerHidden]
    public IEnumerator ShowCoinText()
    {
        return new <ShowCoinText>c__Iterator1EA { <>f__this = this };
    }

    private void ShowTooltip()
    {
        if (this.m_currentStatus != CoinStatus.UNREVEALED)
        {
            this.m_tooltip.SetActive(true);
            float num = 0f;
            float num2 = this.m_tooltipText.GetTextWorldSpaceBounds().size.x + num;
            float x = num2 / this.m_originalMiddleWidth;
            TransformUtil.SetLocalScaleX(this.m_middle, x);
            float num4 = this.m_originalMiddleWidth * 0.223f;
            float z = this.m_originalMiddleWidth * 0.01f;
            TransformUtil.SetPoint(this.m_leftCap, Anchor.RIGHT, this.m_middle, Anchor.LEFT, new Vector3(num4, 0f, z));
            TransformUtil.SetPoint(this.m_rightCap, Anchor.LEFT, this.m_middle, Anchor.RIGHT, new Vector3(-num4, 0f, z));
        }
    }

    private void Start()
    {
        this.m_coinLabel.Text = GameStrings.Get("GLOBAL_PLAY");
    }

    private void StartNextTutorial()
    {
        this.m_nextTutorialStarted = true;
        GameMgr.Get().FindGame(GameType.GT_TUTORIAL, this.m_missionID, 0L, 0L);
    }

    [DebuggerHidden]
    public IEnumerator SwitchCoinToGray()
    {
        return new <SwitchCoinToGray>c__Iterator1EB { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenTransition()
    {
        return new <WaitThenTransition>c__Iterator1EC();
    }

    [CompilerGenerated]
    private sealed class <ShowCoinText>c__Iterator1EA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroCoin <>f__this;

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
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_coinLabel.gameObject.SetActive(true);
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
    private sealed class <SwitchCoinToGray>c__Iterator1EB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HeroCoin <>f__this;

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
                {
                    this.<>f__this.m_material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
                    this.<>f__this.m_coinLabel.gameObject.SetActive(false);
                    object[] args = new object[] { "amount", new Vector3(0.2f, 0.2f, 0.2f), "name", "HeroCoin", "time", 0.5f, "delay", 0, "axis", "none" };
                    iTween.ShakePosition(Camera.main.gameObject, iTween.Hash(args));
                    this.$PC = -1;
                    break;
                }
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
    private sealed class <WaitThenTransition>c__Iterator1EC : IDisposable, IEnumerator, IEnumerator<object>
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
                    this.$current = new WaitForSeconds(1.25f);
                    this.$PC = 1;
                    return true;

                case 1:
                    LoadingScreen.Get().NotifyTransitionBlockerComplete();
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

    public delegate void CoinPressCallback();

    public enum CoinStatus
    {
        ACTIVE,
        DEFEATED,
        UNREVEALED,
        UNREVEALED_TO_ACTIVE,
        ACTIVE_TO_DEFEATED
    }
}

