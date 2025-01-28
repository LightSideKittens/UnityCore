using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public abstract class BaseTransformHandler : BadassAnimation.Vector3Handler
    {
        public Transform transform;
        
        public override Object Target => transform;
    }

    [Serializable]
    public class TransformPosition : BaseTransformHandler
    {
        [SerializeField] private bool useWorldSpace;
        [SerializeField] private bool add;
        private Vector3 startPosition;
        
        protected override string Label => "Position";

        protected override void OnStart()
        {
            if (useWorldSpace)
            {
                startPosition = transform.position;
            }
            else
            {
                startPosition = transform.localPosition;
            }

            value = startPosition;
        }
        
        protected override void OnHandle()
        {
            var target = value;
            if(add) target += startPosition;
            
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
                    transform.position = startPosition;
                }
                else
                {
                    transform.localPosition = startPosition;
                }
            }
        }
    }
}