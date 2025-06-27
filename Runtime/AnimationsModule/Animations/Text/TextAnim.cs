using System;
using DG.Tweening;
using TMPro;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextAnim : BaseAnim<string, TMP_Text>
    {
        protected override void InitAction(TMP_Text target)
        {
            target.text = startValue;
        }

        protected override Tween AnimAction(TMP_Text target)
        {
            return DOTween.Sequence().AppendInterval(duration).AppendCallback(() => target.text = endValue);
        }
    }
}