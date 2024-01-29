using UnityEngine;

namespace LSCore.BattleModule
{
    internal class HeroAttackComponent : AutoAttackComponent
    {
        [SerializeReference] private AutoAttackComponent autoAttack;
        [SerializeReference] private AutoAttackComponent mainAttack;
        
        
        protected override void OnInit()
        {
            
        }

        protected override void Attack(Transform target)
        {
            throw new System.NotImplementedException();
        }
    }
}