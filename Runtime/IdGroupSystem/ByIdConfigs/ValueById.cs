using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace LSCore
{
    public abstract class ValueById<TValue> : ValueByKey<Id, TValue>
    {
#if UNITY_EDITOR
        protected void SetupByIds(IEnumerable<Id> ids, ValueDropdownList<Entry> list)
        {
            foreach (var id in ids)
            {
                list.Add(id, new Entry(){key = id});
            }
        }
        
        protected void SetupByGroup(IdGroup group, ValueDropdownList<Entry> list)
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
