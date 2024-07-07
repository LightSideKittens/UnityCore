using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseMoveComp : BaseComp
    {
        public float speed;
        public Rigidbody2D rigidbody;
        public float Speed => speed * Buffs;
        public Buffs Buffs { get; private set; }

        protected override void OnRegister() => Reg(this);

        protected override void Init()
        {
            useFixedUpdate = true;
            IsRunning = true;
            rigidbody = transform.GetComponent<Rigidbody2D>();
            rigidbody.position = transform.position;
            Buffs = new Buffs();
            data.reset += Buffs.Reset;
        }

        protected abstract void Move();

        protected override void FixedUpdate()
        {
            Move();
        }
    }
    
    [Serializable]
    public class MoveComp : BaseMoveComp
    {
        [SerializeField] private FindTargetComp findTargetComp;
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

        protected override void Init()
        {
            ShouldLookAtTarget = true;
            base.Init();
            findTargetComp.Init(transform);
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
        
        protected override void Move()
        {
            Buffs.Update();
            if (isTargetFound)
            {
                rigidbody.position += (Vector2)transform.up * (Speed * Time.deltaTime);
            }
        }
    }
}