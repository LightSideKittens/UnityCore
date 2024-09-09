using System.IO;
using static LSCore.ConfigModule.FolderNames;

namespace LSCore.ConfigModule.Old
{
    public abstract class BaseStaticConfig<T> : BaseSingleConfig<T> where T : BaseStaticConfig<T>, new()
    {
        static BaseStaticConfig()
        {
            onStaticConstructor.Push(() =>
            {
                isAutoSave = false;
            }); 
        }
        
        protected override string RootPath => Path.Combine(ConfigsPath, StaticData);
    }
}