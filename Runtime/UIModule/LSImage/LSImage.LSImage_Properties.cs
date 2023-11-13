﻿using UnityEngine;

namespace LSCore
{
    public partial class LSImage
    {
        [SerializeField] private int rotateId = 0;
        [SerializeField] private bool invert;
        [SerializeField] private Gradient gradient;
        [SerializeField] private float angle = 45;
        [SerializeField] private float gradientStart;
        [SerializeField] private float gradientEnd;
        private Vector2 gradientStartPoint;
        private Vector2 gradientEndPoint;
        private Mesh cachedMesh;
        
        public int RotateId
        {
            get => rotateId;
            set
            {
                rotateId = value;
                SetColorDirty();
            }
        }

        public bool Invert
        {
            get => invert;
            set
            {
                invert = value;
                UpdateColorEvaluateFunc();
                SetColorDirty();
            }
        }
        public Gradient Gradient
        {
            get => gradient;
            set
            {
                gradient = value;
                SetColorDirty();
            }
        }
        
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                CalculatePerpendicularPoints();
                var direction = GradientDirection;
                gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
                gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
                gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
                gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;

                if (gradient.colorKeys.Length > 2 || gradient.alphaKeys.Length > 2)
                {
                    SetVerticesDirty();
                }
                else
                {
                    SetColorDirty();
                }
            }
        }
        
        public float GradientStart
        {
            get => gradientStart;
            set
            {
                gradientStart = value;
                var direction = GradientDirection;
                gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
                gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
                SetColorDirty();
            }
        }
        
        public float GradientEnd
        {
            get => gradientEnd;
            set
            {
                gradientEnd = value;
                var direction = GradientDirection;
                gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
                gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;
                SetColorDirty();
            }
        }
        
        internal Vector2 GradientStartPoint
        {
            get => gradientStartPoint;
            set
            {
                gradientStartPoint = value;
                SetColorDirty();
            }
        }
        
        internal Vector2 GradientEndPoint
        {
            get => gradientEndPoint;
            set
            {
                gradientEndPoint = value;
                SetColorDirty();
            }
        }

        internal Vector2 GradientDirection { get; private set; }

        private static readonly Vector2[] vertScratch = new Vector2[4];
        private static readonly Vector2[] uVScratch = new Vector2[4];
        
        private static readonly Vector3[] s_Xy = new Vector3[4];
        private static readonly Vector3[] s_Uv = new Vector3[4];
        private InFunc<Vector3, Color> colorEvaluate;
    }
}