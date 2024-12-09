using System;
using LSCore.BattleModule.States;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseMoveComp : BaseComp
    {
        public float speed;
        [SerializeReference] private BaseUnitAnim anim;
        [SerializeField] private State animState;
        [SerializeField] private State state;
        [NonSerialized] public Rigidbody2D rigidbody;
        protected UnitStates unitStates;
        
        private bool isStateActive;
        
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
            unitStates = transform.Get<UnitStates>();
            unitStates.StateEnabled += OnStateEnabled;
            unitStates.StateDisabled += OnStateDisabled;
        }

        private void OnStateEnabled(State obj)
        {
            SetState(obj, true);
        }

        private void OnStateDisabled(State obj)
        {
            SetState(obj, false);
        }

        private void SetState(State obj, bool active)
        {
            if (obj.Equals(state))
            {
                isStateActive = active;
            }
            else if(obj.Equals(animState))
            {
                if (active) anim.Animate();
                else anim.Stop();
            }
        }

        protected abstract void Move();

        protected override void FixedUpdate()
        {
            if (!unitStates.TrySetState(animState))
            {
                unitStates.TrySetState(state);
            }
            
            if (isStateActive)
            {
                Move();
            }
        }
    }
    
    [Serializable]
    public class MoveComp : BaseMoveComp
    {
        [SerializeField] protected FindTargetFactory findTargetFactory;
        protected FindTargetComp findTargetComp;
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
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(transform);
        }

        public Transform Target
        {
            get
            {
                findTargetComp.Find(out target);
                return target;
            }
        }

        protected override void OnRegister()
        {
            base.OnRegister();
            Reg(this);
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