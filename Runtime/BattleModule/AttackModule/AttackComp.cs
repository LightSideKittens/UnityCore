using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LSCore.Async;
using LSCore.BattleModule;
using LSCore.BattleModule.States;
using LSCore.Extensions;
using UnityEngine;
using State = LSCore.BattleModule.States.State;

namespace LSCore
{
    [Serializable]
    public class AttackComp : BaseComp
    {
        [Serializable]
        public abstract class AttackSelector
        {
            public BaseAttack Select(List<BaseAttack> attacks)
            {
                return Select(attacks.Where(x => x.CanAttack));
            }
            
            protected abstract BaseAttack Select(IEnumerable<BaseAttack> attacks);
        }

        [Serializable]
        public class Random : AttackSelector
        {
            protected override BaseAttack Select(IEnumerable<BaseAttack> attacks)
            {
                var attack = attacks.RandomElement();
                return attack;
            }
        }
        
        [Serializable]
        public class First : AttackSelector
        {
            protected override BaseAttack Select(IEnumerable<BaseAttack> attacks)
            {
                var attack = attacks.FirstOrDefault();
                return attack;
            }
        }
        
        [SerializeField] protected State state;
        [SerializeField] protected FindTargetFactory findTargetFactory;
        [SerializeReference] protected List<BaseAttack> attacks;
        [SerializeReference] protected AttackSelector attackSelector;
        
        protected FindTargetComp findTargetComp;
        protected BaseAttack currentAttack;
        protected UnitStates unitStates;
        public float cooldown = 0.5f;
        
        protected override void Init()
        {
            unitStates = transform.Get<UnitStates>();
            unitStates.StateDisabled += OnStateDisabled;
            
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(transform);
            for (int i = 0; i < attacks.Count; i++)
            {
                attacks[i].Init(transform, findTargetComp);
            }
            
            Attack();
            data.destroy += DeInit;
        }

        private void DeInit()
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                attacks[i].DeInit();
            }
        }

        private void OnStateDisabled(State state)
        {
            currentAttack?.Stop();
        }

        private void Attack()
        {
            unitStates.RemoveState(state);
            var attack = attackSelector.Select(attacks);
            if (attack != null)
            {
                var (attackTween, cooldownTween) = attack.Trigger(cooldown);
                currentAttack = attack;
                
                if (attackTween != null)
                {
                    if (!unitStates.TrySetState(state))
                    {
                        goto waitFrame;
                    }
                    
                    var sequence = DOTween.Sequence()
                        .Append(attackTween)
                        .OnComplete(Attack);

                    if (cooldownTween != null)
                    {
                        sequence.Append(cooldownTween);
                    }
                }
                else
                {
                    goto waitFrame;
                }
            }
            else
            {
                goto waitFrame;
            }
            
            return;
            
            waitFrame:
            Wait.Frames(1, Attack);
        }
    }
}