using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedArrayAttribute : Attribute
    {
        public int minLength;
        public int maxLength;
        public bool optional;
        public StronglyApiedArrayAttribute(int minLength = 0, int maxLength = int.MaxValue, bool optional = false)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
            this.optional = optional;
        }
    }
}
