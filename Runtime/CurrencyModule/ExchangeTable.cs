using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class ExchangeTable : SingleScriptableObject<ExchangeTable>, ISerializationCallbackReceiver
    {
        [Serializable]
        internal struct Pair
        {
            [Id(typeof(CurrencyIdGroup))] [HorizontalGroup] [LabelText("1")] [LabelWidth(10)] public Id from;
            [HorizontalGroup(Width = 100)] [LabelText("=")] [LabelWidth(10)] [MinValue(0.0001f)] public float rate;
            [Id(typeof(CurrencyIdGroup))] [HorizontalGroup] [HideLabel] public Id to;
            
            public override bool Equals(object other)
            {
                if (other is Pair pair)
                {
                    return pair.from == from && pair.to == to;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return from.GetHashCode() - to.GetHashCode();
            }
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

        public static bool TryExchange(Id from, Id to, int amount, out Action exchange)
        {
            exchange = null;
            var earnAmount = Convert(from, to, amount);
            var can = Currencies.Spend(from, amount, out var spend);
            if (can)
            {
                exchange = () =>
                {
                    spend();
                    Currencies.Earn(to, earnAmount);
                };
            }
            
            return can;
        }
        
        public static int Convert(Id from, Id to, int amount) => Instance.Internal_Convert(from, to, amount);
        
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

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            EditorApplication.update += Update;
#endif
            World.Updated += Update;

            void Update()
            {
#if UNITY_EDITOR
                EditorApplication.update -= Update;
#endif
                World.Updated -= Update;
                byId.Clear();
            
                for (int i = 0; i < data.Length; i++)
                {
                    var d = data[i];
                    byId.Add(d.pair, d);
                }
            }
        }
    }
}