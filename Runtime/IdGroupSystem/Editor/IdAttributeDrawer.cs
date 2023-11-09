using System.Collections.Generic;

namespace LSCore
{
    public class IdAttributeDrawer : BaseValueDropdownDrawer<IdAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<Id> source;
            
            if (!string.IsNullOrEmpty(Attribute.GroupName))
            {
                source = AssetDatabaseUtils.LoadAny<IdGroup>(Attribute.GroupName);
            }
            else
            {
                source = AssetDatabaseUtils.LoadAllAssets<Id>();
            }

            return source;
        }
    }
}