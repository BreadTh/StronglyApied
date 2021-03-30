using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Exceptions;

namespace BreadTh.StronglyApied.Core
{
    //This class exists to avoid cyclic namespace dependecy when the inner json/xml components calls this to 
    //start a new parse on a stringified piece of json/xml inside a field.
    public static class ModelValidatorImp
    {
        public static async Task<OUTCOME> Parse<OUTCOME, MODEL>(
            Stream rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess    
        ,   bool leaveStreamOpen = true)
        {
            using StreamReader reader = new StreamReader(rawbody, leaveOpen: leaveStreamOpen);
            return await Parse(await reader.ReadToEndAsync(), onValidationError, onSuccess);
        }

        public static async Task<OUTCOME> Parse<OUTCOME, MODEL>(
            string rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            var result = Parse<MODEL>(rawbody);

            return result.errors.Count() == 0
            ?   await onSuccess(result.result)
            :   onValidationError(result.errors);
        }

        public static async Task<(MODEL result, List<ErrorDescription> errors)> Parse<MODEL>(Stream rawbody, bool leaveStreamOpen = true)
        {
            using StreamReader reader = new StreamReader(rawbody, leaveOpen: leaveStreamOpen);
            return Parse<MODEL>(await reader.ReadToEndAsync());
        }

        public static (MODEL result, List<ErrorDescription> errors) Parse<MODEL>(string rawbody)
        {
            var rootAttribute = typeof(MODEL).GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {typeof(MODEL).FullName}");

            var (result, errors) = rootAttribute.datamodel switch
            {   DataModel.Json => new JsonModelMapper().MapModel(rawbody, typeof(MODEL))
            ,    DataModel.Xml => new XmlModelMapper().MapModel(rawbody, typeof(MODEL))
            ,                _ => throw new NotImplementedException()
            };
            return ((MODEL)result, errors);
        }

        public static (object result, List<ErrorDescription> errors) Parse(string rawbody, Type type)
        {
            var rootAttribute = type.GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {type.FullName}");

            return rootAttribute.datamodel switch
            {   DataModel.Json => new JsonModelMapper().MapModel(rawbody, type)
            ,    DataModel.Xml => new XmlModelMapper().MapModel(rawbody, type)
            ,                _ => throw new NotImplementedException()
            };
        }

        //No, this is not an optimal implementation by any measure. If you wanna improve it, be my guest.
        public static List<ErrorDescription> ValidateModel<MODEL>(MODEL value)
        {
            var rootAttribute = typeof(MODEL).GetCustomAttribute<StronglyApiedRootAttribute>(inherit: false);

            if(rootAttribute == null)
                throw new ModelAttributeException(
                    "All objects used as the root type, \"MODEL\", in ModelValidator.TryParse must be tagged with StronglyApiedRootAttribute"
                +   $", but none was found at {typeof(MODEL).FullName}");
            
            return rootAttribute.datamodel switch
            {   DataModel.Json => new JsonModelMapper().MapModel(JsonConvert.SerializeObject(value), typeof(MODEL)).errors
            ,                _ => throw new NotImplementedException()
            };
        }
    }
}
