using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DragCardSoundEffects : MonoBehaviour
{
    private const float AIR_SOUND_MAX_VOLUME = 0.5f;
    private const float AIR_SOUND_MOVEMENT_THRESHOLD = 0.92f;
    private const float AIR_SOUND_VOLUME_SPEED = 0.4f;
    private const float AIR_SOUND_VOLUME_VELOCITY_SCALE = 0.5f;
    private const string CARD_MOTION_LOOP_AIR_SOUND = "card_motion_loop_air";
    private const string CARD_MOTION_LOOP_MAGICAL_SOUND = "card_motion_loop_magical";
    private const float DISABLE_VOLUME_FADE_OUT_TIME = 0.2f;
    private Actor m_Actor;
    private bool m_AirSoundLoading;
    private AudioSource m_AirSoundLoop;
    private float m_AirVelocity;
    private float m_AirVolume;
    private Card m_Card;
    private bool m_Disabled;
    private bool m_FadingOut;
    private bool m_MagicalSoundLoading;
    private AudioSource m_MagicalSoundLoop;
    private float m_MagicalVelocity;
    private float m_MagicalVolume;
    private Vector3 m_PreviousPosition;
    private const float MAGICAL_SOUND_FADE_IN_TIME = 0.5f;
    private const float MAGICAL_SOUND_VOLUME = 0.15f;

    private void Awake()
    {
        this.m_PreviousPosition = base.transform.position;
    }

    public void Disable()
    {
        this.m_Disabled = true;
        if (base.enabled && !this.m_FadingOut)
        {
            base.StartCoroutine("FadeOutSound");
        }
    }

    [DebuggerHidden]
    private IEnumerator FadeOutSound()
    {
        return new <FadeOutSound>c__Iterator27E { <>f__this = this };
    }

    private void LoadAirSound()
    {
        SoundManager.LoadedCallback callback = delegate (AudioSource source, object userData) {
            if (source != null)
            {
                this.m_AirSoundLoading = false;
                this.m_AirSoundLoop = source;
                if (this.m_Disabled || !base.enabled)
                {
                    SoundManager.Get().Stop(this.m_AirSoundLoop);
                }
            }
        };
        SoundManager.Get().LoadAndPlay("card_motion_loop_air", base.gameObject, 0f, callback);
    }

    private void LoadMagicalSound()
    {
        SoundManager.LoadedCallback callback = delegate (AudioSource source, object userData) {
            if (source != null)
            {
                this.m_MagicalSoundLoading = false;
                if (this.m_MagicalSoundLoop != null)
                {
                    SoundManager.Get().Stop(this.m_MagicalSoundLoop);
                }
                this.m_MagicalSoundLoop = source;
                if (this.m_Disabled || !base.enabled)
                {
                    SoundManager.Get().Stop(this.m_MagicalSoundLoop);
                }
            }
        };
        SoundManager.Get().LoadAndPlay("card_motion_loop_magical", base.gameObject, 0f, callback);
    }

    private void OnDestroy()
    {
        base.StopCoroutine("FadeOutSound");
        this.StopSound();
    }

    private void OnDisable()
    {
        this.StopSound();
    }

    public void Restart()
    {
        base.enabled = true;
        this.m_Disabled = false;
    }

    private void StopSound()
    {
        this.m_FadingOut = false;
        this.m_MagicalVolume = 0f;
        this.m_AirVolume = 0f;
        this.m_AirVelocity = 0f;
        if (this.m_AirSoundLoop != null)
        {
            SoundManager.Get().Stop(this.m_AirSoundLoop);
        }
        if (this.m_MagicalSoundLoop != null)
        {
            SoundManager.Get().Stop(this.m_MagicalSoundLoop);
        }
    }

    private void Update()
    {
        if ((!this.m_Disabled && base.enabled) && (!this.m_AirSoundLoading && !this.m_MagicalSoundLoading))
        {
            if (this.m_AirSoundLoop == null)
            {
                this.LoadAirSound();
            }
            else if (this.m_MagicalSoundLoop == null)
            {
                this.LoadMagicalSound();
            }
            else if ((this.m_AirSoundLoop != null) && (this.m_MagicalSoundLoop != null))
            {
                if (this.m_Card == null)
                {
                    this.m_Card = base.GetComponent<Card>();
                }
                if (this.m_Card == null)
                {
                    this.Disable();
                }
                else
                {
                    if (this.m_Actor == null)
                    {
                        this.m_Actor = this.m_Card.GetActor();
                    }
                    if (this.m_Actor == null)
                    {
                        this.Disable();
                    }
                    else if (!this.m_Actor.IsShown())
                    {
                        Log.Kyle.Print(string.Format("Something went wrong in DragCardSoundEffects on {0} and we are killing a stuck sound!", this.m_Card.gameObject.name), new object[0]);
                        this.Disable();
                    }
                    else
                    {
                        this.m_MagicalSoundLoop.transform.position = base.transform.position;
                        this.m_AirSoundLoop.transform.position = base.transform.position;
                        if (this.m_MagicalVolume < 0.15f)
                        {
                            this.m_MagicalVolume = Mathf.SmoothDamp(this.m_MagicalVolume, 0.15f, ref this.m_MagicalVelocity, 0.5f);
                            SoundManager.Get().SetVolume(this.m_MagicalSoundLoop, this.m_MagicalVolume);
                        }
                        else if (this.m_MagicalVolume > 0.15f)
                        {
                            this.m_MagicalVolume = 0.15f;
                            SoundManager.Get().SetVolume(this.m_MagicalSoundLoop, this.m_MagicalVolume);
                        }
                        Vector3 position = base.transform.position;
                        Vector3 vector2 = position - this.m_PreviousPosition;
                        this.m_AirVolume = Mathf.SmoothDamp(this.m_AirVolume, Mathf.Log((vector2.magnitude * 0.5f) + 0.92f), ref this.m_AirVelocity, 0.04f, 1f);
                        SoundManager.Get().SetVolume(this.m_AirSoundLoop, Mathf.Clamp(this.m_AirVolume, 0f, 0.5f));
                        SoundManager.Get().SetVolume(this.m_MagicalSoundLoop, this.m_MagicalVolume);
                        this.m_PreviousPosition = position;
                    }
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <FadeOutSound>c__Iterator27E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DragCardSoundEffects <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    if ((this.<>f__this.m_AirSoundLoop != null) && (this.<>f__this.m_MagicalSoundLoop != null))
                    {
                        this.<>f__this.m_FadingOut = true;
                        break;
                    }
                    this.<>f__this.StopSound();
                    goto Label_017D;

                case 1:
                    break;

                default:
                    goto Label_017D;
            }
            while ((this.<>f__this.m_AirVolume > 0f) && (this.<>f__this.m_MagicalVolume > 0f))
            {
                this.<>f__this.m_AirVolume = Mathf.SmoothDamp(this.<>f__this.m_AirVolume, 0f, ref this.<>f__this.m_AirVelocity, 0.2f);
                this.<>f__this.m_MagicalVolume = Mathf.SmoothDamp(this.<>f__this.m_MagicalVolume, 0f, ref this.<>f__this.m_MagicalVelocity, 0.2f);
                SoundManager.Get().SetVolume(this.<>f__this.m_AirSoundLoop, Mathf.Clamp(this.<>f__this.m_AirVolume, 0f, 1f));
                SoundManager.Get().SetVolume(this.<>f__this.m_MagicalSoundLoop, this.<>f__this.m_MagicalVolume);
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.m_FadingOut = false;
            this.<>f__this.StopSound();
            goto Label_017D;
            this.$PC = -1;
        Label_017D:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

