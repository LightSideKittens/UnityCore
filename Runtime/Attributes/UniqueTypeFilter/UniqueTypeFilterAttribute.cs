using System;

namespace LSCore.Attributes
{
    public class BaseTypeFilterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;
    }
    
    public class UniqueTypeFilterAttribute : BaseTypeFilterAttribute
    {
        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;
        public Type[] ExludeTypes;

        public UniqueTypeFilterAttribute(params Type[] excludeTypes)
        {
            ExludeTypes = excludeTypes;
        }
    }
}