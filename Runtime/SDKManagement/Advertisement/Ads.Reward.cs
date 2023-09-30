using System;
using UnityEngine.Scripting;

namespace LSCore.SDKManagement.Advertisement
{
    public static partial class Ads
    {
        public static class Reward
        {
            public enum Result
            {
                FailedDisplay,
                Closed,
                Received
            }

            [Serializable]
            [Preserve]
            internal abstract class BaseAdapter : BaseIdProvider
            {
                protected override void SetAdapter()
                {
                    Adapter = this;
                }
                
                public abstract void Show(Action<Result> onResult);
            }

            private static Action<Action<Result>> show = NullWarning;
            private static BaseAdapter adapter;
            
            private static BaseAdapter Adapter
            {
                set
                {
                    adapter = value;

                    if (show != DisableWarning)
                    {
                        ConfigureDelegates(value);
                    }
                }
            }
            
            internal static bool Enabled
            {
                set
                {
                    if (value)
                    {
                        ConfigureDelegates(adapter);
                    }
                    else
                    {
                        show = DisableWarning;
                    }
                }
            }

            public static void Show(Action<Result> onResult) => show(onResult);
            
            private static void Internal_Show(Action<Result> onResult)
            {
                Burger.Log("[Ads] Reward Show");
                adapter.Show(onResult);
            }
            
            private static void ConfigureDelegates(BaseAdapter adapter)
            {
                show = adapter == null ? NullWarning : Internal_Show;
            }
            
            private static void NullWarning(Action<Result> onResult)
            {
                Burger.Warning("[Ads] Reward adapter is null");
                onResult(Result.Received);
            }

            private static void DisableWarning(Action<Result> onResult)
            {
                Burger.Warning("[Ads] Reward adapter is disable");
                onResult(Result.Received);
            }
        }
    }
}