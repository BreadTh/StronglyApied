using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedObjectAttribute : Attribute
    {
        public readonly bool optional;
        public readonly bool stringified;
        public StronglyApiedObjectAttribute(bool optional = false, bool stringified = false)
        {
            this.optional = optional;
            this.stringified = stringified;
        }
    }
}
