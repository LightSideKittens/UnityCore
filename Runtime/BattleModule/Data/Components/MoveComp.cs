using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public class MoveComp : BaseComp
    {
        public float speed;
        [SerializeField] private FindTargetComp findTargetComp;
        protected Rigidbody2D rigidbody;
        private bool isTargetFound;
        private Transform target;
        
        private bool shouldLookAtTarget;
        public bool ShouldLookAtTarget
        {
            get => shouldLookAtTarget;
            set
            {
                if (value == shouldLookAtTarget) return;
                
                shouldLookAtTarget = value;
                if (value)
                {
                    data.fixedUpdate += LookAtTarget;
                }
                else
                {
                    data.fixedUpdate -= LookAtTarget;
                }
            }
        }
        
        public float Speed => speed * Buffs;
        public Buffs Buffs { get; private set; }

        protected override void OnRegister() => Reg(this);

        protected override void Init()
        {
            ShouldLookAtTarget = true;
            IsRunning = true;
            rigidbody = transform.GetComponent<Rigidbody2D>();
            findTargetComp.Init(transform);
            Buffs = new Buffs();
            data.reset += Buffs.Reset;
        }

        private void LookAtTarget()
        {
            isTargetFound = findTargetComp.Find(out target);
            if (isTargetFound)
            {
                var position = (Vector3)rigidbody.position;
                var targetPos = target.position;
                transform.up = targetPos - position;
            }
        }

        protected virtual void Move()
        {
            Buffs.Update();
            if (isTargetFound)
            {
                rigidbody.position += (Vector2)transform.up * (Speed * Time.deltaTime);
            }
        }

        protected override void FixedUpdate()
        {
            Move();
        }
    }
}