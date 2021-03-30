using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Exceptions;
using BreadTh.StronglyApied.Attributes.Extending;
using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Core
{
    public class XmlModelMapper : ModelMapperBase
    {
        //This method shares a lot of commonalities with JsonModelMapper, but there are some very core key differences
        //Such as XML's lack of a concept of lists vs single instances, or JSONs lack of attributes.
        //I ultimately decided that code duplication was a lesser evil than trying to abstract those differences away with
        //a wrapper and logic that tries to accomodate both.
        public override (object result, List<ErrorDescription> errors) MapModel(string rawbody, Type rootType)
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
                    StronglyApiedXmlRelationBaseAttribute relationAttribute = 
                        childField.GetCustomAttribute<StronglyApiedXmlRelationBaseAttribute>(true);
                    string fieldName = childField.Name;

                    //It may be unsafe to put the values recieved into your object, eg when receiving null on a non-nullable type.
                    int errorCountBeforeParse = errors.Count;

                    string childPath = $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{fieldName}";

                    dynamic parsed = DetermineFieldTypeCategory(childField.FieldType) switch
                    {    FieldTypeCategory.Array => MapArray(childField, value, childPath)
                    ,   FieldTypeCategory.Object => MapObject(childField, value.Element(XName.Get(fieldName)), childPath)
                    ,    FieldTypeCategory.Value => MapFieldInObject(childField, value, childPath)
                    ,                          _ => throw new NotImplementedException()
                    };

                    //Arrays have to be instantiated differently from other types, except when they're null.
                    //but must be treated the same when they are null.
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
                StronglyApiedFieldBaseAttribute datatypeAttribute = field.GetCustomAttribute<StronglyApiedFieldBaseAttribute>(true);
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

                var tryParseOutcome = datatypeAttribute.Parse(field.FieldType.GetElementType(), value.Value, path);

                if(tryParseOutcome.TryPickT0(out var parsedValue, out var errorValue))
                    return parsedValue.Value;
                else
                {
                    errors.Add(errorValue.description);
                    return errorValue.bestParseAttempt;
                }
            }

            dynamic MapFieldInObject(FieldInfo field, XElement parentValue, string path)
            {
                //As a reminder, we have two attribute types here. One is the usual attribute for describing what value we expect.
                //The other (relationAttribute) is special to XML and describes where the value will actually be located.
                //A regular element or an attribute inside the xml tag.
                StronglyApiedXmlRelationBaseAttribute relationAttribute = field.GetCustomAttribute<StronglyApiedXmlRelationBaseAttribute>(true);
                string childFieldName = field.Name;
                bool childIsAttribute = relationAttribute != null && relationAttribute.GetType() == typeof(StronglyApiedXmlAttributeAttribute);

                StronglyApiedFieldBaseAttribute datatypeAttribute = field.GetCustomAttribute<StronglyApiedFieldBaseAttribute>(true);
                
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

                    //XML attributes are always strings, so here we don't need to check if it's primitive.

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
                
                var tryParseOutcome = datatypeAttribute.Parse(field.FieldType, childValue, path);

                if(tryParseOutcome.TryPickT0(out var parsedValue, out var errorValue))
                    return parsedValue.Value;
                else
                {
                    errors.Add(errorValue.description);
                    return errorValue.bestParseAttempt;
                }
                
            }
        }
    }
}
