using System;

using OneOf;

namespace BreadTh.StronglyApied.Attributes.Extending.Core
{
    public abstract class StronglyApiedBaseAttribute : Attribute
    {
        public readonly bool optional;
        public readonly string name;

        protected StronglyApiedBaseAttribute(string name, bool optional)
        {
            this.name = name;
            this.optional = optional;
        }
    }
}
