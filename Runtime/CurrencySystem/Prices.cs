using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Prices : IEnumerable<BasePrice>
    {
        [SerializeField] 
        [DisableIf("isControls")] 
        public CurrencyIdGroup group;
        
        [OdinSerialize]
        [ValueDropdown("AwailablePrices", IsUniqueList = true)]
        private HashSet<BasePrice> prices = new();
        
        public IEnumerator<BasePrice> GetEnumerator() => prices.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Earn()
        {
            foreach (var price in prices)
            {
                price.Earn();
            }
        }
        
        public bool Spend(out Action spend)
        {
            var allCanSpend = true;
            spend = delegate {  };
            
            foreach (var price in prices)
            {
                if (price.Spend(out var newSpend))
                {
                    spend += newSpend;
                    continue;
                }

                allCanSpend = false;
                break;
            }

            return allCanSpend;
        }

#if UNITY_EDITOR
        [NonSerialized] public bool isControls;

        public void Control()
        {
            foreach (var price in this)
            {
                price.isControls = true;
            }
        }
        
        private ValueDropdownList<Price> awailablePrices;
        private IList<ValueDropdownItem<Price>> AwailablePrices
        {
            get
            {
                awailablePrices ??= new ValueDropdownList<Price>();
                awailablePrices.Clear();

                foreach (var id in group)
                {
                    awailablePrices.Add(id, new Price {id = id});
                }
                
                return awailablePrices;
            }
        }
#endif
    }
}