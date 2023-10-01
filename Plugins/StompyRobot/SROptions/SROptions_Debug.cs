#if DEBUG
using System.ComponentModel;
using LSCore;
using UnityEngine.Scripting;

public partial class SROptions
{
    [Category("Debug Data")]
    [Preserve]
    public string Country
    {
        get => DebugData.Country;
        set => DebugData.Country = value;
    }
    
    [Category("Debug Data")]
    [Preserve]
    public string Environment
    {
        get => DebugData.Environment;
        set => DebugData.Environment = value;
    }
}

#endif