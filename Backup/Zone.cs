using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Zone : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<Card> <>f__am$cache9;
    protected List<Card> m_cards = new List<Card>();
    protected List<UpdateLayoutCompleteListener> m_completeListeners = new List<UpdateLayoutCompleteListener>();
    protected Player m_controller;
    protected int m_inputBlockerCount;
    protected int m_layoutBlockerCount;
    protected bool m_layoutDirty = true;
    public TAG_ZONE m_ServerTag;
    public Player.Side m_Side;
    protected bool m_updatingLayout;
    public const float TRANSITION_SEC = 1f;

    public virtual bool AddCard(Card card)
    {
        this.m_cards.Add(card);
        this.DirtyLayout();
        return true;
    }

    public void AddInputBlocker()
    {
        this.AddInputBlocker(1);
    }

    public void AddInputBlocker(int count)
    {
        int inputBlockerCount = this.m_inputBlockerCount;
        this.m_inputBlockerCount += count;
        if ((inputBlockerCount != this.m_inputBlockerCount) && ((inputBlockerCount * this.m_inputBlockerCount) == 0))
        {
            this.UpdateInput();
        }
    }

    public void AddLayoutBlocker()
    {
        this.m_layoutBlockerCount++;
    }

    public bool AddUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback)
    {
        return this.AddUpdateLayoutCompleteCallback(callback, null);
    }

    public bool AddUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback, object userData)
    {
        UpdateLayoutCompleteListener item = new UpdateLayoutCompleteListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_completeListeners.Contains(item))
        {
            return false;
        }
        this.m_completeListeners.Add(item);
        return true;
    }

    public void BlockInput(bool block)
    {
        int count = !block ? -1 : 1;
        this.AddInputBlocker(count);
    }

    public virtual bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType)
    {
        if (this.m_ServerTag != zoneTag)
        {
            return false;
        }
        if ((this.m_controller != null) && (this.m_controller.GetPlayerId() != controllerId))
        {
            return false;
        }
        if (cardType == TAG_CARDTYPE.ENCHANTMENT)
        {
            return false;
        }
        return true;
    }

    public static int CardSortComparison(Card card1, Card card2)
    {
        int zonePosition = card1.GetZonePosition();
        int num2 = card2.GetZonePosition();
        return (zonePosition - num2);
    }

    public bool ContainsCard(Card card)
    {
        return (this.FindCardPos(card) > 0);
    }

    public void DirtyLayout()
    {
        this.m_layoutDirty = true;
    }

    public int FindCardPos(Card card)
    {
        <FindCardPos>c__AnonStorey2FB storeyfb = new <FindCardPos>c__AnonStorey2FB {
            card = card
        };
        return (1 + this.m_cards.FindIndex(new Predicate<Card>(storeyfb.<>m__10A)));
    }

    protected void FireUpdateLayoutCompleteCallbacks()
    {
        if (this.m_completeListeners.Count != 0)
        {
            UpdateLayoutCompleteListener[] listenerArray = this.m_completeListeners.ToArray();
            this.m_completeListeners.Clear();
            for (int i = 0; i < listenerArray.Length; i++)
            {
                listenerArray[i].Fire(this);
            }
        }
    }

    public Card GetCardAtIndex(int index)
    {
        if (index < 0)
        {
            return null;
        }
        if (index >= this.m_cards.Count)
        {
            return null;
        }
        return this.m_cards[index];
    }

    public Card GetCardAtPos(int pos)
    {
        return this.GetCardAtIndex(pos - 1);
    }

    public int GetCardCount()
    {
        return this.m_cards.Count;
    }

    public List<Card> GetCards()
    {
        return this.m_cards;
    }

    public Player GetController()
    {
        return this.m_controller;
    }

    public int GetControllerId()
    {
        return ((this.m_controller != null) ? this.m_controller.GetPlayerId() : 0);
    }

    public Card GetFirstCard()
    {
        return ((this.m_cards.Count <= 0) ? null : this.m_cards[0]);
    }

    public int GetInputBlockerCount()
    {
        return this.m_inputBlockerCount;
    }

    public Card GetLastCard()
    {
        return ((this.m_cards.Count <= 0) ? null : this.m_cards[this.m_cards.Count - 1]);
    }

    public int GetLastPos()
    {
        return (this.m_cards.Count + 1);
    }

    public int GetLayoutBlockerCount()
    {
        return this.m_layoutBlockerCount;
    }

    public virtual bool InsertCard(int index, Card card)
    {
        this.m_cards.Insert(index, card);
        this.DirtyLayout();
        return true;
    }

    public bool IsBlockingLayout()
    {
        return (this.m_layoutBlockerCount > 0);
    }

    public bool IsInputEnabled()
    {
        return (this.m_inputBlockerCount <= 0);
    }

    public bool IsLayoutDirty()
    {
        return this.m_layoutDirty;
    }

    public bool IsOnlyCard(Card card)
    {
        if (this.m_cards.Count != 1)
        {
            return false;
        }
        return (this.m_cards[0] == card);
    }

    public bool IsUpdatingLayout()
    {
        return this.m_updatingLayout;
    }

    public virtual int RemoveCard(Card card)
    {
        for (int i = 0; i < this.m_cards.Count; i++)
        {
            Card card2 = this.m_cards[i];
            if (card2 == card)
            {
                this.m_cards.RemoveAt(i);
                this.DirtyLayout();
                return i;
            }
        }
        Debug.LogWarning(string.Format("{0}.RemoveCard() - FAILED: {1} tried to remove {2}", this, this.m_controller, card));
        return -1;
    }

    public void RemoveInputBlocker()
    {
        this.AddInputBlocker(-1);
    }

    public void RemoveLayoutBlocker()
    {
        this.m_layoutBlockerCount--;
    }

    public bool RemoveUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback)
    {
        return this.RemoveUpdateLayoutCompleteCallback(callback, null);
    }

    public bool RemoveUpdateLayoutCompleteCallback(UpdateLayoutCompleteCallback callback, object userData)
    {
        UpdateLayoutCompleteListener item = new UpdateLayoutCompleteListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_completeListeners.Remove(item);
    }

    public void SetController(Player controller)
    {
        this.m_controller = controller;
    }

    protected void StartFinishLayoutTimer(float delaySec)
    {
        if (delaySec <= Mathf.Epsilon)
        {
            this.UpdateLayoutFinished();
        }
        else
        {
            if (<>f__am$cache9 == null)
            {
                <>f__am$cache9 = card => card.IsTransitioningZones();
            }
            if (this.m_cards.Find(<>f__am$cache9) == null)
            {
                this.UpdateLayoutFinished();
            }
            else
            {
                object[] args = new object[] { "time", delaySec, "oncomplete", "UpdateLayoutFinished", "oncompletetarget", base.gameObject };
                Hashtable hashtable = iTween.Hash(args);
                iTween.Timer(base.gameObject, hashtable);
            }
        }
    }

    public override string ToString()
    {
        return string.Format("{1} {0}", this.m_ServerTag, this.m_Side);
    }

    protected void UpdateInput()
    {
        bool enabled = this.IsInputEnabled();
        foreach (Card card in this.m_cards)
        {
            Actor actor = card.GetActor();
            if (actor != null)
            {
                actor.ToggleForceIdle(!enabled);
                actor.ToggleCollider(enabled);
                card.UpdateActorState();
            }
        }
        Card mousedOverCard = InputManager.Get().GetMousedOverCard();
        if (enabled && this.m_cards.Contains(mousedOverCard))
        {
            mousedOverCard.UpdateProposedManaUsage();
        }
    }

    public virtual void UpdateLayout()
    {
        if (this.m_cards.Count == 0)
        {
            this.UpdateLayoutFinished();
        }
        else if (GameState.Get().IsMulliganManagerActive())
        {
            this.UpdateLayoutFinished();
        }
        else
        {
            this.m_updatingLayout = true;
            if (this.IsBlockingLayout())
            {
                this.UpdateLayoutFinished();
            }
            else
            {
                for (int i = 0; i < this.m_cards.Count; i++)
                {
                    Card card = this.m_cards[i];
                    if (!card.IsDoNotSort())
                    {
                        card.ShowCard();
                        card.EnableTransitioningZones(true);
                        iTween.MoveTo(card.gameObject, base.transform.position, 1f);
                        iTween.RotateTo(card.gameObject, base.transform.localEulerAngles, 1f);
                        iTween.ScaleTo(card.gameObject, base.transform.localScale, 1f);
                    }
                }
                this.StartFinishLayoutTimer(1f);
            }
        }
    }

    protected void UpdateLayoutFinished()
    {
        for (int i = 0; i < this.m_cards.Count; i++)
        {
            this.m_cards[i].EnableTransitioningZones(false);
        }
        this.m_updatingLayout = false;
        this.m_layoutDirty = false;
        this.FireUpdateLayoutCompleteCallbacks();
    }

    [CompilerGenerated]
    private sealed class <FindCardPos>c__AnonStorey2FB
    {
        internal Card card;

        internal bool <>m__10A(Card currCard)
        {
            return (currCard == this.card);
        }
    }

    public delegate void UpdateLayoutCompleteCallback(Zone zone, object userData);

    protected class UpdateLayoutCompleteListener : EventListener<Zone.UpdateLayoutCompleteCallback>
    {
        public void Fire(Zone zone)
        {
            base.m_callback(zone, base.m_userData);
        }
    }
}

