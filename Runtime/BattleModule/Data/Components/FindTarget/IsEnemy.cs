using System;
using static LSCore.BattleModule.FindTargetComp;

namespace LSCore.BattleModule
{
    [Serializable]
    internal class IsEnemy : TargetChecker
    {
        protected override bool Check() => targetUnit.TeamId != selfUnit.TeamId;
    }
}