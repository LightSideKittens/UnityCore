using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class FundsByLevel : DataByInterval<Funds>
    {
#if UNITY_EDITOR
        protected override string Label => "Funds";
#endif
    }
}