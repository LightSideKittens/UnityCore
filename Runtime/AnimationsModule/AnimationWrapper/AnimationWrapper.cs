using System;
using System.Collections.Generic;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class Log : LSAction
    {
        public string message;
        
        public override void Invoke()
        {
            Debug.Log(message);
        }
    }
    
    [Serializable]
    public class ParticleSystemPlay : LSAction
    {
        public ParticleSystem particleSystem;
        
        public override void Invoke()
        {
            particleSystem.Stop();
            particleSystem.Play();
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                lastTime = EditorApplication.timeSinceStartup;
                EditorApplication.update -= Play;
                EditorApplication.update += Play;
            }
#endif
        }

#if UNITY_EDITOR
        private double lastTime;
        
        private void Play()
        {
            var t = EditorApplication.timeSinceStartup;
            
            if (World.IsEditMode)
            {
                if (particleSystem == null)
                {
                    EditorApplication.update -= Play;
                    return;
                }
                
                particleSystem.Simulate((float)(t - lastTime), true, false);
                SceneView.RepaintAll();
                GameViewExt.Repaint();
            }
            else
            {
                EditorApplication.update -= Play;
            }
            
            lastTime = t;
        }
#endif
    }
    
    [ExecuteAlways]
    [RequireComponent(typeof(Animation))]
    [DefaultExecutionOrder(-1000)]
    public partial class AnimationWrapper : MonoBehaviour
    {
        [SerializeReference] public List<Handler> handlers;
        [PropertySpace(SpaceBefore = 10)]
        [SerializeReference] private List<LSAction> actions;
        private Animation animation;
        
        private AnimationClip lastRuntimeClip;
        private AnimationClip lastClip;

        public Animation Animation => animation;
        
        private void Awake()
        {
            animation = GetComponent<Animation>();
        }
        
        public void Call(string expression)
        {
            foreach (var action in actions.BySelectEx(expression))
            {
                action.Invoke();
            }
        }

        private (AnimationClip clip, float time) GetClip()
        {
            AnimationClip clip = null;
            float time = 0;
            
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                clip = Window.animationClip;
                time = Window.time;
                goto ret;
            }
#endif
            foreach (AnimationState state in animation)
            {
                if (animation[state.name].enabled)
                {
                    clip = state.clip;
                    time = state.time;
                    break;
                }
            }

            ret:
            return (clip, time);
        }
        
        private void OnDidApplyAnimationProperties()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                Handle_Editor();
                return;
            }
#endif
            Handle();
        }

        private void Handle()
        {
            var (clip, time) = GetClip();
            var notEqual = lastRuntimeClip != clip || isPlayCalled;
            
            isPlayCalled = false;
#if UNITY_EDITOR
            TryCallEvent(clip, time);
#endif
            if (notEqual)
            {
                StopHandlers();
                
                foreach (var handler in handlers)
                {
                    handler.Start();
                }
            }
            
            foreach (var handler in handlers)
            {
                handler.Handle();
            }

            lastRuntimeClip = clip;
        }

        private void StopHandlers()
        {
            foreach (var handler in handlers)
            {
                handler.Stop();
            }
        }
        

#if UNITY_EDITOR
        
        private float lastTime = -1;
        
        private void TryCallEvent(AnimationClip clip, float time)
        {
            if(World.IsPlaying) return;
            var events = AnimationUtility.GetAnimationEvents(clip);
            if(events.Length == 0) return;

            if (time == 0)
            {
                lastTime = -1;
            }
            
            if (time > lastTime)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    var e = events[i];
                    var eTime = e.time;
                    
                    if (eTime <= lastTime) continue;

                    if (time >= eTime)
                    {
                        e.Invoke(this);
                    }
                }
            }
            else
            {
                for (int i = events.Length - 1; i >= 0; i--)
                {
                    var e = events[i];
                    var eTime = e.time;
                    
                    if (eTime >= lastTime) continue;

                    if (time <= eTime)
                    {
                        e.Invoke(this);
                    }
                }
            }

            lastTime = time;
        }
        
        private static AnimationWindow window;
        private static AnimationWindow Window => window ??= EditorWindow.GetWindow<AnimationWindow>(null, false);
        
        public AnimationWrapper()
        {
            Patchers.AnimationWindowControl.time.Called += OnWindowTimeChanged;
            Patchers.AnimationWindowControl.PlaybackUpdate.Called += OnWindowTimeChanged;
            Patchers.AnimEditor.OnSelectionChanged.Called += OnSelectionChanged;
            Patchers.AnimEditor.previewing.Called += OnPreviewingChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnWindowTimeChanged() => OnWindowTimeChanged(0);

        private void OnWindowTimeChanged(float obj)
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimationWindowControl.time.Called -= OnWindowTimeChanged;
                Patchers.AnimationWindowControl.PlaybackUpdate.Called -= OnWindowTimeChanged;
                return;
            }

            if(Window == null) return;
            
            var activeRootGameObject = (GameObject)LSReflection.Eval(Window, "state.activeRootGameObject");

            if (activeRootGameObject == null || activeRootGameObject != gameObject)
            {
                return;
            }

            Handle_Editor();
        }

        private void OnEditorUpdate()
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }

            if (isAnimationCalled)
            {
                isAnimationCalled = false;
                Handle();
            }
        }

        private void OnDestroy()
        {
            Patchers.AnimationWindowControl.time.Called -= OnWindowTimeChanged;
            Patchers.AnimationWindowControl.PlaybackUpdate.Called -= OnWindowTimeChanged;
            Patchers.AnimEditor.OnSelectionChanged.Called -= OnSelectionChanged;
            Patchers.AnimEditor.previewing.Called -= OnPreviewingChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnPreviewingChanged(bool state)
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimEditor.previewing.Called -= OnPreviewingChanged;
                return;
            }

            if (!state)
            {
                StopHandlers();
                lastRuntimeClip = null;
                lastTime = -1;
            }
        }
        
        private object lastAnimPlayer;
        
        private void OnSelectionChanged()
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimEditor.OnSelectionChanged.Called -= OnSelectionChanged;
                return;
            }
            
            if(Window == null) return;
            
            var animationPlayer = LSReflection.Eval(Window, "state.activeAnimationPlayer");
            var (clip, time) = GetClip();
            
            if (animationPlayer != lastAnimPlayer || lastClip != clip)
            {
                lastTime = -1;
                lastRuntimeClip = null;
                StopHandlers();
            }
            
            lastClip = clip;
            lastAnimPlayer = animationPlayer;
        }

        private static AnimationWrapper currentInspected;
        
        [OnInspectorGUI]
        private void OnInspectorGui()
        {
            currentInspected = this;
        }

        private bool isAnimationCalled;
        private void Handle_Editor()
        {
            isAnimationCalled = true;
        }
#endif
    }
}