using LSCore.LevelSystem;
using UnityEngine;

namespace LSCore.BattleModule
{
    internal static class TransformExtensions
    {
        public static T Get<T>(this Transform target)
        {
            return ObjectsByTransforms<T>.Get(target);
        }
        
        public static float GetValue<T>(this Transform transform) where T : FloatGameProp
        {
            var unit = ObjectsByTransforms<BaseUnit>.Get(transform);
            return FloatGameProp.GetValue<T>(unit.Props);
        }

        public static bool TryGet<T>(this Transform target, out T result)
        {
            return ObjectsByTransforms<T>.TryGet(target, out result);
        }
    }
}