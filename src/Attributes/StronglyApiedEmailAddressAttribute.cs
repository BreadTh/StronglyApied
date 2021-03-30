using System;
using System.Net.Mail;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedEmailAddressAttribute : StronglyApiedFieldBaseAttribute
    {
        public StronglyApiedEmailAddressAttribute(bool optional = false) : base(optional) { }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(type != typeof(string) && type != typeof(MailAddress))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedEmailAddressAttribute).FullName} "
                +   $"must be {typeof(string).FullName} or {typeof(MailAddress).FullName}, "
                +   $" but the given type was {type.FullName}");

            string trimmedValue = value.Trim();
            MailAddress parsedValue;

            try
            {
                parsedValue = new MailAddress(trimmedValue);
            }
            catch
            {
                return (ErrorDescription.InvalidEmailAddress(trimmedValue, path), trimmedValue);
            }

            if(type == typeof(string))
                //RFC 1035, section 3.1 + RFC 5321, section 2.3.11: 
                //Domain part isn't case sensitive, but user part may be - it's up to the individual mail providers.
                return ParseSuccess.From(string.Format("{0}@{1}", parsedValue.User, parsedValue.Host.ToLower()));
            
            else
                return ParseSuccess.From(parsedValue);
            
        }
    }
}
