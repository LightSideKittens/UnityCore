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
            //Debug.Log($"{GetHashCode()} Rb2DAddPosition Start");
            base.OnStart();
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                startPosition = rigidbody.transform.position;
                return;
            }
#endif
            startPosition = rigidbody.position;
        }
        
        protected override void OnHandle()
        {
            //Debug.Log($"{GetHashCode()} Rb2DAddPosition OnHandle {value}");
            var v = startPosition + value;
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                rigidbody.transform.position = v;
                return;
            }
#endif
            rigidbody.position = v;
        }

#if UNITY_EDITOR
        protected override void OnStop()
        {
            //Debug.Log($"{GetHashCode()} Rb2DAddPosition Stop");
            base.OnStop();

            if (!World.IsPlaying)
            {
                rigidbody.transform.position = startPosition;
            }
        }
#endif
    }
}