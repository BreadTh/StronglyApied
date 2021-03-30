﻿using System;
using System.Globalization;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedDateTimeAttribute : StronglyApiedFieldBaseAttribute
    {
        string exactFormat;
        public StronglyApiedDateTimeAttribute(string exactFormat = null, bool optional = false) : base(optional) 
        {
            this.exactFormat = exactFormat;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(type != typeof(DateTime))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedDateTimeAttribute).FullName} "
                +   $"must be a DateTime, "
                +   $"but the given type was {type.FullName}");
            
            string trimmedValue = value.Trim();

            if(exactFormat is null)
                if(DateTime.TryParse(trimmedValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidLooseTimestamp(trimmedValue, path), default);
            else
                if(DateTime.TryParseExact(trimmedValue, exactFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidExactTimestamp(trimmedValue, exactFormat, path), default);
        }
    }
}