using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
        JsonModelValidator _json;
        XmlModelValidator _xml;

        public ModelValidator(JsonModelValidator json = null, XmlModelValidator xml = null) 
        {
            _json = json ?? new JsonModelValidator();
            _xml = xml ?? new XmlModelValidator();
        }

        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>(Stream rawbody, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            using StreamReader reader = new StreamReader(rawbody);
            return await TryParse(await reader.ReadToEndAsync(), onValidationError, onSuccess);
        }

        public async Task<OUTCOME> TryParse<OUTCOME, MODEL>(string rawbody, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess)
        {
            var result = TryParse<MODEL>(rawbody);

            return result.errors.Count() == 0
            ?   await onSuccess(result.result)
            :   onValidationError(result.errors);
        }

        public async Task<(T result, List<ErrorDescription> errors)> TryParse<T>(Stream rawbody)
        {
            using StreamReader reader = new StreamReader(rawbody);
            return TryParse<T>(await reader.ReadToEndAsync());
        }

        public (T result, List<ErrorDescription> errors) TryParse<T>(string rawbody)
        {
            rawbody = rawbody.Trim();

            StronglyApiedRootAttribute rootAttribute = typeof(T).GetCustomAttribute<StronglyApiedRootAttribute>(false);

            if(rootAttribute == null)
                throw new InvalidOperationException(
                        $"The root model {typeof(T)} must be tagged with the attribute, StronglyApiedRoot, but none was found.");

            switch(rootAttribute.datamodel)
            {
                case DataModel.JSON:
                {

                    var (success, rootToken) = _json.TryTokenize(rawbody);
                    if(success)
                        return _json.MapToModel<T>(new JTokenWrapper(rootToken), new StronglyApiedObjectAttribute());
                    break;
                }

                case DataModel.XML:
                {
                    var (success, document) = _xml.TryTokenize(rawbody);
                    if (success)
                        return _xml.MapToModel<T>(new XElementWrapper(document.Root), new StronglyApiedObjectAttribute());
                    break;
                }

                default:
                    throw new InvalidOperationException("The root object datamodel attribute must be configured with a valid DataModel enum (other than \"Undefined\")");
            }
            
            return (default, new List<ErrorDescription>(){ ErrorDescription.InvalidInputData(rawbody) });
        }

        //No, this is not an optimal implementation by any measure. If you wanna improve it, be my guest.
        public List<ErrorDescription> ValidateModel<T>(T value)
        {
            StronglyApiedRootAttribute rootAttribute = typeof(T).GetCustomAttribute<StronglyApiedRootAttribute>(false);

            if(rootAttribute == null)
                throw new InvalidOperationException(
                        $"The root model {typeof(T)} must be tagged with the attribute, StronglyApiedRoot, but none was found.");

            switch(rootAttribute.datamodel)
            {
                case DataModel.JSON:
                    var (_, root) = _json.TryTokenize(JsonConvert.SerializeObject(value));
                    return _json.MapToModel<T>(new JTokenWrapper(root), new StronglyApiedObjectAttribute()).errors;

                case DataModel.XML:
                    throw new NotImplementedException();

                default:
                    throw new InvalidOperationException("The root object datamodel attribute must be configured with a valid DataModel enum (other than Undefined)");
            }
        }
    }
}
