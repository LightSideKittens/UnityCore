using System;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class TransformScale : BaseTransformHandler
    {
        [SerializeField] private bool add;
        
#if UNITY_EDITOR
        protected override string Label => "Scale";
        protected override string PropertyPath => "m_LocalScale";
#endif
        protected override Vector3 GetStartValue() => transform.localScale;

        protected override void OnHandle()
        {
            var target = value;
            if(add) target += StartValue;
            transform.localScale = target;
        }
        
        protected override void OnStop()
        {
            if (!add)
            {
                transform.localScale = StartValue;
            }
        }
    }
}