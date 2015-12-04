using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CraftingTray : MonoBehaviour
{
    public UIBButton m_doneButton;
    private int m_dustAmount;
    public HighlightState m_highlight;
    public PegUIElement m_massDisenchantButton;
    public Material m_massDisenchantDisabledMaterial;
    public Material m_massDisenchantMaterial;
    public GameObject m_massDisenchantMesh;
    public UberText m_massDisenchantText;
    public UberText m_potentialDustAmount;
    public CheckBox m_showGoldenCheckbox;
    public CheckBox m_showSoulboundCheckbox;
    private static PlatformDependentValue<int> MASS_DISENCHANT_MATERIAL_TO_SWITCH;
    private static CraftingTray s_instance;

    static CraftingTray()
    {
        PlatformDependentValue<int> value2 = new PlatformDependentValue<int>(PlatformCategory.Screen) {
            PC = 0,
            Phone = 1
        };
        MASS_DISENCHANT_MATERIAL_TO_SWITCH = value2;
    }

    private void Awake()
    {
        s_instance = this;
    }

    public static CraftingTray Get()
    {
        return s_instance;
    }

    public void Hide()
    {
        PresenceMgr.Get().SetPrevStatus();
        CollectionManagerDisplay.Get().HideCraftingTray();
        CollectionManagerDisplay.Get().m_pageManager.HideMassDisenchant();
    }

    public bool IsShown()
    {
        return base.gameObject.activeInHierarchy;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnDoneButtonReleased(UIEvent e)
    {
        this.Hide();
    }

    private void OnMassDisenchantButtonOut(UIEvent e)
    {
        if (!CollectionManagerDisplay.Get().m_pageManager.IsShowingMassDisenchant())
        {
            if (int.Parse(this.m_potentialDustAmount.Text) > 0)
            {
                this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            }
            else
            {
                this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            }
        }
    }

    private void OnMassDisenchantButtonOver(UIEvent e)
    {
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
        SoundManager.Get().LoadAndPlay("Hub_Mouseover");
    }

    private void OnMassDisenchantButtonReleased(UIEvent e)
    {
        if (!CollectionManagerDisplay.Get().m_pageManager.ArePagesTurning())
        {
            if (CollectionManagerDisplay.Get().m_pageManager.IsShowingMassDisenchant())
            {
                CollectionManagerDisplay.Get().m_pageManager.HideMassDisenchant();
                this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
            }
            else
            {
                CollectionManagerDisplay.Get().m_pageManager.ShowMassDisenchant();
                base.StartCoroutine(MassDisenchant.Get().StartHighlight());
            }
            SoundManager.Get().LoadAndPlay("Hub_Click");
        }
    }

    public void SetMassDisenchantAmount()
    {
        if (base.gameObject.activeSelf)
        {
            base.StartCoroutine(this.SetMassDisenchantAmountWhenReady());
        }
    }

    [DebuggerHidden]
    private IEnumerator SetMassDisenchantAmountWhenReady()
    {
        return new <SetMassDisenchantAmountWhenReady>c__Iterator47 { <>f__this = this };
    }

    public void Show()
    {
        Enum[] args = new Enum[] { PresenceStatus.CRAFTING };
        PresenceMgr.Get().SetStatus(args);
        this.SetMassDisenchantAmount();
        CollectionManagerDisplay.Get().m_pageManager.ShowCraftingModeCards(!this.m_showSoulboundCheckbox.IsChecked(), this.m_showGoldenCheckbox.IsChecked());
    }

    private void Start()
    {
        this.m_doneButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDoneButtonReleased));
        this.m_massDisenchantButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnMassDisenchantButtonReleased));
        this.m_massDisenchantButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnMassDisenchantButtonOver));
        this.m_massDisenchantButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnMassDisenchantButtonOut));
        this.m_showGoldenCheckbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleShowGolden));
        this.m_showGoldenCheckbox.SetChecked(false);
        this.m_showSoulboundCheckbox.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.ToggleShowSoulbound));
        this.m_showSoulboundCheckbox.SetChecked(false);
        this.SetMassDisenchantAmount();
    }

    private void ToggleShowGolden(UIEvent e)
    {
        bool showGolden = this.m_showGoldenCheckbox.IsChecked();
        CollectionManagerDisplay.Get().m_pageManager.ShowCraftingModeCards(!this.m_showSoulboundCheckbox.IsChecked(), showGolden);
        if (showGolden)
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_on", base.gameObject);
        }
        else
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_off", base.gameObject);
        }
    }

    private void ToggleShowSoulbound(UIEvent e)
    {
        bool flag = this.m_showSoulboundCheckbox.IsChecked();
        CollectionManagerDisplay.Get().m_pageManager.ShowCraftingModeCards(!flag, this.m_showGoldenCheckbox.IsChecked());
        if (flag)
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_on", base.gameObject);
        }
        else
        {
            SoundManager.Get().LoadAndPlay("checkbox_toggle_off", base.gameObject);
        }
    }

    public void UpdateMassDisenchantAmount()
    {
        if (this.m_dustAmount > 0)
        {
            Material[] materials = this.m_massDisenchantMesh.GetComponent<Renderer>().materials;
            materials[(int) MASS_DISENCHANT_MATERIAL_TO_SWITCH] = this.m_massDisenchantMaterial;
            this.m_massDisenchantMesh.GetComponent<Renderer>().materials = materials;
            this.m_highlight.gameObject.SetActive(true);
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
            this.m_massDisenchantButton.SetEnabled(true);
            this.m_massDisenchantText.gameObject.SetActive(true);
            this.m_potentialDustAmount.gameObject.SetActive(true);
        }
        else
        {
            Material[] materialArray2 = this.m_massDisenchantMesh.GetComponent<Renderer>().materials;
            materialArray2[(int) MASS_DISENCHANT_MATERIAL_TO_SWITCH] = this.m_massDisenchantDisabledMaterial;
            this.m_massDisenchantMesh.GetComponent<Renderer>().materials = materialArray2;
            this.m_highlight.gameObject.SetActive(false);
            this.m_massDisenchantButton.SetEnabled(false);
            this.m_massDisenchantText.gameObject.SetActive(false);
            this.m_potentialDustAmount.gameObject.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <SetMassDisenchantAmountWhenReady>c__Iterator47 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingTray <>f__this;
        internal int <amount>__0;

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
                    if (MassDisenchant.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    MassDisenchant.Get().UpdateContents(CollectionManager.Get().GetMassDisenchantArtStacks());
                    this.<amount>__0 = MassDisenchant.Get().GetTotalAmount();
                    this.<>f__this.m_dustAmount = this.<amount>__0;
                    this.<>f__this.m_potentialDustAmount.Text = this.<amount>__0.ToString();
                    this.<>f__this.UpdateMassDisenchantAmount();
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

