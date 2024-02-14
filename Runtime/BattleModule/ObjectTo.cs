using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public static class ObjectTo<T>
    {
        private static Dictionary<Object, T> values = new();

        static ObjectTo()
        {
            World.Destroyed += Clear;
        }

        public static void Set(Object target, T obj) => values[target] = obj;
        public static void Add(Object target, T obj) => values.Add(target, obj);
        public static T Get(Object target) => values[target];
        public static bool TryGet(Object target, out T obj) => values.TryGetValue(target, out obj);
        public static void Remove(Object target) => values.Remove(target);
        public static void Clear() => values.Clear();
        public static IEnumerable<Object> Keys => values.Keys;
        public static IEnumerable<T> All => values.Values;
    }
}