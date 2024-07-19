using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;
using static LSCore.Currencies;

namespace LSCore
{
    [Serializable]
    public class Funds : IEnumerable<BaseFund>
    {
        [SerializeReference] private List<BaseFund> funds = new();
        
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
        
        
        public static void AddOnChanged(Id id, Action<int> onChanged, bool callImmediate = false)
        {
            onChangedActions.TryGetValue(id, out var action);
            action += onChanged;
            onChangedActions[id] = action;
            
            if (callImmediate)
            {
                onChanged(GetValue(id));
            }
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

        public static int GetValue(Id id) => Currencies.GetValue(id);
        public static void Earn(Id id, int value) => Currencies.Earn(id, value);
        public static bool Spend(Id id, int value, out Action action) => Currencies.Spend(id, value, out action);
        public static void Remove(Id id) => Currencies.Remove(id);
        
        public IEnumerator<BaseFund> GetEnumerator() => funds.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
#if UNITY_EDITOR
    public class FundsDrawer : OdinValueDrawer<Funds>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Property.Children.First().Draw(label);
        }
    }
#endif
}