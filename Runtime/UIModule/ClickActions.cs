using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    [Unwrap]
    public class ClickActions : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [GetContext] private Object context;
#endif
        [SerializeReference] public List<LSAction> actions = new();
        [HideInInspector] [SerializeField] private bool isClickSoundOverride;
        
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                isClickSoundOverride = actions?.Any(x => x is PlayOneShotSound) ?? false;
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        public void Init()
        {
#if UNITY_EDITOR
            if(World.IsEditMode) return;
            if(context) return;
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