using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public sealed class CopiedCanvasRenderer : MonoBehaviour
    {
        [SerializeReference] public Get<CanvasRenderer> source = new SerializeField<CanvasRenderer>();
        private CanvasRenderer self;

        private void Awake() => self = GetComponent<CanvasRenderer>();

        private void OnEnable()
        {
            Canvas.willRenderCanvases += Copy;
            Copy();
        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= Copy;
            self?.Clear();
        }

        private void Copy()
        {
            CanvasRenderer source = this.source;
            if (source == null || self == null) return;

            self.cull = source.cull;
            self.hasPopInstruction = source.hasPopInstruction;
            self.SetAlpha(source.GetAlpha());

            int mCount = source.materialCount;
            self.materialCount = mCount;

            for (int i = 0; i < mCount; i++)
            {
                self.SetMaterial(source.GetMaterial(i), i);
            }
            
            mCount = source.popMaterialCount;
            self.popMaterialCount = mCount;

            for (int i = 0; i < mCount; i++)
            {
                self.SetPopMaterial(source.GetPopMaterial(i), i);
            }

            Mesh mesh = source.GetMesh();
            if (mesh != null)
                self.SetMesh(mesh);
        }
    }
}