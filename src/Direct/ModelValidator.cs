using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Direct.Core;
using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Attributes.Extending;
using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Direct
{
    public class ModelValidator : IModelValidator
    {
        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>((Stream rawbody, DataModel dataModel) parameters, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            using StreamReader reader = new StreamReader(parameters.rawbody);
            return await TryParse((await reader.ReadToEndAsync(), parameters.dataModel), onValidationError, onSuccess);
        }

        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>(
            (string rawbody, DataModel dataModel) parameters
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            var result = TryParse<MODEL>(parameters.rawbody, parameters.dataModel);

            return result.errors.Count() == 0
            ?   await onSuccess(result.result)
            :   onValidationError(result.errors);
        }

        public async Task<(T result, List<ErrorDescription> errors)> TryParse<T>(Stream rawbody, DataModel dataModel)
        {
            using StreamReader reader = new StreamReader(rawbody);
            return TryParse<T>(await reader.ReadToEndAsync(), dataModel);
        }

        public (T result, List<ErrorDescription> errors) TryParse<T>(string rawbody, DataModel dataModel) =>
            dataModel switch
            {   DataModel.JSON => MapJsonToModel<T>(rawbody)
            ,   DataModel.XML => MapXmlToModel<T>(rawbody)
            ,   _ => throw new NotImplementedException()
            };

        //No, this is not an optimal implementation by any measure. If you wanna improve it, be my guest.
        public List<ErrorDescription> ValidateModel<T>(T value, DataModel dataModel) =>
            dataModel switch
            {   DataModel.JSON => MapJsonToModel<T>(JsonConvert.SerializeObject(value)).errors
            ,   _ => throw new NotImplementedException()
            };

        private (T result, List<ErrorDescription> errors) MapJsonToModel<T>(string rawbody, string rootPath = "")
        {
            JObject rootToken;
            try
            {
                //though technically the root may be any of the json datatypes (yes, even null), we only want to support object as root.
                rootToken = JObject.Parse(rawbody.Trim());
            }
            catch (Exception)
            {
                return (default, new List<ErrorDescription>() { ErrorDescription.InvalidInputData(rawbody) });
            }

            //Rather than returning and merging a bunch of lists of ErrorDescriptions, 
            //the subsequent functions definied in this method
            //make direct reference to the error list below:
            List<ErrorDescription> errors = new List<ErrorDescription>();
            
            T result = (T)MapObjectPostValidation(typeof(T), rootToken, rootPath);
            
            return (result, errors);
            

            //return types must be dynamic as one cannot infer the fieldtypes of generics at compiletime.

            dynamic MapObject(FieldInfo field, JToken value, string path)
            {
                var attribute = field.GetCustomAttribute<StronglyApiedObjectAttribute>(inherit: false);

                if(attribute == null)
                    throw new ModelAttributeException(
                        "All object fields and array of object fields must be tagged with StronglyApiedObjectAttribute"
                    +   $", but none was found at {typeof(T).FullName}.{path}");

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

            dynamic MapObjectPostValidation(Type type, JToken value, string path)
            {
                dynamic result = Activator.CreateInstance(type);

                foreach (FieldInfo childField in type.GetFields().Where((FieldInfo fieldInfo) => fieldInfo.IsPublic))
                {
                    //It's common understanding that deserialization only fills out the instance fields.
                    if (childField.IsStatic)
                        continue;

                    var childValue = value.SelectToken(childField.Name);
                    var childPath = path + (path == "" ? "" : ".") + childField.Name;

                    childField.SetValue(
                        obj: result,
                        value: DetermineFieldTypeCategory(childField.FieldType) switch
                            {   FieldTypeCategory.Array => MapArray(childField, childValue, childPath)
                            ,   FieldTypeCategory.Class => MapObject(childField, childValue, childPath)
                            ,   FieldTypeCategory.Value => MapField(childField, childValue, childPath)
                            ,   _ => throw new NotImplementedException()
                            });
                }
                return result;
            }

            dynamic[] MapArray(FieldInfo field, JToken value, string path)
            {
                StronglyApiedArrayAttribute arrayAttribute = field.GetCustomAttribute<StronglyApiedArrayAttribute>(false);

                if(arrayAttribute == null)
                    throw new ModelAttributeException(
                        $"All array fields must be tagged with StronglyApiedArrayAttribute"
                    +   $", but none was found at {typeof(T).FullName}.{path}");

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
                    case FieldTypeCategory.Class:
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
                    +   $", but none was found at {typeof(T).FullName}.{path}");

                if(value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
                {
                    if(!fieldAttribute.optional)
                        errors.Add(ErrorDescription.OptionalityViolation(path));       
                    return null;
                }

                StronglyApiedFieldBase.TryParseResult tryParseOutcome = 
                    fieldAttribute.TryParse(field.FieldType, value.ToCultureInvariantString(), path);

                if(tryParseOutcome.status == StronglyApiedFieldBase.TryParseResult.Status.Invalid)
                    errors.Add(tryParseOutcome.error);

                return tryParseOutcome.result;
            }
        }

        enum FieldTypeCategory { Array, Class, Value }

        private FieldTypeCategory DetermineFieldTypeCategory(Type type) 
        {
            ThrowIfFieldUnsupported(type);

            if (type.IsArray)
                return FieldTypeCategory.Array;

            if (type.IsNonStringClass())
                return FieldTypeCategory.Class;

            return FieldTypeCategory.Value;
        }
        private void ThrowIfFieldUnsupported(Type type)
        {
            if (type.IsGenericType && type.Name != "Nullable`1")
                throw new NotImplementedException(
                    "Generic types other than Nullable<T> are not yet supported. "
                +   "If you need a list, use T[] instead of List<T>");

            if (type.IsStruct())
                throw new NotImplementedException("Structs are not yet supported. Use classes.");


            if (type.IsArray && !type.IsSZArray)
                throw new NotImplementedException("Only single-dimensional arrays are currently supported.");
        }

        private (T result, List<ErrorDescription> errors) MapXmlToModel<T>(string rawbody)
        {
            throw new NotImplementedException();
        }
    }
}
