namespace LSCore
{
    public partial class PlayerWebSocketClient : BaseWebSocketClient
    {
        protected override bool IsEditor => false;
    }
}