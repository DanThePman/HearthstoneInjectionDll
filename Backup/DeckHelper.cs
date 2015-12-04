using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckHelper : MonoBehaviour
{
    private const float INNKEEPER_POPUP_DURATION = 7f;
    private List<Actor> m_actors = new List<Actor>();
    private List<string> m_cardIdChoices = new List<string>();
    public Vector3 m_deckCardLocalScale = new Vector3(5.75f, 5.75f, 5.75f);
    public UIBButton m_endButton;
    private Vector3 m_innkeeperFullScale;
    public UIBButton m_innkeeperPopup;
    private bool m_innkeeperPopupShown;
    public PegUIElement m_inputBlocker;
    public UberText m_instructionText;
    private List<DelStateChangedListener> m_listeners = new List<DelStateChangedListener>();
    public Collider m_pickArea;
    public GameObject m_rootObject;
    private bool m_shown;
    private static DeckHelper s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_endButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.EndButtonClick));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            if (this.m_innkeeperPopup != null)
            {
                this.m_innkeeperFullScale = this.m_innkeeperPopup.gameObject.transform.localScale;
                this.m_innkeeperPopup.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.InnkeeperPopupClicked));
            }
        }
        else
        {
            this.m_inputBlocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.EndButtonClick));
        }
    }

    private void CleanOldChoices()
    {
        this.m_cardIdChoices.Clear();
        foreach (Actor actor in this.m_actors)
        {
            UnityEngine.Object.Destroy(actor.gameObject);
        }
        this.m_actors.Clear();
    }

    private void EndButtonClick(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void FinishHidePopup()
    {
        this.m_innkeeperPopup.gameObject.SetActive(false);
    }

    private void FireStateChangedEvent()
    {
        foreach (DelStateChangedListener listener in this.m_listeners.ToArray())
        {
            listener(this.m_shown);
        }
    }

    public static DeckHelper Get()
    {
        return s_instance;
    }

    public List<string> GetCardIDChoices()
    {
        return this.m_cardIdChoices;
    }

    private bool HaveActorsForAllChoices()
    {
        return (this.m_actors.Count == this.m_cardIdChoices.Count);
    }

    public void Hide()
    {
        if (this.m_shown)
        {
            this.m_shown = false;
            this.CleanOldChoices();
            this.m_rootObject.SetActive(false);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                FullScreenFXMgr.Get().EndStandardBlurVignette(0.1f, null);
            }
            this.FireStateChangedEvent();
        }
    }

    private void HideInnkeeperPopup()
    {
        if ((this.m_innkeeperPopup != null) && this.m_innkeeperPopupShown)
        {
            this.m_innkeeperPopupShown = false;
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "easetype", iTween.EaseType.easeInExpo, "time", 0.2f, "oncomplete", "FinishHidePopup", "oncompletetarget", base.gameObject };
            iTween.ScaleTo(this.m_innkeeperPopup.gameObject, iTween.Hash(args));
        }
    }

    private void InnkeeperPopupClicked(UIEvent e)
    {
        this.HideInnkeeperPopup();
    }

    public bool IsActive()
    {
        return this.m_shown;
    }

    private void OnActorLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckHelper.OnActorLoaded() - FAILED to load actor \"{0}\"", name));
        }
        else
        {
            Actor component = go.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("DeckHelper.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", name));
            }
            else
            {
                component.transform.parent = base.transform;
                SceneUtils.SetLayer(component, base.gameObject.layer);
                ActorLoadCallback callback = (ActorLoadCallback) callbackData;
                RDMDeckEntry choice = callback.choice;
                EntityDef entityDef = choice.EntityDef;
                CardDef cardDef = callback.cardDef;
                CardFlair cardFlair = choice.Flair;
                component.SetEntityDef(entityDef);
                component.SetCardDef(cardDef);
                component.SetCardFlair(cardFlair);
                component.UpdateAllComponents();
                component.gameObject.name = cardDef.name + "_actor";
                component.GetCollider().gameObject.AddComponent<DeckHelperVisual>().SetActor(component);
                this.m_actors.Add(component);
                if (this.HaveActorsForAllChoices())
                {
                    this.PositionAndShowChoices();
                }
                else
                {
                    component.Hide();
                }
            }
        }
    }

    private void OnCardDefLoaded(string cardID, CardDef cardDef, object userData)
    {
        RDMDeckEntry entry = (RDMDeckEntry) userData;
        ActorLoadCallback callbackData = new ActorLoadCallback {
            choice = entry,
            cardDef = cardDef
        };
        AssetLoader.Get().LoadActor(ActorNames.GetHandActor(entry.EntityDef, entry.Flair.Premium), new AssetLoader.GameObjectCallback(this.OnActorLoaded), callbackData, false);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private bool OnNavigateBack()
    {
        this.Hide();
        return true;
    }

    private void PositionAndShowChoices()
    {
        this.m_pickArea.enabled = true;
        float num = this.m_pickArea.bounds.center.x - this.m_pickArea.bounds.extents.x;
        float num3 = this.m_pickArea.bounds.size.x / 3f;
        float num4 = (this.m_actors.Count != 2) ? (-num3 / 2f) : 0f;
        for (int i = 0; i < this.m_actors.Count; i++)
        {
            Actor actor = this.m_actors[i];
            actor.transform.position = new Vector3((num + ((i + 1) * num3)) + num4, this.m_pickArea.transform.position.y, this.m_pickArea.transform.position.z);
            actor.Show();
            actor.ActivateSpell(SpellType.SUMMON_IN_FORGE);
            actor.transform.localScale = this.m_deckCardLocalScale;
        }
        this.m_pickArea.enabled = false;
    }

    public void RegisterStateChangedListener(DelStateChangedListener listener)
    {
        if (!this.m_listeners.Contains(listener))
        {
            this.m_listeners.Add(listener);
        }
    }

    public void RemoveStateChangedListener(DelStateChangedListener listener)
    {
        this.m_listeners.Remove(listener);
    }

    public void Show()
    {
        if (!this.m_shown)
        {
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
            SoundManager.Get().LoadAndPlay("bar_button_A_press", base.gameObject);
            this.m_shown = true;
            this.m_rootObject.SetActive(true);
            if (!Options.Get().GetBool(Option.HAS_SEEN_DECK_HELPER, false))
            {
                NotificationManager.Get().CreateInnkeeperQuote(GameStrings.Get("VO_ANNOUNCER_CM_HELP_DECK_50"), "VO_ANNOUNCER_CM_HELP_DECK_50", 0f, null);
                Options.Get().SetBool(Option.HAS_SEEN_DECK_HELPER, true);
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                FullScreenFXMgr.Get().StartStandardBlurVignette(0.1f);
            }
            this.FireStateChangedEvent();
            this.UpdateChoices();
        }
    }

    private void ShowInnkeeperPopup()
    {
        if (this.m_innkeeperPopup != null)
        {
            this.m_innkeeperPopup.gameObject.SetActive(true);
            this.m_innkeeperPopupShown = true;
            this.m_innkeeperPopup.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            object[] args = new object[] { "scale", this.m_innkeeperFullScale, "easetype", iTween.EaseType.easeOutElastic, "time", 1f };
            iTween.ScaleTo(this.m_innkeeperPopup.gameObject, iTween.Hash(args));
            base.StopCoroutine("WaitThenHidePopup");
            base.StartCoroutine("WaitThenHidePopup");
        }
    }

    public void UpdateChoices()
    {
        this.CleanOldChoices();
        if (this.IsActive())
        {
            RandomDeckChoices choices = RandomDeckMaker.GetChoices(CollectionManagerDisplay.Get().GetRDMDeck(), 3);
            if (choices == null)
            {
                UnityEngine.Debug.LogError("DeckHelper.GetChoices() - Can't find choices!!!!");
            }
            else
            {
                foreach (RDMDeckEntry entry in choices.choices)
                {
                    this.m_cardIdChoices.Add(entry.EntityDef.GetCardId());
                }
                bool flag = !this.m_instructionText.Text.Equals(choices.displayString);
                this.m_instructionText.Text = choices.displayString;
                if ((UniversalInputManager.UsePhoneUI != null) && flag)
                {
                    if (NotificationManager.Get().IsQuotePlaying)
                    {
                        this.m_instructionText.Text = string.Empty;
                    }
                    else
                    {
                        this.ShowInnkeeperPopup();
                    }
                }
                foreach (RDMDeckEntry entry2 in choices.choices)
                {
                    Log.Ben.Print(entry2.EntityDef.GetDebugName(), new object[0]);
                    CollectionCardCache.Get().LoadCardDef(entry2.EntityDef.GetCardId(), new CollectionCardCache.LoadCardDefCallback(this.OnCardDefLoaded), entry2, new CardPortraitQuality(3, entry2.Flair.Premium));
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenHidePopup()
    {
        return new <WaitThenHidePopup>c__Iterator4D { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenHidePopup>c__Iterator4D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DeckHelper <>f__this;

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
                    this.$current = new WaitForSeconds(7f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.HideInnkeeperPopup();
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

    private class ActorLoadCallback
    {
        public CardDef cardDef;
        public RDMDeckEntry choice;
    }

    public delegate void DelStateChangedListener(bool isActive);
}

