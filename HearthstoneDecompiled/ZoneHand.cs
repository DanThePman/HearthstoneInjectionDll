using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneHand : Zone
{
    private const float ANGLE_OF_CARDS = 40f;
    private readonly PlatformDependentValue<float> BASELINE_ASPECT_RATIO;
    private static PlatformDependentValue<float[]> CARD_PIXEL_WIDTHS;
    private static float[] CARD_PIXEL_WIDTHS_PHONE = new float[] { 0f, 0.148f, 0.148f, 0.148f, 0.148f, 0.148f, 0.148f, 0.143f, 0.125f, 0.111f, 0.1f };
    private static float[] CARD_PIXEL_WIDTHS_TABLET = new float[] { 0f, 0.08f, 0.08f, 0.08f, 0.08f, 0.074f, 0.069f, 0.064f, 0.06f, 0.056f, 0.054f };
    private const float CARD_WIDTH = 2.049684f;
    private Vector3 centerOfHand;
    private const float DEFAULT_ANIMATE_TIME = 0.35f;
    private const float DRIFT_AMOUNT = 0.08f;
    private bool enemyHand;
    public const float HAND_SCALE = 0.62f;
    public const float HAND_SCALE_OPPONENT = 0.682f;
    public const float HAND_SCALE_OPPONENT_Y = 0.225f;
    public const float HAND_SCALE_Y = 0.225f;
    private int lastMousedOver;
    private bool m_doNotUpdateLayout;
    public float m_enlargedHandCardMaxX;
    public float m_enlargedHandCardMinX;
    public Vector3 m_enlargedHandCardScale;
    public float m_enlargedHandDefaultCardSpacing;
    public Vector3 m_enlargedHandPosition;
    public Vector3 m_enlargedHandScale;
    private bool m_flipHandCards;
    private bool m_handEnlarged;
    public float m_handHidingDistance;
    private bool m_handMoving;
    public GameObject m_heroHitbox;
    public float m_heroWidthInHand;
    private CardStandIn m_hiddenStandIn;
    public GameObject m_iPhoneCardPosition;
    public GameObject m_iPhonePreviewBone;
    public GameObject m_leftArrow;
    public ManaCrystalMgr m_manaGemMgr;
    public GameObject m_manaGemPosition;
    private float m_maxWidth;
    public GameObject m_playCardButton;
    public GameObject m_rightArrow;
    public Float_MobileOverride m_SelectCardOffsetZ;
    public Float_MobileOverride m_SelectCardScale;
    private Vector3 m_startingPosition;
    private Vector3 m_startingScale;
    private bool m_targetingMode;
    public Float_MobileOverride m_TouchDragResistanceFactorY;
    private int m_touchedSlot;
    private static int MAX_CARDS = 10;
    public const float MOUSE_OVER_SCALE = 1.5f;
    private const float RESISTANCE_BASE = 10f;
    private List<CardStandIn> standIns;
    private const float Z_ROTATION_ON_LEFT = 354.5f;
    private const float Z_ROTATION_ON_RIGHT = 3f;

    static ZoneHand()
    {
        PlatformDependentValue<float[]> value2 = new PlatformDependentValue<float[]>(PlatformCategory.Screen) {
            PC = CARD_PIXEL_WIDTHS_TABLET,
            Tablet = CARD_PIXEL_WIDTHS_TABLET,
            Phone = CARD_PIXEL_WIDTHS_PHONE
        };
        CARD_PIXEL_WIDTHS = value2;
    }

    public ZoneHand()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1.333333f,
            Tablet = 1.333333f,
            Phone = 1.775f
        };
        this.BASELINE_ASPECT_RATIO = value2;
        this.lastMousedOver = -1;
        this.m_doNotUpdateLayout = true;
    }

    public override bool AddCard(Card card)
    {
        return base.AddCard(card);
    }

    private void Awake()
    {
        this.enemyHand = base.m_Side == Player.Side.OPPOSING;
        this.m_startingPosition = base.gameObject.transform.localPosition;
        this.m_startingScale = base.gameObject.transform.localScale;
        this.UpdateCenterAndWidth();
    }

    private void BlowUpOldStandins()
    {
        if (this.standIns == null)
        {
            this.standIns = new List<CardStandIn>();
        }
        else
        {
            foreach (CardStandIn @in in this.standIns)
            {
                if (@in != null)
                {
                    UnityEngine.Object.Destroy(@in.gameObject);
                }
            }
            this.standIns = new List<CardStandIn>();
        }
    }

    protected bool CanAnimateCard(Card card)
    {
        bool flag = this.enemyHand && (card.GetPrevZone() is ZonePlay);
        if (card.IsDoNotSort())
        {
            if (flag)
            {
                object[] args = new object[] { card };
                Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED card.IsDoNotSort()", args);
            }
            return false;
        }
        if (!card.IsActorReady())
        {
            if (flag)
            {
                object[] objArray2 = new object[] { card };
                Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED !card.IsActorReady()", objArray2);
            }
            return false;
        }
        if ((!base.m_controller.IsFriendlySide() || (TurnStartManager.Get() == null)) || !TurnStartManager.Get().IsCardDrawHandled(card))
        {
            if (this.IsCardNotInEnemyHandAnymore(card))
            {
                if (flag)
                {
                    object[] objArray3 = new object[] { card };
                    Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED IsCardNotInEnemyHandAnymore()", objArray3);
                }
                return false;
            }
            if (!card.HasBeenGrabbedByEnemyActionHandler())
            {
                return true;
            }
            if (flag)
            {
                object[] objArray4 = new object[] { card };
                Log.FaceDownCard.Print("ZoneHand.CanAnimateCard() - card={0} FAILED card.HasBeenGrabbedByEnemyActionHandler()", objArray4);
            }
        }
        return false;
    }

    private bool CanPlaySpellPowerHint(Card card)
    {
        if (!card.IsShown())
        {
            return false;
        }
        if (!card.GetActor().IsShown())
        {
            return false;
        }
        Entity entity = card.GetEntity();
        return (entity.IsAffectedBySpellPower() || TextUtils.HasBonusDamage(entity.GetStringTag(GAME_TAG.CARDTEXT_INHAND)));
    }

    private void CardColliderLoadedCallback(string name, GameObject go, object callbackData)
    {
        Card card = (Card) callbackData;
        Actor actor = card.GetActor();
        if (actor != null)
        {
            actor.GetMeshRenderer().gameObject.layer = 0;
        }
        go.transform.localEulerAngles = this.GetCardRotation(card);
        go.transform.position = this.GetCardPosition(card);
        go.transform.localScale = this.GetCardScale(card);
        CardStandIn component = go.GetComponent<CardStandIn>();
        component.slot = card.GetZonePosition();
        component.linkedCard = card;
        this.standIns.Add(component);
        if (!component.linkedCard.CardStandInIsInteractive())
        {
            component.DisableStandIn();
        }
    }

    private void DuplicateColliderAndStuffItIn(Card card)
    {
        AssetLoader.Get().LoadActor("Card_Collider_Standin", new AssetLoader.GameObjectCallback(this.CardColliderLoadedCallback), card, false);
    }

    public void ForceStandInUpdate()
    {
        this.BlowUpOldStandins();
        for (int i = 0; i < base.m_cards.Count; i++)
        {
            Card card = base.m_cards[i];
            this.DuplicateColliderAndStuffItIn(card);
        }
    }

    public Vector3 GetCardPosition(Card card)
    {
        return this.GetCardPosition(card, -1);
    }

    public Vector3 GetCardPosition(Card card, int overrideCardCount)
    {
        int num = card.GetZonePosition() - 1;
        if (card.IsDoNotSort())
        {
            num = base.GetCards().Count - 1;
        }
        float num2 = 0f;
        float num3 = 0f;
        float num4 = 0f;
        int count = base.m_cards.Count;
        if ((overrideCardCount >= 0) && (overrideCardCount < base.m_cards.Count))
        {
            count = overrideCardCount;
        }
        if (!this.enemyHand)
        {
            count -= TurnStartManager.Get().GetNumCardsToDraw();
        }
        if (this.IsHandScrunched())
        {
            num4 = 1f;
            float num6 = 40f;
            if (!this.enemyHand)
            {
                num6 += count * 2;
            }
            num2 = num6 / ((float) count);
            num3 = -num6 / 2f;
        }
        float f = 0f;
        if (this.enemyHand)
        {
            f = (0f - (num2 * (num + 0.5f))) - num3;
        }
        else
        {
            f += (num2 * num) + num3;
        }
        float num8 = 0f;
        if ((this.enemyHand && (f < 0f)) || (!this.enemyHand && (f > 0f)))
        {
            num8 = (Mathf.Sin((Mathf.Abs(f) * 3.141593f) / 180f) * this.GetCardSpacing()) / 2f;
        }
        float x = this.centerOfHand.x - ((this.GetCardSpacing() / 2f) * ((count - 1) - (num * 2)));
        if (this.m_handEnlarged && this.m_targetingMode)
        {
            if ((count % 2) > 0)
            {
                if (num < ((count + 1) / 2))
                {
                    x -= this.m_heroWidthInHand;
                }
            }
            else if (num < (count / 2))
            {
                x -= this.m_heroWidthInHand / 2f;
            }
            else
            {
                x += this.m_heroWidthInHand / 2f;
            }
        }
        float y = this.centerOfHand.y;
        float z = this.centerOfHand.z;
        if (count > 1)
        {
            if (this.enemyHand)
            {
                z += ((Mathf.Pow((float) Mathf.Abs((int) (num - (count / 2))), 2f) / ((float) (4 * count))) * num4) + num8;
            }
            else
            {
                z = (this.centerOfHand.z - ((Mathf.Pow((float) Mathf.Abs((int) (num - (count / 2))), 2f) / ((float) (4 * count))) * num4)) - num8;
            }
        }
        if (this.enemyHand && SpectatorManager.Get().IsSpectatingOpposingSide())
        {
            z -= 0.2f;
        }
        return new Vector3(x, y, z);
    }

    public Vector3 GetCardRotation(Card card)
    {
        return this.GetCardRotation(card, -1);
    }

    public Vector3 GetCardRotation(Card card, int overrideCardCount)
    {
        int num = card.GetZonePosition() - 1;
        if (card.IsDoNotSort())
        {
            num = base.GetCards().Count - 1;
        }
        float num2 = 0f;
        float num3 = 0f;
        int count = base.m_cards.Count;
        if ((overrideCardCount >= 0) && (overrideCardCount < base.m_cards.Count))
        {
            count = overrideCardCount;
        }
        if (!this.enemyHand)
        {
            count -= TurnStartManager.Get().GetNumCardsToDraw();
        }
        if (this.IsHandScrunched())
        {
            float num5 = 40f;
            if (!this.enemyHand)
            {
                num5 += count * 2;
            }
            num2 = num5 / ((float) count);
            num3 = -num5 / 2f;
        }
        float y = 0f;
        if (this.enemyHand)
        {
            y = (0f - (num2 * (num + 0.5f))) - num3;
        }
        else
        {
            y += (num2 * num) + num3;
        }
        if (this.enemyHand && SpectatorManager.Get().IsSpectatingOpposingSide())
        {
            y += 180f;
        }
        return new Vector3(0f, y, !this.m_flipHandCards ? 354.5f : 534.5f);
    }

    public Vector3 GetCardScale(Card card)
    {
        if (this.enemyHand)
        {
            return new Vector3(0.682f, 0.225f, 0.682f);
        }
        if ((UniversalInputManager.UsePhoneUI != null) && this.m_handEnlarged)
        {
            return this.m_enlargedHandCardScale;
        }
        return new Vector3(0.62f, 0.225f, 0.62f);
    }

    private float GetCardSpacing()
    {
        float defaultCardSpacing = this.GetDefaultCardSpacing();
        int count = base.m_cards.Count;
        if (!this.enemyHand)
        {
            count -= TurnStartManager.Get().GetNumCardsToDraw();
        }
        float num3 = defaultCardSpacing * count;
        float num4 = this.MaxHandWidth();
        if (num3 > num4)
        {
            defaultCardSpacing = num4 / ((float) count);
        }
        return defaultCardSpacing;
    }

    public float GetCardWidth(int nCards)
    {
        if (nCards < 0)
        {
            return 0f;
        }
        if (nCards > MAX_CARDS)
        {
            nCards = MAX_CARDS;
        }
        float num = ((float) Screen.width) / ((float) Screen.height);
        return (((CARD_PIXEL_WIDTHS[nCards] * Screen.width) * this.BASELINE_ASPECT_RATIO) / num);
    }

    public float GetDefaultCardSpacing()
    {
        if ((UniversalInputManager.UsePhoneUI != null) && this.m_handEnlarged)
        {
            return this.m_enlargedHandDefaultCardSpacing;
        }
        return 1.270804f;
    }

    public int GetLastMousedOverCard()
    {
        return this.lastMousedOver;
    }

    private Vector3 GetMouseOverCardPosition(Card card)
    {
        return new Vector3(this.GetCardPosition(card).x, this.centerOfHand.y + 1f, base.transform.FindChild("MouseOverCardHeight").position.z + this.m_SelectCardOffsetZ);
    }

    private CardStandIn GetStandIn(Card card)
    {
        if (this.standIns != null)
        {
            foreach (CardStandIn @in in this.standIns)
            {
                if ((@in != null) && (@in.linkedCard == card))
                {
                    return @in;
                }
            }
        }
        return null;
    }

    public bool HandEnlarged()
    {
        return this.m_handEnlarged;
    }

    public void HandleInput()
    {
        Card item = null;
        if ((RemoteActionHandler.Get() != null) && (RemoteActionHandler.Get().GetFriendlyHoverCard() != null))
        {
            Card friendlyHoverCard = RemoteActionHandler.Get().GetFriendlyHoverCard();
            if (friendlyHoverCard.GetController().IsFriendlySide() && (friendlyHoverCard.GetZone() is ZoneHand))
            {
                item = friendlyHoverCard;
            }
        }
        int slotMousedOver = -1;
        if (item != null)
        {
            slotMousedOver = base.m_cards.IndexOf(item);
        }
        if (UniversalInputManager.Get().IsTouchMode())
        {
            if (!InputManager.Get().LeftMouseButtonDown || (this.m_touchedSlot < 0))
            {
                this.m_touchedSlot = -1;
                if (slotMousedOver < 0)
                {
                    this.UpdateLayout(-1);
                }
                else
                {
                    this.UpdateLayout(slotMousedOver);
                }
            }
            else
            {
                float num2 = UniversalInputManager.Get().GetMousePosition().x - InputManager.Get().LastMouseDownPosition.x;
                float num3 = Mathf.Max((float) 0f, (float) (UniversalInputManager.Get().GetMousePosition().y - InputManager.Get().LastMouseDownPosition.y));
                float cardWidth = this.GetCardWidth(base.m_cards.Count);
                float a = (this.lastMousedOver - this.m_touchedSlot) * cardWidth;
                float num6 = 10f + (num3 * this.m_TouchDragResistanceFactorY);
                if (num2 < a)
                {
                    num2 = Mathf.Min(a, num2 + num6);
                }
                else
                {
                    num2 = Mathf.Max(a, num2 - num6);
                }
                int num7 = this.m_touchedSlot + ((int) Mathf.Round(num2 / cardWidth));
                this.UpdateLayout(num7);
            }
        }
        else
        {
            RaycastHit hit;
            CardStandIn @in = null;
            int num8 = -1;
            if (!UniversalInputManager.Get().InputHitAnyObject(Camera.main, GameLayer.InvisibleHitBox1) || !UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.CardRaycast, out hit))
            {
                if (slotMousedOver < 0)
                {
                    this.UpdateLayout(-1);
                    return;
                }
            }
            else
            {
                @in = SceneUtils.FindComponentInParents<CardStandIn>(hit.transform);
            }
            if (@in == null)
            {
                if (slotMousedOver < 0)
                {
                    this.UpdateLayout(-1);
                    return;
                }
            }
            else
            {
                num8 = @in.slot - 1;
            }
            if (num8 != this.lastMousedOver)
            {
                bool flag = num8 != -1;
                if (flag || (slotMousedOver < 0))
                {
                    this.UpdateLayout(num8);
                }
                else if (!flag && (slotMousedOver >= 0))
                {
                    this.UpdateLayout(slotMousedOver);
                }
            }
        }
    }

    public void HideCards()
    {
        foreach (Card card in base.m_cards)
        {
            card.GetActor().gameObject.SetActive(false);
        }
    }

    public void HideManaGems()
    {
        this.m_manaGemMgr.transform.position = new Vector3(0f, 0f, 0f);
    }

    private bool IsCardNotInEnemyHandAnymore(Card card)
    {
        return ((card.GetEntity().GetZone() != TAG_ZONE.HAND) && this.enemyHand);
    }

    public bool IsDoNotUpdateLayout()
    {
        return this.m_doNotUpdateLayout;
    }

    public bool IsHandScrunched()
    {
        int count = base.m_cards.Count;
        if (this.m_handEnlarged && (count > 3))
        {
            return true;
        }
        float defaultCardSpacing = this.GetDefaultCardSpacing();
        if (!this.enemyHand)
        {
            count -= TurnStartManager.Get().GetNumCardsToDraw();
        }
        float num3 = defaultCardSpacing * count;
        return (num3 > this.MaxHandWidth());
    }

    public void MakeStandInInteractive(Card card)
    {
        if (this.GetStandIn(card) != null)
        {
            this.GetStandIn(card).EnableStandIn();
        }
    }

    private float MaxHandWidth()
    {
        float maxWidth = this.m_maxWidth;
        if (this.m_handEnlarged && this.m_targetingMode)
        {
            maxWidth -= this.m_heroWidthInHand;
        }
        return maxWidth;
    }

    public void OnSpellPowerEntityEnteredPlay()
    {
        foreach (Card card in base.m_cards)
        {
            if (this.CanPlaySpellPowerHint(card))
            {
                Spell actorSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
                if (actorSpell != null)
                {
                    actorSpell.Reactivate();
                }
            }
        }
    }

    public void OnSpellPowerEntityMousedOut()
    {
        foreach (Card card in base.m_cards)
        {
            Spell actorSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
            if ((actorSpell != null) && actorSpell.IsActive())
            {
                actorSpell.ActivateState(SpellStateType.DEATH);
            }
        }
    }

    public void OnSpellPowerEntityMousedOver()
    {
        if (!TargetReticleManager.Get().IsActive())
        {
            foreach (Card card in base.m_cards)
            {
                if (this.CanPlaySpellPowerHint(card))
                {
                    Spell actorSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
                    if (actorSpell != null)
                    {
                        actorSpell.Reactivate();
                    }
                    Spell spell2 = card.GetActorSpell(SpellType.SPELL_POWER_HINT_IDLE);
                    if (spell2 != null)
                    {
                        spell2.ActivateState(SpellStateType.BIRTH);
                    }
                }
            }
        }
    }

    public void SetDoNotUpdateLayout(bool enable)
    {
        this.m_doNotUpdateLayout = enable;
    }

    public void SetFriendlyHeroTargetingMode(bool enable)
    {
        if (!enable && (this.m_hiddenStandIn != null))
        {
            this.m_hiddenStandIn.gameObject.SetActive(true);
        }
        if (this.m_targetingMode != enable)
        {
            this.m_targetingMode = enable;
            this.m_heroHitbox.SetActive(enable);
            if (this.m_handEnlarged)
            {
                if (enable)
                {
                    this.m_hiddenStandIn = this.CurrentStandIn;
                    if (this.m_hiddenStandIn != null)
                    {
                        this.m_hiddenStandIn.gameObject.SetActive(false);
                    }
                    Vector3 enlargedHandPosition = this.m_enlargedHandPosition;
                    enlargedHandPosition.z -= this.m_handHidingDistance;
                    base.gameObject.transform.localPosition = enlargedHandPosition;
                }
                else
                {
                    base.gameObject.transform.localPosition = this.m_enlargedHandPosition;
                }
                this.UpdateCenterAndWidth();
            }
        }
    }

    public void SetHandEnlarged(bool enlarged)
    {
        this.m_handEnlarged = enlarged;
        if (enlarged)
        {
            base.gameObject.transform.localPosition = this.m_enlargedHandPosition;
            base.gameObject.transform.localScale = this.m_enlargedHandScale;
            ManaCrystalMgr.Get().ShowPhoneManaTray();
        }
        else
        {
            base.gameObject.transform.localPosition = this.m_startingPosition;
            base.gameObject.transform.localScale = this.m_startingScale;
            ManaCrystalMgr.Get().HidePhoneManaTray();
        }
        this.UpdateCenterAndWidth();
        this.m_handMoving = true;
        this.UpdateLayout(-1, true);
        this.m_handMoving = false;
    }

    public void ShowCards()
    {
        foreach (Card card in base.m_cards)
        {
            card.GetActor().gameObject.SetActive(true);
        }
    }

    public void ShowManaGems()
    {
        Vector3 position = this.m_manaGemPosition.transform.position;
        position.x += -0.5f * this.m_manaGemMgr.GetWidth();
        this.m_manaGemMgr.gameObject.transform.position = position;
        this.m_manaGemMgr.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }

    public bool TouchReceived()
    {
        RaycastHit hit;
        if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out hit))
        {
            this.m_touchedSlot = -1;
        }
        CardStandIn @in = SceneUtils.FindComponentInParents<CardStandIn>(hit.transform);
        if (@in != null)
        {
            this.m_touchedSlot = @in.slot - 1;
            return true;
        }
        this.m_touchedSlot = -1;
        return false;
    }

    private void UpdateCenterAndWidth()
    {
        this.centerOfHand = base.GetComponent<Collider>().bounds.center;
        this.m_maxWidth = base.GetComponent<Collider>().bounds.size.x;
    }

    public override void UpdateLayout()
    {
        if (!GameState.Get().IsMulliganManagerActive() && !this.enemyHand)
        {
            this.BlowUpOldStandins();
            for (int i = 0; i < base.m_cards.Count; i++)
            {
                Card card = base.m_cards[i];
                this.DuplicateColliderAndStuffItIn(card);
            }
        }
        this.UpdateLayout(-1, true, -1);
    }

    public void UpdateLayout(int slotMousedOver)
    {
        this.UpdateLayout(slotMousedOver, false, -1);
    }

    public void UpdateLayout(int slotMousedOver, bool forced)
    {
        this.UpdateLayout(slotMousedOver, forced, -1);
    }

    public void UpdateLayout(int slotMousedOver, bool forced, int overrideCardCount)
    {
        base.m_updatingLayout = true;
        if (base.IsBlockingLayout())
        {
            base.UpdateLayoutFinished();
        }
        else
        {
            for (int i = 0; i < base.m_cards.Count; i++)
            {
                Card card = base.m_cards[i];
                if ((!card.IsDoNotSort() && (card.GetTransitionStyle() != ZoneTransitionStyle.VERY_SLOW)) && (!this.IsCardNotInEnemyHandAnymore(card) && !card.HasBeenGrabbedByEnemyActionHandler()))
                {
                    Spell bestSummonSpell = card.GetBestSummonSpell();
                    if ((bestSummonSpell == null) || !bestSummonSpell.IsActive())
                    {
                        card.ShowCard();
                    }
                }
            }
            if (this.m_doNotUpdateLayout)
            {
                base.UpdateLayoutFinished();
            }
            else if (base.m_cards.Count == 0)
            {
                base.UpdateLayoutFinished();
            }
            else if ((slotMousedOver >= base.m_cards.Count) || (slotMousedOver < -1))
            {
                base.UpdateLayoutFinished();
            }
            else if (!forced && (slotMousedOver == this.lastMousedOver))
            {
                base.m_updatingLayout = false;
            }
            else
            {
                base.m_cards.Sort(new Comparison<Card>(Zone.CardSortComparison));
                this.UpdateLayoutImpl(slotMousedOver, overrideCardCount);
            }
        }
    }

    private void UpdateLayoutImpl(int slotMousedOver, int overrideCardCount)
    {
        int num = 0;
        if ((((this.lastMousedOver != slotMousedOver) && (this.lastMousedOver != -1)) && ((this.lastMousedOver < base.m_cards.Count) && (base.m_cards[this.lastMousedOver] != null))) && this.CanAnimateCard(base.m_cards[this.lastMousedOver]))
        {
            Card card = base.m_cards[this.lastMousedOver];
            iTween.Stop(card.gameObject);
            if (!this.enemyHand)
            {
                Vector3 mouseOverCardPosition = this.GetMouseOverCardPosition(card);
                Vector3 cardPosition = this.GetCardPosition(card, overrideCardCount);
                card.transform.position = new Vector3(mouseOverCardPosition.x, this.centerOfHand.y, cardPosition.z + 0.5f);
                card.transform.localScale = this.GetCardScale(card);
                card.transform.localEulerAngles = this.GetCardRotation(card);
            }
            card.NotifyMousedOut();
            GameLayer cardRaycast = GameLayer.Default;
            if ((base.m_Side == Player.Side.OPPOSING) && SpectatorManager.Get().IsSpectatingOpposingSide())
            {
                cardRaycast = GameLayer.CardRaycast;
            }
            SceneUtils.SetLayer(card.gameObject, cardRaycast);
        }
        float delaySec = 0f;
        for (int i = 0; i < base.m_cards.Count; i++)
        {
            Vector3 vector3;
            Card card2 = base.m_cards[i];
            if (!this.CanAnimateCard(card2))
            {
                continue;
            }
            num++;
            float z = !this.m_flipHandCards ? 354.5f : 534.5f;
            card2.transform.rotation = Quaternion.Euler(new Vector3(card2.transform.localEulerAngles.x, card2.transform.localEulerAngles.y, z));
            float num5 = 0.5f;
            if (this.m_handMoving)
            {
                num5 = 0.25f;
            }
            if (this.enemyHand)
            {
                num5 = 1.5f;
            }
            float num6 = 0.25f;
            iTween.EaseType easeOutExpo = iTween.EaseType.easeOutExpo;
            float transitionDelay = card2.GetTransitionDelay();
            card2.SetTransitionDelay(0f);
            ZoneTransitionStyle transitionStyle = card2.GetTransitionStyle();
            card2.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
            switch (transitionStyle)
            {
                case ZoneTransitionStyle.SLOW:
                    easeOutExpo = iTween.EaseType.easeInExpo;
                    num6 = num5;
                    break;

                case ZoneTransitionStyle.VERY_SLOW:
                    easeOutExpo = iTween.EaseType.easeInOutCubic;
                    num6 = 1f;
                    num5 = 1f;
                    break;

                case ZoneTransitionStyle.NORMAL:
                    goto Label_0265;
            }
            card2.GetActor().TurnOnCollider();
        Label_0265:
            vector3 = this.GetCardPosition(card2, overrideCardCount);
            Vector3 cardRotation = this.GetCardRotation(card2, overrideCardCount);
            Vector3 cardScale = this.GetCardScale(card2);
            if (i == slotMousedOver)
            {
                easeOutExpo = iTween.EaseType.easeOutExpo;
                if (this.enemyHand)
                {
                    num6 = 0.15f;
                    float num8 = 0.3f;
                    vector3 = new Vector3(vector3.x, vector3.y, vector3.z - num8);
                }
                else
                {
                    float num9 = 0.5f * i;
                    num9 -= (0.5f * base.m_cards.Count) / 2f;
                    float selectCardScale = (float) this.m_SelectCardScale;
                    float num11 = (float) this.m_SelectCardScale;
                    cardRotation = new Vector3(0f, 0f, 0f);
                    cardScale = new Vector3(selectCardScale, cardScale.y, num11);
                    card2.transform.localScale = cardScale;
                    num5 = 4f;
                    float num12 = 0.1f;
                    vector3 = this.GetMouseOverCardPosition(card2);
                    float x = vector3.x;
                    if (this.m_handEnlarged)
                    {
                        vector3.x = Mathf.Max(vector3.x, this.m_enlargedHandCardMinX);
                        vector3.x = Mathf.Min(vector3.x, this.m_enlargedHandCardMaxX);
                    }
                    card2.transform.position = new Vector3((x == vector3.x) ? card2.transform.position.x : vector3.x, vector3.y, vector3.z - num12);
                    card2.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    iTween.Stop(card2.gameObject);
                    easeOutExpo = iTween.EaseType.easeOutExpo;
                    if (CardTypeBanner.Get() != null)
                    {
                        CardTypeBanner.Get().Show(card2.GetActor());
                    }
                    InputManager.Get().SetMousedOverCard(card2);
                    bool showOnRight = card2.GetActor().GetMeshRenderer().bounds.center.x < base.GetComponent<BoxCollider>().bounds.center.x;
                    float? overrideScale = null;
                    Vector3? overrideOffset = null;
                    KeywordHelpPanelManager.Get().UpdateKeywordHelp(card2, card2.GetActor(), showOnRight, overrideScale, overrideOffset);
                    SceneUtils.SetLayer(card2.gameObject, GameLayer.Tooltip);
                }
            }
            else if (this.GetStandIn(card2) != null)
            {
                CardStandIn standIn = this.GetStandIn(card2);
                iTween.Stop(standIn.gameObject);
                standIn.transform.position = vector3;
                if (!card2.CardStandInIsInteractive())
                {
                    standIn.DisableStandIn();
                }
                else
                {
                    standIn.EnableStandIn();
                }
            }
            card2.EnableTransitioningZones(true);
            string tweenName = ZoneMgr.Get().GetTweenName<ZoneHand>();
            object[] args = new object[] { "scale", cardScale, "delay", transitionDelay, "time", num6, "easeType", easeOutExpo, "name", tweenName };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ScaleTo(card2.gameObject, hashtable);
            object[] objArray2 = new object[] { "rotation", cardRotation, "delay", transitionDelay, "time", num6, "easeType", easeOutExpo, "name", tweenName };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.RotateTo(card2.gameObject, hashtable2);
            object[] objArray3 = new object[] { "position", vector3, "delay", transitionDelay, "time", num5, "easeType", easeOutExpo, "name", tweenName };
            Hashtable hashtable3 = iTween.Hash(objArray3);
            iTween.MoveTo(card2.gameObject, hashtable3);
            float[] values = new float[] { delaySec, transitionDelay + num5, transitionDelay + num6 };
            delaySec = Mathf.Max(values);
        }
        this.lastMousedOver = slotMousedOver;
        if (num > 0)
        {
            base.StartFinishLayoutTimer(delaySec);
        }
        else
        {
            base.UpdateLayoutFinished();
        }
    }

    public CardStandIn CurrentStandIn
    {
        get
        {
            if ((this.lastMousedOver >= 0) && (this.lastMousedOver < base.m_cards.Count))
            {
                return this.GetStandIn(base.m_cards[this.lastMousedOver]);
            }
            return null;
        }
    }
}

