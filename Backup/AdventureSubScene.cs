using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureSubScene : MonoBehaviour
{
    private bool m_IsLoaded;
    [CustomEditField(Sections="Bounds Settings")]
    public Vector3_MobileOverride m_SubSceneBounds;
    private List<SubSceneTransitionFinished> m_SubSceneTransitionListeners = new List<SubSceneTransitionFinished>();
    [CustomEditField(Sections="Animation Settings")]
    public float m_TransitionAnimationTime = 1f;

    public void AddSubSceneTransitionFinishedListener(SubSceneTransitionFinished dlg)
    {
        this.m_SubSceneTransitionListeners.Add(dlg);
    }

    private void FireSubSceneTransitionFinishedEvent()
    {
        foreach (SubSceneTransitionFinished finished in this.m_SubSceneTransitionListeners.ToArray())
        {
            finished();
        }
    }

    public bool IsLoaded()
    {
        return this.m_IsLoaded;
    }

    public void NotifyTransitionComplete()
    {
        this.FireSubSceneTransitionFinishedEvent();
    }

    public void RemoveSubSceneTransitionFinishedListener(SubSceneTransitionFinished dlg)
    {
        this.m_SubSceneTransitionListeners.Remove(dlg);
    }

    public void SetIsLoaded(bool loaded)
    {
        this.m_IsLoaded = loaded;
    }

    public delegate void SubSceneTransitionFinished();
}

