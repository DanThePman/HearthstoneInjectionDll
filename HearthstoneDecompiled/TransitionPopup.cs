using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class TransitionPopup : MonoBehaviour
{
    private Vector3 END_POSITION;
    protected AdventureDbId m_adventureId;
    protected bool m_blockingLoadingScreen;
    private bool m_blurEnabled;
    public UIBButton m_cancelButton;
    public Float_MobileOverride m_endScale;
    protected Camera m_fullScreenEffectsCamera;
    protected List<MatchCanceledEvent> m_matchCanceledListeners = new List<MatchCanceledEvent>();
    public MatchingQueueTab m_queueTab;
    public Float_MobileOverride m_scaleAfterPunch;
    protected bool m_showAnimationFinished;
    protected bool m_shown;
    public Vector3_MobileOverride m_startPosition = new Vector3_MobileOverride(new Vector3(-0.05f, 8.2f, -1.8f));
    public UberText m_title;
    private float POPUP_TIME = 0.3f;
    private float START_SCALE_VAL = 0.1f;

    public event Action<TransitionPopup> OnHidden;

    protected TransitionPopup()
    {
    }

    protected void AnimateBlurBlendOff()
    {
        this.DisableFullScreenBlur();
        base.StartCoroutine(this.DelayDeactivatePopup(0.5f));
    }

    private void AnimateBlurBlendOn()
    {
        this.EnableFullScreenBlur();
    }

    protected virtual void AnimateHide()
    {
        this.m_shown = false;
        this.DisableCancelButton();
        iTween.FadeTo(base.gameObject, 0f, this.POPUP_TIME);
        object[] args = new object[] { "scale", new Vector3(this.START_SCALE_VAL, this.START_SCALE_VAL, this.START_SCALE_VAL), "time", this.POPUP_TIME };
        Hashtable hashtable = iTween.Hash(args);
        if (this.OnHidden != null)
        {
            hashtable["oncomplete"] = data => this.OnHidden(this);
        }
        iTween.ScaleTo(base.gameObject, hashtable);
        this.AnimateBlurBlendOff();
    }

    protected virtual void AnimateShow()
    {
        iTween.Stop(base.gameObject);
        this.m_shown = true;
        this.m_showAnimationFinished = false;
        base.gameObject.SetActive(true);
        SceneUtils.EnableRenderers(base.gameObject, false);
        this.DisableCancelButton();
        this.ShowPopup();
        this.AnimateBlurBlendOn();
    }

    protected virtual void Awake()
    {
        this.m_fullScreenEffectsCamera = Camera.main;
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelButtonReleased));
        this.m_cancelButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnCancelButtonOver));
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        base.gameObject.transform.localPosition = (Vector3) this.m_startPosition;
    }

    public void Cancel()
    {
        if (this.m_shown && (this.m_fullScreenEffectsCamera != null))
        {
            this.DisableFullScreenBlur();
        }
    }

    protected void DeactivatePopup()
    {
        base.gameObject.SetActive(false);
        this.StopBlockingTransition();
    }

    [DebuggerHidden]
    private IEnumerator DelayDeactivatePopup(float waitTime)
    {
        return new <DelayDeactivatePopup>c__IteratorD2 { waitTime = waitTime, <$>waitTime = waitTime, <>f__this = this };
    }

    protected virtual void DisableCancelButton()
    {
        this.m_cancelButton.Flip(false);
        this.m_cancelButton.SetEnabled(false);
    }

    private void DisableFullScreenBlur()
    {
        if (this.m_blurEnabled)
        {
            this.m_blurEnabled = false;
            FullScreenFXMgr.Get().StopBlur(0.5f, iTween.EaseType.easeOutCirc, null);
        }
    }

    protected virtual void EnableCancelButton()
    {
        this.m_cancelButton.Flip(true);
        this.m_cancelButton.SetEnabled(true);
    }

    protected virtual bool EnableCancelButtonIfPossible()
    {
        if (!this.m_showAnimationFinished)
        {
            return false;
        }
        if (GameMgr.Get().IsAboutToStopFindingGame())
        {
            return false;
        }
        if (this.m_cancelButton.IsEnabled())
        {
            return false;
        }
        this.EnableCancelButton();
        return true;
    }

    private void EnableFullScreenBlur()
    {
        if (!this.m_blurEnabled)
        {
            this.m_blurEnabled = true;
            FullScreenFXMgr mgr = FullScreenFXMgr.Get();
            mgr.SetBlurAmount(0.3f);
            mgr.SetBlurBrightness(0.4f);
            mgr.SetBlurDesaturation(0.5f);
            mgr.Blur(1f, 0.5f, iTween.EaseType.easeOutCirc, null);
        }
    }

    protected void FireMatchCanceledEvent()
    {
        MatchCanceledEvent[] eventArray = this.m_matchCanceledListeners.ToArray();
        if (eventArray.Length == 0)
        {
            UnityEngine.Debug.LogError("TransitionPopup.FireMatchCanceledEvent() - Cancel triggered, but nobody was listening!!");
        }
        foreach (MatchCanceledEvent event2 in eventArray)
        {
            event2();
        }
    }

    public void Hide()
    {
        if (this.m_shown)
        {
            this.AnimateHide();
        }
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    protected virtual void OnAnimateShowFinished()
    {
        this.m_showAnimationFinished = true;
    }

    protected virtual void OnCancelButtonOver(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Mouseover");
    }

    protected virtual void OnCancelButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Back_Click");
        this.m_cancelButton.SetEnabled(false);
    }

    protected virtual void OnDestroy()
    {
        if (FullScreenFXMgr.Get() != null)
        {
            this.DisableFullScreenBlur();
        }
        this.StopBlockingTransition();
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
        if (this.m_shown && (this.OnHidden != null))
        {
            this.OnHidden(this);
        }
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        if (this.m_shown)
        {
            switch (eventData.m_state)
            {
                case FindGameState.CLIENT_ERROR:
                case FindGameState.BNET_ERROR:
                    this.OnGameError(eventData);
                    break;

                case FindGameState.BNET_QUEUE_ENTERED:
                    this.OnGameEntered(eventData);
                    break;

                case FindGameState.BNET_QUEUE_DELAYED:
                    this.OnGameDelayed(eventData);
                    break;

                case FindGameState.BNET_QUEUE_UPDATED:
                    this.OnGameUpdated(eventData);
                    break;

                case FindGameState.BNET_QUEUE_CANCELED:
                    this.OnGameCanceled(eventData);
                    break;

                case FindGameState.SERVER_GAME_CONNECTING:
                    this.OnGameConnecting(eventData);
                    break;

                case FindGameState.SERVER_GAME_STARTED:
                    this.OnGameStarted(eventData);
                    break;
            }
        }
        return false;
    }

    protected virtual void OnGameCanceled(FindGameEventData eventData)
    {
    }

    protected virtual void OnGameConnecting(FindGameEventData eventData)
    {
        this.DisableCancelButton();
    }

    protected virtual void OnGameDelayed(FindGameEventData eventData)
    {
    }

    protected virtual void OnGameEntered(FindGameEventData eventData)
    {
        this.m_queueTab.UpdateDisplay(eventData.m_queueMinSeconds, eventData.m_queueMaxSeconds);
    }

    protected virtual void OnGameError(FindGameEventData eventData)
    {
    }

    protected abstract void OnGameplaySceneLoaded();
    protected virtual void OnGameStarted(FindGameEventData eventData)
    {
        this.StartBlockingTransition();
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
    }

    protected virtual void OnGameUpdated(FindGameEventData eventData)
    {
        this.m_queueTab.UpdateDisplay(eventData.m_queueMinSeconds, eventData.m_queueMaxSeconds);
    }

    protected virtual void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (this.m_shown)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
            this.OnGameplaySceneLoaded();
        }
    }

    private void PunchPopup()
    {
        iTween.ScaleTo(base.gameObject, new Vector3((float) this.m_scaleAfterPunch, (float) this.m_scaleAfterPunch, (float) this.m_scaleAfterPunch), 0.15f);
        this.OnAnimateShowFinished();
    }

    public void RegisterMatchCanceledEvent(MatchCanceledEvent callback)
    {
        this.m_matchCanceledListeners.Add(callback);
    }

    public void SetAdventureId(AdventureDbId adventureId)
    {
        this.m_adventureId = adventureId;
    }

    public void Show()
    {
        if (!this.m_shown)
        {
            this.AnimateShow();
        }
    }

    protected virtual void ShowPopup()
    {
        SceneUtils.EnableRenderers(base.gameObject, true);
        iTween.FadeTo(base.gameObject, 1f, this.POPUP_TIME);
        base.gameObject.transform.localScale = new Vector3(this.START_SCALE_VAL, this.START_SCALE_VAL, this.START_SCALE_VAL);
        object[] args = new object[] { "scale", new Vector3((float) this.m_endScale, (float) this.m_endScale, (float) this.m_endScale), "time", this.POPUP_TIME, "oncomplete", "PunchPopup", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ScaleTo(base.gameObject, hashtable);
        object[] objArray2 = new object[] { "position", base.gameObject.transform.localPosition + new Vector3(0.02f, 0.02f, 0.02f), "time", 1.5f, "islocal", true };
        iTween.MoveTo(base.gameObject, iTween.Hash(objArray2));
        this.m_queueTab.ResetTimer();
    }

    protected virtual void Start()
    {
        if (this.m_fullScreenEffectsCamera == null)
        {
            this.m_fullScreenEffectsCamera = Camera.main;
        }
        if (!this.m_shown)
        {
            iTween.FadeTo(base.gameObject, 0f, 0f);
            base.gameObject.SetActive(false);
        }
    }

    protected void StartBlockingTransition()
    {
        this.m_blockingLoadingScreen = true;
        LoadingScreen.Get().AddTransitionBlocker();
        LoadingScreen.Get().AddTransitionObject(base.gameObject);
    }

    protected void StopBlockingTransition()
    {
        if (this.m_blockingLoadingScreen)
        {
            this.m_blockingLoadingScreen = false;
            if (LoadingScreen.Get() != null)
            {
                LoadingScreen.Get().NotifyTransitionBlockerComplete();
            }
        }
    }

    public bool UnregisterMatchCanceledEvent(MatchCanceledEvent callback)
    {
        return this.m_matchCanceledListeners.Remove(callback);
    }

    [CompilerGenerated]
    private sealed class <DelayDeactivatePopup>c__IteratorD2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>waitTime;
        internal TransitionPopup <>f__this;
        internal float waitTime;

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
                    this.$current = new WaitForSeconds(this.waitTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!this.<>f__this.m_shown)
                    {
                        this.<>f__this.DeactivatePopup();
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

    public delegate void MatchCanceledEvent();
}

