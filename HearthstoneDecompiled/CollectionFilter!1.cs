using System;
using System.Collections.Generic;

public class CollectionFilter<T>
{
    private CollectionFilterFunc m_func;
    private T m_key;
    private List<object> m_values;

    public bool DoesValueMatch(bool val)
    {
        foreach (object obj2 in this.m_values)
        {
            CollectionFilterFunc func = this.m_func;
            if (func != CollectionFilterFunc.EQUAL)
            {
                if (func == CollectionFilterFunc.NOT_EQUAL)
                {
                    goto Label_004B;
                }
                continue;
            }
            if (!val.Equals(obj2))
            {
                continue;
            }
            return true;
        Label_004B:
            if (!val.Equals(obj2))
            {
                return true;
            }
        }
        return false;
    }

    public bool DoesValueMatch(int val)
    {
        foreach (object obj2 in this.m_values)
        {
            int num = Convert.ToInt32(obj2);
            switch (this.m_func)
            {
                case CollectionFilterFunc.EQUAL:
                    if (val != num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.NOT_EQUAL:
                    if (val == num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.LESS:
                    if (val >= num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.LESS_EQUAL:
                    if (val > num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.GREATER:
                    if (val <= num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.GREATER_EQUAL:
                    if (val < num)
                    {
                        break;
                    }
                    return true;
            }
        }
        return false;
    }

    public bool DoesValueMatch(long val)
    {
        foreach (object obj2 in this.m_values)
        {
            long num = Convert.ToInt64(obj2);
            switch (this.m_func)
            {
                case CollectionFilterFunc.EQUAL:
                    if (val != num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.NOT_EQUAL:
                    if (val == num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.LESS:
                    if (val >= num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.LESS_EQUAL:
                    if (val > num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.GREATER:
                    if (val <= num)
                    {
                        break;
                    }
                    return true;

                case CollectionFilterFunc.GREATER_EQUAL:
                    if (val < num)
                    {
                        break;
                    }
                    return true;
            }
        }
        return false;
    }

    public bool DoesValueMatch(string val)
    {
        foreach (object obj2 in this.m_values)
        {
            string str = obj2.ToString();
            CollectionFilterFunc func = this.m_func;
            if (func != CollectionFilterFunc.EQUAL)
            {
                if (func == CollectionFilterFunc.NOT_EQUAL)
                {
                    goto Label_0052;
                }
                continue;
            }
            if (!(val == str))
            {
                continue;
            }
            return true;
        Label_0052:
            if (val != str)
            {
                return true;
            }
        }
        return false;
    }

    public CollectionFilterFunc GetFunc()
    {
        return this.m_func;
    }

    public T GetKey()
    {
        return this.m_key;
    }

    public List<object> GetValues()
    {
        return this.m_values;
    }

    public bool HasValue(object val)
    {
        foreach (object obj2 in this.m_values)
        {
            if (obj2.Equals(val))
            {
                return true;
            }
        }
        return false;
    }

    public void SetFunc(CollectionFilterFunc func)
    {
        this.m_func = func;
    }

    public void SetKey(T key)
    {
        this.m_key = key;
    }

    public void SetValue(object val)
    {
        this.m_values = new List<object>();
        this.m_values.Add(val);
    }

    public void SetValues(List<object> values)
    {
        this.m_values = values;
    }
}

