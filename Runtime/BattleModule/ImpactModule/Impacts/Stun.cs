using System;
using LSCore.Async;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Stun : Impact
    {
        public float time = 0.5f;
        
        public override void Apply(Transform target)
        {
            var move = target.Get<MoveComp>();

            if (!move.Enabled) return;
            
            move.SetEnabled(false);
            Wait.Delay(time, () =>
            {
                move.SetEnabled(true);
            });
        }
    }
}