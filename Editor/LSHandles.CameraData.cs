using System;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        [Serializable]
        public class CameraData
        {
            public Color backColor = new(0.2f, 0.2f, 0.2f);
            public Vector3 position = Vector3.forward * -100;

            [SerializeField] private float size = 5;
            
            public float Size
            {
                get => size;
                set
                {
                    size = value;
                    size = Mathf.Clamp(size, sizeRange.x, sizeRange.y);
                }
            }
            
            public Vector2 sizeRange = new(0.02f, 100000);
        }
    }
}