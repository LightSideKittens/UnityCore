namespace LSCore.Firebase
{
    public partial class RemoteConfig
    {
        public static class String
        {
            public static string Get(string name, string defaultValue = "") => remoteConfig.GetValue(name).StringValue;
        }
    }
}