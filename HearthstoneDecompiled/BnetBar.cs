using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class BnetBar : MonoBehaviour
{
    [CompilerGenerated]
    private static Action<object> <>f__am$cache1F;
    [CompilerGenerated]
    private static Func<BnetGameAccountId, string> <>f__am$cache20;
    public static readonly int CameraDepth = 0x2f;
    public Flipbook m_batteryLevel;
    public Flipbook m_batteryLevelPhone;
    public ConnectionIndicator m_connectionIndicator;
    public CurrencyFrame m_currencyFrame;
    public UberText m_currentTime;
    public BnetBarFriendButton m_friendButton;
    private GameMenu m_gameMenu;
    private bool m_gameMenuLoading;
    private bool m_hasUnacknowledgedPendingInvites;
    private float m_initialConnectionIndicatorScaleX;
    private float m_initialFriendButtonScaleX;
    private float m_initialMenuButtonScaleX;
    private float m_initialWidth;
    private bool m_isEnabled = true;
    private bool m_isInitting = true;
    private bool m_isLoggedIn;
    private float m_lastClockUpdate;
    private float m_lightingBlend = 1f;
    private GameObject m_loginTooltip;
    public BnetBarMenuButton m_menuButton;
    public GameObject m_menuButtonMesh;
    public GameObject m_questProgressToastBone;
    public GameObject m_socialToastBone;
    private GameObject m_spectatorCountPanel;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_spectatorCountPrefabPath;
    public TooltipZone m_spectatorCountTooltipZone;
    private GameObject m_spectatorModeIndicator;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_spectatorModeIndicatorPrefab;
    private bool m_suppressLoginTooltip;
    private static BnetBar s_instance;

    private void Awake()
    {
        s_instance = this;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Transform transform = this.m_menuButton.transform;
            transform.localScale = (Vector3) (transform.localScale * 2f);
            Transform transform2 = this.m_friendButton.transform;
            transform2.localScale = (Vector3) (transform2.localScale * 2f);
        }
        else
        {
            this.m_connectionIndicator.gameObject.SetActive(false);
        }
        this.m_initialWidth = base.GetComponent<Renderer>().bounds.size.x;
        this.m_initialFriendButtonScaleX = this.m_friendButton.transform.localScale.x;
        this.m_initialMenuButtonScaleX = this.m_menuButton.transform.localScale.x;
        this.m_menuButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnMenuButtonReleased));
        this.m_friendButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnFriendButtonReleased));
        this.ToggleEnableButtons(false);
        this.m_batteryLevel.gameObject.SetActive(false);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        SpectatorManager.Get().OnInviteReceived += new SpectatorManager.InviteReceivedHandler(this.SpectatorManager_OnInviteReceived);
        SpectatorManager.Get().OnSpectatorToMyGame += new SpectatorManager.SpectatorToMyGameHandler(this.SpectatorManager_OnSpectatorToMyGame);
        SpectatorManager.Get().OnSpectatorModeChanged += new SpectatorManager.SpectatorModeChangedHandler(this.SpectatorManager_OnSpectatorModeChanged);
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        this.m_lightingBlend = this.m_menuButtonMesh.GetComponent<Renderer>().material.GetFloat("_LightingBlend");
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_batteryLevel = this.m_batteryLevelPhone;
            this.m_currentTime.gameObject.SetActive(false);
        }
        this.m_menuButton.SetPhoneStatusBarState(0);
    }

    private void DestroyLoginTooltip()
    {
        if (this.m_loginTooltip != null)
        {
            UnityEngine.Object.Destroy(this.m_loginTooltip);
            this.m_loginTooltip = null;
        }
    }

    public void Disable()
    {
        this.m_isEnabled = false;
        this.m_menuButtonMesh.GetComponent<Renderer>().sharedMaterial.SetFloat("_LightingBlend", 0.6f);
        if ((this.m_gameMenu != null) && this.m_gameMenu.IsShown())
        {
            this.m_gameMenu.Hide();
        }
        if ((OptionsMenu.Get() != null) && OptionsMenu.Get().IsShown())
        {
            OptionsMenu.Get().Hide(true);
        }
    }

    public void Enable()
    {
        this.m_isEnabled = true;
        this.m_menuButtonMesh.GetComponent<Renderer>().sharedMaterial.SetFloat("_LightingBlend", this.m_lightingBlend);
    }

    public static BnetBar Get()
    {
        return s_instance;
    }

    private bool HandleEscapeKey()
    {
        if ((this.m_gameMenu != null) && this.m_gameMenu.IsShown())
        {
            this.m_gameMenu.Hide();
            return true;
        }
        if ((OptionsMenu.Get() != null) && OptionsMenu.Get().IsShown())
        {
            OptionsMenu.Get().Hide(true);
            return true;
        }
        if ((QuestLog.Get() != null) && QuestLog.Get().IsShown())
        {
            QuestLog.Get().Hide();
            return true;
        }
        if ((GeneralStore.Get() != null) && GeneralStore.Get().IsShown())
        {
            GeneralStore.Get().Close();
            return true;
        }
        ChatMgr mgr = ChatMgr.Get();
        if ((mgr == null) || !mgr.HandleKeyboardInput())
        {
            if ((CraftingTray.Get() != null) && CraftingTray.Get().IsShown())
            {
                CraftingTray.Get().Hide();
                return true;
            }
            SceneMgr.Mode mode = SceneMgr.Get().GetMode();
            switch (mode)
            {
                case SceneMgr.Mode.FATAL_ERROR:
                    return true;

                case SceneMgr.Mode.LOGIN:
                    return true;

                case SceneMgr.Mode.STARTUP:
                    return true;
            }
            if ((mode != SceneMgr.Mode.GAMEPLAY) && !DemoMgr.Get().IsHubEscMenuEnabled())
            {
                return true;
            }
            if ((PlatformSettings.OS == OSCategory.Android) && (mode == SceneMgr.Mode.HUB))
            {
                return false;
            }
            this.ToggleGameMenu();
        }
        return true;
    }

    public bool HandleKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            return this.HandleEscapeKey();
        }
        ChatMgr mgr = ChatMgr.Get();
        return ((mgr != null) && mgr.HandleKeyboardInput());
    }

    public void HideFriendList()
    {
        ChatMgr.Get().CloseChatUI();
    }

    public bool IsEnabled()
    {
        return this.m_isEnabled;
    }

    private void OnDestroy()
    {
        SpectatorManager.Get().OnInviteReceived -= new SpectatorManager.InviteReceivedHandler(this.SpectatorManager_OnInviteReceived);
        SpectatorManager.Get().OnSpectatorToMyGame -= new SpectatorManager.SpectatorToMyGameHandler(this.SpectatorManager_OnSpectatorToMyGame);
        SpectatorManager.Get().OnSpectatorModeChanged -= new SpectatorManager.SpectatorModeChangedHandler(this.SpectatorManager_OnSpectatorModeChanged);
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.ToggleEnableButtons(false);
    }

    private void OnFriendButtonReleased(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        this.ToggleFriendListShowing();
    }

    public void OnLoggedIn()
    {
        if (Network.ShouldBeConnectedToAurora())
        {
            this.m_friendButton.gameObject.SetActive(true);
        }
        this.m_isLoggedIn = true;
        this.ToggleActive(true);
        this.Update();
        this.UpdateLayout();
    }

    private void OnMenuButtonReleased(UIEvent e)
    {
        this.ToggleGameMenu();
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (mode != SceneMgr.Mode.FATAL_ERROR)
        {
            this.m_suppressLoginTooltip = false;
            this.m_currencyFrame.RefreshContents();
            bool flag = (mode != SceneMgr.Mode.INVALID) && (mode != SceneMgr.Mode.FATAL_ERROR);
            if (flag && SpectatorManager.Get().IsInSpectatorMode())
            {
                this.SpectatorManager_OnSpectatorModeChanged(OnlineEventType.ADDED, null);
            }
            else if ((this.m_spectatorModeIndicator != null) && this.m_spectatorModeIndicator.activeSelf)
            {
                this.m_spectatorModeIndicator.SetActive(false);
            }
            if (flag && (this.m_spectatorCountPanel != null))
            {
                bool flag2 = SpectatorManager.Get().IsBeingSpectated();
                if (((UniversalInputManager.UsePhoneUI != null) && (SceneMgr.Get() != null)) && !SceneMgr.Get().IsInGame())
                {
                    flag2 = false;
                }
                this.m_spectatorCountPanel.SetActive(flag2);
            }
            this.UpdateForDemoMode();
            this.UpdateLayout();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.UpdateForPhone();
            }
        }
    }

    private void PositionCurrencyFrame(GameObject parent, Vector3 offset)
    {
        GameObject tooltipObject = this.m_currencyFrame.GetTooltipObject();
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
        TransformUtil.SetPoint(this.m_currencyFrame, Anchor.RIGHT, parent, Anchor.LEFT, offset, false);
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(true);
        }
    }

    public void SetCurrencyType(CurrencyFrame.CurrencyType? type)
    {
        this.m_currencyFrame.SetCurrencyOverride(type);
    }

    public void ShowFriendList()
    {
        ChatMgr.Get().ShowFriendsList();
        this.m_hasUnacknowledgedPendingInvites = false;
        this.m_friendButton.ShowPendingInvitesIcon(this.m_hasUnacknowledgedPendingInvites);
    }

    private void ShowGameMenu(string name, GameObject go, object callbackData)
    {
        this.m_gameMenu = go.GetComponent<GameMenu>();
        this.m_gameMenu.GetComponent<GameMenu>().Show();
        this.m_gameMenuLoading = false;
    }

    private static void SpectatorCount_OnRollout(UIEvent evt)
    {
        BnetBar bar = Get();
        if (bar != null)
        {
            bar.m_spectatorCountTooltipZone.HideTooltip();
        }
    }

    private static void SpectatorCount_OnRollover(UIEvent evt)
    {
        BnetBar bar = Get();
        if (bar != null)
        {
            string str2;
            string headline = GameStrings.Get("GLOBAL_SPECTATOR_COUNT_PANEL_HEADER");
            BnetGameAccountId[] spectatorPartyMembers = SpectatorManager.Get().GetSpectatorPartyMembers(true, false);
            if (spectatorPartyMembers.Length == 1)
            {
                string playerBestName = BnetUtils.GetPlayerBestName(spectatorPartyMembers[0]);
                object[] args = new object[] { playerBestName };
                str2 = GameStrings.Format("GLOBAL_SPECTATOR_COUNT_PANEL_TEXT_ONE", args);
            }
            else
            {
                if (<>f__am$cache20 == null)
                {
                    <>f__am$cache20 = id => BnetUtils.GetPlayerBestName(id);
                }
                string[] strArray = Enumerable.Select<BnetGameAccountId, string>(spectatorPartyMembers, <>f__am$cache20).ToArray<string>();
                str2 = string.Join(", ", strArray);
            }
            bar.m_spectatorCountTooltipZone.ShowSocialTooltip(bar.m_spectatorCountPanel, headline, str2, 75f, GameLayer.BattleNetDialog);
            bar.m_spectatorCountTooltipZone.AnchorTooltipTo(bar.m_spectatorCountPanel, Anchor.TOP_LEFT, Anchor.BOTTOM_LEFT);
        }
    }

    private void SpectatorManager_OnInviteReceived(OnlineEventType evt, BnetPlayer inviter)
    {
        if (ChatMgr.Get().IsFriendListShowing() || !SpectatorManager.Get().HasAnyReceivedInvites())
        {
            this.m_hasUnacknowledgedPendingInvites = false;
        }
        else
        {
            this.m_hasUnacknowledgedPendingInvites = this.m_hasUnacknowledgedPendingInvites || (evt == OnlineEventType.ADDED);
        }
        if (this.m_friendButton != null)
        {
            this.m_friendButton.ShowPendingInvitesIcon(this.m_hasUnacknowledgedPendingInvites);
        }
    }

    private void SpectatorManager_OnSpectatorModeChanged(OnlineEventType evt, BnetPlayer spectatee)
    {
        <SpectatorManager_OnSpectatorModeChanged>c__AnonStorey2AC storeyac = new <SpectatorManager_OnSpectatorModeChanged>c__AnonStorey2AC {
            evt = evt,
            spectatee = spectatee
        };
        if ((storeyac.evt == OnlineEventType.ADDED) && (this.m_spectatorModeIndicator == null))
        {
            string name = FileUtils.GameAssetPathToName(this.m_spectatorModeIndicatorPrefab);
            AssetLoader.Get().LoadGameObject(name, new AssetLoader.GameObjectCallback(storeyac.<>m__48), null, false);
        }
        else if (this.m_spectatorModeIndicator != null)
        {
            bool flag = (storeyac.evt == OnlineEventType.ADDED) && SpectatorManager.Get().IsInSpectatorMode();
            if (((UniversalInputManager.UsePhoneUI != null) && (SceneMgr.Get() != null)) && !SceneMgr.Get().IsInGame())
            {
                flag = false;
            }
            this.m_spectatorModeIndicator.SetActive(flag);
            this.UpdateLayout();
        }
    }

    private void SpectatorManager_OnSpectatorToMyGame(OnlineEventType evt, BnetPlayer spectator)
    {
        <SpectatorManager_OnSpectatorToMyGame>c__AnonStorey2AB storeyab = new <SpectatorManager_OnSpectatorToMyGame>c__AnonStorey2AB {
            evt = evt,
            spectator = spectator
        };
        int countSpectatingMe = SpectatorManager.Get().GetCountSpectatingMe();
        if (countSpectatingMe <= 0)
        {
            if (this.m_spectatorCountPanel == null)
            {
                return;
            }
        }
        else if (this.m_spectatorCountPanel == null)
        {
            string name = FileUtils.GameAssetPathToName(this.m_spectatorCountPrefabPath);
            AssetLoader.Get().LoadGameObject(name, new AssetLoader.GameObjectCallback(storeyab.<>m__45), null, false);
            return;
        }
        this.m_spectatorCountPanel.transform.FindChild("UberText").GetComponent<UberText>().Text = countSpectatingMe.ToString();
        bool flag = countSpectatingMe > 0;
        if (((UniversalInputManager.UsePhoneUI != null) && (SceneMgr.Get() != null)) && !SceneMgr.Get().IsInGame())
        {
            flag = false;
        }
        this.m_spectatorCountPanel.SetActive(flag);
        this.UpdateLayout();
        GameObject gameObject = this.m_spectatorCountPanel.transform.FindChild("BeingWatchedHighlight").gameObject;
        iTween.Stop(gameObject, true);
        if (<>f__am$cache1F == null)
        {
            <>f__am$cache1F = delegate (object ud) {
                if (Get() != null)
                {
                    iTween.FadeTo(Get().m_spectatorCountPanel.transform.FindChild("BeingWatchedHighlight").gameObject, 0f, 0.5f);
                }
            };
        }
        Action<object> action = <>f__am$cache1F;
        object[] args = new object[] { "alpha", 1f, "time", 0.5f, "oncomplete", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(gameObject, hashtable);
    }

    private void Start()
    {
        this.m_friendButton.gameObject.SetActive(false);
        this.m_hasUnacknowledgedPendingInvites = SpectatorManager.Get().HasAnyReceivedInvites();
        if (this.m_friendButton != null)
        {
            this.m_friendButton.ShowPendingInvitesIcon(this.m_hasUnacknowledgedPendingInvites);
        }
        this.ToggleActive(false);
    }

    public void SuppressLoginTooltip(bool val)
    {
        this.m_suppressLoginTooltip = val;
        this.UpdateLayout();
    }

    public void ToggleActive(bool active)
    {
        base.gameObject.SetActive(active);
    }

    public void ToggleEnableButtons(bool enabled)
    {
        this.m_menuButton.SetEnabled(enabled);
        this.m_friendButton.SetEnabled(enabled);
    }

    private void ToggleFriendListShowing()
    {
        if (ChatMgr.Get().IsFriendListShowing())
        {
            this.HideFriendList();
        }
        else
        {
            this.ShowFriendList();
        }
        this.m_friendButton.HideTooltip();
    }

    public void ToggleFriendsButton(bool enabled)
    {
        this.m_friendButton.gameObject.SetActive(enabled);
    }

    public void ToggleGameMenu()
    {
        if (this.m_gameMenu == null)
        {
            if (!this.m_gameMenuLoading)
            {
                this.m_gameMenuLoading = true;
                AssetLoader.Get().LoadGameObject("GameMenu", new AssetLoader.GameObjectCallback(this.ShowGameMenu), null, false);
            }
        }
        else if (this.m_gameMenu.IsShown())
        {
            this.m_gameMenu.Hide();
        }
        else
        {
            this.m_gameMenu.Show();
        }
    }

    private void Update()
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if ((realtimeSinceStartup - this.m_lastClockUpdate) > 1f)
        {
            this.m_lastClockUpdate = realtimeSinceStartup;
            if (Localization.GetLocale() == Locale.enGB)
            {
                this.m_currentTime.Text = string.Format("{0:HH:mm}", DateTime.Now);
            }
            else
            {
                object[] args = new object[] { DateTime.Now };
                this.m_currentTime.Text = GameStrings.Format("GLOBAL_CURRENT_TIME", args);
            }
        }
    }

    private void UpdateForDemoMode()
    {
        if (DemoMgr.Get().IsExpoDemo())
        {
            SceneMgr.Mode mode = SceneMgr.Get().GetMode();
            bool flag = false;
            bool flag2 = true;
            switch (DemoMgr.Get().GetMode())
            {
                case DemoMode.PAX_EAST_2013:
                case DemoMode.BLIZZCON_2013:
                case DemoMode.BLIZZCON_2015:
                    flag = mode == SceneMgr.Mode.GAMEPLAY;
                    flag2 = false;
                    this.m_currencyFrame.gameObject.SetActive(false);
                    break;

                case DemoMode.BLIZZCON_2014:
                {
                    bool flag3 = mode != SceneMgr.Mode.FRIENDLY;
                    flag = flag3;
                    flag2 = flag3;
                    break;
                }
                case DemoMode.APPLE_STORE:
                    flag = flag2 = false;
                    break;

                default:
                    flag = (mode != SceneMgr.Mode.FRIENDLY) && (mode != SceneMgr.Mode.TOURNAMENT);
                    break;
            }
            switch (mode)
            {
                case SceneMgr.Mode.GAMEPLAY:
                case SceneMgr.Mode.TOURNAMENT:
                case SceneMgr.Mode.FRIENDLY:
                    flag2 = false;
                    break;
            }
            if (!flag)
            {
                this.m_menuButton.gameObject.SetActive(false);
            }
            if (!flag2)
            {
                this.m_friendButton.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateForPhone()
    {
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        bool flag = ((mode == SceneMgr.Mode.HUB) || (mode == SceneMgr.Mode.LOGIN)) || (mode == SceneMgr.Mode.GAMEPLAY);
        this.m_menuButton.gameObject.SetActive(flag);
    }

    public void UpdateLayout()
    {
        if (this.m_isLoggedIn)
        {
            float num = 0.5f;
            Bounds nearClipBounds = CameraUtils.GetNearClipBounds(PegUI.Get().orthographicUICam);
            float x = (nearClipBounds.size.x + num) / this.m_initialWidth;
            TransformUtil.SetLocalPosX(base.gameObject, (nearClipBounds.min.x - base.transform.parent.localPosition.x) - num);
            TransformUtil.SetLocalScaleX(base.gameObject, x);
            Vector3 zero = Vector3.zero;
            float num3 = -0.03f * x;
            if (GeneralUtils.IsDevelopmentBuildTextVisible())
            {
                num3 -= CameraUtils.ScreenToWorldDist(PegUI.Get().orthographicUICam, 115f);
            }
            float y = 1f * base.transform.localScale.y;
            bool flag = true;
            if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY) && !DemoMgr.Get().IsHubEscMenuEnabled())
            {
                flag = false;
            }
            this.m_menuButton.gameObject.SetActive(flag);
            TransformUtil.SetLocalScaleX(this.m_menuButton, this.m_initialMenuButtonScaleX / x);
            TransformUtil.SetPoint(this.m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(num3, y, 0f) - zero);
            float num5 = 1.5f;
            float num6 = -4f;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                num5 = 190f;
                num6 = 0f;
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                TransformUtil.SetPoint(this.m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(num3, y, 0f));
                TransformUtil.SetLocalPosX(this.m_menuButton, this.m_menuButton.transform.localPosition.x + 0.05f);
                TransformUtil.SetLocalPosY(this.m_menuButton, num5);
                this.m_batteryLevel.gameObject.SetActive(true);
                int nElements = 1 + (!this.m_connectionIndicator.IsVisible() ? 0 : 1);
                this.m_menuButton.SetPhoneStatusBarState(nElements);
                TransformUtil.SetLocalScaleX(this.m_currencyFrame, 2f / x);
                TransformUtil.SetLocalScaleY(this.m_currencyFrame, 0.4f);
                if (this.m_menuButton.gameObject.activeInHierarchy)
                {
                    this.PositionCurrencyFrame(this.m_batteryLevel.gameObject, new Vector3(-35f, num6, 0f));
                }
                else
                {
                    this.PositionCurrencyFrame(this.m_batteryLevel.gameObject, new Vector3(100f, num6, 0f));
                }
            }
            else
            {
                TransformUtil.SetPoint(this.m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(num3, y, 0f));
                TransformUtil.SetLocalScaleX(this.m_currencyFrame, 1f / x);
                this.PositionCurrencyFrame(this.m_menuButton.gameObject, new Vector3(-25f, -5f, 0f));
            }
            MultiSliceElement componentInChildren = this.m_currencyFrame.GetComponentInChildren<MultiSliceElement>();
            if (componentInChildren != null)
            {
                componentInChildren.UpdateSlices();
            }
            bool flag2 = ((this.m_spectatorCountPanel != null) && this.m_spectatorCountPanel.activeInHierarchy) && SpectatorManager.Get().IsBeingSpectated();
            bool flag3 = ((this.m_spectatorModeIndicator != null) && this.m_spectatorModeIndicator.activeInHierarchy) && SpectatorManager.Get().IsInSpectatorMode();
            if (((UniversalInputManager.UsePhoneUI != null) && (SceneMgr.Get() != null)) && !SceneMgr.Get().IsInGame())
            {
                flag2 = false;
                flag3 = false;
            }
            bool flag4 = flag2 || flag3;
            if (this.m_friendButton.gameObject.activeInHierarchy)
            {
                TransformUtil.SetLocalScaleX(this.m_friendButton, this.m_initialFriendButtonScaleX / x);
                TransformUtil.SetPoint(this.m_friendButton, Anchor.LEFT, base.gameObject, Anchor.LEFT, new Vector3(6f, 5f, 0f));
                TransformUtil.SetLocalScaleX(this.m_currentTime, 1f / x);
                TransformUtil.SetLocalScaleX(this.m_socialToastBone, 1f / x);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    TransformUtil.SetLocalPosY(this.m_friendButton, num5);
                }
                if (!flag4)
                {
                    TransformUtil.SetPoint((Component) this.m_currentTime, Anchor.LEFT, (Component) this.m_friendButton, Anchor.RIGHT, new Vector3(22f, num6, 0f) + zero);
                    TransformUtil.SetPoint(this.m_socialToastBone, Anchor.LEFT, this.m_friendButton, Anchor.RIGHT, new Vector3(15f, 0f, -1f) + zero);
                }
                else if (flag2)
                {
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        TransformUtil.SetPoint(this.m_spectatorCountPanel, Anchor.LEFT, base.gameObject, Anchor.LEFT, new Vector3(6f, 5f, 0f));
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.m_spectatorCountPanel, Anchor.LEFT, this.m_friendButton, Anchor.RIGHT, new Vector3(8f, 0f, 0f) + zero);
                        TransformUtil.SetPoint(this.m_socialToastBone, Anchor.LEFT, this.m_spectatorCountPanel, Anchor.RIGHT, new Vector3(7f, 0f, -1f) + zero);
                    }
                    TransformUtil.SetPoint(this.m_currentTime, Anchor.LEFT, this.m_spectatorCountPanel, Anchor.RIGHT, new Vector3(14f, -4f, 0f));
                }
                else if (flag3)
                {
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        TransformUtil.SetPoint(this.m_spectatorModeIndicator, Anchor.LEFT, base.gameObject, Anchor.LEFT, new Vector3(6f, 5f, 0f));
                    }
                    else
                    {
                        TransformUtil.SetPoint(this.m_spectatorModeIndicator, Anchor.LEFT, this.m_friendButton, Anchor.RIGHT, new Vector3(8f, 0f, 0f) + zero);
                        TransformUtil.SetPoint(this.m_socialToastBone, Anchor.LEFT, this.m_spectatorModeIndicator, Anchor.RIGHT, new Vector3(7f, 0f, -1f) + zero);
                    }
                    TransformUtil.SetPoint(this.m_currentTime, Anchor.LEFT, this.m_spectatorModeIndicator, Anchor.RIGHT, new Vector3(14f, -4f, 0f));
                }
                float num8 = 1f;
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    num8 = 2.5f;
                }
                TransformUtil.SetLocalScaleX(this.m_questProgressToastBone, num8 / x);
            }
            else
            {
                GameObject gameObject = base.gameObject;
                if (flag4)
                {
                    TransformUtil.SetPoint(this.m_spectatorCountPanel, Anchor.LEFT, gameObject, Anchor.RIGHT, new Vector3(0f, 0f, 0f));
                    gameObject = this.m_spectatorCountPanel;
                }
                else if (flag3)
                {
                    TransformUtil.SetPoint(this.m_spectatorModeIndicator, Anchor.LEFT, gameObject, Anchor.RIGHT, new Vector3(0f, 0f, 0f) + zero);
                    gameObject = this.m_spectatorModeIndicator;
                }
                TransformUtil.SetLocalScaleX(this.m_currentTime, 1f / x);
                TransformUtil.SetPoint(this.m_currentTime, Anchor.LEFT, base.gameObject, Anchor.LEFT, new Vector3(6f, 5f, 0f) + zero);
            }
            this.UpdateLoginTooltip();
            if (this.m_isInitting)
            {
                this.m_currencyFrame.DeactivateCurrencyFrame();
                this.m_isInitting = false;
            }
        }
    }

    public void UpdateLoginTooltip()
    {
        if ((((!Network.ShouldBeConnectedToAurora() && !this.m_suppressLoginTooltip) && (SceneMgr.Get().IsInGame() && GameMgr.Get().IsTutorial())) && !GameMgr.Get().IsSpectator()) && (DemoMgr.Get().GetMode() != DemoMode.APPLE_STORE))
        {
            if (this.m_loginTooltip == null)
            {
                this.m_loginTooltip = AssetLoader.Get().LoadGameObject("LoginPointer", true, false);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.m_loginTooltip.transform.localScale = new Vector3(60f, 60f, 60f);
                }
                else
                {
                    this.m_loginTooltip.transform.localScale = new Vector3(40f, 40f, 40f);
                }
                TransformUtil.SetEulerAngleX(this.m_loginTooltip, 270f);
                SceneUtils.SetLayer(this.m_loginTooltip, GameLayer.BattleNet);
                this.m_loginTooltip.transform.parent = base.transform;
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                TransformUtil.SetPoint(this.m_loginTooltip, Anchor.RIGHT, this.m_batteryLevel.gameObject, Anchor.LEFT, new Vector3(-32f, 0f, 0f));
            }
            else
            {
                TransformUtil.SetPoint(this.m_loginTooltip, Anchor.RIGHT, this.m_menuButton, Anchor.LEFT, new Vector3(5f, 0f, 0f));
            }
        }
        else
        {
            this.DestroyLoginTooltip();
        }
    }

    private void WillReset()
    {
        if (this.m_gameMenu != null)
        {
            if (this.m_gameMenu.IsShown())
            {
                this.m_gameMenu.Hide();
            }
            UnityEngine.Object.DestroyImmediate(this.m_gameMenu.gameObject);
            this.m_gameMenu = null;
        }
        this.DestroyLoginTooltip();
        this.ToggleActive(false);
        this.m_isLoggedIn = false;
    }

    [CompilerGenerated]
    private sealed class <SpectatorManager_OnSpectatorModeChanged>c__AnonStorey2AC
    {
        internal OnlineEventType evt;
        internal BnetPlayer spectatee;

        internal void <>m__48(string n, GameObject go, object d)
        {
            BnetBar bar = BnetBar.Get();
            if (bar != null)
            {
                if (bar.m_spectatorModeIndicator != null)
                {
                    UnityEngine.Object.Destroy(go);
                }
                else
                {
                    bar.m_spectatorModeIndicator = go;
                    bar.m_spectatorModeIndicator.transform.parent = bar.m_friendButton.transform;
                }
                BnetBar.Get().SpectatorManager_OnSpectatorModeChanged(this.evt, this.spectatee);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SpectatorManager_OnSpectatorToMyGame>c__AnonStorey2AB
    {
        internal OnlineEventType evt;
        internal BnetPlayer spectator;

        internal void <>m__45(string n, GameObject go, object d)
        {
            BnetBar bar = BnetBar.Get();
            if (bar != null)
            {
                if (bar.m_spectatorCountPanel != null)
                {
                    UnityEngine.Object.Destroy(go);
                }
                else
                {
                    bar.m_spectatorCountPanel = go;
                    bar.m_spectatorCountPanel.transform.parent = bar.m_friendButton.transform;
                    PegUIElement component = go.GetComponent<PegUIElement>();
                    if (component != null)
                    {
                        component.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(BnetBar.SpectatorCount_OnRollover));
                        component.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(BnetBar.SpectatorCount_OnRollout));
                    }
                    GameObject gameObject = bar.m_spectatorCountPanel.transform.FindChild("BeingWatchedHighlight").gameObject;
                    Color color = gameObject.GetComponent<Renderer>().material.color;
                    color.a = 0f;
                    gameObject.GetComponent<Renderer>().material.color = color;
                }
                BnetBar.Get().SpectatorManager_OnSpectatorToMyGame(this.evt, this.spectator);
            }
        }
    }
}

