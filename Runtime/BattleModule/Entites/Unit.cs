using System;
using Battle.Data.Components;
using Battle.Data.Components.HitBox;
using UnityEngine;
using static Battle.ObjectsByTransfroms<Battle.Data.Unit>;

namespace Battle.Data
{
    public partial class Unit : BaseUnit
    {
        public static event Action<Unit> Killed;
        public event Action Destroyed;
        
        [SerializeReference] private MoveComponent moveComponent = new();
        [SerializeReference] private FindTargetComponent findTargetComponent = new ();
        [SerializeReference] private BaseAttackComponent attackComponent;
        [SerializeReference] private BaseHealthComponent healthComponent = new();
        [SerializeReference] private HitBoxComponent hitBoxComponent = new ColiderHitBoxComponent();
        
        private float radius;

        public override void Init(string userId)
        {
            base.Init(userId);
            Add(transform, this);
            
            hitBoxComponent.Init(transform);
            findTargetComponent.Init(transform);
            moveComponent.Init(transform, findTargetComponent);
            healthComponent.Init(transform, IsOpponent);
            attackComponent.Init(transform);
        }

        public void Run()
        {
            attackComponent.Update();
            healthComponent.Update();
        }

        public void FixedRun()
        {
            moveComponent.Update();
        }

        public void Resett()
        {
            attackComponent.Buffs.Reset();
            moveComponent.Buffs.Reset();
            healthComponent.Reset();
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            attackComponent.Enable();
        }
        
        public void Disable()
        {
            gameObject.SetActive(false);
            attackComponent.Disable();
        }

        public void Kill()
        {
            Release(this);
            Killed?.Invoke(this);
        }
        
        public override void Destroy()
        {
            base.Destroy();
            hitBoxComponent.Destroy();
            attackComponent.Destroy();
            healthComponent.Destroy();
            moveComponent.Destroy();
            Remove(transform);
            Destroyed?.Invoke();
        }
    }
}