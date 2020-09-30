using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Core;
using BreadTh.StronglyApied.Core.ModelValidators;
using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied
{
    public class ModelValidator : IModelValidator
    {
        public IEnumerable<ValidationError> TryParse<T>(Stream text, out T result)
        {
            using StreamReader reader = new StreamReader(text);
            return TryParse(reader.ReadToEnd(), out result);
        }

        public IEnumerable<ValidationError> TryParse<T>(string text, out T result)
        {
            if(TryParseJson(text, out JObject jsonToken))
                return TryParse(new JTokenWrapper(jsonToken), out result);
            
//            if(TryParseXml(text, out XDocument XmlToken))
//                return Validator.TryParse(new XElementWrapper(XmlToken.), out result);

            result = Activator.CreateInstance<T>();
            return new List<ValidationError>(){ ValidationError.InvalidInputData(text) };
            
        }

        private bool TryParseJson(string input, out JObject result)
        {
            result = default;
                
            if (string.IsNullOrWhiteSpace(input)) 
                return false;

            input = input.Trim();

            //though technically the root can be any of the json datatypes (yes, even null), we only want to support object as root.
            if (!input.StartsWith("{") || !input.EndsWith("}"))
                return false;

            try
            {
                result = JObject.Parse(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryParseXml(string input, out XDocument result)
        {
            try
            {
                result = XDocument.Parse(input);
                return true;
            }
            catch(Exception)
            {
                result = default;
                return false;
            }
        }

        public static IEnumerable<ValidationError> TryParse<T>(IToken rootToken, out T result)
        {
            List<ValidationError> errors = new List<ValidationError>();
            result = (T)TryParseObject(typeof(T), rootToken, "", new StronglyApiedObjectAttribute(false));
            return errors;

            dynamic TryParse(FieldInfo fieldInfo, IToken token, string path)
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

            dynamic TryParseObject(Type objectType, IToken token, string path, StronglyApiedObjectAttribute attribute)
            {
                if(attribute == null)
                    throw new InvalidOperationException(
                        $"All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute, but none was found at {path}");

                if(token.IsNullOrUndefined())
                {
                    if(attribute != null && !attribute.optional)
                        errors.Add(ValidationError.OptionalViolation(path));

                    return null;
                }

                if(!token.IsObject())
                {
                    errors.Add(ValidationError.NotAnObject(token.ToString(), path));
                    return null;
                }

                dynamic result = Activator.CreateInstance(objectType);
                
                foreach(FieldInfo fieldInfo in objectType.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic))
                {
                    IToken fieldToken = true == true ? token.GetChild(fieldInfo.Name) : token.GetAttribute(fieldInfo.Name); 

                    int errorCountBeforeParse = errors.Count;
                    dynamic value = TryParse(fieldInfo, fieldToken, $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{fieldInfo.Name}");

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

            dynamic[] TryParseArray(FieldInfo fieldInfo, IToken token, string path)
            {
                if(!fieldInfo.FieldType.IsSZArray)
                    throw new NotImplementedException("Only single-dimensional arrays are supported.");

                StronglyApiedArrayAttribute arrayAttribute = fieldInfo.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if(arrayAttribute == null)
                    throw new NotImplementedException($"All array fields must be tagged with JsonInputArrayAttribute, but none was found at {path}");

                if(token == null || !token.IsArray())
                {
                    if(token.IsNullOrUndefined())
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
                    List<IToken> tokenAsArray = token.GetChildren().ToList();
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

            dynamic TryParseField(FieldInfo fieldInfo, Type type, IToken token, string path)
            {
                StronglyApiedFieldBase attribute = fieldInfo.GetCustomAttribute<StronglyApiedFieldBase>(true);

                if(token.IsNullOrUndefined())
                {

                    if(attribute != null && !attribute.optional)
                        errors.Add(ValidationError.OptionalViolation(path));
                    
                    return null;
                }

                if(!token.IsPrimitive())
                {
                    errors.Add(ValidationError.NotPrimitive(path, token.ToString()));
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
        } 
    }
}
