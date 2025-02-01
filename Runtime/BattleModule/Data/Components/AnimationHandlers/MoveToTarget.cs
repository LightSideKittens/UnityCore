#if !UNITY_EDITOR
#define RUNTIME
#endif
using System;
using System.Collections.Generic;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class MoveToTarget : BadassAnimation.FloatHandler
    {
        public Rigidbody2D rigidbody;
        public FindTargetFactory findTargetFactory;
        public bool cacheTargetPosition;

        private Collider2D collider;
        private FindTargetComp findTargetComp;
        private Vector3 initialPosition;
        private Vector3 targetPosition;

#if UNITY_EDITOR
        [LabelText("Target for Editor testing")]
        public Collider2D target;
        protected override string Label => "Normalized value";
        protected override string PropertyPath { get; }
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            throw new NotImplementedException();
        }

        public override void StartAnimationMode()
        {
            throw new NotImplementedException();
        }
#endif

        public override Object Target => rigidbody;
        
        private Collider2D Targett
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    return target;
                }
#endif
                findTargetComp.Find(out var target1);
                return target1.Get<Collider2D>();
            }
        }

        private void InitFindTarget()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                return;
            }
#endif
            
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(rigidbody.transform);
        }
        
        protected override void OnStart()
        {
            var colliders = new Collider2D[1];
            rigidbody.GetAttachedColliders(colliders);
            collider = colliders[0];
            
            InitFindTarget();
            
            initialPosition = rigidbody.position;
            if (cacheTargetPosition)
            {
                UpdateTargetPosition();
            }
        }

        protected override void OnHandle()
        {
            if (!cacheTargetPosition)
            {
                UpdateTargetPosition();
            }
            
            if (World.IsEditMode)
            {
                rigidbody.transform.position = Vector3.Lerp(initialPosition, targetPosition, value);
                return;
            }

            rigidbody.position = Vector3.Lerp(initialPosition, targetPosition, value);
        }
        
        private void UpdateTargetPosition()
        {
            var dis = collider.Distance(Targett);
            targetPosition = dis.pointB;
        }
        

        protected override void OnStop()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                rigidbody.transform.position = initialPosition;
            }
#endif
        }
    }
}