using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class RicochetGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "ricochet-icon";
#endif
    }
}