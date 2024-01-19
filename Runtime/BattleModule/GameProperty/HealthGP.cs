using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class HealthGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "health-icon";
#endif
    }
}