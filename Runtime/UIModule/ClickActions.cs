﻿using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Attributes;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace LSCore
{
    [Serializable]
    [Unwrap]
    public class ClickActions : ISerializationCallbackReceiver
    {
        [SerializeReference] public List<DoIt> actions = new();
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
            if(PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            isClickSoundOverride = actions?.Any(x => x is PlayOneShotSound) ?? false;
#endif
            if (!isClickSoundOverride)
            {
                actions ??= new List<DoIt>();
                var action = new PlayOneShotSound();
                var settings = new LaLaLa.Settings();
                action.settings = settings;
                settings.Clip = SingleAsset<AudioClip>.Get("ButtonClick");
                settings.Group = SingleAsset<AudioMixerGroup>.Get("AudioMixer[UI]");
                
                actions.Insert(0, action);
            }
        }

        public void OnClick()
        {
            actions?.Invoke();
        }
    }
}