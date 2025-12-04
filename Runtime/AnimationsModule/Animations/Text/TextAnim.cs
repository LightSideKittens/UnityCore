using System;
using DG.Tweening;
using TMPro;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextAnim : BaseAnim<string, TMPText>
    {
        protected override void InitAction(TMPText target)
        {
            target.text = startValue;
        }

        protected override Tween AnimAction(TMPText target)
        {
            return DOTween.Sequence().AppendInterval(duration).AppendCallback(() => target.text = endValue);
        }
    }
}