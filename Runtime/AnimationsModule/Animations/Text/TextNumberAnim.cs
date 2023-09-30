using System;
using DG.Tweening;
using TMPro;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextNumberAnim : BaseAnim<int>
    {
        public TMP_Text target;

        protected override void Internal_Init()
        {
            target.text = $"{startValue}";
        }

        protected override Tween Internal_Animate()
        {
            return DOVirtual.Int(startValue, endValue, duration, value => target.text = $"{value}");
        }
    }
}