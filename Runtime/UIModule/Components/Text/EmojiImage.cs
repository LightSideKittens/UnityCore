using UnityEngine.UI;

namespace LSCore
{
    public class EmojiImage : RawImage
    {
        public override void OnCullingChanged() { }

        public override void SetMaterialDirty()
        {
            base.SetMaterialDirty();
            if (!IsActive())
                return;
            
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            
            if (!IsActive())
                return;

            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
        }
    }
}