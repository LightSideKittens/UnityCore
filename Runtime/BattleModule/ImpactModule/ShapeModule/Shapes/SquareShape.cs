using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class SquareShape : BaseShape
    {
        public Vector2 size = new Vector2(1, 1);
        public float angleOffset;
        public float Rotation => angleOffset + pivot.eulerAngles.z;
        
        public override Collider2D[] Overlap(in ContactFilter2D filter)
        {
            return Physics2DExt.FindAll(Position, size, Rotation, filter);
        }

        public override void OnDrawGizmos()
        {
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(Position, Quaternion.Euler(0, 0, Rotation), pivot.lossyScale);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = originalMatrix;
        }
    }
}