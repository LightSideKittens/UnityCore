using System.Diagnostics;

namespace LSCore.ConfigModule
{
    public class BaseResourcesConfig<T> : BaseConfig where T : BaseResourcesConfig<T>, new()
    {
        public static T Config => ByName<T>.Get(string.Empty, false);
        protected override string FileName => typeof(T).Name;
        protected override string GeneralFolderName => FolderNames.DefaultSaveData;

        [Conditional("UNITY_EDITOR")]
        internal static void Editor_Save() => Config.Save();
    }
}