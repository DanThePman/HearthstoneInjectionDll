using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class SlidingTray : MonoBehaviour
{
    [CustomEditField(Sections="Parameters")]
    public bool m_animateBounce;
    [CustomEditField(Sections="Parameters")]
    public bool m_inactivateOnHide = true;
    [CustomEditField(Sections="Optional Features")]
    public PegUIElement m_offClickCatcher;
    [CustomEditField(Sections="Parameters")]
    public bool m_playAudioOnSlide = true;
    private bool m_startingPositionSet;
    [CustomEditField(Sections="Bones")]
    public Transform m_trayHiddenBone;
    private bool m_trayShown;
    [CustomEditField(Sections="Bones")]
    public Transform m_trayShownBone;
    [CustomEditField(Sections="Parameters")]
    public float m_traySlideDuration = 0.5f;
    private bool m_traySliderAnimating;
    [CustomEditField(Sections="Optional Features")]
    public PegUIElement m_traySliderButton;
    private TrayToggledListener m_trayToggledListener;
    [CustomEditField(Sections="Parameters")]
    public bool m_useNavigationBack;

    private void Awake()
    {
        if ((UniversalInputManager.UsePhoneUI != null) || (1 != 0))
        {
            if (!this.m_startingPositionSet)
            {
                base.transform.localPosition = this.m_trayHiddenBone.localPosition;
                this.m_trayShown = false;
                if (this.m_inactivateOnHide)
                {
                    base.gameObject.SetActive(false);
                }
                this.m_startingPositionSet = true;
            }
            if (this.m_traySliderButton != null)
            {
                this.m_traySliderButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnTraySliderPressed));
            }
            if (this.m_offClickCatcher != null)
            {
                this.m_offClickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClickCatcherPressed));
            }
        }
        else if ((this.m_traySliderButton != null) && this.m_inactivateOnHide)
        {
            this.m_traySliderButton.gameObject.SetActive(false);
        }
    }

    private bool BackPressed()
    {
        this.ToggleTraySlider(false, null, true);
        return true;
    }

    private void FadeEffectsIn(float time = 0.4f)
    {
        SceneUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
        SceneUtils.SetLayer(Box.Get().m_letterboxingContainer, GameLayer.IgnoreFullScreenEffects);
        FullScreenFXMgr.Get().StartStandardBlurVignette(time);
    }

    private void FadeEffectsOut(float time = 0.2f)
    {
        FullScreenFXMgr.Get().EndStandardBlurVignette(time, new FullScreenFXMgr.EffectListener(this.OnFadeFinished));
    }

    [ContextMenu("Hide")]
    public void HideTray()
    {
        this.ToggleTraySlider(false, null, true);
    }

    private void OnClickCatcherPressed(UIEvent e)
    {
        if (this.m_useNavigationBack)
        {
            Navigation.GoBack();
        }
        else
        {
            this.ToggleTraySlider(false, null, true);
        }
    }

    private void OnDestroy()
    {
        if (this.m_offClickCatcher != null)
        {
            this.m_offClickCatcher.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClickCatcherPressed));
        }
        if (this.m_traySliderButton != null)
        {
            this.m_traySliderButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnTraySliderPressed));
        }
    }

    private void OnFadeFinished()
    {
        if (base.gameObject != null)
        {
            SceneUtils.SetLayer(base.gameObject, GameLayer.Default);
            SceneUtils.SetLayer(Box.Get().m_letterboxingContainer, GameLayer.Default);
        }
    }

    private void OnTraySliderAnimFinished()
    {
        this.m_traySliderAnimating = false;
        if (!this.m_trayShown && this.m_inactivateOnHide)
        {
            base.gameObject.SetActive(false);
        }
    }

    private void OnTraySliderPressed(UIEvent e)
    {
        if (!this.m_useNavigationBack || !this.m_trayShown)
        {
            this.ToggleTraySlider(!this.m_trayShown, null, true);
        }
    }

    public void RegisterTrayToggleListener(TrayToggledListener listener)
    {
        this.m_trayToggledListener = listener;
    }

    [ContextMenu("Show")]
    public void ShowTray()
    {
        this.ToggleTraySlider(true, null, true);
    }

    private void Start()
    {
    }

    public void ToggleTraySlider(bool show, Transform target = null, bool animate = true)
    {
        if (this.m_trayShown != show)
        {
            if (show && (target != null))
            {
                this.m_trayShownBone = target;
            }
            if (show)
            {
                if (this.m_useNavigationBack)
                {
                    Navigation.Push(new Navigation.NavigateBackHandler(this.BackPressed));
                }
                base.gameObject.SetActive(true);
                if (base.gameObject.activeInHierarchy && animate)
                {
                    object[] args = new object[] { "position", this.m_trayShownBone.localPosition, "isLocal", true, "time", this.m_traySlideDuration, "oncomplete", "OnTraySliderAnimFinished", "oncompletetarget", base.gameObject, "easetype", !this.m_animateBounce ? ((object) iTween.Defaults.easeType) : ((object) 0x18) };
                    Hashtable hashtable = iTween.Hash(args);
                    iTween.MoveTo(base.gameObject, hashtable);
                    this.m_traySliderAnimating = true;
                    if (this.m_offClickCatcher != null)
                    {
                        this.FadeEffectsIn(0.4f);
                        this.m_offClickCatcher.gameObject.SetActive(true);
                    }
                    if (this.m_playAudioOnSlide)
                    {
                        SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_on");
                    }
                }
                else
                {
                    base.gameObject.transform.localPosition = this.m_trayShownBone.localPosition;
                }
            }
            else
            {
                if (this.m_useNavigationBack)
                {
                    Navigation.PopUnique(new Navigation.NavigateBackHandler(this.BackPressed));
                }
                if (base.gameObject.activeInHierarchy && animate)
                {
                    object[] objArray2 = new object[] { "position", this.m_trayHiddenBone.localPosition, "isLocal", true, "oncomplete", "OnTraySliderAnimFinished", "oncompletetarget", base.gameObject, "time", !this.m_animateBounce ? (this.m_traySlideDuration / 2f) : this.m_traySlideDuration, "easetype", !this.m_animateBounce ? iTween.EaseType.linear : iTween.EaseType.easeOutBounce };
                    Hashtable hashtable2 = iTween.Hash(objArray2);
                    iTween.MoveTo(base.gameObject, hashtable2);
                    this.m_traySliderAnimating = true;
                    if (this.m_offClickCatcher != null)
                    {
                        this.FadeEffectsOut(0.2f);
                        this.m_offClickCatcher.gameObject.SetActive(false);
                    }
                    if (this.m_playAudioOnSlide)
                    {
                        SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_off");
                    }
                }
                else
                {
                    base.gameObject.transform.localPosition = this.m_trayHiddenBone.localPosition;
                    if (this.m_inactivateOnHide)
                    {
                        base.gameObject.SetActive(false);
                    }
                }
            }
            this.m_trayShown = show;
            this.m_startingPositionSet = true;
            if (this.m_trayToggledListener != null)
            {
                this.m_trayToggledListener(show);
            }
        }
    }

    public bool TraySliderIsAnimating()
    {
        return this.m_traySliderAnimating;
    }

    public void UnregisterTrayToggleListener(TrayToggledListener listener)
    {
        if (this.m_trayToggledListener == listener)
        {
            this.m_trayToggledListener = null;
        }
        else
        {
            Log.JMac.Print("Attempting to unregister a TrayToggleListener that has not been registered!", new object[0]);
        }
    }

    public delegate void TrayToggledListener(bool shown);
}

