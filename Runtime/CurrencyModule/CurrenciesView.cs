using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore
{
    public class CurrenciesView : MonoBehaviour
    {
        [Serializable]
        public class Anim : DoIt
        {
            [Id(typeof(CurrencyIdGroup))]
            public Id id;
            public string placement;
            
            public override void Do()
            {
                /*if (TryGetAnimData(id, placement, out var animData))
                {
                    animData.anim();
                }*/
            }
            
            /*public static bool TryGetAnimData(Id id, string placement, out (int totalDiff, Action anim) data)
            {
                if (anims.TryGetValue(id, out var queue))
                {
                    var p = GetTransactionId(placement);
                    if (queue.Remove(p, out data))
                    {
                        if (queue.Count == 0)
                        {
                            anims.Remove(id);
                        }
                        return true;
                    }
                }
                
                data = default;
                return false;
            }*/
            
            public static int GetParticlesCount(int fundsCount)
            {
                if (fundsCount > 10)
                {
                    fundsCount = (int)Mathf.Lerp(10, 20, (fundsCount - 10) / 500f);
                }
                
                return fundsCount;
            }
        }

        [Serializable]
        public class ParticlesPrefabs : Get<IEnumerable<KeyValuePair<string, UIParticleRenderer>>>
        {
            [Serializable]
            public struct D
            {
                [Id(typeof(CurrencyIdGroup))]
                public Id id;
                public UIParticleRenderer particles;
            }

            public D[] data;

            public override IEnumerable<KeyValuePair<string, UIParticleRenderer>> Data
            {
                get
                {
                    foreach (var d in data)
                    {
                        yield return new(d.id, d.particles);
                    }
                }
            }
        }
        
        [Serializable]
        public class ParticlesAnim : DoIt
        {
            [Id(typeof(CurrencyIdGroup))]
            public Id id;
            public string transactionId;
            [SerializeReference] public Get<Transform> from;
            [SerializeReference] public Get<Transform> to;
            [SerializeReference] public DoIt onAttracted;
            [SerializeReference] public DoIt onComplete;
            
            public override void Do()
            {
                /*var p = Anim.GetTransactionId(transactionId);
                if (Anim.TryGetAnimData(id, p, out var animData))
                {
                    bool animCalled = false;
                    ParticlesAttractor.Create(id, from, to, Anim.GetParticlesCount(animData.totalDiff), OnAttracted, onComplete);
                    
                    void OnAttracted()
                    {
                        if (!animCalled)
                        { 
                            animData.anim(); 
                            animCalled = true;
                        }
                        onAttracted?.Do();
                    }
                }*/
            }
        }
        
        [Serializable]
        public class ApplyTransaction : DoIt
        {
            public string transactionId;
            [SerializeReference] public Get<Transform> from;
            [SerializeReference] public DoIt onAttracted;
            [SerializeReference] public DoIt onComplete;
            
            //TODO: Implement count and scale changing of particles
            public override void Do()
            {
                foreach (var (id, statView) in statViews)
                {
                    if (RemoveTransaction(statView.id, transactionId, out var animData))
                    {
                        bool animCalled = false;
                        var diff = animData.Diff;
                        
                        if (diff > 0)
                        { 
                            ParticlesAttractor.Create(id, from, statView.attractionPoint, Anim.GetParticlesCount(diff), OnAttracted, onComplete);
                        }
                        
                        void OnAttracted()
                        {
                            if (!animCalled)
                            { 
                                statView.Anim(diff); 
                                animCalled = true;
                            }
                            
                            onAttracted?.Do();
                        }
                    }
                }
                
            }
        }
        
        private static Dictionary<Id, StatView> statViews = new();
        private static Dictionary<Id, int> statViewsCount = new();
        
        [Serializable]
        public class StatView
        {
            [Id(typeof(CurrencyIdGroup))] public Id id;
            public Transform attractionPoint;
            public LSButton button;
            public FundText text;
            private string[] acceptableTransactions;
            
            public void Init(string[] acceptableTransactions)
            {
                this.acceptableTransactions = acceptableTransactions;
                ChangedWithoutTransaction += OnChangedWithoutTransaction;
            }

            private void OnChangedWithoutTransaction(string idStr, (int last, int current) data)
            {
                if (idStr == id)
                { 
                    var diff =  data.current - data.last;
                    text.Number += diff;
                }
            }

            public void OnEnable()
            {
                statViews[id] = this;
                statViewsCount.TryGetValue(id, out var count);
                count++;
                statViewsCount[id] = count;
                
                var amount = Funds.GetAmount(id);
                
                foreach (var transaction in GetTransactions(id))
                {
                    amount -= transaction.Diff;
                }
                
                text.Number = amount;
            }

            public void OnDisable()
            {
                statViewsCount.TryGetValue(id, out var count);
                count--;
                if (count == 0)
                {
                    statViewsCount.Remove(id);
                    statViews.Remove(id);
                }
                else
                {
                    statViewsCount[id] = count;
                }
                
                foreach (var acceptableTransaction in acceptableTransactions)
                {
                    RemoveTransaction(id, acceptableTransaction);
                }
            }

            public void DeInit()
            {
                ChangedWithoutTransaction -= OnChangedWithoutTransaction;
            }

            public Tween Anim(int diff)
            {
                var lastCount = (int)text.Number;
                DOTween.Complete(text);
                var completedCount = (int)text.Number;
                return DOVirtual.Int(lastCount, completedCount + diff, 0.5f, v => text.Number = v).SetTarget(text);
            }
        }
        
        public class Transaction
        {
            public int? from;
            public int to;
            public int Diff => to - from.Value;
        }
            
        public static Dictionary<string, Dictionary<string, Transaction>> transactions = new();
        private static event Action<string, (int last, int current)> ChangedWithoutTransaction;
        
        [RuntimeInitializeOnLoadMethod]
        public static void Initialization()
        {
            Currencies.Changed -= OnChange;
            Currencies.Changed += OnChange;
        }

        private static void OnChange(string id, (int last, int current) data)
        {
            if (!Funds.lastTransactionId.IsNullOrEmpty())
            {
                AddTransaction(id, Funds.lastTransactionId, data.last, data.current);
            }
            else
            {
                ChangedWithoutTransaction?.Invoke(id, data);
            }
        }

        public static void AddTransaction(string currencyId, string transactionId, int from, int to)
        {
            var currency = transactions.As(currencyId);
            var transaction = currency.As(transactionId);
            if (transaction.from == null)
            {
                transaction.from = from;
            }
                
            transaction.to = to;
        }
            
        public static bool RemoveTransaction(string currencyId, string transactionId)
        {
            var currency = transactions.As(currencyId);
            return currency.Remove(transactionId);
        }
        
        public static bool RemoveTransaction(string currencyId, string transactionId, out Transaction transaction)
        {
            var currency = transactions.As(currencyId);
            return currency.Remove(transactionId, out transaction);
        }
            
        public static bool TryGetTransaction(string currencyId, string transactionId, out Transaction transaction)
        {
            var currency = transactions.As(currencyId);
            return currency.TryGetValue(transactionId, out transaction);
        }

        public static IEnumerable<Transaction> GetTransactions(string currencyId)
        {
            foreach (var (_, val) in transactions.As(currencyId))
            {
                yield return val;
            }
        }
        
        public StatView[] data;
        public string[] acceptableTransactions;
        
        private void Awake()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Init(acceptableTransactions);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].OnEnable();
            }
        }
        
        private void OnDisable()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].OnDisable();
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