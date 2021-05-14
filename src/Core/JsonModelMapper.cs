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
                rootToken = JObject.Load(new JsonTextReader(new StringReader(rawbody.Trim())) { FloatParseHandling = FloatParseHandling.Decimal }, null);
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
            
            //return types must be dynamic as one cannot infer the fieldtypes of generics at compiletime.

            dynamic MapObject(FieldInfo field, JToken value, string path)
            {
                var attribute = field.GetCustomAttribute<StronglyApiedObjectAttribute>(inherit: false)
                    ?? new StronglyApiedObjectAttribute();

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!attribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if(attribute.stringified)
                {
                    (object innerResult, List<ErrorDescription> innerErrors) = ModelValidatorImp.Parse(value.ToString(), field.FieldType);
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
                    var fieldType = field.FieldType.IsArray 
                    ?   field.FieldType.GetElementType() 
                    :   field.FieldType;
                    return MapObjectPostValidation(fieldType, value, path);
                }
            }

            dynamic MapObjectPostValidation(Type fieldType, JToken value, string path)
            {
                dynamic result = Activator.CreateInstance(fieldType);

                foreach (FieldInfo childField in fieldType.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic && !fieldInfo.IsStatic))
                {
                    StronglyApiedBaseAttribute childfieldAttribute = childField.GetCustomAttribute<StronglyApiedBaseAttribute>(true)
                        ?? new StronglyApiedBaseAttribute();

                    var childName = childfieldAttribute.name ?? childField.Name;

                    var childPath = path + (path == "" ? "" : ".") + childName;
                    var childValue = value.SelectToken(childName);
                    var errorCountBeforeParse = errors.Count;

                    dynamic parsed = DetermineFieldTypeCategory(childField.FieldType) switch
                    {   FieldTypeCategory.Array => MapArray(childField, childValue, childPath)
                    ,   FieldTypeCategory.Object => MapObject(childField, childValue, childPath)
                    ,   FieldTypeCategory.Value => MapField(childField, childField.FieldType, childValue, childPath)
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
                return result;
            }

            dynamic[] MapArray(FieldInfo field, JToken value, string path)
            {
                StronglyApiedArrayAttribute arrayAttribute = field.GetCustomAttribute<StronglyApiedArrayAttribute>(false)
                    ?? new StronglyApiedArrayAttribute();

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

                Type childType = field.FieldType.GetElementType();

                for(int index = 0; index <= valueAsArray.Count - 1; index++)
                
                    switch (DetermineFieldTypeCategory(childType))
                {
                    case FieldTypeCategory.Object:
                            resultList.Add(MapObject(field, valueAsArray[index], $"{path}[{index}]"));
                        break;
                    case FieldTypeCategory.Value:
                            resultList.Add(MapField(field, childType, valueAsArray[index], $"{path}[{index}]"));
                        break;
                }

                return resultList.ToArray();                
            }

            dynamic MapField(FieldInfo field, Type type, JToken value, string path)
            {
                StronglyApiedFieldBaseAttribute fieldAttribute = field.GetCustomAttribute<StronglyApiedFieldBaseAttribute>(true);

                if(fieldAttribute is null)
                {
                    if(type == typeof(bool))
                        fieldAttribute = new StronglyApiedBoolAttribute();
                    else if (type == typeof(bool?))
                        fieldAttribute = new StronglyApiedBoolAttribute(optional: true);

                    else if (type == typeof(DateTime))
                        fieldAttribute = new StronglyApiedDateTimeAttribute();
                    else if (type == typeof(DateTime?))
                        fieldAttribute = new StronglyApiedDateTimeAttribute(optional: true);

                    else if (type == typeof(decimal))
                        fieldAttribute = new StronglyApiedDecimalAttribute();
                    else if (type == typeof(decimal?))
                        fieldAttribute = new StronglyApiedDecimalAttribute(optional: true);

                    else if (type == typeof(MailAddress))
                        fieldAttribute = new StronglyApiedEmailAddressAttribute();

                    else if (type == typeof(int))
                        fieldAttribute = new StronglyApiedIntAttribute();
                    else if (type == typeof(int?))
                        fieldAttribute = new StronglyApiedIntAttribute(optional: true);

                    else if (type == typeof(long))
                        fieldAttribute = new StronglyApiedLongAttribute();
                    else if (type == typeof(long?))
                        fieldAttribute = new StronglyApiedLongAttribute(optional: true);

                    else if (type.IsEnum)
                        fieldAttribute = new StronglyApiedOptionAttribute();

                    else if (type == typeof(string))
                        fieldAttribute = new StronglyApiedStringAttribute();

                    else
                        throw new ModelAttributeException(
                            $"All primitive fields must be tagged with a child of StronglyApiedFieldBaseAttribute"
                        +   $", but none was found at {rootType.FullName}.{path}"
                        +   $" and the type {type.FullName} does not have a default implementation");
                }

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!fieldAttribute.optional)
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
                        fieldAttribute.Parse(baseType.GetGenericArguments()[0], value.ToCultureInvariantString(), path);


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
                        fieldAttribute.Parse(
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
    }
}
