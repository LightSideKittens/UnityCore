using System;
using System.Collections.Generic;
using LSCore;
using LSCore.Async;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public static class LaLaLa
{
    [Serializable]
    public class Settings
    {
        [SerializeField] private float volume = 1;
        [SerializeField] private bool loop;
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioMixerGroup group;
        public bool WasChanged { get; private set; }
        
        public float Volume { get => volume; set => SetStruct(ref volume, value); }
        public bool Loop { get => loop; set => SetStruct(ref loop, value); }
        public AudioClip Clip { get => clip; set => SetClass(ref clip, value); }
        public AudioMixerGroup Group { get => group; set => SetClass(ref group, value); }
        
        private void SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (WasChanged || EqualityComparer<T>.Default.Equals(currentValue, newValue))
            {
                currentValue = newValue;
                return;
            }
            
            currentValue = newValue;
            WasChanged = true;
        }
        
        private void SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if (WasChanged || (currentValue == null && newValue == null) ||
                (currentValue != null && currentValue.Equals(newValue)))
            {
                currentValue = newValue;
                return;
            }
            
            currentValue = newValue;
            WasChanged = true;
        }
        
        public void Copy(Settings settings)
        {
            Volume = settings.volume;
            Loop = settings.loop;
            Clip = settings.clip;
            Group = settings.group;
        }

        public void Apply(AudioSource source)
        {
            WasChanged = false;
            source.volume = volume;
            source.loop = loop;
            source.clip = clip;
            source.outputAudioMixerGroup = group;
        }
    }

#if UNITY_EDITOR
    static LaLaLa()
    {
        World.Destroyed += () =>
        {
            sources.Clear();
            lastPlayOneShotSource = null;
            playSettings = new();
            playOneShotSettings = new();
        };
    }
#endif
    
    private static readonly LSObjectPool<AudioSource> sources = new(Get);
    public static Settings playSettings = new();
    public static Settings playOneShotSettings = new();
    private static AudioSource lastPlayOneShotSource;

    public static AudioSource Play(AudioClip clip)
    {
        var source = sources.Get();
        playSettings.Clip = clip;
        playSettings.Apply(source);
        source.Play();
        return source;
    }
    
    public static AudioSource PlayOneShot(AudioClip clip)
    {
        Debug.Log("PlayOneShot");
        var source = lastPlayOneShotSource;
        playOneShotSettings.Clip = clip;
        if (playOneShotSettings.WasChanged || source == null)
        { 
            source = sources.Get();
        }
        playOneShotSettings.Apply(source);
        source.PlayOneShot(clip);
        lastPlayOneShotSource = source;
        return source;
    }
    
    private static AudioSource Get()
    {
        var source = new GameObject("LaLaLa").AddComponent<AudioSource>(); 
        Object.DontDestroyOnLoad(source.gameObject);
        Wait.Coroutine(new WaitWhile(() => source.isPlaying), () =>
        {
            sources.Release(source);
        });
        return source;
    }
}
