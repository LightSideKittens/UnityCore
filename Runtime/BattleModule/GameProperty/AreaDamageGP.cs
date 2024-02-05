using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class AreaDamageGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "area-damage-icon";
#endif
    }
}