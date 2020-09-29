﻿using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedStringAttribute : StronglyApiedFieldBase
    {
        public int minLength;
        public int maxLength;
        public StronglyApiedStringAttribute(int minLength = 0, int maxLength = int.MaxValue, bool optional = false) : base(optional)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        override public TryParseResult TryParse(Type type, JToken token, string path)
        {
            if(type != typeof(string))
                throw new InvalidOperationException($"Fields tagged with JsonInputStringAttribute must be string-type, but the given type was {type.FullName}");
            
            string value = ((JValue)token).ToString(CultureInfo.InvariantCulture);
            string trimmedValue = value.Trim();
            
            if(trimmedValue.Length < minLength)
                return TryParseResult.Invalid(ValidationError.StringTooShort(minLength, trimmedValue, path));

            if(trimmedValue.Length > maxLength)
                return TryParseResult.Invalid(ValidationError.StringTooLong(maxLength, trimmedValue, path));
            
            return TryParseResult.Ok(trimmedValue);
        }
    }
}