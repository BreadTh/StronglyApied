﻿using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedLongAttribute : StronglyApiedFieldBase
    {
        public long minValue;
        public long maxValue;

        public StronglyApiedLongAttribute(long minValue = long.MinValue, long maxValue = long.MaxValue, bool optional = false) : base(optional)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override TryParseResult TryParse(Type type, JToken token, string path)
        {
            if(type != typeof(long) && type != typeof(long?))
                throw new InvalidOperationException($"Fields tagged with JsonInputLongAttribute must be long-type (int64), but the given type was {type.FullName}");
           
            string value = ((JValue)token).ToString(CultureInfo.InvariantCulture);
            string trimmedValue = value.Trim();
            bool parseSuccessful = long.TryParse(trimmedValue, out long parsedValue);

            if(!parseSuccessful)
                return TryParseResult.Invalid(ValidationError.InvalidInt64(value, path));

            if(parsedValue < minValue)
                return TryParseResult.Invalid(ValidationError.NumericTooSmall(parsedValue, minValue, path));

            if(parsedValue > maxValue)
                return TryParseResult.Invalid(ValidationError.NumericTooLarge(parsedValue, maxValue, path));

            return TryParseResult.Ok(parsedValue);
        }
    }
}
