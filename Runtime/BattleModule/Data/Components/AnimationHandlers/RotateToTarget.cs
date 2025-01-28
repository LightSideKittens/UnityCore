using System;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

public enum Vector3Axis
{
    X,
    Y,
    Z
}

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class RotateToTarget : BadassAnimation.FloatHandler
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
        protected override string Label => "Normalized value";
#endif

        public override Object Target => transform;

        private Transform Targett
        {
            get
            {
#if UNITY_EDITOR
                if (target != null)
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
            if (
#if UNITY_EDITOR
            target != null  &&
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
            var directionToTarget = (Targett.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(transform.forward, directionToTarget);
        }
        

        protected override void OnStop()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                transform.rotation = initialRotation;
            }
#endif
        }
    }
}