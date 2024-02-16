using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.ConditionModule;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public class FindTargetComp : BaseComp
    {
        public static Unit selfUnit;
        public static Unit targetUnit;
        private Unit unit;

        [SerializeReference] private Condition checkers;
        private ConditionBuilder conditions;
        public LayerMask mask;
        private Transform lastTarget;
        private int frame;
        private bool IsFound => lastTarget != null;
        
        protected override void OnRegister() => Reg(this);

        protected override void Init()
        {
            unit = transform.Get<Unit>();
            conditions = ConditionBuilder.If(checkers);
        }

        public IEnumerable<Transform> FindAll(float radius) => FindAll(transform.position, radius);

        public IEnumerable<Transform> FindAll(Vector2 position, float radius)
        {
            selfUnit = unit;
            
            foreach (var targetTransform in Physics2DExt.FindAll(position, radius, mask).Select(x => x.transform))
            {
                targetUnit = targetTransform.Get<Unit>();
                
                if (conditions)
                {
                    yield return targetTransform;
                }
            }
        }

        public IEnumerable<Collider2D> FindAllColliders(float radius) => FindAllColliders(transform.position, radius);
        
        public IEnumerable<Collider2D> FindAllColliders(Vector2 position, float radius)
        {
            selfUnit = unit;
            
            foreach (var collider in Physics2DExt.FindAll(position, radius, mask))
            {
                targetUnit = collider.transform.Get<Unit>();
                
                if (conditions)
                {
                    yield return collider;
                }
            }
        }

        public bool Find(in Vector2 position, float radius, HashSet<Transform> excepted, out Transform target)
        {
            if (frame == Time.frameCount)
            {
                target = lastTarget;
                return IsFound;
            }
            
            frame = Time.frameCount;
            target = null;

            conditions.And(() => excepted.Contains(targetUnit.transform));
            if(Physics2DExt.TryFindNearestCollider(position, FindAllColliders(position, radius), out var col, mask))
            {
                target = col.transform;
            }

            conditions.Clear().Add(checkers);

            lastTarget = target;
            return IsFound;
        }

        public bool Find(in Vector2 position, float radius, out Transform target)
        {
            if (frame == Time.frameCount)
            {
                target = lastTarget;
                return IsFound;
            }
            
            frame = Time.frameCount;
            target = null;
            
            if(Physics2DExt.TryFindNearestCollider(position, FindAllColliders(position, radius), out var col, mask))
            {
                target = col.transform;
            }

            lastTarget = target;
            return IsFound;
        }

        public virtual bool Find(out Transform target) => Find(transform.position, 1000, out target);
        public bool Find(float radius, out Transform target) => Find(transform.position, radius, out target);
        public bool Find(HashSet<Transform> excepted, out Transform target) => Find(transform.position, 1000, excepted, out target);
    }
}