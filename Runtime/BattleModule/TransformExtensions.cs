using LSCore.LevelSystem;
using UnityEngine;

namespace LSCore.BattleModule
{
    public static class TransformExtensions
    {
        public static T Get<T>(this Transform target)
        {
            return TransformDict<T>.Get(target);
        }

        public static bool TryGet<T>(this Transform target, out T result)
        {
            return TransformDict<T>.TryGet(target, out result);
        }
    }
}