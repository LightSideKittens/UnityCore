using System;
using System.Collections.Generic;

namespace LSCore
{
    public abstract class BaseTransactions<T> : List<BaseTransactions<T>.Check> where T : Delegate
    {
        private T AddDelegate(T old, T newD) => (T)Delegate.Combine(old, newD);

        private T RemoveDelegate(T old, T newD) => (T)Delegate.Remove(old, newD);
        protected abstract T Default { get; }
        protected virtual bool CanByDefault { get; }

        public BaseTransactions(bool canByDefault = false)
        {
            CanByDefault = canByDefault;
        }

        public Check Union()
        {
            var allCanTransaction = Count > 0 || CanByDefault;
            var confirm = Default;

            for (int i = 0; i < Count; i++)
            {
                var transaction = this[i];
                if (transaction(out var newConfirm))
                {
                    confirm = AddDelegate(confirm, newConfirm);
                    continue;
                }

                confirm = Default;
                allCanTransaction = false;
                break;
            }

            return Result;

            bool Result(out T action)
            {
                action = confirm;
                return allCanTransaction;
            }
        }
        
        public delegate bool Check(out T action);
    }

    public class Transactions : BaseTransactions<Action>
    {
        protected override Action Default { get; } = delegate { };
        
        public Transactions(){}
        public Transactions(bool canByDefault) : base(canByDefault){}
        public Transactions(Action defaultAction, bool canByDefault = false) : base(canByDefault)
        {
            Default = defaultAction;
        }
    }
}