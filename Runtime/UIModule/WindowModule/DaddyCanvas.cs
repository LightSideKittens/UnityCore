using UnityEngine;

namespace LSCore
{
    public class NotRecordableWindowManager : WindowManager
    {
        protected override void RecordState() { }
    }
    
    [DefaultExecutionOrder(-11)]
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        protected override bool ActiveByDefault => true;
        protected override RectTransform Daddy => null;
        public override WindowManager Manager { get; } = new NotRecordableWindowManager();
    }
}