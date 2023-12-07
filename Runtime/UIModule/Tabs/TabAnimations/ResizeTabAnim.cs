using DG.Tweening;

namespace LightSideCore.Runtime.UIModule.TabAnimations
{
    public class ResizeTabAnim : BaseTabAnim
    {
        public override void ShowAnim()
        {
            group.DOFade(1, duration);
            parent.DOSizeDelta(reference.rect.size, duration);
        }

        public override void HideAnim()
        {
            group.DOFade(0, duration);
        }
    }
}