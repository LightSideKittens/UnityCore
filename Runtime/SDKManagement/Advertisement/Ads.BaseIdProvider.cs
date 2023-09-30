using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.SDKManagement.Advertisement
{
    public static partial class Ads
    {
        [Serializable]
        internal abstract class BaseIdProvider
        {
            /*protected string Id
            {
                get
                {
#if DEBUG && !RELEASE_ADS
                    return DebugId;
#else
                    return id;
#endif
                }
            }*/

            [ShowIf("HasId")]
            [SerializeField] private string id;
            //protected virtual string DebugId { get; }
            protected virtual bool HasId => true;

            public void Init()
            {
                //ErrorIfIdIsEmpty(this);
                Internal_Init();
                SetAdapter();
            }

            protected abstract void Internal_Init();
            protected abstract void SetAdapter();
            
            /*
            [Conditional("DEBUG")]
            private static void ErrorIfIdIsEmpty(BaseIdProvider provider)
            {
                if (provider.HasId && string.IsNullOrEmpty(provider.id))
                {
                    Burger.Error($"{provider.GetType().Name} id is Empty");
                    //provider.id = provider.DebugId;
                }
            }*/

        }
    }
}