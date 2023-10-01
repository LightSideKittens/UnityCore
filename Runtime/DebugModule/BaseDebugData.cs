using LSCore.ConfigModule;

namespace LSCore
{
    public class BaseDebugData<T> : BaseConfig<T> where T : BaseDebugData<T>, new()
    {
        protected override string FolderName => "Debug";
    }
}