using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class MoveIt
{
    [Serializable]
    public abstract class Handler
    {
        public static class PropPaths
        {
            public const string LocalPosition = "m_LocalPosition";
            public const string LocalScale = "m_LocalScale";
            public const string LocalEulerAnglesHint = "m_LocalEulerAnglesHint";
            public const string LocalRotation = "m_LocalRotation";
        }
        
        [GenerateGuid(Hide = true)] public string guid;
        [NonSerialized] public List<HandlerEvaluateData> evaluators = new();
        public abstract UniDict<int, Object> Objects { get; set; }
        
        protected bool isStarted;

#if UNITY_EDITOR
        protected bool CanUse => isPreview && Target != null;
        [NonSerialized] public bool isPreview = true;
#endif

        [HideInInspector] public string fullTypeName;
        public abstract Object Target { get; }
        
        
        public void Start()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }
#endif
            if (!isStarted)
            {
                isStarted = true;
                
                for (int i = 0; i < evaluators.Count; i++)
                {
                    var evaluator = evaluators[i];
                    evaluator.Evaluate();
                    evaluator.isDiff = true;
                }
                
                OnStart();
                Handle();
            }
        }

        public void Stop()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }
#endif

            if (isStarted)
            {
                isStarted = false;
                OnStop();
            }
        }

        [NonSerialized] public bool isDiff;
        public abstract void Handle();

        protected virtual void OnStart(){}
        protected abstract void OnStop();

        public bool TryGetEvaluator(string key, out HandlerEvaluateData evaluator)
        {
            var toRemove = evaluators.FirstOrDefault(x => x.property == key);
            evaluator = toRemove;
            return evaluator != null;
        }
        
        public void AddEvaluator(HandlerEvaluateData evaluator)
        {
            evaluators.Add(evaluator);
        }

        public void ClearEvaluators()
        {
            evaluators.Clear();
        }

        public virtual bool TryGetPropBindingData(out Object obj, out GameObject go)
        {
            obj = default;
            go = default;
            return false;
        }
        
        
#if UNITY_EDITOR
        public abstract Type ValueType { get; }
        public abstract string HandlerName { get; }

        [GUIColor(1f, 0.54f, 0.16f)]
        [ShowInInspector] private bool gizmos;

        public void DrawGizmos()
        {
            if (gizmos)
            {
                OnDrawGizmos();
            }
        }

        protected virtual void OnDrawGizmos()
        {
        }

        public virtual void OnSceneGUI()
        {
        }

        public abstract void TrimModifications(List<UndoPropertyModification> modifications);
        public abstract void StartAnimationMode();
        
        private SerializedObject serializedObject;

        public SerializedProperty FindProperty(string property)
        { 
            serializedObject ??= new SerializedObject(Target);
            return serializedObject.FindProperty(property);
        }
        
#endif
        public virtual void OnLooped() { }
    }

    [Serializable]
    public abstract class Handler<T> : Handler
    {
#if UNITY_EDITOR
        public override Type ValueType => typeof(T);
        public override string HandlerName => GetType().Name;
        
#endif
        
        public sealed override void Handle()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }

            if (!isStarted)
            {
                Start();
                for (int i = 0; i < evaluators.Count; i++)
                {
                    if (!evaluators[i].isDiff) continue;
                    isDiff = true;
                    break;
                }
                return;
            }
#endif
            
            isDiff = false;
            
            for (int i = 0; i < evaluators.Count; i++)
            {
                if (!evaluators[i].isDiff) continue;
                isDiff = true;
                break;
            }
            
            OnHandle();
        }

        protected abstract void OnHandle();
    }
}
