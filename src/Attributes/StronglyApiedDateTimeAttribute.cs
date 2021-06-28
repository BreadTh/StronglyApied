using System;
using System.Globalization;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedDateTimeAttribute : StronglyApiedFieldBaseAttribute
    {
        string exactFormat;
        public StronglyApiedDateTimeAttribute(string name = null, string exactFormat = null, bool optional = false) : base(name, optional) 
        {
            this.exactFormat = exactFormat;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            string trimmedValue = value.Trim();

            if(type != typeof(DateTime))
                if(type == typeof(DateTime?))
                {
                    if(string.IsNullOrWhiteSpace(trimmedValue))
                        return ParseSuccess.From(null);
                }
                else
                    throw new InvalidOperationException(
                        $"Fields tagged with {typeof(StronglyApiedDateTimeAttribute).FullName} "
                    +   $"must be a DateTime, "
                    +   $"but the given type was {type.FullName}");
            
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
