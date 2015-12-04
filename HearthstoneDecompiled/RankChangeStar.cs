using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RankChangeStar : MonoBehaviour
{
    public MeshRenderer m_bottomGlowRenderer;
    public MeshRenderer m_starMeshRenderer;
    public MeshRenderer m_topGlowRenderer;

    public void BlackOut()
    {
        this.m_starMeshRenderer.enabled = false;
    }

    public void Blink(float delay)
    {
        base.StartCoroutine(this.DelayedBlink(delay));
    }

    public void Burst(float delay)
    {
        base.StartCoroutine(this.DelayedBurst(delay));
    }

    [DebuggerHidden]
    public IEnumerator DelayedBlink(float delay)
    {
        return new <DelayedBlink>c__Iterator74 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator DelayedBurst(float delay)
    {
        return new <DelayedBurst>c__Iterator75 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator DelayedDespawn(float delay)
    {
        return new <DelayedDespawn>c__Iterator76 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    [DebuggerHidden]
    public IEnumerator DelayedWipe(float delay)
    {
        return new <DelayedWipe>c__Iterator77 { delay = delay, <$>delay = delay, <>f__this = this };
    }

    public void Despawn()
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("DeSpawn");
    }

    public void FadeIn()
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("FadeIn");
    }

    public void Reset()
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("Reset");
    }

    public void Spawn()
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("Spawn");
    }

    public void UnBlackOut()
    {
        this.m_starMeshRenderer.enabled = true;
    }

    public void Wipe(float delay)
    {
        base.StartCoroutine(this.DelayedWipe(delay));
    }

    [CompilerGenerated]
    private sealed class <DelayedBlink>c__Iterator74 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeStar <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.GetComponent<PlayMakerFSM>().SendEvent("Blink");
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
    private sealed class <DelayedBurst>c__Iterator75 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeStar <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.UnBlackOut();
                    this.<>f__this.GetComponent<PlayMakerFSM>().SendEvent("Burst");
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
    private sealed class <DelayedDespawn>c__Iterator76 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeStar <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.GetComponent<PlayMakerFSM>().SendEvent("DeSpawn");
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
    private sealed class <DelayedWipe>c__Iterator77 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal RankChangeStar <>f__this;
        internal float delay;

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
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.GetComponent<PlayMakerFSM>().SendEvent("Wipe");
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

