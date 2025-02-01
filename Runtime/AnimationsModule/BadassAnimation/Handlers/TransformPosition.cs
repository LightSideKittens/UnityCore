using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public abstract class BaseTransformHandler : BadassAnimation.Vector3Handler
    {
        public Transform transform;
        
        public sealed override Object Target => transform;
    }

    [Serializable]
    public class TransformPosition : BaseTransformHandler
    {
        [SerializeField] private bool useWorldSpace;
        [SerializeField] private bool add;
        
#if UNITY_EDITOR
        protected override string Label => "Position";
        protected override string PropertyPath => "m_LocalPosition";
#endif

        protected override Vector3 GetStartValue() => useWorldSpace ? transform.position : transform.localPosition;
        
        protected override void OnHandle()
        {
            var target = value;
            if(add) target += StartValue;
            
            if (useWorldSpace)
            {
                transform.position = target;
            }
            else
            {
                transform.localPosition = target;
            }
        }
        
        protected override void OnStop()
        {
            if (!add)
            {
                if (useWorldSpace)
                {
                    transform.position = StartValue;
                }
                else
                {
                    transform.localPosition = StartValue;
                }
            }
        }
    }
}