using System.Collections.Generic;
using System.Linq;

namespace LSCore.Attributes.TypeFrom
{
    public class TypeFromDrawer : BaseTypeFilterDrawer<TypeFromAttribute>
    {
        protected override IEnumerable<object> GetRawGetter()
        {
            return allTypes.Where(t => Attribute.from.IsAssignableFrom(t)).Concat(Attribute.additional);
        }
    }
}