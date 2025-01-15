using System;
using UnityEngine;

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
        
        protected override void OnStart()
        {
            base.OnStart();
            
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                if (rigidbody != null)
                {
                    transform = rigidbody.transform;
                    startPosition = transform.position;
                }
                return;
            }
#endif
            
            transform = rigidbody.transform;
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
#if UNITY_EDITOR
                if (World.IsEditMode && transform == null)
                {
                    return;
                }
#endif
                var newValue = transform.TransformDirection(value);
                target.x += newValue.x;
                target.y += newValue.y;
            }
            
#if UNITY_EDITOR
            if (World.IsEditMode && transform != null)
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
            base.OnStop();

            if (World.IsEditMode && transform != null)
            {
                transform.position = startPosition;
            }
        }
#endif
    }
}