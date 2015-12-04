using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(PlayMakerFSM)), RequireComponent(typeof(Animator))]
public class PlayMakerAnimatorMoveProxy : MonoBehaviour
{
    public bool applyRootMotion;

    public event System.Action OnAnimatorMoveEvent;

    private void OnAnimatorMove()
    {
        if (this.OnAnimatorMoveEvent != null)
        {
            this.OnAnimatorMoveEvent();
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}

