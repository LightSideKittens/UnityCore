using LSCore.ConfigModule;

public class BackuperSettings : LocalDynamicConfig
{
    public int maxBackupsCount = 5;
    public int saveInterval = 1;

    public static BackuperSettings Config => Manager.Config;

    public static LocalDynamicConfigManager<BackuperSettings> Manager =>
        ConfigMaster<LocalDynamicConfigManager<BackuperSettings>>.Get(
            $"{LSPaths.LibraryPath}/{nameof(BackuperSettings)}");
}