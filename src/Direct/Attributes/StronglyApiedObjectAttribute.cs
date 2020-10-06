using System;

namespace BreadTh.StronglyApied.Direct.Attributes
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
