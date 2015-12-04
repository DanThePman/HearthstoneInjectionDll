using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureChooserTray : MonoBehaviour
{
    private List<AdventureChooserButton> m_AdventureButtons = new List<AdventureChooserButton>();
    private bool m_AttemptedLoad;
    [SerializeField, CustomEditField(Sections="Choose Frame")]
    public UIBButton m_BackButton;
    [SerializeField]
    private float m_ButtonOffset = -2.5f;
    [SerializeField, CustomEditField(Sections="Choose Frame")]
    public PlayButton m_ChooseButton;
    [SerializeField, CustomEditField(Sections="Choose Frame")]
    public GameObject m_ChooseElementsContainer;
    private AdventureChooserDescription m_CurrentChooserDescription;
    [CustomEditField(Sections="Choose Frame", T=EditType.GAME_OBJECT), SerializeField]
    public string m_DefaultChooserButtonPrefab;
    [CustomEditField(Sections="Choose Frame", T=EditType.GAME_OBJECT), SerializeField]
    public string m_DefaultChooserSubButtonPrefab;
    [CustomEditField(Sections="Description")]
    public GameObject m_DescriptionContainer;
    private Map<AdventureDbId, Map<AdventureModeDbId, AdventureChooserDescription>> m_Descriptions = new Map<AdventureDbId, Map<AdventureModeDbId, AdventureChooserDescription>>();
    [CustomEditField(Sections="Description")]
    public UberText m_DescriptionTitleObject;
    private bool m_isStarted;
    [CustomEditField(Sections="Behavior Settings")]
    public bool m_OnlyOneExpands;
    [SerializeField, CustomEditField(Sections="Sub Scene")]
    public AdventureSubScene m_ParentSubScene;
    private AdventureChooserSubButton m_SelectedSubButton;
    private const int s_DefaultPortraitMaterialIndex = 0;
    private const string s_DefaultPortraitMaterialTextureName = "_MainTex";

    private void Awake()
    {
        this.m_ChooseButton.Disable();
        this.m_BackButton.AddEventListener(UIEventType.RELEASE, e => this.OnBackButton());
        this.m_ChooseButton.AddEventListener(UIEventType.RELEASE, e => this.ChangeSubScene());
        AdventureConfig.Get().AddSelectedModeChangeListener(new AdventureConfig.SelectedModeChange(this.OnSelectedModeChange));
        if (this.m_ChooseElementsContainer == null)
        {
            UnityEngine.Debug.LogError("m_ChooseElementsContainer cannot be null. Unable to create button.", this);
        }
        else
        {
            foreach (AdventureDef def in AdventureScene.Get().GetSortedAdventureDefs())
            {
                if (AdventureScene.Get().IsAdventureOpen(def.GetAdventureId()))
                {
                    this.CreateAdventureChooserButton(def);
                }
            }
            if (this.m_ParentSubScene != null)
            {
                this.m_ParentSubScene.SetIsLoaded(true);
            }
            this.OnButtonVisualUpdated();
            Navigation.PushUnique(new Navigation.NavigateBackHandler(AdventureChooserTray.OnNavigateBack));
            Box.Get().AddTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        }
    }

    private static void BackToMainMenu()
    {
        SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
    }

    private void ButtonExpanded(AdventureChooserButton button, bool expand)
    {
        if (expand)
        {
            AdventureConfig config = AdventureConfig.Get();
            foreach (AdventureChooserSubButton button2 in button.GetSubButtons())
            {
                if (config.IsFeaturedMode(button.GetAdventure(), button2.GetMode()))
                {
                    button2.Flash();
                }
            }
        }
    }

    private void ButtonModeSelected(AdventureChooserSubButton btn)
    {
        foreach (AdventureChooserButton button in this.m_AdventureButtons)
        {
            button.DisableSubButtonHighlights();
        }
        this.m_SelectedSubButton = btn;
        if (AdventureConfig.Get().MarkFeaturedMode(btn.GetAdventure(), btn.GetMode()))
        {
            btn.SetNewGlow(false);
        }
        AdventureConfig.Get().SetSelectedAdventureMode(btn.GetAdventure(), btn.GetMode());
        this.SetTitleText(btn.GetAdventure(), btn.GetMode());
    }

    private void ChangeSubScene()
    {
        this.m_AttemptedLoad = true;
        this.m_ChooseButton.SetText(GameStrings.Get("GLUE_LOADING"));
        this.m_ChooseButton.Disable();
        this.m_BackButton.SetEnabled(false);
        base.StartCoroutine(this.WaitThenChangeSubScene());
    }

    private void CreateAdventureChooserButton(AdventureDef advDef)
    {
        <CreateAdventureChooserButton>c__AnonStorey299 storey = new <CreateAdventureChooserButton>c__AnonStorey299 {
            <>f__this = this
        };
        string defaultChooserButtonPrefab = this.m_DefaultChooserButtonPrefab;
        if (!string.IsNullOrEmpty(advDef.m_ChooserButtonPrefab))
        {
            defaultChooserButtonPrefab = advDef.m_ChooserButtonPrefab;
        }
        storey.newbutton = GameUtils.LoadGameObjectWithComponent<AdventureChooserButton>(defaultChooserButtonPrefab);
        if (storey.newbutton != null)
        {
            GameUtils.SetParent(storey.newbutton, this.m_ChooseElementsContainer, false);
            AdventureDbId adventureId = advDef.GetAdventureId();
            storey.newbutton.gameObject.name = string.Format("AdventureChooserButton({0})", adventureId);
            storey.newbutton.SetAdventure(adventureId);
            storey.newbutton.SetButtonText(advDef.GetAdventureName());
            storey.newbutton.SetPortraitTexture(advDef.m_Texture);
            storey.newbutton.SetPortraitTiling(advDef.m_TextureTiling);
            storey.newbutton.SetPortraitOffset(advDef.m_TextureOffset);
            AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
            AdventureModeDbId clientChooserAdventureMode = AdventureConfig.Get().GetClientChooserAdventureMode(adventureId);
            if (selectedAdventure == adventureId)
            {
                storey.newbutton.Toggle = true;
            }
            List<AdventureSubDef> sortedSubDefs = advDef.GetSortedSubDefs();
            string defaultChooserSubButtonPrefab = this.m_DefaultChooserSubButtonPrefab;
            if (!string.IsNullOrEmpty(advDef.m_ChooserSubButtonPrefab))
            {
                defaultChooserSubButtonPrefab = advDef.m_ChooserSubButtonPrefab;
            }
            foreach (AdventureSubDef def in sortedSubDefs)
            {
                AdventureModeDbId adventureModeId = def.GetAdventureModeId();
                AdventureChooserSubButton button = storey.newbutton.CreateSubButton(adventureModeId, def, defaultChooserSubButtonPrefab, clientChooserAdventureMode == adventureModeId);
                if (button != null)
                {
                    bool active = storey.newbutton.Toggle && (clientChooserAdventureMode == adventureModeId);
                    if (active)
                    {
                        button.SetHighlight(true);
                        this.UpdateChooseButton(adventureId, adventureModeId);
                        this.SetTitleText(adventureId, adventureModeId);
                    }
                    else if (AdventureConfig.Get().IsFeaturedMode(adventureId, adventureModeId))
                    {
                        button.SetNewGlow(true);
                    }
                    button.SetDesaturate(!AdventureConfig.Get().CanPlayMode(adventureId, adventureModeId));
                    this.CreateAdventureChooserDescriptionFromPrefab(adventureId, def, active);
                }
            }
            storey.newbutton.AddVisualUpdatedListener(new AdventureChooserButton.VisualUpdated(this.OnButtonVisualUpdated));
            storey.index = this.m_AdventureButtons.Count;
            storey.newbutton.AddToggleListener(new AdventureChooserButton.Toggled(storey.<>m__6));
            storey.newbutton.AddModeSelectionListener(new AdventureChooserButton.ModeSelection(this.ButtonModeSelected));
            storey.newbutton.AddExpandedListener(new AdventureChooserButton.Expanded(this.ButtonExpanded));
            this.m_AdventureButtons.Add(storey.newbutton);
        }
    }

    private void CreateAdventureChooserDescriptionFromPrefab(AdventureDbId adventureId, AdventureSubDef subDef, bool active)
    {
        if (!string.IsNullOrEmpty((string) subDef.m_ChooserDescriptionPrefab))
        {
            Map<AdventureModeDbId, AdventureChooserDescription> map;
            if (!this.m_Descriptions.TryGetValue(adventureId, out map))
            {
                map = new Map<AdventureModeDbId, AdventureChooserDescription>();
                this.m_Descriptions[adventureId] = map;
            }
            string descText = subDef.GetDescription();
            string requiredText = null;
            if (!AdventureConfig.Get().CanPlayMode(adventureId, subDef.GetAdventureModeId()))
            {
                requiredText = subDef.GetRequirementsDescription();
            }
            AdventureChooserDescription child = GameUtils.LoadGameObjectWithComponent<AdventureChooserDescription>((string) subDef.m_ChooserDescriptionPrefab);
            if (child != null)
            {
                GameUtils.SetParent(child, this.m_DescriptionContainer, false);
                child.SetText(requiredText, descText);
                child.gameObject.SetActive(active);
                map[subDef.GetAdventureModeId()] = child;
                if (active)
                {
                    this.m_CurrentChooserDescription = child;
                }
            }
        }
    }

    private AdventureChooserDescription GetAdventureChooserDescription(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        Map<AdventureModeDbId, AdventureChooserDescription> map;
        AdventureChooserDescription description;
        if (!this.m_Descriptions.TryGetValue(adventureId, out map))
        {
            return null;
        }
        if (!map.TryGetValue(modeId, out description))
        {
            return null;
        }
        return description;
    }

    private void OnAdventureButtonToggled(AdventureChooserButton btn, bool toggled, int index)
    {
        btn.SetSelectSubButtonOnToggle(this.m_OnlyOneExpands);
        if (this.m_OnlyOneExpands)
        {
            if (toggled)
            {
                AdventureChooserButton[] buttonArray = this.m_AdventureButtons.ToArray();
                for (int i = 0; i < buttonArray.Length; i++)
                {
                    if (i != index)
                    {
                        buttonArray[i].Toggle = false;
                    }
                }
            }
        }
        else if (this.m_SelectedSubButton != null)
        {
            btn = this.m_AdventureButtons[index];
            if (btn.ContainsSubButton(this.m_SelectedSubButton))
            {
                this.m_SelectedSubButton.SetHighlight(toggled);
                if (!toggled)
                {
                    this.m_ChooseButton.Disable();
                }
                else if (!this.m_AttemptedLoad)
                {
                    this.m_ChooseButton.Enable();
                }
            }
        }
    }

    private void OnBackButton()
    {
        Navigation.GoBack();
    }

    private void OnBoxTransitionFinished(object userData)
    {
        if (this.m_isStarted && (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE))
        {
            if (this.m_ChooseButton.IsEnabled())
            {
                PlayMakerFSM component = this.m_ChooseButton.GetComponent<PlayMakerFSM>();
                if (component != null)
                {
                    component.SendEvent("Burst");
                }
            }
            else
            {
                this.ShowDisabledAdventureModeRequirementsWarning();
            }
        }
    }

    private void OnButtonVisualUpdated()
    {
        float num = 0f;
        AdventureChooserButton[] buttonArray = this.m_AdventureButtons.ToArray();
        for (int i = 0; i < buttonArray.Length; i++)
        {
            TransformUtil.SetLocalPosZ(buttonArray[i].transform, -num);
            num += buttonArray[i].GetFullButtonHeight() + this.m_ButtonOffset;
        }
    }

    private void OnDestroy()
    {
        if (AdventureConfig.Get() != null)
        {
            AdventureConfig.Get().RemoveSelectedModeChangeListener(new AdventureConfig.SelectedModeChange(this.OnSelectedModeChange));
        }
        if (Box.Get() != null)
        {
            Box.Get().RemoveTransitionFinishedListener(new Box.TransitionFinishedCallback(this.OnBoxTransitionFinished));
        }
        base.CancelInvoke("ShowDisabledAdventureModeRequirementsWarning");
    }

    private static bool OnNavigateBack()
    {
        BackToMainMenu();
        return true;
    }

    private void OnSelectedModeChange(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        AdventureChooserDescription adventureChooserDescription = this.GetAdventureChooserDescription(adventureId, modeId);
        if (this.m_CurrentChooserDescription != null)
        {
            this.m_CurrentChooserDescription.gameObject.SetActive(false);
        }
        this.m_CurrentChooserDescription = adventureChooserDescription;
        if (this.m_CurrentChooserDescription != null)
        {
            this.m_CurrentChooserDescription.gameObject.SetActive(true);
        }
        this.UpdateChooseButton(adventureId, modeId);
        if (this.m_ChooseButton.IsEnabled())
        {
            PlayMakerFSM component = this.m_ChooseButton.GetComponent<PlayMakerFSM>();
            if (component != null)
            {
                component.SendEvent("Burst");
            }
        }
        if (!AdventureConfig.Get().CanPlayMode(adventureId, modeId))
        {
            if (!this.m_isStarted)
            {
                base.Invoke("ShowDisabledAdventureModeRequirementsWarning", 0f);
            }
            else
            {
                this.ShowDisabledAdventureModeRequirementsWarning();
            }
        }
    }

    private void SetTitleText(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        string locString = GameUtils.GetAdventureDataRecord((int) adventureId, (int) modeId).GetLocString("NAME");
        this.m_DescriptionTitleObject.Text = locString;
    }

    private void ShowDisabledAdventureModeRequirementsWarning()
    {
        base.CancelInvoke("ShowDisabledAdventureModeRequirementsWarning");
        if ((this.m_isStarted && (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)) && ((this.m_ChooseButton != null) && !this.m_ChooseButton.IsEnabled()))
        {
            AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
            AdventureModeDbId selectedMode = AdventureConfig.Get().GetSelectedMode();
            if (!AdventureConfig.Get().CanPlayMode(selectedAdventure, selectedMode))
            {
                int adventureId = (int) selectedAdventure;
                int modeId = (int) selectedMode;
                DbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord(adventureId, modeId);
                string locString = adventureDataRecord.GetLocString("REQUIREMENTS_DESCRIPTION");
                if (!string.IsNullOrEmpty(locString))
                {
                    Error.AddWarning(adventureDataRecord.GetLocString("NAME"), locString, new object[0]);
                }
            }
        }
    }

    private void Start()
    {
        this.m_isStarted = true;
    }

    private void UpdateChooseButton(AdventureDbId adventureId, AdventureModeDbId modeId)
    {
        if (!this.m_AttemptedLoad && AdventureConfig.Get().CanPlayMode(adventureId, modeId))
        {
            this.m_ChooseButton.SetText(GameStrings.Get("GLOBAL_ADVENTURE_CHOOSE_BUTTON_TEXT"));
            if (!this.m_ChooseButton.IsEnabled())
            {
                this.m_ChooseButton.Enable();
            }
        }
        else
        {
            this.m_ChooseButton.SetText(GameStrings.Get("GLUE_QUEST_LOG_CLASS_LOCKED"));
            this.m_ChooseButton.Disable();
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenChangeSubScene()
    {
        return new <WaitThenChangeSubScene>c__Iterator0();
    }

    [CustomEditField(Sections="Behavior Settings")]
    public float ButtonOffset
    {
        get
        {
            return this.m_ButtonOffset;
        }
        set
        {
            this.m_ButtonOffset = value;
            this.OnButtonVisualUpdated();
        }
    }

    [CompilerGenerated]
    private sealed class <CreateAdventureChooserButton>c__AnonStorey299
    {
        internal AdventureChooserTray <>f__this;
        internal int index;
        internal AdventureChooserButton newbutton;

        internal void <>m__6(bool toggle)
        {
            this.<>f__this.OnAdventureButtonToggled(this.newbutton, toggle, this.index);
        }
    }

    [CompilerGenerated]
    private sealed class <WaitThenChangeSubScene>c__Iterator0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    AdventureConfig.Get().ChangeSubSceneToSelectedAdventure();
                    this.$PC = -1;
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
}

