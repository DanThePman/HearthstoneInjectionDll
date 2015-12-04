using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticlePlaybackSpeed : MonoBehaviour
{
    private Map<ParticleSystem, float> m_OrgPlaybackSpeed;
    public float m_ParticlePlaybackSpeed = 1f;
    private List<ParticleSystem> m_ParticleSystems;
    private float m_PreviousPlaybackSpeed = 1f;
    public bool m_RestoreSpeedOnDisable = true;

    private void Init()
    {
        if (this.m_ParticleSystems == null)
        {
            this.m_ParticleSystems = new List<ParticleSystem>();
        }
        else
        {
            this.m_ParticleSystems.Clear();
        }
        if (this.m_OrgPlaybackSpeed == null)
        {
            this.m_OrgPlaybackSpeed = new Map<ParticleSystem, float>();
        }
        else
        {
            this.m_OrgPlaybackSpeed.Clear();
        }
        foreach (ParticleSystem system in base.gameObject.GetComponentsInChildren<ParticleSystem>())
        {
            this.m_OrgPlaybackSpeed.Add(system, system.playbackSpeed);
            this.m_ParticleSystems.Add(system);
        }
    }

    private void OnDisable()
    {
        if (this.m_RestoreSpeedOnDisable && (this.m_OrgPlaybackSpeed != null))
        {
            foreach (KeyValuePair<ParticleSystem, float> pair in this.m_OrgPlaybackSpeed)
            {
                pair.Key.playbackSpeed = pair.Value;
            }
        }
        this.m_PreviousPlaybackSpeed = -1E+07f;
        this.m_ParticleSystems.Clear();
        this.m_OrgPlaybackSpeed.Clear();
    }

    private void OnEnable()
    {
        this.Init();
    }

    private void Start()
    {
        this.Init();
    }

    private void Update()
    {
        if (this.m_ParticlePlaybackSpeed != this.m_PreviousPlaybackSpeed)
        {
            this.m_PreviousPlaybackSpeed = this.m_ParticlePlaybackSpeed;
            foreach (ParticleSystem system in this.m_ParticleSystems)
            {
                system.playbackSpeed = this.m_ParticlePlaybackSpeed;
            }
        }
    }
}

