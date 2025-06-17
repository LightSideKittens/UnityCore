using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class FundAction : DoIt
    { 
        public Funds funds;
    }

    [Serializable]
    public class Earn : FundAction
    {
        public override void Do()
        {
            funds.Earn();
        }
    }
    
    [Serializable]
    public class Spend : FundAction
    {
        [SerializeReference] public DoIt onSpent;
        
        public override void Do()
        {
            if (funds.Spend(out var action))
            {
                onSpent.Do();
                action();
            }
        }
    }
}