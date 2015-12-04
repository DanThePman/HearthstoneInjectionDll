using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class TagSet
{
    private Map<int, int> m_values = new Map<int, int>();

    public void Clear()
    {
        this.m_values = new Map<int, int>();
    }

    public TagDeltaSet CreateDeltas(Map<GAME_TAG, int> comp)
    {
        TagDeltaSet set = new TagDeltaSet();
        foreach (KeyValuePair<GAME_TAG, int> pair in comp)
        {
            int key = pair.Key;
            int num2 = 0;
            this.m_values.TryGetValue(key, out num2);
            int curr = pair.Value;
            if (num2 != curr)
            {
                set.Add(key, num2, curr);
            }
        }
        return set;
    }

    public TagDeltaSet CreateDeltas(List<Network.Entity.Tag> comp)
    {
        TagDeltaSet set = new TagDeltaSet();
        foreach (Network.Entity.Tag tag in comp)
        {
            int name = tag.Name;
            int num2 = 0;
            this.m_values.TryGetValue(name, out num2);
            int curr = tag.Value;
            if (num2 != curr)
            {
                set.Add(name, num2, curr);
            }
        }
        return set;
    }

    public TagDeltaSet CreateDeltas(TagSet comp)
    {
        TagDeltaSet set = new TagDeltaSet();
        for (int i = 1; i < 0x200; i++)
        {
            if (this.m_values[i] != comp.m_values[i])
            {
                set.Add(i, this.m_values[i], comp.m_values[i]);
            }
        }
        return set;
    }

    public Map<int, int> GetMap()
    {
        return this.m_values;
    }

    public int GetTag(GAME_TAG enumTag)
    {
        int tag = Convert.ToInt32(enumTag);
        return this.GetTag(tag);
    }

    public TagEnum GetTag<TagEnum>(GAME_TAG enumTag)
    {
        int tag = Convert.ToInt32(enumTag);
        int num2 = this.GetTag(tag);
        return (TagEnum) Enum.ToObject(typeof(TagEnum), num2);
    }

    public int GetTag(int tag)
    {
        int num = 0;
        this.m_values.TryGetValue(tag, out num);
        return num;
    }

    public bool HasTag<TagEnum>(GAME_TAG tag)
    {
        return (Convert.ToUInt32(this.GetTag<TagEnum>(tag)) > 0);
    }

    public bool HasTag(int tag)
    {
        int num = 0;
        if (!this.m_values.TryGetValue(tag, out num))
        {
            return false;
        }
        return (num > 0);
    }

    public void Replace(List<Network.Entity.Tag> tags)
    {
        this.Clear();
        this.SetTags(tags);
    }

    public void Replace(TagSet sourceSet)
    {
        this.Clear();
        this.SetTags(sourceSet.m_values);
    }

    public void SetTag(GAME_TAG tag, int tagValue)
    {
        this.SetTag((int) tag, tagValue);
    }

    public void SetTag<TagEnum>(GAME_TAG tag, TagEnum tagValue)
    {
        this.SetTag((int) tag, Convert.ToInt32(tagValue));
    }

    public void SetTag(int tag, int tagValue)
    {
        this.m_values[tag] = tagValue;
    }

    public void SetTags(Map<GAME_TAG, int> tagMap)
    {
        foreach (KeyValuePair<GAME_TAG, int> pair in tagMap)
        {
            this.SetTag(pair.Key, pair.Value);
        }
    }

    public void SetTags(Map<int, int> tagMap)
    {
        foreach (KeyValuePair<int, int> pair in tagMap)
        {
            this.SetTag(pair.Key, pair.Value);
        }
    }

    public void SetTags(List<Network.Entity.Tag> tags)
    {
        foreach (Network.Entity.Tag tag in tags)
        {
            this.SetTag(tag.Name, tag.Value);
        }
    }

    public bool TryGetValue(int tag, out int value)
    {
        return this.m_values.TryGetValue(tag, out value);
    }
}

