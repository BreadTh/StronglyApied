using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BreadTh.StronglyApied.Direct
{
    public interface IModelValidator
    {
        Task<(T result, List<ValidationError> errors)> TryParse<T>(Stream rawbody);
        (T result, List<ValidationError> errors) TryParse<T>(string rawbody);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(Stream rawbody, Func<List<ValidationError>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(string rawbody, Func<List<ValidationError>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        
        List<ValidationError> ValidateModel<T>(T value);
    }
}
