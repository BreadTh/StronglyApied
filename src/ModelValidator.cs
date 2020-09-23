using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied
{
    public class ModelValidator : IModelValidator
    {
        public IEnumerable<ValidationError> TryParse<T>(Stream json, out T result)
        {
            using StreamReader reader = new StreamReader(json);
            return TryParse(reader.ReadToEnd(), out result);
        }

        public IEnumerable<ValidationError> TryParse<T>(string json, out T result)
        {
            if(!TryParseJObject(json, out JObject rootToken))
            {
                result = Activator.CreateInstance<T>();
                return new List<ValidationError>(){ ValidationError.InvalidJson(json) };
            }

            List<ValidationError> errors = new List<ValidationError>();
            result = (T)TryParseObject(typeof(T), rootToken, "", new StronglyApiedObjectAttribute(false));
            return errors;

            dynamic TryParse(FieldInfo fieldInfo, JToken token, string path)
            {
                if(fieldInfo.FieldType.IsArray)
                    return TryParseArray(fieldInfo, token, path);

                if(fieldInfo.FieldType.IsNonStringClass())
                    return TryParseObject(fieldInfo.FieldType, token, path, fieldInfo.GetCustomAttribute<StronglyApiedObjectAttribute>(false));

                if(fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.Name != "Nullable`1")
                    throw new NotImplementedException("Generic types other than Nullable<T> are not supported. (if you need a list, use array[] instead of List<T>)");

                else
                    return TryParseField(fieldInfo, fieldInfo.FieldType, token, path);
            }

            dynamic TryParseObject(Type objectType, JToken token, string path, StronglyApiedObjectAttribute attribute)
            {
                if(attribute == null)
                    throw new InvalidOperationException($"All object fields and array of object fields must be tagged with JsonInputObjectAttribute, but none was found at {path}");

                if(token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                {
                    if(attribute != null && !attribute.optional)
                        errors.Add(ValidationError.OptionalViolation(path));

                    return null;
                }

                dynamic result = Activator.CreateInstance(objectType);
                
                foreach(FieldInfo fieldInfo in objectType.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic))
                {
                    int errorCountBeforeParse = errors.Count;
                    dynamic value = TryParse(fieldInfo, token.SelectToken(fieldInfo.Name), $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{fieldInfo.Name}");

                    if(fieldInfo.FieldType.IsArray && value != null)
                    {
                        if (errorCountBeforeParse != errors.Count)
                            fieldInfo.SetValue(result, null);
                        else 
                        {
                            Array dynamicallyTypedArray = Array.CreateInstance(fieldInfo.FieldType.GetElementType(), value.Length);
                            Array.Copy(value, dynamicallyTypedArray, value.Length);
                            fieldInfo.SetValue(result, dynamicallyTypedArray);
                        }
                    }
                    else
                        fieldInfo.SetValue(result, value);
                }
                return result;
            }

            dynamic[] TryParseArray(FieldInfo fieldInfo, JToken token, string path)
            {
                if(!fieldInfo.FieldType.IsSZArray)
                    throw new NotImplementedException("Only single-dimensional arrays are supported.");

                StronglyApiedArrayAttribute arrayAttribute = fieldInfo.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if(arrayAttribute == null)
                    throw new NotImplementedException($"All array fields must be tagged with JsonInputArrayAttribute, but none was found at {path}");

                if(token == null || token.Type != JTokenType.Array)
                {
                    if(token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                    {
                        if(arrayAttribute != null && !arrayAttribute.optional)
                            errors.Add(ValidationError.OptionalViolation(path));
                    }
                    else
                        errors.Add(ValidationError.NotAnArray(token.ToString(), path));
                    
                    return null;
                }
                else
                {
                    JArray tokenAsArray = (JArray)token;
                    Type childType = fieldInfo.FieldType.GetElementType();

                    if (arrayAttribute != null)
                        if(tokenAsArray.Count < arrayAttribute.minLength)
                            errors.Add(ValidationError.ArrayTooShort(tokenAsArray.Count, arrayAttribute.minLength, path));

                        else if(tokenAsArray.Count > arrayAttribute.maxLength)
                            errors.Add(ValidationError.ArrayTooLong(tokenAsArray.Count, arrayAttribute.maxLength, path));

                    List<dynamic> resultList = new List<dynamic>();

                    int lastIndex = tokenAsArray.Count - 1;

                    if(childType.IsNonStringClass())
                        for(int index = 0; index <= lastIndex; index++)
                            resultList.Add(TryParseObject(childType, tokenAsArray[index], $"{path}[{index}]", fieldInfo.GetCustomAttribute<StronglyApiedObjectAttribute>(false)));
                        
                    else
                        for(int index = 0; index <= lastIndex; index++)
                            resultList.Add(TryParseField(fieldInfo, childType, tokenAsArray[index], $"{path}[{index}]"));
                        
                    return resultList.ToArray();
                }
            }

            dynamic TryParseField(FieldInfo fieldInfo, Type type, JToken token, string path)
            {
                StronglyApiedFieldBase attribute = fieldInfo.GetCustomAttribute<StronglyApiedFieldBase>(true);

                if(token is null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
                {

                    if(attribute != null && !attribute.optional)
                        errors.Add(ValidationError.OptionalViolation(path));
                    
                    return null;
                }

                string value = token.ToString();
                string trimmedValue = value.Trim();

                if(token.Type == JTokenType.Array || token.Type == JTokenType.Object)
                {
                    errors.Add(ValidationError.NotPrimitive(path, value));
                    return null;
                }
                else
                {
                    if(attribute == null)
                        throw new InvalidOperationException($"All primitive fields must be tagged with a child of JsonInputFieldBase, but none was found at {path}");

                    StronglyApiedFieldBase.TryParseResult tryParseOutcome = attribute.TryParse(type, token, path);

                    if(tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                        errors.Add(tryParseOutcome.error);
                    return tryParseOutcome.result;
                    
                }
            }

            static bool TryParseJObject(string strInput, out JObject result)
            {
                result = default;
                
                if (string.IsNullOrWhiteSpace(strInput)) 
                    return false;

                strInput = strInput.Trim();

                if (!strInput.StartsWith("{") || !strInput.EndsWith("}"))
                    return false;

                try
                {
                    result = JObject.Parse(strInput);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        } 
    }

    internal static class ExtensionMethods 
    {
        public static bool IsStruct(this Type type) =>
            type.IsValueType && !type.IsPrimitive && !type.IsEnum;

        public static bool IsNonStringClass(this Type type) =>
            type.IsClass && type != typeof(string);
    }
}
