using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    private const string CINEMATIC_FILE_NAME = "Cinematic";
    private bool m_isMovieAudioLoaded;
    private bool m_isMovieLoaded;
    private bool m_isPlaying;
    private AudioClip m_MovieAudio;
    private MovieTexture m_MovieTexture;
    private SoundDucker m_soundDucker;
    private const float MOVIE_LOAD_TIMEOUT = 10f;
    private PlayOptions options;

    private void AudioLoaded(string name, GameObject obj, object callbackData)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError("Failed to load Cinematic Audio Track!");
        }
        else
        {
            this.m_isMovieAudioLoaded = true;
            this.m_MovieAudio = obj.GetComponent<AudioSource>().clip;
        }
    }

    private void Awake()
    {
        this.m_soundDucker = base.gameObject.AddComponent<SoundDucker>();
        this.m_soundDucker.m_GlobalDuckDef = new SoundDuckedCategoryDef();
        this.m_soundDucker.m_GlobalDuckDef.m_Volume = 0f;
        this.m_soundDucker.m_GlobalDuckDef.m_RestoreSec = 0f;
        this.m_soundDucker.m_GlobalDuckDef.m_BeginSec = 0f;
    }

    public bool isPlaying()
    {
        return this.m_isPlaying;
    }

    private void MovieLoaded(string name, UnityEngine.Object obj, object callbackData)
    {
        if (obj == null)
        {
            UnityEngine.Debug.LogError("Failed to load Cinematic movie!");
        }
        else
        {
            this.m_isMovieLoaded = true;
            this.m_MovieTexture = obj as MovieTexture;
        }
    }

    private void OnGUI()
    {
        if (this.m_isPlaying)
        {
            GUI.DrawTexture(new Rect(0f, 0f, (float) Screen.width, (float) Screen.height), this.m_MovieTexture, ScaleMode.ScaleToFit, false, 0f);
        }
    }

    public void Play(MovieCallback callback)
    {
        this.options = new PlayOptions();
        this.options.callback = callback;
        string methodName = string.Empty;
        methodName = "PlayPC";
        if (methodName != string.Empty)
        {
            base.StartCoroutine(methodName, this.options);
        }
    }

    private static void PlayCinematic(string gameObjectName, string localeName)
    {
    }

    [DebuggerHidden]
    private IEnumerator PlayPC(PlayOptions options)
    {
        return new <PlayPC>c__Iterator273 { options = options, <$>options = options, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <PlayPC>c__Iterator273 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Cinematic.PlayOptions <$>options;
        internal Cinematic <>f__this;
        internal SoundPlayClipArgs <args>__2;
        internal GameObject <cameraGO>__4;
        internal Camera <CinematicCamera>__5;
        internal AudioClip <movieAudioClip>__1;
        internal AudioSource <movieSound>__3;
        internal float <timeOut>__0;
        internal Cinematic.PlayOptions options;

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
                    AssetLoader.Get().LoadMovie("Cinematic", new AssetLoader.ObjectCallback(this.<>f__this.MovieLoaded), null, false);
                    this.<timeOut>__0 = UnityEngine.Time.time + 10f;
                    break;

                case 1:
                    break;

                case 2:
                case 3:
                    if (!this.<>f__this.m_MovieTexture.isReadyToPlay)
                    {
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_0467;
                    }
                    Options.Get().SetBool(Option.HAS_SEEN_CINEMATIC, true);
                    BnetBar.Get().gameObject.SetActive(false);
                    this.<movieAudioClip>__1 = this.<>f__this.m_MovieTexture.audioClip;
                    switch (Localization.GetLocale())
                    {
                        case Locale.enUS:
                        case Locale.enGB:
                        case Locale.zhTW:
                        case Locale.zhCN:
                            goto Label_01F8;
                    }
                    AssetLoader.Get().LoadSound("CinematicAudio", new AssetLoader.GameObjectCallback(this.<>f__this.AudioLoaded), null, false, null);
                    goto Label_019D;

                case 4:
                    goto Label_019D;

                case 5:
                    goto Label_03A9;

                default:
                    goto Label_0465;
            }
            if (!this.<>f__this.m_isMovieLoaded && (UnityEngine.Time.time < this.<timeOut>__0))
            {
                this.$current = null;
                this.$PC = 1;
            }
            else
            {
                if (this.<>f__this.m_MovieTexture == null)
                {
                    UnityEngine.Debug.LogWarning("m_MovieTexture is null!");
                    goto Label_0465;
                }
                this.$current = null;
                this.$PC = 2;
            }
            goto Label_0467;
        Label_019D:
            while (!this.<>f__this.m_isMovieAudioLoaded && (UnityEngine.Time.time < this.<timeOut>__0))
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0467;
            }
            if (this.<>f__this.m_MovieAudio == null)
            {
                UnityEngine.Debug.LogWarning("m_MovieAudio is null!");
                goto Label_0465;
            }
            this.<movieAudioClip>__1 = this.<>f__this.m_MovieAudio;
        Label_01F8:
            this.<>f__this.m_isPlaying = true;
            this.<>f__this.m_MovieTexture.filterMode = FilterMode.Bilinear;
            this.<>f__this.m_MovieTexture.loop = false;
            PegCursor.Get().Hide();
            this.<>f__this.m_MovieTexture.Play();
            this.<>f__this.m_soundDucker.StartDucking();
            this.<args>__2 = new SoundPlayClipArgs();
            this.<args>__2.m_clip = this.<movieAudioClip>__1;
            this.<args>__2.m_volume = 1f;
            this.<args>__2.m_pitch = 1f;
            this.<args>__2.m_category = 1;
            this.<args>__2.m_parentObject = this.<>f__this.gameObject;
            this.<movieSound>__3 = SoundManager.Get().PlayClip(this.<args>__2);
            SoundManager.Get().Set3d(this.<movieSound>__3, false);
            SoundManager.Get().SetIgnoreDucking(this.<movieSound>__3, true);
            this.<cameraGO>__4 = new GameObject();
            this.<cameraGO>__4.transform.position = new Vector3(-9997.9f, -9998.9f, -9999.9f);
            this.<CinematicCamera>__5 = this.<cameraGO>__4.AddComponent<Camera>();
            this.<CinematicCamera>__5.name = "Cinematic Background Camera";
            this.<CinematicCamera>__5.clearFlags = CameraClearFlags.Color;
            this.<CinematicCamera>__5.backgroundColor = Color.black;
            this.<CinematicCamera>__5.depth = 1000f;
            this.<CinematicCamera>__5.nearClipPlane = 0.01f;
            this.<CinematicCamera>__5.farClipPlane = 0.02f;
        Label_03A9:
            while (this.<>f__this.m_MovieTexture.isPlaying && !Input.anyKey)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_0467;
            }
            if (this.<>f__this.m_MovieTexture.isPlaying)
            {
                SoundManager.Get().Stop(this.<movieSound>__3);
            }
            this.<>f__this.m_MovieTexture.Stop();
            this.<>f__this.m_soundDucker.StopDucking();
            PegCursor.Get().Show();
            BnetBar.Get().gameObject.SetActive(true);
            SocialToastMgr.Get().Reset();
            UnityEngine.Object.Destroy(this.<CinematicCamera>__5);
            this.<>f__this.m_isPlaying = false;
            this.options.callback();
            goto Label_0465;
            this.$PC = -1;
        Label_0465:
            return false;
        Label_0467:
            return true;
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

    public delegate void MovieCallback();

    private class PlayOptions
    {
        public Cinematic.MovieCallback callback;
    }
}

