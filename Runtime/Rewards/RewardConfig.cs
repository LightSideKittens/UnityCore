using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public abstract class RewardConfig : ScriptableObject, IReward
    {
        public abstract IEnumerable<IReward> rewards { get; }

        public virtual bool Claim(out Action claim)
        {
            var transactions = new Transactions();

            foreach (var reward in rewards)
            {
                transactions.Add(reward.Claim);
            }

            return transactions.Union()(out claim);
        }
    }
}