using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetListAnimUtils : MonoBehaviour
{
    public List<GameObject> m_TargetList;

    public void ActivateHierarchyList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.SetActive(true);
            }
        }
    }

    public void DeactivateHierarchyList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.SetActive(false);
            }
        }
    }

    public void DestroyHierarchyList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            UnityEngine.Object.Destroy(obj2);
        }
    }

    public void FadeInList(float FadeSec)
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            iTween.FadeTo(obj2, 1f, FadeSec);
        }
    }

    public void FadeOutList(float FadeSec)
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            iTween.FadeTo(obj2, 0f, FadeSec);
        }
    }

    public void KillParticlesList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.GetComponent<ParticleEmitter>().particles = new UnityEngine.Particle[0];
            }
        }
    }

    public void KillParticlesListInChildren()
    {
        UnityEngine.Particle[] particleArray = new UnityEngine.Particle[0];
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (ParticleEmitter emitter in obj2.GetComponentsInChildren<ParticleEmitter>())
                {
                    emitter.emit = false;
                    emitter.particles = particleArray;
                }
            }
        }
    }

    public void PlayAnimationList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.GetComponent<Animation>().Play();
            }
        }
    }

    public void PlayAnimationListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (Animation animation in obj2.GetComponentsInChildren<Animation>())
                {
                    animation.Play();
                }
            }
        }
    }

    public void PlayNewParticlesListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (ParticleSystem system in obj2.GetComponentsInChildren<ParticleSystem>())
                {
                    system.Play();
                }
            }
        }
    }

    public void PlayParticlesList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.GetComponent<ParticleEmitter>().emit = true;
            }
        }
    }

    public void PlayParticlesListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (ParticleEmitter emitter in obj2.GetComponentsInChildren<ParticleEmitter>())
                {
                    emitter.emit = true;
                }
            }
        }
    }

    public void SetAlphaHierarchyList(float alpha)
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (Renderer renderer in obj2.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.material.HasProperty("_Color"))
                    {
                        Color color = renderer.material.color;
                        color.a = alpha;
                        renderer.material.color = color;
                    }
                }
            }
        }
    }

    public void StopAnimationList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.GetComponent<Animation>().Stop();
            }
        }
    }

    public void StopAnimationListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (Animation animation in obj2.GetComponentsInChildren<Animation>())
                {
                    animation.Stop();
                }
            }
        }
    }

    public void StopNewParticlesListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (ParticleSystem system in obj2.GetComponentsInChildren<ParticleSystem>())
                {
                    system.Stop();
                }
            }
        }
    }

    public void StopParticlesList()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                obj2.GetComponent<ParticleEmitter>().emit = false;
            }
        }
    }

    public void StopParticlesListInChildren()
    {
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                foreach (ParticleEmitter emitter in obj2.GetComponentsInChildren<ParticleEmitter>())
                {
                    emitter.emit = false;
                }
            }
        }
    }
}

