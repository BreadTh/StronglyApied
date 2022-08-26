using System;
using System.Globalization;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StronglyApiedDateOnlyAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        string exactFormat;
        public StronglyApiedDateOnlyAttribute(string name = null, string exactFormat = null, bool optional = false) : base(name, optional)
        {
            this.exactFormat = exactFormat;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            string trimmedValue = value.Trim();

            if(type != typeof(DateOnly))
                if(type == typeof(DateOnly?))
                {
                    if(string.IsNullOrWhiteSpace(trimmedValue))
                        return ParseSuccess.From(null);
                }
                else
                    throw new InvalidOperationException(
                        $"Fields tagged with {typeof(StronglyApiedDateOnlyAttribute).FullName} "
                    +   $"must be a DateOnly, "
                    +   $"but the given type was {type.FullName}");

            if(exactFormat is null)
                if(DateOnly.TryParse(trimmedValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidLooseTimestamp(trimmedValue, path), default);
            else
                if(DateOnly.TryParseExact(trimmedValue, exactFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidExactTimestamp(trimmedValue, exactFormat, path), default);
        }
    }
}
