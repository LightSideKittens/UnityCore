using System;
using DG.Tweening;
using TMPro;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextAnim : BaseAnim<string>
    {
        public TMP_Text target;

        protected override void Internal_Init()
        {
            target.text = startValue;
        }

        protected override Tween Internal_Animate()
        {
            return DOTween.Sequence().AppendInterval(duration).AppendCallback(() => target.text = endValue);
        }
    }
}