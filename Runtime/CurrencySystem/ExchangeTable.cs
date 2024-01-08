using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class ExchangeTable : ScriptableObject
    {
        [Serializable]
        private struct Pair
        {
            [CurrencyId] public Id from;
            [CurrencyId] public Id to;
        }

        [Serializable]
        private struct Data
        {
            public Pair pair;
            public float rate;

            public int ConvertFrom(int from) => Mathf.CeilToInt(from * rate);
            public int ConvertTo(int to) => Mathf.CeilToInt(to / rate);
        }

        [SerializeField] private Data[] data;
        private Dictionary<Pair, Data> byId = new();
        private static ExchangeTable instance;

        public void Init()
        {
            instance = this;
            byId.Clear();
            
            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                byId.Add(d.pair, d);
            }
        }

        public static int Convert(Id from, Id to, int amount) => instance.Internal_Convert(from, to, amount);
        
        private int Internal_Convert(Id from, Id to, int amount)
        {
            var pair = new Pair {from = from, to = to};
            var invertedPair = new Pair {from = to, to = from};

            if (byId.TryGetValue(pair, out var d))
            {
                return d.ConvertFrom(amount);
            }
            
            if (byId.TryGetValue(invertedPair, out d))
            {
                return d.ConvertTo(amount);
            }

            return 0;
        }
    }
}