using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
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
        [NonSerialized] public List<(string property, HandlerEvaluateData evaluator)> evaluators = new();

        protected int evaluatorsCount;
        protected bool isStarted;

#if UNITY_EDITOR
        protected bool CanUse => isPreview && Target != null && applyEvaluationResult != null;
        [NonSerialized] public bool isPreview = true;
#endif

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
                    var evaluator = evaluators[i].evaluator;
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
            evaluator = toRemove.evaluator;
            return evaluator != null;
        }

        public bool AddEvaluator(string key, MoveItCurve curve, out HandlerEvaluateData evaluator)
        {
            var result = evaluators.All(x => x.property != key);
            evaluator = null;
            if (result)
            {
                evaluator = new HandlerEvaluateData{curve = curve};
                applyEvaluationResult += GetApplyEvaluationResultAction(key, evaluator);
                evaluators.Add((key, evaluator));
                evaluatorsCount = evaluators.Count;
            }

            return result;
        }
        
        public bool RemoveEvaluator(string key)
        {
            applyEvaluationResult -= GetApplyEvaluationResultAction(key, null);
            var toRemove = evaluators.FirstOrDefault(x => x.property == key);
            if (toRemove.evaluator != null)
            {
                evaluators.Remove(toRemove);
                evaluatorsCount = evaluators.Count;
                return true;
            }
            
            return false;
        }

        public void ClearEvaluators()
        {
            for (int i = 0; i < evaluators.Count; i++)
            {
                evaluators[i].evaluator.Reset();
            }
        }
        
        protected Action applyEvaluationResult;
        protected abstract Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator);
        
        public virtual bool TryGetPropBindingData(out (Object obj, GameObject go, (string propName, bool isRef)[] propData) data)
        {
            data = default;
            return false;
        }

        public virtual float GetBindableValue(int index) => 0f;

#if UNITY_EDITOR
        public abstract Type ValueType { get; }
        public abstract string HandlerName { get; }

        [GUIColor(1f, 0.54f, 0.16f)]
        [ShowInInspector] private bool gizmos;

        [NonSerialized] public MoveIt animation;

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

        public void TrimModifications(List<UndoPropertyModification> modifications)
        {
            OnTrimModifications(modifications);
            StartAnimationMode();
        }
        
        protected abstract void OnTrimModifications(List<UndoPropertyModification> modifications);
        public abstract void StartAnimationMode();

        protected SerializedProperty FindProperty(string propertyName)
        {
            var clip = animation.Clip;
            if (animation.TryGetData(clip.guid, out var data))
            {
                var so = new SerializedObject(animation);
                var dataProp = so.FindProperty("data");
                SerializedProperty targetDataProp = null; 
                foreach (SerializedProperty prop in dataProp)
                {
                    var targetData = (Data)prop.boxedValue;
                    if (targetData.clip.guid == clip.guid)
                    {
                        targetDataProp = prop;
                        break;
                    }
                }

                if (targetDataProp != null)
                {
                    var handlers = targetDataProp.FindPropertyRelative("handlers");
                    SerializedProperty targetHandlerProp = null;
                     
                    foreach (SerializedProperty handlerProp in handlers)
                    {
                        var targetHandler = (Handler)handlerProp.managedReferenceValue;
                        if (targetHandler.guid == guid)
                        { 
                            targetHandlerProp = handlerProp;
                            break;   
                        }
                    }

                    if (targetHandlerProp != null)
                    {
                        var targetProp = targetHandlerProp.FindPropertyRelative(propertyName);
                        return targetProp;
                    }
                }
            }
            
            return null;
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
                applyEvaluationResult();
                return;
            }
#endif
            
            isDiff = false;
            applyEvaluationResult();
            
            if (!isDiff) return;
            
            OnHandle();
        }

        protected abstract void OnHandle();
    }
}
