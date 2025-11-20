#if !UNITY_EDITOR
#define RUNTIME
#endif
using System.Diagnostics;
using DG.Tweening;
using LSCore.Async;

namespace LSCore.ConfigModule
{
    public abstract class AutoSaveConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        private static float currentSaveDelay;
        private float saveDelay;
        private Tween saveTween;
        
        public override void Load()
        {
            base.Load();
            currentSaveDelay = (currentSaveDelay + 1) % 5;
            saveDelay = currentSaveDelay + 1;
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
            saveTween = Wait.InfinityLoop(saveDelay, OnApplicationPaused);
#endif
        }
        
        private void OnWorldDestroy()
        {
            saveTween.Kill();
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
            saveTween = Wait.InfinityLoop(saveDelay, OnApplicationPaused);
        }
        
        private void OnApplicationPaused()
        {
            saveTween.Restart();
            Save();
        }
        
        [Conditional("RUNTIME")]
        private void UnsubOnApplicationPaused()
        {
            if (!isSaveListening) return;
            isSaveListening = false;
            World.ApplicationPaused -= OnApplicationPaused;
            saveTween.Kill();
        }

        #endregion
    }
}