using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZoneWeapon : Zone
{
    private const float DESTROYED_WEAPON_WAIT_SEC = 1.75f;
    private const float FINAL_TRANSITION_SEC = 0.1f;
    private const float INTERMEDIATE_TRANSITION_SEC = 0.9f;
    private const float INTERMEDIATE_Y_OFFSET = 1.5f;
    private List<Card> m_destroyedWeapons = new List<Card>();

    public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType)
    {
        if (!base.CanAcceptTags(controllerId, zoneTag, cardType))
        {
            return false;
        }
        if (cardType != TAG_CARDTYPE.WEAPON)
        {
            return false;
        }
        return true;
    }

    public override int RemoveCard(Card card)
    {
        int num = base.RemoveCard(card);
        if ((num >= 0) && !this.m_destroyedWeapons.Contains(card))
        {
            this.m_destroyedWeapons.Add(card);
        }
        return num;
    }

    public override string ToString()
    {
        return string.Format("{0} (Weapon)", base.ToString());
    }

    public override void UpdateLayout()
    {
        if (GameState.Get().IsMulliganManagerActive())
        {
            base.UpdateLayoutFinished();
        }
        else
        {
            base.m_updatingLayout = true;
            if (base.IsBlockingLayout())
            {
                base.UpdateLayoutFinished();
            }
            else if (base.m_cards.Count == 0)
            {
                this.m_destroyedWeapons.Clear();
                base.UpdateLayoutFinished();
            }
            else
            {
                base.StartCoroutine(this.UpdateLayoutImpl());
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateLayoutImpl()
    {
        return new <UpdateLayoutImpl>c__IteratorCF { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateLayoutImpl>c__IteratorCF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ZoneWeapon <>f__this;
        internal Card <equippedWeapon>__0;
        internal Vector3 <intermediatePosition>__2;
        internal Hashtable <moveArgs>__3;
        internal Hashtable <rotateArgs>__4;
        internal Hashtable <scaleArgs>__5;
        internal string <tweenName>__1;

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
                    this.<equippedWeapon>__0 = this.<>f__this.m_cards[0];
                    break;

                case 1:
                    break;

                case 2:
                    if (this.<>f__this.m_destroyedWeapons.Count <= 0)
                    {
                        goto Label_026F;
                    }
                    this.$current = new WaitForSeconds(1.75f);
                    this.$PC = 3;
                    goto Label_0316;

                case 3:
                    goto Label_026F;

                default:
                    goto Label_0314;
            }
            if (this.<equippedWeapon>__0.IsDoNotSort())
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                this.<equippedWeapon>__0.ShowCard();
                this.<equippedWeapon>__0.EnableTransitioningZones(true);
                this.<tweenName>__1 = ZoneMgr.Get().GetTweenName<ZoneWeapon>();
                if (this.<>f__this.m_Side == Player.Side.OPPOSING)
                {
                    iTween.StopOthersByName(this.<equippedWeapon>__0.gameObject, this.<tweenName>__1, false);
                }
                this.<intermediatePosition>__2 = this.<>f__this.transform.position;
                this.<intermediatePosition>__2.y += 1.5f;
                object[] objArray1 = new object[] { "name", this.<tweenName>__1, "position", this.<intermediatePosition>__2, "time", 0.9f };
                this.<moveArgs>__3 = iTween.Hash(objArray1);
                iTween.MoveTo(this.<equippedWeapon>__0.gameObject, this.<moveArgs>__3);
                object[] objArray2 = new object[] { "name", this.<tweenName>__1, "rotation", this.<>f__this.transform.localEulerAngles, "time", 0.9f };
                this.<rotateArgs>__4 = iTween.Hash(objArray2);
                iTween.RotateTo(this.<equippedWeapon>__0.gameObject, this.<rotateArgs>__4);
                object[] objArray3 = new object[] { "name", this.<tweenName>__1, "scale", this.<>f__this.transform.localScale, "time", 0.9f };
                this.<scaleArgs>__5 = iTween.Hash(objArray3);
                iTween.ScaleTo(this.<equippedWeapon>__0.gameObject, this.<scaleArgs>__5);
                this.$current = new WaitForSeconds(0.9f);
                this.$PC = 2;
            }
            goto Label_0316;
        Label_026F:
            this.<>f__this.m_destroyedWeapons.Clear();
            object[] args = new object[] { "position", this.<>f__this.transform.position, "time", 0.1f, "easetype", iTween.EaseType.easeOutCubic, "name", this.<tweenName>__1 };
            this.<moveArgs>__3 = iTween.Hash(args);
            iTween.MoveTo(this.<equippedWeapon>__0.gameObject, this.<moveArgs>__3);
            this.<>f__this.StartFinishLayoutTimer(0.1f);
            this.$PC = -1;
        Label_0314:
            return false;
        Label_0316:
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
}

