using System.Diagnostics;
using Sirenix.OdinInspector;

namespace GameCore.Attributes
{
    [Conditional("UNITY_EDITOR")]
    public partial class ColoredFieldAttribute : PropertyGroupAttribute
    {
        partial void Constructor();

        public ColoredFieldAttribute() : base("d")
        {
            Constructor();
        }
    }
}