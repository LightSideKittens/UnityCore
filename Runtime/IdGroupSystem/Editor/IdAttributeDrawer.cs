using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Drawers;

namespace Battle.Editor
{
    public class IdAttributeDrawer : BaseValueDropdownDrawer<IdAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<Id> source;
            
            if (!string.IsNullOrEmpty(Attribute.GroupName))
            {
                source = AssetDatabaseUtils.LoadAny<IdGroup>(Attribute.GroupName).Ids;
            }
            else
            {
                source = AssetDatabaseUtils.LoadAllAssets<Id>();
            }

            return source;
        }
    }
}