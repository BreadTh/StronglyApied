using System;

namespace BreadTh.StronglyApied.Attributes.Extending.Core
{
    public class StronglyApiedBaseAttribute : Attribute
    {
        public readonly bool optional;
        public readonly string name;

        public StronglyApiedBaseAttribute(string name = null, bool optional = false)
        {
            this.name = name;
            this.optional = optional;
        }
    }
}
