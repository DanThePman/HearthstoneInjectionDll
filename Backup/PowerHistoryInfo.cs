using System;

public class PowerHistoryInfo
{
    private int mEffectIndex;
    private bool mShowInHistory;

    public PowerHistoryInfo()
    {
        this.mEffectIndex = -1;
    }

    public PowerHistoryInfo(int index, bool show)
    {
        this.mEffectIndex = -1;
        this.mEffectIndex = index;
        this.mShowInHistory = show;
    }

    public int GetEffectIndex()
    {
        return this.mEffectIndex;
    }

    public bool ShouldShowInHistory()
    {
        return this.mShowInHistory;
    }
}

