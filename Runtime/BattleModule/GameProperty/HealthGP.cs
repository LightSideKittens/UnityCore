using System;
using LSCore.LevelSystem;

namespace LSCore.GameProperty
{
    [Serializable]
    public class HealthGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "health-icon";
#endif
    }
}