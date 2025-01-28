using System;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class TransformRotation : BaseTransformHandler
    {
        [SerializeField] private bool useWorldSpace;
        [SerializeField] private bool add;
        private Vector3 startRotation;
        
#if UNITY_EDITOR
        protected override string Label => "Rotation";
#endif
        
        protected override void OnStart()
        {
            if (useWorldSpace)
            {
                startRotation = transform.eulerAngles;
            }
            else
            {
                startRotation = transform.localEulerAngles;
            }
        }
        
        protected override void OnHandle()
        {
            var target = value;
            if(add) target += startRotation;
            
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
                    transform.eulerAngles = startRotation;
                }
                else
                {
                    transform.localEulerAngles = startRotation;
                }
            }
        }
    }
}