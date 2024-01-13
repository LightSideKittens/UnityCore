using UnityEngine;

namespace LSCore
{
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        protected override bool ActiveByDefault => true;
        protected override float DefaultAlpha => 1;
        protected override Transform Parent => null;

        protected override void RecordState(){}
    }
}