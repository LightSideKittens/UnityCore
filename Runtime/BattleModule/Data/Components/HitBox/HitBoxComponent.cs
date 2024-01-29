using System;
using UnityEngine;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.HitBoxComponent>;

namespace LSCore.BattleModule
{
    [Serializable]
    internal abstract class HitBoxComponent : BaseComp
    {
        protected Transform transform;

        public override void Init(CompData data)
        {
            Add(transform, this);
            transform = data.transform;
            OnInit();
        }

        protected virtual void OnInit(){}

        public void Destroy()
        {
            Remove(transform);
        }
        public abstract bool IsIntersected(in Vector2 position, in float radius, out Vector2 point);
    }
}