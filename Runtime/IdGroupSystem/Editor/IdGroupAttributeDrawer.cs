using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Drawers;

namespace Battle.Editor
{
    public class IdGroupAttributeDrawer : BaseValueDropdownDrawer<IdGroupAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<IdGroup> groups = AssetDatabaseUtils.LoadAllAssets<IdGroup>();
            return groups;
        }
    }
}