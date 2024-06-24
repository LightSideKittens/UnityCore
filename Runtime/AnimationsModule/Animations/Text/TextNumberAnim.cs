using System;
using DG.Tweening;
using TMPro;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextNumberAnim : BaseAnim<int, TMP_Text>
    {
        protected override void InitAction(TMP_Text target)
        {
            target.text = $"{startValue}";
        }

        protected override Tween AnimAction(TMP_Text target)
        {
            return DOVirtual.Int(startValue, endValue,Duration, value => target.text = $"{value}");
        }
    }
}