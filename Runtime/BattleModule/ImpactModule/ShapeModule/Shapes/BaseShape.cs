using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseShape
    {
        [Required]
        public Transform pivot;
        public Vector3 offset;
        
        public Vector3 Position => pivot.position + offset;

        public abstract Collider2D[] Overlap(in ContactFilter2D filter);

        [Conditional("UNITY_EDITOR")]
        public void DrawGizmos()
        {
            if(pivot == null) return;
            var color = Gizmos.color;
            Gizmos.color = GizmosColor;
            OnDrawGizmos();
            Gizmos.color = color;
        }

#if UNITY_EDITOR
        protected virtual Color GizmosColor { get; } = new (0.24f, 1f, 0.23f);
#endif
        [Conditional("UNITY_EDITOR")]
        public abstract void OnDrawGizmos();
    }
}