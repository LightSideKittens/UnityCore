using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSCore.Attributes;
using UnityEngine;
using static LSCore.Currencies;

namespace LSCore
{
    [Unwrap]
    [Serializable]
    public class Funds : IEnumerable<BaseFund>
    {
        public static string lastTransactionId;
        [SerializeReference] private List<BaseFund> funds = new();

        public bool Contains(Id id)
        {
            return funds.Any(x => x.Id == id);
        }
        
        public bool TryGetById(Id id, out BaseFund fund)
        {
            fund = GetById(id);
            return fund != null;
        }
        
        public BaseFund GetById(Id id)
        {
            foreach (var item in funds)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            
            return null;
        }
        
        public void Earn()
        {
            foreach (var fund in funds)
            {
                fund.Earn();
            }
        }
        
        public bool Spend(out Action spend)
        {
            var transactions = new Transactions(true);
            
            foreach (var fund in funds)
            {
                transactions.Add(fund.Spend);
            }

            return transactions.Union()(out spend);
        }
        
        
        public static void AddOnChanged(Id id, Action<(int last, int current)> onChanged, bool callImmediate = false)
        {
            if(onChanged == null) return;
            
            onChangedActions.TryGetValue(id, out var action);
            action += onChanged;
            onChangedActions[id] = action;
            
            if (callImmediate)
            {
                var amount = GetAmount(id);
                onChanged((amount, amount));
            }
        }
        
        public static void RemoveOnChanged(Id id, Action<(int last, int current)> onChanged)
        {
            if (onChangedActions.TryGetValue(id, out var action))
            {
                action -= onChanged;
            }

            if (action == null)
            {
                onChangedActions.Remove(id);
                return;
            }
            
            onChangedActions[id] = action;
        }
        
        public static void ClearOnChanged(Id id)
        {
            onChangedActions.Remove(id);
        }

        public static int GetAmount(Id id) => Currencies.GetAmount(id);
        public static void Earn(Id id, uint value) => Currencies.Earn(id, (int)value);
        public static bool Spend(Id id, uint value, out Action action) => Currencies.Spend(id, (int)value, out action);
        public static void ForceSpend(Id id, uint value) => Currencies.ForceSpend(id, (int)value);
        public static void Remove(Id id) => Currencies.Remove(id);
        
        public IEnumerator<BaseFund> GetEnumerator() => funds.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}