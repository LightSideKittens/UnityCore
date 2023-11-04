using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using static LSCore.Currencies;

namespace LSCore
{
    [Serializable]
    public class Funds : IEnumerable<BaseFund>
    {
        [SerializeField] 
        [DisableIf("isControls")] 
        public CurrencyIdGroup group;
        
        [OdinSerialize]
        [ValueDropdown("AwailableFunds", IsUniqueList = true)]
        private HashSet<BaseFund> funds = new();
        
        public static void OnChanged(Id id, Action<int> onChanged)
        {
            if (onChangedActions.TryGetValue(id, out var action))
            {
                onChanged += action;
            }
            onChangedActions[id] = onChanged;
        }
        
        public static void RemoveOnChanged(Id id, Action<int> onChanged)
        {
            if (onChangedActions.TryGetValue(id, out var action))
            {
                onChanged -= action;
            }
            onChangedActions[id] = onChanged;
        }
        
        public static void ClearOnChanged(Id id)
        {
            onChangedActions.Remove(id);
        }


        public IEnumerator<BaseFund> GetEnumerator() => funds.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Earn()
        {
            foreach (var funds in funds)
            {
                funds.Earn();
            }
        }
        
        public bool Spend(out Action spend)
        {
            var transactions = new Transactions(true);
            
            foreach (var funds in funds)
            {
                transactions.Add(funds.Spend);
            }

            return transactions.Union()(out spend);
        }

#if UNITY_EDITOR
        [NonSerialized] public bool isControls;

        public void Control()
        {
            foreach (var funds in this)
            {
                funds.isControls = true;
            }
        }
        
        private ValueDropdownList<Fund> awailableFunds;
        private IList<ValueDropdownItem<Fund>> AwailableFunds
        {
            get
            {
                awailableFunds ??= new ValueDropdownList<Fund>();
                awailableFunds.Clear();
                if (group == null) return awailableFunds;
                
                foreach (var id in group)
                {
                    awailableFunds.Add(id, new Fund {id = id});
                }
                
                return awailableFunds;
            }
        }
#endif
    }
}