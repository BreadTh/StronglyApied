using System;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedBoolAttribute : StronglyApiedFieldBase
    {
        public StronglyApiedBoolAttribute(bool optional = false) : base(optional) { }

        public override TryParseResult TryParse(Type type, JToken token, string path)
        {
            if(type != typeof(bool) && type != typeof(bool?))
                throw new InvalidOperationException($"Fields tagged with JsonInputBoolAttribute must be bool-type, but the given type was {type.FullName}");
            
            string value = ((JValue)token).ToString(CultureInfo.InvariantCulture);
            string lowerTrimmedValue = value.Trim().ToLower();

            if(!new List<string>(){ "true", "false", "0", "1" }.Contains(lowerTrimmedValue))
                return TryParseResult.Invalid(ValidationError.InvalidBoolean(value, path));
            else
                return TryParseResult.Ok(lowerTrimmedValue == "true" || lowerTrimmedValue == "1");
        }
    }
}
