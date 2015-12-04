using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private bool m_ambienceIsAboutToPlay;
    private int m_ambienceTrackIndex;
    private List<MusicTrack> m_ambienceTracks = new List<MusicTrack>();
    private Map<string, BundleInfo> m_bundleInfos = new Map<string, BundleInfo>();
    private SoundConfig m_config;
    private AudioSource m_currentAmbienceTrack;
    private AudioSource m_currentMusicTrack;
    private Map<SoundCategory, List<DuckState>> m_duckStates = new Map<SoundCategory, List<DuckState>>();
    private List<ExtensionMapping> m_extensionMappings = new List<ExtensionMapping>();
    private List<AudioSource> m_fadingTracks = new List<AudioSource>();
    private uint m_frame;
    private List<AudioSource> m_generatedSources = new List<AudioSource>();
    private List<AudioSource> m_inactiveSources = new List<AudioSource>();
    private bool m_musicIsAboutToPlay;
    private int m_musicTrackIndex;
    private List<MusicTrack> m_musicTracks = new List<MusicTrack>();
    private bool m_mute;
    private uint m_nextDuckStateTweenId;
    private int m_nextSourceId = 1;
    private Map<SoundCategory, List<AudioSource>> m_sourcesByCategory = new Map<SoundCategory, List<AudioSource>>();
    private Map<string, List<AudioSource>> m_sourcesByClipName = new Map<string, List<AudioSource>>();
    private static SoundManager s_instance;

    public void AddAmbienceTracks(List<MusicTrack> tracks)
    {
        this.AddTracks(tracks, this.m_ambienceTracks);
    }

    private void AddExtensionMapping(AudioSource source, SourceExtension extension)
    {
        if ((source != null) && (extension != null))
        {
            ExtensionMapping item = new ExtensionMapping {
                Source = source,
                Extension = extension
            };
            this.m_extensionMappings.Add(item);
        }
    }

    public void AddMusicTracks(List<MusicTrack> tracks)
    {
        this.AddTracks(tracks, this.m_musicTracks);
    }

    private void AddTracks(List<MusicTrack> sourceTracks, List<MusicTrack> destTracks)
    {
        foreach (MusicTrack track in sourceTracks)
        {
            destTracks.Add(track);
        }
    }

    private void AnimateBeginningDuckState(DuckState state)
    {
        <AnimateBeginningDuckState>c__AnonStorey389 storey = new <AnimateBeginningDuckState>c__AnonStorey389 {
            state = state,
            <>f__this = this
        };
        string name = string.Format("DuckState Begin id={0}", this.GetNextDuckStateTweenId());
        storey.state.SetTweenName(name);
        storey.duckedDef = storey.state.GetDuckedDef();
        Action<object> action = new Action<object>(storey.<>m__280);
        object[] args = new object[] { 
            "name", name, "time", storey.duckedDef.m_BeginSec, "easeType", storey.duckedDef.m_BeginEaseType, "from", storey.state.GetVolume(), "to", storey.duckedDef.m_Volume, "onupdate", action, "onupdatetarget", base.gameObject, "oncomplete", "OnDuckStateBeginningComplete", 
            "oncompletetarget", base.gameObject, "oncompleteparams", storey.state
         };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(base.gameObject, hashtable);
    }

    private void AnimateRestoringDuckState(DuckState state)
    {
        <AnimateRestoringDuckState>c__AnonStorey38A storeya = new <AnimateRestoringDuckState>c__AnonStorey38A {
            state = state,
            <>f__this = this
        };
        string name = string.Format("DuckState Finish id={0}", this.GetNextDuckStateTweenId());
        storeya.state.SetTweenName(name);
        storeya.duckedDef = storeya.state.GetDuckedDef();
        Action<object> action = new Action<object>(storeya.<>m__281);
        object[] args = new object[] { 
            "name", name, "time", storeya.duckedDef.m_RestoreSec, "easeType", storeya.duckedDef.m_RestoreEaseType, "from", storeya.state.GetVolume(), "to", 1f, "onupdate", action, "onupdatetarget", base.gameObject, "oncomplete", "OnDuckStateRestoringComplete", 
            "oncompletetarget", base.gameObject, "oncompleteparams", storeya.state
         };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(base.gameObject, hashtable);
    }

    private void Awake()
    {
        s_instance = this;
        this.InitializeOptions();
    }

    [Conditional("SOUND_BUNDLE_DEBUG")]
    private void BundlePrint(string format, params object[] args)
    {
        Log.Sound.Print(format, args);
    }

    [Conditional("SOUND_BUNDLE_DEBUG")]
    private void BundleScreenPrint(string format, params object[] args)
    {
        Log.Sound.ScreenPrint(format, args);
    }

    private bool CanPlayClipOnExistingSource(AudioSource source, AudioClip clip)
    {
        if (clip == null)
        {
            return false;
        }
        if ((!this.IsActive(source) || (source.clip != clip)) && this.ProcessClipLimits(clip))
        {
            return false;
        }
        return true;
    }

    [Conditional("SOUND_CATEGORY_DEBUG")]
    private void CategoryPrint(string format, params object[] args)
    {
        Log.Sound.Print(format, args);
    }

    [Conditional("SOUND_CATEGORY_DEBUG")]
    private void CategoryScreenPrint(string format, params object[] args)
    {
        Log.Sound.ScreenPrint(format, args);
    }

    private void ChangeCurrentAmbienceTrack(AudioSource source)
    {
        this.m_currentAmbienceTrack = source;
    }

    private void ChangeCurrentMusicTrack(AudioSource source)
    {
        this.m_currentMusicTrack = source;
    }

    private void ChangeDuckState(DuckState state, DuckMode mode)
    {
        string tweenName = state.GetTweenName();
        if (tweenName != null)
        {
            iTween.StopByName(base.gameObject, tweenName);
        }
        state.SetMode(mode);
        state.SetTweenName(null);
        switch (mode)
        {
            case DuckMode.BEGINNING:
                this.AnimateBeginningDuckState(state);
                break;

            case DuckMode.RESTORING:
                this.AnimateRestoringDuckState(state);
                break;
        }
    }

    private void CleanInactiveSources()
    {
        foreach (AudioSource source in this.m_inactiveSources)
        {
            this.FinishSource(source);
        }
        this.m_inactiveSources.Clear();
    }

    private void CleanUpSourceList(List<AudioSource> sources)
    {
        if (sources != null)
        {
            int index = 0;
            while (index < sources.Count)
            {
                AudioSource source = sources[index];
                if (source == null)
                {
                    sources.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public void Destroy(AudioSource source)
    {
        if (source != null)
        {
            this.FinishSource(source);
        }
    }

    private AudioClip DetermineClipForPlayback(AudioSource source, SoundDef def, AudioClip oneShotClip)
    {
        AudioClip randomClipFromDef = oneShotClip;
        if (randomClipFromDef != null)
        {
            return randomClipFromDef;
        }
        randomClipFromDef = SoundUtils.GetRandomClipFromDef(def);
        if (randomClipFromDef != null)
        {
            return randomClipFromDef;
        }
        randomClipFromDef = source.clip;
        if (randomClipFromDef != null)
        {
            return randomClipFromDef;
        }
        string str = string.Empty;
        if (ApplicationMgr.IsInternal())
        {
            str = " " + DebugUtils.GetHierarchyPathAndType(source);
        }
        object[] messageArgs = new object[] { source, SceneUtils.FindTopParent(source), str };
        Error.AddDevFatal("{0} has no AudioClip. Top-level parent is {1}{2}.", messageArgs);
        return null;
    }

    [Conditional("SOUND_DUCKING_DEBUG")]
    private void DuckingPrint(string format, params object[] args)
    {
        Log.Sound.Print(format, args);
    }

    [Conditional("SOUND_DUCKING_DEBUG")]
    private void DuckingScreenPrint(string format, params object[] args)
    {
        Log.Sound.ScreenPrint(format, args);
    }

    [DebuggerHidden]
    private IEnumerator FadeTrack(AudioSource source, float targetVolume)
    {
        return new <FadeTrack>c__Iterator269 { source = source, targetVolume = targetVolume, <$>source = source, <$>targetVolume = targetVolume, <>f__this = this };
    }

    private void FadeTrackOut(AudioSource source)
    {
        if (!this.IsActive(source))
        {
            this.FinishSource(source);
        }
        else
        {
            base.StartCoroutine(this.FadeTrack(source, 0f));
        }
    }

    private SoundPlaybackLimitClipDef FindClipDefInPlaybackDef(string clipName, SoundPlaybackLimitDef def)
    {
        if (def.m_ClipDefs != null)
        {
            foreach (SoundPlaybackLimitClipDef def2 in def.m_ClipDefs)
            {
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(def2.m_Path);
                if (clipName == fileNameWithoutExtension)
                {
                    return def2;
                }
            }
        }
        return null;
    }

    private SoundDuckingDef FindDuckingDefForCategory(SoundCategory cat)
    {
        if ((this.m_config != null) && (this.m_config.m_DuckingDefs != null))
        {
            foreach (SoundDuckingDef def in this.m_config.m_DuckingDefs)
            {
                if (cat == def.m_TriggerCategory)
                {
                    return def;
                }
            }
        }
        return null;
    }

    private SoundDuckingDef FindDuckingDefForSource(AudioSource source)
    {
        SoundCategory cat = this.GetCategory(source);
        return this.FindDuckingDefForCategory(cat);
    }

    private void FinishDeadGeneratedSource(AudioSource source)
    {
        for (int i = 0; i < this.m_generatedSources.Count; i++)
        {
            AudioSource source2 = this.m_generatedSources[i];
            if (source2 == source)
            {
                this.m_generatedSources.RemoveAt(i);
                return;
            }
        }
    }

    private void FinishGeneratedSource(AudioSource source)
    {
        for (int i = 0; i < this.m_generatedSources.Count; i++)
        {
            AudioSource source2 = this.m_generatedSources[i];
            if (source2 == source)
            {
                UnityEngine.Object.DestroyImmediate(source.gameObject);
                this.m_generatedSources.RemoveAt(i);
                return;
            }
        }
    }

    private void FinishSource(AudioSource source)
    {
        if (this.m_currentMusicTrack == source)
        {
            this.ChangeCurrentMusicTrack(null);
        }
        else if (this.m_currentAmbienceTrack == source)
        {
            this.ChangeCurrentAmbienceTrack(null);
        }
        for (int i = 0; i < this.m_fadingTracks.Count; i++)
        {
            AudioSource source2 = this.m_fadingTracks[i];
            if (source2 == source)
            {
                this.m_fadingTracks.RemoveAt(i);
                break;
            }
        }
        this.UnregisterSourceByCategory(source);
        this.UnregisterSourceByClip(source);
        SourceExtension ext = this.GetExtension(source);
        if (ext != null)
        {
            this.UnregisterSourceForDucking(source, ext);
            this.UnregisterSourceBundle(source, ext);
            this.UnregisterExtension(source, ext);
        }
        this.FinishGeneratedSource(source);
    }

    private void GarbageCollectBundles()
    {
        Map<string, BundleInfo> map = new Map<string, BundleInfo>();
        foreach (KeyValuePair<string, BundleInfo> pair in this.m_bundleInfos)
        {
            string key = pair.Key;
            BundleInfo info = pair.Value;
            info.EnableGarbageCollect(true);
            if (info.CanGarbageCollect())
            {
                this.UnloadSoundBundle(key);
            }
            else
            {
                map.Add(key, info);
            }
        }
        this.m_bundleInfos = map;
    }

    private AudioSource GenerateAudioSource(AudioSource templateSource, AudioClip clip)
    {
        GameObject obj2;
        AudioSource component;
        string name = string.Format("Audio Object - {0}", clip.name);
        if (templateSource != null)
        {
            obj2 = new GameObject(name);
            SoundUtils.AddAudioSourceComponents(obj2, null);
            component = obj2.GetComponent<AudioSource>();
            SoundUtils.CopyAudioSource(templateSource, component);
        }
        else if (this.m_config.m_PlayClipTemplate != null)
        {
            obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_config.m_PlayClipTemplate.gameObject);
            obj2.name = name;
            component = obj2.GetComponent<AudioSource>();
        }
        else
        {
            obj2 = new GameObject(name);
            SoundUtils.AddAudioSourceComponents(obj2, null);
            component = obj2.GetComponent<AudioSource>();
        }
        this.m_generatedSources.Add(component);
        return component;
    }

    public static SoundManager Get()
    {
        return s_instance;
    }

    public SoundCategory GetCategory(AudioSource source)
    {
        if (source == null)
        {
            return SoundCategory.NONE;
        }
        return this.GetDefFromSource(source).m_Category;
    }

    private float GetCategoryVolume(AudioSource source)
    {
        return SoundUtils.GetCategoryVolume(source.GetComponent<SoundDef>().m_Category);
    }

    public SoundConfig GetConfig()
    {
        return this.m_config;
    }

    public AudioSource GetCurrentAmbienceTrack()
    {
        return this.m_currentAmbienceTrack;
    }

    public List<MusicTrack> GetCurrentAmbienceTracks()
    {
        return this.m_ambienceTracks;
    }

    public AudioSource GetCurrentMusicTrack()
    {
        return this.m_currentMusicTrack;
    }

    public List<MusicTrack> GetCurrentMusicTracks()
    {
        return this.m_musicTracks;
    }

    private SoundDef GetDefFromSource(AudioSource source)
    {
        SoundDef component = source.GetComponent<SoundDef>();
        if (component == null)
        {
            object[] args = new object[] { source };
            Log.Sound.ScreenPrint("SoundUtils.GetDefFromSource() - source={0} has no def. adding new def.", args);
            component = source.gameObject.AddComponent<SoundDef>();
        }
        return component;
    }

    private float GetDuckingVolume(SoundCategory cat)
    {
        List<DuckState> list;
        if (!this.m_duckStates.TryGetValue(cat, out list))
        {
            return 1f;
        }
        float num = 1f;
        foreach (DuckState state in list)
        {
            SoundCategory triggerCategory = state.GetTriggerCategory();
            if ((triggerCategory == SoundCategory.NONE) || SoundUtils.IsCategoryAudible(triggerCategory))
            {
                float volume = state.GetVolume();
                if (num > volume)
                {
                    num = volume;
                }
            }
        }
        return num;
    }

    private float GetDuckingVolume(AudioSource source)
    {
        if (source == null)
        {
            return 1f;
        }
        SoundDef component = source.GetComponent<SoundDef>();
        if (component.m_IgnoreDucking)
        {
            return 1f;
        }
        return this.GetDuckingVolume(component.m_Category);
    }

    private SourceExtension GetExtension(AudioSource source)
    {
        for (int i = 0; i < this.m_extensionMappings.Count; i++)
        {
            ExtensionMapping mapping = this.m_extensionMappings[i];
            if (mapping.Source == source)
            {
                return mapping.Extension;
            }
        }
        return null;
    }

    private uint GetNextDuckStateTweenId()
    {
        this.m_nextDuckStateTweenId = (this.m_nextDuckStateTweenId + 1) & uint.MaxValue;
        return this.m_nextDuckStateTweenId;
    }

    private int GetNextSourceId()
    {
        int nextSourceId = this.m_nextSourceId;
        this.m_nextSourceId = (this.m_nextSourceId != 0x7fffffff) ? (this.m_nextSourceId + 1) : 1;
        return nextSourceId;
    }

    public float GetPitch(AudioSource source)
    {
        if (source == null)
        {
            return 1f;
        }
        SourceExtension extension = this.RegisterExtension(source, false, null);
        if (extension == null)
        {
            return 1f;
        }
        return extension.m_codePitch;
    }

    public GameObject GetPlaceholderSound()
    {
        AudioSource placeholderSource = this.GetPlaceholderSource();
        if (placeholderSource == null)
        {
            return null;
        }
        return placeholderSource.gameObject;
    }

    public AudioSource GetPlaceholderSource()
    {
        if ((this.m_config != null) && ApplicationMgr.IsInternal())
        {
            return this.m_config.m_PlaceholderSound;
        }
        return null;
    }

    private int GetSourceId(AudioSource source)
    {
        SourceExtension extension = this.GetExtension(source);
        if (extension == null)
        {
            return 0;
        }
        return extension.m_id;
    }

    public float GetVolume(AudioSource source)
    {
        if (source == null)
        {
            return 1f;
        }
        SourceExtension extension = this.RegisterExtension(source, false, null);
        if (extension == null)
        {
            return 1f;
        }
        return extension.m_codeVolume;
    }

    public void ImmediatelyKillMusicAndAmbience()
    {
        this.NukeMusicAndAmbiencePlaylists();
        foreach (AudioSource source in this.m_fadingTracks.ToArray())
        {
            this.FinishSource(source);
        }
        if (this.m_currentMusicTrack != null)
        {
            this.FinishSource(this.m_currentMusicTrack);
            this.ChangeCurrentMusicTrack(null);
        }
        if (this.m_currentAmbienceTrack != null)
        {
            this.FinishSource(this.m_currentAmbienceTrack);
            this.ChangeCurrentAmbienceTrack(null);
        }
    }

    private void InitializeOptions()
    {
        Options.Get().RegisterChangedListener(Option.SOUND, new Options.ChangedCallback(this.OnMasterEnabledOptionChanged));
        Options.Get().RegisterChangedListener(Option.SOUND_VOLUME, new Options.ChangedCallback(this.OnMasterVolumeOptionChanged));
        Options.Get().RegisterChangedListener(Option.MUSIC, new Options.ChangedCallback(this.OnEnabledOptionChanged));
        Options.Get().RegisterChangedListener(Option.MUSIC_VOLUME, new Options.ChangedCallback(this.OnVolumeOptionChanged));
        Options.Get().RegisterChangedListener(Option.BACKGROUND_SOUND, new Options.ChangedCallback(this.OnBackgroundSoundOptionChanged));
    }

    private void InitNewClipOnSource(AudioSource source, SoundDef def, SourceExtension ext, AudioClip clip)
    {
        ext.m_defVolume = SoundUtils.GetRandomVolumeFromDef(def);
        ext.m_defPitch = SoundUtils.GetRandomPitchFromDef(def);
        source.clip = clip;
        this.RegisterSourceByClip(source, clip);
    }

    private void InitSourceTransform(AudioSource source, GameObject parentObject)
    {
        source.transform.parent = base.transform;
        if (parentObject == null)
        {
            source.transform.position = Vector3.zero;
        }
        else
        {
            source.transform.position = parentObject.transform.position;
        }
    }

    public bool Is3d(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        return (source.spatialBlend >= 1f);
    }

    public bool IsActive(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        return (this.IsPlaying(source) || this.IsPaused(source));
    }

    private bool IsCategoryEnabled(AudioSource source)
    {
        return SoundUtils.IsCategoryEnabled(source.GetComponent<SoundDef>().m_Category);
    }

    public bool IsIgnoringDucking(AudioSource source)
    {
        if (source == null)
        {
            return true;
        }
        SoundDef component = source.GetComponent<SoundDef>();
        return ((component == null) || component.m_IgnoreDucking);
    }

    public bool IsInitialized()
    {
        return (this.m_config != null);
    }

    public bool IsPaused(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        SourceExtension extension = this.GetExtension(source);
        if (extension == null)
        {
            return false;
        }
        return extension.m_paused;
    }

    public bool IsPlaying(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        return source.isPlaying;
    }

    public bool Load(string soundName)
    {
        return AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnLoadSoundLoaded), null, true, null);
    }

    public void LoadAndPlay(string soundName)
    {
        this.LoadAndPlay(soundName, null, 1f, null, null);
    }

    public void LoadAndPlay(string soundName, float volume)
    {
        this.LoadAndPlay(soundName, null, volume, null, null);
    }

    public void LoadAndPlay(string soundName, GameObject parent)
    {
        this.LoadAndPlay(soundName, parent, 1f, null, null);
    }

    public void LoadAndPlay(string soundName, GameObject parent, float volume)
    {
        this.LoadAndPlay(soundName, parent, volume, null, null);
    }

    public void LoadAndPlay(string soundName, GameObject parent, float volume, LoadedCallback callback)
    {
        this.LoadAndPlay(soundName, parent, volume, callback, null);
    }

    public void LoadAndPlay(string soundName, GameObject parent, float volume, LoadedCallback callback, object userData)
    {
        SoundLoadContext callbackData = new SoundLoadContext();
        callbackData.Init(parent, volume, callback, userData);
        AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnLoadAndPlaySoundLoaded), callbackData, true, this.GetPlaceholderSound());
    }

    public void LoadAndPlayTemplate(AudioSource template, AudioClip clip)
    {
        this.LoadAndPlayTemplate(template, clip, 1f, null, null);
    }

    public void LoadAndPlayTemplate(AudioSource template, AudioClip clip, float volume)
    {
        this.LoadAndPlayTemplate(template, clip, volume, null, null);
    }

    public void LoadAndPlayTemplate(AudioSource template, AudioClip clip, float volume, LoadedCallback callback)
    {
        this.LoadAndPlayTemplate(template, clip, volume, callback, null);
    }

    public void LoadAndPlayTemplate(AudioSource template, AudioClip clip, float volume, LoadedCallback callback, object userData)
    {
        if (template == null)
        {
            Error.AddDevFatal("SoundManager.LoadAndPlayTemplate() - template is null", new object[0]);
        }
        else if (clip == null)
        {
            object[] messageArgs = new object[] { template, SceneUtils.FindTopParent(template) };
            Error.AddDevFatal("SoundManager.LoadAndPlayTemplate() - Attempted to play template {0} with a null clip. Top-level parent is {1}.", messageArgs);
        }
        else
        {
            SoundLoadContext callbackData = new SoundLoadContext {
                m_template = template
            };
            callbackData.Init(template.gameObject, volume, callback, userData);
            AssetLoader.Get().LoadSound(clip.name, new AssetLoader.GameObjectCallback(this.OnLoadAndPlaySoundLoaded), callbackData, true, this.GetPlaceholderSound());
        }
    }

    public void NukeAmbienceAndStopPlayingCurrentTrack()
    {
        this.m_ambienceTracks.Clear();
        this.m_ambienceTrackIndex = 0;
        this.StopCurrentAmbienceTrack();
    }

    public void NukeMusicAndAmbiencePlaylists()
    {
        this.m_musicTracks.Clear();
        this.m_ambienceTracks.Clear();
        this.m_musicTrackIndex = 0;
        this.m_ambienceTrackIndex = 0;
    }

    public void NukeMusicAndStopPlayingCurrentTrack()
    {
        this.m_musicTracks.Clear();
        this.m_musicTrackIndex = 0;
        this.StopCurrentMusicTrack();
    }

    public void NukePlaylistsAndStopPlayingCurrentTracks()
    {
        this.NukeMusicAndAmbiencePlaylists();
        this.StopCurrentMusicTrack();
        this.StopCurrentAmbienceTrack();
    }

    private void OnAmbienceLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.OnAmbienceLoaded() - ERROR \"{0}\" failed to load", name));
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("SoundManager.OnAmbienceLoaded() - ERROR \"{0}\" has no AudioSource", name));
            }
            else
            {
                this.RegisterSourceBundle(name, component);
                MusicTrack item = (MusicTrack) callbackData;
                if (!this.m_ambienceTracks.Contains(item))
                {
                    this.UnregisterSourceBundle(name, component);
                    UnityEngine.Object.DestroyImmediate(go);
                }
                else
                {
                    this.m_generatedSources.Add(component);
                    component.transform.parent = base.transform;
                    component.volume *= item.m_volume;
                    this.ChangeCurrentAmbienceTrack(component);
                    this.Play(component);
                }
                this.m_ambienceIsAboutToPlay = false;
            }
        }
    }

    private void OnAppFocusChanged(bool focus, object userData)
    {
        this.UpdateAppMute();
    }

    private void OnBackgroundSoundOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        this.UpdateAppMute();
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnDuckStateBeginningComplete(DuckState state)
    {
        state.SetMode(DuckMode.HOLD);
        state.SetTweenName(null);
    }

    private void OnDuckStateRestoringComplete(DuckState state)
    {
        SoundCategory key = state.GetDuckedDef().m_Category;
        List<DuckState> list = this.m_duckStates[key];
        for (int i = 0; i < list.Count; i++)
        {
            DuckState state2 = list[i];
            if (state2 == state)
            {
                list.RemoveAt(i);
                if (list.Count == 0)
                {
                    this.m_duckStates.Remove(key);
                }
                break;
            }
        }
    }

    private void OnEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        foreach (KeyValuePair<SoundCategory, Option> pair in SoundDataTables.s_categoryEnabledOptionMap)
        {
            SoundCategory key = pair.Key;
            if (((Option) pair.Value) == option)
            {
                this.UpdateCategoryMute(key);
            }
        }
    }

    private void OnLoadAndPlaySoundLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.OnLoadAndPlaySoundLoaded() - ERROR \"{0}\" failed to load", name));
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Debug.LogError(string.Format("SoundManager.OnLoadAndPlaySoundLoaded() - ERROR \"{0}\" has no AudioSource", name));
            }
            else
            {
                SoundLoadContext context = (SoundLoadContext) callbackData;
                if ((context.m_sceneMode != SceneMgr.Mode.FATAL_ERROR) && SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
                else
                {
                    this.RegisterSourceBundle(name, component);
                    if (context.m_haveCallback && !GeneralUtils.IsCallbackValid(context.m_callback))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                        this.UnregisterSourceBundle(name, component);
                    }
                    else
                    {
                        this.m_generatedSources.Add(component);
                        if (context.m_template != null)
                        {
                            SoundUtils.CopyAudioSource(context.m_template, component);
                        }
                        this.RegisterExtension(component, false, null).m_codeVolume = context.m_volume;
                        this.InitSourceTransform(component, context.m_parent);
                        this.Play(component);
                        if (context.m_callback != null)
                        {
                            context.m_callback(component, context.m_userData);
                        }
                    }
                }
            }
        }
    }

    private void OnLoadSoundLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.OnLoadSoundLoaded() - ERROR \"{0}\" failed to load", name));
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Debug.LogError(string.Format("SoundManager.OnLoadSoundLoaded() - ERROR \"{0}\" has no AudioSource", name));
            }
            else
            {
                this.RegisterSourceBundle(name, component);
                component.volume = 0f;
                component.Play();
                component.Stop();
                this.UnregisterSourceBundle(name, component);
                UnityEngine.Object.DestroyImmediate(component.gameObject);
            }
        }
    }

    private void OnMasterEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        this.UpdateAllMutes();
    }

    private void OnMasterVolumeOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        this.UpdateAllCategoryVolumes();
    }

    private void OnMusicLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.OnMusicLoaded() - ERROR \"{0}\" failed to load", name));
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("SoundManager.OnMusicLoaded() - ERROR \"{0}\" has no AudioSource", name));
            }
            else
            {
                this.RegisterSourceBundle(name, component);
                MusicTrack item = (MusicTrack) callbackData;
                if (!this.m_musicTracks.Contains(item))
                {
                    this.UnregisterSourceBundle(name, component);
                    UnityEngine.Object.DestroyImmediate(go);
                }
                else
                {
                    this.m_generatedSources.Add(component);
                    component.transform.parent = base.transform;
                    component.volume *= item.m_volume;
                    this.ChangeCurrentMusicTrack(component);
                    this.Play(component);
                }
                this.m_musicIsAboutToPlay = false;
            }
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        this.GarbageCollectBundles();
    }

    private void OnVolumeOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        foreach (KeyValuePair<SoundCategory, Option> pair in SoundDataTables.s_categoryVolumeOptionMap)
        {
            SoundCategory key = pair.Key;
            if (((Option) pair.Value) == option)
            {
                this.UpdateCategoryVolume(key);
            }
        }
    }

    public bool Pause(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        if (this.IsPaused(source))
        {
            return false;
        }
        SourceExtension ext = this.RegisterExtension(source, false, null);
        if (ext == null)
        {
            return false;
        }
        ext.m_paused = true;
        this.UpdateSource(source, ext);
        source.Pause();
        return true;
    }

    public bool Play(AudioSource source)
    {
        return (bool) this.PlayImpl(source, null);
    }

    [DebuggerHidden]
    private IEnumerator PlayAmbienceInSeconds(float seconds)
    {
        return new <PlayAmbienceInSeconds>c__Iterator268 { seconds = seconds, <$>seconds = seconds, <>f__this = this };
    }

    public AudioSource PlayClip(SoundPlayClipArgs args)
    {
        if ((args == null) || (args.m_clip == null))
        {
            return this.PlayImpl(null, null);
        }
        AudioSource source = this.GenerateAudioSource(args.m_templateSource, args.m_clip);
        source.clip = args.m_clip;
        if (args.m_volume.HasValue)
        {
            source.volume = args.m_volume.Value;
        }
        if (args.m_pitch.HasValue)
        {
            source.pitch = args.m_pitch.Value;
        }
        if (args.m_spatialBlend.HasValue)
        {
            source.spatialBlend = args.m_spatialBlend.Value;
        }
        if (args.m_category.HasValue)
        {
            source.GetComponent<SoundDef>().m_Category = args.m_category.Value;
        }
        this.InitSourceTransform(source, args.m_parentObject);
        if (this.Play(source))
        {
            return source;
        }
        this.FinishGeneratedSource(source);
        return null;
    }

    private AudioSource PlayImpl(AudioSource source, AudioClip oneShotClip = null)
    {
        if (source == null)
        {
            AudioSource placeholderSource = this.GetPlaceholderSource();
            if (placeholderSource == null)
            {
                Error.AddDevFatal("SoundManager.Play() - source is null and fallback is null", new object[0]);
                return null;
            }
            source = UnityEngine.Object.Instantiate<AudioSource>(placeholderSource);
            this.m_generatedSources.Add(source);
        }
        bool flag = this.IsActive(source);
        SourceExtension ext = this.RegisterExtension(source, true, oneShotClip);
        if (ext == null)
        {
            return null;
        }
        if (!flag)
        {
            this.RegisterSourceForDucking(source, ext);
        }
        this.UpdateSource(source, ext);
        source.Play();
        return source;
    }

    [DebuggerHidden]
    private IEnumerator PlayMusicInSeconds(float seconds)
    {
        return new <PlayMusicInSeconds>c__Iterator267 { seconds = seconds, <$>seconds = seconds, <>f__this = this };
    }

    private bool PlayNextAmbience()
    {
        if (!SoundUtils.IsMusicEnabled())
        {
            return false;
        }
        if (this.m_ambienceTracks.Count <= 0)
        {
            return false;
        }
        MusicTrack callbackData = this.m_ambienceTracks[this.m_ambienceTrackIndex];
        this.m_ambienceTrackIndex = (this.m_ambienceTrackIndex + 1) % this.m_ambienceTracks.Count;
        if (callbackData == null)
        {
            return false;
        }
        string soundName = FileUtils.GameAssetPathToName(callbackData.m_name);
        return AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnAmbienceLoaded), callbackData, true, this.GetPlaceholderSound());
    }

    private bool PlayNextMusic()
    {
        if (!SoundUtils.IsMusicEnabled())
        {
            return false;
        }
        if (this.m_musicTracks.Count <= 0)
        {
            return false;
        }
        MusicTrack callbackData = this.m_musicTracks[this.m_musicTrackIndex];
        this.m_musicTrackIndex = (this.m_musicTrackIndex + 1) % this.m_musicTracks.Count;
        if (callbackData == null)
        {
            return false;
        }
        if (this.m_currentMusicTrack != null)
        {
            this.FadeTrackOut(this.m_currentMusicTrack);
            this.ChangeCurrentMusicTrack(null);
        }
        string soundName = FileUtils.GameAssetPathToName(callbackData.m_name);
        return AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnMusicLoaded), callbackData, true, this.GetPlaceholderSound());
    }

    public bool PlayOneShot(AudioSource source, AudioClip clip, float volume = 1f)
    {
        if (this.PlayImpl(source, clip) == null)
        {
            return false;
        }
        if (this.IsActive(source))
        {
            this.SetVolume(source, volume);
        }
        return true;
    }

    public void PlayPreloaded(AudioSource source)
    {
        this.PlayPreloaded(source, (GameObject) null);
    }

    public void PlayPreloaded(AudioSource source, float volume)
    {
        this.PlayPreloaded(source, null, volume);
    }

    public void PlayPreloaded(AudioSource source, GameObject parentObject)
    {
        this.PlayPreloaded(source, parentObject, 1f);
    }

    public void PlayPreloaded(AudioSource source, GameObject parentObject, float volume)
    {
        if (source == null)
        {
            UnityEngine.Debug.LogError("Preloaded audio source is null! Cannot play!");
        }
        else
        {
            this.RegisterExtension(source, false, null).m_codeVolume = volume;
            this.InitSourceTransform(source, parentObject);
            this.m_generatedSources.Add(source);
            this.Play(source);
        }
    }

    [Conditional("SOUND_CATEGORY_DEBUG")]
    private void PrintAllCategorySources()
    {
        Log.Sound.Print("SoundManager.PrintAllCategorySources()", new object[0]);
        foreach (KeyValuePair<SoundCategory, List<AudioSource>> pair in this.m_sourcesByCategory)
        {
            SoundCategory key = pair.Key;
            List<AudioSource> list = pair.Value;
            object[] args = new object[] { key };
            Log.Sound.Print("Category {0}:", args);
            for (int i = 0; i < list.Count; i++)
            {
                object[] objArray2 = new object[] { i, list[i] };
                Log.Sound.Print("    {0} = {1}", objArray2);
            }
        }
    }

    private bool ProcessClipLimits(AudioClip clip)
    {
        if ((this.m_config != null) && (this.m_config.m_PlaybackLimitDefs != null))
        {
            string name = clip.name;
            bool flag = false;
            AudioSource source = null;
            foreach (SoundPlaybackLimitDef def in this.m_config.m_PlaybackLimitDefs)
            {
                SoundPlaybackLimitClipDef def2 = this.FindClipDefInPlaybackDef(name, def);
                if (def2 != null)
                {
                    int priority = def2.m_Priority;
                    float num2 = 2f;
                    int num3 = 0;
                    foreach (SoundPlaybackLimitClipDef def3 in def.m_ClipDefs)
                    {
                        List<AudioSource> list;
                        string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(def3.m_Path);
                        if (this.m_sourcesByClipName.TryGetValue(fileNameWithoutExtension, out list))
                        {
                            int num4 = def3.m_Priority;
                            foreach (AudioSource source2 in list)
                            {
                                if (this.IsPlaying(source2))
                                {
                                    float num5 = source2.time / source2.clip.length;
                                    if (num5 <= def3.m_ExclusivePlaybackThreshold)
                                    {
                                        num3++;
                                        if ((num4 < priority) && (num5 < num2))
                                        {
                                            source = source2;
                                            priority = num4;
                                            num2 = num5;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (num3 >= def.m_Limit)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                if (source == null)
                {
                    return true;
                }
                this.Stop(source);
            }
        }
        return false;
    }

    private DuckState RegisterDuckState(object trigger, SoundDuckedCategoryDef duckedCatDef)
    {
        DuckState state;
        List<DuckState> list;
        <RegisterDuckState>c__AnonStorey387 storey = new <RegisterDuckState>c__AnonStorey387 {
            trigger = trigger
        };
        SoundCategory key = duckedCatDef.m_Category;
        if (this.m_duckStates.TryGetValue(key, out list))
        {
            state = list.Find(new Predicate<DuckState>(storey.<>m__27E));
            if (state != null)
            {
                return state;
            }
        }
        else
        {
            list = new List<DuckState>();
            this.m_duckStates.Add(key, list);
        }
        state = new DuckState();
        list.Add(state);
        state.SetTrigger(storey.trigger);
        state.SetDuckedDef(duckedCatDef);
        return state;
    }

    private SourceExtension RegisterExtension(AudioSource source, bool aboutToPlay = false, AudioClip oneShotClip = null)
    {
        SoundDef defFromSource = this.GetDefFromSource(source);
        SourceExtension extension = this.GetExtension(source);
        if (extension == null)
        {
            AudioClip clip = this.DetermineClipForPlayback(source, defFromSource, oneShotClip);
            if (clip == null)
            {
                return null;
            }
            if (aboutToPlay && this.ProcessClipLimits(clip))
            {
                return null;
            }
            extension = new SourceExtension {
                m_sourceVolume = source.volume,
                m_sourcePitch = source.pitch,
                m_sourceClip = source.clip,
                m_id = this.GetNextSourceId()
            };
            this.AddExtensionMapping(source, extension);
            this.RegisterSourceByCategory(source, defFromSource.m_Category);
            this.InitNewClipOnSource(source, defFromSource, extension, clip);
            return extension;
        }
        if (aboutToPlay)
        {
            AudioClip clip2 = this.DetermineClipForPlayback(source, defFromSource, oneShotClip);
            if (!this.CanPlayClipOnExistingSource(source, clip2))
            {
                if (this.IsActive(source))
                {
                    this.Stop(source);
                }
                else
                {
                    this.FinishSource(source);
                }
                return null;
            }
            if (source.clip != clip2)
            {
                if (source.clip != null)
                {
                    this.UnregisterSourceByClip(source);
                }
                this.InitNewClipOnSource(source, defFromSource, extension, clip2);
            }
        }
        return extension;
    }

    private void RegisterForDucking(object trigger, List<SoundDuckedCategoryDef> defs)
    {
        foreach (SoundDuckedCategoryDef def in defs)
        {
            DuckState state = this.RegisterDuckState(trigger, def);
            this.ChangeDuckState(state, DuckMode.BEGINNING);
        }
    }

    private BundleInfo RegisterSourceBundle(string name, AudioSource source)
    {
        BundleInfo info;
        if (!this.m_bundleInfos.TryGetValue(name, out info))
        {
            info = new BundleInfo();
            info.SetName(name);
            this.m_bundleInfos.Add(name, info);
        }
        if (source != null)
        {
            info.AddRef(source);
            this.RegisterExtension(source, false, null).m_bundleName = name;
        }
        return info;
    }

    private void RegisterSourceByCategory(AudioSource source, SoundCategory cat)
    {
        List<AudioSource> list;
        if (!this.m_sourcesByCategory.TryGetValue(cat, out list))
        {
            list = new List<AudioSource>();
            this.m_sourcesByCategory.Add(cat, list);
            list.Add(source);
        }
        else if (!list.Contains(source))
        {
            list.Add(source);
        }
    }

    private void RegisterSourceByClip(AudioSource source, AudioClip clip)
    {
        List<AudioSource> list;
        if (!this.m_sourcesByClipName.TryGetValue(clip.name, out list))
        {
            list = new List<AudioSource>();
            this.m_sourcesByClipName.Add(clip.name, list);
            list.Add(source);
        }
        else if (!list.Contains(source))
        {
            list.Add(source);
        }
    }

    private void RegisterSourceForDucking(AudioSource source, SourceExtension ext)
    {
        SoundDuckingDef def = this.FindDuckingDefForSource(source);
        if (def != null)
        {
            this.RegisterForDucking(source, def.m_DuckedCategoryDefs);
            ext.m_ducking = true;
        }
    }

    private void RemoveExtensionMapping(AudioSource source)
    {
        for (int i = 0; i < this.m_extensionMappings.Count; i++)
        {
            ExtensionMapping mapping = this.m_extensionMappings[i];
            if (mapping.Source == source)
            {
                this.m_extensionMappings.RemoveAt(i);
                return;
            }
        }
    }

    public void Set3d(AudioSource source, bool enable)
    {
        if (source != null)
        {
            source.spatialBlend = !enable ? 0f : 1f;
        }
    }

    public void SetCategory(AudioSource source, SoundCategory cat)
    {
        if (source != null)
        {
            SoundDef component = source.GetComponent<SoundDef>();
            if (component != null)
            {
                if (component.m_Category == cat)
                {
                    return;
                }
            }
            else
            {
                component = source.gameObject.AddComponent<SoundDef>();
            }
            component.m_Category = cat;
            this.UpdateSource(source);
        }
    }

    public void SetConfig(SoundConfig config)
    {
        this.m_config = config;
    }

    public void SetIgnoreDucking(AudioSource source, bool enable)
    {
        if (source != null)
        {
            SoundDef component = source.GetComponent<SoundDef>();
            if (component != null)
            {
                component.m_IgnoreDucking = enable;
            }
        }
    }

    public void SetPitch(AudioSource source, float pitch)
    {
        if (source != null)
        {
            SourceExtension ext = this.RegisterExtension(source, false, null);
            if (ext != null)
            {
                ext.m_codePitch = pitch;
                this.UpdatePitch(source, ext);
            }
        }
    }

    public void SetVolume(AudioSource source, float volume)
    {
        if (source != null)
        {
            SourceExtension ext = this.RegisterExtension(source, false, null);
            if (ext != null)
            {
                ext.m_codeVolume = volume;
                this.UpdateVolume(source, ext);
            }
        }
    }

    [Conditional("SOUND_SOURCE_DEBUG")]
    private void SourcePrint(string format, params object[] args)
    {
        Log.Sound.Print(format, args);
    }

    [Conditional("SOUND_SOURCE_DEBUG")]
    private void SourceScreenPrint(string format, params object[] args)
    {
        Log.Sound.ScreenPrint(format, args);
    }

    private void Start()
    {
        this.UpdateAppMute();
        if (ApplicationMgr.Get() != null)
        {
            ApplicationMgr.Get().AddFocusChangedListener(new ApplicationMgr.FocusChangedCallback(this.OnAppFocusChanged));
        }
        if (SceneMgr.Get() == null)
        {
            this.m_config = new GameObject("SoundConfig").AddComponent<SoundConfig>();
        }
        else
        {
            SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
    }

    public bool StartDucking(SoundDucker ducker)
    {
        if (ducker == null)
        {
            return false;
        }
        if (ducker.m_DuckedCategoryDefs == null)
        {
            return false;
        }
        if (ducker.m_DuckedCategoryDefs.Count == 0)
        {
            return false;
        }
        this.RegisterForDucking(ducker, ducker.GetDuckedCategoryDefs());
        return true;
    }

    public bool Stop(AudioSource source)
    {
        if (source == null)
        {
            return false;
        }
        if (!this.IsActive(source))
        {
            return false;
        }
        source.Stop();
        this.FinishSource(source);
        return true;
    }

    public void StopCurrentAmbienceTrack()
    {
        if (this.m_currentAmbienceTrack != null)
        {
            this.FadeTrackOut(this.m_currentAmbienceTrack);
            this.ChangeCurrentAmbienceTrack(null);
        }
    }

    public void StopCurrentMusicTrack()
    {
        if (this.m_currentMusicTrack != null)
        {
            this.FadeTrackOut(this.m_currentMusicTrack);
            this.ChangeCurrentMusicTrack(null);
        }
    }

    public void StopDucking(SoundDucker ducker)
    {
        if (((ducker != null) && (ducker.m_DuckedCategoryDefs != null)) && (ducker.m_DuckedCategoryDefs.Count != 0))
        {
            this.UnregisterForDucking(ducker, ducker.GetDuckedCategoryDefs());
        }
    }

    [Conditional("SOUND_TRACK_DEBUG")]
    private void TrackPrint(string format, params object[] args)
    {
        Log.Sound.Print(format, args);
    }

    [Conditional("SOUND_TRACK_DEBUG")]
    private void TrackScreenPrint(string format, params object[] args)
    {
        Log.Sound.ScreenPrint(format, args);
    }

    private void UnloadSoundBundle(string name)
    {
        AssetCache.ClearSound(name);
    }

    private void UnregisterExtension(AudioSource source, SourceExtension ext)
    {
        source.volume = ext.m_sourceVolume;
        source.pitch = ext.m_sourcePitch;
        source.clip = ext.m_sourceClip;
        this.RemoveExtensionMapping(source);
    }

    private void UnregisterForDucking(object trigger, List<SoundDuckedCategoryDef> defs)
    {
        <UnregisterForDucking>c__AnonStorey388 storey = new <UnregisterForDucking>c__AnonStorey388 {
            trigger = trigger
        };
        foreach (SoundDuckedCategoryDef def in defs)
        {
            List<DuckState> list;
            SoundCategory key = def.m_Category;
            if (!this.m_duckStates.TryGetValue(key, out list))
            {
                UnityEngine.Debug.LogError(string.Format("SoundManager.UnregisterForDucking() - {0} ducks {1}, but no DuckStates were found for {1}", storey.trigger, key));
            }
            else
            {
                DuckState state = list.Find(new Predicate<DuckState>(storey.<>m__27F));
                if (state != null)
                {
                    this.ChangeDuckState(state, DuckMode.RESTORING);
                }
            }
        }
    }

    private void UnregisterSourceBundle(string name, AudioSource source)
    {
        BundleInfo info;
        if ((this.m_bundleInfos.TryGetValue(name, out info) && info.RemoveRef(source)) && info.CanGarbageCollect())
        {
            this.m_bundleInfos.Remove(name);
            this.UnloadSoundBundle(name);
        }
    }

    private void UnregisterSourceBundle(AudioSource source, SourceExtension ext)
    {
        if (ext.m_bundleName != null)
        {
            this.UnregisterSourceBundle(ext.m_bundleName, source);
        }
    }

    private void UnregisterSourceByCategory(AudioSource source)
    {
        List<AudioSource> list;
        SoundCategory key = this.GetCategory(source);
        if (!this.m_sourcesByCategory.TryGetValue(key, out list))
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.UnregisterSourceByCategory() - {0} is untracked. category={1}", this.GetSourceId(source), key));
        }
        else if (list.Remove(source))
        {
        }
    }

    private void UnregisterSourceByClip(AudioSource source)
    {
        AudioClip clip = source.clip;
        if (clip == null)
        {
            UnityEngine.Debug.LogError(string.Format("SoundManager.UnregisterSourceByClip() - id {0} (source {1}) is untracked", this.GetSourceId(source), source));
        }
        else
        {
            List<AudioSource> list;
            if (!this.m_sourcesByClipName.TryGetValue(clip.name, out list))
            {
                UnityEngine.Debug.LogError(string.Format("SoundManager.UnregisterSourceByClip() - id {0} (source {1}) is untracked. clip={2}", this.GetSourceId(source), source, clip));
            }
            else
            {
                list.Remove(source);
                if (list.Count == 0)
                {
                    this.m_sourcesByClipName.Remove(clip.name);
                }
            }
        }
    }

    private void UnregisterSourceForDucking(AudioSource source, SourceExtension ext)
    {
        if (ext.m_ducking)
        {
            SoundDuckingDef def = this.FindDuckingDefForSource(source);
            if (def != null)
            {
                this.UnregisterForDucking(source, def.m_DuckedCategoryDefs);
            }
        }
    }

    private void Update()
    {
        this.m_frame = (this.m_frame + 1) & uint.MaxValue;
        this.UpdateMusicAndSources();
    }

    private void UpdateAllCategoryVolumes()
    {
        foreach (SoundCategory category in this.m_sourcesByCategory.Keys)
        {
            this.UpdateCategoryVolume(category);
        }
    }

    private void UpdateAllMutes()
    {
        foreach (ExtensionMapping mapping in this.m_extensionMappings)
        {
            this.UpdateMute(mapping.Source);
        }
    }

    private void UpdateAppMute()
    {
        this.UpdateMusicAndSources();
        if (ApplicationMgr.Get() != null)
        {
            this.m_mute = !(ApplicationMgr.Get().HasFocus() || Options.Get().GetBool(Option.BACKGROUND_SOUND));
        }
        this.UpdateAllMutes();
    }

    private void UpdateCategoryMute(SoundCategory cat)
    {
        List<AudioSource> list;
        if (this.m_sourcesByCategory.TryGetValue(cat, out list))
        {
            bool categoryEnabled = SoundUtils.IsCategoryEnabled(cat);
            for (int i = 0; i < list.Count; i++)
            {
                AudioSource source = list[i];
                this.UpdateMute(source, categoryEnabled);
            }
        }
    }

    private void UpdateCategoryVolume(SoundCategory cat)
    {
        List<AudioSource> list;
        if (this.m_sourcesByCategory.TryGetValue(cat, out list))
        {
            float categoryVolume = SoundUtils.GetCategoryVolume(cat);
            for (int i = 0; i < list.Count; i++)
            {
                AudioSource source = list[i];
                if (source != null)
                {
                    SourceExtension ext = this.GetExtension(source);
                    float duckingVolume = this.GetDuckingVolume(source);
                    this.UpdateVolume(source, ext, categoryVolume, duckingVolume);
                }
            }
        }
    }

    private void UpdateDuckStates()
    {
        foreach (List<DuckState> list in this.m_duckStates.Values)
        {
            foreach (DuckState state in list)
            {
                if (!state.IsTriggerAlive() && (state.GetMode() != DuckMode.RESTORING))
                {
                    this.ChangeDuckState(state, DuckMode.RESTORING);
                }
            }
        }
    }

    private void UpdateGeneratedSources()
    {
        this.CleanUpSourceList(this.m_generatedSources);
    }

    private void UpdateMusicAndAmbience()
    {
        if (SoundUtils.IsMusicEnabled())
        {
            if (!this.m_musicIsAboutToPlay)
            {
                if (this.m_currentMusicTrack != null)
                {
                    if (!this.IsPlaying(this.m_currentMusicTrack))
                    {
                        base.StartCoroutine(this.PlayMusicInSeconds(this.m_config.m_SecondsBetweenMusicTracks));
                    }
                }
                else
                {
                    this.m_musicIsAboutToPlay = this.PlayNextMusic();
                }
            }
            if (!this.m_ambienceIsAboutToPlay)
            {
                if (this.m_currentAmbienceTrack != null)
                {
                    if (!this.IsPlaying(this.m_currentAmbienceTrack))
                    {
                        base.StartCoroutine(this.PlayAmbienceInSeconds(0f));
                    }
                }
                else
                {
                    this.m_ambienceIsAboutToPlay = this.PlayNextAmbience();
                }
            }
        }
    }

    private void UpdateMusicAndSources()
    {
        this.UpdateMusicAndAmbience();
        this.UpdateSources();
    }

    private void UpdateMute(AudioSource source)
    {
        bool categoryEnabled = this.IsCategoryEnabled(source);
        this.UpdateMute(source, categoryEnabled);
    }

    private void UpdateMute(AudioSource source, bool categoryEnabled)
    {
        source.mute = this.m_mute || !categoryEnabled;
    }

    private void UpdatePitch(AudioSource source, SourceExtension ext)
    {
        source.pitch = (ext.m_codePitch * ext.m_sourcePitch) * ext.m_defPitch;
    }

    private void UpdateSource(AudioSource source)
    {
        SourceExtension ext = this.GetExtension(source);
        this.UpdateSource(source, ext);
    }

    private void UpdateSource(AudioSource source, SourceExtension ext)
    {
        this.UpdateMute(source);
        this.UpdateVolume(source, ext);
        this.UpdatePitch(source, ext);
    }

    private void UpdateSourceBundles()
    {
        foreach (BundleInfo info in this.m_bundleInfos.Values)
        {
            List<AudioSource> refs = info.GetRefs();
            int index = 0;
            bool flag = false;
            while (index < refs.Count)
            {
                AudioSource source = refs[index];
                if (source == null)
                {
                    flag = true;
                    refs.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
            if (flag)
            {
                string name = info.GetName();
                if (info.CanGarbageCollect())
                {
                    this.m_bundleInfos.Remove(name);
                    this.UnloadSoundBundle(name);
                }
            }
        }
    }

    private void UpdateSourceExtensionMappings()
    {
        int index = 0;
        while (index < this.m_extensionMappings.Count)
        {
            ExtensionMapping mapping = this.m_extensionMappings[index];
            AudioSource source = mapping.Source;
            if (source == null)
            {
                this.m_extensionMappings.RemoveAt(index);
            }
            else
            {
                if (!this.IsActive(source))
                {
                    this.m_inactiveSources.Add(source);
                }
                index++;
            }
        }
        this.CleanInactiveSources();
    }

    private void UpdateSources()
    {
        this.UpdateSourceExtensionMappings();
        this.UpdateSourcesByCategory();
        this.UpdateSourcesByClipName();
        this.UpdateSourceBundles();
        this.UpdateGeneratedSources();
        this.UpdateDuckStates();
    }

    private void UpdateSourcesByCategory()
    {
        foreach (List<AudioSource> list in this.m_sourcesByCategory.Values)
        {
            this.CleanUpSourceList(list);
        }
    }

    private void UpdateSourcesByClipName()
    {
        foreach (List<AudioSource> list in this.m_sourcesByClipName.Values)
        {
            this.CleanUpSourceList(list);
        }
    }

    private void UpdateVolume(AudioSource source, SourceExtension ext)
    {
        float categoryVolume = this.GetCategoryVolume(source);
        float duckingVolume = this.GetDuckingVolume(source);
        this.UpdateVolume(source, ext, categoryVolume, duckingVolume);
    }

    private void UpdateVolume(AudioSource source, SourceExtension ext, float categoryVolume, float duckingVolume)
    {
        source.volume = (((ext.m_codeVolume * ext.m_sourceVolume) * ext.m_defVolume) * categoryVolume) * duckingVolume;
    }

    [CompilerGenerated]
    private sealed class <AnimateBeginningDuckState>c__AnonStorey389
    {
        internal SoundManager <>f__this;
        internal SoundDuckedCategoryDef duckedDef;
        internal SoundManager.DuckState state;

        internal void <>m__280(object amount)
        {
            float volume = (float) amount;
            this.state.SetVolume(volume);
            this.<>f__this.UpdateCategoryVolume(this.duckedDef.m_Category);
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateRestoringDuckState>c__AnonStorey38A
    {
        internal SoundManager <>f__this;
        internal SoundDuckedCategoryDef duckedDef;
        internal SoundManager.DuckState state;

        internal void <>m__281(object amount)
        {
            float volume = (float) amount;
            this.state.SetVolume(volume);
            this.<>f__this.UpdateCategoryVolume(this.duckedDef.m_Category);
        }
    }

    [CompilerGenerated]
    private sealed class <FadeTrack>c__Iterator269 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AudioSource <$>source;
        internal float <$>targetVolume;
        internal SoundManager <>f__this;
        internal SoundManager.SourceExtension <ext>__0;
        internal AudioSource source;
        internal float targetVolume;

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
                    this.<>f__this.m_fadingTracks.Add(this.source);
                    this.<ext>__0 = this.<>f__this.GetExtension(this.source);
                    break;

                case 1:
                    if ((this.source != null) && this.<>f__this.IsActive(this.source))
                    {
                        break;
                    }
                    goto Label_0101;

                default:
                    goto Label_0101;
            }
            if (this.<ext>__0.m_codeVolume > 0.0001f)
            {
                this.<ext>__0.m_codeVolume = Mathf.Lerp(this.<ext>__0.m_codeVolume, this.targetVolume, UnityEngine.Time.deltaTime);
                this.<>f__this.UpdateVolume(this.source, this.<ext>__0);
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.<>f__this.FinishSource(this.source);
            this.$PC = -1;
        Label_0101:
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

    [CompilerGenerated]
    private sealed class <PlayAmbienceInSeconds>c__Iterator268 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal SoundManager <>f__this;
        internal float seconds;

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
                    this.<>f__this.m_ambienceIsAboutToPlay = true;
                    this.$current = new WaitForSeconds(this.seconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_ambienceIsAboutToPlay = this.<>f__this.PlayNextAmbience();
                    this.$PC = -1;
                    break;
            }
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

    [CompilerGenerated]
    private sealed class <PlayMusicInSeconds>c__Iterator267 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal SoundManager <>f__this;
        internal float seconds;

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
                    this.<>f__this.m_musicIsAboutToPlay = true;
                    this.$current = new WaitForSeconds(this.seconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_musicIsAboutToPlay = this.<>f__this.PlayNextMusic();
                    this.$PC = -1;
                    break;
            }
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

    [CompilerGenerated]
    private sealed class <RegisterDuckState>c__AnonStorey387
    {
        internal object trigger;

        internal bool <>m__27E(SoundManager.DuckState currState)
        {
            return currState.IsTrigger(this.trigger);
        }
    }

    [CompilerGenerated]
    private sealed class <UnregisterForDucking>c__AnonStorey388
    {
        internal object trigger;

        internal bool <>m__27F(SoundManager.DuckState currState)
        {
            return currState.IsTrigger(this.trigger);
        }
    }

    private class BundleInfo
    {
        private bool m_garbageCollect;
        private string m_name;
        private List<AudioSource> m_refs = new List<AudioSource>();

        public void AddRef(AudioSource instance)
        {
            this.m_garbageCollect = false;
            this.m_refs.Add(instance);
        }

        public bool CanGarbageCollect()
        {
            if (!this.m_garbageCollect)
            {
                return false;
            }
            if (this.m_refs.Count > 0)
            {
                return false;
            }
            if (AssetCache.IsLoading(this.m_name))
            {
                return false;
            }
            return true;
        }

        public void EnableGarbageCollect(bool enable)
        {
            this.m_garbageCollect = enable;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public int GetRefCount()
        {
            return this.m_refs.Count;
        }

        public List<AudioSource> GetRefs()
        {
            return this.m_refs;
        }

        public bool IsGarbageCollectEnabled()
        {
            return this.m_garbageCollect;
        }

        public bool RemoveRef(AudioSource instance)
        {
            return this.m_refs.Remove(instance);
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }
    }

    private enum DuckMode
    {
        IDLE,
        BEGINNING,
        HOLD,
        RESTORING
    }

    private class DuckState
    {
        private SoundDuckedCategoryDef m_duckedDef;
        private SoundManager.DuckMode m_mode;
        private object m_trigger;
        private SoundCategory m_triggerCategory;
        private string m_tweenName;
        private float m_volume = 1f;

        public SoundDuckedCategoryDef GetDuckedDef()
        {
            return this.m_duckedDef;
        }

        public SoundManager.DuckMode GetMode()
        {
            return this.m_mode;
        }

        public object GetTrigger()
        {
            return this.m_trigger;
        }

        public SoundCategory GetTriggerCategory()
        {
            return this.m_triggerCategory;
        }

        public string GetTweenName()
        {
            return this.m_tweenName;
        }

        public float GetVolume()
        {
            return this.m_volume;
        }

        public bool IsTrigger(object trigger)
        {
            return (this.m_trigger == trigger);
        }

        public bool IsTriggerAlive()
        {
            return GeneralUtils.IsObjectAlive(this.m_trigger);
        }

        public void SetDuckedDef(SoundDuckedCategoryDef def)
        {
            this.m_duckedDef = def;
        }

        public void SetMode(SoundManager.DuckMode mode)
        {
            this.m_mode = mode;
        }

        public void SetTrigger(object trigger)
        {
            this.m_trigger = trigger;
            AudioSource source = trigger as AudioSource;
            if (source != null)
            {
                this.m_triggerCategory = SoundManager.Get().GetCategory(source);
            }
        }

        public void SetTweenName(string name)
        {
            this.m_tweenName = name;
        }

        public void SetVolume(float volume)
        {
            this.m_volume = volume;
        }
    }

    private class ExtensionMapping
    {
        public SoundManager.SourceExtension Extension;
        public AudioSource Source;
    }

    public delegate void LoadedCallback(AudioSource source, object userData);

    private class SoundLoadContext
    {
        public SoundManager.LoadedCallback m_callback;
        public bool m_haveCallback;
        public GameObject m_parent;
        public SceneMgr.Mode m_sceneMode;
        public AudioSource m_template;
        public object m_userData;
        public float m_volume;

        public void Init(SoundManager.LoadedCallback callback, object userData)
        {
            this.m_sceneMode = (SceneMgr.Get() != null) ? SceneMgr.Get().GetMode() : SceneMgr.Mode.INVALID;
            this.m_haveCallback = callback != null;
            this.m_callback = callback;
            this.m_userData = userData;
        }

        public void Init(GameObject parent, float volume, SoundManager.LoadedCallback callback, object userData)
        {
            this.m_parent = parent;
            this.m_volume = volume;
            this.Init(callback, userData);
        }
    }

    private class SourceExtension
    {
        public string m_bundleName;
        public float m_codePitch = 1f;
        public float m_codeVolume = 1f;
        public float m_defPitch = 1f;
        public float m_defVolume = 1f;
        public bool m_ducking;
        public int m_id;
        public bool m_paused;
        public AudioClip m_sourceClip;
        public float m_sourcePitch = 1f;
        public float m_sourceVolume = 1f;
    }
}

