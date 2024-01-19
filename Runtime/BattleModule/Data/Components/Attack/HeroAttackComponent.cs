using UnityEngine;

namespace Battle.Data.Components
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