using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Map<TKey, TValue> : IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>>
{
    private int count;
    private const float DEFAULT_LOAD_FACTOR = 0.9f;
    private int emptySlot;
    private int generation;
    private const int HASH_FLAG = -2147483648;
    private IEqualityComparer<TKey> hcp;
    private const int INITIAL_SIZE = 4;
    private TKey[] keySlots;
    private Link[] linkSlots;
    private const int NO_SLOT = -1;
    private int[] table;
    private int threshold;
    private int touchedSlots;
    private TValue[] valueSlots;

    public Map()
    {
        this.Init(4, null);
    }

    public Map(IEnumerable<KeyValuePair<TKey, TValue>> copy)
    {
        this.Init(4, null);
        IEnumerator<KeyValuePair<TKey, TValue>> enumerator = copy.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                KeyValuePair<TKey, TValue> current = enumerator.Current;
                this[current.Key] = current.Value;
            }
        }
        finally
        {
            if (enumerator == null)
            {
            }
            enumerator.Dispose();
        }
    }

    public Map(IEqualityComparer<TKey> comparer)
    {
        this.Init(4, comparer);
    }

    public Map(int count)
    {
        this.Init(count, null);
    }

    public void Add(TKey key, TValue value)
    {
        int emptySlot;
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        int num = this.hcp.GetHashCode(key) | -2147483648;
        int index = (num & 0x7fffffff) % this.table.Length;
        for (emptySlot = this.table[index] - 1; emptySlot != -1; emptySlot = this.linkSlots[emptySlot].Next)
        {
            if ((this.linkSlots[emptySlot].HashCode == num) && this.hcp.Equals(this.keySlots[emptySlot], key))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }
        }
        if (++this.count > this.threshold)
        {
            this.Resize();
            index = (num & 0x7fffffff) % this.table.Length;
        }
        emptySlot = this.emptySlot;
        if (emptySlot == -1)
        {
            emptySlot = this.touchedSlots++;
        }
        else
        {
            this.emptySlot = this.linkSlots[emptySlot].Next;
        }
        this.linkSlots[emptySlot].HashCode = num;
        this.linkSlots[emptySlot].Next = this.table[index] - 1;
        this.table[index] = emptySlot + 1;
        this.keySlots[emptySlot] = key;
        this.valueSlots[emptySlot] = value;
        this.generation++;
    }

    public void Clear()
    {
        if (this.count != 0)
        {
            this.count = 0;
            Array.Clear(this.table, 0, this.table.Length);
            Array.Clear(this.keySlots, 0, this.keySlots.Length);
            Array.Clear(this.valueSlots, 0, this.valueSlots.Length);
            Array.Clear(this.linkSlots, 0, this.linkSlots.Length);
            this.emptySlot = -1;
            this.touchedSlots = 0;
            this.generation++;
        }
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        int num = this.hcp.GetHashCode(key) | -2147483648;
        for (int i = this.table[(num & 0x7fffffff) % this.table.Length] - 1; i != -1; i = this.linkSlots[i].Next)
        {
            if ((this.linkSlots[i].HashCode == num) && this.hcp.Equals(this.keySlots[i], key))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsValue(TValue value)
    {
        IEqualityComparer<TValue> comparer = (IEqualityComparer<TValue>) EqualityComparer<TValue>.Default;
        for (int i = 0; i < this.table.Length; i++)
        {
            for (int j = this.table[i] - 1; j != -1; j = this.linkSlots[j].Next)
            {
                if (comparer.Equals(this.valueSlots[j], value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CopyKeys(TKey[] array, int index)
    {
        for (int i = 0; i < this.touchedSlots; i++)
        {
            if ((this.linkSlots[i].HashCode & -2147483648) != 0)
            {
                array[index++] = this.keySlots[i];
            }
        }
    }

    private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        this.CopyToCheck(array, index);
        for (int i = 0; i < this.touchedSlots; i++)
        {
            if ((this.linkSlots[i].HashCode & -2147483648) != 0)
            {
                array[index++] = new KeyValuePair<TKey, TValue>(this.keySlots[i], this.valueSlots[i]);
            }
        }
    }

    private void CopyToCheck(Array array, int index)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException("index");
        }
        if (index > array.Length)
        {
            throw new ArgumentException("index larger than largest valid index of array");
        }
        if ((array.Length - index) < this.Count)
        {
            throw new ArgumentException("Destination array cannot hold the requested elements!");
        }
    }

    private void CopyValues(TValue[] array, int index)
    {
        for (int i = 0; i < this.touchedSlots; i++)
        {
            if ((this.linkSlots[i].HashCode & -2147483648) != 0)
            {
                array[index++] = this.valueSlots[i];
            }
        }
    }

    private void Do_ICollectionCopyTo<TRet>(Array array, int index, Transform<TKey, TValue, TRet> transform)
    {
        System.Type c = typeof(TRet);
        System.Type elementType = array.GetType().GetElementType();
        try
        {
            if ((c.IsPrimitive || elementType.IsPrimitive) && !elementType.IsAssignableFrom(c))
            {
                throw new Exception();
            }
            object[] objArray = (object[]) array;
            for (int i = 0; i < this.touchedSlots; i++)
            {
                if ((this.linkSlots[i].HashCode & -2147483648) != 0)
                {
                    objArray[index++] = transform(this.keySlots[i], this.valueSlots[i]);
                }
            }
        }
        catch (Exception exception)
        {
            throw new ArgumentException("Cannot copy source collection elements to destination array", "array", exception);
        }
    }

    public Enumerator<TKey, TValue> GetEnumerator()
    {
        return new Enumerator<TKey, TValue>((Map<TKey, TValue>) this);
    }

    private void Init(int capacity, IEqualityComparer<TKey> hcp)
    {
        if (hcp == null)
        {
        }
        this.hcp = EqualityComparer<TKey>.Default;
        capacity = Math.Max(1, (int) (((float) capacity) / 0.9f));
        this.InitArrays(capacity);
    }

    private void InitArrays(int size)
    {
        this.table = new int[size];
        this.linkSlots = new Link[size];
        this.emptySlot = -1;
        this.keySlots = new TKey[size];
        this.valueSlots = new TValue[size];
        this.touchedSlots = 0;
        this.threshold = (int) (this.table.Length * 0.9f);
        if ((this.threshold == 0) && (this.table.Length > 0))
        {
            this.threshold = 1;
        }
    }

    private static KeyValuePair<TKey, TValue> make_pair(TKey key, TValue value)
    {
        return new KeyValuePair<TKey, TValue>(key, value);
    }

    private static TKey pick_key(TKey key, TValue value)
    {
        return key;
    }

    private static TValue pick_value(TKey key, TValue value)
    {
        return value;
    }

    public bool Remove(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        int num = this.hcp.GetHashCode(key) | -2147483648;
        int index = (num & 0x7fffffff) % this.table.Length;
        int next = this.table[index] - 1;
        if (next == -1)
        {
            return false;
        }
        int num4 = -1;
        do
        {
            if ((this.linkSlots[next].HashCode == num) && this.hcp.Equals(this.keySlots[next], key))
            {
                break;
            }
            num4 = next;
            next = this.linkSlots[next].Next;
        }
        while (next != -1);
        if (next == -1)
        {
            return false;
        }
        this.count--;
        if (num4 == -1)
        {
            this.table[index] = this.linkSlots[next].Next + 1;
        }
        else
        {
            this.linkSlots[num4].Next = this.linkSlots[next].Next;
        }
        this.linkSlots[next].Next = this.emptySlot;
        this.emptySlot = next;
        this.linkSlots[next].HashCode = 0;
        this.keySlots[next] = default(TKey);
        this.valueSlots[next] = default(TValue);
        this.generation++;
        return true;
    }

    private void Resize()
    {
        int num = HashPrimeNumbers.ToPrime((this.table.Length << 1) | 1);
        int[] numArray = new int[num];
        Link[] linkArray = new Link[num];
        for (int i = 0; i < this.table.Length; i++)
        {
            for (int j = this.table[i] - 1; j != -1; j = this.linkSlots[j].Next)
            {
                int num4 = linkArray[j].HashCode = this.hcp.GetHashCode(this.keySlots[j]) | -2147483648;
                int index = (num4 & 0x7fffffff) % num;
                linkArray[j].Next = numArray[index] - 1;
                numArray[index] = j + 1;
            }
        }
        this.table = numArray;
        this.linkSlots = linkArray;
        TKey[] destinationArray = new TKey[num];
        TValue[] localArray2 = new TValue[num];
        Array.Copy(this.keySlots, 0, destinationArray, 0, this.touchedSlots);
        Array.Copy(this.valueSlots, 0, localArray2, 0, this.touchedSlots);
        this.keySlots = destinationArray;
        this.valueSlots = localArray2;
        this.threshold = (int) (num * 0.9f);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return new Enumerator<TKey, TValue>((Map<TKey, TValue>) this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator<TKey, TValue>((Map<TKey, TValue>) this);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        int num = this.hcp.GetHashCode(key) | -2147483648;
        for (int i = this.table[(num & 0x7fffffff) % this.table.Length] - 1; i != -1; i = this.linkSlots[i].Next)
        {
            if ((this.linkSlots[i].HashCode == num) && this.hcp.Equals(this.keySlots[i], key))
            {
                value = this.valueSlots[i];
                return true;
            }
        }
        value = default(TValue);
        return false;
    }

    public int Count
    {
        get
        {
            return this.count;
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int num = this.hcp.GetHashCode(key) | -2147483648;
            for (int i = this.table[(num & 0x7fffffff) % this.table.Length] - 1; i != -1; i = this.linkSlots[i].Next)
            {
                if ((this.linkSlots[i].HashCode == num) && this.hcp.Equals(this.keySlots[i], key))
                {
                    return this.valueSlots[i];
                }
            }
            throw new KeyNotFoundException();
        }
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int num = this.hcp.GetHashCode(key) | -2147483648;
            int index = (num & 0x7fffffff) % this.table.Length;
            int next = this.table[index] - 1;
            int num4 = -1;
            while (next != -1)
            {
                if ((this.linkSlots[next].HashCode == num) && this.hcp.Equals(this.keySlots[next], key))
                {
                    break;
                }
                num4 = next;
                next = this.linkSlots[next].Next;
            }
            if (next == -1)
            {
                if (++this.count > this.threshold)
                {
                    this.Resize();
                    index = (num & 0x7fffffff) % this.table.Length;
                }
                next = this.emptySlot;
                if (next == -1)
                {
                    next = this.touchedSlots++;
                }
                else
                {
                    this.emptySlot = this.linkSlots[next].Next;
                }
                this.linkSlots[next].Next = this.table[index] - 1;
                this.table[index] = next + 1;
                this.linkSlots[next].HashCode = num;
                this.keySlots[next] = key;
            }
            else if (num4 != -1)
            {
                this.linkSlots[num4].Next = this.linkSlots[next].Next;
                this.linkSlots[next].Next = this.table[index] - 1;
                this.table[index] = next + 1;
            }
            this.valueSlots[next] = value;
            this.generation++;
        }
    }

    public KeyCollection<TKey, TValue> Keys
    {
        get
        {
            return new KeyCollection<TKey, TValue>((Map<TKey, TValue>) this);
        }
    }

    public ValueCollection<TKey, TValue> Values
    {
        get
        {
            return new ValueCollection<TKey, TValue>((Map<TKey, TValue>) this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Enumerator : IDisposable, IEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private Map<TKey, TValue> dictionary;
        private int next;
        private int stamp;
        internal KeyValuePair<TKey, TValue> current;
        internal Enumerator(Map<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
            this.stamp = dictionary.generation;
        }

        object IEnumerator.Current
        {
            get
            {
                this.VerifyCurrent();
                return this.current;
            }
        }
        void IEnumerator.Reset()
        {
            this.Reset();
        }

        public bool MoveNext()
        {
            this.VerifyState();
            if (this.next >= 0)
            {
                while (this.next < this.dictionary.touchedSlots)
                {
                    int index = this.next++;
                    if ((this.dictionary.linkSlots[index].HashCode & -2147483648) != 0)
                    {
                        this.current = new KeyValuePair<TKey, TValue>(this.dictionary.keySlots[index], this.dictionary.valueSlots[index]);
                        return true;
                    }
                }
                this.next = -1;
            }
            return false;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                return this.current;
            }
        }
        internal TKey CurrentKey
        {
            get
            {
                this.VerifyCurrent();
                return this.current.Key;
            }
        }
        internal TValue CurrentValue
        {
            get
            {
                this.VerifyCurrent();
                return this.current.Value;
            }
        }
        internal void Reset()
        {
            this.VerifyState();
            this.next = 0;
        }

        private void VerifyState()
        {
            if (this.dictionary == null)
            {
                throw new ObjectDisposedException(null);
            }
            if (this.dictionary.generation != this.stamp)
            {
                throw new InvalidOperationException("out of sync");
            }
        }

        private void VerifyCurrent()
        {
            this.VerifyState();
            if (this.next <= 0)
            {
                throw new InvalidOperationException("Current is not valid");
            }
        }

        public void Dispose()
        {
            this.dictionary = null;
        }
    }

    public sealed class KeyCollection : ICollection, IEnumerable, ICollection<TKey>, IEnumerable<TKey>
    {
        private Map<TKey, TValue> dictionary;

        public KeyCollection(Map<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            this.dictionary = dictionary;
        }

        public void CopyTo(TKey[] array, int index)
        {
            this.dictionary.CopyToCheck(array, index);
            this.dictionary.CopyKeys(array, index);
        }

        public Enumerator<TKey, TValue> GetEnumerator()
        {
            return new Enumerator<TKey, TValue>(this.dictionary);
        }

        void ICollection<TKey>.Add(TKey item)
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        void ICollection<TKey>.Clear()
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        bool ICollection<TKey>.Contains(TKey item)
        {
            return this.dictionary.ContainsKey(item);
        }

        bool ICollection<TKey>.Remove(TKey item)
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            TKey[] localArray = array as TKey[];
            if (localArray != null)
            {
                this.CopyTo(localArray, index);
            }
            else
            {
                this.dictionary.CopyToCheck(array, index);
                this.dictionary.Do_ICollectionCopyTo<TKey>(array, index, new Map<TKey, TValue>.Transform<TKey>(Map<TKey, TValue>.pick_key));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        bool ICollection<TKey>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection) this.dictionary).SyncRoot;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IDisposable, IEnumerator, IEnumerator<TKey>
        {
            private Map<TKey, TValue>.Enumerator host_enumerator;
            internal Enumerator(Map<TKey, TValue> host)
            {
                this.host_enumerator = host.GetEnumerator();
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.host_enumerator.CurrentKey;
                }
            }
            void IEnumerator.Reset()
            {
                this.host_enumerator.Reset();
            }

            public void Dispose()
            {
                this.host_enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return this.host_enumerator.MoveNext();
            }

            public TKey Current
            {
                get
                {
                    return this.host_enumerator.current.Key;
                }
            }
        }
    }

    private delegate TRet Transform<TRet>(TKey key, TValue value);

    public sealed class ValueCollection : ICollection, IEnumerable, ICollection<TValue>, IEnumerable<TValue>
    {
        private Map<TKey, TValue> dictionary;

        public ValueCollection(Map<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            this.dictionary = dictionary;
        }

        public void CopyTo(TValue[] array, int index)
        {
            this.dictionary.CopyToCheck(array, index);
            this.dictionary.CopyValues(array, index);
        }

        public Enumerator<TKey, TValue> GetEnumerator()
        {
            return new Enumerator<TKey, TValue>(this.dictionary);
        }

        void ICollection<TValue>.Add(TValue item)
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        bool ICollection<TValue>.Contains(TValue item)
        {
            return this.dictionary.ContainsValue(item);
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotSupportedException("this is a read-only collection");
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            TValue[] localArray = array as TValue[];
            if (localArray != null)
            {
                this.CopyTo(localArray, index);
            }
            else
            {
                this.dictionary.CopyToCheck(array, index);
                this.dictionary.Do_ICollectionCopyTo<TValue>(array, index, new Map<TKey, TValue>.Transform<TValue>(Map<TKey, TValue>.pick_value));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        bool ICollection<TValue>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection) this.dictionary).SyncRoot;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IDisposable, IEnumerator, IEnumerator<TValue>
        {
            private Map<TKey, TValue>.Enumerator host_enumerator;
            internal Enumerator(Map<TKey, TValue> host)
            {
                this.host_enumerator = host.GetEnumerator();
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.host_enumerator.CurrentValue;
                }
            }
            void IEnumerator.Reset()
            {
                this.host_enumerator.Reset();
            }

            public void Dispose()
            {
                this.host_enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return this.host_enumerator.MoveNext();
            }

            public TValue Current
            {
                get
                {
                    return this.host_enumerator.current.Value;
                }
            }
        }
    }
}

