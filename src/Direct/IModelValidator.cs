using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BreadTh.StronglyApied.Direct
{
    public interface IModelValidator
    {
        Task<(T result, List<ErrorDescription> errors)> TryParse<T>(Stream rawbody, bool leaveStreamOpen = true);
        (T result, List<ErrorDescription> errors) TryParse<T>(string rawbody);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(Stream rawbody, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess, bool leaveStreamOpen = true);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(string rawbody, Func<List<ErrorDescription>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        List<ErrorDescription> ValidateModel<T>(T value);
    }
}
