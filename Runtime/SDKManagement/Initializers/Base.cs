using System;

namespace LSCore.SDKManagement
{
    public partial class SDKInitializer
    {
        [Serializable]
        public abstract class Base
        {
            public bool ignore;
            
            protected abstract void Internal_Init(Action<string> onComplete);

            public void TryInit(Action onComplete)
            {
                if (ignore)
                {
                    onComplete();
                    return;
                }
                
                Burger.Log($"[{GetType().Name}] Initializing");
                Internal_Init(OnComplete);
                
                void OnComplete(string message)
                {
                    Burger.Log($"[{GetType().Name}] Initialized. {message}");
                    onComplete();
                }
            }
        }
    }
}