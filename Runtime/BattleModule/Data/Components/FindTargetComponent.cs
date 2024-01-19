using System;
using System.Collections.Generic;
using Battle.Data.Components.TargetProviders;
using Sirenix.Serialization;
using UnityEngine;

namespace Battle.Data.Components
{
    [Serializable]
    public class FindTargetComponent
    {
        [SerializeReference] private List<TargetProvider> providers;
        private Transform lastTarget;
        private Transform transform;
        private int frame;
        private bool IsFound => lastTarget != null;

        public void Init(Transform transform)
        {
            this.transform = transform;
            
            for (int i = 0; i < providers.Count; i++)
            {
                var provider = providers[i];
                provider.findTargetComponent = this;
            }
        }

        public IEnumerable<Transform> FindAll(float radius) => FindAll(transform.position, radius);

        public IEnumerable<Transform> FindAll(Vector2 position, float radius)
        {
            if (frame == Time.frameCount)
            {
                if (IsFound)
                {
                    yield return lastTarget;
                }
                else
                {
                    yield break;
                }
            }
            
            frame = Time.frameCount;
            
            for (int i = 0; i < providers.Count; i++)
            {
                var targets = providers[i].Targets;
                foreach (var target in targets)
                {
                    var hitBox = target.Get<HitBoxComponent>();

                    if (hitBox.IsIntersected(position, radius, out _))
                    {
                        yield return target;
                    }
                }
            }
        }

        public bool Find(Vector2 position, float radius, HashSet<Transform> excepted, out Transform target)
        {
            if (frame == Time.frameCount)
            {
                target = lastTarget;
                return IsFound;
            }
            
            frame = Time.frameCount;
            
            var distance = radius;
            target = null;
            
            for (int i = 0; i < providers.Count; i++)
            {
                var targets = providers[i].Targets;
                foreach (var target1 in targets)
                {
                    if (!excepted.Contains(target1))
                    {
                        var hitBox = target1.Get<HitBoxComponent>();

                        if (hitBox.IsIntersected(position, distance, out var point))
                        {
                            var newDistance = Vector2.Distance(point, position);

                            if (distance > newDistance)
                            {
                                target = target1;
                                distance = newDistance;
                            }
                        }
                    }
                }
            }

            lastTarget = target;
            return IsFound;
        }

        public bool Find(Vector2 position, float radius, out Transform target)
        {
            if (frame == Time.frameCount)
            {
                target = lastTarget;
                return IsFound;
            }
            
            frame = Time.frameCount;
            
            var distance = radius;
            target = null;
            
            for (int i = 0; i < providers.Count; i++)
            {
                var targets = providers[i].Targets;
                foreach (var target1 in targets)
                {
                    var hitBox = target1.Get<HitBoxComponent>();

                    if (hitBox.IsIntersected(position, distance, out var point))
                    {
                        var newDistance = Vector2.Distance(point, position);

                        if (distance > newDistance)
                        {
                            target = target1;
                            distance = newDistance;
                        }
                    }
                }
            }

            lastTarget = target;
            return IsFound;
        }

        public virtual bool Find(out Transform target) => Find(transform.position, 1000, out target);
        public bool Find(float radius, out Transform target) => Find(transform.position, radius, out target);
        public bool Find(HashSet<Transform> excepted, out Transform target) => Find(transform.position, 1000, excepted, out target);
    }
}