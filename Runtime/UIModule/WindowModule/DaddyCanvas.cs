using UnityEngine;

namespace LSCore
{
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        protected override bool ShowByDefault => true;
        protected override Transform Parent => null;

        protected override void OnShowing() => ExcludeFromHidePrevious();
    }
}