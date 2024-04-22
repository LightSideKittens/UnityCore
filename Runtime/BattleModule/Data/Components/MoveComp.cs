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
        public float Speed => speed * Buffs;
        
        public Buffs Buffs { get; private set; }

        protected override void OnRegister() => Reg(this);

        protected override void Init()
        {
            IsRunning = true;
            rigidbody = transform.GetComponent<Rigidbody2D>();
            findTargetComp.Init(transform);
            Buffs = new Buffs();
            data.reset += Buffs.Reset;
        }
        
        protected virtual void Move()
        {
            Buffs.Update();
            if (findTargetComp.Find(out var target))
            {
                var position = rigidbody.position;
                var targetPos = (Vector2)target.position;
                var direction = targetPos - position;
                direction = direction.normalized;
                position += direction * (Speed * Time.deltaTime);
                transform.up = targetPos - position;
                rigidbody.position = position;
            }
        }

        protected override void FixedUpdate()
        {
            Move();
        }
    }
}