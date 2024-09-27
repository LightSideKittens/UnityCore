namespace LSCore
{
    public class GoMark : Mark<bool>
    {
        protected override void HandleView()
        {
            gameObject.SetActive(TryGet(out _));
        }
    }
}