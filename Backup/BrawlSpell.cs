using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BrawlSpell : Spell
{
    public float m_HoldTime = 0.1f;
    public float m_JumpInDuration = 1.5f;
    public iTween.EaseType m_JumpInEaseType = iTween.EaseType.linear;
    public float m_JumpInSoundDelay;
    public AudioSource m_JumpInSoundPrefab;
    public float m_JumpOutDuration = 1.5f;
    public iTween.EaseType m_JumpOutEaseType = iTween.EaseType.easeOutBounce;
    public float m_JumpOutSoundDelay;
    public AudioSource m_JumpOutSoundPrefab;
    private int m_jumpsPending;
    public float m_LandSoundDelay;
    public AudioSource m_LandSoundPrefab;
    public List<GameObject> m_LeftJumpOutBones;
    public float m_MaxJumpHeight = 2.5f;
    public float m_MaxJumpInDelay = 0.2f;
    public float m_MaxJumpOutDelay = 0.2f;
    public float m_MinJumpHeight = 1.5f;
    public float m_MinJumpInDelay = 0.1f;
    public float m_MinJumpOutDelay = 0.1f;
    public List<GameObject> m_RightJumpOutBones;
    private Card m_survivorCard;
    public float m_SurvivorHoldDuration = 0.5f;

    private Card FindSurvivor()
    {
        foreach (ZonePlay play in ZoneMgr.Get().FindZonesOfType<ZonePlay>())
        {
            List<Card> cards = play.GetCards();
            <FindSurvivor>c__AnonStorey329 storey = new <FindSurvivor>c__AnonStorey329();
            using (List<Card>.Enumerator enumerator2 = cards.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    storey.playCard = enumerator2.Current;
                    if (base.m_targets.Find(new Predicate<GameObject>(storey.<>m__15D)) == null)
                    {
                        return storey.playCard;
                    }
                }
            }
        }
        return null;
    }

    private GameObject GetFreeBone(List<GameObject> boneList, List<int> usedBoneIndexes)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < boneList.Count; i++)
        {
            if (!usedBoneIndexes.Contains(i))
            {
                list.Add(i);
            }
        }
        if (list.Count == 0)
        {
            return null;
        }
        int num2 = UnityEngine.Random.Range(0, list.Count - 1);
        int item = list[num2];
        usedBoneIndexes.Add(item);
        return boneList[item];
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        Network.PowerHistory power = task.GetPower();
        if (power.Type != Network.PowerType.TAG_CHANGE)
        {
            return null;
        }
        Network.HistTagChange change = power as Network.HistTagChange;
        if (change.Tag != 360)
        {
            return null;
        }
        if (change.Value != 1)
        {
            return null;
        }
        Entity entity = GameState.Get().GetEntity(change.Entity);
        if (entity == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, change.Entity));
            return null;
        }
        return entity.GetCard();
    }

    [DebuggerHidden]
    private IEnumerator Hold()
    {
        return new <Hold>c__Iterator1C9 { <>f__this = this };
    }

    private bool IsSurvivorAlone()
    {
        Zone zone = this.m_survivorCard.GetZone();
        foreach (GameObject obj2 in base.m_targets)
        {
            if (obj2.GetComponent<Card>().GetZone() == zone)
            {
                return false;
            }
        }
        return true;
    }

    [DebuggerHidden]
    private IEnumerator JumpIn(Card card, float delaySec)
    {
        return new <JumpIn>c__Iterator1C8 { delaySec = delaySec, card = card, <$>delaySec = delaySec, <$>card = card, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator JumpOut(Card card, float delaySec, Vector3 destPos)
    {
        return new <JumpOut>c__Iterator1CA { delaySec = delaySec, card = card, destPos = destPos, <$>delaySec = delaySec, <$>card = card, <$>destPos = destPos, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator LoadAndPlaySound(AudioSource prefab, float delaySec)
    {
        return new <LoadAndPlaySound>c__Iterator1CC { prefab = prefab, delaySec = delaySec, <$>prefab = prefab, <$>delaySec = delaySec, <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        if (base.m_targets.Count > 0)
        {
            this.m_survivorCard = this.FindSurvivor();
            this.StartJumpIns();
        }
        else
        {
            this.OnSpellFinished();
            this.OnStateFinished();
        }
    }

    private void OnJumpInComplete(Card targetCard)
    {
        targetCard.HideCard();
        this.m_jumpsPending--;
        if (this.m_jumpsPending <= 0)
        {
            base.StartCoroutine(this.Hold());
        }
    }

    private void OnJumpOutComplete(Card targetCard)
    {
        this.m_jumpsPending--;
        if (this.m_jumpsPending <= 0)
        {
            base.ActivateState(SpellStateType.DEATH);
            base.StartCoroutine(this.SurvivorHold());
        }
    }

    private void StartJumpIn(Card card, ref float startSec)
    {
        float num = UnityEngine.Random.Range(this.m_MinJumpInDelay, this.m_MaxJumpInDelay);
        base.StartCoroutine(this.JumpIn(card, startSec + num));
        startSec += num;
    }

    private void StartJumpIns()
    {
        this.m_jumpsPending = base.m_targets.Count + 1;
        List<Card> list = new List<Card>(this.m_jumpsPending);
        foreach (GameObject obj2 in base.m_targets)
        {
            Card component = obj2.GetComponent<Card>();
            list.Add(component);
        }
        list.Add(this.m_survivorCard);
        float startSec = 0f;
        while (list.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            Card card = list[index];
            list.RemoveAt(index);
            this.StartJumpIn(card, ref startSec);
        }
    }

    private void StartJumpOuts()
    {
        this.m_jumpsPending = base.m_targets.Count;
        List<int> usedBoneIndexes = new List<int>();
        List<int> list2 = new List<int>();
        float num = 0f;
        bool flag = true;
        for (int i = 0; i < base.m_targets.Count; i++)
        {
            Card component = base.m_targets[i].GetComponent<Card>();
            if (component != this.m_survivorCard)
            {
                GameObject freeBone = null;
                if (flag)
                {
                    freeBone = this.GetFreeBone(this.m_LeftJumpOutBones, usedBoneIndexes);
                    if (freeBone == null)
                    {
                        usedBoneIndexes.Clear();
                        freeBone = this.GetFreeBone(this.m_LeftJumpOutBones, usedBoneIndexes);
                    }
                }
                else
                {
                    freeBone = this.GetFreeBone(this.m_RightJumpOutBones, list2);
                    if (freeBone == null)
                    {
                        list2.Clear();
                        freeBone = this.GetFreeBone(this.m_RightJumpOutBones, list2);
                    }
                }
                float num3 = UnityEngine.Random.Range(this.m_MinJumpOutDelay, this.m_MaxJumpOutDelay);
                base.StartCoroutine(this.JumpOut(component, num + num3, freeBone.transform.position));
                num += num3;
                flag = !flag;
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator SurvivorHold()
    {
        return new <SurvivorHold>c__Iterator1CB { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <FindSurvivor>c__AnonStorey329
    {
        internal Card playCard;

        internal bool <>m__15D(GameObject testObject)
        {
            return (this.playCard == testObject.GetComponent<Card>());
        }
    }

    [CompilerGenerated]
    private sealed class <Hold>c__Iterator1C9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BrawlSpell <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_HoldTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.StartJumpOuts();
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
    private sealed class <JumpIn>c__Iterator1C8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal float <$>delaySec;
        internal BrawlSpell <>f__this;
        internal Hashtable <argTable>__2;
        internal float <jumpHeight>__1;
        internal Vector3[] <path>__0;
        internal Card card;
        internal float delaySec;

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
                    this.$current = new WaitForSeconds(this.delaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    this.<path>__0 = new Vector3[3];
                    this.<path>__0[0] = this.card.transform.position;
                    this.<path>__0[2] = this.<>f__this.transform.position;
                    this.<path>__0[1] = (Vector3) (0.5f * (this.<path>__0[0] + this.<path>__0[2]));
                    this.<jumpHeight>__1 = UnityEngine.Random.Range(this.<>f__this.m_MinJumpHeight, this.<>f__this.m_MaxJumpHeight);
                    this.<path>__0[1].y += this.<jumpHeight>__1;
                    object[] args = new object[] { "path", this.<path>__0, "orienttopath", true, "time", this.<>f__this.m_JumpInDuration, "easetype", this.<>f__this.m_JumpInEaseType, "oncomplete", "OnJumpInComplete", "oncompletetarget", this.<>f__this.gameObject, "oncompleteparams", this.card };
                    this.<argTable>__2 = iTween.Hash(args);
                    iTween.MoveTo(this.card.gameObject, this.<argTable>__2);
                    if (this.<>f__this.m_JumpInSoundPrefab != null)
                    {
                        this.<>f__this.StartCoroutine(this.<>f__this.LoadAndPlaySound(this.<>f__this.m_JumpInSoundPrefab, this.<>f__this.m_JumpInSoundDelay));
                    }
                    this.$PC = -1;
                    break;
                }
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
    private sealed class <JumpOut>c__Iterator1CA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal float <$>delaySec;
        internal Vector3 <$>destPos;
        internal BrawlSpell <>f__this;
        internal Hashtable <argTable>__2;
        internal float <jumpHeight>__1;
        internal Vector3[] <path>__0;
        internal Card card;
        internal float delaySec;
        internal Vector3 destPos;

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
                    this.$current = new WaitForSeconds(this.delaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    this.card.transform.rotation = Quaternion.identity;
                    this.card.ShowCard();
                    this.<path>__0 = new Vector3[3];
                    this.<path>__0[0] = this.card.transform.position;
                    this.<path>__0[2] = this.destPos;
                    this.<path>__0[1] = (Vector3) (0.5f * (this.<path>__0[0] + this.<path>__0[2]));
                    this.<jumpHeight>__1 = UnityEngine.Random.Range(this.<>f__this.m_MinJumpHeight, this.<>f__this.m_MaxJumpHeight);
                    this.<path>__0[1].y += this.<jumpHeight>__1;
                    object[] args = new object[] { "path", this.<path>__0, "time", this.<>f__this.m_JumpOutDuration, "easetype", this.<>f__this.m_JumpOutEaseType, "oncomplete", "OnJumpOutComplete", "oncompletetarget", this.<>f__this.gameObject, "oncompleteparams", this.card };
                    this.<argTable>__2 = iTween.Hash(args);
                    iTween.MoveTo(this.card.gameObject, this.<argTable>__2);
                    if (this.<>f__this.m_JumpOutSoundPrefab != null)
                    {
                        this.<>f__this.StartCoroutine(this.<>f__this.LoadAndPlaySound(this.<>f__this.m_JumpOutSoundPrefab, this.<>f__this.m_JumpOutSoundDelay));
                    }
                    if (this.<>f__this.m_LandSoundPrefab != null)
                    {
                        this.<>f__this.StartCoroutine(this.<>f__this.LoadAndPlaySound(this.<>f__this.m_LandSoundPrefab, this.<>f__this.m_LandSoundDelay));
                    }
                    this.$PC = -1;
                    break;
                }
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
    private sealed class <LoadAndPlaySound>c__Iterator1CC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delaySec;
        internal AudioSource <$>prefab;
        internal BrawlSpell <>f__this;
        internal AudioSource <source>__0;
        internal float delaySec;
        internal AudioSource prefab;

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
                    this.<source>__0 = UnityEngine.Object.Instantiate<AudioSource>(this.prefab);
                    this.<source>__0.transform.parent = this.<>f__this.transform;
                    TransformUtil.Identity(this.<source>__0);
                    this.$current = new WaitForSeconds(this.delaySec);
                    this.$PC = 1;
                    return true;

                case 1:
                    SoundManager.Get().PlayPreloaded(this.<source>__0);
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
    private sealed class <SurvivorHold>c__Iterator1CB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BrawlSpell <>f__this;

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
                    this.<>f__this.m_survivorCard.transform.rotation = Quaternion.identity;
                    this.<>f__this.m_survivorCard.ShowCard();
                    this.$current = new WaitForSeconds(this.<>f__this.m_SurvivorHoldDuration);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.IsSurvivorAlone())
                    {
                        this.<>f__this.m_survivorCard.GetZone().UpdateLayout();
                    }
                    this.<>f__this.OnSpellFinished();
                    this.<>f__this.OnStateFinished();
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

