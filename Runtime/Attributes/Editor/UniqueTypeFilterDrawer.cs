using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;

namespace LSCore.Attributes
{
    /// <summary>
    /// Draws properties marked with <see cref="T:LSCore.Attributes.UniqueTypeFilterAttribute" />.
    /// </summary>
    [DrawerPriority(0.0, 0.0, 2002.0)]
    public sealed class UniqueTypeFilterAttributeDrawer : BaseTypeFilterDrawer<UniqueTypeFilterAttribute>
    {
        protected override IEnumerable<object> GetRawGetter()
        {
            var prop = Property;
            if (!useSpecialListBehaviour)
            {
                prop = prop.Parent;
            }
            
            var set = new HashSet<Type>(Attribute.ExcludeTypes);
            var children = prop.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var val = children[i].ValueEntry.WeakSmartValue;
                if (val != null)
                {
                    set.Add(val.GetType());
                }
            }
            
            return allTypes.Where(t => !set.Contains(t));
        }
    }
}