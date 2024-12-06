using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LSCore.Async;
using LSCore.BattleModule;
using LSCore.Extensions;
using UnityEngine;

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
        public class RandomAttackSelector : AttackSelector
        {
            protected override BaseAttack Select(IEnumerable<BaseAttack> attacks)
            {
                var attack = attacks.RandomElement();
                return attack;
            }
        }
        
        [SerializeField] protected FindTargetFactory findTargetFactory;
        [SerializeReference] protected List<BaseAttack> attacks;
        [SerializeReference] protected AttackSelector attackSelector;
        
        protected FindTargetComp findTargetComp;
        public float cooldown = 0.5f;
        
        protected override void Init()
        {
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(transform);
            for (int i = 0; i < attacks.Count; i++)
            {
                attacks[i].Init(transform, findTargetComp);
            }
            
            Attack();
        }

        private void Attack()
        {
            var attack = attackSelector.Select(attacks);
            if (attack != null)
            {
                var (attackTween, cooldownTween) = attack.Trigger(cooldown);

                if (attackTween != null)
                {
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