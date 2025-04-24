using System;
using System.Collections.Generic;
using LSCore.DataStructs;
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
        public UniDict<int, Object> objects = null;

        public override UniDict<int, Object> Objects
        {
            get => objects;
            set => objects = value;
        }
        
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
        
        public override bool TryGetPropBindingData(out (Object obj, GameObject go, (string propName, bool isRef)[] propData) data)
        {
            data = default;
            data.obj = Target;
            data.go = GO;
            data.propData = new ValueTuple<string, bool>[evaluators.Count];

            for (int i = 0; i < evaluators.Count; i++)
            {
                var evaluator = evaluators[i];
                data.propData[i] = (evaluator.property, evaluator.isRef);
            }
            
            return true;
        }

#if UNITY_EDITOR
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            foreach (var evaluator in evaluators)
            {
                HandlerEvaluateData.TrimModifications(Target, modifications, evaluator, evaluator.property);
            }
        }

        public override void StartAnimationMode()
        {
            foreach (var evaluator in evaluators)
            {
                HandlerEvaluateData.StartAnimationMode(Target, evaluator, evaluator.property, evaluator.startY);
            }
        }
#endif

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