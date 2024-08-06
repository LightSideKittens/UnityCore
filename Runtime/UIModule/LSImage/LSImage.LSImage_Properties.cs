using UnityEngine;

namespace LSCore
{
    public partial class LSImage
    {
        [SerializeField] private int rotateId = 0;
        [SerializeField] private bool invert;
        [SerializeField] private bool combineFilledWithSliced;
        [SerializeField] private BlendMode blendMode;
        private BlendMode lastBlendMode;

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            lastBlendMode = blendMode;
        }

        public int RotateId
        {
            get => rotateId;
            set
            {
                rotateId = value;
                SetVerticesDirty();
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
        
        public BlendMode BlendMode
        {
            set
            {
                if (blendMode == value) return;

                blendMode = value;
                SetBlendMode(value);
            }
            get => blendMode;
        }

        protected static Material s_DefaultUI;
        public static Material defaultGraphicMaterial
        {
            get
            {
                if (s_DefaultUI == null)
                {
                    s_DefaultUI = Resources.Load<Material>("LSCoreUIDefault");
                    s_DefaultUI.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_DefaultUI;
            }
        }

        public override Material defaultMaterial => defaultGraphicMaterial;

        private static readonly Vector2[] vertScratch = new Vector2[4];
        private static readonly Vector2[] uVScratch = new Vector2[4];
        
        private static readonly Vector3[] s_Xy = new Vector3[4];
        private static readonly Vector3[] s_Uv = new Vector3[4];
        
        public void SetBlendMode(BlendMode mode)
        {
            string keyword = GetBlendModeKeyword(mode);
            SetKeyword(GetBlendModeKeyword(lastBlendMode), false);
            SetKeyword(keyword, true);
            SetMaterialDirty();
            lastBlendMode = mode;
        }
        
        private void SetKeyword(string keyword, bool status)
        {
            if (status)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
        
        private string GetBlendModeKeyword(BlendMode mode)
        {
            return "BM_" + mode.ToString().ToUpper();
        }
    }
}