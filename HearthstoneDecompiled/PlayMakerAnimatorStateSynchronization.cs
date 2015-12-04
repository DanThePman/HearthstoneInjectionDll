using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayMakerAnimatorStateSynchronization : MonoBehaviour
{
    private Animator animator;
    public bool debug;
    public bool EveryFrame = true;
    public PlayMakerFSM Fsm;
    private Dictionary<int, FsmState> fsmStateLUT;
    private int lastState;
    private int lastTransition;
    public int LayerIndex;

    private void RegisterHash(string key, FsmState state)
    {
        int num = Animator.StringToHash(key);
        this.fsmStateLUT.Add(num, state);
        if (this.debug)
        {
            Debug.Log(string.Concat(new object[] { "registered ", key, " ->", num }));
        }
    }

    private void Start()
    {
        this.animator = base.GetComponent<Animator>();
        if (this.Fsm != null)
        {
            string layerName = this.animator.GetLayerName(this.LayerIndex);
            this.fsmStateLUT = new Dictionary<int, FsmState>();
            foreach (FsmState state in this.Fsm.Fsm.States)
            {
                string name = state.Name;
                this.RegisterHash(state.Name, state);
                if (!name.StartsWith(layerName + "."))
                {
                    this.RegisterHash(layerName + "." + state.Name, state);
                }
            }
        }
    }

    private void SwitchState(HutongGames.PlayMaker.Fsm fsm, FsmState state)
    {
        MethodInfo method = fsm.GetType().GetMethod("SwitchState", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null)
        {
            object[] parameters = new object[] { state };
            method.Invoke(fsm, parameters);
        }
    }

    public void Synchronize()
    {
        if ((this.animator != null) && (this.Fsm != null))
        {
            bool flag = false;
            if (this.animator.IsInTransition(this.LayerIndex))
            {
                int fullPathHash = this.animator.GetAnimatorTransitionInfo(this.LayerIndex).fullPathHash;
                int userNameHash = this.animator.GetAnimatorTransitionInfo(this.LayerIndex).userNameHash;
                if (this.lastTransition != fullPathHash)
                {
                    if (this.debug)
                    {
                        Debug.Log("is in transition");
                    }
                    if (this.fsmStateLUT.ContainsKey(userNameHash))
                    {
                        FsmState state = this.fsmStateLUT[userNameHash];
                        if (this.Fsm.Fsm.ActiveState != state)
                        {
                            this.SwitchState(this.Fsm.Fsm, state);
                            flag = true;
                        }
                    }
                    if (!flag && this.fsmStateLUT.ContainsKey(fullPathHash))
                    {
                        FsmState state2 = this.fsmStateLUT[fullPathHash];
                        if (this.Fsm.Fsm.ActiveState != state2)
                        {
                            this.SwitchState(this.Fsm.Fsm, state2);
                            flag = true;
                        }
                    }
                    if (!flag && this.debug)
                    {
                        Debug.LogWarning("Fsm is missing animator transition name or username for hash:" + fullPathHash);
                    }
                    this.lastTransition = fullPathHash;
                }
            }
            if (!flag)
            {
                int key = this.animator.GetCurrentAnimatorStateInfo(this.LayerIndex).fullPathHash;
                if (this.lastState != key)
                {
                    if (this.debug)
                    {
                        Debug.Log("Net state switch");
                    }
                    if (this.fsmStateLUT.ContainsKey(key))
                    {
                        FsmState state3 = this.fsmStateLUT[key];
                        if (this.Fsm.Fsm.ActiveState != state3)
                        {
                            this.SwitchState(this.Fsm.Fsm, state3);
                        }
                    }
                    else if (this.debug)
                    {
                        Debug.LogWarning("Fsm is missing animator state hash:" + key);
                    }
                    this.lastState = key;
                }
            }
        }
    }

    private void Update()
    {
        if (this.EveryFrame)
        {
            this.Synchronize();
        }
    }
}

