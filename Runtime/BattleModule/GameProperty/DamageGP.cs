using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class DamageGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "attack-icon";
#endif
    }
}