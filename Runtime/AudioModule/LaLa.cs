using System;
using System.Collections.Generic;
using System.Security.Policy;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public static class LaLa
{
    [Serializable]
    public class Settings
    {
        [SerializeField] private float volume = 1;
        [SerializeField] private float pitch = 1;
        [SerializeField] private bool loop;
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioMixerGroup group;

        public AudioSource LastSource { get; private set; }

        public bool WasChanged { get; private set; }
        
        public float Volume { get => volume; set => SetStruct(ref volume, value); }
        public float Pitch { get => pitch; set => SetStruct(ref pitch, value); }
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
            Pitch = settings.pitch;
            Loop = settings.loop;
            Clip = settings.clip;
            Group = settings.group;
        }

        public void Apply(AudioSource source)
        {
            WasChanged = false;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.clip = clip;
            source.outputAudioMixerGroup = group;
        }
        
        public AudioSource Play()
        {
            var source = sources.Get();
            Apply(source);
            source.Play();
            LastSource = source;
            return source;
        }
    
        public AudioSource PlayOneShot()
        {
            var source = lastPlayOneShotSource;
            if (WasChanged || source == null)
            { 
                source = sources.Get();
            }
            Apply(source);
            source.PlayOneShot(source.clip);
            lastPlayOneShotSource = source;
            LastSource = source;
            return source;
        }
    }
    
    [Serializable]
    public class MixerMuter : BaseToggleData
    {
        [ValueDropdown("Parameters")] public string parameter;

#if UNITY_EDITOR
        private IEnumerable<string> Parameters => LaLaEditor.GetExposedParameterNames(SingleAssets.Get<AudioMixer>("AudioMixer"));
#endif

        protected override bool Get => Unmutes.As(parameter, true);

        protected override bool Set
        {
            set
            {
                var mixer = SingleAssets.Get<AudioMixer>("AudioMixer");
                var volume = ToDb(value ? Volumes.As(parameter, 1) : 0);
                var id = HashCode.Combine(mixer, parameter);
                DOTween.Kill(id);
                mixer.DOSetFloat(parameter, volume, 0.3f).SetId(id);
                Unmutes[parameter] = value;
            }
        }
    }

#if UNITY_EDITOR
    static LaLa()
    {
        World.Creating += () =>
        {
            sources.Clear();
            lastPlayOneShotSource = null;
        };
    }
#endif
    
    private static readonly LSObjectPool<AudioSource> sources = new(Get);
    public static JObject Config => config ?? JTokenGameConfig.Get("LaLaSettings");
    public static JObject Volumes => Config.AsJ<JObject>("volumes");
    public static JObject Unmutes => Config.AsJ<JObject>("unmutes");
    private static JObject config;
    private static AudioSource lastPlayOneShotSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        var mixerMuter = new MixerMuter();
        foreach (var property in Unmutes.Properties())
        {
            mixerMuter.lastIsOn = null;
            mixerMuter.parameter = property.Name;
            mixerMuter.IsOn = property.Value.ToBool();
        }
    }
    
    private static AudioSource Get()
    {
        var source = new GameObject("LaLa").AddComponent<AudioSource>(); 
        Object.DontDestroyOnLoad(source.gameObject);
        Wait.While(() => !source.isPlaying, WaitForRelease);
        
        return source;

        void WaitForRelease()
        {
            Wait.While(() => source.isPlaying, Release);
        }
        
        void Release()
        {
            sources.Release(source);
        }
    }

    public static float ToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    public static float FromDb(float db) => Mathf.Pow(10f, db / 20f);
    
}
