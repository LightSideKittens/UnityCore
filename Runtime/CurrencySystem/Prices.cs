using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Prices : IEnumerable<BasePrice>
    {
#if UNITY_EDITOR

        [OdinSerialize] private HashSet<BasePrice> prices = new();
        private PricesPopup popup;
        private Type[] types;
        public PricesPopup Popup => popup;

        public void Init(params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = types[i].BaseType.GetNestedType("Price").MakeGenericType(types[i]);
            }
            
            this.types = types;
            popup = new PricesPopup(this);
        }

        public void Add(Type type)
        {
            prices.Add((BasePrice)Activator.CreateInstance(type));
        }
        
        public void Remove(Type type)
        {
            prices.Remove((BasePrice)Activator.CreateInstance(type));
        }

        public IEnumerator<BasePrice> GetEnumerator() => prices.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class PricesPopup : PopupWindowContent
        {
            private Dictionary<Type, bool> selections = new();
            private Prices prices;
            
            public PricesPopup(Prices prices)
            {
                this.prices = prices;
                foreach (var type in prices.types)
                {
                    selections[type] = prices.Any(x => x.GetType() == type);
                }
            }
            
            public override void OnGUI(Rect rect)
            {
                foreach (var type in prices.types)
                {
                    selections[type] = GUILayout.Toggle(selections[type], type.GetGenericArguments()[0].Name);
                }
            }

            public override void OnClose()
            {
                foreach (var type in prices.types)
                {
                    if (selections[type])
                    {
                        prices.Add(type);
                    }
                    else
                    {
                        prices.Remove(type);
                    }
                }
            }
        }
#endif
    }
}