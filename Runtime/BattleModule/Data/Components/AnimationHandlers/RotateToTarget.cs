using System;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector;
using UnityEngine;

public enum Vector3Axis
{
    X,
    Y,
    Z
}

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class RotateToTarget : AnimationWrapper.Handler<float>
    {
        public Transform transform;
        public FindTargetFactory findTargetFactory;
        public Vector3Axis rotationAxis = Vector3Axis.Z;

        private FindTargetComp findTargetComp;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

#if UNITY_EDITOR
        [LabelText("Target for Editor testing")]
        public Transform target;
#endif

        protected override string Label => "Normalized value";

        private Transform Target
        {
            get
            {
#if UNITY_EDITOR
                if (target != null || AnimationTestMode.Is)
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
            target != null && !AnimationTestMode.Is &&
#endif
                World.IsPlaying)
            {
                InitFindTarget();
            }
            
            initialRotation = transform.rotation;
        }

        protected override void OnHandle()
        {
            UpdateTargetRotation();
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, value);
        }

        private void UpdateTargetRotation()
        {
            var directionToTarget = (Target.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(transform.forward, directionToTarget);
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