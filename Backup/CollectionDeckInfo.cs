using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectionDeckInfo : MonoBehaviour
{
    public PegUIElement m_deleteButton;
    private Actor m_goldenHeroPowerActor;
    private Actor m_heroPowerActor;
    public UberText m_heroPowerDescription;
    private string m_heroPowerID = string.Empty;
    public UberText m_heroPowerName;
    public GameObject m_heroPowerParent;
    private List<HideListener> m_hideListeners = new List<HideListener>();
    public List<DeckInfoManaBar> m_manaBars;
    public UberText m_manaCurveTooltipText;
    public PegUIElement m_offClicker;
    public PegUIElement m_renameButton;
    public GameObject m_renameButtonCover;
    public GameObject m_root;
    private List<ShowListener> m_showListeners = new List<ShowListener>();
    protected bool m_shown = true;
    private bool m_wasTouchModeEnabled;
    private readonly float MANA_COST_TEXT_MAX_LOCAL_Z = 5.167298f;
    private readonly float MANA_COST_TEXT_MIN_LOCAL_Z;
    private readonly int MAX_MANA_COST_ID = 7;
    private readonly int MIN_MANA_COST_ID;

    private void Awake()
    {
        this.m_manaCurveTooltipText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_INFO_MANA_TOOLTIP");
        foreach (DeckInfoManaBar bar in this.m_manaBars)
        {
            bar.m_costText.Text = this.GetTextForManaCost(bar.m_manaCostID);
        }
        AssetLoader.Get().LoadActor("Card_Play_HeroPower", new AssetLoader.GameObjectCallback(this.OnHeroPowerActorLoaded), null, false);
        AssetLoader.Get().LoadActor(ActorNames.GetNameWithPremiumType("Card_Play_HeroPower", TAG_PREMIUM.GOLDEN), new AssetLoader.GameObjectCallback(this.OnGoldenHeroPowerActorLoaded), null, false);
        this.m_renameButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRenameButtonPressed));
        if (this.m_renameButton is StandardPegButtonNew)
        {
            ((StandardPegButtonNew) this.m_renameButton).SetText(GameStrings.Get("GLUE_COLLECTION_DECK_RENAME"));
        }
        this.m_deleteButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnDeleteButtonPressed));
        if (this.m_deleteButton is StandardPegButtonNew)
        {
            ((StandardPegButtonNew) this.m_deleteButton).SetText(GameStrings.Get("GLUE_COLLECTION_DECK_DELETE"));
        }
        this.m_wasTouchModeEnabled = !UniversalInputManager.Get().IsTouchMode();
    }

    private string GetTextForManaCost(int manaCostID)
    {
        if ((manaCostID < this.MIN_MANA_COST_ID) || (manaCostID > this.MAX_MANA_COST_ID))
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.GetTextForManaCost(): don't know how to handle mana cost ID {0}", manaCostID));
            return string.Empty;
        }
        string str = Convert.ToString(manaCostID);
        if (manaCostID == this.MAX_MANA_COST_ID)
        {
            str = str + GameStrings.Get("GLUE_COLLECTION_PLUS");
        }
        return str;
    }

    public void Hide()
    {
        if (this.m_shown)
        {
            this.m_root.SetActive(false);
            this.m_shown = false;
            foreach (HideListener listener in this.m_hideListeners.ToArray())
            {
                listener();
            }
        }
    }

    public bool IsShown()
    {
        return this.m_shown;
    }

    private void OnClosePressed(UIEvent e)
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            Navigation.GoBack();
        }
    }

    private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
    {
        if (response != AlertPopup.Response.CANCEL)
        {
            Navigation.GoBack();
            CollectionDeckTray.Get().DeleteEditingDeck(true);
            if (CollectionManagerDisplay.Get() != null)
            {
                CollectionManagerDisplay.Get().OnDoneEditingDeck();
            }
        }
    }

    private void OnDeleteButtonPressed(UIEvent e)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_DESC"),
            m_showAlertIcon = false,
            m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
            m_responseCallback = new AlertPopup.ResponseCallback(this.OnDeleteButtonConfirmationResponse)
        };
        DialogManager.Get().ShowPopup(info);
    }

    private void OnGoldenHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            this.m_goldenHeroPowerActor = actorObject.GetComponent<Actor>();
            if (this.m_goldenHeroPowerActor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.OnGoldenHeroPowerActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.m_goldenHeroPowerActor.SetUnlit();
                this.m_goldenHeroPowerActor.transform.parent = this.m_heroPowerParent.transform;
                this.m_goldenHeroPowerActor.transform.localScale = Vector3.one;
                this.m_goldenHeroPowerActor.transform.localPosition = Vector3.zero;
                if (UniversalInputManager.Get().IsTouchMode())
                {
                    this.m_goldenHeroPowerActor.TurnOffCollider();
                }
            }
        }
    }

    private void OnHeroCardDefLoaded(string cardId, CardDef def, object userData)
    {
    }

    private void OnHeroPowerActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{0}\"", actorName));
        }
        else
        {
            this.m_heroPowerActor = actorObject.GetComponent<Actor>();
            if (this.m_heroPowerActor == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.OnHeroPowerActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
            }
            else
            {
                this.m_heroPowerActor.SetUnlit();
                this.m_heroPowerActor.transform.parent = this.m_heroPowerParent.transform;
                this.m_heroPowerActor.transform.localScale = Vector3.one;
                this.m_heroPowerActor.transform.localPosition = Vector3.zero;
                if (UniversalInputManager.Get().IsTouchMode())
                {
                    this.m_heroPowerActor.TurnOffCollider();
                }
            }
        }
    }

    private void OnHeroPowerFullDefLoaded(string cardID, FullDef def, object userData)
    {
        if (this.m_heroPowerActor != null)
        {
            this.SetHeroPowerInfo(cardID, def, (CardFlair) userData);
        }
        else
        {
            base.StartCoroutine(this.SetHeroPowerInfoWhenReady(cardID, def, (CardFlair) userData));
        }
    }

    private void OnRenameButtonPressed(UIEvent e)
    {
        CollectionDeckTray.Get().GetDecksContent().RenameCurrentlyEditingDeck();
    }

    public void RegisterHideListener(HideListener dlg)
    {
        this.m_hideListeners.Add(dlg);
    }

    public void RegisterShowListener(ShowListener dlg)
    {
        this.m_showListeners.Add(dlg);
    }

    public void SetDeck(CollectionDeck deck)
    {
        if (deck == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.SetDeckID(): deck is null", new object[0]));
        }
        else
        {
            this.UpdateManaCurve(deck);
            string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(deck.HeroCardID);
            if (string.IsNullOrEmpty(heroPowerCardIdFromHero))
            {
                UnityEngine.Debug.LogWarning("CollectionDeckInfo.UpdateInfo(): invalid hero power ID");
                this.m_heroPowerID = string.Empty;
            }
            else if (!heroPowerCardIdFromHero.Equals(this.m_heroPowerID))
            {
                this.m_heroPowerID = heroPowerCardIdFromHero;
                string vanillaHeroCardIDFromClass = CollectionManager.Get().GetVanillaHeroCardIDFromClass(deck.GetClass());
                CardFlair bestHeroFlairOwned = CollectionManager.Get().GetBestHeroFlairOwned(vanillaHeroCardIDFromClass);
                DefLoader.Get().LoadFullDef(this.m_heroPowerID, new DefLoader.LoadDefCallback<FullDef>(this.OnHeroPowerFullDefLoaded), (bestHeroFlairOwned == null) ? new CardFlair(TAG_PREMIUM.NORMAL) : bestHeroFlairOwned);
            }
        }
    }

    private void SetHeroPowerInfo(string heroPowerCardID, FullDef def, CardFlair cardFlair)
    {
        if (heroPowerCardID.Equals(this.m_heroPowerID))
        {
            EntityDef entityDef = def.GetEntityDef();
            if (cardFlair.Premium == TAG_PREMIUM.GOLDEN)
            {
                this.m_heroPowerActor.Hide();
                this.m_goldenHeroPowerActor.Show();
                this.m_goldenHeroPowerActor.SetEntityDef(def.GetEntityDef());
                this.m_goldenHeroPowerActor.SetCardDef(def.GetCardDef());
                this.m_goldenHeroPowerActor.SetUnlit();
                this.m_goldenHeroPowerActor.SetCardFlair(cardFlair);
                this.m_goldenHeroPowerActor.UpdateAllComponents();
            }
            else
            {
                this.m_heroPowerActor.Show();
                this.m_goldenHeroPowerActor.Hide();
                this.m_heroPowerActor.SetEntityDef(def.GetEntityDef());
                this.m_heroPowerActor.SetCardDef(def.GetCardDef());
                this.m_heroPowerActor.SetUnlit();
                this.m_heroPowerActor.UpdateAllComponents();
            }
            string name = entityDef.GetName();
            this.m_heroPowerName.Text = name;
            string cardTextInHand = entityDef.GetCardTextInHand();
            this.m_heroPowerDescription.Text = cardTextInHand;
        }
    }

    [DebuggerHidden]
    private IEnumerator SetHeroPowerInfoWhenReady(string heroPowerCardID, FullDef def, CardFlair cardFlair)
    {
        return new <SetHeroPowerInfoWhenReady>c__Iterator2D { heroPowerCardID = heroPowerCardID, def = def, cardFlair = cardFlair, <$>heroPowerCardID = heroPowerCardID, <$>def = def, <$>cardFlair = cardFlair, <>f__this = this };
    }

    public void Show()
    {
        if (!this.m_shown)
        {
            this.m_root.SetActive(true);
            this.m_shown = true;
            if (UniversalInputManager.Get().IsTouchMode())
            {
                Navigation.Push(delegate {
                    this.Hide();
                    return true;
                });
            }
            foreach (ShowListener listener in this.m_showListeners.ToArray())
            {
                listener();
            }
            if (UniversalInputManager.Get().IsTouchMode())
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    bool flag = SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL;
                    this.m_renameButton.SetEnabled(!flag);
                    this.m_renameButtonCover.SetActive(flag);
                }
                else
                {
                    bool flag2 = SceneMgr.Get().GetMode() != SceneMgr.Mode.TAVERN_BRAWL;
                    this.m_deleteButton.gameObject.SetActive(flag2);
                    this.m_renameButton.gameObject.SetActive(flag2);
                }
            }
        }
    }

    private void Start()
    {
        this.m_offClicker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClosePressed));
    }

    public void UnregisterHideListener(HideListener dlg)
    {
        this.m_hideListeners.Remove(dlg);
    }

    public void UnregisterShowListener(ShowListener dlg)
    {
        this.m_showListeners.Remove(dlg);
    }

    private void Update()
    {
        if (this.m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
        {
            this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
            if (UniversalInputManager.Get().IsTouchMode())
            {
                this.m_renameButton.gameObject.SetActive(true);
                this.m_deleteButton.gameObject.SetActive(true);
                this.m_offClicker.gameObject.SetActive(true);
                if (this.m_heroPowerActor != null)
                {
                    this.m_heroPowerActor.TurnOffCollider();
                }
                if (this.m_goldenHeroPowerActor != null)
                {
                    this.m_goldenHeroPowerActor.TurnOffCollider();
                }
            }
            else
            {
                this.m_renameButton.gameObject.SetActive(false);
                this.m_deleteButton.gameObject.SetActive(false);
                this.m_offClicker.gameObject.SetActive(false);
                if (this.m_heroPowerActor != null)
                {
                    this.m_heroPowerActor.TurnOnCollider();
                }
                if (this.m_goldenHeroPowerActor != null)
                {
                    this.m_goldenHeroPowerActor.TurnOnCollider();
                }
            }
        }
    }

    public void UpdateManaCurve()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        this.UpdateManaCurve(taggedDeck);
    }

    public void UpdateManaCurve(CollectionDeck deck)
    {
        if (deck == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.UpdateManaCurve(): deck is null.", new object[0]));
        }
        else
        {
            string heroCardID = deck.HeroCardID;
            CardPortraitQuality quality = new CardPortraitQuality(3, TAG_PREMIUM.NORMAL);
            DefLoader.Get().LoadCardDef(heroCardID, new DefLoader.LoadDefCallback<CardDef>(this.OnHeroCardDefLoaded), new object(), quality);
            foreach (DeckInfoManaBar bar in this.m_manaBars)
            {
                bar.m_numCards = 0;
            }
            int numCards = 0;
            foreach (CollectionDeckSlot slot in deck.GetSlots())
            {
                <UpdateManaCurve>c__AnonStorey2C4 storeyc = new <UpdateManaCurve>c__AnonStorey2C4();
                EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
                storeyc.manaCost = entityDef.GetCost();
                if (storeyc.manaCost > this.MAX_MANA_COST_ID)
                {
                    storeyc.manaCost = this.MAX_MANA_COST_ID;
                }
                DeckInfoManaBar bar2 = this.m_manaBars.Find(new Predicate<DeckInfoManaBar>(storeyc.<>m__8C));
                if (bar2 == null)
                {
                    UnityEngine.Debug.LogWarning(string.Format("CollectionDeckInfo.UpdateManaCurve(): Cannot update curve. Could not find mana bar for {0} (cost {1})", entityDef, storeyc.manaCost));
                    return;
                }
                bar2.m_numCards += slot.Count;
                if (bar2.m_numCards > numCards)
                {
                    numCards = bar2.m_numCards;
                }
            }
            foreach (DeckInfoManaBar bar3 in this.m_manaBars)
            {
                bar3.m_numCardsText.Text = Convert.ToString(bar3.m_numCards);
                float t = (numCards != 0) ? (((float) bar3.m_numCards) / ((float) numCards)) : 0f;
                Vector3 localPosition = bar3.m_numCardsText.transform.localPosition;
                localPosition.z = Mathf.Lerp(this.MANA_COST_TEXT_MIN_LOCAL_Z, this.MANA_COST_TEXT_MAX_LOCAL_Z, t);
                bar3.m_numCardsText.transform.localPosition = localPosition;
                bar3.m_barFill.GetComponent<Renderer>().material.SetFloat("_Percent", t);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <SetHeroPowerInfoWhenReady>c__Iterator2D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardFlair <$>cardFlair;
        internal FullDef <$>def;
        internal string <$>heroPowerCardID;
        internal CollectionDeckInfo <>f__this;
        internal CardFlair cardFlair;
        internal FullDef def;
        internal string heroPowerCardID;

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
                    if (this.<>f__this.m_heroPowerActor == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.SetHeroPowerInfo(this.heroPowerCardID, this.def, this.cardFlair);
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
    private sealed class <UpdateManaCurve>c__AnonStorey2C4
    {
        internal int manaCost;

        internal bool <>m__8C(DeckInfoManaBar obj)
        {
            return (obj.m_manaCostID == this.manaCost);
        }
    }

    public delegate void HideListener();

    public delegate void ShowListener();
}

