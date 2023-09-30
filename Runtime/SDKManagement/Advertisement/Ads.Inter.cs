using System;
using UnityEngine.Scripting;

namespace LSCore.SDKManagement.Advertisement
{
    public static partial class Ads
    {
        public static class Inter
        {
            [Serializable]
            [Preserve]
            internal abstract class BaseAdapter : BaseIdProvider
            {
                protected override void SetAdapter()
                {
                    Adapter = this;
                }
                
                public abstract void Show();
            }

            private static Action show = NullWarning;
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

            public static void Show() => show();

            private static void Internal_Show()
            {
                Burger.Log("[Ads] Inter Show");
                adapter.Show();
            }
            
            private static void ConfigureDelegates(BaseAdapter adapter)
            {
                show = adapter == null ? NullWarning : Internal_Show;
            }
            
            private static void NullWarning() => Burger.Warning("[Ads] Inter adapter is null");
            private static void DisableWarning() => Burger.Warning("[Ads] Inter adapter is disable");
        }
    }
}