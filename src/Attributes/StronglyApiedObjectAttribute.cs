using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedObjectAttribute : Attribute
    {
        public bool optional;
        public StronglyApiedObjectAttribute(bool optional = false)
        {
            this.optional = optional;
        }
    }
}
