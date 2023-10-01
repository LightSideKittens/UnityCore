using LSCore;
using UnityEngine.Scripting;

public static class AdrVar
{
    [Preserve] public static string BuildEnvironment => LSDebugData.Environment;
    
    [Preserve]
    public static string ServerName => "ball-dance-cdn"
#if DEBUG
    + $"/{LSDebugData.Environment}"
#else
    + $"/{Consts.Env.Prod}"
#endif
    ;
}