using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MoveIt;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    public interface IFloatPropertyHandler
    {
        
    }
    
    [Serializable]
    public abstract class FloatPropertyHandler<T> : Handler<float>, IFloatPropertyHandler where T : Object
    {
        private int evaluatorIndex;
        public abstract GameObject GO { get; }
        public abstract void SetTarget(T target);

        protected override void OnStart()
        {
        }
        
        protected override void OnHandle()
        {
            handlersBuffer.Add(this);
        }

        protected override void OnStop()
        {
            handlersBuffer.Add(this);
        }

        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator) => X;
        private void X()
        {
            evaluatorIndex = (evaluatorIndex + 1) % evaluators.Count;
            IsDiff |= evaluators[evaluatorIndex].evaluator.isDiff;
        }

        public override bool TryGetPropBindingData(out (Object obj, GameObject go, (string propName, bool isRef)[] propData) data)
        {
            data = default;
            data.obj = Target;
            data.go = GO;
            data.propData = new ValueTuple<string, bool>[evaluators.Count];

            for (int i = 0; i < evaluators.Count; i++)
            {
                data.propData[i] = (evaluators[i].property, false);
            }
            
            return true;
        }

        protected override void OnTrimModifications(List<UndoPropertyModification> modifications)
        {
            foreach (var property in evaluators)
            {
                HandlerEvaluateData.TrimModifications(Target, modifications, property.evaluator, property.property);
            }
        }

        public override void StartAnimationMode()
        {
            foreach (var property in evaluators)
            {
                var evaluator = property.evaluator;
                HandlerEvaluateData.StartAnimationMode(Target, evaluator, property.property, evaluator.startY);
            }
        }
    }

    [Serializable]
    public class GOFloatPropertyHandler : FloatPropertyHandler<GameObject>
    {
        public GameObject go;
        public override Object Target => go;
        public override GameObject GO => go;
        
        public override void SetTarget(GameObject target)
        {
            go = target;
        }
    }
    
    [Serializable]
    public class CompFloatPropertyHandler : FloatPropertyHandler<Component>
    {
        public Component comp;
        public override Object Target => comp;
        public override GameObject GO => comp.gameObject;
        
        public override void SetTarget(Component target)
        {
            comp = target;
        }
    }
}