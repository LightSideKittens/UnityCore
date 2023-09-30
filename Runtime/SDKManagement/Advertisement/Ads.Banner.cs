using System;

namespace LSCore.SDKManagement.Advertisement
{
    public static partial class Ads
    {
        public static class Banner
        {
            [Serializable]
            internal abstract class BaseAdapter : BaseIdProvider
            {
                protected override void SetAdapter()
                {
                    Adapter = this;
                }

                public abstract void Show();
                public abstract void Hide();
            }

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
                        hide = DisableWarning;
                    }
                }
            }

            private static Action show = NullWarning;
            private static Action hide = NullWarning;
            
            public static void Show() => show();
            public static void Hide() => hide();
            
            private static void Internal_Show()
            {
                Burger.Log("[Ads] Banner Show");
                adapter.Show();
            }

            private static void Internal_Hide()
            {
                Burger.Log("[Ads] Banner Hide");
                adapter.Hide();
            }

            private static void ConfigureDelegates(BaseAdapter adapter)
            {
                if (adapter == null)
                {
                    show = NullWarning;
                    hide = NullWarning;
                }
                else
                {
                    show = Internal_Show;
                    hide = Internal_Hide;
                }
            }
            
            private static void NullWarning() => Burger.Warning("[Ads] Banner adapter is null");
            private static void DisableWarning() => Burger.Warning("[Ads] Banner adapter is disable");
        }
    }
}