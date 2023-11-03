using System;
using System.Collections.Generic;

namespace LSCore
{
    public class BaseTransactions<T> : List<BaseTransactions<T>.Check> where T : Delegate
    {
        private T AddDelegate(T old, T newD) => (T)Delegate.Combine(old, newD);

        private T RemoveDelegate(T old, T newD) => (T)Delegate.Remove(old, newD);

        public Check Union()
        {
            var allCanTransaction = true;
            T confirm = default;

            for (int i = 0; i < Count; i++)
            {
                var transaction = this[i];
                if (transaction(out var newConfirm))
                {
                    confirm = AddDelegate(confirm, newConfirm);
                    continue;
                }

                confirm = default;
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

    }
}