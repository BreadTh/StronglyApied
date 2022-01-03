using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] 
    public sealed class StronglyApiedArrayAttribute : Attribute
    {
        public readonly int minLength;
        public readonly int maxLength;
        public readonly bool optional;
        public StronglyApiedArrayAttribute(int minLength = 0, int maxLength = int.MaxValue, bool optional = false)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
            this.optional = optional;
        }
    }
}
