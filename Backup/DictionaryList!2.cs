using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

[Serializable]
public class DictionaryList<T, U> : List<DictionaryListItem<T, U>>
{
    public bool TryGetValue(T key, out U value)
    {
        foreach (DictionaryListItem<T, U> item in this)
        {
            if (item.m_key.Equals(key))
            {
                value = item.m_value;
                return true;
            }
        }
        value = default(U);
        return false;
    }

    public U this[T key]
    {
        get
        {
            U local;
            if (!this.TryGetValue(key, out local))
            {
                throw new KeyNotFoundException(string.Format("{0} key does not exist in ListDict.", key));
            }
            return local;
        }
        set
        {
            foreach (DictionaryListItem<T, U> item in this)
            {
                if (item.m_key.Equals(key))
                {
                    item.m_value = value;
                    return;
                }
            }
            DictionaryListItem<T, U> item2 = new DictionaryListItem<T, U> {
                m_key = key,
                m_value = value
            };
            this.Add(item2);
        }
    }
}

