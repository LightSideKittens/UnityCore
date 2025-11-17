using System;
using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    public class FundsExchanger : MonoBehaviour
    {
        [Serializable]
        public class DoTryExchange : DoIt
        {
            public FundsExchanger exchanger;
            [SerializeReference] public DoIt onExchanged;
            
            public override void Do()
            {
                if (exchanger.TryExchange(out var exchange))
                {
                    exchange();
                    onExchanged?.Do();
                }
            }
        }
        
        public FundText from;
        public FundText to;

        private void Awake()
        {
#if UNITY_EDITOR
            if(from == null || to == null) return;
#endif
            lastFrom = from;
            lastTo = to;
        }

        public void SetAmountForTo(int amount)
        {
            to.Number = amount;
        }

        public bool TryExchange(out Action exchange)
        {
            return ExchangeTable.TryExchange(from.Id, to.Id, from, out exchange);
        }

        private void UpdateText(FundText from, FundText to)
        {
            var earnAmount = ExchangeTable.Convert(from.Id, to.Id, from);
            to.Number = earnAmount;
        }
        
        private int lastFrom;
        private int lastTo;
        
        private void Update()
        {
#if UNITY_EDITOR
            if(from == null || to == null) return;
#endif
            if (lastFrom != from)
            {
                lastFrom = from;
                UpdateText(from, to);
                lastTo = to;
            }

            if (lastTo != to)
            {
                lastTo = to;
                UpdateText(to, from);
                lastFrom = from;
            }
        }
    }
}