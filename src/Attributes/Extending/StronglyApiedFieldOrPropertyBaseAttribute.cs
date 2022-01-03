using System;
using BreadTh.StronglyApied.Attributes.Extending.Core;
using OneOf;

namespace BreadTh.StronglyApied.Attributes.Extending
{
    public abstract class StronglyApiedFieldOrPropertyBaseAttribute : StronglyApiedBaseAttribute
    {
        protected StronglyApiedFieldOrPropertyBaseAttribute(string name, bool optional)
            : base(name, optional)
        {

        }

        public abstract OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path);

    }
}
