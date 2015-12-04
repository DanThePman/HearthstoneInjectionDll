using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreAdventurePane : GeneralStorePane
{
    private List<GeneralStoreAdventureSelectorButton> m_adventureButtons = new List<GeneralStoreAdventureSelectorButton>();
    [SerializeField]
    private Vector3 m_adventureButtonSpacing;
    private GeneralStoreAdventureContent m_adventureContent;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_adventureSelectionSound;
    private bool m_initializeFirstAdv;

    private void Awake()
    {
        this.m_adventureContent = base.m_parentContent as GeneralStoreAdventureContent;
        if (this.m_adventureContent == null)
        {
            Debug.LogError("m_adventureContent is not the correct type: GeneralStoreAdventureContent");
        }
    }

    private void OnAdventureSelectorButtonClicked(GeneralStoreAdventureSelectorButton btn, AdventureDbId adventureType)
    {
        if (base.m_parentContent.IsContentActive() && btn.IsAvailable())
        {
            this.m_adventureContent.SetAdventureType(adventureType, false);
            foreach (GeneralStoreAdventureSelectorButton button in this.m_adventureButtons)
            {
                button.Unselect();
            }
            btn.Select();
            Options.Get().SetInt(Option.LAST_SELECTED_STORE_ADVENTURE_ID, (int) btn.GetAdventureType());
            if (!string.IsNullOrEmpty(this.m_adventureSelectionSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_adventureSelectionSound));
            }
        }
    }

    protected override void OnRefresh()
    {
        foreach (GeneralStoreAdventureSelectorButton button in this.m_adventureButtons)
        {
            button.UpdateState();
        }
    }

    private void SetupInitialSelectedAdventure()
    {
        AdventureDbId @int = (AdventureDbId) Options.Get().GetInt(Option.LAST_SELECTED_STORE_ADVENTURE_ID, 0);
        Network.Bundle bundle = null;
        bool productExists = false;
        StoreManager.Get().GetAvailableAdventureBundle(@int, out bundle, out productExists);
        if (!productExists)
        {
            @int = AdventureDbId.INVALID;
        }
        foreach (GeneralStoreAdventureSelectorButton button in this.m_adventureButtons)
        {
            if (button.GetAdventureType() == @int)
            {
                this.m_adventureContent.SetAdventureType(@int, false);
                button.Select();
                break;
            }
        }
    }

    private void Start()
    {
        foreach (KeyValuePair<int, StoreAdventureDef> pair in this.m_adventureContent.GetStoreAdventureDefs())
        {
            <Start>c__AnonStorey33B storeyb = new <Start>c__AnonStorey33B {
                <>f__this = this,
                advType = pair.Key
            };
            string storeButtonPrefab = pair.Value.m_storeButtonPrefab;
            GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(storeButtonPrefab), true, false);
            if (obj2 != null)
            {
                storeyb.advButton = obj2.GetComponent<GeneralStoreAdventureSelectorButton>();
                if (storeyb.advButton == null)
                {
                    Debug.LogError(string.Format("{0} does not contain GeneralStoreAdventureSelectorButton component.", storeButtonPrefab));
                    UnityEngine.Object.Destroy(obj2);
                }
                else
                {
                    GameUtils.SetParent(storeyb.advButton, base.m_paneContainer, true);
                    SceneUtils.SetLayer(storeyb.advButton, base.m_paneContainer.layer);
                    storeyb.advButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeyb.<>m__179));
                    storeyb.advButton.SetAdventureType(storeyb.advType);
                    this.m_adventureButtons.Add(storeyb.advButton);
                }
            }
        }
        this.UpdateAdventureButtonPositions();
    }

    public override void StoreShown(bool isCurrent)
    {
        if (!this.m_initializeFirstAdv)
        {
            this.m_initializeFirstAdv = true;
            this.SetupInitialSelectedAdventure();
        }
        this.UpdateAdventureButtonPositions();
    }

    private void UpdateAdventureButtonPositions()
    {
        GeneralStoreAdventureSelectorButton[] buttonArray = this.m_adventureButtons.ToArray();
        int index = 0;
        int num2 = 0;
        while (index < buttonArray.Length)
        {
            GeneralStoreAdventureSelectorButton button = buttonArray[index];
            button.transform.localPosition = this.m_adventureButtonSpacing * num2++;
            index++;
        }
    }

    [CustomEditField(Sections="Layout")]
    public Vector3 AdventureButtonSpacing
    {
        get
        {
            return this.m_adventureButtonSpacing;
        }
        set
        {
            this.m_adventureButtonSpacing = value;
            this.UpdateAdventureButtonPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <Start>c__AnonStorey33B
    {
        internal GeneralStoreAdventurePane <>f__this;
        internal GeneralStoreAdventureSelectorButton advButton;
        internal AdventureDbId advType;

        internal void <>m__179(UIEvent e)
        {
            this.<>f__this.OnAdventureSelectorButtonClicked(this.advButton, this.advType);
        }
    }
}

