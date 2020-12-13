using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Attributes.Extending;
using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Direct.Core
{
    public class XmlModelValidator
    {

        public (bool success, XDocument result) Tokenize(string input)
        {
            try
            {
                return (true, XDocument.Parse(input));
            }
            catch(Exception)
            {
                return (false, null);
            }
        }

        public (T result, List<ErrorDescription> errors) MapToModel<T>(
            XElementWrapper rootToken, StronglyApiedObjectAttribute rootAttribute)
        {
            List<ErrorDescription> errors = new List<ErrorDescription>();
            T result = (T)MapObject(typeof(T), rootToken, "", rootAttribute);
            return (result, errors);

            dynamic DetermineTypeAndMap(FieldInfo fieldInfo, XElementWrapper token, XElementWrapper parentToken, string path)
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

            dynamic MapObject(Type objectType, XElementWrapper token, string path, StronglyApiedObjectAttribute attribute)
            {
                if(attribute == null)
                    throw new InvalidOperationException(
                        $"All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute, but none was found at {path}");

                if(token.IsNullOrUndefinedAsObject())
                {
                    if(attribute != null && !attribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if(!token.IsObject())
                {
                    errors.Add(ErrorDescription.NotAnObject(token.ToString(), path));
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

            dynamic[] MapArray(FieldInfo fieldInfo, XElementWrapper parentToken, string path)
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
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if(!parentToken.IsChildArray(fieldName))
                {
                    errors.Add(ErrorDescription.NotAnArray(JsonConvert.SerializeObject(parentToken.GetChildren(fieldName).Select(child => child.ToString())), path));
                    return null;
                }
                else
                {
                    List<XElementWrapper> tokenAsArray = parentToken.GetChildren(fieldName).ToList();
                    Type childType = fieldInfo.FieldType.GetElementType();

                    if (arrayAttribute != null)
                        if(tokenAsArray.Count < arrayAttribute.minLength)
                            errors.Add(ErrorDescription.ArrayTooShort(tokenAsArray.Count, arrayAttribute.minLength, path));

                        else if(tokenAsArray.Count > arrayAttribute.maxLength)
                            errors.Add(ErrorDescription.ArrayTooLong(tokenAsArray.Count, arrayAttribute.maxLength, path));

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

            dynamic MapFieldInArray(FieldInfo fieldInfo, Type type, XElementWrapper token, string path)
            {
                StronglyApiedFieldBase datatypeAttribute = fieldInfo.GetCustomAttribute<StronglyApiedFieldBase>(true);
                    if(token.IsNullOrUndefinedAsPrimitive())
                    {
                        if(datatypeAttribute != null && !datatypeAttribute.optional)
                            errors.Add(ErrorDescription.OptionalityViolation(path));
                    
                        return null;
                    }
                
                if(!token.IsPrimitive())
                {
                    errors.Add(ErrorDescription.NotPrimitive(path, token.ToString()));
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

            dynamic MapFieldInObject(FieldInfo fieldInfo, Type type, XElementWrapper parentToken, string path)
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
                            errors.Add(ErrorDescription.OptionalityViolation(path));
                        
                        return null;
                    }
                }
                else
                {
                    if(parentToken.IsChildNullOrUndefined(fieldName))
                    {
                        if(datatypeAttribute != null && !datatypeAttribute.optional)
                            errors.Add(ErrorDescription.OptionalityViolation(path));
                        
                        return null;
                    }
                }

                if(!isAttribute && !parentToken.IsChildPrimitive(fieldName))
                {
                    errors.Add(ErrorDescription.NotPrimitive(path, parentToken.GetChildAsText(fieldName)));
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
