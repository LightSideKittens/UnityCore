using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore
{
    public class CurrenciesView : MonoBehaviour
    {
        [Serializable]
        public class Anim : DoIt
        {
            [Id(typeof(CurrencyIdGroup))]
            [SerializeField] private Id id;
            public string placement;
            
            public override void Do()
            {
                if (anims.TryGetValue(id, out var queue))
                {
                    var p = placement.IsNullOrEmpty() ? "default" : placement;
                    if (queue.TryGetValue(p, out var action))
                    {
                        action();
                        queue.Remove(p); 
                        if (queue.Count == 0)
                        {
                            anims.Remove(id);
                        }
                    }
                }
            }
        }
        
        private static Dictionary<Id, Dictionary<string, Action>> anims = new();
        
        [Serializable]
        public class Data
        {
            [Id(typeof(CurrencyIdGroup))]
            [SerializeField] private Id id;

            [SerializeField] private GameObject gameObject;
            [SerializeField] private LSButton button;
            [SerializeField] private LSText text;
            
            public void Init()
            {
                text.text = $"{Funds.GetAmount(id)}";
                Funds.AddOnChanged(id, OnChanged);
            }

            public void DeInit()
            {
                Funds.RemoveOnChanged(id, OnChanged);
                anims.Remove(id);
            }

            private void OnChanged((int last, int current) data)
            {
                var diff = data.current - data.last;
                if (!anims.TryGetValue(id, out var queue))
                {
                    queue = new Dictionary<string, Action>();
                    anims[id] = queue;
                }
                var placement = Funds.lastPlacement.IsNullOrEmpty() ? "default" : Funds.lastPlacement;
                queue[placement] = Anim;
                
                void Anim()
                {
                    var cur = int.Parse(text.text);
                    DOTween.Complete(text);
                    var completedAmount = int.Parse(text.text);
                    text.text = cur.ToString();
                    DOVirtual.Int(cur, completedAmount + diff, 0.5f, v => text.text = $"{v}").SetTarget(text);
                }
            }
        }

        [SerializeField] private Data[] data;

        private void Awake()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Init();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].DeInit();
            }
        }
    }
}