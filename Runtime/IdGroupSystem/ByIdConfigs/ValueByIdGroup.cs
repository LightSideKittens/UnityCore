using Sirenix.OdinInspector;

namespace LSCore
{
    public abstract class ValueByIdGroup<TIdGroup, TValue> : ValueByKey<TIdGroup, TValue> where TIdGroup : IdGroup
    {
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list)
        {
            foreach (var group in AssetDatabaseUtils.LoadAllAssets<TIdGroup>())
            {
                list.Add(group.name, new Entry(){key = group});
            }
        }
#endif
    }
}
