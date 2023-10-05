#if DEBUG
using System.ComponentModel;
using LSCore;
using UnityEngine.Scripting;

public partial class SROptions
{
    [Category("LS Debug Data")]
    [Preserve]
    public string Country
    {
        get => LSDebugData.Country;
        set => LSDebugData.Country = value;
    }
    
    [Category("LS Debug Data")]
    [Preserve]
    public string Environment
    {
        get => LSDebugData.Environment;
        set => LSDebugData.Environment = value;
    }
}

#endif