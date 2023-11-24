using LSCore.ConfigModule;

namespace LSCore
{
    public class BaseDebugData<T> : BaseConfig<T> where T : BaseDebugData<T>, new()
    {
        internal override string GeneralFolderName => FolderNames.Debug;
    }
}