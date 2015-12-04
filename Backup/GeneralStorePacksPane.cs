using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStorePacksPane : GeneralStorePane
{
    [CompilerGenerated]
    private static Comparison<GeneralStorePackSelectorButton> <>f__am$cache5;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_boosterSelectionSound;
    private bool m_initializeFirstPack;
    private List<GeneralStorePackSelectorButton> m_packButtons = new List<GeneralStorePackSelectorButton>();
    [SerializeField]
    private Vector3 m_packButtonSpacing;
    private GeneralStorePacksContent m_packsContent;

    private void Awake()
    {
        this.m_packsContent = base.m_parentContent as GeneralStorePacksContent;
        if (this.m_packsContent == null)
        {
            Debug.LogError("m_packsContent is not the correct type: GeneralStorePacksContent");
        }
    }

    private void OnPackSelectorButtonClicked(GeneralStorePackSelectorButton btn, int boosterId)
    {
        if (base.m_parentContent.IsContentActive())
        {
            this.m_packsContent.SetBoosterId(boosterId, false);
            foreach (GeneralStorePackSelectorButton button in this.m_packButtons)
            {
                button.Unselect();
            }
            btn.Select();
            Options.Get().SetInt(Option.LAST_SELECTED_STORE_BOOSTER_ID, (int) btn.GetBoosterId());
            if (!string.IsNullOrEmpty(this.m_boosterSelectionSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_boosterSelectionSound));
            }
        }
    }

    public override void OnPurchaseFinished()
    {
        this.UpdatePackButtonRecommendedIndicators();
    }

    private void SetupInitialSelectedPack()
    {
        BoosterDbId iNVALID = BoosterDbId.INVALID;
        if (this.ShouldResetPackSelection())
        {
            Options.Get().SetInt(Option.LAST_SELECTED_STORE_BOOSTER_ID, 0);
        }
        else
        {
            iNVALID = (BoosterDbId) Options.Get().GetInt(Option.LAST_SELECTED_STORE_BOOSTER_ID, 0);
        }
        foreach (GeneralStorePackSelectorButton button in this.m_packButtons)
        {
            if (button.GetBoosterId() == iNVALID)
            {
                this.m_packsContent.SetBoosterId((int) iNVALID, true);
                button.Select();
                break;
            }
        }
    }

    private bool ShouldResetPackSelection()
    {
        List<Network.Bundle> list = StoreManager.Get().GetAllBundlesForProduct(ProductType.PRODUCT_TYPE_BOOSTER, 0, 0);
        char[] separator = new char[] { ':' };
        List<string> list2 = new List<string>(Options.Get().GetString(Option.SEEN_PACK_PRODUCT_LIST, string.Empty).Split(separator));
        bool flag = false;
        foreach (Network.Bundle bundle in list)
        {
            if (!list2.Contains(bundle.ProductID))
            {
                list2.Add(bundle.ProductID);
                flag = true;
            }
        }
        Options.Get().SetString(Option.SEEN_PACK_PRODUCT_LIST, string.Join(":", list2.ToArray()));
        return flag;
    }

    private void SortPackButtons()
    {
        if (<>f__am$cache5 == null)
        {
            <>f__am$cache5 = delegate (GeneralStorePackSelectorButton lhs, GeneralStorePackSelectorButton rhs) {
                bool flag = lhs.IsRecommendedForNewPlayer();
                bool flag2 = rhs.IsRecommendedForNewPlayer();
                if (flag != flag2)
                {
                    return !flag ? 1 : -1;
                }
                bool flag3 = lhs.IsPreorder();
                bool flag4 = rhs.IsPreorder();
                if (flag3 != flag4)
                {
                    return !flag3 ? 1 : -1;
                }
                bool flag5 = lhs.IsLatestExpansion();
                bool flag6 = rhs.IsLatestExpansion();
                if (flag5 != flag6)
                {
                    return !flag5 ? 1 : -1;
                }
                return Mathf.Clamp((int) (lhs.GetBoosterId() - rhs.GetBoosterId()), -1, 1);
            };
        }
        this.m_packButtons.Sort(<>f__am$cache5);
    }

    private void Start()
    {
        Map<int, StorePackDef> storePackDefs = this.m_packsContent.GetStorePackDefs();
        int boosterId = this.m_packsContent.GetBoosterId();
        foreach (KeyValuePair<int, StorePackDef> pair in storePackDefs)
        {
            <Start>c__AnonStorey346 storey = new <Start>c__AnonStorey346 {
                <>f__this = this,
                id = pair.Key
            };
            StorePackDef def = pair.Value;
            GameObject child = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(def.m_buttonPrefab), false, false);
            GameUtils.SetParent(child, base.m_paneContainer, true);
            SceneUtils.SetLayer(child, base.m_paneContainer.layer);
            storey.newPackButton = child.GetComponent<GeneralStorePackSelectorButton>();
            storey.newPackButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storey.<>m__197));
            storey.newPackButton.SetBoosterId((BoosterDbId) storey.id);
            if (storey.id == boosterId)
            {
                storey.newPackButton.Select();
            }
            this.m_packButtons.Add(storey.newPackButton);
        }
        this.UpdatePackButtonPositions();
    }

    public override void StoreShown(bool isCurrent)
    {
        if (!this.m_initializeFirstPack)
        {
            this.m_initializeFirstPack = true;
            this.SetupInitialSelectedPack();
        }
        this.UpdatePackButtonPositions();
        this.UpdatePackButtonRecommendedIndicators();
    }

    private void UpdatePackButtonPositions()
    {
        this.SortPackButtons();
        GeneralStorePackSelectorButton[] buttonArray = this.m_packButtons.ToArray();
        int index = 0;
        int num2 = 0;
        while (index < buttonArray.Length)
        {
            GeneralStorePackSelectorButton button = buttonArray[index];
            bool flag = button.IsPurchasable();
            button.gameObject.SetActive(flag);
            if (flag)
            {
                button.transform.localPosition = this.m_packButtonSpacing * num2++;
            }
            index++;
        }
    }

    private void UpdatePackButtonRecommendedIndicators()
    {
        GeneralStorePackSelectorButton[] buttonArray = this.m_packButtons.ToArray();
        for (int i = 0; i < buttonArray.Length; i++)
        {
            buttonArray[i].UpdateRibbonIndicator();
        }
    }

    [CustomEditField(Sections="Layout")]
    public Vector3 PackButtonSpacing
    {
        get
        {
            return this.m_packButtonSpacing;
        }
        set
        {
            this.m_packButtonSpacing = value;
            this.UpdatePackButtonPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <Start>c__AnonStorey346
    {
        internal GeneralStorePacksPane <>f__this;
        internal int id;
        internal GeneralStorePackSelectorButton newPackButton;

        internal void <>m__197(UIEvent e)
        {
            this.<>f__this.OnPackSelectorButtonClicked(this.newPackButton, this.id);
        }
    }
}

