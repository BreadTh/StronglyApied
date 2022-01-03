using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using OneOf;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class StronglyApiedOptionAttribute : StronglyApiedFieldOrPropertyBaseAttribute
    {
        public StronglyApiedOptionAttribute(string name = null, bool optional = false) : base(name, optional) { }

        public override OneOf<ParseSuccess, (ErrorDescription description, dynamic bestParseAttempt)> Parse(
            Type type, string value, string path)
        {
            if(!type.IsEnum)
                throw new InvalidOperationException(
                    $"Fields tagged with {typeof(StronglyApiedOptionAttribute).FullName} "
                +   $"must be an enum, "
                +   $"but the given type was {type.FullName}");
            
            string trimmedValue = value.Trim();

            List<string> enumValues = type.GetFields()
                .Select((FieldInfo fieldInfo) => fieldInfo.Name.ToString())
                .Where((string value) => value != "value__") //junk data which appears when you use Type.GetFields() to get enum values.
                .Where((string value) => value != "Undefined") //TODO: Think of a good way to exclude "Undefined" for me but not for other people. 
                //I use the enum-value of Undefined to signify that the enum is in Identity-state, but that's specific to me.
                .ToList();

            if(!enumValues.Contains(trimmedValue))
                return (ErrorDescription.InvalidOption(value, enumValues, path), default);
            else
                return ParseSuccess.From(Enum.Parse(type, trimmedValue));
        }
    }
}
