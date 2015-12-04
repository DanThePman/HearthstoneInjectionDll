using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RandomPickPlaymakerEvent : MonoBehaviour
{
    public bool m_AllowNoneState = true;
    private bool m_AlternateEventState;
    public List<PickEvent> m_AlternateState;
    public int m_AwakeStateIndex = -1;
    private Collider m_Collider;
    private PickEvent m_CurrentState;
    private bool m_EndAnimationFinished = true;
    private int m_LastAlternateIndex;
    private int m_LastEventIndex;
    private bool m_StartAnimationFinished = true;
    public List<PickEvent> m_State;
    private bool m_StateActive;

    private void Awake()
    {
        this.m_Collider = base.GetComponent<Collider>();
        if (this.m_AwakeStateIndex > -1)
        {
            this.m_CurrentState = this.m_State[this.m_AwakeStateIndex];
            this.m_LastEventIndex = this.m_AwakeStateIndex;
            this.m_StateActive = true;
        }
    }

    private void DisableCollider()
    {
        if (this.m_Collider != null)
        {
            this.m_Collider.enabled = false;
        }
    }

    private void EnableCollider()
    {
        if (this.m_Collider != null)
        {
            this.m_Collider.enabled = true;
        }
    }

    public void EndAnimationFinished()
    {
        this.m_EndAnimationFinished = true;
    }

    public void RandomPickEvent()
    {
        object[] args = new object[] { this.m_StartAnimationFinished, this.m_EndAnimationFinished };
        Log.Kyle.Print("RandomPickEvent {0} {1}", args);
        if (this.m_StartAnimationFinished && this.m_EndAnimationFinished)
        {
            object[] objArray2 = new object[] { this.m_StateActive };
            Log.Kyle.Print("RandomPickEvent m_StateActive={0}", objArray2);
            if ((this.m_StateActive && (this.m_CurrentState.m_EndEvent != string.Empty)) && (this.m_CurrentState.m_FSM != null))
            {
                this.m_CurrentState.m_FSM.SendEvent(this.m_CurrentState.m_EndEvent);
                this.m_EndAnimationFinished = false;
                this.m_StateActive = false;
                base.StartCoroutine(this.WaitForEndAnimation());
            }
            else if (this.m_AlternateState.Count > 0)
            {
                if (this.m_AlternateEventState)
                {
                    this.SendRandomEvent();
                }
                else
                {
                    this.SendAlternateRandomEvent();
                }
            }
            else
            {
                this.SendRandomEvent();
            }
        }
    }

    private void SendAlternateRandomEvent()
    {
        this.m_StateActive = true;
        this.m_AlternateEventState = true;
        List<int> list = new List<int>();
        if (this.m_AlternateState.Count == 1)
        {
            list.Add(0);
        }
        else
        {
            for (int i = 0; i < this.m_AlternateState.Count; i++)
            {
                if (i != this.m_LastAlternateIndex)
                {
                    list.Add(i);
                }
            }
        }
        int num2 = UnityEngine.Random.Range(0, list.Count);
        PickEvent event2 = this.m_AlternateState[list[num2]];
        this.m_CurrentState = event2;
        this.m_LastAlternateIndex = list[num2];
        this.m_StartAnimationFinished = false;
        base.StartCoroutine(this.WaitForStartAnimation());
        event2.m_FSM.SendEvent(event2.m_StartEvent);
    }

    private void SendRandomEvent()
    {
        this.m_StateActive = true;
        this.m_AlternateEventState = false;
        List<int> list = new List<int>();
        if (this.m_State.Count == 1)
        {
            list.Add(0);
        }
        else
        {
            for (int i = 0; i < this.m_State.Count; i++)
            {
                if (i != this.m_LastEventIndex)
                {
                    list.Add(i);
                }
            }
        }
        int num2 = UnityEngine.Random.Range(0, list.Count);
        PickEvent event2 = this.m_State[list[num2]];
        this.m_CurrentState = event2;
        this.m_LastEventIndex = list[num2];
        this.m_StartAnimationFinished = false;
        base.StartCoroutine(this.WaitForStartAnimation());
        event2.m_FSM.SendEvent(event2.m_StartEvent);
    }

    public void StartAnimationFinished()
    {
        this.m_StartAnimationFinished = true;
    }

    [DebuggerHidden]
    private IEnumerator WaitForEndAnimation()
    {
        return new <WaitForEndAnimation>c__Iterator28B { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForStartAnimation()
    {
        return new <WaitForStartAnimation>c__Iterator28A { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitForEndAnimation>c__Iterator28B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal RandomPickPlaymakerEvent <>f__this;

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
                    if (!this.<>f__this.m_EndAnimationFinished)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_00AA;
                    }
                    this.<>f__this.m_CurrentState = null;
                    if (this.<>f__this.m_AllowNoneState)
                    {
                        goto Label_00A8;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_00A8;
            }
            while (!this.<>f__this.m_StartAnimationFinished)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_00AA;
            }
            this.<>f__this.RandomPickEvent();
            goto Label_00A8;
            this.$PC = -1;
        Label_00A8:
            return false;
        Label_00AA:
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
    private sealed class <WaitForStartAnimation>c__Iterator28A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal RandomPickPlaymakerEvent <>f__this;

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
                    if (!this.<>f__this.m_StartAnimationFinished)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
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

    [Serializable]
    public class PickEvent
    {
        [HideInInspector]
        public int m_CurrentItemIndex;
        public string m_EndEvent;
        public PlayMakerFSM m_FSM;
        public string m_StartEvent;
    }
}

