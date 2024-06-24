namespace LSCore
{
    public class ShowUIView : LSAction
    {
        public UIView view;
        public ShowWindowOption option;
        public override void Invoke()
        {
            view.Show(option);
        }
    }
}