using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class RicochetGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "ricochet-icon";
#endif
    }
}