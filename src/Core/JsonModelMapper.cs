using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Exceptions;
using BreadTh.StronglyApied.Attributes.Extending;
using ValueOf;
using BreadTh.StronglyApied.Attributes.Extending.Core;
using System.Net.Mail;

namespace BreadTh.StronglyApied.Core
{
    public class JsonModelMapper : ModelMapperBase
    {
        public override (object result, List<ErrorDescription> errors) MapModel(string rawbody, Type rootType)
        {
            JObject rootToken;
            try
            {
                //though technically the root may be any of the json datatypes (yes, even null), we only want to support object as root
                rootToken = JObject.Load(new JsonTextReader(new StringReader(rawbody.Trim())) { FloatParseHandling = FloatParseHandling.Decimal, DateParseHandling = DateParseHandling.None }, null);
            }
            catch
            {
                return (default, new List<ErrorDescription>() { ErrorDescription.InvalidInputData(rawbody) });
            }

            //Rather than returning and merging a bunch of lists of ErrorDescriptions,
            //the subsequent functions definied in this method
            //make direct reference to the error list below:
            List<ErrorDescription> errors = new List<ErrorDescription>();

            object result = MapObjectPostValidation(rootType, rootToken, "");

            return (result, errors);

            //return types must be dynamic as one cannot infer the field/property types of generics at compiletime.
            dynamic MapObject(MemberInfo member, JToken value, string path)
            {
                var attribute = member.GetCustomAttribute<StronglyApiedObjectAttribute>(inherit: false)
                    ?? new StronglyApiedObjectAttribute(optional: IsNullableReferenceType(member));

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!attribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if(attribute.stringified)
                {
                    var type =
                        member.MemberType == MemberTypes.Field ?
                        ((FieldInfo)member).FieldType :
                        ((PropertyInfo)member).PropertyType;

                    (object innerResult, List<ErrorDescription> innerErrors) = ModelValidatorImp.Parse(value.ToString(), type);
                    errors.AddRange(innerErrors);

                    return innerResult;
                }
                else
                {
                    if (value.Type != JTokenType.Object)
                    {
                        errors.Add(ErrorDescription.NotAnObject(value.ToCultureInvariantString(), path));
                        return null;
                    }

                    Type memberType;
                    if(member.MemberType == MemberTypes.Field)
                    {
                        var field = member as FieldInfo;
                        memberType = field.FieldType.IsArray
                        ?   field.FieldType.GetElementType()
                        :   field.FieldType;
                    }
                    else
                    {
                        var prop = member as PropertyInfo;
                        memberType = prop.PropertyType.IsArray
                        ? prop.PropertyType.GetElementType()
                        : prop.PropertyType;
                    }

                    return MapObjectPostValidation(memberType, value, path);
                }
            }

            dynamic MapObjectPostValidation(Type objectType, JToken value, string path)
            {
                dynamic result = Activator.CreateInstance(objectType);

                foreach (FieldInfo childField in objectType.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic && !fieldInfo.IsStatic))
                {
                    StronglyApiedBaseAttribute childfieldAttribute = childField.GetCustomAttribute<StronglyApiedBaseAttribute>(true)
                        ?? new StronglyApiedBaseAttribute();

                    var childName = childfieldAttribute.name ?? childField.Name;

                    var childPath = path + (path == "" ? "" : ".") + childName;
                    var childValue = value.SelectToken("['" + childName + "']");
                    var errorCountBeforeParse = errors.Count;

                    dynamic parsed = DetermineMemberTypeCategory(childField.FieldType) switch
                    {   MemberTypeCategory.Array => MapArray(childField, childValue, childPath)
                    ,   MemberTypeCategory.Object => MapObject(childField, childValue, childPath)
                    ,   MemberTypeCategory.Value => MapMember(childField, childField.FieldType, childValue, childPath)
                    ,   _ => throw new NotImplementedException()
                    };

                    if (!childField.FieldType.IsArray || parsed == null)
                        childField.SetValue(result, parsed);

                    //Even if we know that "parsed" is an array,
                    //we can't be certain that it doesn't violate a constriction of the field unless it fully validated.
                    //So if we encountered any errors while parsing the array, don't attempt to actually set it.
                    else if (errorCountBeforeParse != errors.Count)
                        childField.SetValue(result, null);
                    else
                    {
                        //.setValue a dynamic into an array field will throw. It must be parsed to an array first.
                        Array dynamicallyTypedArray = Array.CreateInstance(childField.FieldType.GetElementType(), parsed.Length);
                        Array.Copy(parsed, dynamicallyTypedArray, parsed.Length);
                        childField.SetValue(result, dynamicallyTypedArray);
                    }
                }

                foreach (PropertyInfo childProp in objectType.GetProperties().Where((PropertyInfo propInfo) => propInfo.CanWrite))
                {
                    StronglyApiedBaseAttribute childPropertyAttribute = childProp.GetCustomAttribute<StronglyApiedBaseAttribute>(true)
                        ?? new StronglyApiedBaseAttribute();

                    var childName = childPropertyAttribute.name ?? childProp.Name;

                    var childPath = path + (path == "" ? "" : ".") + childName;
                    var childValue = value.SelectToken("['" + childName + "']");
                    var errorCountBeforeParse = errors.Count;

                    dynamic parsed = DetermineMemberTypeCategory(childProp.PropertyType) switch
                    {   MemberTypeCategory.Array => MapArray(childProp, childValue, childPath)
                    ,   MemberTypeCategory.Object => MapObject(childProp, childValue, childPath)
                    ,   MemberTypeCategory.Value => MapMember(childProp, childProp.PropertyType, childValue, childPath)
                    ,   _ => throw new NotImplementedException()
                    };

                    if (!childProp.PropertyType.IsArray || parsed == null)
                        childProp.SetValue(result, parsed);

                    //Even if we know that "parsed" is an array,
                    //we can't be certain that it doesn't violate a constriction of the prop unless it fully validated.
                    //So if we encountered any errors while parsing the array, don't attempt to actually set it.
                    else if (errorCountBeforeParse != errors.Count)
                        childProp.SetValue(result, null);
                    else
                    {
                        //.setValue a dynamic into an array prop will throw. It must be parsed to an array first.
                        Array dynamicallyTypedArray = Array.CreateInstance(childProp.PropertyType.GetElementType(), parsed.Length);
                        Array.Copy(parsed, dynamicallyTypedArray, parsed.Length);
                        childProp.SetValue(result, dynamicallyTypedArray);
                    }
                }

                return result;
            }

            dynamic[] MapArray(MemberInfo member, JToken value, string path)
            {
                StronglyApiedArrayAttribute arrayAttribute = member.GetCustomAttribute<StronglyApiedArrayAttribute>(false)
                    ?? new StronglyApiedArrayAttribute(optional: IsNullableReferenceType(member));

                //validate json content
                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!arrayAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if(value.Type != JTokenType.Array)
                {
                    errors.Add(ErrorDescription.NotAnArray(JsonConvert.SerializeObject(value), path));
                    return null;
                }

                JArray valueAsArray = (JArray)value;

                if(valueAsArray.Count < arrayAttribute.minLength)
                    errors.Add(ErrorDescription.ArrayTooShort(valueAsArray.Count, arrayAttribute.minLength, path));

                else if(valueAsArray.Count > arrayAttribute.maxLength)
                    errors.Add(ErrorDescription.ArrayTooLong(valueAsArray.Count, arrayAttribute.maxLength, path));

                //parse content
                List<dynamic> resultList = new List<dynamic>();

                Type childType;
                if(member.MemberType == MemberTypes.Field)
                    childType = ((FieldInfo)member).FieldType.GetElementType();
                else
                    childType = ((PropertyInfo)member).PropertyType.GetElementType();

                for(int index = 0; index <= valueAsArray.Count - 1; index++)
                    switch (DetermineMemberTypeCategory(childType))
                    {
                        case MemberTypeCategory.Object:
                                resultList.Add(MapObject(member, valueAsArray[index], $"{path}[{index}]"));
                            break;
                        case MemberTypeCategory.Value:
                                resultList.Add(MapMember(member, childType, valueAsArray[index], $"{path}[{index}]"));
                            break;
                    }

                return resultList.ToArray();
            }



            dynamic MapMember(MemberInfo member, Type type, JToken value, string path)
            {
                StronglyApiedFieldOrPropertyBaseAttribute memberAttribute = member.GetCustomAttribute<StronglyApiedFieldOrPropertyBaseAttribute>(true);

                if(memberAttribute is null)
                {
                    if(type == typeof(bool))
                        memberAttribute = new StronglyApiedBoolAttribute();
                    else if (type == typeof(bool?))
                        memberAttribute = new StronglyApiedBoolAttribute(optional: true);

                    else if (type == typeof(DateTime))
                        memberAttribute = new StronglyApiedDateTimeAttribute();
                    else if (type == typeof(DateTime?))
                        memberAttribute = new StronglyApiedDateTimeAttribute(optional: true);

                    else if (type == typeof(DateTimeOffset))
                        memberAttribute = new StronglyApiedDateTimeOffsetAttribute();
                    else if (type == typeof(DateTimeOffset?))
                        memberAttribute = new StronglyApiedDateTimeOffsetAttribute(optional: true);

                    else if (type == typeof(TimeOnly))
                        memberAttribute = new StronglyApiedTimeOnlyAttribute();
                    else if (type == typeof(TimeOnly?))
                        memberAttribute = new StronglyApiedTimeOnlyAttribute(optional: true);

                    else if (type == typeof(DateOnly))
                        memberAttribute = new StronglyApiedDateOnlyAttribute();
                    else if (type == typeof(DateOnly?))
                        memberAttribute = new StronglyApiedDateOnlyAttribute(optional: true);

                    else if (type == typeof(Guid))
                        memberAttribute = new StronglyApiedGuidAttribute();
                    else if (type == typeof(Guid?))
                        memberAttribute = new StronglyApiedGuidAttribute(optional: true);

                    else if (type == typeof(decimal))
                        memberAttribute = new StronglyApiedDecimalAttribute();
                    else if (type == typeof(decimal?))
                        memberAttribute = new StronglyApiedDecimalAttribute(optional: true);

                    else if (type == typeof(MailAddress))
                        memberAttribute = new StronglyApiedEmailAddressAttribute();

                    else if (type == typeof(int))
                        memberAttribute = new StronglyApiedIntAttribute();
                    else if (type == typeof(int?))
                        memberAttribute = new StronglyApiedIntAttribute(optional: true);

                    else if (type == typeof(long))
                        memberAttribute = new StronglyApiedLongAttribute();
                    else if (type == typeof(long?))
                        memberAttribute = new StronglyApiedLongAttribute(optional: true);

                    else if (type.IsEnum)
                        memberAttribute = new StronglyApiedOptionAttribute();
                    else if (type.IsGenericType
                        && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && type.GetGenericArguments()[0].IsEnum
                    )
                        memberAttribute = new StronglyApiedOptionAttribute(optional: true);

                    else if (type == typeof(string))
                        memberAttribute = new StronglyApiedStringAttribute(optional: IsNullableReferenceType(member));

                    else
                        throw new ModelAttributeException(
                            $"All primitive fields/properties must be tagged with a child of StronglyApiedFieldOrPropertyBaseAttribute"
                        +   $", but none was found at {rootType.FullName}.{path}"
                        +   $" and the type {type.FullName} does not have a default implementation");
                }

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!memberAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));
                    return null;
                }

                var baseType = type.BaseType;

                //does the type inherit ValueOf (and is it implemented correctly)
                if( baseType.IsGenericType
                &&  baseType.GetGenericTypeDefinition() == typeof(ValueOf<,>)
                &&  baseType.GetGenericArguments()[1] == type)
                {
                    var tryParseOutcome =
                        memberAttribute.Parse(baseType.GetGenericArguments()[0], value.ToCultureInvariantString(), path);


                    if(tryParseOutcome.TryPickT0(out var success, out var error))
                    {
                        var method = type.BaseType.GetMethod("From", BindingFlags.Public | BindingFlags.Static);
                        return method.Invoke(null, new object[]{ success.Value });
                    }
                    else
                    {
                        errors.Add(error.description);
                        return error.bestParseAttempt;
                    }
                }
                else
                {
                    var tryParseOutcome =
                        memberAttribute.Parse(
                            type.IsArray
                            ? type.GetElementType()
                            : type, value.ToCultureInvariantString()
                        ,   path);

                    if(tryParseOutcome.TryPickT0(out var success, out var error))
                        return success.Value;
                    else
                    {
                        errors.Add(error.description);
                        return error.bestParseAttempt;
                    }

                }

            }
        }
        private bool IsNullableReferenceType(MemberInfo member)
        {
            var writeState =
                member is FieldInfo
                ? new NullabilityInfoContext().Create((FieldInfo)member).WriteState
                : new NullabilityInfoContext().Create((PropertyInfo)member).WriteState;

            return writeState switch
            {
                NullabilityState.NotNull => false,
                NullabilityState.Nullable => true,
                _ => throw new Exception(
                    "You must enable nullable reference type to automatically infer StronglyApied optionallity on properties/fields. " +
                    "Either enable reference nullability by adding <Nullable>enable</Nullable> to your .csproj, or" +
                    "Explicitly decorate your reference types with StronglyApiedString, etc")
            };
        }
    }
}
