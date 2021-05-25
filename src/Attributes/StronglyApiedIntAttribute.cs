using System;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedIntAttribute : StronglyApiedFieldBaseAttribute
    {
        public readonly int minValue;
        public readonly int maxValue;
    
        public StronglyApiedIntAttribute(string name = null, int minValue = int.MinValue, int maxValue = int.MaxValue, bool optional = false) 
            : base(name, optional)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(type != typeof(int) && type != typeof(int?))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedIntAttribute).FullName} "
                +   $"must be a {typeof(int).FullName}, "
                +   $"but the given type was {type.FullName}");

            if (string.IsNullOrEmpty(value))
                if(optional)
                    return ParseSuccess.From(null);
                else
                    return (ErrorDescription.InvalidInt32(value, path), default);

            //allow decimals if they're all 0
            string[] parts = value.Split('.');
            if (parts.Length != 1 && (parts.Length != 2 || parts[1].Trim(new char[] { ' ', '0' }).Length == 0) )
                return (ErrorDescription.InvalidInt32(value, path), default);
            
            bool parseSuccessful = int.TryParse(parts[0].Trim(), out int parsedValue);

            if(!parseSuccessful)
                return (ErrorDescription.InvalidInt32(value, path), default);

            if(parsedValue < minValue)
                return (ErrorDescription.NumericTooSmall(parsedValue, minValue, path), parsedValue);

            if(parsedValue > maxValue)
                return (ErrorDescription.NumericTooLarge(parsedValue, maxValue, path), parsedValue);

            return ParseSuccess.From(parsedValue);
        }
    }
}
