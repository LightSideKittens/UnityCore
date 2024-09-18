using UnityEngine;

namespace LSCore
{
    public abstract class FundAction : LSAction
    {
        [SerializeReference] public BaseFund fund;
    }

    public class Earn : FundAction
    {
        public override void Invoke()
        {
            fund.Earn();
        }
    }
    
    public class Spend : FundAction
    {
        [SerializeReference] public LSAction onSpended;
        
        public override void Invoke()
        {
            if (fund.Spend(out var action))
            {
                onSpended.Invoke();
                action();
            }
        }
    }
}