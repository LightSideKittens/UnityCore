using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class ExchangeTable : ScriptableObject
    {
        [Serializable]
        internal struct Pair
        {
            [Id(typeof(CurrencyIdGroup))] [HorizontalGroup] [LabelText("1")] [LabelWidth(10)] public Id from;
            [HorizontalGroup(Width = 100)] [LabelText("=")] [LabelWidth(10)] public float rate;
            [Id(typeof(CurrencyIdGroup))] [HorizontalGroup] [HideLabel] public Id to;
        }

        [Serializable]
        internal struct Data
        {
            [InlineProperty]
            [HorizontalGroup("table")] 
            [HideLabel] public Pair pair;

            public int ConvertFrom(int from) => Mathf.CeilToInt(from * pair.rate);
            public int ConvertTo(int to) => Mathf.CeilToInt(to / pair.rate);
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