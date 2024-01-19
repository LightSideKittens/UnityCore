using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Data.Components.TargetProviders
{
    [Serializable]
    public abstract class TargetProvider
    {
        [NonSerialized] public FindTargetComponent findTargetComponent;
        public abstract IEnumerable<Transform> Targets { get; }
    }
}