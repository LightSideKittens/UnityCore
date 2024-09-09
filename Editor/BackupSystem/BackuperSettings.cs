using LSCore.ConfigModule;
using LSCore.ConfigModule.Old;
using Sirenix.Utilities;

public class BackuperSettings : BaseStaticConfig<BackuperSettings>
{
    public int maxBackupsCount = 5;
    public int saveInterval = 1;
    protected override string RootPath => ApplicationUtils.LibraryPath;
}