﻿using System;
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
                action -= onChanged;
            }
            onChangedActions[id] = action;
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
}