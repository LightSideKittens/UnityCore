namespace LSCore.Firebase
{
    public partial class RemoteConfig
    {
        public static class Bool
        {
            public static bool Get(string name, bool defaultValue = false) => remoteConfig.GetValue(name).BooleanValue;
        }
    }
}