using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PackOpeningCard : MonoBehaviour
{
    private const TAG_RARITY FALLBACK_RARITY = TAG_RARITY.COMMON;
    private Actor m_actor;
    private NetCache.BoosterCard m_boosterCard;
    private CardDef m_cardDef;
    private CardFlair m_cardFlair;
    public GameObject m_CardParent;
    public Spell m_ClassNameSpell;
    private EntityDef m_entityDef;
    private bool m_inputEnabled;
    private PackOpeningCardRarityInfo m_rarityInfo;
    private PackOpeningCardRarityInfo[] m_RarityInfos;
    private bool m_ready;
    private PegUIElement m_revealButton;
    private bool m_revealed;
    private List<RevealedListener> m_revealedListeners = new List<RevealedListener>();
    private bool m_revealEnabled;
    public GameObject m_SharedHiddenCardObject;
    private Spell m_spell;

    public void AddRevealedListener(RevealedCallback callback)
    {
        this.AddRevealedListener(callback, null);
    }

    public void AddRevealedListener(RevealedCallback callback, object userData)
    {
        RevealedListener item = new RevealedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        this.m_revealedListeners.Add(item);
    }

    public void AttachBoosterCard(NetCache.BoosterCard boosterCard)
    {
        if ((this.m_boosterCard != null) || (boosterCard != null))
        {
            this.m_boosterCard = boosterCard;
            this.m_cardFlair = new CardFlair(this.m_boosterCard.Def.Premium);
            this.Destroy();
            if (this.m_boosterCard == null)
            {
                this.BecomeReady();
            }
            else
            {
                DefLoader.Get().LoadFullDef(this.m_boosterCard.Def.Name, new DefLoader.LoadDefCallback<FullDef>(this.OnFullDefLoaded));
            }
        }
    }

    private void Awake()
    {
        base.StartCoroutine("HackWaitThenDeactivateRarityInfo");
    }

    private void BecomeReady()
    {
        this.m_ready = true;
        this.UpdateInput();
        this.UpdateActor();
    }

    public void Destroy()
    {
        this.m_ready = false;
        if (this.m_actor != null)
        {
            this.m_actor.Destroy();
            this.m_actor = null;
        }
        this.m_rarityInfo = null;
        this.m_spell = null;
        this.m_revealButton = null;
        this.m_revealed = false;
    }

    private bool DetermineRarityInfo()
    {
        TAG_RARITY tag = (this.m_entityDef != null) ? this.m_entityDef.GetRarity() : TAG_RARITY.COMMON;
        PackOpeningRarity packOpeningRarity = GameUtils.GetPackOpeningRarity(tag);
        if (packOpeningRarity == PackOpeningRarity.NONE)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpeningCard.DetermineRarityInfo() - FAILED to determine rarity for {0}", this.GetCardId()));
            return false;
        }
        GameObject packOpeningCardEffects = SceneUtils.FindComponentInParents<PackOpening>(this).GetPackOpeningCardEffects();
        if (packOpeningCardEffects == null)
        {
            UnityEngine.Debug.LogError("PackOpeningCard.DetermineRarityInfo() - Fail to get card effect from PackOpening");
            return false;
        }
        this.m_RarityInfos = packOpeningCardEffects.GetComponentsInChildren<PackOpeningCardRarityInfo>();
        if (this.m_RarityInfos == null)
        {
            UnityEngine.Debug.LogError(string.Format("PackOpeningCard.DetermineRarityInfo() - {0} has no rarity info list. cardId={1}", this, this.GetCardId()));
            return false;
        }
        for (int i = 0; i < this.m_RarityInfos.Length; i++)
        {
            PackOpeningCardRarityInfo info = this.m_RarityInfos[i];
            if (packOpeningRarity == info.m_RarityType)
            {
                this.m_rarityInfo = info;
                this.SetupRarity();
                return true;
            }
        }
        UnityEngine.Debug.LogError(string.Format("PackOpeningCard.DetermineRarityInfo() - {0} has no rarity info for {1}. cardId={2}", this, packOpeningRarity, this.GetCardId()));
        return false;
    }

    public void EnableInput(bool enable)
    {
        this.m_inputEnabled = enable;
        this.UpdateInput();
    }

    private void EnableRarityInfo(PackOpeningCardRarityInfo info, bool enable)
    {
        if (info.m_RarityObject != null)
        {
            info.m_RarityObject.SetActive(enable);
        }
        if (info.m_HiddenCardObject != null)
        {
            info.m_HiddenCardObject.SetActive(enable);
        }
    }

    public void EnableReveal(bool enable)
    {
        this.m_revealEnabled = enable;
        this.UpdateActor();
    }

    private void FireRevealedEvent()
    {
        RevealedListener[] listenerArray = this.m_revealedListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire();
        }
    }

    public void ForceReveal()
    {
        this.OnPress(null);
    }

    public Actor GetActor()
    {
        return this.m_actor;
    }

    public NetCache.BoosterCard GetCard()
    {
        return this.m_boosterCard;
    }

    public CardDef GetCardDef()
    {
        return this.m_cardDef;
    }

    public string GetCardId()
    {
        return ((this.m_boosterCard != null) ? this.m_boosterCard.Def.Name : null);
    }

    private string GetClassName()
    {
        TAG_CLASS tag = this.m_entityDef.GetClass();
        if (tag == TAG_CLASS.INVALID)
        {
            return GameStrings.Get("GLUE_PACK_OPENING_ALL_CLASSES");
        }
        return GameStrings.GetClassName(tag);
    }

    public EntityDef GetEntityDef()
    {
        return this.m_entityDef;
    }

    [DebuggerHidden]
    private IEnumerator HackWaitThenDeactivateRarityInfo()
    {
        return new <HackWaitThenDeactivateRarityInfo>c__Iterator1AD();
    }

    public void HideRarityGlow()
    {
        if (!this.IsRevealed())
        {
            this.OnOut(null);
        }
    }

    public bool IsInputEnabled()
    {
        return this.m_inputEnabled;
    }

    public bool IsReady()
    {
        return this.m_ready;
    }

    public bool IsRevealed()
    {
        return this.m_revealed;
    }

    public bool IsRevealEnabled()
    {
        return this.m_revealEnabled;
    }

    private void OnActorLoaded(string name, GameObject actorObject, object userData)
    {
        if (actorObject == null)
        {
            this.BecomeReady();
            UnityEngine.Debug.LogWarning(string.Format("PackOpeningCard.OnActorLoaded() - FAILED to load actor \"{0}\"", name));
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                this.BecomeReady();
                UnityEngine.Debug.LogWarning(string.Format("PackOpeningCard.OnActorLoaded() - ERROR actor \"{0}\" has no Actor component", name));
            }
            else
            {
                this.m_actor = component;
                this.m_actor.TurnOffCollider();
                SceneUtils.SetLayer(component.gameObject, GameLayer.IgnoreFullScreenEffects);
                this.SetupActor();
                this.BecomeReady();
            }
        }
    }

    private void OnFullDefLoaded(string cardId, FullDef fullDef, object userData)
    {
        if (fullDef == null)
        {
            this.BecomeReady();
            UnityEngine.Debug.LogWarning(string.Format("PackOpeningCard.OnFullDefLoaded() - FAILED to load \"{0}\"", cardId));
        }
        else
        {
            this.m_entityDef = fullDef.GetEntityDef();
            this.m_cardDef = fullDef.GetCardDef();
            if (!this.DetermineRarityInfo())
            {
                this.BecomeReady();
            }
            else
            {
                string handActor = ActorNames.GetHandActor(this.m_entityDef, this.m_cardFlair.Premium);
                AssetLoader.Get().LoadActor(handActor, new AssetLoader.GameObjectCallback(this.OnActorLoaded), null, false);
            }
        }
    }

    private void OnOut(UIEvent e)
    {
        this.m_spell.ActivateState(SpellStateType.CANCEL);
    }

    private void OnOutWhileFlipped(UIEvent e)
    {
        this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
        KeywordHelpPanelManager.Get().HideKeywordHelp();
    }

    private void OnOver(UIEvent e)
    {
        this.m_spell.ActivateState(SpellStateType.BIRTH);
    }

    private void OnOverWhileFlipped(UIEvent e)
    {
        this.m_actor.SetActorState(ActorStateType.CARD_HISTORY);
        KeywordHelpPanelManager.Get().UpdateKeywordHelpForPackOpening(this.m_actor.GetEntityDef(), this.m_actor);
    }

    private void OnPress(UIEvent e)
    {
        this.m_revealed = true;
        this.UpdateInput();
        this.m_spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnSpellFinished));
        this.m_spell.ActivateState(SpellStateType.ACTION);
        this.PlayCorrectSound();
    }

    private void OnSpellFinished(Spell spell, object userData)
    {
        this.FireRevealedEvent();
        this.UpdateInput();
        this.ShowClassName();
        this.m_revealButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnOverWhileFlipped));
        this.m_revealButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnOutWhileFlipped));
    }

    private void PlayCorrectSound()
    {
        switch (this.m_rarityInfo.m_RarityType)
        {
            case PackOpeningRarity.COMMON:
                if (this.m_cardFlair.Premium == TAG_PREMIUM.GOLDEN)
                {
                    SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_C_29");
                }
                break;

            case PackOpeningRarity.RARE:
                if (this.m_cardFlair.Premium != TAG_PREMIUM.GOLDEN)
                {
                    SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_RARE_27");
                    break;
                }
                SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_R_30");
                break;

            case PackOpeningRarity.EPIC:
                if (this.m_cardFlair.Premium != TAG_PREMIUM.GOLDEN)
                {
                    SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_EPIC_26");
                    break;
                }
                SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_E_31");
                break;

            case PackOpeningRarity.LEGENDARY:
                if (this.m_cardFlair.Premium != TAG_PREMIUM.GOLDEN)
                {
                    SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_LEGENDARY_25");
                    break;
                }
                SoundManager.Get().LoadAndPlay("VO_ANNOUNCER_FOIL_L_32");
                break;
        }
    }

    public void RemoveOnOverWhileFlippedListeners()
    {
        this.m_revealButton.RemoveEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnOverWhileFlipped));
        this.m_revealButton.RemoveEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnOutWhileFlipped));
    }

    public void RemoveRevealedListener(RevealedCallback callback)
    {
        this.RemoveRevealedListener(callback, null);
    }

    public void RemoveRevealedListener(RevealedCallback callback, object userData)
    {
        RevealedListener item = new RevealedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        this.m_revealedListeners.Remove(item);
    }

    private void SetupActor()
    {
        this.m_actor.SetEntityDef(this.m_entityDef);
        this.m_actor.SetCardDef(this.m_cardDef);
        this.m_actor.SetCardFlair(this.m_cardFlair);
        this.m_actor.UpdateAllComponents();
    }

    private void SetupRarity()
    {
        GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_rarityInfo.gameObject);
        if (obj2 != null)
        {
            obj2.transform.parent = this.m_CardParent.transform;
            this.m_rarityInfo = obj2.GetComponent<PackOpeningCardRarityInfo>();
            this.m_rarityInfo.m_RarityObject.SetActive(true);
            this.m_rarityInfo.m_HiddenCardObject.SetActive(true);
            Vector3 localPosition = this.m_rarityInfo.m_HiddenCardObject.transform.localPosition;
            this.m_rarityInfo.m_HiddenCardObject.transform.parent = this.m_CardParent.transform;
            this.m_rarityInfo.m_HiddenCardObject.transform.localPosition = localPosition;
            this.m_rarityInfo.m_HiddenCardObject.transform.localRotation = Quaternion.identity;
            this.m_rarityInfo.m_HiddenCardObject.transform.localScale = new Vector3(7.646f, 7.646f, 7.646f);
            TransformUtil.AttachAndPreserveLocalTransform(this.m_rarityInfo.m_RarityObject.transform, this.m_CardParent.transform);
            this.m_spell = this.m_rarityInfo.m_RarityObject.GetComponent<Spell>();
            this.m_revealButton = this.m_rarityInfo.m_RarityObject.GetComponent<PegUIElement>();
            if (UniversalInputManager.Get().IsTouchMode())
            {
                this.m_revealButton.SetReceiveReleaseWithoutMouseDown(true);
            }
            this.m_SharedHiddenCardObject.transform.parent = this.m_rarityInfo.m_HiddenCardObject.transform;
            TransformUtil.Identity(this.m_SharedHiddenCardObject.transform);
        }
    }

    private void ShowClassName()
    {
        string className = this.GetClassName();
        foreach (UberText text in this.m_ClassNameSpell.GetComponentsInChildren<UberText>(true))
        {
            text.Text = className;
        }
        this.m_ClassNameSpell.ActivateState(SpellStateType.BIRTH);
    }

    public void ShowRarityGlow()
    {
        if (!this.IsRevealed())
        {
            this.OnOver(null);
        }
    }

    private void UpdateActor()
    {
        if (this.m_actor != null)
        {
            if (!this.IsRevealEnabled())
            {
                this.m_actor.Hide();
            }
            else
            {
                if (!this.IsRevealed())
                {
                    this.m_actor.Hide();
                }
                Vector3 localScale = this.m_actor.transform.localScale;
                this.m_actor.transform.parent = this.m_rarityInfo.m_RevealedCardObject.transform;
                this.m_actor.transform.localPosition = Vector3.zero;
                this.m_actor.transform.localRotation = Quaternion.identity;
                this.m_actor.transform.localScale = localScale;
            }
        }
    }

    private void UpdateInput()
    {
        if (this.IsReady())
        {
            bool flag = !this.IsRevealed() && this.IsInputEnabled();
            if ((this.m_revealButton != null) && (UniversalInputManager.UsePhoneUI == null))
            {
                if (flag)
                {
                    this.m_revealButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnOver));
                    this.m_revealButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnOut));
                    this.m_revealButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPress));
                    if (PegUI.Get().FindHitElement() == this.m_revealButton)
                    {
                        this.OnOver(null);
                    }
                }
                else
                {
                    this.m_revealButton.RemoveEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnOver));
                    this.m_revealButton.RemoveEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnOut));
                    this.m_revealButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnPress));
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HackWaitThenDeactivateRarityInfo>c__Iterator1AD : IDisposable, IEnumerator, IEnumerator<object>
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
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_005C;

                case 1:
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 2;
                    goto Label_005C;

                case 2:
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_005C:
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

    public delegate void RevealedCallback(object userData);

    private class RevealedListener : EventListener<PackOpeningCard.RevealedCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }
}

