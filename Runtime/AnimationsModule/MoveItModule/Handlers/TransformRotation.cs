/*using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MoveIt;

namespace LSCore.AnimationsModule
{
    public interface ITransformHandler
    {
        public Handler Handler { get; }
        public Transform Transform { get; set; }
    }
    
    [Serializable]
    public class TransformRotation : Handler<TransformRotation.Data>, ITransformHandler
    {
        [Serializable]
        public struct Data
        {
            public Vector3 eulerAngles;
            public Quaternion quaternion;

            public static Data Get(Transform transform)
            {
                Data data = new Data();
                data.eulerAngles = transform.localEulerAngles;
                data.quaternion = transform.localRotation;
                return data;
            }

            public void Apply(Transform transform)
            {
                transform.localEulerAngles = eulerAngles;
            }

            public static Data operator +(Data lhs, Data rhs)
            {
                lhs.eulerAngles += rhs.eulerAngles;
                return lhs;
            }
        }
        
        public Vector3HandlerEvaluateData handlerEvaluateData;
        
        public Transform transform;

        public Transform Transform
        {
            get => transform;
            set => transform = value;
        }
        
        public Handler Handler => this;
        public sealed override UnityEngine.Object Target => transform;
        
        [SerializeField] private bool add;

        protected override bool IsDiff { get; set; }

        protected override void OnHandle()
        {
            var target = value;
            if(add) target += StartValue;
            transform.localEulerAngles = target.eulerAngles;
        }

        protected override void OnStop()
        {
            if (!add)
            {
                transform.localEulerAngles = StartValue.eulerAngles;
            }
        }

        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            handlerEvaluateData ??= new Vector3HandlerEvaluateData();
            return handlerEvaluateData.GetApplyEvaluationResultAction(key, evaluator);
        }
        
#if UNITY_EDITOR
        protected override void OnTrimModifications(List<UndoPropertyModification> modifications)
        {
            handlerEvaluateData ??= new Vector3HandlerEvaluateData();
            handlerEvaluateData.TrimModifications(transform, modifications, PropPaths.LocalEulerAnglesHint);
            QuaternionHandlerEvaluateData.Empty.TrimModifications(transform, modifications, PropPaths.LocalRotation);
        }

        public override void StartAnimationMode()
        {
            handlerEvaluateData ??= new Vector3HandlerEvaluateData();
            handlerEvaluateData.StartAnimationMode(transform, PropPaths.LocalEulerAnglesHint, transform.localEulerAngles);
            QuaternionHandlerEvaluateData.Empty.StartAnimationMode(transform, PropPaths.LocalRotation, transform.localRotation);
        }
#endif
    }
}*/