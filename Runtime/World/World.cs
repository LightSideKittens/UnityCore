using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Compilation;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.Build;
#endif

namespace LSCore
{
    [Conditional("UNITY_EDITOR")]
    public class ResetStaticAttribute : Attribute
    {
        public bool useNew;
        public Type specifiedType;

        public ResetStaticAttribute(){}
        
        public ResetStaticAttribute(Type specifiedType, bool useNew)
        {
            this.specifiedType = specifiedType;
            this.useNew = useNew;
        }
        
        public ResetStaticAttribute(bool useNew) => this.useNew = useNew;
        public ResetStaticAttribute(Type specifiedType) => this.specifiedType = specifiedType;
    }
    
    [DefaultExecutionOrder(-999)]
    public class World : MonoBehaviour
    {
        public static event Action ApplicationPaused;
        public static event Action ApplicationResumed;
        public static event Action Updated;
        public static event Action FixedUpdated;
        public static event Action PreRendering;
        public static event Action CanvasPreRendering;
        
        private static readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private static bool isCreated;
        private static World instance;
        public static float FrameRate => 1f / Time.unscaledDeltaTime;
        public static float DeltaTime { get; set; }

        private static string userId;
        public static string UserId
        {
            get
            {
                if(!string.IsNullOrEmpty(userId)) return userId;
                
                var id = PlayerPrefs.GetString("UserId", Guid.NewGuid().ToString("N"));
                PlayerPrefs.SetString("UserId", id);
                return id;
            }
        }

#if UNITY_EDITOR
        public class BuildHooks : BuildPlayerProcessor
        {
            public int callbackOrder => 0;

            public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
            {
                IsBuilding = true;
                Building.SafeInvoke();
                EditorApplication.update += OnUpdate;
            }

            private void OnUpdate()
            {
                EditorApplication.update -= OnUpdate;
                IsBuilding = false;
            }
        }
        
        public sealed class BuildDoneHook : IPostprocessBuildWithReport
        {
            public int callbackOrder => 0;
            
            public void OnPostprocessBuild(BuildReport report)
            {
                Built.SafeInvoke();
            }
        }
        
        public static event Action Creating;
        public static event Action Created;
        public static event Action Destroyed;
        public static event Action Building;
        public static event Action Built;
        public static event Action Recompiled;
        private static int lastInstanceId; 
        public static int instanceId; 
        public static bool IsPlaying { get; private set; }
        public static bool IsBuilding { get; private set; }
        public static bool IsCompiling => EditorApplication.isCompiling;
        public static bool IsEditMode => !IsPlaying;
        public static bool IsPlayModeDisabling { get; private set; }
#endif
        
        static World()
        {
            Camera.onPreRender += OnPreRendering;
            Canvas.preWillRenderCanvases += OnCanvasPreRendering;
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished += x =>
            {
                Recompiled?.Invoke();
            };
            
            EditorApplication.playModeStateChanged += change =>
            {
                if (change == PlayModeStateChange.ExitingPlayMode)
                {
                    IsPlayModeDisabling = true;
                }
                else if (change == PlayModeStateChange.ExitingEditMode)
                {
                    IsPlayModeDisabling = false;
                }
            };
#endif
        }

        private static int renderedFrame = -1;
        
        private static void OnPreRendering(Camera _)
        {
            var frame = Time.frameCount;
            if(renderedFrame == frame) return;
            renderedFrame = frame;
            PreRendering?.Invoke();
        }

        private static void OnCanvasPreRendering() => CanvasPreRendering?.Invoke();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            renderedFrame = -1;
            Application.targetFrameRate = 120;
            DOTween.SetTweensCapacity(1000, 1000);
#if UNITY_EDITOR
            var fields = TypeCache.GetFieldsWithAttribute<ResetStaticAttribute>();
            foreach (var fieldInfo in fields)
            {
                var resetAtt = fieldInfo.GetCustomAttribute<ResetStaticAttribute>();
                object value = null;
                if (resetAtt.useNew)
                {
                    Type type = resetAtt.specifiedType ?? fieldInfo.FieldType;
                    try
                    {
                        value = Activator.CreateInstance(type);
                    }
                    catch{}
                   
                }
                fieldInfo.SetValue(null, value);
            }
            Creating.SafeInvoke();
#endif
            var go = new GameObject(nameof(World));
            instance = go.AddComponent<World>();
            DontDestroyOnLoad(go);
#if UNITY_EDITOR
            instanceId = instance.GetInstanceID();
            IsPlaying = true;
            Created.SafeInvoke();
#endif
            Burger.Log("[World] Created");
        }

        private void Update()
        {
            DeltaTime = Time.deltaTime;
            Updated?.Invoke();
            CallActions();
        }
        
        private void FixedUpdate()
        {
            FixedUpdated?.Invoke();
        }
        
        private void OnDestroy()
        {
            Burger.Log("[World] OnDestroy");
#if UNITY_EDITOR
            lastInstanceId = instanceId;
            IsPlaying = false;
            Destroyed.SafeInvoke();
#endif
            Burger.logToFile = false;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ApplicationPaused.SafeInvoke();
            }
            else
            {
                ApplicationResumed.SafeInvoke();
            }
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            IsPlaying = false;
#endif
            OnApplicationPause(true);
        }
        
        
        private static readonly ConcurrentQueue<Action> executionQueue = new();
        
        public static void CallInMainThread(Action action)
        {
            executionQueue.Enqueue(action);
        }
        
        private static void CallActions()
        {
            while (executionQueue.Count > 0)
            {
                if (executionQueue.TryDequeue(out var action))
                {
                    action();
                }
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void NeedReset(ref bool needReset)
        {
#if UNITY_EDITOR
            needReset = lastInstanceId != instanceId;
#endif
        }
        
        public static T ResetStatic<T>(ref T value)
        {
#if UNITY_EDITOR
            if(lastInstanceId != instanceId) value = default;
            return value;
#endif
            return value;
        }

        public static Coroutine BeginCoroutine(IEnumerator routine)
        {
            if (instance == null) return null;
            return instance.StartCoroutine(routine);
        }
    }
}