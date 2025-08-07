﻿using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.ConditionModule;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public class FindTargetComp
    {
        public static Unit selfUnit;
        public static Unit targetUnit;
        private Unit unit;

        [SerializeReference] private TargetChecker checker;
        [NonSerialized] public Transform transform;
        private IfBuilder ifs;
        public ContactFilter2D contactFilter;

        public void Init(Transform transform)
        {
            this.transform = transform;
            unit = transform.Get<Unit>();
            ifs = IfBuilder.If(checker);
        }
        
        public IEnumerable<Transform> FindAll(float radius) => FindAll(transform.position, radius);

        public IEnumerable<Transform> FindAll(Vector2 position, float radius)
        {
            selfUnit = unit;
            
            foreach (var targetTransform in Physics2DExt.FindAll(position, radius, contactFilter).Select(x => x.transform))
            {
                targetUnit = targetTransform.Get<Unit>();
                
                if (ifs)
                {
                    yield return targetTransform;
                }
            }
        }

        public IEnumerable<Collider2D> FindAllColliders(float radius) => FindAllColliders(transform.position, radius);
        
        public IEnumerable<Collider2D> FindAllColliders(Vector2 position, float radius)
        {
            selfUnit = unit;
            
            foreach (var collider in Physics2DExt.FindAll(position, radius, contactFilter))
            {
                targetUnit = collider.transform.Get<Unit>();
                
                if (ifs)
                {
                    yield return collider;
                }
            }
        }

        public bool Check(Collider2D collider)
        {
            selfUnit = unit;
            
            if (collider.transform.TryGet(out Unit target))
            {
                targetUnit = target;
                return ifs;
            }
           
            return false;
        }

        public bool Find(in Vector2 position, float radius, HashSet<Transform> excepted, out Transform target)
        {
            target = null;

            ifs.And(() => excepted.Contains(targetUnit.transform));
            if(Physics2DExt.TryFindNearestCollider(position, FindAllColliders(position, radius), out var col))
            {
                target = col.transform;
            }

            ifs.Clear().Add(checker);
            return target != null;
        }

        public bool Find(in Vector2 position, float radius, out Transform target)
        {
            target = null;
            
            if(Physics2DExt.TryFindNearestCollider(position, FindAllColliders(position, radius), out var col))
            {
                target = col.transform;
            }
            
            return target != null;
        }

        public virtual bool Find(out Transform target) => Find(transform.position, 1000, out target);
        public bool Find(float radius, out Transform target) => Find(transform.position, radius, out target);
        public bool Find(HashSet<Transform> excepted, out Transform target) => Find(transform.position, 1000, excepted, out target);
    }
}