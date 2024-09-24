using System;
using LSCore.AnimationsModule;
using UnityEngine;
using UnityEngine.Animations;

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class RotateToTarget : AnimationWrapper.Handler<float>
    {
        public Transform transform;
        public Transform target;
        public FindTargetFactory findTargetFactory;
        public Axis rotationAxis = Axis.Y;
        
        private FindTargetComp findTargetComp;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

        protected override string Label => "Normalized value";

        private Transform Target
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    return target;
                }
#endif
                findTargetComp.Find(out target);
                return target;
            }
        }
        
        private void InitFindTarget()
        {
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(transform);
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            
            if (World.IsPlaying)
            {
                InitFindTarget();
            }
            
            initialRotation = transform.rotation;
            
            var directionToTarget = (Target.position - transform.position).normalized;

            switch (rotationAxis)
            {
                case Axis.Z:
                    targetRotation = Quaternion.LookRotation(directionToTarget, transform.up);
                    break;
                case Axis.X:
                    targetRotation = Quaternion.LookRotation(Vector3.Cross(transform.up, directionToTarget), transform.up);
                    break;
                case Axis.Y:
                    targetRotation = Quaternion.LookRotation(transform.forward, directionToTarget);
                    break;
            }
        }

        protected override void OnHandle()
        {
            transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, value);
        }
        
#if UNITY_EDITOR
        protected override void OnStop()
        {
            base.OnStop();

            if (World.IsEditMode)
            {
                transform.rotation = initialRotation;
            }
        }
#endif
    }
}