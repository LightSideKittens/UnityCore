﻿#if !UNITY_EDITOR
#define RUNTIME
#endif
using System.Diagnostics;

namespace LSCore.ConfigModule
{
    public abstract class AutoSaveConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public override void Load()
        {
            base.Load();
            SetupAutoSave();
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            UnSetupAutoSave();
        }

        private void SetupAutoSave()
        {
#if UNITY_EDITOR
            if(!World.IsPlaying) return;
#endif
            
            UnSetupAutoSave();
            Editor_Init();
            Runtime_Init();
        }
        
        private void UnSetupAutoSave()
        {
#if UNITY_EDITOR
            if(!World.IsPlaying) return;
#endif
            UnsubOnWorldDestroy();
            UnsubOnApplicationPaused();
        }

        #region EDITOR

        [Conditional("UNITY_EDITOR")]
        private void Editor_Init()
        {
            World.Destroyed += OnWorldDestroy;
        }
        
        public void OnWorldDestroy()
        {
            Save();
            LoadOnNextAccess();
            UnsubOnWorldDestroy();
        }
        
        [Conditional("UNITY_EDITOR")]
        private void UnsubOnWorldDestroy() => World.Destroyed -= OnWorldDestroy;

        #endregion
        #region RUNTIME
        
        [Conditional("RUNTIME")]
        private void Runtime_Init()
        {
            World.ApplicationPaused += OnApplicationPaused;
        }
        
        private void OnApplicationPaused()
        {
            UnsubOnApplicationPaused();
            Save();
        }
        
        [Conditional("RUNTIME")]
        private void UnsubOnApplicationPaused() => World.ApplicationPaused -= OnApplicationPaused;
        
        #endregion
    }
}