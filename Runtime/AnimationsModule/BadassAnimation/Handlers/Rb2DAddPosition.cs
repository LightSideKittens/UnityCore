using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class Rb2DAddPosition : BadassAnimation.Vector2Handler
    {
        [SerializeField] private Rigidbody2D rigidbody;
        [SerializeField] private bool useWorldSpace;
        private Transform transform;
        private Vector2 startPosition;
        
        protected override string Label => "Position";

        public override Object Target => rigidbody;
        
        protected override void OnStart()
        {
            transform = rigidbody.transform;
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                startPosition = transform.position;
                return;
            }
#endif
            startPosition = rigidbody.position;
        }
        
        protected override void OnHandle()
        {
            var target = startPosition;
            
            if (useWorldSpace)
            {
                target += value;
            }
            else
            {
                var newValue = transform.TransformDirection(value);
                target.x += newValue.x;
                target.y += newValue.y;
            }
            
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                transform.position = target;
                return;
            }
#endif
            rigidbody.position = target;
        }

#if UNITY_EDITOR
        protected override void OnStop()
        {
            if (World.IsEditMode)
            {
                transform.position = startPosition;
            }
        }
#endif
    }
}