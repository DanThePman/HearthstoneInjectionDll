using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class PackOpeningDirector : MonoBehaviour
{
    private const int CARDS_PER_PACK = 5;
    private Spell m_activePackFxSpell;
    public GameObject m_CardsInsidePack;
    private int m_cardsPendingReveal;
    public Carousel m_Carousel;
    private int m_centerCardIndex;
    public GameObject m_ClassName;
    private PackOpeningCard m_clickedCard;
    private int m_clickedPosition;
    private List<Achievement> m_completeAchievesToDisplay = new List<Achievement>();
    private NormalButton m_doneButton;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public string m_DoneButtonPrefab;
    private bool m_doneButtonShown;
    private int m_effectsPendingDestroy;
    private int m_effectsPendingFinish;
    private List<FinishedListener> m_finishedListeners = new List<FinishedListener>();
    private PackOpeningCard m_glowingCard;
    public PackOpeningCard m_HiddenCard;
    private List<PackOpeningCard> m_hiddenCards = new List<PackOpeningCard>();
    private bool m_loadingDoneButton;
    private Map<int, Spell> m_packFxSpells = new Map<int, Spell>();
    private bool m_playing;
    private readonly Vector3 PACK_OPENING_FX_POSITION = Vector3.zero;

    public void AddFinishedListener(FinishedCallback callback)
    {
        this.AddFinishedListener(callback, null);
    }

    public void AddFinishedListener(FinishedCallback callback, object userData)
    {
        FinishedListener item = new FinishedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        this.m_finishedListeners.Add(item);
    }

    [DebuggerHidden]
    private IEnumerator AttachBoosterCards(List<NetCache.BoosterCard> cards)
    {
        return new <AttachBoosterCards>c__Iterator1AF { cards = cards, <$>cards = cards, <>f__this = this };
    }

    private void AttachCardsToCarousel()
    {
        if (this.m_Carousel != null)
        {
            List<PackOpeningCardCarouselItem> list = new List<PackOpeningCardCarouselItem>();
            for (int i = 0; i < this.m_hiddenCards.Count; i++)
            {
                PackOpeningCard card = this.m_hiddenCards[i];
                card.GetComponent<Collider>().enabled = true;
                PackOpeningCardCarouselItem item = new PackOpeningCardCarouselItem(card);
                list.Add(item);
            }
            this.m_Carousel.Initialize(list.ToArray(), 0);
            this.m_Carousel.SetListeners(null, new Carousel.ItemClicked(this.CarouselItemClicked), new Carousel.ItemReleased(this.CarouselItemReleased), new Carousel.CarouselSettled(this.CarouselSettled), new Carousel.CarouselStartedScrolling(this.CarouselStartedScrolling));
            this.CarouselSettled();
        }
    }

    private void Awake()
    {
        this.InitializeCards();
        this.InitializeUI();
    }

    private void CameraBlurOn()
    {
        Box.Get().GetBoxCamera().GetEventTable().m_BlurSpell.ActivateState(SpellStateType.BIRTH);
    }

    private void CarouselItemClicked(CarouselItem item, int index)
    {
        this.m_clickedCard = item.GetGameObject().GetComponent<PackOpeningCard>();
        this.m_clickedPosition = index;
    }

    private void CarouselItemReleased()
    {
        if (!this.m_Carousel.IsScrolling())
        {
            if (this.m_clickedPosition == this.m_Carousel.GetCurrentIndex())
            {
                if (this.m_clickedCard.IsRevealed())
                {
                    if (this.m_clickedPosition < 4)
                    {
                        this.m_Carousel.SetPosition(this.m_clickedPosition + 1, true);
                    }
                }
                else
                {
                    this.m_clickedCard.ForceReveal();
                }
            }
            else
            {
                this.m_Carousel.SetPosition(this.m_clickedPosition, true);
            }
        }
    }

    private void CarouselSettled()
    {
        PackOpeningCard component = ((PackOpeningCardCarouselItem) this.m_Carousel.GetCurrentItem()).GetGameObject().GetComponent<PackOpeningCard>();
        this.m_glowingCard = component;
        component.ShowRarityGlow();
    }

    private void CarouselStartedScrolling()
    {
        if ((this.m_glowingCard != null) && (this.m_glowingCard.GetEntityDef().GetRarity() != TAG_RARITY.COMMON))
        {
            this.m_glowingCard.HideRarityGlow();
        }
    }

    private void EnableCardInput(bool enable)
    {
        foreach (PackOpeningCard card in this.m_hiddenCards)
        {
            card.EnableInput(enable);
        }
    }

    public bool FinishPackOpen()
    {
        if (!this.m_doneButtonShown)
        {
            return false;
        }
        this.m_activePackFxSpell.ActivateState(SpellStateType.DEATH);
        Box.Get().GetBoxCamera().GetEventTable().m_BlurSpell.ActivateState(SpellStateType.DEATH);
        this.m_effectsPendingFinish = 1 + (2 * this.m_hiddenCards.Count);
        this.m_effectsPendingDestroy = this.m_effectsPendingFinish;
        this.HideDoneButton();
        foreach (PackOpeningCard card in this.m_hiddenCards)
        {
            CardBackDisplay componentInChildren = card.GetComponentInChildren<CardBackDisplay>();
            if (componentInChildren != null)
            {
                componentInChildren.gameObject.GetComponent<Renderer>().enabled = false;
            }
            Spell classNameSpell = card.m_ClassNameSpell;
            classNameSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnHiddenCardSpellFinished));
            classNameSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnHiddenCardSpellStateFinished));
            classNameSpell.ActivateState(SpellStateType.DEATH);
            Spell spell = card.GetActor().GetSpell(SpellType.DEATH);
            spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnHiddenCardSpellFinished));
            spell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnHiddenCardSpellStateFinished));
            spell.Activate();
        }
        this.HideKeywordTooltips();
        return true;
    }

    private void FireFinishedEvent()
    {
        CollectionManager.Get().RemoveAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
        FinishedListener[] listenerArray = this.m_finishedListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire();
        }
    }

    public List<PackOpeningCard> GetHiddenCards()
    {
        return this.m_hiddenCards;
    }

    private void HideDoneButton()
    {
        this.m_doneButtonShown = false;
        SceneUtils.EnableColliders(this.m_doneButton.gameObject, false);
        this.m_doneButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDoneButtonPressed));
        Spell component = this.m_doneButton.m_button.GetComponent<Spell>();
        component.AddFinishedCallback(new Spell.FinishedCallback(this.OnDoneButtonHidden));
        component.ActivateState(SpellStateType.DEATH);
    }

    private void HideKeywordTooltips()
    {
        foreach (PackOpeningCard card in this.m_hiddenCards)
        {
            card.RemoveOnOverWhileFlippedListeners();
        }
        KeywordHelpPanelManager.Get().HideKeywordHelp();
    }

    private void InitializeCards()
    {
        this.m_hiddenCards.Add(this.m_HiddenCard);
        for (int i = 1; i < 5; i++)
        {
            PackOpeningCard component = UnityEngine.Object.Instantiate<GameObject>(this.m_HiddenCard.gameObject).GetComponent<PackOpeningCard>();
            component.transform.parent = this.m_HiddenCard.transform.parent;
            TransformUtil.CopyLocal((Component) component, (Component) this.m_HiddenCard);
            this.m_hiddenCards.Add(component);
        }
        for (int j = 0; j < this.m_hiddenCards.Count; j++)
        {
            PackOpeningCard userData = this.m_hiddenCards[j];
            userData.name = string.Format("Card_Hidden{0}", j + 1);
            userData.EnableInput(false);
            userData.AddRevealedListener(new PackOpeningCard.RevealedCallback(this.OnCardRevealed), userData);
        }
    }

    private void InitializeUI()
    {
        this.m_loadingDoneButton = true;
        string cardName = FileUtils.GameAssetPathToName(this.m_DoneButtonPrefab);
        AssetLoader.Get().LoadActor(cardName, new AssetLoader.GameObjectCallback(this.OnDoneButtonLoaded), null, false);
    }

    public bool IsDoneButtonShown()
    {
        return this.m_doneButtonShown;
    }

    public bool IsPlaying()
    {
        return this.m_playing;
    }

    public void OnBoosterOpened(List<NetCache.BoosterCard> cards)
    {
        if (cards.Count > this.m_hiddenCards.Count)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpeningDirector.OnBoosterOpened() - Not enough PackOpeningCards! Received {0} cards. There are only {1} hidden cards.", cards.Count, this.m_hiddenCards.Count));
        }
        else
        {
            int num = Mathf.Min(cards.Count, this.m_hiddenCards.Count);
            this.m_cardsPendingReveal = num;
            base.StartCoroutine(this.AttachBoosterCards(cards));
            CollectionManager.Get().OnBoosterOpened(cards);
        }
    }

    private void OnCardRevealed(object userData)
    {
        PackOpeningCard card = (PackOpeningCard) userData;
        if (card.GetEntityDef().GetRarity() == TAG_RARITY.LEGENDARY)
        {
            if (card.GetActor().GetCardFlair().Premium == TAG_PREMIUM.GOLDEN)
            {
                BnetPresenceMgr.Get().SetGameField(4, card.GetCardId() + ",1");
            }
            else
            {
                BnetPresenceMgr.Get().SetGameField(4, card.GetCardId() + ",0");
            }
        }
        this.m_cardsPendingReveal--;
        if (this.m_cardsPendingReveal <= 0)
        {
            this.ShowDoneButton();
        }
    }

    private void OnCollectionAchievesCompleted(List<Achievement> achievements)
    {
        this.m_completeAchievesToDisplay.AddRange(achievements);
    }

    private void OnDoneButtonHidden(Spell spell, object userData)
    {
        this.OnEffectFinished();
        this.OnEffectDone();
    }

    private void OnDoneButtonLoaded(string name, GameObject actorObject, object userData)
    {
        this.m_loadingDoneButton = false;
        if (actorObject == null)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpeningDirector.OnDoneButtonLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            this.m_doneButton = actorObject.GetComponent<NormalButton>();
            if (this.m_doneButton == null)
            {
                UnityEngine.Debug.LogError(string.Format("PackOpeningDirector.OnDoneButtonLoaded() - ERROR \"{0}\" has no {1} component", name, typeof(NormalButton)));
            }
            else
            {
                SceneUtils.SetLayer(this.m_doneButton.gameObject, GameLayer.IgnoreFullScreenEffects);
                this.m_doneButton.transform.parent = base.transform;
                TransformUtil.CopyWorld((Component) this.m_doneButton, (Component) PackOpening.Get().m_Bones.m_DoneButton);
                SceneUtils.EnableRenderersAndColliders(this.m_doneButton.gameObject, false);
            }
        }
    }

    private void OnDoneButtonPressed(UIEvent e)
    {
        if (this.m_completeAchievesToDisplay.Count > 0)
        {
            this.ShowCompleteAchieve();
        }
        else
        {
            this.HideKeywordTooltips();
            this.FinishPackOpen();
        }
    }

    private void OnDoneButtonShown(Spell spell, object userData)
    {
        this.m_doneButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDoneButtonPressed));
    }

    private void OnEffectDone()
    {
        this.m_effectsPendingDestroy--;
        if (this.m_effectsPendingDestroy <= 0)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void OnEffectFinished()
    {
        this.m_effectsPendingFinish--;
        if (this.m_effectsPendingFinish <= 0)
        {
            this.FireFinishedEvent();
        }
    }

    private void OnHiddenCardSpellFinished(Spell spell, object userData)
    {
        this.OnEffectFinished();
    }

    private void OnHiddenCardSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (spell.GetActiveState() == SpellStateType.NONE)
        {
            this.OnEffectDone();
        }
    }

    private void OnSpellFinished(Spell spell, object userData)
    {
        foreach (PackOpeningCard card in this.m_hiddenCards)
        {
            card.EnableInput(true);
            card.EnableReveal(true);
        }
        this.AttachCardsToCarousel();
    }

    public void Play(int boosterId)
    {
        if (!this.m_playing)
        {
            this.m_playing = true;
            this.EnableCardInput(false);
            CollectionManager.Get().RegisterAchievesCompletedListener(new CollectionManager.DelOnAchievesCompleted(this.OnCollectionAchievesCompleted));
            base.StartCoroutine(this.PlayWhenReady(boosterId));
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayWhenReady(int boosterId)
    {
        return new <PlayWhenReady>c__Iterator1AE { boosterId = boosterId, <$>boosterId = boosterId, <>f__this = this };
    }

    public void RemoveFinishedListener(FinishedCallback callback)
    {
        this.RemoveFinishedListener(callback, null);
    }

    public void RemoveFinishedListener(FinishedCallback callback, object userData)
    {
        FinishedListener item = new FinishedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        this.m_finishedListeners.Remove(item);
    }

    private void ShowCompleteAchieve()
    {
        if (this.m_completeAchievesToDisplay.Count == 0)
        {
            this.FinishPackOpen();
        }
        else
        {
            Achievement quest = this.m_completeAchievesToDisplay[0];
            this.m_completeAchievesToDisplay.RemoveAt(0);
            QuestToast.ShowQuestToast(userData => this.ShowCompleteAchieve(), true, quest);
        }
    }

    private void ShowDoneButton()
    {
        this.m_doneButtonShown = true;
        SceneUtils.EnableRenderersAndColliders(this.m_doneButton.gameObject, true);
        Spell component = this.m_doneButton.m_button.GetComponent<Spell>();
        component.AddFinishedCallback(new Spell.FinishedCallback(this.OnDoneButtonShown));
        component.ActivateState(SpellStateType.BIRTH);
    }

    private void Update()
    {
        if (this.m_Carousel != null)
        {
            this.m_Carousel.UpdateUI(UniversalInputManager.Get().GetMouseButtonDown(0));
        }
    }

    [CompilerGenerated]
    private sealed class <AttachBoosterCards>c__Iterator1AF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<NetCache.BoosterCard> <$>cards;
        internal PackOpeningDirector <>f__this;
        internal NetCache.BoosterCard <boosterCard>__2;
        internal PackOpeningCard <card>__3;
        internal int <i>__1;
        internal int <minCardCount>__0;
        internal List<NetCache.BoosterCard> cards;

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
                    this.<minCardCount>__0 = Mathf.Min(this.cards.Count, this.<>f__this.m_hiddenCards.Count);
                    this.<i>__1 = 0;
                    break;

                case 1:
                    this.<boosterCard>__2 = this.cards[this.<i>__1];
                    this.<card>__3 = this.<>f__this.m_hiddenCards[this.<i>__1];
                    this.<card>__3.AttachBoosterCard(this.<boosterCard>__2);
                    this.<i>__1++;
                    break;

                default:
                    goto Label_00D0;
            }
            if (this.<i>__1 < this.<minCardCount>__0)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_00D0:
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
    private sealed class <PlayWhenReady>c__Iterator1AE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>boosterId;
        internal PackOpeningDirector <>f__this;
        internal string <assetName>__3;
        internal AssetLoader.GameObjectCallback <callback>__5;
        internal PlayMakerFSM <fsm>__6;
        internal bool <loading>__4;
        internal string <originalAssetName>__2;
        internal DbfRecord <record>__1;
        internal Spell <spell>__0;
        internal int boosterId;

        internal void <>m__146(string name, GameObject go, object callbackData)
        {
            this.<loading>__4 = false;
            this.<>f__this.m_packFxSpells[this.boosterId] = this.<spell>__0;
            if (go == null)
            {
                object[] messageArgs = new object[] { name, this.boosterId };
                Error.AddDevFatal("PackOpeningDirector.PlayWhenReady() - Error loading {0} for booster id {1}", messageArgs);
            }
            else
            {
                this.<spell>__0 = go.GetComponent<Spell>();
                go.transform.parent = this.<>f__this.transform;
                go.transform.localPosition = this.<>f__this.PACK_OPENING_FX_POSITION;
            }
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
                case 1:
                    if (this.<>f__this.m_loadingDoneButton)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0248;
                    }
                    if (this.<>f__this.m_doneButton == null)
                    {
                        this.<>f__this.FireFinishedEvent();
                        goto Label_0246;
                    }
                    if (this.<>f__this.m_packFxSpells.TryGetValue(this.boosterId, out this.<spell>__0))
                    {
                        goto Label_014B;
                    }
                    this.<record>__1 = GameDbf.Booster.GetRecord(this.boosterId);
                    this.<originalAssetName>__2 = this.<record>__1.GetAssetName("PACK_OPENING_FX_PREFAB");
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<assetName>__3 = string.Format("{0}_phone", this.<originalAssetName>__2);
                    }
                    else
                    {
                        this.<assetName>__3 = this.<originalAssetName>__2;
                    }
                    this.<loading>__4 = true;
                    this.<callback>__5 = new AssetLoader.GameObjectCallback(this.<>m__146);
                    AssetLoader.Get().LoadGameObject(this.<assetName>__3, this.<callback>__5, null, false);
                    break;

                case 2:
                    break;

                default:
                    goto Label_0246;
            }
            while (this.<loading>__4)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0248;
            }
        Label_014B:
            if (this.<spell>__0 == null)
            {
                this.<>f__this.FireFinishedEvent();
            }
            else
            {
                this.<>f__this.m_activePackFxSpell = this.<spell>__0;
                this.<fsm>__6 = this.<spell>__0.GetComponent<PlayMakerFSM>();
                if (this.<fsm>__6 != null)
                {
                    this.<fsm>__6.FsmVariables.GetFsmGameObject("CardsInsidePack").Value = this.<>f__this.m_CardsInsidePack;
                    this.<fsm>__6.FsmVariables.GetFsmGameObject("ClassName").Value = this.<>f__this.m_ClassName;
                    this.<fsm>__6.FsmVariables.GetFsmGameObject("PackOpeningDirector").Value = this.<>f__this.gameObject;
                }
                this.<>f__this.m_activePackFxSpell.AddFinishedCallback(new Spell.FinishedCallback(this.<>f__this.OnSpellFinished));
                this.<>f__this.m_activePackFxSpell.ActivateState(SpellStateType.ACTION);
                this.$PC = -1;
            }
        Label_0246:
            return false;
        Label_0248:
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

    public delegate void FinishedCallback(object userData);

    private class FinishedListener : EventListener<PackOpeningDirector.FinishedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

