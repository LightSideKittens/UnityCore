using System;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class TransformRotation : BaseTransformHandler
    {
        [SerializeField] private bool useWorldSpace;
        [SerializeField] private bool add;
        
#if UNITY_EDITOR
        protected override string Label => "Rotation";
        protected override string PropertyPath => "m_LocalRotation";
#endif
        protected override Vector3 GetStartValue() => useWorldSpace ? transform.eulerAngles : transform.localEulerAngles;

        protected override void OnHandle()
        {
            var target = value;
            if(add) target += StartValue;
            
            if (useWorldSpace)
            {
                transform.eulerAngles = target;
            }
            else
            {
                transform.localEulerAngles = target;
            }
        }
        
        protected override void OnStop()
        {
            if (!add)
            {
                if (useWorldSpace)
                {
                    transform.eulerAngles = StartValue;
                }
                else
                {
                    transform.localEulerAngles = StartValue;
                }
            }
        }
    }
}