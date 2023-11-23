using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace LSCore
{
    public abstract class ValuesById<TValue> : ValuesByKeys<Id, TValue>
    {
        private Dictionary<string, TValue> byId;

        public Dictionary<string, TValue> ById
        {
            get
            {
                if (byId == null)
                {
                    byId = new();
                    foreach (var (key, value) in ByKey)
                    {
                        byId.Add(key, value);
                    }
                }

                return byId;
            }
        }
        
#if UNITY_EDITOR
        protected void SetupByIds(IEnumerable<Id> ids, ValueDropdownList<Data> list)
        {
            foreach (var id in ids)
            {
                list.Add(id, new Data(){key = id});
            }
        }
        
        protected void SetupByGroup(IdGroup group, ValueDropdownList<Data> list)
        {
            if (group != null) SetupByIds(group.Ids, list);
        }
#endif

    }
}
