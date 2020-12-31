using System;
using System.Collections.Generic;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedBoolAttribute : StronglyApiedFieldBase
    {
        public StronglyApiedBoolAttribute(bool optional = false) : base(optional) { }

        public override TryParseResult TryParse(Type type, string value, string path)
        {
            if(type != typeof(bool) && type != typeof(bool?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedBoolAttribute).FullName} "
                +   $"must be a {typeof(bool).FullName}, "
                +   $"but the given type was {type.FullName}");
            
            string lowerTrimmedValue = value.Trim().ToLower();

            if(!new List<string>(){ "true", "false", "0", "1" }.Contains(lowerTrimmedValue))
                return TryParseResult.Invalid(ErrorDescription.InvalidBoolean(value, path));
            else
                return TryParseResult.Ok(lowerTrimmedValue == "true" || lowerTrimmedValue == "1");
        }
    }
}
