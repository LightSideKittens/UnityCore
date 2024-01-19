using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class AttackSpeedGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override Prop DrawFields()
        {
            prop.DrawSlider(ValueKey, 0.1f, 2f);
            return prop;
        }
        protected override string IconName => "attack-speed-icon";
#endif
    }
}