using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BreadTh.StronglyApied.Direct.Attributes.Extending;

namespace BreadTh.StronglyApied.Direct.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedOptionAttribute : StronglyApiedFieldBase
    {
        public StronglyApiedOptionAttribute(bool optional = false) : base(optional) { }

        public override TryParseResult TryParse(Type type, string value, string path)
        {
            if(!type.IsEnum)
                throw new InvalidOperationException($"Fields tagged with JsonInputOptionAttribute must be an enum, but the given type was {type.FullName}");
            
            string trimmedValue = value.Trim();

            List<string> enumValues = type.GetFields()
                .Select((FieldInfo fieldInfo) => fieldInfo.Name.ToString())
                .Where((string value) => value != "value__") //junk data which appears when you use Type.GetFields() to get enum values.
                .Where((string value) => value != "Undefined") //TODO: Think of a good way to exclude "Undefined" for me but not for other people. 
                //I use the enum-value of Undefined to signify that the enum is in Identity-state, but that's specific to me.
                .ToList();

            if(!enumValues.Contains(trimmedValue))
                return TryParseResult.Invalid(ValidationError.InvalidOption(value, enumValues, path));
            else
                return TryParseResult.Ok(Enum.Parse(type, trimmedValue));
        }
    }
}
