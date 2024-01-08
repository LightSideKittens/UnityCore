using System.Collections.Generic;
using System.Linq;

namespace LSCore
{
    public class CurrencyIdAttributeDrawer : BaseValueDropdownDrawer<CurrencyIdAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<Id> source;
            
            if (!string.IsNullOrEmpty(Attribute.GroupName))
            {
                source = AssetDatabaseUtils.LoadAny<CurrencyIdGroup>(Attribute.GroupName);
            }
            else
            {
                source = AssetDatabaseUtils.LoadAllAssets<CurrencyIdGroup>().SelectMany(group => group);
            }

            return source;
        }
    }
}