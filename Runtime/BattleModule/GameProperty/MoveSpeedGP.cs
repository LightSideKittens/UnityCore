using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class MoveSpeedGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "speed-icon";
#endif
    }
}