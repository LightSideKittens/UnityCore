using DG.Tweening;
using LSCore.Extensions;

namespace LSCore.AnimationsModule.Animations
{
    public static class Extensions
    {
        public static Tween KillOnDestroy(this Tween tween)
        {
            DestroyEvent.AddOnDestroy(tween.target, tween.KillVoid);
            return tween;
        }
    }
}