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
}

#endif