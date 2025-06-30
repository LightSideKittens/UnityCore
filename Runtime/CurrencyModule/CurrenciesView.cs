using System;
using System.Collections.Generic;
using DG.Tweening;
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
            
            public override void Do()
            {
                if (anims.TryGetValue(id, out var queue))
                {
                    queue.Dequeue()();
                    if (queue.Count == 0)
                    {
                        anims.Remove(id);
                    }
                }
            }
        }
        
        private static Dictionary<Id, Queue<Action>> anims = new();
        
        [Serializable]
        public class Data
        {
            [Id(typeof(CurrencyIdGroup))]
            [SerializeField] private Id id;

            [SerializeField] private GameObject gameObject;
            [SerializeField] private LSButton button;
            [SerializeField] private LSText text;
            private int lastAmount;
            
            public void Init()
            {
                lastAmount = Funds.GetAmount(id);
                text.text = $"{lastAmount}";
                Funds.AddOnChanged(id, OnChanged);
            }

            public void DeInit()
            {
                Funds.RemoveOnChanged(id, OnChanged);
                anims.Remove(id);
            }

            private void OnChanged(int value)
            {
                var la = lastAmount;
                Action anim = () => DOVirtual.Int(la, value, 0.5f, v => text.text = $"{v}");
                
                if (!anims.TryGetValue(id, out var queue))
                {
                    queue = new Queue<Action>();
                    anims[id] = queue;
                }
                
                queue.Enqueue(anim);
                lastAmount = value;
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