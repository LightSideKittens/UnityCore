#if !UNITY_EDITOR
#define RUNTIME
#endif
using System.Diagnostics;

namespace LSCore.ConfigModule
{
    public class AutoSaveConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public new static AutoSaveConfigManager<T> Get(string fullPath) =>
            ConfigMaster<AutoSaveConfigManager<T>>.Get(fullPath); 
        
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
        
        private void OnWorldDestroy()
        {
            UnsubOnWorldDestroy();
            Save();
            LoadOnNextAccess();
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