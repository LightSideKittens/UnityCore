using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class FundAction : DoIt
    {
        public Funds funds;
        public string placement;

        public override void Do()
        {
            Funds.lastPlacement = placement;
        }
    }

    [Serializable]
    public class Earn : FundAction
    {
        public override void Do()
        {
            base.Do();
            funds.Earn();
        }
    }
    
    [Serializable]
    public class Spend : FundAction
    {
        [SerializeReference] public DoIt onSpent;
        
        public override void Do()
        {
            base.Do();
            if (funds.Spend(out var action))
            {
                onSpent.Do();
                action();
            }
        }
    }
}