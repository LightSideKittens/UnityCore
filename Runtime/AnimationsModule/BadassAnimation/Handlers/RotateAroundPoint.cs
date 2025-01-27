using System;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class RotateAroundPoint : BadassAnimation.Vector3Handler
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private Transform target;
        [SerializeField] private bool rotate;
        [SerializeField] private bool setOffset;
        
        [ShowIf("setOffset")]
        [SerializeField] private Vector3 offset;
        
        private Vector3 usedOffset;
        private Vector3 startRotation;
        private Vector3 startPosition;
        protected override string Label => "Rotation";

        public override Object Target => target;

        protected override void OnStart()
        {
            base.OnStart();
            startRotation = target.localEulerAngles;
            startPosition = target.localPosition;
            if (!setOffset)
            {
                usedOffset = target.position - pivot.position;
            }
        }

        protected override void OnHandle()
        {
            usedOffset = pivot.rotation * offset;
            var totalOffset = pivot.position + usedOffset;
            var pos = totalOffset.RotateAroundPivot(pivot.position, value);
            target.position = pos;
            if (rotate)
            {
                target.localEulerAngles = startRotation + value;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            target.localEulerAngles = startRotation;
            target.localPosition = startPosition;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if(pivot == null) return;
            Gizmos.DrawWireSphere(pivot.position , 1);
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if(pivot == null) return;
            pivot.position = Handles.PositionHandle(pivot.position, Quaternion.identity);
        }
#endif
    }
}