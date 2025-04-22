/*using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEngine;
using static BadassAnimation;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class RotateAroundPoint : Handler<RotateAroundPoint.Data>
    {
        [Serializable]
        public struct Data
        {
            public Vector3 position;
            public Vector3 eulerAngles;
            public Quaternion quaternion;
            
            public static Data Get(Transform transform)
            {
                Data data = new Data();
                data.position = transform.localPosition;
                data.eulerAngles = transform.localEulerAngles;
                data.quaternion = transform.localRotation;
                return data;
            }

            public void Apply(Transform transform)
            {
                transform.localPosition = position;
                transform.localEulerAngles = eulerAngles;
            }

            public static Data operator +(Data lhs, Data rhs)
            {
                lhs.position += rhs.position;
                lhs.eulerAngles += rhs.eulerAngles;
                return lhs;
            }
        }
        
        [SerializeField] private Transform target;
        [SerializeField] private Transform pivot;
        [SerializeField] private bool add;
        [SerializeField] private bool rotate;
        public Vector3 offset;
        private Data startData;

        public override Object Target => target;

        protected override void OnStart()
        {
            startData = GetStartValue();
        }

        public override void OnLooped()
        {
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            startData = GetStartValue();
        }

        protected override void OnHandle()
        {
            var data = value;
            if(add) data += startData;
            
            var totalOffset = pivot.position + offset;
            var pos = totalOffset.RotateAroundPivot(pivot.position, data.eulerAngles);
            target.position = pos;
            if (rotate)
            {
                target.localEulerAngles = data.eulerAngles;
            }
        }

        protected override void OnStop()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                StartValue.Apply(target);
                return;
            }
#endif
            if (!add)
            {
                StartValue.Apply(target);
            }
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
        
        protected HandlerEvaluateData x;
        protected HandlerEvaluateData y;
        protected HandlerEvaluateData z;
        
        protected HandlerEvaluateData offset_X;
        protected HandlerEvaluateData offset_Y;
        protected HandlerEvaluateData offset_Z;
        
#if UNITY_EDITOR
        protected override void OnTrimModifications(List<UndoPropertyModification> modifications)
        {
            if (x != null || y != null || z != null)
            {
                TrimModificationsQuaternion(modifications, "m_LocalRotation");
                TrimModifications(modifications, x, "m_LocalEulerAnglesHint", "x");
                TrimModifications(modifications, y, "m_LocalEulerAnglesHint", "y");
                TrimModifications(modifications, z, "m_LocalEulerAnglesHint", "z");
                
                var empty = new HandlerEvaluateData();
                TrimModifications(modifications, empty, "m_LocalPosition", "x");
                TrimModifications(modifications, empty, "m_LocalPosition", "y");
                TrimModifications(modifications, empty, "m_LocalPosition", "z");
            }
            
            var offsetProp = FindProperty("offset");
            if (offsetProp != null)
            {
                TrimModifications(modifications, offset_X, offsetProp.propertyPath, "x");
                TrimModifications(modifications, offset_Y, offsetProp.propertyPath, "y");
                TrimModifications(modifications, offset_Z, offsetProp.propertyPath, "z");
            }
        }
        
        public override void StartAnimationMode()
        {
            if (x != null || y != null || z != null)
            {
                var pos = StartValue.position;
                var quaternion = StartValue.quaternion;
                var angles = StartValue.eulerAngles;

                StartAnimationModeQuaternion("m_LocalRotation", quaternion);
                StartAnimationMode(x, "m_LocalEulerAnglesHint", "x", angles.x);
                StartAnimationMode(y, "m_LocalEulerAnglesHint", "y", angles.y);
                StartAnimationMode(z, "m_LocalEulerAnglesHint", "z", angles.z);

                var empty = new HandlerEvaluateData();
                StartAnimationMode(empty, "m_LocalPosition", "x", pos.x);
                StartAnimationMode(empty, "m_LocalPosition", "y", pos.y);
                StartAnimationMode(empty, "m_LocalPosition", "z", pos.z);
            }

            var offsetProp = FindProperty("offset");
            if (offsetProp != null)
            {
                StartAnimationMode(animation, offset_X, offsetProp.propertyPath, "x", offset.x);
                StartAnimationMode(animation, offset_Y, offsetProp.propertyPath, "y", offset.y);
                StartAnimationMode(animation, offset_Z, offsetProp.propertyPath, "z", offset.z);
            }
        }
#endif
        
        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            switch (key)
            {
                case PropNames.x: x = evaluator; return X;
                case PropNames.y: y = evaluator; return Y;
                case PropNames.z: z = evaluator; return Z;
                case "offset_X" : offset_X = evaluator; return OffsetX;
                case "offset_Y" : offset_Y = evaluator; return OffsetY;
                case "offset_Z" : offset_Z = evaluator; return OffsetZ;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.eulerAngles.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.eulerAngles.y = y.y;}
        private void Z() {isDiff |= z.isDiff; value.eulerAngles.z = z.y;}
        
        private void OffsetX() {isDiff |= offset_X.isDiff; offset.x = offset_X.y;}
        private void OffsetY() {isDiff |= offset_Y.isDiff; offset.y = offset_Y.y;}
        private void OffsetZ() {isDiff |= offset_Z.isDiff; offset.z = offset_Z.y;}
    }
}*/