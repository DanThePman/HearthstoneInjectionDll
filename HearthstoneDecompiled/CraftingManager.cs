using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class CraftingManager : MonoBehaviour
{
    private long m_arcaneDustBalance;
    private bool m_cancellingCraftMode;
    public Transform m_cardCounterBone;
    public CraftCardCountTab m_cardCountTab;
    public Vector3 m_cardCountTabHideScale = new Vector3(1f, 1f, 0f);
    public Vector3 m_cardCountTabShowScale = Vector3.one;
    private CardFlair m_cardFlairOfLastTouchedCard;
    private string m_cardIDOfLastTouchedCard;
    private CardInfoPane m_cardInfoPane;
    public Transform m_cardInfoPaneBone;
    public CraftingUI m_craftingUI;
    private Actor m_currentBigActor;
    public float m_delayBeforeBackCardMovesUp;
    public PegUIElement m_dustJar;
    public iTween.EaseType m_easeTypeForCardFlip;
    public iTween.EaseType m_easeTypeForCardMoveUp;
    public Transform m_faceDownCardBone;
    public Transform m_floatingCardBone;
    private Actor m_ghostGoldenMinionActor;
    private Actor m_ghostGoldenSpellActor;
    private Actor m_ghostGoldenWeaponActor;
    private Actor m_ghostMinionActor;
    private Actor m_ghostSpellActor;
    private Actor m_ghostWeaponActor;
    private Actor m_hiddenActor;
    public Transform m_hideCraftingUIBone;
    private bool m_isCurrentActorAGhost;
    public BoxCollider m_offClickCatcher;
    private PendingTransaction m_pendingTransaction;
    public Transform m_showCraftingUIBone;
    private Actor m_templateGoldenMinionActor;
    private Actor m_templateGoldenSpellActor;
    private Actor m_templateGoldenWeaponActor;
    private Actor m_templateHeroSkinActor;
    private Actor m_templateMinionActor;
    private Actor m_templateSpellActor;
    private Actor m_templateWeaponActor;
    public float m_timeForBackCardToMoveUp;
    public float m_timeForCardToFlipUp;
    private int m_transactions;
    private Actor m_upsideDownActor;
    private static CraftingManager s_instance;

    public void AdjustLocalArcaneDustBalance(int amt)
    {
        this.m_arcaneDustBalance += amt;
    }

    private void Awake()
    {
        s_instance = this;
        this.m_arcaneDustBalance = NetCache.Get().GetNetObject<NetCache.NetCacheArcaneDustBalance>().Balance;
        CollectionManager.Get().RegisterMassDisenchantListener(new CollectionManager.OnMassDisenchant(this.OnMassDisenchant));
    }

    public bool CancelCraftMode()
    {
        base.StopAllCoroutines();
        this.m_offClickCatcher.enabled = false;
        this.m_cancellingCraftMode = true;
        this.m_craftingUI.CleanUpEffects();
        float time = 0.2f;
        if (this.m_currentBigActor != null)
        {
            iTween.Stop(this.m_currentBigActor.gameObject);
            iTween.RotateTo(this.m_currentBigActor.gameObject, new Vector3(0f, 0f, 0f), time);
            this.m_currentBigActor.ToggleForceIdle(false);
            if (this.m_upsideDownActor != null)
            {
                iTween.Stop(this.m_upsideDownActor.gameObject);
                this.m_upsideDownActor.transform.parent = this.m_currentBigActor.transform;
            }
        }
        else if (this.m_upsideDownActor != null)
        {
            object[] args = new object[] { "scale", Vector3.zero, "time", time, "oncomplete", "FinishActorMove", "oncompletetarget", base.gameObject, "easetype", iTween.EaseType.easeOutCirc };
            iTween.ScaleTo(this.m_upsideDownActor.gameObject, iTween.Hash(args));
        }
        CollectionCardVisual currentCardVisual = this.GetCurrentCardVisual();
        iTween.Stop(this.m_cardCountTab.gameObject);
        if (currentCardVisual == null)
        {
            if (this.m_currentBigActor != null)
            {
                object[] objArray2 = new object[] { "scale", Vector3.zero, "time", time, "oncomplete", "FinishActorMove", "oncompletetarget", base.gameObject, "easetype", iTween.EaseType.easeOutCirc };
                iTween.ScaleTo(this.m_currentBigActor.gameObject, iTween.Hash(objArray2));
            }
            this.m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
        }
        else if (this.m_currentBigActor != null)
        {
            SoundManager.Get().LoadAndPlay("Card_Transition_In");
            Vector3 vector = currentCardVisual.transform.TransformPoint(Vector3.zero);
            object[] objArray3 = new object[] { "name", "CancelCraftMode", "position", vector, "time", time, "oncomplete", "FinishActorMove", "oncompletetarget", base.gameObject, "easetype", iTween.EaseType.linear };
            iTween.MoveTo(this.m_currentBigActor.gameObject, iTween.Hash(objArray3));
            if (!this.m_isCurrentActorAGhost)
            {
                object[] objArray4 = new object[] { "name", "CancelCraftMode-not ghost", "position", new Vector3(vector.x, vector.y - 0.5f, vector.z), "time", time, "easetype", iTween.EaseType.linear };
                iTween.MoveTo(this.m_cardCountTab.gameObject, iTween.Hash(objArray4));
            }
            object[] objArray5 = new object[] { "scale", Vector3.zero, "time", 0.18f, "easetype", iTween.EaseType.linear };
            iTween.ScaleTo(this.m_currentBigActor.gameObject, iTween.Hash(objArray5));
            if (this.m_upsideDownActor != null)
            {
                iTween.RotateTo(this.m_upsideDownActor.gameObject, new Vector3(0f, 359f, 180f), time);
                object[] objArray6 = new object[] { "name", "CancelCraftMode2", "position", new Vector3(0f, -1f, 0f), "time", time, "islocal", true };
                iTween.MoveTo(this.m_upsideDownActor.gameObject, iTween.Hash(objArray6));
                iTween.ScaleTo(this.m_upsideDownActor.gameObject, new Vector3(this.m_upsideDownActor.transform.localScale.x * 0.8f, this.m_upsideDownActor.transform.localScale.y * 0.8f, this.m_upsideDownActor.transform.localScale.z * 0.8f), time);
            }
        }
        if ((this.m_craftingUI != null) && this.m_craftingUI.IsEnabled())
        {
            this.m_craftingUI.Disable(this.m_hideCraftingUIBone.position);
        }
        this.m_cardCountTab.m_shadow.GetComponent<Animation>().Play("Crafting2ndCardShadowOff");
        this.FadeEffectsOut();
        if (this.m_cardInfoPane != null)
        {
            iTween.Stop(this.m_cardInfoPane.gameObject);
            this.m_cardInfoPane.gameObject.SetActive(false);
        }
        this.TellServerAboutWhatUserDid();
        return true;
    }

    public void CreateButtonPressed()
    {
        this.m_craftingUI.DoCreate();
    }

    public void DisenchantButtonPressed()
    {
        this.m_craftingUI.DoDisenchant();
    }

    public void EnterCraftMode(CollectionCardVisual cardToDisplay)
    {
        if (!this.m_cancellingCraftMode && !CollectionDeckTray.Get().IsWaitingToDeleteDeck())
        {
            CollectionManagerDisplay.Get().HideAllTips();
            this.m_arcaneDustBalance = NetCache.Get().GetNetObject<NetCache.NetCacheArcaneDustBalance>().Balance;
            this.m_offClickCatcher.enabled = true;
            KeywordHelpPanelManager.Get().HideKeywordHelp();
            this.MoveCardToBigSpot(cardToDisplay, true);
            if (this.m_craftingUI == null)
            {
                string name = (UniversalInputManager.UsePhoneUI == null) ? "CraftingUI" : "CraftingUI_Phone";
                this.m_craftingUI = AssetLoader.Get().LoadGameObject(name, true, false).GetComponent<CraftingUI>();
                this.m_craftingUI.SetStartingActive();
                GameUtils.SetParent(this.m_craftingUI, this.m_showCraftingUIBone.gameObject, false);
            }
            if ((this.m_cardInfoPane == null) && (UniversalInputManager.UsePhoneUI == null))
            {
                this.m_cardInfoPane = AssetLoader.Get().LoadGameObject("CardInfoPane", true, false).GetComponent<CardInfoPane>();
            }
            this.m_craftingUI.gameObject.SetActive(true);
            this.m_craftingUI.Enable(this.m_showCraftingUIBone.position, this.m_hideCraftingUIBone.position);
            this.FadeEffectsIn();
            this.UpdateCardInfoPane();
            Navigation.Push(new Navigation.NavigateBackHandler(this.CancelCraftMode));
        }
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.SetBlurBrightness(1f);
        mgr.SetBlurDesaturation(0f);
        mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
        mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, new FullScreenFXMgr.EffectListener(this.OnVignetteFinished));
        mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
    }

    private void FinishActorMove()
    {
        this.m_cancellingCraftMode = false;
        iTween.Stop(this.m_cardCountTab.gameObject);
        this.m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
        if (this.m_upsideDownActor != null)
        {
            UnityEngine.Object.Destroy(this.m_upsideDownActor.gameObject);
        }
        if (this.m_currentBigActor != null)
        {
            UnityEngine.Object.Destroy(this.m_currentBigActor.gameObject);
        }
    }

    private void FinishBigCardMove(bool animate)
    {
        if (this.m_currentBigActor != null)
        {
            int numOwnedCopies = this.GetNumOwnedCopies(this.m_currentBigActor.GetEntityDef().GetCardId(), this.m_currentBigActor.GetCardFlair().Premium);
            if (animate)
            {
                SoundManager.Get().LoadAndPlay("Card_Transition_Out");
                object[] args = new object[] { "name", "FinishBigCardMove", "position", this.m_floatingCardBone.position, "time", 0.4f };
                iTween.MoveTo(this.m_currentBigActor.gameObject, iTween.Hash(args));
                object[] objArray2 = new object[] { "scale", this.m_floatingCardBone.localScale, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuad };
                iTween.ScaleTo(this.m_currentBigActor.gameObject, iTween.Hash(objArray2));
                if (numOwnedCopies > 0)
                {
                    this.m_cardCountTab.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    iTween.MoveTo(this.m_cardCountTab.gameObject, this.m_cardCounterBone.position, 0.4f);
                    iTween.ScaleTo(this.m_cardCountTab.gameObject, this.m_cardCountTabShowScale, 0.4f);
                }
            }
            else
            {
                this.m_currentBigActor.transform.position = this.m_floatingCardBone.position;
                this.m_currentBigActor.transform.localScale = this.m_floatingCardBone.localScale;
                if (numOwnedCopies > 0)
                {
                    this.m_cardCountTab.transform.position = this.m_cardCounterBone.position;
                }
            }
        }
    }

    public void FinishCreateAnims()
    {
        if (!this.m_cancellingCraftMode)
        {
            iTween.ScaleTo(this.m_cardCountTab.gameObject, this.m_cardCountTabShowScale, 0.4f);
            this.m_currentBigActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
            this.m_isCurrentActorAGhost = false;
            int numOwnedCopies = this.GetNumOwnedCopies(this.m_currentBigActor.GetEntityDef().GetCardId(), this.m_currentBigActor.GetCardFlair().Premium);
            this.m_cardCountTab.UpdateText(numOwnedCopies);
            this.m_cardCountTab.transform.position = this.m_cardCounterBone.position;
        }
    }

    public void FinishFlipCurrentActorEarly()
    {
        base.StopAllCoroutines();
        iTween.Stop(this.m_currentBigActor.gameObject);
        this.m_currentBigActor.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        this.m_currentBigActor.transform.position = this.m_floatingCardBone.position;
        this.m_currentBigActor.Show();
        GameObject hiddenStandIn = this.m_currentBigActor.GetHiddenStandIn();
        if (hiddenStandIn != null)
        {
            hiddenStandIn.SetActive(false);
            UnityEngine.Object.Destroy(hiddenStandIn);
        }
    }

    public void FlipCurrentActor()
    {
        if ((this.m_currentBigActor != null) && !this.m_isCurrentActorAGhost)
        {
            this.m_cardCountTab.transform.localScale = this.m_cardCountTabHideScale;
            this.m_upsideDownActor = this.m_currentBigActor;
            this.m_upsideDownActor.name = "UpsideDownActor";
            this.m_upsideDownActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
            this.m_currentBigActor = null;
            iTween.Stop(this.m_upsideDownActor.gameObject);
            object[] args = new object[] { "rotation", new Vector3(0f, 350f, 180f), "time", 1f };
            iTween.RotateTo(this.m_upsideDownActor.gameObject, iTween.Hash(args));
            object[] objArray2 = new object[] { "name", "FlipCurrentActor", "position", this.m_faceDownCardBone.position, "time", 1f };
            iTween.MoveTo(this.m_upsideDownActor.gameObject, iTween.Hash(objArray2));
            base.StartCoroutine(this.ReplaceFaceDownActorWithHiddenCard());
        }
    }

    public void FlipUpsideDownCard(Actor oldActor)
    {
        if (!this.m_cancellingCraftMode)
        {
            int numOwnedCopies = this.GetNumOwnedCopies(this.m_currentBigActor.GetEntityDef().GetCardId(), this.m_currentBigActor.GetCardFlair().Premium);
            if (numOwnedCopies > 1)
            {
                this.m_upsideDownActor = this.GetAndPositionNewUpsideDownActor(this.m_currentBigActor, false);
                this.m_upsideDownActor.name = "UpsideDownActor";
                base.StartCoroutine(this.ReplaceFaceDownActorWithHiddenCard());
            }
            if (numOwnedCopies >= 1)
            {
                object[] objArray1 = new object[] { "scale", this.m_cardCountTabShowScale, "time", 0.4f, "delay", this.m_timeForCardToFlipUp };
                iTween.ScaleTo(this.m_cardCountTab.gameObject, iTween.Hash(objArray1));
                this.m_cardCountTab.UpdateText(numOwnedCopies);
            }
            if (this.m_isCurrentActorAGhost)
            {
                this.m_currentBigActor.gameObject.transform.position = this.m_floatingCardBone.position;
            }
            else
            {
                object[] objArray2 = new object[] { "name", "FlipUpsideDownCard", "position", this.m_floatingCardBone.position, "time", this.m_timeForCardToFlipUp, "easetype", this.m_easeTypeForCardFlip };
                iTween.MoveTo(this.m_currentBigActor.gameObject, iTween.Hash(objArray2));
            }
            object[] args = new object[] { "rotation", new Vector3(0f, 0f, 0f), "time", this.m_timeForCardToFlipUp, "easetype", this.m_easeTypeForCardFlip };
            iTween.RotateTo(this.m_currentBigActor.gameObject, iTween.Hash(args));
            base.StartCoroutine(this.ReplaceHiddenCardwithRealActor(this.m_currentBigActor));
        }
    }

    public void ForceNonGhostFlagOn()
    {
        this.m_isCurrentActorAGhost = false;
    }

    public static CraftingManager Get()
    {
        return s_instance;
    }

    private Actor GetAndPositionNewActor(Actor oldActor, int numCopies)
    {
        Actor ghostActor;
        if (numCopies == 0)
        {
            ghostActor = this.GetGhostActor(oldActor);
        }
        else
        {
            ghostActor = this.GetNonGhostActor(oldActor);
        }
        ghostActor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        return ghostActor;
    }

    private Actor GetAndPositionNewUpsideDownActor(Actor oldActor, bool fromPage)
    {
        Actor andPositionNewActor = this.GetAndPositionNewActor(oldActor, 1);
        SceneUtils.SetLayer(andPositionNewActor.gameObject, GameLayer.IgnoreFullScreenEffects);
        if (fromPage)
        {
            andPositionNewActor.transform.position = oldActor.transform.position + new Vector3(0f, -2f, 0f);
            andPositionNewActor.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            iTween.RotateTo(andPositionNewActor.gameObject, new Vector3(0f, 350f, 180f), 0.4f);
            object[] objArray1 = new object[] { "name", "GetAndPositionNewUpsideDownActor", "position", this.m_faceDownCardBone.position, "time", 0.4f };
            iTween.MoveTo(andPositionNewActor.gameObject, iTween.Hash(objArray1));
            iTween.ScaleTo(andPositionNewActor.gameObject, this.m_faceDownCardBone.localScale, 0.4f);
            return andPositionNewActor;
        }
        andPositionNewActor.transform.localEulerAngles = new Vector3(0f, 350f, 180f);
        andPositionNewActor.transform.position = this.m_faceDownCardBone.position + new Vector3(0f, -6f, 0f);
        andPositionNewActor.transform.localScale = this.m_faceDownCardBone.localScale;
        object[] args = new object[] { "name", "GetAndPositionNewUpsideDownActor", "position", this.m_faceDownCardBone.position, "time", this.m_timeForBackCardToMoveUp, "easetype", this.m_easeTypeForCardMoveUp, "delay", this.m_delayBeforeBackCardMovesUp };
        iTween.MoveTo(andPositionNewActor.gameObject, iTween.Hash(args));
        return andPositionNewActor;
    }

    public NetCache.CardValue GetCardValue(string cardID, TAG_PREMIUM premium)
    {
        NetCache.CardValue value2;
        NetCache.NetCacheCardValues netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardValues>();
        NetCache.CardDefinition key = new NetCache.CardDefinition {
            Name = cardID,
            Premium = premium
        };
        if (!netObject.Values.TryGetValue(key, out value2))
        {
            return null;
        }
        return value2;
    }

    private CollectionCardVisual GetCurrentCardVisual()
    {
        EntityDef def;
        CardFlair flair;
        if (!this.GetShownCardInfo(out def, out flair))
        {
            return null;
        }
        return CollectionManagerDisplay.Get().m_pageManager.GetCardVisual(def.GetCardId(), flair);
    }

    private Actor GetGhostActor(Actor actor)
    {
        this.m_isCurrentActorAGhost = true;
        bool flag = actor.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN;
        Actor ghostMinionActor = this.m_ghostMinionActor;
        switch (actor.GetEntityDef().GetCardType())
        {
            case TAG_CARDTYPE.MINION:
                if (!flag)
                {
                    ghostMinionActor = this.m_ghostMinionActor;
                }
                else
                {
                    ghostMinionActor = this.m_ghostGoldenMinionActor;
                }
                break;

            case TAG_CARDTYPE.SPELL:
                if (!flag)
                {
                    ghostMinionActor = this.m_ghostSpellActor;
                }
                else
                {
                    ghostMinionActor = this.m_ghostGoldenSpellActor;
                }
                break;

            case TAG_CARDTYPE.WEAPON:
                if (!flag)
                {
                    ghostMinionActor = this.m_ghostWeaponActor;
                }
                else
                {
                    ghostMinionActor = this.m_ghostGoldenWeaponActor;
                }
                break;

            default:
                UnityEngine.Debug.LogError("CraftingManager.GetGhostActor() - tried to get a ghost actor for a cardtype that we haven't anticipated!!");
                break;
        }
        return this.SetUpGhostActor(ghostMinionActor, actor);
    }

    public long GetLocalArcaneDustBalance()
    {
        return this.m_arcaneDustBalance;
    }

    private Actor GetNonGhostActor(Actor actor)
    {
        this.m_isCurrentActorAGhost = false;
        return this.SetUpNonGhostActor(this.GetTemplateActor(actor), actor);
    }

    public int GetNumOwnedCopies(string cardID, TAG_PREMIUM premium)
    {
        return this.GetNumOwnedCopies(cardID, premium, true);
    }

    public int GetNumOwnedCopies(string cardID, TAG_PREMIUM premium, bool includePending)
    {
        int numCopiesInCollection = CollectionManager.Get().GetNumCopiesInCollection(cardID, premium);
        if (includePending)
        {
            return (numCopiesInCollection + this.m_transactions);
        }
        return numCopiesInCollection;
    }

    public int GetNumTransactions()
    {
        return this.m_transactions;
    }

    public PendingTransaction GetPendingTransaction()
    {
        return this.m_pendingTransaction;
    }

    public Actor GetShownActor()
    {
        return this.m_currentBigActor;
    }

    public bool GetShownCardInfo(out EntityDef entityDef, out CardFlair cardFlair)
    {
        if (this.m_currentBigActor == null)
        {
            entityDef = null;
            cardFlair = null;
            return false;
        }
        entityDef = this.m_currentBigActor.GetEntityDef();
        cardFlair = this.m_currentBigActor.GetCardFlair();
        return ((entityDef != null) && (cardFlair != null));
    }

    private Actor GetTemplateActor(Actor actor)
    {
        bool flag = actor.GetCardFlair().Premium == TAG_PREMIUM.GOLDEN;
        switch (actor.GetEntityDef().GetCardType())
        {
            case TAG_CARDTYPE.HERO:
                return this.m_templateHeroSkinActor;

            case TAG_CARDTYPE.MINION:
                if (!flag)
                {
                    return this.m_templateMinionActor;
                }
                return this.m_templateGoldenMinionActor;

            case TAG_CARDTYPE.SPELL:
                if (!flag)
                {
                    return this.m_templateSpellActor;
                }
                return this.m_templateGoldenSpellActor;

            case TAG_CARDTYPE.WEAPON:
                if (!flag)
                {
                    return this.m_templateWeaponActor;
                }
                return this.m_templateGoldenWeaponActor;
        }
        UnityEngine.Debug.LogError("CraftingManager.GetGhostActor() - tried to get a ghost actor for a cardtype that we haven't anticipated!!");
        return this.m_templateMinionActor;
    }

    public bool IsCancelling()
    {
        return this.m_cancellingCraftMode;
    }

    public bool IsCardShowing()
    {
        return (this.m_currentBigActor != null);
    }

    private void LoadActor(string actorName, ref Actor actor)
    {
        GameObject obj2 = AssetLoader.Get().LoadActor(actorName, false, false);
        obj2.transform.position = new Vector3(-99999f, 99999f, 99999f);
        actor = obj2.GetComponent<Actor>();
        actor.TurnOffCollider();
    }

    private void LoadActor(string actorName, ref Actor actor, ref Actor actorCopy)
    {
        GameObject obj2 = AssetLoader.Get().LoadActor(actorName, false, false);
        obj2.transform.position = new Vector3(-99999f, 99999f, 99999f);
        actor = obj2.GetComponent<Actor>();
        actorCopy = UnityEngine.Object.Instantiate<Actor>(actor);
        actor.TurnOffCollider();
        actorCopy.TurnOffCollider();
    }

    public void LoadGhostActorIfNecessary()
    {
        if (!this.m_cancellingCraftMode)
        {
            iTween.ScaleTo(this.m_cardCountTab.gameObject, this.m_cardCountTabHideScale, 0.4f);
            if (this.GetNumOwnedCopies(this.m_currentBigActor.GetEntityDef().GetCardId(), this.m_currentBigActor.GetCardFlair().Premium) > 0)
            {
                if (this.m_upsideDownActor == null)
                {
                    this.m_currentBigActor = this.GetAndPositionNewActor(this.m_currentBigActor, 1);
                    this.m_currentBigActor.name = "CurrentBigActor";
                    this.m_currentBigActor.transform.position = this.m_floatingCardBone.position;
                    this.m_currentBigActor.transform.localScale = this.m_floatingCardBone.localScale;
                    this.m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
                    this.SetBigActorLayer(true);
                }
                else
                {
                    this.m_upsideDownActor.transform.parent = null;
                    this.m_currentBigActor = this.m_upsideDownActor;
                    this.m_currentBigActor.name = "CurrentBigActor";
                    this.m_upsideDownActor = null;
                }
            }
            else
            {
                this.m_currentBigActor = this.GetAndPositionNewActor(this.m_currentBigActor, 0);
                this.m_currentBigActor.name = "CurrentBigActor";
                this.m_currentBigActor.transform.position = this.m_floatingCardBone.position;
                this.m_currentBigActor.transform.localScale = this.m_floatingCardBone.localScale;
                this.m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
                this.SetBigActorLayer(true);
            }
        }
    }

    public Actor LoadNewActorAndConstructIt()
    {
        if (this.m_cancellingCraftMode)
        {
            return null;
        }
        if (!this.m_isCurrentActorAGhost)
        {
            Actor currentBigActor = this.m_currentBigActor;
            if (this.m_currentBigActor == null)
            {
                currentBigActor = this.m_upsideDownActor;
            }
            else
            {
                this.m_currentBigActor.name = "Current_Big_Actor_Lost_Refernce";
            }
            this.m_currentBigActor = this.GetAndPositionNewActor(currentBigActor, 0);
            this.m_isCurrentActorAGhost = false;
            this.m_currentBigActor.name = "CurrentBigActor";
            this.m_currentBigActor.transform.position = this.m_floatingCardBone.position;
            this.m_currentBigActor.transform.localScale = this.m_floatingCardBone.localScale;
            this.SetBigActorLayer(true);
        }
        this.m_currentBigActor.transform.parent = this.m_floatingCardBone;
        this.m_currentBigActor.ActivateSpell(SpellType.CONSTRUCT);
        return this.m_currentBigActor;
    }

    [DebuggerHidden]
    private IEnumerator MakeSureActorIsCleanedUp(Actor oldActor)
    {
        return new <MakeSureActorIsCleanedUp>c__Iterator45 { oldActor = oldActor, <$>oldActor = oldActor };
    }

    private void MoveCardToBigSpot(CollectionCardVisual card, bool animate)
    {
        if (card != null)
        {
            Actor oldActor = card.GetActor();
            if (oldActor != null)
            {
                EntityDef entityDef = oldActor.GetEntityDef();
                if (entityDef != null)
                {
                    int numOwnedCopies = this.GetNumOwnedCopies(entityDef.GetCardId(), oldActor.GetCardFlair().Premium);
                    this.m_currentBigActor = this.GetAndPositionNewActor(oldActor, numOwnedCopies);
                    this.m_currentBigActor.name = "CurrentBigActor";
                    if (this.m_isCurrentActorAGhost)
                    {
                        this.m_currentBigActor.transform.position = oldActor.transform.position;
                        this.m_currentBigActor.transform.Translate(0f, -1f, 0f, Space.World);
                    }
                    else
                    {
                        this.m_currentBigActor.transform.position = oldActor.transform.position;
                        this.m_currentBigActor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    }
                    this.SetBigActorLayer(true);
                    this.m_currentBigActor.ToggleForceIdle(true);
                    this.m_currentBigActor.SetActorState(ActorStateType.CARD_IDLE);
                    card.ShowNewItemCallout(false);
                    if (entityDef.IsHero())
                    {
                        this.m_cardCountTab.gameObject.SetActive(false);
                    }
                    else
                    {
                        this.m_cardCountTab.gameObject.SetActive(true);
                        if (numOwnedCopies > 1)
                        {
                            this.m_upsideDownActor = this.GetAndPositionNewUpsideDownActor(oldActor, true);
                            this.m_upsideDownActor.name = "UpsideDownActor";
                            base.StartCoroutine(this.ReplaceFaceDownActorWithHiddenCard());
                        }
                        if (numOwnedCopies > 0)
                        {
                            this.m_cardCountTab.UpdateText(numOwnedCopies);
                            this.m_cardCountTab.transform.position = new Vector3(oldActor.transform.position.x, oldActor.transform.position.y - 2f, oldActor.transform.position.z);
                        }
                    }
                    this.FinishBigCardMove(animate);
                }
            }
        }
    }

    public void NotifyOfTransaction(int amt)
    {
        this.m_transactions += amt;
    }

    public void OnCardCraftingEventNotActiveError(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_CARD_CRAFTING_EVENT_NOT_ACTIVE"),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void OnCardCreated(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        NetCache.Get().OnArcaneDustBalanceChanged((long) -sale.Amount);
        CollectionManagerDisplay.Get().m_pageManager.RefreshCurrentPageContents(new CollectionPageManager.DelOnPageTransitionComplete(this.OnCardCreatedPageTransitioned), sale);
        CollectionCardVisual cardVisual = CollectionManagerDisplay.Get().m_pageManager.GetCardVisual(sale.AssetName, new CardFlair(sale.Premium));
        if ((cardVisual != null) && cardVisual.IsShown())
        {
            cardVisual.OnDoneCrafting();
        }
    }

    private void OnCardCreatedPageTransitioned(object callbackData)
    {
        CollectionManagerDisplay.Get().UpdateCurrentPageCardLocks(false);
    }

    public void OnCardDisenchanted(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        NetCache.Get().OnArcaneDustBalanceChanged((long) sale.Amount);
        if (CollectionManager.Get().GetNumCopiesInCollection(sale.AssetName, sale.Premium) > 0)
        {
            this.OnCardDisenchantedPageTransitioned(sale);
        }
        else
        {
            CollectionManagerDisplay.Get().m_pageManager.RefreshCurrentPageContents(new CollectionPageManager.DelOnPageTransitionComplete(this.OnCardDisenchantedPageTransitioned), sale);
        }
        CollectionCardVisual cardVisual = CollectionManagerDisplay.Get().m_pageManager.GetCardVisual(sale.AssetName, new CardFlair(sale.Premium));
        if ((cardVisual != null) && cardVisual.IsShown())
        {
            cardVisual.OnDoneCrafting();
        }
    }

    private void OnCardDisenchantedPageTransitioned(object callbackData)
    {
        CollectionManagerDisplay.Get().UpdateCurrentPageCardLocks(false);
    }

    public void OnCardDisenchantSoulboundError(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_CARD_SOULBOUND"),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void OnCardGenericError(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_GENERIC_ERROR"),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void OnCardPermissionError(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER"),
            m_text = GameStrings.Get("GLUE_COLLECTION_CARD_PERMISSION_ERROR"),
            m_showAlertIcon = true,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    public void OnCardUnknownError(Network.CardSaleResult sale)
    {
        this.m_pendingTransaction = null;
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER")
        };
        object[] args = new object[] { sale.Action };
        info.m_text = GameStrings.Format("GLUE_COLLECTION_CARD_UNKNOWN_ERROR", args);
        info.m_showAlertIcon = true;
        info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        DialogManager.Get().ShowPopup(info);
    }

    private void OnDestroy()
    {
        CollectionManager.Get().RemoveMassDisenchantListener(new CollectionManager.OnMassDisenchant(this.OnMassDisenchant));
        s_instance = null;
    }

    public void OnMassDisenchant(int amount)
    {
        if (MassDisenchant.Get() == null)
        {
            this.AdjustLocalArcaneDustBalance(amount);
            this.m_craftingUI.UpdateBankText();
        }
    }

    private void OnVignetteFinished()
    {
        this.SetBigActorLayer(false);
        if (this.GetCurrentCardVisual() != null)
        {
            this.GetCurrentCardVisual().OnDoneCrafting();
        }
        if (this.m_currentBigActor != null)
        {
            this.m_currentBigActor.name = "USED_TO_BE_CurrentBigActor";
            base.StartCoroutine(this.MakeSureActorIsCleanedUp(this.m_currentBigActor));
        }
        this.m_currentBigActor = null;
        this.m_craftingUI.gameObject.SetActive(false);
    }

    [DebuggerHidden]
    private IEnumerator ReplaceFaceDownActorWithHiddenCard()
    {
        return new <ReplaceFaceDownActorWithHiddenCard>c__Iterator42 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ReplaceHiddenCardwithRealActor(Actor actor)
    {
        return new <ReplaceHiddenCardwithRealActor>c__Iterator43 { actor = actor, <$>actor = actor };
    }

    private void SetBigActorLayer(bool inCraftingMode)
    {
        if (this.m_currentBigActor != null)
        {
            GameLayer layer = !inCraftingMode ? GameLayer.CardRaycast : GameLayer.IgnoreFullScreenEffects;
            SceneUtils.SetLayer(this.m_currentBigActor.gameObject, layer);
        }
    }

    private Actor SetUpGhostActor(Actor templateActor, Actor actor)
    {
        Actor actor2 = UnityEngine.Object.Instantiate<Actor>(templateActor);
        actor2.SetEntityDef(actor.GetEntityDef());
        actor2.SetCardFlair(actor.GetCardFlair());
        actor2.SetCardDef(actor.GetCardDef());
        actor2.UpdateAllComponents();
        actor2.UpdatePortraitTexture();
        actor2.UpdateCardColor();
        if (actor.isMissingCard())
        {
            actor2.ActivateSpell(SpellType.MISSING_BIGCARD);
            return actor2;
        }
        actor2.ActivateSpell(SpellType.GHOSTMODE);
        return actor2;
    }

    private Actor SetUpNonGhostActor(Actor templateActor, Actor actor)
    {
        Actor actor2 = UnityEngine.Object.Instantiate<Actor>(templateActor);
        actor2.SetEntityDef(actor.GetEntityDef());
        actor2.SetCardFlair(actor.GetCardFlair());
        actor2.SetCardDef(actor.GetCardDef());
        actor2.UpdateAllComponents();
        return actor2;
    }

    public void ShowCraftingUI(UIEvent e)
    {
        if (this.m_craftingUI.IsEnabled())
        {
            this.m_craftingUI.Disable(this.m_hideCraftingUIBone.position);
        }
        else
        {
            this.m_craftingUI.Enable(this.m_showCraftingUIBone.position, this.m_hideCraftingUIBone.position);
        }
    }

    private void Start()
    {
        this.LoadActor("Card_Hand_Weapon", ref this.m_ghostWeaponActor, ref this.m_templateWeaponActor);
        this.LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.WEAPON, TAG_PREMIUM.GOLDEN), ref this.m_ghostGoldenWeaponActor, ref this.m_templateGoldenWeaponActor);
        this.LoadActor("Card_Hand_Ally", ref this.m_ghostMinionActor, ref this.m_templateMinionActor);
        this.LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.MINION, TAG_PREMIUM.GOLDEN), ref this.m_ghostGoldenMinionActor, ref this.m_templateGoldenMinionActor);
        this.LoadActor("Card_Hand_Ability", ref this.m_ghostSpellActor, ref this.m_templateSpellActor);
        this.LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.SPELL, TAG_PREMIUM.GOLDEN), ref this.m_ghostGoldenSpellActor, ref this.m_templateGoldenSpellActor);
        this.LoadActor("Card_Hero_Skin", ref this.m_templateHeroSkinActor);
        this.LoadActor("Card_Hidden", ref this.m_hiddenActor);
        this.m_hiddenActor.GetMeshRenderer().transform.localEulerAngles = new Vector3(0f, 180f, 180f);
        SceneUtils.SetLayer(this.m_hiddenActor.gameObject, GameLayer.IgnoreFullScreenEffects);
        SoundManager.Get().Load("Card_Transition_Out");
        SoundManager.Get().Load("Card_Transition_In");
    }

    private void TellServerAboutWhatUserDid()
    {
        Actor currentBigActor = this.m_currentBigActor;
        if (this.m_currentBigActor == null)
        {
            currentBigActor = this.m_upsideDownActor;
        }
        if (currentBigActor != null)
        {
            CardFlair cardFlair = currentBigActor.GetCardFlair();
            string cardId = currentBigActor.GetEntityDef().GetCardId();
            int assetID = GameUtils.TranslateCardIdToDbId(cardId);
            Log.Ben.Print("Final Transaction Amount = " + this.m_transactions, new object[0]);
            if (this.m_transactions != 0)
            {
                this.m_pendingTransaction = new PendingTransaction();
                this.m_pendingTransaction.CardID = cardId;
                this.m_pendingTransaction.transactionAmt = this.m_transactions;
                this.m_pendingTransaction.cardFlair = cardFlair;
            }
            NetCache.CardValue cardValue = this.GetCardValue(cardId, cardFlair.Premium);
            if (this.m_transactions < 0)
            {
                Network.SellCard(assetID, cardFlair, -this.m_transactions, cardValue.Sell);
            }
            else if (this.m_transactions > 0)
            {
                Network.BuyCard(assetID, cardFlair, this.m_transactions, cardValue.Buy);
            }
            this.m_transactions = 0;
        }
    }

    public void UpdateBankText()
    {
        if (this.m_craftingUI != null)
        {
            this.m_craftingUI.UpdateBankText();
        }
    }

    private void UpdateCardInfoPane()
    {
        if (this.m_cardInfoPane != null)
        {
            this.m_cardInfoPane.gameObject.SetActive(true);
            this.m_cardInfoPane.UpdateText();
            this.m_cardInfoPane.transform.position = this.m_currentBigActor.transform.position - new Vector3(0f, 1f, 0f);
            Vector3 localScale = this.m_cardInfoPaneBone.localScale;
            this.m_cardInfoPane.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            iTween.MoveTo(this.m_cardInfoPane.gameObject, this.m_cardInfoPaneBone.position, 0.5f);
            iTween.ScaleTo(this.m_cardInfoPane.gameObject, localScale, 0.5f);
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForGhostModeToFinishThenAnimate(Vector3 startPos, bool animate)
    {
        return new <WaitForGhostModeToFinishThenAnimate>c__Iterator44 { startPos = startPos, animate = animate, <$>startPos = startPos, <$>animate = animate, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <MakeSureActorIsCleanedUp>c__Iterator45 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>oldActor;
        internal Actor oldActor;

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
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.oldActor != null)
                    {
                        UnityEngine.Object.DestroyImmediate(this.oldActor);
                        this.$PC = -1;
                        break;
                    }
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
    private sealed class <ReplaceFaceDownActorWithHiddenCard>c__Iterator42 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CraftingManager <>f__this;
        internal GameObject <hiddenBuddy>__0;

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
                    if ((this.<>f__this.m_upsideDownActor != null) && (this.<>f__this.m_upsideDownActor.transform.localEulerAngles.z < 90f))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.<>f__this.m_upsideDownActor != null)
                    {
                        this.<hiddenBuddy>__0 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.m_hiddenActor.gameObject);
                        this.<>f__this.m_upsideDownActor.Hide();
                        this.<hiddenBuddy>__0.transform.parent = this.<>f__this.m_upsideDownActor.transform;
                        this.<>f__this.m_upsideDownActor.SetHiddenStandIn(this.<hiddenBuddy>__0);
                        this.<hiddenBuddy>__0.transform.localScale = new Vector3(1f, 1f, 1f);
                        this.<hiddenBuddy>__0.transform.localPosition = new Vector3(0f, 0f, 0f);
                        this.<hiddenBuddy>__0.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                        this.$PC = -1;
                    }
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
    private sealed class <ReplaceHiddenCardwithRealActor>c__Iterator43 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal GameObject <standIn>__0;
        internal Actor actor;

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
                    if (((this.actor != null) && (this.actor.transform.localEulerAngles.z > 90f)) && (this.actor.transform.localEulerAngles.z < 270f))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (this.actor != null)
                    {
                        this.actor.Show();
                        this.<standIn>__0 = this.actor.GetHiddenStandIn();
                        if (this.<standIn>__0 != null)
                        {
                            this.<standIn>__0.SetActive(false);
                            UnityEngine.Object.Destroy(this.<standIn>__0);
                            this.$PC = -1;
                        }
                    }
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
    private sealed class <WaitForGhostModeToFinishThenAnimate>c__Iterator44 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>animate;
        internal Vector3 <$>startPos;
        internal CraftingManager <>f__this;
        internal Spell <ghostModeSpell>__0;
        internal bool animate;
        internal Vector3 startPos;

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
                    this.<ghostModeSpell>__0 = this.<>f__this.m_currentBigActor.GetSpell(SpellType.GHOSTMODE);
                    break;

                case 1:
                    break;

                default:
                    goto Label_0094;
            }
            if (this.<ghostModeSpell>__0.GetActiveState() != SpellStateType.NONE)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.m_currentBigActor.transform.position = this.startPos;
            this.<>f__this.FinishBigCardMove(this.animate);
            this.$PC = -1;
        Label_0094:
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

