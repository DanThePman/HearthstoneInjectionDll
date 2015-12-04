using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BnetPlayerChangelist
{
    private List<BnetPlayerChange> m_changes = new List<BnetPlayerChange>();

    public void AddChange(BnetPlayerChange change)
    {
        this.m_changes.Add(change);
    }

    public BnetPlayerChange FindChange(BnetAccountId id)
    {
        BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(id);
        return this.FindChange(player);
    }

    public BnetPlayerChange FindChange(BnetGameAccountId id)
    {
        BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(id);
        return this.FindChange(player);
    }

    public BnetPlayerChange FindChange(BnetPlayer player)
    {
        <FindChange>c__AnonStorey395 storey = new <FindChange>c__AnonStorey395 {
            player = player
        };
        if (storey.player == null)
        {
            return null;
        }
        return this.m_changes.Find(new Predicate<BnetPlayerChange>(storey.<>m__29B));
    }

    public List<BnetPlayerChange> GetChanges()
    {
        return this.m_changes;
    }

    public bool HasChange(BnetAccountId id)
    {
        BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(id);
        return this.HasChange(player);
    }

    public bool HasChange(BnetGameAccountId id)
    {
        BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(id);
        return this.HasChange(player);
    }

    public bool HasChange(BnetPlayer player)
    {
        return (this.FindChange(player) != null);
    }

    [CompilerGenerated]
    private sealed class <FindChange>c__AnonStorey395
    {
        internal BnetPlayer player;

        internal bool <>m__29B(BnetPlayerChange change)
        {
            return (change.GetPlayer() == this.player);
        }
    }
}

