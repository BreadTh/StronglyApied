using System;

using OneOf;

namespace BreadTh.StronglyApied.Attributes.Extending
{
    public abstract class StronglyApiedFieldBaseAttribute : Attribute
    {
        public readonly bool optional;

        protected StronglyApiedFieldBaseAttribute(bool optional)
        {
            this.optional = optional;
        }

        public abstract OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path);

    }
}
