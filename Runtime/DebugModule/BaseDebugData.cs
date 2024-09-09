using System.IO;
using LSCore.ConfigModule.Old;
using static LSCore.ConfigModule.FolderNames;

namespace LSCore
{
    public class BaseDebugData<T> : BaseSingleConfig<T> where T : BaseDebugData<T>, new()
    {
        protected sealed override string RootPath => Path.Combine(ConfigsPath, DebugData);
    }
}