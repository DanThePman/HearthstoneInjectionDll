using System;
using UnityEngine;

public class TargetAnimUtils2 : MonoBehaviour
{
    public GameObject m_Target;

    public void ActivateHierarchy2()
    {
        this.m_Target.SetActive(true);
    }

    public void DeactivateHierarchy2()
    {
        this.m_Target.SetActive(false);
    }

    public void DestroyHierarchy2()
    {
        UnityEngine.Object.Destroy(this.m_Target);
    }

    public void FadeIn2(float FadeSec)
    {
        iTween.FadeTo(this.m_Target, 1f, FadeSec);
    }

    public void FadeOut2(float FadeSec)
    {
        iTween.FadeTo(this.m_Target, 0f, FadeSec);
    }

    public void KillParticlesInChildren2()
    {
        UnityEngine.Particle[] particleArray = new UnityEngine.Particle[0];
        foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
        {
            emitter.emit = false;
            emitter.particles = particleArray;
        }
    }

    public void PlayAnimation2()
    {
        if (this.m_Target.GetComponent<Animation>() != null)
        {
            this.m_Target.GetComponent<Animation>().Play();
        }
    }

    public void PlayAnimationsInChildren2()
    {
        foreach (Animation animation in this.m_Target.GetComponentsInChildren<Animation>())
        {
            animation.Play();
        }
    }

    public void PlayDefaultSound2()
    {
        if (this.m_Target.GetComponent<AudioSource>() == null)
        {
            Debug.LogError(string.Format("TargetAnimUtils2.PlayDefaultSound() - Tried to play the AudioSource on {0} but it has no AudioSource. You need an AudioSource to use this function.", this.m_Target));
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

    public void PlayNewParticles2()
    {
        this.m_Target.GetComponent<ParticleSystem>().Play();
    }

    public void PlayParticles2()
    {
        if (this.m_Target.GetComponent<ParticleEmitter>() != null)
        {
            this.m_Target.GetComponent<ParticleEmitter>().emit = true;
        }
    }

    public void PlayParticlesInChildren2()
    {
        foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
        {
            emitter.emit = true;
        }
    }

    public void PlaySound2(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError(string.Format("TargetAnimUtils2.PlayDefaultSound() - No clip was given when trying to play the AudioSource on {0}. You need a clip to use this function.", this.m_Target));
        }
        else if (this.m_Target.GetComponent<AudioSource>() == null)
        {
            Debug.LogError(string.Format("TargetAnimUtils2.PlayDefaultSound() - Tried to play clip {0} on {1} but it has no AudioSource. You need an AudioSource to use this function.", clip, this.m_Target));
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

    public void PrintLog2(string message)
    {
        Debug.Log(message);
    }

    public void PrintLogError2(string message)
    {
        Debug.LogError(message);
    }

    public void PrintLogWarning2(string message)
    {
        Debug.LogWarning(message);
    }

    public void SetAlphaHierarchy2(float alpha)
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

    public void StopAnimation2()
    {
        if (this.m_Target.GetComponent<Animation>() != null)
        {
            this.m_Target.GetComponent<Animation>().Stop();
        }
    }

    public void StopAnimationsInChildren2()
    {
        foreach (Animation animation in this.m_Target.GetComponentsInChildren<Animation>())
        {
            animation.Stop();
        }
    }

    public void StopNewParticles2()
    {
        this.m_Target.GetComponent<ParticleSystem>().Stop();
    }

    public void StopParticles2()
    {
        if (this.m_Target.GetComponent<ParticleEmitter>() != null)
        {
            this.m_Target.GetComponent<ParticleEmitter>().emit = false;
        }
    }

    public void StopParticlesInChildren2()
    {
        foreach (ParticleEmitter emitter in this.m_Target.GetComponentsInChildren<ParticleEmitter>())
        {
            emitter.emit = false;
        }
    }
}

