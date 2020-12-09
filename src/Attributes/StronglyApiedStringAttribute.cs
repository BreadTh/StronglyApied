using System;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedStringAttribute : StronglyApiedFieldBase
    {
        public int minLength;
        public int maxLength;

        public StronglyApiedStringAttribute(int minLength = 0, int maxLength = int.MaxValue, bool optional = false) : base(optional)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        override public TryParseResult TryParse(Type type, string value, string path)
        {
            if(type != typeof(string))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedStringAttribute).FullName} "
                +   $"must be a {typeof(string).FullName}, "
                +   $"but the given type was {type.FullName}");
            
            string trimmedValue = value.Trim();
            
            if(trimmedValue.Length < minLength)
                return TryParseResult.Invalid(ErrorDescription.StringTooShort(minLength, trimmedValue, path));

            if(trimmedValue.Length > maxLength)
                return TryParseResult.Invalid(ErrorDescription.StringTooLong(maxLength, trimmedValue, path));
            
            return TryParseResult.Ok(trimmedValue);
        }
    }
}
