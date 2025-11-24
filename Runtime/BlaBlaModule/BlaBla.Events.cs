using Sendbird.Chat;

public static partial class BlaBla
{
    public static class Events
    {
        public static SbConnectionHandler Connection { get; } = new();
        public static SbUserEventHandler User { get; } = new();
        public static SbOpenChannelHandler OpenChannel { get; } = new();
        public static SbGroupChannelHandler GroupChannel { get; } = new();

        internal static void Setup()
        {
            var id = "BlaBla";
            SendbirdChat.AddConnectionHandler(id, Connection);
            SendbirdChat.AddUserEventHandler(id, User);
            SendbirdChat.OpenChannel.AddOpenChannelHandler(id, OpenChannel);
            SendbirdChat.GroupChannel.AddGroupChannelHandler(id, GroupChannel);
        }

        internal static void TearDown()
        {
            var id = "BlaBla";
            SendbirdChat.RemoveConnectionHandler(id);
            SendbirdChat.RemoveUserEventHandler(id);
            SendbirdChat.OpenChannel.RemoveOpenChannelHandler(id);
            SendbirdChat.GroupChannel.RemoveGroupChannelHandler(id);
        }
    }
}