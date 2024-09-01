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
        
        public override void Start()
        {
            Debug.Log($"{GetHashCode()} Rb2DAddPosition Start");
            base.Start();
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
        public override void Stop()
        {
            Debug.Log($"{GetHashCode()} Rb2DAddPosition Stop");
            base.Stop();

            if (!World.IsPlaying)
            {
                rigidbody.transform.position = startPosition;
            }
        }
#endif
    }
}