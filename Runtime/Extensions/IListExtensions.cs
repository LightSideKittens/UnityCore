using System.Collections.Generic;

namespace LSCore.Extensions
{
    public static class IListExtensions
    {
        public static T Random<T>(this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];
    }
}