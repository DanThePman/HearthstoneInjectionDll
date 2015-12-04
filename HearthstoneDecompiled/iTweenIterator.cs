using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct iTweenIterator
{
    private iTweenCollection TweenCollection;
    private int Index;
    public iTweenIterator(iTweenCollection collection)
    {
        this.TweenCollection = collection;
        this.Index = 0;
    }

    public iTween GetNext()
    {
        if (this.TweenCollection != null)
        {
            while (this.Index < this.TweenCollection.LastIndex)
            {
                iTween tween = this.TweenCollection.Tweens[this.Index];
                this.Index++;
                if (tween != null)
                {
                    return tween;
                }
            }
        }
        return null;
    }
}

