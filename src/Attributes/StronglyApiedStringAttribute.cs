using System;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedStringAttribute : StronglyApiedFieldBaseAttribute
    {
        public readonly int minLength;
        public readonly int maxLength;

        public StronglyApiedStringAttribute(string name = null, int minLength = 0, int maxLength = int.MaxValue, bool optional = false) : base(name, optional)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(type != typeof(string))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedStringAttribute).FullName} "
                +   $"must be a {typeof(string).FullName}, "
                +   $"but the given type was {type.FullName}");
            
            string trimmedValue = value.Trim();
            
            if(trimmedValue.Length < minLength)
                return (ErrorDescription.StringTooShort(minLength, trimmedValue, path), trimmedValue);

            if(trimmedValue.Length > maxLength)
                return (ErrorDescription.StringTooLong(maxLength, trimmedValue, path), trimmedValue);
            
            return ParseSuccess.From(trimmedValue);
        }
    }
}
