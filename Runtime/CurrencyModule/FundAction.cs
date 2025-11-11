using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class FundAction : DoIt
    {
        public Funds funds;
        public string transactionId;

        public override void Do()
        {
            Funds.lastTransactionId =  transactionId;
        }
    }

    [Serializable]
    public class Earn : FundAction
    {
        public override void Do()
        {
            base.Do();
            funds.Earn();
            Funds.lastTransactionId = null;
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
            Funds.lastTransactionId = null;
        }
    }
}