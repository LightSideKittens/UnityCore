namespace LSCore
{
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        public new static DaddyCanvas Instance => IsExistsInManager ? BaseWindow<DaddyCanvas>.Instance : null;
        protected override bool ShowByDefault => true;
    }
}