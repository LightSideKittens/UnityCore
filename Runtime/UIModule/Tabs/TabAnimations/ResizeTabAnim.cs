using DG.Tweening;

namespace LightSideCore.Runtime.UIModule.TabAnimations
{
    public class ResizeTabAnim : BaseTabAnim
    {
        public override void ShowAnim()
        {
            parent.pivot = reference.pivot;
            parent.anchorMin = reference.anchorMin;
            parent.anchorMax = reference.anchorMax;
            parent.anchoredPosition = reference.anchoredPosition;
            
            group.DOFade(1, duration);
            parent.DOSizeDelta(reference.sizeDelta, duration);
        }

        public override void HideAnim()
        {
            group.DOFade(0, duration);
        }
    }
}