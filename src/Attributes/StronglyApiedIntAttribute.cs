using System;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedInt : StronglyApiedFieldBase
    {
        public int minValue;
        public int maxValue;
    
        public StronglyApiedInt(int minValue = int.MinValue, int maxValue = int.MaxValue, bool optional = false) 
            : base(optional) 
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override TryParseResult TryParse(Type type, string value, string path)
        {
            if(type != typeof(int) && type != typeof(int?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedInt).FullName} "
                +   $"must be a {typeof(int).FullName}, "
                +   $"but the given type was {type.FullName}");
           
            string trimmedValue = value.Trim();
            bool parseSuccessful = int.TryParse(trimmedValue, out int parsedValue);

            if(!parseSuccessful)
                return TryParseResult.Invalid(ErrorDescription.InvalidInt32(value, path));

            if(parsedValue < minValue)
                return TryParseResult.Invalid(ErrorDescription.NumericTooSmall(parsedValue, minValue, path));

            if(parsedValue > maxValue)
                return TryParseResult.Invalid(ErrorDescription.NumericTooLarge(parsedValue, maxValue, path));

            return TryParseResult.Ok(parsedValue);
        }
    }
}
