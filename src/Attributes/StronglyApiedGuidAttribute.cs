using System;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StronglyApiedGuidAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        string exactFormat;
        public StronglyApiedGuidAttribute(string name = null, string exactFormat = null, bool optional = false) : base(name, optional)
        {
            this.exactFormat = exactFormat;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            string trimmedValue = value.Trim();

            if(type != typeof(Guid))
                if(type == typeof(Guid?))
                {
                    if(string.IsNullOrWhiteSpace(trimmedValue))
                        return ParseSuccess.From(null);
                }
                else
                    throw new InvalidOperationException(
                        $"Fields tagged with {typeof(StronglyApiedGuidAttribute).FullName} "
                    +   $"must be a Guid, "
                    +   $"but the given type was {type.FullName}");

            if(exactFormat is null)
                if(Guid.TryParse(trimmedValue, out Guid result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidLooseTimestamp(trimmedValue, path), default);
            else
                if(Guid.TryParseExact(trimmedValue, exactFormat, out Guid result))
                    return ParseSuccess.From(result);
                else
                    return (ErrorDescription.InvalidExactTimestamp(trimmedValue, exactFormat, path), default);
        }
    }
}
