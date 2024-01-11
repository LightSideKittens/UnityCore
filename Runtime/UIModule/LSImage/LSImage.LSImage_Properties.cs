using UnityEngine;

namespace LSCore
{
    public partial class LSImage
    {
        [SerializeField] private int rotateId = 0;
        [SerializeField] private bool invert;
        [SerializeField] private LSGradient gradient;
        [SerializeField] private float angle = 45;
        [SerializeField] private float gradientStart;
        [SerializeField] private float gradientEnd;
        [SerializeField] private bool combineFilledWithSliced;
        internal Vector2 gradientStartPoint;
        internal Vector2 gradientEndPoint;
        private Mesh resultMesh;
        private Vector3[] resultMeshVerts;
        private Color[] resultMeshColors;
        private Mesh withoutGradientMesh;

        public override Color color
        {
            get => gradient[0].color;
            set
            {
                gradient.SetColor(0, value);
                SetColorDirty();
            }
        }

        public int RotateId
        {
            get => rotateId;
            set
            {
                rotateId = value;
                SetGradientDirty();
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
        public LSGradient Gradient
        {
            get => gradient;
            set
            {
                gradient = value;
                SetGradientDirty();
            }
        }
        
        public float Angle
        {
            get => angle;
            set
            {
                angle = (value + 360) % 360;
                if (gradient.Count > 1)
                {
                    CalculatePerpendicularPoints();
                    var direction = GradientDirection;
                    gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
                    gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
                    gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
                    gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;
                    
                    SetVerticesDirty();
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
                SetGradientDirty();
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
                SetGradientDirty();
            }
        }
        
        public bool CombineFilledWithSliced
        {
            get => combineFilledWithSliced;
            set
            {
                combineFilledWithSliced = value;
                SetVerticesDirty();
            }
        }
        
        internal Vector2 GradientDirection { get; private set; }

        private static readonly Vector2[] vertScratch = new Vector2[4];
        private static readonly Vector2[] uVScratch = new Vector2[4];
        
        private static readonly Vector3[] s_Xy = new Vector3[4];
        private static readonly Vector3[] s_Uv = new Vector3[4];
        private InFunc<Vector3, Color> colorEvaluate;

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            isRectDirty = true;
        }
    }
}