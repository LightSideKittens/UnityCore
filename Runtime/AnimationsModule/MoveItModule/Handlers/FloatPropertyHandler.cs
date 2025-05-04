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
        [HideInInspector] public UniDict<int, Object> objects = null;

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
        
        public override bool TryGetPropBindingData(out Object obj, out GameObject go)
        {
            obj = Target;
            go = GO;
            return true;
        }

#if UNITY_EDITOR
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            foreach (var evaluator in evaluators)
            {
                HandlerEvaluator.TrimModifications(Target, modifications, evaluator, evaluator.rawProperty);
            }
        }

        public override void StartAnimationMode()
        {
            foreach (var evaluator in evaluators)
            {
                HandlerEvaluator.StartAnimationMode(Target, evaluator, evaluator.rawProperty);
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
            fullTypeName = target.GetType().FullName;
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
            fullTypeName = target.GetType().FullName;
            comp = target;
        }
    }
}