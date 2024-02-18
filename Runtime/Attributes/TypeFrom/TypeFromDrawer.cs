using System.Collections.Generic;
using System.Linq;

namespace LSCore.Attributes
{
    public class TypeFromDrawer : BaseTypeFilterDrawer<TypeFromAttribute>
    {
        protected override IEnumerable<object> GetRawGetter()
        {
            var type = Attribute.from;
            if (type == null)
            {
                type = Property.ValueEntry.BaseValueType;
            }
            return allTypes.Where(t => type.IsAssignableFrom(t)).Concat(Attribute.additional);
        }
    }
}