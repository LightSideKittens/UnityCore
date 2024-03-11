using System;

namespace LSCore.Attributes
{
    public class ExcludeTypeAttribute : BaseTypeFilterAttribute
    {
        public Type[] excludedTypes;

        public ExcludeTypeAttribute(params Type[] excludedTypes)
        {
            this.excludedTypes = excludedTypes;
        }
    }
}