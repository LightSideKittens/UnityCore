using System;
using LSCore.Async;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class WhenAll : AsyncOperationBase<WhenAll>
    {
        private int dependenciesCount;
        private int resolvedDependenciesCount;

        public AsyncOperationHandle<WhenAll> Handle { get; private set; }
        private WhenAll(){}

        public static WhenAll Wait<T>(AsyncOperationHandle<T> task) where T : Object
        {
            var whenAll = new WhenAll().And(task);
            whenAll.Handle = Addressables.ResourceManager.StartOperation(whenAll, default);
            return whenAll;
        }

        public WhenAll And<T>(AsyncOperationHandle<T> task) where T : Object
        {
            dependenciesCount++;
            task.OnComplete(OnResolvedT);
            return this;
            void OnResolvedT(T asset) => resolvedDependenciesCount++;
        }
        
        public WhenAll And(AsyncOperationHandle task)
        {
            dependenciesCount++;
            task.OnComplete(OnResolved);
            return this;
        }
        
        public WhenAll And(Wait.WhenAllActions task)
        {
            dependenciesCount++;
            task.OnComplete(OnResolved);
            return this;
        }

        private void OnResolved() => resolvedDependenciesCount++;

        public WhenAll OnComplete(Action onComplete)
        {
            Handle.OnComplete(onComplete);
            return this;
        }
        
        protected override void Execute()
        {
            World.Updated += Update;
        }

        private void Update()
        {
            if (dependenciesCount == resolvedDependenciesCount)
            {
                World.Updated -= Update;
                Complete(this, true, string.Empty);
            }
        }
    }
}