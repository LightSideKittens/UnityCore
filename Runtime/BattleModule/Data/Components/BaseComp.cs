using System;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseComp
    {
        public abstract void Init(CompData data);
    }
}