using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    public delegate void TriggeredAction(ref Particle particle, Collider2D collider);
    public delegate void TriggerAction(HashSet<Collider2D> set1, HashSet<Collider2D> set2, ref Particle particle);
    
    [Serializable]
    public class TriggerHandler : ParticlesHandler
    {
        private static Collider2D[] maxColliders = new Collider2D[100];
        private static List<int> keysToRemove = new();
        
        public ParticleSystemTriggerEventType type = ParticleSystemTriggerEventType.Enter;
        public ContactFilter2D contactFilter;
        
        [UniqueTypeFilter]
        [SerializeReference] public List<OnTriggerHandler> handlers;
        
        public event TriggeredAction Triggered;
        public Collider2D ignoredCollider;

        private TriggerAction triggerAction;
        private Dictionary<int, HashSet<Collider2D>> colliders = new();
        private HashSet<int> ids = new();
        private List<Vector4> customData = new();
        private ParticleSystem ps;

        public void Init(ParticleSystem ps, List<Vector4> customData)
        {
            this.ps = ps;
            this.customData = customData;
            InitTriggerAction();
        }
        
        public override void Handle(Particle[] particles)
        {
            var radiusScale = ps.trigger.radiusScale;
            Vector2 point = default;
            ids.Clear();
            if(ignoredCollider != null) ignoredCollider.enabled = false;
            for (int i = 0; i < particles.Length; i++)
            {
                ref var particle = ref particles[i];
                var id = (int)customData[i].x;
                ids.Add(id);
                var radius = particle.GetCurrentSize(ps) * radiusScale / 2;
                var pos = particle.position;
                point.x = pos.x;
                point.y = pos.y;
                var currentColliders = Physics2DExt.FindAll(point, radius, contactFilter);
                
                if (!colliders.TryGetValue(id, out var previousColliders))
                {
                    previousColliders = new HashSet<Collider2D>();
                }

                var currentSet = new HashSet<Collider2D>(currentColliders);
                
                triggerAction(currentSet, previousColliders, ref particle);
                
                colliders[id] = currentSet;
            }
            if(ignoredCollider != null) ignoredCollider.enabled = true;
            
            keysToRemove.Clear();
            
            foreach (var id in colliders.Keys)
            {
                if (!ids.Contains(id))
                {
                    keysToRemove.Add(id);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
            {
                colliders.Remove(keysToRemove[i]);
            }
        }

        private void OnEnter(HashSet<Collider2D> currentSet, HashSet<Collider2D> previousColliders, ref Particle particle)
        {
            int colliderNum = 0;
            foreach (var collider in currentSet)
            {
                if (!previousColliders.Contains(collider))
                {
                    maxColliders[colliderNum] = collider;
                    colliderNum++;
                }
            }
            
            if(colliderNum == 0) return;

            for (int i = 0; i < colliderNum; i++)
            {
                var collider = maxColliders[i];
                Triggered?.Invoke(ref particle, collider);
                for (int k = 0; k < handlers.Count; k++)
                {
                    handlers[k].Handle(ref particle, collider);
                }
            }
        }

        private void OnExit(HashSet<Collider2D> currentSet, HashSet<Collider2D> previousColliders, ref Particle particle)
        {
            int colliderNum = 0;
            foreach (var collider in previousColliders)
            {
                if (!currentSet.Contains(collider))
                {
                    maxColliders[colliderNum] = collider;
                    colliderNum++;
                }
            }
            
            if(colliderNum == 0) return;

            for (int i = 0; i < colliderNum; i++)
            {
                var collider = maxColliders[i];
                Triggered?.Invoke(ref particle, collider);
                for (int k = 0; k < handlers.Count; k++)
                {
                    handlers[k].Handle(ref particle, collider);
                }
            }
        }

        private void InitTriggerAction()
        {
            switch (type)
            {
                case ParticleSystemTriggerEventType.Inside:
                    break;
                case ParticleSystemTriggerEventType.Outside:
                    break;
                case ParticleSystemTriggerEventType.Enter:
                    triggerAction = OnEnter;
                    break;
                case ParticleSystemTriggerEventType.Exit:
                    triggerAction = OnExit;
                    break;
            }
        }
    }
}