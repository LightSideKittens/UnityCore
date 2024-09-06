using System;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class Rb2DAddPosition : AnimationWrapper.Handler<Vector2>
    {
        [SerializeField] private Rigidbody2D rigidbody;
        private Vector2 startPosition;
        
        protected override string Label => "Position";
        
        protected override void OnStart()
        {
            base.OnStart();
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                startPosition = rigidbody.transform.position;
                return;
            }
#endif
            startPosition = rigidbody.position;
        }
        
        protected override void OnHandle()
        {
            var target = startPosition + value;
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                rigidbody.transform.position = target;
                return;
            }
#endif
            rigidbody.position = target;
        }

#if UNITY_EDITOR
        protected override void OnStop()
        {
            base.OnStop();

            if (World.IsEditMode)
            {
                rigidbody.transform.position = startPosition;
            }
        }
#endif
    }
}