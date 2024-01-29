using System;
using UnityEngine;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.Unit>;

namespace LSCore.BattleModule
{
    public partial class Unit : BaseUnit
    {
        public static event Action<Unit> Killed;
        public event Action Destroyed;

        [SerializeReference] private BaseComp[] comps =
        {
            new BaseHealthComp(),
            new FindTargetComp(),
            new MoveComp(),
            new ColliderHitBoxComp(),
            new ShootAttackComp()
        };
        
        private CompData compData;
        
        public override void Init(string userId, string teamId)
        {
            base.Init(userId, teamId);
            Add(transform, this);
            compData.transform = transform;

            for (int i = 0; i < comps.Length; i++)
            {
                comps[i].Init(compData);
            }
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

        public void Resett()
        {
            compData.reset?.Invoke();
        }

        public void Enable()
        {
            compData.enable?.Invoke();
            gameObject.SetActive(true);
        }
        
        public void Disable()
        {
            compData.disable?.Invoke();
            gameObject.SetActive(false);
        }

        public void Kill()
        {
            Release(this);
            Killed?.Invoke(this);
        }
        
        public override void Destroy()
        {
            base.Destroy();
            compData.destroy?.Invoke();
            Remove(transform);
            Destroyed?.Invoke();
        }
    }
}