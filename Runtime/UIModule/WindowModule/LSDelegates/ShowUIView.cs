namespace LSCore
{
    public class ShowUIView : DoIt
    {
        public UIView view;
        public ShowWindowOption option;
        public override void Invoke()
        {
            view.Show(option);
        }
    }
}