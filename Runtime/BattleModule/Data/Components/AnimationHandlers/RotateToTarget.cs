using System;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class RotateToTarget : AnimationWrapper.Handler<float>
    {
        public Transform transform;
        public FindTargetFactory findTargetFactory;
        public Axis rotationAxis = Axis.Y;

        private FindTargetComp findTargetComp;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

#if UNITY_EDITOR
        public bool forceUseTarget;
        [LabelText("Target for Editor testing")]
        public Transform target;
#endif

        protected override string Label => "Normalized value";

        private Transform Target
        {
            get
            {
#if UNITY_EDITOR
                if (target != null || forceUseTarget)
                {
                    return target;
                }
#endif
                findTargetComp.Find(out var target1);
                return target1;
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
            
            if (
#if UNITY_EDITOR
            target != null && !forceUseTarget &&
#endif
                World.IsPlaying)
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