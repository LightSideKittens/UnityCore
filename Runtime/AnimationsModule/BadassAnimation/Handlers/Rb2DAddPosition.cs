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

#if UNITY_EDITOR
        protected override string Label => "Position";
        protected override string PropertyPath { get; }
#endif

        public override Object Target => rigidbody;

        protected override Vector2 GetStartValue()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                return transform.position;
            }
#endif
            return rigidbody.position;
        }

        protected override void OnStart()
        {
            transform = rigidbody.transform;
        }
        
        protected override void OnHandle()
        {
            var target = StartValue;
            
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


        protected override void OnStop()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                transform.position = StartValue;
            }
#endif
        }
    }
}