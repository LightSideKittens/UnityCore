using System;

namespace Telegram.Bot.Serialization
{
    /// <summary>Same as <see cref="JsonDerivedTypeAttribute"/> but used for the hack below.
    /// Necessary because using the built-in attribute will lead to NotSupportedExceptions.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    internal class CustomJsonDerivedTypeAttribute : Attribute
    {
        /// <summary>Same as <see cref="JsonDerivedTypeAttribute"/> but used for the hack below.
        /// Necessary because using the built-in attribute will lead to NotSupportedExceptions.</summary>
        public CustomJsonDerivedTypeAttribute(Type subtype, string? discriminator = default)
        {
            Subtype = subtype;
            Discriminator = discriminator;
        }

        public Type Subtype { get; }
        public string? Discriminator { get; set; }
    }
}
