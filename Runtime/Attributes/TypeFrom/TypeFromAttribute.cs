using System;

namespace LSCore.Attributes.TypeFrom
{
    public class TypeFromAttribute : BaseTypeFilterAttribute
    {
        public Type from;
        public Type[] additional;

        public TypeFromAttribute(Type from, params Type[] additional)
        {
            this.from = from;
            this.additional = additional;
        }
    }
}