namespace LSCore
{
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        public new static DaddyCanvas Instance => IsNull ? null : BaseWindow<DaddyCanvas>.Instance;
        protected override bool ShowByDefault => true;
    }
}