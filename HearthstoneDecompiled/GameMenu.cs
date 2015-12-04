using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class GameMenu : ButtonListMenu
{
    private readonly Vector3 BUTTON_SCALE;
    private readonly Vector3 BUTTON_SCALE_PHONE;
    private Network.BnetRegion m_AccountRegion;
    private UIBButton m_concedeButton;
    private bool m_hasSeenLoginTooltip;
    private Notification m_loginButtonPopup;
    public Transform m_menuBone;
    private UIBButton m_optionsButton;
    private OptionsMenu m_optionsMenu;
    private UIBButton m_quitButton;
    private GameObject m_ratingsObject;
    [CustomEditField(Sections="Template Items")]
    public Vector3 m_ratingsObjectMinPadding = new Vector3(0f, 0f, -0.06f);
    private UIBButton m_resumeButton;
    private PlatformDependentValue<string> OPTIONS_MENU_NAME;
    private static GameMenu s_instance;

    public GameMenu()
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "OptionsMenu",
            Phone = "OptionsMenu_phone"
        };
        this.OPTIONS_MENU_NAME = value2;
        this.BUTTON_SCALE = (Vector3) (15f * Vector3.one);
        this.BUTTON_SCALE_PHONE = (Vector3) (25f * Vector3.one);
    }

    protected override void Awake()
    {
        base.m_menuParent = this.m_menuBone;
        base.Awake();
        s_instance = this;
        this.LoadRatings();
        this.m_concedeButton = base.CreateMenuButton("ConcedeButton", "GLOBAL_CONCEDE", new UIEvent.Handler(this.ConcedeButtonPressed));
        if (ApplicationMgr.CanQuitGame != null)
        {
            this.m_quitButton = base.CreateMenuButton("QuitButton", "GLOBAL_QUIT", new UIEvent.Handler(this.QuitButtonPressed));
        }
        else
        {
            this.m_quitButton = base.CreateMenuButton("LogoutButton", !Network.ShouldBeConnectedToAurora() ? "GLOBAL_LOGIN" : "GLOBAL_LOGOUT", new UIEvent.Handler(this.LogoutButtonPressed));
        }
        this.m_resumeButton = base.CreateMenuButton("ResumeButton", "GLOBAL_RESUME_GAME", new UIEvent.Handler(this.ResumeButtonPressed));
        this.m_optionsButton = base.CreateMenuButton("OptionsButton", "GLOBAL_OPTIONS", new UIEvent.Handler(this.OptionsButtonPressed));
        base.m_menu.m_headerText.Text = GameStrings.Get("GLOBAL_GAME_MENU");
    }

    private void ConcedeButtonPressed(UIEvent e)
    {
        if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
        {
            SpectatorManager.Get().LeaveSpectatorMode();
        }
        else if (GameState.Get() != null)
        {
            GameState.Get().Concede();
        }
        this.Hide();
    }

    public static GameMenu Get()
    {
        return s_instance;
    }

    protected override List<UIBButton> GetButtons()
    {
        List<UIBButton> list = new List<UIBButton>();
        if (this.IsInGameMenu())
        {
            list.Add(this.m_concedeButton);
            list.Add(null);
        }
        if (!DemoMgr.Get().IsExpoDemo())
        {
            list.Add(this.m_optionsButton);
            list.Add(this.m_quitButton);
            list.Add(null);
        }
        list.Add(this.m_resumeButton);
        return list;
    }

    public override void Hide()
    {
        base.Hide();
        this.HideLoginTooltip();
        BnetBar.Get().m_menuButton.SetSelected(false);
    }

    public void HideLoginTooltip()
    {
        if (this.m_loginButtonPopup != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.m_loginButtonPopup);
        }
        this.m_loginButtonPopup = null;
    }

    public bool IsInGameMenu()
    {
        if ((!SceneMgr.Get().IsInGame() || !SceneMgr.Get().IsSceneLoaded()) || LoadingScreen.Get().IsTransitioning())
        {
            return false;
        }
        if (GameState.Get() == null)
        {
            return false;
        }
        if (!GameState.Get().IsConcedingAllowed() && !GameMgr.Get().IsSpectator())
        {
            return false;
        }
        if (GameState.Get().IsGameOver())
        {
            return false;
        }
        if ((TutorialProgressScreen.Get() != null) && TutorialProgressScreen.Get().gameObject.activeInHierarchy)
        {
            return false;
        }
        return true;
    }

    protected override void LayoutMenu()
    {
        base.LayoutMenuButtons();
        if (this.m_ratingsObject != null)
        {
            base.m_menu.m_buttonContainer.AddSlice(this.m_ratingsObject, Vector3.zero, this.m_ratingsObjectMinPadding, false);
        }
        base.m_menu.m_buttonContainer.UpdateSlices();
        if (this.m_concedeButton != null)
        {
            string text = GameStrings.Get(!GameMgr.Get().IsSpectator() ? "GLOBAL_CONCEDE" : "GLOBAL_LEAVE_SPECTATOR_MODE");
            this.m_concedeButton.SetText(text);
        }
        base.LayoutMenuBackground();
    }

    private void LoadRatings()
    {
        if (this.UseKoreanRating())
        {
            AssetLoader.Get().LoadGameObject("Korean_Ratings_OptionsScreen", delegate (string name, GameObject go, object data) {
                if (go != null)
                {
                    Quaternion localRotation = go.transform.localRotation;
                    GameUtils.SetParent(go, base.m_menu.m_buttonContainer, false);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = localRotation;
                    this.m_ratingsObject = go;
                    this.LayoutMenu();
                }
            }, null, false);
        }
    }

    private void LogoutButtonPressed(UIEvent e)
    {
        this.HideLoginTooltip();
        GameUtils.LogoutConfirmation();
        this.Hide();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (this.m_optionsMenu != null)
        {
            this.m_optionsMenu.RemoveHideHandler(new OptionsMenu.hideHandler(this.OnOptionsMenuHidden));
        }
        s_instance = null;
    }

    private void OnOptionsMenuHidden()
    {
        UnityEngine.Object.Destroy(this.m_optionsMenu.gameObject);
        this.m_optionsMenu = null;
        AssetCache.ClearGameObject((string) this.OPTIONS_MENU_NAME);
        if ((!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR) && !ApplicationMgr.Get().IsResetting()) && BnetBar.Get().IsEnabled())
        {
            this.Show();
        }
    }

    private void OptionsButtonPressed(UIEvent e)
    {
        this.ShowOptionsMenu();
    }

    private void QuitButtonPressed(UIEvent e)
    {
        Network.AutoConcede();
        ApplicationMgr.Get().Exit();
    }

    private void ResumeButtonPressed(UIEvent e)
    {
        this.Hide();
    }

    public override void Show()
    {
        if ((OptionsMenu.Get() != null) && OptionsMenu.Get().IsShown())
        {
            UniversalInputManager.Get().CancelTextInput(base.gameObject, true);
            OptionsMenu.Get().Hide(true);
        }
        else
        {
            base.Show();
            if ((UniversalInputManager.UsePhoneUI != null) && (this.m_ratingsObject != null))
            {
                this.m_ratingsObject.SetActive(this.UseKoreanRating());
                base.m_menu.m_buttonContainer.UpdateSlices();
                base.LayoutMenuBackground();
            }
            this.ShowLoginTooltipIfNeeded();
            BnetBar.Get().m_menuButton.SetSelected(true);
        }
    }

    public void ShowLoginTooltipIfNeeded()
    {
        if ((!Network.ShouldBeConnectedToAurora() && !this.m_hasSeenLoginTooltip) && (this.m_quitButton != null))
        {
            Vector3 vector;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                vector = new Vector3(-82.9f, 42.1f, 17.2f);
                this.m_loginButtonPopup = NotificationManager.Get().CreatePopupText(vector, this.BUTTON_SCALE_PHONE, GameStrings.Get("GLOBAL_MOBILE_LOG_IN_TOOLTIP"), false);
            }
            else
            {
                vector = new Vector3(-46.9f, 34.2f, 9.4f);
                this.m_loginButtonPopup = NotificationManager.Get().CreatePopupText(vector, this.BUTTON_SCALE, GameStrings.Get("GLOBAL_MOBILE_LOG_IN_TOOLTIP"), false);
            }
            this.m_loginButtonPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
            this.m_hasSeenLoginTooltip = true;
        }
    }

    public void ShowOptionsMenu()
    {
        this.Hide();
        if (this.m_optionsMenu == null)
        {
            this.m_optionsMenu = AssetLoader.Get().LoadGameObject((string) this.OPTIONS_MENU_NAME, true, false).GetComponent<OptionsMenu>();
            if (this.m_optionsMenu != null)
            {
                this.SwitchToOptionsMenu();
            }
        }
        else
        {
            this.SwitchToOptionsMenu();
        }
    }

    private void Start()
    {
        base.gameObject.SetActive(false);
    }

    private void SwitchToOptionsMenu()
    {
        this.m_optionsMenu.SetHideHandler(new OptionsMenu.hideHandler(this.OnOptionsMenuHidden));
        this.m_optionsMenu.Show();
    }

    private bool UseKoreanRating()
    {
        if (SceneMgr.Get().IsInGame())
        {
            return false;
        }
        return (BattleNet.GetAccountCountry() == "KOR");
    }
}

