namespace LSCore
{
    public class ShowUIView : DoIt
    {
        public UIView view;
        public ShowWindowOption option;
        public override void Do()
        {
            view.Show(option);
        }
    }
}