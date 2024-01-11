using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace LSCore
{
    public abstract class ValuesById<TValue> : ValuesByKeys<Id, TValue>
    {
        public Dictionary<Id, TValue> ById => ByKey;
        
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
            if (group != null) SetupByIds(group, list);
        }

        protected IEnumerable<Id> AllIdsByGroupType<T>() where T : IdGroup
        {
            return AssetDatabaseUtils.LoadAllAssets<T>().SelectMany(x => x);
        }
#endif

    }
}
