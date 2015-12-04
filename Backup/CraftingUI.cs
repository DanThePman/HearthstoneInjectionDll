using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    private GameObject m_activeObject;
    public UberText m_bankAmountText;
    public CreateButton m_buttonCreate;
    public DisenchantButton m_buttonDisenchant;
    private Actor m_constructingActor;
    public float m_craftDelayBeforeConstructSpell;
    public float m_craftDelayBeforeGhostDeath;
    public SoundDef m_craftingSound;
    private Notification m_craftNotification;
    public UberText m_craftValue;
    public float m_disenchantDelayBeforeBallsComeOut;
    public float m_disenchantDelayBeforeCardExplodes;
    public float m_disenchantDelayBeforeCardFlips;
    public SoundDef m_disenchantSound;
    public UberText m_disenchantValue;
    private bool m_enabled;
    private Actor m_explodingActor;
    public GameObject m_glowballs;
    private bool m_initializedPositions;
    private bool m_isAnimating;
    private bool m_mousedOver;
    public Collider m_mouseOverCollider;
    public UberText m_soulboundDesc;
    public GameObject m_soulboundNotification;
    public UberText m_soulboundTitle;
    private List<GameObject> m_thingsToDestroy = new List<GameObject>();

    public void CleanUpEffects()
    {
        if (this.m_explodingActor != null)
        {
            Spell spell = this.m_explodingActor.GetSpell(SpellType.DECONSTRUCT);
            if ((spell != null) && (spell.GetActiveState() != SpellStateType.NONE))
            {
                this.m_explodingActor.GetSpell(SpellType.DECONSTRUCT).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
                this.m_explodingActor.Hide();
            }
        }
        if (this.m_constructingActor != null)
        {
            Spell spell2 = this.m_constructingActor.GetSpell(SpellType.CONSTRUCT);
            if ((spell2 != null) && (spell2.GetActiveState() != SpellStateType.NONE))
            {
                this.m_constructingActor.GetSpell(SpellType.CONSTRUCT).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
                this.m_constructingActor.Hide();
            }
        }
        base.GetComponent<PlayMakerFSM>().SendEvent("Cancel");
        this.m_isAnimating = false;
    }

    private void CreateCraftNotification()
    {
        if (this.m_buttonCreate.IsButtonEnabled())
        {
            Vector3 vector;
            Notification.PopUpArrowDirection down;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                vector = new Vector3(73.3f, 1f, 55.4f);
                down = Notification.PopUpArrowDirection.Down;
            }
            else
            {
                vector = new Vector3(55f, 1f, -56f);
                down = Notification.PopUpArrowDirection.Left;
            }
            this.m_craftNotification = NotificationManager.Get().CreatePopupText(vector, (Vector3) (16f * Vector3.one), GameStrings.Get("GLUE_COLLECTION_TUTORIAL06"), false);
            this.m_craftNotification.ShowPopUpArrow(down);
        }
    }

    private void CreateDisenchantNotification()
    {
        if (!this.m_buttonDisenchant.IsButtonEnabled())
        {
        }
    }

    public void Disable(Vector3 hidePosition)
    {
        this.m_enabled = false;
        object[] args = new object[] { "time", 0.4f, "position", hidePosition };
        iTween.MoveTo(this.m_activeObject, iTween.Hash(args));
        this.HideTips();
    }

    public void DoCreate()
    {
        this.UpdateTips();
        if (!Options.Get().GetBool(Option.HAS_CRAFTED))
        {
            Options.Get().SetBool(Option.HAS_CRAFTED, true);
        }
        string cardId = CraftingManager.Get().GetShownActor().GetEntityDef().GetCardId();
        TAG_PREMIUM premium = CraftingManager.Get().GetShownActor().GetCardFlair().Premium;
        CraftingManager.Get().AdjustLocalArcaneDustBalance(-this.GetCardBuyValue(cardId, premium));
        CraftingManager.Get().NotifyOfTransaction(1);
        if (CraftingManager.Get().GetNumOwnedCopies(cardId, premium) > 1)
        {
            CraftingManager.Get().ForceNonGhostFlagOn();
        }
        this.UpdateText();
        this.StopCurrentAnim();
        base.StartCoroutine(this.DoCreateAnims());
        CraftingManager.Get().StartCoroutine(this.StartDisenchantCooldown());
    }

    [DebuggerHidden]
    private IEnumerator DoCreateAnims()
    {
        return new <DoCreateAnims>c__Iterator4B { <>f__this = this };
    }

    public void DoDisenchant()
    {
        this.UpdateTips();
        Options.Get().SetBool(Option.HAS_DISENCHANTED, true);
        CraftingManager.Get().AdjustLocalArcaneDustBalance(this.GetCardSellValue(CraftingManager.Get().GetShownActor().GetEntityDef().GetCardId(), CraftingManager.Get().GetShownActor().GetCardFlair().Premium));
        CraftingManager.Get().NotifyOfTransaction(-1);
        this.UpdateText();
        if (this.m_isAnimating)
        {
            CraftingManager.Get().FinishFlipCurrentActorEarly();
        }
        this.StopCurrentAnim();
        base.StartCoroutine(this.DoDisenchantAnims());
        CraftingManager.Get().StartCoroutine(this.StartCraftCooldown());
    }

    [DebuggerHidden]
    private IEnumerator DoDisenchantAnims()
    {
        return new <DoDisenchantAnims>c__Iterator4A { <>f__this = this };
    }

    public void Enable(Vector3 showPosition, Vector3 hidePosition)
    {
        if (!this.m_initializedPositions)
        {
            base.transform.position = hidePosition;
            this.m_soulboundNotification.transform.position = base.transform.position;
            this.m_soulboundTitle.Text = GameStrings.Get("GLUE_CRAFTING_SOULBOUND");
            this.m_soulboundDesc.Text = GameStrings.Get("GLUE_CRAFTING_SOULBOUND_DESC");
            this.m_activeObject = base.gameObject;
            this.m_initializedPositions = true;
        }
        this.m_enabled = true;
        this.UpdateText();
        this.m_activeObject.SetActive(true);
        object[] args = new object[] { "time", 0.5f, "position", showPosition };
        iTween.MoveTo(this.m_activeObject, iTween.Hash(args));
        this.ShowFirstTimeTips();
    }

    private int GetCardBuyValue(string cardID, TAG_PREMIUM premium)
    {
        NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(cardID, premium);
        if (CraftingManager.Get().GetNumTransactions() >= 0)
        {
            return cardValue.Buy;
        }
        return cardValue.Sell;
    }

    private int GetCardSellValue(string cardID, TAG_PREMIUM premium)
    {
        NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(cardID, premium);
        if (CraftingManager.Get().GetNumTransactions() <= 0)
        {
            return cardValue.Sell;
        }
        return cardValue.Buy;
    }

    private void HideTips()
    {
        if (this.m_craftNotification != null)
        {
            NotificationManager.Get().DestroyNotificationNowWithNoAnim(this.m_craftNotification);
        }
    }

    private bool IsCraftingEventForCardActive(string cardID)
    {
        DbfRecord cardRecord = GameUtils.GetCardRecord(cardID);
        if (cardRecord == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("CraftingUI.IsCraftingEventForCardActive could not find DBF record for card {0}, assuming it cannot be crafted or disenchanted", cardID));
            return false;
        }
        string eventName = cardRecord.GetString("CRAFTING_EVENT");
        return SpecialEventManager.Get().IsEventActive(eventName, true);
    }

    public bool IsEnabled()
    {
        return this.m_enabled;
    }

    private void NotifyOfMouseOut()
    {
        if (this.m_mousedOver)
        {
            this.m_mousedOver = false;
            base.GetComponent<PlayMakerFSM>().SendEvent("IdleCancel");
        }
    }

    private void NotifyOfMouseOver()
    {
        if (!this.m_mousedOver)
        {
            this.m_mousedOver = true;
            base.GetComponent<PlayMakerFSM>().SendEvent("Idle");
        }
    }

    public void SetStartingActive()
    {
        this.m_soulboundNotification.SetActive(false);
        base.gameObject.SetActive(false);
    }

    private void ShowFirstTimeTips()
    {
        if ((this.m_activeObject != this.m_soulboundNotification) && !Options.Get().GetBool(Option.HAS_CRAFTED))
        {
            this.CreateDisenchantNotification();
            this.CreateCraftNotification();
        }
    }

    [DebuggerHidden]
    private IEnumerator StartCraftCooldown()
    {
        return new <StartCraftCooldown>c__Iterator49 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator StartDisenchantCooldown()
    {
        return new <StartDisenchantCooldown>c__Iterator48 { <>f__this = this };
    }

    private void StopCurrentAnim()
    {
        if (this.m_isAnimating)
        {
            base.StopAllCoroutines();
            this.CleanUpEffects();
            foreach (GameObject obj2 in this.m_thingsToDestroy)
            {
                if (obj2 != null)
                {
                    UnityEngine.Object.Destroy(obj2);
                }
            }
        }
    }

    private void Update()
    {
        if (this.m_enabled)
        {
            if (this.m_isAnimating)
            {
                this.m_mousedOver = false;
            }
            else
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
                LayerMask mask = 0x200;
                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, (int) mask))
                {
                    if (hit.collider == this.m_mouseOverCollider)
                    {
                        this.NotifyOfMouseOver();
                    }
                    else
                    {
                        this.NotifyOfMouseOut();
                    }
                }
            }
        }
    }

    public void UpdateBankText()
    {
        this.m_bankAmountText.Text = CraftingManager.Get().GetLocalArcaneDustBalance().ToString();
        BnetBar.Get().m_currencyFrame.RefreshContents();
        if ((UniversalInputManager.UsePhoneUI != null) && (CraftingTray.Get() != null))
        {
            ArcaneDustAmount.Get().UpdateCurrentDustAmount();
        }
    }

    public void UpdateText()
    {
        EntityDef def;
        CardFlair flair;
        this.UpdateBankText();
        if (!CraftingManager.Get().GetShownCardInfo(out def, out flair))
        {
            this.m_buttonDisenchant.DisableButton();
            this.m_buttonCreate.DisableButton();
            return;
        }
        NetCache.CardDefinition definition = new NetCache.CardDefinition {
            Name = def.GetCardId(),
            Premium = flair.Premium
        };
        int numOwnedCopies = CraftingManager.Get().GetNumOwnedCopies(definition.Name, definition.Premium);
        bool flag = false;
        string str = string.Empty;
        string howToEarnText = string.Empty;
        TAG_CARD_SET cardSet = def.GetCardSet();
        string cardSetName = GameStrings.GetCardSetName(cardSet);
        NetCache.CardValue cardValue = CraftingManager.Get().GetCardValue(definition.Name, definition.Premium);
        if (cardValue == null)
        {
            flag = false;
            str = GameStrings.Get("GLUE_CRAFTING_SOULBOUND");
            if (numOwnedCopies == 0)
            {
                howToEarnText = def.GetHowToEarnText(definition.Premium);
            }
            else
            {
                switch (cardSet)
                {
                    case TAG_CARD_SET.CORE:
                        howToEarnText = GameStrings.Get("GLUE_CRAFTING_SOULBOUND_BASIC_DESC");
                        goto Label_01D7;

                    case TAG_CARD_SET.REWARD:
                    case TAG_CARD_SET.PROMO:
                        howToEarnText = GameStrings.Get("GLUE_CRAFTING_SOULBOUND_REWARD_DESC");
                        goto Label_01D7;
                }
                howToEarnText = GameStrings.Get("GLUE_CRAFTING_SOULBOUND_DESC");
            }
        }
        else if (this.IsCraftingEventForCardActive(definition.Name))
        {
            int numTransactions = CraftingManager.Get().GetNumTransactions();
            int buy = cardValue.Buy;
            if (numTransactions < 0)
            {
                buy = cardValue.Sell;
            }
            int sell = cardValue.Sell;
            if (numTransactions > 0)
            {
                sell = cardValue.Buy;
            }
            this.m_disenchantValue.Text = "+" + sell.ToString();
            this.m_craftValue.Text = "-" + buy.ToString();
            flag = true;
        }
        else
        {
            str = GameStrings.Get("GLUE_CRAFTING_EVENT_NOT_ACTIVE_TITLE");
            object[] args = new object[] { cardSetName };
            howToEarnText = GameStrings.Format("GLUE_CRAFTING_EVENT_NOT_ACTIVE_DESCRIPTION", args);
            flag = false;
        }
    Label_01D7:
        if (!flag)
        {
            this.m_buttonDisenchant.DisableButton();
            this.m_buttonCreate.DisableButton();
            this.m_soulboundNotification.SetActive(true);
            this.m_activeObject = this.m_soulboundNotification;
            this.m_soulboundTitle.Text = str;
            this.m_soulboundDesc.Text = howToEarnText;
        }
        else if (!FixedRewardsMgr.Get().CanCraftCard(definition.Name, definition.Premium))
        {
            this.m_buttonDisenchant.DisableButton();
            this.m_buttonCreate.DisableButton();
            this.m_soulboundNotification.SetActive(true);
            this.m_activeObject = this.m_soulboundNotification;
            this.m_soulboundTitle.Text = cardSetName;
            this.m_soulboundDesc.Text = def.GetHowToEarnText(definition.Premium);
        }
        else
        {
            this.m_soulboundNotification.SetActive(false);
            this.m_activeObject = base.gameObject;
            if (numOwnedCopies <= 0)
            {
                this.m_buttonDisenchant.DisableButton();
            }
            else
            {
                this.m_buttonDisenchant.EnableButton();
            }
            int num5 = !def.IsElite() ? 2 : 1;
            long localArcaneDustBalance = CraftingManager.Get().GetLocalArcaneDustBalance();
            if ((numOwnedCopies >= num5) || (localArcaneDustBalance < this.GetCardBuyValue(definition.Name, definition.Premium)))
            {
                this.m_buttonCreate.DisableButton();
            }
            else
            {
                this.m_buttonCreate.EnableButton();
            }
        }
    }

    private void UpdateTips()
    {
        if (Options.Get().GetBool(Option.HAS_CRAFTED))
        {
            this.HideTips();
        }
        else if (this.m_craftNotification == null)
        {
            this.CreateCraftNotification();
        }
        else if (!this.m_buttonCreate.IsButtonEnabled())
        {
            NotificationManager.Get().DestroyNotification(this.m_craftNotification, 0f);
        }
    }

    [CompilerGenerated]
    private sealed class <DoCreateAnims>c__Iterator4B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingUI <>f__this;
        internal CardDef <cardDef>__0;
        internal PlayMakerFSM <playmaker>__1;

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
                    this.<cardDef>__0 = CraftingManager.Get().GetShownActor().GetCardDef();
                    SoundManager.Get().Play(this.<>f__this.m_craftingSound.GetComponent<AudioSource>());
                    SoundManager.Get().Stop(this.<>f__this.m_disenchantSound.GetComponent<AudioSource>());
                    this.<>f__this.m_isAnimating = true;
                    CraftingManager.Get().FlipCurrentActor();
                    this.<playmaker>__1 = this.<>f__this.GetComponent<PlayMakerFSM>();
                    this.<playmaker>__1.SendEvent("Birth");
                    this.$current = new WaitForSeconds(this.<>f__this.m_craftDelayBeforeConstructSpell);
                    this.$PC = 1;
                    goto Label_01A3;

                case 1:
                    if (!CraftingManager.Get().IsCancelling())
                    {
                        this.<>f__this.m_constructingActor = CraftingManager.Get().LoadNewActorAndConstructIt();
                        this.<>f__this.UpdateBankText();
                        this.$current = new WaitForSeconds(this.<>f__this.m_craftDelayBeforeGhostDeath);
                        this.$PC = 2;
                        goto Label_01A3;
                    }
                    break;

                case 2:
                    if (!CraftingManager.Get().IsCancelling())
                    {
                        if ((this.<cardDef>__0 != null) && (this.<cardDef>__0.m_PlayEffectDef != null))
                        {
                            GameUtils.PlayCardEffectDefSounds(this.<cardDef>__0.m_PlayEffectDef);
                        }
                        CraftingManager.Get().FinishCreateAnims();
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 3;
                        goto Label_01A3;
                    }
                    break;

                case 3:
                    this.<>f__this.m_isAnimating = false;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_01A3:
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
    private sealed class <DoDisenchantAnims>c__Iterator4A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingUI <>f__this;
        internal Actor <oldActor>__1;
        internal PlayMakerFSM <playmaker>__0;

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
                    SoundManager.Get().Play(this.<>f__this.m_disenchantSound.GetComponent<AudioSource>());
                    SoundManager.Get().Stop(this.<>f__this.m_craftingSound.GetComponent<AudioSource>());
                    this.<>f__this.m_isAnimating = true;
                    this.<playmaker>__0 = this.<>f__this.GetComponent<PlayMakerFSM>();
                    this.<playmaker>__0.SendEvent("Birth");
                    this.$current = new WaitForSeconds(this.<>f__this.m_disenchantDelayBeforeCardExplodes);
                    this.$PC = 1;
                    goto Label_0268;

                case 1:
                case 2:
                    if (CraftingManager.Get().GetShownActor() == null)
                    {
                        this.$current = null;
                        this.$PC = 2;
                    }
                    else
                    {
                        this.<>f__this.m_explodingActor = CraftingManager.Get().GetShownActor();
                        this.<>f__this.UpdateBankText();
                        if (CraftingManager.Get().IsCancelling())
                        {
                            break;
                        }
                        CraftingManager.Get().LoadGhostActorIfNecessary();
                        this.<>f__this.m_explodingActor.ActivateSpell(SpellType.DECONSTRUCT);
                        this.$current = new WaitForSeconds(this.<>f__this.m_disenchantDelayBeforeCardFlips);
                        this.$PC = 3;
                    }
                    goto Label_0268;

                case 3:
                    if (!CraftingManager.Get().IsCancelling())
                    {
                        CraftingManager.Get().FlipUpsideDownCard(this.<>f__this.m_explodingActor);
                        this.<oldActor>__1 = this.<>f__this.m_explodingActor;
                        this.<>f__this.m_thingsToDestroy.Add(this.<>f__this.m_explodingActor.gameObject);
                        this.$current = new WaitForSeconds(this.<>f__this.m_disenchantDelayBeforeBallsComeOut);
                        this.$PC = 4;
                        goto Label_0268;
                    }
                    break;

                case 4:
                    if (!CraftingManager.Get().IsCancelling())
                    {
                        this.<playmaker>__0.SendEvent("Action");
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 5;
                        goto Label_0268;
                    }
                    break;

                case 5:
                    this.<>f__this.m_isAnimating = false;
                    this.$current = new WaitForSeconds(10f);
                    this.$PC = 6;
                    goto Label_0268;

                case 6:
                    if (this.<oldActor>__1 != null)
                    {
                        UnityEngine.Object.Destroy(this.<oldActor>__1.gameObject);
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0268:
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
    private sealed class <StartCraftCooldown>c__Iterator49 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingUI <>f__this;

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
                    if (this.<>f__this.m_buttonCreate.GetComponent<Collider>().enabled)
                    {
                        this.<>f__this.m_buttonCreate.GetComponent<Collider>().enabled = false;
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.<>f__this.m_buttonCreate.GetComponent<Collider>().enabled = true;
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
    private sealed class <StartDisenchantCooldown>c__Iterator48 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingUI <>f__this;

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
                    if (this.<>f__this.m_buttonDisenchant.GetComponent<Collider>().enabled)
                    {
                        this.<>f__this.m_buttonDisenchant.GetComponent<Collider>().enabled = false;
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.<>f__this.m_buttonDisenchant.GetComponent<Collider>().enabled = true;
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

