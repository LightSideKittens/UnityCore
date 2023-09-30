using System.Diagnostics;

namespace LSCore.ConfigModule.Editor
{
    public static partial class ConfigUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void SetAsDefault<T>() where T : BaseResourcesConfig<T>, new()
        {
            BaseResourcesConfig<T>.Editor_Save();
        }
    }
}