using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using BreadTh.StronglyApied.Attributes;

namespace BreadTh.StronglyApied.Direct
{
    public interface IModelValidator
    {
        Task<(T result, List<ErrorDescription> errors)> TryParse<T>(Stream rawbody, DataModel dataModel);
        (T result, List<ErrorDescription> errors) TryParse<T>(string rawbody, DataModel dataModel);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>((Stream rawbody, DataModel dataModel) parameters, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>((string rawbody, DataModel dataModel) parameters, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        List<ErrorDescription> ValidateModel<T>(T value, DataModel dataModel);
    }
}
