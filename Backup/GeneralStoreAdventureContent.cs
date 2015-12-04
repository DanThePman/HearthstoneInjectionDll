using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreAdventureContent : GeneralStoreContent
{
    [CustomEditField(Sections="Rewards")]
    public List<GameObject> m_adventureCardPreviewBones;
    [CustomEditField(Sections="Rewards")]
    public PegUIElement m_adventureCardPreviewOffClicker;
    [CustomEditField(Sections="Rewards")]
    public GameObject m_adventureCardPreviewPanel;
    [CustomEditField(Sections="Rewards")]
    public UberText m_adventureCardPreviewText;
    [CustomEditField(Sections="General Store")]
    public GeneralStoreAdventureContentDisplay m_adventureDisplay;
    private GeneralStoreAdventureContentDisplay m_adventureDisplay1;
    private GeneralStoreAdventureContentDisplay m_adventureDisplay2;
    [CustomEditField(Sections="General Store")]
    public GameObject m_adventureEmptyDisplay;
    [CustomEditField(Sections="General Store/Buttons")]
    public GameObject m_adventureOwnedCheckmark;
    [CustomEditField(Sections="General Store/Buttons")]
    public RadioButton m_adventureRadioButton;
    [CustomEditField(Sections="General Store/Buttons")]
    public GameObject m_adventureRadioButtonContainer;
    [CustomEditField(Sections="General Store/Buttons")]
    public UberText m_adventureRadioButtonCostText;
    [CustomEditField(Sections="General Store/Buttons")]
    public UberText m_adventureRadioButtonText;
    [CustomEditField(Sections="Animation")]
    public float m_backgroundFlipAnimTime = 0.5f;
    [CustomEditField(Sections="Sounds & Music", T=EditType.SOUND_PREFAB)]
    public string m_backgroundFlipSound;
    private int m_currentDisplay = -1;
    private Map<string, Actor> m_loadedPreviewCards = new Map<string, Actor>();
    [CustomEditField(Sections="Animation/Preorder")]
    public GeneralStoreRewardsCardBack m_preorderCardBackReward;
    private AdventureDbId m_selectedAdventureType;
    private bool m_showPreviewCards;
    private Map<int, StoreAdventureDef> m_storeAdvDefs = new Map<int, StoreAdventureDef>();

    private void AnimateAdventureRadioButtonBar()
    {
        if (this.m_adventureRadioButtonContainer != null)
        {
            this.m_adventureRadioButtonContainer.SetActive(false);
            if (this.m_selectedAdventureType != AdventureDbId.INVALID)
            {
                iTween.Stop(this.m_adventureRadioButtonContainer);
                this.m_adventureRadioButtonContainer.transform.localRotation = Quaternion.identity;
                this.m_adventureRadioButtonContainer.SetActive(true);
                this.m_adventureRadioButton.SetSelected(true);
                object[] args = new object[] { "amount", new Vector3(-1f, 0f, 0f), "time", this.m_backgroundFlipAnimTime, "delay", 0.001f };
                iTween.RotateBy(this.m_adventureRadioButtonContainer, iTween.Hash(args));
            }
        }
    }

    private void AnimateAndUpdateDisplay(int id, bool forceImmediate)
    {
        <AnimateAndUpdateDisplay>c__AnonStorey33A storeya = new <AnimateAndUpdateDisplay>c__AnonStorey33A();
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.HideCardBackReward();
        }
        storeya.currDisplay = null;
        GameObject target = null;
        if (this.m_currentDisplay == -1)
        {
            this.m_currentDisplay = 1;
            storeya.currDisplay = this.m_adventureEmptyDisplay;
        }
        else
        {
            storeya.currDisplay = this.GetCurrentDisplayContainer();
        }
        target = this.GetNextDisplayContainer();
        this.m_currentDisplay = (this.m_currentDisplay + 1) % 2;
        target.SetActive(true);
        if (!forceImmediate)
        {
            storeya.currDisplay.transform.localRotation = Quaternion.identity;
            target.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            iTween.StopByName(storeya.currDisplay, "ROTATION_TWEEN");
            iTween.StopByName(target, "ROTATION_TWEEN");
            object[] args = new object[] { "amount", new Vector3(0.5f, 0f, 0f), "time", 0.5f, "name", "ROTATION_TWEEN", "oncomplete", new Action<object>(storeya.<>m__176) };
            iTween.RotateBy(storeya.currDisplay, iTween.Hash(args));
            object[] objArray2 = new object[] { "amount", new Vector3(0.5f, 0f, 0f), "time", 0.5f, "name", "ROTATION_TWEEN" };
            iTween.RotateBy(target, iTween.Hash(objArray2));
            if (!string.IsNullOrEmpty(this.m_backgroundFlipSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_backgroundFlipSound));
            }
        }
        else
        {
            target.transform.localRotation = Quaternion.identity;
            storeya.currDisplay.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            storeya.currDisplay.SetActive(false);
        }
        bool preorder = this.IsPreOrder();
        StoreAdventureDef storeAdventureDef = this.GetStoreAdventureDef(id);
        GeneralStoreAdventureContentDisplay currentDisplay = this.GetCurrentDisplay();
        currentDisplay.UpdateAdventureType(storeAdventureDef);
        currentDisplay.SetPreOrder(preorder);
        if ((this.m_preorderCardBackReward != null) && preorder)
        {
            this.m_preorderCardBackReward.SetCardBack(storeAdventureDef.m_preorderCardBackId);
            this.m_preorderCardBackReward.ShowCardBackReward();
        }
    }

    public override bool AnimateEntranceEnd()
    {
        this.m_adventureRadioButton.gameObject.SetActive(true);
        return true;
    }

    public override bool AnimateExitEnd()
    {
        return true;
    }

    public override bool AnimateExitStart()
    {
        this.m_adventureRadioButton.gameObject.SetActive(false);
        return true;
    }

    private void Awake()
    {
        this.m_adventureDisplay1 = this.m_adventureDisplay;
        this.m_adventureDisplay2 = UnityEngine.Object.Instantiate<GeneralStoreAdventureContentDisplay>(this.m_adventureDisplay);
        this.m_adventureDisplay2.transform.parent = this.m_adventureDisplay1.transform.parent;
        this.m_adventureDisplay2.transform.localPosition = this.m_adventureDisplay1.transform.localPosition;
        this.m_adventureDisplay2.transform.localScale = this.m_adventureDisplay1.transform.localScale;
        this.m_adventureDisplay2.transform.localRotation = this.m_adventureDisplay1.transform.localRotation;
        this.m_adventureDisplay2.gameObject.SetActive(false);
        if (this.m_adventureDisplay1.m_rewardChest != null)
        {
            this.m_adventureDisplay1.m_rewardChest.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnAdventuresShowPreviewCard));
            this.m_adventureDisplay2.m_rewardChest.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnAdventuresShowPreviewCard));
            if (UniversalInputManager.UsePhoneUI == null)
            {
                this.m_adventureDisplay1.m_rewardChest.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnAdventuresHidePreviewCard));
                this.m_adventureDisplay2.m_rewardChest.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnAdventuresHidePreviewCard));
            }
        }
        AdventureProgressMgr.Get().RegisterProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(this.OnAdventureProgressUpdated));
        this.m_adventureCardPreviewPanel.SetActive(false);
        base.m_parentStore.SetChooseDescription(GameStrings.Get("GLUE_STORE_CHOOSE_ADVENTURE"));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_adventureCardPreviewOffClicker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnAdventuresHidePreviewCard));
        }
        foreach (DbfRecord record in GameUtils.GetAdventureRecordsWithStorePrefab())
        {
            string assetPath = record.GetAssetPath("STORE_PREFAB");
            GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetPath), true, false);
            if (obj2 != null)
            {
                StoreAdventureDef component = obj2.GetComponent<StoreAdventureDef>();
                if (component == null)
                {
                    Debug.LogError(string.Format("StoreAdventureDef not found in object: {0}", assetPath));
                }
                else
                {
                    this.m_storeAdvDefs.Add(record.GetId(), component);
                }
            }
        }
    }

    public AdventureDbId GetAdventureType()
    {
        return this.m_selectedAdventureType;
    }

    private GeneralStoreAdventureContentDisplay GetCurrentDisplay()
    {
        return ((this.m_currentDisplay != 0) ? this.m_adventureDisplay2 : this.m_adventureDisplay1);
    }

    private GameObject GetCurrentDisplayContainer()
    {
        return this.GetCurrentDisplay().gameObject;
    }

    public override string GetMoneyDisplayOwnedText()
    {
        return GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_COST_OWNED_TEXT");
    }

    private GameObject GetNextDisplayContainer()
    {
        return ((((this.m_currentDisplay + 1) % 2) != 0) ? this.m_adventureDisplay2.gameObject : this.m_adventureDisplay1.gameObject);
    }

    public StoreAdventureDef GetStoreAdventureDef(int advId)
    {
        StoreAdventureDef def = null;
        this.m_storeAdvDefs.TryGetValue(advId, out def);
        return def;
    }

    public Map<int, StoreAdventureDef> GetStoreAdventureDefs()
    {
        return this.m_storeAdvDefs;
    }

    private void HidePreviewCardPanel()
    {
        iTween.StopByName(this.m_adventureCardPreviewPanel, "PreviewCardPanelScale");
        object[] args = new object[] { "scale", new Vector3(0.02f, 0.02f, 0.02f), "time", 0.1f, "name", "PreviewCardPanelScale", "oncomplete", delegate (object o) {
            this.m_adventureCardPreviewPanel.SetActive(false);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                base.m_parentStore.ActivateCover(false);
            }
        }, "easetype", iTween.EaseType.linear };
        iTween.ScaleTo(this.m_adventureCardPreviewPanel, iTween.Hash(args));
    }

    private bool IsPreOrder()
    {
        Network.Bundle currentMoneyBundle = base.GetCurrentMoneyBundle();
        return ((currentMoneyBundle != null) && currentMoneyBundle.IsPreOrder());
    }

    public override bool IsPurchaseDisabled()
    {
        return (this.m_selectedAdventureType == AdventureDbId.INVALID);
    }

    private void LoadAdventurePreviewCard(string previewCard, DelOnAdventurePreviewCardLoaded onLoadComplete)
    {
        <LoadAdventurePreviewCard>c__AnonStorey338 storey = new <LoadAdventurePreviewCard>c__AnonStorey338 {
            onLoadComplete = onLoadComplete,
            previewCard = previewCard,
            <>f__this = this
        };
        Actor actor = null;
        if (this.m_loadedPreviewCards.TryGetValue(storey.previewCard, out actor))
        {
            storey.onLoadComplete(actor);
        }
        else
        {
            DefLoader.Get().LoadFullDef(storey.previewCard, new DefLoader.LoadDefCallback<FullDef>(storey.<>m__174));
        }
    }

    private void OnAdventureProgressUpdated(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
    {
        if ((newProgress != null) && (((oldProgress == null) || !oldProgress.IsOwned()) && newProgress.IsOwned()))
        {
            DbfRecord record = GameDbf.Wing.GetRecord(newProgress.Wing);
            if ((record != null) && (record.GetInt("ADVENTURE_ID") == this.m_selectedAdventureType))
            {
                Network.Bundle bundle = null;
                StoreManager.Get().GetAvailableAdventureBundle(this.GetAdventureType(), out bundle);
                base.SetCurrentMoneyBundle(bundle, false);
                if (base.m_parentStore != null)
                {
                    base.m_parentStore.RefreshContent();
                }
            }
        }
    }

    private void OnAdventuresHidePreviewCard(UIEvent e)
    {
        this.m_showPreviewCards = false;
        this.HidePreviewCardPanel();
    }

    private void OnAdventuresShowPreviewCard(UIEvent e)
    {
        <OnAdventuresShowPreviewCard>c__AnonStorey337 storey = new <OnAdventuresShowPreviewCard>c__AnonStorey337 {
            <>f__this = this
        };
        StoreAdventureDef storeAdventureDef = this.GetStoreAdventureDef((int) this.m_selectedAdventureType);
        if (storeAdventureDef == null)
        {
            Debug.LogError(string.Format("Unable to find preview cards for {0} adventure.", this.m_selectedAdventureType));
        }
        else
        {
            storey.previewCards = storeAdventureDef.m_previewCards.ToArray();
            if (storey.previewCards.Length == 0)
            {
                Debug.LogError(string.Format("No preview cards defined for {0} adventure.", this.m_selectedAdventureType));
            }
            else
            {
                this.m_showPreviewCards = true;
                SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over");
                foreach (KeyValuePair<string, Actor> pair in this.m_loadedPreviewCards)
                {
                    pair.Value.gameObject.SetActive(false);
                }
                storey.loadedPreviewCards = 0;
                int num = 0;
                foreach (string str in storey.previewCards)
                {
                    <OnAdventuresShowPreviewCard>c__AnonStorey336 storey2 = new <OnAdventuresShowPreviewCard>c__AnonStorey336 {
                        <>f__ref$823 = storey,
                        <>f__this = this,
                        cardIndex = num
                    };
                    this.LoadAdventurePreviewCard(str, new DelOnAdventurePreviewCardLoaded(storey2.<>m__173));
                    num++;
                }
            }
        }
    }

    protected override void OnBundleChanged(NoGTAPPTransactionData goldBundle, Network.Bundle moneyBundle)
    {
        this.UpdateRadioButtonText(moneyBundle);
        this.UpdateAdventureDescription(moneyBundle);
    }

    private void OnDestroy()
    {
        AdventureProgressMgr.Get().RemoveProgressUpdatedListener(new AdventureProgressMgr.AdventureProgressUpdatedCallback(this.OnAdventureProgressUpdated));
    }

    protected override void OnRefresh()
    {
        Network.Bundle bundle = null;
        StoreManager.Get().GetAvailableAdventureBundle(this.m_selectedAdventureType, out bundle);
        base.SetCurrentMoneyBundle(bundle, false);
        this.UpdateRadioButtonText(bundle);
        this.UpdateAdventureDescription(bundle);
    }

    public override void PostStoreFlipIn(bool animateIn)
    {
        this.UpdateAdventureTypeMusic();
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.ShowCardBackReward();
        }
    }

    public override void PreStoreFlipIn()
    {
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.HideCardBackReward();
        }
    }

    public override void PreStoreFlipOut()
    {
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.HideCardBackReward();
        }
    }

    public void SetAdventureType(AdventureDbId adventure, bool forceImmediate = false)
    {
        if (this.m_selectedAdventureType != adventure)
        {
            this.m_selectedAdventureType = adventure;
            Network.Bundle bundle = null;
            StoreManager.Get().GetAvailableAdventureBundle(this.m_selectedAdventureType, out bundle);
            base.SetCurrentMoneyBundle(bundle, false);
            this.AnimateAndUpdateDisplay((int) adventure, forceImmediate);
            this.AnimateAdventureRadioButtonBar();
            this.UpdateAdventureDescription(bundle);
            this.UpdateAdventureTypeMusic();
        }
    }

    private void ShowPreviewCardPanel()
    {
        this.m_adventureCardPreviewPanel.SetActive(true);
        this.m_adventureCardPreviewPanel.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        iTween.StopByName(this.m_adventureCardPreviewPanel, "PreviewCardPanelScale");
        object[] args = new object[] { "scale", Vector3.one, "time", 0.1f, "name", "PreviewCardPanelScale", "easetype", iTween.EaseType.linear };
        iTween.ScaleTo(this.m_adventureCardPreviewPanel, iTween.Hash(args));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            base.m_parentStore.ActivateCover(true);
        }
    }

    public override void StoreHidden(bool isCurrent)
    {
        foreach (KeyValuePair<string, Actor> pair in this.m_loadedPreviewCards)
        {
            UnityEngine.Object.Destroy(pair.Value.gameObject);
        }
        this.m_loadedPreviewCards.Clear();
        if (isCurrent)
        {
            this.HidePreviewCardPanel();
        }
    }

    public override void StoreShown(bool isCurrent)
    {
        if (isCurrent)
        {
            this.UpdateAdventureTypeMusic();
        }
    }

    public override void TryBuyWithGold(GeneralStoreContent.BuyEvent successBuyCB = null, GeneralStoreContent.BuyEvent failedBuyCB = null)
    {
        if (successBuyCB != null)
        {
            successBuyCB();
        }
    }

    public override void TryBuyWithMoney(GeneralStoreContent.BuyEvent successBuyCB = null, GeneralStoreContent.BuyEvent failedBuyCB = null)
    {
        <TryBuyWithMoney>c__AnonStorey335 storey = new <TryBuyWithMoney>c__AnonStorey335 {
            failedBuyCB = failedBuyCB,
            successBuyCB = successBuyCB,
            <>f__this = this
        };
        if (base.IsContentActive())
        {
            if (!AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.VANILLA_HEROES))
            {
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLUE_STORE_ADVENTURE_LOCKED_HEROES_WARNING_TITLE"),
                    m_text = GameStrings.Get("GLUE_STORE_ADVENTURE_LOCKED_HEROES_WARNING_TEXT"),
                    m_showAlertIcon = true,
                    m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                    m_responseCallback = new AlertPopup.ResponseCallback(storey.<>m__172)
                };
                base.m_parentStore.ActivateCover(true);
                DialogManager.Get().ShowPopup(info);
            }
            else if (storey.successBuyCB != null)
            {
                storey.successBuyCB();
            }
        }
        else if (storey.failedBuyCB != null)
        {
            storey.failedBuyCB();
        }
    }

    private void UpdateAdventureDescription(Network.Bundle bundle)
    {
        if (this.m_selectedAdventureType != AdventureDbId.INVALID)
        {
            string title = string.Empty;
            string desc = string.Empty;
            string warning = string.Empty;
            DbfRecord record = GameDbf.Adventure.GetRecord((int) this.m_selectedAdventureType);
            if (record == null)
            {
                Debug.LogError(string.Format("Unable to find adventure record ID: {0}", this.m_selectedAdventureType));
            }
            else if (bundle == null)
            {
                title = record.GetLocString("STORE_OWNED_HEADLINE");
                desc = record.GetLocString("STORE_OWNED_DESC");
            }
            else if (this.IsPreOrder())
            {
                title = record.GetLocString("STORE_PREORDER_HEADLINE");
                desc = record.GetLocString("STORE_PREORDER_DESC");
            }
            else
            {
                int nonPreorderItemCount = StoreManager.Get().GetNonPreorderItemCount(bundle.Items);
                title = record.GetLocString(string.Format("STORE_BUY_WINGS_{0}_HEADLINE", nonPreorderItemCount));
                desc = record.GetLocString(string.Format("STORE_BUY_WINGS_{0}_DESC", nonPreorderItemCount));
            }
            if (StoreManager.Get().IsKoreanCustomer())
            {
                warning = GameStrings.Get("GLUE_STORE_KOREAN_PRODUCT_DETAILS_ADVENTURE");
            }
            if (this.m_adventureCardPreviewText != null)
            {
                this.m_adventureCardPreviewText.Text = record.GetLocString("STORE_PREVIEW_REWARDS_TEXT");
            }
            base.m_parentStore.SetDescription(title, desc, warning);
            StoreAdventureDef storeAdventureDef = this.GetStoreAdventureDef((int) this.m_selectedAdventureType);
            if (storeAdventureDef != null)
            {
                Texture texture = null;
                if (!string.IsNullOrEmpty(storeAdventureDef.m_accentTextureName))
                {
                    texture = AssetLoader.Get().LoadTexture(FileUtils.GameAssetPathToName(storeAdventureDef.m_accentTextureName), false);
                }
                base.m_parentStore.SetAccentTexture(texture);
            }
        }
        else
        {
            base.m_parentStore.HideAccentTexture();
            base.m_parentStore.SetChooseDescription(GameStrings.Get("GLUE_STORE_CHOOSE_ADVENTURE"));
        }
    }

    private void UpdateAdventureTypeMusic()
    {
        if (base.m_parentStore.GetMode() != GeneralStoreMode.NONE)
        {
            StoreAdventureDef storeAdventureDef = this.GetStoreAdventureDef((int) this.m_selectedAdventureType);
            if (((storeAdventureDef == null) || (storeAdventureDef.m_playlist == MusicPlaylistType.Invalid)) || !MusicManager.Get().StartPlaylist(storeAdventureDef.m_playlist))
            {
                base.m_parentStore.ResumePreviousMusicPlaylist();
            }
        }
    }

    private void UpdateRadioButtonText(Network.Bundle moneyBundle)
    {
        this.m_adventureRadioButton.SetSelected(true);
        if (moneyBundle == null)
        {
            this.m_adventureRadioButtonText.Text = GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_TEXT_PURCHASED");
            this.m_adventureRadioButtonCostText.Text = string.Empty;
        }
        else
        {
            string key = !this.IsPreOrder() ? "GLUE_STORE_DUNGEON_BUTTON_TEXT" : "GLUE_STORE_DUNGEON_BUTTON_PREORDER_TEXT";
            this.m_adventureRadioButtonText.Text = GameStrings.Get(key);
            string str2 = StoreManager.Get().FormatCostBundle(moneyBundle);
            int nonPreorderItemCount = StoreManager.Get().GetNonPreorderItemCount(moneyBundle.Items);
            object[] args = new object[] { nonPreorderItemCount, str2 };
            this.m_adventureRadioButtonCostText.Text = GameStrings.Format("GLUE_STORE_DUNGEON_BUTTON_COST_TEXT", args);
        }
        if (this.m_adventureOwnedCheckmark != null)
        {
            this.m_adventureOwnedCheckmark.SetActive(moneyBundle == null);
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateAndUpdateDisplay>c__AnonStorey33A
    {
        internal GameObject currDisplay;

        internal void <>m__176(object o)
        {
            this.currDisplay.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadAdventurePreviewCard>c__AnonStorey338
    {
        internal GeneralStoreAdventureContent <>f__this;
        internal GeneralStoreAdventureContent.DelOnAdventurePreviewCardLoaded onLoadComplete;
        internal string previewCard;

        internal void <>m__174(string cardID, FullDef fullDef, object data)
        {
            <LoadAdventurePreviewCard>c__AnonStorey339 storey = new <LoadAdventurePreviewCard>c__AnonStorey339 {
                <>f__ref$824 = this,
                fullDef = fullDef
            };
            AssetLoader.Get().LoadActor(ActorNames.GetHandActor(storey.fullDef.GetEntityDef()), new AssetLoader.GameObjectCallback(storey.<>m__177), null, false);
        }

        private sealed class <LoadAdventurePreviewCard>c__AnonStorey339
        {
            internal GeneralStoreAdventureContent.<LoadAdventurePreviewCard>c__AnonStorey338 <>f__ref$824;
            internal FullDef fullDef;

            internal void <>m__177(string actorName, GameObject actorObject, object data2)
            {
                if (actorObject == null)
                {
                    Debug.LogWarning(string.Format("FAILED to load actor \"{0}\"", actorName));
                    this.<>f__ref$824.onLoadComplete(null);
                }
                else
                {
                    Actor component = actorObject.GetComponent<Actor>();
                    if (component == null)
                    {
                        Debug.LogWarning(string.Format("ERROR actor \"{0}\" has no Actor component", actorName));
                        this.<>f__ref$824.onLoadComplete(null);
                    }
                    else
                    {
                        component.SetCardDef(this.fullDef.GetCardDef());
                        component.SetEntityDef(this.fullDef.GetEntityDef());
                        component.UpdateAllComponents();
                        SceneUtils.SetLayer(component.gameObject, this.<>f__ref$824.<>f__this.gameObject.layer);
                        component.Show();
                        this.<>f__ref$824.<>f__this.m_loadedPreviewCards.Add(this.<>f__ref$824.previewCard, component);
                        this.<>f__ref$824.onLoadComplete(component);
                    }
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <OnAdventuresShowPreviewCard>c__AnonStorey336
    {
        internal GeneralStoreAdventureContent.<OnAdventuresShowPreviewCard>c__AnonStorey337 <>f__ref$823;
        internal GeneralStoreAdventureContent <>f__this;
        internal int cardIndex;

        internal void <>m__173(Actor cardActor)
        {
            cardActor.transform.position = this.<>f__this.m_adventureCardPreviewBones[this.cardIndex].transform.position;
            cardActor.transform.rotation = this.<>f__this.m_adventureCardPreviewBones[this.cardIndex].transform.rotation;
            cardActor.transform.parent = this.<>f__this.m_adventureCardPreviewBones[this.cardIndex].transform;
            cardActor.transform.localScale = Vector3.one;
            this.<>f__ref$823.loadedPreviewCards++;
            cardActor.gameObject.SetActive(this.<>f__this.m_showPreviewCards);
            if (this.<>f__this.m_showPreviewCards && (this.<>f__ref$823.loadedPreviewCards == this.<>f__ref$823.previewCards.Length))
            {
                this.<>f__this.ShowPreviewCardPanel();
            }
        }
    }

    [CompilerGenerated]
    private sealed class <OnAdventuresShowPreviewCard>c__AnonStorey337
    {
        internal GeneralStoreAdventureContent <>f__this;
        internal int loadedPreviewCards;
        internal string[] previewCards;
    }

    [CompilerGenerated]
    private sealed class <TryBuyWithMoney>c__AnonStorey335
    {
        internal GeneralStoreAdventureContent <>f__this;
        internal GeneralStoreContent.BuyEvent failedBuyCB;
        internal GeneralStoreContent.BuyEvent successBuyCB;

        internal void <>m__172(AlertPopup.Response response, object data)
        {
            if (response == AlertPopup.Response.CANCEL)
            {
                this.<>f__this.m_parentStore.ActivateCover(false);
                if (this.failedBuyCB != null)
                {
                    this.failedBuyCB();
                }
            }
            else if (this.successBuyCB != null)
            {
                this.successBuyCB();
            }
        }
    }

    public delegate void DelOnAdventurePreviewCardLoaded(Actor previewCard);
}

