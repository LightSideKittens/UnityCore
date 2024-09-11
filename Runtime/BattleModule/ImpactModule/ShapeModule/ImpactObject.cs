using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class ImpactObject : BaseImpactObject
    {
        public List<ShapeTrigger> triggers;
        [SerializeReference] public List<ShapeImpact> impacts;
        
        private void Awake()
        {
            for (int i = 0; i < triggers.Count; i++)
            {
                triggers[i].Init();
                triggers[i].Triggered += OnTriggered;
            }
        }

        private void OnTriggered(Collider2D collider)
        {
            if(!canImpactChecker(collider)) return;

            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i].Apply(initiator, collider);
            }
        }

#if UNITY_EDITOR

        [SerializeField] private bool alwaysGizmos;

        private void OnDrawGizmosSelected()
        {
            if(alwaysGizmos) return;
            DrawGizmos();
        }

        private void OnDrawGizmos()
        {
            if(!alwaysGizmos) return;
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            if(triggers == null) return;
            
            foreach (var trigger in triggers)
            {
                trigger.DrawGizmos();
            }
        }
#endif

        private void FixedUpdate()
        {
            IgnoreCollider = true;
            
            for (int i = 0; i < triggers.Count; i++)
            {
                triggers[i].Update();
            }
            
            IgnoreCollider = false;
        }
    }
}