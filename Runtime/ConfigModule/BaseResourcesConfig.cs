using System.Diagnostics;

namespace LSCore.ConfigModule
{
    public class BaseResourcesConfig<T> : BaseConfig<T> where T : BaseResourcesConfig<T>, new()
    {
        protected override string GeneralFolderName => FolderNames.DefaultSaveData;

        [Conditional("UNITY_EDITOR")]
        internal static void Editor_Save() => Save();
    }
}