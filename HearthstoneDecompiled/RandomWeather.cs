using System;
using UnityEngine;

public class RandomWeather : MonoBehaviour
{
    private bool m_active;
    private ParticleSystem[] m_particleSystems;
    private float m_runEndTime;
    public float m_StartDelayMaxMinutes = 10f;
    public float m_StartDelayMinMinutes = 1f;
    private float m_startTime;
    public float m_WeatherMaxMinutes = 5f;
    public float m_WeatherMinMinutes = 2f;

    private void Start()
    {
        this.m_particleSystems = base.GetComponentsInChildren<ParticleSystem>();
        this.m_startTime = UnityEngine.Random.Range((float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_StartDelayMinMinutes * 60f)), (float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_StartDelayMaxMinutes * 60f)));
    }

    [ContextMenu("Start Weather")]
    private void StartWeather()
    {
        this.m_active = true;
        this.m_runEndTime = UnityEngine.Random.Range((float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_WeatherMinMinutes * 60f)), (float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_WeatherMaxMinutes * 60f)));
        foreach (ParticleSystem system in this.m_particleSystems)
        {
            if (system != null)
            {
                system.Play();
            }
        }
    }

    [ContextMenu("Stop Weather")]
    private void StopWeather()
    {
        this.m_active = false;
        foreach (ParticleSystem system in this.m_particleSystems)
        {
            if (system != null)
            {
                system.Stop();
            }
        }
    }

    private void Update()
    {
        if (this.m_active)
        {
            if (UnityEngine.Time.timeSinceLevelLoad > this.m_runEndTime)
            {
                this.StopWeather();
            }
        }
        else if (UnityEngine.Time.timeSinceLevelLoad > this.m_startTime)
        {
            this.StartWeather();
            this.m_startTime = UnityEngine.Random.Range((float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_StartDelayMinMinutes * 60f)), (float) (UnityEngine.Time.timeSinceLevelLoad + (this.m_StartDelayMaxMinutes * 60f)));
        }
    }
}

