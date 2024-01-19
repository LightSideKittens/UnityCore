using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class MoveSpeedGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "speed-icon";
#endif
    }
}