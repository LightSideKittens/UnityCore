using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class RandomExt
    {
        public static int MinusPlus => Random.Range(0, 2) * 2 - 1;
    }
}