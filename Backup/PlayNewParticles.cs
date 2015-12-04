using System;
using UnityEngine;

public class PlayNewParticles : MonoBehaviour
{
    public GameObject m_Target;
    public GameObject m_Target2;
    public GameObject m_Target3;
    public GameObject m_Target4;

    public void PlayNewParticles3()
    {
        if (this.m_Target != null)
        {
            this.m_Target.GetComponent<ParticleSystem>().Play();
        }
    }

    public void PlayNewParticles3andChilds()
    {
        if (this.m_Target2 != null)
        {
            this.m_Target2.GetComponent<ParticleSystem>().Play(true);
        }
    }

    public void PlayNewParticles3andChilds2()
    {
        if (this.m_Target3 != null)
        {
            this.m_Target3.GetComponent<ParticleSystem>().Play(true);
        }
    }

    public void PlayNewParticles3andChilds3()
    {
        if (this.m_Target4 != null)
        {
            this.m_Target4.GetComponent<ParticleSystem>().Play(true);
        }
    }

    public void StopNewParticles3()
    {
        if (this.m_Target != null)
        {
            this.m_Target.GetComponent<ParticleSystem>().Stop();
        }
    }

    public void StopNewParticles3andChilds()
    {
        if (this.m_Target2 != null)
        {
            this.m_Target2.GetComponent<ParticleSystem>().Stop(true);
        }
    }

    public void StopNewParticles3andChilds2()
    {
        if (this.m_Target3 != null)
        {
            this.m_Target3.GetComponent<ParticleSystem>().Stop(true);
        }
    }

    public void StopNewParticles3andChilds3()
    {
        if (this.m_Target4 != null)
        {
            this.m_Target4.GetComponent<ParticleSystem>().Stop(true);
        }
    }
}

