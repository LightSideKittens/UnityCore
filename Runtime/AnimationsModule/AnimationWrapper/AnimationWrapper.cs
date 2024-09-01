using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    [ExecuteAlways]
    [RequireComponent(typeof(AnimationWindow))]
    public partial class AnimationWrapper : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct Data
        {
            [Required]
            [OnValueChanged("OnClipChanged")]
            public AnimationClip clip;
            [SerializeReference] public List<Handler> handlers;

            private void OnClipChanged()
            {
                currentInspected.FillHandlers();
            }
        }

        [SerializeField] private Data[] handlers;
        [PropertySpace(SpaceBefore = 10)]
        [SerializeReference] private List<LSAction> actions;
        private Animation animation;

        private readonly Dictionary<AnimationClip, List<Handler>> handlersByClip = new();
        private AnimationClip lastRuntimeClip;
        private AnimationClip lastClip;

        public Animation Animation => animation;
        
        private void Awake()
        {
            animation = GetComponent<Animation>();
        }
        
        public void Call(string expression)
        {
            if (int.TryParse(expression, out var ind))
            {
                actions[ind].Invoke();
            }
            else if (expression.Contains(','))
            {
                var split = expression.Split(',');

                for (int i = 0; i < split.Length; i++)
                {
                    actions[int.Parse(split[i])].Invoke();
                }
            }
            else if(expression.TryParseIndex(out var index))
            {
                actions[index].Invoke();
            }
            else if(expression.TryParseRange(out var range))
            {
                var a = actions.Range(range);

                for (int i = 0; i < a.Count; i++)
                {
                    a[i].Invoke();
                }
            }
        }

        private (AnimationClip clip, float time) GetClip()
        {
            AnimationClip clip = null;
            float time = 0;
            
#if UNITY_EDITOR
            if (!World.IsPlaying)
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

        private float lastTime = -1;
        
        private void OnDidApplyAnimationProperties()
        {
            var (clip, time) = GetClip();
            var notEqual = lastRuntimeClip != clip || isPlayCalled;
            
            isPlayCalled = false;
            if (Mathf.Approximately(lastTime, time) && !notEqual)
            {
                lastTime = time;
                return;
            }
            
            lastTime = time;
            TryCallEvent(clip, time);
            
            var currentClipHandlers = handlersByClip[clip];

            if (notEqual)
            {
                StopLastClip(lastRuntimeClip);
                
                foreach (var handler in currentClipHandlers)
                {
                    handler.Start();
                }
            }
            
            foreach (var handler in currentClipHandlers)
            {
                handler.Handle();
            }

            lastRuntimeClip = clip;
        }

        private void StopLastClip(AnimationClip clip)
        {
            if (clip != null)
            {
                if (handlersByClip.TryGetValue(clip, out var lastClipHandlers))
                {
                    foreach (var handler in lastClipHandlers)
                    {
                        handler.Stop();
                    }   
                }
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            FillHandlers();
        }

        private void FillHandlers()
        {
            handlersByClip.Clear();

            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                handlersByClip.Add(h.clip, h.handlers);
            }
        }

        private AnimationEvent[] events;
        private AnimationClip eventClip;
        
        [Conditional("UNITY_EDITOR")]
        private void TryCallEvent(AnimationClip clip, float time)
        {
            if(World.IsPlaying) return;
            events = eventClip != clip ? AnimationUtility.GetAnimationEvents(clip) : events;
            eventClip = clip;
            if(events.Length == 0 || time > events[^1].time || time < events[0].time) return;
            events.ClosestBinarySearch(e => e.time, time).Invoke(this);
        }

#if UNITY_EDITOR

        private static AnimationWindow window;
        private static AnimationWindow Window => window ??= EditorWindow.GetWindow<AnimationWindow>();
        
        public AnimationWrapper()
        {
            Debug.Log($"Init {GetHashCode()}");
            Patchers.AnimEditor.OnSelectionChanged.Changed += OnSelectionChanged;
        }
        
        private void OnDestroy()
        {
            Debug.Log($"OnDestroy {GetHashCode()}");
            Patchers.AnimEditor.OnSelectionChanged.Changed -= OnSelectionChanged;
        }
        
        
        private object lastAnimPlayer;
        
        private void OnSelectionChanged()
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimEditor.OnSelectionChanged.Changed -= OnSelectionChanged;
                return;
            }
            
            if(Window == null) return;
            
            var animationPlayer = LSReflection.Eval(Window, "state.activeAnimationPlayer");
            var (clip, time) = GetClip();
            
            if (animationPlayer != lastAnimPlayer || lastClip != clip)
            {
                lastRuntimeClip = null;
                StopLastClip(lastClip);
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
#endif
    }
}