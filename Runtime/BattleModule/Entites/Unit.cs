using System;
using UnityEngine;
using static LSCore.BattleModule.ObjectTo<LSCore.BattleModule.Unit>;

namespace LSCore.BattleModule
{
    public partial class Unit : BaseUnit
    {
        public static event Action<Unit> Killed;
        public event Action Destroyed;

        [SerializeReference] private BaseComp[] comps =
        {
            new HealthComp(),
            new FindTargetComp(),
            new MoveComp(),
            new ColliderHitBoxComp(),
            new AutoAttackComponent()
        };
        
        private readonly CompData compData = new();
        private new Transform transform;
        
        public override void Init(string userId, string teamId)
        {
            base.Init(userId, teamId); 
            transform = GetComponent<Transform>();
            Add(transform, this);
            
            for (int i = 0; i < comps.Length; i++)
            {
                comps[i].Init(transform, compData);
            }
        }

        public void RegisterComps()
        {
            transform = gameObject.GetComponent<Transform>();
            
            for (int i = 0; i < comps.Length; i++)
            {
                comps[i].Register(transform);
            }
        }

        public T GetComp<T>() where T : BaseComp
        {
            return ObjectTo<T>.Get(transform);
        }
        
        public override void OnInit()
        {
            compData.onInit?.Invoke();
        }

        public void Run()
        {
            compData.update?.Invoke();
        }

        public void FixedRun()
        {
            compData.fixedUpdate?.Invoke();
        }

        private void Resett()
        {
            compData.reset?.Invoke();
        }

        private void Enable()
        {
            compData.enable?.Invoke();
            gameObject.SetActive(true);
        }

        private void Disable()
        {
            compData.disable?.Invoke();
            gameObject.SetActive(false);
        }

        public void Kill()
        {
            Release(this);
            Killed?.Invoke(this);
        }
        
        public override void DeInit()
        {
            base.DeInit();
            Remove(transform);
            compData.destroy?.Invoke();
            Destroyed?.Invoke();
        }
    }
}