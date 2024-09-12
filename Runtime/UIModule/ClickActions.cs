using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.Audio;

namespace LSCore
{
    [Serializable]
    public class ClickActions : ISerializationCallbackReceiver
    {
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
            
#if UNITY_EDITOR
        public class Drawer : OdinValueDrawer<ClickActions>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                Property.Children.First().Draw(label);
            }
        }
#endif
    }
}