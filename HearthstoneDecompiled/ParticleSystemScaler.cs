using System;
using UnityEngine;

public class ParticleSystemScaler : MonoBehaviour
{
    private Map<ParticleSystem, ParticleSystemSizes> m_initialValues = new Map<ParticleSystem, ParticleSystemSizes>();
    private float m_unitMagnitude;
    public GameObject ObjectToInherit;
    public float ParticleSystemScale = 1f;

    private void Awake()
    {
        this.m_unitMagnitude = Vector3.one.magnitude;
    }

    private void ScaleParticleSystems(float scaleFactor)
    {
        foreach (ParticleSystem system in base.GetComponentsInChildren<ParticleSystem>())
        {
            system.startSpeed *= scaleFactor;
            system.startSize *= scaleFactor;
            system.gravityModifier *= scaleFactor;
        }
    }

    private void Update()
    {
        if (this.ObjectToInherit != null)
        {
            this.ParticleSystemScale = this.ObjectToInherit.transform.lossyScale.magnitude / this.m_unitMagnitude;
        }
        foreach (ParticleSystem system in base.GetComponentsInChildren<ParticleSystem>())
        {
            if (!this.m_initialValues.ContainsKey(system))
            {
                this.m_initialValues.Add(system, new ParticleSystemSizes());
                this.m_initialValues[system].startSpeed = system.startSpeed;
                this.m_initialValues[system].startSize = system.startSize;
                this.m_initialValues[system].gravityModifier = system.gravityModifier;
            }
            system.startSize = this.m_initialValues[system].startSize * this.ParticleSystemScale;
            system.startSpeed = this.m_initialValues[system].startSpeed * this.ParticleSystemScale;
            system.gravityModifier = this.m_initialValues[system].gravityModifier * this.ParticleSystemScale;
        }
    }
}

