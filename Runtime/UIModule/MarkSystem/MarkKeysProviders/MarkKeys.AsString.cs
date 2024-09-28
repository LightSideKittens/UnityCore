using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public abstract partial class MarkKeys
    {
        [Serializable]
        public class AsString : MarkKeys
        {
            [field: SerializeField] public override string Id { get; set; }
            public List<string> group;
            [field: SerializeField] public override string MarkTypeId { get; set; }

            public override IEnumerable<string> Group => group;
        }
    }
}