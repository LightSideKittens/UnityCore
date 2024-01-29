using System.Collections.Generic;
using LSCore;
using UnityEngine;

namespace LSCore.BattleModule
{
    public static class ObjectsByTransforms<T>
    {
        private static Dictionary<Transform, T> objects = new();

        static ObjectsByTransforms()
        {
            World.Created += Clear;
        }

        public static void Set(Transform target, T obj) => objects[target] = obj;
        public static void Add(Transform target, T obj) => objects.Add(target, obj);
        public static T Get(Transform target) => objects[target];
        public static bool TryGet(Transform target, out T obj) => objects.TryGetValue(target, out obj);
        public static void Remove(Transform target) => objects.Remove(target);
        public static void Clear() => objects.Clear();
        public static IEnumerable<Transform> Keys => objects.Keys;
        public static IEnumerable<T> All => objects.Values;
    }
}