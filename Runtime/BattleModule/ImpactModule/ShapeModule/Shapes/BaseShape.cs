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
            var newColor = default(Color);
            GetGizmosColor(ref newColor);
            Gizmos.color = newColor;
            OnDrawGizmos();
            Gizmos.color = color;
        }

        [Conditional("UNITY_EDITOR")]
        protected virtual void GetGizmosColor(ref Color color) => color = new (0.24f, 1f, 0.23f);
        
        [Conditional("UNITY_EDITOR")]
        public abstract void OnDrawGizmos();
    }
}