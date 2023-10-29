using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class IdGroupAttributeDrawer : BaseValueDropdownDrawer<IdGroupAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<Object> groups = AssetDatabaseUtils.LoadAllAssets(Property.ValueEntry.TypeOfValue);
            return groups;
        }
    }
}