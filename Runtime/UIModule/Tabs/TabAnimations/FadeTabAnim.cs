using DG.Tweening;

namespace LightSideCore.Runtime.UIModule.TabAnimations
{
    public class FadeTabAnim : BaseTabAnim
    {
        public override void ShowAnim()
        {
            group.DOFade(1, duration);
        }

        public override void HideAnim()
        {
            group.DOFade(0, duration);
        }
    }
}