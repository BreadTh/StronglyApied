using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Core;
using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Attributes.Extending;
using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied
{
    public class ModelValidator : IModelValidator
    {
        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>(
            Stream rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess    
        ,   bool leaveStreamOpen = true)
        {
            using StreamReader reader = new StreamReader(rawbody, leaveOpen: leaveStreamOpen);
            return await TryParse(await reader.ReadToEndAsync(), onValidationError, onSuccess);
        }

        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>(
            string rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            var result = TryParse<MODEL>(rawbody);

            return result.errors.Count() == 0
            ?   await onSuccess(result.result)
            :   onValidationError(result.errors);
        }

        public async Task<(MODEL result, List<ErrorDescription> errors)> TryParse<MODEL>(Stream rawbody, bool leaveStreamOpen = true)
        {
            using StreamReader reader = new StreamReader(rawbody, leaveOpen: leaveStreamOpen);
            return TryParse<MODEL>(await reader.ReadToEndAsync());
        }

        public (MODEL result, List<ErrorDescription> errors) TryParse<MODEL>(string rawbody)
        {
            var rootAttribute = typeof(MODEL).GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {typeof(MODEL).FullName}");

            var (result, errors) = rootAttribute.datamodel switch
            {   DataModel.Json => MapJsonToModel(rawbody, typeof(MODEL))
            ,   DataModel.Xml => MapXmlToModel(rawbody, typeof(MODEL))
            ,   _ => throw new NotImplementedException()
            };
            return ((MODEL)result, errors);
        }

        public (object result, List<ErrorDescription> errors) TryParse(string rawbody, Type type)
        {
            var rootAttribute = type.GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {type.FullName}");

            return rootAttribute.datamodel switch
            {   DataModel.Json => MapJsonToModel(rawbody, type)
            ,   DataModel.Xml => MapXmlToModel(rawbody, type)
            ,   _ => throw new NotImplementedException()
            };
        }

        //No, this is not an optimal implementation by any measure. If you wanna improve it, be my guest.
        public List<ErrorDescription> ValidateModel<MODEL>(MODEL value)
        {
            var rootAttribute = typeof(MODEL).GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {typeof(MODEL).FullName}");
            
            return rootAttribute.datamodel switch
            {   DataModel.Json => MapJsonToModel(JsonConvert.SerializeObject(value), typeof(MODEL)).errors
            ,   _ => throw new NotImplementedException()
            };
        }

        private (object result, List<ErrorDescription> errors) MapJsonToModel(string rawbody, Type rootType)
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
                var attribute = field.GetCustomAttribute<StronglyApiedObjectAttribute>(inherit: false);

                if(attribute == null)
                    throw new ModelAttributeException(
                        "All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute"
                    +   $", but none was found at {rootType.FullName}.{path}");

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!attribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if (value.Type != JTokenType.Object)
                {
                    errors.Add(ErrorDescription.NotAnObject(value.ToCultureInvariantString(), path));
                    return null;
                }

                return MapObjectPostValidation(field.FieldType, value, path);
            }

            dynamic MapObjectPostValidation(Type fieldType, JToken value, string path)
            {
                dynamic result = Activator.CreateInstance(fieldType);

                foreach (FieldInfo childField in fieldType.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic && !fieldInfo.IsStatic))
                {
                    var childValue = value.SelectToken(childField.Name);
                    var childPath = path + (path == "" ? "" : ".") + childField.Name;
                    var errorCountBeforeParse = errors.Count;

                    dynamic parsed = DetermineFieldTypeCategory(childField.FieldType) switch
                    {   FieldTypeCategory.Array => MapArray(childField, childValue, childPath)
                    ,   FieldTypeCategory.Object => MapObject(childField, childValue, childPath)
                    ,   FieldTypeCategory.Value => MapField(childField, childValue, childPath)
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
                StronglyApiedArrayAttribute arrayAttribute = field.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if(arrayAttribute == null)
                    throw new ModelAttributeException(
                        $"All array fields must be tagged with StronglyApiedArrayAttribute"
                    +   $", but none was found at {rootType.FullName}.{path}");

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

                List<dynamic> resultList = new List<dynamic>();

                Type childType = field.FieldType.GetElementType();

                switch (DetermineFieldTypeCategory(childType))
                {
                    case FieldTypeCategory.Object:
                        for(int index = 0; index <= valueAsArray.Count - 1; index++)
                            resultList.Add(MapObject(field, valueAsArray[index], $"{path}[{index}]"));
                        break;
                    case FieldTypeCategory.Value:
                        for(int index = 0; index <= valueAsArray.Count - 1; index++)
                            resultList.Add(MapField(field, valueAsArray[index], $"{path}[{index}]"));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return resultList.ToArray();                
            }

            dynamic MapField(FieldInfo field, JToken value, string path)
            {                
                StronglyApiedFieldBase fieldAttribute = field.GetCustomAttribute<StronglyApiedFieldBase>(true);

                if(fieldAttribute == null)
                    throw new ModelAttributeException(
                        $"All primitive fields must be tagged with a child of StronglyApiedFieldBaseAttribute"
                    +   $", but none was found at {rootType.FullName}.{path}");

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!fieldAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));       
                    return null;
                }

                StronglyApiedFieldBase.TryParseResult tryParseOutcome = 
                    fieldAttribute.TryParse(field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType, value.ToCultureInvariantString(), path);

                if(tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                    errors.Add(tryParseOutcome.error);

                return tryParseOutcome.result;
            }
        }

        enum FieldTypeCategory { Array, Object, Value }

        private FieldTypeCategory DetermineFieldTypeCategory(Type type) 
        {
            ThrowIfFieldUnsupported(type);

            if (type.IsArray)
                return FieldTypeCategory.Array;

            if (type.IsNonStringClass())
                return FieldTypeCategory.Object;

            return FieldTypeCategory.Value;
        }

        private void ThrowIfFieldUnsupported(Type type)
        {
            if (type.IsGenericType && type.Name != "Nullable`1")
                throw new NotImplementedException(
                    "Generic types other than Nullable<T> are not yet supported. "
                +   "If you need a list, use T[] instead of List<T>");

            if (type.IsStruct() && type != typeof(decimal) && type != typeof(decimal?))
                throw new NotImplementedException("Structs are not yet supported. Use classes.");

            if (type.IsArray && !type.IsSZArray)
                throw new NotImplementedException("Only single-dimensional arrays are currently supported.");
        }

        //This method shares a lot of commonalities with MapJsonToModel, but there are some very core key differences
        //Such as XML's lack of a concept of lists vs single instances, or JSONs lack of attributes.
        //I ultimately decided that code duplication was a lesser evil than trying to abstract those differences away with
        //a wrapper and logic that tries to accomodate both.
        private (object result, List<ErrorDescription> errors) MapXmlToModel(string rawbody, Type rootType)
        {
            XDocument document;

            try
            {
                document = XDocument.Parse(rawbody);
            }
            catch (Exception)
            {
                return (default, new List<ErrorDescription>() { ErrorDescription.InvalidInputData(rawbody) });
            }

            List<ErrorDescription> errors = new List<ErrorDescription>();
            object result = MapObjectPostValidation(rootType, new XElement("document", document.Root), "");
            return (result, errors);

            dynamic MapObject(FieldInfo field, XElement value, string path)
            {
                var attribute = field.GetCustomAttribute<StronglyApiedObjectAttribute>(inherit: false);

                if (attribute == null)
                    throw new ModelAttributeException(
                        $"All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute, "
                    +   $"but none was found at {path}");

                if (value == null)
                {
                    if (!attribute.optional) 
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                //Here we cannot validate if value is an object. All (non-null) xml elements can be objects, 
                //as they can hold values in their attributes even if only child is a primitive.
                //While we could say that a completely blank element wasn't an object, 
                //this wouldn't work if all attributes and children were optional.

                return MapObjectPostValidation(field.FieldType, value, path);
            }

            dynamic MapObjectPostValidation(Type type, XElement value, string path) 
            {
                dynamic result = Activator.CreateInstance(type);

                foreach (FieldInfo childField in type.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic && !fieldInfo.IsStatic))
                {
                    StronglyApiedXmlRelationBaseAttribute relationAttribute = childField.GetCustomAttribute<StronglyApiedXmlRelationBaseAttribute>(true);
                    string fieldName = childField.Name;

                    int errorCountBeforeParse = errors.Count;

                    string childPath = $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{fieldName}";

                    dynamic parsed = DetermineFieldTypeCategory(childField.FieldType) switch
                    {   FieldTypeCategory.Array => MapArray(childField, value, childPath)
                    ,   FieldTypeCategory.Object => MapObject(childField, value.Element(XName.Get(fieldName)), childPath)
                    ,   FieldTypeCategory.Value => MapFieldInObject(childField, value, childPath)
                    ,   _ => throw new NotImplementedException()
                    };

                    if (!childField.FieldType.IsArray || parsed == null)
                        childField.SetValue(result, parsed);

                    else if (errorCountBeforeParse != errors.Count)
                        childField.SetValue(result, null);
                    else
                    {
                        Array dynamicallyTypedArray = Array.CreateInstance(childField.FieldType.GetElementType(), parsed.Length);
                        Array.Copy(parsed, dynamicallyTypedArray, parsed.Length);
                        childField.SetValue(result, dynamicallyTypedArray);
                    }
                }
                return result;
            }

            dynamic[] MapArray(FieldInfo field, XElement parentValue, string path)
            {
                StronglyApiedArrayAttribute arrayAttribute = field.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if (arrayAttribute == null)
                    throw new ModelAttributeException(
                            "All array fields must be tagged with StronglyApiedArrayAttribute, "
                        +   $"but none was found at {path}");

                StronglyApiedXmlRelationBaseAttribute relationAttribute = field.GetCustomAttribute<StronglyApiedXmlRelationBaseAttribute>(true);
                string fieldName = field.Name;

                //XML doesn't have a concept of lists, so testing if one is null/empty is troublesome.
                //A parent with an empty list looks exactly like a parent without a list.
                //The closest we can come is testing if the parent is a value-type
                //that is, the parent can't contain any elements.
                if (parentValue.IsPrimitive()) 
                {
                    if (!arrayAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                List<XElement> childValues = parentValue.Elements(XName.Get(fieldName)).ToList();
                Type childFieldType = field.FieldType.GetElementType();

                if (childValues.Count < arrayAttribute.minLength)
                    errors.Add(ErrorDescription.ArrayTooShort(childValues.Count, arrayAttribute.minLength, path));

                else if (childValues.Count > arrayAttribute.maxLength)
                    errors.Add(ErrorDescription.ArrayTooLong(childValues.Count, arrayAttribute.maxLength, path));

                List<dynamic> resultList = new List<dynamic>();

                int lastIndex = childValues.Count - 1;

                switch (DetermineFieldTypeCategory(childFieldType))
                {
                    case FieldTypeCategory.Object:
                        for (int index = 0; index <= lastIndex; index++)
                            resultList.Add(MapObject(field, childValues[index], $"{path}[{index}]"));
                        break;
                    case FieldTypeCategory.Value:
                        for (int index = 0; index <= lastIndex; index++)
                            resultList.Add(MapFieldInArray(field, childValues[index], $"{path}[{index}]"));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return resultList.ToArray();
            }
        
            //a singular field in an object must be tested for multiplicity and more using the parent value, however the child is accessed
            //differently when the child is a specific element of an array of elements on the parent compared to when 
            //it is accessing a singular element on the parent element. 
            //Furthermore, array fields cannot be attributes.
            //This is why MapField is split in two.
            dynamic MapFieldInArray(FieldInfo field, XElement value, string path)
            {
                StronglyApiedFieldBase datatypeAttribute = field.GetCustomAttribute<StronglyApiedFieldBase>(true);
                if (datatypeAttribute == null)
                    throw new ModelAttributeException(
                            "All primitive fields must be tagged with a child of StronglyApiedFieldBase, "
                        +   $"but none was found at {path}");

                if (value == null || value.FirstNode == null)
                {
                    if (!datatypeAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));

                    return null;
                }

                if (!value.IsPrimitive())
                {
                    errors.Add(ErrorDescription.NotPrimitive(path, value.ToString()));
                    return null;
                }

                var tryParseOutcome = datatypeAttribute.TryParse(field.FieldType.GetElementType(), value.Value, path);

                if (tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                    errors.Add(tryParseOutcome.error);

                return tryParseOutcome.result;
            }

            dynamic MapFieldInObject(FieldInfo field, XElement parentValue, string path)
            {
                StronglyApiedXmlRelationBaseAttribute relationAttribute = field.GetCustomAttribute<StronglyApiedXmlRelationBaseAttribute>(true);
                string childFieldName = field.Name;
                bool childIsAttribute = relationAttribute != null && relationAttribute.GetType() == typeof(StronglyApiedXmlAttributeAttribute);

                StronglyApiedFieldBase datatypeAttribute = field.GetCustomAttribute<StronglyApiedFieldBase>(true);
                
                if (datatypeAttribute == null)
                    throw new ModelAttributeException(
                        "All primitive fields must be tagged with a child of StronglyApiedFieldBaseAttribute, "
                    +   $"but none was found at {path}");

                string childValue;

                if (childIsAttribute)
                {
                    var childAttribute = parentValue.Attribute(XName.Get(childFieldName));
                    if (childAttribute == null)
                    {
                        if (!datatypeAttribute.optional)
                            errors.Add(ErrorDescription.OptionalityViolation(path));

                        return null;
                    }

                    //XML attributes are always primitives, so we don't need to check that.

                    childValue = childAttribute.Value;
                }
                else
                {
                    var childElement = parentValue.Element(XName.Get(childFieldName));
                    if (childElement == null)
                    {
                        if (datatypeAttribute != null && !datatypeAttribute.optional)
                            errors.Add(ErrorDescription.OptionalityViolation(path));

                        return null;
                    }
                    if (!childElement.IsPrimitive())
                    {
                        errors.Add(ErrorDescription.NotPrimitive(path, childElement.ToString()));
                        return null;
                    }

                    childValue = childElement.Value.ToString();
                }
                
                var tryParseOutcome = datatypeAttribute.TryParse(field.FieldType, childValue, path);

                if (tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                    errors.Add(tryParseOutcome.error);

                return tryParseOutcome.result;
                
            }
        }
    }
}
