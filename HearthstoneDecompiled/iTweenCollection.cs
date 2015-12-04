using System;

public class iTweenCollection
{
    public int Count;
    public int DeletedCount;
    public int LastIndex;
    public iTween[] Tweens = new iTween[0x100];

    public void Add(iTween tween)
    {
        if (tween != null)
        {
            if (this.LastIndex >= this.Tweens.Length)
            {
                Array.Resize<iTween>(ref this.Tweens, this.Tweens.Length * 2);
            }
            this.Tweens[this.LastIndex] = tween;
            this.LastIndex++;
            this.Count++;
        }
    }

    public void CleanUp()
    {
        if (this.DeletedCount != 0)
        {
            int index = 0;
            for (int i = 0; i < this.LastIndex; i++)
            {
                if (this.Tweens[i] != null)
                {
                    this.Tweens[index] = this.Tweens[i];
                    index++;
                }
            }
            this.LastIndex -= this.DeletedCount;
            this.DeletedCount = 0;
        }
    }

    public iTweenIterator GetIterator()
    {
        return new iTweenIterator(this);
    }

    public void Remove(iTween tween)
    {
        if ((tween != null) && !tween.destroyed)
        {
            for (int i = 0; i < this.LastIndex; i++)
            {
                if (this.Tweens[i] == tween)
                {
                    this.Tweens[i].destroyed = true;
                    this.Tweens[i] = null;
                    this.Count--;
                    this.DeletedCount++;
                    break;
                }
            }
        }
    }
}

