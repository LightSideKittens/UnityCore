using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace LSCore
{
    [Serializable]
    public class ClickActions : ISerializationCallbackReceiver
    {
        [SerializeReference] public List<LSAction> actions;
        [HideInInspector] [SerializeField] private bool isClickSoundOverride;
        
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                isClickSoundOverride = actions?.Any(x => x is PlayOneShotSound) ?? false;
            }
#endif
        }

        public void OnAfterDeserialize() { }

        public void Init()
        {
#if UNITY_EDITOR
            isClickSoundOverride = actions?.Any(x => x is PlayOneShotSound) ?? false;
#endif
            if (!isClickSoundOverride)
            {
                actions ??= new List<LSAction>();
                var action = new PlayOneShotSound();
                var settings = new LaLaLa.Settings();
                action.settings = settings;
                settings.Clip = SingleAsset<AudioClip>.Get("ButtonClick");
                settings.Group = SingleAsset<AudioMixerGroup>.Get("AudioMixer[UI]");
                    
                actions.Add(action);
            }
        }

        public void OnClick()
        {
            actions?.Invoke();
        }
    }
}