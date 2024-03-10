using System.IO;
using LSCore.ConfigModule;
using static LSCore.ConfigModule.FolderNames;

namespace LSCore
{
    public class BaseDebugData<T> : BaseSingleConfig<T> where T : BaseDebugData<T>, new()
    {
        protected sealed override string RootPath => Path.Combine(ConfigsPath, DebugData);
    }
}