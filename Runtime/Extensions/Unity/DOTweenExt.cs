using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.Extensions
{
    public static class DOTweenExt
    {
        public static void KillVoid(this Tween tween)
        {
#if UNITY_EDITOR
            if(World.IsPlayModeDisabling) return;
#endif
            
            var sequenceParent = PathAccessor.GetRef(tween, "sequenceParent");
            
            if (sequenceParent.Get(tween) is Sequence sequence)
            {
                sequence.KillVoid();
                return;
            }
            
            tween.Kill();
        }

        public static void Complete(object id) => DOTween.TweensById(id)?.ForEach(x => x.Goto(x.isBackwards ? 0 : 1));

        public static Tween DOFloat(this MaterialPropertyBlock target, float duration, int propId, float endValue)
        {
            if (!target.HasProperty(propId))
            {
                if (Debugger.logPriority > 0)
                    Debugger.LogMissingMaterialProperty(propId);
                return null;
            }

            var value = target.GetFloat(propId);
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => value, x =>
            {
                value = x;
                target.SetFloat(propId, x);
            }, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        public static Tween DOSize(this SpriteRenderer target, Vector2 endValue, float duration)
        {
            var value = target.size;
            TweenerCore<Vector2, Vector2, VectorOptions> t = DOTween.To(() => value, x =>
            {
                value = x;
                target.size = x;
            }, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        public static Tween DOWobble(this Transform target,
            float amplitude = 0.5f,
            float frequency = 1f,
            LSVector3.Axis axis = LSVector3.Axis.All)
        {
            Vector3 seed = new Vector3(
                Random.value * 10f,
                Random.value * 10f,
                Random.value * 10f);
            
            var startPos = target.localPosition;
            var ax = axis.ToVector();
            
            var wobble = DOTween.Sequence().AppendInterval(1).OnUpdate(() =>
            {
                float time = UnityEngine.Time.time * frequency;

                float offsetX = ax.x * (Mathf.PerlinNoise(seed.x, time) - 0.5f) * 2f;
                float offsetY = ax.y * (Mathf.PerlinNoise(seed.y, time) - 0.5f) * 2f;
                float offsetZ = ax.z * (Mathf.PerlinNoise(seed.z, time) - 0.5f) * 2f;
                
                Vector3 offset = new Vector3(offsetX, offsetY, offsetZ) * amplitude;

                target.localPosition = startPos + offset;
            }).SetEase(Ease.Linear).SetLoops(-1).OnKill(() => target.localPosition = startPos);
            wobble.target = target;
            
            return wobble;
        }
    }
}