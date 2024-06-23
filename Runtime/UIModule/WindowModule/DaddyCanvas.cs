using UnityEngine;

namespace LSCore
{
    public class DaddyWindowManager : WindowManager
    {
        protected override void RecordState() { }
    }
    
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        protected override bool ActiveByDefault => true;
        protected override float DefaultAlpha => 1;
        protected override RectTransform Daddy => null;

        protected override WindowManager Manager { get; } = new DaddyWindowManager();
    }
}