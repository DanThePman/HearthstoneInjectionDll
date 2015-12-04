using System;
using UnityEngine;

public class TargetAnimUtils : MonoBehaviour
{
    public GameObject m_Target;

    public void ActivateHierarchy()
    {
        this.m_Target.SetActive(true);
    }

    private void Awake()
    {
        if (this.m_Target == null)
        {
            base.enabled = false;
        }
    }

    public void DeactivateHierarchy()
    {
        if (this.m_Target != null)
        {
            this.m_Target.SetActive(false);
        }
    }

    public void DestroyHierarchy()
    {
        if (this.m_Target != null)
        {
            UnityEngine.Object.Destroy(this.m_Target);
        }
    }

    public void FadeIn(float FadeSec)
    {
        if (this.m_Target != null)
        {
            iTween.FadeTo(this.m_Target, 1f, FadeSec);
        }
    }

    public void FadeOut(float FadeSec)
    {
        if (this.m_Target != null)
        {
            iTween.FadeTo(this.m_Target, 0f, FadeSec);
        }
    }

    public void KillParticlesInChildren()
    {
        if (this.m_Target != null)
        {
            UnityEngine.Particle[] particleArray = new UnityEngine.Particle[0];
            foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
            {
                emitter.emit = false;
                emitter.particles = particleArray;
            }
        }
    }

    public void PlayAnimation()
    {
        if ((this.m_Target != null) && (this.m_Target.GetComponent<Animation>() != null))
        {
            this.m_Target.GetComponent<Animation>().Play();
        }
    }

    public void PlayAnimationsInChildren()
    {
        if (this.m_Target != null)
        {
            foreach (Animation animation in this.m_Target.GetComponentsInChildren<Animation>())
            {
                animation.Play();
            }
        }
    }

    public void PlayDefaultSound()
    {
        if (this.m_Target != null)
        {
            if (this.m_Target.GetComponent<AudioSource>() == null)
            {
                Debug.LogError(string.Format("TargetAnimUtils.PlayDefaultSound() - Tried to play the AudioSource on {0} but it has no AudioSource. You need an AudioSource to use this function.", this.m_Target));
            }
            else if (SoundManager.Get() == null)
            {
                this.m_Target.GetComponent<AudioSource>().Play();
            }
            else
            {
                SoundManager.Get().Play(this.m_Target.GetComponent<AudioSource>());
            }
        }
    }

    public void PlayNewParticles()
    {
        this.m_Target.GetComponent<ParticleSystem>().Play();
    }

    public void PlayParticles()
    {
        if ((this.m_Target != null) && (this.m_Target.GetComponent<ParticleEmitter>() != null))
        {
            this.m_Target.GetComponent<ParticleEmitter>().emit = true;
        }
    }

    public void PlayParticlesInChildren()
    {
        if (this.m_Target != null)
        {
            foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
            {
                emitter.emit = true;
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (this.m_Target != null)
        {
            if (clip == null)
            {
                Debug.LogError(string.Format("TargetAnimUtils.PlayDefaultSound() - No clip was given when trying to play the AudioSource on {0}. You need a clip to use this function.", this.m_Target));
            }
            else if (this.m_Target.GetComponent<AudioSource>() == null)
            {
                Debug.LogError(string.Format("TargetAnimUtils.PlayDefaultSound() - Tried to play clip {0} on {1} but it has no AudioSource. You need an AudioSource to use this function.", clip, this.m_Target));
            }
            else if (SoundManager.Get() == null)
            {
                this.m_Target.GetComponent<AudioSource>().PlayOneShot(clip);
            }
            else
            {
                SoundManager.Get().PlayOneShot(this.m_Target.GetComponent<AudioSource>(), clip, 1f);
            }
        }
    }

    public void PrintLog(string message)
    {
        Debug.Log(message);
    }

    public void PrintLogError(string message)
    {
        Debug.LogError(message);
    }

    public void PrintLogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void SetAlphaHierarchy(float alpha)
    {
        if (this.m_Target != null)
        {
            foreach (Renderer renderer in this.m_Target.GetComponentsInChildren<Renderer>())
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

    public void StopAnimation()
    {
        if ((this.m_Target != null) && (this.m_Target.GetComponent<Animation>() != null))
        {
            this.m_Target.GetComponent<Animation>().Stop();
        }
    }

    public void StopAnimationsInChildren()
    {
        if (this.m_Target != null)
        {
            foreach (Animation animation in this.m_Target.GetComponentsInChildren<Animation>())
            {
                animation.Stop();
            }
        }
    }

    public void StopNewParticles()
    {
        if (this.m_Target != null)
        {
            this.m_Target.GetComponent<ParticleSystem>().Stop();
        }
    }

    public void StopParticles()
    {
        if ((this.m_Target != null) && (this.m_Target.GetComponent<ParticleEmitter>() != null))
        {
            this.m_Target.GetComponent<ParticleEmitter>().emit = false;
        }
    }

    public void StopParticlesInChildren()
    {
        if (this.m_Target != null)
        {
            foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
            {
                emitter.emit = false;
            }
        }
    }
}

