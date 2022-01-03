using System;
using System.Collections.Generic;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StronglyApiedBoolAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        public StronglyApiedBoolAttribute(string name = null, bool optional = false) : base(name, optional) { }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(type != typeof(bool) && type != typeof(bool?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedBoolAttribute).FullName} "
                +   $"must be a {typeof(bool).FullName}, "
                +   $"but the given type was {type.FullName}");
            
            string lowerTrimmedValue = value.Trim().ToLower();

            if(!new List<string>(){ "true", "false", "0", "1" }.Contains(lowerTrimmedValue))
                return (ErrorDescription.InvalidBoolean(value, path), default);
            else
                return ParseSuccess.From(lowerTrimmedValue == "true" || lowerTrimmedValue == "1");
        }
    }
}
