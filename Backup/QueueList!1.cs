using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

public class QueueList<T> : IEnumerable<T>, IEnumerable
{
    protected List<T> m_list;

    public QueueList()
    {
        this.m_list = new List<T>();
    }

    public void Clear()
    {
        this.m_list.Clear();
    }

    public bool Contains(T item)
    {
        return this.m_list.Contains(item);
    }

    public T Dequeue()
    {
        T local = this.m_list[0];
        this.m_list.RemoveAt(0);
        return local;
    }

    public int Enqueue(T item)
    {
        int count = this.m_list.Count;
        this.m_list.Add(item);
        return count;
    }

    [DebuggerHidden]
    protected IEnumerable<T> Enumerate()
    {
        return new <Enumerate>c__IteratorC5<T> { <>f__this = (QueueList<T>) this, $PC = -2 };
    }

    public int GetCount()
    {
        return this.m_list.Count;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this.Enumerate().GetEnumerator();
    }

    public T GetItem(int index)
    {
        return this.m_list[index];
    }

    public List<T> GetList()
    {
        return this.m_list;
    }

    public T Peek()
    {
        return this.m_list[0];
    }

    public bool Remove(T item)
    {
        return this.m_list.Remove(item);
    }

    public T RemoveAt(int position)
    {
        if (this.m_list.Count <= position)
        {
            return default(T);
        }
        T local = this.m_list[position];
        this.m_list.RemoveAt(position);
        return local;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Format("Count={0}", this.Count);
    }

    public int Count
    {
        get
        {
            return this.m_list.Count;
        }
    }

    public T this[int index]
    {
        get
        {
            return this.m_list[index];
        }
        set
        {
            this.m_list[index] = value;
        }
    }

    [CompilerGenerated]
    private sealed class <Enumerate>c__IteratorC5 : IDisposable, IEnumerator, IEnumerable<T>, IEnumerable, IEnumerator<T>
    {
        internal T $current;
        internal int $PC;
        internal QueueList<T> <>f__this;
        internal int <i>__0;

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
                    this.<i>__0 = 0;
                    break;

                case 1:
                    this.<i>__0++;
                    break;

                default:
                    goto Label_0085;
            }
            if (this.<i>__0 < this.<>f__this.m_list.Count)
            {
                this.$current = this.<>f__this.m_list[this.<i>__0];
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_0085:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
            {
                return this;
            }
            return new QueueList<T>.<Enumerate>c__IteratorC5 { <>f__this = this.<>f__this };
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
        }

        T IEnumerator<T>.Current
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

