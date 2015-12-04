using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class MassDisenchant : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<CollectionCardStack.ArtStack> <>f__am$cache1B;
    public UberText m_detailsHeadlineText;
    public UberText m_detailsText;
    public NormalButton m_disenchantButton;
    public GameObject m_disenchantContainer;
    public List<DisenchantBar> m_doubleDisenchantBars;
    public GameObject m_doubleRoot;
    public UberText m_doubleSubHeadlineText;
    public MassDisenchantFX m_FX;
    public UberText m_headlineText;
    private int m_highestGlowBalls;
    public UIBButton m_infoButton;
    private Vector3 m_origDustScale;
    private Vector3 m_origTotalScale;
    public Material m_rarityBarGoldMaterial;
    public Mesh m_rarityBarGoldMesh;
    public Material m_rarityBarNormalMaterial;
    public Mesh m_rarityBarNormalMesh;
    public GameObject m_root;
    public List<DisenchantBar> m_singleDisenchantBars;
    public GameObject m_singleRoot;
    public UberText m_singleSubHeadlineText;
    public MassDisenchantSound m_sound;
    private int m_totalAmount;
    public UberText m_totalAmountText;
    private int m_totalCardsToDisenchant;
    private bool m_useSingle = true;
    private static MassDisenchant s_Instance;

    private void Awake()
    {
        s_Instance = this;
        this.m_headlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_HEADLINE");
        this.m_detailsHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS_HEADLINE");
        this.m_disenchantButton.SetText(GameStrings.Get("GLUE_MASS_DISENCHANT_BUTTON_TEXT"));
        if (this.m_detailsText != null)
        {
            this.m_detailsText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS");
        }
        if (this.m_singleSubHeadlineText != null)
        {
            this.m_singleSubHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_SUB_HEADLINE_TEXT");
        }
        if (this.m_doubleSubHeadlineText != null)
        {
            this.m_doubleSubHeadlineText.Text = GameStrings.Get("GLUE_MASS_DISENCHANT_SUB_HEADLINE_TEXT");
        }
        this.m_disenchantButton.SetUserOverYOffset(-0.04409015f);
        foreach (DisenchantBar bar in this.m_singleDisenchantBars)
        {
            bar.Init();
        }
        foreach (DisenchantBar bar2 in this.m_doubleDisenchantBars)
        {
            bar2.Init();
        }
        CollectionManager.Get().RegisterMassDisenchantListener(new CollectionManager.OnMassDisenchant(this.OnMassDisenchant));
    }

    private void BlockUI(bool block = true)
    {
        this.m_FX.m_blockInteraction.SetActive(block);
    }

    [DebuggerHidden]
    private IEnumerator DoDisenchantAnims(int maxGlowBalls, int disenchantTotal)
    {
        return new <DoDisenchantAnims>c__Iterator4F { disenchantTotal = disenchantTotal, maxGlowBalls = maxGlowBalls, <$>disenchantTotal = disenchantTotal, <$>maxGlowBalls = maxGlowBalls, <>f__this = this };
    }

    private float DrainBarAndDust(DisenchantBar bar, int drainRun, float duration, float rate)
    {
        float numCards = bar.GetNumCards();
        numCards -= ((drainRun + 1) * numCards) / (duration / rate);
        if (numCards < 0f)
        {
            numCards = 0f;
        }
        float amountDust = bar.GetAmountDust();
        amountDust -= ((drainRun + 1) * amountDust) / (duration / rate);
        if (amountDust < 0f)
        {
            amountDust = 0f;
        }
        bar.m_numCardsText.Text = Convert.ToInt32(numCards).ToString();
        bar.m_amountBar.GetComponent<Renderer>().material.SetFloat("_Percent", numCards / ((float) this.m_totalCardsToDisenchant));
        bar.m_amountText.Text = Convert.ToInt32(amountDust).ToString();
        return amountDust;
    }

    public static MassDisenchant Get()
    {
        return s_Instance;
    }

    private Vector3 GetRanBoxPt(GameObject box)
    {
        Vector3 localScale = box.transform.localScale;
        Vector3 position = box.transform.position;
        float x = UnityEngine.Random.Range((float) (-localScale.x / 2f), (float) (localScale.x / 2f));
        float y = UnityEngine.Random.Range((float) (-localScale.y / 2f), (float) (localScale.y / 2f));
        Vector3 vector3 = new Vector3(x, y, UnityEngine.Random.Range((float) (-localScale.z / 2f), (float) (localScale.z / 2f)));
        return (position + vector3);
    }

    private RarityFX GetRarityFX(DisenchantBar bar)
    {
        RarityFX yfx = new RarityFX();
        switch (bar.m_rarity)
        {
            case TAG_RARITY.RARE:
                yfx.burstFX = this.m_FX.m_burstFX_Rare;
                yfx.explodeFX = (UniversalInputManager.UsePhoneUI == null) ? BnetBar.Get().m_currencyFrame.m_explodeFX_Rare : ArcaneDustAmount.Get().m_explodeFX_Rare;
                yfx.glowBallMat = this.m_FX.m_glowBallMat_Rare;
                yfx.glowTrailMat = this.m_FX.m_glowTrailMat_Rare;
                return yfx;

            case TAG_RARITY.EPIC:
                yfx.burstFX = this.m_FX.m_burstFX_Epic;
                yfx.explodeFX = (UniversalInputManager.UsePhoneUI == null) ? BnetBar.Get().m_currencyFrame.m_explodeFX_Epic : ArcaneDustAmount.Get().m_explodeFX_Epic;
                yfx.glowBallMat = this.m_FX.m_glowBallMat_Epic;
                yfx.glowTrailMat = this.m_FX.m_glowTrailMat_Epic;
                return yfx;

            case TAG_RARITY.LEGENDARY:
                yfx.burstFX = this.m_FX.m_burstFX_Legendary;
                yfx.explodeFX = (UniversalInputManager.UsePhoneUI == null) ? BnetBar.Get().m_currencyFrame.m_explodeFX_Legendary : ArcaneDustAmount.Get().m_explodeFX_Legendary;
                yfx.glowBallMat = this.m_FX.m_glowBallMat_Legendary;
                yfx.glowTrailMat = this.m_FX.m_glowTrailMat_Legendary;
                return yfx;
        }
        yfx.burstFX = this.m_FX.m_burstFX_Common;
        yfx.explodeFX = (UniversalInputManager.UsePhoneUI == null) ? BnetBar.Get().m_currencyFrame.m_explodeFX_Legendary : ArcaneDustAmount.Get().m_explodeFX_Legendary;
        yfx.glowBallMat = this.m_FX.m_glowBallMat_Common;
        yfx.glowTrailMat = this.m_FX.m_glowTrailMat_Common;
        return yfx;
    }

    private RaritySound GetRaritySound(DisenchantBar bar)
    {
        RaritySound sound = new RaritySound();
        switch (bar.m_rarity)
        {
            case TAG_RARITY.RARE:
                sound.m_drainSound = this.m_sound.m_rare.m_drainSound;
                sound.m_jarSound = this.m_sound.m_rare.m_jarSound;
                sound.m_missileSound = this.m_sound.m_rare.m_missileSound;
                return sound;

            case TAG_RARITY.EPIC:
                sound.m_drainSound = this.m_sound.m_epic.m_drainSound;
                sound.m_jarSound = this.m_sound.m_epic.m_jarSound;
                sound.m_missileSound = this.m_sound.m_epic.m_missileSound;
                return sound;

            case TAG_RARITY.LEGENDARY:
                sound.m_drainSound = this.m_sound.m_legendary.m_drainSound;
                sound.m_jarSound = this.m_sound.m_legendary.m_jarSound;
                sound.m_missileSound = this.m_sound.m_legendary.m_missileSound;
                return sound;
        }
        sound.m_drainSound = this.m_sound.m_common.m_drainSound;
        sound.m_jarSound = this.m_sound.m_common.m_jarSound;
        sound.m_missileSound = this.m_sound.m_common.m_missileSound;
        return sound;
    }

    public int GetTotalAmount()
    {
        return this.m_totalAmount;
    }

    public void Hide()
    {
        this.m_root.SetActive(false);
    }

    [DebuggerHidden]
    private IEnumerator LaunchGlowball(DisenchantBar bar, RarityFX rareFX, int glowBallNum, int totalGlowBalls, int m_highestGlowBalls)
    {
        return new <LaunchGlowball>c__Iterator50 { glowBallNum = glowBallNum, m_highestGlowBalls = m_highestGlowBalls, totalGlowBalls = totalGlowBalls, rareFX = rareFX, bar = bar, <$>glowBallNum = glowBallNum, <$>m_highestGlowBalls = m_highestGlowBalls, <$>totalGlowBalls = totalGlowBalls, <$>rareFX = rareFX, <$>bar = bar, <>f__this = this };
    }

    private void OnDestroy()
    {
        CollectionManager.Get().RemoveMassDisenchantListener(new CollectionManager.OnMassDisenchant(this.OnMassDisenchant));
    }

    private void OnDisenchantButtonOut(UIEvent e)
    {
        if (CollectionManagerDisplay.Get().m_pageManager.IsShowingMassDisenchant())
        {
            this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        }
        else
        {
            this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        }
    }

    private void OnDisenchantButtonOver(UIEvent e)
    {
        if (CollectionManagerDisplay.Get().m_pageManager.IsShowingMassDisenchant())
        {
            this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
            SoundManager.Get().LoadAndPlay("Hub_Mouseover");
        }
        else
        {
            this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        }
    }

    private void OnDisenchantButtonPressed(UIEvent e)
    {
        Options.Get().SetBool(Option.HAS_DISENCHANTED, true);
        this.m_disenchantButton.SetEnabled(false);
        this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        Network.MassDisenchant();
    }

    private void OnInfoButtonPressed(UIEvent e)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_MASS_DISENCHANT_BUTTON_TEXT"),
            m_text = string.Format("{0}\n\n{1}", GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS_HEADLINE"), GameStrings.Get("GLUE_MASS_DISENCHANT_DETAILS")),
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void OnMassDisenchant(int amount)
    {
        int maxGlowBalls = 10;
        GraphicsQuality renderQualityLevel = GraphicsManager.Get().RenderQualityLevel;
        if (renderQualityLevel == GraphicsQuality.Low)
        {
            maxGlowBalls = 3;
        }
        else if (renderQualityLevel == GraphicsQuality.Medium)
        {
            maxGlowBalls = 6;
        }
        else
        {
            maxGlowBalls = 10;
        }
        this.BlockUI(true);
        base.StartCoroutine(this.DoDisenchantAnims(maxGlowBalls, amount));
    }

    private void SetDustBalance(float bal)
    {
        int num = (int) bal;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            ArcaneDustAmount.Get().m_dustCount.Text = num.ToString();
        }
        else
        {
            BnetBar.Get().m_currencyFrame.m_amount.Text = num.ToString();
        }
    }

    private void SetGemSaturation(List<DisenchantBar> disenchantBars, float saturation, bool onlyActive = false, bool onlyInactive = false)
    {
        foreach (DisenchantBar bar in disenchantBars)
        {
            int numCards = bar.GetNumCards();
            if (((onlyActive && (numCards != 0)) || (onlyInactive && (numCards == 0))) || (!onlyInactive && !onlyActive))
            {
                bar.m_rarityGem.GetComponent<Renderer>().material.SetColor("_Fade", new Color(saturation, saturation, saturation, 1f));
            }
        }
    }

    public void Show()
    {
        this.m_root.SetActive(true);
    }

    private void Start()
    {
        this.m_disenchantButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDisenchantButtonPressed));
        this.m_disenchantButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnDisenchantButtonOver));
        this.m_disenchantButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnDisenchantButtonOut));
        if (this.m_infoButton != null)
        {
            this.m_infoButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnInfoButtonPressed));
        }
    }

    [DebuggerHidden]
    public IEnumerator StartHighlight()
    {
        return new <StartHighlight>c__Iterator4E { <>f__this = this };
    }

    private void Unbloomify(List<GameObject> glows, float newVal)
    {
        foreach (GameObject obj2 in glows)
        {
            obj2.GetComponent<RenderToTexture>().m_BloomIntensity = newVal;
        }
    }

    private void UncolorTotal(float newVal)
    {
        this.m_totalAmountText.TextColor = Color.Lerp(Color.white, new Color(0.7f, 0.85f, 1f, 1f), newVal);
    }

    public void UpdateContents(List<CollectionCardStack.ArtStack> disenchantableArtStacks)
    {
        if (<>f__am$cache1B == null)
        {
            <>f__am$cache1B = obj => TAG_PREMIUM.GOLDEN == obj.Flair.Premium;
        }
        CollectionCardStack.ArtStack stack = disenchantableArtStacks.Find(<>f__am$cache1B);
        this.m_useSingle = stack == null;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_useSingle = true;
        }
        List<DisenchantBar> list = !this.m_useSingle ? this.m_doubleDisenchantBars : this.m_singleDisenchantBars;
        foreach (DisenchantBar bar in list)
        {
            bar.Reset();
        }
        this.m_totalAmount = 0;
        this.m_totalCardsToDisenchant = 0;
        <UpdateContents>c__AnonStorey2E5 storeye = new <UpdateContents>c__AnonStorey2E5();
        using (List<CollectionCardStack.ArtStack>.Enumerator enumerator2 = disenchantableArtStacks.GetEnumerator())
        {
            while (enumerator2.MoveNext())
            {
                storeye.disenchantableArtStack = enumerator2.Current;
                <UpdateContents>c__AnonStorey2E6 storeye2 = new <UpdateContents>c__AnonStorey2E6 {
                    <>f__ref$741 = storeye
                };
                NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(storeye.disenchantableArtStack.CardID, storeye.disenchantableArtStack.Flair.Premium);
                if (cardValue != null)
                {
                    storeye2.entityDef = DefLoader.Get().GetEntityDef(storeye.disenchantableArtStack.CardID);
                    int sellAmount = cardValue.Sell * storeye.disenchantableArtStack.Count;
                    DisenchantBar bar2 = list.Find(new Predicate<DisenchantBar>(storeye2.<>m__C9));
                    if (bar2 == null)
                    {
                        object[] objArray1 = new object[] { !this.m_useSingle ? "double" : "single", storeye2.entityDef, storeye.disenchantableArtStack.Flair, storeye.disenchantableArtStack.Count };
                        UnityEngine.Debug.LogWarning(string.Format("MassDisenchant.UpdateContents(): Could not find {0} bar to modify for card {1} (flair {2}, disenchant count {3})", objArray1));
                    }
                    else
                    {
                        bar2.AddCards(storeye.disenchantableArtStack.Count, sellAmount, storeye.disenchantableArtStack.Flair.Premium);
                        this.m_totalCardsToDisenchant += storeye.disenchantableArtStack.Count;
                        this.m_totalAmount += sellAmount;
                    }
                }
            }
        }
        if (this.m_totalAmount > 0)
        {
            this.m_singleRoot.SetActive(this.m_useSingle);
            if (this.m_doubleRoot != null)
            {
                this.m_doubleRoot.SetActive(!this.m_useSingle);
            }
            this.m_disenchantButton.SetEnabled(true);
        }
        foreach (DisenchantBar bar3 in list)
        {
            bar3.UpdateVisuals(this.m_totalCardsToDisenchant);
        }
        object[] args = new object[] { this.m_totalAmount };
        this.m_totalAmountText.Text = GameStrings.Format("GLUE_MASS_DISENCHANT_TOTAL_AMOUNT", args);
    }

    [CompilerGenerated]
    private sealed class <DoDisenchantAnims>c__Iterator4F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>disenchantTotal;
        internal int <$>maxGlowBalls;
        internal List<DisenchantBar>.Enumerator <$s_441>__9;
        internal List<DisenchantBar>.Enumerator <$s_442>__12;
        internal List<DisenchantBar>.Enumerator <$s_443>__20;
        internal List<DisenchantBar>.Enumerator <$s_444>__26;
        internal List<DisenchantBar>.Enumerator <$s_445>__28;
        internal MassDisenchant <>f__this;
        internal DisenchantBar <bar>__10;
        internal DisenchantBar <bar>__13;
        internal DisenchantBar <bar>__21;
        internal DisenchantBar <bar>__27;
        internal DisenchantBar <bar>__29;
        internal float <curDustTotal>__19;
        internal float <curTotal>__25;
        internal List<DisenchantBar> <disenchantBars>__0;
        internal float <duration>__2;
        internal Color <glowColor>__5;
        internal List<GameObject> <glows>__4;
        internal int <i>__17;
        internal int <i>__18;
        internal int <maxBalls>__16;
        internal int <numCards>__11;
        internal int <numCards>__14;
        internal int <numCards>__23;
        internal float <origInten>__8;
        internal float <origXSpeed>__7;
        internal float <origYSpeed>__6;
        internal RarityFX <rareFX>__15;
        internal RaritySound <rareSound>__22;
        internal float <rate>__3;
        internal Vector3 <textScale>__24;
        internal float <vigTime>__1;
        internal int disenchantTotal;
        internal int maxGlowBalls;

        internal void <>m__CA(object newVal)
        {
            this.<>f__this.SetGemSaturation(this.<disenchantBars>__0, (float) newVal, false, false);
        }

        internal void <>m__CB(object newVal)
        {
            this.<>f__this.SetGemSaturation(this.<disenchantBars>__0, (float) newVal, true, false);
        }

        internal void <>m__CC(object newVal)
        {
            this.<>f__this.Unbloomify(this.<glows>__4, (float) newVal);
        }

        internal void <>m__CD(object newVal)
        {
            this.<>f__this.UncolorTotal((float) newVal);
        }

        internal void <>m__CE(object newVal)
        {
            this.<>f__this.SetDustBalance((float) newVal);
        }

        internal void <>m__CF(object newVal)
        {
            CollectionManagerDisplay.Get().m_pageManager.UpdateMassDisenchant();
            FullScreenFXMgr.Get().StopVignette(this.<vigTime>__1, iTween.EaseType.easeInOutCubic, null);
            this.<>f__this.BlockUI(false);
        }

        internal void <>m__D0(object newVal)
        {
            this.<>f__this.SetGemSaturation(this.<disenchantBars>__0, (float) newVal, false, true);
        }

        internal void <>m__D1(object newVal)
        {
            this.<>f__this.SetGemSaturation(this.<disenchantBars>__0, (float) newVal, true, false);
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
                    if (this.disenchantTotal != 0)
                    {
                        break;
                    }
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_0E61;

                case 1:
                    break;

                case 2:
                {
                    this.<duration>__2 = 0.5f;
                    this.<rate>__3 = this.<duration>__2 / 20f;
                    object[] objArray2 = new object[] { "from", 0.3f, "to", 1.75f, "time", 1.5f * this.<duration>__2, "easeInType", iTween.EaseType.easeInCubic, "onupdate", new Action<object>(this.<>m__CB) };
                    iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray2));
                    this.<glows>__4 = new List<GameObject>();
                    if (this.<>f__this.m_FX.m_glowTotal != null)
                    {
                        this.<glows>__4.Add(this.<>f__this.m_FX.m_glowTotal);
                    }
                    this.<>f__this.m_totalAmountText.transform.localScale = (Vector3) (this.<>f__this.m_origTotalScale * 2.54f);
                    object[] objArray3 = new object[] { "scale", this.<>f__this.m_origTotalScale, "time", 3.0 };
                    iTween.ScaleTo(this.<>f__this.m_totalAmountText.gameObject, iTween.Hash(objArray3));
                    if (this.<>f__this.m_FX.m_glowTotal != null)
                    {
                        this.<>f__this.m_FX.m_glowTotal.SetActive(true);
                    }
                    this.<>f__this.m_highestGlowBalls = 0;
                    this.<glowColor>__5 = new Color(0.7f, 0.85f, 1f, 1f);
                    this.<origYSpeed>__6 = 0f;
                    this.<origXSpeed>__7 = 0f;
                    this.<origInten>__8 = 0f;
                    this.<$s_441>__9 = this.<disenchantBars>__0.GetEnumerator();
                    try
                    {
                        while (this.<$s_441>__9.MoveNext())
                        {
                            this.<bar>__10 = this.<$s_441>__9.Current;
                            this.<numCards>__11 = this.<bar>__10.GetNumCards();
                            if (this.<numCards>__11 > this.<>f__this.m_highestGlowBalls)
                            {
                                this.<>f__this.m_highestGlowBalls = this.<numCards>__11;
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_441>__9.Dispose();
                    }
                    this.<>f__this.m_highestGlowBalls = (this.<>f__this.m_highestGlowBalls <= this.maxGlowBalls) ? this.<>f__this.m_highestGlowBalls : this.maxGlowBalls;
                    this.<$s_442>__12 = this.<disenchantBars>__0.GetEnumerator();
                    try
                    {
                        while (this.<$s_442>__12.MoveNext())
                        {
                            this.<bar>__13 = this.<$s_442>__12.Current;
                            this.<numCards>__14 = this.<bar>__13.GetNumCards();
                            if (this.<numCards>__14 > 0)
                            {
                                this.<rareFX>__15 = this.<>f__this.GetRarityFX(this.<bar>__13);
                                this.<maxBalls>__16 = (this.<numCards>__14 <= this.maxGlowBalls) ? this.<numCards>__14 : this.maxGlowBalls;
                                this.<i>__17 = 0;
                                while (this.<i>__17 < this.<maxBalls>__16)
                                {
                                    this.<>f__this.StartCoroutine(this.<>f__this.LaunchGlowball(this.<bar>__13, this.<rareFX>__15, this.<i>__17, this.<maxBalls>__16, this.<>f__this.m_highestGlowBalls));
                                    this.<i>__17++;
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_442>__12.Dispose();
                    }
                    this.<i>__18 = 0;
                    while (this.<i>__18 < (this.<duration>__2 / this.<rate>__3))
                    {
                        this.<curDustTotal>__19 = 0f;
                        this.<$s_443>__20 = this.<disenchantBars>__0.GetEnumerator();
                        try
                        {
                            while (this.<$s_443>__20.MoveNext())
                            {
                                this.<bar>__21 = this.<$s_443>__20.Current;
                                this.<rareSound>__22 = this.<>f__this.GetRaritySound(this.<bar>__21);
                                this.<numCards>__23 = this.<bar>__21.GetNumCards();
                                if ((this.<i>__18 == 0) && (this.<numCards>__23 != 0))
                                {
                                    if (this.<rareSound>__22.m_drainSound != string.Empty)
                                    {
                                        SoundManager.Get().LoadAndPlay(this.<rareSound>__22.m_drainSound);
                                    }
                                    if ((this.<bar>__21.m_numGoldText != null) && this.<bar>__21.m_numGoldText.gameObject.activeSelf)
                                    {
                                        this.<bar>__21.m_numGoldText.gameObject.SetActive(false);
                                        TransformUtil.SetLocalPosX(this.<bar>__21.m_numCardsText, 2.902672f);
                                    }
                                    this.<textScale>__24 = this.<bar>__21.m_numCardsText.gameObject.transform.localScale;
                                    object[] objArray4 = new object[] { "x", this.<textScale>__24.x * 2.28f, "y", this.<textScale>__24.y * 2.28f, "z", this.<textScale>__24.z * 2.28f, "time", 3.0 };
                                    iTween.ScaleFrom(this.<bar>__21.m_numCardsText.gameObject, iTween.Hash(objArray4));
                                    this.<bar>__21.m_numCardsText.TextColor = this.<glowColor>__5;
                                    object[] objArray5 = new object[] { "r", 1f, "g", 1f, "b", 1f, "time", 3.0 };
                                    iTween.ColorTo(this.<bar>__21.m_numCardsText.gameObject, iTween.Hash(objArray5));
                                    if ((GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.High) && (this.<bar>__21.m_glow != null))
                                    {
                                        this.<glows>__4.Add(this.<bar>__21.m_glow);
                                        this.<bar>__21.m_glow.GetComponent<RenderToTexture>().m_BloomIntensity = 0.01f;
                                        this.<bar>__21.m_glow.SetActive(true);
                                    }
                                    this.<origYSpeed>__6 = this.<bar>__21.m_rarityGem.GetComponent<Renderer>().material.GetFloat("_YSpeed");
                                    this.<origXSpeed>__7 = this.<bar>__21.m_rarityGem.GetComponent<Renderer>().material.GetFloat("_XSpeed");
                                    this.<origInten>__8 = this.<bar>__21.m_amountBar.GetComponent<Renderer>().material.GetFloat("_Intensity");
                                    this.<bar>__21.m_rarityGem.GetComponent<Renderer>().material.SetFloat("_YSpeed", -10f);
                                    this.<bar>__21.m_rarityGem.GetComponent<Renderer>().material.SetFloat("_XSpeed", 20f);
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_443>__20.Dispose();
                        }
                        if (this.<i>__18 == 0)
                        {
                            if (GraphicsManager.Get().RenderQualityLevel == GraphicsQuality.High)
                            {
                                object[] objArray6 = new object[] { "from", 1f, "to", 0.1f, "time", this.<duration>__2 * 3f, "onupdate", new Action<object>(this.<>m__CC) };
                                iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray6));
                            }
                            object[] objArray7 = new object[] { "from", 1f, "to", 0.1f, "time", this.<duration>__2 * 3f, "onupdate", new Action<object>(this.<>m__CD) };
                            iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray7));
                            this.<curTotal>__25 = CraftingManager.Get().GetLocalArcaneDustBalance();
                            object[] objArray8 = new object[] { "from", this.<curTotal>__25, "to", this.<curTotal>__25 + this.disenchantTotal, "time", 3f * this.<duration>__2, "onupdate", new Action<object>(this.<>m__CE), "oncomplete", new Action<object>(this.<>m__CF) };
                            iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray8));
                        }
                        this.<$s_444>__26 = this.<disenchantBars>__0.GetEnumerator();
                        try
                        {
                            while (this.<$s_444>__26.MoveNext())
                            {
                                this.<bar>__27 = this.<$s_444>__26.Current;
                                if (this.<bar>__27.GetNumCards() != 0)
                                {
                                    this.<bar>__27.m_amountBar.GetComponent<Renderer>().material.SetFloat("_Intensity", 2f);
                                    this.<curDustTotal>__19 += this.<>f__this.DrainBarAndDust(this.<bar>__27, this.<i>__18, this.<duration>__2, this.<rate>__3);
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_444>__26.Dispose();
                        }
                        this.<>f__this.m_totalAmountText.Text = Convert.ToInt32(this.<curDustTotal>__19).ToString();
                        this.$current = new WaitForSeconds(this.<rate>__3 / this.<duration>__2);
                        this.$PC = 3;
                        goto Label_0E61;
                    Label_0BCA:
                        this.<i>__18++;
                    }
                    if (this.<>f__this.m_FX.m_glowTotal != null)
                    {
                        this.<>f__this.m_FX.m_glowTotal.SetActive(false);
                    }
                    this.<>f__this.m_totalAmountText.Text = "0";
                    this.<>f__this.m_totalAmountText.TextColor = Color.white;
                    object[] objArray9 = new object[] { "from", 0.3f, "to", 1f, "time", this.<duration>__2, "delay", this.<vigTime>__1, "onupdate", new Action<object>(this.<>m__D0) };
                    iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray9));
                    object[] objArray10 = new object[] { "from", 1.75f, "to", 1f, "time", this.<duration>__2, "delay", this.<vigTime>__1, "onupdate", new Action<object>(this.<>m__D1) };
                    iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(objArray10));
                    this.<$s_445>__28 = this.<disenchantBars>__0.GetEnumerator();
                    try
                    {
                        while (this.<$s_445>__28.MoveNext())
                        {
                            this.<bar>__29 = this.<$s_445>__28.Current;
                            if (this.<bar>__29.m_glow != null)
                            {
                                this.<bar>__29.m_glow.SetActive(false);
                            }
                            this.<bar>__29.m_numCardsText.TextColor = Color.white;
                            this.<bar>__29.m_rarityGem.GetComponent<Renderer>().material.SetFloat("_YSpeed", this.<origYSpeed>__6);
                            this.<bar>__29.m_rarityGem.GetComponent<Renderer>().material.SetFloat("_XSpeed", this.<origXSpeed>__7);
                            this.<bar>__29.m_amountBar.GetComponent<Renderer>().material.SetFloat("_Intensity", this.<origInten>__8);
                        }
                    }
                    finally
                    {
                        this.<$s_445>__28.Dispose();
                    }
                    this.$PC = -1;
                    goto Label_0E5F;
                }
                case 3:
                    goto Label_0BCA;

                default:
                    goto Label_0E5F;
            }
            this.<>f__this.m_origTotalScale = this.<>f__this.m_totalAmountText.transform.localScale;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<>f__this.m_origDustScale = ArcaneDustAmount.Get().m_dustJar.transform.localScale;
            }
            else
            {
                this.<>f__this.m_origDustScale = BnetBar.Get().m_currencyFrame.m_dustJar.transform.localScale;
            }
            this.<disenchantBars>__0 = !this.<>f__this.m_useSingle ? this.<>f__this.m_doubleDisenchantBars : this.<>f__this.m_singleDisenchantBars;
            this.<vigTime>__1 = 0.2f;
            FullScreenFXMgr.Get().Vignette(0.8f, this.<vigTime>__1, iTween.EaseType.easeInOutCubic, null);
            object[] args = new object[] { "from", 1f, "to", 0.3f, "time", this.<vigTime>__1, "onupdate", new Action<object>(this.<>m__CA) };
            iTween.ValueTo(this.<>f__this.gameObject, iTween.Hash(args));
            if (this.<>f__this.m_sound.m_intro != string.Empty)
            {
                SoundManager.Get().LoadAndPlay(this.<>f__this.m_sound.m_intro);
            }
            this.$current = new WaitForSeconds(this.<vigTime>__1);
            this.$PC = 2;
            goto Label_0E61;
        Label_0E5F:
            return false;
        Label_0E61:
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
    private sealed class <LaunchGlowball>c__Iterator50 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DisenchantBar <$>bar;
        internal int <$>glowBallNum;
        internal int <$>m_highestGlowBalls;
        internal RarityFX <$>rareFX;
        internal int <$>totalGlowBalls;
        internal MassDisenchant <>f__this;
        internal GameObject <burst>__11;
        internal List<Vector3> <curvePoints>__7;
        internal float <delayEnd>__5;
        internal float <delayStart>__4;
        internal float <duration>__0;
        internal GameObject <dustFX>__13;
        internal GameObject <dustJar>__8;
        internal Vector3 <dustJarPos>__9;
        internal Vector3 <dustScale>__16;
        internal GameObject <glowBall>__6;
        internal float <glowBallTime>__12;
        internal float <gNum>__2;
        internal float <pad>__1;
        internal RaritySound <rareSound>__10;
        internal GameObject <receiveBurstFX>__15;
        internal GameObject <receiveFX>__14;
        internal float <timeRange>__3;
        internal DisenchantBar bar;
        internal int glowBallNum;
        internal int m_highestGlowBalls;
        internal RarityFX rareFX;
        internal int totalGlowBalls;

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
                    this.<duration>__0 = 1f;
                    this.<pad>__1 = 0.02f;
                    this.<gNum>__2 = this.glowBallNum;
                    this.<timeRange>__3 = (this.<duration>__0 - (this.<pad>__1 * this.m_highestGlowBalls)) / ((float) this.totalGlowBalls);
                    this.<delayStart>__4 = (this.<gNum>__2 * this.<timeRange>__3) + (this.<gNum>__2 * this.<pad>__1);
                    this.<delayEnd>__5 = ((this.<gNum>__2 + 1f) * this.<timeRange>__3) + ((this.<gNum>__2 + 1f) * this.<pad>__1);
                    this.<glowBall>__6 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.m_FX.m_glowBall);
                    this.<glowBall>__6.GetComponent<Renderer>().sharedMaterial = this.rareFX.glowBallMat;
                    this.<glowBall>__6.GetComponent<TrailRenderer>().material = this.rareFX.glowTrailMat;
                    this.<glowBall>__6.transform.position = this.bar.m_rarityGem.transform.position;
                    this.<glowBall>__6.transform.position = new Vector3(this.<glowBall>__6.transform.position.x, this.<glowBall>__6.transform.position.y + 0.5f, this.<glowBall>__6.transform.position.z);
                    this.<curvePoints>__7 = new List<Vector3>();
                    this.<curvePoints>__7.Add(this.<glowBall>__6.transform.position);
                    if (UnityEngine.Random.Range((float) 0f, (float) 1f) >= 0.5)
                    {
                        this.<curvePoints>__7.Add(this.<>f__this.GetRanBoxPt(this.<>f__this.m_FX.m_gemBoxRight1));
                        this.<curvePoints>__7.Add(this.<>f__this.GetRanBoxPt(this.<>f__this.m_FX.m_gemBoxRight2));
                        break;
                    }
                    this.<curvePoints>__7.Add(this.<>f__this.GetRanBoxPt(this.<>f__this.m_FX.m_gemBoxLeft1));
                    this.<curvePoints>__7.Add(this.<>f__this.GetRanBoxPt(this.<>f__this.m_FX.m_gemBoxLeft2));
                    break;

                case 1:
                {
                    this.<rareSound>__10 = this.<>f__this.GetRaritySound(this.bar);
                    if (this.<rareSound>__10.m_missileSound != string.Empty)
                    {
                        SoundManager.Get().LoadAndPlay(this.<rareSound>__10.m_missileSound);
                    }
                    if (this.glowBallNum == 0)
                    {
                        this.<burst>__11 = UnityEngine.Object.Instantiate<GameObject>(this.rareFX.burstFX);
                        this.<burst>__11.transform.position = this.bar.m_rarityGem.transform.position;
                        this.<burst>__11.GetComponent<ParticleSystem>().Play();
                        UnityEngine.Object.Destroy(this.<burst>__11, 3f);
                    }
                    this.<glowBallTime>__12 = 0.4f;
                    this.<glowBall>__6.SetActive(true);
                    object[] args = new object[] { "path", this.<curvePoints>__7.ToArray(), "time", this.<glowBallTime>__12, "easetype", iTween.EaseType.linear };
                    iTween.MoveTo(this.<glowBall>__6, iTween.Hash(args));
                    UnityEngine.Object.Destroy(this.<glowBall>__6, this.<glowBallTime>__12);
                    this.$current = new WaitForSeconds(this.<glowBallTime>__12);
                    this.$PC = 2;
                    goto Label_07D2;
                }
                case 2:
                {
                    if (this.<rareSound>__10.m_jarSound != string.Empty)
                    {
                        SoundManager.Get().LoadAndPlay(this.<rareSound>__10.m_jarSound);
                    }
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<dustFX>__13 = ArcaneDustAmount.Get().m_dustFX;
                    }
                    else
                    {
                        this.<dustFX>__13 = BnetBar.Get().m_currencyFrame.m_dustFX;
                    }
                    if (UnityEngine.Random.Range((float) 0f, (float) 1f) > 0.7f)
                    {
                        this.<receiveFX>__14 = UnityEngine.Object.Instantiate<GameObject>(this.<dustFX>__13);
                        this.<receiveFX>__14.transform.parent = this.<dustFX>__13.transform.parent;
                        this.<receiveFX>__14.transform.localPosition = this.<dustFX>__13.transform.localPosition;
                        this.<receiveFX>__14.transform.localScale = this.<dustFX>__13.transform.localScale;
                        this.<receiveFX>__14.transform.localRotation = this.<dustFX>__13.transform.localRotation;
                        this.<receiveFX>__14.SetActive(true);
                        this.<receiveFX>__14.GetComponent<ParticleSystem>().Play();
                        UnityEngine.Object.Destroy(this.<receiveFX>__14, 4.9f);
                    }
                    this.<receiveBurstFX>__15 = UnityEngine.Object.Instantiate<GameObject>(this.rareFX.explodeFX);
                    this.<receiveBurstFX>__15.transform.parent = this.rareFX.explodeFX.transform.parent;
                    this.<receiveBurstFX>__15.transform.localPosition = this.rareFX.explodeFX.transform.localPosition;
                    this.<receiveBurstFX>__15.transform.localScale = this.rareFX.explodeFX.transform.localScale;
                    this.<receiveBurstFX>__15.transform.localRotation = this.rareFX.explodeFX.transform.localRotation;
                    this.<receiveBurstFX>__15.SetActive(true);
                    this.<receiveBurstFX>__15.GetComponent<ParticleSystem>().Play();
                    UnityEngine.Object.Destroy(this.<receiveBurstFX>__15, 3f);
                    this.<dustScale>__16 = this.<dustJar>__8.transform.localScale + ((UniversalInputManager.UsePhoneUI == null) ? new Vector3(50f, 50f, 50f) : new Vector3(6f, 6f, 6f));
                    object[] objArray2 = new object[] { "scale", this.<dustScale>__16, "time", 0.15f };
                    iTween.ScaleTo(this.<dustJar>__8, iTween.Hash(objArray2));
                    object[] objArray3 = new object[] { "scale", this.<>f__this.m_origDustScale, "delay", 0.1, "time", 1f };
                    iTween.ScaleTo(this.<dustJar>__8, iTween.Hash(objArray3));
                    this.$current = null;
                    this.$PC = 3;
                    goto Label_07D2;
                }
                case 3:
                    this.$PC = -1;
                    goto Label_07D0;

                default:
                    goto Label_07D0;
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.<dustJar>__8 = ArcaneDustAmount.Get().m_dustJar;
                this.<curvePoints>__7.Add(this.<dustJar>__8.transform.position);
            }
            else
            {
                this.<dustJar>__8 = BnetBar.Get().m_currencyFrame.m_dustJar;
                this.<dustJarPos>__9 = Camera.main.ViewportToWorldPoint(BaseUI.Get().m_BnetCamera.WorldToViewportPoint(this.<dustJar>__8.transform.position));
                this.<dustJarPos>__9.y = 20f;
                this.<curvePoints>__7.Add(Camera.main.ViewportToWorldPoint(BaseUI.Get().m_BnetCamera.WorldToViewportPoint(this.<dustJar>__8.transform.position)));
            }
            this.$current = new WaitForSeconds(UnityEngine.Random.Range(this.<delayStart>__4, this.<delayEnd>__5));
            this.$PC = 1;
            goto Label_07D2;
        Label_07D0:
            return false;
        Label_07D2:
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
    private sealed class <StartHighlight>c__Iterator4E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MassDisenchant <>f__this;

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
                    this.<>f__this.m_FX.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
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

    [CompilerGenerated]
    private sealed class <UpdateContents>c__AnonStorey2E5
    {
        internal CollectionCardStack.ArtStack disenchantableArtStack;
    }

    [CompilerGenerated]
    private sealed class <UpdateContents>c__AnonStorey2E6
    {
        internal MassDisenchant.<UpdateContents>c__AnonStorey2E5 <>f__ref$741;
        internal EntityDef entityDef;

        internal bool <>m__C9(DisenchantBar obj)
        {
            return (((obj.m_premiumType == this.<>f__ref$741.disenchantableArtStack.Flair.Premium) || (UniversalInputManager.UsePhoneUI != null)) && (obj.m_rarity == this.entityDef.GetRarity()));
        }
    }
}

