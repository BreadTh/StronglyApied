using System;
using System.Globalization;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] 
    public sealed class StronglyApiedDecimalAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        public readonly string minValue;
        public readonly string maxValue;
        public readonly int minDecimalDigits;
        public readonly int maxDecimalDigits;

        //decimals aren't primitives, so they aren't allowed in metadata in the current version of dotnet, hence the use of strings.
        public StronglyApiedDecimalAttribute(string name = null, string minValue = "-79228162514264337593543950335", string maxValue = "79228162514264337593543950335", int minDecimalDigits = 0, int maxDecimalDigits = 29, bool optional = false)
            : base(name, optional)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.minDecimalDigits = minDecimalDigits;
            this.maxDecimalDigits = maxDecimalDigits;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(Type type, string value, string path)
        {
            if(type != typeof(decimal) && type != typeof(decimal?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedDecimalAttribute).FullName} "
                +   $"must be a {typeof(decimal).FullName},"
                +   $"but the given type was {type.FullName}");;

            string trimmedValue = value.Trim();

            if(!decimal.TryParse(trimmedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedValue))
                return (ErrorDescription.InvalidInt64(value, path), default);

            if(parsedValue < decimal.Parse(minValue, NumberStyles.Number, CultureInfo.InvariantCulture))
                return (ErrorDescription.NumericTooSmall(parsedValue.ToString(CultureInfo.InvariantCulture), minValue, path), parsedValue);

            if(parsedValue > decimal.Parse(maxValue, NumberStyles.Number, CultureInfo.InvariantCulture))
                return (ErrorDescription.NumericTooLarge(parsedValue.ToString(CultureInfo.InvariantCulture), maxValue, path), parsedValue);

            string[] parts = trimmedValue.Split('.');
            int decimalDigits = parts.Length != 2 ? 0 : parts[1].Length;

            if(decimalDigits < minDecimalDigits)
                return (ErrorDescription.TooFewDecimalDigits(parsedValue, minDecimalDigits, path), parsedValue);

            if(decimalDigits > maxDecimalDigits)
                return (ErrorDescription.TooManyDecimalDigits(parsedValue, maxDecimalDigits, path), parsedValue);

            return ParseSuccess.From(parsedValue);
        }
    }
}
