using Battle.Data;
using LSCore.LevelSystem;
using UnityEngine;

namespace Battle
{
    internal static class TransformExtensions
    {
        public static T Get<T>(this Transform target)
        {
            return ObjectsByTransfroms<T>.Get(target);
        }
        
        public static float GetValue<T>(this Transform transform) where T : FloatGameProp
        {
            var unit = ObjectsByTransfroms<BaseUnit>.Get(transform);
            return FloatGameProp.GetValue<T>(unit.Props);
        }

        public static bool TryGet<T>(this Transform target, out T result)
        {
            return ObjectsByTransfroms<T>.TryGet(target, out result);
        }
    }
}