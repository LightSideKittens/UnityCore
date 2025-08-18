#if !UNITY_EDITOR
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

        public override void Unload()
        {
            base.Unload();
            UnSetupAutoSave();
        }

        private void SetupAutoSave()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                World.Creating += OnCreating;

                void OnCreating()
                {
                    World.Creating -= OnCreating;
                    Unload();
                }
                return;
            }
#endif
            
            UnSetupAutoSave();
            Editor_Init();
            Runtime_Init();
        }
        
        private void UnSetupAutoSave()
        {
#if UNITY_EDITOR
            if(World.IsEditMode) return;
#endif
            UnsubOnWorldDestroy();
            UnsubOnApplicationPaused();
        }
        
        

#region EDITOR

        [Conditional("UNITY_EDITOR")]
        private void Editor_Init()
        {
#if UNITY_EDITOR
            World.Destroyed += OnWorldDestroy;
#endif
        }
        
        private void OnWorldDestroy()
        {
            Save();
            Unload();
            UnsubOnWorldDestroy();
        }
        
        [Conditional("UNITY_EDITOR")]
        private void UnsubOnWorldDestroy()
        {
#if UNITY_EDITOR
            World.Destroyed -= OnWorldDestroy;
#endif
        }

        #endregion
        
        
#region RUNTIME

        private bool isSaveListening;
        [Conditional("RUNTIME")]
        private void Runtime_Init()
        {
            if (isSaveListening) return;
            isSaveListening = true;
            World.ApplicationPaused += OnApplicationPaused;
        }
        
        private void OnApplicationPaused()
        {
            Save();
        }
        
        [Conditional("RUNTIME")]
        private void UnsubOnApplicationPaused()
        {
            if (!isSaveListening) return;
            isSaveListening = false;
            World.ApplicationPaused -= OnApplicationPaused;
        }

        #endregion
    }
}