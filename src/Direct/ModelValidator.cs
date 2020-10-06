using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Direct.Core;
using BreadTh.StronglyApied.Direct.Attributes;
using BreadTh.StronglyApied.Direct.Attributes.Extending;
using BreadTh.StronglyApied.Direct.Attributes.Core;

namespace BreadTh.StronglyApied.Direct
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
            text = text.Trim();

            StronglyApiedRootAttribute rootAttribute = typeof(T).GetCustomAttribute<StronglyApiedRootAttribute>(false);

            if(rootAttribute == null)
                throw new InvalidOperationException(
                        $"The root model {typeof(T)} must be tagged with the attribute, StronglyApiedObjectAttribute, but none was found.");

            switch(rootAttribute.datamodel)
            {
                case DataModel.JSON:
                    if(TryTokenizeJson(text, out JObject jsonToken))
                        return MapToModel(new JTokenWrapper(jsonToken), new StronglyApiedObjectAttribute(), out result);
                    break;
                case DataModel.XML:
                    if(TryTokenizeXml(text, out XDocument XmlToken))
                        return MapToModel(new XElementWrapper(XmlToken.Root), new StronglyApiedObjectAttribute(), out result);
                    break;
                default:
                    throw new InvalidOperationException("The root object datamodel attribute must be configured with a valid DataModel enum (other than Undefined)");
            }

            result = default;
            return new List<ValidationError>(){ ValidationError.InvalidInputData(text) };
        }

        private bool TryTokenizeJson(string input, out JObject result)
        {
            //though technically the root can be any of the json datatypes (yes, even null), we only want to support object as root.
            if (!input.StartsWith("{") || !input.EndsWith("}"))
            { 
                result = null;
                return false;
            }

            try
            {
                result = JObject.Parse(input);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private bool TryTokenizeXml(string input, out XDocument result)
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

        private static IEnumerable<ValidationError> MapToModel<T>(IToken rootToken, StronglyApiedObjectAttribute rootAttribute, out T result)
        {
            List<ValidationError> errors = new List<ValidationError>();
            result = (T)MapObject(typeof(T), rootToken, "", rootAttribute);
            return errors;

            dynamic DetermineTypeAndMap(FieldInfo fieldInfo, IToken token, IToken parentToken, string path)
            {
                if(fieldInfo.FieldType.IsArray)
                    return MapArray(fieldInfo, parentToken, path);

                if(fieldInfo.FieldType.IsNonStringClass())
                    return MapObject(fieldInfo.FieldType, token, path, fieldInfo.GetCustomAttribute<StronglyApiedObjectAttribute>(false));

                if(fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.Name != "Nullable`1")
                    throw new NotImplementedException("Generic types other than Nullable<T> are not supported. (if you need a list, use array[] instead of List<T>)");

                else
                    return MapFieldInObject(fieldInfo, fieldInfo.FieldType, parentToken, path);
            }

            dynamic MapObject(Type objectType, IToken token, string path, StronglyApiedObjectAttribute attribute)
            {
                if(attribute == null)
                    throw new InvalidOperationException(
                        $"All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute, but none was found at {path}");

                if(token.IsNullOrUndefinedAsObject())
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
                    if(fieldInfo.IsStatic)
                        continue;

                    StronglyApiedRelationBaseAttribute relationAttribute = fieldInfo.GetCustomAttribute<StronglyApiedRelationBaseAttribute>(true);
                    string fieldName = relationAttribute?.name ?? fieldInfo.Name;

                    int errorCountBeforeParse = errors.Count;
                    dynamic value = DetermineTypeAndMap(fieldInfo, token.GetChild(fieldName), token, $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{fieldName}");

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

            dynamic[] MapArray(FieldInfo fieldInfo, IToken parentToken, string path)
            {
                if(!fieldInfo.FieldType.IsSZArray)
                    throw new NotImplementedException("Only single-dimensional arrays are supported.");

                StronglyApiedArrayAttribute arrayAttribute = fieldInfo.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if(arrayAttribute == null)
                    throw new NotImplementedException($"All array fields must be tagged with StronglyApiedArrayAttribute, but none was found at {path}");

                StronglyApiedRelationBaseAttribute relationAttribute = fieldInfo.GetCustomAttribute<StronglyApiedRelationBaseAttribute>(true);
                string fieldName = relationAttribute?.name ?? fieldInfo.Name;

                if(parentToken.IsChildAsArrayNullOrUndefined(fieldName))
                {
                    if(!arrayAttribute.optional)
                        errors.Add(ValidationError.OptionalViolation(path));

                    return null;
                }

                if(!parentToken.IsChildArray(fieldName))
                {
                    errors.Add(ValidationError.NotAnArray(JsonConvert.SerializeObject(parentToken.GetChildren(fieldName).Select(child => child.ToString())), path));
                    return null;
                }
                else
                {
                    List<IToken> tokenAsArray = parentToken.GetChildren(fieldName).ToList();
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
                            resultList.Add(MapObject(childType, tokenAsArray[index], $"{path}[{index}]", fieldInfo.GetCustomAttribute<StronglyApiedObjectAttribute>(false)));
                        
                    else
                        for(int index = 0; index <= lastIndex; index++)
                            resultList.Add(MapFieldInArray(fieldInfo, childType, tokenAsArray[index], $"{path}[{index}]"));
                        
                    return resultList.ToArray();
                }
            }

            dynamic MapFieldInArray(FieldInfo fieldInfo, Type type, IToken token, string path)
            {
                StronglyApiedFieldBase datatypeAttribute = fieldInfo.GetCustomAttribute<StronglyApiedFieldBase>(true);
                    if(token.IsNullOrUndefinedAsPrimitive())
                    {
                        if(datatypeAttribute != null && !datatypeAttribute.optional)
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
                    if(datatypeAttribute == null)
                        throw new InvalidOperationException($"All primitive fields must be tagged with a child of StronglyApiedFieldBase, but none was found at {path}");

                    StronglyApiedFieldBase.TryParseResult tryParseOutcome = datatypeAttribute.TryParse(type, token.ToString(), path);

                    if(tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                        errors.Add(tryParseOutcome.error);
                    return tryParseOutcome.result;
                }
            }

            dynamic MapFieldInObject(FieldInfo fieldInfo, Type type, IToken parentToken, string path)
            {
                StronglyApiedRelationBaseAttribute relationAttribute = fieldInfo.GetCustomAttribute<StronglyApiedRelationBaseAttribute>(true);
                string fieldName = relationAttribute?.name ?? fieldInfo.Name;
                bool isAttribute = relationAttribute != null && relationAttribute.GetType() == typeof(StronglyApiedAttributeAttribute);


                StronglyApiedFieldBase datatypeAttribute = fieldInfo.GetCustomAttribute<StronglyApiedFieldBase>(true);

                if(isAttribute)
                {
                    if(parentToken.IsAttributeNullOrUndefined(fieldName))
                    {
                        if(datatypeAttribute != null && !datatypeAttribute.optional)
                            errors.Add(ValidationError.OptionalViolation(path));
                        
                        return null;
                    }
                }
                else
                {
                    if(parentToken.IsChildNullOrUndefined(fieldName))
                    {
                        if(datatypeAttribute != null && !datatypeAttribute.optional)
                            errors.Add(ValidationError.OptionalViolation(path));
                        
                        return null;
                    }
                }

                if(!isAttribute && !parentToken.IsChildPrimitive(fieldName))
                {
                    errors.Add(ValidationError.NotPrimitive(path, parentToken.GetChildAsText(fieldName)));
                    return null;
                }
                else
                {
                    if(datatypeAttribute == null)
                        throw new InvalidOperationException($"All primitive fields must be tagged with a child of StronglyApiedFieldBaseAttribute, but none was found at {path}");

                    string value = isAttribute ? parentToken.GetAttribute(fieldName) : parentToken.GetChild(fieldName).ToString();

                    StronglyApiedFieldBase.TryParseResult tryParseOutcome = datatypeAttribute.TryParse(type, value, path);

                    if(tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                        errors.Add(tryParseOutcome.error);
                    return tryParseOutcome.result;
                }
            }
        } 
    }
}
