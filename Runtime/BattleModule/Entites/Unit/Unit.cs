using System;
using System.Collections.Generic;
using LSCore.Attributes;
using UnityEngine;
using static LSCore.BattleModule.StaticDict<UnityEngine.Transform,LSCore.BattleModule.Unit>;

namespace LSCore.BattleModule
{
    public partial class Unit : BaseUnit
    {
        public static event Action<Unit> Killed;
        public event Action Destroyed;
        
        [UniqueTypeFilter]
        [SerializeReference] private List<BaseComp> comps = new()
        {
            new HealthComp(),
            new MoveComp(),
            new AutoAttackComponent()
        };
        
        private readonly CompData compData = new();
        private new Transform transform;
        
        public override void Init(string userId, string teamId)
        {
            base.Init(userId, teamId);
            transform = GetComponent<Transform>();
            compData.transform = transform;
            Add(transform, this);
            
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].Init(compData);
            }
        }

        public void RegisterComps()
        {
            transform = gameObject.GetComponent<Transform>();
            compData.transform = transform;
            
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].Register(compData);
            }
        }

        public T GetComp<T>() where T : BaseComp
        {
            return TransformDict<T>.Get(transform);
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