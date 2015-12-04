using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayAnimRandomStart : MonoBehaviour
{
    public string animName = "Bubble1";
    public List<GameObject> m_Bubbles;
    public float MaxSpeed = 1.1f;
    public float maxWait = 10f;
    public float MinSpeed = 0.2f;
    public float minWait;

    [DebuggerHidden]
    private IEnumerator PlayRandomBubbles()
    {
        return new <PlayRandomBubbles>c__Iterator287 { <>f__this = this };
    }

    private void Start()
    {
        base.StartCoroutine(this.PlayRandomBubbles());
    }

    [CompilerGenerated]
    private sealed class <PlayRandomBubbles>c__Iterator287 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal PlayAnimRandomStart <>f__this;
        internal int <bubbleIndex>__0;
        internal GameObject <bubbles>__1;

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
                    break;

                case 1:
                    this.<bubbleIndex>__0 = UnityEngine.Random.Range(0, this.<>f__this.m_Bubbles.Count);
                    this.<bubbles>__1 = this.<>f__this.m_Bubbles[this.<bubbleIndex>__0];
                    if (this.<bubbles>__1 != null)
                    {
                        this.<bubbles>__1.GetComponent<Animation>().Play();
                        this.<bubbles>__1.GetComponent<Animation>()[this.<>f__this.animName].speed = UnityEngine.Random.Range(this.<>f__this.MinSpeed, this.<>f__this.MaxSpeed);
                    }
                    break;

                default:
                    goto Label_00F9;
            }
            this.$current = new WaitForSeconds(UnityEngine.Random.Range(this.<>f__this.minWait, this.<>f__this.maxWait));
            this.$PC = 1;
            return true;
            this.$PC = -1;
        Label_00F9:
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

