using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class RadiusGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "radius-icon";
#endif
    }
}