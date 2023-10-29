using Sirenix.OdinInspector;

namespace LSCore
{
    public abstract class ValuesByIdGroup<TIdGroup, TValue> : ValuesByKeys<TIdGroup, TValue> where TIdGroup : IdGroup
    {
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Data> list)
        {
            foreach (var group in AssetDatabaseUtils.LoadAllAssets<TIdGroup>())
            {
                list.Add(group.name, new Data(){key = group});
            }
        }
#endif
    }
}
