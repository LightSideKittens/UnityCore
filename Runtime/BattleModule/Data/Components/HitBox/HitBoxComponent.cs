using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    internal abstract class HitBoxComponent : BaseComp
    {
        public abstract bool IsIntersected(in Vector2 position, in float radius, out Vector2 point);
    }
}