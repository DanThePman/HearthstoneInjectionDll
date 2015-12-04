using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureWingProgressDisplay : MonoBehaviour
{
    public virtual bool HasProgressAnimationToPlay()
    {
        return false;
    }

    public virtual void PlayProgressAnimation(OnAnimationComplete onAnimComplete = null)
    {
        if (onAnimComplete != null)
        {
            onAnimComplete();
        }
    }

    public virtual void UpdateProgress(WingDbId wingDbId, bool normalComplete)
    {
    }

    public delegate void OnAnimationComplete();
}

