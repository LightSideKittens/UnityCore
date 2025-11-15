using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore;
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
    public class Play : CreateSinglePrefab<AudioSource>
    {
        private float volume;
        private Tween tween;
        
        public override void Do()
        {
            base.Do();
            PlaySource();
        }

        protected virtual void PlaySource()
        {
            obj.Play();
        }

        public void Stop()
        {
            if (obj != null)
            { 
                obj.Stop();
            }
        }

        public void FadeIn(float duration = 0.5f)
        {
            Do();
            if (tween == null)
            {
                obj.volume = 0; 
                tween ??= obj.DOFade(volume, duration).SetAutoKill(false).KillOnDestroy();
            }
            
            tween.PlayForward();
        }

        public void FadeOut()
        {
            tween?.PlayBackwards();
        }
        
        protected override void OnCreated()
        {
            base.OnCreated();
            Object.DontDestroyOnLoad(obj.gameObject);
            volume = obj.volume;
            tween = null;
        }
    }
    
    [Serializable]
    public class PlayClip : Play
    {
        public AudioClip clip;

        protected override void PlaySource()
        {
            obj.clip = clip;
            base.PlaySource();
        }
    }
    
    [Serializable]
    public class PlayOneShot : CreateSinglePrefab<AudioSource>
    {
        public AudioClip clip;
        
        public override void Do()
        {
            base.Do();
            obj.PlayOneShot(clip);
        }

        protected override void OnCreated()
        {
            base.OnCreated();
            Object.DontDestroyOnLoad(obj.gameObject);
        }
    }
    
    [Serializable]
    public class MixerMuter : ToggleData
    {
#if UNITY_EDITOR
        static MixerMuter()
        {
            World.Destroyed += () => Changed = null;
        }
#endif
        public static event Action<string, bool> Changed;
        
        [ValueDropdown("Parameters")] public string parameter;

#if UNITY_EDITOR
        private IEnumerable<string> Parameters => LaLaEditor.GetExposedParameterNames(SingleAssets.Get<AudioMixer>());
#endif

        protected override bool Get => Unmutes.As(parameter, true);

        protected override bool Set
        {
            set
            {
                var mixer = SingleAssets.Get<AudioMixer>();
                var volume = ToDb(value ? Volumes.As(parameter, 1) : 0);
                var id = HashCode.Combine(mixer, parameter);
                DOTween.Kill(id);
                mixer.DOSetFloat(parameter, volume, 0.3f).SetId(id);
                Unmutes[parameter] = value;
                Changed?.Invoke(parameter, value);
            }
        }
    }
    
    public static JObject Config => config ?? JTokenGameConfig.Get("LaLaSettings");
    public static JObject Volumes => Config.AsJ<JObject>("volumes");
    public static JObject Unmutes => Config.AsJ<JObject>("unmutes");
    private static JObject config;

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

    public static float ToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    public static float FromDb(float db) => Mathf.Pow(10f, db / 20f);
    
}
