using System.Collections.Generic;
using System.Linq;

namespace LSCore.Attributes
{
    public class ExcludeTypeDrawer : BaseTypeFilterDrawer<ExcludeTypeAttribute>
    {
        protected override IEnumerable<object> GetRawGetter()
        {
            return allTypes.Except(Attribute.excludedTypes);
        }
    }
}