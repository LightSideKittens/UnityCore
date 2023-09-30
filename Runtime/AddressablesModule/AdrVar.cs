using LSCore;
using UnityEngine.Scripting;

public static class AdrVar
{
    [Preserve] public static string BuildEnvironment => DebugData.Environment;
    
    [Preserve]
    public static string ServerName => "ball-dance-cdn"
#if DEBUG
    + $"/{DebugData.Environment}"
#else
    + $"/{Consts.Env.Prod}"
#endif
    ;
}