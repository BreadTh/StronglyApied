using System;
using System.Globalization;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StronglyApiedTimeOnlyAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        string exactFormat;
        public StronglyApiedTimeOnlyAttribute(string name = null, string exactFormat = null, bool optional = false) : base(name, optional)
        {
            this.exactFormat = exactFormat;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            string trimmedValue = value.Trim();

            if(type != typeof(TimeOnly))
                if(type == typeof(TimeOnly?))
                {
                    if(string.IsNullOrWhiteSpace(trimmedValue))
                        return ParseSuccess.From(null);
                }
                else
                    throw new InvalidOperationException(
                        $"Fields tagged with {typeof(StronglyApiedTimeOnlyAttribute).FullName} "
                    +   $"must be a TimeOnly, "
                    +   $"but the given type was {type.FullName}");

            if(exactFormat is null)
                if(TimeOnly.TryParse(trimmedValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidLooseTimestamp(trimmedValue, path), default);
            else
                if(TimeOnly.TryParseExact(trimmedValue, exactFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidExactTimestamp(trimmedValue, exactFormat, path), default);
        }
    }
}
