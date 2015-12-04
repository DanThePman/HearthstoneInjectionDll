using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(PlayMakerFSM)), RequireComponent(typeof(Animator))]
public class PlayMakerAnimatorIKProxy : MonoBehaviour
{
    private Animator _animator;

    public event Action<int> OnAnimatorIKEvent;

    private void OnAnimatorIK(int layerIndex)
    {
        if (this.OnAnimatorIKEvent != null)
        {
            this.OnAnimatorIKEvent(layerIndex);
        }
    }
}

