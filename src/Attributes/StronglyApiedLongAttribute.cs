using System;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedLongAttribute : StronglyApiedFieldBaseAttribute
    {
        public readonly long minValue;
        public readonly long maxValue;

        public StronglyApiedLongAttribute(string name = null, long minValue = long.MinValue, long maxValue = long.MaxValue, bool optional = false) : base(name, optional)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(Type type, string value, string path)
        {
            if(type != typeof(long) && type != typeof(long?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedLongAttribute).FullName} "
                +   $"must be a {typeof(long).FullName},"
                +   $"but the given type was {type.FullName}");
           
            string trimmedValue = value.Trim();
            bool parseSuccessful = long.TryParse(trimmedValue, out long parsedValue);

            if(!parseSuccessful)
                return (ErrorDescription.InvalidInt64(value, path), default);

            if(parsedValue < minValue)
                return (ErrorDescription.NumericTooSmall(parsedValue, minValue, path), parsedValue);

            if(parsedValue > maxValue)
                return (ErrorDescription.NumericTooLarge(parsedValue, maxValue, path), parsedValue);

            return ParseSuccess.From(parsedValue);
        }
    }
}
