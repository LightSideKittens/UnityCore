using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public class CameraData
        {
            public Color backColor = new(0.2f, 0.2f, 0.2f);
            public Vector3 position = Vector3.forward * -10;
            public float size = 10;
            public Vector2 sizeRange = new(0.000001f, 9.08581038E+13f);
        }
    }
}