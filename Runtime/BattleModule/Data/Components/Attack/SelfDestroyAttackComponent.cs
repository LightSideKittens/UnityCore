using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    internal class SelfDestroyAttackComponent : AutoAttackComponent
    {
        /*protected override Tween AttackAnimation()
        {
            attackLoopEmiter.Kill();
            return transform.DOMove(lastHitPoint, duration)
                .OnComplete(() =>
                {
                    TryApplyDamage();
                    if(transform.TryGet<Health>(out var health)) health.Kill();
                });
        }*/
        protected override void Attack(Transform target)
        {
            throw new NotImplementedException();
        }
    }
}