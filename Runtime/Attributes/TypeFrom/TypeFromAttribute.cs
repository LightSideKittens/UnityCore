using System;

namespace LSCore.Attributes
{
    public class TypeFromAttribute : BaseTypeFilterAttribute
    {
        public Type from;
        public Type[] additional;

        public TypeFromAttribute(params Type[] additional)
        {
            this.additional = additional;
        }
        
        public TypeFromAttribute(Type from, params Type[] additional)
        {
            this.from = from;
            this.additional = additional;
        }
    }
}