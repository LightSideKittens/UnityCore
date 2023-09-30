using System.Diagnostics;

namespace LSCore.ConfigModule.Editor
{
    public static partial class ConfigUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void Save<T>() where T : BaseConfig<T>, new()
        {
            BaseConfig<T>.Save();
        }
    }
}