using System;
using DG.Tweening;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextNumberAnim : BaseAnim<int, LSNumber>
    {
        protected override void InitAction(LSNumber target)
        {
            target.Number = startValue;
        }

        protected override Tween AnimAction(LSNumber target)
        {
            return DOVirtual.Int(startValue, endValue,duration, value => target.Number = value);
        }
    }
}