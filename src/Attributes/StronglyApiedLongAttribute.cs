using System;

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

        public override TryParseResult TryParse(Type type, string value, string path)
        {
            if(type != typeof(long) && type != typeof(long?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedLongAttribute).FullName} "
                +   $"must be a {typeof(long).FullName},"
                +   $"but the given type was {type.FullName}");
           
            string trimmedValue = value.Trim();
            bool parseSuccessful = long.TryParse(trimmedValue, out long parsedValue);

            if(!parseSuccessful)
                return TryParseResult.Invalid(ErrorDescription.InvalidInt64(value, path));

            if(parsedValue < minValue)
                return TryParseResult.Invalid(ErrorDescription.NumericTooSmall(parsedValue, minValue, path));

            if(parsedValue > maxValue)
                return TryParseResult.Invalid(ErrorDescription.NumericTooLarge(parsedValue, maxValue, path));

            return TryParseResult.Ok(parsedValue);
        }
    }
}
