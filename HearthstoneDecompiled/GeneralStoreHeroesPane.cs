using HutongGames.PlayMaker;
using PegasusUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreHeroesPane : GeneralStorePane
{
    [CompilerGenerated]
    private static Comparison<GeneralStoreHeroesSelectorButton> <>f__am$cache1C;
    [CompilerGenerated]
    private static Comparison<GeneralStoreHeroesSelectorButton> <>f__am$cache1D;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_buttonsSlideUpSound;
    private int m_currentPurchaseRemovalIdx;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_heroAnimationFrame;
    private GeneralStoreHeroesContent m_heroesContent;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_heroPurchasedFrame;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_heroSelectionSound;
    [CustomEditField(Sections="Prefabs", T=EditType.GAME_OBJECT)]
    public string m_heroUnpurchasedFrame;
    private bool m_initializeFirstHero;
    [CustomEditField(Sections="Layout")]
    public float m_maxPurchasedHeightAdd;
    [CustomEditField(Sections="Purchase Flow")]
    public GameObject m_purchaseAnimationBlocker;
    [CustomEditField(Sections="Animations")]
    public GameObject m_purchaseAnimationEndBone;
    [CustomEditField(Sections="Animations")]
    public Vector3 m_purchaseAnimationMidPointWorldOffset = new Vector3(0f, 0f, -7.5f);
    [CustomEditField(Sections="Animations")]
    public string m_purchaseAnimationName = "HeroSkin_HeroHolderPopOut";
    [CustomEditField(Sections="Layout/Purchased Section")]
    public GameObject m_purchasedButtonContainer;
    [CustomEditField(Sections="Layout")]
    public float m_purchasedHeroButtonHeight;
    [CustomEditField(Sections="Layout")]
    public float m_purchasedHeroButtonHeightPadding = 0.01f;
    [SerializeField]
    private Vector3 m_purchasedHeroButtonSpacing = new Vector3(0f, 0f, 0.092f);
    private List<GeneralStoreHeroesSelectorButton> m_purchasedHeroesButtons = new List<GeneralStoreHeroesSelectorButton>();
    [CustomEditField(Sections="Layout/Purchased Section")]
    public MultiSliceElement m_purchasedSection;
    [CustomEditField(Sections="Layout/Purchased Section")]
    public GameObject m_purchasedSectionBottom;
    private List<GameObject> m_purchasedSectionMidMeshes = new List<GameObject>();
    [CustomEditField(Sections="Layout/Purchased Section")]
    public GameObject m_purchasedSectionMidTemplate;
    [CustomEditField(Sections="Layout/Purchased Section")]
    public Vector3 m_purchasedSectionOffset = new Vector3(0f, 0f, 0.145f);
    [CustomEditField(Sections="Layout/Purchased Section")]
    public GameObject m_purchasedSectionTop;
    [CustomEditField(Sections="Scroll")]
    public UIBScrollable m_scrollUpdate;
    [CustomEditField(Sections="Layout")]
    public float m_unpurchasedHeroButtonHeight = 0.0275f;
    [SerializeField]
    private Vector3 m_unpurchasedHeroButtonSpacing = new Vector3(0f, 0f, 0.285f);
    private List<GeneralStoreHeroesSelectorButton> m_unpurchasedHeroesButtons = new List<GeneralStoreHeroesSelectorButton>();

    [DebuggerHidden]
    private IEnumerator AnimateShowPurchase(int btnIndex)
    {
        return new <AnimateShowPurchase>c__Iterator1DA { btnIndex = btnIndex, <$>btnIndex = btnIndex, <>f__this = this };
    }

    private void Awake()
    {
        this.m_heroesContent = base.m_parentContent as GeneralStoreHeroesContent;
        this.PopulateHeroes();
        this.m_purchaseAnimationBlocker.SetActive(false);
        StoreManager.Get().RegisterSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.OnItemPurchased));
        CheatMgr.Get().RegisterCheatHandler("herobuy", new CheatMgr.ProcessCheatCallback(this.OnHeroPurchased_cheat), null, null, null);
    }

    private GeneralStoreHeroesSelectorButton CreateNewHeroButton(string heroCardId, Network.Bundle heroBundle)
    {
        return ((heroBundle != null) ? this.CreateUnpurchasedHeroButton(heroCardId, heroBundle) : this.CreatePurchasedHeroButton(heroCardId, heroBundle));
    }

    private GeneralStoreHeroesSelectorButton CreatePurchasedHeroButton(string heroCardId, Network.Bundle heroBundle)
    {
        GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(this.m_heroPurchasedFrame), true, false);
        GeneralStoreHeroesSelectorButton component = obj2.GetComponent<GeneralStoreHeroesSelectorButton>();
        if (component == null)
        {
            UnityEngine.Debug.LogError("Prefab does not contain GeneralStoreHeroesSelectorButton component.");
            UnityEngine.Object.Destroy(obj2);
            return null;
        }
        GameUtils.SetParent(component, this.m_purchasedButtonContainer, true);
        SceneUtils.SetLayer(component, this.m_purchasedButtonContainer.layer);
        this.m_purchasedHeroesButtons.Add(component);
        this.SetupHeroButton(heroCardId, component);
        return component;
    }

    private GeneralStoreHeroesSelectorButton CreateUnpurchasedHeroButton(string heroCardId, Network.Bundle heroBundle)
    {
        GameObject obj2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(this.m_heroUnpurchasedFrame), true, false);
        GeneralStoreHeroesSelectorButton component = obj2.GetComponent<GeneralStoreHeroesSelectorButton>();
        if (component == null)
        {
            UnityEngine.Debug.LogError("Prefab does not contain GeneralStoreHeroesSelectorButton component.");
            UnityEngine.Object.Destroy(obj2);
            return null;
        }
        GameUtils.SetParent(component, base.m_paneContainer, true);
        SceneUtils.SetLayer(component, base.m_paneContainer.layer);
        this.m_unpurchasedHeroesButtons.Add(component);
        this.SetupHeroButton(heroCardId, component);
        return component;
    }

    private void OnDestroy()
    {
        CheatMgr.Get().UnregisterCheatHandler("herobuy", new CheatMgr.ProcessCheatCallback(this.OnHeroPurchased_cheat));
        StoreManager.Get().RemoveSuccessfulPurchaseAckListener(new StoreManager.SuccessfulPurchaseAckCallback(this.OnItemPurchased));
    }

    private void OnHeroPurchased(int heroDbId)
    {
        <OnHeroPurchased>c__AnonStorey33F storeyf = new <OnHeroPurchased>c__AnonStorey33F {
            heroDbId = heroDbId
        };
        int btnIndex = this.m_unpurchasedHeroesButtons.FindIndex(new Predicate<GeneralStoreHeroesSelectorButton>(storeyf.<>m__185));
        if (btnIndex == -1)
        {
            UnityEngine.Debug.LogError(string.Format("Hero DB ID {0} does not exist in button list.", storeyf.heroDbId));
        }
        else
        {
            this.RunHeroPurchaseAnimation(btnIndex);
        }
    }

    private bool OnHeroPurchased_cheat(string func, string[] args, string rawArgs)
    {
        if (args.Length != 0)
        {
            int result = -1;
            if ((int.TryParse(args[0], out result) && (result >= 0)) && (result < this.m_unpurchasedHeroesButtons.Count))
            {
                this.RunHeroPurchaseAnimation(result);
            }
        }
        return true;
    }

    private void OnItemPurchased(Network.Bundle bundle, PaymentMethod purchaseMethod, object userData)
    {
        if ((bundle != null) && (bundle.Items != null))
        {
            foreach (Network.BundleItem item in bundle.Items)
            {
                if ((item != null) && (item.Product == ProductType.PRODUCT_TYPE_HERO))
                {
                    this.OnHeroPurchased(item.ProductData);
                    break;
                }
            }
        }
    }

    private void PopulateHeroes()
    {
        foreach (DbfRecord record in GameDbf.Hero.GetRecords())
        {
            if (record.GetBool("STORE_BOUGHT"))
            {
                Network.Bundle heroBundle = null;
                string heroId = record.GetString("CARD_ID");
                if (StoreManager.Get().GetHeroBundle(heroId, out heroBundle))
                {
                    this.CreateNewHeroButton(heroId, heroBundle);
                }
            }
        }
        this.PositionAllHeroButtons();
    }

    private void PositionAllHeroButtons()
    {
        this.PositionUnpurchasedHeroButtons();
        this.PositionPurchasedHeroButtons(true);
    }

    private void PositionPurchasedHeroButtons(bool sortAndSetSectionPos = true)
    {
        if (sortAndSetSectionPos)
        {
            if (<>f__am$cache1D == null)
            {
                <>f__am$cache1D = (lhs, rhs) => string.Compare(lhs.GetHeroId(), rhs.GetHeroId());
            }
            this.m_purchasedHeroesButtons.Sort(<>f__am$cache1D);
            this.m_purchasedSection.transform.localPosition = (this.m_unpurchasedHeroButtonSpacing * (this.m_unpurchasedHeroesButtons.Count - 1)) + this.m_purchasedSectionOffset;
        }
        for (int i = 0; i < this.m_purchasedHeroesButtons.Count; i++)
        {
            this.m_purchasedHeroesButtons[i].transform.localPosition = this.m_purchasedHeroButtonSpacing * i;
        }
        this.UpdatePurchasedSectionLayout();
    }

    private void PositionUnpurchasedHeroButtons()
    {
        if (<>f__am$cache1C == null)
        {
            <>f__am$cache1C = (lhs, rhs) => string.Compare(lhs.GetHeroId(), rhs.GetHeroId());
        }
        this.m_unpurchasedHeroesButtons.Sort(<>f__am$cache1C);
        for (int i = 0; i < this.m_unpurchasedHeroesButtons.Count; i++)
        {
            this.m_unpurchasedHeroesButtons[i].transform.localPosition = this.m_unpurchasedHeroButtonSpacing * i;
        }
    }

    public override void PrePaneSwappedIn()
    {
        this.SetupInitialSelectedHero();
    }

    private void RunHeroPurchaseAnimation(int btnIndex)
    {
        this.m_currentPurchaseRemovalIdx = btnIndex;
        base.StartCoroutine(this.AnimateShowPurchase(btnIndex));
    }

    private void SelectHero(GeneralStoreHeroesSelectorButton button)
    {
        foreach (GeneralStoreHeroesSelectorButton button2 in this.m_unpurchasedHeroesButtons)
        {
            button2.Unselect();
        }
        foreach (GeneralStoreHeroesSelectorButton button3 in this.m_purchasedHeroesButtons)
        {
            button3.Unselect();
        }
        button.Select();
        Options.Get().SetInt(Option.LAST_SELECTED_STORE_HERO_ID, button.GetHeroDbId());
        this.m_heroesContent.SelectHero(button.GetHeroId(), true);
        if (!string.IsNullOrEmpty(this.m_heroSelectionSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_heroSelectionSound));
        }
    }

    private void SetupHeroButton(string heroCardId, GeneralStoreHeroesSelectorButton heroButton)
    {
        <SetupHeroButton>c__AnonStorey33E storeye = new <SetupHeroButton>c__AnonStorey33E {
            heroButton = heroButton,
            <>f__this = this
        };
        int heroDbId = -1;
        DbfRecord cardRecord = GameUtils.GetCardRecord(heroCardId);
        if (cardRecord != null)
        {
            heroDbId = cardRecord.GetId();
        }
        storeye.heroButton.SetHeroIds(heroDbId, heroCardId);
        storeye.heroButton.SetPurchased(false);
        storeye.heroButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeye.<>m__181));
        DefLoader.Get().LoadFullDef(heroCardId, new DefLoader.LoadDefCallback<FullDef>(storeye.<>m__182));
    }

    private void SetupInitialSelectedHero()
    {
        if (!this.m_initializeFirstHero)
        {
            this.m_initializeFirstHero = true;
            int @int = Options.Get().GetInt(Option.LAST_SELECTED_STORE_HERO_ID, -1);
            if (@int != -1)
            {
                Network.Bundle heroBundle = null;
                StoreManager.Get().GetHeroBundle(@int, out heroBundle);
                List<GeneralStoreHeroesSelectorButton> list = new List<GeneralStoreHeroesSelectorButton>();
                list.AddRange(this.m_unpurchasedHeroesButtons);
                list.AddRange(this.m_purchasedHeroesButtons);
                foreach (GeneralStoreHeroesSelectorButton button in list)
                {
                    if (button.GetHeroDbId() == @int)
                    {
                        this.m_heroesContent.SelectHero(button.GetHeroId(), false);
                        button.Select();
                        break;
                    }
                }
            }
        }
    }

    private void UpdatePurchasedSectionLayout()
    {
        if (this.m_purchasedHeroesButtons.Count == 0)
        {
            this.m_purchasedButtonContainer.SetActive(false);
            this.m_purchasedSection.gameObject.SetActive(false);
        }
        else
        {
            this.m_purchasedButtonContainer.SetActive(true);
            this.m_purchasedSection.gameObject.SetActive(true);
            if (this.m_purchasedSectionMidMeshes.Count < this.m_purchasedHeroesButtons.Count)
            {
                int num = this.m_purchasedHeroesButtons.Count - this.m_purchasedSectionMidMeshes.Count;
                for (int i = 0; i < num; i++)
                {
                    GameObject item = (GameObject) GameUtils.Instantiate(this.m_purchasedSectionMidTemplate, this.m_purchasedSection.gameObject, true);
                    item.SetActive(true);
                    this.m_purchasedSectionMidMeshes.Add(item);
                }
            }
            this.m_purchasedSection.ClearSlices();
            this.m_purchasedSection.AddSlice(this.m_purchasedSectionTop);
            foreach (GameObject obj3 in this.m_purchasedSectionMidMeshes)
            {
                this.m_purchasedSection.AddSlice(obj3);
            }
            this.m_purchasedSection.AddSlice(this.m_purchasedSectionBottom);
            this.m_purchasedSection.UpdateSlices();
        }
    }

    [CustomEditField(Sections="Layout")]
    public Vector3 PurchasedHeroButtonSpacing
    {
        get
        {
            return this.m_purchasedHeroButtonSpacing;
        }
        set
        {
            this.m_purchasedHeroButtonSpacing = value;
            this.PositionAllHeroButtons();
        }
    }

    [CustomEditField(Sections="Layout")]
    public Vector3 UnpurchasedHeroButtonSpacing
    {
        get
        {
            return this.m_unpurchasedHeroButtonSpacing;
        }
        set
        {
            this.m_unpurchasedHeroButtonSpacing = value;
            this.PositionAllHeroButtons();
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateShowPurchase>c__Iterator1DA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>btnIndex;
        internal GeneralStoreHeroesPane <>f__this;
        internal GeneralStoreHeroesSelectorButton <animateBtn>__3;
        internal GameObject <animateBtnObj>__2;
        internal PlayMakerFSM <animation>__4;
        internal FsmBool <animComplete>__11;
        internal FsmString <animName>__10;
        internal GeneralStoreHeroesSelectorButton <btn>__14;
        internal Camera <cam>__8;
        internal FsmGameObject <camShake>__9;
        internal FsmVector3 <endPos>__7;
        internal int <i>__13;
        internal FsmVector3 <midPos>__6;
        internal GeneralStoreHeroesSelectorButton <newBtn>__12;
        internal GeneralStoreHeroesSelectorButton <removeBtn>__0;
        internal float <scrollPos>__1;
        internal FsmVector3 <startPos>__5;
        internal int btnIndex;

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
                    this.<>f__this.m_purchaseAnimationBlocker.SetActive(true);
                    this.<>f__this.m_scrollUpdate.Pause(true);
                    if (GeneralStore.Get().GetMode() == GeneralStoreMode.HEROES)
                    {
                        break;
                    }
                    GeneralStore.Get().SetMode(GeneralStoreMode.HEROES);
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    goto Label_06A1;

                case 1:
                    break;

                case 2:
                    this.<animateBtnObj>__2 = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(this.<>f__this.m_heroAnimationFrame), true, false);
                    this.<animateBtn>__3 = this.<animateBtnObj>__2.GetComponent<GeneralStoreHeroesSelectorButton>();
                    SceneUtils.SetLayer(this.<animateBtn>__3, GameLayer.PerspectiveUI);
                    this.<animateBtn>__3.transform.position = this.<removeBtn>__0.transform.position;
                    this.<animateBtn>__3.UpdatePortrait(this.<removeBtn>__0);
                    this.<animateBtn>__3.UpdateName(this.<removeBtn>__0);
                    this.<removeBtn>__0.gameObject.SetActive(false);
                    this.<animation>__4 = this.<animateBtn>__3.GetComponent<PlayMakerFSM>();
                    this.<startPos>__5 = this.<animation>__4.FsmVariables.FindFsmVector3("PopStartPos");
                    this.<midPos>__6 = this.<animation>__4.FsmVariables.FindFsmVector3("PopMidPos");
                    this.<endPos>__7 = this.<animation>__4.FsmVariables.FindFsmVector3("PopEndPos");
                    this.<startPos>__5.Value = this.<removeBtn>__0.transform.position;
                    this.<midPos>__6.Value = this.<removeBtn>__0.transform.position + this.<>f__this.m_purchaseAnimationMidPointWorldOffset;
                    this.<endPos>__7.Value = this.<>f__this.m_purchaseAnimationEndBone.transform.position;
                    this.<cam>__8 = CameraUtils.FindFirstByLayer(this.<>f__this.gameObject.layer);
                    if (this.<cam>__8 != null)
                    {
                        this.<camShake>__9 = this.<animation>__4.FsmVariables.FindFsmGameObject("CameraObjectShake");
                        this.<camShake>__9.Value = this.<cam>__8.gameObject;
                    }
                    this.<animName>__10 = this.<animation>__4.FsmVariables.FindFsmString("PopOutAnimationName");
                    this.<animName>__10.Value = this.<>f__this.m_purchaseAnimationName;
                    this.<animation>__4.SendEvent("PopOut");
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 3;
                    goto Label_06A1;

                case 3:
                    this.<>f__this.m_heroesContent.PlayCurrentHeroPurchaseEmote();
                    this.$current = null;
                    this.$PC = 4;
                    goto Label_06A1;

                case 4:
                    this.<animComplete>__11 = this.<animation>__4.FsmVariables.FindFsmBool("AnimationComplete");
                    goto Label_0392;

                case 5:
                    goto Label_0392;

                case 6:
                case 7:
                    while (!UniversalInputManager.Get().GetMouseButtonDown(0))
                    {
                        this.$current = null;
                        this.$PC = 7;
                        goto Label_06A1;
                    }
                    this.<animation>__4.SendEvent("EchoHero");
                    this.$current = null;
                    this.$PC = 8;
                    goto Label_06A1;

                case 8:
                    this.<animComplete>__11 = this.<animation>__4.FsmVariables.FindFsmBool("AnimationComplete");
                    goto Label_0488;

                case 9:
                    goto Label_0488;

                case 10:
                    UnityEngine.Object.Destroy(this.<removeBtn>__0.gameObject);
                    UnityEngine.Object.Destroy(this.<animateBtnObj>__2);
                    this.<animateBtnObj>__2 = null;
                    this.<>f__this.m_scrollUpdate.Pause(false);
                    this.<>f__this.m_purchaseAnimationBlocker.SetActive(false);
                    this.$PC = -1;
                    goto Label_069F;

                default:
                    goto Label_069F;
            }
            this.<removeBtn>__0 = this.<>f__this.m_unpurchasedHeroesButtons[this.btnIndex];
            this.<scrollPos>__1 = ((float) this.btnIndex) / ((float) ((this.<>f__this.m_unpurchasedHeroesButtons.Count + this.<>f__this.m_purchasedHeroesButtons.Count) - 1));
            this.<>f__this.m_scrollUpdate.SetScroll(this.<scrollPos>__1, iTween.EaseType.easeInOutCirc, 0.2f, false, true);
            this.$current = new WaitForSeconds(0.21f);
            this.$PC = 2;
            goto Label_06A1;
        Label_0392:
            if (!this.<animComplete>__11.Value)
            {
                this.$current = null;
                this.$PC = 5;
            }
            else
            {
                this.<newBtn>__12 = this.<>f__this.CreateNewHeroButton(this.<removeBtn>__0.GetHeroId(), null);
                this.<newBtn>__12.Select();
                this.<>f__this.m_unpurchasedHeroesButtons.Remove(this.<removeBtn>__0);
                this.<>f__this.PositionPurchasedHeroButtons(false);
                this.$current = new WaitForSeconds(0.25f);
                this.$PC = 6;
            }
            goto Label_06A1;
        Label_0488:
            while (!this.<animComplete>__11.Value)
            {
                this.$current = null;
                this.$PC = 9;
                goto Label_06A1;
            }
            this.<i>__13 = this.<>f__this.m_currentPurchaseRemovalIdx;
            while (this.<i>__13 < this.<>f__this.m_unpurchasedHeroesButtons.Count)
            {
                this.<btn>__14 = this.<>f__this.m_unpurchasedHeroesButtons[this.<i>__13];
                object[] objArray1 = new object[] { "position", this.<>f__this.m_unpurchasedHeroButtonSpacing * this.<i>__13, "islocal", true, "easetype", iTween.EaseType.easeInOutCirc, "time", 0.25f };
                iTween.MoveTo(this.<btn>__14.gameObject, iTween.Hash(objArray1));
                this.<i>__13++;
            }
            object[] args = new object[] { "position", (this.<>f__this.m_unpurchasedHeroButtonSpacing * (this.<>f__this.m_unpurchasedHeroesButtons.Count - 1)) + this.<>f__this.m_purchasedSectionOffset, "islocal", true, "easetype", iTween.EaseType.easeInOutCirc, "time", 0.25f };
            iTween.MoveTo(this.<>f__this.m_purchasedSection.gameObject, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.<>f__this.m_buttonsSlideUpSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.<>f__this.m_buttonsSlideUpSound));
            }
            this.$current = new WaitForSeconds(0.25f);
            this.$PC = 10;
            goto Label_06A1;
        Label_069F:
            return false;
        Label_06A1:
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
    private sealed class <OnHeroPurchased>c__AnonStorey33F
    {
        internal int heroDbId;

        internal bool <>m__185(GeneralStoreHeroesSelectorButton e)
        {
            return (e.GetHeroDbId() == this.heroDbId);
        }
    }

    [CompilerGenerated]
    private sealed class <SetupHeroButton>c__AnonStorey33E
    {
        internal GeneralStoreHeroesPane <>f__this;
        internal GeneralStoreHeroesSelectorButton heroButton;

        internal void <>m__181(UIEvent e)
        {
            this.<>f__this.SelectHero(this.heroButton);
        }

        internal void <>m__182(string cardId, FullDef fullDef, object data)
        {
            this.heroButton.UpdatePortrait(fullDef.GetEntityDef(), fullDef.GetCardDef());
            this.heroButton.UpdateName(fullDef.GetEntityDef().GetName());
        }
    }
}

