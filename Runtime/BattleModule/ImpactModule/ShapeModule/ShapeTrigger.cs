using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class ShapeTrigger
    {
        public delegate void TriggeredAction(Collider2D collider);
        public delegate void TriggerAction(HashSet<Collider2D> set1, HashSet<Collider2D> set2);
        
        public enum EventType
        {
            Enter,
            Exit
        }

        private static Collider2D[] maxColliders = new Collider2D[100];
        
        [SerializeReference] public List<BaseShape> shapes = new();
        public EventType type = EventType.Enter;
        public ContactFilter2D contactFilter;
        
        public event TriggeredAction Triggered;

        private TriggerAction triggerAction;
        private HashSet<Collider2D> colliders = new();

        public void Init()
        {
            InitTriggerAction();
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawGizmos()
        {
            if(shapes == null) return;
            foreach (var shape in shapes)
            {
                shape.DrawGizmos();
            }
        }

        public IEnumerable<Collider2D> Overlapped
        {
            get
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    var shape = shapes[i];
                    var result = shape.Overlap(contactFilter);
                    
                    for (int j = 0; j < result.Length; j++)
                    {
                        yield return result[j];
                    }
                }
            }
        }
        
        public void Update()
        {
            var currentSet = new HashSet<Collider2D>(Overlapped);
            
            triggerAction(currentSet, colliders);
            
            colliders = currentSet;
        }

        private void OnEnter(HashSet<Collider2D> currentSet, HashSet<Collider2D> previousColliders)
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
                Triggered?.Invoke(collider);
            }
        }

        private void OnExit(HashSet<Collider2D> currentSet, HashSet<Collider2D> previousColliders)
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
                Triggered?.Invoke(collider);
            }
        }

        private void InitTriggerAction() =>
            triggerAction = type switch
            {
                EventType.Enter => OnEnter,
                EventType.Exit => OnExit,
                _ => triggerAction
            };
    }
}