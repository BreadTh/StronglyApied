using System;
using System.Net.Mail;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedEmailAddress : StronglyApiedFieldBase
    {
        public StronglyApiedEmailAddress(bool optional = false) : base(optional) { }

        public override TryParseResult TryParse(Type type, string value, string path)
        {
            if(type != typeof(string))
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedEmailAddress).FullName} "
                +   $"must be {typeof(string).FullName}, "
                +   $" but the given type was {type.FullName}");

            string trimmedValue = value.Trim();
            MailAddress parsedValue;

            try
            {
                parsedValue = new MailAddress(trimmedValue);
            }
            catch
            {
                return TryParseResult.Invalid(ErrorDescription.InvalidEmailAddress(trimmedValue, path));
            }

            //RFC 1035, section 3.1 + RFC 5321, section 2.3.11: 
            //Domain part isn't case sensitive, but user part may be - it's up to the individual mail providers.
            return TryParseResult.Ok(string.Format("{0}@{1}", parsedValue.User, parsedValue.Host.ToLower()));
        }
    }
}
