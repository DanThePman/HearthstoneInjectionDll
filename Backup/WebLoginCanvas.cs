using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WebLoginCanvas : MonoBehaviour
{
    private PlatformDependentValue<bool> KOBOLD_SHOWN_ON_ACCOUNT_CREATION;
    public GameObject m_accountCreation;
    private float m_acFlipbookCur;
    private float m_acFlipbookSwap = 30f;
    public PegUIElement m_backButton;
    public GameObject m_bottomRightBone;
    private bool m_canGoBack;
    public AccountCreationFlipbook m_flipbook;
    private object m_prevSelection;
    public UIBButton m_regionButton;
    private RegionMenu m_regionMenu;
    private Map<Network.BnetRegion, string> m_regionNames;
    public GameObject m_regionSelectContents;
    public GameObject m_regionSelectDropdownBone;
    public DropdownControl m_regionSelectDropdownPrefab;
    private DropdownControl m_regionSelector;
    private KeywordHelpPanel m_regionSelectTooltip;
    public GameObject m_regionSelectTooltipBone;
    private object m_selection;
    public GameObject m_topLeftBone;
    private static Map<Network.BnetRegion, string> s_regionStringNames;
    private PlatformDependentValue<bool> USE_REGION_DROPDOWN;

    static WebLoginCanvas()
    {
        Map<Network.BnetRegion, string> map = new Map<Network.BnetRegion, string>();
        map.Add(Network.BnetRegion.REGION_UNKNOWN, "Cuba");
        map.Add(Network.BnetRegion.REGION_US, "GLOBAL_REGION_AMERICAS");
        map.Add(Network.BnetRegion.REGION_EU, "GLOBAL_REGION_EUROPE");
        map.Add(Network.BnetRegion.REGION_KR, "GLOBAL_REGION_ASIA");
        map.Add(Network.BnetRegion.REGION_TW, "Taiwan");
        map.Add(Network.BnetRegion.REGION_CN, "GLOBAL_REGION_CHINA");
        map.Add(Network.BnetRegion.REGION_LIVE_VERIFICATION, "LiveVerif");
        s_regionStringNames = map;
    }

    public WebLoginCanvas()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            PC = true,
            Tablet = true,
            Phone = false
        };
        this.KOBOLD_SHOWN_ON_ACCOUNT_CREATION = value2;
        value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            PC = true,
            Tablet = true,
            Phone = false
        };
        this.USE_REGION_DROPDOWN = value2;
    }

    private void AddButtonForRegion(List<UIBButton> buttons, Network.BnetRegion region)
    {
        <AddButtonForRegion>c__AnonStorey32B storeyb = new <AddButtonForRegion>c__AnonStorey32B {
            region = region,
            <>f__this = this,
            currentRegion = MobileDeviceLocale.GetCurrentRegionId()
        };
        buttons.Add(this.m_regionMenu.CreateMenuButton(null, this.onRegionText(storeyb.region), new UIEvent.Handler(storeyb.<>m__160)));
    }

    private void Awake()
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        bool flag = ApplicationMgr.GetMobileEnvironment() == MobileEnv.DEVELOPMENT;
        this.m_regionNames = new Map<Network.BnetRegion, string>();
        foreach (Network.BnetRegion region in s_regionStringNames.Keys)
        {
            if (flag)
            {
                char[] separator = new char[] { ' ' };
                this.m_regionNames[region] = GameStrings.Get(s_regionStringNames[region]).Split(separator)[0];
            }
            else
            {
                this.m_regionNames[region] = GameStrings.Get(s_regionStringNames[region]);
            }
        }
        if (this.USE_REGION_DROPDOWN != null)
        {
            this.SetUpRegionDropdown();
        }
        else
        {
            this.SetUpRegionButton();
        }
        if ((UniversalInputManager.UsePhoneUI != null) && flag)
        {
            this.SetUpRegionDropdown();
        }
        this.m_backButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBackPressed));
        if (!Options.Get().GetBool(Option.CONNECT_TO_AURORA))
        {
            this.m_backButton.gameObject.SetActive(true);
        }
        Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
    }

    private void CauseReconnect()
    {
        WebAuth.ClearLoginData();
        BattleNet.RequestCloseAurora();
        ApplicationMgr.Get().ResetAndForceLogin();
    }

    private void OnBackPressed(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void OnDestroy()
    {
        if (this.m_regionSelectTooltip != null)
        {
            UnityEngine.Object.Destroy(this.m_regionSelectTooltip.gameObject);
        }
    }

    private bool OnDialogProcess(DialogBase dialog, object userData)
    {
        GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_regionSelectContents);
        TransformUtil.AttachAndPreserveLocalTransform(obj2.transform, dialog.transform);
        obj2.SetActive(true);
        return true;
    }

    private void onMenuShown(bool shown)
    {
        if (shown)
        {
            this.m_regionSelectTooltip.gameObject.SetActive(true);
            WebAuth.UpdateRegionSelectVisualState(true);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                SplashScreen.Get().HideWebAuth();
            }
        }
        else
        {
            this.m_regionSelectTooltip.gameObject.SetActive(false);
            WebAuth.UpdateRegionSelectVisualState(false);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                SplashScreen.Get().UnHideWebAuth();
            }
        }
    }

    private bool OnNavigateBack()
    {
        if (this.m_canGoBack)
        {
            WebAuth.GoBackWebPage();
        }
        else if (!Options.Get().GetBool(Option.CONNECT_TO_AURORA))
        {
            ApplicationMgr.Get().Reset();
            return true;
        }
        return false;
    }

    private void onRegionChange(object selection, object prevSelection)
    {
        if (selection != prevSelection)
        {
            Network.BnetRegion region = (Network.BnetRegion) ((int) selection);
            Options.Get().SetInt(Option.PREFERRED_REGION, (int) region);
            this.CauseReconnect();
        }
    }

    private void onRegionChangeCB(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CONFIRM)
        {
            this.onRegionChange(this.m_selection, this.m_prevSelection);
        }
        else
        {
            if (this.m_regionSelector != null)
            {
                this.m_regionSelector.setSelection(this.m_prevSelection);
            }
            SplashScreen.Get().UnHideWebAuth();
        }
        this.m_regionSelectContents.SetActive(false);
    }

    private string onRegionText(object val)
    {
        Network.BnetRegion key = (Network.BnetRegion) ((int) val);
        string str = string.Empty;
        this.m_regionNames.TryGetValue(key, out str);
        if (ApplicationMgr.GetMobileEnvironment() != MobileEnv.DEVELOPMENT)
        {
            return str;
        }
        MobileDeviceLocale.ConnectionData connectionDataFromRegionId = MobileDeviceLocale.GetConnectionDataFromRegionId(key, true);
        string name = connectionDataFromRegionId.name;
        if (string.IsNullOrEmpty(name))
        {
            char[] separator = new char[] { '-' };
            name = string.Format("{0}:{1}:{2}", connectionDataFromRegionId.address.Split(separator)[0], connectionDataFromRegionId.port, connectionDataFromRegionId.version);
        }
        if (string.IsNullOrEmpty(str))
        {
            return name;
        }
        return string.Format("{0} ({1})", name, str);
    }

    private void onRegionWarning(object selection, object prevSelection)
    {
        this.m_selection = selection;
        this.m_prevSelection = prevSelection;
        if (!selection.Equals(prevSelection))
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLUE_MOBILE_REGION_SELECT_WARNING_HEADER"),
                m_text = GameStrings.Get("GLUE_MOBILE_REGION_SELECT_WARNING"),
                m_showAlertIcon = false,
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_responseCallback = new AlertPopup.ResponseCallback(this.onRegionChangeCB),
                m_padding = 60f
            };
            if (UniversalInputManager.UsePhoneUI != null)
            {
                info.m_padding = 80f;
            }
            info.m_scaleOverride = new Vector3(300f, 300f, 300f);
            SplashScreen.Get().HideWebAuth();
            DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnDialogProcess));
        }
    }

    private void SetUpRegionButton()
    {
        if (this.m_regionButton != null)
        {
            this.m_regionButton.AddEventListener(UIEventType.RELEASE, e => this.ShowRegionMenu());
            string text = this.onRegionText(MobileDeviceLocale.GetCurrentRegionId());
            this.m_regionButton.SetText(text);
        }
    }

    private void SetUpRegionDropdown()
    {
        bool flag = ApplicationMgr.GetMobileEnvironment() == MobileEnv.DEVELOPMENT;
        this.m_regionSelector = UnityEngine.Object.Instantiate<DropdownControl>(this.m_regionSelectDropdownPrefab);
        this.m_regionSelector.transform.parent = base.gameObject.transform;
        TransformUtil.CopyLocal((Component) this.m_regionSelector.transform, (Component) this.m_regionSelectDropdownBone.transform);
        this.m_regionSelector.clearItems();
        this.m_regionSelector.setItemTextCallback(new DropdownControl.itemTextCallback(this.onRegionText));
        this.m_regionSelector.setMenuShownCallback(new DropdownControl.menuShownCallback(this.onMenuShown));
        this.m_regionSelector.setItemChosenCallback(new DropdownControl.itemChosenCallback(this.onRegionWarning));
        if (flag)
        {
            foreach (Network.BnetRegion region in MobileDeviceLocale.s_regionIdToDevIP.Keys)
            {
                this.m_regionSelector.addItem(region);
            }
        }
        else
        {
            this.m_regionSelector.addItem(Network.BnetRegion.REGION_US);
            this.m_regionSelector.addItem(Network.BnetRegion.REGION_EU);
            this.m_regionSelector.addItem(Network.BnetRegion.REGION_KR);
        }
        Network.BnetRegion currentRegionId = MobileDeviceLocale.GetCurrentRegionId();
        this.m_regionSelector.setSelection(currentRegionId);
        if (MobileDeviceLocale.UseClientConfigForEnv())
        {
            this.m_regionSelector.gameObject.SetActive(false);
        }
        this.m_regionSelectTooltip = KeywordHelpPanelManager.Get().CreateKeywordPanel(0);
        this.m_regionSelectTooltip.Reset();
        this.m_regionSelectTooltip.Initialize(GameStrings.Get("GLUE_MOBILE_REGION_SELECT_TOOLTIP_HEADER"), GameStrings.Get("GLUE_MOBILE_REGION_SELECT_TOOLTIP"));
        this.m_regionSelectTooltip.transform.position = this.m_regionSelectTooltipBone.transform.position;
        this.m_regionSelectTooltip.transform.localScale = this.m_regionSelectTooltipBone.transform.localScale;
        this.m_regionSelectTooltip.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        SceneUtils.SetLayer(this.m_regionSelectTooltip.gameObject, GameLayer.UI);
        this.m_regionSelectTooltip.gameObject.SetActive(false);
    }

    private void ShowRegionMenu()
    {
        if (this.m_regionMenu != null)
        {
            this.m_regionMenu.Show();
        }
        else
        {
            this.m_regionMenu = ((GameObject) GameUtils.InstantiateGameObject("RegionMenu", null, false)).GetComponent<RegionMenu>();
            List<UIBButton> buttons = new List<UIBButton>();
            Debug.Log("creating region menu..");
            this.AddButtonForRegion(buttons, Network.BnetRegion.REGION_US);
            this.AddButtonForRegion(buttons, Network.BnetRegion.REGION_EU);
            this.AddButtonForRegion(buttons, Network.BnetRegion.REGION_KR);
            this.m_regionMenu.SetButtons(buttons);
            this.m_regionMenu.Show();
        }
    }

    private void Update()
    {
        if (this.KOBOLD_SHOWN_ON_ACCOUNT_CREATION != null)
        {
            if (this.m_acFlipbookCur < this.m_acFlipbookSwap)
            {
                this.m_acFlipbookCur += UnityEngine.Time.deltaTime * 60f;
            }
            else
            {
                if (this.m_flipbook.m_acFlipbook.GetComponent<Renderer>().sharedMaterial.mainTexture == this.m_flipbook.m_acFlipbookTextures[0])
                {
                    this.m_flipbook.m_acFlipbook.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_flipbook.m_acFlipbookTextures[1];
                    this.m_acFlipbookSwap = this.m_flipbook.m_acFlipbookTimeAlt;
                }
                else
                {
                    this.m_flipbook.m_acFlipbook.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_flipbook.m_acFlipbookTextures[0];
                    this.m_acFlipbookSwap = UnityEngine.Random.Range(this.m_flipbook.m_acFlipbookTimeMin, this.m_flipbook.m_acFlipbookTimeMax);
                }
                this.m_acFlipbookCur = 0f;
            }
        }
    }

    public void WebViewBackButtonPressed(string dummyState)
    {
        Navigation.GoBack();
    }

    public void WebViewDidFinishLoad(string pageState)
    {
        Debug.Log("web view page state: " + pageState);
        if (pageState != null)
        {
            string[] separator = new string[] { "|" };
            string[] strArray = pageState.Split(separator, StringSplitOptions.None);
            if (strArray.Length < 2)
            {
                Debug.LogWarning(string.Format("WebViewDidFinishLoad() - Invalid parsed pageState ({0})", pageState));
            }
            else
            {
                this.m_canGoBack = strArray[strArray.Length - 1].Equals("canGoBack");
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                for (int i = 0; i < (strArray.Length - 1); i++)
                {
                    string str = strArray[i];
                    if (str.Equals("STATE_ACCOUNT_CREATION", StringComparison.InvariantCultureIgnoreCase))
                    {
                        flag = true;
                    }
                    if (str.Equals("STATE_ACCOUNT_CREATED", StringComparison.InvariantCultureIgnoreCase))
                    {
                        flag2 = true;
                    }
                    if (str.Equals("STATE_NO_BACK", StringComparison.InvariantCultureIgnoreCase))
                    {
                        flag3 = true;
                    }
                }
                if (this.KOBOLD_SHOWN_ON_ACCOUNT_CREATION != null)
                {
                    this.m_accountCreation.SetActive(flag);
                }
                flag3 |= flag2;
                if (flag2)
                {
                    WebAuth.SetIsNewCreatedAccount(true);
                }
                this.m_backButton.gameObject.SetActive(!flag3 && (this.m_canGoBack || !Options.Get().GetBool(Option.CONNECT_TO_AURORA)));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AddButtonForRegion>c__AnonStorey32B
    {
        internal WebLoginCanvas <>f__this;
        internal Network.BnetRegion currentRegion;
        internal Network.BnetRegion region;

        internal void <>m__160(UIEvent e)
        {
            this.<>f__this.m_regionMenu.Hide();
            this.<>f__this.onRegionWarning(this.region, this.currentRegion);
        }
    }
}

