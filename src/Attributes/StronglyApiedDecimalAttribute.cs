using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedDecimalAttribute : StronglyApiedFieldBase
    {
        public string minValue;
        public string maxValue;
        public int minDecimalDigits;
        public int maxDecimalDigits;

        //decimals aren't primitives, so they aren't allowed in metadata in the current version of dotnet, hence the use of strings.
        public StronglyApiedDecimalAttribute(string minValue = "-79228162514264337593543950335", string maxValue = "79228162514264337593543950335", int minDecimalDigits = 0, int maxDecimalDigits = 9, bool optional = true)
            : base(optional)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.minDecimalDigits = minDecimalDigits;
            this.maxDecimalDigits = maxDecimalDigits;
        }

        public override TryParseResult TryParse(Type type, JToken token, string path)
        {
            if(type != typeof(decimal) && type != typeof(decimal?))
                throw new InvalidOperationException($"Fields tagged with JsonInputDecimalAttribute must be decimal, but the given type was {type.FullName}");

            string value = ((JValue)token).ToString(CultureInfo.InvariantCulture);
            string trimmedValue = value.Trim();
            bool parseSuccessful = decimal.TryParse(trimmedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedValue);

            if(!parseSuccessful)
                return TryParseResult.Invalid(ValidationError.InvalidInt64(value, path));

            if(parsedValue < decimal.Parse(minValue))
                return TryParseResult.Invalid(ValidationError.NumericTooSmall(parsedValue.ToString(CultureInfo.InvariantCulture), minValue, path));

            if(parsedValue > decimal.Parse(maxValue))
                return TryParseResult.Invalid(ValidationError.NumericTooLarge(parsedValue.ToString(CultureInfo.InvariantCulture), maxValue, path));

            string[] parts = trimmedValue.Split('.');
            int decimalDigits = parts.Length != 2 ? 0 : parts[1].Length;

            if(decimalDigits < minDecimalDigits)
                return TryParseResult.Invalid(ValidationError.TooFewDecimalDigits(parsedValue, minDecimalDigits, path));

            if(decimalDigits > maxDecimalDigits)
                return TryParseResult.Invalid(ValidationError.TooManyDecimalDigits(parsedValue, maxDecimalDigits, path));


            return TryParseResult.Ok(parsedValue);
        }
    }
}
