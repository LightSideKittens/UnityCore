using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace LSCore.Attributes
{
    public class IValueDropdownEqualityComparer : IEqualityComparer<object>
    {
        private bool isTypeLookup;

        public IValueDropdownEqualityComparer(bool isTypeLookup) => this.isTypeLookup = isTypeLookup;

        public bool Equals(object x, object y)
        {
            if (x is ValueDropdownItem)
                x = ((ValueDropdownItem) x).Value;
            if (y is ValueDropdownItem)
                y = ((ValueDropdownItem) y).Value;
            if (EqualityComparer<object>.Default.Equals(x, y))
                return true;
            if (x == null != (y == null) || !this.isTypeLookup)
                return false;
            Type type1 = x as Type;
            if ((object) type1 == null)
                type1 = x.GetType();
            Type type2 = type1;
            Type type3 = y as Type;
            if ((object) type3 == null)
                type3 = y.GetType();
            Type type4 = type3;
            return type2 == type4;
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return -1;
            if (obj is ValueDropdownItem)
                obj = ((ValueDropdownItem) obj).Value;
            if (obj == null)
                return -1;
            if (!this.isTypeLookup)
                return obj.GetHashCode();
            Type type = obj as Type;
            if ((object) type == null)
                type = obj.GetType();
            return type.GetHashCode();
        }
    }
}