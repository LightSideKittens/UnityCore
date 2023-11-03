using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public static class FundViewExtensions
    {
        public static void Earn(this IEnumerable<FundView> funds)
        {
            foreach (var view in funds)
            {
                view.Earn();
            }
        }

        public static bool Spend(this IEnumerable<FundView> funds, out Action spend)
        {
            var transaction = new Transactions();
            foreach (var view in funds)
            {
                transaction.Add(view.Spend);
            }
            return transaction.Union()(out spend);
        }
    }

    public class FundView : MonoBehaviour
    {
        [SerializeField] private Id id;
        [SerializeField] private LSNumber number;
        
        public void Earn()
        {
            Currencies.Earn(id, number);
        }

        public bool Spend(out Action spend)
        {
            return Currencies.Spend(id, number, out spend);
        }
    }
}