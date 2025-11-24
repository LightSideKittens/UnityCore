using LSCore;
using Sendbird.Chat;
using SourceGenerators;

[InstanceProxy]
public partial class BlaBlaSettings : SingleScriptableObject<BlaBlaSettings>
{
    public string _appId;
    public SbLogLevel _logLevel;
    public string _apiToken;
}