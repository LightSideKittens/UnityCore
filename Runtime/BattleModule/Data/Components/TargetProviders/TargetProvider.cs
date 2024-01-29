using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class TargetProvider
    {
        public abstract IEnumerable<Transform> Targets { get; }
    }
}