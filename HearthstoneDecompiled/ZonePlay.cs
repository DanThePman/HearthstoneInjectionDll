using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZonePlay : Zone
{
    private const float DEFAULT_TRANSITION_TIME = 1f;
    public int m_MaxSlots = 7;
    private int m_slotMousedOver = -1;
    private float m_slotWidth;
    private float m_transitionTime = 1f;
    private static Vector3 PHONE_ACTOR_SCALE = new Vector3(1.06f, 1.06f, 1.06f);
    private float[] PHONE_WIDTH_MODIFIERS = new float[] { 0.25f, 0.25f, 0.25f, 0.25f, 0.22f, 0.19f, 0.15f, 0.1f };

    private void Awake()
    {
        this.m_slotWidth = base.GetComponent<Collider>().bounds.size.x / ((float) this.m_MaxSlots);
    }

    public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType)
    {
        if (!base.CanAcceptTags(controllerId, zoneTag, cardType))
        {
            return false;
        }
        return (cardType == TAG_CARDTYPE.MINION);
    }

    protected bool CanAnimateCard(Card card)
    {
        if (card.IsDoNotSort())
        {
            return false;
        }
        return true;
    }

    public Vector3 GetCardPosition(Card card)
    {
        <GetCardPosition>c__AnonStorey301 storey = new <GetCardPosition>c__AnonStorey301 {
            card = card
        };
        int index = base.m_cards.FindIndex(new Predicate<Card>(storey.<>m__113));
        return this.GetCardPosition(index);
    }

    public Vector3 GetCardPosition(int index)
    {
        if ((index < 0) || (index >= base.m_cards.Count))
        {
            return base.transform.position;
        }
        int count = base.m_cards.Count;
        if (this.m_slotMousedOver >= 0)
        {
            count++;
        }
        Vector3 center = base.GetComponent<Collider>().bounds.center;
        float num2 = 0.5f * this.GetSlotWidth();
        float num3 = count * num2;
        float num4 = (center.x - num3) + num2;
        int num5 = ((this.m_slotMousedOver < 0) || (index < this.m_slotMousedOver)) ? 0 : 1;
        for (int i = 0; i < index; i++)
        {
            Card card = base.m_cards[i];
            if (this.CanAnimateCard(card))
            {
                num5++;
            }
        }
        return new Vector3(num4 + (num5 * this.GetSlotWidth()), center.y, center.z);
    }

    public int GetSlotMousedOver()
    {
        return this.m_slotMousedOver;
    }

    public float GetSlotWidth()
    {
        this.m_slotWidth = base.GetComponent<Collider>().bounds.size.x / ((float) this.m_MaxSlots);
        int count = base.m_cards.Count;
        if (this.m_slotMousedOver >= 0)
        {
            count++;
        }
        count = Mathf.Clamp(count, 0, this.m_MaxSlots);
        float num2 = 1f;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            num2 += this.PHONE_WIDTH_MODIFIERS[count];
        }
        return (this.m_slotWidth * num2);
    }

    public float GetTransitionTime()
    {
        return this.m_transitionTime;
    }

    public void ResetTransitionTime()
    {
        this.m_transitionTime = 1f;
    }

    public void SetTransitionTime(float transitionTime)
    {
        this.m_transitionTime = transitionTime;
    }

    public void SortWithSpotForHeldCard(int slot)
    {
        this.m_slotMousedOver = slot;
        this.UpdateLayout();
    }

    public override void UpdateLayout()
    {
        base.m_updatingLayout = true;
        if (base.IsBlockingLayout())
        {
            base.UpdateLayoutFinished();
        }
        else
        {
            if ((InputManager.Get() != null) && (InputManager.Get().GetHeldCard() == null))
            {
                this.m_slotMousedOver = -1;
            }
            int num = 0;
            base.m_cards.Sort(new Comparison<Card>(Zone.CardSortComparison));
            float a = 0f;
            for (int i = 0; i < base.m_cards.Count; i++)
            {
                Card card = base.m_cards[i];
                if (this.CanAnimateCard(card))
                {
                    string tweenName = ZoneMgr.Get().GetTweenName<ZonePlay>();
                    if (base.m_Side == Player.Side.OPPOSING)
                    {
                        iTween.StopOthersByName(card.gameObject, tweenName, false);
                    }
                    Vector3 cardPosition = this.GetCardPosition(i);
                    float transitionDelay = card.GetTransitionDelay();
                    card.SetTransitionDelay(0f);
                    ZoneTransitionStyle transitionStyle = card.GetTransitionStyle();
                    card.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
                    if (transitionStyle == ZoneTransitionStyle.INSTANT)
                    {
                        card.EnableTransitioningZones(false);
                        card.transform.position = cardPosition;
                        card.transform.rotation = base.transform.rotation;
                        card.transform.localScale = (UniversalInputManager.UsePhoneUI == null) ? base.transform.localScale : PHONE_ACTOR_SCALE;
                    }
                    else
                    {
                        card.EnableTransitioningZones(true);
                        num++;
                        object[] args = new object[] { "scale", (UniversalInputManager.UsePhoneUI == null) ? base.transform.localScale : PHONE_ACTOR_SCALE, "delay", transitionDelay, "time", this.m_transitionTime, "name", tweenName };
                        Hashtable hashtable = iTween.Hash(args);
                        iTween.ScaleTo(card.gameObject, hashtable);
                        object[] objArray2 = new object[] { "rotation", base.transform.eulerAngles, "delay", transitionDelay, "time", this.m_transitionTime, "name", tweenName };
                        Hashtable hashtable2 = iTween.Hash(objArray2);
                        iTween.RotateTo(card.gameObject, hashtable2);
                        object[] objArray3 = new object[] { "position", cardPosition, "delay", transitionDelay, "time", this.m_transitionTime, "name", tweenName };
                        Hashtable hashtable3 = iTween.Hash(objArray3);
                        iTween.MoveTo(card.gameObject, hashtable3);
                        a = Mathf.Max(a, transitionDelay + this.m_transitionTime);
                    }
                }
            }
            if (num > 0)
            {
                base.StartFinishLayoutTimer(a);
            }
            else
            {
                base.UpdateLayoutFinished();
            }
        }
    }

    [CompilerGenerated]
    private sealed class <GetCardPosition>c__AnonStorey301
    {
        internal Card card;

        internal bool <>m__113(Card currCard)
        {
            return (currCard == this.card);
        }
    }
}

