using LSCore.ConfigModule;
using Sirenix.Utilities;

[GlobalConfig("Assets/" + FolderNames.ProjectSettings)]
public class BackuperSettings : GlobalConfig<BackuperSettings>
{
    public int maxBackupsCount = 5;
    public int saveInterval = 1;
}