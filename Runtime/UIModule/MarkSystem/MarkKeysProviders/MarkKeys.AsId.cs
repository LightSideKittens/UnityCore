using System;
using System.Collections.Generic;

namespace LSCore
{
    public abstract partial class MarkKeys
    {
        [Serializable]
        public class AsId : MarkKeys
        {
            [Id(typeof(MarkIdGroup), "AllMarks")] public Id id;
            [IdGroup] public IdGroup group;
            [Id(typeof(MarkIdGroup), "AllMarkTypes")] public Id markTypeId;

            public override string Id
            {
                get => id;
                set { }
            }

            public override IEnumerable<string> Group
            {
                get
                {
                    if (group == null)
                    {
                        yield break;
                    }
                    
                    foreach (var id in group)
                    {
                        yield return id;
                    }
                }
            }

            public override string MarkTypeId
            {
                get => markTypeId;
                set { }
            }
        }
    }
}