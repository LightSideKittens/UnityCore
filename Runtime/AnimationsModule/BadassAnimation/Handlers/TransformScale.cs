using System;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class TransformScale : BaseTransformHandler
    {
        [SerializeField] private bool add;
        private Vector3 startScale;
        
#if UNITY_EDITOR
        protected override string Label => "Scale";
#endif
        
        protected override void OnStart()
        {
            startScale = transform.localScale;
        }
        
        protected override void OnHandle()
        {
            var target = value;
            if(add) target += startScale;
            transform.localScale = target;
        }
        
        protected override void OnStop()
        {
            if (!add)
            {
                transform.localScale = startScale;
            }
        }
    }
}