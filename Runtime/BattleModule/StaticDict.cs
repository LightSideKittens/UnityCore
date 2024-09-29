using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class StaticDict<TKey, TValue>
    {
        private static readonly Dictionary<TKey, TValue> values = new();

        static StaticDict() => World.Destroyed += Clear;

        public static void Set(TKey target, TValue obj) => values[target] = obj;
        public static void Add(TKey target, TValue obj) => values.Add(target, obj);
        public static TValue Get(TKey target) => values[target];
        public static bool TryGet(TKey target, out TValue obj) => values.TryGetValue(target, out obj);
        public static void Remove(TKey target) => values.Remove(target);
        public static void Clear() => values.Clear();
        public static IEnumerable<TKey> Keys => values.Keys;
        public static IEnumerable<TValue> All => values.Values;
    }
    
    public class TransformDict<TValue> : StaticDict<Transform, TValue>{}
    public class StringDict<TValue> : StaticDict<string, TValue>{}
}