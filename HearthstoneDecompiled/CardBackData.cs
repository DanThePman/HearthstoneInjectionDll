using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class CardBackData
{
    public CardBackData(int id, CardBackSource source, long sourceData, string name, bool enabled, string prefabName)
    {
        this.ID = id;
        this.Source = source;
        this.SourceData = sourceData;
        this.Name = name;
        this.Enabled = enabled;
        this.PrefabName = prefabName;
    }

    public override string ToString()
    {
        object[] args = new object[] { this.ID, this.Name, this.Source, this.SourceData, this.Enabled, this.PrefabName };
        return string.Format("[CardBackData: ID={0}, Source={1}, SourceData={2}, Name={3}, Enabled={4}, PrefabName={5}]", args);
    }

    public bool Enabled { get; private set; }

    public int ID { get; private set; }

    public string Name { get; private set; }

    public string PrefabName { get; private set; }

    public CardBackSource Source { get; private set; }

    public long SourceData { get; private set; }

    public enum CardBackSource
    {
        [Description("achieve")]
        ACHIEVE = 2,
        [Description("fixed_reward")]
        FIXED_REWARD = 3,
        [Description("season")]
        SEASON = 1,
        [Description("startup")]
        STARTUP = 0,
        [Description("tavern_brawl")]
        TAVERN_BRAWL = 5
    }
}

