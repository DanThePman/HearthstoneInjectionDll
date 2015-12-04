using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class OptionsMenu : MonoBehaviour
{
    [CompilerGenerated]
    private static SoundManager.LoadedCallback <>f__am$cache1D;
    private Vector3 HIDDEN_SCALE;
    private readonly PlatformDependentValue<bool> LANGUAGE_SELECTION;
    [CustomEditField(Sections="Sound")]
    public CheckBox m_backgroundSound;
    [CustomEditField(Sections="Misc")]
    public UIBButton m_cinematicButton;
    [CustomEditField(Sections="Misc")]
    public UIBButton m_creditsButton;
    [CustomEditField(Sections="Graphics")]
    public CheckBox m_fullScreenCheckbox;
    private List<GraphicsResolution> m_fullScreenResolutions = new List<GraphicsResolution>();
    [CustomEditField(Sections="Graphics")]
    public GameObject m_graphicsGroup;
    [CustomEditField(Sections="Graphics")]
    public DropdownControl m_graphicsQuality;
    [CustomEditField(Sections="Graphics")]
    public DropdownControl m_graphicsRes;
    private hideHandler m_hideHandler;
    [CustomEditField(Sections="Internal Stuff")]
    public PegUIElement m_inputBlocker;
    private bool m_isShown;
    [CustomEditField(Sections="Language")]
    public DropdownControl m_languageDropdown;
    [CustomEditField(Sections="Language")]
    public FontDef m_languageDropdownFont;
    [CustomEditField(Sections="Language")]
    public GameObject m_languageGroup;
    [CustomEditField(Sections="Language")]
    public CheckBox m_languagePackCheckbox;
    [CustomEditField(Sections="Layout")]
    public MultiSliceElement m_leftPane;
    [CustomEditField(Sections="Sound")]
    public ScrollbarControl m_masterVolume;
    [CustomEditField(Sections="Layout")]
    public MultiSliceElement m_middlePane;
    [CustomEditField(Sections="Misc")]
    public GameObject m_miscGroup;
    [CustomEditField(Sections="Sound")]
    public ScrollbarControl m_musicVolume;
    [CustomEditField(Sections="Preferences")]
    public CheckBox m_nearbyPlayers;
    [CustomEditField(Sections="Layout")]
    public MultiSliceElement m_rightPane;
    [CustomEditField(Sections="Sound")]
    public GameObject m_soundGroup;
    [CustomEditField(Sections="Preferences")]
    public CheckBox m_spectatorOpenJoinCheckbox;
    [CustomEditField(Sections="Internal Stuff")]
    public UberText m_versionLabel;
    private Vector3 NORMAL_SCALE;
    private static OptionsMenu s_instance;

    public OptionsMenu()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = false,
            Mac = false
        };
        this.LANGUAGE_SELECTION = value2;
    }

    private void Awake()
    {
        s_instance = this;
        this.NORMAL_SCALE = base.transform.localScale;
        this.HIDDEN_SCALE = (Vector3) (0.01f * this.NORMAL_SCALE);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.m_graphicsRes.setItemTextCallback(new DropdownControl.itemTextCallback(this.OnGraphicsResolutionDropdownText));
            this.m_graphicsRes.setItemChosenCallback(new DropdownControl.itemChosenCallback(this.OnNewGraphicsResolution));
            foreach (GraphicsResolution resolution in this.GetGoodGraphicsResolution())
            {
                this.m_graphicsRes.addItem(resolution);
            }
            this.m_graphicsRes.setSelection(this.GetCurrentGraphicsResolution());
            this.m_fullScreenCheckbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnToggleFullScreenCheckbox));
            this.m_fullScreenCheckbox.SetChecked(Options.Get().GetBool(Option.GFX_FULLSCREEN, Screen.fullScreen));
            this.m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"));
            this.m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM"));
            this.m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH"));
            this.m_graphicsQuality.setSelection(this.GetCurrentGraphicsQuality());
            this.m_graphicsQuality.setItemChosenCallback(new DropdownControl.itemChosenCallback(this.OnNewGraphicsQuality));
        }
        this.m_masterVolume.SetValue(Options.Get().GetFloat(Option.SOUND_VOLUME));
        this.m_masterVolume.SetUpdateHandler(new ScrollbarControl.UpdateHandler(this.OnNewMasterVolume));
        this.m_masterVolume.SetFinishHandler(new ScrollbarControl.FinishHandler(this.OnMasterVolumeRelease));
        this.m_musicVolume.SetValue(Options.Get().GetFloat(Option.MUSIC_VOLUME));
        this.m_musicVolume.SetUpdateHandler(new ScrollbarControl.UpdateHandler(this.OnNewMusicVolume));
        if (this.m_backgroundSound != null)
        {
            this.m_backgroundSound.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleBackgroundSound));
            this.m_backgroundSound.SetChecked(Options.Get().GetBool(Option.BACKGROUND_SOUND));
        }
        this.UpdateCreditsUI();
        this.m_nearbyPlayers.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleNearbyPlayers));
        this.m_nearbyPlayers.SetChecked(Options.Get().GetBool(Option.NEARBY_PLAYERS));
        this.m_spectatorOpenJoinCheckbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleSpectatorOpenJoin));
        this.m_spectatorOpenJoinCheckbox.SetChecked(Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN));
        this.m_languageGroup.gameObject.SetActive((bool) this.LANGUAGE_SELECTION);
        if (this.LANGUAGE_SELECTION != null)
        {
            if (Localization.GetLocale() == Locale.jaJP)
            {
                this.m_languageDropdown.setFontWithoutLocalization(this.m_languageDropdownFont);
            }
            else
            {
                this.m_languageDropdown.setFont(this.m_languageDropdownFont.m_Font);
            }
            IEnumerator enumerator = Enum.GetValues(typeof(Locale)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Locale current = (Locale) ((int) enumerator.Current);
                    switch (current)
                    {
                        case Locale.UNKNOWN:
                        case Locale.enGB:
                        {
                            continue;
                        }
                    }
                    this.m_languageDropdown.addItem(GameStrings.Get(this.StringNameFromLocale(current)));
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
            this.m_languageDropdown.setSelection(this.GetCurrentLanguage());
            this.m_languageDropdown.setItemChosenCallback(new DropdownControl.itemChosenCallback(this.OnNewLanguage));
        }
        if ((AssetLoader.DOWNLOADABLE_LANGUAGE_PACKS != null) && ApplicationMgr.IsInternal())
        {
            this.m_languagePackCheckbox.gameObject.SetActive(true);
            this.m_languagePackCheckbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleLanguagePackCheckbox));
            this.m_languagePackCheckbox.SetChecked(Downloader.Get().AllLocalizedAudioBundlesDownloaded());
        }
        this.m_creditsButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCreditsButtonReleased));
        this.m_cinematicButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCinematicButtonReleased));
        this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, e => this.Hide(true));
        this.ShowOrHide(false);
        this.m_leftPane.UpdateSlices();
        this.m_rightPane.UpdateSlices();
        this.m_middlePane.UpdateSlices();
    }

    private bool CanShowCredits()
    {
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        switch (mode)
        {
            case SceneMgr.Mode.GAMEPLAY:
            case SceneMgr.Mode.PACKOPENING:
                break;

            default:
                switch (mode)
                {
                    case SceneMgr.Mode.CREDITS:
                    case SceneMgr.Mode.ADVENTURE:
                        break;

                    case SceneMgr.Mode.RESET:
                        goto Label_003D;

                    default:
                        goto Label_003D;
                }
                break;
        }
        return false;
    Label_003D:
        if ((GeneralStore.Get() != null) && GeneralStore.Get().IsShown())
        {
            return false;
        }
        if (Network.Get().IsFindingGame())
        {
            return false;
        }
        if (!GameUtils.AreAllTutorialsComplete())
        {
            return false;
        }
        if (WelcomeQuests.Get() != null)
        {
            return false;
        }
        if ((ArenaStore.Get() != null) && ArenaStore.Get().IsShown())
        {
            return false;
        }
        if ((DraftDisplay.Get() != null) && (DraftDisplay.Get().GetDraftMode() == DraftDisplay.DraftMode.IN_REWARDS))
        {
            return false;
        }
        return true;
    }

    public static OptionsMenu Get()
    {
        return s_instance;
    }

    private string GetCurrentGraphicsQuality()
    {
        switch (Options.Get().GetInt(Option.GFX_QUALITY))
        {
            case 0:
                return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW");

            case 1:
                return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM");

            case 2:
                return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH");
        }
        return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW");
    }

    private GraphicsResolution GetCurrentGraphicsResolution()
    {
        int @int = Options.Get().GetInt(Option.GFX_WIDTH, Screen.currentResolution.width);
        int height = Options.Get().GetInt(Option.GFX_HEIGHT, Screen.currentResolution.height);
        return GraphicsResolution.create(@int, height);
    }

    private string GetCurrentLanguage()
    {
        return GameStrings.Get(this.StringNameFromLocale(Localization.GetLocale()));
    }

    private List<GraphicsResolution> GetGoodGraphicsResolution()
    {
        if (this.m_fullScreenResolutions.Count == 0)
        {
            foreach (GraphicsResolution resolution in GraphicsResolution.list)
            {
                if (((resolution.x >= 0x400) && (resolution.y >= 0x2d8)) && (((resolution.aspectRatio - 0.01) <= 1.7777777777777777) && ((resolution.aspectRatio + 0.01) >= 1.3333333333333333)))
                {
                    this.m_fullScreenResolutions.Add(resolution);
                }
            }
        }
        return this.m_fullScreenResolutions;
    }

    public hideHandler GetHideHandler()
    {
        return this.m_hideHandler;
    }

    public void Hide(bool callHideHandler = true)
    {
        this.ShowOrHide(false);
        if ((this.m_hideHandler != null) && callHideHandler)
        {
            this.m_hideHandler();
            this.m_hideHandler = null;
        }
    }

    public bool IsShown()
    {
        return this.m_isShown;
    }

    private void OnChangeLanguageConfirmationResponse(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CANCEL)
        {
            this.m_languageDropdown.setSelection(this.GetCurrentLanguage());
            return;
        }
        string localeName = null;
        string str2 = (string) userData;
        IEnumerator enumerator = Enum.GetValues(typeof(Locale)).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Locale current = (Locale) ((int) enumerator.Current);
                if (str2 == GameStrings.Get(this.StringNameFromLocale(current)))
                {
                    localeName = current.ToString();
                    goto Label_0095;
                }
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
    Label_0095:
        if (localeName == null)
        {
            Debug.LogError(string.Format("OptionsMenu.OnChangeLanguageConfirmationResponse() - locale not found", new object[0]));
        }
        else
        {
            Localization.SetLocaleName(localeName);
            Options.Get().SetString(Option.LOCALE, localeName);
            this.Hide(false);
            ApplicationMgr.Get().Reset();
        }
    }

    private void OnCinematicButtonReleased(UIEvent e)
    {
        Cinematic componentInChildren = SceneMgr.Get().GetComponentInChildren<Cinematic>();
        if (componentInChildren != null)
        {
            this.Hide(false);
            componentInChildren.Play(new Cinematic.MovieCallback(GameMenu.Get().ShowOptionsMenu));
        }
        else
        {
            Debug.LogWarning("Failed to locate Cinematic component on SceneMgr!");
        }
    }

    private void OnCreditsButtonReleased(UIEvent e)
    {
        this.Hide(false);
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.CREDITS);
    }

    public void OnDestroy()
    {
        if (FatalErrorMgr.Get() != null)
        {
            FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        }
        s_instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.Hide(true);
    }

    private string OnGraphicsResolutionDropdownText(object val)
    {
        GraphicsResolution resolution = (GraphicsResolution) val;
        return string.Format("{0} x {1}", resolution.x, resolution.y);
    }

    private void OnMasterVolumeRelease()
    {
        if (<>f__am$cache1D == null)
        {
            <>f__am$cache1D = (source, userData) => SoundManager.Get().Set3d(source, false);
        }
        SoundManager.LoadedCallback callback = <>f__am$cache1D;
        SoundManager.Get().LoadAndPlay("UI_MouseClick_01", base.gameObject, 1f, callback);
    }

    private void OnNewGraphicsQuality(object selection, object prevSelection)
    {
        GraphicsQuality low = GraphicsQuality.Low;
        string str = (string) selection;
        if (str == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"))
        {
            low = GraphicsQuality.Low;
        }
        else if (str == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM"))
        {
            low = GraphicsQuality.Medium;
        }
        else if (str == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH"))
        {
            low = GraphicsQuality.High;
        }
        Log.Kyle.Print("Graphics Quality: " + low.ToString(), new object[0]);
        GraphicsManager.Get().RenderQualityLevel = low;
    }

    private void OnNewGraphicsResolution(object selection, object prevSelection)
    {
        GraphicsResolution resolution = (GraphicsResolution) selection;
        GraphicsManager.Get().SetScreenResolution(resolution.x, resolution.y, this.m_fullScreenCheckbox.IsChecked());
        Options.Get().SetInt(Option.GFX_WIDTH, resolution.x);
        Options.Get().SetInt(Option.GFX_HEIGHT, resolution.y);
    }

    private void OnNewLanguage(object selection, object prevSelection)
    {
        if (selection != prevSelection)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_CONFIRM_TITLE"),
                m_text = GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_CONFIRM_MESSAGE"),
                m_showAlertIcon = false,
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_responseCallback = new AlertPopup.ResponseCallback(this.OnChangeLanguageConfirmationResponse),
                m_responseUserData = selection
            };
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void OnNewMasterVolume(float newVolume)
    {
        Options.Get().SetFloat(Option.SOUND_VOLUME, newVolume);
    }

    private void OnNewMusicVolume(float newVolume)
    {
        Options.Get().SetFloat(Option.MUSIC_VOLUME, newVolume);
    }

    private void OnToggleFullScreenCheckbox(UIEvent e)
    {
        GraphicsResolution resolution = this.m_graphicsRes.getSelection() as GraphicsResolution;
        if (resolution == null)
        {
            this.m_graphicsRes.setSelectionToLastItem();
            resolution = this.m_graphicsRes.getSelection() as GraphicsResolution;
        }
        if (resolution != null)
        {
            GraphicsManager.Get().SetScreenResolution(resolution.x, resolution.y, this.m_fullScreenCheckbox.IsChecked());
            Options.Get().SetBool(Option.GFX_FULLSCREEN, this.m_fullScreenCheckbox.IsChecked());
        }
    }

    public void RemoveHideHandler(hideHandler handler)
    {
        if (this.m_hideHandler == handler)
        {
            this.m_hideHandler = null;
        }
    }

    public void SetHideHandler(hideHandler handler)
    {
        this.m_hideHandler = handler;
    }

    public void Show()
    {
        this.UpdateCreditsUI();
        this.ShowOrHide(true);
        AnimationUtil.ShowWithPunch(base.gameObject, this.HIDDEN_SCALE, (Vector3) (1.1f * this.NORMAL_SCALE), this.NORMAL_SCALE, null, true, null, null, null);
    }

    private void ShowOrHide(bool showOrHide)
    {
        this.m_isShown = showOrHide;
        base.gameObject.SetActive(showOrHide);
        this.m_leftPane.UpdateSlices();
        this.m_rightPane.UpdateSlices();
        this.m_middlePane.UpdateSlices();
    }

    private string StringNameFromLocale(Locale locale)
    {
        return ("GLOBAL_LANGUAGE_NATIVE_" + locale.ToString().ToUpper());
    }

    private void ToggleBackgroundSound(UIEvent e)
    {
        Options.Get().SetBool(Option.BACKGROUND_SOUND, this.m_backgroundSound.IsChecked());
    }

    private void ToggleLanguagePackCheckbox(UIEvent e)
    {
        if (this.m_languagePackCheckbox.IsChecked())
        {
            Downloader.Get().DownloadLocalizedBundles();
        }
        else
        {
            Downloader.Get().DeleteLocalizedBundles();
        }
    }

    private void ToggleNearbyPlayers(UIEvent e)
    {
        Options.Get().SetBool(Option.NEARBY_PLAYERS, this.m_nearbyPlayers.IsChecked());
    }

    private void ToggleSpectatorOpenJoin(UIEvent e)
    {
        Options.Get().SetBool(Option.SPECTATOR_OPEN_JOIN, this.m_spectatorOpenJoinCheckbox.IsChecked());
    }

    private void UpdateCreditsUI()
    {
        this.m_miscGroup.SetActive(this.CanShowCredits());
    }

    public delegate void hideHandler();
}

