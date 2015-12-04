using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePacksContent : GeneralStoreContent
{
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache3D;
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache3E;
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache3F;
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache40;
    private bool m_animatingLogo;
    private bool m_animatingPacks;
    [CustomEditField(Sections="Pack Buy Buttons/Preorder")]
    public UberText m_availableDateText;
    private Vector3 m_availableDateTextOrigScale;
    [CustomEditField(Sections="Animation")]
    public float m_backgroundFlipAnimTime = 0.5f;
    [CustomEditField(Sections="Animation")]
    public float m_backgroundFlipShake = 20f;
    [CustomEditField(Sections="Animation")]
    public float m_backgroundFlipShakeDelay;
    [CustomEditField(Sections="Sounds & Music", T=EditType.SOUND_PREFAB)]
    public string m_backgroundFlipSound;
    [CustomEditField(Sections="China Button")]
    public UIBButton m_ChinaInfoButton;
    private int m_currentDisplay = -1;
    private int m_currentGoldPackQuantity = 1;
    private bool m_hasLogo;
    private int m_lastBundleIndex;
    [CustomEditField(Sections="Packs")]
    public LogoAnimation m_logoAnimation;
    [CustomEditField(Sections="Animation/Appear")]
    public GameObject m_logoAnimationEndBone;
    [CustomEditField(Sections="Animation/Appear")]
    public GameObject m_logoAnimationStartBone;
    private Coroutine m_logoAnimCoroutine;
    [CustomEditField(Sections="Animation/Appear")]
    public Vector3 m_logoAppearOffset;
    [CustomEditField(Sections="Animation/Appear")]
    public float m_logoDisplayPunchTime = 0.5f;
    [CustomEditField(Sections="Animation/Appear")]
    public MeshRenderer m_logoGlowMesh;
    private MeshRenderer m_logoGlowMesh1;
    private MeshRenderer m_logoGlowMesh2;
    [CustomEditField(Sections="Animation/Appear")]
    public float m_logoHoldTime = 1f;
    [CustomEditField(Sections="Animation/Appear")]
    public float m_logoIntroTime = 0.25f;
    [CustomEditField(Sections="Animation/Appear")]
    public MeshRenderer m_logoMesh;
    private MeshRenderer m_logoMesh1;
    private MeshRenderer m_logoMesh2;
    [CustomEditField(Sections="Animation/Appear")]
    public float m_logoOutroTime = 0.25f;
    [CustomEditField(Sections="Packs")]
    public int m_maxPackBuyButtons = 10;
    [CustomEditField(Sections="Animation")]
    public float m_maxPackFlyInXShake = 20f;
    [CustomEditField(Sections="Animation")]
    public float m_maxPackFlyOutXShake = 12f;
    private Coroutine m_packAnimCoroutine;
    [CustomEditField(Sections="Pack Buy Buttons")]
    public MultiSliceElement m_packBuyButtonContainer;
    [CustomEditField(Sections="Pack Buy Buttons")]
    public GeneralStorePackBuyButton m_packBuyButtonPrefab;
    private List<GeneralStorePackBuyButton> m_packBuyButtons = new List<GeneralStorePackBuyButton>();
    [CustomEditField(Sections="Pack Buy Buttons")]
    public GameObject m_packBuyContainer;
    [CustomEditField(Sections="Pack Buy Buttons")]
    public MultiSliceElement m_packBuyFrameContainer;
    [CustomEditField(Sections="Pack Buy Buttons/Preorder")]
    public GeneralStorePackBuyButton m_packBuyPreorderButton;
    [CustomEditField(Sections="Pack Buy Buttons/Preorder")]
    public GameObject m_packBuyPreorderContainer;
    public GameObject m_packContainer;
    public GeneralStorePacksContentDisplay m_packDisplay;
    private GeneralStorePacksContentDisplay m_packDisplay1;
    private GeneralStorePacksContentDisplay m_packDisplay2;
    public GameObject m_packEmptyDisplay;
    [CustomEditField(Sections="Animation")]
    public float m_packFlyInAnimTime = 0.2f;
    [CustomEditField(Sections="Animation")]
    public float m_packFlyInDelay = 0.01f;
    [CustomEditField(Sections="Animation")]
    public float m_packFlyOutAnimTime = 0.1f;
    [CustomEditField(Sections="Animation")]
    public float m_packFlyOutDelay = 0.005f;
    [CustomEditField(Sections="Animation")]
    public float m_packFlyShakeTime = 2f;
    [CustomEditField(Sections="Animation")]
    public float m_PackYDegreeVariationMag = 2f;
    private Coroutine m_preorderCardBackAnimCoroutine;
    [CustomEditField(Sections="Animation/Preorder")]
    public GeneralStoreRewardsCardBack m_preorderCardBackReward;
    [CustomEditField(Sections="Animation/Appear")]
    public Vector3 m_punchAmount;
    public StoreQuantityPrompt m_quantityPrompt;
    private Vector3 m_savedLocalPosition;
    private int m_selectedBoosterId;
    [CustomEditField(Sections="Animation")]
    public float m_shakeObjectDelayMultiplier = 0.7f;
    private Map<int, StorePackDef> m_storePackDefs = new Map<int, StorePackDef>();
    [CustomEditField(Sections="Pack Buy Buttons", ListTable=true)]
    public List<ToggleableButtonFrame> m_toggleableButtonFrames = new List<ToggleableButtonFrame>();
    private int m_visiblePackCount;
    private static readonly int MAX_QUANTITY_BOUGHT_WITH_GOLD = 50;
    private const string PREV_PLAYLIST_NAME = "StorePrevCurrentPlaylist";

    private void AnimateAndUpdateDisplay(int id, bool forceImmediate = false)
    {
        <AnimateAndUpdateDisplay>c__AnonStorey343 storey = new <AnimateAndUpdateDisplay>c__AnonStorey343();
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.HideCardBackReward();
        }
        storey.currDisplay = null;
        GameObject target = null;
        if (this.m_currentDisplay == -1)
        {
            this.m_currentDisplay = 1;
            storey.currDisplay = this.m_packEmptyDisplay;
        }
        else
        {
            storey.currDisplay = this.GetCurrentDisplayContainer();
        }
        target = this.GetNextDisplayContainer();
        this.GetCurrentLogo().gameObject.SetActive(false);
        this.GetCurrentDisplay().ClearPacks();
        this.m_currentDisplay = (this.m_currentDisplay + 1) % 2;
        target.SetActive(true);
        if (!forceImmediate)
        {
            storey.currDisplay.transform.localRotation = Quaternion.identity;
            target.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            iTween.StopByName(storey.currDisplay, "ROTATION_TWEEN");
            iTween.StopByName(target, "ROTATION_TWEEN");
            object[] args = new object[] { "amount", new Vector3(0.5f, 0f, 0f), "time", 0.5f, "name", "ROTATION_TWEEN", "oncomplete", new Action<object>(storey.<>m__192) };
            iTween.RotateBy(storey.currDisplay, iTween.Hash(args));
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
            storey.currDisplay.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
            storey.currDisplay.SetActive(false);
        }
        StorePackDef storePackDef = this.GetStorePackDef(id);
        this.GetCurrentDisplay().UpdatePackType(storePackDef);
        storey.currLogo = this.GetCurrentLogo();
        if (storey.currLogo != null)
        {
            this.m_hasLogo = !string.IsNullOrEmpty(storePackDef.m_logoTextureName);
            storey.currLogo.gameObject.SetActive(this.m_hasLogo);
            if (this.m_hasLogo)
            {
                <AnimateAndUpdateDisplay>c__AnonStorey344 storey2 = new <AnimateAndUpdateDisplay>c__AnonStorey344 {
                    <>f__ref$835 = storey
                };
                AssetLoader.Get().LoadTexture(FileUtils.GameAssetPathToName(storePackDef.m_logoTextureName), new AssetLoader.ObjectCallback(storey2.<>m__193), null, false);
                storey2.glowLogo = this.GetCurrentGlowLogo();
                if (storey2.glowLogo != null)
                {
                    AssetLoader.Get().LoadTexture(FileUtils.GameAssetPathToName(storePackDef.m_logoTextureGlowName), new AssetLoader.ObjectCallback(storey2.<>m__194), null, false);
                }
            }
        }
        this.AnimateBuyBar();
    }

    private void AnimateBuyBar()
    {
        Network.Bundle bundle;
        GameObject target = !StoreManager.Get().IsBoosterPreorderActive(this.m_selectedBoosterId, out bundle) ? this.m_packBuyPreorderContainer : this.m_packBuyContainer;
        if (this.m_selectedBoosterId != 0)
        {
            iTween.Stop(target);
            target.transform.localRotation = Quaternion.identity;
            object[] args = new object[] { "amount", new Vector3(-1f, 0f, 0f), "time", this.m_backgroundFlipAnimTime, "delay", 0.001f };
            iTween.RotateBy(target, iTween.Hash(args));
        }
    }

    [DebuggerHidden]
    private IEnumerator AnimateFadeLogo(MeshRenderer logo)
    {
        return new <AnimateFadeLogo>c__Iterator1DB { logo = logo, <$>logo = logo, <>f__this = this };
    }

    private void AnimateLogo(bool animateLogo)
    {
        if ((this.m_hasLogo && base.gameObject.activeInHierarchy) && (this.m_selectedBoosterId != 0))
        {
            MeshRenderer currentLogo = this.GetCurrentLogo();
            currentLogo.gameObject.SetActive(true);
            switch (this.m_logoAnimation)
            {
                case LogoAnimation.Slam:
                    if (animateLogo)
                    {
                        this.m_logoAnimCoroutine = base.StartCoroutine(this.AnimateSlamLogo(currentLogo));
                    }
                    else if (!this.m_animatingLogo)
                    {
                        currentLogo.transform.localPosition = this.m_logoAnimationEndBone.transform.localPosition;
                        currentLogo.gameObject.SetActive(true);
                    }
                    break;

                case LogoAnimation.Fade:
                    if (animateLogo)
                    {
                        this.m_logoAnimCoroutine = base.StartCoroutine(this.AnimateFadeLogo(currentLogo));
                    }
                    else if (!this.m_animatingLogo)
                    {
                        currentLogo.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator AnimatePacks(GeneralStorePacksContentDisplay display, int numVisiblePacks, bool forceImmediate)
    {
        return new <AnimatePacks>c__Iterator1DD { display = display, numVisiblePacks = numVisiblePacks, forceImmediate = forceImmediate, <$>display = display, <$>numVisiblePacks = numVisiblePacks, <$>forceImmediate = forceImmediate, <>f__this = this };
    }

    private void AnimatePacksFlying(int numVisiblePacks, bool forceImmediate = false)
    {
        if (base.gameObject.activeInHierarchy)
        {
            GeneralStorePacksContentDisplay currentDisplay = this.GetCurrentDisplay();
            if (this.m_packAnimCoroutine != null)
            {
                base.StopCoroutine(this.m_packAnimCoroutine);
            }
            if (this.m_preorderCardBackAnimCoroutine != null)
            {
                base.StopCoroutine(this.m_preorderCardBackAnimCoroutine);
            }
            this.m_packAnimCoroutine = base.StartCoroutine(this.AnimatePacks(currentDisplay, numVisiblePacks, forceImmediate));
            this.m_preorderCardBackAnimCoroutine = base.StartCoroutine(this.AnimatePreorderUI(currentDisplay));
        }
    }

    [DebuggerHidden]
    private IEnumerator AnimatePreorderUI(GeneralStorePacksContentDisplay display)
    {
        return new <AnimatePreorderUI>c__Iterator1DE { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateSlamLogo(MeshRenderer logo)
    {
        return new <AnimateSlamLogo>c__Iterator1DC { logo = logo, <$>logo = logo, <>f__this = this };
    }

    private void Awake()
    {
        this.m_packDisplay1 = this.m_packDisplay;
        this.m_packDisplay2 = UnityEngine.Object.Instantiate<GeneralStorePacksContentDisplay>(this.m_packDisplay);
        this.m_packDisplay2.transform.parent = this.m_packDisplay1.transform.parent;
        this.m_packDisplay2.transform.localPosition = this.m_packDisplay1.transform.localPosition;
        this.m_packDisplay2.transform.localScale = this.m_packDisplay1.transform.localScale;
        this.m_packDisplay2.transform.localRotation = this.m_packDisplay1.transform.localRotation;
        this.m_packDisplay2.gameObject.SetActive(false);
        this.m_logoMesh1 = this.m_logoMesh;
        this.m_logoMesh2 = UnityEngine.Object.Instantiate<MeshRenderer>(this.m_logoMesh);
        this.m_logoMesh2.transform.parent = this.m_logoMesh1.transform.parent;
        this.m_logoMesh2.transform.localPosition = this.m_logoMesh1.transform.localPosition;
        this.m_logoMesh2.transform.localScale = this.m_logoMesh1.transform.localScale;
        this.m_logoMesh2.transform.localRotation = this.m_logoMesh1.transform.localRotation;
        this.m_logoMesh2.gameObject.SetActive(false);
        this.m_logoGlowMesh1 = this.m_logoGlowMesh;
        this.m_logoGlowMesh2 = UnityEngine.Object.Instantiate<MeshRenderer>(this.m_logoGlowMesh);
        this.m_logoGlowMesh2.transform.parent = this.m_logoGlowMesh1.transform.parent;
        this.m_logoGlowMesh2.transform.localPosition = this.m_logoGlowMesh1.transform.localPosition;
        this.m_logoGlowMesh2.transform.localScale = this.m_logoGlowMesh1.transform.localScale;
        this.m_logoGlowMesh2.transform.localRotation = this.m_logoGlowMesh1.transform.localRotation;
        this.m_logoGlowMesh2.gameObject.SetActive(false);
        this.m_packDisplay1.SetParent(this);
        this.m_packDisplay2.SetParent(this);
        base.m_productType = ProductType.PRODUCT_TYPE_BOOSTER;
        this.m_packBuyContainer.SetActive(false);
        if (this.m_availableDateText != null)
        {
            this.m_availableDateTextOrigScale = this.m_availableDateText.transform.localScale;
        }
        if (this.m_ChinaInfoButton != null)
        {
            this.m_ChinaInfoButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnChinaKoreaInfoPressed));
        }
        foreach (DbfRecord record in GameUtils.GetPackRecordsWithStorePrefab())
        {
            int id = record.GetId();
            string assetName = record.GetAssetName("STORE_PREFAB");
            GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetName), false, false);
            if (obj2 == null)
            {
                UnityEngine.Debug.LogError(string.Format("Unable to load store pack def: {0}", assetName));
            }
            else
            {
                StorePackDef component = obj2.GetComponent<StorePackDef>();
                if (component == null)
                {
                    UnityEngine.Debug.LogError(string.Format("StorePackDef component not found: {0}", assetName));
                }
                else
                {
                    this.m_storePackDefs.Add(id, component);
                }
            }
        }
        this.UpdateChinaKoreaInfoButton();
    }

    private GeneralStorePackBuyButton CreatePackBuyButton(int index)
    {
        if (index >= this.m_packBuyButtons.Count)
        {
            int num = (index - this.m_packBuyButtons.Count) + 1;
            for (int i = 0; i < num; i++)
            {
                GeneralStorePackBuyButton item = (GeneralStorePackBuyButton) GameUtils.Instantiate(this.m_packBuyButtonPrefab, this.m_packBuyButtonContainer.gameObject, true);
                SceneUtils.SetLayer(item.gameObject, this.m_packBuyButtonContainer.gameObject.layer);
                item.transform.localRotation = Quaternion.identity;
                item.transform.localScale = Vector3.one;
                this.m_packBuyButtonContainer.AddSlice(item.gameObject);
                this.m_packBuyButtons.Add(item);
            }
            this.m_packBuyButtonContainer.UpdateSlices();
        }
        return this.m_packBuyButtons[index];
    }

    public int GetBoosterId()
    {
        return this.m_selectedBoosterId;
    }

    private GeneralStorePacksContentDisplay GetCurrentDisplay()
    {
        return ((this.m_currentDisplay != 0) ? this.m_packDisplay2 : this.m_packDisplay1);
    }

    private GameObject GetCurrentDisplayContainer()
    {
        return this.GetCurrentDisplay().gameObject;
    }

    private MeshRenderer GetCurrentGlowLogo()
    {
        return ((this.m_currentDisplay != 0) ? this.m_logoGlowMesh2 : this.m_logoGlowMesh1);
    }

    private NoGTAPPTransactionData GetCurrentGTAPPTransactionData()
    {
        return new NoGTAPPTransactionData { Product = base.m_productType, ProductData = this.m_selectedBoosterId, Quantity = this.m_currentGoldPackQuantity };
    }

    private MeshRenderer GetCurrentLogo()
    {
        return ((this.m_currentDisplay != 0) ? this.m_logoMesh2 : this.m_logoMesh1);
    }

    public override string GetMoneyDisplayOwnedText()
    {
        return GameStrings.Get("GLUE_STORE_PACK_BUTTON_COST_OWNED_TEXT");
    }

    private GameObject GetNextDisplayContainer()
    {
        return ((((this.m_currentDisplay + 1) % 2) != 0) ? this.m_packDisplay2.gameObject : this.m_packDisplay1.gameObject);
    }

    private GeneralStorePackBuyButton GetPackBuyButton(int index)
    {
        if (index < this.m_packBuyButtons.Count)
        {
            return this.m_packBuyButtons[index];
        }
        return null;
    }

    public StorePackDef GetStorePackDef(int packDbId)
    {
        StorePackDef def = null;
        this.m_storePackDefs.TryGetValue(packDbId, out def);
        return def;
    }

    public Map<int, StorePackDef> GetStorePackDefs()
    {
        return this.m_storePackDefs;
    }

    private void HandleGoldPackBuyButtonClick()
    {
        NoGTAPPTransactionData bundle = new NoGTAPPTransactionData {
            Product = base.m_productType,
            ProductData = this.m_selectedBoosterId,
            Quantity = this.m_currentGoldPackQuantity
        };
        base.SetCurrentGoldBundle(bundle);
        this.UpdatePacksDescription();
    }

    private void HandleGoldPackBuyButtonDoubleClick(GeneralStorePackBuyButton button)
    {
        <HandleGoldPackBuyButtonDoubleClick>c__AnonStorey342 storey = new <HandleGoldPackBuyButtonDoubleClick>c__AnonStorey342 {
            button = button,
            <>f__this = this
        };
        base.m_parentStore.ActivateCover(true);
        this.m_quantityPrompt.Show(MAX_QUANTITY_BOUGHT_WITH_GOLD, new StoreQuantityPrompt.OkayListener(storey.<>m__190), new StoreQuantityPrompt.CancelListener(storey.<>m__191));
    }

    private void HandleMoneyPackBuyButtonClick(int bundleIndex)
    {
        Network.Bundle bundle = null;
        List<Network.Bundle> list = StoreManager.Get().GetAllBundlesForProduct(base.m_productType, this.m_selectedBoosterId, 0);
        if ((list != null) && (list.Count > 0))
        {
            if (bundleIndex >= list.Count)
            {
                bundleIndex = 0;
            }
            bundle = list[bundleIndex];
        }
        base.SetCurrentMoneyBundle(bundle, true);
        this.m_lastBundleIndex = bundleIndex;
        this.UpdatePacksDescription();
    }

    public override bool IsPurchaseDisabled()
    {
        return (this.m_selectedBoosterId == 0);
    }

    protected override void OnBundleChanged(NoGTAPPTransactionData goldBundle, Network.Bundle moneyBundle)
    {
        if (goldBundle != null)
        {
            this.m_visiblePackCount = goldBundle.Quantity;
        }
        else if (moneyBundle != null)
        {
            if (<>f__am$cache3E == null)
            {
                <>f__am$cache3E = obj => obj.Product == ProductType.PRODUCT_TYPE_BOOSTER;
            }
            Network.BundleItem item = moneyBundle.Items.Find(<>f__am$cache3E);
            this.m_visiblePackCount = (item != null) ? item.Quantity : 0;
        }
        this.AnimatePacksFlying(this.m_visiblePackCount, false);
    }

    private void OnChinaKoreaInfoPressed(UIEvent e)
    {
        bool flag = StoreManager.Get().IsChinaCustomer();
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get(!flag ? "GLUE_STORE_KOREAN_DISCLAIMER_HEADLINE" : "GLUE_STORE_CHINA_DISCLAIMER_HEADLINE"),
            m_text = GameStrings.Get(!flag ? "GLUE_STORE_KOREAN_DISCLAIMER_DETAILS" : "GLUE_STORE_CHINA_DISCLAIMER_DETAILS"),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    protected override void OnRefresh()
    {
        this.UpdatePackBuyButtons();
        this.UpdatePacksDescription();
        if (!base.HasBundleSet() && (this.m_selectedBoosterId != 0))
        {
            this.UpdateSelectedBundle(true);
        }
    }

    public override void PostStoreFlipIn(bool animatedFlipIn)
    {
        this.UpdatePacksTypeMusic();
        this.AnimateLogo(animatedFlipIn);
        this.AnimatePacksFlying(this.m_visiblePackCount, !animatedFlipIn);
        this.UpdateChinaKoreaInfoButton();
        this.m_savedLocalPosition = base.gameObject.transform.localPosition;
    }

    public override void PreStoreFlipOut()
    {
        this.ResetAnimations();
        this.GetCurrentDisplay().ClearPacks();
        this.UpdateChinaKoreaInfoButton();
    }

    private void ResetAnimations()
    {
        if (this.m_preorderCardBackReward != null)
        {
            this.m_preorderCardBackReward.HideCardBackReward();
        }
        if (this.m_availableDateText != null)
        {
            this.m_availableDateText.gameObject.SetActive(false);
        }
        if (this.m_logoAnimCoroutine != null)
        {
            iTween.Stop(this.m_logoMesh1.gameObject);
            iTween.Stop(this.m_logoMesh2.gameObject);
            base.StopCoroutine(this.m_logoAnimCoroutine);
        }
        this.m_logoMesh1.gameObject.SetActive(false);
        this.m_logoMesh2.gameObject.SetActive(false);
        if (this.m_packAnimCoroutine != null)
        {
            base.StopCoroutine(this.m_packAnimCoroutine);
        }
        if (this.m_preorderCardBackAnimCoroutine != null)
        {
            base.StopCoroutine(this.m_preorderCardBackAnimCoroutine);
        }
        this.m_animatingLogo = false;
        this.m_animatingPacks = false;
    }

    private void SelectPackBuyButton(GeneralStorePackBuyButton packBuyBtn)
    {
        foreach (GeneralStorePackBuyButton button in this.m_packBuyButtons)
        {
            button.Unselect();
        }
        packBuyBtn.Select();
    }

    public void SetBoosterId(int id, bool forceImmediate = false)
    {
        if (this.m_selectedBoosterId != id)
        {
            bool flag = this.m_selectedBoosterId == 0;
            this.GetCurrentDisplay().ClearPacks();
            this.m_visiblePackCount = 0;
            this.m_selectedBoosterId = id;
            if (flag)
            {
                this.UpdateSelectedBundle(false);
            }
            this.ResetAnimations();
            this.AnimateAndUpdateDisplay(id, forceImmediate);
            this.AnimateLogo(!forceImmediate);
            this.AnimatePacksFlying(this.m_visiblePackCount, forceImmediate);
            this.UpdatePackBuyButtons();
            this.UpdatePacksDescription();
            this.UpdatePacksTypeMusic();
            this.UpdateChinaKoreaInfoButton();
            if (base.GetCurrentGoldBundle() != null)
            {
                base.SetCurrentGoldBundle(this.GetCurrentGTAPPTransactionData());
            }
            else if (base.GetCurrentMoneyBundle() != null)
            {
                this.HandleMoneyPackBuyButtonClick(this.m_lastBundleIndex);
            }
        }
    }

    public void ShakeStore(int numPacks, float maxXRotation, float delay = 0f)
    {
        if (numPacks != 0)
        {
            int b = 1;
            foreach (Network.Bundle bundle in StoreManager.Get().GetAllBundlesForProduct(ProductType.PRODUCT_TYPE_BOOSTER, this.m_selectedBoosterId, 0))
            {
                if (<>f__am$cache3D == null)
                {
                    <>f__am$cache3D = obj => obj.Product == ProductType.PRODUCT_TYPE_BOOSTER;
                }
                Network.BundleItem item = bundle.Items.Find(<>f__am$cache3D);
                if (item != null)
                {
                    b = Mathf.Max(item.Quantity, b);
                }
            }
            int num2 = b - 1;
            if (num2 != 0)
            {
                float xRotationAmount = (((float) numPacks) / ((float) num2)) * maxXRotation;
                base.m_parentStore.ShakeStore(xRotationAmount, this.m_packFlyShakeTime, delay);
            }
        }
    }

    private void ShowPreorderBuyButtons(Network.Bundle preOrderBundle)
    {
        this.m_packBuyContainer.SetActive(false);
        this.m_packBuyPreorderContainer.SetActive(true);
        if (!this.m_packBuyPreorderButton.HasEventListener(UIEventType.PRESS))
        {
            this.m_packBuyPreorderButton.AddEventListener(UIEventType.PRESS, delegate (UIEvent e) {
                if (base.IsContentActive())
                {
                    this.HandleMoneyPackBuyButtonClick(0);
                    this.m_packBuyPreorderButton.Select();
                }
            });
        }
        string quantityText = string.Empty;
        if (preOrderBundle != null)
        {
            if (<>f__am$cache40 == null)
            {
                <>f__am$cache40 = obj => obj.Product == ProductType.PRODUCT_TYPE_BOOSTER;
            }
            Network.BundleItem item = preOrderBundle.Items.Find(<>f__am$cache40);
            object[] args = new object[] { item.Quantity };
            quantityText = GameStrings.Format("GLUE_STORE_PACKS_BUTTON_PREORDER_TEXT", args);
        }
        this.m_packBuyPreorderButton.SetMoneyValue(preOrderBundle, quantityText);
        this.HandleMoneyPackBuyButtonClick(0);
        this.m_packBuyPreorderButton.Select();
    }

    private void ShowStandardBuyButtons()
    {
        <ShowStandardBuyButtons>c__AnonStorey340 storey = new <ShowStandardBuyButtons>c__AnonStorey340 {
            <>f__this = this
        };
        this.m_packBuyPreorderContainer.SetActive(false);
        this.m_packBuyContainer.SetActive(true);
        int selectedBoosterId = this.m_selectedBoosterId;
        System.Action action = null;
        storey.goldButton = this.GetPackBuyButton(0);
        if (storey.goldButton == null)
        {
            storey.goldButton = this.CreatePackBuyButton(0);
            storey.goldButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storey.<>m__188));
            if (UniversalInputManager.UsePhoneUI == null)
            {
                storey.goldButton.AddEventListener(UIEventType.DOUBLECLICK, new UIEvent.Handler(storey.<>m__189));
            }
        }
        if (this.m_selectedBoosterId != 0)
        {
            storey.goldButton.UpdateFromGTAPP(this.GetCurrentGTAPPTransactionData());
        }
        action = new System.Action(storey.<>m__18A);
        storey.goldButton.Unselect();
        List<Network.Bundle> range = StoreManager.Get().GetAllBundlesForProduct(base.m_productType, selectedBoosterId, 0);
        if (range.Count > (this.m_maxPackBuyButtons - 1))
        {
            range = range.GetRange(0, this.m_maxPackBuyButtons - 1);
        }
        int index = 1;
        foreach (Network.Bundle bundle in range)
        {
            <ShowStandardBuyButtons>c__AnonStorey341 storey2 = new <ShowStandardBuyButtons>c__AnonStorey341 {
                <>f__this = this
            };
            if (<>f__am$cache3F == null)
            {
                <>f__am$cache3F = obj => obj.Product == ProductType.PRODUCT_TYPE_BOOSTER;
            }
            Network.BundleItem item = bundle.Items.Find(<>f__am$cache3F);
            if (item == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("GeneralStorePacksContent.UpdatePackBuyButtons() bundle {0} has no packs bundle item!", bundle.ProductID));
            }
            else
            {
                storey2.moneyButton = this.GetPackBuyButton(index);
                storey2.bundleIndex = index - 1;
                if (storey2.moneyButton == null)
                {
                    storey2.moneyButton = this.CreatePackBuyButton(index);
                    storey2.moneyButton.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storey2.<>m__18C));
                }
                string quantityText = StoreManager.Get().GetProductQuantityText(item.Product, item.ProductData, item.Quantity);
                storey2.moneyButton.SetMoneyValue(bundle, quantityText);
                storey2.moneyButton.gameObject.SetActive(true);
                if (storey2.moneyButton.IsSelected() || (base.GetCurrentMoneyBundle() == bundle))
                {
                    action = new System.Action(storey2.<>m__18D);
                }
                storey2.moneyButton.Unselect();
                index++;
            }
        }
        bool flag = StoreManager.Get().CanBuyBoosterWithGold(this.m_selectedBoosterId);
        storey.goldButton.gameObject.SetActive(flag);
        if (!flag)
        {
            index--;
        }
        for (int i = index; i < this.m_packBuyButtons.Count; i++)
        {
            GeneralStorePackBuyButton button = this.m_packBuyButtons[i];
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
        int num4 = 1;
        foreach (ToggleableButtonFrame frame in this.m_toggleableButtonFrames)
        {
            bool flag2 = num4 < index;
            frame.m_IBar.SetActive(flag2);
            frame.m_Middle.SetActive(flag2);
            num4++;
        }
        if (this.m_packBuyFrameContainer != null)
        {
            this.m_packBuyFrameContainer.UpdateSlices();
        }
        this.m_packBuyButtonContainer.UpdateSlices();
        if (action != null)
        {
            action();
        }
    }

    public override void StoreHidden(bool isCurrent)
    {
        if (isCurrent)
        {
            this.ResetAnimations();
            if (this.m_quantityPrompt != null)
            {
                this.m_quantityPrompt.Hide();
            }
            this.GetCurrentDisplay().ClearPacks();
        }
    }

    public override void StoreShown(bool isCurrent)
    {
        if (isCurrent)
        {
            this.AnimateLogo(false);
            this.AnimatePacksFlying(this.m_visiblePackCount, true);
            this.UpdatePackBuyButtons();
            this.UpdatePacksTypeMusic();
            this.UpdateChinaKoreaInfoButton();
        }
    }

    private void UpdateChinaKoreaInfoButton()
    {
        if (this.m_ChinaInfoButton != null)
        {
            bool flag2 = ((StoreManager.Get().IsChinaCustomer() || StoreManager.Get().IsKoreanCustomer()) && base.IsContentActive()) && (this.m_selectedBoosterId != 0);
            this.m_ChinaInfoButton.gameObject.SetActive(flag2);
        }
    }

    private void UpdatePackBuyButtons()
    {
        if (this.m_selectedBoosterId != 0)
        {
            Network.Bundle bundle;
            if (StoreManager.Get().IsBoosterPreorderActive(this.m_selectedBoosterId, out bundle))
            {
                this.ShowPreorderBuyButtons(bundle);
            }
            else
            {
                this.ShowStandardBuyButtons();
            }
        }
    }

    private void UpdatePacksDescription()
    {
        if (this.m_selectedBoosterId == 0)
        {
            base.m_parentStore.HideAccentTexture();
            base.m_parentStore.SetChooseDescription(GameStrings.Get("GLUE_STORE_CHOOSE_PACK"));
        }
        else
        {
            DbfRecord record = GameDbf.Booster.GetRecord(this.m_selectedBoosterId);
            string locString = record.GetLocString("NAME");
            string title = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_HEADLINE_PACK");
            object[] args = new object[] { locString };
            string desc = GameStrings.Format("GLUE_STORE_PRODUCT_DETAILS_PACK", args);
            Network.Bundle currentMoneyBundle = base.GetCurrentMoneyBundle();
            bool flag = false;
            if (currentMoneyBundle != null)
            {
                flag = currentMoneyBundle.IsPreOrder();
                if (flag && (record.GetId() == 10))
                {
                    desc = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_TGT_PACK_PRESALE");
                    title = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_HEADLINE_TGT_PACK_PRESALE");
                }
            }
            string warning = string.Empty;
            if (StoreManager.Get().IsKoreanCustomer())
            {
                if (flag)
                {
                    warning = GameStrings.Get("GLUE_STORE_KOREAN_PRODUCT_DETAILS_PACKS_PREORDER");
                }
                else
                {
                    warning = GameStrings.Get("GLUE_STORE_KOREAN_PRODUCT_DETAILS_EXPERT_PACK");
                }
            }
            base.m_parentStore.SetDescription(title, desc, warning);
            StorePackDef storePackDef = this.GetStorePackDef(this.m_selectedBoosterId);
            if (storePackDef != null)
            {
                Texture texture = null;
                if (!string.IsNullOrEmpty(storePackDef.m_accentTextureName))
                {
                    texture = AssetLoader.Get().LoadTexture(FileUtils.GameAssetPathToName(storePackDef.m_accentTextureName), false);
                }
                base.m_parentStore.SetAccentTexture(texture);
            }
        }
    }

    private void UpdatePacksTypeMusic()
    {
        if (base.m_parentStore.GetMode() != GeneralStoreMode.NONE)
        {
            StorePackDef storePackDef = this.GetStorePackDef(this.m_selectedBoosterId);
            if (((storePackDef == null) || (storePackDef.m_playlist == MusicPlaylistType.Invalid)) || !MusicManager.Get().StartPlaylist(storePackDef.m_playlist))
            {
                base.m_parentStore.ResumePreviousMusicPlaylist();
            }
        }
    }

    private void UpdateSelectedBundle(bool forceUpdate = false)
    {
        NoGTAPPTransactionData noGTAPPTransactionData = new NoGTAPPTransactionData {
            Product = base.m_productType,
            ProductData = this.m_selectedBoosterId,
            Quantity = 1
        };
        if (StoreManager.Get().GetGoldCostNoGTAPP(noGTAPPTransactionData) != 0)
        {
            base.SetCurrentGoldBundle(noGTAPPTransactionData);
        }
        else
        {
            Network.Bundle bundle = StoreManager.Get().GetLowestCostUnownedBundle(base.m_productType, this.m_selectedBoosterId, 0);
            if (bundle != null)
            {
                base.SetCurrentMoneyBundle(bundle, forceUpdate);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateAndUpdateDisplay>c__AnonStorey343
    {
        internal GameObject currDisplay;
        internal MeshRenderer currLogo;

        internal void <>m__192(object o)
        {
            this.currDisplay.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateAndUpdateDisplay>c__AnonStorey344
    {
        internal GeneralStorePacksContent.<AnimateAndUpdateDisplay>c__AnonStorey343 <>f__ref$835;
        internal MeshRenderer glowLogo;

        internal void <>m__193(string name, UnityEngine.Object obj, object data)
        {
            Texture texture = obj as Texture;
            if (texture == null)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to load texture {0}!", name));
            }
            else
            {
                this.<>f__ref$835.currLogo.material.mainTexture = texture;
            }
        }

        internal void <>m__194(string name, UnityEngine.Object obj, object data)
        {
            Texture texture = obj as Texture;
            if (texture == null)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to load texture {0}!", name));
            }
            else
            {
                this.glowLogo.material.mainTexture = texture;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateFadeLogo>c__Iterator1DB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MeshRenderer <$>logo;
        internal GeneralStorePacksContent <>f__this;
        internal PlayMakerFSM <logoFSM>__0;
        internal MeshRenderer logo;

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
                    if ((this.logo != null) && this.logo.gameObject.activeInHierarchy)
                    {
                        break;
                    }
                    goto Label_0241;

                case 1:
                    break;

                case 2:
                    goto Label_01C5;

                case 3:
                    this.<>f__this.m_animatingLogo = false;
                    this.$PC = -1;
                    goto Label_0241;

                default:
                    goto Label_0241;
            }
            while (this.<>f__this.m_animatingLogo)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0243;
            }
            this.<>f__this.m_animatingLogo = true;
            this.<logoFSM>__0 = this.logo.GetComponent<PlayMakerFSM>();
            this.logo.transform.localPosition = this.<>f__this.m_logoAnimationStartBone.transform.localPosition;
            object[] args = new object[] { "position", this.logo.transform.localPosition - this.<>f__this.m_logoAppearOffset, "easetype", iTween.EaseType.easeInQuint, "time", this.<>f__this.m_logoIntroTime, "islocal", true };
            iTween.MoveFrom(this.logo.gameObject, iTween.Hash(args));
            AnimationUtil.FadeTexture(this.logo, 0f, 1f, this.<>f__this.m_logoIntroTime, 0f, null);
            if (this.<logoFSM>__0 != null)
            {
                this.<logoFSM>__0.SendEvent("FadeIn");
            }
            if (this.<>f__this.m_logoHoldTime > 0f)
            {
                this.$current = new WaitForSeconds(this.<>f__this.m_logoHoldTime);
                this.$PC = 2;
                goto Label_0243;
            }
        Label_01C5:
            AnimationUtil.FadeTexture(this.logo, 1f, 0f, this.<>f__this.m_logoOutroTime, 0f, null);
            if (this.<logoFSM>__0 != null)
            {
                this.<logoFSM>__0.SendEvent("FadeOut");
            }
            this.$current = new WaitForSeconds(this.<>f__this.m_logoOutroTime);
            this.$PC = 3;
            goto Label_0243;
        Label_0241:
            return false;
        Label_0243:
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
    private sealed class <AnimatePacks>c__Iterator1DD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GeneralStorePacksContentDisplay <$>display;
        internal bool <$>forceImmediate;
        internal int <$>numVisiblePacks;
        internal GeneralStorePacksContent <>f__this;
        internal float <delay>__3;
        internal int <packCount>__1;
        internal int <packsFlown>__0;
        internal float <shakeX>__2;
        internal GeneralStorePacksContentDisplay display;
        internal bool forceImmediate;
        internal int numVisiblePacks;

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
                    if (this.<>f__this.m_animatingLogo)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0186;
                    }
                    this.<>f__this.m_animatingPacks = true;
                    this.<packsFlown>__0 = this.display.ShowPacks(this.numVisiblePacks, this.<>f__this.m_packFlyInAnimTime, this.<>f__this.m_packFlyOutAnimTime, this.<>f__this.m_packFlyInDelay, this.<>f__this.m_packFlyOutDelay, this.forceImmediate);
                    if (!this.forceImmediate && (this.<packsFlown>__0 != 0))
                    {
                        this.<packCount>__1 = Mathf.Abs(this.<packsFlown>__0);
                        this.<shakeX>__2 = (this.<packCount>__1 <= 0) ? this.<>f__this.m_maxPackFlyOutXShake : this.<>f__this.m_maxPackFlyInXShake;
                        this.<delay>__3 = (this.<packCount>__1 <= 0) ? this.<>f__this.m_packFlyOutDelay : this.<>f__this.m_packFlyInDelay;
                        this.<>f__this.ShakeStore(this.<packCount>__1, this.<shakeX>__2, (this.<packCount>__1 * this.<delay>__3) * this.<>f__this.m_shakeObjectDelayMultiplier);
                        this.$current = new WaitForSeconds(this.<delay>__3);
                        this.$PC = 2;
                        goto Label_0186;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0184;
            }
            this.<>f__this.m_animatingPacks = false;
            this.$PC = -1;
        Label_0184:
            return false;
        Label_0186:
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
    private sealed class <AnimatePreorderUI>c__Iterator1DE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        private static Predicate<Network.BundleItem> <>f__am$cache7;
        internal GeneralStorePacksContent <>f__this;
        internal Network.BundleItem <cardBackItem>__2;
        internal bool <isPreorder>__1;
        internal StorePackDef <packDef>__3;
        internal Network.Bundle <preOrderBundle>__0;

        private static bool <>m__195(Network.BundleItem o)
        {
            return (o.Product == ProductType.PRODUCT_TYPE_CARD_BACK);
        }

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
                    if (this.<>f__this.m_preorderCardBackReward != null)
                    {
                        this.<>f__this.m_preorderCardBackReward.HideCardBackReward();
                    }
                    if (this.<>f__this.m_availableDateText != null)
                    {
                        this.<>f__this.m_availableDateText.gameObject.SetActive(false);
                        this.<>f__this.m_availableDateText.transform.localScale = this.<>f__this.m_availableDateTextOrigScale;
                    }
                    if (!this.<>f__this.IsContentActive() || (this.<>f__this.m_selectedBoosterId == 0))
                    {
                        goto Label_0295;
                    }
                    this.<isPreorder>__1 = StoreManager.Get().IsBoosterPreorderActive(this.<>f__this.m_selectedBoosterId, out this.<preOrderBundle>__0);
                    if (!this.<isPreorder>__1)
                    {
                        goto Label_0295;
                    }
                    break;

                case 1:
                    break;

                default:
                    goto Label_0295;
            }
            while (this.<>f__this.m_animatingLogo || this.<>f__this.m_animatingPacks)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (<>f__am$cache7 == null)
            {
                <>f__am$cache7 = new Predicate<Network.BundleItem>(GeneralStorePacksContent.<AnimatePreorderUI>c__Iterator1DE.<>m__195);
            }
            this.<cardBackItem>__2 = this.<preOrderBundle>__0.Items.Find(<>f__am$cache7);
            if (this.<cardBackItem>__2 != null)
            {
                this.<>f__this.m_preorderCardBackReward.SetCardBack(this.<cardBackItem>__2.ProductData);
                this.<>f__this.m_preorderCardBackReward.ShowCardBackReward();
                if (this.<>f__this.m_availableDateText != null)
                {
                    this.<packDef>__3 = this.<>f__this.GetStorePackDef(this.<>f__this.m_selectedBoosterId);
                    if (this.<packDef>__3 != null)
                    {
                        this.<>f__this.m_availableDateText.Text = this.<packDef>__3.m_preorderAvailableDateString;
                    }
                    this.<>f__this.m_availableDateText.gameObject.SetActive(true);
                    this.<>f__this.m_availableDateText.transform.localScale = (Vector3) (this.<>f__this.m_availableDateTextOrigScale * 0.01f);
                    object[] args = new object[] { "scale", this.<>f__this.m_availableDateTextOrigScale, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuad };
                    iTween.ScaleTo(this.<>f__this.m_availableDateText.gameObject, iTween.Hash(args));
                }
                this.$PC = -1;
            }
        Label_0295:
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
    private sealed class <AnimateSlamLogo>c__Iterator1DC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MeshRenderer <$>logo;
        internal GeneralStorePacksContent <>f__this;
        internal PlayMakerFSM <logoFSM>__0;
        internal MeshRenderer logo;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            object[] objArray2;
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    if ((this.logo != null) && this.logo.gameObject.activeInHierarchy)
                    {
                        break;
                    }
                    goto Label_0341;

                case 1:
                    break;

                case 2:
                    if (this.<>f__this.m_logoHoldTime <= 0f)
                    {
                        goto Label_01EF;
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_logoHoldTime);
                    this.$PC = 3;
                    goto Label_0343;

                case 3:
                    goto Label_01EF;

                case 4:
                    if (this.<logoFSM>__0 != null)
                    {
                        this.<logoFSM>__0.SendEvent("PostSlamIn");
                    }
                    this.<>f__this.gameObject.transform.localPosition = this.<>f__this.m_savedLocalPosition;
                    iTween.Stop(this.<>f__this.gameObject);
                    iTween.PunchScale(this.<>f__this.gameObject, this.<>f__this.m_punchAmount, this.<>f__this.m_logoDisplayPunchTime);
                    this.$current = new WaitForSeconds(this.<>f__this.m_logoDisplayPunchTime * 0.5f);
                    this.$PC = 5;
                    goto Label_0343;

                case 5:
                    this.<>f__this.m_animatingLogo = false;
                    this.$PC = -1;
                    goto Label_0341;

                default:
                    goto Label_0341;
            }
            while (this.<>f__this.m_animatingLogo)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0343;
            }
            this.<>f__this.m_animatingLogo = true;
            this.<logoFSM>__0 = this.logo.GetComponent<PlayMakerFSM>();
            this.logo.transform.localPosition = this.<>f__this.m_logoAnimationStartBone.transform.localPosition;
            object[] args = new object[] { "position", this.logo.transform.localPosition - this.<>f__this.m_logoAppearOffset, "easetype", iTween.EaseType.easeInQuint, "time", this.<>f__this.m_logoIntroTime, "islocal", true };
            iTween.MoveFrom(this.logo.gameObject, iTween.Hash(args));
            AnimationUtil.FadeTexture(this.logo, 0f, 1f, this.<>f__this.m_logoIntroTime, 0f, null);
            if (this.<logoFSM>__0 != null)
            {
                this.<logoFSM>__0.SendEvent("FadeIn");
            }
            this.$current = new WaitForSeconds(this.<>f__this.m_logoIntroTime);
            this.$PC = 2;
            goto Label_0343;
        Label_01EF:
            objArray2 = new object[] { "position", this.<>f__this.m_logoAnimationEndBone.transform.localPosition, "easetype", iTween.EaseType.easeInQuint, "time", this.<>f__this.m_logoOutroTime, "islocal", true };
            iTween.MoveTo(this.logo.gameObject, iTween.Hash(objArray2));
            this.$current = new WaitForSeconds(this.<>f__this.m_logoOutroTime);
            this.$PC = 4;
            goto Label_0343;
        Label_0341:
            return false;
        Label_0343:
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
    private sealed class <HandleGoldPackBuyButtonDoubleClick>c__AnonStorey342
    {
        internal GeneralStorePacksContent <>f__this;
        internal GeneralStorePackBuyButton button;

        internal void <>m__190(int quantity)
        {
            this.<>f__this.m_parentStore.ActivateCover(false);
            this.<>f__this.m_currentGoldPackQuantity = quantity;
            NoGTAPPTransactionData currentGTAPPTransactionData = this.<>f__this.GetCurrentGTAPPTransactionData();
            this.button.UpdateFromGTAPP(currentGTAPPTransactionData);
            this.<>f__this.SetCurrentGoldBundle(currentGTAPPTransactionData);
        }

        internal void <>m__191()
        {
            this.<>f__this.m_parentStore.ActivateCover(false);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowStandardBuyButtons>c__AnonStorey340
    {
        internal GeneralStorePacksContent <>f__this;
        internal GeneralStorePackBuyButton goldButton;

        internal void <>m__188(UIEvent e)
        {
            if (this.<>f__this.IsContentActive())
            {
                this.<>f__this.HandleGoldPackBuyButtonClick();
                this.<>f__this.SelectPackBuyButton(this.goldButton);
            }
        }

        internal void <>m__189(UIEvent e)
        {
            this.<>f__this.HandleGoldPackBuyButtonDoubleClick(this.goldButton);
        }

        internal void <>m__18A()
        {
            this.<>f__this.HandleGoldPackBuyButtonClick();
            this.<>f__this.SelectPackBuyButton(this.goldButton);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowStandardBuyButtons>c__AnonStorey341
    {
        internal GeneralStorePacksContent <>f__this;
        internal int bundleIndex;
        internal GeneralStorePackBuyButton moneyButton;

        internal void <>m__18C(UIEvent e)
        {
            if (this.<>f__this.IsContentActive())
            {
                this.<>f__this.HandleMoneyPackBuyButtonClick(this.bundleIndex);
                this.<>f__this.SelectPackBuyButton(this.moneyButton);
            }
        }

        internal void <>m__18D()
        {
            this.<>f__this.HandleMoneyPackBuyButtonClick(this.bundleIndex);
            this.<>f__this.SelectPackBuyButton(this.moneyButton);
        }
    }

    public enum LogoAnimation
    {
        None,
        Slam,
        Fade
    }

    [Serializable]
    public class ToggleableButtonFrame
    {
        public GameObject m_IBar;
        public GameObject m_Middle;
    }
}

