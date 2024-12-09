using Sirenix.OdinInspector;

namespace LSCore.Attributes
{
    public class LSValueDropdownAttribute : ValueDropdownAttribute
    {
        public LSValueDropdownAttribute(string valuesGetter) : base(valuesGetter)
        {
        }
    }
}