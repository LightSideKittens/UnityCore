namespace LSCore.Firebase
{
    public partial class RemoteConfig
    {
        public static class Long
        {
            public static long Get(string name, long defaultValue = 0) => remoteConfig.GetValue(name).LongValue;
        }
    }
}