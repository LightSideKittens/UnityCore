using DG.Tweening;

namespace LSCore.AnimationsModule.Animations.Options
{
    public interface IOption
    {
        void ApplyTo(Tween tween);
    }
}