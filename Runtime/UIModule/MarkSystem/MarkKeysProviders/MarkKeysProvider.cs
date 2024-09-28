using System;
using System.Collections.Generic;

namespace LSCore
{
    [Serializable]
    public abstract partial class MarkKeys
    {
        public abstract string Id { get; set; }
        public abstract IEnumerable<string> Group { get; }
        public abstract string MarkTypeId { get; set; }
    }
}